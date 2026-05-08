using System;

namespace iPMCloud.Mobile.Services
{
    public class UploadProgressEventArgs : EventArgs
    {
        public double ProgressPercent { get; set; }
        public string StatusText { get; set; }
        public int ProcessedJobs { get; set; }
        public int TotalJobs { get; set; }
    }

    public class UploadCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int ProcessedJobs { get; set; }
        public int TotalJobs { get; set; }
    }
}
