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
        private const int PreviewWarmupDelayMs = 120;
        private const int PreviewCameraToggleDelayMs = 120;
        private const int PreviewEnableResetDelayMs = 60;
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
            _previewCts?.Cancel();
            _previewCts = new CancellationTokenSource();
            var token = _previewCts.Token;
            AppModel.Logger.Info($"[ScanObjModalPage] OnAppearing start (Android). {GetReaderViewStateForLog()}");
            ReaderView.IsDetecting = false;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    AppModel.Logger.Info($"[ScanObjModalPage] Android preview workaround: delay {PreviewWarmupDelayMs}ms before reinit.");
                    await Task.Delay(PreviewWarmupDelayMs, token);

                    var originalCamera = ReaderView.CameraLocation;
                    var toggledCamera = originalCamera == CameraLocation.Rear ? CameraLocation.Front : CameraLocation.Rear;
                    await TrySetCameraLocationAsync(toggledCamera, originalCamera, token);
                    await Task.Delay(PreviewCameraToggleDelayMs, token);
                    await TrySetCameraLocationAsync(originalCamera, toggledCamera, token);

                    AppModel.Logger.Info("[ScanObjModalPage] Android preview workaround: ReaderView IsEnabled false -> true.");
                    ReaderView.IsEnabled = false;
                    await Task.Delay(PreviewEnableResetDelayMs, token);
                    ReaderView.IsEnabled = true;

                    AppModel.Logger.Info($"[ScanObjModalPage] Android preview workaround: enabling detection. {GetReaderViewStateForLog()}");
                    ReaderView.IsDetecting = true;
                    AppModel.Logger.Info($"[ScanObjModalPage] Android preview workaround complete. {GetReaderViewStateForLog()}");
                }
                catch (OperationCanceledException)
                {
                    AppModel.Logger.Info("[ScanObjModalPage] Android preview workaround cancelled.");
                }
                catch (Exception ex)
                {
                    AppModel.Logger.Error($"[ScanObjModalPage] Android preview workaround failed: {ex}");
                    ReaderView.IsDetecting = true;
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
                await Navigation.PopModalAsync(animated: false);
            });
        }

        private string GetReaderViewStateForLog()
        {
            return $"Page={Width:F0}x{Height:F0}, ReaderView={ReaderView.Width:F0}x{ReaderView.Height:F0}, " +
                   $"Visible={ReaderView.IsVisible}, Enabled={ReaderView.IsEnabled}, " +
                   $"Camera={ReaderView.CameraLocation}, Detecting={ReaderView.IsDetecting}";
        }

        private Task TrySetCameraLocationAsync(CameraLocation targetCamera, CameraLocation currentCamera, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.CompletedTask;

            try
            {
                AppModel.Logger.Info($"[ScanObjModalPage] Android preview workaround: camera toggle {currentCamera} -> {targetCamera}.");
                ReaderView.CameraLocation = targetCamera;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Warn($"[ScanObjModalPage] Android preview workaround: camera toggle {currentCamera} -> {targetCamera} skipped ({ex.Message}).");
            }

            return Task.CompletedTask;
        }
    }
    
}
