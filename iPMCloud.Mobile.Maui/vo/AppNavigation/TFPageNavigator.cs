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

        public void NavigateToMainPageAfterStartTransition(string subPage = "")
        {
            Interlocked.Exchange(ref _skipStartToMainTransitionOnce, 1);
            NavigateTo(PAGE_MAINPAGE, subPage);
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
#if DEBUG
                            var swStartCtor = Stopwatch.StartNew();
#endif
                            StartPageObj = new StartPage();
#if DEBUG
                            swStartCtor.Stop();
                            AppModel.Logger.Info($"PERF: new StartPage() took {swStartCtor.ElapsedMilliseconds} ms");
#endif
                            AppModel.Instance.StartPage = StartPageObj;
                        }
#if DEBUG
                        var swStartGetPage = Stopwatch.StartNew();
#endif
                        var startPage = StartPageObj.GetPage(subPage);
#if DEBUG
                        swStartGetPage.Stop();
                        AppModel.Logger.Info($"PERF: StartPageObj.GetPage took {swStartGetPage.ElapsedMilliseconds} ms");
                        var swStartSetPage = Stopwatch.StartNew();
#endif
                        await SetPageAsync(startPage);
#if DEBUG
                        swStartSetPage.Stop();
                        AppModel.Logger.Info($"PERF: SetPageAsync(startPage) took {swStartSetPage.ElapsedMilliseconds} ms");
                        var swStartPageAgain = Stopwatch.StartNew();
#endif
                        AppModel.Instance.StartPage.StartPageAgain();
#if DEBUG
                        swStartPageAgain.Stop();
                        AppModel.Logger.Info($"PERF: StartPage.StartPageAgain took {swStartPageAgain.ElapsedMilliseconds} ms");
#endif
                    }
                    //else
                    //{
                    //    StartPageObj.GetPage(subPage);
                    //}
                    break;


                case PAGE_MAINPAGE:
                    if (LastMainPage == PAGE_STARTPAGE &&
                        Interlocked.CompareExchange(ref _skipStartToMainTransitionOnce, 0, 0) == 0)
                    {
                        AppModel.Logger.Info("Showing StartPage -> MainPage transition splash.");
                        await SetPageAsync(new StartToMainTransitionSplashPage(subPage));
                        return;
                    }

                    bool continueFromTransitionSplash = Interlocked.Exchange(ref _skipStartToMainTransitionOnce, 0) == 1;

                    if (continueFromTransitionSplash)
                    {
                        AppModel.Logger.Info("StartPage -> MainPage transition splash finished; continuing to MainPage.");
                    }

                    if (LastMainPage != CurrentMainPage || continueFromTransitionSplash)
                    {
                        if(AppModel.Instance.MainPage != null)
                        {
                            AppModel.Logger.Info("Reusing existing MainPage instance.");
                            MainPageObj = AppModel.Instance.MainPage;
                        }
                        else
                        {
                            AppModel.Logger.Info("Creating new MainPage instance.");
#if DEBUG
                            var swMainCtor = Stopwatch.StartNew();
#endif
                            MainPageObj = new MainPage();
#if DEBUG
                            swMainCtor.Stop();
                            AppModel.Logger.Info($"PERF: new MainPage() took {swMainCtor.ElapsedMilliseconds} ms");
#endif
                            AppModel.Instance.MainPage = MainPageObj;
                        }
#if DEBUG
                        var swMainGetPage = Stopwatch.StartNew();
#endif
                        var mainPageContent = MainPageObj.GetPage(subPage);
#if DEBUG
                        swMainGetPage.Stop();
                        AppModel.Logger.Info($"PERF: MainPageObj.GetPage took {swMainGetPage.ElapsedMilliseconds} ms");
                        var swMainSetPage = Stopwatch.StartNew();
#endif
                        await SetPageAsync(mainPageContent);
#if DEBUG
                        swMainSetPage.Stop();
                        AppModel.Logger.Info($"PERF: SetPageAsync(mainPageContent) took {swMainSetPage.ElapsedMilliseconds} ms");
                        var swMainPageAgain = Stopwatch.StartNew();
#endif
                        AppModel.Instance.MainPage.MainPageAgain();
#if DEBUG
                        swMainPageAgain.Stop();
                        AppModel.Logger.Info($"PERF: MainPage.MainPageAgain took {swMainPageAgain.ElapsedMilliseconds} ms");
#endif
                    }
                    //else
                    //{
                    //    MainPageObj.GetPage(subPage);
                    //}
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
