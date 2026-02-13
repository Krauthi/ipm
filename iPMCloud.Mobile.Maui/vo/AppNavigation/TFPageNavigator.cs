using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Threading;

namespace iPMCloud.Mobile.vo
{
    public class TFPageNavigator
    {
        //public const string PAGE_CLOSEAPP = "closeapp";

        public const string PAGE_STARTPAGE = "startpage";
        //public const string SUBPAGE_STARTPAGE_MENU = "startpage_menu";
        //public const string SUBPAGE_STARTPAGE_SETTINGS = "startpage_settings";

        public const string PAGE_MAINPAGE = "mainpage";

                              
        public AppModel model;

        public StartPage StartPageObj { get; set; }
        public MainPage MainPageObj { get; set; }

        public string CurrentMainPage { get; set; } = "";
        public string CurrentSubPage { get; set; } = "";
        public string LastMainPage { get; set; } = "";
        public string LastSubPage { get; set; } = "";


        public TFPageNavigator()
        {
        }
        public TFPageNavigator(AppModel _model)
        {
            model = _model;
        }

        public void NavigateTo(string mainPage, string subPage = "")
        {
            LastMainPage = ""+CurrentMainPage;
            LastSubPage = ""+CurrentSubPage; 
            CurrentMainPage = mainPage;
            CurrentSubPage = subPage;
            switch (mainPage)
            {
                case PAGE_STARTPAGE:
                    //if (StartPageObj == null) { StartPageObj = new StartPage(model); }
                    if (LastMainPage != CurrentMainPage)
                    {
                        StartPageObj = new StartPage(model);
                        AppModel.Instance.StartPage = StartPageObj;
                        //model.App.MainPage = StartPageObj.GetPage(subPage); 
                        if (model.App.Windows.Count > 0)
                        {
                            model.App.Windows[0].Page = StartPageObj.GetPage(subPage);
                        }
                    }
                    else
                    {
                        StartPageObj.GetPage(subPage);
                    }
                    break;


                case PAGE_MAINPAGE:
                    //if (GroupCalendarObj == null) { GroupCalendarObj = new GroupCalendarPage(model); }                    
                    if (LastMainPage != CurrentMainPage)
                    {
                        MainPageObj = new MainPage(model);
                        AppModel.Instance.MainPage = MainPageObj;
                        //model.App.MainPage = MainPageObj.GetPage(subPage);
                        if (model.App.Windows.Count > 0)
                        {
                            model.App.Windows[0].Page = StartPageObj.GetPage(subPage);
                        }
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
