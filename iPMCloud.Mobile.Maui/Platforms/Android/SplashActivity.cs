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
    [Activity(Label = "iPM-Cloud", Icon = "@drawable/icon", Theme = "@style/MainTheme.Splash", Exported = true,
        MainLauncher = true, NoHistory = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "iPM-ClouD Mobile V2.0.0: " + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            try
            {
                //Hide Bottom NavigationBar 
                int uiOptions = (int)Window.DecorView.SystemUiVisibility;
                uiOptions |= (int)SystemUiFlags.LowProfile;
                uiOptions |= (int)SystemUiFlags.Fullscreen;
                uiOptions |= (int)SystemUiFlags.HideNavigation;
                uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;

                base.OnCreate(savedInstanceState, persistentState);
                Log.Debug(TAG, "SplashActivity.OnCreate");
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, "SplashActivity.OnCreate.catch:" + ex.ToString());
            }
        }

        // Launches the startup task
        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                Task startupWork = new Task(() => { SimulateStartup(); });
                startupWork.Start();
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, "SplashActivity.OnResume:" + ex.ToString());
            }
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed()
        {
        }


        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        //{
        //    Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}

        // Simulates background work that happens behind the splash screen
        async void SimulateStartup()
        {
            try
            {
                await Task.Delay(100);
                var mainintent = new Intent(Application.Context, typeof(MainActivity));
                if (Intent.Extras != null) { mainintent.PutExtras(Intent.Extras); }
                StartActivity(mainintent);
                //StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, "SplashActivity.SimulateStartup:" + ex.ToString());
            }
        }

    }
}