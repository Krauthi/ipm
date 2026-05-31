using iPMCloud.Mobile.Views;

namespace iPMCloud.Mobile
{
    public partial class TodoModalPage : ContentPage
    {
        private static int _isModalOpen;

        public TodoModalPage()
        {
            InitializeComponent();
            RegisterEventHandlers();
        }

        public static async Task ShowAsync(Page callerPage)
        {
            if (Interlocked.Exchange(ref _isModalOpen, 1) == 1)
            {
                return;
            }

            try
            {
                var page = new TodoModalPage();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    callerPage.Navigation.PushModalAsync(page, animated: false));
            }
            catch
            {
                Interlocked.Exchange(ref _isModalOpen, 0);
                throw;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            TodoPageView.SetVisible(true);
            TodoPageView.btn_todo_faelligTapped(null, null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Interlocked.Exchange(ref _isModalOpen, 0);
        }

        private void RegisterEventHandlers()
        {
            TodoPageView.BtnTodoBack.GestureRecognizers.Clear();
            var backTap = new TapGestureRecognizer();
            backTap.Tapped += OnBackTapped;
            TodoPageView.BtnTodoBack.GestureRecognizers.Add(backTap);
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            TodoPageView.ClearTodoList();
            this.Focus();
            await Navigation.PopModalAsync(animated: false);
        }
    }
}
