using System;
using System.IO;
using Microsoft.Maui.Controls;
// TODO: ExifLib not available - needs replacement or alternative
// using ExifLib;
// TODO: FFImageLoading not MAUI-compatible - consider FFImageLoading.Maui
// using FFImageLoading;

namespace iPMCloud.Mobile.vo
{
    public class PhotoUtils
    {
        public static PhotoResponse GetImages(Stream stream)
        {
            Stream cloneStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(cloneStream);
            cloneStream.Position = 0;
            ExifOrientation orientation = ExifOrientation.TopLeft;
            JpegInfo photoInfo = null;
            try
            {
                photoInfo = ExifReader.ReadJpeg(cloneStream);
                orientation = photoInfo.Orientation;
            }
            catch (Exception ex)
            {
                photoInfo = null;
            }

            stream.Position = 0;
            var bytes = stream.ToByteArray();

            // TODO: Replace DependencyService with DI - IImageResizer
            // Migrate to MAUI DI system via MauiProgram.cs
            // bytes = DependencyService.Get<IImageResizer>().ResizeWidth(1024, bytes, 50);
            // if (orientation == ExifOrientation.TopRight)
            // {
            //     bytes = DependencyService.Get<IImageResizer>().Rotate(bytes, 90);
            // }
            // else if (orientation == ExifOrientation.BottomRight)
            // {
            //     bytes = DependencyService.Get<IImageResizer>().Rotate(bytes, 180);
            // }
            // else if (orientation == ExifOrientation.BottomLeft)
            // {
            //     bytes = DependencyService.Get<IImageResizer>().Rotate(bytes, 270);
            // }
            // var thumbbytes = DependencyService.Get<IImageResizer>().ResizeWidth(256, bytes, 50);

            // Temporary: Return unprocessed images until IImageResizer is migrated
            var thumbbytes = bytes;

            return new PhotoResponse
            {
                imageBytes = bytes,
                thumbBytes = thumbbytes,
                createDate = photoInfo != null ? ModifiedDate(photoInfo.DateTimeOriginal) : ""
            };
        }
        private static string ModifiedDate(string photodate)
        {
            // 2024:03:10 11:21:47   to    10.03.2024 11:21
            if (string.IsNullOrWhiteSpace(photodate)) { return ""; }
            var s = photodate.Replace(" ", ":").Replace("T", ":").Split(':');
            if (s.Length > 5)
            {
                return s[2] + "." + s[1] + "." + s[0] + " " + s[3] + ":" + s[4];
            }
            return "";
        }

        public static PhotoResponse AddInfoToImage(PhotoResponse photoResponse, BuildingWSO b, string address = null)
        {
            string text = "";
            if (b != null && address == null)
            {
                text = b.plz + " " + b.ort + " - " + b.strasse + " " + b.hsnr;
            }
            else
            {
                text = address != null ? address : "";
            }

            // TODO: Replace DependencyService with DI - IImageResizer
            // Migrate to MAUI DI system via MauiProgram.cs
            /*
            if (AppModel.Instance.AppControll.imageIncludeLocation && AppModel.Instance.DeviceSystem == "android")
            {
                photoResponse.thumbBytes = DependencyService.Get<IImageResizer>().AddWatermark(photoResponse.thumbBytes, photoResponse.createDate, text, true);
            }
            if (AppModel.Instance.AppControll.imageIncludeLocation && AppModel.Instance.DeviceSystem == "android")
            {
                photoResponse.imageBytes = DependencyService.Get<IImageResizer>().AddWatermark(photoResponse.imageBytes, photoResponse.createDate, text, false);
            }
            if (AppModel.Instance.AppControll.imageIncludeLocation && AppModel.Instance.DeviceSystem == "ios")
            {
                photoResponse.thumbBytes = DependencyService.Get<IImageResizer>().AddWatermark(photoResponse.thumbBytes, photoResponse.createDate, text, true);
            }
            if (AppModel.Instance.AppControll.imageIncludeLocation && AppModel.Instance.DeviceSystem == "ios")
            {
                photoResponse.imageBytes = DependencyService.Get<IImageResizer>().AddWatermark(photoResponse.imageBytes, photoResponse.createDate, text, false);
            }
            */
            return photoResponse;
        }
    }


    public class PhotoResponse
    {
        public byte[] imageBytes = null;

        public byte[] thumbBytes = null;

        public string createDate = "";

        public string objektAdress = "";

        public PhotoResponse() { }
        public ImageSource GetImageSource()
        {
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }
        public ImageSource GetImageSourceAsThumb()
        {
            return ImageSource.FromStream(() => new MemoryStream(thumbBytes));
        }
    }

}
