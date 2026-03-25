using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class DayOverPageView : ContentView
    {
        public Grid Container => DayOverPage_Container;

        public DayOverPageView()
        {
            InitializeComponent();



        }

        public void SetVisible(bool visible)
        {
            btn_back_dayover.GestureRecognizers.Clear();
            var tgr_back_dayover = new TapGestureRecognizer();
            tgr_back_dayover.Tapped += AppModel.Instance.MainPage.btn_DayOverBackTapped;
            btn_back_dayover.GestureRecognizers.Add(tgr_back_dayover);

            btn_dayover_yes.GestureRecognizers.Clear();
            var tgr_dayover_yes = new TapGestureRecognizer();
            tgr_dayover_yes.Tapped += btn_DayOverYesTapped;
            btn_dayover_yes.GestureRecognizers.Add(tgr_dayover_yes);

            //btn_dayover_no.GestureRecognizers.Clear();
            //var tgr_dayover_no = new TapGestureRecognizer();
            //tgr_dayover_no.Tapped += AppModel.Instance.MainPage.btn_DayOverBackTapped;
            //btn_dayover_no.GestureRecognizers.Add(tgr_dayover_no);

            btn_back_dayover_img.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;
            DayOverPage_Container.IsVisible = visible;
        }

        public VerticalStackLayout LastDayOverStack => lastDayOverStack;



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
                personid = AppModel.Instance.Person.id,
                gruppeid = AppModel.Instance.Person.gruppeid,
            };
            DayOverWSO.Save(AppModel.Instance, d);
            DayOverWSO.ToUploadStack(AppModel.Instance, d);
            AppModel.Instance.MainPage.SyncDayOver();
            var dt = new DateTime(d.endticks);
            AppModel.Instance.MainPage
                .SetDayOverLastDate(dt.ToString("dd.MM.yyyy") + " - " + dt.ToString("HH:mm"));

            if (AppModel.Instance.LastBuilding != null)
            {
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
            }
            AppModel.Instance.MainPage.ShowMainPage();
        }


    }


}
