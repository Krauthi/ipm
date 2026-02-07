using CoreGraphics;
using System;
using UIKit;

namespace iPMCloud.Mobile.vo
{
    public class ImageAddText : IImageAddText
    {
        public ImageAddText()
        {
        }

        public byte[] AddWatermark(byte[] imgByteArray, string date, string text, bool isThumb)
        {
            UIImage image = ImageFromByteArray(imgByteArray);
            var Hoehe = image.Size.Height;
            var Breite = image.Size.Width;


            UIGraphics.BeginImageContext(new CGSize(image.Size.Width, image.Size.Height));
            using (CGContext context = UIGraphics.GetCurrentContext())
            {
                // Copy original image
                var rect = new CGRect(0, 0, image.Size.Width, image.Size.Height);
                context.SetFillColor(UIColor.Black.CGColor);
                image.Draw(rect);

                // Use ScaleCTM to correct upside-down imaging
                context.ScaleCTM(1f, -1f);

                // Set the fill color for the text
                context.SetTextDrawingMode(CGTextDrawingMode.Fill);
                context.SetFillColor(UIColor.FromRGB(255, 255, 255).CGColor);

                // Draw the text with textSize
                var textSize = isThumb ? 12f : 18f;
                context.SelectFont("Arial", textSize, CGTextEncoding.MacRoman);
                context.ShowTextAtPoint(5, isThumb ? 25 : 25, text);
                context.SelectFont("Arial", textSize, CGTextEncoding.MacRoman);
                context.ShowTextAtPoint(5, isThumb ? 42 : 48, text);
            }

            // Get the resulting image from context
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            var bytesImagen = resultImage.AsJPEG().ToArray();
            resultImage.Dispose();
            return bytesImagen;
        }



        public UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            UIImage image;
            try
            {
                image = new UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }


    }
}
