namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Platform-specific service for starting/stopping queued uploads.
    /// Android: runs uploads in a foreground service with notification + wake lock.
    /// iOS: best-effort in-process execution.
    /// </summary>
    public interface IUploadService
    {
        void StartUploads();
        void StopUploads();
    }
}
