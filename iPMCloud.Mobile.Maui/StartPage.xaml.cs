using iPMCloud.Mobile.vo;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

//using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
//https://docs.microsoft.com/de-de/xamarin/essentials/preferences?tabs=android

namespace iPMCloud.Mobile
{
    public partial class StartPage : ContentPage
    {
        // private BackgroundWorker backgroundWorker = new BackgroundWorker();

        public bool isInitialize = false;

        public StartPage()
        {        
            isInitialize = true;
            InitializeComponent();

            InitStartPage();
            ShowDisconnected();
        }
        private void InitStartPage(bool switchCustomer = false)
        {

            btn_regScan_limg.Source = AppModel.Instance.imagesBase.QrScan;
            //btn_regScanWarn_img.Source = AppModel.Instance.imagesBase.WarnTriangleYellow;

            btn_flashlight_img.Source = AppModel.Instance.imagesBase.Flashlight;
            btn_flashlight_AddRegScan_img.Source = AppModel.Instance.imagesBase.Flashlight;

            img_gpsinfo.Source = AppModel.Instance.imagesBase.Pin;

            btn_login_img.Source = AppModel.Instance.imagesBase.Login;
            btn_toregist_img.Source = AppModel.Instance.imagesBase.QrScan;
            btn_addRegScan_img.Source = AppModel.Instance.imagesBase.AddImageWithe;
            btn_addRegScan2_img.Source = AppModel.Instance.imagesBase.AddImageWithe;
            btn_ToRegScanManagement_img.Source = AppModel.Instance.imagesBase.Change;
            img_logo.Source = AppModel.Instance.imagesBase.Logo;
            img_backBtn_RegManagement.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;
            img_backBtn_inRegAddScan_img.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;

            lb_version.Text = "V" + AppModel.Instance.Version;// + " (" + AppModel.Instance.Build + ")";

            InitStartPageHandlers();

            InitPermission();

            if (AppModel.Instance.SettingModel.IsCredentialsSettingsReady)//|| AppModel.Instance.IsTest)
            {
                ShowLoginPage(switchCustomer);
            }
            else
            {
                ShowBeforeRegScan();
                //ShowRegScan();
            }
        }

        private async void InitPermission()
        {
            _ = await CheckPermissions(false, false);
        }

        //private async void StartGPS()
        //{
        //    var status = await AppModel.Instance.CheckPermissionGPS();
        //    if (String.IsNullOrWhiteSpace(status))
        //    {
        //        Task.Run(async () =>
        //        {
        //            AppModel.Instance.SetLastLocationGPS();
        //        });
        //    }
        //}

        private async void ShowBeforeRegScan()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BeforeLogin_Container.IsVisible = true;
            RegScan_Container.IsVisible = false;
            Login_Container.IsVisible = false;
            AddRegScan_Container.IsVisible = false;
            RegManagement_Container.IsVisible = false;

            //InitStartPageHandlers();

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        private async void ShowRegScan()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BeforeLogin_Container.IsVisible = false;
            RegScan_Container.IsVisible = true;
            Login_Container.IsVisible = false;
            AddRegScan_Container.IsVisible = false;
            RegManagement_Container.IsVisible = false;

            InitStartPageHandlers();
            lay_regscan.Children.Clear();
            AppModel.Instance.UseExternHardware = true;
            AppModel.Instance.Scan.ScanRegView(this, lay_regscan, MethodAfterScan);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public bool MethodAfterScan()
        {
            //StartGPS();
            AppModel.Instance.UseExternHardware = false;
            lay_regscan.Children.Clear();
            popupContainer_gpsinfo.IsVisible = true;
            return true;
        }

        public async void Btn_GPSInfoTapped(object sender, EventArgs e)
        {
            _ = await CheckPermissions(true, true);
            ShowLoginPage();
            popupContainer_gpsinfo.IsVisible = false;
        }

        protected override void OnDisappearing()
        {
            if (AppModel.Instance._cts != null && !AppModel.Instance._cts.IsCancellationRequested)
                AppModel.Instance._cts.Cancel();
            base.OnDisappearing();
        }

