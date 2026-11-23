using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Views
{
    public partial class SettingsPageView : ContentPage
    {
        private static int _isModalOpen;
        private readonly iPMCloud.Mobile.MainPage _mainPage;
        //private const int LogSendDelayMilliseconds = 2000;

        public SettingsPageView(iPMCloud.Mobile.MainPage mainPage = null)
        {
            _mainPage = mainPage;
            InitializeComponent();

            lb_settings_sel_trans.Text = AppModel.Instance.Lang.text.Replace("(Standard)", "");


            btn_settings_count_positionen.GestureRecognizers.Clear();
            var tgr_btn_settings_count_positionen = new TapGestureRecognizer();
            tgr_btn_settings_count_positionen.Tapped += btn_SettingsSyncUploadTapped;
            btn_settings_count_positionen.GestureRecognizers.Add(tgr_btn_settings_count_positionen);

            
            btn_settings_synctimesub.GestureRecognizers.Clear();
            var tgr_synctimesub = new TapGestureRecognizer();
            tgr_synctimesub.Tapped += btn_settings_synctimesub_Tapped;
            btn_settings_synctimesub.GestureRecognizers.Add(tgr_synctimesub);
            btn_settings_synctimeadd.GestureRecognizers.Clear();
            var tgr_synctimeadd = new TapGestureRecognizer();
            tgr_synctimeadd.Tapped += btn_settings_synctimeadd_Tapped;
            btn_settings_synctimeadd.GestureRecognizers.Add(tgr_synctimeadd);
            btn_back_settings.GestureRecognizers.Clear();
            var tgr_back_settings = new TapGestureRecognizer();
            tgr_back_settings.Tapped += async (s, e) =>
            {
                if (Navigation.ModalStack.Contains(this))
                {
                    await Navigation.PopModalAsync(animated: false);
                }
            };
            btn_back_settings.GestureRecognizers.Add(tgr_back_settings);

            btn_settings_sendlog.GestureRecognizers.Clear();
            var tgr_namestacksend = new TapGestureRecognizer();
            tgr_namestacksend.Tapped += ShowSendLogAsyncNow;
            btn_settings_sendlog.GestureRecognizers.Add(tgr_namestacksend);

            btn_settings_sendlogB.GestureRecognizers.Clear();
            var tgr_namestacksendB = new TapGestureRecognizer();
            tgr_namestacksendB.Tapped += ShowSendLogAsync;
            btn_settings_sendlogB.GestureRecognizers.Add(tgr_namestacksendB);
        }

        public static async Task ShowAsync(Page callerPage)
        {
            if (Interlocked.Exchange(ref _isModalOpen, 1) == 1)
            {
                return;
            }

            try
            {
                var page = new SettingsPageView(callerPage as iPMCloud.Mobile.MainPage);
                await MainThread.InvokeOnMainThreadAsync(() =>
                    callerPage.Navigation.PushModalAsync(page, animated: false));
            }
            catch
            {
                Interlocked.Exchange(ref _isModalOpen, 0);
                throw;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var mainPage = _mainPage ?? AppModel.Instance.MainPage;

            btn_settings_sendlog.IsVisible = false;
            grid_settings_sendlog.IsVisible = false;
            kli = 0;
            __logKli = 0;

            lb_settings_synctimehours.Text = "" + (AppModel.Instance.SettingModel?.SettingDTO?.SyncTimeHours ?? 1);

            int countAll = AppModel.Instance.GetOpenUploadsCount();
            settings_count_positionen.Text = (countAll > 0 ? "" + countAll : "Keine Daten vorhanden");
            btn_settings_count_positionen.IsVisible = countAll > 0;
            btn_settings_count_positionen_list2.IsVisible = countAll > 0;

            string currentLang = AppModel.Instance.AppControll?.lang;
            string lang = AppModel.Instance.Langs?.Find(l => l != null && l.lang == currentLang)?.text;
            lb_settings_sel_trans.Text = lang != null ? lang : "Deutsch";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Interlocked.Exchange(ref _isModalOpen, 0);
        }

        public void SetSendLog()
        {
            btn_settings_sendlog.IsVisible = false;
            grid_settings_sendlog.IsVisible = false;
        }

        private int __logKli = 0;
        private async void ShowSendLogAsync(object sender, EventArgs e)
        {
            if (__logKli > 6)
            {
                btn_settings_sendlog.IsVisible = true;
                grid_settings_sendlog.IsVisible = true;
            }
            else
            {
                __logKli++;
            }
        }

        private async void ShowSendLogAsyncNow(object sender, EventArgs e)
        {
            __logKli = 0;
            var confirm = await DisplayAlertAsync(
                "Support-Log senden",
                "Möchten Sie die Log-Daten jetzt an den Support senden?",
                "Senden",
                "Abbrechen");
            if (!confirm) return;

            SetSendLog();
            var ok = AppModel.Instance.SendLogZipFile();
            if (!ok)
            {
                await DisplayAlertAsync("Fehler", "Log-Daten konnten nicht gesendet werden.", "OK");
            }
        }

        private void Settings_Log_includeCache_Switch_Toggled(object sender, ToggledEventArgs e)
        {
            AppModel.Instance.InclFilesAsJson = e.Value;
        }

        public async void btn_settings_synctimesub_Tapped(object sender, EventArgs e)
        {
            var settingDto = AppModel.Instance.SettingModel?.SettingDTO;
            if (settingDto == null || settingDto.SyncTimeHours == 0) { return; }
            settingDto.SyncTimeHours--;
            AppModel.Instance.SettingModel.SaveSettings();
            lb_settings_synctimehours.Text = "" + settingDto.SyncTimeHours;
        }
        public async void btn_settings_synctimeadd_Tapped(object sender, EventArgs e)
        {
            var settingDto = AppModel.Instance.SettingModel?.SettingDTO;
            if (settingDto == null || settingDto.SyncTimeHours == 15) { return; }
            settingDto.SyncTimeHours++;
            AppModel.Instance.SettingModel.SaveSettings();
            lb_settings_synctimehours.Text = "" + settingDto.SyncTimeHours;
        }

        public async void btn_SettingsSyncUploadTapped(object sender, EventArgs e)
        {
            var mainPage = _mainPage ?? AppModel.Instance.MainPage;
            mainPage?.CheckAllSyncFromUpload();
            settings_count_positionen.Text = "Versucht hochzuladen";
            btn_settings_count_positionen.IsVisible = false;
        }

        public async void SettingsShowListDown()
        {
            var mainPage = _mainPage ?? AppModel.Instance.MainPage;
            settings_positionen_list.IsVisible = true;
        }

        public async void btn_SettingsSyncUploadListClose(object sender, EventArgs e)
        {
            settings_positionen_list.IsVisible = false;
        }




        private int kli = 0;
        public async void btn_SettingsShowListTapped(object sender, EventArgs e)
        { 
            if(kli > 6)
            {
                btn_SettingsShowListTappedNow(sender, e);
            }
            else
            {
                kli++;
            }
        }


        public async void btn_SettingsShowListTappedNow(object sender, EventArgs e)
        {
            kli = 0;

            var mainPage = _mainPage ?? AppModel.Instance.MainPage;
            settings_positionen_list.Clear();
            settings_positionen_list.IsVisible = true;
            if (AppModel.Instance.GetOpenUploadsCount() > 0)
            {

                var allDayOverWSO = DayOverWSO.LoadAllFromUploadStack(AppModel.Instance);
                if (allDayOverWSO != null && allDayOverWSO.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"Feierabend (" + allDayOverWSO.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var day in allDayOverWSO)
                    {
                        var lb_zaehler = new Label { Text = $"GUID: {day.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (Feierabend)",
                            Command = new Command(async () =>
                            {
                                var result = await SyncDayOver_Done(new List<DayOverWSO> { day });
                                if (result != null && result.success)
                                {
                                    DayOverWSO.DeleteFromUploadStack(AppModel.Instance,day);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (Feierabend {day.guid}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }


                var allCheck = CheckClass.LoadAllFromUploadStack();
                if (allCheck != null && allCheck.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"Checklisten (" + allCheck.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var check in allCheck)
                    {
                        var lb_zaehler = new Label { Text = $"CheckId/GUID: {check.id} - {check.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (Checkliste: " + check.id + ")",
                            Command = new Command(async () =>
                            {
                                var checkJson = CheckClass.LoadAllFromUploadStack_AsJson();
                                var result = await SyncChecks_Done(check);
                                if (result != null && result.success)
                                {
                                    CheckClass.DeleteFromUploadStack(check);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (CheckId {check.id}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }


                var allCheckBem = CheckLeistungAntwortBemImg.LoadAllFromStack();
                if (allCheckBem != null && allCheckBem.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"Checklisten-Bemerkungen (" + allCheckBem.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var checkBI in allCheckBem)
                    {
                        var lb_zaehler = new Label { Text = $"CheckBemrkungId/GUID: {checkBI.bem_id} - {checkBI.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (CheckBemerkung: " + checkBI.bem_id + ")",
                            Command = new Command(async () =>
                            {
                                var result = await SyncChecksBemImg_Done(checkBI);
                                if (result != null && result.success)
                                {
                                    CheckLeistungAntwortBemImg.DeleteFromStack(checkBI);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (CheckBemerkung {checkBI.bem_id}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }


                var allBemerkungWSO = BemerkungWSO.LoadAllFromUploadStack(AppModel.Instance);
                if (allBemerkungWSO != null && allBemerkungWSO.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"Bemerkungen (" + allBemerkungWSO.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var bem in allBemerkungWSO)
                    {

                        var lb_zaehler = new Label { Text = $"Bemerkung-Guid: {bem.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (Bemerkung)",
                            Command = new Command(async () =>
                            {
                                var result = await SyncSingleNotice_Done(bem);
                                if (result != null && result.success)
                                {
                                    BemerkungWSO.DeleteFromUploadStack(AppModel.Instance,bem);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (Bemerkung {bem.guid}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }


                var allLeistungPackWSO = LeistungPackWSO.LoadAllFromUploadStack(AppModel.Instance);
                if (allLeistungPackWSO != null && allLeistungPackWSO.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"LeistungsPack (" + allLeistungPackWSO.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var pack in allLeistungPackWSO)
                    {
                        var lb_zaehler = new Label { Text = $"GUID: {pack.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (LeistungsPack)",
                            Command = new Command(async () =>
                            {
                                var result = pack.ticket == null ? await SyncPosition_Done(pack) : await SyncPositionTicket_Done(pack);
                                if (result != null && result.success)
                                {
                                    LeistungPackWSO.DeleteFromUploadStack(AppModel.Instance, pack);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (LeistungsPack {pack.guid}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }


                //var allBildWSO = BildWSO.LoadAllFromStack();

                var allObjektDataWSO = ObjektDataWSO.LoadAllFromUploadStack(AppModel.Instance);
                if (allObjektDataWSO != null && allObjektDataWSO.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"ObjektData Zähler (" + allObjektDataWSO.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var pack in allObjektDataWSO)
                    {
                        var lb_zaehler = new Label { Text = $"ZählerId/GUID: {pack.id} - {pack.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (Obj.Data Zähler)",
                            Command = new Command(async () =>
                            {
                                var result = await SyncObjectValues_Done(new List<ObjektDataWSO> { pack });
                                if (result != null && result.success)
                                {
                                    ObjektDataWSO.DeleteFromUploadStack(AppModel.Instance, pack);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (Zähler  {pack.id} - {pack.guid}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }


                var allObjektDatenBildWSO = ObjektDatenBildWSO.LoadAllFromUploadStack(AppModel.Instance);
                if (allObjektDatenBildWSO != null && allObjektDatenBildWSO.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"ObjektData Zähler BILD (" + allObjektDatenBildWSO.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var pack in allObjektDatenBildWSO)
                    {
                        var lb_zaehler = new Label { Text = $"Zähler BILD-GUID: {pack.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (Obj.Data Zähler BILD)",
                            Command = new Command(async () =>
                            {
                                var result = await SyncObjectValueBild_Done(pack);
                                if (result != null && result.success)
                                {
                                    ObjektDatenBildWSO.DeleteFromUploadStack(AppModel.Instance, pack);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (Zähler BILD {pack.id} - {pack.guid}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }

                var allTicketChat = TicketChat.LoadAllFromUploadStack();
                if (allTicketChat != null && allTicketChat.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"Ticket Chat (" + allTicketChat.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var pack in allTicketChat)
                    {
                        var lb_zaehler = new Label { Text = $"Ticket Chat: {pack.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (Ticket Chat)",
                            Command = new Command(async () =>
                            {
                                var result = await SyncTicketChat_Done(pack);
                                if (result != null && result.success)
                                {
                                    TicketChat.DeleteFromUploadStack(pack);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (Ticket Chat {pack.id} - {pack.guid}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }

                var allTicketBesitzerStatusUpload = TicketBesitzerStatusUpload.LoadAllFromUploadStack();
                if (allTicketBesitzerStatusUpload != null && allTicketBesitzerStatusUpload.Count > 0)
                {
                    settings_positionen_list.Children.Add(new Label { Text = $"Ticket BesitzerStatus (" + allTicketBesitzerStatusUpload.Count + ")", Margin = new Thickness(0, 15, 0, 0) });
                    foreach (var pack in allTicketBesitzerStatusUpload)
                    {
                        var lb_zaehler = new Label { Text = $"Ticket BesitzerStatus-GUID: {pack.guid}" };
                        settings_positionen_list.Children.Add(lb_zaehler);
                        Button btn_zaehler = null;
                        btn_zaehler = new Button
                        {
                            BackgroundColor = Color.FromArgb("#99009900"),
                            TextColor = Color.FromArgb("#ffffff"),
                            HorizontalOptions = LayoutOptions.End,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0),
                            CornerRadius = 9,
                            HeightRequest = 28,
                            FontSize = 11,
                            Text = "Hochladen (Obj.Data Zähler BILD)",
                            Command = new Command(async () =>
                            {
                                var result = await SyncTicketBesitzerStatus_Done(pack);
                                if (result != null && result.success)
                                {
                                    TicketBesitzerStatusUpload.DeleteFromUploadStack(pack);
                                    settings_positionen_list.Children.Remove(btn_zaehler);
                                    lb_zaehler.Text = "Erfolgreich hochgeladen";
                                    lb_zaehler.TextColor = Color.FromArgb("#00aa00");
                                }
                                else
                                {
                                    lb_zaehler.Text = $"Fehler beim Hochladen (Ticket BesitzerStatus {pack.guid}): {result?.message}";
                                    lb_zaehler.TextColor = Color.FromArgb("#aa0000");
                                }
                            })
                        };
                        settings_positionen_list.Children.Add(btn_zaehler);
                    }
                }


            }
        }

        private async Task<DayOverResponse> SyncDayOver_Done(List<DayOverWSO> dayOvers)
        {
            DayOverResponse dayOverResponse = (await Task.Run(() => { return AppModel.Instance.Connections.DayOverSync(dayOvers); }));
            if (dayOverResponse == null)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Feierabend FEHLGESCHLAGEN => SyncDayOver_Done : dayOverResponse == null ");
                return null;
            }
            if (dayOverResponse != null && !dayOverResponse.success)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Feierabend FEHLGESCHLAGEN => SyncDayOver_Done : " + dayOverResponse.message);
                return dayOverResponse;
            }
            return dayOverResponse;
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
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Checkliste hochladen FEHLGESCHLAGEN => SyncChecks_Done : CheckResponse == null ");
                return null;
            }
            if (checkResponse != null && !checkResponse.success)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Checkliste hochladen FEHLGESCHLAGEN => SyncChecks_Done : " + checkResponse.message);
                return checkResponse;
            }
            return checkResponse;
        }

        private async Task<ChecksResponse> SyncChecksBemImg_Done(CheckLeistungAntwortBemImg pic)
        {
            ChecksResponse response = await Task.Run(() => { return AppModel.Instance.Connections.SetCheckABemImg(pic); });
            if (response == null)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Checkliste Bild(er) FEHLGESCHLAGEN => SyncChecksBemImg_Done : response == null ");
                return null;
            }
            if (response != null && !response.success)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Checkliste Bild(er) FEHLGESCHLAGEN => SyncChecksBemImg_Done : " + response.message);
                return response;
            }
            return response;
        }

        private async Task<SingleNoticeResponse> SyncSingleNotice_Done(BemerkungWSO bem)
        {
            SingleNoticeResponse noticeResponse = (await Task.Run(() => { return AppModel.Instance.Connections.SingleNoticeSync(bem); }));
            if (noticeResponse == null)
            {
                AppModel.Logger.Warn("WARN:  (0): Bemerkungen FEHLGESCHLAGEN => SyncNotice_Done : noticeResponse == null ");
                return null;
            }
            if (noticeResponse != null && !noticeResponse.success)
            {
                AppModel.Logger.Warn("WARN:  (0): Bemerkungen FEHLGESCHLAGEN => SyncNotice_Done : " + noticeResponse.message);
                return noticeResponse;
            }
            return noticeResponse;
        }

        private async Task<PositionResponse> SyncPositionTicket_Done(LeistungPackWSO pack)
        {
            PositionResponse positionResponse = await AppModel.Instance.Connections.TicketPositionSync(pack);
            if (positionResponse == null)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / LeistungsTicketpakete FEHLGESCHLAGEN => SyncPositionTicket_Done : positionResponse == null");
                return null;
            }
            else if (positionResponse != null && !positionResponse.success)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / LeistungsTicketpakete FEHLGESCHLAGEN => SyncPositionTicket_Done : " + positionResponse.message);
                return positionResponse;
            }
            return positionResponse;
        }
        
        private async Task<PositionResponse> SyncPosition_Done(LeistungPackWSO pack)
        {
            PositionResponse positionResponse = await AppModel.Instance.Connections.PositionSync(pack);
            if (positionResponse == null)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Leistungspakete FEHLGESCHLAGEN => SyncPosition_Done : positionResponse == null");
                return null;
            }
            else if (positionResponse != null && !positionResponse.success)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Leistungspakete FEHLGESCHLAGEN => SyncPosition_Done : " + positionResponse.message);
                return positionResponse;
            }
            return positionResponse;            
        }

        private async Task<ObjectValuesResponse> SyncObjectValues_Done(List<ObjektDataWSO> objectValues)
        {
            ObjectValuesResponse objectValuesResponse = (await Task.Run(() => { return AppModel.Instance.Connections.ObjectValuesSync(objectValues); }));
            if (objectValuesResponse == null)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Zaehlerstaende FEHLGESCHLAGEN => SyncObjectValues_Done : objectValuesResponse == null");
                return null;
            }
            if (objectValuesResponse != null && !objectValuesResponse.success)
            {
                AppModel.Logger.Warn("WARN:  (0): EINSTELLUNGEN / Zaehlerstaende FEHLGESCHLAGEN => SyncObjectValues_Done : " + objectValuesResponse.message);
                return objectValuesResponse;
            }
            return objectValuesResponse;
        }

        private async Task<ObjectValueBildResponse> SyncObjectValueBild_Done(ObjektDatenBildWSO value)
        {
            ObjectValueBildResponse response = (await Task.Run(() => { return AppModel.Instance.Connections.ObjectValueBildSync(value); }));
            if (response == null)
            {
                AppModel.Logger.Warn("WARN:  (0): Zaehlerbilder FEHLGESCHLAGEN => SyncObjectValueBild_Done : response == null");
                return null;
            }
            if (response != null && !response.success)
            {
                AppModel.Logger.Warn("WARN:  (0): Zaehlerbilder FEHLGESCHLAGEN => SyncObjectValueBild_Done : " + response.message);
                return response;
            }
            return response;
        }

        private async Task<TicketStatusResponse> SyncTicketBesitzerStatus_Done(TicketBesitzerStatusUpload value)
        {
            TicketStatusResponse response = (await Task.Run(() => { return AppModel.Instance.Connections.SetTicketBesitzerStatus(value); }));
            if (response == null)
            {
                AppModel.Logger.Warn("WARN:  (0): TicketBesitzerStatus FEHLGESCHLAGEN => SyncTicketBesitzerStatus_Done : response == null");
                return null;
            }
            if (response != null && !response.success)
            {
                AppModel.Logger.Warn("WARN:  (0): TicketBesitzerStatus FEHLGESCHLAGEN => SyncTicketBesitzerStatus_Done : " + response.message);
                return response;
            }
            return response;
        }

        private async Task<TicketChatResponse> SyncTicketChat_Done(TicketChat value)
        {
            TicketChatResponse response = (await Task.Run(() => { return AppModel.Instance.Connections.AddTicketChat(value); }));
            if (response == null)
            {
                AppModel.Logger.Warn("WARN:  (0): TicketChat FEHLGESCHLAGEN => SyncTicketChat_Done : response == null");
                return null;
            }
            if (response != null && !response.success)
            {
                AppModel.Logger.Warn("WARN:  (0): TicketChat FEHLGESCHLAGEN => SyncTicketChat_Done : " + response.message);
                return response;
            }
            return response;
        }









    }
}
