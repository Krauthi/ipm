using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class SettingsPageView : ContentView
    {
        //public Grid Container => SettingsPage_Container;

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
        }

        public void SetSendLog(bool visible)
        {
            btn_settings_sendlog.Opacity = visible ? 1 : 0.4;
            btn_settings_sendlog.IsEnabled = visible;
            btn_settings_sendlog.IsVisible = !visible;

            string lang = AppModel.Instance.Langs.Find(l => l.lang == AppModel.Instance.AppControll.lang)?.text;
            lb_settings_sel_trans.Text = lang != null ? lang: "Deutsch";
        }

        private void Settings_Log_includeCache_Switch_Toggled(object sender, ToggledEventArgs e)
        {
            AppModel.Instance.InclFilesAsJson = e.Value;            
        }

        public void SetVisible(bool visible)
        {


            btn_back_settings.GestureRecognizers.Clear();
            var tgr_back_settings = new TapGestureRecognizer();
            tgr_back_settings.Tapped += AppModel.Instance.MainPage.btn_SettingsBackTapped;
            btn_back_settings.GestureRecognizers.Add(tgr_back_settings);

            btn_settings_sendlog.GestureRecognizers.Clear();
            var tgr_namestacksend = new TapGestureRecognizer();
            tgr_namestacksend.Tapped += AppModel.Instance.MainPage.ShowSendLog;
            btn_settings_sendlog.GestureRecognizers.Add(tgr_namestacksend);


            btn_settings_sel_trans_lang.GestureRecognizers.Clear();
            var tgr_btn_settings_sel_trans_lang = new TapGestureRecognizer();
            tgr_btn_settings_sel_trans_lang.Tapped += AppModel.Instance.MainPage.OpenLanguage;
            btn_settings_sel_trans_lang.GestureRecognizers.Add(tgr_btn_settings_sel_trans_lang);



            btn_settings_sendlog.IsVisible = true; // BTN SendLOG wieder aneigen!

            //Einstellungen
            btn_back_settings_img.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;

            // Einstellungen Defaults
            lb_settings_synctimehours.Text = "" + AppModel.Instance.SettingModel.SettingDTO.SyncTimeHours;


            int countAll = AppModel.Instance.MainPage.GetAllSyncFromUploadCount();
            settings_count_positionen.Text = (countAll > 0 ? "" + countAll : "Keine Daten vorhanden");
            btn_settings_count_positionen.IsVisible = countAll > 0;

            SettingsPage_Container.IsVisible = visible;
            lb_settings_sel_trans.Text = AppModel.Instance.AppControll.lang;
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
