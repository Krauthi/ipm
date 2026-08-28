using Android.Content;
using Android.Util;
using AndroidX.Work;
using iPMCloud.Mobile.Platforms.Android.Workers;
using iPMCloud.Mobile.vo;
using System;
using System.Collections.Generic;
using System.Linq;
using Java.Util.Concurrent;

namespace iPMCloud.Mobile.Platforms.Android
{
    /// <summary>
    /// Helper-Klasse zum Planen und Verwalten von periodischen Upload-Retry-Jobs mit WorkManager.
    /// </summary>
    public static class UploadRetryScheduler
    {
        private const string TAG = "UploadRetryScheduler";
        private const string UPLOAD_RETRY_WORK_NAME = "ipm_upload_retry_work";

        /// <summary>
        /// Plant einen periodischen Upload-Retry-Job.
        /// Wird ausgeführt wenn Netzwerk verfügbar ist, alle 10 Minuten.
        /// </summary>
        public static void ScheduleUploadRetry(Context context)
        {
            try
            {
                var workManager = WorkManager.GetInstance(context);

                // Constraints: Nur wenn Netzwerk verfügbar ist
                var constraints = new Constraints.Builder()
                    .SetRequiredNetworkType(NetworkType.Connected)
                    .SetRequiresBatteryNotLow(false)  // Auch bei niedrigem Akku versuchen
                    .Build();

                // Periodischer Job: Alle 10 Minuten (WorkManager minimum ist 15 min, aber wir verwenden 15min + flex)
                var uploadRetryWork = new PeriodicWorkRequest.Builder(
                        typeof(UploadRetryWorker),
                        30, // repeatInterval (minimum erlaubt)
                        TimeUnit.Minutes,
                        8, // flexInterval - erlaubt WorkManager 5min Spielraum = effektiv 10-15min
                        TimeUnit.Minutes)
                    .SetConstraints(constraints)
                    .SetBackoffCriteria(
                        BackoffPolicy.Exponential,
                        4, // initialDelay
                        TimeUnit.Minutes)
                    .AddTag("upload_retry")
                    .Build();

                // ExistingPeriodicWorkPolicy.KEEP: Wenn bereits geplant, behalte den existierenden Job
                // Das verhindert, dass bei jedem App-Start ein neuer Job erstellt wird
                workManager.EnqueueUniquePeriodicWork(
                    UPLOAD_RETRY_WORK_NAME,
                    ExistingPeriodicWorkPolicy.Keep,
                    uploadRetryWork);

                Log.Info(TAG, "Upload retry work scheduled successfully (5-15min interval)");
                AppModel.Logger?.Info("Upload-Wiederholungs-Job geplant: alle 5-15min wenn Netzwerk verfügbar");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Failed to schedule upload retry work: {ex.Message}");
                AppModel.Logger?.Error($"UploadRetryScheduler: Fehler beim Planen - {ex.Message}");
            }
        }

        /// <summary>
        /// Plant einen einmaligen sofortigen Upload-Versuch.
        /// Wird verwendet wenn Netzwerk wiederhergestellt wird.
        /// </summary>
        public static void ScheduleImmediateUploadRetry(Context context)
        {
            try
            {
                var workManager = WorkManager.GetInstance(context);

                // Constraints: Nur wenn Netzwerk verfügbar ist
                var constraints = new Constraints.Builder()
                    .SetRequiredNetworkType(NetworkType.Connected)
                    .Build();

                // Einmaliger Job: Sofort ausführen
                var oneTimeWork = new OneTimeWorkRequest.Builder(typeof(UploadRetryWorker))
                    .SetConstraints(constraints)
                    .SetBackoffCriteria(
                        BackoffPolicy.Exponential,
                        5, // initialDelay bei Retry
                        TimeUnit.Minutes)
                    .AddTag("upload_retry_immediate")
                    .Build();

                workManager.Enqueue(oneTimeWork);

                Log.Info(TAG, "Immediate upload retry scheduled");
                AppModel.Logger?.Info("Sofortiger Upload-Wiederholungs-Versuch geplant");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Failed to schedule immediate upload retry: {ex.Message}");
                AppModel.Logger?.Error($"UploadRetryScheduler: Fehler beim sofortigen Planen - {ex.Message}");
            }
        }

        /// <summary>
        /// Storniert alle geplanten Upload-Retry-Jobs.
        /// Sollte aufgerufen werden wenn alle Uploads erfolgreich abgeschlossen sind.
        /// </summary>
        public static void CancelUploadRetry(Context context)
        {
            try
            {
                var workManager = WorkManager.GetInstance(context);
                workManager.CancelUniqueWork(UPLOAD_RETRY_WORK_NAME);

                Log.Info(TAG, "Upload retry work cancelled");
                AppModel.Logger?.Info("Upload-Wiederholungs-Job storniert");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Failed to cancel upload retry work: {ex.Message}");
                AppModel.Logger?.Warn($"UploadRetryScheduler: Fehler beim Stornieren - {ex.Message}");
            }
        }

        /// <summary>
        /// Prüft ob ein Upload-Retry-Job geplant ist.
        /// </summary>
        public static bool IsUploadRetryScheduled(Context context)
        {
            try
            {
                var workManager = WorkManager.GetInstance(context);
                var workInfosFuture = workManager.GetWorkInfosForUniqueWork(UPLOAD_RETRY_WORK_NAME);
                var workInfosObj = workInfosFuture.Get();

                // Cast zu IList<WorkInfo>
                if (workInfosObj is not IList<WorkInfo> workInfos || workInfos.Count == 0)
                    return false;

                // Prüfe ob mindestens ein Job enqueued oder running ist
                foreach (var workInfo in workInfos)
                {
                    // WorkInfo.State ist die Property, nicht der Typ
                    var state = workInfo.GetState();
                    if (state == WorkInfo.State.Enqueued || state == WorkInfo.State.Running)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Failed to check upload retry status: {ex.Message}");
                return false;
            }
        }
    }
}
