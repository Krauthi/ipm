using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
                        StartPageObj = new StartPage();
                        AppModel.Instance.StartPage = StartPageObj;
                        var startPage = StartPageObj.GetPage(subPage);
                        SetPage(startPage);
                    }
                    else
                    {
                        StartPageObj.GetPage(subPage);
                    }
                    break;


                case PAGE_MAINPAGE:
                    if (LastMainPage != CurrentMainPage)
                    {
                        MainPageObj = new MainPage();
                        AppModel.Instance.MainPage = MainPageObj;
                        var mainPageContent = MainPageObj.GetPage(subPage); // Fixed: was using StartPageObj
                        SetPage(mainPageContent);
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

        private static void SetPage(Page targetPage)
        {
            var app = Application.Current ?? AppModel.Instance?.App;
            if (app == null)
                return;

            if (app.Windows != null && app.Windows.Count > 0)
            {
                app.Windows[0].Page = targetPage;
            }
            else
            {
                AppModel.Logger.Info("No window available yet – setting MainPage as fallback.");
                app.MainPage = targetPage;
            }
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
