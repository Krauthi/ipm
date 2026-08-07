using CommunityToolkit.Maui.Core.Platform;
using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Cloud.Translation.V2;
using iPMCloud.Mobile.Helpers;
using iPMCloud.Mobile.Interfaces;
using iPMCloud.Mobile.Views;
using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.vo.GlobalObjects;
using iPMCloud.Mobile.vo.wso;
using MetadataExtractor.Formats.Photoshop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
// TODO: Xamarin.RangeSlider not MAUI-compatible - needs replacement
// using Xamarin.RangeSlider.Forms;

//using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Storage;
// TODO: NativeMedia not MAUI-compatible - needs replacement with Microsoft.Maui.Media
// using NativeMedia;
//using Plugin.Permissions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ZXing.Net.Maui;
//using static System.Net.Mime.MediaTypeNames;
//using static Android.Graphics.ColorSpace;
//https://docs.microsoft.com/de-de/xamarin/essentials/preferences?tabs=android

namespace iPMCloud.Mobile
{
    public partial class MainPage : ContentPage
    {
        // private BackgroundWorker backgroundWorker = new BackgroundWorker();

        // Forwarding properties for elements moved into DSGVOPageContainerView
        //private Grid DSGVOPage_Container => DSGVOPageContainerView.ContainerGrid;
        //private Border btn_back_dsgvo => DSGVOPageContainerView.BtnBackDsgvo;
        private WorkerPageContainerView _workerPageContainerView;
        private PersonTimesPageView _personTimesPageView;
        private WorkerPageContainerView WorkerPageContainerView => _workerPageContainerView ??= new WorkerPageContainerView();
        private PersonTimesPageView PersonTimesPageView => _personTimesPageView ??= new PersonTimesPageView();

        // Forwarding properties for elements moved into WorkerPageContainerView
        private Grid WorkerPage_Container => WorkerPageContainerView.ContainerGrid;
        private Border btn_worker_back => WorkerPageContainerView.BtnWorkerBack;
        private VerticalStackLayout btn_workercategorysearch => WorkerPageContainerView.BtnWorkercategorysearch;
        private VerticalStackLayout btn_workernamesearch => WorkerPageContainerView.BtnWorkernamesearch;
        private VerticalStackLayout btn_workerbuildingsearch => WorkerPageContainerView.BtnWorkerbuildingsearch;
        private Label lb_workerbuildingsearche => WorkerPageContainerView.LbWorkerbuildingsearche;
        private CustomEntry entry_workersearch => WorkerPageContainerView.EntryWorkersearch;
        private ScrollView list_worker_scroll => WorkerPageContainerView.ListWorkerScroll;
        private StackLayout list_worker => WorkerPageContainerView.ListWorker;



        public DisplayInfo di = DeviceDisplay.MainDisplayInfo;
        public double density = 0;//> di.Density;           // px pro dp
        public double screenWidthDp = 0;//> di.Width / di.Density;
        public double screenHeightDp = 0;//> di.height / di.Density;

        public string lastPlanTab = "A";
        public string lastPlanTabView = "A";


        public bool isInitialize = false;
        public bool _isShowing = false;
        private bool _isOpeningWorkerModal = false;
        private bool _isOpeningPersonTimesModal = false;

        private iPMCloud.Mobile.Services.IUploadService _uploadService;
        private iPMCloud.Mobile.Services.IBackgroundSyncService _backgroundSyncService;


        public MainPage()
        {
            isInitialize = true;

            InitializeComponent();

            density = di.Density;
            screenWidthDp = di.Width / di.Density;
            screenHeightDp = di.Height / di.Density;
            //MainPageAgain();
        }

        public async void MainPageAgain()
        {
            try
            {

                isInitialize = true;
                //AppModel.Instance.anImage = backgroundIMG;

                AppModel.Instance.MainPageOverlay = overlay;

                AppModel.Instance._showall_again_OrderCategory_frame = btn_back_inBuildingOrder_category_showall_again;
                AppModel.Instance._showall_OrderCategory_frame = btn_back_inBuildingOrder_category_showall;

                // Show spinner immediately so the page looks responsive, then yield to
                // allow the first frame to paint before heavy initialization runs.
                overlay.IsVisible = true;
                await Task.Yield();


                AppModel.Instance.Lang = Lang.Load();

                ShowDisconnected();

                var checkPerm = await CheckLocationPermissionsAndInitGps();
                if (checkPerm)
                {
                    //if(AppModel.Instance.AllBuildings == null || AppModel.Instance.AllBuildings.Count == 0)
                    //{
                    //   AppModel.Instance.InitBuildingsAsync();
                    //}

                    CheckAllSyncFromUpload(true);
                    InitStartPageHandlers();

                    //ObjektPlanWeekMobile.Delete(AppModel.Instance);
                    // Objekte sycnen erforderlich nach 4 Stunden
                    SyncBuilding();

                    // ***  Wird mit BuildSync ausgeführt !!! ***
                    // ***  Init_PlanTabs(((int)DateTime.Now.DayOfWeek));
                    // Gespeichert PlanPerson KW vom Mobile Laden wenn vorhanden.
                    if (AppModel.Instance.AppControll.showObjektPlans)
                    {
                        //var PlanResponse = ObjektPlanWeekMobile.Load(AppModel.Instance);
                        //if (PlanResponse != null) { AppModel.Instance.PlanResponse = PlanResponse; }
                        Fill_DayPicker();
                    }

                    GetChecksInfo(checkInfoLastView);

                    // Load position data on a background thread to avoid blocking the UI thread.
                    // LeistungPackWSO.Load only reads AppModel data and performs I/O; the result
                    // is assigned back on the UI thread after the await completes.
                    AppModel.Instance.allPositionInWork = await Task.Run(() => LeistungPackWSO.Load(AppModel.Instance));

                    ShowMainPage();


                    frame_plantabA.Margin = new Thickness(0, -8, 2, 0);
                    frame_plantabB.Margin = new Thickness(0, 0, 2, 0);
                    frame_plantabCe.Margin = new Thickness(0, 0, 2, 0);
                    frame_plantabC.Margin = new Thickness(0, 0, 2, 0);
                    frame_planConA.IsVisible = true;
                    frame_planConB.IsVisible = false;
                    frame_planConCe.IsVisible = false;
                    frame_planConC.IsVisible = false;
                }
                else
                {
                    overlay.IsVisible = false;
                    await DisplayAlertAsync("Fehlende Berechtigungen!", "Bitte beenden Sie die App und starten diese neu!\n\nAktivieren Sie nach dem neustart die benötigten Berechtigungen!", "OK");
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Fehler in MainPageAgain: " + ex.Message + " | StackTrace: " + ex.StackTrace);
                await DisplayAlertAsync("Fehler!", "Es ist ein Fehler aufgetreten! Bitte senden Sie uns die Mobile LOG über die Einstellungen, damit wir das Problem analysieren können.", "OK");
            }
        }


        private int checkInfoLastView = 7;  // 7 = offene/faellige    99 = alle
        private async void GetChecksInfo(int view, bool showLoader = false)
        {
            if (AppModel.Instance.AppControll.showChecks)
            {
                if (showLoader)
                {
                    overlay.IsVisible = true;
                    await Task.Delay(1);
                }

                if (view != 0)
                {
                    checkInfoLastView = view;
                }

                var result = Task.Run(() => { return AppModel.Instance.Connections.GetChecksInfo(AppModel.Instance.Person.id, checkInfoLastView); }).Result;
                if (result != null && result.checks != null)
                {
                    AppModel.Instance.ChecksInfoResponse = result;
                    CheckClass.SaveChecksInfo(AppModel.Instance.ChecksInfoResponse);
                }
                else
                {
                    var loadResp = CheckClass.LoadChecksInfo();
                    AppModel.Instance.ChecksInfoResponse = loadResp;// Wenn keine da, dann New Response()
                }

                SetChecksCount();

                if (showLoader || true)
                {
                    BuildChecksInfoList();
                }
            }
        }


        private async void BuildChecksInfoList()
        {



            //Checklisten nicht anzeigen wenn diese nochim UpladStack sind!
            var stackChecks = CheckClass.LoadAllFromUploadStack();
            List<Int32> inStackIds = new List<Int32>();
            if (stackChecks != null && stackChecks.Count > 0) { stackChecks.ForEach(_ => { inStackIds.Add(_.id); }); }

            AppModel.Instance.ChecksInfoResponse.checks = AppModel.Instance.ChecksInfoResponse.checks.Where(_ => !inStackIds.Contains(_.checkA_id)).ToList();
            var checks = AppModel.Instance.ChecksInfoResponse.checks.OrderBy(_ => _.naeststeFaelligkeitDate);

            frame_planListCeoffen.Children.Clear();// = null;
            frame_planListCefaellig.Children.Clear();// = null;
            frame_planListCeerl.Children.Clear();// = null;

            var oa = checks.Where(_ => _.lastStateOfCheck_a == "Offen").ToList();
            var ob = checks.Where(_ => _.lastStateOfCheck_a != "Offen"
                    && _.naeststeFaelligkeitDate < 8
                    && _.berechnunginterval > 0).ToList();
            var oc = checks.Where(_ => (_.lastStateOfCheck_a != "Offen"
                && _.naeststeFaelligkeitDate >= 8)
                || (_.lastStateOfCheck_a != "Offen"
                && _.berechnunginterval == 0)).ToList();

            foreach (var oaI in oa)
            {
                frame_planListCeoffen.Children.Add(Check.GetOffeneList(
                    oaI, screenWidthDp, new Command<IntBoolParam>(StartOrOpenCheckA)
                    ));
            }
            foreach (var obI in ob)
            {
                frame_planListCefaellig.Children.Add(Check.GetOffeneList(
                obI, screenWidthDp, new Command<IntBoolParam>(StartOrOpenCheckA)
                ));
            }
            if (checkInfoLastView == 99)
            {
                foreach (var ocI in oc)
                {
                    frame_planListCeerl.Children.Add(Check.GetOffeneList(
                    ocI, screenWidthDp, new Command<IntBoolParam>(StartOrOpenCheckA)
                    ));
                }
                frame_planConCe_erlhead.IsVisible = true;
            }
            else
            {
                frame_planConCe_erlhead.IsVisible = false;
            }


            SetChecksCount();
            overlay.IsVisible = false;
            await Task.Delay(1);
        }


        public async void SelectedObjektAufterNotScan_Check(IntBoolParam intBol)
        {
            if (AppModel.Instance.allPositionInWork != null)
            {
                popupContainer_info_notscan_titel.Text = "ACHTUNG!";
                popupContainer_info_notscan_text.Text = "Es sind noch nicht abgeschlossene Arbeiten aktiv. Bitte erst beenden, bevor Sie ein anderes Objekt direkt auswählen.";
                popupContainer_info_notscan_okbtn.GestureRecognizers.Clear();
                var tgr_over_ns = new TapGestureRecognizer();
                tgr_over_ns.Tapped += (object o, TappedEventArgs ev) => { popupContainer_info_notscan.IsVisible = false; };
                tgr_over_ns.Tapped += (object o, TappedEventArgs ev) => { popupContainer_info_notscan.IsVisible = false; };
                popupContainer_info_notscan_okbtn.GestureRecognizers.Add(tgr_over_ns);
                btn_endselectedcancel.GestureRecognizers.Clear();
                popupContainer_info_notscan.IsVisible = true;
            }
            else
            {
                overlay.IsVisible = true;
                await Task.Delay(1);
                AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
                AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == intBol.val);
                AppModel.Instance.SettingModel.SaveSettings();
                list_notscan.Children.Clear();
                if (intBol.bol) { ShowMainPage(); }
                else
                {
                    await lastBuilding_Container.FadeToAsync(0, 500, Easing.SpringOut);
                    SetLastBuilding();
                    await lastBuilding_Container.FadeToAsync(1, 500, Easing.SpringIn);
                    overlay.IsVisible = false;
                    await Task.Delay(1);
                }
            }
        }






        private int checkQuestIndex = 0;
        public async void StartOrOpenCheckA(IntBoolParam intBol)
        {
            AppModel.Instance.selectedCheckA = null;
            AppModel.Instance.selectedCheckInfo = null;
            overlay.IsVisible = true;
            await Task.Delay(50);


            foreach (var check in AppModel.Instance.ChecksInfoResponse.checks)
            {
                if (check.id == intBol.val && AppModel.Instance.selectedCheckInfo == null)
                {
                    AppModel.Instance.selectedCheckInfo = check;
                }
            }


            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                if (AppModel.Instance.allPositionInWork != null)
                {
                    popupContainer_info_notscan_titel.Text = "ACHTUNG!";
                    popupContainer_info_notscan_text.Text = "Es sind noch nicht abgeschlossene Arbeiten aktiv. Bitte erst beenden, bevor Sie ein anderes Objekt direkt auswählen oder eine Checkliste bearbeiten möchten.";
                    popupContainer_info_notscan_okbtn.GestureRecognizers.Clear();
                    var tgr_over_ns = new TapGestureRecognizer();
                    tgr_over_ns.Tapped += (object o, TappedEventArgs ev) => { popupContainer_info_notscan.IsVisible = false; };
                    tgr_over_ns.Tapped += (object o, TappedEventArgs ev) => { popupContainer_info_notscan.IsVisible = false; };
                    popupContainer_info_notscan_okbtn.GestureRecognizers.Add(tgr_over_ns);
                    btn_endselectedcancel.GestureRecognizers.Clear();
                    popupContainer_info_notscan.IsVisible = true;
                    AppModel.Instance.selectedCheckInfo = null;
                    overlay.IsVisible = false;
                    await Task.Delay(1);
                    return;
                }

                if (AppModel.Instance.LastBuilding == null || (AppModel.Instance.LastBuilding != null
                    && AppModel.Instance.LastBuilding.id != AppModel.Instance.selectedCheckInfo.objektid))
                {
                    // letztes Objekt ist NICHT GLEICH
                    AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = AppModel.Instance.selectedCheckInfo.objektid;
                    // Zurücksetzten aller States für die Auswahl der Ausführungen
                    AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
                    AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = AppModel.Instance.selectedCheckInfo.objektid;
                    AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == AppModel.Instance.selectedCheckInfo.objektid);
                    try
                    {
                        AppModel.Logger.Info("CHECK-IN (OHNE QR-SCAN): " + AppModel.Instance.LastBuilding.strasse + " " + AppModel.Instance.LastBuilding.hsnr + AppModel.Instance.LastBuilding.plz + " " + AppModel.Instance.LastBuilding.ort);
                    }
                    catch (Exception) { }
                    AppModel.Instance.SettingModel.SaveSettings();

                    await lastBuilding_Container.FadeToAsync(0, 500, Easing.SpringOut);
                    SetLastBuilding();
                    await lastBuilding_Container.FadeToAsync(1, 500, Easing.SpringIn);
                    overlay.IsVisible = false;
                    await Task.Delay(1);
                }
                StartOrOpenCheckA_next(intBol);
            }
            else
            {
                overlay.IsVisible = false;
                await Task.Delay(1);
                // OBJEKT SCANEN !!!
                if (AppModel.Instance.LastBuilding != null
                    && AppModel.Instance.LastBuilding.id == AppModel.Instance.selectedCheckInfo.objektid)
                {
                    StartOrOpenCheckA_next(intBol);
                }
                else
                {
                    if (AppModel.Instance.allPositionInWork != null && AppModel.Instance.allPositionInWork.leistungen.Count > 0)
                    {
                        await DisplayAlertAsync("OFFENE ARBEITEN",
                            "Die Checkliste kann nicht bearbeitet werden! Es sind noch offene Arbeiten in einem anderen Objekt aktiv. Diese müssen Sie zuerst beenden.",
                            "OK");
                    }
                    else
                    {
                        ShowBuildingScanPage_Check(intBol);
                    }
                }
            }
        }
        IntBoolParam intBol_Check = null;
        private async void ShowBuildingScanPage_Check(IntBoolParam intBol)
        {
            intBol_Check = intBol;
            ShowBuildingScanPage(true);
        }
        public bool MethodAfterScan_check()
        {
            //lay_buildingscan.Children.Clear();
            ShowMainPage();
            if (AppModel.Instance.LastBuilding != null
                && AppModel.Instance.LastBuilding.id == AppModel.Instance.selectedCheckInfo.objektid)
            {
                StartOrOpenCheckA_next(intBol_Check);
            }
            return true;
        }

        public async void StartOrOpenCheckA_next(IntBoolParam intBol)
        {
            overlay.IsVisible = false;
            await Task.Delay(1);
            intBol_Check = intBol;
            var existCheckAInWork = CheckClass.GiveCheckAToWork();
            if (existCheckAInWork == -1 || (existCheckAInWork > 0 && existCheckAInWork == AppModel.Instance.selectedCheckInfo.checkA_id))
            {
                if (!intBol.bol)
                {
                    // Keine Offene
                    popupContainer_quest_startcheckquest.IsVisible = true;
                }
                else
                {
                    StartOrOpenCheckA_next_now(intBol_Check);
                }
            }
            else
            {
                overlay.IsVisible = false;
                await Task.Delay(1);
                // Es wird noch eine bearbeitet "Offen"
                await DisplayAlertAsync("CHECKLISTE", "Diese Checkliste kann nicht gestartet werden, da Sie aktuell noch eine andere Checkliste in Bearbeitung haben und zuerst fertig stellen müssen.", "OK");
                //popupContainer_dialog_titel.Text = "CHECKLISTE";
                //popupContainer_dialog_text.Text = "Diese Checkliste kann nicht gestartet werden, da Sie aktuell noch eine andere Checkliste in bearbeitung haben und zuerst fertig stellen müssen.";
                //popupContainer_dialog.IsVisible = true;

                //popupContainer_dialog_btn_ok.GestureRecognizers.Clear();
                //var tgr_popupContainer_dialog_btn_ok = new TapGestureRecognizer();
                //tgr_popupContainer_dialog_btn_ok.Tapped += (object o, TappedEventArgs ev) =>
                //{
                //    popupContainer_dialog.IsVisible = false;
                //    popupContainer_dialog_btn_ok.GestureRecognizers.Clear();
                //};
                //popupContainer_dialog_btn_ok.GestureRecognizers.Add(tgr_popupContainer_dialog_btn_ok);

                intBol_Check = null;
            }
        }
        public async void StartOrOpenCheckA_next_start()
        {
            popupContainer_quest_startcheckquest.IsVisible = false;
            StartOrOpenCheckA_next_now(intBol_Check);
        }
        public async void StartOrOpenCheckA_next_cancel()
        {
            intBol_Check = null;
            popupContainer_quest_startcheckquest.IsVisible = false;
        }

        public async void StartOrOpenCheckA_next_now(IntBoolParam intBol)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            var list = AppModel.Instance.ChecksInfoResponse.checks;


            if (!intBol.bol)// Keine Offene
            {
                // Starte eine neu Befragung und öffne direkt zur Bearbeitung
                var result = await Task.Run(() => { return AppModel.Instance.Connections.StartCheck(intBol.val); });
                if (result != null)
                {
                    // Wenn es schonmal die Befragung (CheckA) gegeben hatte, dann löschen und aktuelle verwenden
                    var delCheckA = CheckClass.DeleteCheckA(result.id);
                    result.start = JavaScriptDateConverter.Convert(DateTime.Now);
                    CheckClass.SaveCheckA(result);
                    AppModel.Instance.selectedCheckA = result;
                    UpdateChecksInfoResponse(intBol.val);
                }
                else
                {
                    // DIALOG - Konnte nicht geladen werden!
                    // NUR ONLINE MÖGLICH
                    if (!AppModel.Instance.IsInternet)
                    {
                        overlay.IsVisible = false;
                        await Task.Delay(1);
                        await DisplayAlertAsync("Sie sind nicht Online!",
                            "Für das Starten einer Checkliste müssen Sie zum herunterladen der Checklistendaten Online sein!",
                            "OK");
                    }
                    else
                    {
                        overlay.IsVisible = false;
                        await Task.Delay(1);
                        await DisplayAlertAsync("Fehler bei Checkliste starten!",
                            "Es konnten keine Checklistendaten heruntergeladen werden! Bitte gehen Sie unter Einstellungen und senden Sie uns die Mobile LOG. Wir werden versuchen das Problem zu analysieren.",
                            "OK");
                    }
                    return;
                }
            }
            else
            {
                // Öffne offene Befragung und weiter bearbeiten
                var checkA = CheckClass.LoadCheckA(AppModel.Instance.selectedCheckInfo.checkA_id);
                if (checkA != null)
                {
                    AppModel.Instance.selectedCheckA = checkA;
                }
                else
                {
                    // Offen Befragung wurde noch nicht gespeichert!   Hole Offene Befragung
                    var result = await Task.Run(() => { return AppModel.Instance.Connections.GetCheckA(AppModel.Instance.selectedCheckInfo.checkA_id).Result; });
                    if (result != null)
                    {
                        result.start = JavaScriptDateConverter.Convert(DateTime.Now);
                        CheckClass.SaveCheckA(result);
                        AppModel.Instance.selectedCheckA = result;
                        UpdateChecksInfoResponse(intBol.val);
                    }
                    else
                    {
                        // DIALOG - Konnte nicht geladen werden!
                        // NUR ONLINE MÖGLICH
                        if (!AppModel.Instance.IsInternet)
                        {
                            overlay.IsVisible = false;
                            await Task.Delay(1);
                            await DisplayAlertAsync("Sie sind nicht Online!",
                                "Für das Starten einer Checkliste müssen Sie zum herunterladen der Checklistendaten Online sein!",
                                "OK");
                        }
                        else
                        {
                            overlay.IsVisible = false;
                            await Task.Delay(1);
                            await DisplayAlertAsync("Fehler bei Checkliste starten!",
                                "Es konnten keine Checklistendaten heruntergeladen werden! Bitte gehen Sie unter Einstellungen und senden Sie uns die Mobile LOG. Wir werden versuchen das Problem zu analysieren.",
                                "OK");
                        }
                        return;
                    }

                }
            }

            double w = screenWidthDp;
            CheckPage_Container.WidthRequest = w;
            CheckPage_position_Container.WidthRequest = w;
            CheckPage_Container.IsVisible = true;

            BuildChecksInfoList();

            if (AppModel.Instance.selectedCheckA != null)
            {
                //btn_check_sub.GestureRecognizers.Clear();
                //var tgr_btn_check_sub = new TapGestureRecognizer();
                //tgr_btn_check_sub.Tapped += (object o, TappedEventArgs ev) => { checkQuestIndex--; BuildCheckQuestStack(); };
                //btn_check_sub.GestureRecognizers.Add(tgr_btn_check_sub);
                //btn_check_add.GestureRecognizers.Clear();
                //var tgr_btn_check_add = new TapGestureRecognizer();
                //tgr_btn_check_add.Tapped += (object o, TappedEventArgs ev) => { checkQuestIndex++; BuildCheckQuestStack(); };
                //btn_check_add.GestureRecognizers.Add(tgr_btn_check_add);
                //btn_check_ready.GestureRecognizers.Clear();
                //var tgr_btn_check_ready = new TapGestureRecognizer();
                //tgr_btn_check_ready.Tapped += (object o, TappedEventArgs ev) => { SetReadyCheckA(); };
                //btn_check_ready.GestureRecognizers.Add(tgr_btn_check_ready);

                btn_info_check_text1.Text = AppModel.Instance.selectedCheckA.bezeichnung;
                btn_info_check_text2.Text = "Datum: " + JavaScriptDateConverter.Convert(long.Parse(AppModel.Instance.selectedCheckA.datum)).ToString("dd.MM.yyyy");
                frame_info_check_badge.Children.Clear();
                frame_info_check_badge.Children.Add(Check.GetBadgeFrame(AppModel.Instance.selectedCheckInfo.naeststeFaelligkeitDate, 30));

                BuildCheckQuestStack();
            }
            overlay.IsVisible = false;
            await Task.Delay(1);
        }

        public void UpdateChecksInfoResponse(Int32 checkid, string state = "Offen")
        {
            foreach (var check in AppModel.Instance.ChecksInfoResponse.checks)
            {
                if (check.id == checkid)
                {
                    check.lastStateOfCheck_a = state;
                }
            }
            if (AppModel.Instance.selectedCheckInfo != null)
            {
                AppModel.Instance.selectedCheckInfo.lastStateOfCheck_a = state;
            }
            CheckClass.SaveChecksInfo(AppModel.Instance.ChecksInfoResponse);
            BuildChecksInfoList();
        }

        public void UpdateChecksInfoResponseWhenDeleteCheckA(Int32 checkid)
        {
            foreach (var check in AppModel.Instance.ChecksInfoResponse.checks)
            {
                if (check.id == checkid)
                {
                    check.checkA_id = 0;
                    check.lastStateOfCheck_a = "-";
                }
            }
            AppModel.Instance.selectedCheckInfo = null;
            AppModel.Instance.selectedCheckA = null;
            CheckClass.SaveChecksInfo(AppModel.Instance.ChecksInfoResponse);
            BuildChecksInfoList();
        }


        public void OpenDelCheckA()
        {
            popupContainer_quest_delcheckquest.IsVisible = true;
        }
        public async void DelCheckA_now()
        {
            popupContainer_quest_delcheckquest.IsVisible = false;

            var result = await Task.Run(() => { return AppModel.Instance.Connections.DelCheckA(AppModel.Instance.selectedCheckA.id).Result; });
            if (result)
            {
                CheckClass.DeleteCheckA(AppModel.Instance.selectedCheckA.id);
                UpdateChecksInfoResponseWhenDeleteCheckA(AppModel.Instance.selectedCheckInfo.id);
                checkQuestStack.Children.Clear();
                CheckPage_Container.IsVisible = false;
            }
        }
        public void DelCheckA_cancel()
        {
            popupContainer_quest_delcheckquest.IsVisible = false;
        }


        public void BuildCheckQuestStack()
        {
            if (checkQuestIndex < 0)
            { checkQuestIndex = 0; }
            if (checkQuestIndex >= AppModel.Instance.selectedCheckA.antworten.Count)
            { checkQuestIndex = AppModel.Instance.selectedCheckA.antworten.Count - 1; }

            int i = 0;
            string holdKatName = "";
            checkQuestStack.Children.Clear();
            CheckLeistungAntwort quest = null;
            AppModel.Instance.selectedCheckA.antworten.ForEach(q =>
            {
                // Wenn "kat" LEER - dann ist für Mobile deaktiviert!
                if (!String.IsNullOrWhiteSpace(q.kat))
                {
                    if (holdKatName != q.kat)
                    {
                        checkQuestStack.Children.Add(Check.GetQuestKategorieHeader(q.kat));
                    }
                    switch (q.type)
                    {
                        case "0":// Ja / Nein / Keine
                            checkQuestStack.Children.Add(Check.GetQuestMain_0(q));
                            break;
                        case "1":// Text
                            checkQuestStack.Children.Add(Check.GetQuestMain_1(q));
                            break;
                        case "2":// Wert
                            checkQuestStack.Children.Add(Check.GetQuestMain_2(q));
                            break;
                        case "3":// Bild
                            checkQuestStack.Children.Add(Check.GetQuestMain_3(q));
                            break;
                        case "4":// Multi
                            if (q.multi == 1)
                            {
                                checkQuestStack.Children.Add(Check.GetQuestMain_4a(q));
                            }
                            else
                            {
                                checkQuestStack.Children.Add(Check.GetQuestMain_4b(q));
                            }
                            break;
                        case "7":// Unterschrift
                            checkQuestStack.Children.Add(Check.GetQuestMain_7(q));
                            break;
                        case "10":// Text
                            checkQuestStack.Children.Add(Check.GetQuestMain_10(q));
                            break;
                    }
                    holdKatName = q.kat;
                }

                if (i == checkQuestIndex) { quest = q; }
                i++;
            });

            //Spacer
            checkQuestStack.Children.Add(
                new StackLayout
                {
                    HeightRequest = 100,
                    MinimumHeightRequest = 100,
                    HorizontalOptions = LayoutOptions.Fill
                });

            UpdateCheckAState();
        }

        public void UpdateCheckAState()
        {
            checkQuestStack_scroll.HeightRequest = this.Height - checkQuestStack_scroll.Y - img_info_check_typ_container.Height - 6;

            int antMax = AppModel.Instance.selectedCheckA.antworten.Where(_ => _.required == 1 && _.isReady && _.type != "10").Count();
            int antVon = AppModel.Instance.selectedCheckA.antworten.Where(_ => _.required == 1 && _.type != "10").Count();
            int diff = antVon - antMax;

            info_check_text2a.IsVisible = antMax != antVon;
            btn_notice_save_check_ready.IsVisible = antMax == antVon;
            info_check_text1a.Text = "Gesamt " + AppModel.Instance.selectedCheckA.antworten.Where(_ => _.type != "10").Count() + " Fragen";
            info_check_text2a.Text = "(Pflicht) " + AppModel.Instance.selectedCheckA.antworten.Where(_ => _.required == 1 && _.isReady && _.type != "10").Count()
                + " von " + AppModel.Instance.selectedCheckA.antworten.Where(_ => _.required == 1 && _.type != "10").Count() + " beantwortet";
        }

        public async void CloseCheckA(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            await Task.Delay(1000);

            CheckClass.SaveCheckA(AppModel.Instance.selectedCheckA);
            CheckPage_Container.IsVisible = false;
            // Update CheckInfo mit CheckA
            foreach (var check in AppModel.Instance.ChecksInfoResponse.checks)
            {
                if (check.id == AppModel.Instance.selectedCheckInfo.id)
                {
                    check.checkA_id = AppModel.Instance.selectedCheckA.id;
                    check.lastStateOfCheck_a = "Offen";
                }
            }
            CheckClass.SaveChecksInfo(AppModel.Instance.ChecksInfoResponse);

            await Task.Delay(1000);
            // Befragung speichern und pausieren
            AppModel.Instance.selectedCheckA.antworten.ForEach(_ => _.ClearGui());
            checkQuestStack.Children.Clear();
            overlay.IsVisible = false;
        }

        public async void btn_ReadyCheckAToUploadTapped_check_bem(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            // Convert CheckA to RealRequestCheckA
            AppModel.Instance.selectedCheckA = Check.ConvertToCheckARequest(AppModel.Instance.selectedCheckA);
            AppModel.Instance.selectedCheckA.end = JavaScriptDateConverter.Convert(DateTime.Now);
            foreach (var item in AppModel.Instance.ChecksInfoResponse.checks)
            {
                if (AppModel.Instance.selectedCheckA.refid == item.id)
                {
                    DateTime newFaellig = DateTime.Now;// JavaScriptDateConverter.Convert(long.Parse(item.gueltigbis), 0);
                    if (item.berechnunginterval == 30)
                    {
                        item.gueltigbis = JavaScriptDateConverter.Convert(newFaellig.AddMonths(1)).ToString(); // nächste Ausführung
                        item.naeststeFaelligkeitDate = 30;
                    }
                    else
                    {
                        item.gueltigbis = JavaScriptDateConverter.Convert(newFaellig.AddDays(item.berechnunginterval)).ToString(); // nächste Ausführung 
                        item.naeststeFaelligkeitDate = item.berechnunginterval;
                    }
                    if (item.berechnunginterval == 0)
                    {
                        item.gueltigbis = "0";
                    }
                    item.lastStateOfCheck_a = "Erledigt";
                }
            }
            CheckClass.SaveChecksInfo(AppModel.Instance.ChecksInfoResponse);
            BuildChecksInfoList();

            // Set Guids
            foreach (var antwort in AppModel.Instance.selectedCheckA.antworten)
            {
                antwort.check_guid = AppModel.Instance.selectedCheckA.guid;
                antwort.guid = Guid.NewGuid().ToString();
                if (antwort.bem != null)
                {
                    antwort.bem.antwort_guid = antwort.guid;
                    antwort.bem.guid = Guid.NewGuid().ToString();
                    if (antwort.bem.imgs != null && antwort.bem.imgs.Count > 0)
                    {
                        foreach (var img in antwort.bem.imgs)
                        {
                            img.bem_guid = antwort.bem.guid;
                            img.guid = Guid.NewGuid().ToString();
                            CheckLeistungAntwortBemImg.Save(img);
                            img.url = "";
                        }
                        ;
                    }
                }
            }
            ;

            if (CheckClass.ToUploadStack(AppModel.Instance.selectedCheckA))
            {
                CheckClass.DeleteCheckA(AppModel.Instance.selectedCheckA.id);
                CheckAllSyncFromUpload();
            }

            await Task.Delay(1000);
            //AppModel.Instance.selectedCheckA.antworten.ForEach(_ => _.ClearGui());
            checkQuestStack.Children.Clear();
            CheckPage_Container.IsVisible = false;

            // //GetChecksInfo(checkInfoLastView, true);
            overlay.IsVisible = false;
            await Task.Delay(1);
        }

        public async void OpenCheckA_Singature(CheckLeistungAntwort quest)
        {
            SignatureResult result = await SignatureModalPage.ShowAsync(this);
            if (result is null)
            {
                quest.img_sig.Source = null;
                //quest.signPad.Clear();
                return;
            }

            byte[] pngBytes = result.PngBytes;
            ImageSource signatureImage = result.Image;

            await quest.Tap_a7_ReturnSig(pngBytes, signatureImage);
            quest.img_sig.Source = signatureImage;
        }

        public void OpenCheckA_Singatureaa(CheckLeistungAntwort quest)
        {
            CheckPage_position_Container2.WidthRequest = screenWidthDp;
            checkQuestStack_signature_scroll.HeightRequest =
                screenHeightDp - checkQuestStack_signature_scroll.Y - 13;
            checkQuestStack_signature.Children.Clear();
            quest.signPad = Check.GetSignElement();
            //checkQuestStack_signature.Children.Add(Check.GetQuestMain_7_PopUp(quest));
            CheckPage_Signature_Container.IsVisible = true;
        }
        public void CloseCheckA_Singature(object sender, EventArgs e)
        {
            CheckPage_Signature_Container.IsVisible = false;
        }

        public async void SetCheckAToReady(object sender, EventArgs e)
        {
            // Befragung beenden
            checkQuestStack.Children.Clear();
            CheckPage_Container.IsVisible = false;
        }



        private bool _SelectedPosForNotice_check_bem_isquest = false;
        private CheckLeistungAntwort _SelectedPosForNotice_check_bem = null;

        public async void ShowNoticeView_check_bem(CheckLeistungAntwort quest, bool isQuest = false, bool isSign = false)
        {
            _SelectedPosForNotice_check_bem_isquest = isQuest;
            double w = screenWidthDp;
            double h = screenHeightDp;

            CheckPage_Bem_Container.WidthRequest = w;
            CheckPage_Bem_inner_Container.WidthRequest = w;
            CheckPage_Bem_inner_Container.HeightRequest = h;
            btn_takePhoto_frame_check_bem.IsVisible = true;
            btn_takePhotoAttachment_frame_check_bem.IsVisible = true;

            entry_notice_container_check_bem.IsVisible = !isQuest || isSign;
            photobar_check_bem.IsVisible = !isSign;
            CheckPage_Bem_Title.Text = !isQuest ? "Bemerkung" : "Bilder hinzufügen";

            overlay.IsVisible = true;
            await Task.Delay(1);

            //_SelectedBemerkungForNotice_check_bem = new BemerkungWSO();
            if (quest.bemWSO == null) { quest.bemWSO = new BemerkungWSO(); }
            _SelectedPosForNotice_check_bem = quest;

            var stackPos = Check.GetQuestMain_3_inlay(quest);
            noticeFor_Pos_check_bem.Children.Clear();
            noticeFor_Pos_check_bem.Children.Add(stackPos);
            if (quest.bemWSO != null && quest.bemWSO.photos != null && quest.bemWSO.photos.Count > 0)
            {
                foreach (var ph in quest.bemWSO.photos)
                {
                    if (ph.bytes != null && ph.bytes.Length > 0)
                    {
                        ph.stack = BildWSO.GetAttachmentForNoticeElement(
                                ImageSource.FromStream(() => new MemoryStream(ph.bytes)),
                                 new DateTime(long.Parse(ph.name)).ToString("dd.MM.yyyy-HH:mm:ss"),
                                 null);

                        var frame = (Border)((StackLayout)(ph.stack.Children[0])).Children[2];
                        frame.GestureRecognizers.Clear();
                        frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork_check_bem), CommandParameter = ph });
                        noticePhotoStack_check_bem.Children.Add(ph.stack);
                    }
                }
                btn_takePhoto_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
                btn_takePhotoAttachment_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
            }
            if (quest.bemWSO != null)
            {
                entry_notice_check_bem.Text = quest.bemWSO.text;
            }


            //CheckNoticeFalid_check_bem();

            CheckPage_Bem_Container.IsVisible = true;

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        public void btn_NoticeBackTapped_check_bem(object sender, EventArgs e)
        {
            this.Focus();
            entry_notice_check_bem.Text = "";
            noticePhotoStack_check_bem.Children.Clear();
            CheckPage_Bem_Container.IsVisible = false;
        }

        public async void btn_NoticeSaveTapped_check_bem(object sender, EventArgs e)
        {
            this.Focus();
            if (!String.IsNullOrWhiteSpace(entry_notice_check_bem.Text.Trim())
                || (_SelectedPosForNotice_check_bem.bemWSO.photos != null
                && _SelectedPosForNotice_check_bem.bemWSO.photos.Count > 0))
            {
                overlay.IsVisible = true;
                await Task.Delay(1);

                _SelectedPosForNotice_check_bem.bemWSO.prio = 0;
                _SelectedPosForNotice_check_bem.bemWSO.gruppeid = AppModel.Instance.selectedCheckInfo.gruppeid;
                _SelectedPosForNotice_check_bem.bemWSO.personid = AppModel.Instance.Person.id;
                _SelectedPosForNotice_check_bem.bemWSO.objektid = AppModel.Instance.selectedCheckA.objektid;
                _SelectedPosForNotice_check_bem.bemWSO.leistungid = _SelectedPosForNotice_check_bem.id;
                _SelectedPosForNotice_check_bem.bemWSO.datum = JavaScriptDateConverter.Convert(DateTime.Now);
                _SelectedPosForNotice_check_bem.bemWSO.text = !_SelectedPosForNotice_check_bem_isquest ? entry_notice_check_bem.Text.Trim() : "";
                _SelectedPosForNotice_check_bem.bemWSO.id = 0;

                if (_SelectedPosForNotice_check_bem_isquest)
                {
                    _SelectedPosForNotice_check_bem.Tap_a3_Pic_Refresh();
                }
                else
                {
                    _SelectedPosForNotice_check_bem.Tap_a_Bem_Refresh();
                }
                await Task.Delay(1);

                entry_notice_check_bem.Text = "";
                noticePhotoStack_check_bem.Children.Clear();


                CheckPage_Bem_Container.IsVisible = false;

                await Task.Delay(1);
                overlay.IsVisible = false;

            }
            else
            {
                _SelectedPosForNotice_check_bem.bemWSO = new BemerkungWSO();
                if (_SelectedPosForNotice_check_bem_isquest)
                {
                    _SelectedPosForNotice_check_bem.Tap_a3_Pic_Refresh();
                }
                else
                {
                    _SelectedPosForNotice_check_bem.Tap_a_Bem_Refresh();
                }

                CheckPage_Bem_Container.IsVisible = false;
            }
        }




        public async Task btn_takePhoto_check_bem(object sender, EventArgs e)
        {
            if (_SelectedPosForNotice_check_bem.bemWSO?.photos?.Count >= 3)
            {
                await DisplayAlertAsync("Limit erreicht", "Maximal 3 Fotos erlaubt", "OK");
                return;
            }


            overlay.IsVisible = true;
            await Task.Delay(1);

            AppModel.Instance.UseExternHardware = true;

            try
            {

                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    RotateImage = true,
                    SelectionLimit = 1,
                    PreserveMetaData = true,
                });

                if (photo != null)
                {
                    var photoResponse = await PhotoResize.CreatePhotoResponseAsync(photo);
                    var reCo = new Command<BildWSO>(RemoveBildInWork_check_bem);

                    long bildName = DateTime.Now.Ticks;
                    var bildWSO = new BildWSO(_SelectedPosForNotice_check_bem.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = bildName.ToString(),
                        stack = BildWSO.GetAttachmentForNoticeElement(
                            photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                            reCo)
                    };
                    var frame = (Border)((StackLayout)(bildWSO.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = reCo,
                        CommandParameter = bildWSO
                    });

                    //BildWSO.Save(AppModel.Instance, bildWSO);
                    //_selectedBemerkungForNotice.photos.Add(bildWSO);
                    //noticePhotoStack.Children.Add(bildWSO.stack);
                    //CheckNoticeFalid();

                    if (bildWSO != null)
                    {
                        // BildWSO.Save(AppModel.Instance, bild);
                        _SelectedPosForNotice_check_bem.bemWSO.photos.Add(bildWSO);
                        noticePhotoStack_check_bem.Children.Add(bildWSO.stack);

                        UpdatePhotoButtonsVisibility_check_bem();
                    }
                }
            }
            catch (PhotoPickerException photoEx)
            {
                AppModel.Logger.Error(
                    photoEx,
                    $"(btn_takePhoto_check_bem) Kamera-Fehler in Schritt '{photoEx.Stage}' ({photoEx.FailureKind}).");
                await DisplayAlertAsync("Fehler", photoEx.UserMessage, "OK");
            }
            catch (OperationCanceledException)
            {
                // Benutzer hat abgebrochen
            }
            catch (Exception ex)
            {
                // Andere Fehler
                AppModel.Logger.Error(ex, "(btn_takePhoto_check_bem) Unerwarteter Fehler beim Foto aufnehmen.");
                await DisplayAlertAsync("Fehler", "Foto konnte nicht aufgenommen werden.", "OK");
            }
            finally
            {
                AppModel.Instance.UseExternHardware = false;
                overlay.IsVisible = false;
            }
        }
        private void UpdatePhotoButtonsVisibility_check_bem()
        {
            var bemWSO = _SelectedPosForNotice_check_bem?.bemWSO;
            if (bemWSO?.photos != null)
            {
                bool hasSpace = bemWSO.photos.Count < 3;
                btn_takePhoto_frame_check_bem.IsVisible = hasSpace;
                btn_takePhotoAttachment_frame_check_bem.IsVisible = hasSpace;
            }
        }

        public async Task btn_pickPhotos_check_bem(object sender, EventArgs e)
        {
            int zp = 0;
            if (_SelectedPosForNotice_check_bem.bemWSO?.photos?.Count >= 3)
            {
                await DisplayAlertAsync("Limit erreicht", "Maximal 3 Fotos erlaubt", "OK");
                return;
            }
            if (_SelectedPosForNotice_check_bem.bemWSO?.photos != null)
            {
                zp = _SelectedPosForNotice_check_bem.bemWSO.photos.Count;
            }

            AppModel.Instance.UseExternHardware = true;

            try
            {
                if (!MediaPicker.IsCaptureSupported)
                {
                    await DisplayAlertAsync("Fehler", "Kamera nicht verfügbar", "OK");
                    return;
                }

                overlay.IsVisible = true;
                _ = Task.Delay(1);


                var options = new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    SelectionLimit = 3 - zp,
                    PreserveMetaData = true,
                    RotateImage = true
                };
#if !IOS
                options.RotateImage = true;
#endif

                var photos = await MediaPicker.PickPhotosAsync(options);


                if (photos != null && photos.Count() > 0)
                {
                    foreach (var photo in photos)
                    {
                        var reCo = new Command<BildWSO>(RemoveBildInWork_check_bem);

                        var photoResponse = await PhotoResize.CreatePhotoResponseAsync(photo);

                        long bildName = DateTime.Now.Ticks;
                        var bildWSO = new BildWSO(_SelectedPosForNotice_check_bem.guid)
                        {
                            bytes = photoResponse.imageBytes,
                            name = bildName.ToString(),
                            stack = BildWSO.GetAttachmentForNoticeElement(
                                photoResponse.GetImageSourceAsThumb(),
                                new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                                reCo)
                        };
                        var frame = (Border)((StackLayout)(bildWSO.stack.Children[0])).Children[2];
                        frame.GestureRecognizers.Clear();
                        frame.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = reCo,
                            CommandParameter = bildWSO
                        });


                        if (bildWSO != null)
                        {
                            // BildWSO.Save(AppModel.Instance, bildWSO);
                            _SelectedPosForNotice_check_bem.bemWSO.photos.Add(bildWSO);
                            noticePhotoStack_check_bem.Children.Add(bildWSO.stack);

                            // UI Update
                            if (_SelectedPosForNotice_check_bem.bemWSO?.photos != null)
                            {
                                bool hasSpace = _SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3;
                                btn_takePhoto_frame_check_bem.IsVisible = hasSpace;
                                btn_takePhotoAttachment_frame_check_bem.IsVisible = hasSpace;
                            }
                        }

                    }
                }
            }
            catch (FeatureNotSupportedException exn)
            {
                // Kamera wird nicht unterstützt
                AppModel.Logger.Error($"Fehler Kamera wird nicht unterstützt: {exn.Message}");
            }
            catch (PermissionException exp)
            {
                // Berechtigungen wurden nicht erteilt
                AppModel.Logger.Error("Keine Kamera-Berechtigung (1): " + exp.Message + " :: " + exp.StackTrace);
            }
            catch (OperationCanceledException)
            {
                // Benutzer hat abgebrochen
            }
            catch (Exception ex)
            {
                // Andere Fehler
                AppModel.Logger.Error($"Fehler beim Foto aufnehmen: {ex.Message}");
            }
            finally
            {

                AppModel.Instance.UseExternHardware = false;
                overlay.IsVisible = false;
            }
        }


        public async void RemoveBildInWork_check_bem(BildWSO b)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            noticePhotoStack_check_bem.Children.Remove(b.stack);
            await Task.Delay(1);
            BildWSO.Delete(AppModel.Instance, b);
            await Task.Delay(1);
            _SelectedPosForNotice_check_bem.bemWSO.photos.Remove(b);
            btn_takePhoto_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
            btn_takePhotoAttachment_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
            //CheckNoticeFalid_check_bem();

            await Task.Delay(1);
            overlay.IsVisible = false;
        }





        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Run(async () => await Ticket.LoadTicketsFromBackendAsync());
            // Reset camera state when returning to MainPage (e.g., after company change)
            // This ensures the camera is properly reinitialized
            if (ReaderView != null && ReaderView.IsDetecting)
            {
                try
                {
                    // Stop any active camera session
                    ReaderView.IsDetecting = false;
                    ReaderView.IsTorchOn = false;

#if ANDROID
                    // On Android, explicitly reset visibility and opacity
                    ReaderView.IsVisible = false;
                    ReaderView.Opacity = 0.0;

                    // Reset options to ensure clean state
                    //ReaderView.Options = null;
#endif

                }
                catch (Exception ex)
                {
                    AppModel.Logger.Error($"[MainPage] OnAppearing - Error resetting ReaderView: {ex.Message}", ex);
                }
            }
        }

        protected override void OnDisappearing()
        {
            // Stop camera when leaving MainPage
            if (ReaderView != null)
            {
                System.Diagnostics.Debug.WriteLine("[MainPage] OnDisappearing - Stopping ReaderView");
                try
                {
                    ReaderView.IsDetecting = false;
                    ReaderView.IsTorchOn = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainPage] OnDisappearing - Error stopping ReaderView: {ex.Message}");
                }
            }

            if (AppModel.Instance._cts != null && !AppModel.Instance._cts.IsCancellationRequested)
                AppModel.Instance._cts.Cancel();
            base.OnDisappearing();
        }

        private async Task<bool> CheckLocationPermissionsAndInitGps()
        {
            var checkPermissionGPSMessage = await AppModel.Instance.CheckPermissionGPS();
            if (!String.IsNullOrWhiteSpace(checkPermissionGPSMessage))
            {
                checkPermissionGPSMessage = checkPermissionGPSMessage.Replace(";", "\n\n");
                await DisplayAlertAsync("Berechtigungsproblem!", checkPermissionGPSMessage, "OK");
                //AppModel.Instance.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                return false;
            }

            return await AppModel.Instance.InitGPSTimer();
        }

        /*********/
        /* PAGES */
        /*********/

        // Back To LoginPage
        private async void BackToLoginPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            //AppModel.Instance.SendLogZipFile();

            AppModel.Instance.State.IsBackTappedToLogin = true;

            ClearPageViews();
            ClearPlanDataView();
            StartPage_Container.IsVisible = true;


            AppModel.Instance.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
            return;
        }

        public async void ShowMainPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            StartPage_Container.IsVisible = true;

            SetLastBuilding();
            await Task.Delay(1);


            // Selektierte Arbeiten zur Ausführung (noch nicht gestartete Arbeiten)
            btn_showselected_pos_container.IsVisible = AppModel.Instance.allSelectedPositionToWork.Count > 0;
            btn_showselected_pos_container_not.IsVisible = !(AppModel.Instance.allSelectedPositionToWork.Count > 0);
            await Task.Delay(1);
            btn_showselected_pos_container2.IsVisible = AppModel.Instance.allSelectedPositionToWork.Count > 0;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;


            var dt = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ?
                DateTime.Now.AddDays(-2) : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
            box_buildingInformation.Children.Clear();
            box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(AppModel.Instance, dt));

            await Task.Delay(1);
            SetChecksCount();
        }


        //private async void ShowPN_Page()
        //{
        //    isInitialize = true;
        //    overlay.IsVisible = true;
        //    await Task.Delay(1);

        //    ClearPageViews();
        //    PN_Page_Container.IsVisible = true;


        //    await Task.Delay(1);
        //    overlay.IsVisible = false;
        //    isInitialize = false;
        //}
        public void btn_PN_BackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }

        private async void ShowDSGVOPage()
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            MainMenuTapped_Done(false);
            await DSGVOPageContainerView.ShowAsync(this);
            overlay.IsVisible = false;
        }


        private async void ShowWorkerPage()
        {
            if (_isOpeningWorkerModal) { return; }
            _isOpeningWorkerModal = true;
            isInitialize = true;
            overlay.IsVisible = true;
            try
            {
                await Task.Delay(1);

                if (!Navigation.ModalStack.Contains(WorkerPageContainerView))
                {
                    await Navigation.PushModalAsync(WorkerPageContainerView, animated: false);
                }
                WorkerPage_Container.IsVisible = true;

                // Handwerker nach Kategorien start anzeigen
                if (workerSelectedViewIndex == 0)
                {
                    // Wenn noch nicht aufgerufen, dann Initialisieren
                    //btn_WorkerCategorySearchTapped(null, null);
                    btn_WorkerBuildingSearchTapped(null, null);
                }

                await Task.Delay(1);
                overlay.IsVisible = false;
                isInitialize = false;
            }
            finally
            {
                _isOpeningWorkerModal = false;
            }
        }

        private async void ShowNachbuchenPage(int pos)
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            NachbuchenPage_Container.IsVisible = true;
            btn_nachbuchen_Tapped(pos);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }




        //private async void ShowBuildingScanPage(bool isCheck)
        //{
        //    try
        //    {
        //        var result = await ScanObjModalPage.ScanAsync(this);
        //        if (!string.IsNullOrWhiteSpace(result))
        //        {
        //            var sp = result.Replace("http://www.ipm-cloud.de/?objektid=", "")
        //                           .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

        //            if (sp != null && sp.Length > 0)
        //            {
        //                var CustomerNumber = sp.Length == 1 ? "1" : "" + sp[1];
        //                Int32 buildingid = Int32.Parse(sp[0]);

        //                if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
        //                {
        //                    AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;

        //                    if (buildingid > 0 && AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
        //                    {
        //                        AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
        //                        AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;
        //                        AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);

        //                        AppModel.Logger.Info("CHECK-IN: " + AppModel.Instance.LastBuilding.strasse + " " +
        //                                                 AppModel.Instance.LastBuilding.hsnr + " " +
        //                                                 AppModel.Instance.LastBuilding.plz + " " +
        //                                                 AppModel.Instance.LastBuilding.ort);

        //                    }

        //                    AppModel.Instance.SettingModel.SaveSettings();

        //                    AppModel.Instance.UseExternHardware = false;
        //                    if (isCheck)
        //                    {
        //                        MethodAfterScan_check();
        //                    }
        //                    else
        //                    {
        //                        ShowMainPage();
        //                    }
        //                }
        //                else
        //                {
        //                    await DisplayAlertAsync("QR-Code nicht erkannt!",
        //                        "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
        //                        "OK");
        //                    AppModel.Instance.UseExternHardware = false;
        //                }
        //            }
        //            else
        //            {
        //                await DisplayAlertAsync("QR-Code nicht erkannt!",
        //                    "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
        //                    "OK");
        //                AppModel.Instance.UseExternHardware = false;
        //            }

        //        }
        //        else
        //        {
        //            AppModel.Instance.UseExternHardware = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
        //        AppModel.Instance.UseExternHardware = false;
        //    }
        //}



        private async void ShowBuildingNotScanPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            NotScanPage_Container.IsVisible = true;
            btn_notscan_allTapped(null, null);
            entry_notscansearch.Focus();

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void btn_notscan_allTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_notscan.IsVisible = false;
            //todoRangeSlider_container.IsVisible = true;
            entry_notscansearch.Text = "";
            entry_notscansearch_container.IsVisible = true;
            await Task.Delay(1);
            list_notscan.Children.Clear();
            await list_notscan_scroll.ScrollToAsync(0, 0, false);
            //_holdLastTodoList = 1;
        }
        public async void BuildNotScanList(string s)
        {
            list_notscan.Children.Clear();
            await list_notscan_scroll.ScrollToAsync(0, 0, false);
            await Task.Delay(1);
            list_notscan.Children.Add(BuildingWSO.GetObjektNotScanListView(AppModel.Instance, new Command<IntBoolParam>(SelectedObjektAufterNotScan), s));
            await Task.Delay(1);
            list_notscan.IsVisible = true;
            overlay.IsVisible = false;
        }
        private async void Entry_notscansearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            list_notscan.IsVisible = true;
            await Task.Delay(1);
            BuildNotScanList(e.NewTextValue.ToLower());
        }
        public async void SelectedObjektAufterNotScan(IntBoolParam intBol)
        {
            if (AppModel.Instance.allPositionInWork != null)
            {
                popupContainer_info_notscan_titel.Text = "ACHTUNG!";
                popupContainer_info_notscan_text.Text = "Es sind noch nicht abgeschlossene Arbeiten aktiv. Bitte erst beenden, bevor Sie ein anderes Objekt direkt auswählen.";
                popupContainer_info_notscan_okbtn.GestureRecognizers.Clear();
                var tgr_over_ns = new TapGestureRecognizer();
                tgr_over_ns.Tapped += (object o, TappedEventArgs ev) => { popupContainer_info_notscan.IsVisible = false; };
                tgr_over_ns.Tapped += (object o, TappedEventArgs ev) => { popupContainer_info_notscan.IsVisible = false; };
                popupContainer_info_notscan_okbtn.GestureRecognizers.Add(tgr_over_ns);
                btn_endselectedcancel.GestureRecognizers.Clear();
                popupContainer_info_notscan.IsVisible = true;
            }
            else
            {
                overlay.IsVisible = true;
                await Task.Delay(1);
                AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
                AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == intBol.val);
                try
                {
                    AppModel.Logger.Info("CHECK-IN (OHNE QR-SCAN): " + AppModel.Instance.LastBuilding.strasse + " " + AppModel.Instance.LastBuilding.hsnr + AppModel.Instance.LastBuilding.plz + " " + AppModel.Instance.LastBuilding.ort);
                }
                catch (Exception) { }
                AppModel.Instance.SettingModel.SaveSettings();
                list_notscan.Children.Clear();
                if (intBol.bol) { ShowMainPage(); }
                else
                {
                    await lastBuilding_Container.FadeToAsync(0, 500, Easing.SpringOut);
                    SetLastBuilding();
                    await lastBuilding_Container.FadeToAsync(1, 500, Easing.SpringIn);
                    overlay.IsVisible = false;
                    await Task.Delay(1);
                }
            }
        }


        private void ShowBuildingScanPage(bool isCheck)
        {
            ShowBuildingScanPageALL(isCheck);

        }

        //private async void ShowBuildingScanPageAndroid(bool isCheck)
        //{
        //    try
        //    {
        //        overlay.IsVisible = true;
        //        await Task.Delay(1);

        //        var result = await ScanObjModalPage.ScanAsync(this);
        //        if (string.IsNullOrWhiteSpace(result))
        //        {
        //            await Task.Delay(1);
        //            overlay.IsVisible = false;
        //            AppModel.Instance.UseExternHardware = false;
        //            //await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
        //            return;
        //        }
        //        const string marker = "objektid=";
        //        var markerIndex = result?.IndexOf(marker) ?? -1;

        //        if (markerIndex >= 0)
        //        {
        //            var sp = result.Substring(markerIndex + marker.Length)
        //                           .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

        //            if (sp != null && sp.Length > 0 && Int32.TryParse(sp[0], out Int32 buildingid))
        //            {
        //                var CustomerNumber = sp.Length == 1 ? "1" : "" + sp[1];
        //                if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
        //                {
        //                    AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;

        //                    if (buildingid > 0 && AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
        //                    {
        //                        AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
        //                        AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;
        //                        AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);

        //                        AppModel.Logger.Info("CHECK-IN: " + AppModel.Instance.LastBuilding.strasse + " " +
        //                                                 AppModel.Instance.LastBuilding.hsnr + " " +
        //                                                 AppModel.Instance.LastBuilding.plz + " " +
        //                                                 AppModel.Instance.LastBuilding.ort);

        //                    }

        //                    AppModel.Instance.SettingModel.SaveSettings();

        //                    AppModel.Instance.UseExternHardware = false;
        //                    await Task.Delay(1);
        //                    overlay.IsVisible = false;
        //                    if (isCheck)
        //                    {
        //                        MethodAfterScan_check();
        //                    }
        //                    else
        //                    {
        //                        ShowMainPage();
        //                    }
        //                }
        //                else
        //                {
        //                    await Task.Delay(1);
        //                    overlay.IsVisible = false;
        //                    await DisplayAlertAsync("QR-Code nicht erkannt!",
        //                        "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
        //                        "OK");
        //                    AppModel.Instance.UseExternHardware = false;
        //                }
        //            }
        //            else
        //            {
        //                await Task.Delay(1);
        //                overlay.IsVisible = false;
        //                await DisplayAlertAsync("QR-Code nicht erkannt!",
        //                    "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
        //                    "OK");
        //                AppModel.Instance.UseExternHardware = false;
        //            }

        //        }
        //        else
        //        {
        //            await Task.Delay(1);
        //            overlay.IsVisible = false;
        //            await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
        //            AppModel.Instance.UseExternHardware = false;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        await Task.Delay(1);
        //        overlay.IsVisible = false;
        //        await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
        //        AppModel.Instance.UseExternHardware = false;
        //    }
        //}

        private bool __isCheck = false;
        private bool __isOutScan = false;
        private int __scanHandled = 0; // 0 = idle, 1 = processing; use Interlocked for thread-safe access

        // EXPERIMENTELL: Setzen Sie dies auf true, wenn Samsung-Geräte immer noch Probleme haben
        // Dies deaktiviert alle Delays und verwendet minimale Einstellungen
        private const bool USE_ULTRA_FAST_MODE = true;

        private async void ShowBuildingScanPageALL(bool isCheck)
        {
            try
            {

                __isCheck = isCheck;
                __isOutScan = false;
                Interlocked.Exchange(ref __scanHandled, 0);

                var hasCameraPermission = await PermissionHelper.EnsureCameraPermissionAsync(
                    "ScanObjModalPage.ScanAsync",
                    async () => await DisplayAlertAsync(
                        "Kamerazugriff verweigert",
                        "Bitte erlauben Sie den Kamerazugriff in den Einstellungen.",
                        "OK"));
                if (!hasCameraPermission)
                {
                    AppModel.Logger.Error("ShowBuildingScanPageIOS: Kamerazugriff verweigert");
                    return;
                }

                overlay.IsVisible = true;
                await Task.Delay(1);

                view_ScanObjModalPage.IsEnabled = true;

                btn_back_inAddRegScan.GestureRecognizers.Clear();
                var tgr7 = new TapGestureRecognizer();
                tgr7.Tapped -= OnCancelScanObjClicked;
                tgr7.Tapped += OnCancelScanObjClicked;
                btn_back_inAddRegScan.GestureRecognizers.Add(tgr7);
                btn_back_inAddRegScan.InputTransparent = false;

                view_ScanObjModalPage.IsVisible = true;

#if ANDROID
                // Android-spezifische Initialisierung für Kamera

                // Reset to clean state first (wichtig nach Firmenwechsel!)
                try
                {
                    ReaderView.IsDetecting = false;
                    ReaderView.IsTorchOn = false;
                    ReaderView.IsVisible = false;
                    ReaderView.Opacity = 0.0;
                    ReaderView.Options = null;
                    if (!USE_ULTRA_FAST_MODE)
                    {
                        await Task.Delay(100); // Kürzeres Cleanup-Delay
                    }
                    // Ultra-Fast-Mode: kein Delay
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Warn($"[MainPage] Reset error (non-critical): {ex.Message}");
                }


                // Warten auf View-Layout - minimal  
                if (!USE_ULTRA_FAST_MODE)
                {
                    await Task.Delay(50);
                }
                // Ultra-Fast-Mode: kein Delay

                // Sichtbarkeit und Opazität sicherstellen
                ReaderView.IsVisible = true;
                ReaderView.Opacity = 1.0;


                // Layout-Update erzwingen
                ReaderView.InvalidateMeasure();

                // Kamera-Optionen setzen - Aggressiv optimiert für schnelles Scannen auf Samsung
                // Basierend auf funktionierende Xamarin-Einstellungen
                ReaderView.Options = new ZXing.Net.Maui.BarcodeReaderOptions
                {
                    Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
                    AutoRotate = true,
                    Multiple = false,
                    TryHarder = false, // False für schnelleres Scannen
                    TryInverted = true, // Hilft bei invertierten QR-Codes
                    DelayBetweenAnalyzingFrames = USE_ULTRA_FAST_MODE ? 30 : 50,
                    InitialDelayBeforeAnalyzingFrames = USE_ULTRA_FAST_MODE ? 0 : 100,
                    DelayBetweenContinuousScans = USE_ULTRA_FAST_MODE ? 0 : 100,
                    CharacterSet = "UTF-8",
                    CameraResolutionSelector = availableResolutions =>
                    {
                        var resolutions = availableResolutions.ToList();

                        // Log verfügbare Auflösungen für Debugging
                        AppModel.Logger.Info($"[MainPage] Available resolutions: {string.Join(", ", resolutions.Select(r => $"{r.Width}x{r.Height}"))}");

                        if (USE_ULTRA_FAST_MODE)
                        {
                            // Ultra-Fast-Mode: Niedrigste sinnvolle Auflösung für maximale Speed
                            var selected = resolutions
                                .Where(r => r.Width >= 640 && r.Height >= 480) // Minimum VGA
                                .OrderBy(r => r.Width * r.Height) // Niedrigste zuerst
                                .FirstOrDefault() ?? resolutions.First();

                            AppModel.Logger.Info($"[MainPage] ULTRA FAST MODE - Selected resolution: {selected.Width}x{selected.Height}");
                            return selected;
                        }
                        else
                        {
                            // Standard: Mittlere Auflösung (720p Bereich)
                            var selected = resolutions
                                .OrderBy(r => Math.Abs((r.Width * r.Height) - (1280 * 720)))
                                .ThenByDescending(r => r.Width * r.Height)
                                .First();

                            AppModel.Logger.Info($"[MainPage] Selected resolution: {selected.Width}x{selected.Height}");
                            return selected;
                        }
                    }
                };

                // Kürzeres Delay für schnellere Kamera-Initialisierung
                // Samsung braucht etwas Zeit, aber nicht zu viel
                if (!USE_ULTRA_FAST_MODE)
                {
                    await Task.Delay(150);
                }
                // Ultra-Fast-Mode: kein Delay

#endif

                ReaderView.IsTorchOn = false;
                ReaderView.IsDetecting = true;

#if ANDROID
                System.Diagnostics.Debug.WriteLine("[MainPage] IsDetecting = true - Camera should now be active");

                // Minimales Check-Delay für Samsung-Geräte
                await Task.Delay(50);
                System.Diagnostics.Debug.WriteLine($"[MainPage] 50ms after IsDetecting=true - IsDetecting={ReaderView.IsDetecting}");
#endif

                await Task.Delay(1);
                overlay.IsVisible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPage] ERROR in ShowBuildingScanPageALL: {ex.Message}");
                AppModel.Logger?.Error($"ShowBuildingScanPageALL error: {ex}");
                OnCancelScanObjClicked(null, null);
                await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
            }
        }
        private async void ReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            // Atomically claim the event; discard duplicates / rapid repeated fires.
            if (Interlocked.CompareExchange(ref __scanHandled, 1, 0) != 0) return;

            var result = e.Results?.FirstOrDefault()?.Value;

            // BarcodesDetected may fire on a background thread – marshal all UI work to the main thread.
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (overlay == null)
                    {
                        AppModel.Logger?.Error("ERROR: overlay is null in ReaderView_BarcodesDetected");
                        return;
                    }

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    if (string.IsNullOrWhiteSpace(result))
                    {
                        OnCancelScanObjClicked(null, null);
                        return;
                    }

                    const string marker = "objektid=";
                    var markerIndex = result?.IndexOf(marker) ?? -1;

                    if (markerIndex >= 0)
                    {
                        var sp = result.Substring(markerIndex + marker.Length)
                                       .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

                        if (sp != null && sp.Length > 0 && Int32.TryParse(sp[0], out Int32 buildingid))
                        {
                            var CustomerNumber = sp.Length == 1 ? "1" : "" + sp[1];

                            // Null-Check für SettingModel und SettingDTO
                            if (AppModel.Instance?.SettingModel?.SettingDTO == null)
                            {
                                AppModel.Logger?.Error("ERROR: SettingModel or SettingDTO is null in ReaderView_BarcodesDetected");
                                OnCancelScanObjClicked(null, null);
                                await DisplayAlertAsync("Fehler!", "Einstellungen konnten nicht geladen werden.", "OK");
                                return;
                            }

                            if (!__isOutScan)
                            {
                                if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                {
                                    AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;

                                    if (buildingid > 0 && AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
                                    {
                                        AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
                                        AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;
                                        AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);

                                        if (AppModel.Instance.LastBuilding != null)
                                        {
                                            AppModel.Logger.Info("CHECK-IN: " + AppModel.Instance.LastBuilding.strasse + " " +
                                                                 AppModel.Instance.LastBuilding.hsnr + " " +
                                                                 AppModel.Instance.LastBuilding.plz + " " +
                                                                 AppModel.Instance.LastBuilding.ort);
                                        }
                                        else
                                        {
                                            AppModel.Logger.Warn("WARN: CHECK-IN Objekt mit ID " + buildingid + " nicht gefunden in AllBuildings (List).");
                                        }

                                    }

                                    AppModel.Instance.SettingModel.SaveSettings();

                                    OnCancelScanObjClicked(null, null);

                                    if (__isCheck)
                                    {
                                        MethodAfterScan_check();
                                    }
                                    else
                                    {
                                        ShowMainPage();
                                    }
                                }
                                else
                                {

                                    OnCancelScanObjClicked(null, null);
                                    await DisplayAlertAsync("QR-Code nicht erkannt!",
                                        "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
                                        "OK");
                                }
                            }
                            else
                            {
                                AppModel.Instance.OutScanBuilding = null;
                                if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                {
                                    if (AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
                                    {
                                        AppModel.Instance.OutScanBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);
                                        if (AppModel.Instance.OutScanBuilding != null)
                                        {
                                            AppModel.Logger.Info("CHECK-OUT: " + AppModel.Instance.OutScanBuilding.strasse + " " +
                                                                 AppModel.Instance.OutScanBuilding.hsnr + " " +
                                                                 AppModel.Instance.OutScanBuilding.plz + " " +
                                                                 AppModel.Instance.OutScanBuilding.ort);
                                        }
                                        else
                                        {
                                            AppModel.Logger.Warn("WARN: CHECK-OUT Objekt mit ID " + buildingid + " nicht gefunden in AllBuildings (List).");
                                        }
                                    }

                                    OnCancelScanObjClicked(null, null);
                                    MethodAfterOutScan();
                                }
                                else
                                {
                                    OnCancelScanObjClicked(null, null);
                                    await DisplayAlertAsync("QR-Code nicht erkannt!",
                                        "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
                                        "OK");
                                }
                            }


                        }
                        else
                        {
                            OnCancelScanObjClicked(null, null);
                            await DisplayAlertAsync("QR-Code nicht erkannt!",
                                "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
                                "OK");
                        }

                    }
                    else
                    {
                        OnCancelScanObjClicked(null, null);
                        await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    AppModel.Logger?.Error($"ERROR in ReaderView_BarcodesDetected: {ex.Message}\n{ex.StackTrace}");
                    OnCancelScanObjClicked(null, null);
                    await DisplayAlertAsync("Fehler!", "Ein unerwarteter Fehler ist aufgetreten.", "OK");
                }
            });
        }

        private async void OnCancelScanObjClicked(object sender, TappedEventArgs e)
        {

            await Task.Delay(1);
            __isCheck = false;
            __isOutScan = false;
            overlay.IsVisible = false;

            try
            {
                ReaderView.IsTorchOn = false;
                ReaderView.IsDetecting = false;

#if ANDROID
                // On Android, reset visibility and options for clean restart
                await Task.Delay(100);
                ReaderView.IsVisible = false;
                ReaderView.Opacity = 0.0;
                ReaderView.Options = null;
#endif
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"OnCancelScanObjClicked camera error: {ex.Message}");
            }

            view_ScanObjModalPage.IsEnabled = false;
            view_ScanObjModalPage.IsVisible = false;
            await Task.Delay(100);
            AppModel.Instance.UseExternHardware = false;
        }


        private void FlashCameraClicked(object sender, EventArgs e)
        {
            ReaderView.IsTorchOn = !ReaderView.IsTorchOn;
        }

        private void ShowBuildingOutScanPage()
        {
#if ANDROID
            ShowBuildingOutScanPageAndroid();
#else
            ShowBuildingOutScanPageIOS();
#endif
        }

        private async void ShowBuildingOutScanPageAndroid()
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("[MainPage:OutScanAndroid] Starting...");

                __isOutScan = true;
                Interlocked.Exchange(ref __scanHandled, 0);

                overlay.IsVisible = true;
                await Task.Delay(1);

                view_ScanObjModalPage.IsEnabled = true;

                btn_back_inAddRegScan.GestureRecognizers.Clear();
                var tgr7 = new TapGestureRecognizer();
                tgr7.Tapped -= OnCancelScanObjClicked;
                tgr7.Tapped += OnCancelScanObjClicked;
                btn_back_inAddRegScan.GestureRecognizers.Add(tgr7);
                btn_back_inAddRegScan.InputTransparent = false;

                view_ScanObjModalPage.IsVisible = true;

                // Android-spezifische Initialisierung für Kamera (gleich wie in ShowBuildingScanPageALL)
                //System.Diagnostics.Debug.WriteLine("[MainPage:OutScanAndroid] Resetting ReaderView to clean state...");

                // Reset to clean state first (wichtig nach Firmenwechsel!)
                try
                {
                    ReaderView.IsDetecting = false;
                    ReaderView.IsTorchOn = false;
                    ReaderView.IsVisible = false;
                    ReaderView.Opacity = 0.0;
                    ReaderView.Options = null;
                    await Task.Delay(150);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainPage:OutScanAndroid] Reset error (non-critical): {ex.Message}");
                }

                //System.Diagnostics.Debug.WriteLine("[MainPage:OutScanAndroid] Starting camera initialization...");
                await Task.Delay(100);
                ReaderView.IsVisible = true;
                ReaderView.Opacity = 1.0;
                ReaderView.InvalidateMeasure();

                ReaderView.Options = new ZXing.Net.Maui.BarcodeReaderOptions
                {
                    Formats = ZXing.Net.Maui.BarcodeFormats.TwoDimensional,
                    AutoRotate = true,
                    Multiple = false,
                    DelayBetweenAnalyzingFrames = 30,
                    InitialDelayBeforeAnalyzingFrames = 0,
                    DelayBetweenContinuousScans = 0,
                    CharacterSet = "ISO-8859-1",
                    CameraResolutionSelector = availableResolutions =>
                    {
                        var resolutions = availableResolutions.ToList();
                        var selected = resolutions
                            .OrderBy(r => Math.Abs((r.Width * r.Height) - (1280 * 720)))
                            .ThenBy(r => Math.Abs(r.Width - 1280) + Math.Abs(r.Height - 720))
                            .First();
                        //System.Diagnostics.Debug.WriteLine($"[MainPage:OutScanAndroid] Selected resolution: {selected.Width}x{selected.Height}");
                        return selected;
                    }
                };

                await Task.Delay(700);
                //System.Diagnostics.Debug.WriteLine("[MainPage:OutScanAndroid] Enabling camera detection...");

                ReaderView.IsDetecting = true;


                await Task.Delay(500);
                //System.Diagnostics.Debug.WriteLine($"[MainPage:OutScanAndroid] Camera active - IsDetecting={ReaderView.IsDetecting}");


                await Task.Delay(1);
                overlay.IsVisible = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"ShowBuildingOutScanPageAndroid error: {ex}");
                OnCancelScanObjClicked(null, null);
                await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
            }

        }

        private async void ShowBuildingOutScanPageIOS()
        {
            try
            {
                __isOutScan = true;
                Interlocked.Exchange(ref __scanHandled, 0);

                overlay.IsVisible = true;
                await Task.Delay(1);

                view_ScanObjModalPage.IsEnabled = true;

                btn_back_inAddRegScan.GestureRecognizers.Clear();
                var tgr7 = new TapGestureRecognizer();
                tgr7.Tapped -= OnCancelScanObjClicked;
                tgr7.Tapped += OnCancelScanObjClicked;
                btn_back_inAddRegScan.GestureRecognizers.Add(tgr7);
                btn_back_inAddRegScan.InputTransparent = false;

                view_ScanObjModalPage.IsVisible = true;

                ReaderView.IsDetecting = true;


                await Task.Delay(1);
                overlay.IsVisible = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"[MainPage:OutScan] ERROR: {ex.Message}", ex);
                OnCancelScanObjClicked(null, null);
                await DisplayAlertAsync("Fehler beim Scannen!", "QR-Code konnte nicht gelesen werden.", "OK");
            }

        }

        public bool MethodAfterOutScan()
        {
            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                try
                {
                    string b = "";
                    if (AppModel.Instance.LastBuilding != null && !String.IsNullOrWhiteSpace(AppModel.Instance.LastBuilding.hsnr))
                    {
                        b = ": " + AppModel.Instance.LastBuilding.strasse + " " + AppModel.Instance.LastBuilding.hsnr + AppModel.Instance.LastBuilding.plz + " " + AppModel.Instance.LastBuilding.ort;
                    }
                    AppModel.Logger.Info("CHECK-OUT (OHNE QR-SCAN) " + b);
                }
                catch (Exception) { }
                SavesRunningWorksOver(false);
                ShowMainPage();
                return true;
            }
            else
            {
                AppModel.Instance.UseExternHardware = false;
                //lay_buildingoutscan.Children.Clear();
                if (AppModel.Instance.OutScanBuilding != null)
                {
                    if (AppModel.Instance.OutScanBuilding.id == AppModel.Instance.LastBuilding.id)
                    {
                        SavesRunningWorksOver(false);
                        ShowMainPage();
                        return true;
                    }
                    else
                    {
                        popupContainer_quest_overtootherBuilding.IsVisible = true;
                        SavesRunningWorksOver(true);
                    }
                }
            }
            return false;
        }
        private async void AuftraegeAuswaehlenView()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            BuildingOrderPage_Container.IsVisible = true;

            ShowOrderPage();

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        private async void ShowOrderPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BuildingOrderPage_category_Container.IsVisible = false;
            BuildingOrderPage_position_Container.IsVisible = false;

            buildingorderlist_category_container.Children.Clear();

            buildingorderlist_order_container.Children.Clear();
            buildingorderlist_order_container.Children.Add(AuftragWSO.GetOrderListView(AppModel.Instance, new Command<AuftragWSO>(SelectOrder)));
            BuildingOrderPage_order_Container.IsVisible = true;

            AppModel.Instance.LastSelectedOrder = null;
            AppModel.Instance.LastSelectedCategory = null;
            AppModel.Instance.LastSelectedPosition = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void SelectOrder(AuftragWSO order)
        {
            AppModel.Instance.LastSelectedOrder = order;
            lb_inBuildingOrder_category_text.Text = "" + order.GetMobileText();// + " \nNr.: " + order.id + "  Typ: " + order.typ;
            ShowOrderCategoryPage(order);
        }

        private async void ShowOrderCategoryPage(AuftragWSO order)
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BuildingOrderPage_order_Container.IsVisible = false;
            BuildingOrderPage_position_Container.IsVisible = false;

            buildingorderlist_category_container.Children.Clear();
            buildingorderlist_category_container.Children.Add(KategorieWSO.GetCategoryListView(AppModel.Instance, new Command<KategorieWSO>(SelectCategory)));
            BuildingOrderPage_category_Container.IsVisible = true;

            AppModel.Instance.LastSelectedCategory = null;
            AppModel.Instance.LastSelectedPosition = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }

        public void btn_showall_OrderCategoryTapped(object sender, EventArgs e)
        {
            AppModel.Instance._showall_OrderCategory = !AppModel.Instance._showall_OrderCategory;
            btn_back_inBuildingOrder_category_showall_txt.Text = AppModel.Instance._showall_OrderCategory ? "Meine zeigen" : "Alle zeigen";

            buildingorderlist_category_container.Children.Clear();
            buildingorderlist_category_container.Children.Add(KategorieWSO.GetCategoryListView(AppModel.Instance, new Command<KategorieWSO>(SelectCategory)));
            BuildingOrderPage_category_Container.IsVisible = true;
        }

        public async void SelectCategory(KategorieWSO category)
        {
            AppModel.Instance.LastSelectedCategory = category;
            lb_inBuildingOrder_categorypos_text.Text = AppModel.Instance.LastSelectedOrder.GetMobileText(); // + " \nNr.: " + AppModel.Instance.LastSelectedOrder.id + "  Typ: " + AppModel.Instance.LastSelectedOrder.typ;
            lb_inBuildingOrder_position_text.Text = category.GetMobileText();
            ShowOrderPositionPage();
        }
        private async void ShowOrderPositionPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BuildingOrderPage_order_Container.IsVisible = false;
            BuildingOrderPage_category_Container.IsVisible = false;

            buildingorderlist_position_container.Children.Clear();
            buildingorderlist_position_container.Children.Add(LeistungWSO.GetPositionListView(AppModel.Instance, new Command<LeistungWSO>(SelectPositionToWork)));
            BuildingOrderPage_position_Container.IsVisible = true;

            AppModel.Instance.LastSelectedPosition = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void SelectPositionToWork(LeistungWSO position)
        {
            bool inWork = false;
            if (AppModel.Instance.allPositionInWork != null)
            {
                var foundInWork = AppModel.Instance.allPositionInWork.leistungen.Find(l => l.id == position.id);
                inWork = foundInWork != null;
            }
            if (position.disabled || inWork) { return; }

            overlay.IsVisible = true;
            await Task.Delay(100);

            AppModel.Instance.LastSelectedPosition = position;
            Border framePos = null;
            var selPost = AppModel.Instance.allSelectedPositionToWork.Find(p => p.id == position.id);
            if (selPost != null)
            {
                // entfernen da schon selectiert 
                AppModel.Instance.allSelectedPositionToWork.Remove(position);
                if (AppModel.Instance.allPositionInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = false;
                    framePos.Content = LeistungWSO.GetPositionCardView(position, AppModel.Instance, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            else
            {
                // hinzufügen
                AppModel.Instance.allSelectedPositionToWork.Add(position);
                if (AppModel.Instance.allPositionInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = true;
                    framePos.Content = LeistungWSO.GetSelectedPositionCardView(position, AppModel.Instance, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            btn_showselected_pos_container.IsVisible = AppModel.Instance.allSelectedPositionToWork.Count > 0;
            btn_showselected_pos_container_not.IsVisible = !(AppModel.Instance.allSelectedPositionToWork.Count > 0);
            //btn_showselected_pos_container2.IsVisible = AppModel.Instance.allSelectedPositionToWork.Count > 0;
            CheckForOptionalToWork();

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void RemoveSelectPositionFromToWork(LeistungWSO position)
        {
            Border framePos;
            SwipeView swipePos;
            // entfernen da schon selectiert 
            AppModel.Instance.allSelectedPositionToWork.Remove(position);
            if (AppModel.Instance.allPositionInShowingListView.TryGetValue(position.id, out framePos))
            {
                framePos.Content = LeistungWSO.GetPositionCardView(position, AppModel.Instance, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
            }
            if (AppModel.Instance.allPositionInShowingSmallListView.TryGetValue(position.id, out swipePos))
            {
                swipePos.IsVisible = false;
            }

            btn_showselected_pos_container.IsVisible = AppModel.Instance.allSelectedPositionToWork.Count > 0;
            btn_showselected_pos_container_not.IsVisible = !(AppModel.Instance.allSelectedPositionToWork.Count > 0);
            btn_showselected_pos_container2.IsVisible = AppModel.Instance.allSelectedPositionToWork.Count > 0;
            CheckForOptionalToWork();

            position.selected = false;
            if (AppModel.Instance.allSelectedPositionToWork.Count == 0)
            {
                //await Task.Delay(100);
                AuswahlAnzeigenTapped_Done(false);
                //await Task.Delay(100);
                if (BuildingOrderPage_order_Container.IsVisible)
                {
                    buildingorderlist_order_container.Children.Clear();
                    buildingorderlist_order_container.Children.Add(AuftragWSO.GetOrderListView(AppModel.Instance, new Command<AuftragWSO>(SelectOrder)));
                }
            }
        }
        public async void CheckForOptionalToWork()
        {
            //return;
            AppModel.Instance.IsOptionalToWork = false;

            if (AppModel.Instance.allSelectedPositionToWork.Count == 0)
            {
                // alles zurücksetzen / in alle Aufträg / alle Kategorien / alle Leistungen
                AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(l =>
                        {
                            l.disabled = false;
                            //if (l.selected)
                            //{
                            Border framePos;
                            if (AppModel.Instance.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                            {
                                var func = ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command;
                                bool inWork = false;
                                if (AppModel.Instance.allPositionInWork != null)
                                {
                                    var foundInWork = AppModel.Instance.allPositionInWork.leistungen.Find(le => le.id == l.id);
                                    inWork = foundInWork != null;
                                }
                                framePos.Content = inWork ? LeistungWSO.GetInWorkPositionCardView(l, AppModel.Instance, func).Content
                                : LeistungWSO.GetPositionCardView(l, AppModel.Instance, func).Content;
                            }
                            //}
                        });
                    });
                });
            }
            else if (AppModel.Instance.allSelectedPositionToWork.Count > 0)
            {
                var foundProduktPos = AppModel.Instance.allSelectedPositionToWork.Find(i => (i.art == "Produkt"));
                var foundOPPos = AppModel.Instance.allSelectedPositionToWork.Find(i => (i.art == "Leistung" && i.nichtpauschal == 1));
                var foundRegPos = AppModel.Instance.allSelectedPositionToWork.Find(i => (i.art == "Leistung" && i.nichtpauschal == 0));
                // erste Einträge prüfen IsOptional
                if (foundOPPos != null)
                {
                    // OP Leistung gefunden dann keine Regulären leistungen zulassen
                    AppModel.Instance.IsOptionalToWork = true;
                    lb_PosSelectionType_text.Text = "Nur optionale Positionen und Produkte aktiv!";
                    lb_PosSelectionType_text2.Text = "Nur optionale Positionen und Produkte aktiv!";
                }
                else if (foundRegPos != null)
                {
                    // Reguläre Leistung gefunden dann keine OP's Leistunge/Produkte/etc. zulassen
                    AppModel.Instance.IsOptionalToWork = false;
                    lb_PosSelectionType_text.Text = "Nur geplante Positionen und Produkte aktiv!";
                    lb_PosSelectionType_text2.Text = "Nur geplante Positionen und Produkte aktiv!";
                }
                if (foundOPPos != null || foundRegPos != null)
                {
                    // check nach prüfen alle enable/disable setzten
                    AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                    {
                        o.kategorien.ForEach(c =>
                        {
                            c.leistungen.ForEach(l =>
                            {
                                bool inWork = false;
                                if (AppModel.Instance.allPositionInWork != null)
                                {
                                    var foundInWork = AppModel.Instance.allPositionInWork.leistungen.Find(le => le.id == l.id);
                                    inWork = foundInWork != null;
                                }
                                ICommand func = null;
                                Border framePos;
                                if (AppModel.Instance.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                {
                                    func = ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command;
                                }

                                if (AppModel.Instance.IsOptionalToWork)
                                {
                                    // Nur OPTIONALE
                                    if (l.art == "Leistung" && l.nichtpauschal == 1)
                                    {
                                        l.disabled = false;
                                        if (AppModel.Instance.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            var stackPos = inWork ? LeistungWSO.GetInWorkPositionCardView(l, AppModel.Instance, func)
                                            : (l.disabled ? LeistungWSO.GetDisabledPositionCardView(l, AppModel.Instance, func)
                                            : (l.selected ? LeistungWSO.GetSelectedPositionCardView(l, AppModel.Instance, func)
                                            : LeistungWSO.GetPositionCardView(l, AppModel.Instance, func)));
                                            framePos.Content = stackPos.Content;
                                        }
                                    }
                                    else if (l.art == "Leistung" && l.nichtpauschal == 0)
                                    {
                                        l.disabled = true;
                                        if (AppModel.Instance.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            framePos.Content = inWork ? LeistungWSO.GetInWorkPositionCardView(l, AppModel.Instance, func).Content
                                            : LeistungWSO.GetDisabledPositionCardView(l, AppModel.Instance, func).Content;
                                        }
                                    }
                                }
                                else
                                {
                                    if (l.art == "Leistung" && l.nichtpauschal == 1)
                                    {
                                        l.disabled = true;
                                        if (AppModel.Instance.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            framePos.Content = inWork ? LeistungWSO.GetInWorkPositionCardView(l, AppModel.Instance, func).Content
                                            : LeistungWSO.GetDisabledPositionCardView(l, AppModel.Instance, func).Content;
                                        }
                                    }
                                    else
                                    {
                                        l.disabled = false;
                                        if (AppModel.Instance.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            var stackPos = inWork ? LeistungWSO.GetInWorkPositionCardView(l, AppModel.Instance, func)
                                            : (l.selected ? LeistungWSO.GetSelectedPositionCardView(l, AppModel.Instance, func)
                                            : LeistungWSO.GetPositionCardView(l, AppModel.Instance, func));
                                            framePos.Content = stackPos.Content;
                                        }
                                    }
                                }
                            });
                        });
                    });
                }
                else
                {
                    // alles zurücksetzen 
                    AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                    {
                        o.kategorien.ForEach(c =>
                        {
                            c.leistungen.ForEach(l =>
                            {
                                //if(!l.selected)
                                //{
                                l.disabled = false;
                                Border framePos;
                                if (AppModel.Instance.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                {
                                    var func = ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command;
                                    bool inWork = false;
                                    if (AppModel.Instance.allPositionInWork != null)
                                    {
                                        var foundInWork = AppModel.Instance.allPositionInWork.leistungen.Find(le => le.id == l.id);
                                        inWork = foundInWork != null;
                                    }

                                    var stackPos = inWork ? LeistungWSO.GetInWorkPositionCardView(l, AppModel.Instance, func)
                                    : (l.selected ? LeistungWSO.GetSelectedPositionCardView(l, AppModel.Instance, func)
                                    : LeistungWSO.GetPositionCardView(l, AppModel.Instance, func));
                                    framePos.Content = stackPos.Content;
                                }

                                //}
                            });
                        });
                    });
                    lb_PosSelectionType_text.Text = "Bisher nur Produkte gewählt!";
                    lb_PosSelectionType_text2.Text = "Bisher nur Produkte gewählt!";
                }
            }

        }

        private async void ShowRunningWorksView()
        {
            try
            {
                isInitialize = true;
                overlay.IsVisible = true;
                await Task.Delay(1);

                ClearPageViews();
                RunningWorksPage_Container.IsVisible = true;

                ShowRunningWorksPage();

                await Task.Delay(1);
                overlay.IsVisible = false;
                isInitialize = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: ShowRunningWorksView(): ");
            }
        }
        private async void ShowRunningWorksPage()
        {
            try
            {
                isInitialize = true;
                overlay.IsVisible = true;
                await Task.Delay(1);

                var startDT = new DateTime(AppModel.Instance.allPositionInWork.startticks);
                var endDT = DateTime.Now;
                var ts = (endDT - startDT);

                timespan_inwork.Clear();
                timespan_inwork.Add(new Image
                {
                    Margin = new Thickness(0, 0, 10, 0),
                    HeightRequest = 30,
                    WidthRequest = 30,
                    VerticalOptions = LayoutOptions.Center,
                    Source = "time.png"
                }, 0, 0);
                timespan_inwork.Add(new Label
                {
                    Text = startDT.ToString("dd.MM.yy") + "\n" + startDT.ToString("HH:mm"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 16,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                }, 1, 0);
                timespan_inwork.Add(new Label
                {
                    Text = " - ",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 14,
                    TextColor = Colors.Yellow,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                }, 2, 0);
                timespan_inwork.Add(new Label
                {
                    Text = endDT.ToString("dd.MM.yy") + "\n" + endDT.ToString("HH:mm"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 16,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                }, 3, 0);
                timespan_inwork.Add(new Label
                {
                    Text = " = ",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 14,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                }, 4, 0);
                timespan_inwork.Add(new Label
                {
                    Text = (ts.TotalDays > 1 ? ts.ToString("%d") + "T " : "") + ts.ToString(@"hh\:mm"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 20,
                    TextColor = Colors.Yellow,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                }, 5, 0);
                runningworks_list.Children.Clear();
                runningworks_list.Children.Add(LeistungWSO.GetInWorkPositionListView(AppModel.Instance, new Command<LeistungWSO>(TapNoticeFromPosInWork)));

                await Task.Delay(1);
                overlay.IsVisible = false;
                isInitialize = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: ShowRunningWorksPage(): ");
            }
        }


        private async void ShowDayOverPage()
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            MainMenuTapped_Done(false);
            await DayOverPageView.ShowAsync(this);
            overlay.IsVisible = false;
        }


        public async void TapNoticeFromPosInWorkDirektPosMuell(LeistungWSO p)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            //var l = new LeistungWSO
            //{
            //    leiInWork = p,
            //    id = p.id,
            //    auftragid = p.auftragid,
            //    objektid = p.objektid,
            //    kategorieid = p.kategorieid,
            //    gruppeid = p.gruppeid,
            //    anzahl = p.anzahl,
            //    lastwork = p.lastwork,
            //    inout = p.inout,
            //    winterservice = p.winterservice,
            //    workat = p.workat,
            //};
            await ShowNoticeViewDirektPos(false, p, "muellpos");

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void TapNoticeFromPosInWorkDirektPos(LeistungWSO position)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            await ShowNoticeViewDirektPos(false, position, "winterpos");

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        private async Task ShowNoticeViewDirektPos(bool prio, LeistungWSO pos = null, string backTo = null)
        {
            View posCard = null;
            BemerkungWSO existingBemerkung = null;
            if (pos != null)
            {
                BuildingWSO building = BuildingWSO.LoadBuilding(AppModel.Instance, pos.objektid);
                var o = building.ArrayOfAuftrag.Find(auf => auf.id == pos.auftragid);
                var c = o.kategorien.Find(kat => kat.id == pos.kategorieid);
                var l = c.leistungen.Find(f => f.id == pos.id);
                posCard = LeistungWSO.GetInWorkPositionSmallCardView_DirektPos(o, c, l, l);

                if (_SelectedBemerkungForNoticeList_DirektPos != null)
                {
                    existingBemerkung = _SelectedBemerkungForNoticeList_DirektPos
                        .FirstOrDefault(item => item.id == pos.id)?.bem;
                }
            }

            var result = await NoticeDirektPosModalPage.ShowAsync(this, pos, backTo, prio, posCard, existingBemerkung);
            if (result == null)
            {
                return;
            }

            overlay.IsVisible = true;
            await Task.Delay(1);
            if (result.IsDeleted)
            {
                UpdateDirektPosNotice(result.Pos, null);
            }
            else if (result.Bemerkung != null)
            {
                result.Bemerkung.gruppeid = result.Pos.gruppeid;
                result.Bemerkung.personid = AppModel.Instance.Person.id;
                result.Bemerkung.objektid = result.Pos.objektid;
                result.Bemerkung.leistungid = result.Pos.id;
                result.Bemerkung.datum = DateTime.Now.Ticks;
                UpdateDirektPosNotice(result.Pos, result.Bemerkung);
            }
            overlay.IsVisible = false;
            await Task.Delay(1);
        }

        private void UpdateDirektPosNotice(LeistungWSO position, BemerkungWSO bemerkung)
        {
            if (position == null || _SelectedBemerkungForNoticeList_DirektPos == null)
            {
                return;
            }

            foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
            {
                if (item.id != position.id)
                {
                    continue;
                }

                item.bem = bemerkung;
                if (item.bem == null)
                {
                    item.badge.Text = "";
                    item.badgeStack.IsVisible = false;
                }
                else
                {
                    item.badge.Text = "" + (item.bem.photos.Count + (string.IsNullOrWhiteSpace(item.bem.text) ? 0 : 1));
                    item.badgeStack.IsVisible = item.bem.photos.Count() > 0 || !string.IsNullOrWhiteSpace(item.bem.text);
                }
                return;
            }
        }



        public async void TapNoticeFromPosInWork(LeistungWSO position)
        {
            await ShowNoticeView(false, position, "inwork");
        }

        private async Task ShowNoticeView(bool prio, LeistungWSO pos = null, string backTo = null)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            View posCard = null;
            if (pos != null)
            {
                BuildingWSO building = AppModel.Instance.LastBuilding
                    ?? BuildingWSO.LoadBuilding(AppModel.Instance, pos.objektid);
                var o = building.ArrayOfAuftrag.Find(auf => auf.id == pos.auftragid);
                var c = o.kategorien.Find(kat => kat.id == pos.kategorieid);
                var l = c.leistungen.Find(f => f.id == pos.id);
                var lInWork = AppModel.Instance.allPositionInWork.leistungen.Find(f => f.id == pos.id);
                posCard = LeistungWSO.GetInWorkPositionSmallCardView(o, c, l, lInWork, AppModel.Instance);
            }

            await Task.Delay(1);
            overlay.IsVisible = false;

            var result = await NoticeModalPage.ShowAsync(this, pos, backTo, prio, posCard);

            if (result != null)
            {
                overlay.IsVisible = true;
                await Task.Delay(1);

                result.Bemerkung.gruppeid = AppModel.Instance.LastBuilding?.gruppeid ?? 0;
                result.Bemerkung.personid = AppModel.Instance.Person.id;
                result.Bemerkung.objektid = AppModel.Instance.LastBuilding?.id ?? 0;
                result.Bemerkung.leistungid = 0;
                result.Bemerkung.datum = DateTime.Now.Ticks;

                if (result.Pos != null)
                {
                    var posInWork = AppModel.Instance.allPositionInWork.leistungen.Find(p => p.id == result.Pos.id);
                    posInWork.bemerkungen ??= new List<BemerkungWSO>();
                    result.Bemerkung.leistungid = result.Pos.id;
                    posInWork.bemerkungen.Add(result.Bemerkung);
                    LeistungPackWSO.Save(AppModel.Instance, AppModel.Instance.allPositionInWork);
                }
                else
                {
                    BemerkungWSO.ToUploadStack(AppModel.Instance, result.Bemerkung);
                    CheckAllSyncFromUpload(); //SyncSingleNotice();
                }

                await Task.Delay(1);
                overlay.IsVisible = false;
            }

            if (result?.BackTo == "inwork")
            {
                ShowRunningWorksView();
            }
            else
            {
                ShowMainPage();
            }
        }

        private async void ShowObjectValuesView()
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            btn_objectValuesNowTapped(null, null);

            ClearPageViews();

            ObjectValues_BuildingInfo.Children.Clear();
            //ObjectValues_BuildingInfo.Children.Add(Elements.GetBoxViewLine());
            ObjectValues_BuildingInfo.Children.Add(BuildingWSO.GetBuildingInfoElement(AppModel.Instance.LastBuilding, AppModel.Instance));
            //ObjectValues_BuildingInfo.Children.Add(Elements.GetBoxViewLine());

            ObjectValuesStack.Children.Clear();
            var vStack = ObjektDataWSO.GetObjektDataListView(AppModel.Instance, new Command<ObjektDataWSO>(TapObjektData), overlay);
            ObjectValuesStack.Children.Add(vStack);

            ObjectValuesStackChangedToday.Children.Clear();
            var vStackToday = ObjektDataWSO.GetObjektDataListView(AppModel.Instance, new Command<ObjektDataWSO>(TapObjektData), overlay, true);
            ObjectValuesStackChangedToday.Children.Add(vStackToday);

            AppModel.Instance.selectedObjectValue = null;

            ObjectValuesPage_Container.IsVisible = true;
            ObjectValuesPage_position_Container.IsVisible = true;

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void btn_objectValuesNowTapped(object sender, EventArgs e)
        {
            scroll_ObjectValuesStack.IsVisible = true;
            scroll_ObjectValuesStackChangedToday.IsVisible = false;
            btn_objectValuesNow.BackgroundColor = Color.FromArgb("#999999");
            btn_objectValuesToday.BackgroundColor = Color.FromArgb("#042d53");
        }
        public async void btn_objectValuesTodayTapped(object sender, EventArgs e)
        {
            scroll_ObjectValuesStack.IsVisible = false;
            scroll_ObjectValuesStackChangedToday.IsVisible = true;
            btn_objectValuesNow.BackgroundColor = Color.FromArgb("#042d53");
            btn_objectValuesToday.BackgroundColor = Color.FromArgb("#999999");
        }

        public async void TapObjektData(ObjektDataWSO od)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            AppModel.Instance.selectedObjectValue = od;
            ShowObjectValuesEditView();
        }
        private async void ShowObjectValuesEditView()
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();

            ObjectValues_BuildingInfo_edit.Children.Clear();
            ObjectValues_BuildingInfo_edit.Children.Add(Elements.GetBoxViewLine());
            ObjectValues_BuildingInfo_edit.Children.Add(BuildingWSO.GetBuildingInfoElement(AppModel.Instance.LastBuilding, AppModel.Instance));
            ObjectValues_BuildingInfo_edit.Children.Add(Elements.GetBoxViewLine());
            ObjectValues_BuildingInfo_edit.Children.Add(ObjektDataWSO.GetObjektValueInfoElement(AppModel.Instance.selectedObjectValue, AppModel.Instance, null));
            ObjectValues_BuildingInfo_edit.Children.Add(Elements.GetBoxViewLine());

            ObjectValues_Info_edit.Children.Clear();
            ObjectValues_Info_edit.Children.Add(ObjektDataWSO.EditObjektValueField(AppModel.Instance.selectedObjectValue, AppModel.Instance,
                new Command<ObjektDataWSO>(SaveObjektValue),
                new Command(SwitchObjectValueFlashlight),
                new Command(OpenCamObjectValuesView)));

            ObjectValuesPage_Container.IsVisible = true;
            ObjectValuesPage_Edit_Container.IsVisible = true;

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        private async void OpenCamObjectValuesView()
        {
            var sent = await ObjectValuesBildModalPage.ShowAsync(this);
            if (sent)
            {
                CheckAllSyncFromUpload(); //SyncObjectValueBild();
            }
        }

        private async void SwitchObjectValueFlashlight()
        {
            AppModel.Instance.Btn_FlashlightAloneTapped(null, null);
        }

        public async void SaveObjektValue(ObjektDataWSO newod)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            if (AppModel.Instance.isFlashLigthAloneON)
            {
                AppModel.Instance.Btn_FlashlightAloneTapped(null, null);
            }
            await Task.Delay(1);

            long datum = JavaScriptDateConverter.Convert(DateTime.Now, -2);
            newod.standGeaendertAm = "" + datum;
            newod.standdatum = "" + datum;
            newod.lastchange = "" + datum;
            AppModel.Instance.selectedObjectValue = newod;

            AppModel.Instance.LastBuilding.ArrayOfObjektdata.ForEach(od =>
            {
                if (od.id == newod.id)
                {
                    od.lastStand = Utils.formatDEStr3(decimal.Parse(od.stand, CultureInfo.GetCultureInfo("de-DE")));
                    od.stand = Utils.formatDEStr3(decimal.Parse(newod.firstStand, CultureInfo.GetCultureInfo("de-DE")));
                    od.standdatum = "" + datum;
                    od.standGeaendertAm = "" + datum;
                    od.lastchange = "" + datum;
                    od.ablesegrund = "" + newod.ablesegrund;
                }
            });
            BuildingWSO.Save(AppModel.Instance, AppModel.Instance.LastBuilding);

            newod.guid = Guid.NewGuid().ToString();
            newod.ticks = DateTime.Now.Ticks;
            newod.lastStand = Utils.formatDEStr3(decimal.Parse(newod.stand, CultureInfo.GetCultureInfo("de-DE")));
            newod.stand = Utils.formatDEStr3(decimal.Parse(newod.firstStand, CultureInfo.GetCultureInfo("de-DE")));
            ObjektDataWSO.ToUploadStack(AppModel.Instance, newod);
            await Task.Delay(1);
            CheckAllSyncFromUpload(); //SyncObjectValues();

            await Task.Delay(1);
            ShowObjectValuesView();
        }

        public void ClearPageViews()
        {
            NotScanPage_Container.IsVisible = false;
            _personTimesPageView?.SetVisible(false);
            NachbuchenPage_Container.IsVisible = false;
            StartPage_Container.IsVisible = false;
            //PN_Page_Container.IsVisible = false;
            _workerPageContainerView?.ContainerGrid.IsVisible = false;
            //BuildingScanPage_Container.IsVisible = false;
            //BuildingOutScanPage_Container.IsVisible = false;
            BuildingOrderPage_Container.IsVisible = false;
            RunningWorksPage_Container.IsVisible = false;
            ObjectValuesPage_Container.IsVisible = false;
            ObjectValuesPage_position_Container.IsVisible = false;
            ObjectValuesPage_Edit_Container.IsVisible = false;
            //MapPage_Container.IsVisible = false;
        }


        public void btn_SettingsBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }
        public async void btn_SettingsTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            MainMenuTapped_Done(false);
            await SettingsPageView.ShowAsync(this);
            overlay.IsVisible = false;
        }


        public void btn_MapBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }



        /*********************/
        /* HAPUTMENU BUTTONS */
        /*********************/

        private double tabContentWidth = 0;

        private async void daypicker_SelectedIndexChanged(object o, int day)
        {
            //await tourScroller.ScrollToAsync(0, 0, false);
            frame_planConA_offenGrid.IsVisible = true;
            frame_planConA_erlGrid.IsVisible = false;
            frame_planConA_veroffenGrid.IsVisible = false;

            popupContainer_quest_daypicker.IsVisible = false;
            var xday = int.Parse(((Label)o).ClassId);
            Update_PlanTabs(xday);
        }

        async Task FadeIn(View v)
        {//frame_planConA_offenhead
            v.Opacity = 0;
            v.IsVisible = true;
            await v.FadeToAsync(1, 800, Easing.CubicOut);
        }

        async Task FadeOut(View v)
        {
            await v.FadeToAsync(0, 200, Easing.CubicIn);
            v.IsVisible = false;
        }

        private void Fill_DayPicker()
        {
            double w = screenWidthDp - 13;
            tabContentWidth = w; //28 ;

            var today = ((int)DateTime.Now.DayOfWeek);
            daypicker_items.Children.Clear();
            for (int i = 1; i <= 7; i++)
            {
                if (i == 7) { i = 0; }
                var lb_day = new Label
                {
                    Text = Utils.DaysInUtils[i] + (i == today ? " (Heute)" : ""),
                    Margin = new Thickness(0),
                    Padding = new Thickness(0, 7, 0, 7),
                    FontSize = 18,
                    TextColor = Color.FromArgb("#cccccc"),
                    HorizontalOptions = LayoutOptions.Fill,
                    ClassId = "" + i,
                };
                lb_day.GestureRecognizers.Clear();
                var t_lb = new TapGestureRecognizer();
                t_lb.Tapped -= (object o, TappedEventArgs ev) => { daypicker_SelectedIndexChanged(o, i); };
                t_lb.Tapped += (object o, TappedEventArgs ev) => { daypicker_SelectedIndexChanged(o, i); };
                lb_day.GestureRecognizers.Add(t_lb);
                daypicker_items.Children.Add(lb_day);
                var bv = new Border
                {
                    BackgroundColor = Colors.Gray,
                    HeightRequest = 2,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0)
                };
                if (i != 0)
                {
                    daypicker_items.Children.Add(bv);
                }
                if (i == 0) { i = 8; }
            }


            frame_planConA_offenbtn.GestureRecognizers.Clear();
            var t_frame_planConA_offentxt = new TapGestureRecognizer();
            t_frame_planConA_offentxt.Tapped += async (object o, TappedEventArgs ev) =>
            {
                frame_planConA_offenGrid.IsVisible = true;
                frame_planConA_erlGrid.IsVisible = false;
                frame_planConA_veroffenGrid.IsVisible = false;
                frame_planConA_erlhead.IsVisible = false;
                frame_planConA_veroffenhead.IsVisible = false;
                frame_planConA_offenhead.IsVisible = true;
            };
            frame_planConA_offenbtn.GestureRecognizers.Add(t_frame_planConA_offentxt);

            frame_planConA_erlbtn.GestureRecognizers.Clear();
            var t_frame_planConA_erltxt = new TapGestureRecognizer();
            t_frame_planConA_erltxt.Tapped += async (object o, TappedEventArgs ev) =>
            {
                frame_planConA_offenGrid.IsVisible = false;
                frame_planConA_erlGrid.IsVisible = true;
                frame_planConA_offenhead.IsVisible = false;
                frame_planConA_veroffenhead.IsVisible = false;
                frame_planConA_erlhead.IsVisible = true;
                frame_planConA_veroffenGrid.IsVisible = false;
            };
            frame_planConA_erlbtn.GestureRecognizers.Add(t_frame_planConA_erltxt);
            frame_planConA_veroffenbtn.GestureRecognizers.Clear();
            var t_frame_planConA_veroffentxt = new TapGestureRecognizer();
            t_frame_planConA_veroffentxt.Tapped += async (object o, TappedEventArgs ev) =>
            {
                frame_planConA_offenGrid.IsVisible = false;
                frame_planConA_erlGrid.IsVisible = false;
                frame_planConA_veroffenGrid.IsVisible = true;
                frame_planConA_offenhead.IsVisible = false;
                frame_planConA_erlhead.IsVisible = false;
                frame_planConA_veroffenhead.IsVisible = true;
            };
            frame_planConA_veroffenbtn.GestureRecognizers.Add(t_frame_planConA_veroffentxt);

            frame_planConB_offenbtn.GestureRecognizers.Clear();
            var t_frame_planConB_offentxt = new TapGestureRecognizer();
            t_frame_planConB_offentxt.Tapped += async (object o, TappedEventArgs ev) =>
            {
                tourScrollerB_containerA.IsVisible = true;
                tourScrollerB_containerB.IsVisible = false;
                frame_planConB_erlhead.IsVisible = false;
                frame_planConB_offenhead.IsVisible = true;
            };
            frame_planConB_offenbtn.GestureRecognizers.Add(t_frame_planConB_offentxt);
            frame_planConB_erlbtn.GestureRecognizers.Clear();
            var t_frame_planConB_erltxt = new TapGestureRecognizer();
            t_frame_planConB_erltxt.Tapped += async (object o, TappedEventArgs ev) =>
            {
                tourScrollerB_containerA.IsVisible = false;
                tourScrollerB_containerB.IsVisible = true;
                frame_planConB_offenhead.IsVisible = false;
                frame_planConB_erlhead.IsVisible = true;
            };
            frame_planConB_erlbtn.GestureRecognizers.Add(t_frame_planConB_erltxt);

            frame_planConC_offenbtn.GestureRecognizers.Clear();
            var t_frame_planConC_offentxt = new TapGestureRecognizer();
            t_frame_planConC_offentxt.Tapped -= async (object o, TappedEventArgs ev) => { tourScrollerCaa.IsVisible = true; tourScrollerCbb.IsVisible = false; tourScrollerCcc.IsVisible = false; };
            t_frame_planConC_offentxt.Tapped += async (object o, TappedEventArgs ev) => { tourScrollerCaa.IsVisible = true; tourScrollerCbb.IsVisible = false; tourScrollerCcc.IsVisible = false; };
            frame_planConC_offenbtn.GestureRecognizers.Add(t_frame_planConC_offentxt);
            frame_planConC_workbtn.GestureRecognizers.Clear();
            var t_frame_planConC_worktxt = new TapGestureRecognizer();
            t_frame_planConC_worktxt.Tapped -= async (object o, TappedEventArgs ev) => { tourScrollerCaa.IsVisible = false; tourScrollerCbb.IsVisible = true; tourScrollerCcc.IsVisible = false; };
            t_frame_planConC_worktxt.Tapped += async (object o, TappedEventArgs ev) => { tourScrollerCaa.IsVisible = false; tourScrollerCbb.IsVisible = true; tourScrollerCcc.IsVisible = false; };
            frame_planConC_workbtn.GestureRecognizers.Add(t_frame_planConC_worktxt);
            frame_planConC_erlbtn.GestureRecognizers.Clear();
            var t_frame_planConC_erltxt = new TapGestureRecognizer();
            t_frame_planConC_erltxt.Tapped -= async (object o, TappedEventArgs ev) => { tourScrollerCaa.IsVisible = false; tourScrollerCbb.IsVisible = false; tourScrollerCcc.IsVisible = true; };
            t_frame_planConC_erltxt.Tapped += async (object o, TappedEventArgs ev) => { tourScrollerCaa.IsVisible = false; tourScrollerCbb.IsVisible = false; tourScrollerCcc.IsVisible = true; };
            frame_planConC_erlbtn.GestureRecognizers.Add(t_frame_planConC_erltxt);

            Init_PlanTabs();
            //Update_PlanTabs(today);

        }

        public async void SetChecksCount()
        {
            if (AppModel.Instance.AppControll.showChecks)
            {
                if (AppModel.Instance.ChecksInfoResponse.checks != null && AppModel.Instance.ChecksInfoResponse.checks.Count > 0)
                {
                    var checks = AppModel.Instance.ChecksInfoResponse.checks.OrderBy(_ => _.naeststeFaelligkeitDate);

                    int offen = checks.Where(_ => _.lastStateOfCheck_a == "Offen").Count();
                    int faellig = checks.Where(_ => _.lastStateOfCheck_a != "Offen" && _.naeststeFaelligkeitDate < 8 && _.berechnunginterval > 0).Count();
                    int inRed = offen + faellig;
                    int inGreen = 0;

                    if (checkInfoLastView == 99)
                    {
                        inGreen = checks.Where(_ => (_.lastStateOfCheck_a != "Offen" && _.naeststeFaelligkeitDate >= 8) || (_.lastStateOfCheck_a != "Offen" && _.berechnunginterval == 0)).Count();
                    }

                    if (inRed > 0)
                    {
                        frame_plantabCe_badge_count.Text = "" + inRed;
                        frame_plantabCe_badge.IsVisible = true;
                    }
                    else
                    {
                        frame_plantabCe_badge.IsVisible = false;
                    }
                    if (inGreen > 0)
                    {
                        frame_plantabCe_badge_count_g.Text = "" + inGreen;
                        frame_plantabCe_badge_g.IsVisible = true;
                    }
                    else
                    {
                        frame_plantabCe_badge_g.IsVisible = false;
                    }
                }
                else
                {
                    frame_plantabCe_badge.IsVisible = false;
                    frame_plantabCe_badge_g.IsVisible = false;
                }
            }
            await Task.Delay(1);
        }


        public void OpenOtherPerson()
        {
            double w = screenWidthDp;
            double h = screenHeightDp;

            empListView.SelectedItem = null;
            popupContainer_quest_personpicker_inner.HeightRequest = h - 100;
            popupContainer_quest_personpicker_inner.WidthRequest = w - 40;
            popupContainer_quest_personpicker.IsVisible = true;

            var empList = AppModel.Instance.PlanResponse.persons;

            var groupedData =
                empList.OrderBy(e => e.name)
                    .GroupBy(e => e.name[0].ToString())
                    .Select(e => new ObservablePersonSmallWSOCollection<string, PersonSmallWSO>(e))
                    .ToList();

            empListView.ItemsSource = new ObservableCollection<ObservablePersonSmallWSOCollection<string, PersonSmallWSO>>(groupedData);

        }

        private void empListViewItem_Tapped(object sender, TappedEventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is PersonSmallWSO person)
            {
                AppModel.Instance.PlanResponse.selectedPerson = person;
                CloseOtherPerson();
                LoadOtherPersonPlanData(person);
            }
        }

        private void empListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var p = (PersonSmallWSO)e.CurrentSelection[0];
                AppModel.Instance.PlanResponse.selectedPerson = p;
                CloseOtherPerson();
                LoadOtherPersonPlanData(p);
            }
        }
        public void CloseOtherPerson()
        {
            popupContainer_quest_personpicker.IsVisible = false;
        }


        public void OpenLeistungInfoDialog(LeistungWSO o)
        {
            popupContainer_infodialog_text.Text = o.notiz;
            popupContainer_infodialog.IsVisible = true;
        }
        public void OpenKategorieInfoDialog(KategorieWSO o)
        {
            popupContainer_infodialog_text.Text = o.notiz;
            popupContainer_infodialog.IsVisible = true;
        }

        public void OpenObjektInfoDialog()
        {
            popupContainer_infodialog_text.Text = AppModel.Instance.LastBuilding.notiz;
            popupContainer_infodialog.IsVisible = true;
        }
        public void OpenObjektInfoDialogB(string n)
        {
            popupContainer_infodialog_text.Text = n;
            popupContainer_infodialog.IsVisible = true;
        }
        public void OpenBuildingInfoDialog(BuildingWSO b)
        {
            popupContainer_infodialog_text.Text = b.notiz;
            popupContainer_infodialog.IsVisible = true;
        }
        public void CloseInfoDialog()
        {
            popupContainer_infodialog.IsVisible = false;
        }

        public async void LoadOtherPersonPlanData(PersonSmallWSO p)
        {
            SetAppControll();
            if (!AppModel.Instance.AppControll.showObjektPlans) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);

            AppModel.Instance.PlanResponse.selectedPerson = p;
            if (p != null)
            {

                var result = await Task.Run(() => { return AppModel.Instance.Connections.GetPlanPersons(p.id, true); });
                if (result)
                {
                    frame_plantabA.Margin = new Thickness(0, -8, 2, 0);
                    frame_plantabB.Margin = new Thickness(0, 0, 2, 0);
                    frame_plantabCe.Margin = new Thickness(0, 0, 2, 0);
                    frame_plantabC.Margin = new Thickness(0, 0, 2, 0);


                    //ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                    frame_planConA_img_reloadx.Source = GetMuellInOutXImageName(AppModel.Instance.AppSetModel.ViewOnlyMuell);
                    //frame_planConA_img_reload.Source = AppModel.Instance.imagesBase.DropLeftImage;
                    //frame_planConA_img_reload2.Source = AppModel.Instance.imagesBase.DropLeftImage;

                    frame_planConA_reload_text.Text = "Mein Plan";
                    frame_planConA_reload2_text.Text = "Mein Plan";
                    frame_planConA_reload_text.TextColor = Colors.Yellow;
                    frame_planConA_reload2_text.TextColor = Colors.Yellow;
                    frame_planConA_otherperson_name2.TextColor = Colors.Yellow;
                    frame_planConA_otherperson_name.TextColor = Colors.Yellow;
                    frame_planConA_otherperson_name.Text = p.name.Length > 9 ? p.name.Substring(0, 10) + "..." : p.name;
                    frame_planConA_otherperson_name2.Text = p.name.Length > 12 ? p.name.Substring(0, 13) + "..." : p.name;
                }
                else
                {
                    // Alert nicht Online oder es konnten keine Daten geladen werden
                }

                if (AppModel.Instance.PlanOthePersonResponse != null && AppModel.Instance.PlanOthePersonResponse.lastCall != null)
                {
                    ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Color.FromArgb("#aaaaaa");
                    ObjektPlanWeekMobil_Stack_ABC_text.Text =
                        "Andere Planliste: " + AppModel.Instance.PlanOthePersonResponse.lastCall.Value.ToString("dd.MM.yyyy - HH:mm");
                }
                else
                {
                    ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Colors.Yellow;
                    ObjektPlanWeekMobil_Stack_ABC_text.Text = "Andere Planliste: - Konnte nicht geladen werden!";
                }
            }
            Update_PlanTabs((int)DateTime.Now.DayOfWeek);
        }
        public async void PlanTypeChange()
        {
            if (!AppModel.Instance.AppControll.showObjektPlans) { return; }
            var PlanResp = AppModel.Instance.PlanResponse;
            if (AppModel.Instance.PlanResponse != null && AppModel.Instance.PlanResponse.selectedPerson != null)
            {
                PlanResp = AppModel.Instance.PlanOthePersonResponse;
            }
            if (AppModel.Instance.AppSetModel.ViewOnlyMuell == 2)
            {
                AppModel.Instance.AppSetModel.ViewOnlyMuell = 0;
            }
            else
            {
                AppModel.Instance.AppSetModel.ViewOnlyMuell++;
            }
            frame_planConA_img_reloadx.Source = GetMuellInOutXImageName(AppModel.Instance.AppSetModel.ViewOnlyMuell);

            //foreach (var o in frame_planListA.Children)
            //{
            //    var isMuell = !String.IsNullOrWhiteSpace(o.ClassId);
            //    o.IsVisible = AppModel.Instance.AppSetModel.ViewOnlyMuell == 0 
            //        || AppModel.Instance.AppSetModel.ViewOnlyMuell == 1 && !isMuell 
            //        || AppModel.Instance.AppSetModel.ViewOnlyMuell == 2 && isMuell;
            //} 

            // Children durchgehen und Sichtbarkeit setzen
            foreach (var child in frame_planListA.Children)
            {
                // Cast zu VisualElement (hat IsVisible und ClassId)
                if (child is VisualElement element)
                {
                    var isMuell = !string.IsNullOrWhiteSpace(element.ClassId);

                    // Sichtbarkeit basierend auf ViewOnlyMuell setzen
                    element.IsVisible = AppModel.Instance.AppSetModel.ViewOnlyMuell switch
                    {
                        0 => true,                    // Beides anzeigen
                        1 => !isMuell,                // Nur Plan (nicht Müll)
                        2 => isMuell,                 // Nur Müll
                        _ => true
                    };
                }
            }

            ////Update_PlanTabs((int)DateTime.Now.DayOfWeek);
            ////PlanResp.planweek.ForEach(p =>
            ////{
            ////    if (p.day > 0 && p.view != null)
            ////    {
            ////        p.view.IsVisible = AppModel.Instance.AppSetModel.ViewOnlyMuell == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 1 && p.muelltoid == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 2 && p.muelltoid > 0;
            ////    }
            ////});
        }

        public async void ReloadPlanData(int tab)
        {
            ClearPlanDataView();

            bool reloadOr = frame_planConA_reload_text.Text == "Mein Plan";
            if (reloadOr)
            {
                Update_PlanTabs((int)DateTime.Now.DayOfWeek);
            }
            else
            {
                Load_PlanTabs((int)DateTime.Now.DayOfWeek);
            }
        }
        public async void ClearPlanDataView()
        {
            btn_PlanTabATapped(null, null);
            //if (tab == 0) { btn_PlanTabATapped(null, null); }
            //if (tab == 1) { btn_PlanTabBTapped(null, null); }

            if (!AppModel.Instance.AppControll.showObjektPlans) { return; }
            frame_planConA_img_reloadx.Source = GetMuellInOutXImageName(AppModel.Instance.AppSetModel.ViewOnlyMuell);
            frame_planConA_reload_text.Text = "Neu laden";
            frame_planConA_reload2_text.Text = "Neu laden";
            frame_planConA_otherperson_name.Text = "Arbeiter";
            frame_planConA_otherperson_name2.Text = "Arbeiter";
            frame_planConA_reload_text.TextColor = Colors.White;
            frame_planConA_reload2_text.TextColor = Colors.White;
            frame_planConA_otherperson_name2.TextColor = Colors.White;
            frame_planConA_otherperson_name.TextColor = Colors.White;
            AppModel.Instance.PlanResponse.selectedPerson = null;

            frame_planListA.Children.Clear();
            frame_planListAb.Children.Clear();
            frame_planListAc.Children.Clear();
            frame_planListBoffen.Children.Clear();
            frame_planListBerl.Children.Clear();
            frame_planListCoffen.Children.Clear();
            frame_planListCwork.Children.Clear();
            frame_planListCerl.Children.Clear();

            await Task.Delay(10);

        }
        /// <summary>
        /// Async Task version of Load_PlanTabs to properly handle async/await chain
        /// </summary>
        public async Task Load_PlanTabsAsync(int today)
        {
            try
            {
                SetAppControll();
                if (!AppModel.Instance.AppControll.showObjektPlans) { return; }
                overlay.IsVisible = true;
                await Task.Delay(1);

                var result = await Task.Run(() => { return AppModel.Instance.Connections.GetPlanPersons(AppModel.Instance.Person.id); });
                if (result)
                {
                    if (AppModel.Instance.PlanResponse.lastCall != null)
                    {
                        ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Color.FromArgb("#aaaaaa");
                        ObjektPlanWeekMobil_Stack_ABC_text.Text =
                            "Meine Planliste: " + AppModel.Instance.PlanResponse.lastCall.Value.ToString("dd.MM.yyyy - HH:mm");
                    }
                    else
                    {
                        ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Colors.Yellow;
                        ObjektPlanWeekMobil_Stack_ABC_text.Text = "Meine Planliste: - Konnte noch nicht neu geladen werden!";
                    }
                    //ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                    buildFilterFromPlanKategories();
                }
                else
                {
                    var resp = ObjektPlanWeekMobile.Load(AppModel.Instance);
                    if (resp == null)
                    {
                        AppModel.Instance.PlanResponse = new PlanResponse();
                    }
                    else
                    {
                        AppModel.Instance.PlanResponse = resp;
                    }
                    ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Colors.Yellow;
                    if (AppModel.Instance.PlanResponse.lastCall != null)
                    {
                        ObjektPlanWeekMobil_Stack_ABC_text.Text = "Meine Planliste: (" + AppModel.Instance.PlanResponse.lastCall.Value.ToString("dd.MM. - HH:mm") + ") - vom Cache geholt!";
                    }
                    else
                    {
                        ObjektPlanWeekMobil_Stack_ABC_text.Text = "Meine Planliste: KEINE DATEN!";
                    }
                    buildFilterFromPlanKategories();
                }
                Update_PlanTabs(today);
                var dt = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ?
                    DateTime.Now.AddDays(-2) : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                box_buildingInformation.Children.Clear();
                box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(AppModel.Instance, dt));
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"Method => MainPage-Load_PlanTabsAsync(catch): {ex.Message} | StackTrace: {ex.StackTrace}");
                throw; // Re-throw to be caught by caller
            }
        }

        public async void Load_PlanTabs(int today)
        {
            // Keep the old async void method for backward compatibility, but call the async Task version
            try
            {
                await Load_PlanTabsAsync(today);
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"Method => MainPage-Load_PlanTabs wrapper(catch): {ex.Message}");
            }
        }

        private void buildFilterFromPlanKategories()
        {
            AppModel.Instance.Plan_ObjekteThisWeek = new List<Int32>();
            AppModel.Instance.Plan_KatThisWeek = new List<Int32>();
            if (AppModel.Instance.AppControll.filterKategories && !AppModel.Instance.AppControll.ignoreKategorieFilterByPerson
                && AppModel.Instance.PlanResponse.planweek != null && AppModel.Instance.PlanResponse.planweek.days != null)
            {
                AppModel.Instance.PlanResponse.planweek.days.ForEach(day =>
                {
                    day.ForEach(item =>
                    {
                        item.more.ForEach(itemM =>
                        {
                            AppModel.Instance.Plan_KatThisWeek.Add(itemM.katid);
                            AppModel.Instance.Plan_ObjekteThisWeek.Add(itemM.objektid);
                        });
                    });
                });
                AppModel.Instance.Plan_KatThisWeek = AppModel.Instance.Plan_KatThisWeek.Distinct().ToList();
                AppModel.Instance.Plan_ObjekteThisWeek = AppModel.Instance.Plan_ObjekteThisWeek.Distinct().ToList();
            }
        }

        public void Init_PlanTabs()
        {
            var w = screenWidthDp;
            frame_planConA_veroffen.Text = "";//Badge Counter
            frame_planConA_veroffentxt.Text = "Vergangene\r\nOffene";
            frame_planConA_veroffen_count_con.IsVisible = false;
            frame_planConA_erl.Text = "";//Badge Counter
            frame_planConA_erltxt.Text = "Heute\r\nErledigt";
            frame_planConA_erl_count_con.IsVisible = false;
            frame_planConA_offen.Text = ""; //Badge Counter
            frame_planConA_typeoffen.Text = "";
            frame_planConA_offentxt.Text = "Offen";
            frame_planConA_offen_count_con.IsVisible = false;
            frame_planConA_offen_typecount_con.IsVisible = false;

            frame_planConB_offen.Text = "";//Badge Counter
            frame_planConB_offentxt.Text = "Nach Bedarf";
            frame_planConB_offen_count_con.IsVisible = false;
            frame_planConB_erl.Text = "";//Badge Counter
            frame_planConB_erltxt.Text = "Heute Erledigte";
            frame_planConB_erl_count_con.IsVisible = false;


            frame_planConC_offen.Text = "";//Badge Counter
            frame_planConC_offentxt.Text = "Offene";
            frame_planConC_offen_count_con.IsVisible = false;
            frame_planConC_erl.Text = "";//Badge Counter
            frame_planConC_erltxt.Text = "Erledigte";
            frame_planConC_erl_count_con.IsVisible = false;
            frame_planConC_work.Text = "";//Badge Counter
            frame_planConC_worktxt.Text = "In Arbeit";
            frame_planConC_work_count_con.IsVisible = false;


            //await Task.Delay(1);
            frame_planListAb.WidthRequest = w;
            frame_planListAc.WidthRequest = w;
            frame_planListA.WidthRequest = w;

            frame_planListBoffen.WidthRequest = w;
            frame_planListBerl.WidthRequest = w;

            frame_planListCeoffen.WidthRequest = w;
            frame_planListCefaellig.WidthRequest = w;
            frame_planListCeerl.WidthRequest = w;

            frame_planListCoffen.WidthRequest = w;
            frame_planListCwork.WidthRequest = w;
            frame_planListCerl.WidthRequest = w;
        }

        public async void Update_PlanTabs(int xday)
        {
            try
            {
                overlay.IsVisible = true;
                await Task.Delay(1);
                var today = (int)DateTime.Now.DayOfWeek;
                var isToday = xday == today;
                frame_planConA_daytext.Text = Utils.DaysInUtils[xday];// + (isToday ? " (Heute)":"");

                var PlanResp = AppModel.Instance.PlanResponse;
                if (AppModel.Instance.PlanResponse.selectedPerson != null)
                {
                    PlanResp = AppModel.Instance.PlanOthePersonResponse;
                }

                //Init_PlanTabs();

                frame_planListA.Children.Clear();
                frame_planListAb.Children.Clear();
                frame_planListAc.Children.Clear();
                frame_planListBoffen.Children.Clear();
                frame_planListBerl.Children.Clear();
                frame_planListCoffen.Children.Clear();
                frame_planListCwork.Children.Clear();
                frame_planListCerl.Children.Clear();

                await Task.Delay(1);

                if (PlanResp.success)
                {

                    if (PlanResp.planweek != null)
                    {
                        // gib alle Plans die von Heute oder vorher die nch nicht bearbeitet wurden von diesem Objekt zurück incl. Kategorie(NachBedarf)
                        List<PlanPersonMobile> plansReady = PlanResp.planweek.days[xday].FindAll(p => p.haswork == 1 && p.day > -1).OrderBy(o => o.sort).ToList();
                        List<PlanPersonMobile> plansToday = PlanResp.planweek.days[xday].FindAll(p => p.haswork == 0 && p.day > -1).OrderBy(o => o.sort).ToList();
                        List<PlanPersonMobile> plansLast = new List<PlanPersonMobile>();
                        PlanResp.planweek.days.ForEach(d =>
                        {
                            d.ForEach(item =>
                            {

                                if (item.haswork == 0 && item.day < today && today > 0 && item.day > 0)
                                {
                                    plansLast.Add(item);
                                }
                                //if (item.haswork == 0 && today == 0 && item.day >= 0)
                                //{
                                //    plansLast.Add(item);
                                //}
                            });
                        });


                        frame_planConA_veroffen.Text = plansLast.Count + "";
                        frame_planConA_veroffentxt.Text = "Vergangene\r\nOffene";

                        frame_planConA_erl.Text = plansReady.Count + "";
                        frame_planConA_erltxt.Text = "Heute\r\nErledigt";

                        frame_planConA_offen.Text = plansToday.Count + "";
                        frame_planConA_typeoffen.Text = plansToday.Count + "";


                        if (isToday)
                        {
                            frame_planConA_offentxt.Text = "Heute\r\nOffen";
                        }
                        else
                        {
                            frame_planConA_offentxt.Text = Utils.DaysInUtils[xday] + "\r\nOffen";
                        }

                        frame_planConA_offen_count_con.IsVisible = plansToday.Count > 0;
                        frame_planConA_offen_typecount_con.IsVisible = plansToday.Count > 0;

                        frame_planListA.IsVisible = false;
                        frame_planListA.Children.Clear();
                        plansToday = [.. plansToday.Distinct()];
                        plansToday.ForEach(p =>
                        {
                            var objekt = AppModel.Instance.AllBuildings.Find(ob => ob.id == p.objektid);
                            var stack = ObjektPlanWeekMobile.GetPlanedTodayList(p, new Command<IntBoolParam>(SelectedObjektAufterNotScan));
                            var containerA = new VerticalStackLayout
                            {
                                Padding = new Thickness(0),
                                Margin = new Thickness(0),
                                Spacing = 0,
                                HorizontalOptions = LayoutOptions.Fill,
                                Children = { stack },
                                ClassId = p.muelltoid > 0 ? "Muell" : "",
                                IsVisible = AppModel.Instance.AppSetModel.ViewOnlyMuell == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 1 && p.muelltoid == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 2 && p.muelltoid > 0
                            };
                            var containerB = new VerticalStackLayout
                            {
                                Padding = new Thickness(0),
                                Margin = new Thickness(0),
                                Spacing = 0,
                                HorizontalOptions = LayoutOptions.Fill,
                                IsVisible = false
                            };
                            containerA.Children.Add(containerB);
                            var o = new List<Object>(){
                                    containerB,
                                    AppModel.Instance,
                                    objekt,
                                    overlay,
                                    p
                                    };
                            stack.GestureRecognizers.Clear();
                            // p.view = containerA;
                            if (p.muelltoid > 0 && objekt != null)
                            {
                                stack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<Object>(OpenDirektbuchenAusPlanliste), CommandParameter = o });
                            }
                            else
                            {
                                stack.GestureRecognizers.Add(new TapGestureRecognizer()
                                {
                                    Command = new Command<Object>(BuildingWSO.ShowOrderContainer),
                                    CommandParameter = o
                                });
                            }

                            frame_planListA.Children.Add(containerA);

                            //frame_planListA.Children.Add(ObjektPlanWeekMobile.GetPlanedTodayList(p));
                        });
                        frame_planListA.IsVisible = true;


                        frame_planConA_erl_count_con.IsVisible = plansReady.Count > 0;
                        plansReady.ForEach(p =>
                        {
                            VerticalStackLayout containerReady = new VerticalStackLayout();
                            try
                            {
                                containerReady = ObjektPlanWeekMobile.GetPlanedReadyTodayList(p);
                            }
                            catch (Exception ex)
                            {
                                containerReady.Children.Add(new Label() { Text = "Fehler beim Laden der erledigten Leistung: ", TextColor = Colors.Red });
                                AppModel.Logger.Warn("ERROR: Fehler beim Laden der erledigten Leistung: " + (p == null ? "(p = null) :: " : " ")
                                        + ex.Message + "-- - " + ex.StackTrace != null ? ex.StackTrace : "");
                            }
                            //containerReady.IsVisible = true;
                            //p.view = containerReady;
                            frame_planListAb.Children.Add(containerReady);
                        });


                        frame_planConA_veroffen_count_con.IsVisible = plansLast.Count > 0;

                        frame_plantabA_badge_count.Text = (plansLast.Count + plansToday.Count) + "";
                        frame_plantabA_badge.IsVisible = plansLast.Count + plansToday.Count > 0;

                        int ii = 1;
                        for (int i = 1; i <= 7; i++)
                        {
                            if (ii == 7) { ii = 0; }
                            var pl = plansLast.Where(p => p.day == ii).OrderBy(p => p.sort).ToList();
                            if (pl.Count > 0 && xday != ii)
                            {
                                var stdayI = new StackLayout()
                                {
                                    Padding = new Thickness(5),
                                    Margin = new Thickness(0),
                                    Spacing = 0,
                                    Orientation = StackOrientation.Horizontal,
                                    HorizontalOptions = LayoutOptions.Fill,
                                    BackgroundColor = Colors.Transparent,
                                    Children = {
                                    new Label() {
                                        Text = Utils.DaysInUtils[ii] + (ii == today ? " (Heute)" : ""),
                                        TextColor = Color.FromArgb("#ffcc00"),
                                        Margin = new Thickness(3, 0, 5, 1),
                                        FontSize = 18,
                                        HorizontalOptions = LayoutOptions.Start,
                                        LineBreakMode = LineBreakMode.WordWrap,
                                    }
                                }
                                };
                                frame_planListAc.Children.Add(stdayI);
                                pl.ForEach(p =>
                                {
                                    var build = AppModel.Instance.AllBuildings.Find(ob => ob.id == p.objektid);
                                    var stack = ObjektPlanWeekMobile.GetPlanedTodayList(p, new Command<IntBoolParam>(SelectedObjektAufterNotScan));
                                    var containerA = new VerticalStackLayout
                                    {
                                        Padding = new Thickness(0),
                                        Margin = new Thickness(0),
                                        Spacing = 0,
                                        HorizontalOptions = LayoutOptions.Fill,
                                        Children = { stack },
                                        IsVisible = true
                                    };
                                    var containerB = new VerticalStackLayout
                                    {
                                        Padding = new Thickness(0),
                                        Margin = new Thickness(0),
                                        Spacing = 0,
                                        HorizontalOptions = LayoutOptions.Fill,
                                        IsVisible = false
                                    };
                                    containerA.Children.Add(containerB);
                                    var o = new List<Object>(){
                                    containerB,
                                    AppModel.Instance,
                                    build,
                                    overlay,
                                    p
                                    };
                                    stack.GestureRecognizers.Clear();

                                    if (p.muelltoid > 0 && build != null)
                                    {
                                        stack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<Object>(OpenDirektbuchenAusPlanliste), CommandParameter = o });
                                    }
                                    else
                                    {
                                        stack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<Object>(BuildingWSO.ShowOrderContainer), CommandParameter = o });
                                    }
                                    frame_planListAc.Children.Add(containerA);
                                });
                            }
                            ii++;
                        }

                        // Kategorien (Nach Bedarf)

                        List<List<PlanPersonMobile>> lk = new List<List<PlanPersonMobile>>();
                        List<List<PlanPersonMobile>> lkWinter = new List<List<PlanPersonMobile>>();
                        List<PlanPersonMobile> lkall = PlanResp.planweek.days[7].Where(lkw => lkw.winterservice == 0).ToList();
                        List<PlanPersonMobile> lkallWinter = PlanResp.planweek.days[7].Where(lkw => lkw.winterservice == 1).ToList();
                        Dictionary<string, string> kats = new Dictionary<string, string>();
                        Dictionary<string, string> katsWinter = new Dictionary<string, string>();
                        lkall.ForEach(k =>
                        {
                            if (!kats.ContainsKey(k.katname)) kats.Add(k.katname, k.katname);
                        });
                        lkallWinter.ForEach(k =>
                        {
                            if (!katsWinter.ContainsKey(k.katname)) katsWinter.Add(k.katname, k.katname);
                        });
                        kats.OrderBy(kn => kn.Value).ToList().ForEach(ka =>
                        {
                            List<PlanPersonMobile> lka = new List<PlanPersonMobile>();
                            lkall.ForEach(k =>
                            {
                                if (ka.Value == k.katname) { lka.Add(k); }
                            });
                            lk.Add(lka);
                        });
                        katsWinter.OrderBy(kn => kn.Value).ToList().ForEach(ka =>
                        {
                            List<PlanPersonMobile> lka = new List<PlanPersonMobile>();
                            lkallWinter.ForEach(k =>
                            {
                                if (ka.Value == k.katname) { lka.Add(k); }
                            });
                            lkWinter.Add(lka);
                        });


                        int kza = 0;
                        lkWinter.ForEach(li =>
                        {
                            int zx = 0;
                            li.ForEach(p =>
                            {
                                zx++;
                                bool heute = false;
                                if (!String.IsNullOrWhiteSpace(p.lastwork))
                                {
                                    heute = Utils.StringDateToDateTime(p.lastwork).ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd");
                                }
                                if (!heute)
                                {
                                    zx++;
                                }
                            });
                            bool showKat = zx != li.Count;
                            var stdayB = new StackLayout()
                            {
                                Padding = new Thickness(5),
                                Margin = new Thickness(0),
                                Spacing = 0,
                                Orientation = StackOrientation.Horizontal,
                                HorizontalOptions = LayoutOptions.Fill,
                                BackgroundColor = Colors.Transparent,
                                Children = {
                                new Image
                                {
                                    Margin = new Thickness(0,0,0,0),
                                    HeightRequest = 22,
                                    WidthRequest = 22,
                                    Source = "win_26_img.png",
                                    HorizontalOptions = LayoutOptions.Start,
                                    VerticalOptions = LayoutOptions.End,
                                },
                                new Label()
                                {
                                    Text = lkWinter[kza][0].katname,
                                    TextColor = Color.FromArgb("#ffcc00"),
                                    Margin = new Thickness(4, 10, 5, 1),
                                    FontSize = 18,
                                    HorizontalOptions = LayoutOptions.Start,
                                    VerticalOptions = LayoutOptions.Center,
                                    LineBreakMode = LineBreakMode.WordWrap,
                                }
                                }
                            };
                            if (showKat)
                            {
                                frame_planListBoffen.Children.Add(stdayB);
                            }
                            int z = 0;
                            li.ForEach(p =>
                            {
                                z++;
                                bool heute = false;
                                if (!String.IsNullOrWhiteSpace(p.lastwork))
                                {
                                    var last = Utils.StringDateToDateTime(p.lastwork);
                                    heute = last.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd");
                                }
                                var build = AppModel.Instance.AllBuildings.Find(ob => ob.id == p.objektid);
                                var o = new List<Object>(){
                                    AppModel.Instance,
                                    build,
                                    overlay,
                                    p
                                    };
                                var stack = ObjektPlanWeekMobile.GetPlanedOptListWinter(p, o, heute);
                                stack.GestureRecognizers.Clear();
                                stack.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<Object>(OpenDirektbuchenWinterAusPlanliste), CommandParameter = o });
                                if (!heute)
                                {
                                    frame_planListBoffen.Children.Add(stack);
                                }
                                else
                                {
                                    frame_planListBerl.Children.Add(stack);
                                }
                            });
                            kza++;
                        });



                        int kz = 0;
                        lk.ForEach(li =>
                        {
                            var stdayB = new StackLayout()
                            {
                                Padding = new Thickness(5),
                                Margin = new Thickness(0),
                                Spacing = 0,
                                Orientation = StackOrientation.Horizontal,
                                HorizontalOptions = LayoutOptions.Fill,
                                BackgroundColor = Colors.Transparent,
                                Children = {
                                new Label()
                                {
                                    Text = lk[kz][0].katname,
                                    TextColor = Color.FromArgb("#ffcc00"),
                                    Margin = new Thickness(5, 10, 5, 1),
                                    FontSize = 18,
                                    HorizontalOptions = LayoutOptions.Start,
                                    LineBreakMode = LineBreakMode.WordWrap,
                                }
                                }
                            };

                            frame_planListBoffen.Children.Add(stdayB);

                            int z = 0;
                            li.ForEach(p =>
                            {
                                z++;
                                if (String.IsNullOrWhiteSpace(p.lastwork))
                                {
                                    //var build = AppModel.Instance.AllBuildings.Find(ob => ob.id == p.objektid);
                                    var stack = ObjektPlanWeekMobile.GetPlanedOptList(p, false);
                                    var containerA = new StackLayout
                                    {
                                        Padding = new Thickness(0),
                                        Margin = new Thickness(0),
                                        Spacing = 0,
                                        Orientation = StackOrientation.Vertical,
                                        HorizontalOptions = LayoutOptions.Fill,
                                        Children = { stack },
                                        IsVisible = true
                                    };
                                    var containerB = new StackLayout
                                    {
                                        Padding = new Thickness(0),
                                        Margin = new Thickness(0),
                                        Spacing = 0,
                                        Orientation = StackOrientation.Vertical,
                                        HorizontalOptions = LayoutOptions.Fill,
                                        IsVisible = false
                                    };
                                    containerA.Children.Add(containerB);
                                    frame_planListBoffen.Children.Add(containerA);
                                }
                                else
                                {
                                    var last = Utils.StringDateToDateTime(p.lastwork);
                                    var heute = last.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd");
                                    if (!heute) { frame_planListBoffen.Children.Add(ObjektPlanWeekMobile.GetPlanedOptList(p, false)); }
                                }
                            });
                            li.ForEach(p =>
                            {
                                if (!String.IsNullOrWhiteSpace(p.lastwork))
                                {
                                    var last = Utils.StringDateToDateTime(p.lastwork);
                                    var heute = last.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd");
                                    if (heute) { frame_planListBoffen.Children.Add(ObjektPlanWeekMobile.GetPlanedOptList(p, true)); }
                                }
                            });
                            kz++;
                        });

                        frame_planConB_offen.Text = frame_planListBoffen.Children.Count.ToString();
                        frame_planConB_offen_count_con.IsVisible = frame_planListBoffen.Children.Count > 0;
                        frame_planConB_erl.Text = frame_planListBerl.Children.Count.ToString();
                        frame_planConB_erl_count_con.IsVisible = frame_planListBerl.Children.Count > 0;

                    }


                    //await Task.Delay(1);
                    //frame_planListAb.WidthRequest = tabContentWidth;
                    //frame_planListAc.WidthRequest = tabContentWidth;
                    //frame_planListA.WidthRequest = tabContentWidth;
                }

                ////frame_planListA.IsVisible = true;
                //frame_planListAb.WidthRequest = tabContentWidth;
                //frame_planListAc.WidthRequest = tabContentWidth;
                //frame_planListA.WidthRequest = tabContentWidth;
                //await Task.Delay(1);
                overlay.IsVisible = false;

            }
            catch (Exception ex)
            {
                overlay.IsVisible = false;
                AppModel.Logger.Warn("ERROR: (MainPage.cs(Update_PlanTabs)) - " + ex.Message + " --- " + ex.StackTrace != null ? ex.StackTrace : "");
            }
        }



        private PlanPersonMobile selectedDirektbuchenWinterObj = null;
        private List<AuftragWSO> selectedDirektbuchenWinterObjAuftraege = null;

        //Winterdienste OpenDialog
        public List<BemerkungWSO> winterBemerkungen = new List<BemerkungWSO>();
        public List<IntBemerkungWSOPair> _SelectedBemerkungForNoticeList_DirektPos = new List<IntBemerkungWSOPair>();
        public async void OpenDirektbuchenWinterAusPlanliste(Object value)
        {
            _SelectedBemerkungForNoticeList_DirektPos = new List<IntBemerkungWSOPair>();
            winterBemerkungen = new List<BemerkungWSO>();
            btn_quest_direktbuchen_cancel.IsVisible = false;
            btn_quest_direktbuchenwinter_cancel.IsVisible = true;
            //var AppModel.Instance = ((value as List<Object>)[0] as AppModel);
            //var list = ((value as List<Object>)[1] as BuildingWSO).ArrayOfAuftrag;
            var p = ((value as List<Object>)[3] as PlanPersonMobile);
            var obj = ((value as List<Object>)[1] as BuildingWSO);
            List<AuftragWSO> list = null;
            if (obj != null)
            {
                list = obj.ArrayOfAuftrag.FindAll(_ => _.id == p.auftragid);
                //list = obj.ArrayOfAuftrag != null ? obj.ArrayOfAuftrag : new List<AuftragWSO>();
            }
            var overlay = ((value as List<Object>)[2] as AbsoluteLayout);
            selectedDirektbuchenWinterObj = p;
            selectedDirektbuchenWinterObjAuftraege = list;

            overlay.IsVisible = true;
            await Task.Delay(1);

            double w = screenWidthDp;
            double h = screenHeightDp;
            popupContainer_quest_direktbuchen.IsVisible = true;
            popupContainer_quest_direktbuchen_st.WidthRequest = w - 20;
            popupContainer_quest_direktbuchen_st.HeightRequest = h - 120;

            btn_quest_direktbuchen_pos.Children.Clear();

            bool buildListToShow = false;

            list.ForEach(order =>
            {
                order.kategorien.ForEach(k =>
                {
                    k.leistungen.ForEach(l =>
                    {
                        if (k.winterservice > 0)
                        {
                            _SelectedBemerkungForNoticeList_DirektPos.Add(new IntBemerkungWSOPair { id = l.id, lei = l, count = 0 });
                        }
                    });
                });
                bool isWinterKat = false;
                order.kategorien.ForEach(k =>
                {
                    if (k.winterservice > 0 && !isWinterKat)
                    {
                        isWinterKat = true;
                    }
                });
                if (isWinterKat)
                {
                    btn_quest_direktbuchen_pos.Children.Add(
                        ObjektPlanWeekMobile.GetOptWinterCheckItemHeadItem(
                            value, order, btn_quest_direktbuchenwinter, btn_quest_direktbuchen_i,
                            new Command<LeistungWSO>(TapNoticeFromPosInWorkDirektPos),
                            _SelectedBemerkungForNoticeList_DirektPos
                            )
                        );
                    buildListToShow = true;
                }
            });
            if (!buildListToShow)
            {
                btn_quest_direktbuchenwinter.IsVisible = false;
                btn_quest_direktbuchen_i.IsVisible = false;
            }

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public void CloseDirektbuchenWinterAusPlanliste()
        {
            if (selectedDirektbuchenWinterObjAuftraege != null && selectedDirektbuchenWinterObjAuftraege.Count > 0)
            {
                selectedDirektbuchenWinterObjAuftraege.ForEach(order =>
                {
                    order.kategorien.ForEach(k =>
                    {
                        k.leistungen.ForEach(l => { l.selected = false; });
                    });
                });
            }
            _SelectedBemerkungForNoticeList_DirektPos = null;
            selectedDirektbuchenWinterObj = null;
            selectedDirektbuchenWinterObjAuftraege = null;
            popupContainer_quest_direktbuchen.IsVisible = false;
            btn_quest_direktbuchen.IsVisible = false;
            btn_quest_direktbuchen_cancel.IsVisible = false;
            btn_quest_direktbuchenwinter.IsVisible = false;
            btn_quest_direktbuchenwinter_cancel.IsVisible = false;
        }
        public async void SaveDirektbuchenWinterAusPlanliste()
        {
            if (!AppModel.Instance.AppControll.showObjektPlans) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);
            //bool ok = true;

            List<LeistungWSO> leisAtWork = new List<LeistungWSO>();
            if (selectedDirektbuchenWinterObjAuftraege != null && selectedDirektbuchenWinterObjAuftraege.Count > 0)
            {
                selectedDirektbuchenWinterObjAuftraege.ForEach(order =>
                {
                    order.kategorien.ForEach(k =>
                    {
                        k.leistungen.ForEach(l =>
                        {
                            if (l.selected)
                            {
                                leisAtWork.Add(l);
                            }
                            l.selected = false;
                        });
                    });
                });
            }

            SaveDirektbuchenWinterAusPlanlisteNow(leisAtWork);

            var lastworker = AppModel.Instance.Person.name + " " + (AppModel.Instance.Person.vorname.Length > 1 ? (AppModel.Instance.Person.vorname.Substring(0, 1) + ".") : AppModel.Instance.Person.vorname);
            selectedDirektbuchenWinterObj.haswork = 1;
            selectedDirektbuchenWinterObj.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
            selectedDirektbuchenWinterObj.lastworker = lastworker;
            AppModel.Instance.PlanResponse.planweek.days = CleanPlanweekList(AppModel.Instance.PlanResponse.planweek.days);
            ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);

            if (AppModel.Instance.PlanResponse.selectedPerson != null)
            {
                ReloadPlanData(0);
            }
            else
            {
                var today = (int)DateTime.Now.DayOfWeek;
                Update_PlanTabs(today);
            }

            //await Task.Delay(1);
            overlay.IsVisible = false;

            CloseDirektbuchenWinterAusPlanliste();
            CheckAllSyncFromUpload(); //SyncPosition();
        }
        private long addTicksWinter = 0;
        public async void SaveDirektbuchenWinterAusPlanlisteNow(List<LeistungWSO> leis)
        {
            if (leis == null || leis.Count == 0) { return; }
            var geo = AppModel.Instance.LocationStr;
            string geoMessage = "";
            if (geo != null && geo.Length > 0)
            {
                geoMessage = geo.Substring(0, 1) == "#" ? geo.Substring(1) : "GPS OK";
                geo = geoMessage == "GPS OK" ? geo : null;
            }
            else
            {
                geo = null;
                geoMessage = "geo = null";
            }
            //AppModel.Logger.Info("Info: --------------- STARTE ARBEITEN => DirektbuchenAusPlanlisteNow");
            //AppModel.Logger.Info("Info: Verwendete GPS (" + geoMessage + " - " + AppModel.Instance.LocationStr + ")");

            var latin = geo != null ? geo.Split(';')[0] : "";
            var lonin = geo != null ? (geo.Split(';').Length > 0 ? geo.Split(';')[1] : "") : "";


            long maxEndMin = 0;
            List<LeistungInWorkWSO> leisInWork = new List<LeistungInWorkWSO>();
            leis.ForEach(liw =>
            {
                maxEndMin = Math.Max(maxEndMin, ((liw.dstd * 60) + liw.dmin));
                liw.leiInWork.ppm = selectedDirektbuchenWinterObj;//PPM
                liw.leiInWork.winterservice = 1;
                if (_SelectedBemerkungForNoticeList_DirektPos != null)
                {
                    foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                    {
                        if (item.id == liw.id && item.bem != null)
                        {
                            if (liw.leiInWork.bemerkungen == null) { liw.leiInWork.bemerkungen = new List<BemerkungWSO>(); }
                            liw.leiInWork.bemerkungen.Add(item.bem);
                        }
                    }
                }
                leisInWork.Add(liw.leiInWork);
            });
            var start = DateTime.Now.AddTicks(addTicksWinter);
            var end = start.AddMinutes(maxEndMin);

            AppModel.Instance.allPositionDirectWork = new LeistungPackWSO
            {
                latin = latin,
                lonin = lonin,
                messagein = geoMessage,
                latout = "",
                lonout = "",
                messageout = "",
                preview = false,
                status = 2,   // 0 = in Arbeit , 1 = Ausgesetzt , 2 = Fertig
                startticks = start.Ticks,
                endticks = end.Ticks,
                personid = AppModel.Instance.Person.id,
                diffObjekt = 2,// Direktbuchung
                leistungen = leisInWork,
                winterservice = 1,
            };
            AppModel.Instance.allPositionDirectWork.endticks = AppModel.Instance.allPositionDirectWork.startticks;
            addTicksWinter++;

            var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(AppModel.Instance.allPositionDirectWork.startticks), -2);
            var building = BuildingWSO.LoadBuilding(AppModel.Instance, leis[0].objektid);
            building.ArrayOfAuftrag.ForEach(o =>
            {
                o.kategorien.ForEach(c =>
                {
                    c.leistungen.ForEach(le =>
                    {
                        var foundPos = AppModel.Instance.allPositionDirectWork.leistungen.Find(lei => lei.id == le.id);
                        if (foundPos != null)
                        {
                            foundPos.lastwork = lastWorkTicks;
                            foundPos.workat = "";
                            le.lastwork = lastWorkTicks;
                            le.workat = "";
                            le.selected = false;
                            if (le.muell == 1 && le.inout != null)
                            {
                                le.inout.inout = le.inout.inout == 1 ? 0 : 1;   // 1 = rausgestellt / 0 = drinne
                            }
                        }
                    });
                });
            });
            BuildingWSO.Save(AppModel.Instance, building);

            LeistungPackWSO.ToUploadStack(AppModel.Instance, AppModel.Instance.allPositionDirectWork);

            AppModel.Instance.allPositionDirectWork = null;
            await Task.Delay(1);
        }





        private PlanPersonMobile selectedDirektbuchenObj = null;
        public async void OpenDirektbuchenAusPlanliste(Object value)
        {
            _SelectedBemerkungForNoticeList_DirektPos = new List<IntBemerkungWSOPair>();
            btn_quest_direktbuchen_cancel.IsVisible = true;
            btn_quest_direktbuchenwinter_cancel.IsVisible = false;
            var stack = ((value as List<Object>)[0] as VerticalStackLayout);
            //var AppModel.Instance = ((value as List<Object>)[1] as AppModel);
            var obj = ((value as List<Object>)[2] as BuildingWSO);
            List<AuftragWSO> alist = null;
            if (obj != null && obj.ArrayOfAuftrag != null && obj.ArrayOfAuftrag.Count > 0)
            {
                alist = obj.ArrayOfAuftrag;
            }
            var overlay = ((value as List<Object>)[3] as AbsoluteLayout);
            var p = ((value as List<Object>)[4] as PlanPersonMobile);
            selectedDirektbuchenObj = p;

            overlay.IsVisible = true;
            await Task.Delay(1);

            double w = screenWidthDp;
            double h = screenHeightDp;
            popupContainer_quest_direktbuchen.IsVisible = true;
            popupContainer_quest_direktbuchen_st.WidthRequest = w - 20;
            popupContainer_quest_direktbuchen_st.HeightRequest = h - 120;

            btn_quest_direktbuchen_pos.Children.Clear();

            int mobileCount = 0;
            if (p.more != null && p.more.Count > 0)
            {
                p.more.ForEach(pp =>
                {
                    if (pp.mobil == 1)
                    {
                        mobileCount++;
                    }
                });
                bool sel = mobileCount == 1;
                btn_quest_direktbuchen.IsVisible = sel;
                btn_quest_direktbuchen_i.IsVisible = !sel;
                p.more.ForEach(pp =>
                {
                    if (pp.mobil == 1)
                    {
                        LeistungWSO lei = null;
                        var ibwp = new IntBemerkungWSOPair();
                        try
                        {
                            Int32 leiId = Int32.Parse(pp.info.Split('#')[3]);
                            alist.ForEach(_a =>
                            {
                                _a.kategorien.ForEach(_k =>
                                {
                                    _k.leistungen.ForEach(_l =>
                                    {
                                        if (_l.id == leiId && lei == null)
                                        {
                                            lei = _l;
                                        }
                                    });
                                });
                            });
                            ibwp.lei = lei;
                            ibwp.id = leiId;
                            ibwp.count = 0;
                            _SelectedBemerkungForNoticeList_DirektPos.Add(ibwp);
                        }
                        catch (Exception)
                        {
                            ibwp = null;
                        }
                        pp.isSelected = sel;
                        btn_quest_direktbuchen_pos.Children.Add(
                            ObjektPlanWeekMobile.GetPlanedTodayCheckItem(sel, pp, btn_quest_direktbuchen, btn_quest_direktbuchen_i, selectedDirektbuchenObj,
                                new Command<LeistungWSO>(TapNoticeFromPosInWorkDirektPosMuell),
                                ibwp));
                    }
                    else
                    {
                        btn_quest_direktbuchen_pos.Children.Add(ObjektPlanWeekMobile.GetPlanedTodayNotMobileItem(pp));
                    }
                });
            }
            if (mobileCount == 0)
            {
                btn_quest_direktbuchen.IsVisible = false;
                btn_quest_direktbuchen_i.IsVisible = false;
            }

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public void CloseDirektbuchenAusPlanliste()
        {
            if (selectedDirektbuchenObj != null && selectedDirektbuchenObj.more != null && selectedDirektbuchenObj.more.Count > 0)
            {
                selectedDirektbuchenObj.more.ForEach(o => { o.isSelected = false; });
            }
            selectedDirektbuchenObj = null;
            popupContainer_quest_direktbuchen.IsVisible = false;
            btn_quest_direktbuchen.IsVisible = false;
            btn_quest_direktbuchen_cancel.IsVisible = false;
            btn_quest_direktbuchenwinter.IsVisible = false;
            btn_quest_direktbuchenwinter_cancel.IsVisible = false;
        }
        public async void SaveDirektbuchenAusPlanliste()
        {
            if (!AppModel.Instance.AppControll.showObjektPlans) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);
            bool ok = true;

            List<PlanPersonMobile> ppms = new List<PlanPersonMobile>();
            List<LeistungInWorkWSO> leisIW = new List<LeistungInWorkWSO>();
            List<LeistungWSO> leiss = new List<LeistungWSO>();

            if (selectedDirektbuchenObj != null && selectedDirektbuchenObj.more != null && selectedDirektbuchenObj.more.Count > 0)
            {
                try
                {
                    selectedDirektbuchenObj.more.ForEach(pp =>
                    {
                        if (pp.isSelected)
                        {
                            if (!String.IsNullOrWhiteSpace(pp.info))
                            {
                                string[] all = pp.info.Split('#');
                                var lei = BuildingWSO.FindLeistung(Int32.Parse(all[3]));
                                var leiIW = LeistungInWorkWSO.ConvertLeistungTo(lei);
                                leiIW.ppm = pp;
                                leiIW.ppm.leiid = lei.id;
                                leisIW.Add(leiIW);
                                leiss.Add(lei);
                            }
                            ppms.Add(pp);
                        }
                    });
                    SaveDirektbuchenAusPlanlisteNow(leiss, leisIW);
                }
                catch (Exception) { ok = false; }
                if (ok)
                {
                    AppModel.Instance.PlanResponse.planweek.days = CleanPlanweekList(AppModel.Instance.PlanResponse.planweek.days);
                    ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                    if (AppModel.Instance.PlanResponse.selectedPerson != null)
                    {
                        ReloadPlanData(0);
                    }
                    else
                    {
                        var today = (int)DateTime.Now.DayOfWeek;
                        Update_PlanTabs(today);
                    }
                    CheckAllSyncFromUpload(); //SyncPosition();
                }
            }

            await Task.Delay(1);
            overlay.IsVisible = false;
            CloseDirektbuchenAusPlanliste();
        }


        private long addTicks = 0;

        //public async void SaveDirektbuchenAusPlanlisteNow(List<LeistungInWorkWSO> leis)
        public async void SaveDirektbuchenAusPlanlisteNow(List<LeistungWSO> leiss, List<LeistungInWorkWSO> leisIW)
        {
            try
            {
                // Validate inputs
                if (leiss == null || leiss.Count == 0)
                {
                    AppModel.Logger?.Warn("SaveDirektbuchenAusPlanlisteNow: leiss is null or empty");
                    return;
                }

                if (leisIW == null || leisIW.Count == 0)
                {
                    AppModel.Logger?.Warn("SaveDirektbuchenAusPlanlisteNow: leisIW is null or empty");
                    return;
                }

                if (AppModel.Instance == null)
                {
                    AppModel.Logger?.Error("SaveDirektbuchenAusPlanlisteNow: AppModel.Instance is null");
                    return;
                }

                var geo = AppModel.Instance.LocationStr;
                string geoMessage = "";
                if (geo != null && geo.Length > 0)
                {
                    geoMessage = geo.Substring(0, 1) == "#" ? geo.Substring(1) : "GPS OK";
                    geo = geoMessage == "GPS OK" ? geo : null;
                }
                else
                {
                    geo = null;
                    geoMessage = "geo = null";
                }
                //AppModel.Logger.Info("Info: --------------- STARTE ARBEITEN => DirektbuchenAusPlanlisteNow");
                //AppModel.Logger.Info("Info: Verwendete GPS (" + geoMessage + " - " + AppModel.Instance.LocationStr + ")");

                var latin = geo != null ? geo.Split(';')[0] : "";
                var lonin = geo != null ? (geo.Split(';').Length > 0 ? geo.Split(';')[1] : "") : "";



                long maxEndMin = 0;
                List<LeistungInWorkWSO> leisInWork = new List<LeistungInWorkWSO>();
                leiss.ForEach(lei =>
                {
                    if (lei == null)
                    {
                        AppModel.Logger?.Warn("SaveDirektbuchenAusPlanlisteNow: Skipping null lei in leiss");
                        return;
                    }

                    var leiIW = leisIW.FindAll(_ => _ != null && _.ppm != null && _.ppm.leiid == lei.id).FirstOrDefault();

                    if (leiIW == null)
                    {
                        AppModel.Logger?.Warn($"SaveDirektbuchenAusPlanlisteNow: No matching leisIW found for lei.id={lei.id}");
                        return;
                    }

                    lei.leiInWork = leiIW;
                    //lei.leiInWork.ppm = selectedDirektbuchenObj;//PPM

                    maxEndMin = Math.Max(maxEndMin, ((lei.dstd * 60) + lei.dmin));

                    if (_SelectedBemerkungForNoticeList_DirektPos != null)
                    {
                        foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                        {
                            if (item != null && item.id == lei.id && item.bem != null)
                            {
                                if (lei.leiInWork.bemerkungen == null) 
                                { 
                                    lei.leiInWork.bemerkungen = new List<BemerkungWSO>(); 
                                }
                                lei.leiInWork.bemerkungen.Add(item.bem);
                            }
                        }
                    }
                    leisInWork.Add(lei.leiInWork);
                });

                if (leisInWork.Count == 0)
                {
                    AppModel.Logger?.Warn("SaveDirektbuchenAusPlanlisteNow: No valid leisInWork items created");
                    return;
                }

                var start = DateTime.Now.AddTicks(addTicksWinter);
                var end = start.AddMinutes(maxEndMin);

                try
                {
                    if (_SelectedBemerkungForNoticeList_DirektPos != null && selectedDirektbuchenObj?.more != null)
                    {
                        foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                        {
                            if (item == null || item.bem == null)
                                continue;

                            var found = leiss.Find(_ => _ != null && _.id == item.id);
                            var ppm = selectedDirektbuchenObj.more.FindAll(pp => pp != null && pp.info != null && item.id.ToString() == pp.info.Split('#')[3]).FirstOrDefault();

                            if (ppm != null && item.bem != null && found == null && item.lei != null)
                            {
                                // Bemrkung zur Müllpos - JEDOCh nicht selektiert !!!!
                                /* Nur Bemerkung zum Objekt erstellen*/
                                item.bem.auftragid = 0;
                                item.bem.leistungid = 0;
                                item.bem.text = "LEISTUNG: " + item.lei.beschreibung + " \r\nBEMERKUNG: " + item.bem.text;
                                BemerkungWSO.ToUploadStack(AppModel.Instance, item.bem);
                                //btn_NoticeSaveForOnlyObjektOnlyMuellPosThatNotSelected(ppm, item);
                            }
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    AppModel.Logger?.Warn($"SaveDirektbuchenAusPlanlisteNow: Error processing bemerkungen: {ex.Message}");
                }


                AppModel.Instance.allPositionDirectWork = new LeistungPackWSO
                {
                    latin = latin,
                    lonin = lonin,
                    messagein = geoMessage,
                    latout = "",
                    lonout = "",
                    messageout = "",
                    preview = false,
                    status = 2,   // 0 = in Arbeit , 1 = Ausgesetzt , 2 = Fertig
                    startticks = DateTime.Now.Ticks + addTicks,
                    endticks = DateTime.Now.Ticks + addTicks,
                    personid = AppModel.Instance.Person.id,
                    diffObjekt = 2,// Direktbuchung
                    leistungen = leisInWork // leisIW
                };
            AppModel.Instance.allPositionDirectWork.endticks = AppModel.Instance.allPositionDirectWork.startticks;
            addTicks++;

            var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(AppModel.Instance.allPositionDirectWork.startticks), -2);
            var building = BuildingWSO.LoadBuilding(AppModel.Instance, leisIW[0].objektid);

            if (building == null)
            {
                AppModel.Logger?.Error($"SaveDirektbuchenAusPlanlisteNow: Failed to load building for objektid={leisIW[0].objektid}");
                // Still upload the work package even if building load failed
                LeistungPackWSO.ToUploadStack(AppModel.Instance, AppModel.Instance.allPositionDirectWork);
            }
            else
            {
                building.ArrayOfAuftrag?.ForEach(o =>
                {
                    o?.kategorien?.ForEach(c =>
                    {
                        c?.leistungen?.ForEach(le =>
                        {
                            if (le == null) return;

                            var foundPos = AppModel.Instance.allPositionDirectWork.leistungen.Find(lei => lei != null && lei.id == le.id);
                            if (foundPos != null)
                            {
                                foundPos.lastwork = lastWorkTicks;
                                foundPos.workat = "";
                                le.lastwork = lastWorkTicks;
                                le.workat = "";
                                le.selected = false;
                                if (le.muell == 1 && le.inout != null)
                                {
                                    le.inout.inout = le.inout.inout == 1 ? 0 : 1;   // 1 = rausgestellt / 0 = drinne
                                }
                            }
                        });
                    });
                });
                BuildingWSO.Save(AppModel.Instance, building);
                //AppModel.Instance.allPositionDirectWork.leistungen = null;
                LeistungPackWSO.ToUploadStack(AppModel.Instance, AppModel.Instance.allPositionDirectWork);
            }

            leisIW.ForEach(l =>
            {
                if (l == null) return;

                int haswork = 1;
                if (l.ppm != null && !String.IsNullOrWhiteSpace(l.ppm.info))
                {
                    string[] all = l.ppm.info.Split('#');
                    if (all.Length >= 4)
                    {
                        string name = all[0];
                        string col = all[1];
                        string statem = all[2];
                        string leiid = all[3];
                        if (statem == "3")
                        {
                            statem = "2";
                            haswork = 0;
                            l.ppm.info = name + "#" + col + "#2#" + leiid;
                        }
                    }
                }

                if (AppModel.Instance?.Person != null)
                {
                    var lastworker = AppModel.Instance.Person.name + " " + (AppModel.Instance.Person.vorname.Length > 1 ? (AppModel.Instance.Person.vorname.Substring(0, 1) + ".") : AppModel.Instance.Person.vorname);
                    if (l.ppm != null)
                    {
                        l.ppm.haswork = haswork;
                        l.ppm.lastwork = new DateTime(AppModel.Instance.allPositionDirectWork.endticks).ToString("dd.MM.yyyy - HH:mm");
                        l.ppm.lastworker = lastworker;
                    }
                }
            });
            AppModel.Instance.allPositionDirectWork = null;
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            AppModel.Logger?.Error(ex, "ERROR: SaveDirektbuchenAusPlanlisteNow - Unexpected error");

            // Clean up in case of error
            if (AppModel.Instance != null)
            {
                AppModel.Instance.allPositionDirectWork = null;
            }
        }
    }

        public async void btn_NoticeSaveForOnlyObjektOnlyMuellPosThatNotSelected(PlanPersonMobile ppm, IntBemerkungWSOPair ibwp)
        {
            //this.Focus();
            //if (!String.IsNullOrWhiteSpace(_SelectedBemerkungForNotice.text.Trim()) || (_SelectedBemerkungForNotice.photos != null && _SelectedBemerkungForNotice.photos.Count > 0))
            //{
            //    overlay.IsVisible = true;
            //    await Task.Delay(1);

            //    int am = sw_alertmessage.IsToggled ? 2 : 0;
            //    int im = sw_internmessage.IsToggled ? 1 : 0;
            //    _SelectedBemerkungForNotice.prio = (am + im);
            //    _SelectedBemerkungForNotice.gruppeid = AppModel.Instance.LastBuilding.gruppeid;
            //    _SelectedBemerkungForNotice.personid = AppModel.Instance.Person.id;
            //    _SelectedBemerkungForNotice.objektid = AppModel.Instance.LastBuilding.id;
            //    _SelectedBemerkungForNotice.leistungid = 0;
            //    _SelectedBemerkungForNotice.datum = DateTime.Now.Ticks;

            //    if (_SelectedPosForNotice != null)
            //    {
            //        var posInWork = AppModel.Instance.allPositionInWork.leistungen.Find(pos => pos.id == _SelectedPosForNotice.id);
            //        if (posInWork.bemerkungen == null) { posInWork.bemerkungen = new List<BemerkungWSO>(); }
            //        _SelectedBemerkungForNotice.leistungid = _SelectedPosForNotice.id;
            //        posInWork.bemerkungen.Add(_SelectedBemerkungForNotice);
            //        LeistungPackWSO.Save(AppModel.Instance, AppModel.Instance.allPositionInWork);
            //        //LeistungPackWSO.Load(AppModel.Instance);
            //    }
            //    else
            //    {
            //Task.Run(() => { 
            //SyncSingleNotice();
            //}).ConfigureAwait(false);   // Im Hintergrund ausführen
            //    }


            //    await Task.Delay(1);

            //    _SelectedPosForNotice = null;
            //    _SelectedBemerkungForNotice = null;
            //    _BackToFromNotice = null;
            //    entry_notice.Text = "";
            //    noticePhotoStack.Children.Clear();

            //    await Task.Delay(1);
            //    overlay.IsVisible = false;

            //    if (_BackToFromNotice != null && _BackToFromNotice == "inwork")
            //    {
            //        ShowRunningWorksView();
            //    }
            //    else
            //    {
            //        ShowMainPage();
            //    }
            //}
        }




        public List<List<PlanPersonMobile>> CleanPlanweekList(List<List<PlanPersonMobile>> weekDays)
        {
            var days = new List<List<PlanPersonMobile>> {
                        new List<PlanPersonMobile>(),//Sonntag
                        new List<PlanPersonMobile>(),//Montag
                        new List<PlanPersonMobile>(),//Dienstag
                        new List<PlanPersonMobile>(),//Mittwoch
                        new List<PlanPersonMobile>(),//Donnerstag
                        new List<PlanPersonMobile>(),//Freitag
                        new List<PlanPersonMobile>(),//Samstag
                        new List<PlanPersonMobile>(),////Bedarf
                    };
            int i = 0;
            weekDays.ForEach(day =>
            {
                var newday = new List<PlanPersonMobile>();
                day.ForEach(items =>
                {
                    items.more.ForEach(item =>
                    {
                        var fItem = newday.Find(f => f.objektid == item.objektid && f.haswork == 0 && f.more.Count > 0);
                        if (fItem != null && item.haswork == 0)
                        {
                            fItem.more.Add(PlanPersonMobile.ToNewPlanPersonMobile(item));
                        }
                        else
                        {
                            if (item.haswork >= 0)
                            {
                                item.more.Add(PlanPersonMobile.ToNewPlanPersonMobile(item));
                            }
                            newday.Add(item);
                        }
                    });
                });
                days[i] = newday;
                i++;
            });
            return days;
        }


        public async void btn_PlanTabATapped(object sender, EventArgs e)
        {
            frame_plantabA.Margin = new Thickness(0, -8, 2, 0);
            frame_plantabB.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabCe.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabC.Margin = new Thickness(0, 0, 2, 0);
            frame_planConA.IsVisible = true;
            frame_planConB.IsVisible = false;
            frame_planConCe.IsVisible = false;
            frame_planConC.IsVisible = false;
            if (sender != null)
            {
                //Load_PlanTabs((int)DateTime.Now.DayOfWeek);
            }
        }
        public void btn_PlanTabBTapped(object sender, EventArgs e)
        {
            frame_plantabB.Margin = new Thickness(0, -8, 2, 0);
            frame_plantabA.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabCe.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabC.Margin = new Thickness(0, 0, 2, 0);
            frame_planConA.IsVisible = false;
            frame_planConB.IsVisible = true;
            frame_planConCe.IsVisible = false;
            frame_planConC.IsVisible = false;
        }
        public async void btn_PlanTabCTapped(object sender, EventArgs e)
        {
            frame_plantabC.Margin = new Thickness(0, -8, 2, 0);
            frame_plantabA.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabB.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabCe.Margin = new Thickness(0, 0, 2, 0);
            frame_planConA.IsVisible = false;
            frame_planConB.IsVisible = false;
            frame_planConCe.IsVisible = false;
            frame_planConC.IsVisible = true;

            // Tickets laden und anzeigen
            await LoadAndDisplayTicketsAsync();
        }

        public async void touch_ReloadTickets(object o, EventArgs e)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            await Ticket.LoadTicketsFromBackendAsync();
            await LoadAndDisplayTicketsAsync();

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        private async Task LoadAndDisplayTicketsAsync()
        {
            try
            {
                // Zeige Ladeindikator
                overlay.IsVisible = true;
                if (AppModel.Instance.TicketResponse != null)
                {
                    // Versuche Tickets zu laden (Implementierung kann später erfolgen)
                    //await Ticket.LoadTicketsFromBackendAsync();

                    // Falls keine Tickets vorhanden, zeige leere Listen und Badge = 0
                    if (AppModel.Instance.TicketResponse.tickets == null || AppModel.Instance.TicketResponse.tickets.Count == 0)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            RenderTicketsInFrame(frame_planListCoffen, new List<Ticket>());
                            RenderTicketsInFrame(frame_planListCwork, new List<Ticket>());
                            RenderTicketsInFrame(frame_planListCerl, new List<Ticket>());
                        });
                        return;
                    }

                    // Nach Status gruppieren
                    var offeneTickets = AppModel.Instance.TicketResponse.tickets.Where(t =>
                        t.status == (int)Ticket.TicketStatus.Neu ||
                        t.status == (int)Ticket.TicketStatus.Offen ||
                        t.status == (int)Ticket.TicketStatus.Wartend
                    ).ToList();

                    var inArbeitTickets = AppModel.Instance.TicketResponse.tickets.Where(t =>
                        t.status == (int)Ticket.TicketStatus.InArbeit ||
                        (t.status == (int)Ticket.TicketStatus.Rueckfrage &&
                         t.besitzerstatus == (int)Ticket.BesitzerStatus.Gestartet)
                    ).ToList();

                    // Erledigte der letzten Woche
                    var oneWeekAgo = DateTime.Now.AddDays(-7);
                    var erledigteTickets = AppModel.Instance.TicketResponse.tickets.Where(t =>
                    {
                        if (t.status != (int)Ticket.TicketStatus.Erledigt)
                            return false;

                        // updateat ist string im Format "yyyy-MM-dd HH:mm:ss"
                        if (DateTime.TryParse(t.updateat, out DateTime updateDate))
                        {
                            return updateDate >= oneWeekAgo;
                        }
                        return false;
                    }).ToList();

                    // UI aktualisieren
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        RenderTicketsInFrame(frame_planListCoffen, offeneTickets);
                        RenderTicketsInFrame(frame_planListCwork, inArbeitTickets);
                        RenderTicketsInFrame(frame_planListCerl, erledigteTickets);

                        // Badge-Count als Fallback nur aktualisieren, wenn er noch nicht vom Backend gesetzt wurde
                        // (z.B. wenn response.counts null war)
                        // In der Regel wird der Badge-Count bereits in LoadTicketsFromBackendAsync gesetzt
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Tickets: {ex.Message}");
                AppModel.Logger.Error($"LoadAndDisplayTicketsAsync: {ex.Message}");

                // Bei Fehler: Badge auf 0 setzen
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    UpdateTicketBadgeCount(0);
                });
            }
            finally
            {
                overlay.IsVisible = false;
            }
        }

        /// <summary>
        /// Aktualisiert die Badge-Anzahl für den Ticket-Tab
        /// </summary>
        /// <param name="count">Anzahl der aktiven Tickets (offen + in Arbeit)</param>
        public void UpdateTicketBadgeCount(int count)
        {
            try
            {
                // Badge 1: Offene Tickets mit Prio Gering (0) und Normal (1)
                int normalCount = AppModel.Instance.TicketResponse?.tickets?
                    .Where(t => t.status == 2 && (t.prio == 0 || t.prio == 1))
                    .Count() ?? 0;

                // Badge 2: Offene mit Prio Hoch (2) und Notfall (3+) sowie In Arbeit (Status 4)
                int urgentCount = AppModel.Instance.TicketResponse?.tickets?
                    .Where(t => (t.status == 2 && (t.prio == 2 || t.prio >= 3)) || t.status == 4)
                    .Count() ?? 0;

                // Badge 1 (Normal-Prio) aktualisieren
                if (normalCount > 0)
                {
                    frame_plantabC_badge_count.Text = normalCount > 99 ? "99+" : normalCount.ToString();
                    if (frame_plantabC_badge_count.Parent is VisualElement badgeContainer)
                    {
                        badgeContainer.IsVisible = true;
                    }
                }
                else
                {
                    frame_plantabC_badge_count.Text = "0";
                    if (frame_plantabC_badge_count.Parent is VisualElement badgeContainer)
                    {
                        badgeContainer.IsVisible = false;
                    }
                }

                // Badge 2 (Dringend) aktualisieren
                if (frame_plantabC_badge_count2 != null)
                {
                    if (urgentCount > 0)
                    {
                        frame_plantabC_badge_count2.Text = urgentCount > 99 ? "99+" : urgentCount.ToString();
                        if (frame_plantabC_badge_count2.Parent is VisualElement badgeContainer2)
                        {
                            badgeContainer2.IsVisible = true;
                        }
                    }
                    else
                    {
                        frame_plantabC_badge_count2.Text = "0";
                        if (frame_plantabC_badge_count2.Parent is VisualElement badgeContainer2)
                        {
                            badgeContainer2.IsVisible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Aktualisieren des Ticket-Badges: {ex.Message}");
                AppModel.Logger.Error($"UpdateTicketBadgeCount: {ex.Message}");
            }
        }

        private void RenderTicketsInFrame(VerticalStackLayout container, List<Ticket> tickets)
        {
            container.Children.Clear();

            // Keine Tickets? Infomeldung hinzufügen
            if (tickets == null || tickets.Count == 0)
            {
                container.Children.Add(new Label
                {
                    Text = "Keine Tickets",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#999999"),
                    Margin = new Thickness(3, 5, 0, 1),
                    HorizontalOptions = LayoutOptions.Start
                });
                return;
            }

            // Tickets sortieren (höchste Priorität zuerst, dann Fälligkeitsdatum)
            var sortedTickets = tickets
                .OrderByDescending(t => t.prio) // Höchste Priorität zuerst
                .ThenBy(t =>
                {
                    // Fälligkeitsdatum parsen (end) - JavaScript Timestamps sind in Millisekunden
                    if (long.TryParse(t.end, out long endTimestamp) && endTimestamp > 0)
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(endTimestamp).DateTime;
                    }
                    return DateTime.MaxValue; // Tickets ohne Fälligkeitsdatum ans Ende
                })
                .ToList();

            // Ticket-Cards erstellen
            foreach (var ticket in sortedTickets)
            {
                var ticketCard = CreateTicketCard(ticket);
                container.Children.Add(ticketCard);
            }
        }

        private Border CreateTicketCard(Ticket ticket)
        {
            // Statusfarbe aus ticket.status ermitteln
            string statusColor = GetStatusColor(ticket.status);

            var border = new Border
            {
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(0),
                BackgroundColor = Color.FromArgb("#2a2a2a"),
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#444444"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(5) }
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto }, // Status-Indikator
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Content
                    new ColumnDefinition { Width = GridLength.Auto } // ID Badge
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }, // Titel
                    new RowDefinition { Height = GridLength.Auto }, // Beschreibung (optional)
                    new RowDefinition { Height = GridLength.Auto }, // Info (Zeit, Prio, Chat)
                    new RowDefinition { Height = GridLength.Auto }  // Status + Start/End
                }
            };

            // Status-Indikator (genau am linken Rand, 6px breit, volle Höhe, abgerundete Ecken)
            var statusIndicator = new Border
            {
                BackgroundColor = Color.FromArgb(statusColor),
                WidthRequest = 6,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle 
                { 
                    CornerRadius = new CornerRadius(5, 0, 0, 5) // Nur linke Ecken abgerundet
                }
            };
            grid.Add(statusIndicator, 0, 0);
            Grid.SetRowSpan(statusIndicator, 4); // Über alle 4 Zeilen

            // Titel-Zeile mit ID vorne
            var titleStack = new HorizontalStackLayout
            {
                Margin = new Thickness(12, 8, 8, 2),
                Spacing = 8,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            // Ticket-ID Badge (blauer abgerundeter Hintergrund vor dem Titel)
            var idBadge = new Border
            {
                BackgroundColor = Color.FromArgb("#0078d7"),
                Padding = new Thickness(6, 3),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = $"#{ticket.id}",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#ffffff"),
                    FontAttributes = FontAttributes.Bold
                }
            };
            titleStack.Children.Add(idBadge);

            // Titel
            var titleLabel = new Label
            {
                Text = ticket.titel ?? "Ohne Titel",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#ffffff"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                WidthRequest = screenWidthDp - 150,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            titleStack.Children.Add(titleLabel);

            grid.Add(titleStack, 1, 0);

            // Rechte Spalte: Prio oben, Zeit darunter
            var rightStack = new VerticalStackLayout
            {
                Margin = new Thickness(0, 8, 8, 0),
                Spacing = 5,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            };

            // Priorität als Chip darstellen
            if (ticket.prio >= 0)
            {
                string prioText;
                string prioBackgroundColor;
                string prioTextColor;

                if (ticket.prio == 0)
                {
                    prioText = "Gering";
                    prioBackgroundColor = "#4472C4";
                    prioTextColor = "#FFFFFF";
                }
                else if (ticket.prio == 1)
                {
                    prioText = "Normal";
                    prioBackgroundColor = "#009900";
                    prioTextColor = "#ffffff";
                }
                else if (ticket.prio == 2)
                {
                    prioText = "Hoch";
                    prioBackgroundColor = "#aa5500";
                    prioTextColor = "#FFFFFF";
                }
                else
                {
                    prioText = "NOTFALL";
                    prioBackgroundColor = "#990000";
                    prioTextColor = "#FFFFFF";
                }

                var prioChip = new Border
                {
                    BackgroundColor = Color.FromArgb(prioBackgroundColor),
                    Padding = new Thickness(8, 4),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(4) },
                    HorizontalOptions = LayoutOptions.End,
                    Content = new Label
                    {
                        Text = prioText,
                        FontSize = 11,
                        TextColor = Color.FromArgb(prioTextColor),
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };
                rightStack.Children.Add(prioChip);
            }

            // Zeitstempel unter der Prio
            DateTime.TryParse(ticket.updateat, out DateTime updateDate);
            var timeLabel = new Label
            {
                Text = FormatTicketTime(updateDate),
                FontSize = 10,
                TextColor = Color.FromArgb("#888888"),
                HorizontalOptions = LayoutOptions.End
            };
            rightStack.Children.Add(timeLabel);

            grid.Add(rightStack, 2, 0);
            Grid.SetRowSpan(rightStack, 2); // Über 2 Zeilen

            // Beschreibung
            //if (!string.IsNullOrEmpty(ticket.text))
            //{
            //    var descLabel = new Label
            //    {
            //        Text = ticket.text,
            //        FontSize = 12,
            //        TextColor = Color.FromArgb("#cccccc"),
            //        Margin = new Thickness(10, 0, 0, 2),
            //        LineBreakMode = LineBreakMode.TailTruncation,
            //        MaxLines = 2
            //    };
            //    grid.Add(descLabel, 0, 1);
            //    Grid.SetColumnSpan(descLabel, 2);
            //}

            // Chat-Badge (wenn neue Nachrichten vorhanden)
            //int newChatCount = ticket.chats?.Count(c => c.id == ticket.newchat?.id) ?? 0;
            //if (newChatCount > 0)
            //{
            //    var chatBadge = new Border
            //    {
            //        BackgroundColor = Color.FromArgb("#ff0000"),
            //        Padding = new Thickness(6, 2),
            //        Margin = new Thickness(12, 2, 8, 2),
            //        HorizontalOptions = LayoutOptions.Start,
            //        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
            //        Content = new Label
            //        {
            //            Text = $"{newChatCount} neu",
            //            FontSize = 10,
            //            TextColor = Color.FromArgb("#ffffff"),
            //            FontAttributes = FontAttributes.Bold
            //        }
            //    };
            //    grid.Add(chatBadge, 1, 2);
            //}

            // Zeile 3: Status-Chip und Start/End-Zeiten
            var statusAndDatesStack = new HorizontalStackLayout
            {
                Margin = new Thickness(12, 2, 8, 8),
                Spacing = 10
            };

            // Status-Chip links
            var statusChip = new Border
            {
                BackgroundColor = Color.FromArgb(statusColor),
                Padding = new Thickness(8, 4),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(4) },
                Content = new Label
                {
                    Text = GetStatusText(ticket.status),
                    FontSize = 10,
                    TextColor = Color.FromArgb("#ffffff"),
                    FontAttributes = FontAttributes.Bold
                }
            };
            statusAndDatesStack.Children.Add(statusChip);

            // Start/End-Zeiten vertikal
            var datesStack = new VerticalStackLayout
            {
                Spacing = 5
            };

            // Start-Datum/Status (basierend auf ticket.startab)
            if (!string.IsNullOrEmpty(ticket.startab))
            {
                string startValueText = "";
                DateTime? startDateTime = null;

                if (ticket.startab == "0")
                {
                    startValueText = "Ohne Angaben";
                }
                else if (ticket.startab == "-1")
                {
                    startValueText = "SOFORT";
                }
                else
                {
                    // Datum aus ticket.startab verwenden (UTC -> Lokalzeit)
                    if (long.TryParse(ticket.startab, out long startValue) && startValue > 0)
                    {
                        startDateTime = DateTimeOffset.FromUnixTimeMilliseconds(startValue).ToLocalTime().DateTime;
                        startValueText = $"{startDateTime.Value:dd.MM.yyyy HH:mm}";
                    }
                }

                if (!string.IsNullOrEmpty(startValueText))
                {
                    var startRowStack = new HorizontalStackLayout
                    {
                        Spacing = 8
                    };

                    var startLabel = new Label
                    {
                        FontSize = 10,
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span { Text = "START: ", TextColor = Color.FromArgb("#ffffff"), FontAttributes = FontAttributes.Bold },
                                new Span { Text = startValueText, TextColor = Color.FromArgb("#ffcc00") }
                            }
                        }
                    };
                    startRowStack.Children.Add(startLabel);

                    // Countdown bis zum Start (wenn in der Zukunft)
                    if (startDateTime.HasValue && startDateTime.Value > DateTime.Now)
                    {
                        var timeSpan = startDateTime.Value - DateTime.Now;
                        string countdownText = "";

                        if (timeSpan.TotalDays >= 1)
                        {
                            countdownText = $"(in {(int)timeSpan.TotalDays} Tag{(timeSpan.TotalDays >= 2 ? "en" : "")})";
                        }
                        else if (timeSpan.TotalHours >= 1)
                        {
                            countdownText = $"(in {(int)timeSpan.TotalHours} Std.)";
                        }
                        else if (timeSpan.TotalMinutes >= 1)
                        {
                            countdownText = $"(in {(int)timeSpan.TotalMinutes} Min.)";
                        }

                        if (!string.IsNullOrEmpty(countdownText))
                        {
                            var countdownLabel = new Label
                            {
                                Text = countdownText,
                                FontSize = 9,
                                TextColor = Color.FromArgb("#88ff88"),
                                VerticalOptions = LayoutOptions.Center
                            };
                            startRowStack.Children.Add(countdownLabel);
                        }
                    }

                    datesStack.Children.Add(startRowStack);
                }
            }

            // End-Datum/Status (basierend auf ticket.endbis)
            if (!string.IsNullOrEmpty(ticket.endbis))
            {
                string endValueText = "";

                if (ticket.endbis == "0")
                {
                    endValueText = "Ohne Angaben";
                }
                else if (ticket.endbis == "-1")
                {
                    endValueText = "SOFORT";
                }
                else 
                {
                    // Datum aus ticket.endbis verwenden (UTC -> Lokalzeit)
                    if (long.TryParse(ticket.endbis, out long endValue) && endValue > 0)
                    {
                        DateTime endDate = DateTimeOffset.FromUnixTimeMilliseconds(endValue).ToLocalTime().DateTime;
                        endValueText = $"{endDate:dd.MM.yyyy HH:mm}";
                    }
                }

                if (!string.IsNullOrEmpty(endValueText))
                {
                    var endLabel = new Label
                    {
                        FontSize = 10,
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span { Text = "ENDE: ", TextColor = Color.FromArgb("#ffffff"), FontAttributes = FontAttributes.Bold },
                                new Span { Text = endValueText, TextColor = Color.FromArgb("#ffcc00") }
                            }
                        }
                    };
                    datesStack.Children.Add(endLabel);
                }
            }

            // Nur hinzufügen, wenn mindestens ein Datum vorhanden ist
            if (datesStack.Children.Count > 0)
            {
                statusAndDatesStack.Children.Add(datesStack);
            }

            // Nur hinzufügen, wenn Status oder Daten vorhanden sind
            if (statusAndDatesStack.Children.Count > 0)
            {
                grid.Add(statusAndDatesStack, 1, 3);
                Grid.SetColumnSpan(statusAndDatesStack, 2);
            }

            border.Content = grid;

            // Tap-Handler zum Öffnen des Tickets
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnTicketCardTapped(ticket);
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }

        private string FormatTicketTime(DateTime dateTime)
        {
            var now = DateTime.Now;
            var diff = now - dateTime;

            if (diff.TotalMinutes < 1)
                return "Gerade eben";
            if (diff.TotalMinutes < 60)
                return $"vor {(int)diff.TotalMinutes} Min";
            if (diff.TotalHours < 24)
                return $"vor {(int)diff.TotalHours} Std";
            if (diff.TotalDays < 7)
                return $"vor {(int)diff.TotalDays} Tagen";

            return dateTime.ToString("dd.MM.yyyy");
        }

        private void OnTicketCardTapped(Ticket ticket)
        {
            try
            {
                // Ticket öffnen und Chat-Ansicht anzeigen
                Console.WriteLine($"Ticket #{ticket.id} wurde getippt: {ticket.titel}");

                // Editticket_container öffnen und Ticket-Chat laden
                if (ticket != null)
                {
                    editticket_container.IsVisible = true;
                    LoadTicketChat(ticket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Öffnen des Tickets: {ex.Message}");
                AppModel.Logger?.Error(ex, "ERROR: OnTicketCardTapped");
            }
        }


        private void OnEditTicketCloseContainer_Tapped(object sender, EventArgs e)
        {
            try
            {
                // Editticket_container schließen
                editticket_container.IsVisible = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: OnEditTicketCloseContainer_Tapped", ex);
            }
        }

        // Tab-Wechsel Event Handler
        private void OnTabBeschreibung_Clicked(object sender, EventArgs e)
        {
            SwitchToTab("beschreibung");
        }

        private void OnTabObjekt_Clicked(object sender, EventArgs e)
        {
            SwitchToTab("objekt");
        }

        private void OnTabChat_Clicked(object sender, EventArgs e)
        {
            SwitchToTab("chat");
        }

        private void SwitchToTab(string tabName)
        {
            try
            {
                // Alle Tabs ausblenden
                beschreibung_tab_content.IsVisible = false;
                objekt_tab_content.IsVisible = false;
                chat_tab_content.IsVisible = false;

                // Button-Styles zurücksetzen
                tab_beschreibung_btn.BackgroundColor = Color.FromArgb("#122446");
                tab_beschreibung_btn.TextColor = Color.FromArgb("#aaaaaa");
                tab_objekt_btn.BackgroundColor = Color.FromArgb("#122446");
                tab_objekt_btn.TextColor = Color.FromArgb("#aaaaaa");
                tab_chat_btn.BackgroundColor = Color.FromArgb("#122446");
                tab_chat_btn.TextColor = Color.FromArgb("#aaaaaa");

                // Gewählten Tab anzeigen und stylen
                switch (tabName.ToLower())
                {
                    case "beschreibung":
                        beschreibung_tab_content.IsVisible = true;
                        tab_beschreibung_btn.BackgroundColor = Color.FromArgb("#234567");
                        tab_beschreibung_btn.TextColor = Colors.White;
                        break;
                    case "objekt":
                        objekt_tab_content.IsVisible = true;
                        tab_objekt_btn.BackgroundColor = Color.FromArgb("#234567");
                        tab_objekt_btn.TextColor = Colors.White;
                        break;
                    case "chat":
                        chat_tab_content.IsVisible = true;
                        tab_chat_btn.BackgroundColor = Color.FromArgb("#234567");
                        tab_chat_btn.TextColor = Colors.White;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Tab-Wechsel: {ex.Message}");
                AppModel.Logger?.Error(ex, "ERROR: SwitchToTab");
            }
        }


        private Ticket currentTicket = null;


        /// <summary>
        /// Lädt und zeigt den Ticket-Verlauf (Chat)
        /// </summary>
        public void LoadTicketChat(Ticket ticket)
        {
            try
            {
                currentTicket = ticket;
                editticket_vscroll.Children.Clear();

                if (ticket == null)
                {
                    return;
                }

                // Titel mit Ticket-ID setzen
                editticket_title_label.Text = $"TICKET #{ticket.id}";
                editticket_subtitle_label.Text = ticket.titel ?? "";

                // Theme-abhängige Hintergrundfarben setzen
                SetThemeColors();

                // Tab 1: Beschreibung laden
                LoadTicketBeschreibung(ticket);

                // Tab 2: Objekt/Auftrag laden
                LoadTicketObjektAuftrag(ticket);

                // Tab 3: Chat-Verlauf laden
                if (ticket.chats != null)
                {
                    // Sortiere Nachrichten nach Datum
                    var sortedMessages = ticket.chats
                        .OrderBy(m => m.GetDateTime())
                        .ToList();

                    foreach (var message in sortedMessages)
                    {
                        AddChatMessageToUI(message);
                    }

                    // Scrolle zum Ende (neueste Nachricht)
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Task.Delay(100);
                        if (chatScrollView != null)
                        {
                            await chatScrollView.ScrollToAsync(0, chatScrollView.ContentSize.Height, false);
                        }
                    });
                }

                // Standardmäßig Beschreibung-Tab anzeigen
                SwitchToTab("beschreibung");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadTicketChat");
            }
        }

        /// <summary>
        /// Setzt Theme-abhängige Farben für UI-Elemente
        /// </summary>
        private void SetThemeColors()
        {
            try
            {
                bool isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
                Color backgroundColor = isDarkMode ? Color.FromArgb("#234567") : Colors.White;

                // Setze Hintergrundfarben für Tabs
                if (beschreibung_tab_content != null)
                    beschreibung_tab_content.BackgroundColor = backgroundColor;

                if (ticket_beschreibung_webview != null)
                    ticket_beschreibung_webview.BackgroundColor = backgroundColor;

                if (objekt_tab_content != null)
                    objekt_tab_content.BackgroundColor = isDarkMode ? Color.FromArgb("#234567") : Color.FromArgb("#f5f5f5");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Setzen der Theme-Farben: {ex.Message}");
            }
        }

        /// <summary>
        /// Lädt die Ticket-Beschreibung in den ersten Tab
        /// </summary>
        private async void LoadTicketBeschreibung(Ticket ticket)
        {
            try
            {
                if (ticket == null)
                {
                    AppModel.Logger?.Warn("LoadTicketBeschreibung: ticket is null");
                    return;
                }


                // WICHTIG: Prüfe ob WebView überhaupt existiert
                if (ticket_beschreibung_webview == null)
                {
                    AppModel.Logger?.Error("LoadTicketBeschreibung: ticket_beschreibung_webview is NULL!");
                    return;
                }


                // Lade Beschreibung (Base64 -> HTML/Text)
                string decodedText = Ticket.DecodeBase64RichText(ticket.text);


                if (string.IsNullOrWhiteSpace(decodedText))
                {
                    decodedText = "<p>Keine Beschreibung vorhanden.</p>";
                }

                // Prüfe ob es vollständiges HTML ist
                bool isFullHtml = decodedText.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
                                  decodedText.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase);

                string finalHtml;
                if (isFullHtml)
                {
                    // Bereits vollständiges HTML - verwende es direkt
                    finalHtml = decodedText;
                }
                else
                {
                    // Falls es HTML-Tags enthält, behalte sie; sonst wrap in <p>
                    string bodyContent = decodedText.Contains("<") ? decodedText : $"<p>{System.Net.WebUtility.HtmlEncode(decodedText)}</p>";
                    finalHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta charset='UTF-8'>
    <style>
        * {{
            -webkit-text-size-adjust: 100%;
        }}
        body {{ 
            background-color: #234567 !important; 
            color: #ffffff !important; 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; 
            padding: 15px;
            font-size: 15px;
            line-height: 1.6;
            margin: 0;
        }}
        p {{ 
            margin: 8px 0; 
        }}
        strong, b {{ 
            font-weight: bold; 
            color: #ffffff;
        }}
        em, i {{ 
            font-style: italic; 
        }}
        ul, ol {{ 
            margin: 10px 0; 
            padding-left: 25px; 
        }}
        li {{ 
            margin: 5px 0; 
        }}
        a {{ 
            color: #4da6ff; 
            pointer-events: none;
            text-decoration: none; 
        }}
        img {{ 
            max-width: 100%; 
            height: auto; 
        }}
    </style>
</head>
<body>
    {bodyContent}
</body>
</html>";
                }


                // Setze HTML direkt (OHNE BaseUrl - das war das iOS-Problem!)
                ticket_beschreibung_webview.Source = new HtmlWebViewSource { Html = finalHtml };

            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadTicketBeschreibung");
                Console.WriteLine($"Fehler: {ex.Message}");

                // Fallback bei Fehler
                if (ticket_beschreibung_webview != null)
                {
                    string errorHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ background-color: #2a2a3a; color: #ff6b6b; padding: 15px; font-family: sans-serif; }}
    </style>
</head>
<body>
    <p><strong>Fehler beim Laden:</strong></p>
    <p>{System.Net.WebUtility.HtmlEncode(ex.Message)}</p>
</body>
</html>";
                    ticket_beschreibung_webview.Source = new HtmlWebViewSource { Html = errorHtml };
                }
            }
        }

        /// <summary>
        /// Erstellt die Ticket-Info-Karte (ähnlich wie TicketCard)
        /// </summary>
        private void CreateTicketInfoCard(Ticket ticket)
        {
            try
            {
                if (ticket_info_container == null || ticket == null)
                    return;

                ticket_info_container.Children.Clear();

                // Status-Farbe bestimmen
                string statusColor = GetStatusColor(ticket.status);

                // Haupt-Border
                var border = new Border
                {
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    BackgroundColor = Color.FromArgb("#2a2a2a"),
                    StrokeThickness = 1,
                    Stroke = Color.FromArgb("#444444"),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(5) }
                };

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto }, // Status-Indikator (6px)
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Content
                        new ColumnDefinition { Width = GridLength.Auto } // Rechte Spalte (Prio/Zeit)
                    },
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = GridLength.Auto }, // Titel
                        new RowDefinition { Height = GridLength.Auto }, // Info (Chat-Badge optional)
                        new RowDefinition { Height = GridLength.Auto }, // Start/End
                        new RowDefinition { Height = GridLength.Auto }  // Status
                    }
                };

                // Status-Indikator (genau am linken Rand, 6px breit, volle Höhe, abgerundete Ecken)
                var statusIndicator = new Border
                {
                    BackgroundColor = Color.FromArgb(statusColor),
                    WidthRequest = 6,
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle 
                    { 
                        CornerRadius = new CornerRadius(5, 0, 0, 5) // Nur linke Ecken abgerundet
                    }
                };
                grid.Add(statusIndicator, 0, 0);
                Grid.SetRowSpan(statusIndicator, 4);

                // Titel-Zeile mit ID vorne
                var titleStack = new HorizontalStackLayout
                {
                    Margin = new Thickness(12, 8, 8, 2),
                    Spacing = 8,
                    VerticalOptions = LayoutOptions.Center
                };

                // Ticket-ID Badge (blauer abgerundeter Hintergrund vor dem Titel)
                var idBadge = new Border
                {
                    BackgroundColor = Color.FromArgb("#0078d7"),
                    Padding = new Thickness(6, 3),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
                    VerticalOptions = LayoutOptions.Center,
                    Content = new Label
                    {
                        Text = $"#{ticket.id}",
                        FontSize = 11,
                        TextColor = Color.FromArgb("#ffffff"),
                        FontAttributes = FontAttributes.Bold
                    }
                };
                titleStack.Children.Add(idBadge);

                // Titel
                var titleLabel = new Label
                {
                    Text = ticket.titel ?? "Ohne Titel",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#ffffff"),
                    LineBreakMode = LineBreakMode.TailTruncation,
                    MaxLines = 2,
                    WidthRequest = screenWidthDp - 150,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start
                };
                titleStack.Children.Add(titleLabel);

                grid.Add(titleStack, 1, 0);

                // Rechte Spalte: Prio oben, Zeit darunter
                var rightStack = new VerticalStackLayout
                {
                    Margin = new Thickness(0, 8, 8, 0),
                    Spacing = 5,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start
                };

                // Priorität als Chip darstellen
                if (ticket.prio >= 0)
                {
                    string prioText;
                    string prioBackgroundColor;
                    string prioTextColor;

                    if (ticket.prio == 0)
                    {
                        prioText = "Gering";
                        prioBackgroundColor = "#4472C4";
                        prioTextColor = "#FFFFFF";
                    }
                    else if (ticket.prio == 1)
                    {
                        prioText = "Normal";
                        prioBackgroundColor = "#009900";
                        prioTextColor = "#ffffff";
                    }
                    else if (ticket.prio == 2)
                    {
                        prioText = "Hoch";
                        prioBackgroundColor = "#aa5500";
                        prioTextColor = "#FFFFFF";
                    }
                    else
                    {
                        prioText = "NOTFALL";
                        prioBackgroundColor = "#990000";
                        prioTextColor = "#FFFFFF";
                    }

                    var prioChip = new Border
                    {
                        BackgroundColor = Color.FromArgb(prioBackgroundColor),
                        Padding = new Thickness(8, 4),
                        StrokeThickness = 0,
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(4) },
                        HorizontalOptions = LayoutOptions.End,
                        Content = new Label
                        {
                            Text = prioText,
                            FontSize = 11,
                            TextColor = Color.FromArgb(prioTextColor),
                            FontAttributes = FontAttributes.Bold,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        }
                    };
                    rightStack.Children.Add(prioChip);
                }

                // Zeitstempel unter der Prio
                DateTime.TryParse(ticket.updateat, out DateTime updateDate);
                var timeLabel = new Label
                {
                    Text = FormatTicketTime(updateDate),
                    FontSize = 10,
                    TextColor = Color.FromArgb("#888888"),
                    HorizontalOptions = LayoutOptions.End
                };
                rightStack.Children.Add(timeLabel);

                grid.Add(rightStack, 2, 0);
                Grid.SetRowSpan(rightStack, 2);

                // Zeile 2: Status-Chip und Start/End-Zeiten
                var statusAndDatesStack = new HorizontalStackLayout
                {
                    Margin = new Thickness(12, 0, 8, 8),
                    Spacing = 10
                };

                // Status-Chip links
                var statusChip = new Border
                {
                    BackgroundColor = Color.FromArgb(statusColor),
                    Padding = new Thickness(8, 6,8,3),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(4) },
                    Content = new Label
                    {
                        Text = GetStatusText(ticket.status),
                        FontSize = 10,
                        TextColor = Color.FromArgb("#ffffff"),
                        FontAttributes = FontAttributes.Bold
                    }
                };
                statusAndDatesStack.Children.Add(statusChip);

                // Start/End-Zeiten vertikal
                var datesStack = new VerticalStackLayout
                {
                    Spacing = 5
                };

                // Start-Datum/Status (basierend auf ticket.startab)
                if (!string.IsNullOrEmpty(ticket.startab))
                {
                    string startValueText = "";
                    DateTime? startDateTime = null;

                    if (ticket.startab == "0")
                    {
                        startValueText = "Ohne Angaben";
                    }
                    else if (ticket.startab == "-1")
                    {
                        startValueText = "SOFORT";
                    }
                    else
                    {
                        // Datum aus ticket.startab verwenden (UTC -> Lokalzeit)
                        if (long.TryParse(ticket.startab, out long startValue) && startValue > 0)
                        {
                            startDateTime = DateTimeOffset.FromUnixTimeMilliseconds(startValue).ToLocalTime().DateTime;
                            startValueText = $"{startDateTime.Value:dd.MM.yyyy HH:mm}";
                        }
                    }

                    if (!string.IsNullOrEmpty(startValueText))
                    {
                        var startRowStack = new HorizontalStackLayout
                        {
                            Spacing = 8
                        };

                        var startLabel = new Label
                        {
                            FontSize = 11,
                            FormattedText = new FormattedString
                            {
                                Spans =
                                {
                                    new Span { Text = "START: ", TextColor = Color.FromArgb("#ffffff"), FontAttributes = FontAttributes.Bold },
                                    new Span { Text = startValueText, TextColor = Color.FromArgb("#ffcc00") }
                                }
                            }
                        };
                        startRowStack.Children.Add(startLabel);

                        // Countdown bis zum Start (wenn in der Zukunft)
                        if (startDateTime.HasValue && startDateTime.Value > DateTime.Now)
                        {
                            var timeSpan = startDateTime.Value - DateTime.Now;
                            string countdownText = "";

                            if (timeSpan.TotalDays >= 1)
                            {
                                countdownText = $"(in {(int)timeSpan.TotalDays} Tag{(timeSpan.TotalDays >= 2 ? "en" : "")})";
                            }
                            else if (timeSpan.TotalHours >= 1)
                            {
                                countdownText = $"(in {(int)timeSpan.TotalHours} Std.)";
                            }
                            else if (timeSpan.TotalMinutes >= 1)
                            {
                                countdownText = $"(in {(int)timeSpan.TotalMinutes} Min.)";
                            }

                            if (!string.IsNullOrEmpty(countdownText))
                            {
                                var countdownLabel = new Label
                                {
                                    Text = countdownText,
                                    FontSize = 10,
                                    TextColor = Color.FromArgb("#88ff88"),
                                    VerticalOptions = LayoutOptions.Center
                                };
                                startRowStack.Children.Add(countdownLabel);
                            }
                        }

                        datesStack.Children.Add(startRowStack);
                    }
                }

                // End-Datum/Status (basierend auf ticket.endbis)
                if (!string.IsNullOrEmpty(ticket.endbis))
                {
                    string endValueText = "";

                    if (ticket.endbis == "0")
                    {
                        endValueText = "Ohne Angaben";
                    }
                    else if (ticket.endbis == "-1")
                    {
                        endValueText = "SOFORT";
                    }
                    else 
                    {
                        // Datum aus ticket.endbis verwenden (UTC -> Lokalzeit)
                        if (long.TryParse(ticket.endbis, out long endValue) && endValue > 0)
                        {
                            DateTime endDate = DateTimeOffset.FromUnixTimeMilliseconds(endValue).ToLocalTime().DateTime;
                            endValueText = $"{endDate:dd.MM.yyyy HH:mm}";
                        }
                    }

                    if (!string.IsNullOrEmpty(endValueText))
                    {
                        var endLabel = new Label
                        {
                            FontSize = 11,
                            FormattedText = new FormattedString
                            {
                                Spans =
                                {
                                    new Span { Text = "ENDE: ", TextColor = Color.FromArgb("#ffffff"), FontAttributes = FontAttributes.Bold },
                                    new Span { Text = endValueText, TextColor = Color.FromArgb("#ffcc00") }
                                }
                            }
                        };
                        datesStack.Children.Add(endLabel);
                    }
                }

                // Nur hinzufügen, wenn mindestens ein Datum vorhanden ist
                if (datesStack.Children.Count > 0)
                {
                    statusAndDatesStack.Children.Add(datesStack);
                }

                // Nur hinzufügen, wenn Status oder Daten vorhanden sind
                if (statusAndDatesStack.Children.Count > 0)
                {
                    grid.Add(statusAndDatesStack, 1, 2);
                    Grid.SetColumnSpan(statusAndDatesStack, 2);
                }

                border.Content = grid;
                ticket_info_container.Children.Add(border);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CreateTicketInfoCard");
            }
        }

        /// <summary>
        /// Gibt die Farbe für einen Ticket-Status zurück
        /// </summary>
        private string GetStatusColor(int status)
        {
            return status switch
            {
                1 => "#6B7280", // Neue - Grau
                2 => "#3B82F6", // Offene - Blau
                4 => "#F59E0B", // In Arbeit - Gelb/Orange
                5 => "#F97316", // Rückfrage - Orange
                9 => "#DC2626", // Erledigte - Rot/Weinrot
                10 => "#4B5563", // Archiv - Dunkelgrau
                _ => "#ffffff"  // Unbekannt - Weiß
            };
        }

        /// <summary>
        /// Gibt den Text für einen Ticket-Status zurück
        /// </summary>
        private string GetStatusText(int status)
        {
            return status switch
            {
                1 => "Neue",
                2 => "Offen",
                4 => "In Arbeit",
                5 => "Rückfrage",
                9 => "Erledigte",
                10 => "Archiv",
                _ => "Unbekannt"
            };
        }

        private void LoadTicketObjektAuftrag(Ticket ticket)
        {
            try
            {
                objekt_auftrag_content.Children.Clear();

                if (ticket == null)
                    return;

                // Erstelle Ticket-Info-Karte am Anfang des Details-Tabs
                CreateTicketInfoCard(ticket);

                // Kunde-Informationen
                //if (ticket.kunde != null || !string.IsNullOrWhiteSpace(ticket.kundename))
                //{
                //    var kundeSection = CreateDetailSection("KUNDE", "#2f94ed");

                //    if (!string.IsNullOrWhiteSpace(ticket.kundename))
                //    {
                //        kundeSection.Children.Add(CreateDetailLabel($"Firma: {ticket.kundename}"));
                //    }

                //    if (ticket.kunde != null)
                //    {
                //        if (!string.IsNullOrWhiteSpace(ticket.kunde.name))
                //        {
                //            kundeSection.Children.Add(CreateDetailLabel($"Kontakt: {ticket.kunde.name}"));
                //        }
                //        if (!string.IsNullOrWhiteSpace(ticket.kunde.mail))
                //        {
                //            kundeSection.Children.Add(CreateDetailLabel($"E-Mail: {ticket.kunde.mail}"));
                //        }
                //    }

                //    objekt_auftrag_content.Children.Add(kundeSection);
                //}

                // Ansprechpartner (ASP)
                //if (ticket.asp != null)
                //{
                //    var aspSection = CreateDetailSection("ANSPRECHPARTNER", "#2f94ed");

                //    if (!string.IsNullOrWhiteSpace(ticket.asp.firma))
                //    {
                //        aspSection.Children.Add(CreateDetailLabel($"Firma: {ticket.asp.firma}"));
                //    }

                //    string aspName = ticket.asp.GetFullName();
                //    if (!string.IsNullOrWhiteSpace(aspName))
                //    {
                //        aspSection.Children.Add(CreateDetailLabel($"Name: {aspName}"));
                //    }

                //    if (!string.IsNullOrWhiteSpace(ticket.asp.mail))
                //    {
                //        aspSection.Children.Add(CreateDetailLabel($"E-Mail: {ticket.asp.mail}"));
                //    }

                //    if (!string.IsNullOrWhiteSpace(ticket.asp.telefon))
                //    {
                //        aspSection.Children.Add(CreateDetailLabel($"Telefon: {ticket.asp.telefon}"));
                //    }

                //    if (!string.IsNullOrWhiteSpace(ticket.asp.mobile))
                //    {
                //        aspSection.Children.Add(CreateDetailLabel($"Mobil: {ticket.asp.mobile}"));
                //    }

                //    objekt_auftrag_content.Children.Add(aspSection);
                //}

                // Objekt-Informationen
                if (ticket.objektid > 0)
                {
                    var objektSection = CreateDetailSection("OBJEKT", "#2f94ed");

                    objektSection.Children.Add(CreateDetailLabel($"Objekt-ID: {ticket.objektid}"));

                    //if (!string.IsNullOrWhiteSpace(ticket.objektname))
                    //{
                    //    objektSection.Children.Add(CreateDetailLabel($"{ticket.objektname}"));
                    //}

                    if (ticket.objekt != null)
                    {
                        if (!string.IsNullOrWhiteSpace(ticket.objekt.objektnr))
                        {
                            objektSection.Children.Add(CreateDetailLabel($"Objekt-Nr: {ticket.objekt.objektnr}"));
                        }
                        if (!string.IsNullOrWhiteSpace(ticket.objekt.objektname))
                        {
                            objektSection.Children.Add(CreateDetailLabel($"Objektname: {ticket.objekt.objektname}"));
                        }


                        if (!string.IsNullOrWhiteSpace(ticket.objekt.adresse))
                        {
                            objektSection.Children.Add(CreateDetailLabel($"{ticket.objekt.adresse}"));
                        }

                        if (!string.IsNullOrWhiteSpace(ticket.objekt.plz) || !string.IsNullOrWhiteSpace(ticket.objekt.ort))
                        {
                            objektSection.Children.Add(CreateDetailLabel($"{ticket.objekt.plz} {ticket.objekt.ort}"));
                        }
                    }

                    objekt_auftrag_content.Children.Add(objektSection);
                }

                // Auftrag-Informationen
                if (ticket.auftragid > 0)
                {
                    var auftragSection = CreateDetailSection("AUFTRAG", "#2f94ed");
                    auftragSection.Children.Add(CreateDetailLabel($"Auftrags-ID: {ticket.auftragid}"));
                    objekt_auftrag_content.Children.Add(auftragSection);
                }

                // Ticket-Details (Start, Ende, Priorität, Status, etc.)
                var detailsSection = CreateDetailSection("TICKET-DETAILS", "#2f94ed");

                // Starten ab
                if (long.TryParse(ticket.start, out long startTimestamp) && startTimestamp > 0)
                {
                    // JavaScript Timestamps sind in Millisekunden
                    var startDate = DateTimeOffset.FromUnixTimeMilliseconds(startTimestamp).DateTime;
                    detailsSection.Children.Add(CreateDetailLabel($"📅 Starten ab: {startDate:dd.MM.yyyy HH:mm}"));
                }
                else
                {
                    detailsSection.Children.Add(CreateDetailLabel($"📅 Starten ab: Ohne Zeitvorgabe", "#ff6b6b"));
                }

                // Fertig bis
                if (long.TryParse(ticket.end, out long endTimestamp) && endTimestamp > 0)
                {
                    // JavaScript Timestamps sind in Millisekunden
                    var endDate = DateTimeOffset.FromUnixTimeMilliseconds(endTimestamp).DateTime;
                    detailsSection.Children.Add(CreateDetailLabel($"🏁 Fertig bis: {endDate:dd.MM.yyyy HH:mm}"));
                }
                else
                {
                    detailsSection.Children.Add(CreateDetailLabel($"🏁 Fertig bis: Ohne Zeitvorgabe", "#ff6b6b"));
                }

                // Priorität
                string prioText = ticket.prio switch
                {
                    3 => "🔴 NOTFALL",
                    2 => "🟠 HOCH",
                    1 => "🟡 NORMAL",
                    0 => "🟢 GERING",
                    _ => "UNBEKANNT"
                };
                detailsSection.Children.Add(CreateDetailLabel($"Wichtigkeit/Priorität: {prioText}"));

                // Status
                string statusText = ticket.status switch
                {
                    1 => "Neu",
                    2 => "Offen",
                    3 => "In Arbeit",
                    4 => "Rückfrage",
                    5 => "Erledigt",
                    6 => "Archiviert",
                    _ => "Unbekannt"
                };
                detailsSection.Children.Add(CreateDetailLabel($"Status: {statusText}"));

                // Zugewiesener (Besitzer)
                if (ticket.besitzer != null || !string.IsNullOrWhiteSpace(ticket.besitzername))
                {
                    string besitzerName = ticket.besitzername ?? ticket.besitzer?.name ?? "Nicht zugewiesen";
                    detailsSection.Children.Add(CreateDetailLabel($"👤 Zugewiesener: {besitzerName}"));

                    // Besitzerstatus
                    if (ticket.besitzerstatus == -1)
                    {
                        detailsSection.Children.Add(CreateDetailLabel($"⚠️ Noch nicht geöffnet/gesehen", "#ff9800"));
                    }
                }
                else
                {
                    detailsSection.Children.Add(CreateDetailLabel($"👤 Zugewiesener: Nicht zugewiesen"));
                }

                // Ersteller
                if (ticket.ersteller != null || !string.IsNullOrWhiteSpace(ticket.erstellername))
                {
                    string erstellerName = ticket.erstellername ?? ticket.ersteller?.name ?? "Unbekannt";
                    DateTime? createDate = null;
                    if (long.TryParse(ticket.start, out long createTimestamp) && createTimestamp > 0)
                    {
                        // JavaScript Timestamps sind in Millisekunden
                        createDate = DateTimeOffset.FromUnixTimeMilliseconds(createTimestamp).DateTime;
                    }

                    string dateStr = createDate.HasValue ? $" ({createDate.Value:dd.MM.yyyy})" : "";
                    detailsSection.Children.Add(CreateDetailLabel($"👨‍💼 Ersteller: {erstellerName}{dateStr}"));
                }

                // Intern/Extern
                detailsSection.Children.Add(CreateDetailLabel($"Typ: {(ticket.intern ? "Intern" : "Extern")}"));

                objekt_auftrag_content.Children.Add(detailsSection);

                // Wenn gar keine Informationen vorhanden
                if (objekt_auftrag_content.Children.Count == 0)
                {
                    var noDataSection = CreateDetailSection("KEINE DATEN", "#999999");
                    noDataSection.Children.Add(CreateDetailLabel("Keine zusätzlichen Informationen vorhanden."));
                    objekt_auftrag_content.Children.Add(noDataSection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden von Ticket-Details: {ex.Message}");
                AppModel.Logger?.Error(ex, "ERROR: LoadTicketObjektAuftrag");
            }
        }

        private StackLayout CreateDetailSection(string title, string titleColor)
        {
            var section = new StackLayout
            {
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(15),
                //BackgroundColor = Color.FromArgb("#2a2a3a"),
                Spacing = 5
            };

            section.Children.Add(new Label
            {
                Text = title,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                TextColor = Color.FromArgb(titleColor)
            });

            return section;
        }

        private Label CreateDetailLabel(string text, string color = "#ffffff")
        {
            return new Label
            {
                Text = text,
                FontSize = 14,
                TextColor = Color.FromArgb(color),
                Margin = new Thickness(0, 2, 0, 2)
            };
        }

        /// <summary>
        /// Fügt eine Chat-Nachricht zur UI hinzu
        /// </summary>
        private void AddChatMessageToUI(TicketChat message)
        {
            try
            {
                // Bestimme ob die Nachricht vom aktuellen Benutzer ist
                int currentUserId = AppModel.Instance?.Person?.id ?? 0;
                bool isOwnMessage = message.personid == currentUserId;

                // Dekodiere Base64-kodierten Nachrichtentext
                string decodedText = Ticket.DecodeBase64RichText(message.t);
                if (string.IsNullOrWhiteSpace(decodedText) || decodedText.Trim() == "<div></div>")
                {
                    decodedText = "";
                    return;
                }

                // Prüfe ob die Nachricht HTML/Bild enthält
                bool containsHtml = !string.IsNullOrEmpty(decodedText) && 
                    (decodedText.Contains("<img") || decodedText.Contains("<html") || decodedText.Contains("<div") || decodedText.Contains("data:image"));

                // Chat-Bubble Container
                var messageContainer = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 8),
                    HorizontalOptions = isOwnMessage ? LayoutOptions.End : LayoutOptions.Start,
                    WidthRequest = screenWidthDp * 0.75
                };

                // Wenn es eine eigene Nachricht mit Bild ist, füge Spalte für Delete-Button hinzu
                if (isOwnMessage && containsHtml)
                {
                    messageContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                    messageContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = 40 });
                }

                // Chat-Bubble
                var messageBubble = new Border
                {
                    Padding = containsHtml ? new Thickness(4) : new Thickness(12, 8),
                    BackgroundColor = isOwnMessage ? Color.FromArgb("#DCF8C6") : Color.FromArgb("#FFFFFF"),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                    {
                        CornerRadius = new CornerRadius(
                            isOwnMessage ? 15 : 2,
                            isOwnMessage ? 2 : 15,
                            isOwnMessage ? 2 : 15,
                            15
                        )
                    },
                    Shadow = new Shadow
                    {
                        Brush = Colors.Black,
                        Opacity = 0.1f,
                        Radius = 4,
                        Offset = new Point(0, 1)
                    }
                };

                var messageContent = new VerticalStackLayout
                {
                    Spacing = 4
                };

                // Absender Name (nur bei fremden Nachrichten)
                if (!isOwnMessage)
                {
                    messageContent.Children.Add(new Label
                    {
                        Text = message.personname,
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#075E54")
                    });
                }

                // Nachrichteninhalt - WebView für HTML/Bilder, Label für Text
                if (containsHtml)
                {
                    var bgColor = isOwnMessage ? "#DCF8C6" : "#FFFFFF";
                    var htmlContent = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
                            <meta charset='UTF-8'>
                            <style>
                                body {{
                                    margin: 0;
                                    padding: 8px;
                                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                                    font-size: 15px;
                                    color: #000000;
                                    background-color: {bgColor};
                                    overflow-x: hidden;
                                }}
                                img {{
                                    max-width: 100%;
                                    height: auto;
                                    display: block;
                                    border-radius: 8px;
                                    margin: 4px 0;
                                    cursor: pointer;
                                }}
                                p {{
                                    margin: 0;
                                    padding: 0;
                                    word-wrap: break-word;
                                }}
                            </style>
                        </head>
                        <body>
                            {decodedText}
                        </body>
                        </html>";

                    var webView = new WebView
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        BackgroundColor = Colors.Transparent
                    };

                    webView.Source = new HtmlWebViewSource { Html = htmlContent };

                    // Dynamische Höhe basierend auf Inhalt
                    webView.HeightRequest = 200; // Minimale Höhe für Bilder

                    // Tap-Handler für Bildvorschau
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += (s, e) => OnChatImageTapped(message);
                    webView.GestureRecognizers.Add(tapGesture);

                    messageContent.Children.Add(webView);
                }
                else
                {
                    // Normaler Text als Label (dekodiert)
                    messageContent.Children.Add(new Label
                    {
                        Text = decodedText,
                        FontSize = 15,
                        TextColor = Color.FromArgb("#000000"),
                        LineBreakMode = LineBreakMode.WordWrap
                    });
                }

                // Zeit
                var timeLabel = new Label
                {
                    Text = message.GetFormattedTime(),
                    FontSize = 11,
                    TextColor = Color.FromArgb("#667781"),
                    HorizontalOptions = LayoutOptions.End,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                messageContent.Children.Add(timeLabel);

                messageBubble.Content = messageContent;

                // Füge Bubble zum Container hinzu
                if (isOwnMessage && containsHtml)
                {
                    Grid.SetColumn(messageBubble, 0);
                    messageContainer.Children.Add(messageBubble);

                    // Delete-Button für eigene Bilder
                    var deleteButton = new Border
                    {
                        BackgroundColor = Color.FromArgb("#FF3B30"),
                        StrokeThickness = 0,
                        Padding = new Thickness(8),
                        Margin = new Thickness(4, 0, 0, 0),
                        VerticalOptions = LayoutOptions.Center,
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                        {
                            CornerRadius = 20
                        },
                        Content = new Label
                        {
                            Text = "🗑️",
                            FontSize = 18,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        }
                    };

                    var deleteTap = new TapGestureRecognizer();
                    deleteTap.Tapped += async (s, e) => await OnDeleteChatMessage(message);
                    deleteButton.GestureRecognizers.Add(deleteTap);

                    Grid.SetColumn(deleteButton, 1);
                    messageContainer.Children.Add(deleteButton);
                }
                else
                {
                    messageContainer.Children.Add(messageBubble);
                }

                editticket_vscroll.Children.Add(messageContainer);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: AddChatMessageToUI");
            }
        }

        /// <summary>
        /// Event Handler für das Senden einer neuen Nachricht
        /// </summary>
        private async void OnSendTicketMessage_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (currentTicket == null)
                {
                    await DisplayAlertAsync("Fehler", "Kein Ticket ausgewählt", "OK");
                    return;
                }

                string messageText = ticketMessageEditor?.Text?.Trim();

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    return;
                }

                // Hole aktuelle Benutzer-ID und Name
                int currentUserId = AppModel.Instance?.Person?.id ?? 0;
                string currentUserName = !string.IsNullOrEmpty(AppModel.Instance?.Person?.name)
                    ? $"{AppModel.Instance.Person.vorname} {AppModel.Instance.Person.name}".Trim()
                    : AppModel.Instance?.SettingModel?.SettingDTO?.LoginName ?? "Unbekannt";

                // Füge Nachricht zum Ticket hinzu
                currentTicket.AddChatMessage(currentUserId, currentUserName, messageText, "info", true);

                // Speichere Ticket
                Ticket.Save(currentTicket);

                // Füge Nachricht zur UI hinzu
                var newMessage = currentTicket.chats.Last();
                AddChatMessageToUI(newMessage);

                // Leere Editor
                if (ticketMessageEditor != null)
                {
                    ticketMessageEditor.Text = string.Empty;
                }

                // Scrolle zum Ende
                await Task.Delay(100);
                if (chatScrollView != null)
                {
                    await chatScrollView.ScrollToAsync(0, chatScrollView.ContentSize.Height, false);
                }

                // Verstecke Tastatur
                if (ticketMessageEditor != null)
                {
                    await ticketMessageEditor.HideKeyboardAsync(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: OnSendTicketMessage_Clicked");
                await DisplayAlertAsync("Fehler", "Nachricht konnte nicht gesendet werden", "OK");
            }
        }

        private async void OnCameraButton_Tapped(object sender, EventArgs e)
        {
            try
            {
                if (currentTicket == null)
                {
                    await DisplayAlertAsync("Fehler", "Kein Ticket ausgewählt", "OK");
                    return;
                }

                // Prüfe Kamera-Berechtigung
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync("Berechtigung erforderlich", "Kamera-Zugriff wurde verweigert. Bitte aktivieren Sie die Kamera-Berechtigung in den App-Einstellungen.", "OK");
                        return;
                    }
                }

                // Prüfe ob Kamera verfügbar ist
                if (!MediaPicker.IsCaptureSupported)
                {
                    await DisplayAlertAsync("Nicht verfügbar", "Kamera ist auf diesem Gerät nicht verfügbar", "OK");
                    return;
                }


                var options = new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    SelectionLimit = 1,
                    PreserveMetaData = true,
                    RotateImage = true
                };
#if !IOS
                options.RotateImage = true;
#endif

                var photo = await MediaPicker.CapturePhotoAsync(options);

                if (photo != null)
                {
                    await ProcessAndAddImageToChat(photo);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                AppModel.Logger?.Error(fnsEx, "ERROR: OnCameraButton_Tapped - Feature not supported");
                await DisplayAlertAsync("Nicht unterstützt", "Kamera wird auf diesem Gerät nicht unterstützt", "OK");
            }
            catch (PermissionException pEx)
            {
                AppModel.Logger?.Error(pEx, "ERROR: OnCameraButton_Tapped - Permission error");
                await DisplayAlertAsync("Berechtigung erforderlich", "Kamera-Berechtigung wird benötigt", "OK");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: OnCameraButton_Tapped");
                await DisplayAlertAsync("Fehler", $"Foto konnte nicht aufgenommen werden: {ex.Message}", "OK");
            }
        }

        private async void OnGalleryButton_Tapped(object sender, EventArgs e)
        {
            try
            {
                if (currentTicket == null)
                {
                    await DisplayAlertAsync("Fehler", "Kein Ticket ausgewählt", "OK");
                    return;
                }

                // Prüfe Foto-Berechtigung
                var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Photos>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync("Berechtigung erforderlich", "Foto-Zugriff wurde verweigert. Bitte aktivieren Sie die Foto-Berechtigung in den App-Einstellungen.", "OK");
                        return;
                    }
                }

                var options = new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    SelectionLimit = 1,
                    PreserveMetaData = true,
                    RotateImage = true
                };
#if !IOS
                options.RotateImage = true;
#endif
                // Foto aus Galerie auswählen
                var photos = await MediaPicker.PickPhotosAsync(options);

                if (photos != null && photos.Any())
                {
                    await ProcessAndAddImageToChat(photos.First());
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                AppModel.Logger?.Error(fnsEx, "ERROR: OnGalleryButton_Tapped - Feature not supported");
                await DisplayAlertAsync("Nicht unterstützt", "Galerie wird auf diesem Gerät nicht unterstützt", "OK");
            }
            catch (PermissionException pEx)
            {
                AppModel.Logger?.Error(pEx, "ERROR: OnGalleryButton_Tapped - Permission error");
                await DisplayAlertAsync("Berechtigung erforderlich", "Foto-Berechtigung wird benötigt", "OK");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: OnGalleryButton_Tapped");
                await DisplayAlertAsync("Fehler", $"Foto konnte nicht ausgewählt werden: {ex.Message}", "OK");
            }
        }

        private async Task ProcessAndAddImageToChat(FileResult photo)
        {
            try
            {
                if (photo == null)
                {
                    AppModel.Logger?.Warn("ProcessAndAddImageToChat called with null photo");
                    return;
                }

                // Zeige Loader
                ShowChatLoader();

                //AppModel.Logger?.Info($"ProcessAndAddImageToChat - Processing file: {photo.FileName}");

                // Hole aktuelle Benutzer-ID und Name
                int currentUserId = AppModel.Instance?.Person?.id ?? 0;
                string currentUserName = !string.IsNullOrEmpty(AppModel.Instance?.Person?.name)
                    ? $"{AppModel.Instance.Person.vorname} {AppModel.Instance.Person.name}".Trim()
                    : AppModel.Instance?.SettingModel?.SettingDTO?.LoginName ?? "Unbekannt";

                // Lade Bild in Byte-Array
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                //AppModel.Logger?.Info($"ProcessAndAddImageToChat - Image loaded, size: {imageBytes.Length} bytes");

                // Konvertiere zu Base64
                string base64Image = Convert.ToBase64String(imageBytes);
                string imageDataUrl = $"data:image/jpeg;base64,{base64Image}";

                // Erstelle Chat-Nachricht mit Bild
                string messageText = $"<img src=\"{imageDataUrl}\" style=\"max-width: 100%; border-radius: 8px;\" />";

                // Füge Nachricht zum Ticket hinzu
                currentTicket.AddChatMessage(currentUserId, currentUserName, messageText, "info", true);

                // Speichere Ticket
                Ticket.Save(currentTicket);

                // Füge Nachricht zur UI hinzu
                var newMessage = currentTicket.chats.Last();
                AddChatMessageToUI(newMessage);

                // Scrolle zum Ende
                await Task.Delay(100);
                if (chatScrollView != null)
                {
                    await chatScrollView.ScrollToAsync(0, chatScrollView.ContentSize.Height, false);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ProcessAndAddImageToChat");
                await DisplayAlertAsync("Fehler", $"Bild konnte nicht verarbeitet werden: {ex.Message}", "OK");
            }
            finally
            {
                // Verstecke Loader
                HideChatLoader();
            }
        }

        /// <summary>
        /// Zeigt den Loader-Overlay an
        /// </summary>
        private void ShowChatLoader()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (loader_overlay != null)
                    {
                        loader_overlay.IsVisible = true;
                    }
                });
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ShowChatLoader");
            }
        }

        /// <summary>
        /// Versteckt den Loader-Overlay
        /// </summary>
        private void HideChatLoader()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (loader_overlay != null)
                    {
                        loader_overlay.IsVisible = false;
                    }
                });
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: HideChatLoader");
            }
        }

        /// <summary>
        /// Zeigt Bildvorschau in einem skalierbaren Overlay
        /// </summary>
        private async void OnChatImageTapped(TicketChat message)
        {
            try
            {
                // Extrahiere Base64-Bild aus der Nachricht
                if (string.IsNullOrEmpty(message.t) || !message.t.Contains("data:image"))
                {
                    return;
                }

                // Zeige Bildvorschau-Overlay
                await ShowImagePreview(message.t);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: OnChatImageTapped");
            }
        }

        /// <summary>
        /// Löscht eine Chat-Nachricht (nur eigene)
        /// </summary>
        private async Task OnDeleteChatMessage(TicketChat message)
        {
            try
            {
                int currentUserId = AppModel.Instance?.Person?.id ?? 0;
                if (message.personid != currentUserId)
                {
                    await DisplayAlertAsync("Nicht erlaubt", "Sie können nur eigene Nachrichten löschen", "OK");
                    return;
                }

                bool confirm = await DisplayAlertAsync("Löschen bestätigen", "Möchten Sie diese Nachricht wirklich löschen?", "Ja", "Nein");
                if (!confirm)
                {
                    return;
                }

                // Entferne Nachricht aus dem Ticket
                if (currentTicket?.chats != null && currentTicket.chats.Contains(message))
                {
                    currentTicket.chats.Remove(message);
                    Ticket.Save(currentTicket);

                    // Aktualisiere Chat-UI
                    ReloadChatMessages();

                    AppModel.Logger?.Info($"Chat message deleted: {message.id}");
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: OnDeleteChatMessage");
                await DisplayAlertAsync("Fehler", "Nachricht konnte nicht gelöscht werden", "OK");
            }
        }

        /// <summary>
        /// Lädt Chat-Nachrichten neu
        /// </summary>
        private void ReloadChatMessages()
        {
            try
            {
                if (currentTicket == null || editticket_vscroll == null)
                {
                    return;
                }

                // Lösche alte Nachrichten
                editticket_vscroll.Children.Clear();

                // Lade Nachrichten neu
                if (currentTicket.chats != null)
                {
                    var sortedMessages = currentTicket.chats
                        .OrderBy(m => m.GetDateTime())
                        .ToList();

                    foreach (var message in sortedMessages)
                    {
                        AddChatMessageToUI(message);
                    }

                    // Scrolle zum Ende
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Task.Delay(100);
                        if (chatScrollView != null)
                        {
                            await chatScrollView.ScrollToAsync(0, chatScrollView.ContentSize.Height, false);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ReloadChatMessages");
            }
        }

        /// <summary>
        /// Zeigt Bild in einem skalierbaren Overlay
        /// </summary>
        private async Task ShowImagePreview(string htmlContent)
        {
            try
            {
                // Erstelle Overlay
                var imagePreviewOverlay = new AbsoluteLayout
                {
                    BackgroundColor = Color.FromArgb("#E6000000") // 90% Transparenz
                };

                // Close Button
                var closeButton = new Border
                {
                    BackgroundColor = Color.FromArgb("#FFFFFF"),
                    StrokeThickness = 0,
                    Padding = new Thickness(12),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                    {
                        CornerRadius = 25
                    },
                    Content = new Label
                    {
                        Text = "✕",
                        FontSize = 24,
                        TextColor = Color.FromArgb("#000000"),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };

                AbsoluteLayout.SetLayoutBounds(closeButton, new Rect(0.9, 0.05, 50, 50));
                AbsoluteLayout.SetLayoutFlags(closeButton, AbsoluteLayoutFlags.PositionProportional);

                // Bild WebView mit Zoom-Support
                var imageWebView = new WebView
                {
                    BackgroundColor = Colors.Transparent
                };

                var zoomableHtml = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=5.0, user-scalable=yes'>
                        <style>
                            body {{
                                margin: 0;
                                padding: 0;
                                display: flex;
                                justify-content: center;
                                align-items: center;
                                min-height: 100vh;
                                background: transparent;
                                overflow: auto;
                            }}
                            img {{
                                max-width: 100%;
                                height: auto;
                                display: block;
                                border-radius: 8px;
                                box-shadow: 0 4px 20px rgba(0,0,0,0.3);
                            }}
                        </style>
                    </head>
                    <body>
                        {htmlContent}
                    </body>
                    </html>";

                imageWebView.Source = new HtmlWebViewSource { Html = zoomableHtml };

                AbsoluteLayout.SetLayoutBounds(imageWebView, new Rect(0, 0, 1, 1));
                AbsoluteLayout.SetLayoutFlags(imageWebView, AbsoluteLayoutFlags.All);

                imagePreviewOverlay.Children.Add(imageWebView);
                imagePreviewOverlay.Children.Add(closeButton);

                // Tap zum Schließen
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    if (this.Content is AbsoluteLayout mainLayout && mainLayout.Children.Contains(imagePreviewOverlay))
                    {
                        mainLayout.Children.Remove(imagePreviewOverlay);
                    }
                };
                closeButton.GestureRecognizers.Add(tapGesture);

                // Füge Overlay zu MainLayout hinzu
                if (this.Content is AbsoluteLayout layout)
                {
                    AbsoluteLayout.SetLayoutBounds(imagePreviewOverlay, new Rect(0, 0, 1, 1));
                    AbsoluteLayout.SetLayoutFlags(imagePreviewOverlay, AbsoluteLayoutFlags.All);
                    layout.Children.Add(imagePreviewOverlay);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ShowImagePreview");
                await DisplayAlertAsync("Fehler", "Bildvorschau konnte nicht angezeigt werden", "OK");
            }
        }








        public void btn_PlanTabCeTapped(object sender, EventArgs e)
        {
            frame_plantabCe.Margin = new Thickness(0, -8, 2, 0);
            frame_plantabA.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabB.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabC.Margin = new Thickness(0, 0, 2, 0);
            frame_planConA.IsVisible = false;
            frame_planConB.IsVisible = false;
            frame_planConCe.IsVisible = true;
            frame_planConC.IsVisible = false;

            GetChecksInfo(7);
        }






        public void btn_MainMenuTapped(object sender, EventArgs e)
        {
            MainMenuTapped_Done(!panelContainer.IsVisible);
        }
        public async void MainMenuTapped_Done(bool visible)
        {
            if (!visible)
            {
                panelContainer.IsVisible = visible;
            }
            else
            {
                panelContainer.IsVisible = visible;
                SetAllSyncState();
            }
        }

        public int SetAllSyncState()
        {
            ShowDisconnected();
            var countAll = GetAllSyncFromUploadCount();
            btn_settings_frame_count.IsVisible = countAll > 0;
            btn_StartPage_frame_count.IsVisible = countAll > 0;
            btn_settings_count.Text = "" + countAll;
            btn_StartPage_count.Text = "" + countAll;
            return countAll;
        }
        public void btn_BuildingScanTapped(object sender, EventArgs e)
        {
            ShowBuildingScanPage(false);
        }
        public void btn_BuildingNotScanTapped(object sender, EventArgs e)
        {
            ShowBuildingNotScanPage();
        }
        public void btn_back_BuildingScanTapped(object sender, EventArgs e)
        {
            ShowMainPage();
        }
        public void btn_back_BuildingOutScanTapped(object sender, EventArgs e)
        {
            ShowMainPage();
        }
        public void btn_done_BuildingOutScanTapped(object sender, EventArgs e)
        {
            popupContainer_quest_overtootherBuilding.IsVisible = false;

            // Zurücksetzten aller States für die Auswahl der Ausführungen
            AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
            ShowMainPage();
        }
        public void btn_back_BuildingOrderTapped(object sender, EventArgs e)
        {
            AppModel.Instance.LastSelectedOrder = null;
            ShowMainPage();
        }
        public void btn_back_OrderCategoryTapped(object sender, EventArgs e)
        {
            btn_back_inBuildingOrder_category_showall_txt.Text = "Alle zeigen";
            AppModel.Instance._showall_OrderCategory = false;
            AppModel.Instance.LastSelectedCategory = null;
            ShowOrderPage();
        }
        public void btn_back_CategoryPositionTapped(object sender, EventArgs e)
        {
            AppModel.Instance.LastSelectedPosition = null;
            ShowOrderCategoryPage(AppModel.Instance.LastSelectedOrder);
        }




        public async void btn_AuswahlAnzeigen(object sender, EventArgs e)
        {
            AuswahlAnzeigenTapped_Done(!panelShowSelectedPos_Container.IsVisible);
        }
        public async void AuswahlAnzeigenTapped_Done(bool visible)
        {
            if (visible)
            {
                panelShowSelectedPos_Container.IsVisible = visible;
                selectedPosList_container.Children.Add(LeistungWSO.GetSelectedPositionListView(
                    AppModel.Instance, new Command<LeistungWSO>(RemoveSelectPositionFromToWork),
                    new Command<ChangeSelectedMuellPos>(ChangeSelectedMuellPos)));
            }
            else
            {
                panelShowSelectedPos_Container.IsVisible = visible;
                selectedPosList_container.Children.Clear();
            }
        }
        public async void ChangeSelectedMuellPos(ChangeSelectedMuellPos obj)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            popupContainer_quest_changemuellpos.IsVisible = true;

            btn_quest_changemuellPos.GestureRecognizers.Clear();
            var tgr_quest_changemuellPos = new TapGestureRecognizer();
            tgr_quest_changemuellPos.Tapped += (object o, TappedEventArgs ev) => { ChangeSelectedMuellPosNow(obj, 1); };
            btn_quest_changemuellPos.GestureRecognizers.Add(tgr_quest_changemuellPos);

            btn_quest_changemuellpos_raus.GestureRecognizers.Clear();
            var tgr_quest_changemuellpos_raus = new TapGestureRecognizer();
            tgr_quest_changemuellpos_raus.Tapped += (object o, TappedEventArgs ev) => { ChangeSelectedMuellPosNow(obj, 0); };
            btn_quest_changemuellpos_raus.GestureRecognizers.Add(tgr_quest_changemuellpos_raus);

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void ChangeSelectedMuellPosNow(ChangeSelectedMuellPos obj, int status)
        {
            // !!! Hier ist der Status INVERS   - rausstellen heist hier status = 0!
            obj.pos.inout.inout = status; //obj.pos.inout.inout == 1 ? 0 : 1;
            obj.img.Source = obj.pos.inout.inout == 0 ? "muell_out_tonne.png" : "muell_in.png";
            obj.img2.Source = obj.pos.inout.inout == 0 ? "muell_out.png" : "muell_in_tonne.png";
            obj.lb.Text = obj.pos.inout.inout == 0 ? "Ich werde RAUSSTELLEN" : "Ich werde REINSTELLEN";
            obj.lb.TextColor = Color.FromArgb(obj.pos.inout.inout == 0 ? "#dd0000" : "#00aa00");

            popupContainer_quest_changemuellpos.IsVisible = false;
        }


        public async void btn_AuswahlAnzeigen_Again(object sender, EventArgs e)
        {
            AuswahlAnzeigenTapped_Again_Done(!panelShowSelectedPos_Container.IsVisible);
        }
        public async void AuswahlAnzeigenTapped_Again_Done(bool visible)
        {
            if (visible)
            {
                panelShowSelectedPos_Container.IsVisible = visible;
                selectedPosList_container.Children.Add(LeistungWSO.GetSelectedPositionAgainListView(
                    new Command<LeistungWSO>(RemoveSelectPositionAgainFromToWork),
                    new Command<ChangeSelectedMuellPos>(ChangeSelectedMuellPos)));
            }
            else
            {
                panelShowSelectedPos_Container.IsVisible = visible;
                selectedPosList_container.Children.Clear();
            }
        }


        public async void StartSelectedPos(object sender, EventArgs e)
        {
            if (AppModel.Instance.allSelectedPositionToWork.Count > 0)
            {
                StartSelectedPosTapped_Done();
            }
            else
            {
                if (AppModel.Instance.allSelectedPositionAgainToWork.Count > 0)
                {
                    btn_startselectedwork.GestureRecognizers.Clear();
                    var tgr_btn_startselectedwork = new TapGestureRecognizer();
                    tgr_btn_startselectedwork.Tapped += StartselectedworkAgainTapped;
                    btn_startselectedwork.GestureRecognizers.Add(tgr_btn_startselectedwork);
                    btn_startselectedcancel.GestureRecognizers.Clear();
                    var tgr_btn_startselectedcancel = new TapGestureRecognizer();
                    tgr_btn_startselectedcancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_startwork.IsVisible = false; };
                    btn_startselectedcancel.GestureRecognizers.Add(tgr_btn_startselectedcancel);
                    btn_startselectedwork_text.Text = "Möchten Sie wirklich Ihre Auswahl jetzt zur laufenden Ausführung nachbuchen?";
                    popupContainer_quest_startwork.IsVisible = true;
                    //StartSelectedPosAgainTapped_Done();
                }
            }
        }
        //public async void StartselectedworkTapped(object sender, EventArgs e)
        //{
        //    popupContainer_quest_startwork.IsVisible = false;
        //    StartSelectedPosTapped_Done();
        //}
        public async void StartSelectedPosTapped_Done(object sender = null, EventArgs e = null)
        {
            AuswahlAnzeigenTapped_Done(false);

            var isOthersAsProdukte = AppModel.Instance.allSelectedPositionToWork.Find(l => l.art != "Produkt");
            var onlyProdukte = isOthersAsProdukte == null;

            var geo = AppModel.Instance.LocationStr;
            string geoMessage = "";
            if (geo != null && geo.Length > 0)
            {
                geoMessage = geo.Substring(0, 1) == "#" ? geo.Substring(1) : "GPS OK";
                geo = geoMessage == "GPS OK" ? geo : null;
            }
            else
            {
                geo = null;
                geoMessage = "geo = null";
            }
            //AppModel.Logger.Info("Info: --------------- STARTE ARBEITEN => StartSelectedPosTapped_Done");
            //AppModel.Logger.Info("Info: Verwendete GPS (" + geoMessage + " - " + AppModel.Instance.LocationStr + ")");

            var latin = geo != null ? geo.Split(';')[0] : "";
            var lonin = geo != null ? (geo.Split(';').Length > 0 ? geo.Split(';')[1] : "") : "";

            AppModel.Instance.allPositionInWork = new LeistungPackWSO
            {
                latin = latin,
                lonin = lonin,
                messagein = geoMessage,
                preview = true,
                status = 0,   // 0 = in Arbeit , 1 = Ausgesetzt , 2 = Fertig
                startticks = DateTime.Now.Ticks,
                endticks = DateTime.Now.Ticks,
                personid = AppModel.Instance.Person.id,
            };

            AppModel.Instance.allPositionDirectWork = new LeistungPackWSO
            {
                latin = latin,
                lonin = lonin,
                messagein = geoMessage,
                latout = "",
                lonout = "",
                messageout = "",
                preview = false,
                status = 2,   // 0 = in Arbeit , 1 = Ausgesetzt , 2 = Fertig
                startticks = DateTime.Now.Ticks,
                endticks = DateTime.Now.Ticks,
                personid = AppModel.Instance.Person.id,
            };

            AppModel.Instance.allPositionDirectWork.endticks = AppModel.Instance.allPositionDirectWork.startticks;

            AppModel.Instance.allSelectedPositionToWork.ForEach(l =>
            {
                decimal anzahlValue = 1m;
                var rawProduktAnzahl = l.produktAnzahl?.Trim();

                if (!string.IsNullOrWhiteSpace(rawProduktAnzahl))
                {
                    if (decimal.TryParse(rawProduktAnzahl, NumberStyles.Number, CultureInfo.GetCultureInfo("de-DE"), out var deValue) && deValue > 0)
                    {
                        anzahlValue = deValue;
                    }
                    else if (decimal.TryParse(rawProduktAnzahl, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue) && invariantValue > 0)
                    {
                        anzahlValue = invariantValue;
                    }
                }

                var work = new LeistungInWorkWSO
                {
                    id = l.id,
                    gruppeid = l.gruppeid,
                    objektid = l.objektid,
                    auftragid = l.auftragid,
                    kategorieid = l.kategorieid,
                    anzahl = Utils.formatDEStr(anzahlValue),
                    bemerkungen = null,
                    inout = l.inout,
                };

                if ((l.type == "1" && l.art == "Leistung") || onlyProdukte)
                {
                    AppModel.Instance.allPositionDirectWork.leistungen.Add(work);
                }
                else
                {
                    AppModel.Instance.allPositionInWork.leistungen.Add(work);
                }
            });

            //var dummyLeistungInWork = new List<LeistungInWorkWSO>();
            if (AppModel.Instance.allPositionDirectWork.leistungen.Count > 0)
            {
                var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(AppModel.Instance.allPositionDirectWork.startticks), -2);
                AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(p =>
                        {
                            var foundPos = AppModel.Instance.allPositionDirectWork.leistungen.Find(lei => lei.id == p.id);
                            if (foundPos != null)
                            {
                                foundPos.lastwork = lastWorkTicks;
                                foundPos.workat = "";
                                p.lastwork = lastWorkTicks;
                                p.workat = "";
                                p.selected = false;
                                if (p.muell == 1 && p.inout != null)
                                {
                                    p.inout.inout = p.inout.inout == 1 ? 0 : 1;
                                    //dummyLeistungInWork.Add(foundPos);
                                }
                            }
                        });
                    });
                });

                List<LeistungInWorkWSO> newleis = new List<LeistungInWorkWSO>();
                AppModel.Instance.allPositionDirectWork.leistungen.ForEach(l =>
                {
                    newleis.Add(SetPlanPersonMobileToLeistungInWork(l));
                });
                AppModel.Instance.allPositionDirectWork.leistungen = newleis;

                BuildingWSO.Save(AppModel.Instance, AppModel.Instance.LastBuilding);
                LeistungPackWSO.ToUploadStack(AppModel.Instance, AppModel.Instance.allPositionDirectWork);
                CheckAllSyncFromUpload(); //SyncPosition();
                UpdateObjektPersonPlanMobileAfterUpload(AppModel.Instance.allPositionDirectWork);
            }

            AppModel.Instance.allPositionDirectWork = null;

            if (AppModel.Instance.allPositionInWork.leistungen.Count > 0)
            {
                LeistungPackWSO.Save(AppModel.Instance, AppModel.Instance.allPositionInWork);
                SyncPosition(AppModel.Instance.allPositionInWork.preview);
            }
            else
            {
                AppModel.Instance.allPositionInWork = null;
            }

            // Zurücksetzten aller States für die Auswahl der Ausführungen
            AppModel.Instance.LastSelectedOrder = null;
            AppModel.Instance.LastSelectedCategory = null;
            AppModel.Instance.LastSelectedPosition = null;
            AppModel.Instance.allPositionInShowingListView = new Dictionary<int, Border>();
            AppModel.Instance.allPositionInShowingSmallListView = new Dictionary<int, SwipeView>();
            AppModel.Instance.allSelectedPositionToWork = new List<LeistungWSO>();
            // alle selektionen und disabled zurücksetzen 
            AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                o.kategorien.ForEach(c =>
                {
                    c.leistungen.ForEach(l =>
                    {
                        l.selected = false;
                        l.disabled = false;
                    });
                });
            });

            ShowMainPage();
        }







        public async void StartselectedworkAgainTapped(object sender, EventArgs e)
        {
            popupContainer_quest_startwork.IsVisible = false;
            StartSelectedPosAgainTapped_Done();
        }
        public async void StartSelectedPosAgainTapped_Done_old(object sender = null, EventArgs e = null)
        {
            AuswahlAnzeigenTapped_Done(false);


            AppModel.Instance.allSelectedPositionAgainToWork.ForEach(l =>
            {
                var work = new LeistungInWorkWSO
                {
                    id = l.id,
                    gruppeid = l.gruppeid,
                    objektid = l.objektid,
                    auftragid = l.auftragid,
                    kategorieid = l.kategorieid,
                    anzahl = Utils.formatDEStr(decimal.Parse(l.produktAnzahl, CultureInfo.GetCultureInfo("de-DE")) > 0 ? decimal.Parse(l.produktAnzahl, CultureInfo.GetCultureInfo("de-DE")) : 1),
                    bemerkungen = null,
                    inout = l.inout,
                    again = 1,
                };
                AppModel.Instance.allPositionInWork.leistungen.Add(work);
            });
            var dummyLeistungInWork = new List<LeistungInWorkWSO>();

            if (AppModel.Instance.allPositionInWork.leistungen.Count > 0)
            {
                LeistungPackWSO.Save(AppModel.Instance, AppModel.Instance.allPositionInWork);
                CheckAllSyncFromUpload(); //SyncPositionAgain();
            }
            else
            {
                AppModel.Instance.allPositionInWork = null;
            }

            // Zurücksetzten aller States für die Auswahl der Ausführungen
            AppModel.Instance.LastSelectedOrder = null;
            AppModel.Instance.LastSelectedCategory = null;
            AppModel.Instance.LastSelectedPosition = null;
            AppModel.Instance.LastSelectedOrderAgain = null;
            AppModel.Instance.LastSelectedCategoryAgain = null;
            AppModel.Instance.LastSelectedPositionAgain = null;
            AppModel.Instance.allPositionInShowingListView = new Dictionary<int, Border>();
            AppModel.Instance.allPositionInShowingSmallListView = new Dictionary<int, SwipeView>();
            AppModel.Instance.allSelectedPositionToWork = new List<LeistungWSO>();

            AppModel.Instance.allPositionAgainInShowingListView = new Dictionary<int, Border>();
            AppModel.Instance.allPositionAgainInShowingSmallListView = new Dictionary<int, SwipeView>();
            AppModel.Instance.allSelectedPositionAgainToWork = new List<LeistungWSO>();

            // alle selektionen und disabled zurücksetzen 
            AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                o.kategorien.ForEach(c =>
                {
                    c.leistungen.ForEach(l =>
                    {
                        l.selected = false;
                        l.disabled = false;
                        l.objekt = null;
                    });
                });
            });

            ShowMainPage();
        }


        public async void StartSelectedPosAgainTapped_Done(object sender = null, EventArgs e = null)
        {
            AuswahlAnzeigenTapped_Done(false);

            AppModel.Instance.allSelectedPositionAgainToWork.ForEach(l =>
            {
                decimal anzahlValue = 1m;
                var rawProduktAnzahl = l.produktAnzahl?.Trim();

                if (!string.IsNullOrWhiteSpace(rawProduktAnzahl))
                {
                    if (decimal.TryParse(rawProduktAnzahl, NumberStyles.Number, CultureInfo.GetCultureInfo("de-DE"), out var deValue) && deValue > 0)
                    {
                        anzahlValue = deValue;
                    }
                    else if (decimal.TryParse(rawProduktAnzahl, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue) && invariantValue > 0)
                    {
                        anzahlValue = invariantValue;
                    }
                }

                var work = new LeistungInWorkWSO
                {
                    id = l.id,
                    gruppeid = l.gruppeid,
                    objektid = l.objektid,
                    auftragid = l.auftragid,
                    kategorieid = l.kategorieid,
                    anzahl = Utils.formatDEStr(anzahlValue),
                    bemerkungen = null,
                    inout = l.inout,
                    again = 1,
                };

                AppModel.Instance.allPositionInWork.leistungen.Add(work);
            });

            var dummyLeistungInWork = new List<LeistungInWorkWSO>();

            if (AppModel.Instance.allPositionInWork.leistungen.Count > 0)
            {
                LeistungPackWSO.Save(AppModel.Instance, AppModel.Instance.allPositionInWork);
                CheckAllSyncFromUpload(); //SyncPositionAgain();
            }
            else
            {
                AppModel.Instance.allPositionInWork = null;
            }

            // Zurücksetzten aller States für die Auswahl der Ausführungen
            AppModel.Instance.LastSelectedOrder = null;
            AppModel.Instance.LastSelectedCategory = null;
            AppModel.Instance.LastSelectedPosition = null;
            AppModel.Instance.LastSelectedOrderAgain = null;
            AppModel.Instance.LastSelectedCategoryAgain = null;
            AppModel.Instance.LastSelectedPositionAgain = null;
            AppModel.Instance.allPositionInShowingListView = new Dictionary<int, Border>();
            AppModel.Instance.allPositionInShowingSmallListView = new Dictionary<int, SwipeView>();
            AppModel.Instance.allSelectedPositionToWork = new List<LeistungWSO>();

            AppModel.Instance.allPositionAgainInShowingListView = new Dictionary<int, Border>();
            AppModel.Instance.allPositionAgainInShowingSmallListView = new Dictionary<int, SwipeView>();
            AppModel.Instance.allSelectedPositionAgainToWork = new List<LeistungWSO>();

            // alle selektionen und disabled zurücksetzen 
            AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                o.kategorien.ForEach(c =>
                {
                    c.leistungen.ForEach(l =>
                    {
                        l.selected = false;
                        l.disabled = false;
                        l.objekt = null;
                    });
                });
            });

            ShowMainPage();
        }



        // ClearLastBuilding
        public void btn_ClearLastBuildingTapped(object sender, EventArgs e)
        {
            if (AppModel.Instance.allPositionInWork != null && AppModel.Instance.allPositionInWork.leistungen.Count > 0)
            {
                btn_quest_removeLastBuildingSave.GestureRecognizers.Clear();
                var tgr_save = new TapGestureRecognizer();
                tgr_save.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_removeLastBuilding.IsVisible = false; ShowRunningWorksView(); };
                tgr_save.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_removeLastBuilding.IsVisible = false; ShowRunningWorksView(); };
                btn_quest_removeLastBuildingSave.GestureRecognizers.Add(tgr_save);
                btn_quest_removeLastBuildingCancel.GestureRecognizers.Clear();
                var tgr_cancel = new TapGestureRecognizer();
                tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_removeLastBuilding.IsVisible = false; };
                tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_removeLastBuilding.IsVisible = false; };
                btn_quest_removeLastBuildingCancel.GestureRecognizers.Add(tgr_cancel);

                popupContainer_quest_removeLastBuilding.IsVisible = true;
            }
            else
            {
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
                ShowMainPage();
            }
        }
        // ShowBuildingOrder List    
        public void btn_AuftraegeAuswaehlen(object sender, EventArgs e)
        {
            AuftraegeAuswaehlenView();
        }

        // ShowBuildingOrder List    
        public void btn_ShowRunningWorks(object sender, EventArgs e)
        {
            ShowRunningWorksView();
        }
        public void btn_RunningWorksBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }
        public void btn_RunningWorksOverTapped(object sender, EventArgs e)
        {
            // Dialog öffnen
            popupContainer_quest_endwork.IsVisible = true;
        }
        public async void ScanRunningWorksOver(object sender, EventArgs e)
        {
            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                popupContainer_quest_endwork.IsVisible = false;
                MethodAfterOutScan();
            }
            else
            {
                popupContainer_quest_endwork.IsVisible = false;
                ShowBuildingOutScanPage();
            }
        }
        public async void SavesRunningWorksOver(bool isDiffObjekt)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            var geo = AppModel.Instance.LocationStr;
            string geoMessage = "";
            try
            {
                if (geo != null && geo.Length > 0)
                {
                    geoMessage = geo.Substring(0, 1) == "#" ? geo.Substring(1) : "GPS OK";
                    geo = geoMessage == "GPS OK" ? geo : null;
                }
                else
                {
                    geo = null;
                    geoMessage = "geo = null";
                }
            }
            catch (Exception) { }
            //AppModel.Logger.Info("Info: --------------- BEENDE ARBEITEN => SavesRunningWorksOver");
            //AppModel.Logger.Info("Info: Verwendete GPS (" + geoMessage + " - " + AppModel.Instance.LocationStr + ")");

            var latout = geo != null ? geo.Split(';')[0] : "";
            var lonout = geo != null ? (geo.Split(';').Length > 0 ? geo.Split(';')[1] : "") : "";


            if (AppModel.Instance.allPositionInWork == null)
            {
                popupContainer_quest_endwork.IsVisible = false;
                await Task.Delay(1);
                overlay.IsVisible = false;
                ShowMainPage();
            }
            else
            {
                AppModel.Instance.allPositionInWork.endticks = DateTime.Now.Ticks;
                AppModel.Instance.allPositionInWork.latout = latout;
                AppModel.Instance.allPositionInWork.lonout = lonout;
                AppModel.Instance.allPositionInWork.messageout = geoMessage;
                AppModel.Instance.allPositionInWork.preview = false;
                AppModel.Instance.allPositionInWork.status = 2;
                AppModel.Instance.allPositionInWork.personid = AppModel.Instance.Person.id;
                AppModel.Instance.allPositionInWork.diffObjekt = isDiffObjekt ? 1 : 0;

                var dummyLeistungInWork = new List<LeistungInWorkWSO>();
                var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(AppModel.Instance.allPositionInWork.endticks), -2);
                AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(p =>
                        {
                            var foundPos = AppModel.Instance.allPositionInWork.leistungen.Find(lei => lei.id == p.id);
                            if (foundPos != null)
                            {
                                foundPos.lastwork = lastWorkTicks;
                                foundPos.workat = "";
                                p.lastwork = lastWorkTicks;
                                p.workat = "";
                                if (p.muell == 1 && p.inout != null)
                                {
                                    dummyLeistungInWork.Add(foundPos);
                                }
                            }
                        });
                    });
                });
                List<LeistungInWorkWSO> newleis = new List<LeistungInWorkWSO>();
                AppModel.Instance.allPositionInWork.leistungen.ForEach(l =>
                {
                    newleis.Add(SetPlanPersonMobileToLeistungInWork(l));
                });
                AppModel.Instance.allPositionInWork.leistungen = newleis;

                BuildingWSO.Save(AppModel.Instance, AppModel.Instance.LastBuilding);
                LeistungPackWSO.ToUploadStack(AppModel.Instance, AppModel.Instance.allPositionInWork); // Beendete Arbeiten in Stacklist für Upload
                LeistungPackWSO.Delete(AppModel.Instance);// Aktive Arbeiten aus Datenspeicher löschen

                UpdatePersonPlanMobile(AppModel.Instance.allPositionInWork);

                AppModel.Instance.allPositionInWork = null;

                if (dummyLeistungInWork.Count > 0)
                {
                    AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                    {
                        o.kategorien.ForEach(c =>
                        {
                            LeistungInWorkWSO foundPos = null;
                            c.leistungen.ForEach(p =>
                            {
                                foundPos = dummyLeistungInWork.Find(lei => lei.id == p.id);
                                if (foundPos != null)
                                {
                                    if (p.muell == 1 && p.inout != null)
                                    {
                                        p.inout.inout = p.inout.inout == 1 ? 0 : 1;
                                    }
                                }
                            });
                        });
                    });
                    BuildingWSO.Save(AppModel.Instance, AppModel.Instance.LastBuilding);
                }
                dummyLeistungInWork = null;

                // Versuche direkt zu senden sonst von Stack!
                CheckAllSyncFromUpload(); //SyncPosition();

                // Dialog schliessen
                popupContainer_quest_endwork.IsVisible = false;

                //await Task.Delay(1);
                //overlay.IsVisible = false;
                ShowMainPage();
            }
        }

        public LeistungInWorkWSO SetPlanPersonMobileToLeistungInWork(LeistungInWorkWSO lei)
        {
            if (!AppModel.Instance.AppControll.showObjektPlans) { return lei; }
            PlanPersonMobile ppm = null;
            List<PlanPersonMobile> ppms = new List<PlanPersonMobile>(); ;
            if (AppModel.Instance.PlanResponse != null && AppModel.Instance.PlanResponse.planweek != null && AppModel.Instance.PlanResponse.planweek.days != null)
            {
                AppModel.Instance.PlanResponse.planweek.days.ForEach(day =>
                {
                    day.ForEach(item =>
                    {
                        if (item.haswork == 0 && item.muelltoid > 0 && !String.IsNullOrWhiteSpace(item.info))
                        {
                            string[] all = item.info.Split('#');
                            var leiid = Int32.Parse(all[3]);
                            if (leiid == lei.id)
                            {
                                ppms.Add(item);
                            }
                        }
                    });
                });
            }
            if (ppms.Count > 0)
            {
                var daynow = (int)DateTime.Now.DayOfWeek;
                List<PlanPersonMobile> ppmsVorherHeute = new List<PlanPersonMobile>();
                List<PlanPersonMobile> ppmsZukunft = new List<PlanPersonMobile>();
                if (daynow == 0)
                {
                    ppmsVorherHeute = ppms.Where(i => i.day > -1 && i.day < 7).OrderByDescending(i => i.day).ToList();
                }
                else
                {
                    ppmsVorherHeute = ppms.Where(i => i.day > 0 && i.day <= daynow).OrderByDescending(i => i.day).ToList();
                    ppmsZukunft = ppms.Where(i => i.day == 0 || i.day > daynow).OrderBy(i => i.day).ToList();
                }
                if (ppmsVorherHeute != null && ppmsVorherHeute.Count > 0)
                {
                    ppm = ppmsVorherHeute[0];
                }
                else
                {
                    if (ppmsZukunft != null && ppmsZukunft.Count > 0)
                    {
                        ppm = ppmsZukunft[0];
                    }
                }
            }
            lei.ppm = ppm;
            return lei;
        }

        public void UpdateObjektPersonPlanMobileAfterUpload(LeistungPackWSO pack)
        {
            if (!AppModel.Instance.AppControll.showObjektPlans) { return; }
            if (AppModel.Instance.PlanResponse.selectedPerson != null)
            {
                ReloadPlanData(0);
            }
            else
            {
                Update_PlanTabs((int)DateTime.Now.DayOfWeek);
            }
            return;
            /*
            
            if (pack.leistungen != null && pack.leistungen.Count > 0)
            {
                List<string> katNames = new List<string>();

                var today = (int)DateTime.Now.DayOfWeek;

                pack.leistungen.ForEach(l =>
                {
                    AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(b =>
                    {
                        b.kategorien.ForEach(k =>
                        {
                            if (k.id == l.kategorieid)
                            {
                                katNames.Add(k.titel);
                            }
                        });
                    });
                });

                // Geleistete Arbeiten abhacken von PlanListe Today
                // gib alle Plans die von Heute oder vorher die nch nicht bearbeitet wurden von diesem Objekt zurück incl. Kategorie(NachBedarf)
                List<PlanPersonMobile> plans = new List<PlanPersonMobile>();
                List<PlanPersonMobile> sendplans = new List<PlanPersonMobile>();
                if (AppModel.Instance.PlanResponse.planweek != null)
                {
                    //if (today == 0)// Sonntag 
                    //{
                    //    plans = AppModel.Instance.PlanResponse.week.FindAll(p => p.objektid == AppModel.Instance.LastBuilding.id && p.haswork == 0 && p.muelltoid == 0);
                    //}
                    //else
                    //{
                    //    plans = AppModel.Instance.PlanResponse.week.FindAll(p => p.objektid == AppModel.Instance.LastBuilding.id && p.day != 0 && p.day <= today && p.haswork == 0 && p.muelltoid == 0);
                    //}
                }
                else { plans = null; }

                if (plans != null && plans.Count > 0)
                {
                    plans.ForEach(p =>
                    {
                        int haswork = 1;
                        string newkatname = "";
                        if (p.day > -1)
                        {
                            p.haswork = haswork;
                            p.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
                            p.lastworker = AppModel.Instance.Person.vorname + " " + AppModel.Instance.Person.name;
                        }
                        else
                        {
                            // Kategorien nach Bedarf prüfen ob hier gearbeitet wurde
                            if (katNames.IndexOf(p.katname) > -1)
                            {
                                p.haswork = haswork;
                                p.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
                                p.lastworker = AppModel.Instance.Person.vorname + " " + AppModel.Instance.Person.name;
                            }
                        }
                    });
                }

                ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);

                if (AppModel.Instance.PlanResponse.selectedPerson != null)
                {
                    ReloadPlanData();
                }
                else
                {
                    Update_PlanTabs(today);
                }
            }
            */
        }




        public void UpdatePersonPlanMobile(LeistungPackWSO pack)
        {
            if (AppModel.Instance.PlanResponse.selectedPerson != null)
            {
                ReloadPlanData(0);
            }
            else
            {
                Update_PlanTabs((int)DateTime.Now.DayOfWeek);
            }
            return;

            /*
            if (pack.leistungen != null && pack.leistungen.Count > 0)
            {
                Int32 objektid = 0;
                List<string> katNames = new List<string>();
                BuildingWSO building = null;


                List<PlanPersonMobile> muellOPWM = new List<PlanPersonMobile>();
                List<PlanPersonMobile> foundMuellOPWM = new List<PlanPersonMobile>();
                //var muellObjbPlanWeekMobile = null;// AppModel.Instance.PlanResponse.week.Where(p => p.day > -1 && p.katname.Contains("#")).ToList();
                //muellObjbPlanWeekMobile.ForEach(p => {
                //    var a = p.katname.Split('#');
                //    if (a.Length > 2)
                //    {
                //        p.leiid = Int32.Parse(a[3]);
                //        muellOPWM.Add(p);

                //    }
                //});
                List<LeistungInWorkWSO> holdLeiIds = new List<LeistungInWorkWSO>();
                pack.leistungen.ForEach(l =>
                {
                    var muellLeiFound = muellOPWM.Find(o => o.leiid == l.id);
                    if (muellLeiFound == null)
                    {
                        holdLeiIds.Add(l);
                    }
                    else
                    {
                        foundMuellOPWM.Add(muellLeiFound);
                    }
                });

                var today = (int)DateTime.Now.DayOfWeek;

                if (foundMuellOPWM.Count > 0)
                {
                    foundMuellOPWM.ForEach(p =>{
                        int haswork = 1;
                        if (p.muelltoid > 0)
                        {
                            string[] all = p.info.Split('#');
                            string name = all[0];
                            string col = all[1];
                            string statem = all[2];
                            string leiid = all[3];
                            if (statem == "3")
                            {
                                statem = "2";
                                haswork = 0;
                                p.info = name + "#" + col + "#2#" + leiid; ;
                            }
                        }
                        p.haswork = haswork;
                        p.lastwork = new DateTime(pack.endticks).ToString("dd.MM.yyyy - HH:mm");
                        p.lastworker = AppModel.Instance.Person.vorname + " " + AppModel.Instance.Person.name;
                    });
                    ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                }

                pack.leistungen = holdLeiIds;

                if (pack.leistungen.Count > 0)
                { //Leistungen waren nur Müllpositionen 

                    pack.leistungen.ForEach(l =>
                    {
                        objektid = l.objektid;
                        if (building == null)
                        {
                            building = BuildingWSO.LoadBuilding(AppModel.Instance, objektid);
                        }
                        building.ArrayOfAuftrag.ForEach(b =>
                        {
                            b.kategorien.ForEach(k =>
                            {
                                if (k.id == l.kategorieid)
                                {
                                    katNames.Add(k.titel);
                                }
                            });
                        });
                    });

                    // Geleistete Arbeiten abhacken von PlanListe Today
                    // gib alle Plans die von Heute oder vorher die nch nicht bearbeitet wurden von diesem Objekt zurück incl. Kategorie(NachBedarf)
                    List<PlanPersonMobile> plans = new List<PlanPersonMobile>();
                    List<PlanPersonMobile> sendplans = new List<PlanPersonMobile>();
                    if (AppModel.Instance.PlanResponse.planweek != null)
                    {
                        //if (today == 0)// Sonntag 
                        //{
                        //    plans = AppModel.Instance.PlanResponse.week.FindAll(p => p.objektid == objektid && p.haswork == 0);
                        //}
                        //else
                        //{
                        //    plans = AppModel.Instance.PlanResponse.week.FindAll(p => p.objektid == objektid && p.day != 0 && p.day <= today && p.haswork == 0);
                        //}
                    }
                    else { plans = null; }

                    if (plans != null && plans.Count > 0)
                    {
                        plans.ForEach(p =>
                        {
                            int haswork = 1;
                            string newkatname = "";
                            if (p.muelltoid > 0)
                            {
                                var lei = BuildingWSO.FindLeistung(p.leiid);
                                var katTitel = BuildingWSO.FindKategorieName(lei.kategorieid);
                                if (katNames.Contains(katTitel))
                                {
                                    string[] all = p.info.Split('#');
                                    string name = all[0];
                                    string col = all[1];
                                    string statem = all[2];
                                    string leiid = all[3];
                                    if (statem == "3")
                                    {
                                        statem = "2";
                                        haswork = 0;
                                        newkatname = name + "#" + col + "#2#" + leiid;
                                        p.info = newkatname;
                                    }
                                    p.haswork = haswork;
                                    p.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
                                    p.lastworker = AppModel.Instance.Person.vorname + " " + AppModel.Instance.Person.name;
                                }
                            }
                            else
                            {
                                if (p.day > -1)
                                {
                                    p.haswork = haswork;
                                    p.lastwork = new DateTime(AppModel.Instance.allPositionInWork.endticks).ToString("dd.MM.yyyy - HH:mm");
                                    p.lastworker = AppModel.Instance.Person.vorname + " " + AppModel.Instance.Person.name;
                                }
                                else
                                {
                                    // Kategorien nach Bedarf prüfen ob hier gearbeitet wurde
                                    if (katNames.IndexOf(p.katname) > -1)
                                    {
                                        p.haswork = haswork;
                                        p.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
                                        p.lastworker = AppModel.Instance.Person.vorname + " " + AppModel.Instance.Person.name;
                                    }
                                }
                            }
                        });
                        ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                    }
                }

                if (AppModel.Instance.PlanResponse.selectedPerson != null)
                {
                    ReloadPlanData();
                }
                else
                {
                    Update_PlanTabs(today);
                }
            }
        */
        }



        public void ShowSendLog(object sender, EventArgs e)
        {
            double w = screenWidthDp;
            double h = screenHeightDp;
            popupContainer_container_sendlog.WidthRequest = w - 40;
            //popupContainer_container_sendlog.Margin = new Thickness(0,100,0,0);
            btn_sendlogtosupport.GestureRecognizers.Clear();
            var tgr_over = new TapGestureRecognizer();
            tgr_over.Tapped -= btn_nlogsendTapped;
            tgr_over.Tapped += btn_nlogsendTapped;
            btn_sendlogtosupport.GestureRecognizers.Add(tgr_over);

            btn_cancellogtosupport.GestureRecognizers.Clear();
            var tgr_cancel = new TapGestureRecognizer();
            tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_sendlog.IsVisible = false; };
            tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_sendlog.IsVisible = false; };
            btn_cancellogtosupport.GestureRecognizers.Add(tgr_cancel);

            // Dialog öffnen
            popupContainer_quest_sendlog.IsVisible = true;
        }
        public void ShowSendLog_fail()
        {
            double w = screenWidthDp;
            double h = screenHeightDp;
            popupContainer_container_sendlog_fail.WidthRequest = w - 40;
            //popupContainer_container_sendlog.Margin = new Thickness(0,100,0,0);
            btn_cancellogtosupport_fail.GestureRecognizers.Clear();
            var tgr_cancel = new TapGestureRecognizer();
            tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_sendlog_fail.IsVisible = false; };
            tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_sendlog_fail.IsVisible = false; };
            btn_cancellogtosupport_fail.GestureRecognizers.Add(tgr_cancel);

            // Dialog öffnen
            popupContainer_quest_sendlog_fail.IsVisible = true;
        }
        public async void btn_nlogsendTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            popupContainer_quest_sendlog.IsVisible = false;
            await Task.Delay(1);

            var ok = AppModel.Instance.SendLogZipFile();
            await Task.Delay(2000);
            if (ok)
            {
                overlay.IsVisible = false;
            }
            else
            {
                await Task.Delay(1);
                overlay.IsVisible = false;
                ShowSendLog_fail();
            }
        }



        public void btn_ShowObjectValuesTapped(object sender, EventArgs e)
        {
            ShowObjectValuesView();
        }
        public void btn_CloseObjectValuesTapped(object sender, EventArgs e)
        {
            ShowMainPage();
        }
        public void btn_CloseObjectValuesEditTapped(object sender, EventArgs e)
        {
            ObjectValuesPage_Edit_Container.IsVisible = false;
            //ShowObjectValuesView();
        }
        public async void btn_ShowNoticeTapped(object sender, EventArgs e)
        {
            await ShowNoticeView(false, null, null);
        }
        public async void btn_ShowNoticePrioTapped(object sender, EventArgs e)
        {
            await ShowNoticeView(true, null, null);
        }

        public void btn_DSGVOBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }




        //public void OpenLanguage(object sender, EventArgs e)
        //{
        //    double w = screenWidthDp;
        //    double h = screenHeightDp;
        //    langListView.SelectedItem = null;
        //    popupContainer_quest_langpicker_inner.HeightRequest = h - 100;
        //    popupContainer_quest_langpicker_inner.WidthRequest = w - 40;
        //    popupContainer_quest_langpicker.IsVisible = true;

        //    var empList = AppModel.Instance.Langs;
        //    var groupedData =
        //        empList.OrderBy(el => el.text)
        //            .GroupBy(el => el.text[0].ToString())
        //            .Select(el => new ObservableLangItemCollection<string, Lang>(el))
        //            .ToList();

        //    BindingContext = new ObservableCollection<ObservableLangItemCollection<string, Lang>>(groupedData);

        //}
        //private void langListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        //    {
        //        var l = (Lang)e.CurrentSelection[0];
        //        AppModel.Instance.Lang = l;
        //        CloseLanguage();
        //        ShowTranslate(null, null);
        //    }
        //}
        //public async void CloseLanguage()
        //{
        //    await Task.Delay(1);
        //    popupContainer_quest_langpicker.IsVisible = false;
        //}

        //public void ShowTranslate(object sender, EventArgs e)
        //{
        //    double w = screenWidthDp;
        //    double h = screenHeightDp;
        //    popupContainer_container_changelang.WidthRequest = w - 40;
        //    //popupContainer_container_changelang.Margin = new Thickness(0,100,0,0);
        //    btn_changelang.GestureRecognizers.Clear();
        //    //var tgr_over = new TapGestureRecognizer();
        //    //tgr_over.Tapped -= btn_translateTapped;
        //    //tgr_over.Tapped += btn_translateTapped;
        //    //btn_changelang.GestureRecognizers.Add(tgr_over);

        //    btn_cancellang.GestureRecognizers.Clear();
        //    var tgr_cancel = new TapGestureRecognizer();
        //    tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_changelang.IsVisible = false; };
        //    tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_changelang.IsVisible = false; };
        //    btn_cancellang.GestureRecognizers.Add(tgr_cancel);

        //    popupContainer_container_changelang_titel.Text = "Kategorien und Leistungen ändern in (" + AppModel.Instance.Lang.text.Replace("(Standard)", "") + ")";

        //    // Dialog öffnen
        //    popupContainer_quest_changelang.IsVisible = true;
        //}
        //public void ShowTranslate_fail()
        //{
        //    double w = screenWidthDp;
        //    double h = screenHeightDp;
        //    popupContainer_quest_changelang_fail.WidthRequest = w - 40;
        //    //popupContainer_container_sendlog.Margin = new Thickness(0,100,0,0);
        //    btn_cancellogtosupport_fail.GestureRecognizers.Clear();
        //    var tgr_cancel = new TapGestureRecognizer();
        //    tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_changelang_fail.IsVisible = false; };
        //    tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_changelang_fail.IsVisible = false; };
        //    btn_cancellogtosupport_fail.GestureRecognizers.Add(tgr_cancel);

        //    // Dialog öffnen
        //    popupContainer_quest_changelang_fail.IsVisible = true;
        //}

        //public async void btn_translateTapped(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        overlay.IsVisible = true;
        //        await Task.Delay(1);

        //        if (AppModel.Instance.Lang.lang.ToLower() == "de")
        //        {
        //            // fertig
        //            popupContainer_container_changelang_status.Text = "";
        //            popupContainer_quest_changelang.IsVisible = false;

        //            //Lang.Save(AppModel.Instance.Lang);
        //            //lb_settings_sel_trans.Text = AppModel.Instance.Lang.text.Replace("(Standard)", "");

        //            await Task.Delay(1000);
        //            overlay.IsVisible = false;
        //            return;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        popupContainer_quest_changelang.IsVisible = false;
        //        AppModel.Logger.Error("Error: Sprache zu DE (btn_translateTapped) - " + ex.Message + "");
        //        await Task.Delay(1);
        //        overlay.IsVisible = false;
        //        ShowTranslate_fail();
        //        return;
        //    }

        //    //translate/Buildings(AppModel.Instance.AllBuildings, true);
        //}

        public async Task<bool> translateAfterSyncedBuildings(string lang, List<BuildingWSO> buildings, bool isChangeLang = false)
        {
            long allSigns = 0;
            try
            {
                if ((!isChangeLang && lang == "de") || !AppModel.Instance.AppControll.translation || buildings == null || buildings.Count == 0)
                {
                    return true;
                }
                overlay.IsVisible = true;
                await Task.Delay(1);
                int countFrom = 0;
                int countReady = 0;
                List<AuftragWSO> al = new List<AuftragWSO>();
                List<LeistungWSO> ll = new List<LeistungWSO>();
                List<KategorieWSO> kl = new List<KategorieWSO>();
                List<string> als = new List<string>();
                List<string> lls = new List<string>();
                List<string> kls = new List<string>();

                var req = new TransRequest { list = new List<TransListItem>(), to = AppModel.Instance.Lang.lang };
                buildings.ForEach(b =>
                {
                    var lb = BuildingWSO.LoadBuilding(AppModel.Instance, b.id);
                    //Vergleiche firsches Objekt vom Backend mit gespeichertem Objekt wenn vorhanden
                    if (b.del == 0)
                    {
                        if (b.ArrayOfAuftrag != null && b.ArrayOfAuftrag.Count > 0)
                        {
                            b.ArrayOfAuftrag.ForEach(a =>
                            {
                                AuftragWSO lba = null;
                                KategorieWSO lbk = null;
                                LeistungWSO lbl = null;
                                if (lb != null) { lba = lb.ArrayOfAuftrag.Find(f => f.id == a.id); }
                                if (a.del == 0)
                                {
                                    // übersetzen wenn Sprache sich ändert oder Objekt nicht existiert oder der Auftrag noch garnicht existiert
                                    if (isChangeLang || lb == null || lba == null)
                                    {
                                        countFrom++;
                                        als.Add(a.bezeichnung);
                                    }
                                    // übersetzen wenn Auf.Bez. noch gar nicht übersetzt
                                    else if (!isChangeLang && String.IsNullOrWhiteSpace(lba.bezeichnungLang))
                                    {
                                        countFrom++;
                                        als.Add(a.bezeichnung);
                                    }
                                    // übersetzen wenn AufBez sich geändert hat
                                    else if (!isChangeLang && !String.IsNullOrWhiteSpace(lba.bezeichnungLang) && a.bezeichnung != lba.bezeichnung)
                                    {
                                        countFrom++;
                                        als.Add(a.bezeichnung);
                                    }
                                    // Einfach übernehmen aus voeherigen Datensatz , so das die vorherige Übersetzung übernommen wird
                                    else if (!isChangeLang && !String.IsNullOrWhiteSpace(lba.bezeichnungLang) && a.bezeichnung == lba.bezeichnung)
                                    {
                                        a.bezeichnung = lba.bezeichnung;
                                    }
                                    if (a.kategorien != null && a.kategorien.Count > 0)
                                    {
                                        a.kategorien.ForEach(k =>
                                        {
                                            if (lb != null && lba != null) { lbk = lba.kategorien.Find(f => f.id == k.id); }
                                            if (k.del == 0 && (k.art == "Leistung" || k.art == "Produkt"))
                                            {
                                                // übersetzen wenn Sprache sich ändert oder Objekt nicht existiert oder der Auftrag noch garnicht existiert
                                                // oder die Kategorie nicht existiert
                                                if (isChangeLang || lb == null || lba == null || lbk == null)
                                                {
                                                    countFrom++;
                                                    kls.Add(k.titel);
                                                }
                                                // übersetzen wenn Auf.Bez. noch gar nicht übersetzt
                                                else if (!isChangeLang && String.IsNullOrWhiteSpace(lbk.titelLang))
                                                {
                                                    countFrom++;
                                                    kls.Add(k.titel);
                                                }
                                                // übersetzen wenn KatTit sich geändert hat
                                                else if (!isChangeLang && !String.IsNullOrWhiteSpace(lbk.titelLang) && k.titel != lbk.titel)
                                                {
                                                    countFrom++;
                                                    kls.Add(k.titel);
                                                }
                                                // Einfach übernehmen aus voeherigen Datensatz , so das die vorherige Übersetzung übernommen wird
                                                else if (!isChangeLang && !String.IsNullOrWhiteSpace(lbk.titelLang) && k.titel == lbk.titel)
                                                {
                                                    k.titelLang = lbk.titelLang;
                                                }
                                                if (k.leistungen != null && k.leistungen.Count > 0)
                                                {
                                                    k.leistungen.ForEach(l =>
                                                    {
                                                        if (lb != null && lba != null && lbk != null) { lbl = lbk.leistungen.Find(f => f.id == l.id); }
                                                        if (l.del == 0 && (l.art == "Leistung" || l.art == "Produkt"))
                                                        {
                                                            if (l.ext == null) { l.ext = new LeistungExtWSO(); }
                                                            // übersetzen wenn Sprache sich ändert oder Objekt nicht existiert oder der Auftrag noch garnicht existiert
                                                            // oder die Kategorie nicht existiert oder die Leistung noch nicht existiert
                                                            if (isChangeLang || lb == null || lba == null || lbk == null || lbl == null)
                                                            {
                                                                countFrom++;
                                                                lls.Add(l.GetMobileOriginalText());
                                                            }
                                                            // übersetzen wenn Leistungstext noch gar nicht übersetzt
                                                            else if (!isChangeLang && String.IsNullOrWhiteSpace(lbl.GetMobileLangText()))
                                                            {
                                                                countFrom++;
                                                                lls.Add(l.GetMobileOriginalText());
                                                            }
                                                            //AnweisungsText ist Leer
                                                            else if (!isChangeLang && !String.IsNullOrWhiteSpace(lbl.GetMobileLangText()) && String.IsNullOrWhiteSpace(l.ext.anweisung))
                                                            {
                                                                // BESCHREIBUNG verwenden -  übersetzen wenn Leistungstext sich geändert hat
                                                                if (l.beschreibung != lbl.beschreibung)
                                                                {
                                                                    countFrom++;
                                                                    lls.Add(l.beschreibung);
                                                                }
                                                                // BESCHREIBUNG verwenden - Einfach übernehmen aus voeherigen Datensatz , so das die vorherige Übersetzung übernommen wird.
                                                                else
                                                                {
                                                                    l.ext.anweisungLang = lbl.ext.anweisungLang;
                                                                }
                                                            }
                                                            //AnweisungsText ist gefüllt
                                                            else if (!isChangeLang && !String.IsNullOrWhiteSpace(lbl.GetMobileLangText()) && !String.IsNullOrWhiteSpace(l.ext.anweisung))
                                                            {
                                                                // BESCHREIBUNG verwenden -  übersetzen wenn Leistungstext sich geändert hat
                                                                if (l.ext.anweisung != lbl.ext.anweisung)
                                                                {
                                                                    countFrom++;
                                                                    lls.Add(l.ext.anweisung);
                                                                }
                                                                // BESCHREIBUNG verwenden - Einfach übernehmen aus voeherigen Datensatz , so das die vorherige Übersetzung übernommen wird.
                                                                else
                                                                {
                                                                    l.ext.anweisungLang = lbl.ext.anweisungLang;
                                                                }
                                                            }
                                                        }
                                                    });
                                                }
                                            }
                                        });
                                    }
                                }
                            });
                        }
                    }
                });

                var alist = ListExtensions.ChunkBy(als.Distinct().ToList(), 100);
                var klist = ListExtensions.ChunkBy(kls.Distinct().ToList(), 100);
                var llist = ListExtensions.ChunkBy(lls.Distinct().ToList(), 100);
                ////countFrom = klist.Count + llist.Count;
                //popupContainer_container_changelang_status.Text = "" + countReady + " von " + countFrom;
                await Task.Delay(1);
                var atr = new List<TranslationResult>();
                var ktr = new List<TranslationResult>();
                var ltr = new List<TranslationResult>();
                var service = new TranslateService(new BaseClientService.Initializer { ApiKey = AppModel.Google_Translation_ApiKey });
                var client = new TranslationClientImpl(service, TranslationModel.ServiceDefault);
                //var la = JsonConvert.SerializeObject(client.ListLanguages("de"));
                alist.ForEach(sa =>
                {
                    if (!AppModel.Instance.IsTest)
                    {
                        atr.AddRange(client.TranslateText(sa, lang, "de"));
                    }
                });
                atr.ForEach(tr => { allSigns = allSigns + long.Parse("" + tr.OriginalText.Length); });
                klist.ForEach(sk =>
                {
                    ktr.AddRange(client.TranslateText(sk, lang, "de"));
                });
                ktr.ForEach(tr => { allSigns = allSigns + long.Parse("" + tr.OriginalText.Length); });
                llist.ForEach(sl =>
                {
                    if (!AppModel.Instance.IsTest)
                    {
                        ltr.AddRange(client.TranslateText(sl, lang, "de"));
                    }
                });
                ltr.ForEach(tr => { allSigns = allSigns + long.Parse("" + tr.OriginalText.Length); });

                buildings.ForEach(b =>
                {
                    if (b.del == 0)
                    {
                        if (b.ArrayOfAuftrag != null && b.ArrayOfAuftrag.Count > 0)
                        {
                            b.ArrayOfAuftrag.ForEach(a =>
                            {
                                if (a.del == 0)
                                {
                                    var fa = atr.Find(f => f.OriginalText == a.bezeichnung);
                                    if (fa != null)
                                    {
                                        countReady++;
                                        a.bezeichnungLang = fa.TranslatedText;
                                    }
                                    if (a.kategorien != null && a.kategorien.Count > 0)
                                    {
                                        a.kategorien.ForEach(k =>
                                        {
                                            if (k.del == 0 && (k.art == "Leistung" || k.art == "Produkt"))
                                            {
                                                var fk = ktr.Find(f => f.OriginalText == k.titel);
                                                if (fk != null)
                                                {
                                                    countReady++;
                                                    k.titelLang = fk.TranslatedText;
                                                    AppModel.Instance.AddKategorieNames(k);
                                                }
                                                if (k.leistungen != null && k.leistungen.Count > 0)
                                                {
                                                    k.leistungen.ForEach(l =>
                                                    {
                                                        if (l.del == 0 && (l.art == "Leistung" || l.art == "Produkt") && l.ext != null)
                                                        {
                                                            var fl = ltr.Find(f => f.OriginalText == l.GetMobileOriginalText());
                                                            if (fl != null)
                                                            {
                                                                countReady++;
                                                                l.ext.anweisungLang = fl.TranslatedText;
                                                            }
                                                        }
                                                    });
                                                }
                                            }
                                        });
                                    }
                                }
                            });
                            BuildingWSO.Save(AppModel.Instance, b);
                            // popupContainer_container_changelang_status.Text = "" + countReady + " von " + countFrom;
                        }
                    }
                });

                if (allSigns > 0)
                {
                    // fertig
                    var allTransSignsItem = new AllTransSignRequest
                    {
                        allTransSign = allSigns,
                        ticks = DateTime.Now.Ticks,
                        personid = AppModel.Instance.Person.id,
                        guid = Guid.NewGuid().ToString(),
                        token = AppModel.Instance.SettingModel.SettingDTO.LoginToken
                    };
                    AllTransSign.ToUploadStack(allTransSignsItem);
                    CheckAllSyncFromUpload(); //SyncTransSigns();
                }

                //popupContainer_container_changelang_status.Text = "" + countReady + " von " + countFrom;
                //popupContainer_quest_changelang.IsVisible = false;

                //Lang.Save(AppModel.Instance.Lang);
                //lb_settings_sel_trans.Text = lang; // AppModel.Instance.Lang.text.Replace("(Standard)", "");

                await Task.Delay(1000);
                overlay.IsVisible = false;
                return true;
            }
            catch (Exception ex)
            {
                //popupContainer_quest_changelang.IsVisible = false;
                AppModel.Logger.Error("Error: (translateBuildings) - " + ex.Message + "");
                await Task.Delay(1);
                overlay.IsVisible = false;
                await DisplayAlertAsync("Fehler", "Fehler beim Übersetzen: " + ex.Message, "OK");
                //ShowTranslate_fail();
                return false;
            }
        }





        public void btn_DayOverBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }

        public void SetDayOverLastDate(string s)
        {
            dayOverLastDate.Text = s;
        }


        public void btn_DayOverNoTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }

        // BTN Feierabend
        public async void DayOverTapped(object sender, EventArgs e)
        {
            ShowDayOverPage();
        }

        // BTN Handwerker
        public async void btn_WorkerListTapped(object sender, EventArgs e)
        {
            // Handwerker Liste
            MainMenuTapped_Done(false);
            await Task.Delay(210);
            ShowWorkerPage();
        }
        // BTN DSGVO
        public async void btn_DSGVOTapped(object sender, EventArgs e)
        {
            MainMenuTapped_Done(false);
            await Task.Delay(210);
            ShowDSGVOPage();
        }
        // BTN Allgemein
        public async void btn_SyncTapped(object sender, EventArgs e)
        {
            popupContainerSyncFaild.IsVisible = false;
            await Task.Delay(1);
            MainMenuTapped_Done(false);
            await Task.Delay(210);
            SyncBuilding(true);
        }
        // BTN Registrierung
        public async void btn_RegistTapped(object sender, EventArgs e)
        {
            // Handwerker Liste
            MainMenuTapped_Done(false);
            await Task.Delay(210);
            BackToLoginPage();
            // ShowWorkerPage();
        }

        /*********************/
        /* WORKERS METHODS   */
        /*********************/
        private int workerSelectedViewIndex = 0;

        Dictionary<string, List<PersonWSO>> workerCategories = new Dictionary<string, List<PersonWSO>>();
        Dictionary<string, Object> workerCategoriesElements = new Dictionary<string, Object>();
        public async void btn_WorkerCategorySearchTapped(object sender, EventArgs e)
        {
            if (workerSelectedViewIndex == 1) { return; }
            overlay.IsVisible = true;
            list_worker.IsVisible = false;
            WorkerPageContainerView.EntryWorkersearchContainer.IsVisible = false;
            workerSelectedViewIndex = 1;
            btn_workercategorysearch.BackgroundColor = Color.FromArgb("#999999");
            btn_workernamesearch.BackgroundColor = Color.FromArgb("#042d53");
            btn_workerbuildingsearch.BackgroundColor = Color.FromArgb("#042d53");
            await list_worker_scroll.ScrollToAsync(0, 0, false);
            await Task.Delay(1);
            list_worker.Children.Clear();
            BuildWorkerCategoryList();
        }
        private async void BuildWorkerCategoryList()
        {
            if (list_worker.Children != null && list_worker.Children.Count > 0) { return; }
            workerCategories = new Dictionary<string, List<PersonWSO>>();
            workerCategoriesElements = new Dictionary<string, Object>();
            AppModel.Instance.AllWorkers.ForEach(ha =>
            {
                if (workerCategories.ContainsKey(ha.kategorie))
                {
                    workerCategories[ha.kategorie].Add(ha);
                    workerCategories[ha.kategorie] = workerCategories[ha.kategorie].OrderBy(o => o.firma).ToList();
                }
                else
                {
                    workerCategories.Add(ha.kategorie, new List<PersonWSO> { ha });
                }
            });
            workerCategories.ToList().ForEach(item =>
            {

                var mainVertStack = new StackLayout()
                {
                    Padding = new Thickness(6, 0, 0, 0),
                    Margin = new Thickness(0, 5, 0, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.Fill,
                    ClassId = ("##" + item.Key).ToLower()
                };
                var mainSubStack = new StackLayout()
                {
                    Padding = new Thickness(6, 0, 0, 0),
                    Margin = new Thickness(0, -5, 10, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.Fill,
                    IsVisible = false,
                    ClassId = "" + item.Key,
                };


                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => { _CategoryCommand(s, e); };
                Border sfb = Elements.GetWorkerCategoryTreeItem(item.Key, "" + item.Value.Count, null);
                sfb.GestureRecognizers.Clear();
                sfb.GestureRecognizers.Add(tapGestureRecognizer);
                sfb.ClassId = ("##" + item.Key).ToLower();
                workerCategoriesElements.Add(item.Key, sfb);
                //list_worker.Children.Add(sfb);
                mainVertStack.Children.Add(sfb);
                mainVertStack.Children.Add(mainSubStack);
                list_worker.Children.Add(mainVertStack);
            });
            await Task.Delay(1);
            list_worker.IsVisible = true;
            overlay.IsVisible = false;
        }
        private void _CategoryCommand(object s, EventArgs e)
        {
            var childs = ((VerticalStackLayout)((Border)s).Content).Children;
            var category = ((Label)((VerticalStackLayout)childs[0]).Children[1]).Text;
            //var container = (StackLayout)childs[1];
            var parentChilds = ((StackLayout)((Border)s).Parent).Children;
            var container = (StackLayout)parentChilds[1];
            if (container.IsVisible)
            {
                container.IsVisible = false;
            }
            else
            {
                CloseAllWorkerCategories();
                if (container.Children.Count == 0)
                {
                    AppModel.Instance.AllWorkers.ForEach(ha =>
                    {
                        if (category == ha.kategorie)
                        {
                            var sfbgf = Elements.GetWorkerTreeItem(ha, "worker.png", null, _navigationCommand);
                            //sfbgf.ClassId = ("bu_" + ha.firma + "," + ha.name + "," + ha.vorname + "," + ha.strasse + "," + ha.plz + "," + ha.ort + "," + ha.kategorie).ToLower();
                            sfbgf.IsVisible = true;
                            sfbgf.HorizontalOptions = LayoutOptions.Fill;
                            container.Children.Add(sfbgf);
                        }
                    });
                }
                container.IsVisible = true;
            }
        }
        private void CloseAllWorkerCategories()
        {
            workerCategoriesElements.ToList().ForEach(item =>
            {
                var el = ((StackLayout)((Border)item.Value).Parent).Children[1];
                if (el is VisualElement element) element.IsVisible = false;
            });
        }
        private ICommand _navigationCommand = new Command<string>((url) =>
        {
            AppModel.Instance.UseExternHardware = true;
            Launcher.OpenAsync(new Uri(url));
        });


        Dictionary<string, PersonWSO> workerNames = new Dictionary<string, PersonWSO>();
        Dictionary<string, Object> workerNamesElements = new Dictionary<string, Object>();
        public async void btn_WorkerNameSearchTapped(object sender, EventArgs e)
        {
            if (workerSelectedViewIndex == 2) { return; }
            overlay.IsVisible = true;
            list_worker.IsVisible = false;
            workerSelectedViewIndex = 2;
            btn_workercategorysearch.BackgroundColor = Color.FromArgb("#042d53");
            btn_workernamesearch.BackgroundColor = Color.FromArgb("#999999");
            btn_workerbuildingsearch.BackgroundColor = Color.FromArgb("#042d53");
            lb_workerbuildingsearche.Text = "Handwerker suchen:";
            entry_workersearch.Text = "";
            await Task.Delay(1);
            list_worker.Children.Clear();
            await list_worker_scroll.ScrollToAsync(0, 0, false);
            BuildWorkerNamesList();
        }
        private async void BuildWorkerNamesList()
        {
            if (list_worker.Children != null && workerNames.Count > 0 && list_worker.Children.Count == workerNames.ToList().Count) { return; }

            workerNames = new Dictionary<string, PersonWSO>();
            workerNamesElements = new Dictionary<string, Object>();

            var workers = AppModel.Instance.AllWorkers.OrderBy(o => (String.IsNullOrEmpty(o.firma) ? o.name : o.firma)).ToList();
            workers.ForEach(ha => { workerNames["" + ha.id] = ha; });
            //var i = 0;
            workerNames.ToList().ForEach(item =>
            {

                var mainVertStack = new StackLayout()
                {
                    Padding = new Thickness(6, 0, 0, 0),
                    Margin = new Thickness(0, 5, 0, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.Fill,
                    ClassId = ("##" + (String.IsNullOrEmpty(item.Value.firma) ? item.Value.name : item.Value.firma) + ";" + item.Value.strasse + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.kategorie).ToLower()
                };
                var mainSubStack = new StackLayout()
                {
                    Padding = new Thickness(6, 0, 0, 0),
                    Margin = new Thickness(6, -5, 10, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.Fill,
                    IsVisible = false,
                    ClassId = "" + item.Value.id,
                    BackgroundColor = Color.FromArgb("#144d73"),
                };

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => { _NamesCommand(s, e); };
                Border sfb = Elements.GetWorkerNamesTreeItem(item.Value, "worker.png", null);
                sfb.GestureRecognizers.Clear();
                sfb.GestureRecognizers.Add(tapGestureRecognizer);
                sfb.ClassId = ("##" + (String.IsNullOrEmpty(item.Value.firma) ? item.Value.name : item.Value.firma) + ";" + item.Value.strasse + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.kategorie).ToLower();
                workerNamesElements.Add(item.Key, sfb);
                mainVertStack.Children.Add(sfb);
                mainVertStack.Children.Add(mainSubStack);
                list_worker.Children.Add(mainVertStack);
            });
            WorkerPageContainerView.EntryWorkersearchContainer.IsVisible = true;
            await Task.Delay(1);
            list_worker.IsVisible = true;
            overlay.IsVisible = false;
        }
        private void _NamesCommand(object s, EventArgs e)
        {
            var parentChilds = ((StackLayout)((Border)s).Parent).Children;
            var container = (StackLayout)parentChilds[1];
            var workerid = container.ClassId;
            if (container.IsVisible)
            {
                container.IsVisible = false;
            }
            else
            {
                CloseAllWorkerNames();
                if (container.Children.Count == 0)
                {
                    AppModel.Instance.AllWorkers.ForEach(ha =>
                    {
                        if (workerid == ("" + ha.id))
                        {
                            var sfbgf = Elements.GetWorkerDetailsTreeItem(ha, "worker.png", null, _navigationCommand);
                            sfbgf.IsVisible = true;
                            sfbgf.HorizontalOptions = LayoutOptions.Fill;
                            container.Children.Add(sfbgf);
                        }
                    });
                }
                container.IsVisible = true;
            }
        }
        private void CloseAllWorkerNames()
        {
            workerNamesElements.ToList().ForEach(item =>
            {
                var el = ((StackLayout)((Border)item.Value).Parent).Children[1];
                if (el is VisualElement element) element.IsVisible = false;
            });
        }


        Dictionary<string, BuildingWSO> workerBuildings = new Dictionary<string, BuildingWSO>();
        Dictionary<string, Object> workerBuildingsElements = new Dictionary<string, Object>();
        public async void btn_WorkerBuildingSearchTapped(object sender, EventArgs e)
        {
            if (workerSelectedViewIndex == 3) { return; }
            overlay.IsVisible = true;
            list_worker.IsVisible = false;
            workerSelectedViewIndex = 3;
            lb_workerbuildingsearche.Text = "Objekt suchen:";
            btn_workercategorysearch.BackgroundColor = Color.FromArgb("#042d53");
            btn_workernamesearch.BackgroundColor = Color.FromArgb("#042d53");
            btn_workerbuildingsearch.BackgroundColor = Color.FromArgb("#999999");
            entry_workersearch.Text = "";
            await Task.Delay(1);
            list_worker.Children.Clear();
            await list_worker_scroll.ScrollToAsync(0, 0, false);
            BuildWorkerBuildingList();
        }
        private async void BuildWorkerBuildingList()
        {
            if (list_worker.Children != null && workerBuildings.Count > 0 && list_worker.Children.Count == workerBuildings.ToList().Count) { return; }

            workerBuildings = new Dictionary<string, BuildingWSO>();
            workerBuildingsElements = new Dictionary<string, Object>();

            var buildings = AppModel.Instance.AllBuildings.OrderBy(o => o.plz + o.ort + o.strasse + o.hsnr).ToList();
            buildings.ForEach(bu => { workerBuildings["" + bu.id] = bu; });
            workerBuildings.ToList().ForEach(item =>
            {
                if (item.Value.ArrayOfHandwerker.Count > 0)
                {

                    var mainVertStack = new StackLayout()
                    {
                        Padding = new Thickness(6, 0, 0, 0),
                        Margin = new Thickness(0, 5, 0, 0),
                        Spacing = 0,
                        Orientation = StackOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.Fill,
                        ClassId = ("bu_" + item.Value.strasse + ";" + item.Value.hsnr + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.objektname + ";" + item.Value.objektnr).ToLower()
                    };
                    var mainSubStack = new StackLayout()
                    {
                        Padding = new Thickness(6, 0, 0, 0),
                        Margin = new Thickness(0, -5, 10, 0),
                        Spacing = 0,
                        Orientation = StackOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.Fill,
                        IsVisible = false,
                        ClassId = "" + item.Value.id,
                    };

                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) => { WorkerBuildingCommand(s, e); };
                    Border sfb = Elements.GetWorkerBuildingTreeItem(item.Value, "building.png", null);
                    sfb.GestureRecognizers.Clear();
                    sfb.GestureRecognizers.Add(tapGestureRecognizer);
                    sfb.ClassId = ("bu_" + item.Value.strasse + ";" + item.Value.hsnr + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.objektname + ";" + item.Value.objektnr).ToLower();
                    var tapGestureRecognizerInfo = new TapGestureRecognizer();
                    tapGestureRecognizerInfo.Tapped += (s, e) => { AppModel.Instance.MainPage.OpenBuildingInfoDialog(item.Value); };
                    Border sfbb = Elements.GetWorkerBuildingTreeInfoItem(item.Value, sfb, tapGestureRecognizerInfo);
                    sfbb.ClassId = ("bu_" + item.Value.strasse + ";" + item.Value.hsnr + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.objektname + ";" + item.Value.objektnr).ToLower();
                    workerBuildingsElements.Add(item.Key, sfb);
                    mainVertStack.Children.Add(sfbb);
                    mainVertStack.Children.Add(mainSubStack);
                    list_worker.Children.Add(mainVertStack);
                }
            });
            await Task.Delay(1);
            WorkerPageContainerView.EntryWorkersearchContainer.IsVisible = true;
            list_worker.IsVisible = true;
            overlay.IsVisible = false;
        }
        private void WorkerBuildingCommand(object s, EventArgs e)
        {
            //var parentChilds = ((StackLayout)((Frame)s).Parent).Children;
            var parentChilds1 = ((Border)s).Parent;
            var parentChilds2 = ((StackLayout)parentChilds1).Parent;
            var parentChilds3 = ((Border)parentChilds2).Parent;
            var parentChilds = ((StackLayout)parentChilds3).Children;
            var container = (StackLayout)parentChilds[1];
            var buildingid = container.ClassId;
            if (container.IsVisible)
            {
                container.IsVisible = false;
            }
            else
            {
                CloseAllWorkerBuildings();
                if (container.Children.Count == 0)
                {
                    AppModel.Instance.AllBuildings.Find(b => ("" + b.id) == buildingid).ArrayOfHandwerker.ForEach(ha =>
                    {
                        var sfbgf = Elements.GetWorkerTreeItem(ha, "worker.png", null, _navigationCommand);
                        sfbgf.IsVisible = true;
                        sfbgf.HorizontalOptions = LayoutOptions.Fill;
                        container.Children.Add(sfbgf);
                    });
                }
                container.IsVisible = true;
            }
        }
        private void CloseAllWorkerBuildings()
        {
            workerBuildingsElements.ToList().ForEach(item =>
            {
                var parentChilds1 = ((Border)item.Value).Parent;
                var parentChilds2 = ((StackLayout)parentChilds1).Parent;
                var parentChilds3 = ((Border)parentChilds2).Parent;
                var parentChilds = ((StackLayout)parentChilds3).Children;
                var container = (StackLayout)parentChilds[1];
                container.IsVisible = false;
            });
        }

        public async void btn_WorkerBackTapped(object sender, EventArgs e)
        {
            WorkerPage_Container.IsVisible = false;
            if (Navigation.ModalStack.LastOrDefault() == WorkerPageContainerView)
            {
                await Navigation.PopModalAsync(animated: false);
            }
            this.Focus();
            ShowMainPage();
        }

        // PersonTimes
        public async void btn_PersonTimesTapped(object sender, EventArgs e)
        {
            // Handwerker Liste
            MainMenuTapped_Done(false);
            await Task.Delay(210);
            ShowPersonTimesPage();
        }

        private async void ShowPersonTimesPage()
        {
            if (_isOpeningPersonTimesModal) { return; }
            _isOpeningPersonTimesModal = true;
            isInitialize = true;
            overlay.IsVisible = true;
            try
            {
                await Task.Delay(1);

                if (!Navigation.ModalStack.Contains(PersonTimesPageView))
                {
                    await Navigation.PushModalAsync(PersonTimesPageView, animated: false);
                }
                await Task.Delay(1);
                // await list_persontimes_scroll.ScrollToAsync(0, 0, false); // moved into PersonTimesPageView
                await PersonTimesPageView.ListPersontimesScroll.ScrollToAsync(0, 0, false);

                // pick_persontimes_year.Items.Clear(); // moved into PersonTimesPageView
                // pick_persontimes_year.Items.Add(DateTime.Now.ToString("yyyy")); // moved into PersonTimesPageView
                // pick_persontimes_year.Items.Add(DateTime.Now.AddYears(-1).ToString("yyyy")); // moved into PersonTimesPageView
                // pick_persontimes_year.Items.Add(DateTime.Now.AddYears(-2).ToString("yyyy")); // moved into PersonTimesPageView
                // pick_persontimes_year.SelectedItem = DateTime.Now.ToString("yyyy"); // moved into PersonTimesPageView
                // pick_persontimes_month.SelectedItem = DateTime.Now.ToString("MMMM"); // moved into PersonTimesPageView
                PersonTimesPageView.PickPersontimesYear.Items.Clear();
                PersonTimesPageView.PickPersontimesYear.Items.Add(DateTime.Now.ToString("yyyy"));
                PersonTimesPageView.PickPersontimesYear.Items.Add(DateTime.Now.AddYears(-1).ToString("yyyy"));
                PersonTimesPageView.PickPersontimesYear.Items.Add(DateTime.Now.AddYears(-2).ToString("yyyy"));
                PersonTimesPageView.PickPersontimesYear.SelectedItem = DateTime.Now.ToString("yyyy");
                PersonTimesPageView.PickPersontimesMonth.SelectedItem = DateTime.Now.ToString("MMMM");

                // PersonTimesPage_Container.IsVisible = true; // moved into PersonTimesPageView
                PersonTimesPageView.SetVisible(true);

                await Task.Delay(1);
                overlay.IsVisible = false;
                isInitialize = false;
            }
            finally
            {
                _isOpeningPersonTimesModal = false;
            }
        }
        private async void pick_persontimes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitialize) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);
            // list_persontimes.Children.Clear(); // moved into PersonTimesPageView
            // stack_persontimes_top.Children.Clear(); // moved into PersonTimesPageView
            // stack_persontimes_top.Children.Add(PersonWSO.GetPersonTimesViewHeaderItem()); // moved into PersonTimesPageView
            // stack_persontimes_bottom.Children.Clear(); // moved into PersonTimesPageView
            PersonTimesPageView.ListPersontimes.Children.Clear();
            PersonTimesPageView.StackPersontimesTop.Children.Clear();
            PersonTimesPageView.StackPersontimesTop.Children.Add(PersonWSO.GetPersonTimesViewHeaderItem());
            PersonTimesPageView.StackPersontimesBottom.Children.Clear();

            PersonTimeResponse pts = await Task.Run(() =>
            {
                // return AppModel.Instance.Connections.GetPersonTimes(int.Parse(pick_persontimes_year.SelectedItem.ToString()), pick_persontimes_month.SelectedIndex + 1); // moved into PersonTimesPageView
                return AppModel.Instance.Connections.GetPersonTimes(int.Parse(PersonTimesPageView.PickPersontimesYear.SelectedItem.ToString()), PersonTimesPageView.PickPersontimesMonth.SelectedIndex + 1);
            });

            if (!pts.success)
            {
                await DisplayAlertAsync("Abruf nicht möglich!", pts.message, "OK");
            }
            else
            {
                int allMin = 0;
                int allHours = 0;
                int allMins = 0;
                if (pts.times != null && pts.times.Count > 0)
                {
                    pts.times.ForEach(pt =>
                    {
                        try
                        {
                            bool isMinus = false;
                            if (pt.dauer.Contains("--:--")) { pt.dauer = "00:00"; }
                            if (pt.dauer.Contains("-")) { isMinus = true; }

                            var da = pt.dauer.Split(':');

                            // Validierung: Prüfen ob die gesplitteten Werte gültig sind
                            if (da.Length == 2 &&
                                !string.IsNullOrWhiteSpace(da[0]) &&
                                !string.IsNullOrWhiteSpace(da[1]))
                            {
                                var hoursStr = da[0].Replace("-", "").Trim();
                                var minsStr = da[1].Trim();

                                // Prüfen ob nur Zahlen enthalten sind
                                if (int.TryParse(hoursStr, out int hours) &&
                                    int.TryParse(minsStr, out int mins))
                                {
                                    var damin = (hours * 60) + mins;
                                    if (isMinus) { damin = damin * -1; }
                                    allMin += damin;
                                }
                                else
                                {
                                    AppModel.Logger.Warn($"WARN: Ungültiges Zeitformat in pt.dauer: '{pt.dauer}' - Stunden: '{hoursStr}', Minuten: '{minsStr}'");
                                }
                            }
                            else
                            {
                                AppModel.Logger.Warn($"WARN: Ungültiges Zeitformat in pt.dauer: '{pt.dauer}' - Format entspricht nicht HH:MM");
                            }
                        }
                        catch (Exception ex)
                        {
                            AppModel.Logger.Error($"ERROR: Fehler beim Parsen von pt.dauer: '{pt.dauer}' - {ex.Message}");
                        }
                    });
                }
                // list_persontimes.Children.Add(PersonWSO.GetPersonTimesView(pts.times)); // moved into PersonTimesPageView
                PersonTimesPageView.ListPersontimes.Children.Add(PersonWSO.GetPersonTimesView(pts.times));
                if (pts.times != null && pts.times.Count > 0)
                {
                    allHours = int.Parse("" + (allMin / 60));
                    allMins = allMin - (allHours * 60);
                    pts.times[0].all = (allHours > 9 ? ("" + allHours) : ("0" + allHours)) + ":" + (allMins > 9 ? ("" + allMins) : ("0" + allMins));
                    // stack_persontimes_bottom.Children.Add(PersonWSO.GetPersonTimesViewAllItem(pts.times[0])); // moved into PersonTimesPageView
                    PersonTimesPageView.StackPersontimesBottom.Children.Add(PersonWSO.GetPersonTimesViewAllItem(pts.times[0]));
                }
            }
            await Task.Delay(1);
            overlay.IsVisible = false;
            this.Focus();
        }
        private int GetTimeSubPause(int m, string p)
        {
            var ps = p.Split(';');
            if (m < 360)
            {
                return int.Parse(ps[0]);
            }
            else if (m >= 360 && m < 540)
            {
                return int.Parse(ps[1]);
            }
            else if (m >= 540)
            {
                return int.Parse(ps[2]);
            }
            return 0;
        }
        public async void btn_PersontimesBackTapped(object sender, EventArgs e)
        {
            // list_persontimes.Children.Clear(); // moved into PersonTimesPageView
            // list_persontimes_scroll.ScrollToAsync(0, 0, false); // moved into PersonTimesPageView
            PersonTimesPageView.ListPersontimes.Children.Clear();
            await PersonTimesPageView.ListPersontimesScroll.ScrollToAsync(0, 0, false);
            PersonTimesPageView.SetVisible(false);
            if (Navigation.ModalStack.LastOrDefault() == PersonTimesPageView)
            {
                await Navigation.PopModalAsync(animated: false);
            }
            this.Focus();
            ShowMainPage();
        }


        public async void btn_TodosTapped(object sender, EventArgs e)
        {
            // Handwerker Liste
            MainMenuTapped_Done(false);
            await Task.Delay(210);
            await ShowTodoPage();
        }
        private async Task ShowTodoPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            try
            {
                await TodoModalPage.ShowAsync(this);
            }
            finally
            {
                overlay.IsVisible = false;
                isInitialize = false;
            }
        }

        public void btn_NotScanBackTapped(object sender, EventArgs e)
        {
            list_notscan.Children.Clear();
            this.Focus();
            ShowMainPage();
        }



        public void btn_showall_again_OrderCategoryTapped(object sender, EventArgs e)
        {
            AppModel.Instance._showall_again_OrderCategory = !AppModel.Instance._showall_again_OrderCategory;
            btn_back_inBuildingOrder_category_showall_again_txt.Text = AppModel.Instance._showall_again_OrderCategory ? "Meine zeigen" : "Alle zeigen";

            buildingorderlist_category_container_Again.Children.Clear();
            buildingorderlist_category_container_Again.Children.Add(KategorieWSO.GetCategoryAgainListView(AppModel.Instance, new Command<KategorieWSO>(SelectCategoryAgain)));
            BuildingOrderPage_category_Container_Again.IsVisible = true;
        }

        public async void btn_nachbuchen_Tapped(int pos)
        {
            try
            {
                // WICHTIG: Prüfe ob ein Gebäude ausgewählt ist
                if (AppModel.Instance?.LastBuilding == null)
                {
                    AppModel.Logger?.Warn("btn_nachbuchen_Tapped: Kein Gebäude ausgewählt");
                    await DisplayAlertAsync("Hinweis", "Bitte wählen Sie zuerst ein Objekt aus, bevor Sie Positionen nachbuchen können.", "OK");
                    return;
                }

                if (AppModel.Instance.LastBuilding.ArrayOfAuftrag == null || AppModel.Instance.LastBuilding.ArrayOfAuftrag.Count == 0)
                {
                    AppModel.Logger?.Warn("btn_nachbuchen_Tapped: Keine Aufträge im ausgewählten Gebäude");
                    await DisplayAlertAsync("Hinweis", "Das ausgewählte Objekt hat keine Aufträge.", "OK");
                    return;
                }

                AppModel.Instance.posAgain = pos;
                overlay.IsVisible = true;
                AppModel.Instance.LastSelectedCategoryAgain = null;
                AppModel.Instance.LastSelectedPositionAgain = null;
                btn_nachbuchen_Pos.BackgroundColor = pos == 0 ? Color.FromArgb("#042d53") : Color.FromArgb("#999999");
                btn_nachbuchen_Produkte.BackgroundColor = pos == 0 ? Color.FromArgb("#999999") : Color.FromArgb("#042d53");
                //await Task.Delay(1);
                //await buildingorderlist_category_scroll_Again.ScrollToAsync(0, 0, false);
                BuildNachbuchenList();
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"btn_nachbuchen_Tapped: {ex.Message}");
                await DisplayAlertAsync("Fehler", $"Fehler beim Öffnen der Nachbuchung: {ex.Message}", "OK");
                overlay.IsVisible = false;
            }
        }

        public async void BuildNachbuchenList()
        {
            try
            {
                AppModel.Instance.LastSelectedOrderAgain = null;
                AppModel.Instance.LastSelectedCategoryAgain = null;
                AppModel.Instance.LastSelectedPositionAgain = null;

                // Prüfe ob LastBuilding nach Customer-Wechsel vorhanden ist
                if (AppModel.Instance?.LastBuilding == null)
                {
                    AppModel.Logger?.Error("BuildNachbuchenList: LastBuilding is null (möglicherweise nach Customer-Wechsel)");
                    await DisplayAlertAsync("Fehler", "Keine Objektdaten verfügbar. Bitte wählen Sie ein Objekt aus.", "OK");
                    overlay.IsVisible = false;
                    return;
                }

                if (AppModel.Instance.LastBuilding.ArrayOfAuftrag == null)
                {
                    AppModel.Logger?.Error("BuildNachbuchenList: ArrayOfAuftrag is null");
                    await DisplayAlertAsync("Fehler", "Keine Aufträge verfügbar.", "OK");
                    overlay.IsVisible = false;
                    return;
                }

                BuildingOrderPage_category_Container_Again.IsVisible = true;
                BuildingOrderPage_position_Container_Again.IsVisible = false;
                inBuildingOrder_position_stack_Again.IsVisible = false;

                LeistungWSO firstLeistungInWork = null;
                AppModel.Instance.IsOptionalPosAgain = false;
                var selOrderId = -1;
                if (AppModel.Instance.allPositionInWork != null && AppModel.Instance.allPositionInWork.leistungen != null && AppModel.Instance.allPositionInWork.leistungen.Count > 0)
                {
                    AppModel.Instance.allPositionInWork.leistungen.ForEach(liw =>
                    {
                        AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(a =>
                        {
                            a.kategorien?.ForEach(k =>
                            {
                                if (firstLeistungInWork == null)
                                {
                                    firstLeistungInWork = k.leistungen?.Find(l => l.art == "Leistung" && liw.id == l.id);
                                }
                            });
                        });
                    });
                    AppModel.Instance.IsOptionalPosAgain = firstLeistungInWork != null && firstLeistungInWork.nichtpauschal == 1;
                    var first = AppModel.Instance.allPositionInWork.leistungen.First();
                    if (first != null) { selOrderId = first.auftragid; } else { selOrderId = -1; }
                }
                AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    if (o.id == selOrderId || selOrderId < 0)
                    {
                        AppModel.Instance.LastSelectedOrderAgain = o;
                        lb_inBuildingOrder_categorypos_text_Again.Text = o.GetMobileText();// + " \nNr.: " + o.id + "  Typ: " + o.typ;
                        lb_inBuildingOrder_position_text_Again.Text = "";
                    }
                });

                ShowOrderCategoryAgainPage(AppModel.Instance.LastSelectedOrderAgain);

                await Task.Delay(1);
                list_nachbuchen.IsVisible = true;
                overlay.IsVisible = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"BuildNachbuchenList: {ex.Message}");
                await DisplayAlertAsync("Fehler", $"Fehler beim Laden der Nachbuchungsliste: {ex.Message}", "OK");
                overlay.IsVisible = false;
            }
        }

        private async void ShowOrderCategoryAgainPage(AuftragWSO order)
        {
            try
            {
                isInitialize = true;
                overlay.IsVisible = true;
                await Task.Delay(1);

                // Prüfe ob Order vorhanden ist (wichtig nach Customer-Wechsel)
                if (order == null)
                {
                    AppModel.Logger?.Error("ShowOrderCategoryAgainPage: order is null (möglicherweise nach Customer-Wechsel)");
                    await DisplayAlertAsync("Fehler", "Kein Auftrag ausgewählt. Bitte laden Sie die Daten neu.", "OK");
                    overlay.IsVisible = false;
                    isInitialize = false;
                    return;
                }

                // WICHTIG: Zuerst auf null setzen, BEVOR die UI erstellt wird
                AppModel.Instance.LastSelectedCategoryAgain = null;
                AppModel.Instance.LastSelectedPositionAgain = null;

                BuildingOrderPage_position_Container_Again.IsVisible = false;
                inBuildingOrder_position_stack_Again.IsVisible = false;

                buildingorderlist_category_container_Again.Children.Clear();
                buildingorderlist_category_container_Again.Children.Add(KategorieWSO.GetCategoryAgainListView(AppModel.Instance, new Command<KategorieWSO>(SelectCategoryAgain)));
                BuildingOrderPage_category_Container_Again.IsVisible = true;

                await Task.Delay(1);
                overlay.IsVisible = false;
                isInitialize = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"ShowOrderCategoryAgainPage: {ex.Message}");
                await DisplayAlertAsync("Fehler", $"Fehler beim Laden der Kategorien: {ex.Message}", "OK");
                overlay.IsVisible = false;
                isInitialize = false;
            }
        }
        public async void SelectCategoryAgain(KategorieWSO category)
        {
            try
            {
                // Null-Check für übergebene Kategorie
                if (category == null)
                {
                    AppModel.Logger?.Error("SelectCategoryAgain: category parameter is null");
                    await DisplayAlertAsync("Fehler", "Ungültige Kategorie ausgewählt.", "OK");
                    return;
                }

                //// Prüfe ob bereits eine Operation läuft
                //if (isInitialize)
                //{
                //    AppModel.Logger?.Warn("SelectCategoryAgain: Operation already in progress, ignoring click");
                //    return;
                //}

                AppModel.Instance.LastSelectedCategoryAgain = category;
                BuildingOrderPage_category_Container_Again.IsVisible = false;
                inBuildingOrder_position_stack_Again.IsVisible = true;
                lb_inBuildingOrder_categorypos_text_Again.Text = AppModel.Instance.LastSelectedOrderAgain.GetMobileText();// + " \nNr.: " + AppModel.Instance.LastSelectedOrderAgain.id + "  Typ: " + AppModel.Instance.LastSelectedOrderAgain.typ;
                lb_inBuildingOrder_position_text_Again.Text = category.GetMobileText();
                ShowOrderPositionPageAgain();
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"SelectCategoryAgain: {ex.Message}", ex);
                await DisplayAlertAsync("Fehler", $"Fehler beim Auswählen der Kategorie: {ex.Message}", "OK");
            }
        }
        private async void ShowOrderPositionPageAgain()
        {
            try
            {
                isInitialize = true;
                overlay.IsVisible = true;
                await Task.Delay(1);

                // Null-Check vor dem Zugriff
                if (AppModel.Instance?.LastSelectedCategoryAgain == null)
                {
                    AppModel.Logger?.Error("ShowOrderPositionPageAgain: LastSelectedCategoryAgain is null");
                    await DisplayAlertAsync("Fehler", "Keine Kategorie ausgewählt.", "OK");
                    overlay.IsVisible = false;
                    isInitialize = false;
                    return;
                }

                buildingorderlist_position_container_Again.Children.Clear();
                buildingorderlist_position_container_Again.Children.Add(LeistungWSO.GetPositionAgainListView(AppModel.Instance, new Command<LeistungWSO>(SelectPositionToWorkAgain)));
                BuildingOrderPage_position_Container_Again.IsVisible = true;

                AppModel.Instance.LastSelectedPositionAgain = null;

                await Task.Delay(1);
                overlay.IsVisible = false;
                isInitialize = false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"ShowOrderPositionPageAgain: {ex.Message}", ex);
                await DisplayAlertAsync("Fehler", $"Fehler beim Laden der Positionen: {ex.Message}", "OK");
                overlay.IsVisible = false;
                isInitialize = false;
            }   
        }
        public async void SelectPositionToWorkAgain(LeistungWSO position)
        {


            //if(AppModel.Instance.IsOptionalPosAgain == false) { }
            //bool inWork = false;
            //if (AppModel.Instance.allPositionInWork != null)
            //{
            //    var foundInWork = AppModel.Instance.allPositionInWork.leistungen.Find(l => l.id == position.id);
            //    inWork = foundInWork != null;
            //}
            //if (position.disabled || inWork) { return; }

            overlay.IsVisible = true;
            //await Task.Delay(1);

            AppModel.Instance.LastSelectedPositionAgain = position;
            Border framePos = null;
            var selPost = AppModel.Instance.allSelectedPositionAgainToWork.Find(p => p.id == position.id);
            if (selPost != null)
            {
                // entfernen da schon selectiert 
                AppModel.Instance.allSelectedPositionAgainToWork.Remove(position);
                if (AppModel.Instance.allPositionAgainInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = false;
                    framePos.Content = LeistungWSO.GetPositionCardView(position, AppModel.Instance, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            else
            {
                // hinzufügen
                AppModel.Instance.allSelectedPositionAgainToWork.Add(position);
                if (AppModel.Instance.allPositionAgainInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = true;
                    framePos.Content = LeistungWSO.GetSelectedPositionCardView(position, AppModel.Instance, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            btn_showselected_pos_container_Again.IsVisible = AppModel.Instance.allSelectedPositionAgainToWork.Count > 0;
            btn_showselected_pos_container_not_Again.IsVisible = !(AppModel.Instance.allSelectedPositionAgainToWork.Count > 0);
            btn_showselected_pos_container2.IsVisible = AppModel.Instance.allSelectedPositionAgainToWork.Count > 0;
            CheckForOptionalToWorkAgain();

            //await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void RemoveSelectPositionAgainFromToWork(LeistungWSO position)
        {
            overlay.IsVisible = true;
            //await Task.Delay(1);

            Border framePos;
            SwipeView swipePos;
            // entfernen da schon selectiert 
            AppModel.Instance.allSelectedPositionAgainToWork.Remove(position);
            position.selected = false;
            if (AppModel.Instance.allPositionAgainInShowingListView.TryGetValue(position.id, out framePos))
            {
                framePos.Content = LeistungWSO.GetPositionCardView(position, AppModel.Instance, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
            }
            if (AppModel.Instance.allPositionAgainInShowingSmallListView.TryGetValue(position.id, out swipePos))
            {
                swipePos.IsVisible = false;
            }

            btn_showselected_pos_container_Again.IsVisible = AppModel.Instance.allSelectedPositionAgainToWork.Count > 0;
            btn_showselected_pos_container_not_Again.IsVisible = !(AppModel.Instance.allSelectedPositionAgainToWork.Count > 0);
            btn_showselected_pos_container2.IsVisible = AppModel.Instance.allSelectedPositionAgainToWork.Count > 0;
            CheckForOptionalToWorkAgain();
            if (AppModel.Instance.allSelectedPositionAgainToWork.Count == 0)
            {
                await Task.Delay(100);
                AuswahlAnzeigenTapped_Done(false);
                //await Task.Delay(100);
                //if (BuildingOrderPage_order_Container.IsVisible)
                //{
                //    buildingorderlist_order_container.Children.Clear();
                //    buildingorderlist_order_container.Children.Add(AuftragWSO.GetOrderListView(AppModel.Instance, new Command<AuftragWSO>(SelectOrder)));
                //}
            }
            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void CheckForOptionalToWorkAgain()
        {
            if (AppModel.Instance.IsOptionalPosAgain)
            {
                lb_PosSelectionType_text_Again.Text = "Nur optionale Positionen und Produkte aktiv!";
            }
            else
            {
                lb_PosSelectionType_text_Again.Text = "Nur geplante Positionen und Produkte aktiv!";
            }
        }






        private void WorkerListLayoutRefresh()
        {
            if (list_worker.Children.Count > 0)
            {
                foreach (var stack in list_worker.Children)
                {
                    // Cast zu VisualElement (hat IsVisible und ClassId)
                    if (stack is VisualElement element)
                    {
                        element.IsVisible = true;
                    }
                }
            }
        }












        public void InitStartPageHandlers()
        {
            try
            {
                //btn_regScanWarn_img.Source = imagesBase.AlertMessage;

                frame_planConA_img_reloadx.Source = GetMuellInOutXImageName(AppModel.Instance.AppSetModel.ViewOnlyMuell);

                // LoginPerson and Version 
                lb_LoginUser.Text = AppModel.Instance.Person.anrede + " " + (String.IsNullOrWhiteSpace(AppModel.Instance.Person.vorname) ? "" : (AppModel.Instance.Person.vorname.Length > 0 ? AppModel.Instance.Person.vorname.Substring(0, 1) + ". " : "")) + AppModel.Instance.Person.name;
                lb_version.Text = "V" + AppModel.Instance.Version; //+ " (" + AppModel.Instance.Build + ")";
                if (AppModel.Instance.Companies.Count > -1)
                {
                    lb_LoginCustomer.IsVisible = true;
                    lb_LoginCustomer.Text = AppModel.Instance.SettingModel.SettingDTO.CustomerName.Length > 30 ? (AppModel.Instance.SettingModel.SettingDTO.CustomerName.Substring(0, 30) + "...") : AppModel.Instance.SettingModel.SettingDTO.CustomerName;
                }
                else
                {
                    lb_LoginCustomer.IsVisible = false;
                }

                frm_img_LoginUser.IsVisible = false;
                if (AppModel.Instance.Person.userIcon != null)
                {
                    if (AppModel.Instance.Person != null && AppModel.Instance.Person.userIcon != null && AppModel.Instance.Person.userIcon.Length > 0)
                    {
                        ImageSource userIconImageSource = ImageSource.FromStream(() => new MemoryStream(AppModel.Instance.Person.userIcon));
                        img_LoginUser.Source = userIconImageSource;
                        frm_img_LoginUser.IsVisible = true;
                    }
                }

                SetAppControll();

                // Jetzt beenden
                btn_endselectedwork.GestureRecognizers.Clear();
                var tgr_over = new TapGestureRecognizer();
                tgr_over.Tapped -= ScanRunningWorksOver;
                tgr_over.Tapped += ScanRunningWorksOver;
                btn_endselectedwork.GestureRecognizers.Add(tgr_over);
                btn_endselectedcancel.GestureRecognizers.Clear();
                var tgr_cancel = new TapGestureRecognizer();
                tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_endwork.IsVisible = false; };
                tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_endwork.IsVisible = false; };
                btn_endselectedcancel.GestureRecognizers.Add(tgr_cancel);


                //****************************************
                // Checks Bemerkung
                btn_notice_save_check_ready.GestureRecognizers.Clear();
                var tgr_btn_notice_save_check_ready = new TapGestureRecognizer();
                tgr_btn_notice_save_check_ready.Tapped += btn_ReadyCheckAToUploadTapped_check_bem;
                btn_notice_save_check_ready.GestureRecognizers.Add(tgr_btn_notice_save_check_ready);

                btn_back_notice_check_bem.GestureRecognizers.Clear();
                var tgr_back_notice_check_bem = new TapGestureRecognizer();
                tgr_back_notice_check_bem.Tapped += btn_NoticeBackTapped_check_bem;
                btn_back_notice_check_bem.GestureRecognizers.Add(tgr_back_notice_check_bem);

                btn_notice_save_check_bem.GestureRecognizers.Clear();
                var tgr_back_notice_save_check_bem = new TapGestureRecognizer();
                tgr_back_notice_save_check_bem.Tapped += btn_NoticeSaveTapped_check_bem;
                btn_notice_save_check_bem.GestureRecognizers.Add(tgr_back_notice_save_check_bem);

                btn_takePhoto_frame_check_bem.GestureRecognizers.Clear();
                var tgr_btn_takePhoto_check_bem = new TapGestureRecognizer();
                tgr_btn_takePhoto_check_bem.Tapped += async (s, e) => await btn_takePhoto_check_bem(s, e);
                btn_takePhoto_frame_check_bem.GestureRecognizers.Add(tgr_btn_takePhoto_check_bem);
                btn_takePhotoAttachment_frame_check_bem.GestureRecognizers.Clear();
                var tgr_btn_takePhotoAttachment_check_bem = new TapGestureRecognizer();
                tgr_btn_takePhotoAttachment_check_bem.Tapped += async (s, e) => await btn_pickPhotos_check_bem(s, e);
                btn_takePhotoAttachment_frame_check_bem.GestureRecognizers.Add(tgr_btn_takePhotoAttachment_check_bem);

                btn_startcheckquest.GestureRecognizers.Clear();
                var tgr_btn_startcheckquest = new TapGestureRecognizer();
                tgr_btn_startcheckquest.Tapped += (object o, TappedEventArgs ev) => { StartOrOpenCheckA_next_start(); };
                btn_startcheckquest.GestureRecognizers.Add(tgr_btn_startcheckquest);

                btn_startcheckquestcancel.GestureRecognizers.Clear();
                var tgr_btn_startcheckquestcancel = new TapGestureRecognizer();
                tgr_btn_startcheckquestcancel.Tapped += (object o, TappedEventArgs ev) => { StartOrOpenCheckA_next_cancel(); };
                btn_startcheckquestcancel.GestureRecognizers.Add(tgr_btn_startcheckquestcancel);


                btn_check_del.GestureRecognizers.Clear();
                var tgr_btn_check_del = new TapGestureRecognizer();
                tgr_btn_check_del.Tapped += (object o, TappedEventArgs ev) => { OpenDelCheckA(); };
                btn_check_del.GestureRecognizers.Add(tgr_btn_check_del);
                btn_delcheckquest.GestureRecognizers.Clear();
                var tgr_btn_delcheckquest = new TapGestureRecognizer();
                tgr_btn_delcheckquest.Tapped += (object o, TappedEventArgs ev) => { DelCheckA_now(); };
                btn_delcheckquest.GestureRecognizers.Add(tgr_btn_delcheckquest);

                btn_delcheckquestcancel.GestureRecognizers.Clear();
                var tgr_btn_delcheckquestcancel = new TapGestureRecognizer();
                tgr_btn_delcheckquestcancel.Tapped += (object o, TappedEventArgs ev) => { DelCheckA_cancel(); };
                btn_delcheckquestcancel.GestureRecognizers.Add(tgr_btn_delcheckquestcancel);


                // Direktbuchen WINTER Dialog
                btn_quest_direktbuchenwinter_cancel.GestureRecognizers.Clear();
                var t_quest_direktbuchenwinter_cancel = new TapGestureRecognizer();
                t_quest_direktbuchenwinter_cancel.Tapped -= (object o, TappedEventArgs ev) => { CloseDirektbuchenWinterAusPlanliste(); };
                t_quest_direktbuchenwinter_cancel.Tapped += (object o, TappedEventArgs ev) => { CloseDirektbuchenWinterAusPlanliste(); };
                btn_quest_direktbuchenwinter_cancel.GestureRecognizers.Add(t_quest_direktbuchenwinter_cancel);
                btn_quest_direktbuchenwinter.GestureRecognizers.Clear();
                var t_quest_direktbuchenwinter = new TapGestureRecognizer();
                t_quest_direktbuchenwinter.Tapped -= (object o, TappedEventArgs ev) => { SaveDirektbuchenWinterAusPlanliste(); };
                t_quest_direktbuchenwinter.Tapped += (object o, TappedEventArgs ev) => { SaveDirektbuchenWinterAusPlanliste(); };
                btn_quest_direktbuchenwinter.GestureRecognizers.Add(t_quest_direktbuchenwinter);

                // Direktbuchen Dialog
                btn_quest_direktbuchen.GestureRecognizers.Clear();
                var t_quest_direktbuchen = new TapGestureRecognizer();
                t_quest_direktbuchen.Tapped -= (object o, TappedEventArgs ev) => { SaveDirektbuchenAusPlanliste(); };
                t_quest_direktbuchen.Tapped += (object o, TappedEventArgs ev) => { SaveDirektbuchenAusPlanliste(); };
                btn_quest_direktbuchen.GestureRecognizers.Add(t_quest_direktbuchen);
                btn_quest_direktbuchen_cancel.GestureRecognizers.Clear();
                var t_quest_direktbuchen_cancel = new TapGestureRecognizer();
                t_quest_direktbuchen_cancel.Tapped -= (object o, TappedEventArgs ev) => { CloseDirektbuchenAusPlanliste(); };
                t_quest_direktbuchen_cancel.Tapped += (object o, TappedEventArgs ev) => { CloseDirektbuchenAusPlanliste(); };
                btn_quest_direktbuchen_cancel.GestureRecognizers.Add(t_quest_direktbuchen_cancel);



                // StartPage
                //frame_plantabA.GestureRecognizers.Clear();
                //var t_frame_plantabA = new TapGestureRecognizer();
                //t_frame_plantabA.Tapped += btn_PlanTabATapped;
                //frame_plantabA.GestureRecognizers.Add(t_frame_plantabA);
                //frame_plantabB.GestureRecognizers.Clear();
                //var t_frame_plantabB = new TapGestureRecognizer();
                //t_frame_plantabB.Tapped += btn_PlanTabBTapped;
                //frame_plantabB.GestureRecognizers.Add(t_frame_plantabB);
                //frame_plantabCe.GestureRecognizers.Clear();
                //var t_frame_plantabCe = new TapGestureRecognizer();
                //t_frame_plantabCe.Tapped += btn_PlanTabCeTapped;
                //frame_plantabCe.GestureRecognizers.Add(t_frame_plantabCe);

                //frame_plantabC.GestureRecognizers.Clear();
                //var t_frame_plantabC = new TapGestureRecognizer();
                //t_frame_plantabC.Tapped += btn_PlanTabCTapped;
                //frame_plantabC.GestureRecognizers.Add(t_frame_plantabC);


                btn_objektinfo.GestureRecognizers.Clear();
                var t_btn_objektinfo = new TapGestureRecognizer();
                t_btn_objektinfo.Tapped += (object o, TappedEventArgs ev) => { OpenObjektInfoDialog(); };
                btn_objektinfo.GestureRecognizers.Add(t_btn_objektinfo);

                popupContainer_infodialog_close.GestureRecognizers.Clear();
                var t_popupContainer_infodialog_close = new TapGestureRecognizer();
                t_popupContainer_infodialog_close.Tapped += (object o, TappedEventArgs ev) => { CloseInfoDialog(); };
                popupContainer_infodialog_close.GestureRecognizers.Add(t_popupContainer_infodialog_close);


                popupContainer_quest_daypicker_close.GestureRecognizers.Clear();
                var t_popupContainer_quest_daypicker_close = new TapGestureRecognizer();
                t_popupContainer_quest_daypicker_close.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_daypicker.IsVisible = false; };
                popupContainer_quest_daypicker_close.GestureRecognizers.Add(t_popupContainer_quest_daypicker_close);
                popupContainer_quest_daypicker_open.GestureRecognizers.Clear();
                var t_popupContainer_quest_daypicker_open = new TapGestureRecognizer();
                t_popupContainer_quest_daypicker_open.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_daypicker.IsVisible = true; };
                popupContainer_quest_daypicker_open.GestureRecognizers.Add(t_popupContainer_quest_daypicker_open);

                popupContainer_ObjektPlanWeek_otherperson.GestureRecognizers.Clear();
                var t_popupContainer_ObjektPlanWeek_otherperson = new TapGestureRecognizer();
                t_popupContainer_ObjektPlanWeek_otherperson.Tapped += (object o, TappedEventArgs ev) => { OpenOtherPerson(); };
                popupContainer_ObjektPlanWeek_otherperson.GestureRecognizers.Add(t_popupContainer_ObjektPlanWeek_otherperson);

                popupContainer_ObjektPlanWeek_otherperson2.GestureRecognizers.Clear();
                var t_popupContainer_ObjektPlanWeek_otherperson2 = new TapGestureRecognizer();
                t_popupContainer_ObjektPlanWeek_otherperson2.Tapped += (object o, TappedEventArgs ev) => { OpenOtherPerson(); };
                popupContainer_ObjektPlanWeek_otherperson2.GestureRecognizers.Add(t_popupContainer_ObjektPlanWeek_otherperson2);

                popupContainer_quest_personpicker_close.GestureRecognizers.Clear();
                var t_popupContainer_ObjektPlanWeek_personpicker_close = new TapGestureRecognizer();
                t_popupContainer_ObjektPlanWeek_personpicker_close.Tapped += (object o, TappedEventArgs ev) => { CloseOtherPerson(); };
                popupContainer_quest_personpicker_close.GestureRecognizers.Add(t_popupContainer_ObjektPlanWeek_personpicker_close);

                //popupContainer_quest_langpicker_close.GestureRecognizers.Clear();
                //var t_popupContainer_langpicker_close = new TapGestureRecognizer();
                //t_popupContainer_langpicker_close.Tapped += (object o, TappedEventArgs ev) => { CloseLanguage(); };
                //popupContainer_quest_langpicker_close.GestureRecognizers.Add(t_popupContainer_langpicker_close);

                popupContainer_ObjektPlanWeek_Type.GestureRecognizers.Clear();
                var t_popupContainer_ObjektPlanWeek_Type = new TapGestureRecognizer();
                t_popupContainer_ObjektPlanWeek_Type.Tapped += (object o, TappedEventArgs ev) => { PlanTypeChange(); };
                popupContainer_ObjektPlanWeek_Type.GestureRecognizers.Add(t_popupContainer_ObjektPlanWeek_Type);

                popupContainer_ObjektPlanWeek_Reload.GestureRecognizers.Clear();
                var t_popupContainer_ObjektPlanWeek_Reload = new TapGestureRecognizer();
                t_popupContainer_ObjektPlanWeek_Reload.Tapped += (object o, TappedEventArgs ev) => { ReloadPlanData(0); };
                popupContainer_ObjektPlanWeek_Reload.GestureRecognizers.Add(t_popupContainer_ObjektPlanWeek_Reload);

                popupContainer_ObjektPlanWeek_Reload2.GestureRecognizers.Clear();
                var t_popupContainer_ObjektPlanWeek_Reload2 = new TapGestureRecognizer();
                t_popupContainer_ObjektPlanWeek_Reload2.Tapped += (object o, TappedEventArgs ev) => { ReloadPlanData(1); };
                popupContainer_ObjektPlanWeek_Reload2.GestureRecognizers.Add(t_popupContainer_ObjektPlanWeek_Reload2);




                frame_planConCe_LoadAll.GestureRecognizers.Clear();
                var t_frame_planConCe_LoadAll = new TapGestureRecognizer();
                t_frame_planConCe_LoadAll.Tapped += (object o, TappedEventArgs ev) => { GetChecksInfo(checkInfoLastView, true); };
                frame_planConCe_LoadAll.GestureRecognizers.Add(t_frame_planConCe_LoadAll);

                frame_planConCe_LoadAll1.GestureRecognizers.Clear();
                var t_frame_planConCe_LoadAll1 = new TapGestureRecognizer();
                t_frame_planConCe_LoadAll1.Tapped += (object o, TappedEventArgs ev) => { GetChecksInfo(7, true); };
                frame_planConCe_LoadAll1.GestureRecognizers.Add(t_frame_planConCe_LoadAll1);

                frame_planConCe_LoadAll2.GestureRecognizers.Clear();
                var t_frame_planConCe_LoadAll2 = new TapGestureRecognizer();
                t_frame_planConCe_LoadAll2.Tapped += (object o, TappedEventArgs ev) => { GetChecksInfo(99, true); };
                frame_planConCe_LoadAll2.GestureRecognizers.Add(t_frame_planConCe_LoadAll2);



                popupContainerSyncFaild_btn.GestureRecognizers.Clear();
                var tgr_popupContainerSyncFaild_btn = new TapGestureRecognizer();
                tgr_popupContainerSyncFaild_btn.Tapped += btn_SyncTapped;
                popupContainerSyncFaild_btn.GestureRecognizers.Add(tgr_popupContainerSyncFaild_btn);


                btn_mainmenu.GestureRecognizers.Clear();
                var tgr_MainMenu = new TapGestureRecognizer();
                tgr_MainMenu.Tapped += btn_MainMenuTapped;
                btn_mainmenu.GestureRecognizers.Add(tgr_MainMenu);

                btn_objScan.GestureRecognizers.Clear();
                var tgr_BuildingScan = new TapGestureRecognizer();
                tgr_BuildingScan.Tapped += btn_BuildingScanTapped;
                btn_objScan.GestureRecognizers.Add(tgr_BuildingScan);
                btn_objScanB.GestureRecognizers.Clear();
                var tgr_BuildingScanB = new TapGestureRecognizer();
                tgr_BuildingScanB.Tapped += btn_BuildingScanTapped;
                btn_objScanB.GestureRecognizers.Add(tgr_BuildingScanB);
                btn_objNotScan.GestureRecognizers.Clear();
                var tgr_BuildingNotScan = new TapGestureRecognizer();
                tgr_BuildingNotScan.Tapped += btn_BuildingNotScanTapped;
                btn_objNotScan.GestureRecognizers.Add(tgr_BuildingNotScan);

                btn_workerlist.GestureRecognizers.Clear();
                var tgr_WorkerList = new TapGestureRecognizer();
                tgr_WorkerList.Tapped += btn_WorkerListTapped;
                btn_workerlist.GestureRecognizers.Add(tgr_WorkerList);
                btn_todos.GestureRecognizers.Clear();
                var tgr_Todos = new TapGestureRecognizer();
                tgr_Todos.Tapped += btn_TodosTapped;
                btn_todos.GestureRecognizers.Add(tgr_Todos);

                btn_persontimes.GestureRecognizers.Clear();
                var tgr_persontimes = new TapGestureRecognizer();
                tgr_persontimes.Tapped += btn_PersonTimesTapped;
                btn_persontimes.GestureRecognizers.Add(tgr_persontimes);

                btn_regist.GestureRecognizers.Clear();
                var tgr_Regist = new TapGestureRecognizer();
                tgr_Regist.Tapped += btn_RegistTapped;
                btn_regist.GestureRecognizers.Add(tgr_Regist);

                btn_settings.GestureRecognizers.Clear();
                var tgr_Settings = new TapGestureRecognizer();
                tgr_Settings.Tapped += btn_SettingsTapped;
                btn_settings.GestureRecognizers.Add(tgr_Settings);




                btn_sync.GestureRecognizers.Clear();
                var tgr_sync = new TapGestureRecognizer();
                tgr_sync.Tapped += btn_SyncTapped;
                btn_sync.GestureRecognizers.Add(tgr_sync);

                btn_dsgvo.GestureRecognizers.Clear();
                var tgr_dsgvo = new TapGestureRecognizer();
                tgr_dsgvo.Tapped += btn_DSGVOTapped;
                btn_dsgvo.GestureRecognizers.Add(tgr_dsgvo);

                btn_worker_back.GestureRecognizers.Clear();
                var tgr_WorkerBack = new TapGestureRecognizer();
                tgr_WorkerBack.Tapped += btn_WorkerBackTapped;
                btn_worker_back.GestureRecognizers.Add(tgr_WorkerBack);

                btn_notscan_back.GestureRecognizers.Clear();
                var tgr_NotScanBack = new TapGestureRecognizer();
                tgr_NotScanBack.Tapped += btn_NotScanBackTapped;
                btn_notscan_back.GestureRecognizers.Add(tgr_NotScanBack);

                // btn_persontimes_back.GestureRecognizers.Clear(); // moved into PersonTimesPageView
                // tgr_PersontimesBack.Tapped += btn_PersontimesBackTapped; // moved into PersonTimesPageView
                // btn_persontimes_back.GestureRecognizers.Add(tgr_PersontimesBack); // moved into PersonTimesPageView
                PersonTimesPageView.BtnPersontimesBack.GestureRecognizers.Clear();
                var tgr_PersontimesBack = new TapGestureRecognizer();
                tgr_PersontimesBack.Tapped += btn_PersontimesBackTapped;
                PersonTimesPageView.BtnPersontimesBack.GestureRecognizers.Add(tgr_PersontimesBack);

                // btn_persontime_load.GestureRecognizers.Clear(); // moved into PersonTimesPageView
                // tgr_PersontimesLoad.Tapped += pick_persontimes_SelectedIndexChanged; // moved into PersonTimesPageView
                // btn_persontime_load.GestureRecognizers.Add(tgr_PersontimesLoad); // moved into PersonTimesPageView
                PersonTimesPageView.BtnPersontimeLoad.GestureRecognizers.Clear();
                var tgr_PersontimesLoad = new TapGestureRecognizer();
                tgr_PersontimesLoad.Tapped += pick_persontimes_SelectedIndexChanged;
                PersonTimesPageView.BtnPersontimeLoad.GestureRecognizers.Add(tgr_PersontimesLoad);




                //popupContainer_Alert_btn.GestureRecognizers.Clear();
                //var tgr9 = new TapGestureRecognizer();
                //tgr9.Tapped -= HideAlertMessage;
                //tgr9.Tapped += HideAlertMessage;
                //popupContainer_Alert_btn.GestureRecognizers.Add(tgr9);

                // Handwerker nach Kategorien suchen
                btn_workercategorysearch.GestureRecognizers.Clear();
                var tgr_workercategorysearch = new TapGestureRecognizer();
                tgr_workercategorysearch.Tapped += btn_WorkerCategorySearchTapped;
                btn_workercategorysearch.GestureRecognizers.Add(tgr_workercategorysearch);
                // Handwerker nach Namen suchen
                btn_workernamesearch.GestureRecognizers.Clear();
                var tgr_WorkerNamesearch = new TapGestureRecognizer();
                tgr_WorkerNamesearch.Tapped += btn_WorkerNameSearchTapped;
                btn_workernamesearch.GestureRecognizers.Add(tgr_WorkerNamesearch);
                // Handwerker nach Objekten suchen
                btn_workerbuildingsearch.GestureRecognizers.Clear();
                var tgr_WorkerBuildingsearch = new TapGestureRecognizer();
                tgr_WorkerBuildingsearch.Tapped += btn_WorkerBuildingSearchTapped;
                btn_workerbuildingsearch.GestureRecognizers.Add(tgr_WorkerBuildingsearch);


                // BuidlingOutScan Back to MainPage
                btn_overtootherBuildingSave.GestureRecognizers.Clear();
                var tgr_overtootherBuildingSave = new TapGestureRecognizer();
                tgr_overtootherBuildingSave.Tapped += btn_done_BuildingOutScanTapped;
                btn_overtootherBuildingSave.GestureRecognizers.Add(tgr_overtootherBuildingSave);
                //btn_back_inBuildingOutScan.GestureRecognizers.Clear();
                //var tgr_back_inBuildingOutScan = new TapGestureRecognizer();
                //tgr_back_inBuildingOutScan.Tapped += btn_back_BuildingOutScanTapped;
                //btn_back_inBuildingOutScan.GestureRecognizers.Add(tgr_back_inBuildingOutScan);
                //btn_flashlight_Out_container.GestureRecognizers.Clear();
                //var tapGestureRecognizer1b = new TapGestureRecognizer();
                //tapGestureRecognizer1b.Tapped += AppModel.Instance.Scan.Btn_FlashlightTapped;
                //btn_flashlight_Out_container.GestureRecognizers.Add(tapGestureRecognizer1b);

                // BuidlingScan Back to MainPage
                //btn_back_inBuildingScan.GestureRecognizers.Clear();
                //var tgr_back_inBuildingScan = new TapGestureRecognizer();
                //tgr_back_inBuildingScan.Tapped += btn_back_BuildingScanTapped;
                //btn_back_inBuildingScan.GestureRecognizers.Add(tgr_back_inBuildingScan);
                //btn_flashlight_container.GestureRecognizers.Clear();
                //var tapGestureRecognizer1 = new TapGestureRecognizer();
                //tapGestureRecognizer1.Tapped += AppModel.Instance.Scan.Btn_FlashlightTapped;
                //btn_flashlight_container.GestureRecognizers.Add(tapGestureRecognizer1);


                // BuildingOrder 
                btn_back_inBuildingOrder.GestureRecognizers.Clear();
                var tapGestureRecognizer2 = new TapGestureRecognizer();
                tapGestureRecognizer2.Tapped += btn_back_BuildingOrderTapped;
                btn_back_inBuildingOrder.GestureRecognizers.Add(tapGestureRecognizer2);
                btn_back_inBuildingOrder_category.GestureRecognizers.Clear();
                var tapGestureRecognizer2b = new TapGestureRecognizer();
                tapGestureRecognizer2b.Tapped += btn_back_OrderCategoryTapped;
                btn_back_inBuildingOrder_category.GestureRecognizers.Add(tapGestureRecognizer2b);

                btn_back_inBuildingOrder_category_showall.GestureRecognizers.Clear();
                var tapGestureRecognizer2ball = new TapGestureRecognizer();
                tapGestureRecognizer2ball.Tapped += btn_showall_OrderCategoryTapped;
                btn_back_inBuildingOrder_category_showall.GestureRecognizers.Add(tapGestureRecognizer2ball);

                btn_back_inBuildingOrder_category_showall_again.GestureRecognizers.Clear();
                var tapGestureRecognizer2ball_again = new TapGestureRecognizer();
                tapGestureRecognizer2ball_again.Tapped += btn_showall_again_OrderCategoryTapped;
                btn_back_inBuildingOrder_category_showall_again.GestureRecognizers.Add(tapGestureRecognizer2ball_again);


                btn_back_inBuildingOrder_position.GestureRecognizers.Clear();
                var tapGestureRecognizer2c = new TapGestureRecognizer();
                tapGestureRecognizer2c.Tapped += btn_back_CategoryPositionTapped;
                btn_back_inBuildingOrder_position.GestureRecognizers.Add(tapGestureRecognizer2c);


                btn_objvalues_container.GestureRecognizers.Clear();
                var tgr_objvalues_container = new TapGestureRecognizer();
                tgr_objvalues_container.Tapped += btn_ShowObjectValuesTapped;
                btn_objvalues_container.GestureRecognizers.Add(tgr_objvalues_container);

                btn_back_ObjectValues.GestureRecognizers.Clear();
                var tgr_back_ObjectValue = new TapGestureRecognizer();
                tgr_back_ObjectValue.Tapped += btn_CloseObjectValuesTapped;
                btn_back_ObjectValues.GestureRecognizers.Add(tgr_back_ObjectValue);

                btn_back_ObjectValues_edit.GestureRecognizers.Clear();
                var tgr_back_ObjectValue_edit = new TapGestureRecognizer();
                tgr_back_ObjectValue_edit.Tapped += btn_CloseObjectValuesEditTapped;
                btn_back_ObjectValues_edit.GestureRecognizers.Add(tgr_back_ObjectValue_edit);

                btn_buildingout_container.GestureRecognizers.Clear();
                var tapGestureRecognizer4 = new TapGestureRecognizer();
                tapGestureRecognizer4.Tapped += btn_ClearLastBuildingTapped;
                btn_buildingout_container.GestureRecognizers.Add(tapGestureRecognizer4);

                btn_buildingorder.GestureRecognizers.Clear();
                var tapGestureRecognizer3 = new TapGestureRecognizer();
                tapGestureRecognizer3.Tapped += btn_AuftraegeAuswaehlen;
                btn_buildingorder.GestureRecognizers.Add(tapGestureRecognizer3);


                btn_inwork.GestureRecognizers.Clear();
                var tapGestureRecognizer6 = new TapGestureRecognizer();
                tapGestureRecognizer6.Tapped += btn_ShowRunningWorks;
                btn_inwork.GestureRecognizers.Add(tapGestureRecognizer6);

                // Show Leistungen zur Ausführen ausgewählt
                btn_showselected_pos.GestureRecognizers.Clear();
                var tapGestureRecognizer7 = new TapGestureRecognizer();
                tapGestureRecognizer7.Tapped += btn_AuswahlAnzeigen;
                btn_showselected_pos.GestureRecognizers.Add(tapGestureRecognizer7);
                btn_showselected_pos2.GestureRecognizers.Clear();
                var tapGestureRecognizer8 = new TapGestureRecognizer();
                tapGestureRecognizer8.Tapped += btn_AuswahlAnzeigen;
                btn_showselected_pos2.GestureRecognizers.Add(tapGestureRecognizer8);


                btn_startselected_pos.GestureRecognizers.Clear();
                var tgr_startselected_pos = new TapGestureRecognizer();
                tgr_startselected_pos.Tapped += StartSelectedPos;
                btn_startselected_pos.GestureRecognizers.Add(tgr_startselected_pos);

                // RunningWorks
                btn_back_runningworks.GestureRecognizers.Clear();
                var tgr_back_runningworks = new TapGestureRecognizer();
                tgr_back_runningworks.Tapped += btn_RunningWorksBackTapped;
                btn_back_runningworks.GestureRecognizers.Add(tgr_back_runningworks);
                btn_runningworks_over.GestureRecognizers.Clear();
                var tgr_runningworks_over = new TapGestureRecognizer();
                tgr_runningworks_over.Tapped += btn_RunningWorksOverTapped;
                btn_runningworks_over.GestureRecognizers.Add(tgr_runningworks_over);

                // Bemerkung
                btn_alertmessage_container.GestureRecognizers.Clear();
                var tgr_alertmessage_container = new TapGestureRecognizer();
                tgr_alertmessage_container.Tapped += btn_ShowNoticePrioTapped;
                btn_alertmessage_container.GestureRecognizers.Add(tgr_alertmessage_container);
                btn_message_container.GestureRecognizers.Clear();
                var tgr_message_container = new TapGestureRecognizer();
                tgr_message_container.Tapped += btn_ShowNoticeTapped;
                btn_message_container.GestureRecognizers.Add(tgr_message_container);


                //ChecklistContainer
                btn_back_check.GestureRecognizers.Clear();
                var tgr_back_check = new TapGestureRecognizer();
                tgr_back_check.Tapped += CloseCheckA;
                btn_back_check.GestureRecognizers.Add(tgr_back_check);
                btn_back_check_signature.GestureRecognizers.Clear();
                var tgr_back_check_signature = new TapGestureRecognizer();
                tgr_back_check_signature.Tapped += CloseCheckA_Singature;
                btn_back_check_signature.GestureRecognizers.Add(tgr_back_check_signature);


                btn_exitwork.GestureRecognizers.Clear();
                var tgr_ExitWork = new TapGestureRecognizer();
                tgr_ExitWork.Tapped += DayOverTapped;
                btn_exitwork.GestureRecognizers.Add(tgr_ExitWork);



                //btn_back_pn.GestureRecognizers.Clear();
                //var tgr_back_pn = new TapGestureRecognizer();
                //tgr_back_pn.Tapped += btn_PN_BackTapped;
                //btn_back_pn.GestureRecognizers.Add(tgr_back_pn);


                btn_nachbuchen_back.GestureRecognizers.Clear();
                var tgr_btn_nachbuchen_back = new TapGestureRecognizer();
                tgr_btn_nachbuchen_back.Tapped += (object o, TappedEventArgs ev) =>
                {
                    if (AppModel.Instance.LastSelectedCategoryAgain == null)
                    {
                        btn_back_inBuildingOrder_category_showall_again_txt.Text = "Alle zeigen";
                        AppModel.Instance._showall_again_OrderCategory = false;
                        this.Focus(); ShowMainPage();
                    }
                    else
                    {
                        btn_nachbuchen_Tapped(AppModel.Instance.posAgain);
                    }
                };
                btn_nachbuchen_back.GestureRecognizers.Add(tgr_btn_nachbuchen_back);
                btn_nachbuchen_cat_back.GestureRecognizers.Clear();
                var tgr_nachbuchen_cat_back = new TapGestureRecognizer();
                tgr_nachbuchen_cat_back.Tapped += (object o, TappedEventArgs ev) => { btn_nachbuchen_Tapped(AppModel.Instance.posAgain); };
                btn_nachbuchen_cat_back.GestureRecognizers.Add(tgr_nachbuchen_cat_back);
                btn_nachbuchen.GestureRecognizers.Clear();
                var tgr_nachbuchen = new TapGestureRecognizer();
                tgr_nachbuchen.Tapped += (object o, TappedEventArgs ev) => { ShowNachbuchenPage(AppModel.Instance.posAgain); };
                btn_nachbuchen.GestureRecognizers.Add(tgr_nachbuchen);
                btn_nachbuchen_Produkte.GestureRecognizers.Clear();
                var tgr_produkt_nachbuchen = new TapGestureRecognizer();
                tgr_produkt_nachbuchen.Tapped += (object o, TappedEventArgs ev) => { btn_nachbuchen_Tapped(0); };
                btn_nachbuchen_Produkte.GestureRecognizers.Add(tgr_produkt_nachbuchen);
                btn_nachbuchen_Pos.GestureRecognizers.Clear();
                var tgr_nachbuchen_Pos = new TapGestureRecognizer();
                tgr_nachbuchen_Pos.Tapped += (object o, TappedEventArgs ev) => { btn_nachbuchen_Tapped(1); };
                btn_nachbuchen_Pos.GestureRecognizers.Add(tgr_nachbuchen_Pos);
                btn_showselected_pos_Again.GestureRecognizers.Clear();
                var tgr_showselected_pos_Again = new TapGestureRecognizer();
                tgr_showselected_pos_Again.Tapped += btn_AuswahlAnzeigen_Again;
                btn_showselected_pos_Again.GestureRecognizers.Add(tgr_showselected_pos_Again);


                btn_objectValuesNow.GestureRecognizers.Clear();
                var tgr_btn_objectValuesNow = new TapGestureRecognizer();
                tgr_btn_objectValuesNow.Tapped += btn_objectValuesNowTapped;
                btn_objectValuesNow.GestureRecognizers.Add(tgr_btn_objectValuesNow);
                btn_objectValuesToday.GestureRecognizers.Clear();
                var tgr_btn_objectValuesToday = new TapGestureRecognizer();
                tgr_btn_objectValuesToday.Tapped += btn_objectValuesTodayTapped;
                btn_objectValuesToday.GestureRecognizers.Add(tgr_btn_objectValuesToday);
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Error in InitStartPageHandlers(MainPage): " + ex.Message + " | Stacktrace: " + ex.StackTrace);
            }
        }

        /*******************/
        /* LAST  BUILDINGS */
        /*******************/
        private async void SetLastBuilding()
        {
            lastBuilding_Container.IsVisible = (AppModel.Instance.LastBuilding != null);
            lastBuilding_ContainerBottom.IsVisible = (AppModel.Instance.LastBuilding != null);
            btn_objektinfo_container.IsVisible = (AppModel.Instance.LastBuilding != null && !String.IsNullOrWhiteSpace(AppModel.Instance.LastBuilding.notiz));

            btn_buildingorder_container.IsVisible = AppModel.Instance.LastBuilding != null;
            btn_exitwork.IsVisible = AppModel.Instance.allPositionInWork == null;
            //btn_buildingorderToTime_container.IsVisible = AppModel.Instance.LastBuilding != null;
            btn_inwork_container.IsVisible = AppModel.Instance.allPositionInWork != null;
            btn_nachbuchen_container.IsVisible = AppModel.Instance.allPositionInWork != null;
            btn_regist.IsVisible = AppModel.Instance.allPositionInWork == null;

            // Plan zeigen/ ausblenden
            if (btn_nachbuchen_container.IsVisible)
            {
                //HidePlaningView(); 
                // ObjektPlanWeekMobil_Stack_Spacer.IsVisible = true; 
            }
            else
            {
                //SetAppControll();
                // ObjektPlanWeekMobil_Stack_Spacer.IsVisible = false; 
            }


            ObjektPlanWeekMobil_Stack_A.Margin = new Thickness(2, (AppModel.Instance.allPositionInWork == null ? 20 : 0), 2, 0);

            // Trennlinie zeigen
            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                btn_objScan.IsVisible = false;
                btn_objNotScan.IsVisible = (AppModel.Instance.LastBuilding == null || AppModel.Instance.allPositionInWork == null);
                btn_objScanB.IsVisible = (AppModel.Instance.LastBuilding == null || AppModel.Instance.allPositionInWork == null);
            }
            else
            {
                btn_objScan.IsVisible = (AppModel.Instance.LastBuilding == null || AppModel.Instance.allPositionInWork == null);
                btn_objNotScan.IsVisible = false;
                btn_objScanB.IsVisible = false;
            }

            if (AppModel.Instance.LastBuilding != null)
            {
                last_building_name.IsVisible = !String.IsNullOrWhiteSpace(AppModel.Instance.LastBuilding.objektname);
                //last_building_addressZipCity.IsVisible = !String.IsNullOrWhiteSpace(AppModel.Instance.LastBuilding.objektname);
                last_building_name.Text = AppModel.Instance.LastBuilding.objektname;
                last_building_address.Text = AppModel.Instance.LastBuilding.strasse + " " + AppModel.Instance.LastBuilding.hsnr;
                var la = AppModel.Instance.LastBuilding.land.Length > 2 ? AppModel.Instance.LastBuilding.land.Substring(0, 3) : ((String.IsNullOrWhiteSpace(AppModel.Instance.LastBuilding.land) ? "" : AppModel.Instance.LastBuilding.land));
                last_building_zip_city.Text = (String.IsNullOrWhiteSpace(la) ? "" : la + " ") + AppModel.Instance.LastBuilding.plz + " " + AppModel.Instance.LastBuilding.ort;

                // MainPage Badge in Ausgewähltes Objekt
                double _prio = 100000000;
                AppModel.Instance.LastBuilding.ArrayOfAuftrag.ForEach(order =>
                {
                    order.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(l =>
                        {
                            l.prio = Prio.GetLeistungPrio(l, AppModel.Instance);
                            _prio = Math.Min(_prio, l.prio.days);
                        });
                    });
                });
                // Zeige Heute und Fällige 
                btn_buildingorderToTime_count.IsVisible = (_prio < 1);
                btn_buildingorderToTime_counttext.Text = "" + _prio;

            }

            if (AppModel.Instance.allPositionInWork != null)
            {
                btn_buildingorder_container.IsVisible = false;
                btn_exitwork.IsVisible = false;
                // Erstmal deaktiviert, da implementierung noch gemacht werden mus für das Stopen der laufenden und dann wieder die Neuen 
                var ts = (DateTime.Now - new DateTime(AppModel.Instance.allPositionInWork.startticks));
                inwork_starttime_text.Text = (ts.TotalDays > 1 ? ts.ToString("%d") + "T " : "") + ts.ToString(@"hh\:mm");
                inwork_start_count_text.Text = "" + AppModel.Instance.allPositionInWork.leistungen.Count;
            }

            var dayOverLast = DayOverWSO.LoadLast(AppModel.Instance);
            if (dayOverLast != null)
            {
                var dt = new DateTime(dayOverLast.endticks);
                dayOverLastDate.Text = dt.ToString("dd.MM.yyyy") + " - " + dt.ToString("HH:mm");
            }
            //if (AppModel.Instance.LastBuilding != null)
            //{
            //    await lastBuilding_Container.FadeToAsync(1, 500, Easing.SpringIn);
            //}
            //else
            //{
            //}
        }

        /*******************/
        /* CHECK BUILDINGS */
        /*******************/
        private async void SyncBuilding(bool manuellSync = false)
        {
            if (manuellSync || String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks))
            {
                SyncBuildingManuell(true);
            }
            else
            {
                if (long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) < DateTime.Now.AddDays(-4).Ticks)
                {
                    SyncBuildingManuell(true);
                }
                else
                {
                    FastSync();
                }
            }
            // Checlisten Count setzen
            SetChecksCount();
        }

        private async void CheckForBuildingFailed(IpmBuildingResponse ipmBuildingResponse)
        {
            if (AppModel.Instance.AllBuildings == null || AppModel.Instance.AllBuildings.Count == 0)
            {
                await DisplayAlertAsync("Objektprüfung nicht möglich!",
                    ipmBuildingResponse != null ? ipmBuildingResponse.message : "FEHLER: Muss Online gehen, kann aber nicht!", "Zurück");
            }
        }
        private void SyncBuildingDone(IpmBuildingResponse ipmBuildingResponse)
        {
            ipmBuildingResponse.builgings.ForEach(b => { BuildingWSO.Save(AppModel.Instance, b); });
            AppModel.Instance.AllBuildings = ipmBuildingResponse.builgings.OrderBy(o => o.id).ToList();
            AppModel.Instance.InitBuildingsAgain();
            SetLastBuilding();
        }

        /// <summary>
        /// Starts the building sync (Gebäude + Aufträge) with background protection.
        /// On Android: Starts ForegroundService which handles the sync
        /// On iOS: Disables IdleTimer and runs sync on UI thread
        /// Progress and completion are communicated back via SyncCoordinator events.
        /// </summary>
        private async void SyncBuildingManuell(bool manuellSync = false)
        {
            try
            {
                popupContainer.IsVisible = true;
                popupContainer_count.Text = "SYNCHRONISATION (0%)";
                await Task.Delay(1);

                // Get or initialize the background sync service
                if (_backgroundSyncService == null)
                {
                    try
                    {
                        _backgroundSyncService = IPlatformApplication.Current?.Services?.GetService<iPMCloud.Mobile.Services.IBackgroundSyncService>();
                    }
                    catch (Exception ex)
                    {
                        AppModel.Logger?.Warn($"Could not resolve IBackgroundSyncService: {ex.Message}");
                    }
                }

                // Always unsubscribe before subscribing to prevent duplicate registrations
                iPMCloud.Mobile.Services.SyncCoordinator.ProgressChanged -= OnSyncProgress;
                iPMCloud.Mobile.Services.SyncCoordinator.SyncCompleted -= OnSyncCompleted;
                iPMCloud.Mobile.Services.SyncCoordinator.ProgressChanged += OnSyncProgress;
                iPMCloud.Mobile.Services.SyncCoordinator.SyncCompleted += OnSyncCompleted;

                // Start background protection and sync
                bool protectionStarted = false;
                if (_backgroundSyncService != null)
                {
                    protectionStarted = await _backgroundSyncService.StartSyncProtectionAsync();
                    AppModel.Logger?.Info($"Background sync protection started: {protectionStarted}");
                }

                // On Android, the ForegroundService handles the sync.
                // On iOS/other platforms, we run it here with IdleTimer disabled.
#if !ANDROID
                    // iOS and other platforms: run sync here with background protection (IdleTimer disabled)
                    Task.Run(() => iPMCloud.Mobile.Services.SyncCoordinator.Instance.RunAsync())
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                                AppModel.Logger.Error("SyncBuildingManuell Task faulted: " + t.Exception?.GetBaseException()?.Message);
                        }, TaskContinuationOptions.OnlyOnFaulted);
#else
                // Android: Sync is handled by SyncForegroundService (already started above)
                // Do not start sync here to avoid duplicate execution
                AppModel.Logger?.Info("Android: Sync delegated to SyncForegroundService");
#endif

            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"Method => MainPage-SyncBuildingManuell(catch): {ex.Message} | StackTrace: {ex.StackTrace}");
                AppModel.Instance.InclFilesAsJson = true;
                var ok = AppModel.Instance.SendLogZipFile();
                await Task.Delay(2000);

                // Ensure UI is reset in case of error
                try
                {
                    popupContainer.IsVisible = false;
                }
                catch { }
            }
        }

        /// <summary>Updates the progress popup text during sync.</summary>
        private void OnSyncProgress(object sender, iPMCloud.Mobile.Services.SyncProgressEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try { popupContainer_count.Text = e.StatusText; }
                catch (Exception ex) { AppModel.Logger.Warn("OnSyncProgress UI: " + ex.Message); }
            });
        }

        /// <summary>Called when SyncCoordinator finishes (success or failure).</summary>
        private void OnSyncCompleted(object sender, iPMCloud.Mobile.Services.SyncCompletedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Always unsubscribe first
                    iPMCloud.Mobile.Services.SyncCoordinator.ProgressChanged -= OnSyncProgress;
                    iPMCloud.Mobile.Services.SyncCoordinator.SyncCompleted -= OnSyncCompleted;

#if !ANDROID
                    // On iOS and other non-Android platforms, stop the background protection
                    // (On Android, the ForegroundService stops itself)
                    if (_backgroundSyncService != null && _backgroundSyncService.IsActive)
                    {
                        try
                        {
                            await _backgroundSyncService.StopSyncProtectionAsync();
                            AppModel.Logger?.Info("Background sync protection stopped (iOS)");
                        }
                        catch (Exception ex)
                        {
                            AppModel.Logger?.Error($"Error stopping background sync protection: {ex.Message}");
                        }
                    }
#endif

                    if (e.Success && e.Response != null)
                    {
                        // AppControll UI refresh (data was already saved by SyncCoordinator)
                        SetAppControll();
                        UpdateSyncCounter(100d);
                        box_buildingInformation.Children.Clear();
                        box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(AppModel.Instance, DateTime.Now));
                        await SyncNewBuildingManuell_nextAsync(e.Response);
                    }
                    else
                    {
                        // Synchronisierung FAILED
                        AppModel.Logger.Warn("WARN: iPM.Mobile Error (0): Sync FEHLGESCHLAGEN  => NewSyncBuilding: " + e.ErrorMessage);
                        popupContainer.IsVisible = false;
                        await Task.Delay(1);
                        if (e.ErrorMessage == "Nicht vollständig synchronisiert")
                        {
                            popupContainerSyncFaild.IsVisible = true;
                        }
                        else
                        {
                            CheckForNewBuildingFailed(e.Response);
                            await Load_PlanTabsAsync(((int)DateTime.Now.DayOfWeek));

                            var dt = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ?
                                DateTime.Now.AddDays(-2) : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                            box_buildingInformation.Children.Clear();
                            box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(AppModel.Instance, dt));
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Error($"Method => MainPage-OnSyncCompleted(catch): {ex.Message} | StackTrace: {ex.StackTrace}");

                    // Ensure UI is in a consistent state even if an error occurs
                    try
                    {
                        popupContainer.IsVisible = false;
                    }
                    catch { }
                }
                finally
                {
                    __isFirstInit = false;
                }
            });
        }
        private async void UpdateSyncCounter(double pr)
        {
            popupContainer_count.Text = "SYNCHRONISATION (" + pr.ToString("###,##") + "%)";
            await Task.Delay(1);
        }

        /// <summary>
        /// Async version of SyncNewBuildingManuell_next to properly handle async/await chain
        /// </summary>
        private async Task SyncNewBuildingManuell_nextAsync(IpmNewSyncResponse ipmNewBuildingResponse)
        {
            try
            {
                // Erfolgreich synchronisiert
                // AppModel.Logger.Info("Info: Sync war erfolgreich => SyncBuilding");
                AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = DateTime.Now.Ticks.ToString();
                //var dt = new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                AppModel.Instance.SettingModel.SaveSettings();

                BuildingWSO.DeleteBuildings(ipmNewBuildingResponse.deletedBuidlings);

                if (AppModel.Instance.AppControll.lang == "de" || !AppModel.Instance.AppControll.translation)
                {
                    NewSyncBuildingDone(ipmNewBuildingResponse);
                    AppModel.Instance.SetAllKategorieNames();
                }
                else
                {
                    //Sync und Übersetzen
                    var _ = await translateAfterSyncedBuildings(AppModel.Instance.AppControll.lang, ipmNewBuildingResponse.builgings, AppModel.Instance.Lang.lang != AppModel.Instance.AppControll.lang);
                    AppModel.Instance.AllBuildings = ipmNewBuildingResponse.builgings.OrderBy(o => o.id).ToList();
                    AppModel.Instance.InitBuildingsAgain();
                    SetLastBuilding();
                    AppModel.Instance.SetAllKategorieNames();
                }

                if (AppModel.Instance.Lang.lang != AppModel.Instance.AppControll.lang)
                {
                    AppModel.Instance.Lang.lang = AppModel.Instance.AppControll.lang;
                    Lang.Save(AppModel.Instance.Lang);
                }


                popupContainer.IsVisible = false;
                await Task.Delay(1000);
                //********* Update Plandaten 
                await Load_PlanTabsAsync(((int)DateTime.Now.DayOfWeek));

            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"Method => MainPage-SyncNewBuildingManuell_nextAsync(catch): {ex.Message} | StackTrace: {ex.StackTrace}");
                AppModel.Instance.InclFilesAsJson = false;
                throw; // Re-throw to be caught by OnSyncCompleted's catch block
            }
        }


        private async void CheckForNewBuildingFailed(IpmNewSyncResponse ipmNewBuildingResponse)
        {
            if (AppModel.Instance.AllBuildings == null || AppModel.Instance.AllBuildings.Count == 0)
            {
                await DisplayAlertAsync("Objektprüfung nicht möglich!",
                    ipmNewBuildingResponse != null ? ipmNewBuildingResponse.message : "FEHLER: Verbindung Online nicht möglich!", "Zurück");
            }
        }
        private void NewSyncBuildingDone(IpmNewSyncResponse ipmNewBuildingResponse)
        {
            ipmNewBuildingResponse.builgings.ForEach(b => { BuildingWSO.Save(AppModel.Instance, b); });
            AppModel.Instance.AllBuildings = ipmNewBuildingResponse.builgings.OrderBy(o => o.id).ToList();
            AppModel.Instance.InitBuildingsAgain();
            SetLastBuilding();
        }

        private async void FastSync(bool run = false)
        {
            var dt = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ?
                DateTime.Now.AddDays(-2) : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
            if (run || dt.AddHours(AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours) < DateTime.Now) //(dt.AddHours(4) < DateTime.Now || manuellSync)
            {
                //AppModel.Logger.Info("Info: STARTE FastSync Objekte/Auftraege/Leistungen/weitere... => FastSync");
                // Objekte sycnen erforderlich nach 12 Stunden
                popupContainer.IsVisible = true;
                await Task.Delay(1);

                IpmBuildingResponse fastSyncResponse = await Task.Run(() => { return AppModel.Instance.Connections.IpmFastSync(); });
                if (fastSyncResponse == null || !fastSyncResponse.success)
                {
                    // Synchronisierung FAILED
                    AppModel.Logger.Warn("WARN: iPM.Mobile Error (0): FastSync FEHLGESCHLAGEN  => FastSync" +
                        (fastSyncResponse != null ? fastSyncResponse.message : ""));
                    popupContainer.IsVisible = false;
                    await Task.Delay(1);
                    //********* Update Plandaten 
                    __isFirstInit = false;
                    Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                }
                else
                {
                    if (fastSyncResponse.AppControll != null)
                    {
                        AppModel.Instance.AppControll = fastSyncResponse.AppControll;
                        if (AppModel.Instance.AppControll == null) { AppModel.Instance.AppControll = new AppControll(); }
                        AppControll.Save(AppModel.Instance, AppModel.Instance.AppControll);
                        SetAppControll();
                    }

                    // Erfolgreich synchronisiert
                    //AppModel.Logger.Info("Info: FastSync war erfolgreich => FastSync");
                    AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = DateTime.Now.Ticks.ToString();
                    //dt = new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                    AppModel.Instance.SettingModel.SaveSettings();

                    //BuildingWSO.DeleteBuildings(fastSyncResponse.deletedBuidlings);

                    // Sprache hat sich geändert 
                    if (AppModel.Instance.Lang.lang != AppModel.Instance.AppControll.lang && AppModel.Instance.AppControll.translation)
                    {
                        SyncBuildingManuell(true);
                        return;
                    }
                    else
                    {
                        if (AppModel.Instance.AppControll.lang == "de" || !AppModel.Instance.AppControll.translation)
                        {
                            FastSyncUpdate(fastSyncResponse, true);
                            AppModel.Instance.SetAllKategorieNames();
                        }
                        else
                        {
                            //Sync und Übersetzen
                            var _ = await translateAfterSyncedBuildings(AppModel.Instance.AppControll.lang, fastSyncResponse.builgings, AppModel.Instance.Lang.lang != AppModel.Instance.AppControll.lang);
                            FastSyncUpdate(fastSyncResponse, false);
                            AppModel.Instance.SetAllKategorieNames();
                        }
                    }

                    if (AppModel.Instance.Lang.lang != AppModel.Instance.AppControll.lang)
                    {
                        AppModel.Instance.Lang.lang = AppModel.Instance.AppControll.lang;
                        Lang.Save(AppModel.Instance.Lang);
                    }


                    popupContainer.IsVisible = false;
                    await Task.Delay(1);

                    //********* Update Plandaten
                    __isFirstInit = false;
                    Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                    var dts = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ?
                        DateTime.Now.AddDays(-2) : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                    box_buildingInformation.Children.Clear();
                    box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(AppModel.Instance, dts));
                }
            }
            else
            {
                __isFirstInit = false;
                Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                var dtss = String.IsNullOrEmpty(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ?
                    DateTime.Now.AddDays(-2) : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                box_buildingInformation.Children.Clear();
                box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(AppModel.Instance, dtss));
            }
        }
        private void FastSyncUpdate(IpmBuildingResponse fastSyncResponse, bool saveBuildingByNotTranslation)
        {
            if (fastSyncResponse.builgings != null)
            {
                fastSyncResponse.builgings.ForEach(b =>
                {
                    if (b.del == 0 && b.ArrayOfAuftrag != null && b.ArrayOfAuftrag.Count > 0)
                    {
                        bool isKategories = false;
                        bool isLeistungen = false;
                        foreach (var auf in b.ArrayOfAuftrag)
                        {
                            if (auf.kategorien != null && auf.kategorien.Count > 0)
                            {
                                isKategories = true;
                                auf.kategorien.ForEach(k =>
                                {
                                    if (k.leistungen != null && k.leistungen.Count > 0)
                                    {
                                        isLeistungen = true;
                                    }
                                });
                            }
                        }
                        if (isLeistungen)
                        {
                            if (saveBuildingByNotTranslation) { BuildingWSO.Save(AppModel.Instance, b); }
                            var i = AppModel.Instance.AllBuildings.FindIndex(f => f.id == b.id);
                            if (i > -1)
                            {
                                AppModel.Instance.AllBuildings[i] = b;
                            }
                            else
                            {
                                AppModel.Instance.AllBuildings.Add(b);
                            }
                        }
                        else
                        {
                            if (isKategories)
                            {
                                AppModel.Logger.Warn("WARN: FastSync - Aufträge ohne Leistungen: Objekt:" + b.id + " " +
                                    b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr);
                            }
                            else
                            {
                                AppModel.Logger.Warn("WARN: FastSync - Aufträge ohne Kategorien: Objekt: " + b.id + " " +
                                    b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr);
                            }
                        }
                    }
                    else
                    {
                        AppModel.Logger.Warn("WARN: FastSync - Objekt gelöscht oder keine Aufträge vorhanden: " + b.id + " " +
                            b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr);
                        BuildingWSO.DeleteBuilding(b.id);
                        AppModel.Instance.AllBuildings.Remove(b);
                    }
                });
                //AppModel.Instance.AllBuildings.ForEach(b =>
                //{
                //    if (b.del > 0 || b.ArrayOfAuftrag.Count == 0)
                //    {
                //        BuildingWSO.DeleteBuilding(b.id);
                //    }
                //});
                //AppModel.Instance.AllBuildings.Remove All(b => b.del > 0 || b.ArrayOfAuftrag.Count == 0);
                AppModel.Instance.AllBuildings = AppModel.Instance.AllBuildings.OrderBy(o => o.id).ToList();
                AppModel.Instance.InitBuildingsAgain();
                SetLastBuilding();
            }

        }







        /*******************/
        /* CHACK ALL SYNCS */
        /*******************/
        private int _checks = 0;
        private int _checksBemImg = 0;
        private int _bemerkungen = 0;
        private int _bilder = 0;
        private int _packs = 0;
        private int _trans = 0;
        private int _dayovers = 0;
        private int _objectValues = 0;
        private int _objectValueBilds = 0;
        private int _pn = 0;
        private int _allCountFromUpload = 0;
        private bool _allCountFromUploadFalied = false;
        public int GetAllSyncFromUploadCount()
        {
            _checks = CheckClass.CountFromStack();
            _checksBemImg = CheckLeistungAntwortBemImg.CountFromStack();
            _bemerkungen = BemerkungWSO.CountFromStack();
            _bilder = BildWSO.CountFromStack();
            _packs = LeistungPackWSO.CountFromStack();
            _trans = AllTransSign.CountFromStack();
            _dayovers = DayOverWSO.CountFromStack();
            _objectValues = ObjektDataWSO.CountFromStack();
            _objectValueBilds = ObjektDatenBildWSO.CountFromStack();
            //_pn = PNWSO.CountFromStack();
            int allCountFromUpload = 0;
            allCountFromUpload += _checks;
            allCountFromUpload += _checksBemImg;
            allCountFromUpload += _bemerkungen;
            allCountFromUpload += _bilder;
            allCountFromUpload += _packs;
            allCountFromUpload += _trans;
            allCountFromUpload += _dayovers;
            allCountFromUpload += _objectValues;
            allCountFromUpload += _objectValueBilds;
            //allCountFromUpload += _pn;
            return allCountFromUpload;
        }

        private bool __isFirstInit = false;
        public async void CheckAllSyncFromUpload(bool isFirstInit = false)
        {
            //popupContainer_quest_countfromupload.IsVisible = false;
            __isFirstInit = isFirstInit;
            var pendingUploads = iPMCloud.Mobile.Services.UploadCoordinator.Instance.GetPendingUploadCount();
            if (pendingUploads <= 0 && !__isFirstInit)
            {
                ReloadPlanData(0);
                return;
            }

            if (pendingUploads > 0)
            {
                iPMCloud.Mobile.Services.UploadCoordinator.ProgressChanged -= OnUploadProgress;
                iPMCloud.Mobile.Services.UploadCoordinator.UploadCompleted -= OnUploadCompleted;
                iPMCloud.Mobile.Services.UploadCoordinator.ProgressChanged += OnUploadProgress;
                iPMCloud.Mobile.Services.UploadCoordinator.UploadCompleted += OnUploadCompleted;

                if (_uploadService == null)
                {
                    try
                    {
                        _uploadService = IPlatformApplication.Current?.Services
                            ?.GetService<iPMCloud.Mobile.Services.IUploadService>();
                    }
                    catch (Exception ex)
                    {
                        AppModel.Logger.Warn("IUploadService DI lookup: " + ex.Message);
                    }

                    if (_uploadService == null)
                    {
#if ANDROID
                        _uploadService = new iPMCloud.Mobile.Platforms.Android.AndroidUploadService();
#elif IOS
                    _uploadService = new iPMCloud.Mobile.Platforms.iOS.iOSUploadService();
#endif
                    }
                }

#if IOS
            DeviceDisplay.Current.KeepScreenOn = true;
#endif
                if (!await EnsureNotificationPermissionForForegroundWorkAsync())
                {
                    //overlay.IsVisible = false;
                    return;
                }

                _uploadService?.StartUploads();
            }
        }

        private async Task<bool> EnsureNotificationPermissionForForegroundWorkAsync()
        {
#if ANDROID
            if (DeviceInfo.Version.Major >= 13)
            {
                var status = await MainThread.InvokeOnMainThreadAsync(() =>
                    Permissions.CheckStatusAsync<Permissions.PostNotifications>());

                if (status != PermissionStatus.Granted)
                {
                    status = await MainThread.InvokeOnMainThreadAsync(() =>
                        Permissions.RequestAsync<Permissions.PostNotifications>());
                }

                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlertAsync(
                        "Berechtigungsproblem!",
                        "Für Synchronisation und Upload im Hintergrund wird die Benachrichtigungsberechtigung benötigt.",
                        "OK");
                    return false;
                }
            }
#endif
            return true;
        }

        private void OnUploadProgress(object sender, iPMCloud.Mobile.Services.UploadProgressEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    //overlay.IsVisible = true;
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Warn("OnUploadProgress UI: " + ex.Message);
                }
            });
        }

        private void OnUploadCompleted(object sender, iPMCloud.Mobile.Services.UploadCompletedEventArgs e)
        {
            iPMCloud.Mobile.Services.UploadCoordinator.ProgressChanged -= OnUploadProgress;
            iPMCloud.Mobile.Services.UploadCoordinator.UploadCompleted -= OnUploadCompleted;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    overlay.IsVisible = false;
                    int c = SetAllSyncState();

                    if (!e.Success && !string.IsNullOrWhiteSpace(e.ErrorMessage))
                    {
                        AppModel.Logger.Warn("Uploads fehlgeschlagen: " + e.ErrorMessage);
                    }
                    await Task.Delay(1);
                    CheckAllSyncFromUpload(__isFirstInit);
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Warn("OnUploadCompleted UI: " + ex.Message);
                }
                finally
                {
                    //__isFirstInit = false;
                    overlay.IsVisible = false;
#if IOS
                    DeviceDisplay.Current.KeepScreenOn = false;
#endif
                }
            });
        }




        /*******************/
        /* SYNC CHECKLIST (auch in Background)
        /*******************/
        private async void SyncChecks()
        {
            var checklist = CheckClass.LoadAllFromUploadStack();
            List<string> guidsList = new List<string>();
            checklist.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var ch = checklist.Find(b => b.guid == guid);
                    if (ch != null)
                    {
                        checklist.Remove(ch);
                        CheckClass.DeleteFromUploadStack(ch);
                    }
                });
            }

            if (resGuidsList != null && checklist.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Checkliste hochladen => SyncChecks");
                }
                foreach (var check in checklist)
                {
                    try
                    {
                        var result = await SyncChecks_Done(check);
                        if (result != null && result.success)
                        {
                            CheckClass.DeleteFromUploadStack(check);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppModel.Logger.Warn("ERROR: Checkliste hochladen FEHLGESCHLAGEN => SyncChecks : " + ex.Message);
                    }
                }

                SetAllSyncState();
                await Task.Delay(100);
                BuildChecksInfoList();
                await Task.Delay(100);

                if (_checksBemImg > 0)
                {
                    SyncChecksBemImg();
                    await Task.Delay(100);
                }
            }
            ;
        }
        private async Task<ChecksResponse> SyncChecks_Done(Check check)
        {
            ChecksResponse checkResponse = await Task.Run(() => { return AppModel.Instance.Connections.SetCheckANonePic(check); });
            if (checkResponse != null && checkResponse.success)
            {
                checkResponse.checkA.antworten.ForEach(ant =>
                {
                    if (ant.bem != null && ant.bem.imgs != null && ant.bem.imgs.Count > 0)
                    {
                        ant.bem.imgs.ForEach(bemImg =>
                        {
                            var clabis = CheckLeistungAntwortBemImg.LoadFromGuid(bemImg.guid);
                            clabis.ForEach(clabi =>
                            {
                                clabi.bem_id = bemImg.bem_id;
                                CheckLeistungAntwortBemImg.SaveToStack(clabi);
                                CheckLeistungAntwortBemImg.Delete(clabi);
                            });
                        });
                    }
                });
                await Task.Delay(1);
                return checkResponse;
            }
            if (checkResponse == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Checkliste hochladen FEHLGESCHLAGEN => SyncChecks_Done : CheckResponse == null ");
                return null;
            }
            if (checkResponse != null && !checkResponse.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Checkliste hochladen FEHLGESCHLAGEN => SyncChecks_Done : " + checkResponse.message);
                return null;
            }
            return checkResponse;
        }

        /*******************/
        /* SYNC Check BemImg
        /*******************/
        private async void SyncChecksBemImg()
        {
            var pics = CheckLeistungAntwortBemImg.LoadAllFromStack();
            List<string> guidsList = new List<string>();
            pics.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var p = pics.Find(b => b.guid == guid);
                    if (p != null)
                    {
                        CheckLeistungAntwortBemImg.DeleteFromStack(p);
                        pics.Remove(p);
                    }
                });
            }
            if (resGuidsList != null && pics.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Checkliste Bild(er)(Anzahl:" + pics.Count + ") => SyncChecksBemImg");
                }
                for (int i = 0; i < pics.Count; i++)
                {
                    var result = await SyncChecksBemImg_Done(pics[i]);
                    if (result != null && result.success)
                    {
                        CheckLeistungAntwortBemImg.DeleteFromStack(pics[i]);
                    }
                }
            }
        }
        private async Task<ChecksResponse> SyncChecksBemImg_Done(CheckLeistungAntwortBemImg pic)
        {
            ChecksResponse response = await Task.Run(() => { return AppModel.Instance.Connections.SetCheckABemImg(pic); });
            if (response == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Checkliste Bild(er) FEHLGESCHLAGEN => SyncChecksBemImg_Done : response == null ");
                return null;
            }
            if (response != null && !response.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Checkliste Bild(er) FEHLGESCHLAGEN => SyncChecksBemImg_Done : " + response.message);
                return null;
            }
            // Erfolgreich 
            //AppModel.Logger.Info("Info: Bild(er) erfolgreich hochgeladen => SyncChecksBemImg_Done");
            return response;
        }





        /*******************/
        /* SYNC ObjectValues (auch in Background)
        /*******************/
        private async void SyncObjectValues()
        {
            var objectValues = ObjektDataWSO.LoadAllFromUploadStack(AppModel.Instance);
            List<string> guidsList = new List<string>();
            objectValues.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var da = objectValues.Find(b => b.guid == guid);
                    if (da != null)
                    {
                        objectValues.Remove(da);
                        ObjektDataWSO.DeleteFromUploadStack(AppModel.Instance, da);
                    }
                });
            }

            if (resGuidsList != null && objectValues.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Zaehlerstaende hochladen => SyncObjectValues");
                }
                var result = await SyncObjectValues_Done(objectValues);
                if (result != null && result.success)
                {
                    objectValues.ForEach(d =>
                    {
                        ObjektDataWSO.DeleteFromUploadStack(AppModel.Instance, d);
                    });
                }
            }
            ;
        }
        private async Task<ObjectValuesResponse> SyncObjectValues_Done(List<ObjektDataWSO> objectValues)
        {
            ObjectValuesResponse objectValuesResponse = (await Task.Run(() => { return AppModel.Instance.Connections.ObjectValuesSync(objectValues); }));
            if (objectValuesResponse == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Zaehlerstaende FEHLGESCHLAGEN => SyncObjectValues_Done : objectValuesResponse == null");
                return null;
            }
            if (objectValuesResponse != null && !objectValuesResponse.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Zaehlerstaende FEHLGESCHLAGEN => SyncObjectValues_Done : " + objectValuesResponse.message);
                return null;
            }
            //AppModel.Logger.Info("Info: Zaehlerstaende erfolgreich hochgeladen => SyncObjectValues_Done");
            return objectValuesResponse;
        }



        /*******************/
        /* SYNC ObjectValueBild (auch in Background)
        /*******************/
        private async void SyncObjectValueBild()
        {
            var objectValueBilds = ObjektDatenBildWSO.LoadAllFromUploadStack(AppModel.Instance);
            List<string> guidsList = new List<string>();
            objectValueBilds.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var da = objectValueBilds.Find(b => b.guid == guid);
                    if (da != null)
                    {
                        objectValueBilds.Remove(da);
                        ObjektDatenBildWSO.DeleteFromUploadStack(AppModel.Instance, da);
                    }
                });
            }

            if (resGuidsList != null && objectValueBilds.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Zaehlerbilder hochladen => SyncObjectValues");
                }
                objectValueBilds.ForEach(async value =>
                {
                    var result = await SyncObjectValueBild_Done(value);
                    if (result != null && result.success)
                    {
                        ObjektDatenBildWSO.DeleteFromUploadStack(AppModel.Instance, value);
                    }
                });
            }
            ;
        }
        private async Task<ObjectValueBildResponse> SyncObjectValueBild_Done(ObjektDatenBildWSO value)
        {
            ObjectValueBildResponse response = (await Task.Run(() => { return AppModel.Instance.Connections.ObjectValueBildSync(value); }));
            if (response == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Zaehlerbilder FEHLGESCHLAGEN => SyncObjectValueBild_Done : response == null");
                return null;
            }
            if (response != null && !response.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Zaehlerbilder FEHLGESCHLAGEN => SyncObjectValueBild_Done : " + response.message);
                return null;
            }
            //AppModel.Logger.Info("Info: Zaehlerbilder erfolgreich hochgeladen => SyncObjectValueBild_Done");
            return response;
        }


        /*******************/
        /* SYNC PN (auch in Background)
        /*******************/
        private async void SyncPN()
        {
            var pn = PNWSO.LoadFromUploadStack();
            pn.personid = AppModel.Instance.Person.id;
            var resPN = await Task.Run(() => { return AppModel.Instance.Connections.PNSync(pn); });
            if (resPN.success)
            {
                PNWSO.DeleteFromUploadStack();
                AppModel.Instance.SettingModel.SettingDTO.PNToken = pn.token;
                AppModel.Instance.SettingModel.SaveSettings();
            }
            else
            {
                // FAILED
                _allCountFromUploadFalied = true;
            }
        }



        /*******************/
        /* SYNC DAYOVER (auch in Background)
        /*******************/
        public async void SyncDayOver()
        {
            var dayOvers = DayOverWSO.LoadAllFromUploadStack(AppModel.Instance);
            List<string> guidsList = new List<string>();
            dayOvers.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var da = dayOvers.Find(b => b.guid == guid);
                    if (da != null)
                    {
                        dayOvers.Remove(da);
                        DayOverWSO.DeleteFromUploadStack(AppModel.Instance, da);
                    }
                });
            }

            if (resGuidsList != null && dayOvers.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Feierabend hochladen => SyncDayOver");
                }
                var result = await SyncDayOver_Done(dayOvers);
                if (result != null && result.success)
                {
                    dayOvers.ForEach(d =>
                    {
                        DayOverWSO.DeleteFromUploadStack(AppModel.Instance, d);
                    });
                }
            }
            ;
        }
        private async Task<DayOverResponse> SyncDayOver_Done(List<DayOverWSO> dayOvers)
        {
            DayOverResponse dayOverResponse = (await Task.Run(() => { return AppModel.Instance.Connections.DayOverSync(dayOvers); }));
            if (dayOverResponse == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Feierabend FEHLGESCHLAGEN => SyncDayOver_Done : dayOverResponse == null ");
                return null;
            }
            if (dayOverResponse != null && !dayOverResponse.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Feierabend FEHLGESCHLAGEN => SyncDayOver_Done : " + dayOverResponse.message);
                return null;
            }
            //AppModel.Logger.Info("Info: Feierabend erfolgreich hochgeladen => SyncDayOver_Done");
            return dayOverResponse;
        }





        /*******************/
        /* SYNC TransSigns (auch in Background)
        /*******************/
        private async void SyncTransSigns()
        {
            var transSigns = AllTransSign.LoadAllFromUploadStack();
            List<string> guidsList = new List<string>();
            transSigns.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var da = transSigns.Find(b => b.guid == guid);
                    if (da != null)
                    {
                        transSigns.Remove(da);
                        AllTransSign.DeleteFromUploadStack(da);
                    }
                });
            }

            if (resGuidsList != null && transSigns.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Feierabend hochladen => SyncDayOver");
                }
                transSigns.ForEach(async transS =>
                {
                    var result = await SyncTransSigns_Done(transS);
                    if (result != null && result.success)
                    {
                        transSigns.ForEach(d =>
                        {
                            AllTransSign.DeleteFromUploadStack(d);
                        });
                    }
                });
            }
            ;
        }
        private async Task<AllTransSignResponse> SyncTransSigns_Done(AllTransSignRequest transSign)
        {
            AllTransSignResponse res = (await Task.Run(() => { return AppModel.Instance.Connections.AllTransSignSync(transSign); }));
            if (res == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): AllTransSignSync FEHLGESCHLAGEN => SyncTransSigns_Done : response == null ");
                return null;
            }
            if (res != null && !res.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): AllTransSignSync FEHLGESCHLAGEN => SyncTransSigns_Done : " + res.message);
                return null;
            }
            return res;
        }



        /*******************/
        /* SYNC NOTICE (auch in Background)
        /*******************/
        private async void SyncSingleNotice()
        {
            var bemerkungen = BemerkungWSO.LoadAllFromUploadStack(AppModel.Instance);
            List<string> guidsList = new List<string>();
            bemerkungen.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var bem = bemerkungen.Find(b => b.guid == guid);
                    if (bem != null)
                    {
                        bemerkungen.Remove(bem);
                        BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, bem);
                    }
                });
            }
            if (resGuidsList != null && bemerkungen.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Bemerkung => SyncNotice");
                }
                for (int i = 0; i < bemerkungen.Count; i++)
                {
                    //bemerkungen.ForEach(async bem =>
                    //{
                    if (!String.IsNullOrWhiteSpace(bemerkungen[i].text.Trim()) || (bemerkungen[i].photos != null && bemerkungen[i].photos.Count > 0))
                    {
                        var resultBemId = await SyncSingleNotice_Done(bemerkungen[i]);
                        if (resultBemId > 0)
                        {
                            bemerkungen[i].hasSend = true;
                            var pics = BildWSO.LoadFromGuid(AppModel.Instance, bemerkungen[i].guid);
                            pics.ForEach(p =>
                            {
                                p.bemId = resultBemId;
                                if (bemerkungen[i].prio < 2) //wenn keine Störmeldung dann seperat hochladen
                                {
                                    BildWSO.SaveToStack(AppModel.Instance, p);
                                }
                                BildWSO.Delete(AppModel.Instance, p);
                            });
                            await Task.Delay(1);
                            // Bilder abgelegt unter BemId - dann bemerkung löschen, weil erfolgreich hochgeladen
                            // Bilder im nächsten stepp hochladen 
                            BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, bemerkungen[i]);
                        }
                    }
                    else
                    {
                        bemerkungen[i].hasSend = true;
                        BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, bemerkungen[i]);
                    }
                    //});
                }
                //var _bilderStack = BildWSO.CountFromStack();

                SetAllSyncState();
                await Task.Delay(100);

                if (_bilder > 0)
                {
                    SyncNoticeBild();
                    await Task.Delay(100);
                }
            }
        }
        private async Task<Int32> SyncSingleNotice_Done(BemerkungWSO bem)
        {
            SingleNoticeResponse noticeResponse = (await Task.Run(() => { return AppModel.Instance.Connections.SingleNoticeSync(bem); }));
            if (noticeResponse == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Bemerkungen FEHLGESCHLAGEN => SyncNotice_Done : noticeResponse == null ");
                return -1;
            }
            if (noticeResponse != null && !noticeResponse.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Bemerkungen FEHLGESCHLAGEN => SyncNotice_Done : " + noticeResponse.message);
                return -1;
            }
            // Erfolgreich 
            //AppModel.Logger.Info("Info: Bemerkungen erfolgreich hochgeladen => SyncNotice_Done");
            return noticeResponse.bemid;
        }


        /*******************/
        /* SYNC NOTICE (auch in Background)
        /*******************/
        private async void SyncNoticeBild()
        {
            var pics = BildWSO.LoadAllFromStack();
            List<string> guidsList = new List<string>();
            pics.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var p = pics.Find(b => b.guid == guid);
                    if (p != null)
                    {
                        BildWSO.DeleteFromStack(p);
                        pics.Remove(p);
                    }
                });
            }
            if (resGuidsList != null && pics.Count > 0)
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("Warn: Internet/Online -OFF- ... Bild(er)(Anzahl:" + pics.Count + ") => SyncNoticeBild");
                }
                for (int i = 0; i < pics.Count; i++)
                {
                    var result = await SyncNoticeBild_Done(pics[i]);
                    if (result != null && result.success)
                    {
                        BildWSO.DeleteFromStack(pics[i]);
                    }
                }
            }
        }
        private async Task<NoticeBildResponse> SyncNoticeBild_Done(BildWSO pic)
        {
            NoticeBildResponse response = await Task.Run(() => { return AppModel.Instance.Connections.NoticeBildSync(pic); });
            if (response == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Bild(er) FEHLGESCHLAGEN => SyncNoticeBild_Done : response == null ");
                return null;
            }
            if (response != null && !response.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Bild(er) FEHLGESCHLAGEN => SyncNoticeBild_Done : " + response.message);
                return null;
            }
            // Erfolgreich 
            //AppModel.Logger.Info("Info: Bild(er) erfolgreich hochgeladen => SyncNoticeBild_Done");
            return response;
        }






        /*******************/
        /* SYNC POSITION  (auch in Background)
        /*******************/
        private async void SyncPosition(bool preview = false)
        {
            string[] resGuidsList = null;

            List<LeistungPackWSO> packs = null;
            if (preview)
            {
                packs = new List<LeistungPackWSO> { AppModel.Instance.allPositionInWork };
            }
            else
            {
                packs = LeistungPackWSO.LoadAllFromUploadStack(AppModel.Instance);
                packs.ForEach(lp =>
                {
                    if (lp.leistungen != null && lp.leistungen.Count > 0)
                    {
                        lp.leistungen.ForEach(l =>
                        {
                            if (l.bemerkungen != null && l.bemerkungen.Count > 0)
                            {
                                l.bemerkungen = l.bemerkungen.Where(b => !String.IsNullOrWhiteSpace(b.text.Trim()) || (b.photos != null && b.photos.Count > 0)).ToList();
                            }
                            if (l.bemerkungen != null && l.bemerkungen.Count == 0)
                            {
                                l.bemerkungen = null;
                            }

                        });
                    }
                });
            }
            if (!preview)
            {
                List<string> guidsList = new List<string>();
                packs.ForEach(v => { guidsList.Add(v.guid); });
                resGuidsList = await Task.Run(() => { return AppModel.Instance.Connections.GuidsCheck(guidsList.ToArray()); });
                if (resGuidsList != null && resGuidsList.Length > 0)
                {
                    resGuidsList.ToList().ForEach(guid =>
                    {
                        var pa = packs.Find(b => b.guid == guid);
                        if (pa != null)
                        {
                            packs.Remove(pa);
                            LeistungPackWSO.DeleteFromUploadStack(AppModel.Instance, pa);
                        }
                    });
                }
            }
            if ((preview && packs.Count > 0) || (resGuidsList != null && packs.Count > 0))
            {
                if (!AppModel.Instance.IsInternet)
                {
                    AppModel.Logger.Warn("WARN: Internet/Online -OFF- ... " + (preview ? "VORSCHAU-" : "") + "Leistungspakete(" + packs.Count + ") => SyncPosition");
                }
                for (int i = 0; i < packs.Count; i++)
                {
                    var result = await SyncPosition_Done(packs[i]);
                    if (result != null)
                    {
                        if (!preview)
                        {
                            if (result.leistungen != null && result.leistungen.Count > 0)
                            {
                                result.leistungen.ForEach(l =>
                                {
                                    if (l.bemerkungen != null && l.bemerkungen.Count > 0)
                                    {
                                        l.bemerkungen.ForEach(b =>
                                        {
                                            if (b.id > 0)
                                            {
                                                b.hasSend = true;
                                                var pics = BildWSO.LoadFromGuid(AppModel.Instance, b.guid);
                                                pics.ForEach(p =>
                                                {
                                                    p.bemId = b.id;
                                                    if (b.prio < 2)
                                                    {
                                                        BildWSO.SaveToStack(AppModel.Instance, p);
                                                    }
                                                    BildWSO.Delete(AppModel.Instance, p);
                                                });
                                            }
                                        });
                                    }
                                });
                            }
                            // workat von result aktuell setzten
                            var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(result.endticks), -2);
                            BuildingWSO building = null;
                            if (AppModel.Instance.LastBuilding == null && result.leistungen != null && result.leistungen.Count > 0)
                            {
                                building = BuildingWSO.LoadBuilding(AppModel.Instance, result.leistungen[0].objektid);
                            }
                            if (AppModel.Instance.LastBuilding != null)
                            {
                                building = AppModel.Instance.LastBuilding;
                            }
                            if (building != null && result.leistungen != null && result.leistungen.Count > 0)
                            {
                                building.ArrayOfAuftrag.ForEach(o =>
                                {
                                    o.kategorien.ForEach(c =>
                                    {
                                        c.leistungen.ForEach(p =>
                                        {
                                            var foundPos = result.leistungen.Find(lei => lei.id == p.id);
                                            if (foundPos != null)
                                            {
                                                if (double.Parse(p.lastwork) > 0 && p.timevaldays > 0)
                                                {
                                                    if (String.IsNullOrWhiteSpace(foundPos.workat) || foundPos.workat == "0")
                                                    {
                                                        foundPos.workat = "" + (double.Parse(p.lastwork) + (double.Parse("" + p.timevaldays) * 24 * 60 * 60 * 1000));
                                                    }
                                                    p.workat = foundPos.workat;
                                                }
                                            }
                                        });
                                    });
                                });
                                BuildingWSO.Save(AppModel.Instance, building);
                            }
                            LeistungPackWSO.DeleteFromUploadStack(AppModel.Instance, packs[i]);
                        }
                    }

                }

                SetAllSyncState();
                await Task.Delay(100);

                if (_bilder > 0)
                {
                    SyncNoticeBild();
                    await Task.Delay(1);
                }

            }
        }
        private async Task<LeistungPackWSO> SyncPosition_Done(LeistungPackWSO pack)
        {
            PositionResponse positionResponse = await Task.Run(() => { return AppModel.Instance.Connections.PositionSync(pack); });
            if (positionResponse == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Leistungspakete FEHLGESCHLAGEN => SyncPosition_Done : positionResponse == null");
                return null;
            }
            else if (positionResponse != null && !positionResponse.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Leistungspakete FEHLGESCHLAGEN => SyncPosition_Done : " + positionResponse.message);
                return null;
            }
            else
            {
                // Erfolgreich 
                //AppModel.Logger.Info("Info: Leistungspakete erfolgreich hochgeladen => SyncPosition_Done");

                if (AppModel.Instance.AppControll.showObjektPlans)
                {
                    if (positionResponse.planweek != null && AppModel.Instance.PlanResponse.selectedPerson == null)
                    {
                        AppModel.Instance.PlanResponse.planweek = positionResponse.planweek;
                        ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                        Update_PlanTabs((int)DateTime.Now.DayOfWeek);

                    }
                }
                return positionResponse.pack;
            }
        }

        /******************************/
        /* SYNC POSITION AGAIN AS Preview
        /**********************/
        private async void SyncPositionAgain()
        {
            var packs = new List<LeistungPackWSO> { AppModel.Instance.allPositionInWork };
            if (packs.Count > 0)
            {
                packs.ForEach(async pack =>
                {
                    var result = await SyncPositionAgain_Done(pack);
                    AppModel.Instance.allPositionInWork.leistungen.ForEach(l => { l.again = 0; });
                });
            }
        }
        private async Task<LeistungPackWSO> SyncPositionAgain_Done(LeistungPackWSO pack)
        {
            PositionResponse positionResponse = await Task.Run(() => { return AppModel.Instance.Connections.PositionAgainSync(pack); });
            if (positionResponse == null)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Leistungspakete(Nachbuchen) FEHLGESCHLAGEN => SyncPositionAgain_Done : positionResponse == null");
                return null;
            }
            if (positionResponse != null && !positionResponse.success)
            {
                // FAILED
                _allCountFromUploadFalied = true;
                AppModel.Logger.Warn("WARN:  (0): Leistungspakete(Nachbuchen) FEHLGESCHLAGEN => SyncPositionAgain_Done : " + positionResponse.message);
                return null;
            }
            // Erfolgreich 
            //AppModel.Logger.Info("Info: Leistungspakete(Nachbuchen) erfolgreich hochgeladen => SyncPositionAgain_Done");
            return positionResponse.pack;
        }



        public async void SetAppControll()
        {
            if (AppModel.Instance.AppControll != null)
            {
                frame_PersonTimes.IsVisible = AppModel.Instance.AppControll.showPersonTimes;

                // beide NICHT zeigen (Plans und Ticktes)
                if (!AppModel.Instance.AppControll.showObjektPlans && !AppModel.Instance.AppControll.showTickets && !AppModel.Instance.AppControll.showChecks)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = false;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = false;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = true; // Space wenn beide nichts gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = false;
                }
                // Ticket NICHT zeigen (Plans und Ticktes)
                if (AppModel.Instance.AppControll.showObjektPlans && !AppModel.Instance.AppControll.showTickets)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = false; // Space wenn beide nichts gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = true;

                    frame_plantabA.IsVisible = true;
                    frame_plantabB.IsVisible = true;
                    frame_plantabC.IsVisible = false;
                    frame_plantabCe.IsVisible = AppModel.Instance.AppControll.showChecks;
                    frame_planConA.IsVisible = true;
                    frame_planConB.IsVisible = false;
                    frame_planConCe.IsVisible = false;
                    frame_planConC.IsVisible = false;
                }
                // Plan NICHT zeigen (nur Ticktes)
                if (!AppModel.Instance.AppControll.showObjektPlans && AppModel.Instance.AppControll.showTickets)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = false; // Space wenn beide nichts gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = true;

                    frame_plantabA.IsVisible = false;
                    frame_plantabB.IsVisible = false;
                    frame_plantabCe.IsVisible = AppModel.Instance.AppControll.showChecks;
                    frame_plantabC.IsVisible = true;
                    frame_planConA.IsVisible = false;
                    frame_planConB.IsVisible = false;
                    frame_planConCe.IsVisible = false;
                    frame_planConC.IsVisible = true;
                    frame_plantabC.Margin = new Thickness(0, -8, 2, 0);// Tab hochstellen
                }
                // beide zeigen (Plans und Ticktes)
                if (AppModel.Instance.AppControll.showObjektPlans && AppModel.Instance.AppControll.showTickets)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = false; // Space wenn beide nicht gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = true;

                    frame_plantabA.IsVisible = true;
                    frame_plantabB.IsVisible = true;
                    frame_plantabCe.IsVisible = AppModel.Instance.AppControll.showChecks;
                    frame_plantabC.IsVisible = true;

                    frame_planConA.IsVisible = true;
                    frame_planConB.IsVisible = false;
                    frame_planConCe.IsVisible = false;
                    frame_planConC.IsVisible = false;
                }
            }
        }

        public async void HidePlaningView()
        {
            ObjektPlanWeekMobil_Stack_A.IsVisible = false;
            ObjektPlanWeekMobil_Stack_B.IsVisible = false;
            ObjektPlanWeekMobil_Stack_C.IsVisible = true;
            ObjektPlanWeekMobil_Stack_ABC.IsVisible = false;

            frame_plantabA.IsVisible = false;
            frame_plantabB.IsVisible = false;
            frame_plantabCe.IsVisible = false;
            frame_plantabC.IsVisible = false;

            frame_planConA.IsVisible = false;
            frame_planConB.IsVisible = false;
            frame_planConCe.IsVisible = false;
            frame_planConC.IsVisible = false;
        }

        public Page GetPage(string subPage = "")
        {
            return this;
        }

        public void ShowDisconnected()
        {
            img_onlinestate.Source = AppModel.Instance.IsInternet ? "isonline_b.png" : "isoffline_b.png";
            string statetext = "";
            int l = AppModel.Instance.connectionProfiles.Count;
            AppModel.Instance.connectionProfiles.ForEach(profile =>
            {
                var s = profile;
                if (s.ToLower() == "cellular") { s = "G|LTE"; }
                if (s.ToLower() == "desktop") { s = "Ethernet"; }
                if (!statetext.Contains(s))
                {
                    statetext = statetext + (statetext.Length > 0 ? "/" : "") + s;
                }
            });
            lb_onlinestate.Text = statetext;
        }
        public void ShowDisGPS()
        {
            if (!AppModel.Instance.gpsAlertHasSend)
            {
                img_gpsstate.Source = !AppModel.Instance.gpsPermissionReady ? "gpsoff.png" : "gpson.png";
            }
            else
            {
                img_gpsstate.Source = "gpsoff2_img.png";
            }
            //string vor = "--:--";
            //if (AppModel.Instance.lastServerPing > 0)
            //{
            //    var ts = DateTime.Now - (new DateTime(AppModel.Instance.lastServerPing));
            //    vor = new DateTime(AppModel.Instance.lastServerPing).ToString("HH:mm:ss");
            //}
            //lb_pingstate.Text = "ServerPing:" + vor;
        }






        public async void ShowAlertMessage(string titel, string message, bool enableBtn = false)
        {
            await DisplayAlertAsync(titel, message, "OK");
            //if (popupContainer_Alert.IsVisible) { return; }
            //popupContainer_Alert_Titel.Text = titel;
            //popupContainer_Alert_Text.Text = message;
            //popupContainer_Alert.IsVisible = true;
            //popupContainer_Alert_btn.IsVisible = enableBtn;
        }
        //public void HideAlertMessage(object sender, EventArgs e)
        //{
        //    popupContainer_Alert.IsVisible = false;
        //    popupContainer_Alert_Titel.Text = "";
        //    popupContainer_Alert_Text.Text = "";
        //    popupContainer_Alert_btn.IsVisible = true;
        //}
        private void OnOverlayTapped(object sender, EventArgs e)
        {
            // Implementierung hier - z.B. das Overlay ausblenden
            //if (popupContainer_infodialog != null)
            //{
            //    popupContainer_infodialog.IsVisible = false;
            //}
        }

        private static string GetMuellInOutXImageName(int viewOnlyMuell) => viewOnlyMuell switch
        {
            0 => "muell_in_out_x0_img.png",
            1 => "muell_in_out_x1_img.png",
            2 => "muell_in_out_x2_img.png",
            _ => "muell_in_out_x.png"
        };


    }


    public class GestTappedBuildingTreeItemObject
    {
        public BuildingWSO building = null;
        public StackLayout stacklayout = null;
        public object sfButton = null;
        public int index = 0;
    }






}
