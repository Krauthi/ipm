using Foundation;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.vo;
using UIKit;
using UserNotifications;
using FirebaseApp = Firebase.Core.App;

namespace iPMCloud.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Configure();
        }

        UNUserNotificationCenter.Current.Delegate = this;
        UNUserNotificationCenter.Current.RequestAuthorization(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
            (granted, error) =>
            {
                if (error != null)
                {
                    AppModel.Logger?.Error($"Push authorization error: {error.LocalizedDescription}");
                }
                else if (!granted)
                {
                    AppModel.Logger?.Warn("Push authorization not granted by user.");
                }
            });

        PushNotificationService.Initialize();
        Messaging.SharedInstance.Delegate = this;

        return base.FinishedLaunching(application, launchOptions);
    }

    public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        Messaging.SharedInstance.ApnsToken = deviceToken;
        base.RegisteredForRemoteNotifications(application, deviceToken);
    }

    public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        AppModel.Logger?.Error($"FailedToRegisterForRemoteNotifications: {error?.LocalizedDescription}");
        base.FailedToRegisterForRemoteNotifications(application, error);
    }

    [Export("messaging:didReceiveRegistrationToken:")]
    public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
    {
        PushNotificationService.HandleTokenRefresh(fcmToken);
    }

    [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
    public void WillPresentNotification(
        UNUserNotificationCenter center,
        UNNotification notification,
        Action<UNNotificationPresentationOptions> completionHandler)
    {
        completionHandler(
            UNNotificationPresentationOptions.Banner |
            UNNotificationPresentationOptions.List |
            UNNotificationPresentationOptions.Sound |
            UNNotificationPresentationOptions.Badge);
    }

    [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
    public void DidReceiveNotificationResponse(
        UNUserNotificationCenter center,
        UNNotificationResponse response,
        Action completionHandler)
    {
        completionHandler();
    }
}
