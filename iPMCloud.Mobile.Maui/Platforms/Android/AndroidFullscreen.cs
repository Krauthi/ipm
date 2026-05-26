#if ANDROID
using Android.App;
using AndroidX.Core.View;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile;

public static class AndroidFullscreen
{
    public static void SetFullscreen(bool enabled)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var activity = Platform.CurrentActivity;
            if (activity == null) return;

            var window = activity.Window;
            if (window == null || window.DecorView == null) return;

            WindowCompat.SetDecorFitsSystemWindows(window, !enabled);

            var controller = WindowCompat.GetInsetsController(window, window.DecorView);
            if (controller == null) return;

            if (enabled)
            {
                controller.Hide(WindowInsetsCompat.Type.StatusBars() | WindowInsetsCompat.Type.NavigationBars());
                controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
            }
            else
            {
                controller.Show(WindowInsetsCompat.Type.StatusBars() | WindowInsetsCompat.Type.NavigationBars());
            }
        });
    }
}
#endif