using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
//using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
//https://docs.microsoft.com/de-de/xamarin/essentials/preferences?tabs=android

namespace iPMCloud.Mobile
{
    public partial class StartPage : ContentPage
    {
        // private BackgroundWorker backgroundWorker = new BackgroundWorker();

        public bool isInitialize { get; set; } = false;
        private bool _isUpdatingLoginName = false;
        private bool _isUpdatingLoginPassword = false;

        public StartPage()
        {
            isInitialize = true;
            InitializeComponent();
            //if(StorageMigration.HasMigrateIpmFolder())
            //{
                //InitStartPage();
                //ShowDisconnected();
            //}
            //else
            //{
            //    popupContainer_migration.IsVisible = true;
            //    Task.Run(async () =>
            //    {
            //        var migrated = await StorageMigration.MigrateIpmFolderAsync();
            //        if (migrated)
            //        {
            //            MainThread.BeginInvokeOnMainThread(() =>
            //            {
            //                popupContainer_migration.IsVisible = false;
            //                InitStartPage();
            //                ShowDisconnected();
            //            });
            //        }
            //        else
            //        {
            //            MainThread.BeginInvokeOnMainThread(async () =>
            //            {
            //                popupContainer_migration.IsVisible = false;
            //                await DisplayAlertAsync("Fehler", "Die Migration der Daten ist fehlgeschlagen. Bitte kontaktieren Sie den Support.", "OK");
            //            });
            //        }
            //    });
            //}
        }


        public void StartPageAgain()
        {
            isInitialize = true;
            InitStartPage();
            ShowDisconnected();
        }

        public void InitStartPage(bool switchCustomer = false)
        {
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
            //Reg/Scan_Container.IsVisible = false;
            Login_Container.IsVisible = false;
            //AddReg/Scan_Container.IsVisible = false;
            RegManagement_Container.IsVisible = false;

            //InitStartPageHandlers();

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }

