using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;

namespace iPMCloud.Mobile.Platforms.Android.Services
{
    /// <summary>
    /// Native Android Flashlight Implementation
    /// Verwendet Camera2 API für bessere Kompatibilität
    /// </summary>
    public class AndroidFlashlightService
    {
        private static CameraManager? _cameraManager;
        private static string? _cameraId;
        private static bool _isFlashlightOn = false;
        private static bool _isInitialized = false;

        public static void Initialize(Context context)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: Initialize START");

                if (context == null)
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: Context is NULL!");
                    return;
                }

                _cameraManager = (CameraManager?)context.GetSystemService(Context.CameraService);
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService: CameraManager obtained: {_cameraManager != null}");

                if (_cameraManager != null)
                {
                    var cameraIdList = _cameraManager.GetCameraIdList();
                    System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService: Camera count: {cameraIdList?.Length ?? 0}");

                    if (cameraIdList != null && cameraIdList.Length > 0)
                    {
                        // Verwende die erste Kamera (normalerweise die Rückkamera)
                        foreach (var id in cameraIdList)
                        {
                            System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService: Checking camera ID: {id}");

                            var characteristics = _cameraManager.GetCameraCharacteristics(id);
                            var hasFlash = (bool?)characteristics?.Get(CameraCharacteristics.FlashInfoAvailable);

                            System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService: Camera {id} has flash: {hasFlash}");

                            if (hasFlash == true)
                            {
                                _cameraId = id;
                                _isInitialized = true;
                                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService: Selected camera ID: {_cameraId}");
                                break;
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService: Initialize COMPLETE - Initialized: {_isInitialized}, CameraId: {_cameraId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.Initialize EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.Initialize StackTrace: {ex.StackTrace}");
            }
        }

        public static bool TurnOn()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.TurnOn: START");
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOn: Initialized={_isInitialized}, CameraManager={_cameraManager != null}, CameraId={_cameraId}");

                // Versuche automatisch zu initialisieren, falls noch nicht geschehen
                if (!_isInitialized || _cameraManager == null || string.IsNullOrEmpty(_cameraId))
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.TurnOn: Not initialized, attempting auto-init");
                    var context = global::Android.App.Application.Context;
                    System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOn: Application.Context available: {context != null}");

                    if (context != null)
                    {
                        Initialize(context);
                    }

                    // Prüfe erneut nach Initialisierung
                    if (!_isInitialized || _cameraManager == null || string.IsNullOrEmpty(_cameraId))
                    {
                        System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOn: FAILED after auto-init - Initialized={_isInitialized}, CameraManager={_cameraManager != null}, CameraId={_cameraId}");
                        return false;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOn: Android SDK version: {(int)Build.VERSION.SdkInt}");

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    // Die Kamera kann kurz nach dem Schließen des Scanners noch belegt sein
                    // (CAMERA_IN_USE). Die Freigabe erfolgt asynchron, daher mehrere Versuche.
                    return SetTorchWithRetry(true);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.TurnOn: FAILED - Android version too old (needs API 23+)");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOn EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOn StackTrace: {ex.StackTrace}");
                _isFlashlightOn = false;
                return false;
            }
        }

        public static bool TurnOff()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.TurnOff: START");

                // Versuche automatisch zu initialisieren, falls noch nicht geschehen
                if (!_isInitialized || _cameraManager == null || string.IsNullOrEmpty(_cameraId))
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.TurnOff: Not initialized, attempting auto-init");
                    var context = global::Android.App.Application.Context;
                    if (context != null)
                    {
                        Initialize(context);
                    }

                    // Prüfe erneut nach Initialisierung
                    if (!_isInitialized || _cameraManager == null || string.IsNullOrEmpty(_cameraId))
                    {
                        System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.TurnOff: FAILED - Not initialized after auto-init");
                        return false;
                    }
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    return SetTorchWithRetry(false);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.TurnOff: FAILED - Android version too old (needs API 23+)");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOff EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOff StackTrace: {ex.StackTrace}");
                _isFlashlightOn = false;
                return false;
            }
        }

        /// <summary>
        /// Ruft SetTorchMode auf und wiederholt den Versuch, falls die Kamera noch
        /// von einem anderen Nutzer (z.B. dem gerade geschlossenen Scanner) belegt ist
        /// (CameraAccessException / CAMERA_IN_USE). Die Freigabe der Kamera erfolgt
        /// asynchron, daher sind mehrere Versuche mit kurzer Wartezeit nötig.
        /// </summary>
        private static bool SetTorchWithRetry(bool turnOn)
        {
            // Ein einzelner Versuch genügt: Ist die Kamera frei, gelingt SetTorchMode sofort.
            // Wird sie von der CameraX-Session des ReaderView gehalten (CAMERA_IN_USE), helfen
            // Wiederholungen nicht – dann übernimmt der FlashlightManager-Fallback über den
            // Kamera-Besitzer. So bleibt das Umschalten schnell und ohne UI-Verzögerung.
            const int maxAttempts = 1;
            const int delayMs = 150;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.SetTorchWithRetry: Attempt {attempt}/{maxAttempts} - SetTorchMode({_cameraId}, {turnOn})");
                    _cameraManager!.SetTorchMode(_cameraId!, turnOn);
                    _isFlashlightOn = turnOn;
                    System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.SetTorchWithRetry: SUCCESS - Flashlight {(turnOn ? "ON" : "OFF")}");
                    return true;
                }
                catch (CameraAccessException ex)
                {
                    // Kamera wird bereits verwendet (z.B. durch Scanner, dessen Freigabe noch läuft)
                    System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.SetTorchWithRetry: CameraAccessException on attempt {attempt} - Reason: {ex.Reason}, Message: {ex.Message}");

                    if (attempt < maxAttempts)
                    {
                        System.Threading.Thread.Sleep(delayMs);
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService.SetTorchWithRetry: FAILED - camera still in use after all attempts");
                    _isFlashlightOn = false;
                    return false;
                }
            }

            _isFlashlightOn = false;
            return false;
        }

        public static bool IsFlashlightOn()
        {
            return _isFlashlightOn;
        }

        public static bool IsAvailable()
        {
            return _isInitialized && _cameraManager != null && !string.IsNullOrEmpty(_cameraId);
        }
    }
}
