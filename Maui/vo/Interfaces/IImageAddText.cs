namespace iPMCloud.Mobile
{
    public interface IImageAddText
    {

        byte[] AddWatermark(byte[] imgByteArray, string date, string text, bool isThumb);

    }
}