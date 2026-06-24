using System;
using UIKit;

namespace iPMCloud.Mobile;

internal static partial class SyncExecutionCoordinator
{
    private static partial IDisposable BeginPlatformScope(SyncExecutionMode mode, string title, string message)
    {
        return new IosSyncExecutionScope(title);
    }

    private static partial void OnAppBackgroundChangedPlatform(bool isInBackground)
    {
    }
}

internal sealed class IosSyncExecutionScope : IDisposable
{
    private readonly UIApplication _application;
    private UIBackgroundTaskIdentifier _backgroundTaskId;

    public IosSyncExecutionScope(string title)
    {
        _application = UIApplication.SharedApplication;
        _backgroundTaskId = UIApplication.BackgroundTaskInvalid;
        _backgroundTaskId = _application.BeginBackgroundTask(title ?? "Synchronisierung", EndTask);
    }

    public void Dispose()
    {
        EndTask();
    }

    private void EndTask()
    {
        if (_backgroundTaskId == UIApplication.BackgroundTaskInvalid)
        {
            return;
        }

        _application.EndBackgroundTask(_backgroundTaskId);
        _backgroundTaskId = UIApplication.BackgroundTaskInvalid;
    }
}
