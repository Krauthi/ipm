using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using NLog.Extensions.Logging;
using ZXing.Net.Maui.Controls;
using iPMCloud.Mobile.Services;
using MintedTextEditor.Maui;

#if ANDROID
using iPMCloud.Mobile.Platforms.Android;
#elif IOS
using iPMCloud.Mobile.Platforms.iOS;
using Microsoft.Maui.Handlers;
using UIKit;
#endif

namespace iPMCloud.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Initialize NLog as early as possible, before App() constructor runs,
            // to ensure all startup log calls are captured.
            try
            {
                new LogService().Initialize(typeof(MauiProgram).Assembly, "iPMCloud.Mobile.Maui");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NLog initialization failed: {ex.Message}");
            }

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .UseMauiCommunityToolkit()
                .UseMintedTextEditor()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
                  // NLog aktivieren

            builder.Logging.ClearProviders();              // Standard-Logger entfernen
            builder.Logging.SetMinimumLevel(LogLevel.Trace);
            builder.Logging.AddNLog();
            // Configure services for dependency injection
            // Migrate DependencyService registrations here

            // Platform-specific sync service (Android: ForegroundService; iOS: inline)
#if ANDROID
            builder.Services.AddSingleton<ISyncService, AndroidSyncService>();
            builder.Services.AddSingleton<IUploadService, AndroidUploadService>();
#elif IOS
            builder.Services.AddSingleton<ISyncService, iOSSyncService>();
            builder.Services.AddSingleton<IUploadService, iOSUploadService>();
#endif

            // Example: builder.Services.AddSingleton<IImageResizer, ImageResizer>();
            // TODO: Register all services that were previously using DependencyService

#if DEBUG
            builder.Logging.AddDebug();

            
#endif

            // TODO: Initialize Firebase
            // TODO: Initialize Maps
            // TODO: Initialize ZXing Scanner
            // TODO: Configure Permissions

#if IOS
            // Disable Dynamic Type so that changing the system text size in iOS Settings
            // does not affect the app's font sizes (mirrors the Android FontScale = 1.0f fix).
            LabelHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
                handler.PlatformView.AdjustsFontForContentSizeCategory = false);

            EntryHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
                handler.PlatformView.AdjustsFontForContentSizeCategory = false);

            EditorHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
                handler.PlatformView.AdjustsFontForContentSizeCategory = false);

            ButtonHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
                handler.PlatformView.TitleLabel.AdjustsFontForContentSizeCategory = false);

            SearchBarHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
            {
                handler.PlatformView.SearchTextField.AdjustsFontForContentSizeCategory = false;
            });

            PickerHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
                handler.PlatformView.AdjustsFontForContentSizeCategory = false);
#endif

            return builder.Build();
        }
    }
}
