using iPMCloud.Mobile.vo;
// TODO: Replace with MAUI alternative
// using Matcha.BackgroundService;
// TODO: Replace with MAUI Firebase plugin
// using Plugin.FirebasePushNotification;
// TODO: Replace with MAUI notification plugin
// using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.ApplicationModel;
using iPMCloud.Mobile.Services;

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

                AppModel.Instance.SendLogZipFile(true);
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: (2)Global(APP.cs): " + ex.Message + " - " + (ex.StackTrace ?? ""));
            }
        }

        public App()
        {
            InitializeComponent();

            // Erzwinge Dark Mode
            Application.Current.UserAppTheme = AppTheme.Dark;

            // Register handler for unobserved async Task exceptions (critical for iOS stability)
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            AppStart(); 
        }

        /// <summary>
        /// Handler for unobserved Task exceptions. Critical for iOS where these can cause silent crashes.
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                e.SetObserved(); // Prevent app crash

                var exception = e.Exception?.GetBaseException() ?? e.Exception;
                var message = exception?.Message ?? "Unknown error";
                var stackTrace = exception?.StackTrace ?? "";

                System.Diagnostics.Debug.WriteLine($"UNOBSERVED TASK EXCEPTION: {message} - {stackTrace}");
                AppModel.Logger?.Error($"UNOBSERVED TASK EXCEPTION: {message} | StackTrace: {stackTrace}");

                // Log to file for debugging TestFlight crashes
                try
                {
                    AppModel.Instance?.SendLogZipFile(true);
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UnobservedTaskException handler: {ex.Message}");
            }
        }

        public void AppStart()
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                LocalApplicationDataBackupProtection.EnsureExcludedFromBackup();
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Warn($"LocalApplicationData konnte nicht vom Backup ausgeschlossen werden: {ex.Message}");
            }

            //try
            //{

            //    var cachePath = System.IO.Path.GetTempPath();
            //    // If exist, delete the cache directory and everything in it recursivly
            //    if (System.IO.Directory.Exists(cachePath))
            //        System.IO.Directory.Delete(cachePath, true);
            //    // If not exist, restore just the directory that was deleted
            //    if (!System.IO.Directory.Exists(cachePath))
            //        System.IO.Directory.CreateDirectory(cachePath);
            //}
            //catch (Exception)
            //{
            //    AppModel.Logger.Warn("WARN: AppCache konnte nicht eglöscht werden!");
            //}

            //try
            //{
            //    LocalApplicationDataBackupProtection.EnsureExcludedFromBackup();
            //}
            //catch (Exception ex)
            //{
            //    AppModel.Logger?.Warn($"WARN: LocalApplicationData konnte nicht vom Backup ausgeschlossen werden: {ex.Message}");
            //}

            InitApp();
        }


        public void InitApp()
        {
            // TODO: Migrate to MAUI-compatible Firebase
            // Plugin.FirebasePushNotification is not MAUI-compatible
            // Consider: Plugin.Firebase or native Firebase SDK
            // OnStartIntiFirebase();

            // Reset stale page-navigator state that may survive in the AppModel singleton
            // across an Android ClearTask restart (e.g. when the user taps a push
            // notification and the OS destroys/recreates the MainActivity).  Without this
            // reset, TFPageNavigator's guard "if (LastMainPage != CurrentMainPage)" is
            // never true on the second session and SplashOverlayPage never navigates away
            // — causing the app to hang on the splash screen indefinitely.
            try
            {
                var model = AppModel.Instance;
                if (model != null)
                {
                    if (model.PageNavigator != null)
                    {
                        model.PageNavigator.CurrentMainPage = "";
                        model.PageNavigator.LastMainPage = "";
                    }
                    // Drop stale page references so the navigator always creates fresh pages
                    // bound to the new MAUI window instead of reusing pages from the previous
                    // window that was destroyed by ClearTask.
                    //model.MainPage = null;
                    //model.StartPage = null;
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"App.InitApp: failed to reset navigator state: {ex.Message}");
            }

            AppModel.Instance.InitDeviceInformation();
            PushNotificationService.Initialize();
            AppModel.Instance.App = this;
            if (!AppModel.Instance.HasInitAppmodel) { 
                AppModel.Instance.HasInitAppmodel = AppModel.Instance.InitAppModel(); 
            }
            if (AppModel.Instance.Person != null)
            {
                AppModel.Logger.Info("INFO: App neu gestartet V" + AppModel.Instance.Version + " (" + AppModel.Instance.Person.name + " " + AppModel.Instance.Person.vorname + ")");
            }
            else if (AppModel.Instance.Person == null)
            {
                AppModel.Logger.Warn("WARN: App neu gestartet (Person noch nicht bekannt - Neuinstallation)");
            }
        }


        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new SplashOverlayPage());
        }

        protected override void OnStart()
        {
            AppModel.Instance.AppOnStart = DateTime.Now;
            base.OnStart();
        }

        protected async override void OnSleep()
        {
            AppModel.Instance.isInBackground = true;
            AppModel.Logger.Info("(OnSleep) App in den Hintergrund gelegt");
            AppModel.Instance.AppOnSleep = DateTime.Now;

            base.OnSleep();
        }

        protected override void OnResume()
        {
            try
            {
                AppModel.Instance.AppOnResume = DateTime.Now;

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

                // ⬇️ Schutz: Nicht ausführen wenn App gerade erst gestartet wurde (< 5 Sekunden)
                //var timeSinceStart = (DateTime.Now - AppModel.Instance.AppOnStart).TotalSeconds;
                //if (timeSinceStart < 5)
                //{
                //    base.OnResume();
                //    return;
                //}

                var dt = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? DateTime.Now.AddDays(-2) 
                    : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                if (dt.AddHours(AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours) < DateTime.Now && !AppModel.Instance.UseExternHardware) //(dt.AddHours(4) < DateTime.Now || manuellSync)
                {
                    InitApp();
                }
                AppModel.Instance.UseExternHardware = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR in OnResume (App.xaml.cs)");
                AppModel.Logger.Error(ex);
            }
            //}
            base.OnResume();
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
