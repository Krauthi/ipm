using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Views
{
    public partial class DayOverPageView : ContentPage
    {
        public DayOverPageView()
        {
            InitializeComponent();
            btn_back_dayover.GestureRecognizers.Clear();
            var tgr_back_dayover = new TapGestureRecognizer();
            tgr_back_dayover.Tapped += async (s, e) => await Navigation.PopModalAsync(animated: false);
            btn_back_dayover.GestureRecognizers.Add(tgr_back_dayover);

            btn_dayover_yes.GestureRecognizers.Clear();
            var tgr_dayover_yes = new TapGestureRecognizer();
            tgr_dayover_yes.Tapped += btn_DayOverYesTapped;
            btn_dayover_yes.GestureRecognizers.Add(tgr_dayover_yes);
        }

        public VerticalStackLayout LastDayOverStack => lastDayOverStack;

        public static async Task ShowAsync(Page callerPage)
        {
            var page = new DayOverPageView();
            await MainThread.InvokeOnMainThreadAsync(() =>
                callerPage.Navigation.PushModalAsync(page, animated: false));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LastDayOverStack.Children.Clear();
            var dayOvers = DayOverWSO.LoadAll(AppModel.Instance);
            dayOvers.ForEach(d =>
            {
                var dt = new DateTime(d.endticks);
                LastDayOverStack.Children.Add(new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center,
                    BackgroundColor = Color.FromArgb("#042d53"),
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
                            TextColor = Color.FromArgb("#ffffff"),
                        }
                    }
                });
            });
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
                //AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
            }
            await Navigation.PopModalAsync(animated: false);
        }
    }
}
