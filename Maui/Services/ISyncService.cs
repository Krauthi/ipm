namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Platform-specific service for starting/stopping the building sync.
    /// Android: starts a ForegroundService with a PARTIAL_WAKE_LOCK and persistent notification.
    /// iOS: runs the sync inline; see SyncCoordinator for iOS background limitations.
    /// </summary>
    public interface ISyncService
    {
        /// <summary>Starts the sync. On Android this launches the foreground service.</summary>
        void StartSync(bool manuellSync = false);

        /// <summary>Requests cancellation and stops the foreground service (Android).</summary>
        void StopSync();
    }
}
