using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace iPMCloud.Mobile.Helpers
{
    public enum PhotoCaptureFailureReason
    {
        None = 0,
        PermissionDenied,
        CaptureNotSupported,
        UserCanceled,
        CaptureFailed,
        ProcessingFailed
    }

    public sealed class PhotoCaptureResult
    {
        public BildWSO Photo { get; private init; }
        public PhotoCaptureFailureReason FailureReason { get; private init; }
        public PermissionStatus? PermissionStatus { get; private init; }
        public bool? IsCaptureSupported { get; private init; }
        public Exception Exception { get; private init; }

        public bool IsSuccess => Photo != null && FailureReason == PhotoCaptureFailureReason.None;

        public static PhotoCaptureResult Success(BildWSO photo, PermissionStatus permissionStatus, bool isCaptureSupported)
            => new()
            {
                Photo = photo,
                FailureReason = PhotoCaptureFailureReason.None,
                PermissionStatus = permissionStatus,
                IsCaptureSupported = isCaptureSupported
            };

        public static PhotoCaptureResult Failure(
            PhotoCaptureFailureReason reason,
            PermissionStatus? permissionStatus = null,
            bool? isCaptureSupported = null,
            Exception exception = null)
            => new()
            {
                FailureReason = reason,
                PermissionStatus = permissionStatus,
                IsCaptureSupported = isCaptureSupported,
                Exception = exception
            };
    }

    public static class PhotoPickerHelper
    {
        public static async Task<List<FileResult>> PickMultiplePhotosAsync(int maxCount)
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    // Berechtigungen prüfen
                    var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.Photos>();
                        if (status != PermissionStatus.Granted)
                            return null;
                    }
                }

                // File Types definieren
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.image" } },
                        { DevicePlatform.Android, new[] { "image/*" } },
                    });

                var options = new PickOptions
                {
                    PickerTitle = maxCount > 1
                        ? $"Bitte bis zu {maxCount} Foto(s) auswählen"
                        : "Bitte ein Foto auswählen",
                    FileTypes = customFileType,
                };

                // Multi-Select
                var results = await FilePicker.Default.PickMultipleAsync(options);

                if (results == null || !results.Any())
                    return null;

                // Limit anwenden
                return results.Take(maxCount).ToList();
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"Fehler beim Foto-Picker: {ex}");
                return null;
            }
        }

        public static async Task<byte[]> FileResultToByteArrayAsync(FileResult file)
        {
            if (file == null)
                return null;

            using var stream = await file.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }


        public static async Task<bool> PickAndProcessPhotosAsync(
                int maxPhotos,
                List<BildWSO> photoList,
                VerticalStackLayout targetStack,
                string parentGuid,
                Command<BildWSO> removeCommand,
                BuildingWSO building = null,
                string customBuildingText = null,
                Action onComplete = null)
        {
            try
            {
                // Prüfe Limit
                int remainingPhotos = maxPhotos - photoList.Count;
                if (remainingPhotos <= 0)
                    return false;

                // Photo Picker
                var selectedPhotos = await PickMultiplePhotosAsync(remainingPhotos);
                if (selectedPhotos == null || !selectedPhotos.Any())
                    return false;

                // Fotos verarbeiten
                foreach (var photo in selectedPhotos)
                {
                    try
                    {
                        using var stream = await photo.OpenReadAsync();

                        var photoResponse = PhotoUtils.GetImages(stream);
                        photoResponse = PhotoUtils.AddInfoToImage(
                            photoResponse,
                            building,
                            customBuildingText);

                        long bildName = DateTime.Now.Ticks;
                        var bildWSO = new BildWSO(parentGuid)
                        {
                            bytes = photoResponse.imageBytes,
                            name = bildName.ToString(),
                            stack = BildWSO.GetAttachmentForNoticeElement(
                                photoResponse.GetImageSourceAsThumb(),
                                new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                                removeCommand)
                        };

                        var frame = (Border)((StackLayout)(bildWSO.stack.Children[0])).Children[2];
                        frame.GestureRecognizers.Clear();
                        frame.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = removeCommand,
                            CommandParameter = bildWSO
                        });

                        BildWSO.Save(AppModel.Instance, bildWSO);
                        photoList.Add(bildWSO);
                        targetStack.Children.Add(bildWSO.stack);
                    }
                    catch (Exception photoEx)
                    {
                        AppModel.Logger?.Error($"Fehler bei Fotoverarbeitung: {photoEx}");
                    }
                }

                onComplete?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"Fehler beim Fotoauswahl-Flow: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Captures and processes a camera photo and returns a structured result
        /// with either the processed <see cref="BildWSO"/> or a concrete failure reason.
        /// </summary>
        /// <param name="parentGuid">Parent GUID used for created image metadata.</param>
        /// <param name="removeCommand">Command bound to the generated preview/remove UI.</param>
        /// <param name="building">Optional building metadata for image annotation.</param>
        /// <param name="customBuildingText">Optional custom building text for annotation.</param>
        /// <returns>
        /// A <see cref="PhotoCaptureResult"/> that distinguishes permission denied, capture not supported,
        /// user cancel, capture failure, and processing failure.
        /// </returns>
        public static async Task<PhotoCaptureResult> TryTakeAndProcessPhotoAsync(
            string parentGuid,
            Command<BildWSO> removeCommand,
            BuildingWSO building = null,
            string customBuildingText = null)
        {
            PermissionStatus permissionStatus = PermissionStatus.Unknown;
            bool isCaptureSupported = false;

            try
            {
                permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                AppModel.Logger?.Info($"TryTakeAndProcessPhotoAsync - Kamera PermissionStatus (check): {permissionStatus}");

                if (permissionStatus != PermissionStatus.Granted)
                {
                    permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
                    AppModel.Logger?.Info($"TryTakeAndProcessPhotoAsync - Kamera PermissionStatus (request): {permissionStatus}");
                }

                if (permissionStatus != PermissionStatus.Granted)
                {
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.PermissionDenied,
                        permissionStatus: permissionStatus);
                }

                isCaptureSupported = MediaPicker.Default.IsCaptureSupported;
                AppModel.Logger?.Info($"TryTakeAndProcessPhotoAsync - IsCaptureSupported: {isCaptureSupported}");

                if (!isCaptureSupported)
                {
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.CaptureNotSupported,
                        permissionStatus: permissionStatus,
                        isCaptureSupported: false);
                }

                FileResult photo;
                try
                {
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                    AppModel.Logger?.Info($"TryTakeAndProcessPhotoAsync - Capture result null: {photo == null}");
                }
                catch (FeatureNotSupportedException ex)
                {
                    AppModel.Logger?.Error($"TryTakeAndProcessPhotoAsync - Capture not supported exception: {ex}");
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.CaptureNotSupported,
                        permissionStatus: permissionStatus,
                        isCaptureSupported: isCaptureSupported,
                        exception: ex);
                }
                catch (PermissionException ex)
                {
                    AppModel.Logger?.Error($"TryTakeAndProcessPhotoAsync - Permission exception: {ex}");
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.PermissionDenied,
                        permissionStatus: permissionStatus,
                        isCaptureSupported: isCaptureSupported,
                        exception: ex);
                }
                catch (OperationCanceledException ex)
                {
                    AppModel.Logger?.Info("TryTakeAndProcessPhotoAsync - User canceled capture.");
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.UserCanceled,
                        permissionStatus: permissionStatus,
                        isCaptureSupported: isCaptureSupported,
                        exception: ex);
                }
                catch (Exception ex)
                {
                    AppModel.Logger?.Error($"TryTakeAndProcessPhotoAsync - Capture failed: {ex}");
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.CaptureFailed,
                        permissionStatus: permissionStatus,
                        isCaptureSupported: isCaptureSupported,
                        exception: ex);
                }

                if (photo == null)
                {
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.UserCanceled,
                        permissionStatus: permissionStatus,
                        isCaptureSupported: isCaptureSupported);
                }

                try
                {
                    using var stream = await photo.OpenReadAsync();

                    var photoResponse = PhotoUtils.GetImages(stream);
                    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, building, customBuildingText);

                    long bildName = DateTime.Now.Ticks;
                    var bildWSO = new BildWSO(parentGuid)
                    {
                        bytes = photoResponse.imageBytes,
                        name = bildName.ToString(),
                        stack = BildWSO.GetAttachmentForNoticeElement(
                            photoResponse.GetImageSourceAsThumb(),
                            new DateTime(bildName).ToString("dd.MM.yyyy-HH:mm:ss"),
                            removeCommand)
                    };

                    var frame = (Border)((StackLayout)(bildWSO.stack.Children[0])).Children[2];
                    frame.GestureRecognizers.Clear();
                    frame.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = removeCommand,
                        CommandParameter = bildWSO
                    });

                    return PhotoCaptureResult.Success(bildWSO, permissionStatus, isCaptureSupported);
                }
                catch (Exception ex)
                {
                    AppModel.Logger?.Error($"TryTakeAndProcessPhotoAsync - Processing failed: {ex}");
                    return PhotoCaptureResult.Failure(
                        PhotoCaptureFailureReason.ProcessingFailed,
                        permissionStatus: permissionStatus,
                        isCaptureSupported: isCaptureSupported,
                        exception: ex);
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error($"TryTakeAndProcessPhotoAsync - Unexpected error: {ex}");
                return PhotoCaptureResult.Failure(
                    PhotoCaptureFailureReason.CaptureFailed,
                    permissionStatus: permissionStatus,
                    exception: ex);
            }
        }

        /// <summary>
        /// Nimmt ein Foto auf und verarbeitet es komplett
        /// </summary>
        public static async Task<BildWSO> TakeAndProcessPhotoAsync(
            string parentGuid,
            Command<BildWSO> removeCommand,
            BuildingWSO building = null,
            string customBuildingText = null)
        {
            var result = await TryTakeAndProcessPhotoAsync(parentGuid, removeCommand, building, customBuildingText);
            return result.Photo;
        }


    }
}