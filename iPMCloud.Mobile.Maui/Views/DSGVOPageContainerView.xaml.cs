using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Views
{
    public partial class DSGVOPageContainerView : ContentPage
    {
        public DSGVOPageContainerView()
        {
            InitializeComponent();
            btn_back_dsgvo.GestureRecognizers.Clear();
            var tgr_back_dsgvo = new TapGestureRecognizer();
            tgr_back_dsgvo.Tapped += async (s, e) => await Navigation.PopModalAsync(animated: false);
            btn_back_dsgvo.GestureRecognizers.Add(tgr_back_dsgvo);
        }

        public static async Task ShowAsync(Page callerPage)
        {
            var page = new DSGVOPageContainerView();
            await MainThread.InvokeOnMainThreadAsync(() =>
                callerPage.Navigation.PushModalAsync(page, animated: false));
        }
    }
}
