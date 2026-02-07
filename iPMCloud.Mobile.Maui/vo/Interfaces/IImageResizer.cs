using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    public interface IImageResizer
    {
        byte[] Rotate(byte[] Image, float degrees = 90);
        byte[] Resize(float width, float height, byte[] imageData, int quality);
        byte[] ResizeImage(float width, float height, byte[] imageData, int quality);
        byte[] ResizeHeight(float toHeight, byte[] Image, int quality = 100);
        byte[] ResizeWidth(float toWidth, byte[] Image, int quality = 100);

        Size GetSize(byte[] imageData);

        byte[] AddWatermark(byte[] imgByteArray, string date, string text, bool isThumb);
        void ClearBadge();

    }
}