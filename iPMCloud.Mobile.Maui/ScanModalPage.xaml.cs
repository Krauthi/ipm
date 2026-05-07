using ZXing.Net.Maui;

namespace iPMCloud.Mobile
{
    public partial class ScanModalPage : ContentPage
    {
        private TaskCompletionSource<string> _tcs;
        private bool _completed;

        private ScanModalPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens the barcode scanner as a modal page and returns the scanned value,
        /// or null if the user cancels or camera permission is denied.
        /// Can be called from any thread; modal presentation is marshalled to the UI thread internally.
        /// </summary>
        public static async Task<string> ScanAsync(Page callerPage)
        {
            // Check / request camera permission before opening the scanner.
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                await callerPage.DisplayAlert(
                    "Kamerazugriff verweigert",
                    "Bitte erlauben Sie den Kamerazugriff in den Einstellungen.",
                    "OK");
                return null;
            }

            var page = new ScanModalPage();
            page._tcs = new TaskCompletionSource<string>();

            // PushModalAsync works without a NavigationPage wrapper because
            // modal presentation is handled at the OS / Window level.
            await MainThread.InvokeOnMainThreadAsync(() =>
                callerPage.Navigation.PushModalAsync(page));

            return await page._tcs.Task;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _completed = false;
            ReaderView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            // Stop the camera feed whenever the page leaves the screen.
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
            _tcs?.TrySetResult(null);
            await Navigation.PopModalAsync();
        }

        private async void ReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_completed) return;

            var value = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            _completed = true;

            // BarcodesDetected may fire on a background thread – marshal to UI thread.
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                ReaderView.IsDetecting = false;
                _tcs?.TrySetResult(value);
                await Navigation.PopModalAsync();
            });
        }
    }
}
