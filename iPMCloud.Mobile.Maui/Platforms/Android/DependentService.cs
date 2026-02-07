using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using iPMCloud.Mobile.Droid;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

[assembly: Xamarin.Forms.Dependency(typeof(DependentService))]
namespace iPMCloud.Mobile.Droid
{
    [Service]
    public class DependentService : Service, IDependentService
    {

        System.Timers.Timer Timer1 = new System.Timers.Timer();
        public void Start()
        {
            Timer1.Interval = 10000;
            Timer1.Enabled = true;
            Timer1.Elapsed -= (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                Loop();
            };
            Timer1.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                Loop();
            };

            var intent = new Intent(Android.App.Application.Context, typeof(DependentService));
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                Android.App.Application.Context.StartService(intent);
                // OneTimeWorkRequest request = new OneTimeWorkRequest.Builder(DependentService.class ).addTag( "BACKUP_WORKER_TAG" ).build();
                // WorkManager.getInstance(context ).enqueue(request );
                //Android.App.Application.Context.StartForegroundService(intent);
            }
            else if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                Android.App.Application.Context.StartForegroundService(intent);
            }
            else
            {
                Android.App.Application.Context.StartService(intent);
            }



            //var serviceIntent = new Intent(Android.App.Application.Context, typeof(DependentService));
            //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            //{
            //    Android.App.Application.Context.StartForegroundService(serviceIntent);
            //}
            //else
            //{
            //    Android.App.Application.Context.StartService(serviceIntent);
            //}
        }


        public void Stop()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(DependentService));
            Android.App.Application.Context.StopService(intent);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public bool isRunning = false;
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // From shared code or in your PCL
            CreateNotificationChannel();
            string messageBody = "Dieser Service läuft nur solange die App aktiv ist.";
            var notification = new Notification.Builder(this, "10111")
            .SetContentTitle("iPM-Cloud Service")
            .SetContentText(messageBody)
            .SetSmallIcon(Resource.Drawable.ipmlogo_m)
            .SetOngoing(true)
            .Build();
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
            MessagingCenter.Subscribe<MainPage, string>(this, "MapIntentReceived", (sender, arg) =>
            {
                Toast.MakeText(Android.App.Application.Context, arg, ToastLength.Short).Show();
                // await SearchForRooms(arg);
            });

            Timer1.Start();
            Loop();
            //do you work
            return StartCommandResult.Sticky;
        }
        async void Loop()
        {
            //if (isRunning) { return ""; }
            isRunning = true;
            int KGPSTimeout = 5;
            Location LowLoc = null;
            try
            {
                var GeoRequest = new GeolocationRequest(GeolocationAccuracy.Medium);
                for (int i = 0; i < 5; i++)
                {
                    Location Loc = null;
                    CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(KGPSTimeout));
                    Loc = await Geolocation.GetLocationAsync(GeoRequest, cts.Token);
                    if (Loc == null) { continue; }
                    if (LowLoc == null) { LowLoc = Loc; }

                    else
                    {
                        if (Loc.Accuracy < LowLoc.Accuracy) { LowLoc = Loc; }
                        if (LowLoc.Accuracy < 10) { break; }
                    }
                }
                GeoRequest = null;
                isRunning = false;
            }
            catch
            {
                isRunning = false;
            }
        }
        async Task<String> CreateMomCookies(Location pos)
        {
            string acc, alt;
            var percentage = Battery.ChargeLevel * 100;
            try { acc = pos.Accuracy?.ToString("0"); }
            catch { acc = "1000"; }
            try { alt = (pos.Altitude * 3.2808399)?.ToString("0"); }
            catch { alt = "N/A"; }
            var s = pos.Latitude.ToString("0.000000") + "," +
                pos.Longitude.ToString("0.000000") + "," +
                pos.Timestamp.ToLocalTime().ToString("hh:mm:ss tt") + "," +
                acc + "," + alt + "," + percentage.ToString("0") + ",MomData";
            //await Task.Delay(1);
            ////  Toast.MakeText(Android.App.Application.Context, s, ToastLength.Short).Show();
            //Console.WriteLine("================" + s + "=================");
            return s;
        }
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }
            var channelName = MainActivity.CHANNEL_ID;
            //var channelDescription = MainActivity.NOTIFICATION_ID;
            var channel = new NotificationChannel("10111", channelName, NotificationImportance.Default)
            {
                Description = "iPM-Cloud Service - Dieser Service läuft nur solange wie die App aktive genutzt wird. Dieser Service dient dazu, um im Hintergrund während die App läuft, Daten abzurufen und somit die App selber nicht belastet."
            };
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }

}
