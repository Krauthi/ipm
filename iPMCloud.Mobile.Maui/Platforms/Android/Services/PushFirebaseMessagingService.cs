using Android.App;
using Android.Content;
using Firebase.Messaging;
using iPMCloud.Mobile.Services;

namespace iPMCloud.Mobile.Platforms.Android.Services
{
    [Service(Name = "com.ipmcloud.ipm.mobile.PushFirebaseMessagingService", Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            PushNotificationService.HandleTokenRefresh(token);
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            PushNotificationService.ShowForegroundNotification(message);
        }
    }
}
