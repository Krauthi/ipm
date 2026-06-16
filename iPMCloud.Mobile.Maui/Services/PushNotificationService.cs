using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Firebase.Messaging;
using AndroidApp = Android.App.Application;
#elif IOS
using Foundation;
using UIKit;
#endif

namespace iPMCloud.Mobile.Services
{
    public static class PushNotificationService
    {
        public const string AndroidChannelId = "ipmcloud_message_channel";
        private static readonly long[] AndroidVibrationPattern = new long[] { 0, 500, 250, 500 };
        private static bool _initialized;
        private static bool _androidNotificationPermissionRequestStarted;
        private static readonly object _initLock = new();
        private static int _nextAndroidNotificationId = 1000;

        public static void Initialize()
        {
            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }

                _initialized = true;
            }

#if ANDROID
            EnsureAndroidNotificationChannel();
            FirebaseMessaging.Instance.AutoInitEnabled = true;
            FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(new FirebaseTokenCompleteListener());
#elif IOS
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
#endif
        }

        public static void HandleTokenRefresh(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            try
            {
                var fullToken = $"{token};;{DeviceInfo.Platform};;{DeviceInfo.Manufacturer} - {DeviceInfo.Name} ({DeviceInfo.Model})";
                PNWSO.ToUploadStack(new PNWSO { token = fullToken });
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: HandleTokenRefresh");
            }
        }

#if ANDROID
        public static void EnsureAndroidNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var notificationManager = AndroidApp.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            if (notificationManager?.GetNotificationChannel(AndroidChannelId) != null)
            {
                return;
            }

            var channel = new NotificationChannel(AndroidChannelId, "iPM Cloud Benachrichtigungen", NotificationImportance.High)
            {
                Description = "Wichtige Benachrichtigungen von iPM Cloud Mobile",
                LockscreenVisibility = NotificationVisibility.Public
            };

            channel.EnableLights(true);
            channel.EnableVibration(true);
            channel.SetVibrationPattern(AndroidVibrationPattern);
            notificationManager?.CreateNotificationChannel(channel);
        }

        public static void ShowForegroundNotification(RemoteMessage message)
        {
            if (message == null)
            {
                return;
            }

            EnsureAndroidNotificationChannel();

            var title = message.GetNotification()?.Title;
            var body = message.GetNotification()?.Body;

            if (string.IsNullOrWhiteSpace(title) && message.Data.TryGetValue("title", out var dataTitle))
            {
                title = dataTitle;
            }

            if (string.IsNullOrWhiteSpace(body) && message.Data.TryGetValue("body", out var dataBody))
            {
                body = dataBody;
            }

            title ??= "iPM Cloud";
            body ??= string.Empty;

            var launchIntent = Platform.CurrentActivity?.PackageManager?.GetLaunchIntentForPackage(AndroidApp.Context.PackageName)
                               ?? new Intent(AndroidApp.Context, typeof(MainActivity));
            launchIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            foreach (var item in message.Data)
            {
                launchIntent.PutExtra(item.Key, item.Value);
            }

            var pendingFlags = PendingIntentFlags.UpdateCurrent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                pendingFlags |= PendingIntentFlags.Immutable;
            }

            var pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, 0, launchIntent, pendingFlags);

            var builder = new NotificationCompat.Builder(AndroidApp.Context, AndroidChannelId)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetCategory(NotificationCompat.CategoryMessage)
                .SetVisibility(NotificationCompat.VisibilityPublic)
                .SetDefaults((int)NotificationDefaults.All)
                .SetContentIntent(pendingIntent);

            NotificationManagerCompat.From(AndroidApp.Context)
                .Notify(Interlocked.Increment(ref _nextAndroidNotificationId), builder.Build());
        }

        public static void EnsureAndroidNotificationPermissionRequest()
        {
            RequestAndroidNotificationPermissionIfRequired();
        }

        private static void RequestAndroidNotificationPermissionIfRequired()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
            {
                return;
            }

            if (_androidNotificationPermissionRequestStarted)
            {
                return;
            }

            var activity = Platform.CurrentActivity ?? MainActivity.Instance;
            if (activity == null || activity.IsFinishing || activity.IsDestroyed)
            {
                return;
            }

            _androidNotificationPermissionRequestStarted = true;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var currentActivity = Platform.CurrentActivity ?? MainActivity.Instance;
                    if (currentActivity == null || currentActivity.IsFinishing || currentActivity.IsDestroyed)
                    {
                        _androidNotificationPermissionRequestStarted = false;
                        return;
                    }

                    var currentStatus = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                    if (currentStatus != PermissionStatus.Granted)
                    {
                        await Permissions.RequestAsync<Permissions.PostNotifications>();
                    }
                }
                catch (Exception ex)
                {
                    _androidNotificationPermissionRequestStarted = false;
                    AppModel.Logger?.Error(ex, "ERROR: RequestAndroidNotificationPermissionIfRequired");
                }
            });
        }

        private sealed class FirebaseTokenCompleteListener : Java.Lang.Object, Android.Gms.Tasks.IOnCompleteListener
        {
            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                if (!task.IsSuccessful)
                {
                    return;
                }

                var token = task.Result?.ToString();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    HandleTokenRefresh(token);
                }
            }
        }
#endif
    }
}
