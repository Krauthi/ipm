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
