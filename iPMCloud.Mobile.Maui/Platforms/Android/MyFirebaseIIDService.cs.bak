using Android.App;
using Android.Content;
using Android.Graphics;
using AndroidX.Core.App;
using Firebase.Messaging;
using System.Collections.Generic;

namespace iPMCloud.Mobile.Droid
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFireBaseMessagingService";

        public override void OnNewToken(string token)
        {
            // SendRegistrationToServer(token);
        }

        public override void OnMessageReceived(RemoteMessage rm)
        {
            // SendNotification(rm);
        }

        public void SendNotification(RemoteMessage rm)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            IDictionary<string, string> data = rm.Data;

            foreach (var key in data.Keys)
            {
                intent.PutExtra(key, data[key]);
            }

            var prio = rm.Priority;
            var oprio = rm.OriginalPriority;
            var to = rm.To;
            var sent = rm.SentTime;
            var ckey = rm.CollapseKey;
            var from = rm.From;
            var type = rm.MessageType;
            var mID = rm.MessageId;

            var n = rm.GetNotification();
            var title = n.Title;
            var text = n.Body;
            var color = n.Color;
            var nPrio = n.NotificationPriority;
            var imageUrl = n.ImageUrl;
            var icon = n.Icon;


            var pendingIntent = PendingIntent.GetActivity(this, MainActivity.NOTIFICATION_ID, intent, PendingIntentFlags.OneShot);

            var builder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                                      .SetSmallIcon(Resource.Drawable.ipmlogo_m)
                                      .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.ipmlogo_m))
                                      .SetContentTitle(title)
                                      .SetContentText(text)
                                      .SetAutoCancel(true)
                                      .SetOngoing(true)
                                        .SetDefaults(NotificationCompat.DefaultAll)
                                        .SetCategory(Notification.CategoryMessage)
                                      .SetContentIntent(pendingIntent);

            //notificationBuilder.SetContentTitle("Big Text Notification Title");
            //var textStyle = new NotificationCompat.BigTextStyle();
            //textStyle.BigText(text);
            //textStyle.SetSummaryText("The summary text goes here.");
            //notificationBuilder.SetStyle(textStyle);

            //builder.SetContentTitle("Image Notification");
            //var picStyle = new NotificationCompat.BigPictureStyle();
            //picStyle.BigPicture(BitmapFactory.DecodeResource(Resources, Resource.Drawable.logo));
            //picStyle.SetSummaryText("The summary text goes here.");
            ///// Alternately, uncomment this code to use an image from the SD card.
            ///// (In production code, wrap DecodeFile in an exception handler in case
            ///// the image is too large and throws an out of memory exception.):
            ///// BitmapFactory.Options options = new BitmapFactory.Options();
            ///// options.InSampleSize = 2;
            ///// string imagePath = "/sdcard/Pictures/my-tshirt.jpg";
            ///// picStyle.BigPicture(BitmapFactory.DecodeFile(imagePath, options));
            ///// picStyle.SetSummaryText("Check out my new T-shirt!");
            //builder.SetStyle(picStyle);

            //var inboxStyle = new NotificationCompat.InboxStyle();
            //builder.SetContentTitle("5 new messages");
            //builder.SetContentText("chimchim@xamarin.com");
            //inboxStyle.AddLine("Cheeta: Bananas on sale");
            //inboxStyle.AddLine("George: Curious about your blog post");
            //inboxStyle.AddLine("Nikko: Need a ride to Evolve?");
            //inboxStyle.SetSummaryText("+2 more");
            //builder.SetStyle(inboxStyle);

            var rmv = builder.HeadsUpContentView;
            builder.SetCustomHeadsUpContentView(rmv);



            Notification notification = builder.Build();
            // Turn on sound if the sound switch is on:
            if (true)
                notification.Defaults |= NotificationDefaults.Sound;
            // Turn on vibrate if the sound switch is on:
            if (true)
                notification.Defaults |= NotificationDefaults.Vibrate;

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, notification);
        }

        void SendRegistrationToServer(string token)
        {
            PNWSO.ToUploadStack(new PNWSO { token = token });
        }


    }


    //[Service(Exported = false)]
    //[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    //public class MyFirebaseIIDService : FirebaseInstanceIdService
    //{
    //    const string TAG = "MyFirebaseIIDService";
    //    public override void OnTokenRefresh()
    //    {
    //        var refreshedToken = FirebaseInstanceId.Instance.Token;
    //        //Log.Debug(TAG, "Refreshed token: " + refreshedToken);
    //        SendRegistrationToServer(refreshedToken);
    //    }
    //    void SendRegistrationToServer(string token)
    //    {
    //        PNWSO.ToUploadStack(new PNWSO { token = token });
    //    }
    //}
}