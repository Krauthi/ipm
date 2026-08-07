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

        public static void Initialize(Context context)
        {
            try
            {
                _cameraManager = (CameraManager?)context.GetSystemService(Context.CameraService);
                if (_cameraManager != null)
                {
                    var cameraIdList = _cameraManager.GetCameraIdList();
                    if (cameraIdList != null && cameraIdList.Length > 0)
                    {
                        // Verwende die erste Kamera (normalerweise die Rückkamera)
                        foreach (var id in cameraIdList)
                        {
                            var characteristics = _cameraManager.GetCameraCharacteristics(id);
                            var hasFlash = (bool?)characteristics?.Get(CameraCharacteristics.FlashInfoAvailable);
                            if (hasFlash == true)
                            {
                                _cameraId = id;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.Initialize failed: {ex.Message}");
            }
        }

        public static bool TurnOn()
        {
            try
            {
                if (_cameraManager == null || string.IsNullOrEmpty(_cameraId))
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: CameraManager or CameraId not initialized");
                    return false;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    _cameraManager.SetTorchMode(_cameraId, true);
                    _isFlashlightOn = true;
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: Flashlight turned ON");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: Android version too old (needs API 23+)");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOn failed: {ex.Message}");
                return false;
            }
        }

        public static bool TurnOff()
        {
            try
            {
                if (_cameraManager == null || string.IsNullOrEmpty(_cameraId))
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: CameraManager or CameraId not initialized");
                    return false;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    _cameraManager.SetTorchMode(_cameraId, false);
                    _isFlashlightOn = false;
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: Flashlight turned OFF");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AndroidFlashlightService: Android version too old (needs API 23+)");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFlashlightService.TurnOff failed: {ex.Message}");
                return false;
            }
        }

        public static bool IsFlashlightOn()
        {
            return _isFlashlightOn;
        }

        public static bool IsAvailable()
        {
            return _cameraManager != null && !string.IsNullOrEmpty(_cameraId);
        }
    }
}
