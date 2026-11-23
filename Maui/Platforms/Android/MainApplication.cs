using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using iPMCloud.Mobile.vo;
using System;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// MAUI Application Entry Point
    /// </summary>
    [Application]
    public class MainApplication : MauiApplication
    {
        private static readonly string TAG = "iPMCloud-MainApplication";

        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            Log.Debug(TAG, "MainApplication Constructor");
        }

        protected override MauiApp CreateMauiApp()
        {
            try
            {
                Log.Info(TAG, "Creating MauiApp...");
                return MauiProgram.CreateMauiApp();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"CreateMauiApp Error: {ex.Message}");
                throw;
            }
        }



        public override void OnCreate()
        {
            try
            {
                base.OnCreate();

                //var model = AppModel.Instance;
                Log.Info(TAG, "MainApplication OnCreate");

                // Notification Channel beim App-Start erstellen
                //CreateNotificationChannelEarly();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnCreate Error: {ex.Message}");
            }
        }

        private void CreateNotificationChannelEarly()
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    const string channelId = "ipmcloud_message_channel";
                    const string channelName = "iPM Cloud Benachrichtigungen";

                    var notificationManager = GetSystemService(NotificationService) as NotificationManager;

                    if (notificationManager?.GetNotificationChannel(channelId) == null)
                    {
                        var channel = new NotificationChannel(
                            channelId,
                            channelName,
                            NotificationImportance.High)
                        {
                            Description = "Wichtige Benachrichtigungen von iPM Cloud"
                        };

                        channel.EnableLights(true);
                        channel.LightColor = Android.Graphics.Color.ParseColor("#0078D4");
                        channel.EnableVibration(true);
                        channel.SetVibrationPattern(new long[] { 0, 500, 250, 500 });

                        notificationManager.CreateNotificationChannel(channel);

                        Log.Info(TAG, "Notification Channel erstellt");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"CreateNotificationChannel Error: {ex.Message}");
            }
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            Log.Warn(TAG, "Low Memory Warning!");
            AppModel.Logger?.Warn("MainApplication.OnLowMemory: System meldet Speichermangel - Hintergrund-Prozess-Kill ist wahrscheinlich (mogliche Ursache fur naechsten unerwarteten App-Neustart).");

            // Speicher freigeben. GC.WaitForPendingFinalizers() wird bewusst NICHT aufgerufen,
            // da dies den Thread blockiert und in Kombination mit Finalizern zu Deadlocks fuehren kann.
            GC.Collect();
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            base.OnTrimMemory(level);

            // Nur bei ernsthaftem Speicherdruck loggen. UiHidden/Background/RunningModerate/RunningLow
            // treten sehr haeufig und unkritisch auf (z.B. beim simplen In-den-Hintergrund-Wechsel)
            // und wuerden das Log unnoetig fuellen.
            if (level >= TrimMemory.RunningCritical)
            {
                Log.Warn(TAG, $"Trim Memory: {level}");
                AppModel.Logger?.Warn($"MainApplication.OnTrimMemory: level={level}");
            }

            // Hinweis: Die TRIM_MEMORY-Konstanten von Android sind NICHT nach Schweregrad aufsteigend sortiert:
            // RunningModerate=5, RunningLow=10, RunningCritical=15, UiHidden=20, Background=40, Moderate=60, Complete=80.
            // Ein Vergleich mit "level >= TrimMemory.RunningCritical" wuerde daher auch bei den haeufigen,
            // unkritischen Background/UiHidden-Events (die App laeuft nur im Hintergrund) einen vollen GC.Collect()
            // ausloesen. Das ist unnoetig teuer (CPU/Akku) und kann beim Zurueckkehren in den Vordergrund zu
            // Rucklern fuehren. Ein GC soll daher nur bei wirklich ernstem Speicherdruck erfolgen.
            if (level >= TrimMemory.Moderate)
            {
                GC.Collect();
            }
        }
    }
}