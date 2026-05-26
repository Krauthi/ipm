using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile
{
    public partial class NoticeModalPage : ModalFullscreenPage
    {
        private readonly TaskCompletionSource<NoticeResult?> _tcs = new();

        private BemerkungWSO _SelectedBemerkungForNotice;
        private LeistungWSO _SelectedPosForNotice;
        private string _BackToFromNotice;
        private bool _manuelTextChange;

        private NoticeModalPage(LeistungWSO pos, string backTo, bool isPrio, View posCard)
        {
            InitializeComponent();

            _SelectedBemerkungForNotice = new BemerkungWSO();
            _SelectedPosForNotice = pos;
            _BackToFromNotice = backTo;

            btn_alertmessage_tit.Text = isPrio ? "Störmeldung" : "Bemerkung";
            sw_alertmessage.IsToggled = isPrio;
            btn_alertmessage_img2.IsVisible = isPrio;
            sw_internmessage.IsToggled = false;
            btn_internmessage_img2.IsVisible = false;

            if (posCard != null)
            {
                noticeFor.IsVisible = true;
                noticeFor_Pos.Children.Add(posCard);
            }

            CheckNoticeFalid();

            btn_back_notice.GestureRecognizers.Clear();
            var tgr_back = new TapGestureRecognizer();
            tgr_back.Tapped += OnBackTapped;
            btn_back_notice.GestureRecognizers.Add(tgr_back);

            btn_notice_save.GestureRecognizers.Clear();
            var tgr_save = new TapGestureRecognizer();
            tgr_save.Tapped += OnSaveTapped;
            btn_notice_save.GestureRecognizers.Add(tgr_save);

            btn_takePhoto_frame.GestureRecognizers.Clear();
            var tgr_photo = new TapGestureRecognizer();
            tgr_photo.Tapped += async (s, e) => await btn_takePhotos(s, e);
            btn_takePhoto_frame.GestureRecognizers.Add(tgr_photo);

            btn_takePhotoAttachment_frame.GestureRecognizers.Clear();
            var tgr_attach = new TapGestureRecognizer();
            tgr_attach.Tapped += async (s, e) => await btn_pickPhotosForNotice(s, e);
            btn_takePhotoAttachment_frame.GestureRecognizers.Add(tgr_attach);
        }

        /// <summary>
        /// Opens the notice page as a fullscreen modal and returns the result when the user
        /// saves or cancels. Returns null if the user cancels without saving.
        /// </summary>
        public static async Task<NoticeResult?> ShowAsync(
            Page callerPage,
            LeistungWSO pos,
            string backTo,
            bool isPrio,
            View posCard)
        {
            var page = new NoticeModalPage(pos, backTo, isPrio, posCard);
            await MainThread.InvokeOnMainThreadAsync(() =>
                callerPage.Navigation.PushModalAsync(page, animated: false));
            return await page._tcs.Task;
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            this.Focus();
            entry_notice.Text = "";
            noticePhotoStack.Children.Clear();
            _tcs.TrySetResult(null);
            await Navigation.PopModalAsync(animated: false);
        }

        private async void OnSaveTapped(object sender, EventArgs e)
        {
            this.Focus();

            var text = entry_notice.Text?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(text) ||
                (_SelectedBemerkungForNotice.photos != null && _SelectedBemerkungForNotice.photos.Count > 0))
            {
                int am = sw_alertmessage.IsToggled ? 2 : 0;
                int im = sw_internmessage.IsToggled ? 1 : 0;
                _SelectedBemerkungForNotice.prio = am + im;
                _SelectedBemerkungForNotice.text = text;

                _tcs.TrySetResult(new NoticeResult(
                    _SelectedBemerkungForNotice,
                    _SelectedPosForNotice,
                    _BackToFromNotice));

                entry_notice.Text = "";
                noticePhotoStack.Children.Clear();
                await Navigation.PopModalAsync(animated: false);
            }
        }

        private void AlertMessage_Switch_Toggled(object sender, ToggledEventArgs e)
        {
            btn_alertmessage_img2.IsVisible = e.Value;
        }

        private void InternMessage_Switch_Toggled(object sender, ToggledEventArgs e)
        {
            btn_internmessage_img2.IsVisible = e.Value;
        }

        private void entry_notice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_SelectedBemerkungForNotice != null && !_manuelTextChange)
            {
                _manuelTextChange = true;
                //_SelectedBemerkungForNotice.text = e.NewTextValue?.Trim() ?? "";
                CheckNoticeFalid();
                _manuelTextChange = false;
            }
        }

        private void CheckNoticeFalid()
        {
            var text = entry_notice.Text?.Trim() ?? "";
            if (text != null && text.Length > 0)
            {
                notizSave_stack.IsVisible =
                    !string.IsNullOrWhiteSpace(text) ||
                    _SelectedBemerkungForNotice.photos.Count > 0;
            }
            else
            {
                notizSave_stack.IsVisible = false;
            }
        }

        public async Task btn_takePhotos(object sender, TappedEventArgs e)
        {
            if (_SelectedBemerkungForNotice.photos.Count >= 5)
            {
                await DisplayAlertAsync("Limit erreicht", "Maximal 5 Fotos erlaubt", "OK");
                return;
            }
            notizSave_stack.IsVisible = false;
            await Task.Delay(1);

            AppModel.Instance.UseExternHardware = true;

            try
            {
                overlay.IsVisible = true;
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

                    var reCo = new Command<BildWSO>(RemoveBildInWork);

                    long bildName = DateTime.Now.Ticks;
                    var bildWSO = new BildWSO(_SelectedBemerkungForNotice.guid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = bildName.ToString(),
                        stack = BildWSO.GetAttachmentForNoticeElement(
                            photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                            reCo)
                    };
                    var frame = (Border)((StackLayout)(bildWSO.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = reCo,
                        CommandParameter = bildWSO
                    });

                    BildWSO.Save(AppModel.Instance, bildWSO);
                    _SelectedBemerkungForNotice.photos.Add(bildWSO);
                    noticePhotoStack.Children.Add(bildWSO.stack);

                    CheckNoticeFalid();
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
                // Benutzer hat abgebrochen
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Foto aufnehmen: {ex.Message}");
            }
            finally
            {
                overlay.IsVisible = false;
                AppModel.Instance.UseExternHardware = false;
            }
        }

        public async Task btn_pickPhotosForNotice(object sender, TappedEventArgs e)
        {
            if (_SelectedBemerkungForNotice.photos.Count >= 5)
            {
                await DisplayAlertAsync("Limit erreicht", "Maximal 5 Fotos erlaubt", "OK");
                return;
            }

            notizSave_stack.IsVisible = false;
            AppModel.Instance.UseExternHardware = true;

            try
            {
                overlay.IsVisible = true;
                await Task.Delay(1);

                var photos = await MediaPicker.PickPhotosAsync(new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    RotateImage = true,
                    SelectionLimit = 5 - _SelectedBemerkungForNotice.photos.Count,
                    PreserveMetaData = true,
                });


                if (photos != null && photos.Count() > 0)
                {
                    foreach (var photo in photos)
                    {
                        var reCo = new Command<BildWSO>(RemoveBildInWork);
                        var photoResponse = await PhotoResize.CreatePhotoResponseAsync(photo);

                        long bildName = DateTime.Now.Ticks;
                        var bildWSO = new BildWSO(_SelectedBemerkungForNotice.guid)
                        {
                            bytes = photoResponse.imageBytes,
                            name = bildName.ToString(),
                            stack = BildWSO.GetAttachmentForNoticeElement(
                                photoResponse.GetImageSourceAsThumb(),
                                new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                                reCo)
                        };
                        var frame = (Border)((StackLayout)(bildWSO.stack.Children[0])).Children[2];
                        frame.GestureRecognizers.Clear();
                        frame.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = reCo,
                            CommandParameter = bildWSO
                        });

                        BildWSO.Save(AppModel.Instance, bildWSO);
                        _SelectedBemerkungForNotice.photos.Add(bildWSO);
                        noticePhotoStack.Children.Add(bildWSO.stack);
                    }
                    CheckNoticeFalid();
                }
            }
            catch (FeatureNotSupportedException exn)
            {
                AppModel.Logger.Error($"Fehler Kamera wird nicht unterstützt: {exn.Message}");
            }
            catch (PermissionException exp)
            {
                AppModel.Logger.Error($"Fehler Keine Kamera-Berechtigung: {exp.Message}");
            }
            catch (OperationCanceledException)
            {
                // Benutzer hat abgebrochen
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"Fehler beim Foto aufnehmen: {ex.Message}");
            }
            finally
            {
                overlay.IsVisible = false;
                CheckNoticeFalid();
                AppModel.Instance.UseExternHardware = false;
            }
        }

        public async void RemoveBildInWork(BildWSO b)
        {
            noticePhotoStack.Children.Remove(b.stack);
            await Task.Delay(1);
            BildWSO.Delete(AppModel.Instance, b);
            await Task.Delay(1);
            _SelectedBemerkungForNotice.photos.Remove(b);
            CheckNoticeFalid();
        }

        private void OnOverlayTapped(object sender, EventArgs e)
        {
            // Implementierung hier - z.B. das Overlay ausblenden
            //if (popupContainer_infodialog != null)
            //{
            //    popupContainer_infodialog.IsVisible = false;
            //}
        }
    }

    public record NoticeResult(BemerkungWSO Bemerkung, LeistungWSO Pos, string BackTo);
}
