namespace iPMCloud.Mobile.Views
{
    public partial class SettingsPageView : ContentView
    {
        public Grid Container => SettingsPage_Container;

        public SettingsPageView()
        {
            InitializeComponent();
        }

        public void SetVisible(bool visible) => SettingsPage_Container.IsVisible = visible;
    }
}
