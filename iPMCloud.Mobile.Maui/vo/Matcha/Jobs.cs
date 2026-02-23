// TODO: Matcha.BackgroundService not MAUI-compatible
// Consider: Native Android WorkManager / iOS Background Tasks
// using Matcha.BackgroundService;
using System;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.vo
{
    // TODO: All classes in this file (SendUpload, LocationInfo) use Matcha.BackgroundService 
    // which is not MAUI-compatible and have been removed temporarily
    // Jobs class kept as placeholder
    public class Jobs
    {     
    }

    public class LocationInfo : IPeriodicTask
    {
        public bool GpsIsRunning { get; set; } = false;
        public LocationInfo(int seconds)
        {
            Interval = TimeSpan.FromSeconds(seconds);
        }

        public TimeSpan Interval { get; set; }

        public async Task<bool> StartJob()
        {
            if (!AppModel.Instance.isInBackground && !AppModel.Instance.GpsIsRunning
                && AppModel.Instance.SettingModel.SettingDTO.GPSInfoHasShow)
            {
                try
                {
                    if (!GpsIsRunning)
                    {
                        //AppModel.Logger.Info("Ping GPS");
                        GpsIsRunning = true;

                        AppModel.Instance.CheckPermissionGPS();
                        if (String.IsNullOrWhiteSpace(AppModel.Instance.checkPermissionGPSMessage))
                        {
                            //    return true;
                            //}
                            //if (CheckPermissionGPS().Result)
                            //{
                            AppModel.Instance.SetLocationGPS(true);// GetGeoLocation().Result;
                        }
                        GpsIsRunning = false;
                    }
                }
                catch (Exception ex)
                {
                    GpsIsRunning = false;
                    AppModel.Logger.Error("ERROR: Ping GPS in JOB: " + ex.Message);
                }
            }
            return true;
        }
    }
}
