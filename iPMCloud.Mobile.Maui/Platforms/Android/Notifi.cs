
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using System;
using System.Threading;

namespace iPMCloud.Mobile.Droid
{
    public class Notifi
    {

        public string Titel { get; set; }
        public string Text { get; set; }
        public bool largeIconSw { get; set; } = false;
        public bool soundSw { get; set; } = false;
        public bool vibrateSw { get; set; } = false;
        public string style { get; set; } = "Big Text";
        public string visibility { get; set; } = "Public";
        public string prio { get; set; } = "High";
        public string category { get; set; } = "Message";

        public Notifi()
        {

        }

        public void Create(Notification.Builder builder)
        {

                // Instantiate the notification builder:
                builder.SetContentTitle("Sample Notification")
                    .SetContentText(Text)
                    .SetSmallIcon(Resource.Drawable.icon)
                    .SetAutoCancel(true);

                // Add large icon if selected:
                //if (largeIconSw)
                //    builder.SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon));

                // Extend style based on Style spinner selection.
                switch (style)
                {
                    case "Big Text":

                        // Extend the message with the big text format style. This will
                        // use a larger screen area to display the notification text.

                        // Set the title for demo purposes:
                        builder.SetContentTitle("Big Text Notification Title");

                        // Using the Big Text style:
                        var textStyle = new Notification.BigTextStyle();

                        // Use the text in the edit box at the top of the screen.
                        textStyle.BigText(Text);
                        textStyle.SetSummaryText("The summary text goes here.");

                        // Plug this style into the builder:
                        builder.SetStyle(textStyle);
                        break;

                    case "Inbox":

                        // Present the notification in inbox format instead of normal text style.
                        // Note that this does not display the notification message entered
                        // in the edit text box; instead, it displays the fake email inbox
                        // summary constructed below.

                        // Using the inbox style:
                        var inboxStyle = new Notification.InboxStyle();

                        // Set the title of the notification:
                        builder.SetContentTitle("5 new messages");
                        builder.SetContentText("chimchim@xamarin.com");

                        // Generate inbox notification text:
                        inboxStyle.AddLine("Cheeta: Bananas on sale");
                        inboxStyle.AddLine("George: Curious about your blog post");
                        inboxStyle.AddLine("Nikko: Need a ride to Evolve?");
                        inboxStyle.SetSummaryText("+2 more");

                        // Plug this style into the builder:
                        builder.SetStyle(inboxStyle);
                        break;

                    case "Image":

                        // Extend the message with image (big picture) format style. This displays
                        // the Resources/drawables/x_bldg.jpg image in the notification body.

                        // Set the title for demo purposes:
                        builder.SetContentTitle("Image Notification");

                        // Using the Big Picture style:
                        var picStyle = new Notification.BigPictureStyle();

                        // Convert the image file to a bitmap before passing it into the style
                        // (there is no exception handler since we know the size of the image):
                        //picStyle.BigPicture(BitmapFactory.DecodeResource(Resources, Resource.Drawable.logo));
                        picStyle.SetSummaryText("The summary text goes here.");

                        // Alternately, uncomment this code to use an image from the SD card.
                        // (In production code, wrap DecodeFile in an exception handler in case
                        // the image is too large and throws an out of memory exception.):
                        // BitmapFactory.Options options = new BitmapFactory.Options();
                        // options.InSampleSize = 2;
                        // string imagePath = "/sdcard/Pictures/my-tshirt.jpg";
                        // picStyle.BigPicture(BitmapFactory.DecodeFile(imagePath, options));
                        // picStyle.SetSummaryText("Check out my new T-shirt!");

                        // Plug this style into the builder:
                        builder.SetStyle(picStyle);
                        break;

                    default:
                        // Normal text notification is the default.
                        break;
                }

                // Set visibility based on Visibility spinner selection:
                switch (visibility)
                {
                    case "Public":
                        builder.SetVisibility(NotificationVisibility.Public);
                        break;
                    case "Private":
                        builder.SetVisibility(NotificationVisibility.Private);
                        break;
                    case "Secret":
                        builder.SetVisibility(NotificationVisibility.Secret);
                        break;
                }

                // Set priority based on Priority spinner selection:
                switch (prio)
                {
                    case "High":
                        builder.SetPriority((int)NotificationPriority.High);
                        break;
                    case "Low":
                        builder.SetPriority((int)NotificationPriority.Low);
                        break;
                    case "Maximum":
                        builder.SetPriority((int)NotificationPriority.Max);
                        break;
                    case "Minimum":
                        builder.SetPriority((int)NotificationPriority.Min);
                        break;
                    default:
                        builder.SetPriority((int)NotificationPriority.Default);
                        break;
                }

                // Set category based on Category spinner selection:
                switch (category)
                {
                    case "Call":
                        builder.SetCategory(Notification.CategoryCall);
                        break;
                    case "Message":
                        builder.SetCategory(Notification.CategoryMessage);
                        break;
                    case "Alarm":
                        builder.SetCategory(Notification.CategoryAlarm);
                        break;
                    case "Email":
                        builder.SetCategory(Notification.CategoryEmail);
                        break;
                    case "Event":
                        builder.SetCategory(Notification.CategoryEvent);
                        break;
                    case "Promo":
                        builder.SetCategory(Notification.CategoryPromo);
                        break;
                    case "Progress":
                        builder.SetCategory(Notification.CategoryProgress);
                        break;
                    case "Social":
                        builder.SetCategory(Notification.CategorySocial);
                        break;
                    case "Error":
                        builder.SetCategory(Notification.CategoryError);
                        break;
                    case "Transport":
                        builder.SetCategory(Notification.CategoryTransport);
                        break;
                    case "System":
                        builder.SetCategory(Notification.CategorySystem);
                        break;
                    case "Service":
                        builder.SetCategory(Notification.CategoryService);
                        break;
                    case "Recommendation":
                        builder.SetCategory(Notification.CategoryRecommendation);
                        break;
                    case "Status":
                        builder.SetCategory(Notification.CategoryStatus);
                        break;
                    default:
                        builder.SetCategory(Notification.CategoryStatus);
                        break;
                }

            
        }

    }
}

