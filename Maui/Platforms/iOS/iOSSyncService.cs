using iPMCloud.Mobile.Services;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.Platforms.iOS
{
    /// <summary>
    /// iOS implementation of ISyncService.
    ///
    /// iOS Background Limitations:
    /// iOS does not allow arbitrary long-running background tasks initiated by the user.
    /// BGProcessingTask and BGAppRefreshTask are system-scheduled and not suitable for
    /// immediate user-triggered syncs. The sync will run as long as the app is in the
    /// foreground.  If the app is backgrounded mid-sync, iOS may suspend it after a
    /// short grace period (~30 s).
    ///
    /// Mitigation: keep the screen on during sync (DeviceDisplay.KeepScreenOn or
    /// equivalent) and guide users not to background the app while syncing.
    ///
    /// The SyncCoordinator still runs fully on iOS – it just does so in-process
    /// without the foreground-service guarantee that Android provides.
    /// </summary>
    public class iOSSyncService : ISyncService
    {
        private System.Threading.CancellationTokenSource _cts;

        public void StartSync(bool manuellSync = false)
        {
            _cts = new System.Threading.CancellationTokenSource();
            // Run coordinator in background thread; UI progress is handled via events.
            Task.Run(() => SyncCoordinator.Instance.RunAsync(_cts.Token));
        }

        public void StopSync()
        {
            _cts?.Cancel();
        }
    }
}
