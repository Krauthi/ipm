#if IOS
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace iPMCloud.Mobile;

public static class iOSFullscreen
{
    private static UIWindow? _overlayWindow;

    public static void SetFullscreen(bool enabled)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (enabled)
                ShowOverlayThatHidesStatusBar();
            else
                HideOverlay();
        });
    }

    private static void ShowOverlayThatHidesStatusBar()
    {
        if (_overlayWindow != null)
            return;

        var windowScene = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .FirstOrDefault(s => s.ActivationState == UISceneActivationState.ForegroundActive);

        if (windowScene == null)
            return;

        _overlayWindow = new UIWindow(windowScene)
        {
            WindowLevel = UIWindowLevel.StatusBar + 1, // über der Statusbar
            RootViewController = new StatusBarHiddenController(),
            Hidden = false
        };

        _overlayWindow.MakeKeyAndVisible();
    }

    private static void HideOverlay()
    {
        if (_overlayWindow == null)
            return;

        _overlayWindow.Hidden = true;
        _overlayWindow.RootViewController?.Dispose();
        _overlayWindow.Dispose();
        _overlayWindow = null;
    }

    private sealed class StatusBarHiddenController : UIViewController
    {
        public override bool PrefersStatusBarHidden() => true;
    }
}
#endif