using System.Diagnostics;
using iPMCloud.Mobile.vo;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace iPMCloud.Mobile.Helpers
{
    public enum PhotoPickerFailureKind
    {
        PermissionDenied,
        CaptureNotSupported,
        CaptureFailed,
        ProcessingFailed
    }

    public sealed class PhotoPickerException : Exception
    {
        public PhotoPickerException(
            PhotoPickerFailureKind failureKind,
            string userMessage,
            string stage,
            Exception innerException = null)
            : base(userMessage, innerException)
        {
            FailureKind = failureKind;
            UserMessage = userMessage;
            Stage = stage;
        }

        public PhotoPickerFailureKind FailureKind { get; }
        public string UserMessage { get; }
        public string Stage { get; }
    }

    public static class PhotoPickerHelper
    {
        private const string LogPrefix = "[PhotoPickerHelper]";

        private static void LogInfo(string message)
        {
            var logMessage = $"{LogPrefix} {message}";
            Debug.WriteLine(logMessage);
            AppModel.Logger?.Info(logMessage);
        }

        private static void LogWarning(string message)
        {
            var logMessage = $"{LogPrefix} {message}";
            Debug.WriteLine(logMessage);
            AppModel.Logger?.Warn(logMessage);
        }

        private static void LogError(string message, Exception ex)
        {
            var logMessage = $"{LogPrefix} {message}";
            Debug.WriteLine($"{logMessage}{Environment.NewLine}{ex}");
            AppModel.Logger?.Error(ex, logMessage);
        }

        private static async Task<PhotoResponse> ProcessPhotoResponseAsync(
            FileResult photo,
            BuildingWSO building,
            string customBuildingText,
            string operationName)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            Stream stream = null;
            try
            {
                LogInfo($"{operationName}: Öffne Stream für '{photo.FileName}'.");
                stream = await photo.OpenReadAsync();
                LogInfo($"{operationName}: Stream geöffnet.");
            }
            catch (Exception ex)
            {
                LogError($"{operationName}: OpenReadAsync fehlgeschlagen.", ex);
                throw new PhotoPickerException(
                    PhotoPickerFailureKind.ProcessingFailed,
                    "Das aufgenommene Foto konnte nicht geöffnet werden.",
                    "OpenReadAsync",
                    ex);
            }

            try
            {
                return await Task.Run(() =>
                {
                    PhotoResponse photoResponse;

                    try
                    {
                        LogInfo($"{operationName}: Starte PhotoUtils.GetImages.");
                        photoResponse = PhotoUtils.GetImages(stream);
                        LogInfo($"{operationName}: PhotoUtils.GetImages abgeschlossen.");
                    }
                    catch (Exception ex)
                    {
                        LogError($"{operationName}: PhotoUtils.GetImages fehlgeschlagen.", ex);
                        throw new PhotoPickerException(
                            PhotoPickerFailureKind.ProcessingFailed,
                            "Das Foto konnte nicht verarbeitet werden.",
                            "PhotoUtils.GetImages",
                            ex);
                    }

                    //try
                    //{
                    //    LogInfo($"{operationName}: Starte PhotoUtils.AddInfoToImage.");
                    //    photoResponse = PhotoUtils.AddInfoToImage(photoResponse, building, customBuildingText);
                    //    LogInfo($"{operationName}: PhotoUtils.AddInfoToImage abgeschlossen.");
                    //}
                    //catch (Exception ex)
                    //{
                    //    LogError($"{operationName}: PhotoUtils.AddInfoToImage fehlgeschlagen.", ex);
                    //    throw new PhotoPickerException(
                    //        PhotoPickerFailureKind.ProcessingFailed,
                    //        "Das Foto konnte nicht nachbearbeitet werden.",
                    //        "PhotoUtils.AddInfoToImage",
                    //        ex);
                    //}

                    if (photoResponse == null || photoResponse.imageBytes == null || photoResponse.imageBytes.Length == 0)
                    {
                        throw new PhotoPickerException(
                            PhotoPickerFailureKind.ProcessingFailed,
                            "Das Foto konnte nicht verarbeitet werden.",
                            "ProcessedPhotoValidation");
                    }

                    return photoResponse;
                });
            }
            finally
            {
                stream?.Dispose();
                LogInfo($"{operationName}: Stream geschlossen.");
            }
        }

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
                LogError("Fehler beim Foto-Picker.", ex);
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
                        var photoResponse = await ProcessPhotoResponseAsync(
                            photo,
                            building,
                            customBuildingText,
                            $"PickAndProcessPhotosAsync[{photo?.FileName ?? "unknown"}]");

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
                        LogError($"Fehler beim Verarbeiten von '{photo?.FileName ?? "unknown"}'.", photoEx);
                    }
                }

                onComplete?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                LogError("Fehler in PickAndProcessPhotosAsync.", ex);
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
                LogInfo("TakeAndProcessPhotoAsync: Starte Fotoaufnahme.");

                // Berechtigungen prüfen
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                LogInfo($"TakeAndProcessPhotoAsync: Kamera-Berechtigungsstatus = {status}.");
                if (status != PermissionStatus.Granted)
                {
                    LogInfo("TakeAndProcessPhotoAsync: Fordere Kamera-Berechtigung an.");
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    LogInfo($"TakeAndProcessPhotoAsync: Ergebnis Berechtigungsanfrage = {status}.");
                    if (status != PermissionStatus.Granted)
                    {
                        throw new PhotoPickerException(
                            PhotoPickerFailureKind.PermissionDenied,
                            "Bitte erlauben Sie Kamera-Zugriff.",
                            "Permissions.RequestAsync");
                    }
                }

                // Kamera verfügbar?
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    LogWarning("TakeAndProcessPhotoAsync: Capture wird auf diesem Gerät nicht unterstützt.");
                    throw new PhotoPickerException(
                        PhotoPickerFailureKind.CaptureNotSupported,
                        "Kamera nicht verfügbar.",
                        "MediaPicker.IsCaptureSupported");
                }

                // Foto aufnehmen
                LogInfo("TakeAndProcessPhotoAsync: Starte MediaPicker.Default.CapturePhotoAsync.");
                var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    CompressionQuality = 75,
                    MaximumHeight = 1024,
                    MaximumWidth = 1024,
                    RotateImage = true,
                    PreserveMetaData = true,
                });
                LogInfo("TakeAndProcessPhotoAsync: MediaPicker.Default.CapturePhotoAsync beendet.");
                if (photo == null)
                {
                    LogInfo("TakeAndProcessPhotoAsync: Keine Datei zurückgegeben (Abbruch oder kein Ergebnis).");
                    return null;
                }

                LogInfo($"TakeAndProcessPhotoAsync: Capture-Datei erhalten '{photo.FileName}' ({photo.FullPath}).");
                var photoResponse = await ProcessPhotoResponseAsync(
                    photo,
                    building,
                    customBuildingText,
                    "TakeAndProcessPhotoAsync");

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

                LogInfo("TakeAndProcessPhotoAsync: Bild erfolgreich verarbeitet.");
                return bildWSO;
            }
            catch (OperationCanceledException)
            {
                LogInfo("TakeAndProcessPhotoAsync: Benutzer hat die Aktion abgebrochen.");
                throw;
            }
            catch (PhotoPickerException)
            {
                throw;
            }
            catch (PermissionException ex)
            {
                LogError("TakeAndProcessPhotoAsync: PermissionException.", ex);
                throw new PhotoPickerException(
                    PhotoPickerFailureKind.PermissionDenied,
                    "Bitte erlauben Sie Kamera-Zugriff.",
                    "PermissionException",
                    ex);
            }
            catch (FeatureNotSupportedException ex)
            {
                LogError("TakeAndProcessPhotoAsync: FeatureNotSupportedException.", ex);
                throw new PhotoPickerException(
                    PhotoPickerFailureKind.CaptureNotSupported,
                    "Kamera wird nicht unterstützt.",
                    "FeatureNotSupportedException",
                    ex);
            }
            catch (Exception ex)
            {
                LogError("TakeAndProcessPhotoAsync: Unerwarteter Fehler beim Foto aufnehmen.", ex);
                throw new PhotoPickerException(
                    PhotoPickerFailureKind.CaptureFailed,
                    "Foto konnte nicht aufgenommen oder verarbeitet werden.",
                    "Unhandled",
                    ex);
            }
        }


    }
}