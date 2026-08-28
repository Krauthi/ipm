using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using AndroidX.Work;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.vo;
using System;

namespace iPMCloud.Mobile.Platforms.Android.Workers
{
    /// <summary>
    /// WorkManager Worker für periodische Upload-Wiederholungen.
    /// Wird automatisch geplant wenn Uploads fehlschlagen, und alle 15-30 Minuten ausgeführt.
    /// Prüft ob pending Uploads existieren und führt sie als Foreground-Worker direkt aus.
    /// </summary>
    public class UploadRetryWorker : Worker
    {
        private const string TAG = "UploadRetryWorker";
        private const string CHANNEL_ID = "ipmcloud_upload_channel";
        private const int NOTIFICATION_ID = 20002;

        public UploadRetryWorker(Context context, WorkerParameters workerParams) 
            : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {
            try
            {
                Log.Info(TAG, "UploadRetryWorker started - checking for pending uploads");

                // Prüfe ob Internet verfügbar ist
                if (!AppModel.Instance.IsInternet)
                {
                    Log.Info(TAG, "No internet connection, will retry later");
                    // Bei keinem Internet: Retry (WorkManager wird später wiederholen)
                    return Result.InvokeRetry();
                }

                // Prüfe ob bereits ein Upload läuft
                if (UploadCoordinator.Instance.IsRunning)
                {
                    Log.Info(TAG, "Upload already running, skipping");
                    return Result.InvokeSuccess();
                }

                // Prüfe ob Uploads pending sind
                var pendingCount = UploadCoordinator.Instance.GetPendingUploadCount();

                if (pendingCount <= 0)
                {
                    Log.Info(TAG, "No pending uploads found");
                    return Result.InvokeSuccess();
                }

                Log.Info(TAG, $"Found {pendingCount} pending uploads, running foreground worker upload");

                try
                {
                    PromoteToForeground();
                }
                catch (Exception foregroundEx)
                {
                    Log.Error(TAG, $"Failed to promote worker to foreground: {foregroundEx}");
                    AppModel.Logger?.Error($"UploadRetryWorker: Foreground-Aktivierung fehlgeschlagen - {foregroundEx.Message}");
                    return Result.InvokeRetry();
                }

                try
                {
                    UploadCoordinator.Instance.RunAsync().GetAwaiter().GetResult();

                    var remainingCount = UploadCoordinator.Instance.GetPendingUploadCount();
                    if (remainingCount > 0)
                    {
                        Log.Warn(TAG, $"Upload run finished with {remainingCount} pending uploads remaining");
                        AppModel.Logger?.Warn($"UploadRetryWorker: {remainingCount} Uploads weiterhin ausstehend, Wiederholung folgt");
                    }
                    else
                    {
                        Log.Info(TAG, "Upload run finished successfully from Worker");
                    }

                    return Result.InvokeSuccess();
                }
                catch (Exception runEx)
                {
                    Log.Error(TAG, $"Upload execution failed: {runEx}");
                    AppModel.Logger?.Error($"UploadRetryWorker: Upload-Ausführung fehlgeschlagen - {runEx.Message}");

                    return Result.InvokeRetry();
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"UploadRetryWorker error: {ex.Message}");
                AppModel.Logger?.Error($"UploadRetryWorker error: {ex.Message}");

                // Bei unbekannten Fehlern: Failure (stoppt Worker, muss neu geplant werden)
                return Result.InvokeFailure();
            }
        }

        private void PromoteToForeground()
        {
            EnsureNotificationChannelExists();

            var notification = BuildNotification();

            ForegroundInfo foregroundInfo;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                foregroundInfo = new ForegroundInfo(
                    NOTIFICATION_ID,
                    notification,
                    (int)global::Android.Content.PM.ForegroundService.TypeDataSync);
            }
            else
            {
                foregroundInfo = new ForegroundInfo(NOTIFICATION_ID, notification);
            }

            SetForegroundAsync(foregroundInfo).Get();
        }

        private Notification BuildNotification()
        {
            var intent = new Intent(ApplicationContext, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(
                ApplicationContext,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            return new NotificationCompat.Builder(ApplicationContext, CHANNEL_ID)
                .SetContentTitle("iPM-Cloud Upload")
                .SetContentText("Ausstehende Uploads werden verarbeitet…")
                .SetSmallIcon(global::Android.Resource.Drawable.StatSysUpload)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true)
                .SetContentIntent(pendingIntent)
                .SetCategory(NotificationCompat.CategoryService)
                .SetPriority(NotificationCompat.PriorityLow)
                .Build();
        }

        private void EnsureNotificationChannelExists()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var notificationManager = ApplicationContext.GetSystemService(Context.NotificationService) as NotificationManager;
            if (notificationManager?.GetNotificationChannel(CHANNEL_ID) != null)
                return;

            var channel = new NotificationChannel(
                CHANNEL_ID,
                "iPM Uploads",
                NotificationImportance.Low)
            {
                Description = "Zeigt Hintergrund-Uploads an."
            };

            notificationManager?.CreateNotificationChannel(channel);
        }
    }
}
