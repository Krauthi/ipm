using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;
using NLog.Extensions.Logging;

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

            // Example: builder.Services.AddSingleton<IImageResizer, ImageResizer>();
            // TODO: Register all services that were previously using DependencyService

#if DEBUG
            builder.Logging.AddDebug();

            
#endif

            // TODO: Initialize Firebase
            // TODO: Initialize Maps
            // TODO: Initialize ZXing Scanner
            // TODO: Configure Permissions

            return builder.Build();
        }
    }
}
