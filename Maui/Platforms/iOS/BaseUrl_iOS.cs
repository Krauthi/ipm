using Foundation;
using iPMCloud.Mobile.Interfaces;

namespace iPMCloud.Mobile.Platforms.iOS
{
    /// <summary>
    /// iOS-spezifische Implementation für BaseUrl
    /// Benötigt für WebView HTML-Rendering auf iOS
    /// </summary>
    public class BaseUrl_iOS : IBaseUrl
    {
        public string Get()
        {
            return NSBundle.MainBundle.BundlePath;
        }
    }
}
