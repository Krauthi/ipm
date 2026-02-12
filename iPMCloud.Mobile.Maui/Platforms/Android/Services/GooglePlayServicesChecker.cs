using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Util;
using System;

namespace iPMCloud.Mobile.Platforms.Android.Services
{
    public static class GooglePlayServicesChecker
    {
        private static readonly string TAG = "GooglePlayServicesChecker";

        public static bool IsAvailable(Activity activity)
        {
            try
            {
                var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(activity);
                
                if (resultCode != ConnectionResult.Success)
                {
                    Log.Warn(TAG, $"Google Play Services nicht verfügbar: {resultCode}");
                    
                    if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    {
                        // Zeige Dialog zum Update
                        var dialog = GoogleApiAvailability.Instance.GetErrorDialog(activity, resultCode, 9000);
                        dialog?.Show();
                    }
                    
                    return false;
                }
                
                Log.Info(TAG, "Google Play Services verfügbar");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"IsAvailable Error: {ex.Message}");
                return false;
            }
        }
    }
}