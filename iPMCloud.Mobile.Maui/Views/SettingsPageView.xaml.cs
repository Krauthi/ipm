using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Views
{
    public partial class SettingsPageView : ContentPage
    {
        private static int _isModalOpen;
        //private const int LogSendDelayMilliseconds = 2000;

        public SettingsPageView()
        {
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
            tgr_back_settings.Tapped += async (s, e) => await Navigation.PopModalAsync(animated: false);
            btn_back_settings.GestureRecognizers.Add(tgr_back_settings);

            btn_settings_sendlog.GestureRecognizers.Clear();
            var tgr_namestacksend = new TapGestureRecognizer();
            tgr_namestacksend.Tapped += ShowSendLogAsync;
            btn_settings_sendlog.GestureRecognizers.Add(tgr_namestacksend);
        }

        public static async Task ShowAsync(Page callerPage)
        {
            if (Interlocked.Exchange(ref _isModalOpen, 1) == 1)
            {
                return;
            }

            try
            {
                var page = new SettingsPageView();
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

            SetSendLog(true);

            lb_settings_synctimehours.Text = "" + AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours;

            int countAll = AppModel.Instance.MainPage.GetAllSyncFromUploadCount();
            settings_count_positionen.Text = (countAll > 0 ? "" + countAll : "Keine Daten vorhanden");
            btn_settings_count_positionen.IsVisible = countAll > 0;

            string lang = AppModel.Instance.Langs.Find(l => l.lang == AppModel.Instance.AppControll.lang)?.text;
            lb_settings_sel_trans.Text = lang != null ? lang : "Deutsch";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Interlocked.Exchange(ref _isModalOpen, 0);
        }

        public void SetSendLog(bool visible)
        {
            btn_settings_sendlog.IsVisible = visible;
        }

        private async void ShowSendLogAsync(object sender, EventArgs e)
        {
            var confirm = await DisplayAlertAsync(
                "Support-Log senden",
                "Möchten Sie die Log-Daten jetzt an den Support senden?",
                "Senden",
                "Abbrechen");
            if (!confirm) return;

            SetSendLog(false);
            var ok = AppModel.Instance.SendLogZipFile();
            //await Task.Delay(LogSendDelayMilliseconds);
            //SetSendLog(true);

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
            if (AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours == 0) { return; }
            AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours--;
            AppModel.Instance.SettingModel.SaveSettings();
            lb_settings_synctimehours.Text = "" + AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours;
        }
        public async void btn_settings_synctimeadd_Tapped(object sender, EventArgs e)
        {
            if (AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours == 15) { return; }
            AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours++;
            AppModel.Instance.SettingModel.SaveSettings();
            lb_settings_synctimehours.Text = "" + AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours;
        }

        public async void btn_SettingsSyncUploadTapped(object sender, EventArgs e)
        {
            AppModel.Instance.MainPage.CheckAllSyncFromUpload();
            settings_count_positionen.Text = "Versucht hochzuladen";
            btn_settings_count_positionen.IsVisible = false;
        }

    }
}
