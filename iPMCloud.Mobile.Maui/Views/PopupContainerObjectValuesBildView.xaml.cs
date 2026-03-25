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
    }
}
