using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace iPMCloud.Mobile.Helpers
{
    public static class PhotoPickerHelper
    {
        public static async Task<List<FileResult>> PickMultiplePhotosAsync(int maxCount)
        {
            try
            {
                // Berechtigungen prüfen
                var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Photos>();
                    if (status != PermissionStatus.Granted)
                        return null;
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
                System.Diagnostics.Debug.WriteLine($"Fehler beim Foto-Picker: {ex.Message}");
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
    StackLayout targetStack,
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
                        System.Diagnostics.Debug.WriteLine($"Fehler: {photoEx.Message}");
                    }
                }

                onComplete?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler: {ex.Message}");
                return false;
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
            try
            {
                // Berechtigungen prüfen
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                        return null;
                }

                // Kamera verfügbar?
                if (!MediaPicker.Default.IsCaptureSupported)
                    return null;

                // Foto aufnehmen
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo == null)
                    return null;

                // Foto verarbeiten
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

                // Frame Gesture Recognizer einrichten
                var frame = (Border)((StackLayout)(bildWSO.stack.Children[0])).Children[2];
                frame.GestureRecognizers.Clear();
                frame.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = removeCommand,
                    CommandParameter = bildWSO
                });

                return bildWSO;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Foto aufnehmen: {ex.Message}");
                return null;
            }
        }


    }
}