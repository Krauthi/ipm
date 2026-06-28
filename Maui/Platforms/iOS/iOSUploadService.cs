using iPMCloud.Mobile.Services;
using System.Threading;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.Platforms.iOS
{
    /// <summary>
    /// iOS best-effort upload service.
    ///
    /// iOS limitation: user-triggered, long-running HTTP uploads are not guaranteed to
    /// continue when the app is backgrounded or terminated. iOS may suspend/kill the
    /// process shortly after backgrounding. For this reason uploads are executed
    /// in-process while the app is active.
    /// </summary>
    public class iOSUploadService : IUploadService
    {
        private CancellationTokenSource _cts;

        public void StartUploads()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            Task.Run(() => UploadCoordinator.Instance.RunAsync(_cts.Token));
        }

        public void StopUploads()
        {
            _cts?.Cancel();
        }
    }
}
