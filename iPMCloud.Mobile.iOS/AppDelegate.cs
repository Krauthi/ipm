using Foundation;
using iPMCloud.Mobile.vo;
using Matcha.BackgroundService.iOS;
using Plugin.FirebasePushNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace iPMCloud.Mobile.iOS
{


    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate
    {

        public UIWindow window = new UIWindow();


        static readonly string TAG = "iPM-Cloud Mobile V2.0.0: " + typeof(AppDelegate).Name;

        public AppModel model;
        public App app;



        //List<KeyValuePair<string, string>> kvList = new List<KeyValuePair<string, string>>();




        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {


            Forms.SetFlags(new string[] { "SwipeView_Experimental", "RadioButton_Experimental" });

            Xamarin.FormsMaps.Init();

            global::Xamarin.Forms.Forms.Init();

            Xamarin.Forms.DependencyService.Register<IImageResizer, ImageResizer>();


            var settings = UIUserNotificationSettings.GetSettingsForTypes(
                 UIUserNotificationType.Alert
                 | UIUserNotificationType.Badge
                 | UIUserNotificationType.Sound,
                 new NSSet());
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            UIApplication.SharedApplication.RegisterForRemoteNotifications();


            //FirebasePushNotificationManager.Initialize(launchOptions, true);
            FirebasePushNotificationManager.Initialize(launchOptions, new NotificationUserCategory[]
            {
                new NotificationUserCategory("message",new List<NotificationUserAction> {
                    new NotificationUserAction("Reply","Reply",NotificationActionType.Foreground)
                }),
                new NotificationUserCategory("request",new List<NotificationUserAction> {
                    new NotificationUserAction("Accept","Accept"),
                    new NotificationUserAction("Reject","Reject",NotificationActionType.Destructive)
                })

            }, AppModel.Instance.RefreshPNToken);


            NativeMedia.Platform.Init(GetTopViewController);

            UIApplication.SharedApplication.StatusBarHidden = true;

            model = AppModel.Instance;
            //model.pnDatas = kvList;
            model.Version = GetVersion();
            model.Build = "" + GetBuild();

            BackgroundAggregator.Init(this);

            InitializeNLog();

            uiApplication.ApplicationIconBadgeNumber = -1;

            model.InitAppModel();

            app = new App();
            LoadApplication(app);
            UINavigationBar.Appearance.BarTintColor = UIColor.Black;
            // Use for Barcodescan
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public UIViewController GetTopViewController()
        {
            var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (vc is UINavigationController navController)
                vc = navController.ViewControllers.Last();

            return vc;
        }
        private void InitializeNLog()
        {
            Assembly assembly = this.GetType().Assembly;
            string assemblyName = assembly.GetName().Name;
            new LogService().Initialize(assembly, assemblyName);
        }



        // FOR FIREBASE
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            FirebasePushNotificationManager.DidRegisterRemoteNotifications(deviceToken);

        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            FirebasePushNotificationManager.RemoteNotificationRegistrationFailed(error);
        }
        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired 'till the user taps on the notification launching the application.

            // If you disable method swizzling, you'll need to call this method. 
            // This lets FCM track message delivery and analytics, which is performed
            // automatically with method swizzling enabled.
            FirebasePushNotificationManager.DidReceiveMessage(userInfo);
            // Do your magic to handle the notification data
            System.Console.WriteLine(userInfo);

            completionHandler(UIBackgroundFetchResult.NewData);
        }

        public string GetVersion()
        {
            return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
        }
        public int GetBuild()
        {
            return int.Parse(NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString());
        }


    }
}
