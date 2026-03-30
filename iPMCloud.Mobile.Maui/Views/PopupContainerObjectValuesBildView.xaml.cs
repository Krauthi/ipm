using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class PopupContainerObjectValuesBildView : ContentView
    {
        public VerticalStackLayout PopupStack => popupContainer_objectvaluesbild_stack;
        public Image ImgPhoto => img_photo_objectvaluesbild;
        public Border BtnNewPhoto => btn_newphoto_objectvaluesbild;
        public Image BtnNewPhotoImg => btn_newphoto_objectvaluesbild_img;
        public Editor EditorNotice => editor_notice_objectvaluesbild;
        public Label LblSendErr => btn_send_objectvaluesbild_err;
        public Border BtnCancel => btn_cancel_objectvaluesbild;
        public Border BtnSend => btn_send_objectvaluesbild;

        public PopupContainerObjectValuesBildView()
        {
            InitializeComponent();
        }


        public void SetVisible(bool visible)
        {
            //Zählerfoto
            BtnNewPhotoImg.Source = AppModel.Instance.imagesBase.Cam;

            //Flashlight in ObjektValuesEdit ...
            BtnNewPhoto.GestureRecognizers.Clear();
            var tgr_btn_newphoto_objectvaluesbild = new TapGestureRecognizer();
            tgr_btn_newphoto_objectvaluesbild.Tapped += async (s, e) => 
                await AppModel.Instance.MainPage.btn_takePhotoForMeterstand(s, e);
            BtnNewPhoto.GestureRecognizers.Add(tgr_btn_newphoto_objectvaluesbild);

            BtnSend.GestureRecognizers.Clear();
            var tgr_btn_send_objectvaluesbild = new TapGestureRecognizer();
            tgr_btn_send_objectvaluesbild.Tapped += AppModel.Instance.MainPage.btn_sendPhotoForMeterstand;
            BtnSend.GestureRecognizers.Add(tgr_btn_send_objectvaluesbild);
            
            BtnCancel.GestureRecognizers.Clear();
            var tgr_btn_cancel_objectvaluesbild = new TapGestureRecognizer();
            tgr_btn_cancel_objectvaluesbild.Tapped += (object o, TappedEventArgs ev) =>
                {
                    AppModel.Instance.MainPage.RemoveObjektMeterStandBild(); 
                    popupContainer_objectvaluesbild.IsVisible = false; 
                };
            BtnCancel.GestureRecognizers.Add(tgr_btn_cancel_objectvaluesbild);

            popupContainer_objectvaluesbild.IsVisible = visible;

        }


    }
}
