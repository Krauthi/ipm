using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using NLog.Extensions.Logging;
using ZXing.Net.Maui.Controls;
using iPMCloud.Mobile.Services;
using iPMCloud.Mobile.Interfaces;
using MintedTextEditor.Maui;
using Microsoft.Maui.Handlers;

#if ANDROID
using iPMCloud.Mobile.Platforms.Android;
using iPMCloud.Mobile.Platforms.Android.Handlers;
#elif IOS
using iPMCloud.Mobile.Platforms.iOS;
using UIKit;
using WebKit;
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

            // Platform-specific upload service
#if ANDROID
            builder.Services.AddSingleton<IUploadService, AndroidUploadService>();
            builder.Services.AddSingleton<IBaseUrl, BaseUrl_Android>();
#elif IOS
            builder.Services.AddSingleton<IUploadService, iOSUploadService>();
            builder.Services.AddSingleton<IBaseUrl, BaseUrl_iOS>();
#endif

            // Platform-specific background sync service
#if ANDROID
            builder.Services.AddSingleton<IBackgroundSyncService, iPMCloud.Mobile.Platforms.Android.BackgroundSyncService>();
#elif IOS
            builder.Services.AddSingleton<IBackgroundSyncService, iPMCloud.Mobile.Platforms.iOS.BackgroundSyncService>();
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
            {
                if (handler.PlatformView.TitleLabel is { } titleLabel)
                    titleLabel.AdjustsFontForContentSizeCategory = false;
            });

            SearchBarHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
            {
                if (handler.PlatformView.SearchTextField is { } searchTextField)
                    searchTextField.AdjustsFontForContentSizeCategory = false;
            });

            PickerHandler.Mapper.AppendToMapping("NoDynamicType", (handler, _) =>
                handler.PlatformView.AdjustsFontForContentSizeCategory = false);

            // WebView theme support for iOS
            WebViewHandler.Mapper.AppendToMapping("ThemeSupport", (handler, view) =>
            {
                if (handler.PlatformView is WKWebView wkWebView)
                {
                    wkWebView.Opaque = true;  // ÄNDERUNG: true statt false macht iOS Content sichtbar!

                    // Dynamische Farbe basierend auf Theme
                    var isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
                    var backgroundColor = isDarkMode 
                        ? UIColor.FromRGB(42, 42, 58)  // Dark: #2a2a3a
                        : UIColor.White;                // Light: #ffffff

                    wkWebView.BackgroundColor = backgroundColor;
                    wkWebView.ScrollView.BackgroundColor = backgroundColor;

                    // iOS-Fix: Scroll aktivieren und Bouncing erlauben
                    wkWebView.ScrollView.ScrollEnabled = true;
                    wkWebView.ScrollView.Bounces = true;
                    wkWebView.ScrollView.AlwaysBounceVertical = true;
                }
            });
#endif

#if ANDROID
            // WebView theme support for Android
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("ThemeSupport", (handler, view) =>
            {
                if (handler.PlatformView is Android.Webkit.WebView androidWebView)
                {
                    // Dynamische Farbe basierend auf Theme
                    var isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
                    var backgroundColor = isDarkMode
                        ? Android.Graphics.Color.Rgb(42, 42, 58)  // Dark: #2a2a3a
                        : Android.Graphics.Color.White;            // Light: #ffffff

                    androidWebView.SetBackgroundColor(backgroundColor);
                }
            });

            // Configure safe modal navigation handler to prevent NullPointerException
            // in ModalNavigationManager.ModalFragment.CustomComponentDialog.DispatchTouchEvent
            SafeModalNavigationHandler.ConfigureHandler();
#endif

            return builder.Build();
        }
    }
}
