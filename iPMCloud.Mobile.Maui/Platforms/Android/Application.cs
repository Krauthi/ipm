using Android.App;
using Android.OS;
using Android.Runtime;
using iPMCloud.Mobile.vo;
// TODO: Replace Plugin.FirebasePushNotification with MAUI alternative
// using Plugin.FirebasePushNotification;
using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    //You can specify additional application information in this attribute
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          : base(handle, transer)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnCreate()
        {
            base.OnCreate();

            // TODO: Replace Firebase initialization with MAUI alternative
            /*
            //Set the default notification channel for your app when running Android Oreo
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                //Change for your default notification channel id here
                FirebasePushNotificationManager.DefaultNotificationChannelId = "iPMCloudPnChanelId";

                //Change for your default notification channel name here
                FirebasePushNotificationManager.DefaultNotificationChannelName = "iPMCloudPnChanelName";
            }

            FirebasePushNotificationManager.Initialize(this, new NotificationUserCategory[]
            {
            new NotificationUserCategory("message",new List<NotificationUserAction> {
                new NotificationUserAction("Reply","Reply",NotificationActionType.Foreground),
                new NotificationUserAction("Forward","Forward",NotificationActionType.Foreground)

            }),
            new NotificationUserCategory("request",new List<NotificationUserAction> {
                new NotificationUserAction("Accept","Accept",NotificationActionType.Default,"check"),
                new NotificationUserAction("Reject","Reject",NotificationActionType.Default,"cancel")
            })

            }, AppModel.Instance.RefreshPNToken);
            */
        }
    }
}