using Android.Graphics;
using System;
using System.IO;
using Xamarin.Forms;

namespace iPMCloud.Mobile.vo
{
    public class ImageResizer : IImageResizer
    {
        public ImageResizer()
        {
        }

        public void ClearBadge()
        {

        }



        public byte[] AddWatermark(byte[] imgByteArray, string date, string text, bool isThumb)
        {
            var bitmap = BitmapFactory.DecodeByteArray(imgByteArray, 0, imgByteArray.Length);
            var mutableBitmap = bitmap.Copy(Android.Graphics.Bitmap.Config.Argb8888, true);
            Canvas canvas = new Canvas(mutableBitmap);
            // Canvas canvas = new Canvas(bitmap);  
            Paint paintS = new Paint();
            paintS.Color = Android.Graphics.Color.Black;
            paintS.TextSize = isThumb ? 12 : 18;
            canvas.DrawText(text, 6, isThumb ? 25 : 25, paintS);
            canvas.DrawText(date, 6, isThumb ? 40 : 45, paintS);

            Paint paint = new Paint();
            paint.Color = Android.Graphics.Color.White;
            paint.TextSize = isThumb ? 12 : 18;
            canvas.DrawText(text, 5, isThumb ? 25 : 25, paint);
            canvas.DrawText(date, 5, isThumb ? 40 : 45, paint);

            MemoryStream stream = new MemoryStream();
            mutableBitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
            byte[] bitmapData = stream.ToArray();
            return bitmapData;
        }


        public Size GetSize(byte[] imageData)
        {
            var b = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            return new Size((double)b.Width, (double)b.Height);
        }

        public byte[] Rotate(byte[] Image, float degrees = 90)
        {
            Bitmap originalImage = BitmapFactory.DecodeByteArray(Image, 0, Image.Length);

            // Erstelle eine Matrix für die Rotation um 90 Grad
            Matrix matrix = new Matrix();
            matrix.PostRotate(degrees);

            // Rotiere das Bild
            Bitmap rotatedBitmap = Bitmap.CreateBitmap(originalImage, 0, 0, originalImage.Width, originalImage.Height, matrix, true);

            // Konvertiere das gedrehte Bild zurück in einen Stream
            using (MemoryStream ms = new MemoryStream())
            {
                rotatedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public byte[] Resize(float toWidth, float toHeight, byte[] Image, int quality = 100)
        {
            Bitmap originalImage = BitmapFactory.DecodeByteArray(Image, 0, Image.Length);
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)toWidth, (int)toHeight, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, ms);
                return ms.ToArray();
            }
        }

        public byte[] ResizeImage(float width, float height, byte[] imageData, int quality = 100)
        {
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            float targetHeight = 0;
            float targetWidth = 0;
            var heightO = originalImage.Height;
            var widthO = originalImage.Width;
            if (heightO > widthO)
            {
                targetHeight = height;
                float factor = heightO / height;
                targetWidth = widthO / factor;
            }
            else
            {
                targetWidth = width;
                float factor = widthO / width;
                targetHeight = heightO / factor;
            }
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)targetWidth, (int)targetHeight, false);
            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, ms);
                return ms.ToArray();
            }
        }

        public byte[] ResizeHeight(float toHeight, byte[] Image, int quality = 100)
        {
            Bitmap originalImage = BitmapFactory.DecodeByteArray(Image, 0, Image.Length);
            float toWidth = (Convert.ToSingle(originalImage.Width) / Convert.ToSingle(originalImage.Height)) * toHeight;
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)toWidth, (int)toHeight, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, ms);
                return ms.ToArray();
            }
        }

        public byte[] ResizeWidth(float toWidth, byte[] Image, int quality = 100)
        {
            Bitmap originalImage = BitmapFactory.DecodeByteArray(Image, 0, Image.Length);
            float toHeight = (Convert.ToSingle(originalImage.Height) / Convert.ToSingle(originalImage.Width)) * toWidth;
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)toWidth, (int)toHeight, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, ms);
                return ms.ToArray();
            }
        }


        /*
        ﻿using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Plugin.ImageResizer.ImageResizer))]
namespace Plugin.ImageResizer
    {
        /// <summary>
        /// Interface for Feature1
        /// </summary>
        public class ImageResizer : IImageResizer
        {
            public byte[] Resize(float height, float width, byte[] imageData)
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

            public byte[] ResizeHeight(float height, byte[] imageData)
            {
                UIImage originalImage = ImageFromByteArray(imageData);
                float width = (float)((originalImage.Size.Width / originalImage.Size.Height) * height);
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

            public byte[] ResizeWidth(float width, byte[] imageData)
            {
                UIImage originalImage = ImageFromByteArray(imageData);
                float height = (float)((originalImage.Size.Height / originalImage.Size.Width) * width);
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
            public static UIKit.UIImage ImageFromByteArray(byte[] data)
            {
                if (data == null)
                {
                    return null;
                }

                UIKit.UIImage image;
                try
                {
                    image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Image load failed: " + e.Message);
                    return null;
                }
                return image;
            }
        }
    }*/







    }
}
