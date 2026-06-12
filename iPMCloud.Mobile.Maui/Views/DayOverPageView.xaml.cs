using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Views
{
    public partial class DayOverPageView : ContentPage
    {
        private static int _isModalOpen;

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
            if (Interlocked.Exchange(ref _isModalOpen, 1) == 1)
            {
                return;
            }

            try
            {
                var page = new DayOverPageView();
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Interlocked.Exchange(ref _isModalOpen, 0);
        }

        public async void btn_DayOverYesTapped(object sender, EventArgs e)
        {
            var instance = AppModel.Instance;
            if (instance?.Person == null || instance.MainPage == null)
                return;

            var geo = instance.LocationStr;
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

            var geoParts = geo?.Split(';');
            var latin = geoParts != null && geoParts.Length > 0 ? geoParts[0] : "";
            var lonin = geoParts != null && geoParts.Length > 1 ? geoParts[1] : "";

            var d = new DayOverWSO
            {
                endticks = DateTime.Now.Ticks,
                latin = latin,
                lonin = lonin,
                messagein = geoMessage,
                personid = instance.Person.id,
                gruppeid = instance.Person.gruppeid,
            };
            DayOverWSO.Save(instance, d);
            DayOverWSO.ToUploadStack(instance, d);
            
            instance.MainPage.CheckAllSyncFromUpload(); //AppModel.Instance.MainPage.SyncDayOver();
            var dt = new DateTime(d.endticks);
            instance.MainPage
                .SetDayOverLastDate(dt.ToString("dd.MM.yyyy") + " - " + dt.ToString("HH:mm"));

            if (instance.LastBuilding != null)
            {
                // Zurücksetzten aller States für die Auswahl der Ausführungen
                //AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
            }
            await Navigation.PopModalAsync(animated: false);
        }
    }
}
