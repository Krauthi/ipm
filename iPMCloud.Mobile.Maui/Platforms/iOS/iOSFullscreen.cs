#if IOS
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace iPMCloud.Mobile;

public static class iOSFullscreen
{
    public static void SetFullscreen(bool enabled)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplyStatusBarVisibility(enabled);
            ApplyToActiveViewController(enabled);
        });
    }

    private static void ApplyStatusBarVisibility(bool hidden)
    {
        UIApplication.SharedApplication.SetStatusBarHidden(hidden, UIStatusBarAnimation.None);
    }

    private static void ApplyToActiveViewController(bool hidden)
    {
        var activeScene = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .FirstOrDefault(scene => scene.ActivationState == UISceneActivationState.ForegroundActive);

        if (activeScene == null)
        {
            return;
        }

        foreach (var window in activeScene.Windows)
        {
            if (window.RootViewController == null)
            {
                continue;
            }

            ApplyToController(window.RootViewController, hidden);
        }
    }

    private static void ApplyToController(UIViewController controller, bool hidden)
    {
        if (hidden)
        {
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
        }

        controller.ModalPresentationCapturesStatusBarAppearance = hidden;
        controller.SetNeedsStatusBarAppearanceUpdate();

        var presentedController = controller.PresentedViewController;
        if (presentedController != null)
        {
            ApplyToController(presentedController, hidden);
        }

        foreach (var child in controller.Children)
        {
            ApplyToController(child, hidden);
        }
    }
}
#endif
