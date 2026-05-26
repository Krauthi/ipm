using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile
{
    public partial class ObjectValuesBildModalPage : ModalFullscreenPage
    {
        private readonly TaskCompletionSource<bool> _tcs = new();

        private ObjectValuesBildModalPage()
        {
            InitializeComponent();

            btn_back_bild.GestureRecognizers.Clear();
            var tgr_back = new TapGestureRecognizer();
            tgr_back.Tapped += OnCancelTapped;
            btn_back_bild.GestureRecognizers.Add(tgr_back);

            btn_cancel_bild.GestureRecognizers.Clear();
            var tgr_cancel = new TapGestureRecognizer();
            tgr_cancel.Tapped += OnCancelTapped;
            btn_cancel_bild.GestureRecognizers.Add(tgr_cancel);

            btn_newphoto_bild.GestureRecognizers.Clear();
            var tgr_photo = new TapGestureRecognizer();
            tgr_photo.Tapped += async (s, e) => await TakePhotoAsync(s, e);
            btn_newphoto_bild.GestureRecognizers.Add(tgr_photo);

            btn_send_bild.GestureRecognizers.Clear();
            var tgr_send = new TapGestureRecognizer();
            tgr_send.Tapped += OnSendTapped;
            btn_send_bild.GestureRecognizers.Add(tgr_send);
        }

        /// <summary>
        /// Opens the meter-reading photo page as a full-screen modal.
        /// Returns true when the user sends the photo, false when cancelled.
        /// Must be called from the UI thread (or will be marshalled internally).
        /// </summary>
        public static async Task<bool> ShowAsync(Page callerPage)
        {
            var page = new ObjectValuesBildModalPage();
            await MainThread.InvokeOnMainThreadAsync(() =>
                callerPage.Navigation.PushModalAsync(page, animated: false));
            return await page._tcs.Task;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            img_photo_bild.Source = null;
            editor_notice_bild.Text = "";
            btn_send_bild.IsVisible = false;
            lbl_send_err_bild.Opacity = 0;
            AppModel.Instance.selectedObjectValueBild = null;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Ensure the awaiter is always released (e.g. hardware back button on Android).
            _tcs.TrySetResult(false);
        }

        private async void OnCancelTapped(object sender, EventArgs e)
        {
            AppModel.Instance.selectedObjectValueBild = null;
            _tcs.TrySetResult(false);
            await Navigation.PopModalAsync(animated: false);
        }

        private async Task TakePhotoAsync(object sender, EventArgs e)
        {
            await Task.Delay(1);

            try
            {
                AppModel.Instance.UseExternHardware = true;

                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlertAsync("Fehler", "Kamera nicht verfügbar", "OK");
                    return;
                }

                overlay_bild.IsVisible = true;
                await Task.Delay(1);

                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    RotateImage = true,
                    SelectionLimit = 1,
                    PreserveMetaData = true,
                });

                if (photo != null)
                {
                    var photoResponse = await PhotoResize.CreatePhotoResponseAsync(photo);

                    AppModel.Instance.selectedObjectValueBild = new ObjektDatenBildWSO { bytes = photoResponse.imageBytes };

                    img_photo_bild.Source = photoResponse.GetImageSourceAsThumb();
                    btn_send_bild.IsVisible = true;
                    await Task.Delay(1);
                    overlay_bild.IsVisible = false;
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlertAsync("Fehler", "Kamera wird nicht unterstützt", "OK");
            }
            catch (PermissionException)
            {
                await DisplayAlertAsync("Fehler", "Keine Kamera-Berechtigung", "OK");
            }
            catch (OperationCanceledException)
            {
                // User cancelled
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Foto aufnehmen: {ex.Message}");
            }
            finally
            {
                AppModel.Instance.UseExternHardware = false;
                overlay_bild.IsVisible = false;
            }
        }

        private async void OnSendTapped(object sender, EventArgs e)
        {
            if (AppModel.Instance.selectedObjectValueBild == null)
            {
                lbl_send_err_bild.Opacity = 1;
                return;
            }

            overlay_bild.IsVisible = true;
            await Task.Delay(1);

            if (AppModel.Instance.isFlashLigthAloneON)
            {
                AppModel.Instance.Btn_FlashlightAloneTapped(null, null);
            }
            await Task.Delay(1);

            AppModel.Instance.selectedObjectValueBild.filename = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
            AppModel.Instance.selectedObjectValueBild.bemerkung = editor_notice_bild.Text?.Trim() ?? "";
            AppModel.Instance.selectedObjectValueBild.meterid = AppModel.Instance.selectedObjectValue.id;
            AppModel.Instance.selectedObjectValueBild.lastchange = JavaScriptDateConverter.Convert(DateTime.Now).ToString();
            AppModel.Instance.selectedObjectValueBild.standid = 0;

            ObjektDatenBildWSO.ToUploadStack(AppModel.Instance, AppModel.Instance.selectedObjectValueBild);

            overlay_bild.IsVisible = false;

            _tcs.TrySetResult(true);
            await Navigation.PopModalAsync(animated: false);
        }
    }
}