        private async void ShowRegScan()
        {

            overlay.IsVisible = true;
            await Task.Delay(1);
            //Reg/Scan_Container.IsVisible = true;
            Login_Container.IsVisible = false;
            //AddReg/Scan_Container.IsVisible = false;
            RegManagement_Container.IsVisible = false;


            var result = await ScanModalPage.ScanAsync(this);
            if (!string.IsNullOrWhiteSpace(result))
            {
                var sp = result
                    .Replace("https://", "http://")
                    .Replace("httpss://", "https://")
                    .Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                if (sp.Length < 3)
                {
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    await DisplayAlertAsync("QR-Code nicht gültig!",
                                            "Dieser QR-Code kann für die Registrierung des Unternehmens mit der iPM-Cloud-App nicht verwendet werden.",
                                            "OK");
                    return;
                }
                else
                {

                    AppModel.Instance.UseExternHardware = true;
                    //var newScanSettings = new SettingDTO
                    //{
                    //    ServerUrl = sp[0],
                    //    CustomerNumber = sp[1],
                    //    CustomerName = sp[2]
                    //};
                    var cn = AppModel.Instance.SettingModel.SettingDTO.CustomerNumber;
                    if (!string.IsNullOrWhiteSpace(sp[0]) &&
                        !string.IsNullOrWhiteSpace(sp[1]) &&
                        !string.IsNullOrWhiteSpace(sp[2]) &&
                        sp[1] != cn)
                    {
                        var newScanSettings = new SettingDTO
                        {
                            ServerUrl = sp[0],
                            CustomerNumber = sp[1],
                            CustomerName = sp[2]
                        };

                        string directoryPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "ipm/" + newScanSettings.CustomerNumber);

                        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                        Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

                        AppModel.Instance.SettingModel.SettingDTO = newScanSettings;
                        AppModel.Instance.SettingModel.SaveSettings();

                        AppModel.Instance.UseExternHardware = false;

                        BeforeLogin_Container.IsVisible = false;
                        //StartGPS();
                        popupContainer_gpsinfo.IsVisible = true;
                        await Task.Delay(1);
                        overlay.IsVisible = false;
                    }
                    else
                    {
                        await Task.Delay(1);
                        overlay.IsVisible = false;
                        AppModel.Logger.Error("QR-Code nicht erkannt!" + " Dieser QR-Code kann für die Registrierung des Unternehmens mit der iPM-Cloud-App nicht verwendet werden!");
                        await DisplayAlertAsync("QR-Code nicht erkannt!",
                                                "Dieser QR-Code kann für die Registrierung des Unternehmens mit der iPM-Cloud-App nicht verwendet werden.",
                                                "OK");
                        AppModel.Instance.UseExternHardware = false;
                    }
                }
            }
            else
            {
                //AppModel.Logger.Error("Keine Kamera!" + " Vermutlich ist die Berechtigung der Kamera nicht gesetzt!");
                //await DisplayAlertAsync("Keine Kamera!",
                //                        "Vermutlich ist die Berechtigung der Kamera nicht gesetzt!",
                //                        "OK");

                await Task.Delay(1);
                overlay.IsVisible = false;
                AppModel.Instance.UseExternHardware = false;
            }
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
            try
            {
                if (inclGPS)
                {
                    var checkPermissionGPSMessage = await AppModel.Instance.CheckPermissionGPS();
                    if (!String.IsNullOrWhiteSpace(checkPermissionGPSMessage))
                    {
                        if (showAlert)
                        {
                            checkPermissionGPSMessage = checkPermissionGPSMessage.Replace(";", "\n\n");
                            await DisplayAlertAsync("Berechtigungsproblem!", checkPermissionGPSMessage, "OK");
                            //model.PageNavigator.NavigateTo(TFPageNavigator.PAGE_CLOSEAPP);
                        }
                        return false;
                    }
                    await AppModel.Instance.InitGPSTimer();
                }
                else
                {
                    _ = await AppModel.Instance.CheckPermissionCam();
                }
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissions failed: " + ex.Message + " - " + ex.StackTrace);
                return false;
            }
        }



        public void ShowDisconnected()
        {
            try
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
            catch (Exception ex) { 
                AppModel.Logger.Error("ERROR: ShowDisconnected failed: " + ex.Message + " - " + ex.StackTrace);
            }
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
            overlay.IsVisible = true;
            await Task.Delay(1);

            var result = await ScanModalPage.ScanAsync(this);
            if (!string.IsNullOrWhiteSpace(result))
            {
                var sp = result
                    .Replace("https://", "http://")
                    .Replace("httpss://", "https://")
                    .Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                if (sp.Length < 3)
                {
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    await DisplayAlertAsync("QR-Code nicht gültig!",
                                            "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.",
                                            "OK");
                    return;
                }

                AppModel.Instance.UseExternHardware = true;
                var newScanSettings = new SettingDTO
                {
                    ServerUrl = sp[0],
                    CustomerNumber = sp[1],
                    CustomerName = sp[2]
                };
                var cn = AppModel.Instance.SettingModel.SettingDTO.CustomerNumber;
                if (!string.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                    !string.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                    !string.IsNullOrWhiteSpace(newScanSettings.CustomerName) &&
                    newScanSettings.CustomerNumber != cn)
                {
                    Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

                    string directoryPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ipm/" + newScanSettings.CustomerNumber);

                    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                    AppModel.Instance.SettingModel.SettingDTO = newScanSettings;
                    AppModel.Instance.SettingModel.SaveSettings();

                    AppModel.Instance.UseExternHardware = false;

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
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    ShowLoginPage();
                }
                else
                {
                    await Task.Delay(1);
                    overlay.IsVisible = false;
                    AppModel.Logger.Error("QR-Code nicht erkannt oder existiert schon!" + " Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden!");
                    await DisplayAlertAsync("QR-Code nicht erkannt oder existiert schon!",
                                            "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.",
                                            "OK");
                    AppModel.Instance.UseExternHardware = false;
                }
            }
            else
            {
                await Task.Delay(1);
                overlay.IsVisible = false;
                //AppModel.Logger.Error("Keine Kamera!" + " Vermutlich ist die Berechtigung der Kamera nicht gesetzt!");
                //await DisplayAlertAsync("Keine Kamera!",
                //                        "Vermutlich ist die Berechtigung der Kamera nicht gesetzt!",
                //                        "OK");
                AppModel.Instance.UseExternHardware = false;
            }
        }


