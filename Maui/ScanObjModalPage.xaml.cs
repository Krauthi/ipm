using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.Helpers;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace iPMCloud.Mobile
{
    public partial class ScanObjModalPage : ContentPage
    {
        // Guards ScanAsync so only one instance of this modal can be open at a time.
        private static readonly SemaphoreSlim _scanSemaphore = new SemaphoreSlim(1, 1);

        private TaskCompletionSource<string> _tcs;
        private bool _completed; 
        private bool _isClosing;
#if ANDROID
        private CancellationTokenSource _previewCts;
#endif

        private ScanObjModalPage()
        {
            InitializeComponent();


            BindingContext = new OverlayViewModel(
                onFlashButtonClicked: () => { FlashCameraClicked(null, null); })
            {
                TopText = AppModel.Instance.ScanAddRegText,
                BottomText = AppModel.Instance.ScanAddRegTextSec,
            };

            ReaderView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.TwoDimensional,
                AutoRotate = true,
                Multiple = false,
                CharacterSet = "ISO-8859-1",
                DelayBetweenAnalyzingFrames = 30,
                InitialDelayBeforeAnalyzingFrames = 0,
                DelayBetweenContinuousScans = 0,
                CameraResolutionSelector = availableResolutions =>
                    availableResolutions
                        .OrderBy(r => Math.Abs((r.Width * r.Height) - (1280 * 720)))
                        .ThenBy(r => Math.Abs(r.Width - 1280) + Math.Abs(r.Height - 720))
                        .First()
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
            // Prevent concurrent opens: if a modal is already being shown, silently ignore the tap.
            if (!await _scanSemaphore.WaitAsync(0))
                return null;


            try
            {
                var hasCameraPermission = await PermissionHelper.EnsureCameraPermissionAsync(
                    "ScanObjModalPage.ScanAsync",
                    async () => await callerPage.DisplayAlertAsync(
                        "Kamerazugriff verweigert",
                        "Bitte erlauben Sie den Kamerazugriff in den Einstellungen.",
                        "OK"));

                if (!hasCameraPermission)
                {
                    AppModel.Logger.Error("ScanObjModalPage.ScanAsync: Kamerazugriff verweigert");
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
            finally
            {
                _scanSemaphore.Release();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _completed = false;
            _isClosing = false;
            btn_back_inAddRegScan.InputTransparent = false;

#if ANDROID
            // On some Android devices (e.g. Xiaomi/MIUI) the camera surface is not yet
            // ready when OnAppearing fires, causing a black preview even though frames
            // are still decoded. A short delay gives the renderer time to attach the
            // SurfaceView before detection is enabled.
            _previewCts?.Cancel();
            _previewCts = new CancellationTokenSource();
            var token = _previewCts.Token;
            //System.Diagnostics.Debug.WriteLine(
            //    $"[ScanObjModalPage] OnAppearing – scheduling IsDetecting=true after 300 ms " +
            //    $"(Width={Width:F0}, Height={Height:F0})");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    ReaderView.Options = new BarcodeReaderOptions
                    {
                        Formats = BarcodeFormats.TwoDimensional,
                        AutoRotate = true,
                        Multiple = false,
                        CharacterSet = "ISO-8859-1",
                        DelayBetweenAnalyzingFrames = 30,
                        InitialDelayBeforeAnalyzingFrames = 0,
                        DelayBetweenContinuousScans = 0,
                        CameraResolutionSelector = availableResolutions =>
                            availableResolutions
                                .OrderBy(r => Math.Abs((r.Width * r.Height) - (1280 * 720)))
                                .ThenBy(r => Math.Abs(r.Width - 1280) + Math.Abs(r.Height - 720))
                                .First()
                    };
                    await Task.Delay(300, token);
                    //System.Diagnostics.Debug.WriteLine("[ScanObjModalPage] IsDetecting = true");
                    ReaderView.IsDetecting = true;
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("[ScanObjModalPage] IsDetecting start cancelled");
                }
            });
#else
            ReaderView.IsDetecting = true;
#endif
        }

        protected override void OnDisappearing()
        {
#if ANDROID
            _previewCts?.Cancel();
#endif
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

                if (IsThisPageTopModal())
                {
                    await Navigation.PopModalAsync(animated: false);
                }
            });
        }
    }
    
}
