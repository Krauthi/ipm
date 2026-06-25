using Android.Content;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.Platforms.Android.Services;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Platforms.Android
{
    /// <summary>
    /// Android implementation of IBackgroundSyncService.
    /// Wraps the SyncForegroundService to provide a unified interface.
    /// </summary>
    public class BackgroundSyncService : IBackgroundSyncService
    {
        private bool _isActive;

        public bool IsActive => _isActive;

        /// <summary>
        /// Starts the Android Foreground Service to protect the sync from being interrupted.
        /// </summary>
        public Task<bool> StartSyncProtectionAsync()
        {
            try
            {
                var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;
                if (context == null)
                {
                    vo.AppModel.Logger?.Error("BackgroundSyncService: No context available");
                    return Task.FromResult(false);
                }

                var intent = new Intent(context, typeof(SyncForegroundService));
                intent.SetAction(SyncForegroundService.ACTION_START);
                intent.PutExtra(SyncForegroundService.EXTRA_MANUELL, true);

                // Android O (API 26+) requires startForegroundService
                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
                {
                    context.StartForegroundService(intent);
                }
                else
                {
                    context.StartService(intent);
                }

                _isActive = true;
                vo.AppModel.Logger?.Info("BackgroundSyncService: Foreground service started");
                return Task.FromResult(true);
            }
            catch (System.Exception ex)
            {
                vo.AppModel.Logger?.Error($"BackgroundSyncService.StartSyncProtectionAsync: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Stops the Android Foreground Service.
        /// </summary>
        public Task StopSyncProtectionAsync()
        {
            try
            {
                var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;
                if (context == null)
                {
                    vo.AppModel.Logger?.Warn("BackgroundSyncService: No context available for stop");
                    _isActive = false;
                    return Task.CompletedTask;
                }

                var intent = new Intent(context, typeof(SyncForegroundService));
                intent.SetAction(SyncForegroundService.ACTION_STOP);
                context.StartService(intent);

                _isActive = false;
                vo.AppModel.Logger?.Info("BackgroundSyncService: Foreground service stopped");
            }
            catch (System.Exception ex)
            {
                vo.AppModel.Logger?.Error($"BackgroundSyncService.StopSyncProtectionAsync: {ex.Message}");
                _isActive = false;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates the sync progress notification.
        /// The SyncForegroundService listens to SyncCoordinator events automatically,
        /// so this method is a no-op (progress is already handled).
        /// </summary>
        public void UpdateProgress(string progressText, double progressPercent)
        {
            // Progress updates are automatically handled by SyncForegroundService 
            // via SyncCoordinator.ProgressChanged event subscription.
            // No additional action needed here.
        }
    }
}
