namespace iPMCloud.Mobile.Views
{
    public partial class DSGVOPageContainerView : ContentView
    {
        public Grid ContainerGrid => DSGVOPage_Container;
        public Border BtnBackDsgvo => btn_back_dsgvo;
        public Image BtnBackDsgvoImg => btn_back_dsgvo_img;

        public DSGVOPageContainerView()
        {
            InitializeComponent();
        }
    }
}
