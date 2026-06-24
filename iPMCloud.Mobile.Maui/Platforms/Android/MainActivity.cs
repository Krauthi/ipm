using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.Platforms.Android.Services;
using iPMCloud.Mobile.Services;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile
{
    [Activity(
        Label = "iPM-Cloud", 
        //Icon = "@drawable/icon",  
        Theme = "@style/Maui.SplashTheme",
        Exported = true,
        MainLauncher = false, 
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | 
                               ConfigChanges.Orientation | 
                               ConfigChanges.UiMode | 
                               ConfigChanges.ScreenLayout | 
                               ConfigChanges.SmallestScreenSize | 
                               ConfigChanges.Density,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        #region Fields & Properties
        
        public AppModel model;
        public App app;
        public static MainActivity Instance { get; private set; }

        internal static readonly string CHANNEL_ID = "ipmcloud_message_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        private static readonly string TAG = "IPM-CLOUD-MainActivity";
        private const long SystemUiDebounceMs = 250;

        private bool _systemUiHandlerAttached;
        private bool _pendingSystemUiApply;
        private bool _isApplyingSystemUi;
        private long _lastSystemUiApplyAt;

        #endregion

        #region Lifecycle Methods

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                //Log.Info(TAG, $"OnCreate start (savedInstanceState={(savedInstanceState != null ? "available" : "null")})");

                base.OnCreate(savedInstanceState);

                Instance = this;

                Platform.Init(this, savedInstanceState);
                InitializeNLog();

                model = AppModel.Instance;
                model.HasInitAppmodel = model.InitAppModel();

                //InitFontScale();
                ConfigureUI();
                LogDeferredPermissionStrategy();
                CreateNotificationChannel();

                if (!GooglePlayServicesChecker.IsAvailable(this))
                {
                    Log.Warn(TAG, "Google Play Services nicht verfügbar");
                }

                //Log.Info(TAG, "MainActivity erfolgreich initialisiert");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Fatal OnCreate Error: {ex}");
                AppModel.Logger?.Error($"MainActivity OnCreate fatal: {ex}");
                FinishAfterFatalInitializationError();
            }
        }


        //protected override void AttachBaseContext(Android.Content.Context @base)
        //{
        //    // Setzt den FontScale und die Density fest auf den Standardwert (1.0)
        //    // Dadurch werden Änderungen in den Android-Einstellungen ignoriert.
        //    Configuration configuration = new(@base.Resources.Configuration)
        //    {
        //        FontScale = 1.0f,
        //        DensityDpi = (int)(@base.Resources.DisplayMetrics.Density * 160)
        //    };

        //    base.AttachBaseContext(@base.CreateConfigurationContext(configuration));
        //}
        protected override void AttachBaseContext(Context @base)
        {
            var configuration = new Configuration(@base.Resources.Configuration)
            {
                FontScale = 1.0f,
                //DensityDpi = (int)(@base.Resources.DisplayMetrics.Density * 160)
            };

            var context = @base.CreateConfigurationContext(configuration);

            //var metrics = context.Resources.DisplayMetrics;
            //metrics.Density = 1.0f;
            //metrics.ScaledDensity = 1.0f;
            
            //metrics.DensityDpi = DisplayMetricsDensity.Xxhigh;

            base.AttachBaseContext(context);
        }

        public override void ApplyOverrideConfiguration(Configuration? overrideConfiguration)
        {
            if (overrideConfiguration != null)
            {
                overrideConfiguration.FontScale = 1.0f;
                //overrideConfiguration.DensityDpi = 160;
            }

            base.ApplyOverrideConfiguration(overrideConfiguration);
        }



        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Always update the activity's current intent so subsequent calls to
            // Intent property return the notification-triggered intent rather than
            // the original launch intent.
            Intent = intent;
            
            try
            {
                if (intent?.Extras != null && intent.Extras.KeySet()?.Count > 0)
                {
                    var extras = string.Join("; ",
                        intent.Extras.KeySet().Select(k => $"{k}={intent.Extras.GetString(k)}"));
                    Log.Debug(TAG, $"OnNewIntent Extras: {extras}");
                    AppModel.Logger?.Info($"Notification tap (OnNewIntent): {extras}");
                }
                else
                {
                    Log.Debug(TAG, "OnNewIntent called without notification extras");
                    AppModel.Logger?.Info("OnNewIntent: no notification extras present");
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnNewIntent Error: {ex}");
                AppModel.Logger?.Error($"OnNewIntent failed: {ex.Message}");
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            //Log.Debug(TAG, "OnStart");
        }

        protected override void OnResume()
        {
            base.OnResume();
            PushNotificationService.EnsureAndroidNotificationPermissionRequest();
            ApplySystemBarColors();
            ScheduleSystemUiUpdate("OnResume");
            //Log.Debug(TAG, "OnResume");
        }

        protected override void OnPause()
        {
            base.OnPause();
            //Log.Debug(TAG, "OnPause");
        }

        protected override void OnStop()
        {
            base.OnStop();
            //Log.Debug(TAG, "OnStop");
        }

        protected override void OnDestroy()
        {
            try
            {
                DetachSystemUiVisibilityHandler();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnDestroy Error: {ex}");
            }
            
            base.OnDestroy();
            //Log.Debug(TAG, "OnDestroy");
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            //Log.Debug(TAG, $"OnWindowFocusChanged: hasFocus={hasFocus}");
            //if(AppModel.Logger != null && AppModel.Instance != null)
            //{
            //    AppModel.Logger.Info(TAG, $"OnWindowFocusChanged: hasFocus={hasFocus}");
            //}

            if (hasFocus)
            {
                ScheduleSystemUiUpdate("OnWindowFocusChanged", force: true);
            }
        }

        #endregion

        #region Configuration Methods

        private void InitFontScale()
        {
            try
            {
                Configuration configuration = Resources?.Configuration;
                if (configuration == null) return;

                configuration.FontScale = 1.00f; // Fixed font scale
                DisplayMetrics metrics = new DisplayMetrics();
                WindowManager?.DefaultDisplay?.GetMetrics(metrics);

                if (metrics != null)
                {
                    try
                    {
                        metrics.ScaledDensity = configuration.FontScale * metrics.Density;
                        BaseContext?.Resources?.UpdateConfiguration(configuration, metrics);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(TAG, $"InitFontScale Inner Error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"InitFontScale Error: {ex}");
            }
        }

        private void ConfigureUI()
        {
            try
            {
                ApplySystemBarColors();
                AttachSystemUiVisibilityHandlerIfNeeded();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"ConfigureUI Error: {ex}");
            }
        }

        private void ScheduleSystemUiUpdate(string source, bool force = false)
        {
            try
            {
                var decorView = Window?.DecorView;
                if (decorView == null || IsFinishing || IsDestroyed)
                {
                    return;
                }

                if (_pendingSystemUiApply && !force)
                {
                    return;
                }

                _pendingSystemUiApply = true;
                decorView.Post(() =>
                {
                    _pendingSystemUiApply = false;
                    HideNavAndStatusBar(source, force);
                });
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"ScheduleSystemUiUpdate Error ({source}): {ex}");
                if (AppModel.Logger != null && AppModel.Instance != null)
                {
                    AppModel.Logger.Error(TAG, $"ScheduleSystemUiUpdate Error ({source}): {ex}");
                }
            }
        }

        private void HideNavAndStatusBar(string source, bool force = false)
        {
            try
            {
                if (Window?.DecorView == null || IsFinishing || IsDestroyed || _isApplyingSystemUi)
                {
                    return;
                }

                var now = SystemClock.ElapsedRealtime();
                if (!force && now - _lastSystemUiApplyAt < SystemUiDebounceMs)
                {
                    return;
                }

                _isApplyingSystemUi = true;
                ApplySystemBarColors();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11+ (API 30)
                {
                    var windowInsetsController = Window.InsetsController;

                    if (windowInsetsController != null)
                    {
                        try
                        {
                            windowInsetsController.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                            windowInsetsController.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    try
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        var uiOptions = (int)Window.DecorView.SystemUiVisibility;
                        uiOptions |= (int)SystemUiFlags.LayoutStable;
                        uiOptions |= (int)SystemUiFlags.LayoutHideNavigation;
                        uiOptions |= (int)SystemUiFlags.LayoutFullscreen;
                        uiOptions |= (int)SystemUiFlags.HideNavigation;
                        uiOptions |= (int)SystemUiFlags.Fullscreen;
                        uiOptions |= (int)SystemUiFlags.ImmersiveSticky;

                        Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
#pragma warning restore CS0618
                    } catch (Exception) { }
                }

                _lastSystemUiApplyAt = now;
                //Log.Debug(TAG, $"System UI applied from {source}");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"HideNavAndStatusBar Error ({source}): {ex}");
            }
            finally
            {
                _isApplyingSystemUi = false;
            }
        }

        //private void HideNavAndStatusBar_OLD()
        //{
        //    try
        //    {
        //        if (Window?.DecorView == null) return;

        //        var uiOptions = (int)Window.DecorView.SystemUiVisibility;
        //        uiOptions |= (int)SystemUiFlags.LayoutStable;
        //        uiOptions |= (int)SystemUiFlags.LayoutHideNavigation;
        //        uiOptions |= (int)SystemUiFlags.LayoutFullscreen;
        //        uiOptions |= (int)SystemUiFlags.HideNavigation;
        //        uiOptions |= (int)SystemUiFlags.Fullscreen;
        //        uiOptions |= (int)SystemUiFlags.ImmersiveSticky;

        //        Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(TAG, $"HideNavAndStatusBar Error: {ex.Message}");
        //    }
        //}

        private void DecorView_SystemUiVisibilityChange(object sender, Android.Views.View.SystemUiVisibilityChangeEventArgs e)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R || _isApplyingSystemUi)
                {
                    return;
                }

                ApplySystemBarColors();

#pragma warning disable CS0618 // Type or member is obsolete
                var isNavigationVisible = (((SystemUiFlags)e.Visibility) & SystemUiFlags.HideNavigation) == 0;
                var isStatusVisible = (((SystemUiFlags)e.Visibility) & SystemUiFlags.Fullscreen) == 0;
#pragma warning restore CS0618

                if (isNavigationVisible || isStatusVisible)
                {
                    ScheduleSystemUiUpdate("SystemUiVisibilityChange");
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"DecorView_SystemUiVisibilityChange Error: {ex}");
            }
        }

        #endregion

        #region Permissions

        public override void OnRequestPermissionsResult(
            int requestCode, 
            string[] permissions, 
            [GeneratedEnum] Permission[] grantResults)
        {
            try
            {
                Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                // Log granted/denied permissions
                var resultCount = Math.Min(permissions?.Length ?? 0, grantResults?.Length ?? 0);
                if (resultCount != (permissions?.Length ?? 0))
                {
                    Log.Warn(TAG, $"Permission result count mismatch: permissions={permissions?.Length ?? 0}, grantResults={grantResults?.Length ?? 0}");
                }

                for (int i = 0; i < resultCount; i++)
                {
                    var granted = grantResults[i] == Permission.Granted;
                    Log.Debug(TAG, $"Permission {permissions[i]}: {(granted ? "Granted" : "Denied")}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnRequestPermissionsResult Error: {ex}");
            }
        }

        #endregion

        #region Notifications

        public void CreateNotificationChannel()
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    Log.Warn(TAG, "Notification channels nicht unterstützt (API < 26)");
                    return;
                }

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                
                // Prüfen ob Channel bereits existiert
                if (notificationManager?.GetNotificationChannel(CHANNEL_ID) != null)
                {
                    Log.Debug(TAG, "Notification Channel existiert bereits");
                    return;
                }

                var channel = new NotificationChannel(
                    CHANNEL_ID, 
                    "iPM Cloud Benachrichtigungen", 
                    NotificationImportance.High)
                {
                    Description = "Wichtige Benachrichtigungen von iPM Cloud Mobile"
                };

                channel.EnableLights(true);
                channel.LightColor = Android.Graphics.Color.ParseColor("#0078D4");
                channel.EnableVibration(true);
                channel.SetVibrationPattern(new long[] { 0, 500, 250, 500 });
                channel.LockscreenVisibility = NotificationVisibility.Public;

                notificationManager?.CreateNotificationChannel(channel);
                
                Log.Info(TAG, "Notification Channel erstellt");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"CreateNotificationChannel Error: {ex}");
            }
        }

        #endregion

        #region Logging

        private void InitializeNLog()
        {
            try
            {
                // NLog is initialized early in MauiProgram.CreateMauiApp().
                // This call is kept for safety but is a no-op if already configured.
                var assembly = GetType().Assembly;
                var assemblyName = assembly.GetName().Name;
                new LogService().Initialize(assembly, assemblyName);
                
                Log.Info(TAG, "NLog initialisiert");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"InitializeNLog Error: {ex}");
            }
        }

        #endregion

        #region Back Button Handling
        public override void OnBackPressed()
        {
            //Log.Debug(TAG, "Back Button ignoriert");
        }
        //public override void OnBackPressed()
        //{
        //    // Custom back button handling hier einfügen
        //    // Beispiel: Popups schließen, Navigation zurück, etc.

        //    // Standard-Verhalten (App schließen)
        //    base.OnBackPressed();
        //}

        #endregion

        #region Activity Result (für Camera/Gallery)

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                
                //Log.Debug(TAG, $"OnActivityResult: RequestCode={requestCode}, ResultCode={resultCode}");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnActivityResult Error: {ex}");
            }
        }

        private void ApplySystemBarColors()
        {
            if (Window == null || Build.VERSION.SdkInt >= (BuildVersionCodes)35)
            {
                return;
            }
            try
            {
                Window.SetNavigationBarColor(Android.Graphics.Color.Black);
                Window.SetStatusBarColor(Android.Graphics.Color.Black);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"ApplySystemBarColors Error: {ex}");
            }
        }

        private void AttachSystemUiVisibilityHandlerIfNeeded()
        {
            if (_systemUiHandlerAttached || Build.VERSION.SdkInt >= BuildVersionCodes.R || Window?.DecorView == null)
            {
                return;
            }

            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
            _systemUiHandlerAttached = true;
        }

        private void DetachSystemUiVisibilityHandler()
        {
            if (!_systemUiHandlerAttached || Window?.DecorView == null)
            {
                return;
            }

            Window.DecorView.SystemUiVisibilityChange -= DecorView_SystemUiVisibilityChange;
            _systemUiHandlerAttached = false;
        }

        private static void LogDeferredPermissionStrategy()
        {
            //Log.Info(TAG, "Runtime permissions are requested contextually after startup; no blanket startup permission request is performed.");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                //Log.Info(TAG, "Android 13+ notification permission remains deferred until foreground sync or upload requires it.");
            }
        }

        private void FinishAfterFatalInitializationError()
        {
            try
            {
                DetachSystemUiVisibilityHandler();

                if (!IsFinishing)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        FinishAndRemoveTask();
                    }
                    else
                    {
                        Finish();
                    }
                }
            }
            catch (Exception finishEx)
            {
                Log.Error(TAG, $"FinishAfterFatalInitializationError Error: {finishEx}");
            }
        }

        #endregion
    }
}
