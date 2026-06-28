using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using System;

namespace iPMCloud.Mobile
{
    /*[Application]
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
                
                Log.Info(TAG, "MainApplication OnCreate");
                
                // Notification Channel beim App-Start erstellen
                CreateNotificationChannelEarly();
                
                // App-weite Konfiguration hier
                ConfigureApp();
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

        private void ConfigureApp()
        {
            try
            {
                // App-weite Konfiguration
                Log.Info(TAG, $"App konfiguriert - Version: {ApplicationInfo.LoadLabel(PackageManager)}");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"ConfigureApp Error: {ex.Message}");
            }
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            Log.Warn(TAG, "Low Memory Warning!");
            
            // Speicher freigeben wenn möglich
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            base.OnTrimMemory(level);
            Log.Warn(TAG, $"Trim Memory: {level}");
            
            if (level >= TrimMemory.RunningCritical)
            {
                // Kritischer Speichermangel
                GC.Collect();
            }
        }
    }*/
}