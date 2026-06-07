using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using iPMCloud.Mobile.vo;
using System;
using System.Threading;
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

        private int _mainActivityLaunchStarted;

        #region Lifecycle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Log.Info(TAG, $"SplashActivity.OnCreate start (savedInstanceState={(savedInstanceState != null ? "available" : "null")})");
                base.OnCreate(savedInstanceState);
                
                SetFullscreenMode();
                
                Log.Debug(TAG, "SplashActivity.OnCreate complete");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SplashActivity.OnCreate Error: {ex}");
                
                StartMainActivityImmediate();
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                Log.Debug(TAG, "SplashActivity.OnResume");
                SetFullscreenMode();
                TryStartMainActivity();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SplashActivity.OnResume Error: {ex}");
                StartMainActivityImmediate();
            }
        }

        #endregion

        #region UI Configuration
        private void SetFullscreenMode()
        {
            try
            {
                if (Window?.DecorView == null) return;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    var controller = Window.InsetsController;
                    controller?.Hide(WindowInsets.Type.StatusBars());
                    if (controller != null)
                    {
                        controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                    }
                }
                else
                {
                    var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
                    if (controller != null)
                    {
                        controller.Hide(WindowInsetsCompat.Type.StatusBars());
                        controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
                    }
                }

                // Transparente Bars
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SetFullscreenMode Error: {ex}");
            }
        }
        //private void SetFullscreenMode()
        //{
        //    try
        //    {
        //        if (Window?.DecorView == null)
        //        {
        //            Log.Warn(TAG, "Window.DecorView ist null");
        //            return;
        //        }

        //        var uiOptions = (int)Window.DecorView.SystemUiVisibility;
        //        uiOptions |= (int)SystemUiFlags.LowProfile;
        //        uiOptions |= (int)SystemUiFlags.Fullscreen;
        //        uiOptions |= (int)SystemUiFlags.HideNavigation;
        //        uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
        //        uiOptions |= (int)SystemUiFlags.LayoutStable;
        //        uiOptions |= (int)SystemUiFlags.LayoutFullscreen;
        //        uiOptions |= (int)SystemUiFlags.LayoutHideNavigation;

        //        Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;

        //        // Status Bar transparent
        //        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        //        {
        //            Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(TAG, $"SetFullscreenMode Error: {ex.Message}");
        //    }
        //}

        #endregion

        #region Navigation

        private void TryStartMainActivity()
        {
            if (Interlocked.Exchange(ref _mainActivityLaunchStarted, 1) == 1)
            {
                Log.Debug(TAG, "MainActivity-Start bereits geplant");
                return;
            }

            _ = StartMainActivityAsync();
        }

        private async Task StartMainActivityAsync()
        {
            try
            {
                await Task.Delay(SPLASH_DELAY_MS);

                if (IsFinishing || IsDestroyed)
                {
                    Log.Warn(TAG, "Activity wird bereits beendet");
                    return;
                }

                var mainIntent = new Intent(this, typeof(MainActivity));

                // When MainActivity is already alive (app is in the background), bring it
                // to the foreground without destroying the running MAUI session.  Using
                // ClearTask in that situation kills the live MAUI window and triggers a
                // full re-initialization that races with the still-live process singleton
                // (AppModel) — the root cause of the splash-screen hang on notification tap.
                if (MainActivity.Instance != null)
                {
                    mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                    Log.Info(TAG, "MainActivity bereits aktiv – BringToFront ohne ClearTask");
                    AppModel.Logger?.Info("SplashActivity: MainActivity already running – using SingleTop to bring to front");
                }
                else
                {
                    mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    Log.Info(TAG, "Kalt-Start – ClearTask");
                    AppModel.Logger?.Info("SplashActivity: cold start – using ClearTask");
                }

                if (Intent?.Extras != null)
                {
                    mainIntent.PutExtras(Intent.Extras);
                    
                    Log.Debug(TAG, $"Intent Extras weitergeleitet: {Intent.Extras.KeySet()?.Count ?? 0}");
                    AppModel.Logger?.Info($"SplashActivity: forwarding {Intent.Extras.KeySet()?.Count ?? 0} notification extras to MainActivity");
                }

                StartActivity(mainIntent);
                Finish();
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake) // Android 14 (API 34)
                {
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
                    OverrideActivityTransition(OverrideTransition.Open, 0, 0);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
                }
                else
                {
#pragma warning disable CA1422 // Validate platform compatibility
                    OverridePendingTransition(0, 0);
#pragma warning restore CA1422 // Validate platform compatibility
                }

                Log.Info(TAG, "MainActivity gestartet");
            }
            catch (Exception ex)
            {
                Interlocked.Exchange(ref _mainActivityLaunchStarted, 0);
                Log.Error(TAG, $"StartMainActivityAsync Error: {ex}");
                AppModel.Logger?.Error($"SplashActivity.StartMainActivityAsync failed: {ex.Message}");
                
                StartMainActivityImmediate();
            }
        }

        private void StartMainActivityImmediate()
        {
            try
            {
                Interlocked.Exchange(ref _mainActivityLaunchStarted, 1);

                if (IsFinishing || IsDestroyed) return;

                var mainIntent = new Intent(this, typeof(MainActivity));

                // Apply the same smart-flag logic as StartMainActivityAsync to avoid
                // destroying a live MAUI session in the fallback path.
                if (MainActivity.Instance != null)
                {
                    mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                }
                else
                {
                    mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                }

                if (Intent?.Extras != null)
                {
                    mainIntent.PutExtras(Intent.Extras);
                }

                StartActivity(mainIntent);
                Finish();
                
                Log.Info(TAG, "MainActivity gestartet (Fallback)");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"StartMainActivityImmediate Error: {ex}");
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