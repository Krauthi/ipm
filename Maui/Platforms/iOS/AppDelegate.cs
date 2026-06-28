using Firebase.CloudMessaging;
using Firebase.Core;
using Foundation;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.vo;
using UIKit;
using UserNotifications;

namespace iPMCloud.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        try
        {
            Firebase.Core.App.Configure(new Firebase.Core.Options(
                "1:276903433310:ios:4a9e1ebd6fd90e0defd0cf",
                "276903433310")
            {
                ApiKey = "AIzaSyAkGw-be-PCBXuczQvRcXef7mETWMOKF0A",
                ProjectId = "ipm-cloud-firebase"
            });

            UNUserNotificationCenter.Current.Delegate = this;
            Messaging.SharedInstance.Delegate = this;

            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert |
                UNAuthorizationOptions.Badge |
                UNAuthorizationOptions.Sound,
                (granted, error) =>
                {
                    if (error != null)
                    {
                        AppModel.Logger?.Error($"Push authorization error: {error.LocalizedDescription}");
                        return;
                    }

                    if (!granted)
                    {
                        AppModel.Logger?.Warn("Push authorization not granted by user.");
                        return;
                    }

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UIApplication.SharedApplication.RegisterForRemoteNotifications();
                    });
                });

            PushNotificationService.Initialize();
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "ERROR: iOS push initialization failed");
        }

        return base.FinishedLaunching(application, launchOptions);
    }

    public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        try
        {
            if (deviceToken == null || deviceToken.Length == 0)
            {
                AppModel.Logger?.Warn("Push registration succeeded but APNs token is empty.");
                return;
            }

            // APNs Token an Firebase übergeben
            Messaging.SharedInstance.ApnsToken = deviceToken;

            var tokenBytes = deviceToken.ToArray();
            var apnsToken = BitConverter.ToString(tokenBytes).Replace("-", string.Empty).ToLowerInvariant();

            AppModel.Logger?.Info($"APNs Token: {apnsToken}");

            // Optional speichern, aber NICHT an deinen Firebase-Server als FCM Token senden
            PushNotificationService.HandleApnsTokenReceived(apnsToken);
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "ERROR: Failed to process APNs device token");
        }
    }

    public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        AppModel.Logger?.Error($"FailedToRegisterForRemoteNotifications: {error?.LocalizedDescription}");
    }

    [Export("messaging:didReceiveRegistrationToken:")]
    public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
            {
                AppModel.Logger?.Warn("FCM token is empty.");
                return;
            }

            AppModel.Logger?.Info($"FCM Token iOS: {fcmToken}");

            // DAS ist der Token, den dein Server braucht
            PushNotificationService.HandleFcmTokenReceived(fcmToken);
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "ERROR: Failed to process FCM token");
        }
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