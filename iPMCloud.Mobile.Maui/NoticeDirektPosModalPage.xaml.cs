using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile
{
    public partial class NoticeDirektPosModalPage : ContentPage
    {
        private static readonly SemaphoreSlim _modalSemaphore = new(1, 1);
        private readonly TaskCompletionSource<NoticeDirektPosResult?> _tcs = new();
        private readonly LeistungWSO _selectedPosForNotice;
        private readonly string _backToFromNotice;
        private readonly BemerkungWSO _selectedBemerkungForNotice;
        private bool _completed;

        private NoticeDirektPosModalPage(
            LeistungWSO pos,
            string backTo,
            bool isPrio,
            View posCard,
            BemerkungWSO existingBemerkung)
        {
            InitializeComponent();

            _selectedPosForNotice = pos;
            _backToFromNotice = backTo;
            _selectedBemerkungForNotice = existingBemerkung ?? new BemerkungWSO();

            if (posCard != null)
            {
                noticeFor.IsVisible = true;
                noticeFor_Pos.Children.Add(posCard);
            }
            else
            {
                noticeFor.IsVisible = false;
            }

            btn_alertmessage_tit.Text = isPrio ? "Störmeldung" : "Bemerkung";
            sw_alertmessage.IsToggled = isPrio;
            btn_alertmessage_img2.IsVisible = isPrio;
            sw_internmessage.IsToggled = false;
            btn_internmessage_img2.IsVisible = false;

            if (existingBemerkung != null)
            {
                entry_notice.Text = existingBemerkung.text?.Trim() ?? "";
                sw_internmessage.IsToggled = existingBemerkung.prio == 1 || existingBemerkung.prio == 3;
                sw_alertmessage.IsToggled = existingBemerkung.prio == 2 || existingBemerkung.prio == 3;
                existingBemerkung.photos.ForEach(p => noticePhotoStack.Children.Add(p.stack));
            }

            CheckNoticeFalid();

            btn_back_notice.GestureRecognizers.Clear();
            var tgrBack = new TapGestureRecognizer();
            tgrBack.Tapped += OnBackTapped;
            btn_back_notice.GestureRecognizers.Add(tgrBack);

            btn_notice_save.GestureRecognizers.Clear();
            var tgrSave = new TapGestureRecognizer();
            tgrSave.Tapped += OnSaveTapped;
            btn_notice_save.GestureRecognizers.Add(tgrSave);

            btn_notice_del.GestureRecognizers.Clear();
            var tgrDelete = new TapGestureRecognizer();
            tgrDelete.Tapped += OnDeleteTapped;
            btn_notice_del.GestureRecognizers.Add(tgrDelete);

            btn_takePhoto_frame.GestureRecognizers.Clear();
            var tgrPhoto = new TapGestureRecognizer();
            tgrPhoto.Tapped += async (s, e) => await btn_takePhoto(s, e);
            btn_takePhoto_frame.GestureRecognizers.Add(tgrPhoto);

            btn_takePhotoAttachment_frame.GestureRecognizers.Clear();
            var tgrAttach = new TapGestureRecognizer();
            tgrAttach.Tapped += async (s, e) => await btn_pickPhotos(s, e);
            btn_takePhotoAttachment_frame.GestureRecognizers.Add(tgrAttach);
        }

        public static async Task<NoticeDirektPosResult?> ShowAsync(
            Page callerPage,
            LeistungWSO pos,
            string backTo,
            bool isPrio,
            View posCard,
            BemerkungWSO existingBemerkung = null)
        {
            if (!await _modalSemaphore.WaitAsync(0))
            {
                return null;
            }

            try
            {
                var page = new NoticeDirektPosModalPage(pos, backTo, isPrio, posCard, existingBemerkung);
                await MainThread.InvokeOnMainThreadAsync(() =>
                    callerPage.Navigation.PushModalAsync(page, animated: false));
                return await page._tcs.Task;
            }
            finally
            {
                _modalSemaphore.Release();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Complete(null);
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            Focus();
            entry_notice.Text = "";
            noticePhotoStack.Children.Clear();
            Complete(null);
            await Navigation.PopModalAsync(animated: false);
        }

        private async void OnDeleteTapped(object sender, EventArgs e)
        {
            Focus();
            Complete(new NoticeDirektPosResult(
                null,
                _selectedPosForNotice,
                _backToFromNotice,
                true));
            await Navigation.PopModalAsync(animated: false);
        }

        private async void OnSaveTapped(object sender, EventArgs e)
        {
            Focus();
            if (!string.IsNullOrWhiteSpace(entry_notice.Text?.Trim()) ||
                (_selectedBemerkungForNotice.photos != null && _selectedBemerkungForNotice.photos.Count > 0))
            {
                int am = sw_alertmessage.IsToggled ? 2 : 0;
                int im = sw_internmessage.IsToggled ? 1 : 0;
                _selectedBemerkungForNotice.prio = am + im;
                _selectedBemerkungForNotice.text = entry_notice.Text?.Trim() ?? "";

                Complete(new NoticeDirektPosResult(
                    _selectedBemerkungForNotice,
                    _selectedPosForNotice,
                    _backToFromNotice,
                    false));

                entry_notice.Text = "";
                noticePhotoStack.Children.Clear();
                await Navigation.PopModalAsync(animated: false);
            }
        }

        private void Complete(NoticeDirektPosResult? result)
        {
            if (_completed)
            {
                return;
            }
            _completed = true;
            _tcs.TrySetResult(result);
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
            CheckNoticeFalid();
        }

        private void CheckNoticeFalid()
        {
            var text = entry_notice.Text?.Trim() ?? "";
            notizSave_stack.IsVisible =
                    !string.IsNullOrWhiteSpace(text) ||
                    _selectedBemerkungForNotice.photos.Count > 0;
            btn_notice_del.IsVisible = notizSave_stack.IsVisible;
        }
       

        public async Task btn_takePhoto(object sender, TappedEventArgs e)
        {
            if (_selectedBemerkungForNotice.photos.Count >= 5)
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
                var result = await Helpers.PhotoPickerHelper.TryTakeAndProcessPhotoAsync(
                    parentGuid: _selectedBemerkungForNotice.guid,
                    removeCommand: new Command<BildWSO>(RemoveBildInWork));

                if (result.IsSuccess)
                {
                    BildWSO.Save(AppModel.Instance, result.Photo);
                    _selectedBemerkungForNotice.photos.Add(result.Photo);
                    noticePhotoStack.Children.Add(result.Photo.stack);
                    CheckNoticeFalid();
                    return;
                }

                switch (result.FailureReason)
                {
                    case Helpers.PhotoCaptureFailureReason.PermissionDenied:
                        await DisplayAlertAsync("Fehler", "Keine Kamera-Berechtigung", "OK");
                        break;
                    case Helpers.PhotoCaptureFailureReason.CaptureNotSupported:
                        await DisplayAlertAsync("Fehler", "Kamera wird nicht unterstützt", "OK");
                        break;
                    case Helpers.PhotoCaptureFailureReason.UserCanceled:
                        break;
                    case Helpers.PhotoCaptureFailureReason.ProcessingFailed:
                        await DisplayAlertAsync("Fehler", "Foto konnte nicht verarbeitet werden", "OK");
                        break;
                    default:
                        await DisplayAlertAsync("Fehler", "Foto konnte nicht aufgenommen werden", "OK");
                        break;
                }
            }
            finally
            {
                CheckNoticeFalid();
                AppModel.Instance.UseExternHardware = false;
                overlay.IsVisible = false;
            }
        }

        public async Task btn_pickPhotos(object sender, TappedEventArgs e)
        {
            if (_selectedBemerkungForNotice.photos.Count >= 5)
            {
                await DisplayAlertAsync("Limit erreicht", "Maximal 5 Fotos erlaubt", "OK");
                return;
            }

            notizSave_stack.IsVisible = false;
            AppModel.Instance.UseExternHardware = true;

            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlertAsync("Fehler", "Kamera nicht verfügbar", "OK");
                    return;
                }

                overlay.IsVisible = true;
                await Task.Delay(1);

                var photos = await MediaPicker.PickPhotosAsync(new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    RotateImage = true,
                    SelectionLimit = 5 - _selectedBemerkungForNotice.photos.Count,
                    PreserveMetaData = true,
                });

                if (photos != null && photos.Count() > 0)
                {
                    foreach (var photo in photos)
                    {
                        var reCo = new Command<BildWSO>(RemoveBildInWork);
                        var photoResponse = await PhotoResize.CreatePhotoResponseAsync(photo);

                        long bildName = DateTime.Now.Ticks;
                        var bildWSO = new BildWSO(_selectedBemerkungForNotice.guid)
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
                        _selectedBemerkungForNotice.photos.Add(bildWSO);
                        noticePhotoStack.Children.Add(bildWSO.stack);
                    }
                    CheckNoticeFalid();
                }
            }
            catch (FeatureNotSupportedException exn)
            {
                AppModel.Logger.Error($"Fehler Kamera wird nicht unterstützt: {exn}");
            }
            catch (PermissionException exp)
            {
                AppModel.Logger.Error($"Fehler Keine Kamera-Berechtigung: {exp}");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"Fehler beim Foto aufnehmen: {ex}");
            }
            finally
            {
                CheckNoticeFalid();
                AppModel.Instance.UseExternHardware = false;
                overlay.IsVisible = false;
            }
        }

        public async void RemoveBildInWork(BildWSO b)
        {
            overlay.IsVisible = true;
            await Task.Delay(1);

            noticePhotoStack.Children.Remove(b.stack);
            await Task.Delay(1);
            BildWSO.Delete(AppModel.Instance, b);
            await Task.Delay(1);
            _selectedBemerkungForNotice.photos.Remove(b);
            CheckNoticeFalid();

            await Task.Delay(1);
            overlay.IsVisible = false;
        }

        private void OnOverlayTapped(object sender, EventArgs e)
        {
        }
    }

    public record NoticeDirektPosResult(
        BemerkungWSO? Bemerkung,
        LeistungWSO Pos,
        string BackTo,
        bool IsDeleted);
}
