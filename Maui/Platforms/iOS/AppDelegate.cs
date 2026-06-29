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
            Firebase.Core.App.Configure();

            UNUserNotificationCenter.Current.Delegate = this;

            // Messaging.SharedInstance kann zu diesem Zeitpunkt null sein
            // Der Delegate wird stattdessen in RegisteredForRemoteNotifications gesetzt
            if (Messaging.SharedInstance != null)
            {
                Messaging.SharedInstance.Delegate = this;
                AppModel.Logger?.Info("Firebase Messaging Delegate erfolgreich gesetzt in FinishedLaunching");
            }
            else
            {
                AppModel.Logger?.Warn("Firebase Messaging.SharedInstance ist null in FinishedLaunching - wird später initialisiert");
            }

            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert |
                UNAuthorizationOptions.Badge |
                UNAuthorizationOptions.Sound,
                (granted, error) =>
                {
                    if (granted)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UIApplication.SharedApplication.RegisterForRemoteNotifications();
                        });
                    }
                    else
                    {
                        AppModel.Logger?.Warn($"Push-Berechtigung verweigert: {error?.LocalizedDescription}");
                    }
                });
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "ERROR: iOS push initialization failed");
            AppModel.Instance.SendLogZipFile(true);
        }

        return base.FinishedLaunching(application, launchOptions);
    }

    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        try
        {
            if (Messaging.SharedInstance == null)
            {
                AppModel.Logger?.Error("Firebase Messaging ist nicht initialisiert. APNS-Token kann nicht gesetzt werden.");
                
                return;
            }

            // Stelle sicher, dass der Delegate gesetzt ist
            if (Messaging.SharedInstance.Delegate == null)
            {
                Messaging.SharedInstance.Delegate = this;
            }

            Messaging.SharedInstance.ApnsToken = deviceToken;
            AppModel.Logger?.Info("APNS Token erfolgreich an Firebase Messaging übergeben.");
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "Fehler beim Setzen des APNS-Tokens");
            AppModel.Instance.SendLogZipFile(true);
        }
    }

    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        AppModel.Logger?.Error($"FailedToRegisterForRemoteNotifications: {error?.LocalizedDescription}");
        AppModel.Instance.SendLogZipFile(true);
    }

    [Export("messaging:didReceiveRegistrationToken:")]
    public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
    {
        AppModel.Logger?.Info($"FCM Token iOS: {fcmToken}");

        // Token an Server senden
        try
        {
            PushNotificationService.HandleTokenRefresh(fcmToken);
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "Fehler beim Senden des FCM-Tokens an den Server");
            AppModel.Instance.SendLogZipFile(true);
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
}