namespace iPMCloud.Mobile.Views
{
    public partial class DayOverPageView : ContentView
    {
        public Grid Container => DayOverPage_Container;

        public DayOverPageView()
        {
            InitializeComponent();
        }

        public void SetVisible(bool visible) => DayOverPage_Container.IsVisible = visible;

        // Expose child controls for access from MainPage code-behind
        public Image BtnBackDayoverImg => btn_back_dayover_img;
        public Border BtnBackDayover => btn_back_dayover;
        public Border BtnDayoverYes => btn_dayover_yes;
        public Border BtnDayoverNo => btn_dayover_no;
        public VerticalStackLayout LastDayOverStack => lastDayOverStack;
    }
}
