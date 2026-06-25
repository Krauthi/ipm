using System;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Platform-specific service to prevent sync interruption when the app
    /// goes to background or the device enters sleep mode.
    /// Android: Uses Foreground Service with notification
    /// iOS: Uses Background Task and WakeLock
    /// </summary>
    public interface IBackgroundSyncService
    {
        /// <summary>
        /// Starts the background sync protection.
        /// Should be called before starting a long-running sync operation.
        /// </summary>
        /// <returns>True if successfully started, false otherwise</returns>
        Task<bool> StartSyncProtectionAsync();

        /// <summary>
        /// Stops the background sync protection.
        /// Should be called after sync completion or failure.
        /// </summary>
        Task StopSyncProtectionAsync();

        /// <summary>
        /// Updates the sync progress notification (if supported by platform).
        /// </summary>
        /// <param name="progressText">Progress text to display</param>
        /// <param name="progressPercent">Progress percentage (0-100)</param>
        void UpdateProgress(string progressText, double progressPercent);

        /// <summary>
        /// Indicates if the service is currently active.
        /// </summary>
        bool IsActive { get; }
    }
}
