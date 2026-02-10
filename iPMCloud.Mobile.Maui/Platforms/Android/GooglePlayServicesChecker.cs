using Android.App;
using Android.Content;
using Android.Gms.Common;

namespace iPMCloud.Mobile.Platforms.Android.Services
{
    public static class GooglePlayServicesChecker
    {
        private const int PLAY_SERVICES_RESOLUTION_REQUEST = 9000;

        /// <summary>
        /// Prüft ob Google Play Services verfügbar sind
        /// </summary>
        public static bool IsAvailable(Activity activity)
        {
            try
            {
                var googleApiAvailability = GoogleApiAvailability.Instance;
                int resultCode = googleApiAvailability.IsGooglePlayServicesAvailable(activity);

                if (resultCode != ConnectionResult.Success)
                {
                    if (googleApiAvailability.IsUserResolvableError(resultCode))
                    {
                        string errorString = googleApiAvailability.GetErrorString(resultCode);
                        System.Diagnostics.Debug.WriteLine($"Google Play Services Error: {errorString}");

                        // Optional: Dialog anzeigen
                        ShowErrorDialog(activity, googleApiAvailability, resultCode);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Device not supported for Google Play Services");
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking Play Services: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Zeigt einen Dialog an um Play Services Probleme zu beheben
        /// </summary>
        private static void ShowErrorDialog(Activity activity, GoogleApiAvailability apiAvailability, int resultCode)
        {
            try
            {
                var dialog = apiAvailability.GetErrorDialog(activity, resultCode, PLAY_SERVICES_RESOLUTION_REQUEST);
                dialog?.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Zeigt eine Notification an um Play Services zu updaten
        /// </summary>
        public static void ShowErrorNotification(Context context, int resultCode)
        {
            try
            {
                var googleApiAvailability = GoogleApiAvailability.Instance;
                googleApiAvailability.ShowErrorNotification(context, resultCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing notification: {ex.Message}");
            }
        }
    }
}