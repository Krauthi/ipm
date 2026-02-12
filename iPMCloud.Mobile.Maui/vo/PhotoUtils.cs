using System;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

// Aliase für Namenskonflikte
using MauiImageSource = Microsoft.Maui.Controls.ImageSource;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;
using PointF = SixLabors.ImageSharp.PointF;
using Color = SixLabors.ImageSharp.Color;
using Directory = System.IO.Directory;

namespace iPMCloud.Mobile.vo
{
    public class PhotoUtils
    {
        /// <summary>
        /// Verarbeitet ein Bild: liest EXIF-Daten, rotiert und skaliert
        /// </summary>
        public static PhotoResponse GetImages(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            // Clone Stream für EXIF-Lesung
            Stream cloneStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(cloneStream);
            cloneStream.Position = 0;

            int orientation = 1;
            string createDate = string.Empty;

            try
            {
                // EXIF-Daten mit MetadataExtractor auslesen
                var directories = ImageMetadataReader.ReadMetadata(cloneStream);

                // Orientation auslesen
                var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                if (ifd0Directory != null && ifd0Directory.ContainsTag(ExifDirectoryBase.TagOrientation))
                {
                    orientation = ifd0Directory.GetInt32(ExifDirectoryBase.TagOrientation);
                }

                // DateTime Original auslesen
                var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (exifSubIfdDirectory != null && exifSubIfdDirectory.ContainsTag(ExifDirectoryBase.TagDateTimeOriginal))
                {
                    var dateTimeStr = exifSubIfdDirectory.GetString(ExifDirectoryBase.TagDateTimeOriginal);
                    createDate = ModifiedDate(dateTimeStr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading EXIF: {ex.Message}");
                AppModel.Logger?.Warn(ex, "Could not read EXIF data");
            }

            // Stream zurücksetzen
            cloneStream.Position = 0;

            byte[] imageBytes = null;
            byte[] thumbBytes = null;

            try
            {
                using (var image = ImageSharpImage.Load<Rgba32>(cloneStream))
                {
                    // Bild basierend auf EXIF-Orientation rotieren
                    ApplyOrientation(image, orientation);

                    // Vollbild (max 1024px Breite)
                    var resizedImage = ResizeImage(image, 1024);
                    imageBytes = ImageToBytes(resizedImage);

                    // Thumbnail (max 256px Breite)
                    var thumbImage = ResizeImage(image, 256);
                    thumbBytes = ImageToBytes(thumbImage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                AppModel.Logger?.Error(ex, "Error processing image");

                // Fallback: Original-Bytes verwenden
                cloneStream.Position = 0;
                using (var ms = new MemoryStream())
                {
                    cloneStream.CopyTo(ms);
                    imageBytes = ms.ToArray();
                    thumbBytes = ms.ToArray();
                }
            }

            return new PhotoResponse
            {
                imageBytes = imageBytes,
                thumbBytes = thumbBytes,
                createDate = createDate
            };
        }

        /// <summary>
        /// Wendet EXIF-Orientation auf das Bild an
        /// </summary>
        private static void ApplyOrientation(ImageSharpImage image, int orientation)
        {
            switch (orientation)
            {
                case 1: // Normal
                    // Keine Rotation nötig
                    break;
                case 2: // Horizontal flip
                    image.Mutate(x => x.Flip(FlipMode.Horizontal));
                    break;
                case 3: // Rotate 180°
                    image.Mutate(x => x.Rotate(180));
                    break;
                case 4: // Vertical flip
                    image.Mutate(x => x.Flip(FlipMode.Vertical));
                    break;
                case 5: // Transpose
                    image.Mutate(x => x.Rotate(90).Flip(FlipMode.Horizontal));
                    break;
                case 6: // Rotate 90° CW
                    image.Mutate(x => x.Rotate(90));
                    break;
                case 7: // Transverse
                    image.Mutate(x => x.Rotate(270).Flip(FlipMode.Horizontal));
                    break;
                case 8: // Rotate 270° CW
                    image.Mutate(x => x.Rotate(270));
                    break;
            }
        }

        /// <summary>
        /// Skaliert ein Bild auf maximale Breite
        /// </summary>
        private static Image<Rgba32> ResizeImage(Image<Rgba32> image, int maxWidth)
        {
            if (image.Width <= maxWidth)
            {
                return image.Clone(x => { }); // Keine Skalierung nötig
            }

            int newWidth = maxWidth;
            int newHeight = (int)((double)image.Height / image.Width * maxWidth);

            var resized = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(newWidth, newHeight),
                Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));

            return resized;
        }

        /// <summary>
        /// Konvertiert Image zu Byte-Array (JPEG)
        /// </summary>
        private static byte[] ImageToBytes(ImageSharpImage image)
        {
            using (var ms = new MemoryStream())
            {
                image.SaveAsJpeg(ms, new JpegEncoder
                {
                    Quality = 85
                });
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Formatiert EXIF-Datum zu lesbarem Format
        /// </summary>
        private static string ModifiedDate(string photodate)
        {
            // 2024:03:10 11:21:47 -> 10.03.2024 11:21
            if (string.IsNullOrWhiteSpace(photodate))
            {
                return string.Empty;
            }

            try
            {
                var s = photodate.Replace(" ", ":").Replace("T", ":").Split(':');
                if (s.Length >= 5)
                {
                    return $"{s[2]}.{s[1]}.{s[0]} {s[3]}:{s[4]}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing date: {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// Fügt Wasserzeichen (Standort + Datum) zum Bild hinzu
        /// </summary>
        public static PhotoResponse AddInfoToImage(PhotoResponse photoResponse, BuildingWSO b, string address = null)
        {
            if (photoResponse == null)
            {
                return photoResponse;
            }

            string text = string.Empty;

            if (b != null && address == null)
            {
                text = $"{b.plz} {b.ort} - {b.strasse} {b.hsnr}";
            }
            else if (!string.IsNullOrWhiteSpace(address))
            {
                text = address;
            }

            // Nur wenn imageIncludeLocation aktiviert ist
            if (AppModel.Instance?.AppControll?.imageIncludeLocation != true)
            {
                return photoResponse;
            }

            try
            {
                // Wasserzeichen zu Thumbnail hinzufügen
                if (photoResponse.thumbBytes != null)
                {
                    photoResponse.thumbBytes = AddWatermark(
                        photoResponse.thumbBytes,
                        photoResponse.createDate,
                        text,
                        isThumb: true
                    );
                }

                // Wasserzeichen zu Vollbild hinzufügen
                if (photoResponse.imageBytes != null)
                {
                    photoResponse.imageBytes = AddWatermark(
                        photoResponse.imageBytes,
                        photoResponse.createDate,
                        text,
                        isThumb: false
                    );
                }

                photoResponse.objektAdress = text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding watermark: {ex.Message}");
                AppModel.Logger?.Error(ex, "Error adding watermark to image");
            }

            return photoResponse;
        }

        /// <summary>
        /// Fügt Wasserzeichen mit Datum und Text hinzu
        /// </summary>
        private static byte[] AddWatermark(byte[] imageBytes, string date, string text, bool isThumb)
        {
            try
            {
                using (var image = ImageSharpImage.Load<Rgba32>(imageBytes))
                {
                    // Schriftgröße basierend auf Bildgröße
                    float fontSize = isThumb ? 12f : 24f;
                    float padding = isThumb ? 5f : 10f;

                    // System-Font verwenden (plattformübergreifend)
                    FontFamily fontFamily;
                    try
                    {
                        fontFamily = SystemFonts.Get("Arial");
                    }
                    catch
                    {
                        // Fallback auf ersten verfügbaren Font
                        fontFamily = SystemFonts.Families.FirstOrDefault();
                        //if (fontFamily == null)
                        //{
                        //    Console.WriteLine("No system fonts available");
                        //    return imageBytes;
                        //}
                    }

                    var font = fontFamily.CreateFont(fontSize, FontStyle.Bold);

                    // Watermark-Text zusammenstellen
                    string watermarkText = string.Empty;
                    if (!string.IsNullOrWhiteSpace(date))
                    {
                        watermarkText = date;
                    }
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        watermarkText += string.IsNullOrWhiteSpace(watermarkText) ? text : $"\n{text}";
                    }

                    if (string.IsNullOrWhiteSpace(watermarkText))
                    {
                        return imageBytes; // Kein Text zum Hinzufügen
                    }

                    image.Mutate(ctx =>
                    {
                        // Text-Optionen
                        var textOptions = new RichTextOptions(font)
                        {
                            Origin = new PointF(padding, image.Height - fontSize * 2 - padding),
                            WrappingLength = image.Width - (padding * 2),
                            HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Left,
                            VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Bottom
                        };

                        // Halbdurchsichtiger schwarzer Hintergrund für Text
                        var backgroundColor = new Rgba32(0, 0, 0, 180);
                        var textColor = Color.White;

                        // Text-Größe messen
                        var textSize = TextMeasurer.MeasureSize(watermarkText, textOptions);

                        // Hintergrund-Rechteck zeichnen
                        ctx.Fill(
                            backgroundColor,
                            new RectangleF(
                                padding - 5,
                                image.Height - textSize.Height - padding - 5,
                                textSize.Width + 10,
                                textSize.Height + 10
                            )
                        );

                        // Text zeichnen
                        ctx.DrawText(textOptions, watermarkText, textColor);
                    });

                    return ImageToBytes(image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddWatermark: {ex.Message}");
                AppModel.Logger?.Error(ex, "Error adding watermark");
                return imageBytes; // Original zurückgeben bei Fehler
            }
        }
    }

    /// <summary>
    /// Response mit verarbeiteten Bilddaten
    /// </summary>
    public class PhotoResponse
    {
        public byte[] imageBytes { get; set; } = null;
        public byte[] thumbBytes { get; set; } = null;
        public string createDate { get; set; } = string.Empty;
        public string objektAdress { get; set; } = string.Empty;

        public PhotoResponse() { }

        /// <summary>
        /// Gibt ImageSource für Vollbild zurück
        /// </summary>
        public MauiImageSource GetImageSource()
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return null;
            }

            return MauiImageSource.FromStream(() => new MemoryStream(imageBytes));
        }

        /// <summary>
        /// Gibt ImageSource für Thumbnail zurück
        /// </summary>
        public MauiImageSource GetImageSourceAsThumb()
        {
            if (thumbBytes == null || thumbBytes.Length == 0)
            {
                return null;
            }

            return MauiImageSource.FromStream(() => new MemoryStream(thumbBytes));
        }

        /// <summary>
        /// Speichert das Vollbild in eine Datei
        /// </summary>
        public bool SaveToFile(string filePath)
        {
            try
            {
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllBytes(filePath, imageBytes);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Speichert das Thumbnail in eine Datei
        /// </summary>
        public bool SaveThumbToFile(string filePath)
        {
            try
            {
                if (thumbBytes != null && thumbBytes.Length > 0)
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllBytes(filePath, thumbBytes);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving thumbnail: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gibt die Dateigröße des Vollbilds in KB zurück
        /// </summary>
        public double GetImageSizeKB()
        {
            return (imageBytes?.Length ?? 0) / 1024.0;
        }

        /// <summary>
        /// Gibt die Dateigröße des Thumbnails in KB zurück
        /// </summary>
        public double GetThumbSizeKB()
        {
            return (thumbBytes?.Length ?? 0) / 1024.0;
        }

        /// <summary>
        /// Gibt die Dateigröße des Vollbilds in MB zurück
        /// </summary>
        public double GetImageSizeMB()
        {
            return GetImageSizeKB() / 1024.0;
        }

        /// <summary>
        /// Prüft ob Bilddaten vorhanden sind
        /// </summary>
        public bool HasImage()
        {
            return imageBytes != null && imageBytes.Length > 0;
        }

        /// <summary>
        /// Prüft ob Thumbnail vorhanden ist
        /// </summary>
        public bool HasThumb()
        {
            return thumbBytes != null && thumbBytes.Length > 0;
        }

        /// <summary>
        /// Gibt eine Beschreibung zurück
        /// </summary>
        public override string ToString()
        {
            return $"PhotoResponse [Image: {GetImageSizeKB():F2} KB, Thumb: {GetThumbSizeKB():F2} KB, Date: {createDate}]";
        }
    }
}