using Foundation;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.vo;
using System;
using UIKit;
using UserNotifications;

namespace iPMCloud.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        try
        {
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
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "ERROR: iOS push initialization failed");
        }

        return base.FinishedLaunching(application, launchOptions);
    }

    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        try
        {
            if (deviceToken == null || deviceToken.Length == 0)
            {
                AppModel.Logger?.Warn("Push registration succeeded but APNs token is empty.");
                return;
            }

            var tokenBytes = deviceToken.ToArray();
            var apnsToken = BitConverter.ToString(tokenBytes).Replace("-", string.Empty).ToLowerInvariant();
            PushNotificationService.HandleTokenRefresh(apnsToken);
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "ERROR: Failed to process APNs device token");
        }
    }

    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        AppModel.Logger?.Error($"FailedToRegisterForRemoteNotifications: {error?.LocalizedDescription}");
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
