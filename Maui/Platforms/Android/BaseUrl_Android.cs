using iPMCloud.Mobile.Interfaces;

namespace iPMCloud.Mobile.Platforms.Android
{
    /// <summary>
    /// Android-spezifische Implementation für BaseUrl
    /// </summary>
    public class BaseUrl_Android : IBaseUrl
    {
        public string Get()
        {
            return "file:///android_asset/";
        }
    }
}
