namespace iPMCloud.Mobile.Views
{
    public partial class PersonTimesPageView : ContentView
    {
        public Grid Container => PersonTimesPage_Container;

        public PersonTimesPageView()
        {
            InitializeComponent();
        }

        public void SetVisible(bool visible) => PersonTimesPage_Container.IsVisible = visible;

        // Expose child controls for access from MainPage code-behind
        public ScrollView ListPersontimesScroll => list_persontimes_scroll;
        public Picker PickPersontimesYear => pick_persontimes_year;
        public Picker PickPersontimesMonth => pick_persontimes_month;
        public VerticalStackLayout ListPersontimes => list_persontimes;
        public VerticalStackLayout StackPersontimesTop => stack_persontimes_top;
        public StackLayout StackPersontimesBottom => stack_persontimes_bottom;
        public Image BtnPersontimesBackImg => btn_persontimes_back_img;
        public Image WarnPersontimesLimg => warn_persontimes_limg;
        public Border BtnPersontimesBack => btn_persontimes_back;
        public StackLayout BtnPersontimeLoad => btn_persontime_load;
    }
}
