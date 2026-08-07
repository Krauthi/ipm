using Microsoft.Maui.Handlers;
using ZXing.Net.Maui;

#if ANDROID
using AndroidX.Camera.View;
using AndroidX.Camera.Lifecycle;
using Java.Interop;
#endif

namespace iPMCloud.Mobile.Platforms.Android.Handlers
{
    /// <summary>
    /// Custom handler for ZXing CameraBarcodeReaderView to prevent ObjectDisposedException
    /// when the camera is being disconnected during page navigation.
    /// </summary>
    public class SafeCameraBarcodeReaderHandler
    {
        public static void ConfigureHandler()
        {
#if ANDROID
            // TODO: Fix this handler for .NET 10 / MAUI compatibility
            // Temporarily disabled due to API changes in ZXing.Net.Maui
            System.Diagnostics.Debug.WriteLine("SafeCameraBarcodeReaderHandler: ConfigureHandler() skipped (compatibility issue)");

            /* DISABLED - needs update for current ZXing.Net.Maui version
            // Override the default disconnect behavior to safely handle disposed camera providers
            CameraBarcodeReaderViewHandler.Mapper.AppendToMapping("SafeCameraDisconnect", (handler, view) =>
            {
                // This mapping entry ensures our custom disconnect logic is in place
            });

            // We need to modify the Mapper to handle DisconnectHandler differently
            CameraBarcodeReaderViewHandler.DisconnectHandlerMapper.AppendToMapping(
                "SafeCameraDisconnect",
                (handler, view, platformView) =>
                {
                    SafeDisconnectCamera(platformView);
                });
            */
#endif
        }

#if ANDROID
        /* DISABLED - needs update for current ZXing.Net.Maui version
        private static void SafeDisconnectCamera(PreviewView previewView)
        {
            if (previewView == null)
                return;

            try
            {
                // Access the camera manager through reflection if needed
                // or handle the disconnect more safely
                var cameraManagerField = typeof(ZXing.Net.Maui.CameraManager)
                    .GetField("_cameraProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // Try to safely disconnect
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        // The standard disconnect will be called, but we catch any disposal exceptions
                        previewView?.Controller?.Unbind();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Camera provider already disposed - this is expected during navigation
                        System.Diagnostics.Debug.WriteLine("SafeCameraDisconnect: Camera provider already disposed (expected)");
                    }
                    catch (JniIdentityException)
                    {
                        // JNI object already disposed
                        System.Diagnostics.Debug.WriteLine("SafeCameraDisconnect: JNI object already disposed (expected)");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SafeCameraDisconnect: Unexpected error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SafeCameraDisconnect outer: {ex.Message}");
            }
        }
        */
#endif
    }
}
