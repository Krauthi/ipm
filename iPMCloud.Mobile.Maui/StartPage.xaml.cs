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

        public AppModel model;
        public bool isInitialize = false;

        public StartPage()
        {
            InitializeComponent();
        }


        public StartPage(AppModel model)
        {
            isInitialize = true;
            this.model = model;
            InitializeComponent();
            InitStartPage();
            ShowDisconnected();
        }
        private void InitStartPage(bool switchCustomer = false)
        {

            btn_regScan_limg.Source = model.imagesBase.QrScan;
            //btn_regScanWarn_img.Source = model.imagesBase.WarnTriangleYellow;

            btn_flashlight_img.Source = model.imagesBase.Flashlight;
            btn_flashlight_AddRegScan_img.Source = model.imagesBase.Flashlight;

            img_gpsinfo.Source = model.imagesBase.Pin;

            btn_login_img.Source = model.imagesBase.Login;
            btn_toregist_img.Source = model.imagesBase.QrScan;
            btn_addRegScan_img.Source = model.imagesBase.AddImageWithe;
            btn_addRegScan2_img.Source = model.imagesBase.AddImageWithe;
            btn_ToRegScanManagement_img.Source = model.imagesBase.Change;
            img_logo.Source = model.imagesBase.Logo;
            img_backBtn_RegManagement.Source = model.imagesBase.DropLeftBlueDoubleImage;
            img_backBtn_inRegAddScan_img.Source = model.imagesBase.DropLeftBlueDoubleImage;

            lb_version.Text = "V" + model.Version;// + " (" + model.Build + ")";

            InitStartPageHandlers();

            InitPermission();

            if (model.SettingModel.IsCredentialsSettingsReady)//|| model.IsTest)
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
            model.UseExternHardware = true;
            model.Scan.ScanRegView(this, lay_regscan, MethodAfterScan);

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }
        public bool MethodAfterScan()
        {
            //StartGPS();
            model.UseExternHardware = false;
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
            model.CheckPermissions();
            if (!String.IsNullOrWhiteSpace(model.checkPermissionsMessage))
            {
                if (showAlert)
                {
                    model.checkPermissionsMessage = model.checkPermissionsMessage.Replace(";", "\n\n");
                    await DisplayAlert("Folgendes wird benötigt!", model.checkPermissionsMessage, "OK");
                    //model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                }
                return false;
            }
            if (inclGPS)
            {
                model.CheckPermissionGPS();
                if (!String.IsNullOrWhiteSpace(model.checkPermissionGPSMessage))
                {
                    if (showAlert)
                    {
                        model.checkPermissionGPSMessage = model.checkPermissionGPSMessage.Replace(";", "\n\n");
                        await DisplayAlert("Berechtigungsproblem!", model.checkPermissionGPSMessage, "OK");
                        //model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                    }
                    return false;
                }
                AppModel.Instance.InitGPSTimer();
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
            model.UseExternHardware = true;
            model.Scan.ScanAddRegView(this, lay_addregscan, MethodAfterAddScan, MethodAfterAddScanFail);

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
            model.Companies.ForEach(c =>
            {
                var isSelected = c.CustomerNumber == model.SettingModel.SettingDTO.CustomerNumber;
                var tgr = new TapGestureRecognizer();
                if (!isSelected)
                {
                    tgr.Tapped += (s, e) => { CompanySelected(s, e); };
                }
                Border companyView = Elements.GetCompanySelectionItem(c, model.imagesBase.Building, isSelected);
                companyView.GestureRecognizers.Clear();
                companyView.GestureRecognizers.Add(tgr);
                companyView.ClassId = c.CustomerNumber;
                var tgrDelete = new TapGestureRecognizer();
                if (!isSelected)
                {
                    tgrDelete.Tapped += (s, e) => { CompanyDeleted(s, e); };
                }
                Border xBtn = Elements.GetXButton(c, model.imagesBase.Trash, isSelected);
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
            model.UseExternHardware = false;
            lay_addregscan.Children.Clear();
            model.Person = null;
            entry_login_name.Text = "";
            entry_login_password.Text = "";
            sw_autologin.IsToggled = false;
            model.SettingModel.SettingDTO.LoginName = "";
            model.SettingModel.SettingDTO.LoginPassword = "";
            model.SettingModel.SettingDTO.Autologin = false;
            model.SettingModel.SettingDTO.LoginToken = "";
            model.SettingModel.SettingDTO.LastTokenDateTimeTicks = "";
            model.Connections.InitConnections();
            model.Connections.InitPNConnections();

            model.SettingModel.SaveSettings();
            ShowLoginPage();
            return true;
        }
        public bool MethodAfterAddScanFail()
        {
            model.UseExternHardware = false;
            lay_addregscan.Children.Clear();
            ShowLoginPage();
            return false;
        }


        private async void ShowLoginPage(bool switchCustomer = false)
        {
            AppModel.Instance.InitGPSTimer();
            if (model.Companies != null && model.Companies.Count > 1)
            {
                btn_addRegScan_frame.IsVisible = false;
                btn_ToRegScanManagement_frame.IsVisible = true;
            }
            else
            {
                btn_addRegScan_frame.IsVisible = true;
                btn_ToRegScanManagement_frame.IsVisible = false;
            }

            if (model.SettingModel.SettingDTO.Autologin && !String.IsNullOrWhiteSpace(model.SettingModel.SettingDTO.LoginToken) &&
                !model.State.IsBackTappedToLogin && !switchCustomer)
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

                model.State.IsBackTappedToLogin = false;

                RegScan_Container.IsVisible = false;
                Login_Container.IsVisible = true;
                AddRegScan_Container.IsVisible = false;
                RegManagement_Container.IsVisible = false;

                InitStartPageHandlers();
                lb_login_mandant.Text = model.SettingModel.SettingDTO.CustomerName;

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
            Company.AddUpdateCompany(model, model.SettingModel.SettingDTO);

            var child = ((StackLayout)((Border)s).Content);
            var customerNumber = child.ClassId;
            var company = model.Companies.Find(c => c.CustomerNumber == customerNumber);
            if (company != null)
            {
                model.SettingModel.SettingDTO = Company.ToSettingDTO(company);
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "");
                if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                model.SettingModel.SettingDTO.LoginToken = "";
                model.SettingModel.SettingDTO.LastTokenDateTimeTicks = "";
                entry_login_name.Text = model.SettingModel.SettingDTO.LoginName;
                entry_login_password.Text = model.SettingModel.SettingDTO.LoginPassword;
                sw_autologin.IsToggled = false;                //
                model.SettingModel.SaveSettings();

                model.Connections.InitConnections();
                model.Connections.InitPNConnections();
                InitStartPage(true);
            }
        }
        private async void CompanyDeleted(object s, EventArgs e)
        {

            var a = await DisplayAlert("Unternehmen entfernen?", "\n\nMöchten Sie wirklich das gewählte Unternehmen aus Ihrer App entfernen?", "JETZT ENTFERNEN", "ABBRECHEN");
            if (a)
            {
                var child = ((StackLayout)((Border)s).Content);
                var customerNumber = child.ClassId;
                var company = model.Companies.Find(c => c.CustomerNumber == customerNumber);
                if (company != null)
                {
                    if (Company.DeleteCompany(model, company))
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
            if (model.IsInternet)
            {
                model.SettingModel.SettingDTO.LoginName = entry_login_name.Text;
                model.SettingModel.SettingDTO.LoginPassword = entry_login_password.Text;
                //model.SettingModel.SettingDTO.LoginToken = "";
                model.SettingModel.SettingDTO.Autologin = sw_autologin.IsToggled;
                model.SettingModel.SaveSettings();
                CheckLogin();
            }
            else
            {
                await DisplayAlert("KEIN INTERNET!", "Für diese Aktion brauchen Sie eine Internetverbindung!", "OK");
            }
        }


        public bool showAddRegScanTapped = false;
        public async void Btn_AddRegScanTapped(object sender, EventArgs e)
        {
            if (showAddRegScanTapped) { return; }
            showAddRegScanTapped = true;
            var a = await DisplayAlert("Unternehmen hinzufügen?",
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
            var a = await DisplayAlert("Registrierung löschen?", "Sind Sie sich sicher das Sie diese Registrierung löschen möchten?\n\n", "JETZT LÖSCHEN", "ABBRECHEN");
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
            model.SettingModel.SettingDTO.Autologin = !model.SettingModel.SettingDTO.Autologin;
            sw_autologin.IsToggled = model.SettingModel.IsLoginSettingsReady && model.SettingModel.SettingDTO.Autologin;
            model.SettingModel.SaveSettings();
        }


        public void LoginNameChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_login_name.Text = e.NewTextValue.Replace(" ", String.Empty); };
            model.SettingModel.SettingDTO.LoginName = entry_login_name.Text;
        }

        public void LoginPasswordChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.IndexOf(" ") > -1) { entry_login_password.Text = e.NewTextValue.Replace(" ", String.Empty); };
            model.SettingModel.SettingDTO.LoginPassword = entry_login_password.Text;
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
            tgr1.Tapped -= model.Scan.Btn_FlashlightTapped;
            tgr1.Tapped += model.Scan.Btn_FlashlightTapped;
            btn_flashlight_container.GestureRecognizers.Add(tgr1);

            btn_flashlight_AddRegScan_container.GestureRecognizers.Clear();
            var tgr2 = new TapGestureRecognizer();
            tgr2.Tapped -= model.Scan.Btn_FlashlightTapped;
            tgr2.Tapped += model.Scan.Btn_FlashlightTapped;
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

            sw_autologin.IsToggled = model.SettingModel.IsLoginSettingsReady && model.SettingModel.SettingDTO.Autologin;
            entry_login_name.Text = model.SettingModel.SettingDTO.LoginName;
            entry_login_password.Text = model.SettingModel.SettingDTO.LoginPassword;

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
                DateTime lastTokenDate = String.IsNullOrWhiteSpace(model.SettingModel.SettingDTO.LastTokenDateTimeTicks)
                    ? DateTime.Now.AddDays(-365)
                    : new DateTime(long.Parse(model.SettingModel.SettingDTO.LastTokenDateTimeTicks));
                DateTime nowDate = DateTime.Now.AddDays(-7);
                var d = (nowDate - lastTokenDate).TotalHours;

                // token vorhanden und Letzter erfolgreicher login ist nicht länger als 7 Tage!
                if (!String.IsNullOrWhiteSpace(model.SettingModel.SettingDTO.LoginToken) && d < 0 && model.Person != null)
                {
                    //Login Check mit Token ... erfolgreich
                    model.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SettingDTO.GPSInfoHasShow = true;
                    model.SettingModel.SaveSettings();
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_MAINPAGE);
                    return;
                }
                // 7 Tage sind abgelaufen
                model.SettingModel.SettingDTO.LoginToken = "";
                model.SettingModel.SettingDTO.Autologin = false;
                model.SettingModel.SaveSettings();
                LoginNow();
            }
        }
        private async void LoginNow()
        {
            IpmLoginResponse ipmLoginResponse = await Task.Run(() => { return model.Connections.IpmLogin(false); });


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
                    model.Person = ipmLoginResponse.person;
                    if (ipmLoginResponse.versionCheck != null)
                    {
                        model.AppControll = ipmLoginResponse.versionCheck.AppControll;
                    }
                    if (model.AppControll == null) { model.AppControll = new AppControll(); }
                    AppControll.Save(model, model.AppControll);

                    // Wenn sich die Sprache geändert hat!
                    if (model.AppControll.lang != "de" && model.AppControll.lang != AppModel.Instance.Lang.lang && AppModel.Instance.AppControll.translation)
                    {
                        AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks = null;
                    }

                    PersonWSO.SavePerson(model, model.Person);
                    model.SettingModel.SettingDTO.LoginToken = ipmLoginResponse.sessionkey;
                    model.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SettingDTO.GPSInfoHasShow = true;
                    model.SettingModel.SaveSettings();
                    //try
                    //{
                    //    //await model.Connections.SaveSettings();
                    //}
                    //catch (Exception) { }
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_MAINPAGE);
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
                model.SettingModel.SettingDTO.LoginToken = "";
                model.SettingModel.SettingDTO.Autologin = false;
                isInitialize = true;
                await Task.Delay(1);
                sw_autologin.IsToggled = false;
                await Task.Delay(1);
                isInitialize = false;
                model.SettingModel.SaveSettings();
            }
            await DisplayAlert("Anmeldung nicht möglich!", m, "Zurück");
        }

        //public async void SetAppControll()
        //{
        //    frame_PersonTimes.IsVisible = model.AppControll.showPersonTimes;
        //}

        public Page GetPage(string subPage = "")
        {
            return this;
        }

    }
}
