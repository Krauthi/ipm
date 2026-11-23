using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using iPMCloud.Mobile.Platforms.Android.Services;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.vo;
using System.Linq;

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

                // Direkter context.StartForegroundService()-Aufruf auf UploadForegroundService wurde
                // entfernt: Zwischen der obigen Vordergrund-Prüfung und der tatsächlichen Verarbeitung
                // von OnCreate/StartForeground durch das Android-System kann die App in den
                // Hintergrund wechseln oder eingefroren werden. Trifft StartForeground() dann nicht
                // rechtzeitig (5s) ein, tötet Android den Prozess mit
                // ForegroundServiceDidNotStartInTimeException - unabhängig von Try/Catch hier.
                // Der WorkManager-Pfad (UploadRetryWorker) ruft SetForegroundAsync() erst innerhalb
                // von DoWork() auf und fängt Fehler dort kontrolliert ab (Result.InvokeRetry()),
                // wodurch dieses Race vollständig vermieden wird - egal ob die App im Vorder-
                // oder Hintergrund ist.
                var context = global::Android.App.Application.Context;
                UploadRetryScheduler.ScheduleImmediateUploadRetry(context: context);
                Log.Info(TAG, "Upload scheduled via WorkManager (UploadRetryWorker)");
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
