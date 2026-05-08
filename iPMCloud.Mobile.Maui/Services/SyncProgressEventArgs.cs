using System;

namespace iPMCloud.Mobile.Services
{
    public class SyncProgressEventArgs : EventArgs
    {
        public double ProgressPercent { get; set; }
        public string StatusText { get; set; }
    }

    public class SyncCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public IpmNewSyncResponse Response { get; set; }
        public string ErrorMessage { get; set; }
    }
}
