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
using iPMCloud.Mobile;

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
        private int _skipStartToMainTransitionOnce;

        public string CurrentMainPage { get; set; } = "";
        public string LastMainPage { get; set; } = "";


        public TFPageNavigator()
        {
        }

        public void NavigateTo(string mainPage)
        {
            // Ensure navigation happens on the main thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigateToAsync(mainPage);
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Error($"ERROR: NavigateTo failed for page '{mainPage}': {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"NavigateTo Error: {ex.Message}");
                }
            });
        }

        public void NavigateToMainPageAfterStartTransition()
        {
            Interlocked.Exchange(ref _skipStartToMainTransitionOnce, 1);
            NavigateTo(PAGE_MAINPAGE);
        }

        private async Task NavigateToAsync(string mainPage)
        {
            LastMainPage = ""+CurrentMainPage;
            CurrentMainPage = mainPage;
            
            AppModel.Logger.Info($"INFO: Navigating to page '{mainPage}'");
            
            switch (mainPage)
            {
                case PAGE_STARTPAGE:
                    //if (LastMainPage != CurrentMainPage)
                    //{
                        if(AppModel.Instance.StartPage != null)
                        {
                            StartPageObj = AppModel.Instance.StartPage;
                        }
                        else
                        {
                            AppModel.Logger.Info("Creating new StartPage instance.");

                            StartPageObj = new StartPage();

                            AppModel.Instance.StartPage = StartPageObj;
                        }

                        var startPage = StartPageObj.GetPage();
                        await SetPageAsync(startPage);

                        AppModel.Instance.StartPage.StartPageAgain();

                    //}
                    break;


                case PAGE_MAINPAGE:
                    //if (LastMainPage != CurrentMainPage)
                    //{
                        if(AppModel.Instance.MainPage != null)
                        {
                            MainPageObj = AppModel.Instance.MainPage;
                        }
                        else
                        {
                            AppModel.Logger.Info("Creating new MainPage instance.");

                            MainPageObj = new MainPage();

                            AppModel.Instance.MainPage = MainPageObj;
                        }

                        var mainPageContent = MainPageObj.GetPage();

                        await SetPageAsync(mainPageContent);

                        AppModel.Instance.MainPage.MainPageAgain();

                    //}
                    break;


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
                    appl.Windows[0].Page = targetPage;
                }
                else
                {
                    int retries = 0;
                    while ((appl.Windows == null || appl.Windows.Count == 0) && retries < 20)
                    {
                        await Task.Delay(25);
                        retries++;
                    }

                    if (appl.Windows != null && appl.Windows.Count > 0)
                    {
                        appl.Windows[0].Page = targetPage;
                    }
                    else
                    {
                        AppModel.Logger.Error("SetPage: Windows auch nach Retry leer – Fallback MainPage");
                        appl.MainPage = targetPage;
                    }
                }
            });
        }


    }
}
