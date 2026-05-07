using ZXing.Net.Maui;

namespace iPMCloud.Mobile
{

    public partial class ScannerPage : ContentPage
    {
        private readonly Action<string> _onResult;
        private bool _completed;

        public ScannerPage(Action<string> onResult)
        {
            InitializeComponent();
            _onResult = onResult;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _completed = false;
            ReaderView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            // Kamera sauber stoppen, wenn du die Seite verlässt
            ReaderView.IsDetecting = false;
            base.OnDisappearing();
        }

        private void OnSwitchCameraClicked(object sender, EventArgs e)
        {
            ReaderView.CameraLocation =
                ReaderView.CameraLocation == CameraLocation.Rear
                    ? CameraLocation.Front
                    : CameraLocation.Rear;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            if (_completed) return;
            _completed = true;

            ReaderView.IsDetecting = false;
            await Navigation.PopAsync();
        }

        private async void ReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_completed) return;

            var value = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            _completed = true;

            // Event kann nicht auf UI-Thread sein
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                StatusLabel.Text = "Gefunden…";
                ReaderView.IsDetecting = false;

                // Result an vorherige Seite zurückgeben
                _onResult(value);

                // Zurück zur vorherigen Seite
                await Navigation.PopAsync();
            });
        }
    }
}