        private async void ShowRegManagementScan()
        {
            isInitialize = true;
            overlay.IsVisible = true;
            await Task.Delay(1);

            BeforeLogin_Container.IsVisible = false;
            //Reg/Scan_Container.IsVisible = false;
            //Login_Container.IsVisible = false;
            //AddReg/Scan_Container.IsVisible = false;
            RegManagement_Container.IsVisible = true;

            InitStartPageHandlers();
            lay_selectcompany_container.Children.Clear();
            AppModel.Instance.Companies.ForEach(c =>
            {
                var isSelected = c.CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber;
                
                    Grid companyView = Elements.GetCompanySelectionItem(c, isSelected);
                if (!isSelected)
                {
                    companyView.GestureRecognizers.Clear();
                    var tgr = new TapGestureRecognizer();
                    tgr.Tapped += async (s, e) => { await CompanySelected(s, e); };
                    companyView.GestureRecognizers.Add(tgr);
                }
                    companyView.ClassId = c.CustomerNumber;

                    // Löschbutton erstmal raus !!!
                    //var tgrDelete = new TapGestureRecognizer();
                    //if (!isSelected)
                    //{
                    //    tgrDelete.Tapped += (s, e) => { CompanyDeleted(s, e); };
                    //}
                    //Border xBtn = Elements.GetXButton(c, AppModel.Instance.imagesBase.Trash, isSelected);
                    //xBtn.GestureRecognizers.Clear();
                    //xBtn.GestureRecognizers.Add(tgrDelete);
                    //xBtn.ClassId = c.CustomerNumber;
                var stack = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Fill,
                    Children = { companyView },
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    //Children = { companyView, xBtn }  - Löschen entfernt 
                };
                lay_selectcompany_container.Children.Add(stack);
                
            });