        private async Task<bool> CheckPermissions(bool inclGPS, bool showAlert)
        {
            AppModel.Instance.CheckPermissions();
            if (!String.IsNullOrWhiteSpace(AppModel.Instance.checkPermissionsMessage))
            {
                if (showAlert)
                {
                    AppModel.Instance.checkPermissionsMessage = AppModel.Instance.checkPermissionsMessage.Replace(";", "\n\n");
                    await DisplayAlertAsync("Folgendes wird benötigt!", AppModel.Instance.checkPermissionsMessage, "OK");
                    //model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                }
                return false;
            }
            if (inclGPS)
            {
                AppModel.Instance.CheckPermissionGPS();
                if (!String.IsNullOrWhiteSpace(AppModel.Instance.checkPermissionGPSMessage))
                {
                    if (showAlert)
                    {
                        AppModel.Instance.checkPermissionGPSMessage = AppModel.Instance.checkPermissionGPSMessage.Replace(";", "\n\n");
                        await DisplayAlertAsync("Berechtigungsproblem!", AppModel.Instance.checkPermissionGPSMessage, "OK");
                        //model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                    }
                    return false;
                }
                //AppModel.Instance.InitGPSTimer();
            }
            return true;
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

        private async void ShowAddRegScan()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BeforeLogin_Container.IsVisible = false;
            RegScan_Container.IsVisible = false;
            Login_Container.IsVisible = false;
            AddRegScan_Container.IsVisible = true;
            RegManagement_Container.IsVisible = false;

            lay_addregscan.Children.Clear();
            InitStartPageHandlers();
            AppModel.Instance.UseExternHardware = true;
            AppModel.Instance.Scan.ScanAddRegView(this, lay_addregscan, MethodAfterAddScan, MethodAfterAddScanFail);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }

        private async void ShowRegManagementScan()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BeforeLogin_Container.IsVisible = false;
            RegScan_Container.IsVisible = false;
            Login_Container.IsVisible = false;
            AddRegScan_Container.IsVisible = false;
            RegManagement_Container.IsVisible = true;

            InitStartPageHandlers();
            lay_selectcompany_container.Children.Clear();
            AppModel.Instance.Companies.ForEach(c =>
            {
                var isSelected = c.CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber;
                var tgr = new TapGestureRecognizer();
                if (!isSelected)
                {
                    tgr.Tapped += (s, e) => { CompanySelected(s, e); };
                }
                Border companyView = Elements.GetCompanySelectionItem(c, AppModel.Instance.imagesBase.Building, isSelected);
                companyView.GestureRecognizers.Clear();
                companyView.GestureRecognizers.Add(tgr);
                companyView.ClassId = c.CustomerNumber;
                var tgrDelete = new TapGestureRecognizer();
                if (!isSelected)
                {
                    tgrDelete.Tapped += (s, e) => { CompanyDeleted(s, e); };
                }
                Border xBtn = Elements.GetXButton(c, AppModel.Instance.imagesBase.Trash, isSelected);
                xBtn.GestureRecognizers.Clear();
                xBtn.GestureRecognizers.Add(tgrDelete);
                xBtn.ClassId = c.CustomerNumber;
                var stack = new StackLayout
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.Fill,
                    Orientation = StackOrientation.Horizontal,
                    Children = { companyView, xBtn }
                };
                lay_selectcompany_container.Children.Add(stack);
            });

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }

        public bool MethodAfterAddScan()
        {
            AppModel.Instance.UseExternHardware = false;
            lay_addregscan.Children.Clear();
            AppModel.Instance.Person = null;
            entry_login_name.Text = "";
            entry_login_password.Text = "";
            sw_autologin.IsToggled = false;
            AppModel.Instance.SettingModel.SettingDTO.LoginName = "";
            AppModel.Instance.SettingModel.SettingDTO.LoginPassword = "";
            AppModel.Instance.SettingModel.SettingDTO.Autologin = false;
            AppModel.Instance.SettingModel.SettingDTO.LoginToken = "";
            AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "";
            AppModel.Instance.Connections.InitConnections();
            AppModel.Instance.Connections.InitPNConnections();

            AppModel.Instance.SettingModel.SaveSettings();
            ShowLoginPage();
            return true;
        }
        public bool MethodAfterAddScanFail()
        {
            AppModel.Instance.UseExternHardware = false;
            lay_addregscan.Children.Clear();
            ShowLoginPage();
            return false;
        }


