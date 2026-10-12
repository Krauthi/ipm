using Android.Content;
using Android.OS;
using Android.Util;
using iPMCloud.Mobile.Platforms.Android.Services;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Platforms.Android
{
    public class AndroidUploadService : IUploadService
    {
        private const string TAG = "AndroidUploadService";

        public void StartUploads()
        {
            try
            {
                // Vermeidet redundante startForegroundService()-Aufrufe, wenn bereits
                // ein Upload läuft. Wiederholte kurz aufeinanderfolgende Aufrufe
                // erhöhen das Risiko einer ForegroundServiceDidNotStartInTimeException.
                if (UploadCoordinator.Instance.IsRunning)
                {
                    Log.Info(TAG, "StartUploads skipped, upload already running");
                    return;
                }

                // Android 12+ verbietet in der Regel das Starten eines Foreground-Service,
                // wenn die App im Hintergrund ist. In diesem Fall wird trotzdem eine
                // ServiceRecord angelegt, aber startForeground() kommt nie rechtzeitig an,
                // was zu einer ForegroundServiceDidNotStartInTimeException und einem
                // App-Absturz führt. Deshalb weichen wir im Hintergrund auf den
                // WorkManager-Pfad aus, der für Hintergrund-Ausführung ausgelegt ist.
                if (AppModel.Instance.isInBackground)
                {
                    Log.Info(TAG, "App is in background, routing upload via WorkManager instead of direct foreground service start");
                    UploadRetryScheduler.ScheduleImmediateUploadRetry(context: global::Android.App.Application.Context);
                    return;
                }

                var context = global::Android.App.Application.Context;
                var intent = new Intent(context, typeof(UploadForegroundService));
                intent.SetAction(UploadForegroundService.ACTION_START);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    context.StartForegroundService(intent);
                else
                    context.StartService(intent);

                Log.Info(TAG, "UploadForegroundService started");
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"StartUploads error: {ex.Message}");
                AppModel.Logger?.Error($"AndroidUploadService.StartUploads: {ex.Message}");
            }
        }

        public void StopUploads()
        {
            try
            {
                var context = global::Android.App.Application.Context;
                var intent = new Intent(context, typeof(UploadForegroundService));
                intent.SetAction(UploadForegroundService.ACTION_STOP);

                context.StartService(intent);

                Log.Info(TAG, "UploadForegroundService stop requested");
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"StopUploads error: {ex.Message}");
                AppModel.Logger?.Error($"AndroidUploadService.StopUploads: {ex.Message}");
            }
        }
    }
}
