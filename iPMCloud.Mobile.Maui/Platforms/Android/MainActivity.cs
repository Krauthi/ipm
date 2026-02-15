using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.App;
using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.Platforms.Android.Services;
using System;
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

        #endregion

        #region Lifecycle Methods

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Instance = this;
                
                // ✅ MAUI Platform initialisieren (WICHTIG!)
                Platform.Init(this, savedInstanceState);

                // AppModel initialisieren
                model = AppModel.Instance;
                model.Activity = this;
                // AppModel initialisieren
                model.HasInitAppmodel = model.InitAppModel();

                // Font Scale vor base.OnCreate setzen
                InitFontScale();

                // NLog initialisieren
                InitializeNLog();

                // UI Konfiguration
                ConfigureUI();

                // Permissions anfordern
                RequestRequiredPermissions();

                // App Version setzen
                SetAppVersion();


                // Notification Channel erstellen
                CreateNotificationChannel();

                base.OnCreate(savedInstanceState);

                // Google Play Services Check
                if (!GooglePlayServicesChecker.IsAvailable(this))
                {
                    Log.Warn(TAG, "Google Play Services nicht verfügbar");
                }



                // Status Bar Color
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window?.SetStatusBarColor(Android.Graphics.Color.Argb(255, 0, 0, 0));
                }

                Log.Info(TAG, "MainActivity erfolgreich initialisiert");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnCreate Error: {ex.Message}");
                AppModel.Logger?.Error($"MainActivity OnCreate: {ex.Message}");
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            
            try
            {
                // Notification Intent Handling
                if (intent?.Extras != null)
                {
                    foreach (var key in intent.Extras.KeySet())
                    {
                        var value = intent.Extras.GetString(key);
                        Log.Debug(TAG, $"Intent Extra: {key} = {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnNewIntent Error: {ex.Message}");
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            Log.Debug(TAG, "OnStart");
        }

        protected override void OnResume()
        {
            base.OnResume();
            HideNavAndStatusBar();
            Log.Debug(TAG, "OnResume");
        }

        protected override void OnPause()
        {
            base.OnPause();
            Log.Debug(TAG, "OnPause");
        }

        protected override void OnStop()
        {
            base.OnStop();
            Log.Debug(TAG, "OnStop");
        }

        protected override void OnDestroy()
        {
            try
            {
                if (Window?.DecorView != null)
                {
                    Window.DecorView.SystemUiVisibilityChange -= DecorView_SystemUiVisibilityChange;
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnDestroy Error: {ex.Message}");
            }
            
            base.OnDestroy();
            Log.Debug(TAG, "OnDestroy");
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
                    metrics.ScaledDensity = configuration.FontScale * metrics.Density;
                    BaseContext?.Resources?.UpdateConfiguration(configuration, metrics);
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"InitFontScale Error: {ex.Message}");
            }
        }

        private void ConfigureUI()
        {
            try
            {
                HideNavAndStatusBar();
                
                if (Window?.DecorView != null)
                {
                    Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"ConfigureUI Error: {ex.Message}");
            }
        }

        private void HideNavAndStatusBar()
        {
            try
            {
                if (Window?.DecorView == null) return;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11+ (API 30)
                {
                    // ✅ Moderne API: WindowInsetsController
                    var windowInsetsController = Window.InsetsController;

                    if (windowInsetsController != null)
                    {
                        // System Bars verstecken
                        windowInsetsController.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());

                        // Immersive Sticky Mode
                        windowInsetsController.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                    }
                }
                else
                {
                    // ✅ Fallback für Android 10 und älter
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
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"HideNavAndStatusBar Error: {ex.Message}");
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
            HideNavAndStatusBar();
        }

        #endregion

        #region Permissions

        private void RequestRequiredPermissions()
        {
            try
            {
                var permissions = new System.Collections.Generic.List<string>
                {
                    Manifest.Permission.Internet,
                    Manifest.Permission.Camera,
                    Manifest.Permission.AccessWifiState,
                    Manifest.Permission.AccessNetworkState,
                    Manifest.Permission.ChangeNetworkState,
                    Manifest.Permission.Flashlight,
                    Manifest.Permission.ChangeWifiState,
                };

                // Android 13+ Permissions
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    permissions.Add(Manifest.Permission.PostNotifications);
                    permissions.Add(Manifest.Permission.ReadMediaImages);
                    permissions.Add(Manifest.Permission.ReadMediaVideo);
                }
                else
                {
                    // Ältere Android Versionen
                    permissions.Add(Manifest.Permission.ReadExternalStorage);
                    permissions.Add(Manifest.Permission.WriteExternalStorage);
                }

                ActivityCompat.RequestPermissions(this, permissions.ToArray(), 122);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"RequestPermissions Error: {ex.Message}");
            }
        }

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
                for (int i = 0; i < permissions.Length; i++)
                {
                    var granted = grantResults[i] == Permission.Granted;
                    Log.Debug(TAG, $"Permission {permissions[i]}: {(granted ? "Granted" : "Denied")}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnRequestPermissionsResult Error: {ex.Message}");
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
                Log.Error(TAG, $"CreateNotificationChannel Error: {ex.Message}");
            }
        }

        #endregion

        #region Version Info

        private void SetAppVersion()
        {
            try
            {
                model.Version = GetVersion();
                model.Build = GetBuild().ToString();
                
                Log.Info(TAG, $"App Version: {model.Version}, Build: {model.Build}");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SetAppVersion Error: {ex.Message}");
            }
        }

        public string GetVersion()
        {
            try
            {
                var context = Android.App.Application.Context;
                var manager = context.PackageManager;
                var info = manager?.GetPackageInfo(context.PackageName, 0);
                return info?.VersionName ?? "Unknown";
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"GetVersion Error: {ex.Message}");
                return "Unknown";
            }
        }

        public int GetBuild()
        {
            try
            {
                var context = Android.App.Application.Context;
                var manager = context.PackageManager;
                var info = manager?.GetPackageInfo(context.PackageName, 0);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                {
                    return (int)(info?.LongVersionCode ?? 0);
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    return info?.VersionCode ?? 0;
#pragma warning restore CS0618
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"GetBuild Error: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region Logging

        private void InitializeNLog()
        {
            try
            {
                var assembly = GetType().Assembly;
                var assemblyName = assembly.GetName().Name;
                new LogService().Initialize(assembly, assemblyName);
                
                Log.Info(TAG, "NLog initialisiert");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"InitializeNLog Error: {ex.Message}");
            }
        }

        #endregion

        #region Back Button Handling

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
                
                Log.Debug(TAG, $"OnActivityResult: RequestCode={requestCode}, ResultCode={resultCode}");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnActivityResult Error: {ex.Message}");
            }
        }

        #endregion
    }
}