using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Wraps <see cref="MediaPicker.PickPhotosAsync(MediaPickerOptions)"/> to work around a known
    /// .NET MAUI/iOS bug: when picking more than one photo, the native PHPickerViewController can
    /// return item results on a background thread, which crashes internally in
    /// Microsoft.Maui.Media.MediaPickerImplementation.PickerResultsToMediaFiles with
    /// "UIKit Consistency error: you are calling a UIKit method that can only be invoked from the
    /// UI thread" while accessing PHPickerResult.ItemProvider. Picking a single photo is not
    /// affected, so on iOS with SelectionLimit > 1 we fall back to repeated single-photo picks.
    /// Android (and other platforms) keep the normal, unmodified multi-select behavior.
    /// </summary>
    public static class MediaPickerCompat
    {
        public static async Task<IList<FileResult>> PickPhotosAsync(MediaPickerOptions options)
        {
#if IOS
            if (options.SelectionLimit > 1)
            {
                var results = new List<FileResult>();
                var remaining = options.SelectionLimit;

                while (remaining > 0)
                {
                    var singleOptions = new MediaPickerOptions
                    {
                        Title = options.Title,
                        CompressionQuality = options.CompressionQuality,
                        MaximumHeight = options.MaximumHeight,
                        MaximumWidth = options.MaximumWidth,
                        SelectionLimit = 1,
                        PreserveMetaData = options.PreserveMetaData,
                        RotateImage = options.RotateImage,
                    };

                    var singleResult = await PickSinglePhotoWithRetryAsync(singleOptions);
                    if (singleResult == null || singleResult.Count == 0)
                    {
                        break;
                    }

                    results.AddRange(singleResult);
                    remaining--;
                }

                return results;
            }
#endif
            return await PickSinglePhotoWithRetryAsync(options);
        }

        private static async Task<IList<FileResult>> PickSinglePhotoWithRetryAsync(MediaPickerOptions options)
        {
            const int maxAttempts = 2;
            Exception lastUiThreadException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await MainThread.InvokeOnMainThreadAsync(() => MediaPicker.PickPhotosAsync(options));
                }
                catch (Exception ex) when (IsUiThreadConsistencyError(ex) && attempt < maxAttempts)
                {
                    lastUiThreadException = ex;
                    await Task.Delay(200);
                }
            }

            throw lastUiThreadException ?? new InvalidOperationException("Fotoauswahl fehlgeschlagen.");
        }

        private static bool IsUiThreadConsistencyError(Exception ex)
        {
            return ex.Message?.Contains("UIKit Consistency error", StringComparison.OrdinalIgnoreCase) == true
                || ex.Message?.Contains("EnsureUIThread", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
