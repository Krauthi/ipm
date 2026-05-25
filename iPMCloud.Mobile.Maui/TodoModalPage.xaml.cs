using iPMCloud.Mobile.Views;

namespace iPMCloud.Mobile
{
    public partial class TodoModalPage : ContentPage
    {
        public TodoModalPage()
        {
            InitializeComponent();
            RegisterEventHandlers();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            TodoPageView.SetVisible(true);
            TodoPageView.btn_todo_faelligTapped(null, null);
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
            TodoPageView.ListTodo.Children.Clear();
            this.Focus();
            await Navigation.PopModalAsync(animated: false);
        }
    }
}