            await Task.Delay(1);
            overlay.IsVisible = false;
            isInitialize = false;
        }



        private async void ShowLoginPage(bool switchCustomer = false)
        {
            try
            {
                RegManagement_Container.IsVisible = false;
                await AppModel.Instance.InitGPSTimer();
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

                    Login_Container.IsVisible = true;

                    InitStartPageHandlers();
                    lb_login_mandant.Text = AppModel.Instance.SettingModel.SettingDTO.CustomerName;

                    if (switchCustomer && AppModel.Instance.SettingModel.SettingDTO.Autologin)
                    {
                        await AppModel.Instance.InitBuildings();
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
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: ShowLoginPage failed: " + ex.Message + " - " + ex.StackTrace);
                overlay.IsVisible = false;  // ⬇️ Safety-Reset
                isInitialize = false;
            }
        }

        private async Task CompanySelected(object s, EventArgs e)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        overlay.IsVisible = true;
                        await Task.Delay(1);

                        var toCstomerNumber = ((Grid)s).ClassId;
                        var company = AppModel.Instance.Companies.Find(c => c.CustomerNumber == toCstomerNumber);
                        if (company != null)
                        {
                            AppModel.Logger.Info("INFO: Company wechsel von " + AppModel.Instance.SettingModel.SettingDTO.CustomerName
                                + " zu " + company.CustomerName);
                            // Vorherige aktive Company/SettingDTO speichern
                            Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

                            AppModel.Instance.SettingModel.SettingDTO = Company.ToSettingDTO(company);
                            AppModel.Instance.SettingModel.SaveSettings();

                            string directoryPath = Path.Combine(Environment.GetFolderPath(
                                Environment.SpecialFolder.LocalApplicationData), "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "");
                            if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                            //AppModel.Instance.SettingModel.SettingDTO.LoginToken = "";
                            //AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "";
                            entry_login_name.Text = AppModel.Instance.SettingModel.SettingDTO.LoginName;
                            entry_login_password.Text = AppModel.Instance.SettingModel.SettingDTO.LoginPassword;
                            sw_autologin.IsToggled = AppModel.Instance.SettingModel.SettingDTO.Autologin;
                            AppModel.Instance.SelectedBuilding = null;
                            AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding(); // incl. SaveSettings
                            AppModel.Instance.Lang = Lang.Load();
                            await AppModel.Instance.InitBuildings();
                            AppModel.Logger.Info("INFO: Wechsel starte mit -- " + company.CustomerName);
                        }

                        AppModel.Instance.Connections.InitConnections();
                        //AppModel.Instance.Connections.InitPNConnections();

                        // UI-Operationen im MainThread ausführen
                        RegManagement_Container.IsVisible = false;
                        await Task.Delay(100); // Kurze Verzögerung für UI-Update
                        overlay.IsVisible = false;
                        isInitialize = false;
                        InitStartPage(true);
                    }
                    catch (Exception innerEx)
                    {
                        AppModel.Logger.Error("ERROR: CompanySelected inner failed: " + innerEx.Message + " - " + (innerEx.StackTrace ?? ""));
                        // Sicherstellen, dass UI nicht eingefroren bleibt
                        RegManagement_Container.IsVisible = false;
                        overlay.IsVisible = false;
                        isInitialize = false;
                    }
                });
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CompanySelected failed: " + ex.Message + " - " + (ex.StackTrace ?? ""));
                // Sicherstellen, dass UI nicht eingefroren bleibt
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    RegManagement_Container.IsVisible = false;
                    overlay.IsVisible = false;
                    isInitialize = false;
                });
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

        public async void Btn_toregist_more_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync("http://www.ipm-cloud.de/", BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // An unexpected error occured. No browser may be installed on the device.
                AppModel.Logger.Error("ERROR: Btn_toregist_more_Tapped failed: " + ex.Message + " - " + (ex.StackTrace ?? ""));
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
            catch (Exception ex)
            {
                // An unexpected error occured. No browser may be installed on the device.
                AppModel.Logger.Error("ERROR: Btn_toregistMoreTapped failed: " + ex.Message + " - " + (ex.StackTrace ?? ""));
            }
        }


        public async void Btn_LoginTapped(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: Btn_LoginTapped failed: " + ex.Message + " - " + (ex.StackTrace ?? ""));
                await DisplayAlertAsync("FEHLER!", "Beim Anmelden ist ein Fehler aufgetreten. Bitte versuchen Sie es erneut!", "OK");
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
            //if (isInitialize) { return; }
            //AppModel.Instance.SettingModel.SettingDTO.Autologin = !AppModel.Instance.SettingModel.SettingDTO.Autologin;
            //sw_autologin.IsToggled = AppModel.Instance.SettingModel.IsLoginSettingsReady && AppModel.Instance.SettingModel.SettingDTO.Autologin;
            //AppModel.Instance.SettingModel.SaveSettings();
        }


        public void LoginNameChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingLoginName) { return; }
            try
            {
                var original = e.NewTextValue ?? string.Empty;
                var sanitized = original.Replace(" ", string.Empty);
                if (sanitized != original)
                {
                    _isUpdatingLoginName = true;
                    try
                    {
                        entry_login_name.Text = sanitized;
                    }
                    finally
                    {
                        _isUpdatingLoginName = false;
                    }
                }
                AppModel.Instance.SettingModel.SettingDTO.LoginName = sanitized;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: LoginNameChangedHandeler failed: " + ex.Message + " - " + (ex.StackTrace ?? ""));
            }
        }

        public void LoginPasswordChangedHandeler(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingLoginPassword) { return; }
            try
            {
                var original = e.NewTextValue ?? string.Empty;
                var sanitized = original.Replace(" ", string.Empty);
                if (sanitized != original)
                {
                    _isUpdatingLoginPassword = true;
                    try
                    {
                        entry_login_password.Text = sanitized;
                    }
                    finally
                    {
                        _isUpdatingLoginPassword = false;
                    }
                }
                AppModel.Instance.SettingModel.SettingDTO.LoginPassword = sanitized;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: LoginPasswordChangedHandeler failed: " + ex.Message + " - " + (ex.StackTrace ?? ""));
            }
        }

       // public bool hasInitializedHandlers { get; set; } = false;
        public async void InitStartPageHandlers()
        {
            //if (hasInitializedHandlers) { return; }

            try
            {

                btn_endselectedwork.GestureRecognizers.Clear();
                var tgr1btn_endselectedwork = new TapGestureRecognizer();
                tgr1btn_endselectedwork.Tapped -= Btn_GPSInfoTapped;
                tgr1btn_endselectedwork.Tapped += Btn_GPSInfoTapped;
                btn_endselectedwork.GestureRecognizers.Add(tgr1btn_endselectedwork);

                btn_loginlogin_container.GestureRecognizers.Clear();
                var tgr3 = new TapGestureRecognizer();
                tgr3.Tapped -= Btn_LoginTapped;
                tgr3.Tapped += Btn_LoginTapped;
                btn_loginlogin_container.GestureRecognizers.Add(tgr3);


                btn_toregist_container.GestureRecognizers.Clear();
                var tgr3b = new TapGestureRecognizer();
                tgr3b.Tapped -= Btn_toregistTapped;
                tgr3b.Tapped += Btn_toregistTapped;
                btn_toregist_container.GestureRecognizers.Add(tgr3b);

                btn_toregist_more_container.GestureRecognizers.Clear();
                var tgr3c = new TapGestureRecognizer();
                tgr3c.Tapped -= Btn_toregist_more_Tapped;
                tgr3c.Tapped += Btn_toregist_more_Tapped;
                btn_toregist_more_container.GestureRecognizers.Add(tgr3c);


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

                //btn_back_inAddRegScan.GestureRecognizers.Clear();
                //var tgr7 = new TapGestureRecognizer();
                //tgr7.Tapped -= BackAddRegScanToLoginPage;
                //tgr7.Tapped += BackAddRegScanToLoginPage;
                //btn_back_inAddRegScan.GestureRecognizers.Add(tgr7);

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
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: InitStartPageHandlers failed: " + ex.Message + " - " + ex.StackTrace);
            }
            //hasInitializedHandlers = false;
        }


        private async void CheckLogin(bool smallcheck = false)
        {
            try
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
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckLogin failed: " + ex.Message + " - " + ex.StackTrace);
                overlay.IsVisible = false;  // ⬇️ Safety-Reset
                isInitialize = false;
            }
        }
        private async void LoginNow()
        {
            IpmLoginResponse ipmLoginResponse = null;

            try
            {
                ipmLoginResponse = await Task.Run(() => { return AppModel.Instance.Connections.IpmLogin(false); });
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: LoginNow failed: " + ex.Message + " - " + (ex.StackTrace ?? ""));
                // ⬇️ WICHTIG: overlay immer zurücksetzen!
                overlay.IsVisible = false;
                isInitialize = false;
                LoginFaild(null);
                return;
            }

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
                    AppModel.Logger.Error("ERROR: LoginNow failed: " + e.Message + " - " + e.StackTrace);
                    // ⬇️ WICHTIG: overlay immer zurücksetzen!
                    overlay.IsVisible = false;
                    isInitialize = false;
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

        private void OnOverlayTapped(object sender, EventArgs e)
        {
            // Implementierung hier - z.B. das Overlay ausblenden
            //if (popupContainer_infodialog != null)
            //{
            //    popupContainer_infodialog.IsVisible = false;
            //}
        }
    }
}
