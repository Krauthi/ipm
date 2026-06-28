using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.Maui.Graphics.Platform;
using System;
using System.IO;
using System.Linq;
using Directory = System.IO.Directory;
using MauiImageSource = Microsoft.Maui.Controls.ImageSource;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using System;


namespace iPMCloud.Mobile.vo
{

    public static class PhotoResize
    {
        public static async Task<DateTime?> GetBestDateAsync(FileResult photo)
        {
            await using var stream = await photo.OpenReadAsync();

            var directories = ImageMetadataReader.ReadMetadata(stream);
            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

            if (subIfd != null)
            {
                if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var d1))
                    return d1;

                if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var d2))
                    return d2;
            }

            if (ifd0 != null &&
                ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var d3))
            {
                return d3;
            }

            return null;
        }
        public static async Task<DateTime?> GetCreateDateAsync(FileResult photo)
        {
            await using var stream = await photo.OpenReadAsync();

            var directories = ImageMetadataReader.ReadMetadata(stream);

            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if (subIfd != null &&
                subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime date))
            {
                return date; 
            }

            return null;
        }

        public static async Task<PhotoResponse> CreatePhotoResponseAsync(FileResult photo)
        {
            if (photo is null) throw new ArgumentNullException(nameof(photo));

            //var createDate = await GetBestDateAsync(photo);

            // 1) File -> bytes (einmalig)
            byte[] bytes;
            await using (var input = await photo.OpenReadAsync())
            await using (var ms = new MemoryStream())
            {
                await input.CopyToAsync(ms);
                bytes = ms.ToArray();
            }


            return new PhotoResponse
            {
                imageBytes = bytes,
                thumbBytes = bytes,
                createDate = Utils.GetCurrentGermanDateTimeString(),
                objektAdress = string.Empty
            };


            //int orientation = 1;
            //string createDate = string.Empty;

            // 2) EXIF lesen aus eigenem Stream (dein Block)
            //try
            //{
            //    using var exifStream = new MemoryStream(bytes);
            //    var directories = ImageMetadataReader.ReadMetadata(exifStream);

            //    var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            //    if (ifd0 != null && ifd0.ContainsTag(ExifDirectoryBase.TagOrientation))
            //        orientation = ifd0.GetInt32(ExifDirectoryBase.TagOrientation);

            //    var sub = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            //    if (sub != null && sub.ContainsTag(ExifDirectoryBase.TagDateTimeOriginal))
            //        createDate = ModifiedDate(sub.GetString(ExifDirectoryBase.TagDateTimeOriginal));
            //}
            //catch (Exception ex)
            //{
            //    AppModel.Logger?.Warn(ex, "Could not read EXIF data");
            //}

            // 3) Image -> full + thumb
            //byte[] fullBytes;
            //byte[] thumbBytes;

            //try
            //{
            //    using var image = PlatformImage.FromStream(new MemoryStream(bytes));

            //    // Optional: nur nötig, wenn RotateImage NICHT zuverlässig greift.
            //    // Wenn du RotateImage=true nutzt und die Bilder korrekt sind,
            //    // kannst du die nächste Zeile weglassen.
            //    //using var oriented = ApplyExifOrientation(image, orientation);

            //    using var full = ResizeToMax(image, 1024);
            //    fullBytes = EncodeJpeg(full, quality: 0.80f);

            //    using var thumb = ResizeToMax(full, 256);
            //    thumbBytes = EncodeJpeg(thumb, quality: 0.75f);
            //}
            //catch (Exception ex)
            //{
            //    AppModel.Logger?.Error(ex, "Error processing image");
            //    // Fallback: Original
            //    fullBytes = bytes;
            //    thumbBytes = bytes;
            //}

            //return new PhotoResponse
            //{
            //    imageBytes = fullBytes,
            //    thumbBytes = thumbBytes,
            //    createDate = createDate,
            //    objektAdress = string.Empty
            //};
        }

        private static Microsoft.Maui.Graphics.IImage ResizeToMax(Microsoft.Maui.Graphics.IImage src, float maxSize)
        {
            var scale = Math.Min(maxSize / src.Width, maxSize / src.Height);

            // Immer ein neues Image erzeugen (Dispose-sicher)
            int w = (int)Math.Round(src.Width * Math.Min(scale, 1f));
            int h = (int)Math.Round(src.Height * Math.Min(scale, 1f));

            var context = new SkiaBitmapExportContext(w, h, 1.0f);
            context.Canvas.DrawImage(src, 0, 0, w, h);
            return context.Image;
        }

        private static byte[] EncodeJpeg(Microsoft.Maui.Graphics.IImage img, float quality)
        {
            using var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Jpeg, quality);
            return ms.ToArray();
        }

        // Minimal-Orientation (1/3/6/8). Spiegelungen (2/4/5/7) bei Bedarf ergänzen.
        //private static IImage ApplyExifOrientation(IImage src, int orientation)
        //{
        //    return orientation switch
        //    {
        //        3 => Rotate(src, 180),
        //        6 => Rotate(src, 90),
        //        8 => Rotate(src, 270),
        //        _ => Clone(src)
        //    };
        //}

        private static Microsoft.Maui.Graphics.IImage Clone(Microsoft.Maui.Graphics.IImage src)
        {
            var context = new SkiaBitmapExportContext((int)src.Width, (int)src.Height, 1.0f);
            context.Canvas.DrawImage(src, 0, 0, (int)src.Width, (int)src.Height);
            return context.Image;
        }

        private static Microsoft.Maui.Graphics.IImage Rotate(Microsoft.Maui.Graphics.IImage src, float degrees)
        {
            int w = (int)src.Width;
            int h = (int)src.Height;

            bool swap = degrees % 180 != 0;
            int tw = swap ? h : w;
            int th = swap ? w : h;

            var context = new SkiaBitmapExportContext(tw, th, 1.0f);
            var canvas = context.Canvas;

            canvas.SaveState();
            canvas.Translate(tw / 2f, th / 2f);
            canvas.Rotate(degrees);
            canvas.Translate(-w / 2f, -h / 2f);
            canvas.DrawImage(src, 0, 0, w, h);
            canvas.RestoreState();

            return context.Image;
        }

        // Platzhalter: nimm deine existierende Implementierung
        private static string ModifiedDate(string? exifDateTimeOriginal)
            => string.IsNullOrWhiteSpace(exifDateTimeOriginal) ? string.Empty : exifDateTimeOriginal;
    }


}