        private async void ShowLoginPage(bool switchCustomer = false)
        {
            //AppModel.Instance.InitGPSTimer();
            if (AppModel.Instance.Companies != null && AppModel.Instance.Companies.Count > 1)
            {
                btn_addRegScan_frame.IsVisible = false;
                btn_ToRegScanManagement_frame.IsVisible = true;
            }
            else
            {
                btn_addRegScan_frame.IsVisible = true;
                btn_ToRegScanManagement_frame.IsVisible = false;
            }

            if (AppModel.Instance.SettingModel.SettingDTO.Autologin && !String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LoginToken) &&
                !AppModel.Instance.State.IsBackTappedToLogin && !switchCustomer)
            {
                //Es gibt ein Token und Autologin
                CheckLogin(true);//SmallLoginCheck
            }
            else
            {
                // Kein Autologin - Anmeldeseite zeigen
                isInitialize = true;
                overlay.IsVisible = true;
                await Task.Delay(1);

                AppModel.Instance.State.IsBackTappedToLogin = false;

                RegScan_Container.IsVisible = false;
                Login_Container.IsVisible = true;
                AddRegScan_Container.IsVisible = false;
                RegManagement_Container.IsVisible = false;

                InitStartPageHandlers();
                lb_login_mandant.Text = AppModel.Instance.SettingModel.SettingDTO.CustomerName;

                if (switchCustomer)
                {
                    AppModel.Instance.InitBuildings();
                    AppModel.Instance.SetAllKategorieNames();
                    Btn_LoginTapped(null, null);
                }
                else
                {
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    isInitialize = false;
                }
            }
        }

        private void CompanySelected(object s, EventArgs e)
        {
            // Vorherige aktive Company/SettingDTO speichern
            Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

            var child = ((StackLayout)((Border)s).Content);
            var customerNumber = child.ClassId;
            var company = AppModel.Instance.Companies.Find(c => c.CustomerNumber == customerNumber);
            if (company != null)
            {
                AppModel.Instance.SettingModel.SettingDTO = Company.ToSettingDTO(company);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                AppModel.Instance.SettingModel.SettingDTO.LoginToken = "";
                AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "";
                entry_login_name.Text = AppModel.Instance.SettingModel.SettingDTO.LoginName;
                entry_login_password.Text = AppModel.Instance.SettingModel.SettingDTO.LoginPassword;
                sw_autologin.IsToggled = false;                //
                AppModel.Instance.SettingModel.SaveSettings();

                AppModel.Instance.Connections.InitConnections();
                AppModel.Instance.Connections.InitPNConnections();
                InitStartPage(true);
            }
        }
        private async void CompanyDeleted(object s, EventArgs e)
        {

            var a = await DisplayAlertAsync("Unternehmen entfernen?", "\n\nMöchten Sie wirklich das gewählte Unternehmen aus Ihrer App entfernen?", "JETZT ENTFERNEN", "ABBRECHEN");
            if (a)
            {
                var child = ((StackLayout)((Border)s).Content);
                var customerNumber = child.ClassId;
                var company = AppModel.Instance.Companies.Find(c => c.CustomerNumber == customerNumber);
                if (company != null)
                {
                    if (Company.DeleteCompany(AppModel.Instance, company))
                    {
                        ShowRegManagementScan();
                    }
                }
            }
        }


        public void Btn_toregistTapped(object sender, EventArgs e)
        {
            ShowRegScan();
        }
        public async void Btn_toregistMoreTapped(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync("http://www.ipm-cloud.de/", BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception)
            {
                // An unexpected error occured. No browser may be installed on the device.
            }
        }


        public async void Btn_LoginTapped(object sender, EventArgs e)
        {
            if (AppModel.Instance.IsInternet)
            {
                AppModel.Instance.SettingModel.SettingDTO.LoginName = entry_login_name.Text;
                AppModel.Instance.SettingModel.SettingDTO.LoginPassword = entry_login_password.Text;
                //model.SettingModel.SettingDTO.LoginToken = "";
                AppModel.Instance.SettingModel.SettingDTO.Autologin = sw_autologin.IsToggled;
                AppModel.Instance.SettingModel.SaveSettings();
                CheckLogin();
            }
            else
            {
                await DisplayAlertAsync("KEIN INTERNET!", "Für diese Aktion brauchen Sie eine Internetverbindung!", "OK");
            }
        }


