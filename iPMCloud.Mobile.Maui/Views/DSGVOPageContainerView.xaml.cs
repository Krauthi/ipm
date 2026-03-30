using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class DSGVOPageContainerView : ContentView
    {
        public DSGVOPageContainerView()
        {
            InitializeComponent();
        }


        public void SetVisible(bool visible)
        {
            btn_back_dsgvo_img.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;
            
            btn_back_dsgvo.GestureRecognizers.Clear();
            var tgr_back_dsgvo = new TapGestureRecognizer();
            tgr_back_dsgvo.Tapped += AppModel.Instance.MainPage.btn_DSGVOBackTapped;
            btn_back_dsgvo.GestureRecognizers.Add(tgr_back_dsgvo);

            DSGVOPage_Container.IsVisible = visible;
        }



    }
}
