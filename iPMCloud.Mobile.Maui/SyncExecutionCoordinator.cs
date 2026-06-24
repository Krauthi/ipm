using System;

namespace iPMCloud.Mobile;

internal enum SyncExecutionMode
{
    Foreground = 0,
    Background = 1,
}

internal static partial class SyncExecutionCoordinator
{
    public static IDisposable BeginForeground(string title, string message)
    {
        return BeginPlatformScope(SyncExecutionMode.Foreground, title, message);
    }

    public static IDisposable BeginBackground(string title, string message)
    {
        return BeginPlatformScope(SyncExecutionMode.Background, title, message);
    }

    public static void OnAppBackgroundChanged(bool isInBackground)
    {
        OnAppBackgroundChangedPlatform(isInBackground);
    }

    private static partial IDisposable BeginPlatformScope(SyncExecutionMode mode, string title, string message);

    private static partial void OnAppBackgroundChangedPlatform(bool isInBackground);
}