        public bool showAddRegScanTapped = false;
        public async void Btn_AddRegScanTapped(object sender, EventArgs e)
        {
            if (showAddRegScanTapped) { return; }
            showAddRegScanTapped = true;
            var a = await DisplayAlertAsync("Unternehmen hinzufügen?",
                "Möchten Sie wirklich ein weiteres Unternehmen registrieren? \n\nBei mehreren registrieten Unternehmen können sie wählen für welches Sie gerade Arbeiten. \n\nSie müssen jedoch darauf achten, das Arbeiten/Leistungen in einem Unternehmen abgeschlossen sein müssen, um in einem anderen registrierten Unternehmen tätig zu sein.", "OK", "ABBRECHEN");
            if (a)
            {
                showAddRegScanTapped = false;
                ShowAddRegScan();
            }
            showAddRegScanTapped = false;
        }
        public void Btn_ToRegScanManagementTapped(object sender, EventArgs e)
        {
            ShowRegManagementScan();
        }
        public void BackToLoginPage(object sender, EventArgs e)
        {
            ShowLoginPage();
        }
        public async void Btn_DeleteRegScanTapped(object sender, EventArgs e)
        {
            var a = await DisplayAlertAsync("Registrierung löschen?", "Sind Sie sich sicher das Sie diese Registrierung löschen möchten?\n\n", "JETZT LÖSCHEN", "ABBRECHEN");
            if (a)
            {
                //model.SettingModel.SettingDTO.ServerUrl = "";
                //model.SettingModel.SettingDTO.CustomerNumber = "";
                //model.SettingModel.SettingDTO.CustomerName = "";
                //model.SettingModel.SettingDTO.LoginName = "";
                //model.SettingModel.SettingDTO.LoginPassword = "";
                //model.SettingModel.SettingDTO.LoginToken = "";
                //model.SettingModel.SettingDTO.LastToken = "";
                //model.SettingModel.SettingDTO.Autologin = false;

                //PersonWSO.DeletePerson(model);
                //BuildingWSO.DeleteBuildings(model);
                //model.LastBuilding = null;
                //model.SettingModel.SettingDTO.LastBuildingIdScanned = 0;
                //model.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = null;

                //model.SettingModel.SaveSettings();
                //ShowRegScan();
            }
        }
        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (isInitialize) { return; }
            AppModel.Instance.SettingModel.SettingDTO.Autologin = !AppModel.Instance.SettingModel.SettingDTO.Autologin;
            sw_autologin.IsToggled = AppModel.Instance.SettingModel.IsLoginSettingsReady && AppModel.Instance.SettingModel.SettingDTO.Autologin;
            AppModel.Instance.SettingModel.SaveSettings();
        }


        public void LoginNameChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_login_name.Text = e.NewTextValue.Replace(" ", String.Empty); };
            AppModel.Instance.SettingModel.SettingDTO.LoginName = entry_login_name.Text;
        }

        public void LoginPasswordChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_login_password.Text = e.NewTextValue.Replace(" ", String.Empty); };
            AppModel.Instance.SettingModel.SettingDTO.LoginPassword = entry_login_password.Text;
        }

        public bool hasInitializedHandlers = false;
        public async void InitStartPageHandlers()
        {
            if (hasInitializedHandlers) { return; }

            btn_endselectedwork.GestureRecognizers.Clear();
            var tgr1btn_endselectedwork = new TapGestureRecognizer();
            tgr1btn_endselectedwork.Tapped -= Btn_GPSInfoTapped;
            tgr1btn_endselectedwork.Tapped += Btn_GPSInfoTapped;
            btn_endselectedwork.GestureRecognizers.Add(tgr1btn_endselectedwork);

            btn_flashlight_container.GestureRecognizers.Clear();
            var tgr1 = new TapGestureRecognizer();
            tgr1.Tapped -= AppModel.Instance.Scan.Btn_FlashlightTapped;
            tgr1.Tapped += AppModel.Instance.Scan.Btn_FlashlightTapped;
            btn_flashlight_container.GestureRecognizers.Add(tgr1);

            btn_flashlight_AddRegScan_container.GestureRecognizers.Clear();
            var tgr2 = new TapGestureRecognizer();
            tgr2.Tapped -= AppModel.Instance.Scan.Btn_FlashlightTapped;
            tgr2.Tapped += AppModel.Instance.Scan.Btn_FlashlightTapped;
            btn_flashlight_AddRegScan_container.GestureRecognizers.Add(tgr2);

            btn_loginlogin_container.GestureRecognizers.Clear();
            var tgr3 = new TapGestureRecognizer();
            tgr3.Tapped -= Btn_LoginTapped;
            tgr3.Tapped += Btn_LoginTapped;
            btn_loginlogin_container.GestureRecognizers.Add(tgr3);


            //btn_toregistmore_container.GestureRecognizers.Clear();
            //var tgr3bmore = new TapGestureRecognizer();
            //tgr3bmore.Tapped -= Btn_toregistMoreTapped;
            //tgr3bmore.Tapped += Btn_toregistMoreTapped;
            //btn_toregistmore_container.GestureRecognizers.Add(tgr3bmore);

            btn_toregist_container.GestureRecognizers.Clear();
            var tgr3b = new TapGestureRecognizer();
            tgr3b.Tapped -= Btn_toregistTapped;
            tgr3b.Tapped += Btn_toregistTapped;
            btn_toregist_container.GestureRecognizers.Add(tgr3b);


            btn_addRegScan_container.GestureRecognizers.Clear();
            var tgr4 = new TapGestureRecognizer();
            tgr4.Tapped -= Btn_AddRegScanTapped;
            tgr4.Tapped += Btn_AddRegScanTapped;
            btn_addRegScan_container.GestureRecognizers.Add(tgr4);

            btn_addRegScan2_container.GestureRecognizers.Clear();
            var tgr5 = new TapGestureRecognizer();
            tgr5.Tapped -= Btn_AddRegScanTapped;
            tgr5.Tapped += Btn_AddRegScanTapped;
            btn_addRegScan2_container.GestureRecognizers.Add(tgr5);

            btn_ToRegScanManagement_container.GestureRecognizers.Clear();
            var tgr6 = new TapGestureRecognizer();
            tgr6.Tapped -= Btn_ToRegScanManagementTapped;
            tgr6.Tapped += Btn_ToRegScanManagementTapped;
            btn_ToRegScanManagement_container.GestureRecognizers.Add(tgr6);

            btn_back_inAddRegScan.GestureRecognizers.Clear();
            var tgr7 = new TapGestureRecognizer();
            tgr7.Tapped -= BackToLoginPage;
            tgr7.Tapped += BackToLoginPage;
            btn_back_inAddRegScan.GestureRecognizers.Add(tgr7);

            btn_back_RegManagement.GestureRecognizers.Clear();
            var tgr8 = new TapGestureRecognizer();
            tgr8.Tapped -= BackToLoginPage;
            tgr8.Tapped += BackToLoginPage;
            btn_back_RegManagement.GestureRecognizers.Add(tgr8);

            popupContainer_Alert_btn.GestureRecognizers.Clear();
            var tgr9 = new TapGestureRecognizer();
            tgr9.Tapped -= HideAlertMessage;
            tgr9.Tapped += HideAlertMessage;
            popupContainer_Alert_btn.GestureRecognizers.Add(tgr9);

            sw_autologin.IsToggled = AppModel.Instance.SettingModel.IsLoginSettingsReady && AppModel.Instance.SettingModel.SettingDTO.Autologin;
            entry_login_name.Text = AppModel.Instance.SettingModel.SettingDTO.LoginName;
            entry_login_password.Text = AppModel.Instance.SettingModel.SettingDTO.LoginPassword;

            await Task.Delay(1);
            entry_login_name.TextChanged -= LoginNameChangedHandeler;
            entry_login_name.TextChanged += LoginNameChangedHandeler;
            entry_login_password.TextChanged -= LoginPasswordChangedHandeler;
            entry_login_password.TextChanged += LoginPasswordChangedHandeler;

            hasInitializedHandlers = true;
        }


        private async void CheckLogin(bool smallcheck = false)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);


            if (!smallcheck)
            {
                LoginNow();// Anmeldung am Server
            }
            else
            {
                DateTime lastTokenDate = String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks)
                    ? DateTime.Now.AddDays(-365)
                    : new DateTime(long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks));
                DateTime nowDate = DateTime.Now.AddDays(-7);
                var d = (nowDate - lastTokenDate).TotalHours;

                // token vorhanden und Letzter erfolgreicher login ist nicht länger als 7 Tage!
                if (!String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LoginToken) && d < 0 && AppModel.Instance.Person != null)
                {
                    //Login Check mit Token ... erfolgreich
                    AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SettingDTO.GPSInfoHasShow = true;
                    AppModel.Instance.SettingModel.SaveSettings();
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    AppModel.Instance.PageNavigator.NavigateTo(TFPageNavigator.PAGE_MAINPAGE);
                    return;
                }
                // 7 Tage sind abgelaufen
                AppModel.Instance.SettingModel.SettingDTO.LoginToken = "";
                AppModel.Instance.SettingModel.SettingDTO.Autologin = false;
                AppModel.Instance.SettingModel.SaveSettings();
                LoginNow();
            }
        }
        private async void LoginNow()
        {
            IpmLoginResponse ipmLoginResponse = await Task.Run(() => { return AppModel.Instance.Connections.IpmLogin(false); });


            if (ipmLoginResponse == null || !ipmLoginResponse.success)
            {
                await Task.Delay(1);
                overlay.IsVisible = false;
                LoginFaild(ipmLoginResponse);
            }
            else
            {
                try
                {
                    AppModel.Instance.Person = ipmLoginResponse.person;
                    if (ipmLoginResponse.versionCheck != null)
                    {
                        AppModel.Instance.AppControll = ipmLoginResponse.versionCheck.AppControll;
                    }
                    if (AppModel.Instance.AppControll == null) { AppModel.Instance.AppControll = new AppControll(); }
                    AppControll.Save(AppModel.Instance, AppModel.Instance.AppControll);

                    // Wenn sich die Sprache geändert hat!
                    if (AppModel.Instance.AppControll.lang != "de" && AppModel.Instance.AppControll.lang != AppModel.Instance.Lang.lang && AppModel.Instance.AppControll.translation)
                    {
                        AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = null;
                    }

                    PersonWSO.SavePerson(AppModel.Instance, AppModel.Instance.Person);
                    AppModel.Instance.SettingModel.SettingDTO.LoginToken = ipmLoginResponse.sessionkey;
                    AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SettingDTO.GPSInfoHasShow = true;
                    AppModel.Instance.SettingModel.SaveSettings();
                    //try
                    //{
                    //    //await AppModel.Instance.Connections.SaveSettings();
                    //}
                    //catch (Exception) { }
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    AppModel.Instance.PageNavigator.NavigateTo(TFPageNavigator.PAGE_MAINPAGE);
                }
                catch (Exception e)
                {
                    var a = e.Message;
                }
            }
        }
        private async void LoginFaild(IpmLoginResponse ipmLoginResponse)
        {
            string m = "";
            if (ipmLoginResponse != null)
            {
                m = ipmLoginResponse.message;
            }
            else
            {
                m = "FEHLER: Muss Online gehen, kann aber nicht!";
            }
            if (m.ToLower().Contains("zugangsdaten unbekannt"))
            {
                AppModel.Instance.SettingModel.SettingDTO.LoginToken = "";
                AppModel.Instance.SettingModel.SettingDTO.Autologin = false;
                isInitialize = true;
                await Task.Delay(1);
                sw_autologin.IsToggled = false;
                await Task.Delay(1);
                isInitialize = false;
                AppModel.Instance.SettingModel.SaveSettings();
            }
            await DisplayAlertAsync("Anmeldung nicht möglich!", m, "Zurück");
        }

        //public async void SetAppControll()
        //{
        //    frame_PersonTimes.IsVisible = AppModel.Instance.AppControll.showPersonTimes;
        //}

        public Page GetPage(string subPage = "")
        {
            return this;
        }

    }
}
