using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System;

namespace iPMCloud.Mobile;

internal static partial class SyncExecutionCoordinator
{
    private static partial IDisposable BeginPlatformScope(SyncExecutionMode mode, string title, string message)
    {
        return new AndroidSyncExecutionScope(mode, title, message);
    }

    private static partial void OnAppBackgroundChangedPlatform(bool isInBackground)
    {
        SyncExecutionService.UpdateAppBackground(isInBackground);
    }
}

internal sealed class AndroidSyncExecutionScope : IDisposable
{
    private bool _disposed;

    public AndroidSyncExecutionScope(SyncExecutionMode mode, string title, string message)
    {
        SyncExecutionService.Start(mode, title, message);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        SyncExecutionService.Stop();
    }
}

[Service(Exported = false, ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
public sealed class SyncExecutionService : Service
{
    private const string NotificationChannelId = "ipmcloud_sync_channel";
    private const int NotificationId = 1101;
    private const string DefaultTitle = "Synchronisierung";
    private const string DefaultMessage = "Synchronisierung läuft.";
    private static readonly object SyncRoot = new();
    private static PowerManager.WakeLock _wakeLock;
    private static SyncExecutionService _currentInstance;
    private static SyncExecutionMode _currentMode = SyncExecutionMode.Background;
    private static string _currentTitle = DefaultTitle;
    private static string _currentMessage = DefaultMessage;
    private static bool _appInBackground;

    public static void Start(SyncExecutionMode mode, string title, string message)
    {
        var context = Application.Context;
        var intent = new Intent(context, typeof(SyncExecutionService));
        intent.PutExtra(nameof(SyncExecutionMode), (int)mode);
        intent.PutExtra(nameof(_currentTitle), title ?? DefaultTitle);
        intent.PutExtra(nameof(_currentMessage), message ?? DefaultMessage);

        if (mode == SyncExecutionMode.Foreground && Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            context.StartForegroundService(intent);
            return;
        }

        context.StartService(intent);
    }

    public static void Stop()
    {
        Application.Context.StopService(new Intent(Application.Context, typeof(SyncExecutionService)));
    }

    public static void UpdateAppBackground(bool isInBackground)
    {
        lock (SyncRoot)
        {
            _appInBackground = isInBackground;
            _currentInstance?.UpdateForegroundState();
        }
    }

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        lock (SyncRoot)
        {
            _currentInstance = this;
            _currentMode = (SyncExecutionMode)(intent?.GetIntExtra(nameof(SyncExecutionMode), (int)SyncExecutionMode.Background) ?? (int)SyncExecutionMode.Background);
            _currentTitle = intent?.GetStringExtra(nameof(_currentTitle)) ?? DefaultTitle;
            _currentMessage = intent?.GetStringExtra(nameof(_currentMessage)) ?? DefaultMessage;
            _appInBackground = AppModel.Instance?.isInBackground ?? false;

            EnsureWakeLock();
            UpdateForegroundState();
        }

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        lock (SyncRoot)
        {
            if (ReferenceEquals(_currentInstance, this))
            {
                _currentInstance = null;
            }

            ReleaseWakeLock();
#pragma warning disable CS0618
            StopForeground(true);
#pragma warning restore CS0618
        }

        base.OnDestroy();
    }

    private void UpdateForegroundState()
    {
        if (_currentMode == SyncExecutionMode.Foreground || _appInBackground)
        {
            EnsureNotificationChannel();
            StartForeground(NotificationId, BuildNotification());
            return;
        }

#pragma warning disable CS0618
        StopForeground(true);
#pragma warning restore CS0618
    }

    private Notification BuildNotification()
    {
        var openIntent = new Intent(this, typeof(MainActivity));
        openIntent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            pendingIntentFlags |= PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetActivity(this, 0, openIntent, pendingIntentFlags);
        var contentText = _currentMode == SyncExecutionMode.Foreground
            ? _currentMessage
            : "Synchronisierung läuft im Hintergrund weiter.";

        return new NotificationCompat.Builder(this, NotificationChannelId)
            .SetContentTitle(_currentTitle)
            .SetContentText(contentText)
            .SetSmallIcon(Resource.Drawable.ipmlogo_m)
            .SetOngoing(true)
            .SetOnlyAlertOnce(true)
            .SetContentIntent(pendingIntent)
            .Build();
    }

    private void EnsureNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var notificationManager = GetSystemService(NotificationService) as NotificationManager;
        if (notificationManager?.GetNotificationChannel(NotificationChannelId) != null)
        {
            return;
        }

        var channel = new NotificationChannel(
            NotificationChannelId,
            "iPM Synchronisierung",
            NotificationImportance.Low)
        {
            Description = "Benachrichtigungen für laufende Synchronisierungen."
        };

        notificationManager?.CreateNotificationChannel(channel);
    }

    private void EnsureWakeLock()
    {
        if (_wakeLock?.IsHeld == true)
        {
            return;
        }

        var powerManager = GetSystemService(PowerService) as PowerManager;
        _wakeLock = powerManager?.NewWakeLock(WakeLockFlags.Partial, $"{PackageName}:SyncExecution");
        if (_wakeLock == null)
        {
            return;
        }

        _wakeLock.SetReferenceCounted(false);
        _wakeLock.Acquire();
    }

    private void ReleaseWakeLock()
    {
        if (_wakeLock?.IsHeld == true)
        {
            _wakeLock.Release();
        }

        _wakeLock = null;
    }
}
