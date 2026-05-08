using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using iPMCloud.Mobile.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.Platforms.Android.Services
{
    /// <summary>
    /// Android Foreground Service for building sync.
    /// Keeps a PARTIAL_WAKE_LOCK so the CPU does not sleep during the operation,
    /// shows a persistent notification with progress, and stops itself when done.
    ///
    /// Lifecycle:
    ///   StartForeground → AcquireWakeLock → SyncCoordinator.RunAsync → StopSelf → ReleaseWakeLock
    /// </summary>
    [Service(Name = "com.ipmcloud.ipm.mobile.SyncForegroundService", Exported = false)]
    public class SyncForegroundService : Service
    {
        public const string ACTION_START = "iPMCloud.sync.ACTION_START";
        public const string ACTION_STOP  = "iPMCloud.sync.ACTION_STOP";
        public const string EXTRA_MANUELL = "manuell_sync";

        private const string SYNC_CHANNEL_ID = "ipmcloud_sync_channel";
        private const int    SYNC_NOTIFICATION_ID = 20001;
        private const string TAG = "SyncForegroundService";

        // Max WakeLock duration as a safety cap (10 minutes in ms)
        private const long WAKE_LOCK_TIMEOUT_MS = 10 * 60 * 1000;

        private CancellationTokenSource _cts;
        private PowerManager.WakeLock   _wakeLock;
        private readonly object         _wakeLockLock = new object();

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == ACTION_STOP)
            {
                Log.Info(TAG, "Stop action received.");
                _cts?.Cancel();
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            // Ensure notification channel exists before calling StartForeground
            CreateNotificationChannel();

            var notification = BuildNotification("Synchronisation läuft…", 0);
            StartForeground(SYNC_NOTIFICATION_ID, notification);

            AcquireWakeLock();

            bool manuell = intent?.GetBooleanExtra(EXTRA_MANUELL, false) ?? false;
            _cts = new CancellationTokenSource();

            // Subscribe to progress for notification updates
            SyncCoordinator.ProgressChanged += OnSyncProgress;
            SyncCoordinator.SyncCompleted   += OnSyncCompleted;

            var token = _cts.Token;
            Task.Run(async () =>
            {
                try
                {
                    Log.Info(TAG, $"Starting SyncCoordinator (manuell={manuell})");
                    await SyncCoordinator.Instance.RunAsync(token).ConfigureAwait(false);
                    Log.Info(TAG, "SyncCoordinator finished.");
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, $"SyncCoordinator error: {ex.Message}");
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
            Log.Debug(TAG, "OnDestroy");
        }

        // ── Event Handlers ────────────────────────────────────────────────────────────

        private void OnSyncProgress(object sender, SyncProgressEventArgs e)
        {
            try
            {
                var nm = (NotificationManager)GetSystemService(NotificationService);
                nm?.Notify(SYNC_NOTIFICATION_ID, BuildNotification(e.StatusText, (int)e.ProgressPercent));
            }
            catch (Exception ex)
            {
                Log.Warn(TAG, $"OnSyncProgress notify error: {ex.Message}");
            }
        }

        private void OnSyncCompleted(object sender, SyncCompletedEventArgs e)
        {
            Log.Info(TAG, $"OnSyncCompleted: success={e.Success}, msg={e.ErrorMessage}");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────────

        private void Cleanup()
        {
            try
            {
                SyncCoordinator.ProgressChanged -= OnSyncProgress;
                SyncCoordinator.SyncCompleted   -= OnSyncCompleted;
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                // Remove the ongoing notification from the tray when sync finishes
                try
                {
                    var nm = GetSystemService(NotificationService) as NotificationManager;
                    nm?.Cancel(SYNC_NOTIFICATION_ID);
                }
                catch (Exception nex) { Log.Warn(TAG, $"Cleanup cancel notification: {nex.Message}"); }
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
                    _wakeLock = pm?.NewWakeLock(WakeLockFlags.Partial, "iPMCloud:SyncWakeLock");
                    _wakeLock?.Acquire(WAKE_LOCK_TIMEOUT_MS);
                    Log.Debug(TAG, "WakeLock acquired");
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
                        Log.Debug(TAG, "WakeLock released");
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

            var nm = GetSystemService(NotificationService) as NotificationManager;
            if (nm?.GetNotificationChannel(SYNC_CHANNEL_ID) != null)
                return;

            var channel = new NotificationChannel(
                SYNC_CHANNEL_ID,
                "iPM Synchronisation",
                NotificationImportance.Low)
            {
                Description = "Zeigt den Fortschritt der Datensynchronisation an."
            };

            nm?.CreateNotificationChannel(channel);
            Log.Debug(TAG, "Sync notification channel created");
        }

        private Notification BuildNotification(string text, int progressPercent)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.SingleTop);

            var pi = PendingIntent.GetActivity(
                this, 0, intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(this, SYNC_CHANNEL_ID)
                .SetContentTitle("iPM-Cloud Synchronisation")
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.ipmlogo_m)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true)
                .SetContentIntent(pi)
                .SetCategory(NotificationCompat.CategoryProgress);

            if (progressPercent > 0 && progressPercent < 100)
                builder.SetProgress(100, progressPercent, false);
            else if (progressPercent == 0)
                builder.SetProgress(100, 0, true); // indeterminate at start

            return builder.Build();
        }
    }
}
