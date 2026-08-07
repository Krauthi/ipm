using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using iPMCloud.Mobile.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.Platforms.Android.Services
{
    [Service(
        Name = "com.ipmcloud.ipm.mobile.UploadForegroundService",
        Exported = false,
        ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class UploadForegroundService : Service
    {
        public const string ACTION_START = "iPMCloud.upload.ACTION_START";
        public const string ACTION_STOP = "iPMCloud.upload.ACTION_STOP";

        private const string CHANNEL_ID = "ipmcloud_upload_channel";
        private const int NOTIFICATION_ID = 20002;
        private const string TAG = "UploadForegroundService";
        // Safety cap: expected upload runs are usually <= 5 minutes; 10 minutes avoids stale wake locks on failure paths.
        private const long WAKE_LOCK_TIMEOUT_MS = 10 * 60 * 1000;

        private CancellationTokenSource _cts;
        private PowerManager.WakeLock _wakeLock;
        private readonly object _wakeLockLock = new object();
        private readonly object _foregroundLock = new object();
        private bool _isForegroundStarted;

        public override IBinder OnBind(Intent intent) => null;

        public override void OnCreate()
        {
            base.OnCreate();

            // KRITISCH: StartForeground() muss SOFORT aufgerufen werden
            // Selbst wenn Fehler auftreten, müssen wir in den Foreground-Modus wechseln
            try
            {
                // Channel MUSS vor StartForeground existieren
                CreateNotificationChannel();

                // Verwende minimale Notification für schnellsten Start
                var notification = BuildFallbackNotification();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
#pragma warning disable CA1416
                    StartForeground(
                        NOTIFICATION_ID,
                        notification,
                        ForegroundService.TypeDataSync);
#pragma warning restore CA1416
                }
                else
                {
                    StartForeground(NOTIFICATION_ID, notification);
                }

                lock (_foregroundLock)
                {
                    _isForegroundStarted = true;
                }

                Log.Info(TAG, "StartForeground called successfully in OnCreate");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"CRITICAL: OnCreate StartForeground failed: {ex}");
                // Trotz Fehler versuchen wir den Service zu stoppen
                StopSelf();
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // Prüfe, ob wir bereits im Foreground sind
            lock (_foregroundLock)
            {
                if (!_isForegroundStarted)
                {
                    Log.Error(TAG, "OnStartCommand: Service not in foreground mode, stopping");
                    StopSelf();
                    return StartCommandResult.NotSticky;
                }
            }

            // Handle stop action
            if (intent?.Action == ACTION_STOP)
            {
                _cts?.Cancel();
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            // Check if already running
            if (UploadCoordinator.Instance.IsRunning)
            {
                Log.Info(TAG, "Upload already running, skipping");
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            // Aktualisiere Notification mit besserem Text
            try
            {
                var betterNotification = BuildNotification("Uploads werden vorbereitet…", 0);
                var nm = GetSystemService(NotificationService) as NotificationManager;
                nm?.Notify(NOTIFICATION_ID, betterNotification);
            }
            catch (Exception nex)
            {
                Log.Warn(TAG, $"Could not update notification: {nex.Message}");
            }

            AcquireWakeLock();

            _cts = new CancellationTokenSource();
            UploadCoordinator.ProgressChanged += OnUploadProgress;
            UploadCoordinator.UploadCompleted += OnUploadCompleted;

            var token = _cts.Token;
            Task.Run(async () =>
            {
                try
                {
                    await UploadCoordinator.Instance.RunAsync(token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, $"UploadCoordinator error: {ex.Message}");
                }
                finally
                {
                    Cleanup();
                    StopSelf();
                }
            }, token);

            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            Cleanup();
            base.OnDestroy();
        }

        /// <summary>
        /// Erstellt eine minimal-einfache Notification die garantiert schnell ist.
        /// Verwendet nur Android System-Icons, keine benutzerdefinierten Ressourcen.
        /// </summary>
        private Notification BuildFallbackNotification()
        {
            return new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("iPM-Cloud Upload")
                .SetContentText("Uploads laufen…")
                .SetSmallIcon(global::Android.Resource.Drawable.StatSysUpload)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true)
                .SetCategory(NotificationCompat.CategoryService)
                .SetPriority(NotificationCompat.PriorityLow)
                .Build();
        }

        private void OnUploadProgress(object sender, UploadProgressEventArgs e)
        {
            try
            {
                var nm = (NotificationManager)GetSystemService(NotificationService);
                nm?.Notify(NOTIFICATION_ID, BuildNotification(e.StatusText, (int)e.ProgressPercent));
            }
            catch (Exception ex)
            {
                Log.Warn(TAG, $"OnUploadProgress notify error: {ex.Message}");
            }
        }

        private void OnUploadCompleted(object sender, UploadCompletedEventArgs e)
        {
            Log.Info(TAG, $"OnUploadCompleted: success={e.Success}, msg={e.ErrorMessage}");
        }

        private void Cleanup()
        {
            try
            {
                UploadCoordinator.ProgressChanged -= OnUploadProgress;
                UploadCoordinator.UploadCompleted -= OnUploadCompleted;
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;

                try
                {
                    var nm = GetSystemService(NotificationService) as NotificationManager;
                    nm?.Cancel(NOTIFICATION_ID);
                }
                catch (Exception nex)
                {
                    Log.Warn(TAG, $"Cleanup cancel notification: {nex.Message}");
                }

                ReleaseWakeLock();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Cleanup error: {ex.Message}");
            }
        }

        private void AcquireWakeLock()
        {
            lock (_wakeLockLock)
            {
                try
                {
                    var pm = GetSystemService(PowerService) as PowerManager;
                    _wakeLock = pm?.NewWakeLock(WakeLockFlags.Partial, "iPMCloud:UploadWakeLock");
                    _wakeLock?.Acquire(WAKE_LOCK_TIMEOUT_MS);
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, $"AcquireWakeLock error: {ex.Message}");
                }
            }
        }

        private void ReleaseWakeLock()
        {
            lock (_wakeLockLock)
            {
                try
                {
                    if (_wakeLock != null && _wakeLock.IsHeld)
                    {
                        _wakeLock.Release();
                    }
                    _wakeLock = null;
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, $"ReleaseWakeLock error: {ex.Message}");
                }
            }
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            try
            {
                var nm = GetSystemService(NotificationService) as NotificationManager;
                if (nm?.GetNotificationChannel(CHANNEL_ID) != null)
                    return;

                var channel = new NotificationChannel(CHANNEL_ID, "iPM Uploads", NotificationImportance.Low)
                {
                    Description = "Zeigt den Fortschritt von Uploads an."
                };

                nm?.CreateNotificationChannel(channel);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"CreateNotificationChannel error (non-fatal): {ex.Message}");
                // Non-fatal: Wenn Channel nicht erstellt werden kann, verwenden wir trotzdem 
                // die Notification - Android wird dann den Default-Channel verwenden
            }
        }

        private Notification BuildNotification(string text, int progressPercent)
        {
            try
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.SingleTop);

                var pi = PendingIntent.GetActivity(
                    this,
                    0,
                    intent,
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                    .SetContentTitle("iPM-Cloud Upload")
                    .SetContentText(text)
                    .SetOngoing(true)
                    .SetOnlyAlertOnce(true)
                    .SetContentIntent(pi)
                    .SetCategory(NotificationCompat.CategoryProgress)
                    .SetPriority(NotificationCompat.PriorityLow);

                // Versuche custom Icon, fallback auf System-Icon
                try
                {
                    builder.SetSmallIcon(Resource.Drawable.ipmlogo_m);
                }
                catch
                {
                    builder.SetSmallIcon(global::Android.Resource.Drawable.StatSysUpload);
                }

                if (progressPercent > 0 && progressPercent < 100)
                    builder.SetProgress(100, progressPercent, false);
                else if (progressPercent == 0)
                    builder.SetProgress(100, 0, true);

                return builder.Build();
            }
            catch (Exception ex)
            {
                Log.Warn(TAG, $"BuildNotification error, using fallback: {ex.Message}");
                return BuildFallbackNotification();
            }
        }
    }
}
