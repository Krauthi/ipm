using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Cloud.Translation.V2;
using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.vo.GlobalObjects;
using iPMCloud.Mobile.vo.wso;
// TODO: NativeMedia not MAUI-compatible - needs replacement with Microsoft.Maui.Media
// using NativeMedia;
//using Plugin.Permissions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
// TODO: Xamarin.RangeSlider not MAUI-compatible - needs replacement
// using Xamarin.RangeSlider.Forms;

//using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
//https://docs.microsoft.com/de-de/xamarin/essentials/preferences?tabs=android

namespace iPMCloud.Mobile
{
    public partial class MainPage : ContentPage
    {
        // private BackgroundWorker backgroundWorker = new BackgroundWorker();

        public AppModel model;
        public bool isInitialize = false;
        public bool _isShowing = false;


        public MainPage()
        {
            InitializeComponent();
        }
        public MainPage(AppModel model)
        {
            isInitialize = true;
            this.model = model;
            InitializeComponent();
            AppModel.Instance.anImage = backgroundIMG;

            AppModel.Instance.MainPageOverlay = overlay;

            model._showall_again_OrderCategory_frame = btn_back_inBuildingOrder_category_showall_again;
            model._showall_OrderCategory_frame = btn_back_inBuildingOrder_category_showall;

            AppModel.Instance.Lang = Lang.Load();
            lb_settings_sel_trans.Text = AppModel.Instance.Lang.text.Replace("(Standard)", "");

            ShowDisconnected();

            var checkPerm = CheckPermissions().Result;
            if (checkPerm)
            {
                CheckAllSyncFromUpload();

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


                model.allPositionInWork = LeistungPackWSO.Load(model);
                ShowMainPage();
            }
            else
            {
                DisplayAlert("Fehlende Berechtigungen!", "Bitte beenden Sie die App und starten diese neu!\n\nAktivieren Sie nach dem neustart die benötigten Berechtigungen!", "OK");
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

                var result = Task.Run(() => { return model.Connections.GetChecksInfo(AppModel.Instance.Person.id, checkInfoLastView); }).Result;
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

            frame_planListCeoffen.Children.Add(Check.GetOffeneList(
                checks.Where(_ => _.lastStateOfCheck_a == "Offen").ToList(), frame_planListCeoffen.Width,
                new Command<IntBoolParam>(StartOrOpenCheckA)
                ));
            frame_planListCefaellig.Children.Add(Check.GetOffeneList(
                checks.Where(_ => _.lastStateOfCheck_a != "Offen" && _.naeststeFaelligkeitDate < 8 && _.berechnunginterval > 0).ToList(), frame_planListCeoffen.Width,
                new Command<IntBoolParam>(StartOrOpenCheckA)
                ));
            if (checkInfoLastView == 99)
            {
                frame_planListCeerl.Children.Add(Check.GetOffeneList(
                    checks.Where(_ => (_.lastStateOfCheck_a != "Offen" && _.naeststeFaelligkeitDate >= 8) || (_.lastStateOfCheck_a != "Offen" && _.berechnunginterval == 0)).ToList(), frame_planListCeoffen.Width,
                    new Command<IntBoolParam>(StartOrOpenCheckA)
                    ));
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
            if (model.allPositionInWork != null)
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
                model.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                model.SetAllObjectAndValuesToNoSelectedBuilding();
                model.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                model.LastBuilding = model.AllBuildings.Find(bu => bu.id == intBol.val);
                model.SettingModel.SaveSettings();
                list_notscan.Children.Clear();
                if (intBol.bol) { ShowMainPage(); }
                else
                {
                    await lastBuilding_Container.FadeTo(0, 500, Easing.SpringOut);
                    SetLastBuilding();
                    await lastBuilding_Container.FadeTo(1, 500, Easing.SpringIn);
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
            await Task.Delay(1);
            overlay.IsVisible = true;


            foreach (var check in AppModel.Instance.ChecksInfoResponse.checks)
            {
                if (check.id == intBol.val && AppModel.Instance.selectedCheckInfo == null)
                {
                    AppModel.Instance.selectedCheckInfo = check;
                }
            }


            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                if (model.allPositionInWork != null)
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
                    model.SettingModel.SettingDTO.LastBuildingIdScanned = AppModel.Instance.selectedCheckInfo.objektid;
                    // Zurücksetzten aller States für die Auswahl der Ausführungen
                    model.SetAllObjectAndValuesToNoSelectedBuilding();
                    model.SettingModel.SettingDTO.LastBuildingIdScanned = AppModel.Instance.selectedCheckInfo.objektid;
                    model.LastBuilding = model.AllBuildings.Find(bu => bu.id == AppModel.Instance.selectedCheckInfo.objektid);
                    try
                    {
                        AppModel.Logger.Info("CHECK-IN (OHNE QR-SCAN): " + model.LastBuilding.strasse + " " + model.LastBuilding.hsnr + model.LastBuilding.plz + " " + model.LastBuilding.ort);
                    }
                    catch (Exception) { }
                    model.SettingModel.SaveSettings();

                    await lastBuilding_Container.FadeTo(0, 500, Easing.SpringOut);
                    SetLastBuilding();
                    await lastBuilding_Container.FadeTo(1, 500, Easing.SpringIn);
                    overlay.IsVisible = false;
                    await Task.Delay(1);
                }
                StartOrOpenCheckA_next(intBol);
            }
            else
            {
                // OBJEKT SCANEN !!!
                if (AppModel.Instance.LastBuilding != null
                    && AppModel.Instance.LastBuilding.id == AppModel.Instance.selectedCheckInfo.objektid)
                {
                    StartOrOpenCheckA_next(intBol);
                }
                else
                {
                    ShowBuildingScanPage_Check(intBol);
                }
            }
        }
        IntBoolParam intBol_Check = null;
        private async void ShowBuildingScanPage_Check(IntBoolParam intBol)
        {
            intBol_Check = intBol;
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            BuildingScanPage_Container.IsVisible = true;

            lay_buildingscan.Children.Clear();
            model.UseExternHardware = true;
            model.Scan.ScanBuildingView(this, lay_buildingscan, MethodAfterScan_check);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public bool MethodAfterScan_check()
        {
            lay_buildingscan.Children.Clear();
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
                // Es wird noch eine bearbeitet "Offen"
                popupContainer_dialog_titel.Text = "CHECKLISTE";
                popupContainer_dialog_text.Text = "Diese Checkliste kann nicht gestartet werden, da Sie aktuell noch eine andere Checkliste in bearbeitung haben und zuerst fertig stellen müssen.";
                popupContainer_dialog.IsVisible = true;

                popupContainer_dialog_btn_ok.GestureRecognizers.Clear();
                var tgr_popupContainer_dialog_btn_ok = new TapGestureRecognizer();
                tgr_popupContainer_dialog_btn_ok.Tapped += (object o, TappedEventArgs ev) =>
                {
                    popupContainer_dialog.IsVisible = false;
                    popupContainer_dialog_btn_ok.GestureRecognizers.Clear();
                };
                popupContainer_dialog_btn_ok.GestureRecognizers.Add(tgr_popupContainer_dialog_btn_ok);

                overlay.IsVisible = false;
                await Task.Delay(1);
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
                var result = await Task.Run(() => { return model.Connections.StartCheck(intBol.val); });
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
                        await DisplayAlert("Sie sind nicht Online!",
                            "Für das Starten einer Checkliste müssen Sie zum herunterladen der Checklistendaten Online sein!",
                            "OK");
                    }
                    else
                    {
                        overlay.IsVisible = false;
                        await Task.Delay(1);
                        await DisplayAlert("Fehler bei Checkliste starten!",
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
                    var result = await Task.Run(() => { return model.Connections.GetCheckA(AppModel.Instance.selectedCheckInfo.checkA_id).Result; });
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
                            await DisplayAlert("Sie sind nicht Online!",
                                "Für das Starten einer Checkliste müssen Sie zum herunterladen der Checklistendaten Online sein!",
                                "OK");
                        }
                        else
                        {
                            overlay.IsVisible = false;
                            await Task.Delay(1);
                            await DisplayAlert("Fehler bei Checkliste starten!",
                                "Es konnten keine Checklistendaten heruntergeladen werden! Bitte gehen Sie unter Einstellungen und senden Sie uns die Mobile LOG. Wir werden versuchen das Problem zu analysieren.",
                                "OK");
                        }
                        return;
                    }

                }
            }

            var w = App.Current.MainPage.Width;//-13
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
                frame_info_check_badge.Children.Add(Check.GetBadgeFrame(AppModel.Instance.selectedCheckInfo.naeststeFaelligkeitDate));

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

