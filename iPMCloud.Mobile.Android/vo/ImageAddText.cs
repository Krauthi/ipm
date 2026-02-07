using Android.Graphics;
using iPMCloud.Mobile.vo;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageAddText))]
namespace iPMCloud.Mobile.vo
{
    public class ImageAddText : IImageAddText
    {
        public ImageAddText()
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
    }
}
