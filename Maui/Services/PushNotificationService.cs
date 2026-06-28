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
            try
            {
                lock (_initLock)
                {
                    if (_initialized)
                    {
                        AppModel.Logger?.Info("INFO: PushNotificationService bereits initialisiert.");
                        return;
                    }

                    _initialized = true;
                    AppModel.Logger?.Info("INFO: PushNotificationService wird initialisiert...");
                }

#if ANDROID
                EnsureAndroidNotificationChannel();
                FirebaseMessaging.Instance.AutoInitEnabled = true;
                FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(new FirebaseTokenCompleteListener());
                AppModel.Logger?.Info("INFO: Firebase Messaging erfolgreich initialisiert.");
#elif IOS
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
                AppModel.Logger?.Info("INFO: iOS APNs-Registrierung angestoßen. " +
                    "Hinweis: Firebase iOS Messaging ist noch nicht implementiert – " +
                    "es wird ausschließlich der APNs-Kanal genutzt.");
#endif
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Fehler bei Initialize in PushNotificationService");
                _initialized = false;
            }
        }

        public static void HandleTokenRefresh(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                AppModel.Logger?.Warn("WARN: HandleTokenRefresh mit leerem Token aufgerufen.");
                return;
            }

            try
            {
                AppModel.Logger?.Info($"INFO: HandleTokenRefresh - Token empfangen (Länge: {token.Length})");
                var fullToken = $"{token};;{DeviceInfo.Platform};;{DeviceInfo.Manufacturer} - {DeviceInfo.Name} ({DeviceInfo.Model})";
                PNWSO.ToUploadStack(new PNWSO { token = fullToken });
                AppModel.Logger?.Info("INFO: Token erfolgreich zum Upload-Stack hinzugefügt.");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: HandleTokenRefresh - Token: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
            }
        }

