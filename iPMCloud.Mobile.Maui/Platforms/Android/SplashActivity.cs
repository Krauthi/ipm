using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Util;
using Android.Views;
using System;
using System.Threading.Tasks;

namespace iPMCloud.Mobile
{
    [Activity(
        Label = "iPM-Cloud", 
        Icon = "@drawable/icon", 
        Theme = "@style/Maui.SplashTheme", 
        Exported = true,
        MainLauncher = true, 
        NoHistory = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "iPM-ClouD Mobile: " + typeof(SplashActivity).Name;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetFullscreenMode();
                Log.Debug(TAG, "SplashActivity.OnCreate");
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, $"SplashActivity.OnCreate.catch:{ex}");
            }
        }

        // Launches the startup task

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                _ = StartMainActivityAsync();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnResume Error: {ex}");
            }
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed()
        {
        }

        private void SetFullscreenMode()
        {
            try
            {
                if (Window?.DecorView == null) return;

                var uiOptions = (int)Window.DecorView.SystemUiVisibility;
                uiOptions |= (int)SystemUiFlags.LowProfile;
                uiOptions |= (int)SystemUiFlags.Fullscreen;
                uiOptions |= (int)SystemUiFlags.HideNavigation;
                uiOptions |= (int)SystemUiFlags.ImmersiveSticky;

                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SetFullscreenMode Error: {ex}");
            }
        }



        private async Task StartMainActivityAsync()
        {
            try
            {
                await Task.Delay(500);

                var mainIntent = new Intent(this, typeof(MainActivity));

                if (Intent?.Extras != null)
                {
                    mainIntent.PutExtras(Intent.Extras);
                }

                StartActivity(mainIntent);
                Finish();  
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"StartMainActivity Error: {ex}");
                                
                try
                {
                    StartActivity(new Intent(this, typeof(MainActivity)));
                    Finish();
                }
                catch { }
            }
        }


    }
}