            var result = await Task.Run(() => { return model.Connections.DelCheckA(AppModel.Instance.selectedCheckA.id).Result; });
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
                    HorizontalOptions = LayoutOptions.FillAndExpand
                });

            UpdateCheckAState();
        }

        public void UpdateCheckAState()
        {
            checkQuestStack_scroll.HeightRequest = App.Current.MainPage.Height - checkQuestStack_scroll.Y - img_info_check_typ_container.Height - 6;

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

            // Befragung speichern und pausieren
            AppModel.Instance.selectedCheckA.antworten.ForEach(_ => _.ClearGui());
            CheckClass.SaveCheckA(AppModel.Instance.selectedCheckA);
            checkQuestStack.Children.Clear();
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
                        };
                    }
                }
            };

            if (CheckClass.ToUploadStack(AppModel.Instance.selectedCheckA))
            {
                CheckClass.DeleteCheckA(AppModel.Instance.selectedCheckA.id);
                CheckAllSyncFromUpload();
            }

            checkQuestStack.Children.Clear();
            CheckPage_Container.IsVisible = false;

            // //GetChecksInfo(checkInfoLastView, true);
            overlay.IsVisible = false;
            await Task.Delay(1);
        }



        public void OpenCheckA_Singature(CheckLeistungAntwort quest)
        {
            var w = App.Current.MainPage.Width;//-13
            CheckPage_Signature_Container.WidthRequest = w;
            checkQuestStack_signature_scroll.HeightRequest =
                App.Current.MainPage.Height - checkQuestStack_signature_scroll.Y - 13;
            checkQuestStack_signature.Children.Clear();
            quest.signPad = Check.GetSignElement();
            checkQuestStack_signature.Children.Add(Check.GetQuestMain_7_PopUp(quest));
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
            var w = App.Current.MainPage.Width;
            var h = App.Current.MainPage.Height;
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
                    ph.stack = BildWSO.GetAttachmentForNoticeElement(
                            ImageSource.FromStream(() => new MemoryStream(ph.bytes)),
                             new DateTime(long.Parse(ph.name)).ToString("dd.MM.yyyy-HH:mm:ss"),
                             null);

                    var frame = (Frame)((StackLayout)(ph.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork_check_bem), CommandParameter = ph });
                    noticePhotoStack_check_bem.Children.Add(ph.stack);
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
                _SelectedPosForNotice_check_bem.bemWSO.personid = model.Person.id;
                _SelectedPosForNotice_check_bem.bemWSO.objektid = AppModel.Instance.selectedCheckA.objektid;
                _SelectedPosForNotice_check_bem.bemWSO.leistungid = _SelectedPosForNotice_check_bem.id;
                _SelectedPosForNotice_check_bem.bemWSO.datum = JavaScriptDateConverter.Convert(DateTime.Now);
                _SelectedPosForNotice_check_bem.bemWSO.text = !_SelectedPosForNotice_check_bem_isquest ? entry_notice_check_bem.Text : "";
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

        public async void btn_takePhoto_check_bem(object sender, EventArgs e)
        {
            // notizSave_stack_check_bem.IsVisible = false;
            await Task.Delay(1);

            try
            {
                model.UseExternHardware = true;
                var photo = await MediaGallery.CapturePhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    var photoResponse = PhotoUtils.GetImages(stream);
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, AppModel.Instance.LastBuilding);

                    long bildName = DateTime.Now.Ticks;
                    var b = new BildWSO(_SelectedPosForNotice_check_bem.bemWSO.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = "" + bildName,
                        stack = BildWSO.GetAttachmentForNoticeElement(
                            photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                            new Command<BildWSO>(RemoveBildInWork_check_bem))
                    };
                    var frame = (Frame)((StackLayout)(b.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork_check_bem), CommandParameter = b });

                    //BildWSO.Save(AppModel.Instance, b);
                    _SelectedPosForNotice_check_bem.bemWSO.photos.Add(b);
                    if (_SelectedPosForNotice_check_bem.bemWSO != null && _SelectedPosForNotice_check_bem.bemWSO.photos != null)
                    {
                        btn_takePhoto_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
                        btn_takePhotoAttachment_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
                    }
                    noticePhotoStack_check_bem.Children.Add(b.stack);

                    //CheckNoticeFalid_check_bem();

                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    model.UseExternHardware = false;
                    stream.Dispose();
                }
            }
            catch (OperationCanceledException ex)
            {
                // handling a cancellation request
            }
            catch (Exception ex)
            {
                // handling other exceptions
            }
            finally
            {
                model.UseExternHardware = false;
                overlay.IsVisible = false;
                await Task.Delay(1);
            }

        }
        public async void btn_pickPhotos_check_bem(object sender, EventArgs e)
        {
            //notizSave_stack_check_bem.IsVisible = false;
            await Task.Delay(1);

            try
            {
                var cts = new CancellationTokenSource();
                IMediaFile[] files = null;

                model.UseExternHardware = true;

                var request = new MediaPickRequest(3 -
                    ((_SelectedPosForNotice_check_bem.bemWSO != null && _SelectedPosForNotice_check_bem.bemWSO.photos != null)
                    ? _SelectedPosForNotice_check_bem.bemWSO.photos.Count : 0), MediaFileType.Image)
                {
                    //PresentationSourceBounds = System.Drawing.Rectangle.Empty,
                    UseCreateChooser = false,
                    Title = "Select"
                };

                cts.CancelAfter(TimeSpan.FromMinutes(5));


                var results = await MediaGallery.PickAsync(request, cts.Token);
                files = results?.Files?.ToArray();

                if (files == null)
                {
                    overlay.IsVisible = false;
                    await Task.Delay(1);
                    return;
                }

                overlay.IsVisible = true;
                await Task.Delay(1);
                foreach (var fil in files)
                {
                    var stream = await fil.OpenReadAsync();

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    var photoResponse = PhotoUtils.GetImages(stream);
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, AppModel.Instance.LastBuilding);

                    long bildName = DateTime.Now.Ticks;
                    var b = new BildWSO(_SelectedPosForNotice_check_bem.bemWSO.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = "" + bildName,
                        stack = BildWSO.GetAttachmentForNoticeElement(photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss")
                            , new Command<BildWSO>(RemoveBildInWork_check_bem))
                    };
                    var frame = (Frame)((StackLayout)(b.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork_check_bem), CommandParameter = b });

                    //BildWSO.Save(AppModel.Instance, b);
                    _SelectedPosForNotice_check_bem.bemWSO.photos.Add(b);
                    noticePhotoStack_check_bem.Children.Add(b.stack);
                }
            }
            catch (Exception ex)
            {
                var ae = ex;
            }
            finally
            {
                model.UseExternHardware = false;
            }

            if (_SelectedPosForNotice_check_bem.bemWSO != null && _SelectedPosForNotice_check_bem.bemWSO.photos != null)
            {
                btn_takePhoto_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
                btn_takePhotoAttachment_frame_check_bem.IsVisible = (_SelectedPosForNotice_check_bem.bemWSO.photos.Count < 3);
            }


            //CheckNoticeFalid_check_bem();

            await Task.Delay(1);
            overlay.IsVisible = false;


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



        private void entry_notice_TextChanged_check_bem(object sender, TextChangedEventArgs e)
        {
            //_SelectedPosForNotice_check_bem.bem.text = entry_notice_check_bem.Text;
            //CheckNoticeFalid_check_bem();
        }









        protected override void OnDisappearing()
        {
            if (AppModel.Instance._cts != null && !AppModel.Instance._cts.IsCancellationRequested)
                AppModel.Instance._cts.Cancel();
            base.OnDisappearing();
        }

        private async Task<bool> CheckPermissions()
        {
            model.CheckPermissions();
            if (!String.IsNullOrWhiteSpace(model.checkPermissionsMessage))
            {
                model.checkPermissionsMessage = model.checkPermissionsMessage.Replace(";", "\n\n");
                await DisplayAlert("Folgendes wird benötigt!", model.checkPermissionsMessage, "OK");
                //model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                return false;
            }
            model.CheckPermissionGPS();
            if (!String.IsNullOrWhiteSpace(model.checkPermissionGPSMessage))
            {
                model.checkPermissionGPSMessage = model.checkPermissionGPSMessage.Replace(";", "\n\n");
                await DisplayAlert("Berechtigungsproblem!", model.checkPermissionGPSMessage, "OK");
                //model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                return false;
            }
            return true;
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

            //model.SendLogZipFile();

            model.State.IsBackTappedToLogin = true;

            ClearPageViews();
            StartPage_Container.IsVisible = true;


            model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_STARTPAGE);
            return;
        }

        private async void ShowMainPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            StartPage_Container.IsVisible = true;

            SetLastBuilding();
            await Task.Delay(1);


            // Selektierte Arbeiten zur Ausführung (noch nicht gestartete Arbeiten)
            btn_showselected_pos_container.IsVisible = model.allSelectedPositionToWork.Count > 0;
            btn_showselected_pos_container_not.IsVisible = !(model.allSelectedPositionToWork.Count > 0);
            await Task.Delay(1);
            btn_showselected_pos_container2.IsVisible = model.allSelectedPositionToWork.Count > 0;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;

            /// GEÄNDERT ... SetAllSyncState in DELETE FILE Methode eingesetzt ///
            //______System.Timers.Timer runonce = new System.Timers.Timer(2000);
            //runonce.Elapsed += (s, e) =>
            //{
            //    MainThread.BeginInvokeOnMainThread(() =>
            //    {
            //        SetAllSync();
            //    });
            //};
            //runonce.AutoReset = false;
            //runonce.Start();

            await Task.Delay(1);
            SetChecksCount();
        }


        private async void ShowPN_Page()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            PN_Page_Container.IsVisible = true;


            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public void btn_PN_BackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }

        private async void ShowDSGVOPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            DSGVOPage_Container.IsVisible = true;


            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }


        private async void ShowWorkerPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
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


        private async void ShowBuildingScanPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            BuildingScanPage_Container.IsVisible = true;

            lay_buildingscan.Children.Clear();
            model.UseExternHardware = true;
            model.Scan.ScanBuildingView(this, lay_buildingscan, MethodAfterScan);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }

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
            list_notscan_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoList = 1;
        }
        public async void BuildNotScanList(string s)
        {
            list_notscan.Children.Clear();
            list_notscan_scroll.ScrollToAsync(0, 0, false);
            await Task.Delay(1);
            list_notscan.Children.Add(BuildingWSO.GetObjektNotScanListView(model, new Command<IntBoolParam>(SelectedObjektAufterNotScan), s));
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
            if (model.allPositionInWork != null)
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
                model.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                model.SetAllObjectAndValuesToNoSelectedBuilding();
                model.SettingModel.SettingDTO.LastBuildingIdScanned = intBol.val;
                model.LastBuilding = model.AllBuildings.Find(bu => bu.id == intBol.val);
                try
                {
                    AppModel.Logger.Info("CHECK-IN (OHNE QR-SCAN): " + model.LastBuilding.strasse + " " + model.LastBuilding.hsnr + model.LastBuilding.plz + " " + model.LastBuilding.ort);
                }
                catch (Exception) { }
                model.SettingModel.SaveSettings();
                list_notscan.Children.Clear();
                if (intBol.bol) { ShowMainPage(); }
                else
                {
                    await lastBuilding_Container.FadeTo(0, 500, Easing.SpringOut);
                    SetLastBuilding();
                    await lastBuilding_Container.FadeTo(1, 500, Easing.SpringIn);
                    overlay.IsVisible = false;
                    await Task.Delay(1);
                }
            }
        }



        public bool MethodAfterScan()
        {
            lay_buildingscan.Children.Clear();
            ShowMainPage();
            return true;
        }
        private async void ShowBuildingOutScanPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            BuildingOutScanPage_Container.IsVisible = true;

            lay_buildingoutscan.Children.Clear();
            model.UseExternHardware = true;

            model.Scan.ScanBuildingOutView(this, lay_buildingoutscan, MethodAfterOutScan);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public bool MethodAfterOutScan()
        {
            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                try
                {
                    string b = "";
                    if (model.LastBuilding != null && !String.IsNullOrWhiteSpace(model.LastBuilding.hsnr))
                    {
                        b = ": " + model.LastBuilding.strasse + " " + model.LastBuilding.hsnr + model.LastBuilding.plz + " " + model.LastBuilding.ort;
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
                model.UseExternHardware = false;
                lay_buildingoutscan.Children.Clear();
                if (model.OutScanBuilding != null)
                {
                    if (model.OutScanBuilding.id == model.LastBuilding.id)
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
            buildingorderlist_order_container.Children.Add(AuftragWSO.GetOrderListView(model, new Command<AuftragWSO>(SelectOrder)));
            BuildingOrderPage_order_Container.IsVisible = true;

            model.LastSelectedOrder = null;
            model.LastSelectedCategory = null;
            model.LastSelectedPosition = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void SelectOrder(AuftragWSO order)
        {
            model.LastSelectedOrder = order;
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
            buildingorderlist_category_container.Children.Add(KategorieWSO.GetCategoryListView(model, new Command<KategorieWSO>(SelectCategory)));
            BuildingOrderPage_category_Container.IsVisible = true;

            model.LastSelectedCategory = null;
            model.LastSelectedPosition = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }

        public void btn_showall_OrderCategoryTapped(object sender, EventArgs e)
        {
            model._showall_OrderCategory = !model._showall_OrderCategory;
            btn_back_inBuildingOrder_category_showall_txt.Text = model._showall_OrderCategory ? "Meine zeigen" : "Alle zeigen";

            buildingorderlist_category_container.Children.Clear();
            buildingorderlist_category_container.Children.Add(KategorieWSO.GetCategoryListView(model, new Command<KategorieWSO>(SelectCategory)));
            BuildingOrderPage_category_Container.IsVisible = true;
        }

        public async void SelectCategory(KategorieWSO category)
        {
            model.LastSelectedCategory = category;
            lb_inBuildingOrder_categorypos_text.Text = model.LastSelectedOrder.GetMobileText(); // + " \nNr.: " + model.LastSelectedOrder.id + "  Typ: " + model.LastSelectedOrder.typ;
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
            buildingorderlist_position_container.Children.Add(LeistungWSO.GetPositionListView(model, new Command<LeistungWSO>(SelectPositionToWork)));
            BuildingOrderPage_position_Container.IsVisible = true;

            model.LastSelectedPosition = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void SelectPositionToWork(LeistungWSO position)
        {
            bool inWork = false;
            if (model.allPositionInWork != null)
            {
                var foundInWork = model.allPositionInWork.leistungen.Find(l => l.id == position.id);
                inWork = foundInWork != null;
            }
            if (position.disabled || inWork) { return; }

            overlay.IsVisible = true;
            //await Task.Delay(1);

            model.LastSelectedPosition = position;
            Frame framePos = null;
            var selPost = model.allSelectedPositionToWork.Find(p => p.id == position.id);
            if (selPost != null)
            {
                // entfernen da schon selectiert 
                model.allSelectedPositionToWork.Remove(position);
                if (model.allPositionInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = false;
                    framePos.Content = LeistungWSO.GetPositionCardView(position, model, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            else
            {
                // hinzufügen
                model.allSelectedPositionToWork.Add(position);
                if (model.allPositionInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = true;
                    framePos.Content = LeistungWSO.GetSelectedPositionCardView(position, model, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            btn_showselected_pos_container.IsVisible = model.allSelectedPositionToWork.Count > 0;
            btn_showselected_pos_container_not.IsVisible = !(model.allSelectedPositionToWork.Count > 0);
            //btn_showselected_pos_container2.IsVisible = model.allSelectedPositionToWork.Count > 0;
            CheckForOptionalToWork();

            //await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void RemoveSelectPositionFromToWork(LeistungWSO position)
        {
            overlay.IsVisible = true;
            //await Task.Delay(1);

            Frame framePos;
            SwipeView swipePos;
            // entfernen da schon selectiert 
            model.allSelectedPositionToWork.Remove(position);
            position.selected = false;
            if (model.allPositionInShowingListView.TryGetValue(position.id, out framePos))
            {
                framePos.Content = LeistungWSO.GetPositionCardView(position, model, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
            }
            if (model.allPositionInShowingSmallListView.TryGetValue(position.id, out swipePos))
            {
                swipePos.IsVisible = false;
            }

            btn_showselected_pos_container.IsVisible = model.allSelectedPositionToWork.Count > 0;
            btn_showselected_pos_container_not.IsVisible = !(model.allSelectedPositionToWork.Count > 0);
            btn_showselected_pos_container2.IsVisible = model.allSelectedPositionToWork.Count > 0;
            CheckForOptionalToWork();
            if (model.allSelectedPositionToWork.Count == 0)
            {
                await Task.Delay(100);
                AuswahlAnzeigenTapped_Done(false);
                await Task.Delay(100);
                if (BuildingOrderPage_order_Container.IsVisible)
                {
                    buildingorderlist_order_container.Children.Clear();
                    buildingorderlist_order_container.Children.Add(AuftragWSO.GetOrderListView(model, new Command<AuftragWSO>(SelectOrder)));
                }
            }
            //await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void CheckForOptionalToWork()
        {
            model.IsOptionalToWork = false;

            var foundProduktPos = model.allSelectedPositionToWork.Find(i => (i.art == "Produkt"));
            var foundOPPos = model.allSelectedPositionToWork.Find(i => (i.art == "Leistung" && i.nichtpauschal == 1));
            var foundRegPos = model.allSelectedPositionToWork.Find(i => (i.art == "Leistung" && i.nichtpauschal == 0));
            if (model.allSelectedPositionToWork.Count == 0)
            {
                // alles zurücksetzen 
                model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(l =>
                        {
                            l.disabled = false;
                            Frame framePos;
                            if (model.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                            {
                                var func = ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command;
                                bool inWork = false;
                                if (model.allPositionInWork != null)
                                {
                                    var foundInWork = model.allPositionInWork.leistungen.Find(le => le.id == l.id);
                                    inWork = foundInWork != null;
                                }
                                framePos.Content = inWork ? LeistungWSO.GetInWorkPositionCardView(l, model, func).Content : LeistungWSO.GetPositionCardView(l, model, func).Content;
                            }
                        });
                    });
                });
            }
            else if (model.allSelectedPositionToWork.Count > 0)
            {
                // erste Einträge prüfen IsOptional
                if (foundOPPos != null)
                {
                    // OP Leistung gefunden dann keine Regulären leistungen zulassen
                    model.IsOptionalToWork = true;
                    lb_PosSelectionType_text.Text = "Nur optionale Positionen und Produkte aktiv!";
                    lb_PosSelectionType_text2.Text = "Nur optionale Positionen und Produkte aktiv!";
                }
                else if (foundRegPos != null)
                {
                    // Reguläre Leistung gefunden dann keine OP's Leistunge/Produkte/etc. zulassen
                    model.IsOptionalToWork = false;
                    lb_PosSelectionType_text.Text = "Nur geplante Positionen und Produkte aktiv!";
                    lb_PosSelectionType_text2.Text = "Nur geplante Positionen und Produkte aktiv!";
                }
                if (foundOPPos != null || foundRegPos != null)
                {
                    // check nach prüfen alle enable/disable setzten
                    model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                    {
                        o.kategorien.ForEach(c =>
                        {
                            c.leistungen.ForEach(l =>
                            {
                                bool inWork = false;
                                if (model.allPositionInWork != null)
                                {
                                    var foundInWork = model.allPositionInWork.leistungen.Find(le => le.id == l.id);
                                    inWork = foundInWork != null;
                                }
                                ICommand func = null;
                                Frame framePos;
                                if (model.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                {
                                    func = ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command;
                                }

                                if (model.IsOptionalToWork)
                                {
                                    if (l.art == "Leistung" && l.nichtpauschal == 1)
                                    {
                                        l.disabled = false;
                                        if (model.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            var stackPos = inWork ? LeistungWSO.GetInWorkPositionCardView(l, model, func) : (l.disabled ? LeistungWSO.GetDisabledPositionCardView(l, model, func) : (l.selected ? LeistungWSO.GetSelectedPositionCardView(l, model, func) : LeistungWSO.GetPositionCardView(l, model, func)));
                                            framePos.Content = stackPos.Content;
                                        }
                                    }
                                    else if (l.art == "Leistung" && l.nichtpauschal == 0)
                                    {
                                        l.disabled = true;
                                        if (model.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            framePos.Content = inWork ? LeistungWSO.GetInWorkPositionCardView(l, model, func).Content : LeistungWSO.GetDisabledPositionCardView(l, model, func).Content;
                                        }
                                    }
                                }
                                else
                                {
                                    if (l.art == "Leistung" && l.nichtpauschal == 1)
                                    {
                                        l.disabled = true;
                                        if (model.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            framePos.Content = inWork ? LeistungWSO.GetInWorkPositionCardView(l, model, func).Content : LeistungWSO.GetDisabledPositionCardView(l, model, func).Content;
                                        }
                                    }
                                    else
                                    {
                                        l.disabled = false;
                                        if (model.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                        {
                                            var stackPos = inWork ? LeistungWSO.GetInWorkPositionCardView(l, model, func) : (l.disabled ? LeistungWSO.GetDisabledPositionCardView(l, model, func) : (l.selected ? LeistungWSO.GetSelectedPositionCardView(l, model, func) : LeistungWSO.GetPositionCardView(l, model, func)));
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
                    model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                    {
                        o.kategorien.ForEach(c =>
                        {
                            c.leistungen.ForEach(l =>
                            {
                                l.disabled = false;
                                Frame framePos;
                                if (model.allPositionInShowingListView.TryGetValue(l.id, out framePos))
                                {
                                    var func = ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command;
                                    bool inWork = false;
                                    if (model.allPositionInWork != null)
                                    {
                                        var foundInWork = model.allPositionInWork.leistungen.Find(le => le.id == l.id);
                                        inWork = foundInWork != null;
                                    }
                                    //framePos.Content = inWork ? LeistungWSO.GetInWorkPositionCardView(l, model, func).Content : LeistungWSO.GetPositionCardView(l, model, func).Content;

                                    var stackPos = inWork ? LeistungWSO.GetInWorkPositionCardView(l, model, func) : (l.disabled ? LeistungWSO.GetDisabledPositionCardView(l, model, func) : (l.selected ? LeistungWSO.GetSelectedPositionCardView(l, model, func) : LeistungWSO.GetPositionCardView(l, model, func)));
                                    framePos.Content = stackPos.Content;
                                }
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

                var startDT = new DateTime(model.allPositionInWork.startticks);
                var endDT = DateTime.Now;
                var ts = (endDT - startDT);

                timespan_inwork.Children.Clear();
                timespan_inwork.Children.Add(new Image
                {
                    Margin = new Thickness(0, 0, 10, 0),
                    HeightRequest = 30,
                    WidthRequest = 30,
                    VerticalOptions = LayoutOptions.Center,
                    Source = model.imagesBase.Time
                });
                timespan_inwork.Children.Add(new Label
                {
                    Text = startDT.ToString("dd.MM.yy") + "\n" + startDT.ToString("HH:mm"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    FontSize = 16,
                    TextColor = Color.White,
                    HorizontalTextAlignment = TextAlignment.Start,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                });
                timespan_inwork.Children.Add(new Label
                {
                    Text = " - ",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    FontSize = 14,
                    TextColor = Color.Yellow,
                    HorizontalTextAlignment = TextAlignment.Start,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                });
                timespan_inwork.Children.Add(new Label
                {
                    Text = endDT.ToString("dd.MM.yy") + "\n" + endDT.ToString("HH:mm"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    FontSize = 16,
                    TextColor = Color.White,
                    HorizontalTextAlignment = TextAlignment.Start,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                });
                timespan_inwork.Children.Add(new Label
                {
                    Text = " = ",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    FontSize = 14,
                    TextColor = Color.White,
                    HorizontalTextAlignment = TextAlignment.Start,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                });
                timespan_inwork.Children.Add(new Label
                {
                    Text = (ts.TotalDays > 1 ? ts.ToString("%d") + "T " : "") + ts.ToString(@"hh\:mm"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    FontSize = 16,
                    TextColor = Color.Yellow,
                    HorizontalTextAlignment = TextAlignment.Start,
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0)
                });
                runningworks_list.Children.Clear();
                runningworks_list.Children.Add(LeistungWSO.GetInWorkPositionListView(model, new Command<LeistungWSO>(TapNoticeFromPosInWork)));

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
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            DayOverPage_Container.IsVisible = true;
            lastDayOverStack.Children.Clear();
            var dayOvers = DayOverWSO.LoadAll(model);
            dayOvers.ForEach(d =>
            {
                var dt = new DateTime(d.endticks);
                lastDayOverStack.Children.Add(new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    BackgroundColor = Color.FromHex("#042d53"),
                    Spacing = 0,
                    Margin = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(5),
                    Children =
                    {
                        new Label
                        {
                            Text = "Zuletzt:   " + dt.ToString("dd.MM.yyyy") + "  -  " + dt.ToString("HH:mm"),
                            FontSize = 14,
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            TextColor = Color.FromHex("#ffffff"),
                        }
                    }
                });
            });


            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }


        public async void TapNoticeFromPosInWorkDirektPosMuell(LeistungWSO p)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            btn_alertmessage_tit.Text = "Bemerkung";
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
            ShowNoticeViewDirektPos(false, p, "muellpos");

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void TapNoticeFromPosInWorkDirektPos(LeistungWSO position)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            btn_alertmessage_tit.Text = "Bemerkung";
            ShowNoticeViewDirektPos(false, position, "winterpos");

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        private async void ShowNoticeViewDirektPos(bool prio, LeistungWSO pos = null, string backTo = null)
        {
            _SelectedBemerkungForNotice = new BemerkungWSO();
            _SelectedPosForNotice = pos;
            _BackToFromNotice = backTo;
            btn_notice_del_DirektPos.IsVisible = false;
            if (pos != null)
            {

                BuildingWSO building = BuildingWSO.LoadBuilding(model, pos.objektid);
                var o = building.ArrayOfAuftrag.Find(auf => auf.id == pos.auftragid);
                var c = o.kategorien.Find(kat => kat.id == pos.kategorieid);
                var l = c.leistungen.Find(f => f.id == pos.id);
                //var lInWork = model.allPositionInWork.leistungen.Find(f => f.id == pos.id);
                var stackPos = LeistungWSO.GetInWorkPositionSmallCardView_DirektPos(o, c, l, l);

                noticeFor_DirektPos.IsVisible = true;
                noticeFor_Pos_DirektPos.Children.Clear();
                noticeFor_Pos_DirektPos.Children.Add(stackPos);
                await Task.Delay(1);
                if (_SelectedBemerkungForNoticeList_DirektPos != null)
                {
                    foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                    {
                        if (item.id == _SelectedPosForNotice.id && item.bem != null)
                        {
                            _SelectedBemerkungForNotice = item.bem;
                            entry_notice_DirektPos.Text = item.bem.text;
                            sw_internmessage_DirektPos.IsToggled = item.bem.prio == 1 || item.bem.prio == 3;
                            sw_alertmessage_DirektPos.IsToggled = item.bem.prio == 2 || item.bem.prio == 3;
                            item.bem.photos.ForEach(p =>
                            {
                                noticePhotoStack_DirektPos.Children.Add(p.stack);
                            });
                            btn_notice_del_DirektPos.IsVisible = true;
                        }
                    }
                }
            }
            else
            {
                noticeFor_DirektPos.IsVisible = false;
            }

            overlay.IsVisible = true;
            await Task.Delay(1);

            //ClearPageViews();

            popupContainer_quest_direktbuchen.IsVisible = false;
            CheckNoticeFalid_DirektPos();
            NoticePage_Container_DirektPos.IsVisible = true;

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public void btn_NoticeBackTapped_DirektPos(object sender, EventArgs e)
        {
            this.Focus();

            entry_notice_DirektPos.Text = "";
            noticePhotoStack_DirektPos.Children.Clear();
            //_SelectedBemerkungForNoticeList = null;
            _SelectedPosForNotice = null;
            _SelectedBemerkungForNotice = null;
            _BackToFromNotice = null;

            NoticePage_Container_DirektPos.IsVisible = false;
            popupContainer_quest_direktbuchen.IsVisible = true;
        }
        public async void btn_NoticeDelTapped_DirektPos(object sender, EventArgs e)
        {
            if (_SelectedBemerkungForNotice != null && _SelectedBemerkungForNoticeList_DirektPos != null)
            {
                foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                {
                    if (item.id == _SelectedPosForNotice.id)
                    {
                        item.badge.Text = "";
                        item.badgeStack.IsVisible = false;
                        item.bem = null;
                        sw_internmessage_DirektPos.IsToggled = false;
                        sw_alertmessage_DirektPos.IsToggled = false;
                    }
                }
                btn_NoticeBackTapped_DirektPos(null, null);
            }
        }
        public async void btn_NoticeSaveTapped_DirektPos(object sender, EventArgs e)
        {
            this.Focus();
            if (!String.IsNullOrWhiteSpace(entry_notice_DirektPos.Text.Trim()) || (_SelectedBemerkungForNotice.photos != null && _SelectedBemerkungForNotice.photos.Count > 0))
            {
                overlay.IsVisible = true;
                await Task.Delay(1);

                int am = sw_alertmessage_DirektPos.IsToggled ? 2 : 0;
                int im = sw_internmessage_DirektPos.IsToggled ? 1 : 0;
                _SelectedBemerkungForNotice.prio = (am + im);
                _SelectedBemerkungForNotice.gruppeid = _SelectedPosForNotice.gruppeid;
                _SelectedBemerkungForNotice.personid = model.Person.id;
                _SelectedBemerkungForNotice.objektid = _SelectedPosForNotice.objektid;
                _SelectedBemerkungForNotice.leistungid = _SelectedPosForNotice.id;
                _SelectedBemerkungForNotice.datum = DateTime.Now.Ticks;
                _SelectedBemerkungForNotice.text = "" + entry_notice_DirektPos.Text;

                foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                {
                    if (item.id == _SelectedPosForNotice.id)
                    {
                        item.bem = _SelectedBemerkungForNotice;
                        item.badge.Text = "" + (_SelectedBemerkungForNotice.photos.Count + (string.IsNullOrWhiteSpace(item.bem.text) ? 0 : 1));
                        item.badgeStack.IsVisible = _SelectedBemerkungForNotice.photos.Count() > 0 || !string.IsNullOrWhiteSpace(item.bem.text);
                    }
                }
                //if (_SelectedPosForNotice != null)
                //{
                //    var posInWork = model.allPositionInWork.leistungen.Find(pos => pos.id == _SelectedPosForNotice.id);
                //    if (posInWork.bemerkungen == null) { posInWork.bemerkungen = new List<BemerkungWSO>(); }
                //    _SelectedBemerkungForNotice.leistungid = _SelectedPosForNotice.id;
                //    posInWork.bemerkungen.Add(_SelectedBemerkungForNotice);
                //    LeistungPackWSO.Save(model, model.allPositionInWork);
                //}
                //else
                //{
                //    BemerkungWSO.ToUploadStack(model, _SelectedBemerkungForNotice);
                //    SyncSingleNotice();
                //}


                await Task.Delay(1);
                btn_NoticeBackTapped_DirektPos(null, null);
                overlay.IsVisible = false;
                await Task.Delay(1);
            }
        }
        private void AlertMessage_Switch_Toggled_DirektPos(object sender, ToggledEventArgs e)
        {
            btn_alertmessage_img2_DirektPos.IsVisible = e.Value;
        }
        private void InternMessage_Switch_Toggled_DirektPos(object sender, ToggledEventArgs e)
        {
            btn_internmessage_img2_DirektPos.IsVisible = e.Value;
        }

        public async void btn_takePhoto_DirektPos(object sender, EventArgs e)
        {
            if (_SelectedBemerkungForNotice.photos.Count > 4) { return; }
            notizSave_stack_DirektPos.IsVisible = false;
            await Task.Delay(1);


            try
            {
                model.UseExternHardware = true;
                var photo = await MediaGallery.CapturePhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    var photoResponse = PhotoUtils.GetImages(stream);
                    string text = null;
                    if (AppModel.Instance.LastBuilding == null)
                    {
                        if (_SelectedPosForNotice != null)
                        {
                            var bui = BuildingWSO.LoadBuilding(AppModel.Instance, _SelectedPosForNotice.objektid);
                            if (bui != null)
                            {
                                text = bui.plz + " " + bui.ort + " - " + bui.strasse + " " + bui.hsnr;
                            }
                        }
                    }
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, null, text);

                    long bildName = DateTime.Now.Ticks;
                    var b = new BildWSO(_SelectedBemerkungForNotice.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = "" + bildName,
                        stack = BildWSO.GetAttachmentForNoticeElement(
                            photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                            new Command<BildWSO>(RemoveBildInWork_DirektPos))
                    };
                    var frame = (Frame)((StackLayout)(b.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork_DirektPos), CommandParameter = b });

                    BildWSO.Save(AppModel.Instance, b);
                    _SelectedBemerkungForNotice.photos.Add(b);

                    noticePhotoStack_DirektPos.Children.Add(b.stack);

                    CheckNoticeFalid_DirektPos();

                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    model.UseExternHardware = false;
                    stream.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                // handling a cancellation request
            }
            catch (Exception)
            {
                // handling other exceptions
            }
            finally
            {
                model.UseExternHardware = false;
            }

        }
        public async void btn_pickPhotos_DirektPos(object sender, EventArgs e)
        {
            if (_SelectedBemerkungForNotice.photos.Count > 4) { return; }
            notizSave_stack_DirektPos.IsVisible = false;
            await Task.Delay(1);

            try
            {
                var cts = new CancellationTokenSource();
                IMediaFile[] files = null;

                model.UseExternHardware = true;
                var request = new MediaPickRequest(5 - _SelectedBemerkungForNotice.photos.Count, MediaFileType.Image)
                {
                    //PresentationSourceBounds = System.Drawing.Rectangle.Empty,
                    UseCreateChooser = false,
                    Title = "Select"
                };

                cts.CancelAfter(TimeSpan.FromMinutes(5));

                var results = await MediaGallery.PickAsync(request, cts.Token);
                files = results?.Files?.ToArray();

                if (files == null)
                    return;

                foreach (var fil in files)
                {
                    var stream = await fil.OpenReadAsync();

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    var photoResponse = PhotoUtils.GetImages(stream);
                    string text = null;
                    if (AppModel.Instance.LastBuilding == null)
                    {
                        if (_SelectedPosForNotice != null)
                        {
                            var bui = BuildingWSO.LoadBuilding(AppModel.Instance, _SelectedPosForNotice.objektid);
                            if (bui != null)
                            {
                                text = bui.plz + " " + bui.ort + " - " + bui.strasse + " " + bui.hsnr;
                            }
                        }
                    }
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, null, text);

                    long bildName = DateTime.Now.Ticks;
                    var b = new BildWSO(_SelectedBemerkungForNotice.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = "" + bildName,
                        stack = BildWSO.GetAttachmentForNoticeElement(photoResponse.GetImageSourceAsThumb(), new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                            new Command<BildWSO>(RemoveBildInWork_DirektPos))
                    };
                    var frame = (Frame)((StackLayout)(b.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork_DirektPos), CommandParameter = b });

                    BildWSO.Save(AppModel.Instance, b);
                    _SelectedBemerkungForNotice.photos.Add(b);
                    noticePhotoStack_DirektPos.Children.Add(b.stack);
                }
            }
            catch (Exception ex)
            {
                var a = ex;
            }
            finally
            {
                model.UseExternHardware = false;
            }

            CheckNoticeFalid_DirektPos();

            await Task.Delay(1);
            overlay.IsVisible = false;


        }
        public async void RemoveBildInWork_DirektPos(BildWSO b)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            noticePhotoStack_DirektPos.Children.Remove(b.stack);
            await Task.Delay(1);
            BildWSO.Delete(AppModel.Instance, b);
            await Task.Delay(1);
            _SelectedBemerkungForNotice.photos.Remove(b);
            CheckNoticeFalid_DirektPos();

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        private void entry_notice_TextChanged_DirektPos(object sender, TextChangedEventArgs e)
        {
            if (_SelectedBemerkungForNotice != null && !_manuelTextChange)
            {
                _manuelTextChange = true;
                //_SelectedBemerkungForNotice.text = e.NewTextValue;
                CheckNoticeFalid_DirektPos();
                _manuelTextChange = false;
            }
        }

        private void CheckNoticeFalid_DirektPos()
        {
            if (_SelectedBemerkungForNotice != null)
            {
                notizSave_stack_DirektPos.IsVisible = !String.IsNullOrWhiteSpace(entry_notice_DirektPos.Text) || _SelectedBemerkungForNotice.photos.Count > 0;
                btn_notice_del_DirektPos.IsVisible = true;
            }
            else
            {
                notizSave_stack_DirektPos.IsVisible = false;
                btn_notice_del_DirektPos.IsVisible = false;
            }
        }



        public async void TapNoticeFromPosInWork(LeistungWSO position)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            btn_alertmessage_tit.Text = "Bemerkung";
            sw_alertmessage.IsVisible = false;
            sw_alertmessage.IsToggled = false;
            btn_alertmessage_img2.IsVisible = false;
            sw_internmessage.IsToggled = false;
            btn_internmessage_img2.IsVisible = false;
            ShowNoticeView(false, position, "inwork");

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        private LeistungWSO _SelectedPosForNotice = null;
        private BemerkungWSO _SelectedBemerkungForNotice = null;
        private string _BackToFromNotice = null;
        private async void ShowNoticeView(bool prio, LeistungWSO pos = null, string backTo = null)
        {
            _SelectedBemerkungForNotice = new BemerkungWSO();
            _SelectedPosForNotice = pos;
            _BackToFromNotice = backTo;
            if (pos != null)
            {
                BuildingWSO building;
                if (model.LastBuilding == null)
                {
                    building = BuildingWSO.LoadBuilding(model, pos.objektid);
                }
                else
                {
                    building = model.LastBuilding;
                }
                var o = building.ArrayOfAuftrag.Find(auf => auf.id == pos.auftragid);
                var c = o.kategorien.Find(kat => kat.id == pos.kategorieid);
                var l = c.leistungen.Find(f => f.id == pos.id);
                var lInWork = model.allPositionInWork.leistungen.Find(f => f.id == pos.id);
                var stackPos = LeistungWSO.GetInWorkPositionSmallCardView(o, c, l, lInWork, model);

                noticeFor.IsVisible = true;
                noticeFor_Pos.Children.Clear();
                noticeFor_Pos.Children.Add(stackPos);
            }
            else
            {
                noticeFor.IsVisible = false;
            }

            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();

            CheckNoticeFalid();
            NoticePage_Container.IsVisible = true;

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        private async void ShowObjectValuesView()
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            btn_objectValuesNowTapped(null, null);

            ClearPageViews();

            ObjectValues_BuildingInfo.Children.Clear();
            ObjectValues_BuildingInfo.Children.Add(Elements.GetBoxViewLine());
            ObjectValues_BuildingInfo.Children.Add(BuildingWSO.GetBuildingInfoElement(model.LastBuilding, model));
            ObjectValues_BuildingInfo.Children.Add(Elements.GetBoxViewLine());

            ObjectValuesStack.Children.Clear();
            var vStack = ObjektDataWSO.GetObjektDataListView(model, new Command<ObjektDataWSO>(TapObjektData));
            ObjectValuesStack.Children.Add(vStack);

            ObjectValuesStackChangedToday.Children.Clear();
            var vStackToday = ObjektDataWSO.GetObjektDataListView(model, new Command<ObjektDataWSO>(TapObjektData), true);
            ObjectValuesStackChangedToday.Children.Add(vStackToday);

            model.selectedObjectValue = null;

            ObjectValuesPage_Container.IsVisible = true;
            ObjectValuesPage_position_Container.IsVisible = true;

            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void btn_objectValuesNowTapped(object sender, EventArgs e)
        {
            scroll_ObjectValuesStack.IsVisible = true;
            scroll_ObjectValuesStackChangedToday.IsVisible = false;
            btn_objectValuesNow.BackgroundColor = Color.FromHex("#999999");
            btn_objectValuesToday.BackgroundColor = Color.FromHex("#042d53");
        }
        public async void btn_objectValuesTodayTapped(object sender, EventArgs e)
        {
            scroll_ObjectValuesStack.IsVisible = false;
            scroll_ObjectValuesStackChangedToday.IsVisible = true;
            btn_objectValuesNow.BackgroundColor = Color.FromHex("#042d53");
            btn_objectValuesToday.BackgroundColor = Color.FromHex("#999999");
        }

        public async void TapObjektData(ObjektDataWSO od)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            model.selectedObjectValue = od;
            ShowObjectValuesEditView();
        }
        private async void ShowObjectValuesEditView()
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();

            ObjectValues_BuildingInfo_edit.Children.Clear();
            ObjectValues_BuildingInfo_edit.Children.Add(Elements.GetBoxViewLine());
            ObjectValues_BuildingInfo_edit.Children.Add(BuildingWSO.GetBuildingInfoElement(model.LastBuilding, model));
            ObjectValues_BuildingInfo_edit.Children.Add(Elements.GetBoxViewLine());
            ObjectValues_BuildingInfo_edit.Children.Add(ObjektDataWSO.GetObjektValueInfoElement(model.selectedObjectValue, model, null));
            ObjectValues_BuildingInfo_edit.Children.Add(Elements.GetBoxViewLine());

            ObjectValues_Info_edit.Children.Clear();
            ObjectValues_Info_edit.Children.Add(ObjektDataWSO.EditObjektValueField(model.selectedObjectValue, model,
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
            popupContainer_objectvaluesbild.IsVisible = true;
            //popupContainer_objectvaluesbild_stack.WidthRequest = App.Current.MainPage.Width;
            AbsoluteLayout.SetLayoutFlags(popupContainer_objectvaluesbild_stack, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(popupContainer_objectvaluesbild_stack, new Rectangle(0, 30, App.Current.MainPage.Width, 520));

            editor_notice_objectvaluesbild.Text = "";
            img_photo_objectvaluesbild.Source = null;
            await Task.Delay(1);
            model.selectedObjectValueBild = null;

            btn_send_objectvaluesbild.IsVisible = false;
            btn_send_objectvaluesbild_err.Opacity = 0;
        }

        private async void SwitchObjectValueFlashlight()
        {
            model.Scan.Btn_FlashlightAloneTapped(null, null);
        }

        public async void SaveObjektValue(ObjektDataWSO newod)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            if (AppModel.Instance.isFlashLigthAloneON)
            {
                model.Scan.Btn_FlashlightAloneTapped(null, null);
            }
            await Task.Delay(1);

            long datum = JavaScriptDateConverter.Convert(DateTime.Now, -2);
            newod.standGeaendertAm = "" + datum;
            newod.standdatum = "" + datum;
            newod.lastchange = "" + datum;
            model.selectedObjectValue = newod;

            model.LastBuilding.ArrayOfObjektdata.ForEach(od =>
            {
                if (od.id == newod.id)
                {
                    od.lastStand = Utils.formatDEStr3(decimal.Parse(od.stand));
                    od.stand = Utils.formatDEStr3(decimal.Parse(newod.firstStand));
                    od.standdatum = "" + datum;
                    od.standGeaendertAm = "" + datum;
                    od.lastchange = "" + datum;
                    od.ablesegrund = "" + newod.ablesegrund;
                }
            });
            BuildingWSO.Save(model, model.LastBuilding);

            newod.guid = Guid.NewGuid().ToString();
            newod.ticks = DateTime.Now.Ticks;
            newod.lastStand = Utils.formatDEStr3(decimal.Parse(newod.stand));
            newod.stand = Utils.formatDEStr3(decimal.Parse(newod.firstStand));
            ObjektDataWSO.ToUploadStack(model, newod);
            await Task.Delay(1);
            SyncObjectValues();

            await Task.Delay(1);
            ShowObjectValuesView();
        }

        public void ClearPageViews()
        {
            NotScanPage_Container.IsVisible = false;
            PersonTimesPage_Container.IsVisible = false;
            NachbuchenPage_Container.IsVisible = false;
            TodoPage_Container.IsVisible = false;
            StartPage_Container.IsVisible = false;
            DSGVOPage_Container.IsVisible = false;
            PN_Page_Container.IsVisible = false;
            WorkerPage_Container.IsVisible = false;
            BuildingScanPage_Container.IsVisible = false;
            BuildingOutScanPage_Container.IsVisible = false;
            BuildingOrderPage_Container.IsVisible = false;
            RunningWorksPage_Container.IsVisible = false;
            NoticePage_Container.IsVisible = false;
            DayOverPage_Container.IsVisible = false;
            ObjectValuesPage_Container.IsVisible = false;
            ObjectValuesPage_position_Container.IsVisible = false;
            ObjectValuesPage_Edit_Container.IsVisible = false;
            SettingsPage_Container.IsVisible = false;
            MapPage_Container.IsVisible = false;
        }

        private void Settings_Log_includeCache_Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (!isInitialize)
            {
                AppModel.Instance.InclFilesAsJson = e.Value;
            }
        }

        ////private void Settings_Hintergrundprozess_Switch_Toggled(object sender, ToggledEventArgs e)
        ////{
        ////    if (!isInitialize)
        ////    {
        ////        AppModel.Instance.SettingModel.SettingDTO.RunBackground = e.Value;
        ////        AppModel.Instance.SettingModel.SaveSettings();
        ////    }
        ////}
        public void btn_SettingsBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }
        public async void btn_SettingsTapped(object sender, EventArgs e)
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            btn_settings_sendlog.IsVisible = true; // BTN SendLOG wieder aneigen!

            MainMenuTapped_Done(false);
            await Task.Delay(210);

            ClearPageViews();
            SettingsPage_Container.IsVisible = true;
            ////sw_setting_hintergrundprozess.IsToggled = AppModel.Instance.SettingModel.SettingDTO.RunBackground;

            int countAll = GetAllSyncFromUploadCount();
            settings_count_positionen.Text = (countAll > 0 ? "" + countAll : "Keine Daten vorhanden");
            btn_settings_count_positionen.IsVisible = countAll > 0;

            btn_settings_count_positionen.GestureRecognizers.Clear();
            var tgr_btn_settings_count_positionen = new TapGestureRecognizer();
            tgr_btn_settings_count_positionen.Tapped += btn_SettingsSyncUploadTapped;
            btn_settings_count_positionen.GestureRecognizers.Add(tgr_btn_settings_count_positionen);


            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void btn_SettingsSyncUploadTapped(object sender, EventArgs e)
        {
            CheckAllSyncFromUpload();
            settings_count_positionen.Text = "Versucht hochzuladen";
            btn_settings_count_positionen.IsVisible = false;
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
            await tourScroller.ScrollToAsync(0, 0, false);
            popupContainer_quest_daypicker.IsVisible = false;
            var xday = int.Parse(((Label)o).ClassId);
            Update_PlanTabs(xday);
        }
        private void Fill_DayPicker()
        {
            tabContentWidth = App.Current.MainPage.Width - 13; //28 ;

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
                    TextColor = Color.FromHex("#cccccc"),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    ClassId = "" + i,
                };
                lb_day.GestureRecognizers.Clear();
                var t_lb = new TapGestureRecognizer();
                t_lb.Tapped -= (object o, TappedEventArgs ev) => { daypicker_SelectedIndexChanged(o, i); };
                t_lb.Tapped += (object o, TappedEventArgs ev) => { daypicker_SelectedIndexChanged(o, i); };
                lb_day.GestureRecognizers.Add(t_lb);
                daypicker_items.Children.Add(lb_day);
                var bv = new BoxView
                {
                    BackgroundColor = Color.Gray,
                    HeightRequest = 1,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
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
            t_frame_planConA_offentxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScroller.ScrollToAsync(0, 0, true); };
            t_frame_planConA_offentxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScroller.ScrollToAsync(0, 0, true); };
            frame_planConA_offenbtn.GestureRecognizers.Add(t_frame_planConA_offentxt);
            frame_planConA_erlbtn.GestureRecognizers.Clear();
            var t_frame_planConA_erltxt = new TapGestureRecognizer();
            t_frame_planConA_erltxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScroller.ScrollToAsync(tabContentWidth * 1, 0, true); };
            t_frame_planConA_erltxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScroller.ScrollToAsync(tabContentWidth * 1, 0, true); };
            frame_planConA_erlbtn.GestureRecognizers.Add(t_frame_planConA_erltxt);
            frame_planConA_veroffenbtn.GestureRecognizers.Clear();
            var t_frame_planConA_veroffentxt = new TapGestureRecognizer();
            t_frame_planConA_veroffentxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScroller.ScrollToAsync(tabContentWidth * 2, 0, true); };
            t_frame_planConA_veroffentxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScroller.ScrollToAsync(tabContentWidth * 2, 0, true); };
            frame_planConA_veroffenbtn.GestureRecognizers.Add(t_frame_planConA_veroffentxt);

            frame_planConB_offenbtn.GestureRecognizers.Clear();
            var t_frame_planConB_offentxt = new TapGestureRecognizer();
            t_frame_planConB_offentxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScrollerB.ScrollToAsync(0, 0, true); };
            t_frame_planConB_offentxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScrollerB.ScrollToAsync(0, 0, true); };
            frame_planConB_offenbtn.GestureRecognizers.Add(t_frame_planConB_offentxt);
            frame_planConB_erlbtn.GestureRecognizers.Clear();
            var t_frame_planConB_erltxt = new TapGestureRecognizer();
            t_frame_planConB_erltxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScrollerB.ScrollToAsync(tabContentWidth * 1, 0, true); };
            t_frame_planConB_erltxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScrollerB.ScrollToAsync(tabContentWidth * 1, 0, true); };
            frame_planConB_erlbtn.GestureRecognizers.Add(t_frame_planConB_erltxt);

            frame_planConC_offenbtn.GestureRecognizers.Clear();
            var t_frame_planConC_offentxt = new TapGestureRecognizer();
            t_frame_planConC_offentxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScrollerC.ScrollToAsync(0, 0, true); };
            t_frame_planConC_offentxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScrollerC.ScrollToAsync(0, 0, true); };
            frame_planConC_offenbtn.GestureRecognizers.Add(t_frame_planConC_offentxt);
            frame_planConC_workbtn.GestureRecognizers.Clear();
            var t_frame_planConC_worktxt = new TapGestureRecognizer();
            t_frame_planConC_worktxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScrollerC.ScrollToAsync(tabContentWidth * 1, 0, true); };
            t_frame_planConC_worktxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScrollerC.ScrollToAsync(tabContentWidth * 1, 0, true); };
            frame_planConC_workbtn.GestureRecognizers.Add(t_frame_planConC_worktxt);
            frame_planConC_erlbtn.GestureRecognizers.Clear();
            var t_frame_planConC_erltxt = new TapGestureRecognizer();
            t_frame_planConC_erltxt.Tapped -= async (object o, TappedEventArgs ev) => { await tourScrollerC.ScrollToAsync(tabContentWidth * 2, 0, true); };
            t_frame_planConC_erltxt.Tapped += async (object o, TappedEventArgs ev) => { await tourScrollerC.ScrollToAsync(tabContentWidth * 2, 0, true); };
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
            empListView.SelectedItem = null;
            popupContainer_quest_personpicker_inner.HeightRequest = App.Current.MainPage.Height - 100;
            popupContainer_quest_personpicker_inner.WidthRequest = App.Current.MainPage.Width - 40;
            popupContainer_quest_personpicker.IsVisible = true;

            var empList = AppModel.Instance.PlanResponse.persons;

            var groupedData =
                empList.OrderBy(e => e.name)
                    .GroupBy(e => e.name[0].ToString())
                    .Select(e => new ObservablePersonSmallWSOCollection<string, PersonSmallWSO>(e))
                    .ToList();

            BindingContext = new ObservableCollection<ObservablePersonSmallWSOCollection<string, PersonSmallWSO>>(groupedData);

        }
        private void empListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                var p = (PersonSmallWSO)e.SelectedItem;
                model.PlanResponse.selectedPerson = p;
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
            popupContainer_infodialog_text.Text = model.LastBuilding.notiz;
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
            if (!model.AppControll.showObjektPlans) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);

            model.PlanResponse.selectedPerson = p;
            if (p != null)
            {

                var result = await Task.Run(() => { return model.Connections.GetPlanPersons(p.id, true); });
                if (result)
                {
                    frame_plantabA.Margin = new Thickness(0, -8, 2, 0);
                    frame_plantabB.Margin = new Thickness(0, 0, 2, 0);
                    frame_plantabCe.Margin = new Thickness(0, 0, 2, 0);
                    frame_plantabC.Margin = new Thickness(0, 0, 2, 0);


                    //ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                    frame_planConA_img_reloadx.Source = "muellInOutX" + AppModel.Instance.AppSetModel.ViewOnlyMuell + ".png";
                    frame_planConA_img_reload.Source = model.imagesBase.DropLeftImage;
                    frame_planConA_img_reload2.Source = model.imagesBase.DropLeftImage;

                    frame_planConA_reload_text.Text = "Mein Plan";
                    frame_planConA_reload2_text.Text = "Mein Plan";
                    frame_planConA_reload_text.TextColor = Color.Yellow;
                    frame_planConA_reload2_text.TextColor = Color.Yellow;
                    frame_planConA_otherperson_name2.TextColor = Color.Yellow;
                    frame_planConA_otherperson_name.TextColor = Color.Yellow;
                    frame_planConA_otherperson_name.Text = p.name.Length > 9 ? p.name.Substring(0, 10) + "..." : p.name;
                    frame_planConA_otherperson_name2.Text = p.name.Length > 12 ? p.name.Substring(0, 13) + "..." : p.name;
                }
                else
                {
                    // Alert nicht Online oder es konnten keine Daten geladen werden
                }

                if (AppModel.Instance.PlanOthePersonResponse != null && AppModel.Instance.PlanOthePersonResponse.lastCall != null)
                {
                    ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Color.FromHex("#aaaaaa");
                    ObjektPlanWeekMobil_Stack_ABC_text.Text =
                        "Andere Planliste: " + AppModel.Instance.PlanOthePersonResponse.lastCall.Value.ToString("dd.MM.yyyy - HH:mm");
                }
                else
                {
                    ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Color.Yellow;
                    ObjektPlanWeekMobil_Stack_ABC_text.Text = "Andere Planliste: - Konnte nicht geladen werden!";
                }
            }
            Update_PlanTabs((int)DateTime.Now.DayOfWeek);
        }
        public async void PlanTypeChange()
        {
            if (!model.AppControll.showObjektPlans) { return; }
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
            frame_planConA_img_reloadx.Source = "muellInOutX" + AppModel.Instance.AppSetModel.ViewOnlyMuell + ".png";

            foreach (var o in frame_planListA.Children)
            {
                var isMuell = !String.IsNullOrWhiteSpace(o.ClassId);
                o.IsVisible = AppModel.Instance.AppSetModel.ViewOnlyMuell == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 1 && !isMuell || AppModel.Instance.AppSetModel.ViewOnlyMuell == 2 && isMuell;
            }

            //Update_PlanTabs((int)DateTime.Now.DayOfWeek);
            //PlanResp.planweek.ForEach(p => {
            //if (p.day > 0 && p.view != null)
            //    {
            //        p.view.IsVisible = AppModel.Instance.AppSetModel.ViewOnlyMuell == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 1 && p.muelltoid == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 2 && p.muelltoid > 0;
            //    }
            //});
        }

        public async void ReloadPlanData(int tab)
        {
            btn_PlanTabATapped(null, null);
            //if (tab == 0) { btn_PlanTabATapped(null, null); }
            //if (tab == 1) { btn_PlanTabBTapped(null, null); }

            bool reloadOr = frame_planConA_reload_text.Text == "Mein Plan";
            if (!model.AppControll.showObjektPlans) { return; }
            frame_planConA_img_reloadx.Source = "muellInOutX" + AppModel.Instance.AppSetModel.ViewOnlyMuell + ".png";
            frame_planConA_img_reload.Source = model.imagesBase.Refresh;
            frame_planConA_img_reload2.Source = model.imagesBase.Refresh;
            frame_planConA_reload_text.Text = "Neu laden";
            frame_planConA_reload2_text.Text = "Neu laden";
            frame_planConA_otherperson_name.Text = "Arbeiter";
            frame_planConA_otherperson_name2.Text = "Arbeiter";
            frame_planConA_reload_text.TextColor = Color.White;
            frame_planConA_reload2_text.TextColor = Color.White;
            frame_planConA_otherperson_name2.TextColor = Color.White;
            frame_planConA_otherperson_name.TextColor = Color.White;
            model.PlanResponse.selectedPerson = null;

            if (reloadOr)
            {
                Update_PlanTabs((int)DateTime.Now.DayOfWeek);
            }
            else
            {
                Load_PlanTabs((int)DateTime.Now.DayOfWeek);
            }
        }
        public async void Load_PlanTabs(int today)
        {
            SetAppControll();
            if (!model.AppControll.showObjektPlans) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);

            var result = await Task.Run(() => { return model.Connections.GetPlanPersons(AppModel.Instance.Person.id); });
            if (result)
            {
                if (AppModel.Instance.PlanResponse.lastCall != null)
                {
                    ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Color.FromHex("#aaaaaa");
                    ObjektPlanWeekMobil_Stack_ABC_text.Text =
                        "Meine Planliste: " + AppModel.Instance.PlanResponse.lastCall.Value.ToString("dd.MM.yyyy - HH:mm");
                }
                else
                {
                    ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Color.Yellow;
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
                ObjektPlanWeekMobil_Stack_ABC_text.TextColor = Color.Yellow;
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
        }

        private void buildFilterFromPlanKategories()
        {
            AppModel.Instance.Plan_ObjekteThisWeek = new List<Int32>();
            AppModel.Instance.Plan_KatThisWeek = new List<Int32>();
            if (AppModel.Instance.AppControll.filterKategories && !AppModel.Instance.AppControll.ignoreKategorieFilterByPerson
                && model.PlanResponse.planweek != null && model.PlanResponse.planweek.days != null)
            {
                model.PlanResponse.planweek.days.ForEach(day =>
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
            var w = App.Current.MainPage.Width - 13;//28

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

                        //frame_planListA.IsVisible = false;
                        plansToday.ForEach(p =>
                        {

                            var objekt = AppModel.Instance.AllBuildings.Find(ob => ob.id == p.objektid);
                            var stack = ObjektPlanWeekMobile.GetPlanedTodayList(p, new Command<IntBoolParam>(SelectedObjektAufterNotScan));
                            var containerA = new StackLayout
                            {
                                Padding = new Thickness(0),
                                Margin = new Thickness(0),
                                Spacing = 0,
                                Orientation = StackOrientation.Vertical,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                Children = { stack },
                                ClassId = p.muelltoid > 0 ? "Muell" : "",
                                IsVisible = AppModel.Instance.AppSetModel.ViewOnlyMuell == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 1 && p.muelltoid == 0 || AppModel.Instance.AppSetModel.ViewOnlyMuell == 2 && p.muelltoid > 0
                            };
                            var containerB = new StackLayout
                            {
                                Padding = new Thickness(0),
                                Margin = new Thickness(0),
                                Spacing = 0,
                                Orientation = StackOrientation.Vertical,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                IsVisible = false
                            };
                            containerA.Children.Add(containerB);
                            var o = new List<Object>(){
                                    containerB,
                                    model,
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
                        //frame_planListA.IsVisible = true;


                        frame_planConA_erl_count_con.IsVisible = plansReady.Count > 0;
                        plansReady.ForEach(p =>
                        {
                            var containerReady = ObjektPlanWeekMobile.GetPlanedReadyTodayList(p);
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
                                    HorizontalOptions = LayoutOptions.FillAndExpand,
                                    BackgroundColor = Color.Transparent,
                                    Children = {
                                    new Label() {
                                        Text = Utils.DaysInUtils[ii] + (ii == today ? " (Heute)" : ""),
                                        TextColor = Color.FromHex("#ffcc00"),
                                        Margin = new Thickness(3, 0, 5, 1),
                                        FontSize = 18,
                                        HorizontalOptions = LayoutOptions.StartAndExpand,
                                        LineBreakMode = LineBreakMode.WordWrap,
                                    }
                                }
                                };
                                frame_planListAc.Children.Add(stdayI);
                                pl.ForEach(p =>
                                {
                                    var build = AppModel.Instance.AllBuildings.Find(ob => ob.id == p.objektid);
                                    var stack = ObjektPlanWeekMobile.GetPlanedTodayList(p, new Command<IntBoolParam>(SelectedObjektAufterNotScan));
                                    var containerA = new StackLayout
                                    {
                                        Padding = new Thickness(0),
                                        Margin = new Thickness(0),
                                        Spacing = 0,
                                        Orientation = StackOrientation.Vertical,
                                        HorizontalOptions = LayoutOptions.FillAndExpand,
                                        Children = { stack },
                                        IsVisible = true
                                    };
                                    var containerB = new StackLayout
                                    {
                                        Padding = new Thickness(0),
                                        Margin = new Thickness(0),
                                        Spacing = 0,
                                        Orientation = StackOrientation.Vertical,
                                        HorizontalOptions = LayoutOptions.FillAndExpand,
                                        IsVisible = false
                                    };
                                    containerA.Children.Add(containerB);
                                    var o = new List<Object>(){
                                    containerB,
                                    model,
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
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                BackgroundColor = Color.Transparent,
                                Children = {
                                new Image
                                {
                                    Margin = new Thickness(0,0,0,0),
                                    HeightRequest = 22,
                                    WidthRequest = 22,
                                    Source = "win_26.png",
                                    HorizontalOptions = LayoutOptions.Start,
                                    VerticalOptions = LayoutOptions.End,
                                },
                                new Label()
                                {
                                    Text = lkWinter[kza][0].katname,
                                    TextColor = Color.FromHex("#ffcc00"),
                                    Margin = new Thickness(4, 10, 5, 1),
                                    FontSize = 18,
                                    HorizontalOptions = LayoutOptions.StartAndExpand,
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
                                    model,
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
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                BackgroundColor = Color.Transparent,
                                Children = {
                                new Label()
                                {
                                    Text = lk[kz][0].katname,
                                    TextColor = Color.FromHex("#ffcc00"),
                                    Margin = new Thickness(5, 10, 5, 1),
                                    FontSize = 18,
                                    HorizontalOptions = LayoutOptions.StartAndExpand,
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
                                        HorizontalOptions = LayoutOptions.FillAndExpand,
                                        Children = { stack },
                                        IsVisible = true
                                    };
                                    var containerB = new StackLayout
                                    {
                                        Padding = new Thickness(0),
                                        Margin = new Thickness(0),
                                        Spacing = 0,
                                        Orientation = StackOrientation.Vertical,
                                        HorizontalOptions = LayoutOptions.FillAndExpand,
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

                    frame_plantabC_badge_col.BackgroundColor = Color.FromHex("#009900");
                    frame_plantabC_badge_count.Text = "0";
                    frame_plantabC_badge.IsVisible = true;

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
            var model = ((value as List<Object>)[0] as AppModel);
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

            popupContainer_quest_direktbuchen.IsVisible = true;
            popupContainer_quest_direktbuchen_st.WidthRequest = Application.Current.MainPage.Width - 20;
            popupContainer_quest_direktbuchen_st.HeightRequest = Application.Current.MainPage.Height - 120;

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
            if (!model.AppControll.showObjektPlans) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);
            bool ok = true;

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

            var lastworker = model.Person.name + " " + (model.Person.vorname.Length > 1 ? (model.Person.vorname.Substring(0, 1) + ".") : model.Person.vorname);
            selectedDirektbuchenWinterObj.haswork = 1;
            selectedDirektbuchenWinterObj.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
            selectedDirektbuchenWinterObj.lastworker = lastworker;
            model.PlanResponse.planweek.days = CleanPlanweekList(model.PlanResponse.planweek.days);
            ObjektPlanWeekMobile.Save(model, model.PlanResponse);

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
            SyncPosition();
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


            if (_SelectedBemerkungForNotice != null && _SelectedBemerkungForNoticeList_DirektPos != null)
            {
                foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                {
                    if (item.id == _SelectedPosForNotice.id)
                    {
                    }
                }
            }


            model.allPositionDirectWork = new LeistungPackWSO
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
                personid = model.Person.id,
                diffObjekt = 2,// Direktbuchung
                leistungen = leisInWork,
                winterservice = 1,
            };
            model.allPositionDirectWork.endticks = model.allPositionDirectWork.startticks;
            addTicksWinter++;

            var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(model.allPositionDirectWork.startticks), -2);
            var building = BuildingWSO.LoadBuilding(AppModel.Instance, leis[0].objektid);
            building.ArrayOfAuftrag.ForEach(o =>
            {
                o.kategorien.ForEach(c =>
                {
                    c.leistungen.ForEach(le =>
                    {
                        var foundPos = model.allPositionDirectWork.leistungen.Find(lei => lei.id == le.id);
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
            BuildingWSO.Save(model, building);

            LeistungPackWSO.ToUploadStack(model, model.allPositionDirectWork);

            model.allPositionDirectWork = null;
            await Task.Delay(1);
        }





        private PlanPersonMobile selectedDirektbuchenObj = null;
        public async void OpenDirektbuchenAusPlanliste(Object value)
        {
            _SelectedBemerkungForNoticeList_DirektPos = new List<IntBemerkungWSOPair>();
            btn_quest_direktbuchen_cancel.IsVisible = true;
            btn_quest_direktbuchenwinter_cancel.IsVisible = false;
            var stack = ((value as List<Object>)[0] as StackLayout);
            var model = ((value as List<Object>)[1] as AppModel);
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

            popupContainer_quest_direktbuchen.IsVisible = true;
            popupContainer_quest_direktbuchen_st.WidthRequest = Application.Current.MainPage.Width - 20;
            popupContainer_quest_direktbuchen_st.HeightRequest = Application.Current.MainPage.Height - 120;

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
            if (!model.AppControll.showObjektPlans) { return; }
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
                    model.PlanResponse.planweek.days = CleanPlanweekList(model.PlanResponse.planweek.days);
                    ObjektPlanWeekMobile.Save(model, model.PlanResponse);
                    if (AppModel.Instance.PlanResponse.selectedPerson != null)
                    {
                        ReloadPlanData(0);
                    }
                    else
                    {
                        var today = (int)DateTime.Now.DayOfWeek;
                        Update_PlanTabs(today);
                    }
                    SyncPosition();
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
                var leiIW = leisIW.FindAll(_ => _.ppm.leiid == lei.id).FirstOrDefault();
                lei.leiInWork = leiIW;
                //lei.leiInWork.ppm = selectedDirektbuchenObj;//PPM

                maxEndMin = Math.Max(maxEndMin, ((lei.dstd * 60) + lei.dmin));

                if (_SelectedBemerkungForNoticeList_DirektPos != null)
                {
                    foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                    {
                        if (item.id == lei.id && item.bem != null)
                        {
                            if (lei.leiInWork.bemerkungen == null) { lei.leiInWork.bemerkungen = new List<BemerkungWSO>(); }
                            lei.leiInWork.bemerkungen.Add(item.bem);
                        }
                    }
                }
                leisInWork.Add(lei.leiInWork);
            });
            var start = DateTime.Now.AddTicks(addTicksWinter);
            var end = start.AddMinutes(maxEndMin);

            try
            {
                if (_SelectedBemerkungForNoticeList_DirektPos != null)
                {
                    foreach (var item in _SelectedBemerkungForNoticeList_DirektPos)
                    {
                        var found = leiss.Find(_ => _.id == item.id);
                        var ppm = selectedDirektbuchenObj.more.FindAll(pp => item.id.ToString() == pp.info.Split('#')[3]).FirstOrDefault();
                        if (ppm != null && item.bem != null && found == null)
                        {
                            // Bemrkung zur Müllpos - JEDOCh nicht selektiert !!!!
                            /// Nur Bemerkung zum Objekt erstellen
                            item.bem.auftragid = 0;
                            item.bem.leistungid = 0;
                            item.bem.text = "LEISTUNG: " + item.lei.beschreibung + " \r\nBEMERKUNG: " + item.bem.text;
                            BemerkungWSO.ToUploadStack(model, item.bem);
                            //btn_NoticeSaveForOnlyObjektOnlyMuellPosThatNotSelected(ppm, item);
                        }
                    }
                }
            }
            catch (Exception) { }


            model.allPositionDirectWork = new LeistungPackWSO
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
                personid = model.Person.id,
                diffObjekt = 2,// Direktbuchung
                leistungen = leisInWork // leisIW
            };
            model.allPositionDirectWork.endticks = model.allPositionDirectWork.startticks;
            addTicks++;

            var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(model.allPositionDirectWork.startticks), -2);
            var building = BuildingWSO.LoadBuilding(AppModel.Instance, leisIW[0].objektid);
            building.ArrayOfAuftrag.ForEach(o =>
            {
                o.kategorien.ForEach(c =>
                {
                    c.leistungen.ForEach(le =>
                    {
                        var foundPos = model.allPositionDirectWork.leistungen.Find(lei => lei.id == le.id);
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
            BuildingWSO.Save(model, building);
            //model.allPositionDirectWork.leistungen = null;
            LeistungPackWSO.ToUploadStack(model, model.allPositionDirectWork);

            leisIW.ForEach(l =>
            {
                int haswork = 1;
                if (l.ppm != null && !String.IsNullOrWhiteSpace(l.ppm.info))
                {
                    string[] all = l.ppm.info.Split('#');
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
                var lastworker = model.Person.name + " " + (model.Person.vorname.Length > 1 ? (model.Person.vorname.Substring(0, 1) + ".") : model.Person.vorname);
                l.ppm.haswork = haswork;
                l.ppm.lastwork = new DateTime(model.allPositionDirectWork.endticks).ToString("dd.MM.yyyy - HH:mm");
                l.ppm.lastworker = lastworker;
            });
            model.allPositionDirectWork = null;
            await Task.Delay(1);
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
            //    _SelectedBemerkungForNotice.gruppeid = model.LastBuilding.gruppeid;
            //    _SelectedBemerkungForNotice.personid = model.Person.id;
            //    _SelectedBemerkungForNotice.objektid = model.LastBuilding.id;
            //    _SelectedBemerkungForNotice.leistungid = 0;
            //    _SelectedBemerkungForNotice.datum = DateTime.Now.Ticks;

            //    if (_SelectedPosForNotice != null)
            //    {
            //        var posInWork = model.allPositionInWork.leistungen.Find(pos => pos.id == _SelectedPosForNotice.id);
            //        if (posInWork.bemerkungen == null) { posInWork.bemerkungen = new List<BemerkungWSO>(); }
            //        _SelectedBemerkungForNotice.leistungid = _SelectedPosForNotice.id;
            //        posInWork.bemerkungen.Add(_SelectedBemerkungForNotice);
            //        LeistungPackWSO.Save(model, model.allPositionInWork);
            //        //LeistungPackWSO.Load(model);
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
                Load_PlanTabs((int)DateTime.Now.DayOfWeek);
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
        public void btn_PlanTabCTapped(object sender, EventArgs e)
        {
            frame_plantabC.Margin = new Thickness(0, -8, 2, 0);
            frame_plantabA.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabB.Margin = new Thickness(0, 0, 2, 0);
            frame_plantabCe.Margin = new Thickness(0, 0, 2, 0);
            frame_planConA.IsVisible = false;
            frame_planConB.IsVisible = false;
            frame_planConCe.IsVisible = false;
            frame_planConC.IsVisible = true;
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
            // MainStettings Menü
            if (!visible)
            {
                await panelContainer_frame.TranslateTo(-this.Width, 0, 200, Easing.Linear);
                panelContainer.IsVisible = visible;
            }
            else
            {
                await panelContainer_frame.TranslateTo(-this.Width, 0, 0);
                panelContainer.IsVisible = visible;
                await panelContainer_frame.TranslateTo(-2, 0, 200, Easing.Linear);
                SetAllSyncState();
            }
        }
        public void SetAllSyncState()
        {
            ShowDisconnected();
            var countAll = GetAllSyncFromUploadCount();
            btn_settings_frame_count.IsVisible = countAll > 0;
            btn_StartPage_frame_count.IsVisible = countAll > 0;
            btn_settings_count.Text = "" + countAll;
            btn_StartPage_count.Text = "" + countAll;
        }
        public void btn_BuildingScanTapped(object sender, EventArgs e)
        {
            ShowBuildingScanPage();
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
            model.SetAllObjectAndValuesToNoSelectedBuilding();
            ShowMainPage();
        }
        public void btn_back_BuildingOrderTapped(object sender, EventArgs e)
        {
            model.LastSelectedOrder = null;
            ShowMainPage();
        }
        public void btn_back_OrderCategoryTapped(object sender, EventArgs e)
        {
            btn_back_inBuildingOrder_category_showall_txt.Text = "Alle zeigen";
            model._showall_OrderCategory = false;
            model.LastSelectedCategory = null;
            ShowOrderPage();
        }
        public void btn_back_CategoryPositionTapped(object sender, EventArgs e)
        {
            model.LastSelectedPosition = null;
            ShowOrderCategoryPage(model.LastSelectedOrder);
        }




        public async void btn_AuswahlAnzeigen(object sender, EventArgs e)
        {
            AuswahlAnzeigenTapped_Done(!panelShowSelectedPos_Container.IsVisible);
        }
        public async void AuswahlAnzeigenTapped_Done(bool visible)
        {
            // MainStettings Menü
            if (!visible)
            {
                await panelShowSelectedPos_frame.TranslateTo(-this.Width, 0, 200, Easing.Linear);
                panelShowSelectedPos_Container.IsVisible = visible;
                selectedPosList_container.Children.Clear();
            }
            else
            {
                await panelShowSelectedPos_frame.TranslateTo(-this.Width, 0, 0);
                panelShowSelectedPos_Container.IsVisible = visible;
                await panelShowSelectedPos_frame.TranslateTo(-2, 0, 200, Easing.Linear);
                selectedPosList_container.Children.Add(LeistungWSO.GetSelectedPositionListView(
                    model, new Command<LeistungWSO>(RemoveSelectPositionFromToWork),
                    new Command<ChangeSelectedMuellPos>(ChangeSelectedMuellPos)));
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
            obj.img.Source = obj.pos.inout.inout == 0 ? model.imagesBase.Muell_OutTonne : model.imagesBase.Muell_In;
            obj.img2.Source = obj.pos.inout.inout == 0 ? model.imagesBase.Muell_Out : model.imagesBase.Muell_InTonne;
            obj.lb.Text = obj.pos.inout.inout == 0 ? "Ich werde RAUSSTELLEN" : "Ich werde REINSTELLEN";
            obj.lb.TextColor = Color.FromHex(obj.pos.inout.inout == 0 ? "#dd0000" : "#00aa00");

            popupContainer_quest_changemuellpos.IsVisible = false;
        }


        public async void btn_AuswahlAnzeigen_Again(object sender, EventArgs e)
        {
            AuswahlAnzeigenTapped_Again_Done(!panelShowSelectedPos_Container.IsVisible);
        }
        public async void AuswahlAnzeigenTapped_Again_Done(bool visible)
        {
            if (!visible)
            {
                await panelShowSelectedPos_frame.TranslateTo(-this.Width, 0, 200, Easing.Linear);
                panelShowSelectedPos_Container.IsVisible = visible;
                selectedPosList_container.Children.Clear();
            }
            else
            {
                await panelShowSelectedPos_frame.TranslateTo(-this.Width, 0, 0);
                panelShowSelectedPos_Container.IsVisible = visible;
                await panelShowSelectedPos_frame.TranslateTo(-2, 0, 200, Easing.Linear);
                selectedPosList_container.Children.Add(LeistungWSO.GetSelectedPositionAgainListView(model, new Command<LeistungWSO>(RemoveSelectPositionAgainFromToWork)));
            }
        }


        public async void StartSelectedPos(object sender, EventArgs e)
        {
            if (model.allSelectedPositionToWork.Count > 0)
            {
                StartSelectedPosTapped_Done();
            }
            else
            {
                if (model.allSelectedPositionAgainToWork.Count > 0)
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

            var isOthersAsProdukte = model.allSelectedPositionToWork.Find(l => l.art != "Produkt");
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

            model.allPositionInWork = new LeistungPackWSO
            {
                latin = latin,
                lonin = lonin,
                messagein = geoMessage,
                preview = true,
                status = 0,   // 0 = in Arbeit , 1 = Ausgesetzt , 2 = Fertig
                startticks = DateTime.Now.Ticks,
                endticks = DateTime.Now.Ticks,
                personid = model.Person.id,
            };
            model.allPositionDirectWork = new LeistungPackWSO
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
                personid = model.Person.id,
            };
            model.allPositionDirectWork.endticks = model.allPositionDirectWork.startticks;

            model.allSelectedPositionToWork.ForEach(l =>
            {
                var work = new LeistungInWorkWSO
                {
                    id = l.id,
                    gruppeid = l.gruppeid,
                    objektid = l.objektid,
                    auftragid = l.auftragid,
                    kategorieid = l.kategorieid,
                    anzahl = Utils.formatDEStr(decimal.Parse(l.produktAnzahl) > 0 ? decimal.Parse(l.produktAnzahl) : 1),
                    bemerkungen = null,
                    inout = l.inout,
                };
                if ((l.type == "1" && l.art == "Leistung") || onlyProdukte)
                {
                    model.allPositionDirectWork.leistungen.Add(work);
                }
                else
                {
                    model.allPositionInWork.leistungen.Add(work);
                }
            });

            //var dummyLeistungInWork = new List<LeistungInWorkWSO>();
            if (model.allPositionDirectWork.leistungen.Count > 0)
            {
                var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(model.allPositionDirectWork.startticks), -2);
                model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(p =>
                        {
                            var foundPos = model.allPositionDirectWork.leistungen.Find(lei => lei.id == p.id);
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
                model.allPositionDirectWork.leistungen.ForEach(l =>
                {
                    newleis.Add(SetPlanPersonMobileToLeistungInWork(l));
                });
                model.allPositionDirectWork.leistungen = newleis;

                BuildingWSO.Save(model, model.LastBuilding);
                LeistungPackWSO.ToUploadStack(model, model.allPositionDirectWork);
                SyncPosition();
                UpdateObjektPersonPlanMobileAfterUpload(model.allPositionDirectWork);
            }

            model.allPositionDirectWork = null;

            if (model.allPositionInWork.leistungen.Count > 0)
            {
                LeistungPackWSO.Save(model, model.allPositionInWork);
                SyncPosition(model.allPositionInWork.preview);
            }
            else
            {
                model.allPositionInWork = null;
            }

            // Zurücksetzten aller States für die Auswahl der Ausführungen
            model.LastSelectedOrder = null;
            model.LastSelectedCategory = null;
            model.LastSelectedPosition = null;
            model.allPositionInShowingListView = new Dictionary<int, Frame>();
            model.allPositionInShowingSmallListView = new Dictionary<int, SwipeView>();
            model.allSelectedPositionToWork = new List<LeistungWSO>();
            // alle selektionen und disabled zurücksetzen 
            model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
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
        public async void StartSelectedPosAgainTapped_Done(object sender = null, EventArgs e = null)
        {
            AuswahlAnzeigenTapped_Done(false);


            model.allSelectedPositionAgainToWork.ForEach(l =>
            {
                var work = new LeistungInWorkWSO
                {
                    id = l.id,
                    gruppeid = l.gruppeid,
                    objektid = l.objektid,
                    auftragid = l.auftragid,
                    kategorieid = l.kategorieid,
                    anzahl = Utils.formatDEStr(decimal.Parse(l.produktAnzahl) > 0 ? decimal.Parse(l.produktAnzahl) : 1),
                    bemerkungen = null,
                    inout = l.inout,
                    again = 1,
                };
                model.allPositionInWork.leistungen.Add(work);
            });
            var dummyLeistungInWork = new List<LeistungInWorkWSO>();

            if (model.allPositionInWork.leistungen.Count > 0)
            {
                LeistungPackWSO.Save(model, model.allPositionInWork);
                SyncPositionAgain();
            }
            else
            {
                model.allPositionInWork = null;
            }

            // Zurücksetzten aller States für die Auswahl der Ausführungen
            model.LastSelectedOrder = null;
            model.LastSelectedCategory = null;
            model.LastSelectedPosition = null;
            model.LastSelectedOrderAgain = null;
            model.LastSelectedCategoryAgain = null;
            model.LastSelectedPositionAgain = null;
            model.allPositionInShowingListView = new Dictionary<int, Frame>();
            model.allPositionInShowingSmallListView = new Dictionary<int, SwipeView>();
            model.allSelectedPositionToWork = new List<LeistungWSO>();

            model.allPositionAgainInShowingListView = new Dictionary<int, Frame>();
            model.allPositionAgainInShowingSmallListView = new Dictionary<int, SwipeView>();
            model.allSelectedPositionAgainToWork = new List<LeistungWSO>();

            // alle selektionen und disabled zurücksetzen 
            model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
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
            if (model.allPositionInWork != null && model.allPositionInWork.leistungen.Count > 0)
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
                model.SetAllObjectAndValuesToNoSelectedBuilding();
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


            if (model.allPositionInWork == null)
            {
                popupContainer_quest_endwork.IsVisible = false;
                await Task.Delay(1);
                overlay.IsVisible = false;
                ShowMainPage();
            }
            else
            {
                model.allPositionInWork.endticks = DateTime.Now.Ticks;
                model.allPositionInWork.latout = latout;
                model.allPositionInWork.lonout = lonout;
                model.allPositionInWork.messageout = geoMessage;
                model.allPositionInWork.preview = false;
                model.allPositionInWork.status = 2;
                model.allPositionInWork.personid = model.Person.id;
                model.allPositionInWork.diffObjekt = isDiffObjekt ? 1 : 0;

                var dummyLeistungInWork = new List<LeistungInWorkWSO>();
                var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(model.allPositionInWork.endticks), -2);
                model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
                {
                    o.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(p =>
                        {
                            var foundPos = model.allPositionInWork.leistungen.Find(lei => lei.id == p.id);
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
                model.allPositionInWork.leistungen.ForEach(l =>
                {
                    newleis.Add(SetPlanPersonMobileToLeistungInWork(l));
                });
                model.allPositionInWork.leistungen = newleis;

                BuildingWSO.Save(model, model.LastBuilding);
                LeistungPackWSO.ToUploadStack(model, model.allPositionInWork); // Beendete Arbeiten in Stacklist für Upload
                LeistungPackWSO.Delete(model);// Aktive Arbeiten aus Datenspeicher löschen

                UpdatePersonPlanMobile(model.allPositionInWork);

                model.allPositionInWork = null;

                if (dummyLeistungInWork.Count > 0)
                {
                    model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
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
                    BuildingWSO.Save(model, model.LastBuilding);
                }
                dummyLeistungInWork = null;

                // Versuche direkt zu senden sonst von Stack!
                SyncPosition();

                // Dialog schliessen
                popupContainer_quest_endwork.IsVisible = false;

                //await Task.Delay(1);
                //overlay.IsVisible = false;
                ShowMainPage();
            }
        }

        public LeistungInWorkWSO SetPlanPersonMobileToLeistungInWork(LeistungInWorkWSO lei)
        {
            if (!model.AppControll.showObjektPlans) { return lei; }
            PlanPersonMobile ppm = null;
            List<PlanPersonMobile> ppms = new List<PlanPersonMobile>(); ;
            if (model.PlanResponse != null && model.PlanResponse.planweek != null && model.PlanResponse.planweek.days != null)
            {
                model.PlanResponse.planweek.days.ForEach(day =>
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
            if (!model.AppControll.showObjektPlans) { return; }
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
                    model.LastBuilding.ArrayOfAuftrag.ForEach(b =>
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
                if (model.PlanResponse.planweek != null)
                {
                    //if (today == 0)// Sonntag 
                    //{
                    //    plans = model.PlanResponse.week.FindAll(p => p.objektid == model.LastBuilding.id && p.haswork == 0 && p.muelltoid == 0);
                    //}
                    //else
                    //{
                    //    plans = model.PlanResponse.week.FindAll(p => p.objektid == model.LastBuilding.id && p.day != 0 && p.day <= today && p.haswork == 0 && p.muelltoid == 0);
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
                            p.lastworker = model.Person.vorname + " " + model.Person.name;
                        }
                        else
                        {
                            // Kategorien nach Bedarf prüfen ob hier gearbeitet wurde
                            if (katNames.IndexOf(p.katname) > -1)
                            {
                                p.haswork = haswork;
                                p.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
                                p.lastworker = model.Person.vorname + " " + model.Person.name;
                            }
                        }
                    });
                }

                ObjektPlanWeekMobile.Save(model, model.PlanResponse);

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
                //var muellObjbPlanWeekMobile = null;// model.PlanResponse.week.Where(p => p.day > -1 && p.katname.Contains("#")).ToList();
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
                        p.lastworker = model.Person.vorname + " " + model.Person.name;
                    });
                    ObjektPlanWeekMobile.Save(model, model.PlanResponse);
                }

                pack.leistungen = holdLeiIds;

                if (pack.leistungen.Count > 0)
                { //Leistungen waren nur Müllpositionen 

                    pack.leistungen.ForEach(l =>
                    {
                        objektid = l.objektid;
                        if (building == null)
                        {
                            building = BuildingWSO.LoadBuilding(model, objektid);
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
                    if (model.PlanResponse.planweek != null)
                    {
                        //if (today == 0)// Sonntag 
                        //{
                        //    plans = model.PlanResponse.week.FindAll(p => p.objektid == objektid && p.haswork == 0);
                        //}
                        //else
                        //{
                        //    plans = model.PlanResponse.week.FindAll(p => p.objektid == objektid && p.day != 0 && p.day <= today && p.haswork == 0);
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
                                    p.lastworker = model.Person.vorname + " " + model.Person.name;
                                }
                            }
                            else
                            {
                                if (p.day > -1)
                                {
                                    p.haswork = haswork;
                                    p.lastwork = new DateTime(model.allPositionInWork.endticks).ToString("dd.MM.yyyy - HH:mm");
                                    p.lastworker = model.Person.vorname + " " + model.Person.name;
                                }
                                else
                                {
                                    // Kategorien nach Bedarf prüfen ob hier gearbeitet wurde
                                    if (katNames.IndexOf(p.katname) > -1)
                                    {
                                        p.haswork = haswork;
                                        p.lastwork = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
                                        p.lastworker = model.Person.vorname + " " + model.Person.name;
                                    }
                                }
                            }
                        });
                        ObjektPlanWeekMobile.Save(model, model.PlanResponse);
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



        public void ShowClearLog(object sender, EventArgs e)
        {
            popupContainer_container_clearlog.WidthRequest = App.Current.MainPage.Width - 40;
            btn_clearlogtosupport.GestureRecognizers.Clear();
            var tgr_over = new TapGestureRecognizer();
            tgr_over.Tapped -= btn_nlogclearTapped;
            tgr_over.Tapped += btn_nlogclearTapped;
            btn_clearlogtosupport.GestureRecognizers.Add(tgr_over);

            btn_cancelclearlogtosupport.GestureRecognizers.Clear();
            var tgr_cancel = new TapGestureRecognizer();
            tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_clearlog.IsVisible = false; };
            tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_clearlog.IsVisible = false; };
            btn_cancelclearlogtosupport.GestureRecognizers.Add(tgr_cancel);

            // Dialog öffnen
            popupContainer_quest_clearlog.IsVisible = true;
        }

        public async void btn_nlogclearTapped(object sender, EventArgs e)
        {
            btn_settings_clearlog.IsEnabled = false;
            btn_settings_clearlog.Opacity = 0.4;
            overlay.IsVisible = true;
            await Task.Delay(1);

            popupContainer_quest_clearlog.IsVisible = false;
            await Task.Delay(1);

            model.ClearLog();
            await Task.Delay(1000);
            overlay.IsVisible = false;
            btn_settings_clearlog.Opacity = 1;
            btn_settings_clearlog.IsEnabled = true;
            btn_settings_clearlog.IsVisible = false;
        }



        public void ShowSendLog(object sender, EventArgs e)
        {
            popupContainer_container_sendlog.WidthRequest = App.Current.MainPage.Width - 40;
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
            popupContainer_container_sendlog_fail.WidthRequest = App.Current.MainPage.Width - 40;
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
            btn_settings_sendlog.IsEnabled = false;
            btn_settings_sendlog.Opacity = 0.4;
            overlay.IsVisible = true;
            await Task.Delay(1);

            popupContainer_quest_sendlog.IsVisible = false;
            await Task.Delay(1);

            var ok = model.SendLogZipFile();
            await Task.Delay(2000);
            if (ok)
            {
                overlay.IsVisible = false;
                btn_settings_sendlog.Opacity = 1;
                btn_settings_sendlog.IsEnabled = true;
                btn_settings_sendlog.IsVisible = false;
            }
            else
            {
                btn_settings_sendlog.Opacity = 1;
                btn_settings_sendlog.IsEnabled = true;
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
            ShowObjectValuesView();
        }
        public void btn_ShowNoticeTapped(object sender, EventArgs e)
        {
            btn_alertmessage_tit.Text = "Bemerkung";
            sw_alertmessage.IsVisible = false;
            sw_alertmessage.IsToggled = false;
            btn_alertmessage_img2.IsVisible = false;
            sw_internmessage.IsToggled = false;
            btn_internmessage_img2.IsVisible = false;
            ShowNoticeView(false, null, null);
        }
        public void btn_ShowNoticePrioTapped(object sender, EventArgs e)
        {
            btn_alertmessage_tit.Text = "Störmeldung";
            sw_alertmessage.IsVisible = false;
            sw_alertmessage.IsToggled = true;
            btn_alertmessage_img2.IsVisible = true;
            sw_internmessage.IsToggled = false;
            btn_internmessage_img2.IsVisible = false;
            ShowNoticeView(true, null, null);
        }
        public void btn_NoticeBackTapped(object sender, EventArgs e)
        {
            this.Focus();

            entry_notice.Text = "";
            noticePhotoStack.Children.Clear();
            _SelectedPosForNotice = null;
            _SelectedBemerkungForNotice = null;
            _BackToFromNotice = null;
            if (_BackToFromNotice != null && _BackToFromNotice == "inwork")
            {
                ShowRunningWorksView();
            }
            else
            {
                ShowMainPage();
            }
        }
        private void AlertMessage_Switch_Toggled(object sender, ToggledEventArgs e)
        {
            btn_alertmessage_img2.IsVisible = e.Value;
        }
        private void InternMessage_Switch_Toggled(object sender, ToggledEventArgs e)
        {
            btn_internmessage_img2.IsVisible = e.Value;
        }
        public async void btn_NoticeSaveTapped(object sender, EventArgs e)
        {
            this.Focus();
            if (!String.IsNullOrWhiteSpace(_SelectedBemerkungForNotice.text.Trim()) || (_SelectedBemerkungForNotice.photos != null && _SelectedBemerkungForNotice.photos.Count > 0))
            {
                overlay.IsVisible = true;
                await Task.Delay(1);

                int am = sw_alertmessage.IsToggled ? 2 : 0;
                int im = sw_internmessage.IsToggled ? 1 : 0;
                _SelectedBemerkungForNotice.prio = (am + im);
                _SelectedBemerkungForNotice.gruppeid = model.LastBuilding.gruppeid;
                _SelectedBemerkungForNotice.personid = model.Person.id;
                _SelectedBemerkungForNotice.objektid = model.LastBuilding.id;
                _SelectedBemerkungForNotice.leistungid = 0;
                _SelectedBemerkungForNotice.datum = DateTime.Now.Ticks;

                if (_SelectedPosForNotice != null)
                {
                    var posInWork = model.allPositionInWork.leistungen.Find(pos => pos.id == _SelectedPosForNotice.id);
                    if (posInWork.bemerkungen == null) { posInWork.bemerkungen = new List<BemerkungWSO>(); }
                    _SelectedBemerkungForNotice.leistungid = _SelectedPosForNotice.id;
                    posInWork.bemerkungen.Add(_SelectedBemerkungForNotice);
                    LeistungPackWSO.Save(model, model.allPositionInWork);
                    //LeistungPackWSO.Load(model);
                }
                else
                {
                    BemerkungWSO.ToUploadStack(model, _SelectedBemerkungForNotice);
                    //Task.Run(() => { 
                    SyncSingleNotice();
                    //}).ConfigureAwait(false);   // Im Hintergrund ausführen
                }


                await Task.Delay(1);

                _SelectedPosForNotice = null;
                _SelectedBemerkungForNotice = null;
                _BackToFromNotice = null;
                entry_notice.Text = "";
                noticePhotoStack.Children.Clear();

                await Task.Delay(1);
                overlay.IsVisible = false;

                if (_BackToFromNotice != null && _BackToFromNotice == "inwork")
                {
                    ShowRunningWorksView();
                }
                else
                {
                    ShowMainPage();
                }
            }
        }

        public void btn_DSGVOBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }


        public async void TranslateTo(string s)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            lb_settings_sel_translist.IsVisible = false;
            if (s.Split(',')[0] == "de")
            {
                lb_settings_sel_trans.Text = "Deutsch";
            }
            else
            {
                lb_settings_sel_trans.Text = "" + s.Split(',')[1];
            }
            btn_settings_sel_trans_lang_txt.Text = "ÄNDERN";//s.Split(',')[1]

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        public void OpenLanguage(object sender, EventArgs e)
        {
            langListView.SelectedItem = null;
            popupContainer_quest_langpicker_inner.HeightRequest = App.Current.MainPage.Height - 100;
            popupContainer_quest_langpicker_inner.WidthRequest = App.Current.MainPage.Width - 40;
            popupContainer_quest_langpicker.IsVisible = true;

            var empList = AppModel.Instance.Langs;
            var groupedData =
                empList.OrderBy(el => el.text)
                    .GroupBy(el => el.text[0].ToString())
                    .Select(el => new ObservableLangItemCollection<string, Lang>(el))
                    .ToList();

            BindingContext = new ObservableCollection<ObservableLangItemCollection<string, Lang>>(groupedData);

        }
        private void langListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                var l = (Lang)e.SelectedItem;
                AppModel.Instance.Lang = l;
                CloseLanguage();
                ShowTranslate(null, null);
            }
        }
        public async void CloseLanguage()
        {
            await Task.Delay(1);
            popupContainer_quest_langpicker.IsVisible = false;
        }

        public void ShowTranslate(object sender, EventArgs e)
        {
            popupContainer_container_changelang.WidthRequest = App.Current.MainPage.Width - 40;
            //popupContainer_container_changelang.Margin = new Thickness(0,100,0,0);
            btn_changelang.GestureRecognizers.Clear();
            var tgr_over = new TapGestureRecognizer();
            tgr_over.Tapped -= btn_translateTapped;
            tgr_over.Tapped += btn_translateTapped;
            btn_changelang.GestureRecognizers.Add(tgr_over);

            btn_cancellang.GestureRecognizers.Clear();
            var tgr_cancel = new TapGestureRecognizer();
            tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_changelang.IsVisible = false; };
            tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_changelang.IsVisible = false; };
            btn_cancellang.GestureRecognizers.Add(tgr_cancel);

            popupContainer_container_changelang_titel.Text = "Kategorien und Leistungen ändern in (" + AppModel.Instance.Lang.text.Replace("(Standard)", "") + ")";

            // Dialog öffnen
            popupContainer_quest_changelang.IsVisible = true;
        }
        public void ShowTranslate_fail()
        {
            popupContainer_quest_changelang_fail.WidthRequest = App.Current.MainPage.Width - 40;
            //popupContainer_container_sendlog.Margin = new Thickness(0,100,0,0);
            btn_cancellogtosupport_fail.GestureRecognizers.Clear();
            var tgr_cancel = new TapGestureRecognizer();
            tgr_cancel.Tapped -= (object o, TappedEventArgs ev) => { popupContainer_quest_changelang_fail.IsVisible = false; };
            tgr_cancel.Tapped += (object o, TappedEventArgs ev) => { popupContainer_quest_changelang_fail.IsVisible = false; };
            btn_cancellogtosupport_fail.GestureRecognizers.Add(tgr_cancel);

            // Dialog öffnen
            popupContainer_quest_changelang_fail.IsVisible = true;
        }

        public async void btn_translateTapped(object sender, EventArgs e)
        {
            try
            {
                overlay.IsVisible = true;
                await Task.Delay(1);

                if (AppModel.Instance.Lang.lang.ToLower() == "de")
                {
                    // fertig
                    popupContainer_container_changelang_status.Text = "";
                    popupContainer_quest_changelang.IsVisible = false;

                    //Lang.Save(AppModel.Instance.Lang);
                    lb_settings_sel_trans.Text = AppModel.Instance.Lang.text.Replace("(Standard)", "");

                    await Task.Delay(1000);
                    overlay.IsVisible = false;
                    return;
                }

            }
            catch (Exception ex)
            {
                popupContainer_quest_changelang.IsVisible = false;
                AppModel.Logger.Error("Error: Sprache zu DE (btn_translateTapped) - " + ex.Message + "");
                await Task.Delay(1);
                overlay.IsVisible = false;
                ShowTranslate_fail();
                return;
            }

            //translate/Buildings(AppModel.Instance.AllBuildings, true);
        }

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

                var req = new TransRequest { list = new List<TransListItem>(), to = model.Lang.lang };
                buildings.ForEach(b =>
                {
                    var lb = BuildingWSO.LoadBuilding(AppModel.Instance, b.id);
                    //Vergleiche firsches Objekt vom Backend mit gespeichertem Objekt wenn vorhanden
                    if (b.del == 0)
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
                                });
                            }
                        });
                    }
                });

                var alist = ListExtensions.ChunkBy(als.Distinct().ToList(), 100);
                var klist = ListExtensions.ChunkBy(kls.Distinct().ToList(), 100);
                var llist = ListExtensions.ChunkBy(lls.Distinct().ToList(), 100);
                //countFrom = klist.Count + llist.Count;
                popupContainer_container_changelang_status.Text = "" + countReady + " von " + countFrom;
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
                                });
                            }
                        });
                        BuildingWSO.Save(AppModel.Instance, b);
                        popupContainer_container_changelang_status.Text = "" + countReady + " von " + countFrom;
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
                    SyncTransSigns();
                }

                popupContainer_container_changelang_status.Text = "" + countReady + " von " + countFrom;
                popupContainer_quest_changelang.IsVisible = false;

                //Lang.Save(AppModel.Instance.Lang);
                lb_settings_sel_trans.Text = lang; // AppModel.Instance.Lang.text.Replace("(Standard)", "");

                await Task.Delay(1000);
                overlay.IsVisible = false;
                return true;
            }
            catch (Exception ex)
            {
                popupContainer_quest_changelang.IsVisible = false;
                AppModel.Logger.Error("Error: (translateBuildings) - " + ex.Message + "");
                await Task.Delay(1);
                overlay.IsVisible = false;
                ShowTranslate_fail();
                return false;
            }
        }




        public async void btn_settings_synctimesub_Tapped(object sender, EventArgs e)
        {
            if (model.SettingModel.SettingDTO.SyncTimeHours == 0) { return; }
            model.SettingModel.SettingDTO.SyncTimeHours--;
            model.SettingModel.SaveSettings();
            lb_settings_synctimehours.Text = "" + model.SettingModel.SettingDTO.SyncTimeHours;
        }
        public async void btn_settings_synctimeadd_Tapped(object sender, EventArgs e)
        {
            if (model.SettingModel.SettingDTO.SyncTimeHours == 15) { return; }
            model.SettingModel.SettingDTO.SyncTimeHours++;
            model.SettingModel.SaveSettings();
            lb_settings_synctimehours.Text = "" + model.SettingModel.SettingDTO.SyncTimeHours;
        }

        public void btn_DayOverBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            ShowMainPage();
        }
        public async void btn_DayOverYesTapped(object sender, EventArgs e)
        {
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
            //AppModel.Logger.Info("Info: --------------- FEIERABEND => btn_DayOverYesTapped");
            //AppModel.Logger.Info("Info: Verwendete GPS (" + geoMessage + " - " + AppModel.Instance.LocationStr + ")");

            var latin = geo != null ? geo.Split(';')[0] : "";
            var lonin = geo != null ? (geo.Split(';').Length > 0 ? geo.Split(';')[1] : "") : "";

            var d = new DayOverWSO
            {
                endticks = DateTime.Now.Ticks,
                latin = latin,
                lonin = lonin,
                messagein = geoMessage,
                personid = model.Person.id,
                gruppeid = model.Person.gruppeid,
            };
            DayOverWSO.Save(model, d);
            DayOverWSO.ToUploadStack(model, d);
            SyncDayOver();
            var dt = new DateTime(d.endticks);
            dayOverLastDate.Text = dt.ToString("dd.MM.yyyy") + " - " + dt.ToString("HH:mm");

            if (model.LastBuilding != null)
            {
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                model.SetAllObjectAndValuesToNoSelectedBuilding();
            }
            ShowMainPage();
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


        public async void btn_takePhotoForMeterstand(object sender, EventArgs e)
        {
            notizSave_stack.IsVisible = false;
            await Task.Delay(1);

            try
            {
                model.UseExternHardware = true;
                var photo = await MediaGallery.CapturePhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    var photoResponse = PhotoUtils.GetImages(stream);
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, AppModel.Instance.LastBuilding);

                    model.selectedObjectValueBild = new ObjektDatenBildWSO { bytes = photoResponse.imageBytes };

                    img_photo_objectvaluesbild.Source = photoResponse.GetImageSourceAsThumb();
                    await Task.Delay(1);

                    btn_send_objectvaluesbild.IsVisible = true;

                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    model.UseExternHardware = false;
                }
            }
            catch (OperationCanceledException)
            {
                // handling a cancellation request
            }
            catch (Exception)
            {
                // handling other exceptions
            }
            finally
            {
                model.UseExternHardware = false;
            }

        }
        public async void RemoveObjektMeterStandBild()
        {
            img_photo_objectvaluesbild.Source = null;
            await Task.Delay(1);
            model.selectedObjectValueBild = null;

            btn_send_objectvaluesbild.IsVisible = false;
            await Task.Delay(1);
            btn_send_objectvaluesbild_err.Opacity = 0;
        }
        public async void btn_sendPhotoForMeterstand(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            if (AppModel.Instance.isFlashLigthAloneON)
            {
                model.Scan.Btn_FlashlightAloneTapped(null, null);
            }
            await Task.Delay(1);

            model.selectedObjectValueBild.filename = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
            model.selectedObjectValueBild.bemerkung = editor_notice_objectvaluesbild.Text;
            model.selectedObjectValueBild.meterid = model.selectedObjectValue.id;
            model.selectedObjectValueBild.lastchange = JavaScriptDateConverter.Convert(DateTime.Now).ToString();
            model.selectedObjectValueBild.standid = 0;

            ObjektDatenBildWSO.ToUploadStack(model, model.selectedObjectValueBild);

            await Task.Delay(1);
            SyncObjectValueBild();

            RemoveObjektMeterStandBild();
            popupContainer_objectvaluesbild.IsVisible = false;

            await Task.Delay(1);
            overlay.IsVisible = false;
        }


        public async void btn_takePhoto(object sender, EventArgs e)
        {
            if (_SelectedBemerkungForNotice.photos.Count > 4) { return; }
            notizSave_stack.IsVisible = false;
            await Task.Delay(1);

            try
            {
                model.UseExternHardware = true;
                var photo = await MediaGallery.CapturePhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    var photoResponse = PhotoUtils.GetImages(stream);
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, AppModel.Instance.LastBuilding);

                    long bildName = DateTime.Now.Ticks;
                    var b = new BildWSO(_SelectedBemerkungForNotice.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = "" + bildName,
                        stack = BildWSO.GetAttachmentForNoticeElement(
                            photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                            new Command<BildWSO>(RemoveBildInWork))
                    };
                    var frame = (Frame)((StackLayout)(b.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork), CommandParameter = b });

                    BildWSO.Save(AppModel.Instance, b);
                    _SelectedBemerkungForNotice.photos.Add(b);
                    noticePhotoStack.Children.Add(b.stack);

                    CheckNoticeFalid();

                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    model.UseExternHardware = false;
                    stream.Dispose();
                }
            }
            catch (OperationCanceledException ex)
            {
                // handling a cancellation request
            }
            catch (Exception ex)
            {
                // handling other exceptions
            }
            finally
            {
                model.UseExternHardware = false;
            }

        }
        public async void btn_pickPhotos(object sender, EventArgs e)
        {
            //Bemerkung (+/-intern)
            if (_SelectedBemerkungForNotice.photos.Count > 4) { return; }

            notizSave_stack.IsVisible = false;
            await Task.Delay(1);

            try
            {
                var cts = new CancellationTokenSource();
                IMediaFile[] files = null;

                model.UseExternHardware = true;
                var request = new MediaPickRequest(5 - _SelectedBemerkungForNotice.photos.Count, MediaFileType.Image)
                {
                    //PresentationSourceBounds = System.Drawing.Rectangle.Empty,
                    UseCreateChooser = false,
                    Title = "Select"
                };

                cts.CancelAfter(TimeSpan.FromMinutes(5));

                var results = await MediaGallery.PickAsync(request, cts.Token);
                files = results?.Files?.ToArray();

                if (files == null)
                    return;

                foreach (var fil in files)
                {
                    var stream = await fil.OpenReadAsync();

                    overlay.IsVisible = true;
                    await Task.Delay(1);

                    var photoResponse = PhotoUtils.GetImages(stream);
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, AppModel.Instance.LastBuilding);

                    long bildName = DateTime.Now.Ticks;
                    var b = new BildWSO(_SelectedBemerkungForNotice.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = "" + bildName,
                        stack = BildWSO.GetAttachmentForNoticeElement(
                            photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"), new Command<BildWSO>(RemoveBildInWork))
                    };
                    var frame = (Frame)((StackLayout)(b.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command<BildWSO>(RemoveBildInWork), CommandParameter = b });

                    BildWSO.Save(AppModel.Instance, b);
                    _SelectedBemerkungForNotice.photos.Add(b);
                    noticePhotoStack.Children.Add(b.stack);
                }
            }
            catch (Exception ex)
            {
                var a = ex;
            }
            finally
            {
                model.UseExternHardware = false;
            }



            CheckNoticeFalid();

            await Task.Delay(1);
            overlay.IsVisible = false;


        }
        public async void RemoveBildInWork(BildWSO b)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            noticePhotoStack.Children.Remove(b.stack);
            await Task.Delay(1);
            BildWSO.Delete(AppModel.Instance, b);
            await Task.Delay(1);
            _SelectedBemerkungForNotice.photos.Remove(b);
            CheckNoticeFalid();

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        private bool _manuelTextChange = false;
        private void entry_notice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_SelectedBemerkungForNotice != null && !_manuelTextChange)
            {
                _manuelTextChange = true;
                //entry_notice.Text = System.Text.RegularExpressions.Regex.Replace(e.NewTextValue, @"\p{Cs}", "").Trim(); 
                //_SelectedBemerkungForNotice.text = System.Text.RegularExpressions.Regex.Replace(e.NewTextValue, @"\p{Cs}", "").Trim();
                _SelectedBemerkungForNotice.text = e.NewTextValue;
                CheckNoticeFalid();
                _manuelTextChange = false;
            }
        }

        private void CheckNoticeFalid()
        {
            if (_SelectedBemerkungForNotice != null)
            {
                notizSave_stack.IsVisible = !String.IsNullOrWhiteSpace(_SelectedBemerkungForNotice.text) || _SelectedBemerkungForNotice.photos.Count > 0;
            }
            else
            {
                notizSave_stack.IsVisible = false;
            }
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
            entry_workersearch_container.IsVisible = false;
            workerSelectedViewIndex = 1;
            btn_workercategorysearch.BackgroundColor = Color.FromHex("#999999");
            btn_workernamesearch.BackgroundColor = Color.FromHex("#042d53");
            btn_workerbuildingsearch.BackgroundColor = Color.FromHex("#042d53");
            list_worker_scroll.ScrollToAsync(0, 0, false);
            await Task.Delay(1);
            list_worker.Children.Clear();
            BuildWorkerCategoryList();
        }
        private async void BuildWorkerCategoryList()
        {
            if (list_worker.Children != null && list_worker.Children.Count > 0) { return; }
            workerCategories = new Dictionary<string, List<PersonWSO>>();
            workerCategoriesElements = new Dictionary<string, Object>();
            model.AllWorkers.ForEach(ha =>
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
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    ClassId = ("##" + item.Key).ToLower()
                };
                var mainSubStack = new StackLayout()
                {
                    Padding = new Thickness(6, 0, 0, 0),
                    Margin = new Thickness(0, -5, 10, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    IsVisible = false,
                    ClassId = "" + item.Key,
                };


                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => { _CategoryCommand(s, e); };
                Frame sfb = Elements.GetWorkerCategoryTreeItem(item.Key, "" + item.Value.Count, model.imagesBase.Tools, null);
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
            var childs = ((StackLayout)((Frame)s).Content).Children;
            var category = ((Label)((StackLayout)childs[0]).Children[1]).Text;
            //var container = (StackLayout)childs[1];
            var parentChilds = ((StackLayout)((Frame)s).Parent).Children;
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
                    model.AllWorkers.ForEach(ha =>
                    {
                        if (category == ha.kategorie)
                        {
                            var sfbgf = Elements.GetWorkerTreeItem(ha, model.imagesBase.Worker, null, _navigationCommand);
                            //sfbgf.ClassId = ("bu_" + ha.firma + "," + ha.name + "," + ha.vorname + "," + ha.strasse + "," + ha.plz + "," + ha.ort + "," + ha.kategorie).ToLower();
                            sfbgf.IsVisible = true;
                            sfbgf.HorizontalOptions = LayoutOptions.FillAndExpand;
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
                ((StackLayout)((Frame)item.Value).Parent).Children[1].IsVisible = false;
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
            btn_workercategorysearch.BackgroundColor = Color.FromHex("#042d53");
            btn_workernamesearch.BackgroundColor = Color.FromHex("#999999");
            btn_workerbuildingsearch.BackgroundColor = Color.FromHex("#042d53");
            lb_workerbuildingsearche.Text = "Handwerker suchen:";
            entry_workersearch.Text = "";
            await Task.Delay(1);
            list_worker.Children.Clear();
            list_worker_scroll.ScrollToAsync(0, 0, false);
            BuildWorkerNamesList();
        }
        private async void BuildWorkerNamesList()
        {
            if (list_worker.Children != null && workerNames.Count > 0 && list_worker.Children.Count == workerNames.ToList().Count) { return; }

            workerNames = new Dictionary<string, PersonWSO>();
            workerNamesElements = new Dictionary<string, Object>();

            var workers = model.AllWorkers.OrderBy(o => (String.IsNullOrEmpty(o.firma) ? o.name : o.firma)).ToList();
            workers.ForEach(ha => { workerNames["" + ha.id] = ha; });
            var i = 0;
            workerNames.ToList().ForEach(item =>
            {

                var mainVertStack = new StackLayout()
                {
                    Padding = new Thickness(6, 0, 0, 0),
                    Margin = new Thickness(0, 5, 0, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    ClassId = ("##" + (String.IsNullOrEmpty(item.Value.firma) ? item.Value.name : item.Value.firma) + ";" + item.Value.strasse + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.kategorie).ToLower()
                };
                var mainSubStack = new StackLayout()
                {
                    Padding = new Thickness(6, 0, 0, 0),
                    Margin = new Thickness(6, -5, 10, 0),
                    Spacing = 0,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    IsVisible = false,
                    ClassId = "" + item.Value.id,
                    BackgroundColor = Color.FromHex("#144d73"),
                };

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => { _NamesCommand(s, e); };
                Frame sfb = Elements.GetWorkerNamesTreeItem(item.Value, model.imagesBase.Worker, null);
                sfb.GestureRecognizers.Clear();
                sfb.GestureRecognizers.Add(tapGestureRecognizer);
                sfb.ClassId = ("##" + (String.IsNullOrEmpty(item.Value.firma) ? item.Value.name : item.Value.firma) + ";" + item.Value.strasse + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.kategorie).ToLower();
                workerNamesElements.Add(item.Key, sfb);
                mainVertStack.Children.Add(sfb);
                mainVertStack.Children.Add(mainSubStack);
                list_worker.Children.Add(mainVertStack);
            });
            entry_workersearch_container.IsVisible = true;
            await Task.Delay(1);
            list_worker.IsVisible = true;
            overlay.IsVisible = false;
        }
        private void _NamesCommand(object s, EventArgs e)
        {
            var parentChilds = ((StackLayout)((Frame)s).Parent).Children;
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
                    model.AllWorkers.ForEach(ha =>
                    {
                        if (workerid == ("" + ha.id))
                        {
                            var sfbgf = Elements.GetWorkerDetailsTreeItem(ha, model.imagesBase.Worker, null, _navigationCommand);
                            sfbgf.IsVisible = true;
                            sfbgf.HorizontalOptions = LayoutOptions.FillAndExpand;
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
                ((StackLayout)((Frame)item.Value).Parent).Children[1].IsVisible = false;
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
            btn_workercategorysearch.BackgroundColor = Color.FromHex("#042d53");
            btn_workernamesearch.BackgroundColor = Color.FromHex("#042d53");
            btn_workerbuildingsearch.BackgroundColor = Color.FromHex("#999999");
            entry_workersearch.Text = "";
            await Task.Delay(1);
            list_worker.Children.Clear();
            list_worker_scroll.ScrollToAsync(0, 0, false);
            BuildWorkerBuildingList();
        }
        private async void BuildWorkerBuildingList()
        {
            if (list_worker.Children != null && workerBuildings.Count > 0 && list_worker.Children.Count == workerBuildings.ToList().Count) { return; }

            workerBuildings = new Dictionary<string, BuildingWSO>();
            workerBuildingsElements = new Dictionary<string, Object>();

            var buildings = model.AllBuildings.OrderBy(o => o.plz + o.ort + o.strasse + o.hsnr).ToList();
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
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        ClassId = ("bu_" + item.Value.strasse + ";" + item.Value.hsnr + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.objektname + ";" + item.Value.objektnr).ToLower()
                    };
                    var mainSubStack = new StackLayout()
                    {
                        Padding = new Thickness(6, 0, 0, 0),
                        Margin = new Thickness(0, -5, 10, 0),
                        Spacing = 0,
                        Orientation = StackOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        IsVisible = false,
                        ClassId = "" + item.Value.id,
                    };

                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) => { WorkerBuildingCommand(s, e); };
                    Frame sfb = Elements.GetWorkerBuildingTreeItem(item.Value, model.imagesBase.Building, null);
                    sfb.GestureRecognizers.Clear();
                    sfb.GestureRecognizers.Add(tapGestureRecognizer);
                    sfb.ClassId = ("bu_" + item.Value.strasse + ";" + item.Value.hsnr + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.objektname + ";" + item.Value.objektnr).ToLower();
                    var tapGestureRecognizerInfo = new TapGestureRecognizer();
                    tapGestureRecognizerInfo.Tapped += (s, e) => { AppModel.Instance.MainPage.OpenBuildingInfoDialog(item.Value); };
                    Frame sfbb = Elements.GetWorkerBuildingTreeInfoItem(item.Value, sfb, tapGestureRecognizerInfo);
                    sfbb.ClassId = ("bu_" + item.Value.strasse + ";" + item.Value.hsnr + ";" + item.Value.plz + ";" + item.Value.ort + ";" + item.Value.objektname + ";" + item.Value.objektnr).ToLower();
                    workerBuildingsElements.Add(item.Key, sfb);
                    mainVertStack.Children.Add(sfbb);
                    mainVertStack.Children.Add(mainSubStack);
                    list_worker.Children.Add(mainVertStack);
                }
            });
            await Task.Delay(1);
            entry_workersearch_container.IsVisible = true;
            list_worker.IsVisible = true;
            overlay.IsVisible = false;
        }
        private void WorkerBuildingCommand(object s, EventArgs e)
        {
            //var parentChilds = ((StackLayout)((Frame)s).Parent).Children;
            var parentChilds1 = ((Frame)s).Parent;
            var parentChilds2 = ((StackLayout)parentChilds1).Parent;
            var parentChilds3 = ((Frame)parentChilds2).Parent;
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
                    model.AllBuildings.Find(b => ("" + b.id) == buildingid).ArrayOfHandwerker.ForEach(ha =>
                    {
                        var sfbgf = Elements.GetWorkerTreeItem(ha, model.imagesBase.Worker, null, _navigationCommand);
                        sfbgf.IsVisible = true;
                        sfbgf.HorizontalOptions = LayoutOptions.FillAndExpand;
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
                var parentChilds1 = ((Frame)item.Value).Parent;
                var parentChilds2 = ((StackLayout)parentChilds1).Parent;
                var parentChilds3 = ((Frame)parentChilds2).Parent;
                var parentChilds = ((StackLayout)parentChilds3).Children;
                var container = (StackLayout)parentChilds[1];
                container.IsVisible = false;
            });
        }

        public void btn_WorkerBackTapped(object sender, EventArgs e)
        {
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
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            await Task.Delay(1);
            list_persontimes_scroll.ScrollToAsync(0, 0, false);

            pick_persontimes_year.Items.Clear();
            pick_persontimes_year.Items.Add(DateTime.Now.ToString("yyyy"));
            pick_persontimes_year.Items.Add(DateTime.Now.AddYears(-1).ToString("yyyy"));
            pick_persontimes_year.Items.Add(DateTime.Now.AddYears(-2).ToString("yyyy"));
            pick_persontimes_year.SelectedItem = DateTime.Now.ToString("yyyy");
            pick_persontimes_month.SelectedItem = DateTime.Now.ToString("MMMM");

            PersonTimesPage_Container.IsVisible = true;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        private async void pick_persontimes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitialize) { return; }
            overlay.IsVisible = true;
            await Task.Delay(1);
            list_persontimes.Children.Clear();
            stack_persontimes_top.Children.Clear();
            stack_persontimes_top.Children.Add(PersonWSO.GetPersonTimesViewHeaderItem());
            stack_persontimes_bottom.Children.Clear();

            PersonTimeResponse pts = await Task.Run(() =>
            {
                return model.Connections.GetPersonTimes(int.Parse(pick_persontimes_year.SelectedItem.ToString()), pick_persontimes_month.SelectedIndex + 1);
            });

            if (!pts.success)
            {
                await DisplayAlert("Abruf nicht möglich!", pts.message, "OK");
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
                        bool isMinus = false;
                        if (pt.dauer.Contains("--:--")) { pt.dauer = "00:00"; }
                        if (pt.dauer.Contains("-")) { isMinus = true; }
                        var da = pt.dauer.Split(':');
                        var damin = (int.Parse(da[0].Replace("-", "")) * 60) + (int.Parse(da[1]));
                        if (isMinus) { damin = damin * -1; }
                        allMin += damin;
                    });
                }
                list_persontimes.Children.Add(PersonWSO.GetPersonTimesView(pts.times));
                if (pts.times != null && pts.times.Count > 0)
                {
                    allHours = int.Parse("" + (allMin / 60));
                    allMins = allMin - (allHours * 60);
                    pts.times[0].all = (allHours > 9 ? ("" + allHours) : ("0" + allHours)) + ":" + (allMins > 9 ? ("" + allMins) : ("0" + allMins));
                    stack_persontimes_bottom.Children.Add(PersonWSO.GetPersonTimesViewAllItem(pts.times[0]));
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
        public void btn_PersontimesBackTapped(object sender, EventArgs e)
        {
            list_persontimes.Children.Clear();
            list_persontimes_scroll.ScrollToAsync(0, 0, false);
            this.Focus();
            ShowMainPage();
        }

        // BTN Todos

        private int _holdLastTodoList = 0;
        public int _holdLastTodoPage = 1;
        public int _holdLastTodoPageMax = 1;
        private async void TodoRangeSlider_DragCompleted(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);
            if (!String.IsNullOrWhiteSpace(entry_todosearch.Text))
            {
                entry_todosearch.Text = "";
            }
            RangeSlider slider = (sender as RangeSlider);
            list_todo.IsVisible = false;
            list_todo.Children.Clear();
            list_todo_scroll.ScrollToAsync(0, 0, false);
            BuildTodoList(_holdLastTodoList);
        }
        public async void btn_TodosTapped(object sender, EventArgs e)
        {
            // Handwerker Liste
            MainMenuTapped_Done(false);
            await Task.Delay(210);
            ShowTodoPage();
        }
        private async void ShowTodoPage()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            ClearPageViews();
            TodoPage_Container.IsVisible = true;
            btn_todo_faelligTapped(null, null);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public void btn_TodoBackTapped(object sender, EventArgs e)
        {
            list_todo.Children.Clear();
            this.Focus();
            ShowMainPage();
        }
        public void btn_NotScanBackTapped(object sender, EventArgs e)
        {
            list_notscan.Children.Clear();
            this.Focus();
            ShowMainPage();
        }
        public async void btn_todo_allTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            entry_todosearch.Text = "";
            entry_todosearch_lbb.Text = "";
            entry_todosearch_container.IsVisible = true;
            entry_todosearch_stepcontainer.IsVisible = true;
            btn_todo_faellig.BackgroundColor = Color.FromHex("#53042d");
            btn_todo_all.BackgroundColor = Color.FromHex("#999999");
            btn_todo_inout.BackgroundColor = Color.FromHex("#04532d");
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoList = 1;
            _holdLastTodoPage = 1;
            BuildTodoList(1);
        }
        public async void btn_todo_faelligTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            entry_todosearch.Text = "";
            entry_todosearch_lbb.Text = "";
            entry_todosearch_stepcontainer.IsVisible = true;
            entry_todosearch_container.IsVisible = true;
            Update_Todopaging(_holdLastTodoPage, _holdLastTodoPageMax);
            btn_todo_faellig.BackgroundColor = Color.FromHex("#999999");
            btn_todo_all.BackgroundColor = Color.FromHex("#042d53");
            btn_todo_inout.BackgroundColor = Color.FromHex("#04532d");
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoList = 0;
            _holdLastTodoPage = 1;
            BuildTodoList(0);
        }
        public async void btn_todo_faelligprevTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoPage--;
            if (_holdLastTodoPage < 1) { _holdLastTodoPage = 1; return; }
            BuildTodoList(_holdLastTodoList);
        }
        public async void btn_todo_faellignextTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            await Task.Delay(1);
            list_todo.Children.Clear();
            await list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoPage++;
            if (_holdLastTodoPage > _holdLastTodoPageMax) { _holdLastTodoPage = _holdLastTodoPageMax; return; }
            BuildTodoList(_holdLastTodoList);
        }
        public async void btn_todo_inoutTapped(object sender, EventArgs e)
        {
            overlay.IsVisible = true;
            list_todo.IsVisible = false;
            entry_todosearch.Text = "";
            entry_todosearch_lbb.Text = "";
            entry_todosearch_container.IsVisible = true;
            entry_todosearch_stepcontainer.IsVisible = true;
            btn_todo_faellig.BackgroundColor = Color.FromHex("#53042d");
            btn_todo_all.BackgroundColor = Color.FromHex("#042d53");
            btn_todo_inout.BackgroundColor = Color.FromHex("#999999");
            await Task.Delay(1);
            list_todo.Children.Clear();
            list_todo_scroll.ScrollToAsync(0, 0, false);
            _holdLastTodoList = 2;
            _holdLastTodoPage = 1;
            BuildTodoList(2);
        }
        public async void BuildTodoList(int all = 0)
        {
            if (String.IsNullOrWhiteSpace(entry_todosearch.Text))
            {
                entry_todosearch.Text = "";
            }

            list_todo.Children.Add(AuftragWSO.GetOrderTodoListView(model, all, overlay, entry_todosearch.Text));
            await Task.Delay(1);
            list_todo.IsVisible = true;
            overlay.IsVisible = false;
        }
        private void Entry_todosearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _holdLastTodoPage = 1;
            list_todo.Children.Clear();
            list_todo_scroll.ScrollToAsync(0, 0, false);
            BuildTodoList(1);
        }
        public void Update_Todopaging(int page, int maxpage)
        {
            btn_todo_faellig_count.Text = "Seite " + page + " von " + maxpage;
        }




        public void btn_showall_again_OrderCategoryTapped(object sender, EventArgs e)
        {
            model._showall_again_OrderCategory = !model._showall_again_OrderCategory;
            btn_back_inBuildingOrder_category_showall_again_txt.Text = model._showall_again_OrderCategory ? "Meine zeigen" : "Alle zeigen";

            buildingorderlist_category_container_Again.Children.Clear();
            buildingorderlist_category_container_Again.Children.Add(KategorieWSO.GetCategoryAgainListView(model, new Command<KategorieWSO>(SelectCategoryAgain)));
            BuildingOrderPage_category_Container_Again.IsVisible = true;
        }

        public async void btn_nachbuchen_Tapped(int pos)
        {
            model.posAgain = pos;
            overlay.IsVisible = true;
            model.LastSelectedCategoryAgain = null;
            model.LastSelectedPositionAgain = null;
            btn_nachbuchen_Pos.BackgroundColor = pos == 0 ? Color.FromHex("#042d53") : Color.FromHex("#999999");
            btn_nachbuchen_Produkte.BackgroundColor = pos == 0 ? Color.FromHex("#999999") : Color.FromHex("#042d53");
            await Task.Delay(1);
            buildingorderlist_category_scroll_Again.ScrollToAsync(0, 0, false);
            BuildNachbuchenList();
        }

        public async void BuildNachbuchenList()
        {
            model.LastSelectedOrderAgain = null;
            model.LastSelectedCategoryAgain = null;
            model.LastSelectedPositionAgain = null;

            BuildingOrderPage_category_Container_Again.IsVisible = true;
            BuildingOrderPage_position_Container_Again.IsVisible = false;
            inBuildingOrder_position_stack_Again.IsVisible = false;

            LeistungWSO firstLeistungInWork = null;
            model.IsOptionalPosAgain = false;
            var selOrderId = -1;
            if (model.allPositionInWork != null && model.allPositionInWork.leistungen != null && model.allPositionInWork.leistungen.Count > 0)
            {
                model.allPositionInWork.leistungen.ForEach(liw =>
                {
                    model.LastBuilding.ArrayOfAuftrag.ForEach(a =>
                    {
                        a.kategorien.ForEach(k =>
                        {
                            if (firstLeistungInWork == null)
                            {
                                firstLeistungInWork = k.leistungen.Find(l => l.art == "Leistung" && liw.id == l.id);
                            }
                        });
                    });
                });
                model.IsOptionalPosAgain = firstLeistungInWork != null && firstLeistungInWork.nichtpauschal == 1;
                var first = model.allPositionInWork.leistungen.First();
                if (first != null) { selOrderId = first.auftragid; } else { selOrderId = -1; }
            }
            model.LastBuilding.ArrayOfAuftrag.ForEach(o =>
            {
                if (o.id == selOrderId || selOrderId < 0)
                {
                    model.LastSelectedOrderAgain = o;
                    lb_inBuildingOrder_categorypos_text_Again.Text = o.GetMobileText();// + " \nNr.: " + o.id + "  Typ: " + o.typ;
                    lb_inBuildingOrder_position_text_Again.Text = "";
                }
            });

            ShowOrderCategoryAgainPage(model.LastSelectedOrderAgain);

            await Task.Delay(1);
            list_nachbuchen.IsVisible = true;
            overlay.IsVisible = false;
        }

        private async void ShowOrderCategoryAgainPage(AuftragWSO order)
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BuildingOrderPage_position_Container_Again.IsVisible = false;
            inBuildingOrder_position_stack_Again.IsVisible = false;

            buildingorderlist_category_container_Again.Children.Clear();
            buildingorderlist_category_container_Again.Children.Add(KategorieWSO.GetCategoryAgainListView(model, new Command<KategorieWSO>(SelectCategoryAgain)));
            BuildingOrderPage_category_Container_Again.IsVisible = true;

            model.LastSelectedCategoryAgain = null;
            model.LastSelectedPositionAgain = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void SelectCategoryAgain(KategorieWSO category)
        {
            model.LastSelectedCategoryAgain = category;
            BuildingOrderPage_category_Container_Again.IsVisible = false;
            inBuildingOrder_position_stack_Again.IsVisible = true;
            lb_inBuildingOrder_categorypos_text_Again.Text = model.LastSelectedOrderAgain.GetMobileText();// + " \nNr.: " + model.LastSelectedOrderAgain.id + "  Typ: " + model.LastSelectedOrderAgain.typ;
            lb_inBuildingOrder_position_text_Again.Text = category.GetMobileText();
            ShowOrderPositionPageAgain();
        }
        private async void ShowOrderPositionPageAgain()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);


            buildingorderlist_position_container_Again.Children.Clear();
            buildingorderlist_position_container_Again.Children.Add(LeistungWSO.GetPositionAgainListView(model, new Command<LeistungWSO>(SelectPositionToWorkAgain)));
            BuildingOrderPage_position_Container_Again.IsVisible = true;

            model.LastSelectedPositionAgain = null;

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public async void SelectPositionToWorkAgain(LeistungWSO position)
        {


            //if(model.IsOptionalPosAgain == false) { }
            //bool inWork = false;
            //if (model.allPositionInWork != null)
            //{
            //    var foundInWork = model.allPositionInWork.leistungen.Find(l => l.id == position.id);
            //    inWork = foundInWork != null;
            //}
            //if (position.disabled || inWork) { return; }

            overlay.IsVisible = true;
            //await Task.Delay(1);

            model.LastSelectedPositionAgain = position;
            Frame framePos = null;
            var selPost = model.allSelectedPositionAgainToWork.Find(p => p.id == position.id);
            if (selPost != null)
            {
                // entfernen da schon selectiert 
                model.allSelectedPositionAgainToWork.Remove(position);
                if (model.allPositionAgainInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = false;
                    framePos.Content = LeistungWSO.GetPositionCardView(position, model, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            else
            {
                // hinzufügen
                model.allSelectedPositionAgainToWork.Add(position);
                if (model.allPositionAgainInShowingListView.TryGetValue(position.id, out framePos))
                {
                    position.selected = true;
                    framePos.Content = LeistungWSO.GetSelectedPositionCardView(position, model, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
                }
            }
            btn_showselected_pos_container_Again.IsVisible = model.allSelectedPositionAgainToWork.Count > 0;
            btn_showselected_pos_container_not_Again.IsVisible = !(model.allSelectedPositionAgainToWork.Count > 0);
            btn_showselected_pos_container2.IsVisible = model.allSelectedPositionAgainToWork.Count > 0;
            CheckForOptionalToWorkAgain();

            //await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void RemoveSelectPositionAgainFromToWork(LeistungWSO position)
        {
            overlay.IsVisible = true;
            //await Task.Delay(1);

            Frame framePos;
            SwipeView swipePos;
            // entfernen da schon selectiert 
            model.allSelectedPositionAgainToWork.Remove(position);
            position.selected = false;
            if (model.allPositionAgainInShowingListView.TryGetValue(position.id, out framePos))
            {
                framePos.Content = LeistungWSO.GetPositionCardView(position, model, ((TapGestureRecognizer)framePos.Content.GestureRecognizers[0]).Command).Content;
            }
            if (model.allPositionAgainInShowingSmallListView.TryGetValue(position.id, out swipePos))
            {
                swipePos.IsVisible = false;
            }

            btn_showselected_pos_container_Again.IsVisible = model.allSelectedPositionAgainToWork.Count > 0;
            btn_showselected_pos_container_not_Again.IsVisible = !(model.allSelectedPositionAgainToWork.Count > 0);
            btn_showselected_pos_container2.IsVisible = model.allSelectedPositionAgainToWork.Count > 0;
            CheckForOptionalToWorkAgain();
            if (model.allSelectedPositionAgainToWork.Count == 0)
            {
                await Task.Delay(100);
                AuswahlAnzeigenTapped_Done(false);
                //await Task.Delay(100);
                //if (BuildingOrderPage_order_Container.IsVisible)
                //{
                //    buildingorderlist_order_container.Children.Clear();
                //    buildingorderlist_order_container.Children.Add(AuftragWSO.GetOrderListView(model, new Command<AuftragWSO>(SelectOrder)));
                //}
            }
            await Task.Delay(1);
            overlay.IsVisible = false;
        }
        public async void CheckForOptionalToWorkAgain()
        {
            if (model.IsOptionalPosAgain)
            {
                lb_PosSelectionType_text_Again.Text = "Nur optionale Positionen und Produkte aktiv!";
            }
            else
            {
                lb_PosSelectionType_text_Again.Text = "Nur geplante Positionen und Produkte aktiv!";
            }
        }






        private void Entry_workersearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (list_worker.Children.Count > 0)
            {
                foreach (var stack in list_worker.Children)
                {
                    if (stack.ClassId != null && stack.ClassId.Length > 1 && stack.ClassId.Substring(0, 2) == "##")
                    {
                        stack.IsVisible = stack.ClassId.ToLower().Contains(e.NewTextValue.ToLower());
                    }
                    else if (stack.ClassId != null && stack.ClassId.Length > 2 && stack.ClassId.Substring(0, 3) == "bu_")
                    {
                        stack.IsVisible = stack.ClassId.ToLower().Contains(e.NewTextValue.ToLower());
                    }
                }
            }
        }
        private void WorkerListLayoutRefresh()
        {
            if (list_worker.Children.Count > 0)
            {
                foreach (var stack in list_worker.Children)
                {
                    stack.IsVisible = true;
                }
            }
        }












        public void InitStartPageHandlers()
        {
            //btn_regScanWarn_img.Source = imagesBase.AlertMessage;

            //Zählerfoto
            btn_newphoto_objectvaluesbild_img.Source = model.imagesBase.Cam;

            frame_planConA_img_reloadx.Source = "muellInOutX" + AppModel.Instance.AppSetModel.ViewOnlyMuell + ".png";

            popupContainer_quest_personpicker_img.Source = model.imagesBase.Worker;
            frame_planConA_img_down.Source = model.imagesBase.DropDownImage;
            frame_planConA_img_reload.Source = model.imagesBase.Refresh;
            frame_planConA_img_otherperson.Source = model.imagesBase.Worker;
            frame_planConA_img_reload2.Source = model.imagesBase.Refresh;
            frame_planConA_img_otherperson2.Source = model.imagesBase.Worker;
            frame_planConCe_img_reload2.Source = model.imagesBase.Refresh;
            frame_planConCe_img_LoadAll1.Source = model.imagesBase.Refresh;
            frame_planConCe_img_LoadAll2.Source = model.imagesBase.Refresh;

            //Top Buttons
            btn_objScan_limg.Source = model.imagesBase.QrScan;
            btn_objScan_limgB.Source = model.imagesBase.QrScan;
            btn_objScan_rimg.Source = model.imagesBase.DropRightImage;
            btn_objNotScan_limg.Source = model.imagesBase.SearchImage;
            btn_objNotScan_rimg.Source = model.imagesBase.DropRightImage;
            btn_mainsettings_img.Source = model.imagesBase.MenuImage;
            btn_mainmenu_back.Source = model.imagesBase.XImageBoldRed;
            btn_panelShowSelectedPos_back.Source = model.imagesBase.XImageBoldRed;

            //Bottom Buttons
            btn_worker_limg.Source = model.imagesBase.Worker;
            btn_worker_rimg.Source = model.imagesBase.DropRightImage;
            btn_exitwork_limg.Source = model.imagesBase.Exit;
            btn_todos_limg.Source = model.imagesBase.Todos;
            btn_todos_rimg.Source = model.imagesBase.DropRightImage;
            btn_persontimes_limg.Source = model.imagesBase.Time;
            btn_sync_limg.Source = model.imagesBase.Refresh;
            btn_sync_rimg.Source = model.imagesBase.DropRightImage;
            btn_regist_limg.Source = model.imagesBase.QrScan;
            btn_regist_rimg.Source = model.imagesBase.DropRightImage;
            btn_dsgvo_limg.Source = model.imagesBase.Logo;
            btn_dsgvo_rimg.Source = model.imagesBase.DropRightImage;

            btn_settings_limg.Source = model.imagesBase.Setting;
            btn_settings_rimg.Source = model.imagesBase.DropRightImage;

            // LastBuilding Buttons and init showing
            btn_objektinfo_img.Source = model.imagesBase.InfoCircle;

            btn_alertmessage_img.Source = model.imagesBase.WarnTriangleYellow;
            btn_alertmessage_img2.Source = model.imagesBase.WarnTriangleYellow;
            btn_internmessage_img2.Source = model.imagesBase.InternalNoCustomer;
            //btn_alertmessage_img_DirektPos.Source = model.imagesBase.WarnTriangleYellow;
            btn_alertmessage_img2_DirektPos.Source = model.imagesBase.WarnTriangleYellow;
            btn_internmessage_img2_DirektPos.Source = model.imagesBase.InternalNoCustomer;
            btn_message_img.Source = model.imagesBase.CamMessage;
            btn_objvalues_img.Source = model.imagesBase.ObjectValues;
            btn_buildingout_img2.Source = model.imagesBase.BuildingOut;

            // LoginPerson and Version 
            lb_LoginUser.Text = model.Person.anrede + " " + (String.IsNullOrWhiteSpace(model.Person.vorname) ? "" : (model.Person.vorname.Length > 0 ? model.Person.vorname.Substring(0, 1) + ". " : "")) + model.Person.name;
            lb_version.Text = "V" + model.Version; //+ " (" + model.Build + ")";
            if (model.Companies.Count > -1)
            {
                lb_LoginCustomer.IsVisible = true;
                lb_LoginCustomer.Text = model.SettingModel.SettingDTO.CustomerName.Length > 30 ? (model.SettingModel.SettingDTO.CustomerName.Substring(0, 30) + "...") : model.SettingModel.SettingDTO.CustomerName;
            }
            else
            {
                lb_LoginCustomer.IsVisible = false;
            }

            frm_img_LoginUser.IsVisible = false;
            if (model.Person.userIcon != null)
            {
                ImageSource userIconImageSource = ImageSource.FromStream(() => new MemoryStream(model.Person.userIcon));
                img_LoginUser.Source = userIconImageSource;
                frm_img_LoginUser.IsVisible = true;
            }


            // WorkerPage Buttons
            btn_worker_search_img.Source = model.imagesBase.SearchImage;
            btn_workercategorysearch_img.Source = model.imagesBase.Tools;
            btn_workernamesearch_img.Source = model.imagesBase.Worker;
            btn_workerbuildingsearch_img.Source = model.imagesBase.Building;
            btn_worker_back_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            // BuildingScan 
            img_backBtn_inBuildingScan.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_flashlight_img.Source = model.imagesBase.Flashlight;
            btn_regScan_limg.Source = model.imagesBase.QrScan;
            btn_flashlight_Out_img.Source = model.imagesBase.Flashlight;
            btn_regOutScan_limg.Source = model.imagesBase.QrScan;
            img_backBtn_inBuildingOutScan.Source = model.imagesBase.DropLeftBlueDoubleImage;


            // BuildingOrder 
            img_backBtn_inBuildingOrder.Source = model.imagesBase.DropLeftBlueDoubleImage;
            img_backBtn_inBuildingOrder_category.Source = model.imagesBase.DropLeftBlueDoubleImage;
            img_backBtn_inBuildingOrder_position.Source = model.imagesBase.DropLeftBlueDoubleImage;
            img_inBuildingOrder_category_text.Source = model.imagesBase.OrderFolderTools;
            img_inBuildingOrder_categorypos_text.Source = model.imagesBase.OrderFolderTools;
            img_inBuildingOrder_position_text.Source = model.imagesBase.KategorieSymbol;
            img_inBuildingOrder_categorypos_text_Again.Source = model.imagesBase.OrderFolderTools;
            img_inBuildingOrder_position_text_Again.Source = model.imagesBase.KategorieSymbol;
            btn_buildingorder_limg.Source = model.imagesBase.Work;
            btn_buildingorder_rimg.Source = model.imagesBase.DropRightImage;

            //nachbuchen
            btn_posnachbuchen_limg.Source = model.imagesBase.LeistungSymbol;
            btn_produktenachbuchen_limg.Source = model.imagesBase.ProduktSymbol;
            btn_nachbuchen_img.Source = model.imagesBase.AddArrow;
            btn_nachbuchen_rimg.Source = model.imagesBase.DropRightImage;
            btn_nachbuchen_back_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_nachbuchen_cat_back_img.Source = model.imagesBase.DropLeftWhiteDoubleImage;
            btn_showselected_pos_img_Again.Source = model.imagesBase.PosList;

            // InWork
            btn_inwork_limg.Source = model.imagesBase.WorkerInProgressWarn;
            btn_showselected_pos_img.Source = model.imagesBase.PosList;
            btn_showselected_pos_img2.Source = model.imagesBase.PosList;
            //btn_showselected_pos_img3.Source = model.imagesBase.InfoImage;
            btn_showselected_poslist_img.Source = model.imagesBase.PosList;
            btn_startselected_pos_img.Source = model.imagesBase.CheckWhite;

            //RunningWorks
            btn_back_runningworks_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_runningworks_over_img.Source = model.imagesBase.CheckWhite;

            //Bemerkung
            btn_back_check_bem_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_message_imgA_check_bem.Source = model.imagesBase.Cam;
            btn_message_imgB_check_bem.Source = model.imagesBase.Attachment;
            btn_notice_save_img_check_bem.Source = model.imagesBase.CheckWhite;

            btn_back_notice_img_DirektPos.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_back_notice_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_notice_save_img_DirektPos.Source = model.imagesBase.CheckWhite;
            btn_notice_save_img.Source = model.imagesBase.CheckWhite;
            btn_message_imgA_DirektPos.Source = model.imagesBase.Cam;
            btn_message_imgB_DirektPos.Source = model.imagesBase.Attachment;
            btn_message_imgA.Source = model.imagesBase.Cam;
            btn_message_imgB.Source = model.imagesBase.Attachment;

            // ObjectValues
            btn_back_ObjectValues_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_back_ObjectValues_edit_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            //objectValues_edit_img.Source = model.imagesBase.Pen;

            //Feierabend
            btn_back_dayover_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            //CheckContainer
            btn_back_check_del_img.Source = model.imagesBase.Trash;
            btn_back_check_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_info_check_img.Source = model.imagesBase.CheckSymbol;
            btn_back_check_signature_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            //Einstellungen
            btn_back_settings_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            //Map
            btn_back_map_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            //DSGVO
            btn_back_dsgvo_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            //DSGVO
            btn_back_pn_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            //Todo
            btn_todo_back_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_todosearch_img.Source = model.imagesBase.SearchImage;
            btn_todo_inout2_img.Source = model.imagesBase.Muell_Sign;
            btn_todo_inout_img.Source = model.imagesBase.Muell_Out;

            //NotScan
            btn_notscan_back_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            btn_notscansearch_img.Source = model.imagesBase.SearchImage;

            //Sendlogfile Fail
            popupContainer_container_sendlog_fail_img.Source = model.imagesBase.WarnTriangleYellow;

            //ifondialog
            popupContainer_infodialog_img.Source = model.imagesBase.InfoCircle;


            //Persontimes
            SetAppControll();
            btn_persontimes_back_img.Source = model.imagesBase.DropLeftBlueDoubleImage;
            warn_persontimes_limg.Source = model.imagesBase.InfoCircle;

            // Einstellungen Defaults
            lb_settings_synctimehours.Text = "" + model.SettingModel.SettingDTO.SyncTimeHours;

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
            tgr_btn_takePhoto_check_bem.Tapped += btn_takePhoto_check_bem;
            btn_takePhoto_frame_check_bem.GestureRecognizers.Add(tgr_btn_takePhoto_check_bem);
            btn_takePhotoAttachment_frame_check_bem.GestureRecognizers.Clear();
            var tgr_btn_takePhotoAttachment_check_bem = new TapGestureRecognizer();
            tgr_btn_takePhotoAttachment_check_bem.Tapped += btn_pickPhotos_check_bem;
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
            frame_plantabA.GestureRecognizers.Clear();
            var t_frame_plantabA = new TapGestureRecognizer();
            t_frame_plantabA.Tapped += btn_PlanTabATapped;
            frame_plantabA.GestureRecognizers.Add(t_frame_plantabA);
            frame_plantabB.GestureRecognizers.Clear();
            var t_frame_plantabB = new TapGestureRecognizer();
            t_frame_plantabB.Tapped += btn_PlanTabBTapped;
            frame_plantabB.GestureRecognizers.Add(t_frame_plantabB);
            frame_plantabCe.GestureRecognizers.Clear();
            var t_frame_plantabCe = new TapGestureRecognizer();
            t_frame_plantabCe.Tapped += btn_PlanTabCeTapped;
            frame_plantabCe.GestureRecognizers.Add(t_frame_plantabCe);
            frame_plantabC.GestureRecognizers.Clear();
            var t_frame_plantabC = new TapGestureRecognizer();
            t_frame_plantabC.Tapped += btn_PlanTabCTapped;
            frame_plantabC.GestureRecognizers.Add(t_frame_plantabC);


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

            popupContainer_quest_langpicker_close.GestureRecognizers.Clear();
            var t_popupContainer_langpicker_close = new TapGestureRecognizer();
            t_popupContainer_langpicker_close.Tapped += (object o, TappedEventArgs ev) => { CloseLanguage(); };
            popupContainer_quest_langpicker_close.GestureRecognizers.Add(t_popupContainer_langpicker_close);

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

            btn_back_settings.GestureRecognizers.Clear();
            var tgr_back_settings = new TapGestureRecognizer();
            tgr_back_settings.Tapped += btn_SettingsBackTapped;
            btn_back_settings.GestureRecognizers.Add(tgr_back_settings);


            btn_back_map.GestureRecognizers.Clear();
            var tgr_back_map = new TapGestureRecognizer();
            tgr_back_map.Tapped += btn_MapBackTapped;
            btn_back_map.GestureRecognizers.Add(tgr_back_map);

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

            btn_todo_back.GestureRecognizers.Clear();
            var tgr_TodoBack = new TapGestureRecognizer();
            tgr_TodoBack.Tapped += btn_TodoBackTapped;
            btn_todo_back.GestureRecognizers.Add(tgr_TodoBack);

            btn_notscan_back.GestureRecognizers.Clear();
            var tgr_NotScanBack = new TapGestureRecognizer();
            tgr_NotScanBack.Tapped += btn_NotScanBackTapped;
            btn_notscan_back.GestureRecognizers.Add(tgr_NotScanBack);

            btn_persontimes_back.GestureRecognizers.Clear();
            var tgr_PersontimesBack = new TapGestureRecognizer();
            tgr_PersontimesBack.Tapped += btn_PersontimesBackTapped;
            btn_persontimes_back.GestureRecognizers.Add(tgr_PersontimesBack);

            btn_persontime_load.GestureRecognizers.Clear();
            var tgr_PersontimesLoad = new TapGestureRecognizer();
            tgr_PersontimesLoad.Tapped += pick_persontimes_SelectedIndexChanged;
            btn_persontime_load.GestureRecognizers.Add(tgr_PersontimesLoad);

            btn_todo_all.GestureRecognizers.Clear();
            var tgr_todo_all = new TapGestureRecognizer();
            tgr_todo_all.Tapped += btn_todo_allTapped;
            btn_todo_all.GestureRecognizers.Add(tgr_todo_all);
            btn_todo_inout.GestureRecognizers.Clear();
            var tgr_todo_inout = new TapGestureRecognizer();
            tgr_todo_inout.Tapped += btn_todo_inoutTapped;
            btn_todo_inout.GestureRecognizers.Add(tgr_todo_inout);

            btn_todo_faellig.GestureRecognizers.Clear();
            var tgr_todo_faellig = new TapGestureRecognizer();
            tgr_todo_faellig.Tapped += btn_todo_faelligTapped;
            btn_todo_faellig.GestureRecognizers.Add(tgr_todo_faellig);

            btn_todo_faellig_prev.GestureRecognizers.Clear();
            var tgr_btn_todo_faellig_prev = new TapGestureRecognizer();
            tgr_btn_todo_faellig_prev.Tapped += btn_todo_faelligprevTapped;
            btn_todo_faellig_prev.GestureRecognizers.Add(tgr_btn_todo_faellig_prev);

            btn_todo_faellig_next.GestureRecognizers.Clear();
            var tgr_btn_todo_faellig_next = new TapGestureRecognizer();
            tgr_btn_todo_faellig_next.Tapped += btn_todo_faellignextTapped;
            btn_todo_faellig_next.GestureRecognizers.Add(tgr_btn_todo_faellig_next);


            popupContainer_Alert_btn.GestureRecognizers.Clear();
            var tgr9 = new TapGestureRecognizer();
            tgr9.Tapped -= HideAlertMessage;
            tgr9.Tapped += HideAlertMessage;
            popupContainer_Alert_btn.GestureRecognizers.Add(tgr9);

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
            btn_back_inBuildingOutScan.GestureRecognizers.Clear();
            var tgr_back_inBuildingOutScan = new TapGestureRecognizer();
            tgr_back_inBuildingOutScan.Tapped += btn_back_BuildingOutScanTapped;
            btn_back_inBuildingOutScan.GestureRecognizers.Add(tgr_back_inBuildingOutScan);
            btn_flashlight_Out_container.GestureRecognizers.Clear();
            var tapGestureRecognizer1b = new TapGestureRecognizer();
            tapGestureRecognizer1b.Tapped += model.Scan.Btn_FlashlightTapped;
            btn_flashlight_Out_container.GestureRecognizers.Add(tapGestureRecognizer1b);

            // BuidlingScan Back to MainPage
            btn_back_inBuildingScan.GestureRecognizers.Clear();
            var tgr_back_inBuildingScan = new TapGestureRecognizer();
            tgr_back_inBuildingScan.Tapped += btn_back_BuildingScanTapped;
            btn_back_inBuildingScan.GestureRecognizers.Add(tgr_back_inBuildingScan);
            btn_flashlight_container.GestureRecognizers.Clear();
            var tapGestureRecognizer1 = new TapGestureRecognizer();
            tapGestureRecognizer1.Tapped += model.Scan.Btn_FlashlightTapped;
            btn_flashlight_container.GestureRecognizers.Add(tapGestureRecognizer1);

            //Flashlight in ObjektValuesEdit ...
            btn_newphoto_objectvaluesbild.GestureRecognizers.Clear();
            var tgr_btn_newphoto_objectvaluesbild = new TapGestureRecognizer();
            tgr_btn_newphoto_objectvaluesbild.Tapped += btn_takePhotoForMeterstand;
            btn_newphoto_objectvaluesbild.GestureRecognizers.Add(tgr_btn_newphoto_objectvaluesbild);
            btn_send_objectvaluesbild.GestureRecognizers.Clear();
            var tgr_btn_send_objectvaluesbild = new TapGestureRecognizer();
            tgr_btn_send_objectvaluesbild.Tapped += btn_sendPhotoForMeterstand;
            btn_send_objectvaluesbild.GestureRecognizers.Add(tgr_btn_send_objectvaluesbild);
            btn_cancel_objectvaluesbild.GestureRecognizers.Clear();
            var tgr_btn_cancel_objectvaluesbild = new TapGestureRecognizer();
            tgr_btn_cancel_objectvaluesbild.Tapped -= (object o, TappedEventArgs ev) => { RemoveObjektMeterStandBild(); popupContainer_objectvaluesbild.IsVisible = false; };
            tgr_btn_cancel_objectvaluesbild.Tapped += (object o, TappedEventArgs ev) => { RemoveObjektMeterStandBild(); popupContainer_objectvaluesbild.IsVisible = false; };
            btn_cancel_objectvaluesbild.GestureRecognizers.Add(tgr_btn_cancel_objectvaluesbild);

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
            btn_back_notice.GestureRecognizers.Clear();
            var tgr_back_notice = new TapGestureRecognizer();
            tgr_back_notice.Tapped += btn_NoticeBackTapped;
            btn_back_notice.GestureRecognizers.Add(tgr_back_notice);
            btn_notice_save.GestureRecognizers.Clear();
            var tgr_back_notice_save = new TapGestureRecognizer();
            tgr_back_notice_save.Tapped += btn_NoticeSaveTapped;
            btn_notice_save.GestureRecognizers.Add(tgr_back_notice_save);

            btn_takePhoto_frame.GestureRecognizers.Clear();
            var tgr_btn_takePhoto = new TapGestureRecognizer();
            tgr_btn_takePhoto.Tapped += btn_takePhoto;
            btn_takePhoto_frame.GestureRecognizers.Add(tgr_btn_takePhoto);
            btn_takePhotoAttachment_frame.GestureRecognizers.Clear();
            var tgr_btn_takePhotoAttachment = new TapGestureRecognizer();
            tgr_btn_takePhotoAttachment.Tapped += btn_pickPhotos;
            btn_takePhotoAttachment_frame.GestureRecognizers.Add(tgr_btn_takePhotoAttachment);


            /*btn_alertmessage_container_DirektPos.GestureRecognizers.Clear();
            var tgr_alertmessage_container_DirektPos = new TapGestureRecognizer();
            tgr_alertmessage_container_DirektPos.Tapped += btn_ShowNoticePrioTapped;
            btn_alertmessage_container_DirektPos.GestureRecognizers.Add(tgr_alertmessage_container_DirektPos);
            btn_message_container_DirektPos.GestureRecognizers.Clear();
            var tgr_message_container_DirektPos = new TapGestureRecognizer();
            tgr_message_container_DirektPos.Tapped += btn_ShowNoticeTapped;
            btn_message_container_DirektPos.GestureRecognizers.Add(tgr_message_container_DirektPos);*/
            btn_back_notice_DirektPos.GestureRecognizers.Clear();
            var tgr_back_notice_DirektPos = new TapGestureRecognizer();
            tgr_back_notice_DirektPos.Tapped += btn_NoticeBackTapped_DirektPos;
            btn_back_notice_DirektPos.GestureRecognizers.Add(tgr_back_notice_DirektPos);

            btn_notice_save_DirektPos.GestureRecognizers.Clear();
            var tgr_back_notice_save_DirektPos = new TapGestureRecognizer();
            tgr_back_notice_save_DirektPos.Tapped += btn_NoticeSaveTapped_DirektPos;
            btn_notice_save_DirektPos.GestureRecognizers.Add(tgr_back_notice_save_DirektPos);

            btn_notice_del_DirektPos.GestureRecognizers.Clear();
            var tgr_back_notice_del_DirektPos = new TapGestureRecognizer();
            tgr_back_notice_del_DirektPos.Tapped += btn_NoticeDelTapped_DirektPos;
            btn_notice_del_DirektPos.GestureRecognizers.Add(tgr_back_notice_del_DirektPos);

            btn_takePhoto_frame_DirektPos.GestureRecognizers.Clear();
            var tgr_btn_takePhoto_DirektPos = new TapGestureRecognizer();
            tgr_btn_takePhoto_DirektPos.Tapped += btn_takePhoto_DirektPos;
            btn_takePhoto_frame_DirektPos.GestureRecognizers.Add(tgr_btn_takePhoto_DirektPos);
            btn_takePhotoAttachment_frame_DirektPos.GestureRecognizers.Clear();
            var tgr_btn_takePhotoAttachment_DirektPos = new TapGestureRecognizer();
            tgr_btn_takePhotoAttachment_DirektPos.Tapped += btn_pickPhotos_DirektPos;
            btn_takePhotoAttachment_frame_DirektPos.GestureRecognizers.Add(tgr_btn_takePhotoAttachment_DirektPos);

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

            btn_back_dayover.GestureRecognizers.Clear();
            var tgr_back_dayover = new TapGestureRecognizer();
            tgr_back_dayover.Tapped += btn_DayOverBackTapped;
            btn_back_dayover.GestureRecognizers.Add(tgr_back_dayover);
            btn_dayover_yes.GestureRecognizers.Clear();
            var tgr_dayover_yes = new TapGestureRecognizer();
            tgr_dayover_yes.Tapped += btn_DayOverYesTapped;
            btn_dayover_yes.GestureRecognizers.Add(tgr_dayover_yes);
            btn_dayover_no.GestureRecognizers.Clear();
            var tgr_dayover_no = new TapGestureRecognizer();
            tgr_dayover_no.Tapped += btn_DayOverBackTapped;
            btn_dayover_no.GestureRecognizers.Add(tgr_dayover_no);


            btn_back_dsgvo.GestureRecognizers.Clear();
            var tgr_back_dsgvo = new TapGestureRecognizer();
            tgr_back_dsgvo.Tapped += btn_DSGVOBackTapped;
            btn_back_dsgvo.GestureRecognizers.Add(tgr_back_dsgvo);

            btn_back_pn.GestureRecognizers.Clear();
            var tgr_back_pn = new TapGestureRecognizer();
            tgr_back_pn.Tapped += btn_PN_BackTapped;
            btn_back_pn.GestureRecognizers.Add(tgr_back_pn);


            btn_settings_sendlog.GestureRecognizers.Clear();
            var tgr_namestacksend = new TapGestureRecognizer();
            tgr_namestacksend.Tapped += ShowSendLog;
            btn_settings_sendlog.GestureRecognizers.Add(tgr_namestacksend);

            btn_settings_clearlog.GestureRecognizers.Clear();
            var tgr_clearlog = new TapGestureRecognizer();
            tgr_clearlog.Tapped += ShowClearLog;
            btn_settings_clearlog.GestureRecognizers.Add(tgr_clearlog);


            btn_settings_sel_trans_lang.GestureRecognizers.Clear();
            var tgr_btn_settings_sel_trans_lang = new TapGestureRecognizer();
            tgr_btn_settings_sel_trans_lang.Tapped += OpenLanguage;
            btn_settings_sel_trans_lang.GestureRecognizers.Add(tgr_btn_settings_sel_trans_lang);

            btn_settings_synctimesub.GestureRecognizers.Clear();
            var tgr_synctimesub = new TapGestureRecognizer();
            tgr_synctimesub.Tapped += btn_settings_synctimesub_Tapped;
            btn_settings_synctimesub.GestureRecognizers.Add(tgr_synctimesub);
            btn_settings_synctimeadd.GestureRecognizers.Clear();
            var tgr_synctimeadd = new TapGestureRecognizer();
            tgr_synctimeadd.Tapped += btn_settings_synctimeadd_Tapped;
            btn_settings_synctimeadd.GestureRecognizers.Add(tgr_synctimeadd);


            btn_nachbuchen_back.GestureRecognizers.Clear();
            var tgr_btn_nachbuchen_back = new TapGestureRecognizer();
            tgr_btn_nachbuchen_back.Tapped += (object o, TappedEventArgs ev) =>
            {
                if (model.LastSelectedCategoryAgain == null)
                {
                    btn_back_inBuildingOrder_category_showall_again_txt.Text = "Alle zeigen";
                    model._showall_again_OrderCategory = false;
                    this.Focus(); ShowMainPage();
                }
                else
                {
                    btn_nachbuchen_Tapped(model.posAgain);
                }
            };
            btn_nachbuchen_back.GestureRecognizers.Add(tgr_btn_nachbuchen_back);
            btn_nachbuchen_cat_back.GestureRecognizers.Clear();
            var tgr_nachbuchen_cat_back = new TapGestureRecognizer();
            tgr_nachbuchen_cat_back.Tapped += (object o, TappedEventArgs ev) => { btn_nachbuchen_Tapped(model.posAgain); };
            btn_nachbuchen_cat_back.GestureRecognizers.Add(tgr_nachbuchen_cat_back);
            btn_nachbuchen.GestureRecognizers.Clear();
            var tgr_nachbuchen = new TapGestureRecognizer();
            tgr_nachbuchen.Tapped += (object o, TappedEventArgs ev) => { ShowNachbuchenPage(model.posAgain); };
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

        /*******************/
        /* LAST  BUILDINGS */
        /*******************/
        private async void SetLastBuilding()
        {
            lastBuilding_Container.IsVisible = (model.LastBuilding != null);
            lastBuilding_ContainerBottom.IsVisible = (model.LastBuilding != null);
            btn_objektinfo_container.IsVisible = (model.LastBuilding != null && !String.IsNullOrWhiteSpace(model.LastBuilding.notiz));

            btn_buildingorder_container.IsVisible = model.LastBuilding != null;
            btn_exitwork.IsVisible = model.allPositionInWork == null;
            //btn_buildingorderToTime_container.IsVisible = model.LastBuilding != null;
            btn_inwork_container.IsVisible = model.allPositionInWork != null;
            btn_nachbuchen_container.IsVisible = model.allPositionInWork != null;
            btn_regist.IsVisible = model.allPositionInWork == null;

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


            ObjektPlanWeekMobil_Stack_A.Margin = new Thickness(2, (model.allPositionInWork == null ? 30 : 0), 2, 0);

            // Trennlinie zeigen
            if (AppModel.Instance.AppControll.direktBuchenPos)
            {
                btn_objScan.IsVisible = false;
                btn_objNotScan.IsVisible = (model.LastBuilding == null || model.allPositionInWork == null);
                btn_objScanB.IsVisible = (model.LastBuilding == null || model.allPositionInWork == null);
            }
            else
            {
                btn_objScan.IsVisible = (model.LastBuilding == null || model.allPositionInWork == null);
                btn_objNotScan.IsVisible = false;
                btn_objScanB.IsVisible = false;
            }

            if (model.LastBuilding != null)
            {
                last_building_name.IsVisible = !String.IsNullOrWhiteSpace(model.LastBuilding.objektname);
                last_building_name.Text = model.LastBuilding.objektname;
                last_building_address.Text = model.LastBuilding.strasse + " " + model.LastBuilding.hsnr;
                var la = model.LastBuilding.land.Length > 2 ? model.LastBuilding.land.Substring(0, 3) : ((String.IsNullOrWhiteSpace(model.LastBuilding.land) ? "" : model.LastBuilding.land));
                last_building_zip_city.Text = (String.IsNullOrWhiteSpace(la) ? "" : la + " ") + model.LastBuilding.plz + " " + model.LastBuilding.ort;

                // MainPage Badge in Ausgewähltes Objekt
                double _prio = 100000000;
                model.LastBuilding.ArrayOfAuftrag.ForEach(order =>
                {
                    order.kategorien.ForEach(c =>
                    {
                        c.leistungen.ForEach(l =>
                        {
                            l.prio = Prio.GetLeistungPrio(l, model);
                            _prio = Math.Min(_prio, l.prio.days);
                        });
                    });
                });
                // Zeige Heute und Fällige 
                btn_buildingorderToTime_count.IsVisible = (_prio < 1);
                btn_buildingorderToTime_counttext.Text = "" + _prio;

            }

            if (model.allPositionInWork != null)
            {
                btn_buildingorder_container.IsVisible = false;
                btn_exitwork.IsVisible = false;
                /// Erstmal deaktiviert, da implementierung noch gemacht werden mus für das Stopen der laufenden und dann wieder die Neuen 
                var ts = (DateTime.Now - new DateTime(model.allPositionInWork.startticks));
                inwork_starttime_text.Text = (ts.TotalDays > 1 ? ts.ToString("%d") + "T " : "") + ts.ToString(@"hh\:mm");
                inwork_start_count_text.Text = "" + model.allPositionInWork.leistungen.Count;
            }

            var dayOverLast = DayOverWSO.LoadLast(model);
            if (dayOverLast != null)
            {
                var dt = new DateTime(dayOverLast.endticks);
                dayOverLastDate.Text = dt.ToString("dd.MM.yyyy") + " - " + dt.ToString("HH:mm");
            }
            //if (model.LastBuilding != null)
            //{
            //    await lastBuilding_Container.FadeTo(1, 500, Easing.SpringIn);
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
                if (long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) < DateTime.Now.AddDays(-7).Ticks)
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
        private async void SyncBuildingManuell(bool manuellSync = false)
        {
            SyncNewBuildingManuell(manuellSync);
            /*return;
            try
            {
                var dt = String.IsNullOrEmpty(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? DateTime.Now.AddDays(-2) : new DateTime(long.Parse(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                if (dt.AddHours(model.SettingModel.SettingDTO.SyncTimeHours) < DateTime.Now || manuellSync) //(dt.AddHours(4) < DateTime.Now || manuellSync)
                {
                    //AppModel.Logger.Info("Info: STARTE Sync Objekte/Auftraege/Leistungen => SyncBuilding");
                    // Objekte sycnen erforderlich nach 12 Stunden
                    popupContainer.IsVisible = true;
                    await Task.Delay(1);


                    IpmBuildingResponse ipmBuildingResponse = await Task.Run(() => { return model.Connections.IpmBuildingSync(); });
                    if (ipmBuildingResponse == null || !ipmBuildingResponse.success)
                    {
                        // Synchronisierung FAILED
                        AppModel.Logger.Warn("WARN: iPM.Mobile Error (0): Sync FEHLGESCHLAGEN  => SyncBuilding");
                        popupContainer.IsVisible = false;
                        await Task.Delay(1);
                        CheckForBuildingFailed(ipmBuildingResponse);
                        //********* Update Plandaten 
                        Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                    }
                    else
                    {
                        if (ipmBuildingResponse.AppControll != null)
                        {
                            model.AppControll = ipmBuildingResponse.AppControll;
                            if (model.AppControll == null) { model.AppControll = new AppControll(); }
                            AppControll.Save(model, model.AppControll);
                            SetAppControll();
                        }
                        // Erfolgreich synchronisiert
                        //AppModel.Logger.Info("Info: Sync war erfolgreich => SyncBuilding");
                        model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = DateTime.Now.Ticks.ToString();
                        dt = new DateTime(long.Parse(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                        model.SettingModel.SaveSettings();

                        BuildingWSO.DeleteBuildings(ipmBuildingResponse.deletedBuidlings);

                        if (model.AppControll.lang == "de" || !AppModel.Instance.AppControll.translation)
                        {
                            SyncBuildingDone(ipmBuildingResponse);
                            AppModel.Instance.SetAllKategorieNames();
                        }
                        else
                        {
                            //Sync und Übersetzen
                            var _ = await translateAfterSyncedBuildings(model.AppControll.lang, ipmBuildingResponse.builgings, AppModel.Instance.Lang.lang != model.AppControll.lang);
                            model.AllBuildings = ipmBuildingResponse.builgings.OrderBy(o => o.id).ToList();
                            model.InitBuildingsAgain();
                            SetLastBuilding();
                            AppModel.Instance.SetAllKategorieNames();
                        }

                        if (AppModel.Instance.Lang.lang != model.AppControll.lang)
                        {
                            AppModel.Instance.Lang.lang = model.AppControll.lang;
                            Lang.Save(AppModel.Instance.Lang);
                        }


                        popupContainer.IsVisible = false;
                        await Task.Delay(1);
                        //********* Update Plandaten 
                        Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                    }
                }
                else
                {
                    Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                }
                box_buildingInformation.Children.Clear();
                box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(model, dt));
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Method => MainPage-SyncBuildingManuell(catch): " + ex.Message);
                model.InclFilesAsJson = true;
                var ok = model.SendLogZipFile();
                await Task.Delay(2000);
            }*/
        }
        private async void CheckForBuildingFailed(IpmBuildingResponse ipmBuildingResponse)
        {
            if (model.AllBuildings == null || model.AllBuildings.Count == 0)
            {
                await DisplayAlert("Objektprüfung nicht möglich!",
                    ipmBuildingResponse != null ? ipmBuildingResponse.message : "FEHLER: Muss Online gehen, kann aber nicht!", "Zurück");
            }
        }
        private void SyncBuildingDone(IpmBuildingResponse ipmBuildingResponse)
        {
            ipmBuildingResponse.builgings.ForEach(b => { BuildingWSO.Save(model, b); });
            model.AllBuildings = ipmBuildingResponse.builgings.OrderBy(o => o.id).ToList();
            model.InitBuildingsAgain();
            SetLastBuilding();
        }

        private async void SyncNewBuildingManuell(bool manuellSync = false)
        {
            try
            {
                var dt = String.IsNullOrEmpty(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ?
                    DateTime.Now.AddDays(-2) : new DateTime(long.Parse(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                if (dt.AddHours(model.SettingModel.SettingDTO.SyncTimeHours) < DateTime.Now || manuellSync) //(dt.AddHours(4) < DateTime.Now || manuellSync)
                {
                    // AppModel.Logger.Info("Info: STARTE Sync Objekte/Auftraege/Leistungen => SyncBuilding");
                    // Objekte sycnen erforderlich nach 12 Stunden
                    popupContainer.IsVisible = true;
                    await Task.Delay(1);

                    IpmNewSyncResponse ipmNewBuildingResponse = await Task.Run(() => { return model.Connections.IpmNewBuildingSync(); });
                    if (ipmNewBuildingResponse == null || !ipmNewBuildingResponse.success)
                    {
                        // Synchronisierung FAILED
                        AppModel.Logger.Warn("WARN: iPM.Mobile Error (0): Sync FEHLGESCHLAGEN  => NewSyncBuilding");
                        popupContainer.IsVisible = false;
                        await Task.Delay(1);
                        CheckForNewBuildingFailed(ipmNewBuildingResponse);
                        //********* Update Plandaten 
                        Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                    }
                    else
                    {
                        if (ipmNewBuildingResponse.AppControll != null)
                        {
                            model.AppControll = ipmNewBuildingResponse.AppControll;
                            if (model.AppControll == null) { model.AppControll = new AppControll(); }
                            AppControll.Save(model, model.AppControll);
                            SetAppControll();
                        }
                        int i = 0;
                        int ii = 0;
                        var bs = new List<BuildingWSO>();
                        var blist = ListExtensions.ChunkBy(ipmNewBuildingResponse.builgings.Distinct().ToList(), 30);
                        double pr = 0;
                        popupContainer_count.Text = "SYNCHRONISATION (0%)";
                        await Task.Delay(1);
                        for (int zz = 0; zz < blist.Count; zz++)
                        {
                            pr = Convert.ToDouble(i) / (Convert.ToDouble(blist.Count) / 100);
                            pr = pr == 0 ? 1d : pr;
                            popupContainer_count.Text = "SYNCHRONISATION (" + pr.ToString("###") + "%)";
                            await Task.Delay(10);
                            //UpdateSyncCounter(pr);

                            i++;
                            string objids = "";
                            //var objidsInt = Utils.ConvertStringToListInt(objids);
                            blist[zz].ForEach(b => { objids = objids + (objids.Length > 0 ? "," : "") + b.id; });
                            IpmNewSyncResponse resp = model.Connections.IpmNewAuftragSync(objids);
                            if (resp != null && resp.auftraege != null)
                            {
                                ii++;
                                for (int z = 0; z < blist[zz].Count; z++)
                                {
                                    var aufs = resp.auftraege.FindAll(a => a.objektid == blist[zz][z].id);
                                    blist[zz][z].ArrayOfAuftrag = aufs;
                                };
                            }
                            bs.AddRange(blist[zz]);
                            if (blist.Count == i)
                            {
                                UpdateSyncCounter(100d);
                                if (i == ii)
                                {
                                    // Erfolgreich synchronisiert
                                    ipmNewBuildingResponse.builgings = bs;
                                    SyncNewBuildingManuell_next(ipmNewBuildingResponse);
                                }
                                else
                                {
                                    // Nicht vollständig syncronisiert!!!
                                    popupContainer.IsVisible = false;
                                    popupContainerSyncFaild.IsVisible = true;
                                    await Task.Delay(1);
                                }
                            }
                        };
                    }
                }
                else
                {
                    Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                }
                box_buildingInformation.Children.Clear();
                box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(model, dt));
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Method => MainPage-SyncBuildingManuell(catch): " + ex.Message);
                model.InclFilesAsJson = true;
                var ok = model.SendLogZipFile();
                await Task.Delay(2000);
            }
        }
        private async void UpdateSyncCounter(double pr)
        {
            popupContainer_count.Text = "SYNCHRONISATION (" + pr.ToString("###,##") + "%)";
            await Task.Delay(1);
        }
        private async void SyncNewBuildingManuell_next(IpmNewSyncResponse ipmNewBuildingResponse)
        {
            try
            {
                // Erfolgreich synchronisiert
                // AppModel.Logger.Info("Info: Sync war erfolgreich => SyncBuilding");
                model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = DateTime.Now.Ticks.ToString();
                var dt = new DateTime(long.Parse(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                model.SettingModel.SaveSettings();

                BuildingWSO.DeleteBuildings(ipmNewBuildingResponse.deletedBuidlings);

                if (model.AppControll.lang == "de" || !AppModel.Instance.AppControll.translation)
                {
                    NewSyncBuildingDone(ipmNewBuildingResponse);
                    AppModel.Instance.SetAllKategorieNames();
                }
                else
                {
                    //Sync und Übersetzen
                    var _ = await translateAfterSyncedBuildings(model.AppControll.lang, ipmNewBuildingResponse.builgings, AppModel.Instance.Lang.lang != model.AppControll.lang);
                    model.AllBuildings = ipmNewBuildingResponse.builgings.OrderBy(o => o.id).ToList();
                    model.InitBuildingsAgain();
                    SetLastBuilding();
                    AppModel.Instance.SetAllKategorieNames();
                }

                if (AppModel.Instance.Lang.lang != model.AppControll.lang)
                {
                    AppModel.Instance.Lang.lang = model.AppControll.lang;
                    Lang.Save(AppModel.Instance.Lang);
                }


                popupContainer.IsVisible = false;
                await Task.Delay(1);
                //********* Update Plandaten 
                Load_PlanTabs(((int)DateTime.Now.DayOfWeek));

            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("Method => MainPage-SyncBuildingManuell(catch): " + ex.Message);
                model.InclFilesAsJson = false;
                var ok = model.SendLogZipFile();
                await Task.Delay(2000);
            }
        }


        private async void CheckForNewBuildingFailed(IpmNewSyncResponse ipmNewBuildingResponse)
        {
            if (model.AllBuildings == null || model.AllBuildings.Count == 0)
            {
                await DisplayAlert("Objektprüfung nicht möglich!",
                    ipmNewBuildingResponse != null ? ipmNewBuildingResponse.message : "FEHLER: Verbindung Online nicht möglich!", "Zurück");
            }
        }
        private void NewSyncBuildingDone(IpmNewSyncResponse ipmNewBuildingResponse)
        {
            ipmNewBuildingResponse.builgings.ForEach(b => { BuildingWSO.Save(model, b); });
            model.AllBuildings = ipmNewBuildingResponse.builgings.OrderBy(o => o.id).ToList();
            model.InitBuildingsAgain();
            SetLastBuilding();
        }

        /*
        private async void FastSyncCount()
        {
            IpmBuildingResponse fastSyncResponse = await Task.Run(() => { return model.Connections.IpmFastSyncCount(); });
            if (fastSyncResponse != null && fastSyncResponse.success)
            {
                AppModel.Instance.FastSyncCount = Int32.Parse(fastSyncResponse.message);

                if (fastSyncResponse.AppControll != null)
                {
                    model.AppControll = fastSyncResponse.AppControll;
                    AppControll.Save(model, model.AppControll);
                }

                if (AppModel.Instance.FastSyncCount > 0) { FastSync(true); }

                if (AppModel.Instance.FastSyncCount == 0)
                {
                    Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                }
            }
            else
            {
                Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
            }
        }
        */
        private async void FastSync(bool run = false)
        {
            var dt = String.IsNullOrEmpty(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? DateTime.Now.AddDays(-2) : new DateTime(long.Parse(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
            if (run || dt.AddHours(model.SettingModel.SettingDTO.SyncTimeHours) < DateTime.Now) //(dt.AddHours(4) < DateTime.Now || manuellSync)
            {
                //AppModel.Logger.Info("Info: STARTE FastSync Objekte/Auftraege/Leistungen/weitere... => FastSync");
                // Objekte sycnen erforderlich nach 12 Stunden
                popupContainer.IsVisible = true;
                await Task.Delay(1);

                IpmBuildingResponse fastSyncResponse = await Task.Run(() => { return model.Connections.IpmFastSync(); });
                if (fastSyncResponse == null || !fastSyncResponse.success)
                {
                    // Synchronisierung FAILED
                    AppModel.Logger.Warn("WARN: iPM.Mobile Error (0): FastSync FEHLGESCHLAGEN  => FastSync" +
                        (fastSyncResponse != null ? fastSyncResponse.message : ""));
                    popupContainer.IsVisible = false;
                    await Task.Delay(1);
                    //********* Update Plandaten 
                    Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                }
                else
                {
                    if (fastSyncResponse.AppControll != null)
                    {
                        model.AppControll = fastSyncResponse.AppControll;
                        if (model.AppControll == null) { model.AppControll = new AppControll(); }
                        AppControll.Save(model, model.AppControll);
                        SetAppControll();
                    }

                    // Erfolgreich synchronisiert
                    //AppModel.Logger.Info("Info: FastSync war erfolgreich => FastSync");
                    model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = DateTime.Now.Ticks.ToString();
                    dt = new DateTime(long.Parse(model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks));
                    model.SettingModel.SaveSettings();

                    BuildingWSO.DeleteBuildings(fastSyncResponse.deletedBuidlings);

                    // Sprache hat sich geändert 
                    if (AppModel.Instance.Lang.lang != model.AppControll.lang && AppModel.Instance.AppControll.translation)
                    {
                        SyncBuildingManuell(true);
                        return;
                    }
                    else
                    {
                        if (model.AppControll.lang == "de" || !AppModel.Instance.AppControll.translation)
                        {
                            FastSyncUpdate(fastSyncResponse, true);
                            AppModel.Instance.SetAllKategorieNames();
                        }
                        else
                        {
                            //Sync und Übersetzen
                            var _ = await translateAfterSyncedBuildings(model.AppControll.lang, fastSyncResponse.builgings, AppModel.Instance.Lang.lang != model.AppControll.lang);
                            FastSyncUpdate(fastSyncResponse, false);
                            AppModel.Instance.SetAllKategorieNames();
                        }
                    }

                    if (AppModel.Instance.Lang.lang != model.AppControll.lang)
                    {
                        AppModel.Instance.Lang.lang = model.AppControll.lang;
                        Lang.Save(AppModel.Instance.Lang);
                    }


                    popupContainer.IsVisible = false;
                    await Task.Delay(1);

                    //********* Update Plandaten 
                    Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
                }
            }
            else
            {
                Load_PlanTabs(((int)DateTime.Now.DayOfWeek));
            }
            box_buildingInformation.Children.Clear();
            box_buildingInformation.Children.Add(BuildingWSO.GetBuildingInformation(model, dt));
        }
        private void FastSyncUpdate(IpmBuildingResponse fastSyncResponse, bool saveBuilding)
        {
            if (fastSyncResponse.builgings != null)
            {
                fastSyncResponse.builgings.ForEach(b =>
                {
                    if (saveBuilding) { BuildingWSO.Save(model, b); }
                    var i = AppModel.Instance.AllBuildings.FindIndex(f => f.id == b.id);
                    if (i > -1)
                    {
                        AppModel.Instance.AllBuildings[i] = b;
                    }
                    else
                    {
                        AppModel.Instance.AllBuildings.Add(b);
                    }
                });
                AppModel.Instance.AllBuildings.ForEach(b =>
                {
                    if (b.del > 0 || b.ArrayOfAuftrag.Count == 0)
                    {
                        BuildingWSO.DeleteBuilding(b.id);
                    }
                });
                AppModel.Instance.AllBuildings.RemoveAll(b => b.del > 0 && b.ArrayOfAuftrag.Count == 0);
                AppModel.Instance.AllBuildings = AppModel.Instance.AllBuildings.OrderBy(o => o.id).ToList();
                model.InitBuildingsAgain();
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
        private int GetAllSyncFromUploadCount()
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

        private async void CheckAllSyncFromUpload()
        {
            popupContainer_quest_countfromupload.IsVisible = false;
            _allCountFromUpload = GetAllSyncFromUploadCount();
            _pn = PNWSO.CountFromStack();
            if (_allCountFromUpload > 0)
            {
                // Es gibt Upload Daten !!!
                _allCountFromUploadFalied = false;
                overlay.IsVisible = true;
                await Task.Delay(1);
                if (_dayovers > 0 && !_allCountFromUploadFalied)
                {
                    SyncDayOver();
                    await Task.Delay(300);
                }
                if (_checks > 0 && !_allCountFromUploadFalied)
                {
                    SyncChecks();
                    await Task.Delay(300);
                }
                if (_checksBemImg > 0 && !_allCountFromUploadFalied)
                {
                    SyncChecksBemImg();
                    await Task.Delay(300);
                }
                if (_bemerkungen > 0 && !_allCountFromUploadFalied)
                {
                    SyncSingleNotice();
                    await Task.Delay(300);
                }
                if (_bilder > 0 && !_allCountFromUploadFalied)
                {
                    SyncNoticeBild();
                    await Task.Delay(300);
                }
                if (_packs > 0 && !_allCountFromUploadFalied)
                {
                    SyncPosition();
                    await Task.Delay(300);
                }
                if (_trans > 0 && !_allCountFromUploadFalied)
                {
                    SyncTransSigns();
                    await Task.Delay(300);
                }
                if (_objectValues > 0 && !_allCountFromUploadFalied)
                {
                    SyncObjectValues();
                    await Task.Delay(300);
                }
                if (_objectValueBilds > 0 && !_allCountFromUploadFalied)
                {
                    SyncObjectValueBild();
                    await Task.Delay(300);
                }
                if (_pn > 0 && !_allCountFromUploadFalied)
                {
                    SyncPN();
                    await Task.Delay(500);
                }
                await Task.Delay(1);
                overlay.IsVisible = false;
            }

            if (_pn > 0 && _allCountFromUpload == 0)
            {
                SyncPN();
            }
        }




        /*******************/
        /* SYNC CHECKLIST (auch in Background)
        /*******************/
        private async void SyncChecks()
        {
            var checklist = CheckClass.LoadAllFromUploadStack();
            List<string> guidsList = new List<string>();
            checklist.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
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
            };
        }
        private async Task<ChecksResponse> SyncChecks_Done(Check check)
        {
            ChecksResponse checkResponse = await Task.Run(() => { return model.Connections.SetCheckANonePic(check); });
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
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
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
            ChecksResponse response = await Task.Run(() => { return model.Connections.SetCheckABemImg(pic); });
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
            var objectValues = ObjektDataWSO.LoadAllFromUploadStack(model);
            List<string> guidsList = new List<string>();
            objectValues.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var da = objectValues.Find(b => b.guid == guid);
                    if (da != null)
                    {
                        objectValues.Remove(da);
                        ObjektDataWSO.DeleteFromUploadStack(model, da);
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
                        ObjektDataWSO.DeleteFromUploadStack(model, d);
                    });
                }
            };
        }
        private async Task<ObjectValuesResponse> SyncObjectValues_Done(List<ObjektDataWSO> objectValues)
        {
            ObjectValuesResponse objectValuesResponse = (await Task.Run(() => { return model.Connections.ObjectValuesSync(objectValues); }));
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
            var objectValueBilds = ObjektDatenBildWSO.LoadAllFromUploadStack(model);
            List<string> guidsList = new List<string>();
            objectValueBilds.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var da = objectValueBilds.Find(b => b.guid == guid);
                    if (da != null)
                    {
                        objectValueBilds.Remove(da);
                        ObjektDatenBildWSO.DeleteFromUploadStack(model, da);
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
                        ObjektDatenBildWSO.DeleteFromUploadStack(model, value);
                    }
                });
            };
        }
        private async Task<ObjectValueBildResponse> SyncObjectValueBild_Done(ObjektDatenBildWSO value)
        {
            ObjectValueBildResponse response = (await Task.Run(() => { return model.Connections.ObjectValueBildSync(value); }));
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
            var pn = PNWSO.LoadAllFromUploadStack();
            pn.personid = model.Person.id;
            var resPN = await Task.Run(() => { return model.Connections.PNSync(pn); });
            if (resPN.success)
            {
                PNWSO.DeleteFromUploadStack();
                model.SettingModel.SettingDTO.PNToken = pn.token;
                model.SettingModel.SaveSettings();
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
        private async void SyncDayOver()
        {
            var dayOvers = DayOverWSO.LoadAllFromUploadStack(model);
            List<string> guidsList = new List<string>();
            dayOvers.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var da = dayOvers.Find(b => b.guid == guid);
                    if (da != null)
                    {
                        dayOvers.Remove(da);
                        DayOverWSO.DeleteFromUploadStack(model, da);
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
                        DayOverWSO.DeleteFromUploadStack(model, d);
                    });
                }
            };
        }
        private async Task<DayOverResponse> SyncDayOver_Done(List<DayOverWSO> dayOvers)
        {
            DayOverResponse dayOverResponse = (await Task.Run(() => { return model.Connections.DayOverSync(dayOvers); }));
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
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
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
            };
        }
        private async Task<AllTransSignResponse> SyncTransSigns_Done(AllTransSignRequest transSign)
        {
            AllTransSignResponse res = (await Task.Run(() => { return model.Connections.AllTransSignSync(transSign); }));
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
            var bemerkungen = BemerkungWSO.LoadAllFromUploadStack(model);
            List<string> guidsList = new List<string>();
            bemerkungen.ForEach(v => { guidsList.Add(v.guid); });
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
            if (resGuidsList != null && resGuidsList.Length > 0)
            {
                resGuidsList.ToList().ForEach(guid =>
                {
                    var bem = bemerkungen.Find(b => b.guid == guid);
                    if (bem != null)
                    {
                        bemerkungen.Remove(bem);
                        BemerkungWSO.DeleteFromUploadStack(model, bem);
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
                            BemerkungWSO.DeleteFromUploadStack(model, bemerkungen[i]);
                        }
                    }
                    else
                    {
                        bemerkungen[i].hasSend = true;
                        BemerkungWSO.DeleteFromUploadStack(model, bemerkungen[i]);
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
            SingleNoticeResponse noticeResponse = (await Task.Run(() => { return model.Connections.SingleNoticeSync(bem); }));
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
            var resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
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
            NoticeBildResponse response = await Task.Run(() => { return model.Connections.NoticeBildSync(pic); });
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
                packs = new List<LeistungPackWSO> { model.allPositionInWork };
            }
            else
            {
                packs = LeistungPackWSO.LoadAllFromUploadStack(model);
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
                resGuidsList = await Task.Run(() => { return model.Connections.GuidsCheck(guidsList.ToArray()); });
                if (resGuidsList != null && resGuidsList.Length > 0)
                {
                    resGuidsList.ToList().ForEach(guid =>
                    {
                        var pa = packs.Find(b => b.guid == guid);
                        if (pa != null)
                        {
                            packs.Remove(pa);
                            LeistungPackWSO.DeleteFromUploadStack(model, pa);
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
                            if (model.LastBuilding == null && result.leistungen != null && result.leistungen.Count > 0)
                            {
                                building = BuildingWSO.LoadBuilding(model, result.leistungen[0].objektid);
                            }
                            if (model.LastBuilding != null)
                            {
                                building = model.LastBuilding;
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
                                BuildingWSO.Save(model, building);
                            }
                            LeistungPackWSO.DeleteFromUploadStack(model, packs[i]);
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
            PositionResponse positionResponse = await Task.Run(() => { return model.Connections.PositionSync(pack); });
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

                if (model.AppControll.showObjektPlans)
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
            var packs = new List<LeistungPackWSO> { model.allPositionInWork };
            if (packs.Count > 0)
            {
                packs.ForEach(async pack =>
                {
                    var result = await SyncPositionAgain_Done(pack);
                    model.allPositionInWork.leistungen.ForEach(l => { l.again = 0; });
                });
            }
        }
        private async Task<LeistungPackWSO> SyncPositionAgain_Done(LeistungPackWSO pack)
        {
            PositionResponse positionResponse = await Task.Run(() => { return model.Connections.PositionAgainSync(pack); });
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
            if (model.AppControll != null)
            {
                frame_PersonTimes.IsVisible = model.AppControll.showPersonTimes;

                // beide NICHT zeigen (Plans und Ticktes)
                if (!model.AppControll.showObjektPlans && !model.AppControll.showTickets && !model.AppControll.showChecks)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = false;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = false;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = true; // Space wenn beide nichts gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = false;
                }
                // Ticket NICHT zeigen (Plans und Ticktes)
                if (model.AppControll.showObjektPlans && !model.AppControll.showTickets)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = false; // Space wenn beide nichts gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = true;

                    frame_plantabA.IsVisible = true;
                    frame_plantabB.IsVisible = true;
                    frame_plantabC.IsVisible = false;
                    frame_plantabCe.IsVisible = model.AppControll.showChecks;
                    frame_planConA.IsVisible = true;
                    frame_planConB.IsVisible = false;
                    frame_planConCe.IsVisible = false;
                    frame_planConC.IsVisible = false;
                }
                // Plan NICHT zeigen (nur Ticktes)
                if (!model.AppControll.showObjektPlans && model.AppControll.showTickets)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = false; // Space wenn beide nichts gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = true;

                    frame_plantabA.IsVisible = false;
                    frame_plantabB.IsVisible = false;
                    frame_plantabCe.IsVisible = model.AppControll.showChecks;
                    frame_plantabC.IsVisible = true;
                    frame_planConA.IsVisible = false;
                    frame_planConB.IsVisible = false;
                    frame_planConCe.IsVisible = false;
                    frame_planConC.IsVisible = true;
                    frame_plantabC.Margin = new Thickness(0, -8, 2, 0);// Tab hochstellen
                }
                // beide zeigen (Plans und Ticktes)
                if (model.AppControll.showObjektPlans && model.AppControll.showTickets)
                {
                    ObjektPlanWeekMobil_Stack_A.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_B.IsVisible = true;
                    ObjektPlanWeekMobil_Stack_C.IsVisible = false; // Space wenn beide nicht gezeigt werden
                    ObjektPlanWeekMobil_Stack_ABC.IsVisible = true;

                    frame_plantabA.IsVisible = true;
                    frame_plantabB.IsVisible = true;
                    frame_plantabCe.IsVisible = model.AppControll.showChecks;
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
            img_onlinestate.Source = AppModel.Instance.IsInternet ? "isonlineB.png" : "isofflineB.png";
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
                img_gpsstate.Source = "gpsoff2.png";
            }
            //string vor = "--:--";
            //if (AppModel.Instance.lastServerPing > 0)
            //{
            //    var ts = DateTime.Now - (new DateTime(AppModel.Instance.lastServerPing));
            //    vor = new DateTime(AppModel.Instance.lastServerPing).ToString("HH:mm:ss");
            //}
            //lb_pingstate.Text = "ServerPing:" + vor;
        }






        public void ShowAlertMessage(string titel, string message, bool enableBtn = false)
        {
            if (popupContainer_Alert.IsVisible) { return; }
            popupContainer_Alert_Titel.Text = titel;
            popupContainer_Alert_Text.Text = message;
            popupContainer_Alert.IsVisible = true;
            popupContainer_Alert_btn.IsVisible = enableBtn;
        }
        public void HideAlertMessage(object sender, EventArgs e)
        {
            popupContainer_Alert.IsVisible = false;
            popupContainer_Alert_Titel.Text = "";
            popupContainer_Alert_Text.Text = "";
            popupContainer_Alert_btn.IsVisible = true;
        }

    }


    public class GestTappedBuildingTreeItemObject
    {
        public BuildingWSO building = null;
        public StackLayout stacklayout = null;
        public object sfButton = null;
        public int index = 0;
    }






}
