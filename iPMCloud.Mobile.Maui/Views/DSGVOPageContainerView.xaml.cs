using iPMCloud.Mobile.vo;
using iPMCloud.Mobile;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.Views
{
    public partial class DSGVOPageContainerView : AndroidBackBlockedModalPage
    {
        private static int _isModalOpen;

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
            if (Interlocked.Exchange(ref _isModalOpen, 1) == 1)
            {
                return;
            }

            try
            {
                var page = new DSGVOPageContainerView();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    callerPage.Navigation.PushModalAsync(page, animated: false));
            }
            catch
            {
                Interlocked.Exchange(ref _isModalOpen, 0);
                throw;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Interlocked.Exchange(ref _isModalOpen, 0);
        }
    }
}
