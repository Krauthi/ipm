using System;

namespace iPMCloud.Mobile.Services
{
    public class UploadProgressEventArgs : EventArgs
    {
        public double ProgressPercent { get; set; }

        /// <summary>
        /// Human-readable status text for UI/notification, e.g. "UPLOADS: Positionen (3/12)".
        /// </summary>
        public string StatusText { get; set; }
        public int ProcessedJobs { get; set; }
        public int TotalJobs { get; set; }
    }

    public class UploadCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }

        /// <summary>
        /// Optional error message when <see cref="Success"/> is false.
        /// </summary>
        public string ErrorMessage { get; set; }
        public int ProcessedJobs { get; set; }
        public int TotalJobs { get; set; }
    }
}
