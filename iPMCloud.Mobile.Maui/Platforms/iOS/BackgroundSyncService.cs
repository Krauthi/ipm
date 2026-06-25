using Foundation;
using iPMCloud.Mobile.Services;
using System;
using System.Threading.Tasks;
using UIKit;

namespace iPMCloud.Mobile.Platforms.iOS
{
    /// <summary>
    /// iOS implementation of IBackgroundSyncService.
    /// Uses UIApplication.BeginBackgroundTask to request extra time when app enters background,
    /// and IdleTimerDisabled to prevent the device from sleeping during sync.
    /// 
    /// Note: iOS has strict background execution limitations. BeginBackgroundTask gives us
    /// approximately 30 seconds (up to 3 minutes) of background time. For longer operations,
    /// the sync must complete before this time expires, or iOS will terminate it.
    /// </summary>
    public class BackgroundSyncService : IBackgroundSyncService
    {
        private bool _isActive;
        private bool _wasIdleTimerDisabled;
        private nint _backgroundTaskId = UIApplication.BackgroundTaskInvalid;

        public bool IsActive => _isActive;

        /// <summary>
        /// Prevents the device from auto-locking and requests background execution time.
        /// The screen will stay on during the sync operation, and if the app goes to background,
        /// iOS will grant extra time to complete the sync.
        /// </summary>
        public Task<bool> StartSyncProtectionAsync()
        {
            try
            {
                // Remember the original idle timer state
                _wasIdleTimerDisabled = UIApplication.SharedApplication.IdleTimerDisabled;

                // Disable idle timer to keep the device awake while in foreground
                UIApplication.SharedApplication.IdleTimerDisabled = true;

                // Request background execution time in case the app goes to background
                _backgroundTaskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
                {
                    // This callback is invoked when time expires or the task completes
                    vo.AppModel.Logger?.Warn("iOS Background task time expired - sync may be interrupted");
                    EndBackgroundTask();
                });

                _isActive = true;
                vo.AppModel.Logger?.Info($"BackgroundSyncService (iOS): Protection active (IdleTimer disabled, BackgroundTask: {_backgroundTaskId})");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                vo.AppModel.Logger?.Error($"BackgroundSyncService.StartSyncProtectionAsync (iOS): {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Restores the idle timer to its original state and ends the background task.
        /// The device can now auto-lock again.
        /// </summary>
        public Task StopSyncProtectionAsync()
        {
            try
            {
                // Restore the original idle timer state
                UIApplication.SharedApplication.IdleTimerDisabled = _wasIdleTimerDisabled;

                // End the background task
                EndBackgroundTask();

                _isActive = false;
                vo.AppModel.Logger?.Info("BackgroundSyncService (iOS): Protection stopped (IdleTimer restored, BackgroundTask ended)");
            }
            catch (Exception ex)
            {
                vo.AppModel.Logger?.Error($"BackgroundSyncService.StopSyncProtectionAsync (iOS): {ex.Message}");
                _isActive = false;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Ends the background task if one is active.
        /// </summary>
        private void EndBackgroundTask()
        {
            if (_backgroundTaskId != UIApplication.BackgroundTaskInvalid)
            {
                UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
                _backgroundTaskId = UIApplication.BackgroundTaskInvalid;
            }
        }

        /// <summary>
        /// iOS does not have a built-in progress notification during foreground operations.
        /// This method is a no-op on iOS. Progress is displayed in the app UI.
        /// </summary>
        public void UpdateProgress(string progressText, double progressPercent)
        {
            // iOS doesn't show progress notifications while the app is in the foreground.
            // Progress is displayed via the MainPage UI (popupContainer_count).
            // No additional action needed here.
        }
    }
}
