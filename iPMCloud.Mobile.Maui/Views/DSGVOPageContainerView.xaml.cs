using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class DSGVOPageContainerView : ContentView
    {
        public Grid ContainerGrid => DSGVOPage_Container;
        public Border BtnBackDsgvo => btn_back_dsgvo;

        public DSGVOPageContainerView()
        {
            InitializeComponent();
            btn_back_dsgvo_img.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;
        }
    }
}
