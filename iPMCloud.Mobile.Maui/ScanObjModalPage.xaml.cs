using iPMCloud.Mobile.vo;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace iPMCloud.Mobile
{
    public partial class ScanObjModalPage : ContentPage
    {
        private TaskCompletionSource<string> _tcs;
        private bool _completed; 
        private bool _isClosing;

        private ScanObjModalPage()
        {
            InitializeComponent();


            BindingContext = new OverlayViewModel(
                onFlashButtonClicked: () => { FlashCameraClicked(null, null); })
            {
                TopText = AppModel.Instance.ScanAddRegText,
                BottomText = AppModel.Instance.ScanAddRegTextSec,
            };


            btn_back_inAddRegScan.GestureRecognizers.Clear();
            var tgr7 = new TapGestureRecognizer();
            tgr7.Tapped -= OnCancelClicked;
            tgr7.Tapped += OnCancelClicked;
            btn_back_inAddRegScan.GestureRecognizers.Add(tgr7);
        }

        private bool IsThisPageTopModal()
        {
            var modalStack = Navigation?.ModalStack;
            return modalStack != null
                   && modalStack.Count > 0
                   && ReferenceEquals(modalStack[^1], this);
        }

        private async Task CloseModalSafeAsync(string result = null)
        {
            if (_isClosing)
                return;

            _isClosing = true;
            _completed = true;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    btn_back_inAddRegScan.InputTransparent = true;

                    ReaderView.IsTorchOn = false;
                    ReaderView.IsDetecting = false;

#if ANDROID
                    await Task.Delay(250);
#endif

                    _tcs?.TrySetResult(result);

                    if (IsThisPageTopModal())
                        await Navigation.PopModalAsync(animated: false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CloseModalSafeAsync error: {ex}");
                    _tcs?.TrySetResult(result);
                }
            });
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
                await callerPage.DisplayAlertAsync(
                    "Kamerazugriff verweigert",
                    "Bitte erlauben Sie den Kamerazugriff in den Einstellungen.",
                    "OK");
                return null;
            }

            var page = new ScanObjModalPage();
            page._tcs = new TaskCompletionSource<string>();

            // PushModalAsync works without a NavigationPage wrapper because
            // modal presentation is handled at the OS / Window level.
            await MainThread.InvokeOnMainThreadAsync(() =>
                callerPage.Navigation.PushModalAsync(page, animated: false));

            return await page._tcs.Task;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _completed = false;
            _isClosing = false;
            btn_back_inAddRegScan.InputTransparent = false;
            ReaderView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            // Stop the camera feed whenever the page leaves the screen.
            ReaderView.IsTorchOn = false;
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

        private void FlashCameraClicked(object sender, EventArgs e)
        {
            ReaderView.IsTorchOn = !ReaderView.IsTorchOn;
        }

        private async void OnCancelClicked(object sender, TappedEventArgs e)
        {
            await CloseModalSafeAsync(null);
        }

        private async void ReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_completed || _isClosing)
                return;

            var value = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            await CloseModalSafeAsync(value);
        }


        private async void ReaderView_BarcodesDetected_old(object sender, BarcodeDetectionEventArgs e)
        {
            if (_completed) return;

            var value = e.Results?.FirstOrDefault()?.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            _completed = true;

            // BarcodesDetected may fire on a background thread – marshal to UI thread.
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                ReaderView.IsTorchOn = false;
                ReaderView.IsDetecting = false;
                _tcs?.TrySetResult(value);
                await Navigation.PopModalAsync(animated: false);
            });
        }
    }
    
}
