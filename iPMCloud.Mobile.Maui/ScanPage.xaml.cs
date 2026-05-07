
using ZXing.Net.Maui;

namespace iPMCloud.Mobile
{

    public partial class ScanPage : ContentPage
    {

        private readonly Action<string> _onScanResult;
        private bool _completed;

        public ScanPage(Action<string> onScanResult)
        {
            InitializeComponent();
            _onScanResult = onScanResult;
        }

        private void ShowCaller()
        {
            CallerPanel.IsVisible = true;
            ScannerPanel.IsVisible = false;

            ReaderView.IsDetecting = false; // Kamera aus
            _completed = false;
            StatusLabel.Text = "";
        }

        private void ShowScanner()
        {
            CallerPanel.IsVisible = false;
            ScannerPanel.IsVisible = true;

            _completed = false;
            StatusLabel.Text = "Scan bereit…";
            ReaderView.IsDetecting = true; // Kamera an
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ShowCaller(); // Startzustand
        }

        protected override void OnDisappearing()
        {
            ReaderView.IsDetecting = false;
            base.OnDisappearing();
        }

        private void OnStartScannerClicked(object sender, EventArgs e) => ShowScanner();

        private void OnCancelScanClicked(object sender, EventArgs e) => ShowCaller();

        private void OnSwitchCameraClicked(object sender, EventArgs e)
        {
            ReaderView.CameraLocation =
                ReaderView.CameraLocation == CameraLocation.Rear
                    ? CameraLocation.Front
                    : CameraLocation.Rear;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // zurück zur StartPage ohne Result
        }

        private async void ReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_completed) return;

            var value = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            _completed = true;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                StatusLabel.Text = "Gefunden…";
                ReaderView.IsDetecting = false;

                // Result an StartPage geben
                _onScanResult(value);

                // zurück zur StartPage (weil Caller+Scanner ja in derselben Page sind)
                await Navigation.PopAsync();
            });
        }
    }
}