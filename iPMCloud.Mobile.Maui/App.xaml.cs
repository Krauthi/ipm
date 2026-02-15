using iPMCloud.Mobile.vo;
// TODO: Replace with MAUI alternative
// using Matcha.BackgroundService;
// TODO: Replace with MAUI Firebase plugin
// using Plugin.FirebasePushNotification;
// TODO: Replace with MAUI notification plugin
// using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.ApplicationModel;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace iPMCloud.Mobile
{
    public partial class App : Application
    {
        //private AppModel _model;
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                Exception e = (Exception)args.ExceptionObject;
                System.Diagnostics.Debug.WriteLine(e.Message + " - " + (e.StackTrace ?? ""));
                AppModel.Logger.Error("ERROR: Global(APP.cs): " + e.Message + " - " + (e.StackTrace ?? ""));
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: (2)Global(APP.cs): " + ex.Message + " - " + (ex.StackTrace ?? ""));
            }
        }

        public App()
        {

            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {

                var cachePath = System.IO.Path.GetTempPath();
                // If exist, delete the cache directory and everything in it recursivly
                if (System.IO.Directory.Exists(cachePath))
                    System.IO.Directory.Delete(cachePath, true);
                // If not exist, restore just the directory that was deleted
                if (!System.IO.Directory.Exists(cachePath))
                    System.IO.Directory.CreateDirectory(cachePath);
            }
            catch (Exception)
            {
                AppModel.Logger.Warn("WARN: AppCache konnte nicht eglöscht werden!");
            }


            InitApp();
        }


        public void InitApp()
        {

            // TODO: Migrate to MAUI-compatible Firebase
            // Plugin.FirebasePushNotification is not MAUI-compatible
            // Consider: Plugin.Firebase or native Firebase SDK
            // OnStartIntiFirebase();

            AppModel.Instance.InitDeviceInformation();
            AppModel.Instance.App = this;
            if (!AppModel.Instance.HasInitAppmodel) { AppModel.Instance.HasInitAppmodel = AppModel.Instance.InitAppModel(); }
            if (AppModel.Instance.Person != null)
            {
                AppModel.Logger.Info("INFO: App neu gestartet V" + AppModel.Instance.Version + " (" + AppModel.Instance.Person.name + " " + AppModel.Instance.Person.vorname + ")");
            }
            else if (AppModel.Instance.Person == null)
            {
                AppModel.Logger.Warn("WARN: App neu gestartet (Person noch nicht bekannt - Neuinstallation)");
            }

            // Ensure PageNavigator is initialized before navigating
            if (AppModel.Instance.PageNavigator != null)
            {
                AppModel.Instance.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
            }
            else
            {
                AppModel.Logger.Error("ERROR: PageNavigator is null - cannot navigate to start page");
            }
        }


        protected override void OnStart()
        {
            if (AppModel.Instance.DeviceSystem == "ios")
            {
                // TODO: Replace DependencyService with DI
                // DependencyService.Get<IImageResizer>().ClearBadge();
            }

            //OnStartIntiFirebase();

            //if (AppModel.Instance.DeviceSystem == "android")
            //{
            //    DependencyService.Get<IDependentService>().Start();
            //}

            //StartBackgroundService();
            AppModel.Instance.isInBackground = false;
            AppModel.Instance.AppOnStart = DateTime.Now;
            base.OnStart();
        }

        protected async override void OnSleep()
        {
            AppModel.Instance.isInBackground = true;
            //AppModel.Logger.Info("(OnSleep) App in den Hintergrund gelegt");
            AppModel.Instance.AppOnSleep = DateTime.Now;
            AppModel.Instance.AppOnResume = AppModel.Instance.AppOnSleep;
            //if (!AppModel.Instance.UseExternHardware)
            //{
            //    await Task.Delay(10000);
            //}
            //// OnResume ich öffne früher als 10 Sek die App, dann hier nicht ausführen!
            //if (AppModel.Instance.AppOnResume == AppModel.Instance.AppOnSleep || AppModel.Instance.UseExternHardware)
            //{
            //    if (AppModel.Instance.DeviceSystem == "android")
            //    {
            //        //DependencyService.Get<IDependentService>().Stop();
            //    }
            //    AppModel.Instance.isInBackground = !AppModel.Instance.UseExternHardware;
            //    if (!AppModel.Instance.UseExternHardware)
            //    {
            //        ////if (AppModel.Instance.DeviceSystem == "ios")
            //        ////{

            //        //BackgroundAggregatorService.StopBackgroundService();

            //        ////Thread.CurrentThread.Abort();
            //        ////}
            //        ////else if (AppModel.Instance.DeviceSystem == "android")
            //        ////{
            //        ////AppModel.Logger.Info("(OnSleep) App komplett geschlossen");
            //        ////AppModel.Logger.Info("Service abgeschaltet");
            //        ////System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();// Close to Background
            //        ////System.Diagnostics.Process.GetCurrentProcess().Kill();// Complete Close App
            //        ////}
            //    }
            //    else
            //    {
            //        //AppModel.Logger.Info("(UseHardware) App vorübergehend in den Hintergrund gelegt");
            //    }
            //}

            base.OnSleep();
        }

        protected override void OnResume()
        {

            if (AppModel.Instance.DeviceSystem == "ios")
            {
                // TODO: Replace DependencyService with DI
                // DependencyService.Get<IImageResizer>().ClearBadge();
            }

            AppModel.Instance.AppOnResume = DateTime.Now;
            AppModel.Instance.isInBackground = false;

            //if (AppModel.Instance.AppOnResume > AppModel.Instance.AppOnSleep.AddSeconds(10) || AppModel.Instance.UseExternHardware)
            //{

            //    if (AppModel.Instance.DeviceSystem == "android")
            //    {
            //        //DependencyService.Get<IDependentService>().Start();
            //    }
            ////if (AppModel.Instance.DeviceSystem == "ios")
            ////{

            //StartBackgroundService();

            ////}
            ////AppModel.Logger.Info("App aus dem Hintergrund wieder hervorgerufen");
            //AppModel.Instance.isInBackground = false;

            var dt = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? DateTime.Now.AddDays(-2) : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
            if (dt.AddHours(AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours) < DateTime.Now && !AppModel.Instance.UseExternHardware) //(dt.AddHours(4) < DateTime.Now || manuellSync)
            {
                InitApp();
            }
            AppModel.Instance.UseExternHardware = false;
            //}
            base.OnResume();
        }

        public static void StartBackgroundService()
        {
            // TODO: Implement MAUI-compatible background service
            // Matcha.BackgroundService is not MAUI-compatible
            // Consider: Native Android WorkManager / iOS Background Tasks
            /*
            try
            {
                //AppModel.Logger.Info("GPS Job gestartet");
                BackgroundAggregatorService.Add(() => new LocationInfo(5));

                ////if (AppModel.Instance.SettingModel.SettingDTO.RunBackground) {
                ////    //BackgroundAggregatorService.Instance.Clear();
                ////    AppModel.Logger.Info("STARTE Backgroundprozess SYNC");
                ////    BackgroundAggregatorService.Add(() => new SendUpload(10));
                ////}
                BackgroundAggregatorService.StartBackgroundService();
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR StartBackgroundService");
                AppModel.Logger.Error(ex);
            }
            */
        }


        private void OnStartIntiFirebase()
        {
            // TODO: Migrate to MAUI-compatible Firebase
            // Plugin.FirebasePushNotification is not MAUI-compatible
            // Consider: Plugin.Firebase or native Firebase SDK
            /*
            // Handle when your app starts
            CrossFirebasePushNotification.Current.Subscribe("general");

            CrossFirebasePushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                // Device Model (SMG-950U, iPhone10,6)
                // Manufacturer (Samsung)
                // Device Name (Motz's iPhone)
                // Operating System Version Number (7.0)
                // var version = DeviceInfo.VersionString;
                // Platform (Android)

                //Task.Run(async () =>
                //{
                //    await AppModel.Instance.Connections.PNSync(new PNWSO
                //    {
                //        personid = AppModel.Instance.Person.id,
                //        token = p.Token + ";;" + DeviceInfo.Platform + ";;" +
                //        DeviceInfo.Manufacturer + " - " + DeviceInfo.Name + " (" + DeviceInfo.Model + ")",
                //    });
                //});
            };


            PNWSO.ToUploadStack(new PNWSO
            {
                token = CrossFirebasePushNotification.Current.Token + ";;" + DeviceInfo.Platform + ";;" +
                        DeviceInfo.Manufacturer + " - " + DeviceInfo.Name + " (" + DeviceInfo.Model + ")"
            });

            //System.Diagnostics.Debug.WriteLine($"TOKEN: {CrossFirebasePushNotification.Current.Token}");


            // PN Nachricht kommt hier ein 
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                try
                {
                    List<long> hh = new List<long>() { 6000 };
                    string imgS = null;
                    string imgL = null;
                    string text = "";
                    string title = "";
                    string subtitle = "";
                    string dataj = "";
                    //System.Diagnostics.Debug.WriteLine("NOTIFICATION RECEIVED", p.Data);
                    if (p.Data.TryGetValue("imgS", out object v))
                    {
                        imgS = p.Data["imgS"]?.ToString();
                    }
                    if (p.Data.TryGetValue("imgL", out v))
                    {
                        imgL = p.Data["imgL"]?.ToString();
                    }
                    if (p.Data.TryGetValue("body", out v))
                    {
                        text = p.Data["body"]?.ToString();
                    }
                    if (p.Data.TryGetValue("title", out v))
                    {
                        title = p.Data["title"]?.ToString();
                    }
                    if (p.Data.TryGetValue("subtitle", out v))
                    {
                        subtitle = p.Data["subtitle"]?.ToString();
                    }
                    if (p.Data.TryGetValue("dataj", out v))
                    {
                        dataj = p.Data["dataj"]?.ToString();
                    }
                    var pn = new NotificationRequest
                    {
                        BadgeNumber = 1,
                        Description = text,
                        Title = title,
                        Subtitle = subtitle,
                        Sound = "default",
                        ReturningData = "ReturningData_iPM",
                        NotificationId = 100,
                        Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
                        {
                            Priority = Plugin.LocalNotification.AndroidOption.AndroidPriority.Max,
                            VibrationPattern = hh.ToArray(),
                            IconSmallName = new Plugin.LocalNotification.AndroidOption.AndroidIcon
                            {
                                ResourceName = imgS != null ? imgS : "ipmlogo_m",
                            },
                            IconLargeName = new Plugin.LocalNotification.AndroidOption.AndroidIcon
                            {
                                ResourceName = imgL != null ? imgL : "icon",
                            }
                        },
                        iOS = new Plugin.LocalNotification.iOSOption.iOSOptions
                        {
                            Priority = Plugin.LocalNotification.iOSOption.iOSPriority.Critical,
                            PlayForegroundSound = true,
                            ApplyBadgeValue = true,
                            PresentAsBanner = true,
                            ShowInNotificationCenter = true
                        }
                    };
                    var al = PN.LoadAll();
                    al.ForEach(pnl => PN.Delete(pnl.id));
                    //PN.Save(new PN
                    //{
                    //    titel = title,
                    //    beschreibung = text,
                    //    datum = DateTime.Now,
                    //    data = dataj,
                    //    id = DateTime.Now.Ticks.ToString(),
                    //    status = "Neu",
                    //});
                    LocalNotificationCenter.Current.Show(pn);
                }
                catch (Exception e)
                {
                    AppModel.Logger.Error("ERROR showing PushNotification!");
                    AppModel.Logger.Error(e);
                }
            };

            // PN wird geöffnet
            CrossFirebasePushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                //System.Diagnostics.Debug.WriteLine(p.Identifier);

                System.Diagnostics.Debug.WriteLine("Opened");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        //mPage.Message = p.Identifier;
                    });
                }
                else if (p.Data.ContainsKey("color"))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        //mPage.Navigation.PushAsync(new ContentPage()
                        //{
                        //    BackgroundColor = Color.FromArgb($"{p.Data["color"]}")

                        //});
                    });

                }
                else if (p.Data.ContainsKey("aps.alert.title"))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        //mPage.Message = $"{p.Data["aps.alert.title"]}";
                    });

                }
            };

            // PN Action
            CrossFirebasePushNotification.Current.OnNotificationAction += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Action");

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    System.Diagnostics.Debug.WriteLine($"ActionId: {p.Identifier}");
                    foreach (var data in p.Data)
                    {
                        System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                    }

                }

            };

            CrossFirebasePushNotification.Current.OnNotificationDeleted += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Dismissed");
            };
            */
        }

    }
}
