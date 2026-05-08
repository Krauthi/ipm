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
    [Service(Name = "com.ipmcloud.ipm.mobile.UploadForegroundService", Exported = false)]
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

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == ACTION_STOP)
            {
                _cts?.Cancel();
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            if (UploadCoordinator.Instance.IsRunning)
            {
                return StartCommandResult.NotSticky;
            }

            CreateNotificationChannel();
            StartForeground(NOTIFICATION_ID, BuildNotification("Uploads laufen…", 0));
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

            var nm = GetSystemService(NotificationService) as NotificationManager;
            if (nm?.GetNotificationChannel(CHANNEL_ID) != null)
                return;

            var channel = new NotificationChannel(CHANNEL_ID, "iPM Uploads", NotificationImportance.Low)
            {
                Description = "Zeigt den Fortschritt von Uploads an."
            };

            nm?.CreateNotificationChannel(channel);
        }

        private Notification BuildNotification(string text, int progressPercent)
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
                .SetSmallIcon(Resource.Drawable.ipmlogo_m)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true)
                .SetContentIntent(pi)
                .SetCategory(NotificationCompat.CategoryProgress);

            if (progressPercent > 0 && progressPercent < 100)
                builder.SetProgress(100, progressPercent, false);
            else if (progressPercent == 0)
                builder.SetProgress(100, 0, true);

            return builder.Build();
        }
    }
}
