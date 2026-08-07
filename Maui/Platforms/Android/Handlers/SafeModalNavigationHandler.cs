using Android.App;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace iPMCloud.Mobile.Platforms.Android.Handlers
{
    /// <summary>
    /// Custom handler to fix NullPointerException in ModalNavigationManager.ModalFragment.CustomComponentDialog
    /// when DispatchTouchEvent is called on a null view reference.
    /// </summary>
    public class SafeModalNavigationHandler
    {
        public static void ConfigureHandler()
        {
#if ANDROID
            // Override the default Window handler to use a custom modal implementation
            WindowHandler.Mapper.AppendToMapping("SafeModalNavigation", (handler, view) =>
            {
                if (handler.PlatformView is Activity activity)
                {
                    // The actual fix is applied through MainActivity.DispatchTouchEvent override
                    // This handler ensures the mapping is registered
                }
            });
#endif
        }
    }
}
