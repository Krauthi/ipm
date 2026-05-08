using System;

namespace iPMCloud.Mobile.Services
{
    public class SyncProgressEventArgs : EventArgs
    {
        /// <summary>Current sync progress from 0 to 100.</summary>
        public double ProgressPercent { get; set; }

        /// <summary>Human-readable status text, e.g. "SYNCHRONISATION (35%)".</summary>
        public string StatusText { get; set; }
    }

    public class SyncCompletedEventArgs : EventArgs
    {
        /// <summary>True when all building chunks were successfully synced.</summary>
        public bool Success { get; set; }

        /// <summary>
        /// The server response. May be null on network failures.
        /// When <see cref="Success"/> is false, this may still contain a partial response
        /// (e.g. with <c>success=false</c> and an error message from the server).
        /// </summary>
        public IpmNewSyncResponse Response { get; set; }

        /// <summary>Human-readable error message when <see cref="Success"/> is false.</summary>
        public string ErrorMessage { get; set; }
    }
}
