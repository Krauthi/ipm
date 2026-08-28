using System;
using ZXing.Net.Maui.Controls;

namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Zentrale Taschenlampen-Verwaltung für die gesamte App.
    /// Unterstützt sowohl direkte Taschenlampen-Steuerung als auch Scanner-basierte Steuerung.
    /// </summary>
    public static class FlashlightManager
    {
        private static CameraBarcodeReaderView? _activeScanner;
        private static CameraBarcodeReaderView? _lastKnownScanner;
        private static bool _isFlashlightOn = false;

        /// <summary>
        /// Registriert einen aktiven Scanner, damit die Taschenlampe darüber gesteuert werden kann
        /// </summary>
        public static void RegisterScanner(CameraBarcodeReaderView scanner)
        {
            _activeScanner = scanner;
            _lastKnownScanner = scanner;
            System.Diagnostics.Debug.WriteLine("FlashlightManager: Scanner registered");
        }

        /// <summary>
        /// Entfernt die Scanner-Registrierung.
        /// Hinweis: Die Referenz auf den Scanner (_lastKnownScanner) bleibt erhalten, da dessen
        /// Kamera-Session u.U. noch aktiv ist und die Taschenlampe nur über den Kamera-Besitzer
        /// geschaltet werden kann (CAMERA_IN_USE bei der nativen API).
        /// </summary>
        public static void UnregisterScanner()
        {
            if (_activeScanner != null)
            {
                // Stelle sicher, dass die Taschenlampe ausgeschaltet ist
                try
                {
                    _activeScanner.IsTorchOn = false;
                }
                catch { }

                _activeScanner = null;
                System.Diagnostics.Debug.WriteLine("FlashlightManager: Scanner unregistered");
            }
        }

        /// <summary>
        /// Registriert den Kamera-Besitzer (z.B. den ReaderView der MainPage), dessen CameraX-Session
        /// die Kamera auf Android dauerhaft hält – auch ohne aktives Scannen. Nur so kann die
        /// Taschenlampe über den Besitzer geschaltet werden, wenn die native API CAMERA_IN_USE meldet.
        /// Beeinflusst NICHT den aktiven Scan-Modus (_activeScanner).
        /// </summary>
        public static void RegisterCameraOwner(CameraBarcodeReaderView scanner)
        {
            _lastKnownScanner = scanner;
            System.Diagnostics.Debug.WriteLine("FlashlightManager: Camera owner registered");
        }

        /// <summary>
        /// Löst die zwischengespeicherte Scanner-Referenz vollständig auf.
        /// Sollte aufgerufen werden, wenn die Kamera-Session des Scanners nachweislich
        /// freigegeben wurde (z.B. nach DisconnectHandler).
        /// </summary>
        public static void ClearLastKnownScanner()
        {
            _lastKnownScanner = null;
            System.Diagnostics.Debug.WriteLine("FlashlightManager: Last known scanner cleared");
        }

        /// <summary>
        /// Schaltet die Taschenlampe ein oder aus.
        /// Verwendet automatisch den Scanner, wenn dieser aktiv ist, sonst die native API.
        /// </summary>
        public static async Task<bool> ToggleFlashlightAsync(bool turnOn)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== FlashlightManager.ToggleFlashlightAsync START (turnOn={turnOn}) ===");
                System.Diagnostics.Debug.WriteLine($"FlashlightManager: _activeScanner = {(_activeScanner != null ? "NOT NULL" : "NULL")}");

                // Prüfe, ob ein Scanner aktiv ist
                if (_activeScanner != null)
                {
                    System.Diagnostics.Debug.WriteLine($"FlashlightManager: Using scanner to {(turnOn ? "turn ON" : "turn OFF")} flashlight");
                    _activeScanner.IsTorchOn = turnOn;
                    _isFlashlightOn = turnOn;
                    System.Diagnostics.Debug.WriteLine($"FlashlightManager: Scanner torch set to {turnOn}, returning TRUE");
                    return true;
                }

#if ANDROID
                // Kein Scanner aktiv - verwende native Android API
                System.Diagnostics.Debug.WriteLine($"FlashlightManager: No scanner active, using native Android API to {(turnOn ? "turn ON" : "turn OFF")} flashlight");

                // Der native Aufruf kann bei belegter Kamera kurz blockieren (Retry mit Wartezeit).
                // Deshalb auf einem Hintergrund-Thread ausführen, um den UI-Thread nicht zu blockieren (ANR-Schutz).
                bool success = await Task.Run(() => turnOn
                    ? iPMCloud.Mobile.Platforms.Android.Services.AndroidFlashlightService.TurnOn()
                    : iPMCloud.Mobile.Platforms.Android.Services.AndroidFlashlightService.TurnOff());

                System.Diagnostics.Debug.WriteLine($"FlashlightManager: Native Android API returned success={success}");

                if (success)
                {
                    _isFlashlightOn = turnOn;
                    System.Diagnostics.Debug.WriteLine($"FlashlightManager: Native API succeeded, returning TRUE");
                    return true;
                }

                // Native API fehlgeschlagen (z.B. Kamera durch Scanner belegt / CAMERA_IN_USE).
                // Wenn die Kamera-Session eines (früheren) Scanners noch aktiv ist, kann die
                // Taschenlampe nur über diesen Kamera-Besitzer geschaltet werden.
                if (_lastKnownScanner != null)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("FlashlightManager: Native API failed, trying last known scanner (camera owner)");
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            _lastKnownScanner.IsTorchOn = turnOn;
                        });
                        _isFlashlightOn = turnOn;
                        System.Diagnostics.Debug.WriteLine($"FlashlightManager: Torch set via last known scanner to {turnOn}, returning TRUE");
                        return true;
                    }
                    catch (Exception scannerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"FlashlightManager: Last known scanner torch failed: {scannerEx.Message}");
                        // Referenz ist offenbar ungültig -> auflösen
                        _lastKnownScanner = null;
                    }
                }

                // Der MAUI-Fallback nutzt auf Android denselben CameraManager und kann die
                // Taschenlampe bei belegter Kamera ebenfalls nicht schalten, meldet aber
                // faelschlich Erfolg. Daher wird das ehrliche Ergebnis zurueckgegeben.
                System.Diagnostics.Debug.WriteLine("FlashlightManager: Native API failed (camera likely in use), returning FALSE");
                _isFlashlightOn = false;
                return false;
