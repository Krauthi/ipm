using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.vo
{
    public class TFPageNavigator
    {
        //public const string PAGE_CLOSEAPP = "closeapp";

        public const string PAGE_STARTPAGE = "startpage";
        //public const string SUBPAGE_STARTPAGE_MENU = "startpage_menu";
        //public const string SUBPAGE_STARTPAGE_SETTINGS = "startpage_settings";

        public const string PAGE_MAINPAGE = "mainpage";

                              

        public StartPage StartPageObj { get; set; }
        public MainPage MainPageObj { get; set; }

        public string CurrentMainPage { get; set; } = "";
        public string CurrentSubPage { get; set; } = "";
        public string LastMainPage { get; set; } = "";
        public string LastSubPage { get; set; } = "";


        public TFPageNavigator()
        {
        }

        public void NavigateTo(string mainPage, string subPage = "")
        {
            // Ensure navigation happens on the main thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigateToAsync(mainPage, subPage);
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Error($"ERROR: NavigateTo failed for page '{mainPage}': {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"NavigateTo Error: {ex.Message}");
                }
            });
        }

        private async Task NavigateToAsync(string mainPage, string subPage = "")
        {
            LastMainPage = ""+CurrentMainPage;
            LastSubPage = ""+CurrentSubPage; 
            CurrentMainPage = mainPage;
            CurrentSubPage = subPage;
            
            AppModel.Logger.Info($"INFO: Navigating to page '{mainPage}' with subPage '{subPage}'");
            
            switch (mainPage)
            {
                case PAGE_STARTPAGE:
                    if (LastMainPage != CurrentMainPage)
                    {
                        if(AppModel.Instance.StartPage != null)
                        {
                            AppModel.Logger.Info("Reusing existing StartPage instance.");
                            StartPageObj = AppModel.Instance.StartPage;
                        }
                        else
                        {
                            AppModel.Logger.Info("Creating new StartPage instance.");
                            StartPageObj = new StartPage();
                            AppModel.Instance.StartPage = StartPageObj;
                        }
                        var startPage = StartPageObj.GetPage(subPage);
                        await SetPageAsync(startPage);
                        AppModel.Instance.StartPage.StartPageAgain();
                    }
                    else
                    {
                        StartPageObj.GetPage(subPage);
                    }
                    break;


                case PAGE_MAINPAGE:
                    if (LastMainPage != CurrentMainPage)
                    {
                        if(AppModel.Instance.MainPage != null)
                        {
                            AppModel.Logger.Info("Reusing existing MainPage instance.");
                            MainPageObj = AppModel.Instance.MainPage;
                        }
                        else
                        {
                            AppModel.Logger.Info("Creating new MainPage instance.");
                            MainPageObj = new MainPage();
                            AppModel.Instance.MainPage = MainPageObj;
                        }
#if DEBUG
                        var sw = Stopwatch.StartNew();
#endif
                        var mainPageContent = MainPageObj.GetPage(subPage);
#if DEBUG
                        AppModel.Logger.Debug($"[Timing] GetPage('{subPage}') done in {sw.ElapsedMilliseconds}ms");
                        sw.Restart();
#endif
                        await SetPageAsync(mainPageContent);
#if DEBUG
                        AppModel.Logger.Debug($"[Timing] SetPageAsync done in {sw.ElapsedMilliseconds}ms");
                        sw.Restart();
#endif
                        AppModel.Instance.MainPage.MainPageAgain();
#if DEBUG
                        AppModel.Logger.Debug($"[Timing] MainPageAgain done in {sw.ElapsedMilliseconds}ms");
#endif
                    }
                    else
                    {
                        MainPageObj.GetPage(subPage);
                    }
                    break;


                //case PAGE_CLOSEAPP:

                //    if (LastMainPage != CurrentMainPage)
                //    {
                //        StartPageObj = new StartPage(model);
                //        model.App.MainPage = StartPageObj.GetPage(subPage);
                //    }
                //    else
                //    {
                //        StartPageObj.GetPage(subPage);
                //    }
                //    //App.StartBackgroundService();
                //    // DisplayAlertSheet ... Yes/No
                //    if ( model.DeviceSystem == "ios")
                //    {
                //        Thread.CurrentThread.Abort();
                //    }
                //    else if ( model.DeviceSystem == "android")
                //    {
                //        System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();// Close to Background
                //        System.Diagnostics.Process.GetCurrentProcess().Kill();// Complete Close App
                //    }
                //    break;
            }
        }

        private static async Task SetPageAsync(Page targetPage)
        {
            var appl = Application.Current ?? AppModel.Instance?.App;
            if (appl == null)
            {
                AppModel.Logger.Error("ERROR: Unable to set page - Application instance is null.");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (appl.Windows != null && appl.Windows.Count > 0)
                {
                    // ✅ Normalfall: Window bereits vorhanden
                    appl.Windows[0].Page = targetPage;
                    AppModel.Logger.Info("SetPage: Page gesetzt via Windows[0]");
                }
                else
                {
                    // ⚠️ Window noch nicht bereit → kurz warten und nochmal versuchen
                    AppModel.Logger.Warn("SetPage: Windows noch leer – starte Retry...");
                    int retries = 0;
                    while ((appl.Windows == null || appl.Windows.Count == 0) && retries < 20)
                    {
                        await Task.Delay(50);
                        retries++;
                    }

                    if (appl.Windows != null && appl.Windows.Count > 0)
                    {
                        appl.Windows[0].Page = targetPage;
                        AppModel.Logger.Info($"SetPage: Page gesetzt nach {retries} Retries");
                    }
                    else
                    {
                        AppModel.Logger.Error("SetPage: Windows auch nach Retry leer – Fallback MainPage");
                        appl.MainPage = targetPage;
                    }
                }
            });
        }

        public bool NavigateBackToPreviousPage()
        {
            switch (CurrentMainPage)
            {
                case PAGE_STARTPAGE:
                    //switch (CurrentSubPage)
                    //{
                    //    case SUBPAGE_STARTPAGE_MENU:
                    //        // APP ENDE
                    //        return true;
                    //        //case SUBPAGE_STARTPAGE_SETTINGS:
                    //        //    NavigateTo(PAGE_STARTPAGE, SUBPAGE_STARTPAGE_MENU);
                    //        //    break;
                    //}
                    return true;
                    //break;


                case PAGE_MAINPAGE:
                    NavigateTo(PAGE_STARTPAGE);
                    break;

                default:
                    return true;// close app (only Android)
            }
            return false;
        }

    }
}
