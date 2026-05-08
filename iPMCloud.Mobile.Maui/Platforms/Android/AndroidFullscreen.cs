#if ANDROID
using Android.App;
using Android.OS;
using Android.Views;
using Microsoft.Maui.ApplicationModel;

public static class AndroidFullscreen
{
    public static void SetFullscreen(bool enabled)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var activity = Platform.CurrentActivity;
            if (activity == null) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var controller = activity.Window.InsetsController;
                if (controller == null) return;

                if (enabled)
                {
                    controller.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                    controller.SystemBarsBehavior =
                        (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                }
                else
                {
                    controller.Show(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                }
            }
            else
            {
#pragma warning disable CS0618
                if (enabled)
                    activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                        SystemUiFlags.Fullscreen |
                        SystemUiFlags.HideNavigation |
                        SystemUiFlags.ImmersiveSticky |
                        SystemUiFlags.LayoutFullscreen |
                        SystemUiFlags.LayoutHideNavigation |
                        SystemUiFlags.LayoutStable);
                else
                    activity.Window.DecorView.SystemUiVisibility = StatusBarVisibility.Visible;
#pragma warning restore CS0618
            }
        });
    }
}
#endif