#elif IOS
                // iOS verwendet MAUI Flashlight API
                System.Diagnostics.Debug.WriteLine($"FlashlightManager: iOS - using MAUI API");
                if (turnOn)
                    await Microsoft.Maui.Devices.Flashlight.Default.TurnOnAsync();
                else
                    await Microsoft.Maui.Devices.Flashlight.Default.TurnOffAsync();

                _isFlashlightOn = turnOn;
                return true;
#else
                // Andere Plattformen
                System.Diagnostics.Debug.WriteLine($"FlashlightManager: Other platform - using MAUI API");
                if (turnOn)
                    await Microsoft.Maui.Devices.Flashlight.Default.TurnOnAsync();
                else
                    await Microsoft.Maui.Devices.Flashlight.Default.TurnOffAsync();

                _isFlashlightOn = turnOn;
                return true;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FlashlightManager: EXCEPTION - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"FlashlightManager: StackTrace - {ex.StackTrace}");
                _isFlashlightOn = false;
                return false;
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"=== FlashlightManager.ToggleFlashlightAsync END ===");
            }
        }

        /// <summary>
        /// Gibt zurück, ob die Taschenlampe eingeschaltet ist
        /// </summary>
        public static bool IsFlashlightOn => _isFlashlightOn;

        /// <summary>
        /// Gibt zurück, ob ein Scanner aktiv ist
        /// </summary>
        public static bool IsScannerActive => _activeScanner != null;
    }
}
