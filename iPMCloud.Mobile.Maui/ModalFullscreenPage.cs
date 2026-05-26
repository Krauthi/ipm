using Microsoft.Maui.ApplicationModel;
#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
#endif

namespace iPMCloud.Mobile;

public abstract class ModalFullscreenPage : ContentPage
{
    private const int FirstDelayedReapplyMs = 60;
    private const int SecondDelayedReapplyMs = 160;
    private int _fullscreenPassId;

    protected ModalFullscreenPage()
    {
        ConfigurePageForFullscreen();
        Loaded += OnPageLoaded;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _fullscreenPassId++;
        ApplyFullscreenNow();
        _ = ReapplyFullscreenAsync(_fullscreenPassId);
    }

    protected override void OnDisappearing()
    {
        _fullscreenPassId++;
        base.OnDisappearing();
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        ApplyFullscreenNow();
    }

    private static void ApplyPlatformFullscreen()
    {
#if ANDROID
        AndroidFullscreen.SetFullscreen(true);
#endif
#if IOS
        iOSFullscreen.SetFullscreen(true);
#endif
    }

    private void ConfigurePageForFullscreen()
    {
        NavigationPage.SetHasNavigationBar(this, false);
#if IOS
        this.On<iOS>().SetUseSafeArea(false);
        this.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FullScreen);
#endif
    }

    private void ApplyFullscreenNow()
    {
        ConfigurePageForFullscreen();
        ApplyPlatformFullscreen();
    }

    private async Task ReapplyFullscreenAsync(int passId)
    {
        await Task.Yield();

        if (passId != _fullscreenPassId)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(ApplyFullscreenNow);
        await Task.Delay(FirstDelayedReapplyMs);

        if (passId != _fullscreenPassId || !IsVisible)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(ApplyFullscreenNow);
        await Task.Delay(SecondDelayedReapplyMs);

        if (passId != _fullscreenPassId || !IsVisible)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(ApplyFullscreenNow);
    }
}
