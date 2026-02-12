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
    /// <summary>
    /// Splash Screen Activity - zeigt Logo beim App-Start
    /// </summary>
    [Activity(
        Label = "iPM-Cloud", 
        //Icon = "@drawable/icon", 
        Theme = "@style/Maui.SplashTheme",
        Exported = true,
        MainLauncher = true,
        NoHistory = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | 
                               ConfigChanges.Orientation |
                               ConfigChanges.UiMode,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {
        private static readonly string TAG = "iPMCloud-SplashActivity";
        private const int SPLASH_DELAY_MS = 500;

        #region Lifecycle

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
                Log.Error(TAG, $"OnCreate Error: {ex.Message}");
                
                // Fallback: Direkt zur MainActivity
                StartMainActivityImmediate();
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                
                Log.Debug(TAG, "SplashActivity.OnResume");
                
                // Asynchroner Start der MainActivity
                _ = StartMainActivityAsync();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnResume Error: {ex.Message}");
                StartMainActivityImmediate();
            }
        }

        #endregion

        #region UI Configuration

        private void SetFullscreenMode()
        {
            try
            {
                if (Window?.DecorView == null)
                {
                    Log.Warn(TAG, "Window.DecorView ist null");
                    return;
                }

                var uiOptions = (int)Window.DecorView.SystemUiVisibility;
                uiOptions |= (int)SystemUiFlags.LowProfile;
                uiOptions |= (int)SystemUiFlags.Fullscreen;
                uiOptions |= (int)SystemUiFlags.HideNavigation;
                uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
                uiOptions |= (int)SystemUiFlags.LayoutStable;
                uiOptions |= (int)SystemUiFlags.LayoutFullscreen;
                uiOptions |= (int)SystemUiFlags.LayoutHideNavigation;

                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
                
                // Status Bar transparent
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SetFullscreenMode Error: {ex.Message}");
            }
        }

        #endregion

        #region Navigation

        private async Task StartMainActivityAsync()
        {
            try
            {
                // Kurze Verzögerung für Logo-Anzeige
                await Task.Delay(SPLASH_DELAY_MS);

                if (IsFinishing || IsDestroyed)
                {
                    Log.Warn(TAG, "Activity wird bereits beendet");
                    return;
                }

                var mainIntent = new Intent(this, typeof(MainActivity));
                mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

                // Intent Extras weiterleiten (z.B. von Notifications)
                if (Intent?.Extras != null)
                {
                    mainIntent.PutExtras(Intent.Extras);
                    
                    Log.Debug(TAG, $"Intent Extras weitergeleitet: {Intent.Extras.KeySet()?.Count ?? 0}");
                }

                StartActivity(mainIntent);
                Finish();
                
                // Smooth Transition
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                
                Log.Info(TAG, "MainActivity gestartet");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"StartMainActivityAsync Error: {ex.Message}");
                
                // Fallback ohne Intent Extras
                StartMainActivityImmediate();
            }
        }

        private void StartMainActivityImmediate()
        {
            try
            {
                if (IsFinishing || IsDestroyed) return;

                var mainIntent = new Intent(this, typeof(MainActivity));
                mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                
                StartActivity(mainIntent);
                Finish();
                
                Log.Info(TAG, "MainActivity gestartet (Fallback)");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"StartMainActivityImmediate Error: {ex.Message}");
            }
        }

        #endregion

        #region Back Button Override

        /// <summary>
        /// Verhindert das Schließen während des Splash Screens
        /// </summary>
        public override void OnBackPressed()
        {
            // Nichts tun - Back Button während Splash deaktiviert
            Log.Debug(TAG, "Back Button während Splash ignoriert");
        }

        #endregion
    }
}