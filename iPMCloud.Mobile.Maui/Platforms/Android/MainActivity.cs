using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.App;
using iPMCloud.Mobile.vo;
// TODO: Replace Matcha.BackgroundService with MAUI alternative
// using Matcha.BackgroundService.Droid;
// TODO: Replace Plugin.FirebasePushNotification with MAUI alternative
// using Plugin.FirebasePushNotification;
using System;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    [Activity(Label = "iPM-Cloud", Icon = "@drawable/icon", Theme = "@style/MainTheme", Exported = true,
        MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        public AppModel model;
        public App app;


        public static MainActivity Instance;


        internal static readonly string CHANNEL_ID = "ipmcloud_message_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        private static readonly string TAG = "IPM-CLOUD-" + typeof(MainActivity).Name;

        private string txt = "";

        [Obsolete]
        private void initFontScale()
        {
            Configuration configuration = Resources.Configuration;
            configuration.FontScale = (float)1.00;
            //0.85 small, 1 standard, 1.15 big，1.3 more bigger ，1.45 supper big 
            DisplayMetrics metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);
            metrics.ScaledDensity = configuration.FontScale * metrics.Density;
            BaseContext.Resources.UpdateConfiguration(configuration, metrics);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            if (NativeMedia.Platform.CheckCanProcessResult(requestCode, resultCode, intent))
                NativeMedia.Platform.OnActivityResult(requestCode, resultCode, intent);

            base.OnActivityResult(requestCode, resultCode, intent);
        }

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;
            initFontScale();
            
            // SwipeView is no longer experimental in MAUI
            // Forms.SetFlags("SwipeView_Experimental");

            // TODO: Replace BackgroundAggregator with MAUI alternative
            // BackgroundAggregator.Init(this);

            base.OnCreate(bundle);

            // TODO: Initialize Maps in MauiProgram.cs instead
            // Microsoft.Maui.Controls.Handlers.MapHandler.Mapper...

            try
            {
                model = AppModel.Instance;// init saved Settings or get default
                
                // TODO: Replace NativeMedia with MAUI MediaPicker
                // NativeMedia.Platform.Init(this, bundle);
                
                // MAUI initialization is now handled in MauiProgram.cs
                // Xamarin.Forms.Forms.Init(this, bundle);
                // Xamarin.Essentials.Platform.Init(this, bundle);
                
                // TODO: Replace ZXing with ZXing.Net.Maui
                // global::ZXing.Net.Mobile.Forms.Android.Platform.Init();

                model.Activity = this;

                // TODO: DependencyService is replaced by DI in MauiProgram.cs
                // DependencyService.Register<ImageResizer>();

                InitializeNLog();

                HideNavAndStatusBar();
                Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;

                ActivityCompat.RequestPermissions(this, new String[] {
                        Manifest.Permission.Internet,
                        Manifest.Permission.Camera,
                        Manifest.Permission.ReadExternalStorage,
                        Manifest.Permission.WriteExternalStorage,
                        Manifest.Permission.ReadMediaImages,
                        Manifest.Permission.ReadMediaVideo,
                        Manifest.Permission.AccessWifiState,
                        Manifest.Permission.AccessNetworkState,
                        Manifest.Permission.ChangeNetworkState,
                        Manifest.Permission.ForegroundService,
                        Manifest.Permission.Flashlight,
                        Manifest.Permission.ChangeWifiState,
                        Manifest.Permission.PostNotifications,
                        Manifest.Permission_group.Storage,
                        Manifest.Permission_group.Camera,
                        Manifest.Permission_group.Location,
                        Manifest.Permission_group.Notifications,
                        Manifest.Permission_group.ReadMediaVisual,
                        Manifest.Permission_group.ReadMediaAural,
                    }, 122); // your request code


                model.Version = GetVersion();
                model.Build = "" + GetBuild();

                model.InitAppModel();

                //model.CheckPermissions();

                // Note: App initialization is now handled by MauiProgram.cs
                // app = new App();
                // LoadApplication(app);
                
                Window.SetStatusBarColor(Android.Graphics.Color.Argb(255, 0, 0, 0));

                // TODO: Replace FirebasePushNotificationManager with MAUI alternative
                // FirebasePushNotificationManager.ProcessIntent(this, Intent);

            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR (4) MainActivity LoadApplication: " + ex.Message);
            }
        }



        private void DecorView_SystemUiVisibilityChange(object sender, Android.Views.View.SystemUiVisibilityChangeEventArgs e)
        {
            HideNavAndStatusBar();
        }

        private void HideNavAndStatusBar()
        {
            int uiOptions = (int)Window.DecorView.SystemUiVisibility;
            uiOptions |= (int)SystemUiFlags.LayoutStable;
            uiOptions |= (int)SystemUiFlags.LayoutHideNavigation;
            uiOptions |= (int)SystemUiFlags.LayoutFullscreen;
            uiOptions |= (int)SystemUiFlags.HideNavigation;

            uiOptions |= (int)SystemUiFlags.Fullscreen;
            uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
            uiOptions |= (int)SystemUiFlags.Immersive;

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }


        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    txt = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                else
                {
                    AppModel.Logger.Error("PushNotification FCM IsPLayServiceAvailabel() - This device is not supported!");
                    Finish();
                }
                return false;
            }
            else
            {
                txt = "Google Play Services is available.";
                return true;
            }
        }
        public void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                AppModel.Logger.Warn("PushNotification FCM CreateNotificationChannel() - Notification channels are new in API 26 (and not a part of the support library). There is no need to create a notification channel on older versions of Android.");
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID, "iPM Notifications", NotificationImportance.High)
            {
                Description = "Dieser Nachrichtenkanal ist wichtig für die  Kominikation mit Ihrer registrierten Zentrale und diese App."
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        private void InitializeNLog()
        {
            Assembly assembly = this.GetType().Assembly;
            string assemblyName = assembly.GetName().Name;
            new LogService().Initialize(assembly, assemblyName);
        }

        public string GetVersion()
        {
            var context = global::Android.App.Application.Context;

            PackageManager manager = context.PackageManager;
            PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);


            return info.VersionName;
        }

        public int GetBuild()
        {
            var context = global::Android.App.Application.Context;
            PackageManager manager = context.PackageManager;
            PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);

            return info.VersionCode;//Versionsnummer
        }

        /// for use the flashlight
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //PermissionsImplementation.Current.OnRequestPermissionsResult(25, permissions, grantResults);
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public override void OnBackPressed()
        {
            base.OnBackPressed();
            //if (model.PopupLayoutSub != null)
            //{
            //    model.PopupLayoutSub.Dismiss();
            //    model.PopupLayoutSub = null;
            //    return;
            //}

            //if (model.PopupLayout != null)
            //{
            //    model.PopupLayout.Dismiss();
            //    model.PopupLayout = null;
            //    return;
            //}
            //return;
            //if (model.PageNavigator.NavigateBackToPreviousPage())
            //{
            //  //  base.OnBackPressed(); // close app (only Android)
            //}
        }

        protected override void OnStart()
        {
            base.OnStart();

        }
        protected override void OnPause()
        {
            base.OnPause();
        }
        protected override void OnResume()
        {
            //IsPlayServicesAvailable();
            base.OnResume();
        }
        //protected override void OnRestart()
        //{
        //    base.OnRestart();
        //}
        protected override void OnStop()
        {
            base.OnStop();
        }

    }
}