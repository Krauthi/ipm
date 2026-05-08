using Android.Content;
using Android.OS;
using Android.Util;
using iPMCloud.Mobile.Platforms.Android.Services;
using iPMCloud.Mobile.Services;

namespace iPMCloud.Mobile.Platforms.Android
{
    /// <summary>
    /// Android implementation of ISyncService.
    /// Starts and stops the SyncForegroundService.
    /// </summary>
    public class AndroidSyncService : ISyncService
    {
        private const string TAG = "AndroidSyncService";

        public void StartSync(bool manuellSync = false)
        {
            try
            {
                var context = global::Android.App.Application.Context;
                var intent = new Intent(context, typeof(SyncForegroundService));
                intent.SetAction(SyncForegroundService.ACTION_START);
                intent.PutExtra(SyncForegroundService.EXTRA_MANUELL, manuellSync);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    context.StartForegroundService(intent);
                else
                    context.StartService(intent);

                Log.Info(TAG, $"SyncForegroundService started (manuell={manuellSync})");
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"StartSync error: {ex.Message}");
                AppModel.Logger?.Error($"AndroidSyncService.StartSync: {ex.Message}");
            }
        }

        public void StopSync()
        {
            try
            {
                var context = global::Android.App.Application.Context;
                var intent = new Intent(context, typeof(SyncForegroundService));
                intent.SetAction(SyncForegroundService.ACTION_STOP);
                context.StartService(intent);
                Log.Info(TAG, "SyncForegroundService stop requested");
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"StopSync error: {ex.Message}");
                AppModel.Logger?.Error($"AndroidSyncService.StopSync: {ex.Message}");
            }
        }
    }
}
