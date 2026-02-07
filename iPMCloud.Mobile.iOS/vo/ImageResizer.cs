using CoreGraphics;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace iPMCloud.Mobile.vo
{
    public class ImageResizer : IImageResizer
    {
        public ImageResizer()
        {
        }


        public void ClearBadge()
        {
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = -1;
        }



        public Xamarin.Forms.Size GetSize(byte[] imageData)
        {
            UIImage image = ImageFromByteArray(imageData);
            return new Xamarin.Forms.Size((double)image.Size.Width, (double)image.Size.Height);
        }



        private UIImage ImageFromBytes(byte[] bytes, nfloat width, nfloat height)
        {
            try
            {
                NSData data = NSData.FromArray(bytes);
                UIImage image = UIImage.LoadFromData(data);
                CGSize scaleSize = new CGSize(width, height);
                UIGraphics.BeginImageContextWithOptions(scaleSize, false, 0);
                image.Draw(new CGRect(0, 0, scaleSize.Width, scaleSize.Height));
                UIImage resizedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return resizedImage;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] ResizeImageIOS(byte[] imageData, float width, float height)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            UIImageOrientation orientation = originalImage.Orientation;

            //create a 24bit RGB image
            using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
                                                 (int)width, (int)height, 8,
                                                 4 * (int)width, CGColorSpace.CreateDeviceRGB(),
                                                 CGImageAlphaInfo.PremultipliedFirst))
            {

                RectangleF imageRect = new RectangleF(0, 0, width, height);

                // draw the image
                context.DrawImage(imageRect, originalImage.CGImage);

                UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage(), 0, orientation);

                // save the image as a jpeg
                return resizedImage.AsJPEG().ToArray();
            }
        }

        public byte[] ResizeImage(byte[] imageData, float width, float height, int quality = 100)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            var Hoehe = originalImage.Size.Height;
            var Breite = originalImage.Size.Width;
            nfloat ZielHoehe = 0;
            nfloat ZielBreite = 0;
            if (Hoehe > Breite)
            {
                ZielHoehe = height;
                nfloat teiler = Hoehe / height;
                ZielBreite = Breite / teiler;
            }
            else
            {
                ZielBreite = width;
                nfloat teiler = Breite / width;
                ZielHoehe = Hoehe / teiler;
            }
            width = (float)ZielBreite;
            height = (float)ZielHoehe;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            originalImage.Draw(new RectangleF(0, 0, width, height));
            var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            var bytesImagen = resizedImage.AsJPEG().ToArray();
            resizedImage.Dispose();
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
