using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Runtime;
using System;
using System.Diagnostics;
using Java.Lang;

namespace iPMCloud.Mobile.Platforms.Android.Handlers
{
    /// <summary>
    /// Optimiert die Kamera-Einstellungen für QR-Code-Scanning auf Samsung und anderen Android-Geräten.
    /// Verbessert Fokus, Belichtung und Frame-Rate für schnellere und zuverlässigere Erkennung.
    /// </summary>
    public static class CameraOptimizationHandler
    {
        private const string TAG = "CameraOptimization";

        /// <summary>
        /// Konfiguriert die Kamera für optimales QR-Code-Scanning.
        /// Besonders wichtig für Samsung S21, S24, S26 und ähnliche Geräte.
        /// </summary>
        public static void ConfigureCameraForQRScanning(CaptureRequest.Builder requestBuilder)
        {
            try
            {
                if (requestBuilder == null)
                {
                    Debug.WriteLine($"[{TAG}] requestBuilder is null");
                    return;
                }

                // Autofokus auf kontinuierlichen Video-Modus für schnelles Scannen
                requestBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousVideo);

                // Belichtungsmodus auf automatisch
                requestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);

                // Weißabgleich auf automatisch
                requestBuilder.Set(CaptureRequest.ControlAwbMode, (int)ControlAwbMode.Auto);

                // Stabilisierung aktivieren (falls verfügbar)
                try
                {
                    requestBuilder.Set(CaptureRequest.LensOpticalStabilizationMode, (int)LensOpticalStabilizationMode.On);
                }
                catch
                {
                    // Nicht alle Geräte unterstützen OIS
                }

                try
                {
                    requestBuilder.Set(CaptureRequest.ControlVideoStabilizationMode, (int)ControlVideoStabilizationMode.On);
                }
                catch
                {
                    // Nicht alle Geräte unterstützen Video-Stabilisierung
                }

                // Edge-Enhancement für schärfere QR-Codes
                requestBuilder.Set(CaptureRequest.EdgeMode, (int)EdgeMode.HighQuality);

                // Rauschunterdrückung auf High Quality
                requestBuilder.Set(CaptureRequest.NoiseReductionMode, (int)NoiseReductionMode.HighQuality);

                // Frame-Rate auf Maximum für schnelle Erkennung
                requestBuilder.Set(CaptureRequest.ControlCaptureIntent, (int)ControlCaptureIntent.VideoRecord);

                // Tonmapping für besseren Kontrast (wichtig für QR-Codes)
                requestBuilder.Set(CaptureRequest.TonemapMode, (int)TonemapMode.HighQuality);

                Debug.WriteLine($"[{TAG}] Camera optimizations applied successfully");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[{TAG}] Error applying camera optimizations: {ex.Message}");
                // Fehlschlagen ist nicht kritisch - die Kamera funktioniert trotzdem
            }
        }

        /// <summary>
        /// Prüft, ob das Gerät die Camera2 API unterstützt.
        /// </summary>
        public static bool IsCamera2Supported(global::Android.Content.Context context)
        {
            try
            {
                var cameraManager = context.GetSystemService(global::Android.Content.Context.CameraService) as CameraManager;
                if (cameraManager == null)
                    return false;

                var cameraIds = cameraManager.GetCameraIdList();
                if (cameraIds == null || cameraIds.Length == 0)
                    return false;

                var characteristics = cameraManager.GetCameraCharacteristics(cameraIds[0]);
                var levelObj = characteristics.Get(CameraCharacteristics.InfoSupportedHardwareLevel);
                if (levelObj is Integer levelInt)
                {
                    var level = levelInt.IntValue();
                    // Camera2 wird ab HARDWARE_LEVEL_LIMITED unterstützt
                    return level >= (int)InfoSupportedHardwareLevel.Limited;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[{TAG}] Error checking Camera2 support: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gibt Geräte-spezifische Kamera-Informationen aus (für Debugging).
        /// </summary>
        public static void LogCameraCapabilities(global::Android.Content.Context context)
        {
            try
            {
                var cameraManager = context.GetSystemService(global::Android.Content.Context.CameraService) as CameraManager;
                if (cameraManager == null)
                    return;

                var cameraIds = cameraManager.GetCameraIdList();
                foreach (var id in cameraIds)
                {
                    var characteristics = cameraManager.GetCameraCharacteristics(id);
                    var levelObj = characteristics.Get(CameraCharacteristics.InfoSupportedHardwareLevel);

                    Debug.WriteLine($"[{TAG}] Camera {id}: Level={levelObj}");

                    var afModesObj = characteristics.Get(CameraCharacteristics.ControlAfAvailableModes);
                    if (afModesObj != null)
                    {
                        Debug.WriteLine($"[{TAG}] Camera {id}: AF Modes available");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[{TAG}] Error logging camera capabilities: {ex.Message}");
            }
        }
    }
}
