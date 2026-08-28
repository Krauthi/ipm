using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.Helpers;
using iPMCloud.Mobile.Services;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace iPMCloud.Mobile
{
    public partial class ScanModalPage : ContentPage
    {
        // Guards ScanAsync so only one instance of this modal can be open at a time.
        private static readonly SemaphoreSlim _scanSemaphore = new SemaphoreSlim(1, 1);

        private TaskCompletionSource<string> _tcs;
        private bool _completed; 
        private bool _isClosing;
#if ANDROID
        private CancellationTokenSource _previewCts;
#endif

        private ScanModalPage()
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
                    "ScanModalPage.ScanAsync",
                    async () => await callerPage.DisplayAlertAsync(
                        "Kamerazugriff verweigert",
                        "Bitte erlauben Sie den Kamerazugriff in den Einstellungen.",
                        "OK"));

                if (!hasCameraPermission)
                {
                    AppModel.Logger.Error("ScanObjModalPage.ScanAsync: Kamerazugriff verweigert");
                    return null;
                }

                var page = new ScanModalPage();
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
            AppModel.Logger.Info(
                $"[ScanModalPage] OnAppearing – scheduling IsDetecting=true after 300 ms " +
                $"(Width={Width:F0}, Height={Height:F0})");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    ReaderView.Options = new ZXing.Net.Maui.BarcodeReaderOptions
                    {
                        Formats = ZXing.Net.Maui.BarcodeFormats.TwoDimensional,
                        AutoRotate = true,
                        Multiple = false,
                        DelayBetweenAnalyzingFrames = 30,
                        InitialDelayBeforeAnalyzingFrames = 0,
                        DelayBetweenContinuousScans = 0,
                        CharacterSet = "ISO-8859-1",
                        CameraResolutionSelector = availableResolutions =>
                        {
                            var resolutions = availableResolutions.ToList();
                            var selected = resolutions
                                .OrderBy(r => Math.Abs((r.Width * r.Height) - (1280 * 720)))
                                .ThenBy(r => Math.Abs(r.Width - 1280) + Math.Abs(r.Height - 720))
                                .First();                            
                            return selected;
                        }
                    };
                    await Task.Delay(300, token);
                    AppModel.Logger.Info("[ScanModalPage] IsDetecting = true");
                    ReaderView.IsDetecting = true;

                    // Registriere den Scanner für zentrale Taschenlampen-Steuerung
                    FlashlightManager.RegisterScanner(ReaderView);

                }
                catch (OperationCanceledException ex)
                {
                    AppModel.Logger.Info("[ScanModalPage] IsDetecting start cancelled" + ex.Message + " :: " + ex.StackTrace);
                }
            });
#else
            ReaderView.IsDetecting = true;

            // Registriere den Scanner für zentrale Taschenlampen-Steuerung
            FlashlightManager.RegisterScanner(ReaderView);
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

            // Entferne die Scanner-Registrierung
            FlashlightManager.UnregisterScanner();

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

            var barcodeResult = e.Results?.FirstOrDefault();
            var value = barcodeResult?.Value;

            if (string.IsNullOrWhiteSpace(value))
                return;

            await CloseModalSafeAsync(value);
        }

    }
    public class OverlayViewModel
    {
        public string TopText { get; set; } = "";
        public string BottomText { get; set; } = "";

        public ICommand FlashButtonCommand { get; }

        public OverlayViewModel(Action? onFlashButtonClicked)
        {
            FlashButtonCommand = new Command(() => onFlashButtonClicked?.Invoke());
        }
    }
}