#if ANDROID
        public static void EnsureAndroidNotificationChannel()
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    AppModel.Logger?.Info("INFO: Android SDK < Oreo - Notification Channel nicht erforderlich.");
                    return;
                }

                var notificationManager = AndroidApp.Context.GetSystemService(Context.NotificationService) as NotificationManager;
                if (notificationManager?.GetNotificationChannel(AndroidChannelId) != null)
                {
                    AppModel.Logger?.Info($"INFO: Notification Channel '{AndroidChannelId}' existiert bereits.");
                    return;
                }

                AppModel.Logger?.Info($"INFO: Erstelle Notification Channel '{AndroidChannelId}'...");
                var channel = new NotificationChannel(AndroidChannelId, "iPM Cloud Benachrichtigungen", NotificationImportance.High)
                {
                    Description = "Wichtige Benachrichtigungen von iPM Cloud Mobile",
                    LockscreenVisibility = NotificationVisibility.Public
                };

                channel.EnableLights(true);
                channel.EnableVibration(true);
                channel.SetVibrationPattern(AndroidVibrationPattern);
                notificationManager?.CreateNotificationChannel(channel);
                AppModel.Logger?.Info($"INFO: Notification Channel '{AndroidChannelId}' erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Fehler beim Erstellen des Notification Channels");
            }
        }

        public static void ShowForegroundNotification(RemoteMessage message)
        {
            try
            {
                if (message == null)
                {
                    AppModel.Logger?.Warn("WARN: ShowForegroundNotification - message ist null.");
                    return;
                }

                AppModel.Logger?.Info("INFO: ShowForegroundNotification - Verarbeite eingehende Notification...");
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

                AppModel.Logger?.Info($"INFO: Notification - Titel: '{title}', Body-Länge: {body.Length}");

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

                var notificationId = Interlocked.Increment(ref _nextAndroidNotificationId);
                NotificationManagerCompat.From(AndroidApp.Context)
                    .Notify(notificationId, builder.Build());

                AppModel.Logger?.Info($"INFO: Notification mit ID {notificationId} erfolgreich angezeigt.");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Fehler beim Anzeigen der Foreground Notification");
            }
        }

        public static void EnsureAndroidNotificationPermissionRequest()
        {
            try
            {
                AppModel.Logger?.Info("INFO: EnsureAndroidNotificationPermissionRequest aufgerufen.");
                RequestAndroidNotificationPermissionIfRequired();
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Fehler bei EnsureAndroidNotificationPermissionRequest");
            }
        }

        private static void RequestAndroidNotificationPermissionIfRequired()
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                {
                    AppModel.Logger?.Info("INFO: Android SDK < Tiramisu - Notification Permission nicht erforderlich.");
                    return;
                }

                if (_androidNotificationPermissionRequestStarted)
                {
                    AppModel.Logger?.Info("INFO: Notification Permission Request bereits gestartet.");
                    return;
                }

                var activity = Platform.CurrentActivity ?? MainActivity.Instance;
                if (activity == null || activity.IsFinishing || activity.IsDestroyed)
                {
                    AppModel.Logger?.Warn("WARN: Activity nicht verfügbar oder beendet - Permission Request abgebrochen.");
                    return;
                }

                _androidNotificationPermissionRequestStarted = true;
                AppModel.Logger?.Info("INFO: Starte Notification Permission Request...");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var currentActivity = Platform.CurrentActivity ?? MainActivity.Instance;
                        if (currentActivity == null || currentActivity.IsFinishing || currentActivity.IsDestroyed)
                        {
                            _androidNotificationPermissionRequestStarted = false;
                            AppModel.Logger?.Warn("WARN: Activity während Permission Request nicht mehr verfügbar.");
                            return;
                        }

                        var currentStatus = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                        AppModel.Logger?.Info($"INFO: Aktueller Notification Permission Status: {currentStatus}");

                        if (currentStatus != PermissionStatus.Granted)
                        {
                            AppModel.Logger?.Info("INFO: Fordere Notification Permission an...");
                            var result = await Permissions.RequestAsync<Permissions.PostNotifications>();
                            AppModel.Logger?.Info($"INFO: Notification Permission Ergebnis: {result}");
                        }
                        else
                        {
                            AppModel.Logger?.Info("INFO: Notification Permission bereits erteilt.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _androidNotificationPermissionRequestStarted = false;
                        AppModel.Logger?.Error(ex, "ERROR: Fehler beim Permission Request (inner)");
                    }
                });
            }
            catch (Exception ex)
            {
                _androidNotificationPermissionRequestStarted = false;
                AppModel.Logger?.Error(ex, "ERROR: Fehler bei RequestAndroidNotificationPermissionIfRequired (outer)");
            }
        }

        private sealed class FirebaseTokenCompleteListener : Java.Lang.Object, Android.Gms.Tasks.IOnCompleteListener
        {
            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                try
                {
                    if (!task.IsSuccessful)
                    {
                        AppModel.Logger?.Warn($"WARN: Firebase Token Task nicht erfolgreich. Exception: {task.Exception?.Message}");
                        return;
                    }

                    var token = task.Result?.ToString();
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        AppModel.Logger?.Info("INFO: Firebase/FCM Token (Android) erfolgreich empfangen.");
                        HandleTokenRefresh(token);
                    }
                    else
                    {
                        AppModel.Logger?.Warn("WARN: Firebase Token ist leer oder null.");
                    }
                }
                catch (Exception ex)
                {
                    AppModel.Logger?.Error(ex, "ERROR: Fehler in FirebaseTokenCompleteListener.OnComplete");
                }
            }
        }
#endif

#if IOS
        /// <summary>
        /// Wird aufgerufen, wenn iOS einen APNs Device Token bereitstellt.
        /// Dieser Token ist KEIN Firebase/FCM-Registrierungstoken und darf nicht
        /// in denselben Upload-Pfad wie Android-Firebase-Tokens eingespeist werden.
        /// iOS Firebase Messaging ist noch nicht vollständig implementiert;
        /// der APNs-Token wird geloggt, aber bewusst NICHT in den FCM-Upload-Stack
        /// eingestellt, bis eine korrekte Firebase-iOS-Anbindung vorliegt.
        /// </summary>
        public static void HandleApnsTokenReceived(string apnsToken)
        {
            if (string.IsNullOrWhiteSpace(apnsToken))
            {
                AppModel.Logger?.Warn("WARN: HandleApnsTokenReceived - APNs-Token ist leer.");
                return;
            }

            AppModel.Logger?.Info($"INFO: APNs Device Token empfangen (iOS, Länge: {apnsToken.Length}). " +
                "Dieser Token ist kein Firebase/FCM-Token. " +
                "iOS Firebase Messaging ist noch nicht implementiert – " +
                "Token wird nicht in den FCM-Upload-Stack gelegt.");
        }
#endif
    }
}
