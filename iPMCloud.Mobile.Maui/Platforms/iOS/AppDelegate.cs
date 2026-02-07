using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace iPMCloud.Mobile.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
    // TODO: Implement MAUI-compatible Firebase push notification support
    // The original iOS project used Plugin.FirebasePushNotification which is not MAUI-compatible
    // Consider using:
    // - Firebase.iOS NuGet package directly
    // - Community MAUI Firebase plugins when available
    
    // TODO: Implement MAUI-compatible background service
    // The original iOS project used Matcha.BackgroundService.iOS which is not MAUI-compatible
    // Consider using:
    // - Native iOS background tasks
    // - WorkManager alternatives for MAUI
}
