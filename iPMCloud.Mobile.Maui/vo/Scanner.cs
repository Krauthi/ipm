using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BarcodeScanning;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace iPMCloud.Mobile.vo
{
    public class Scanner
    {
        public Scanner()
        {
        }

        // Gate to serialize scan starts (prevents overlapping Connect/Start races)
        private readonly SemaphoreSlim _scanGate = new(1, 1);

        // Thread-safe guard so StopAsync() is idempotent and never runs concurrently with itself
        private int _isStopping = 0;

        // Stored handler so we can unsubscribe (no inline-lambda-only subscriptions)
        private EventHandler<OnDetectionFinishedEventArg> _detectionHandler;

        // Delay to let in-flight native callbacks to drain before removing the view
        // from the visual tree / reconnecting.
        private const int CameraDrainDelayMs = 300;

        public bool displayIsOpen = false;

        public CameraView cameraView;

        // Eigenes Overlay erstellen
        public ContentView overlayz;

        // NOTE: This grid is shared on the singleton Scanner instance; we therefore must be
        // very careful to tear down before reconnecting.
        public Grid grid = new Grid
        {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
        };

        public Image img = new Image
        {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
        };

        private static void LogException(string context, Exception ex)
        {
            try
            {
                // Ensure stacktrace is included even if logger formatting changes.
                AppModel.Logger.Error(ex, $"ERROR: {context} | {ex.Message} | {ex.StackTrace}");
            }
            catch
            {
                // never throw from logging
            }
        }

        // Hilfsmethode zum Erstellen eines Custom Overlays
        private ContentView CreateOverlay(string topText, string bottomText, bool showFlashButton, Action onFlashButtonClicked)
        {
            var overlayGrid = new Grid
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent
            };

            overlayGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            overlayGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Top Text
            var topLabel = new Label
            {
                Text = topText,
                TextColor = Colors.White,
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 0, 30),
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.7f, Radius = 7, Offset = new Point(3, 3) },
            };
            Grid.SetRow(topLabel, 0);
            overlayGrid.Children.Add(topLabel);

            // Scanner Frame (Mitte)
            var scanFrame = new Border
            {
                Stroke = Colors.White,
                BackgroundColor = Colors.Transparent,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                WidthRequest = 250,
                HeightRequest = 250,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = 0,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
            };
            Grid.SetRow(scanFrame, 1);
            overlayGrid.Children.Add(scanFrame);

            // Bottom Stack
            var bottomStack = new StackLayout
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 10,
                Margin = new Thickness(0, 30, 0, 0)
            };

            var bottomLabel = new Label
            {
                Text = bottomText,
                TextColor = Colors.White,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.7f, Radius = 7, Offset = new Point(3, 3) },
            };
            bottomStack.Children.Add(bottomLabel);

            // Flash Button
            if (showFlashButton)
            {
                var flashButton = new Button
                {
                    ImageSource = "Flashlight.png",
                    Padding = 5,
                    BackgroundColor = Color.FromRgb(20, 77, 147),
                    CornerRadius = 0,
                    WidthRequest = 70,
                    HeightRequest = 70,
                    Margin = new Thickness(0, 20, 0, 0),
                    HorizontalOptions = LayoutOptions.Center
                };
                flashButton.Clicked += (s, e) =>
                {
                    try { onFlashButtonClicked?.Invoke(); }
                    catch (Exception ex) { LogException("CreateOverlay.flashButton.Clicked", ex); }
                };
                bottomStack.Children.Add(flashButton);
            }

            Grid.SetRow(bottomStack, 2);
            overlayGrid.Children.Add(bottomStack);

            return new ContentView { Content = overlayGrid };
        }

        /// <summary>
        /// Asynchronously stops the scanner in a thread-safe, idempotent way.
        /// - Unsubscribes the barcode handler
        /// - Disables camera and torch
        /// - Waits briefly for in-flight native callbacks to drain
        /// - Clears the grid
        /// All UI work is guaranteed to run on the MainThread.
        /// </summary>
        public Task StopAsync()
        {
            if (Interlocked.CompareExchange(ref _isStopping, 1, 0) != 0)
                return Task.CompletedTask;

            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    // Block re-entry while tearing down (synchronized with other UI callbacks)
                    displayIsOpen = true;

                    try
                    {
                        if (cameraView != null)
                        {
                            if (_detectionHandler != null)
                            {
                                try { cameraView.OnDetectionFinished -= _detectionHandler; }
                                catch (Exception ex) { LogException("StopAsync.unsubscribe", ex); }
                                _detectionHandler = null;
                            }

                            try { cameraView.CameraEnabled = false; }
                            catch (Exception ex) { LogException("StopAsync.CameraEnabled=false", ex); }

                            try { cameraView.TorchOn = false; }
                            catch (Exception ex) { LogException("StopAsync.TorchOn=false", ex); }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException("StopAsync.inner", ex);
                    }

                    await Task.Delay(CameraDrainDelayMs);

                    try { grid?.Children.Clear(); }
                    catch (Exception ex) { LogException("StopAsync.grid.Children.Clear", ex); }

                    cameraView = null;
                }
                finally
                {
                    displayIsOpen = false;
                    Interlocked.Exchange(ref _isStopping, 0);
                }
            });
        }

        /// <summary>
        /// Fire-and-forget wrapper around StopAsync(). Safe to call from any thread.
        /// </summary>
        public void Stop()
        {
            StopAsync().ContinueWith(
                t => { _ = t.Exception; },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public async void ScanBuildingOutView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
            await _scanGate.WaitAsync();
            try
            {
                await StopAsync();

                cameraView = new CameraView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    AutomationId = "barcodeScannerView",
                    Margin = new Thickness(0, 0, 0, 0),
                    BarcodeSymbologies = BarcodeFormats.All
                };

                _detectionHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if (!displayIsOpen && e.BarcodeResults?.Count > 0)
                            {
                                displayIsOpen = true;
                                var result = e.BarcodeResults.First();

                                try
                                {
                                    var sp = result.DisplayValue
                                        .Replace("http://www.ipm-cloud.de/?objektid=", "")
                                        .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

                                    if (sp != null && sp.Length > 0)
                                    {
                                        AppModel.Instance.OutScanBuilding = null;

                                        var CustomerNumber = sp.Length == 1 ? "1" : "" + sp[1];
                                        Int32 buildingid = Int32.Parse(sp[0]);

                                        if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                        {
                                            if (AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
                                            {
                                                AppModel.Instance.OutScanBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);
                                                try
                                                {
                                                    AppModel.Logger.Info("CHECK-OUT: " + AppModel.Instance.OutScanBuilding.strasse + " " +
                                                                         AppModel.Instance.OutScanBuilding.hsnr + " " +
                                                                         AppModel.Instance.OutScanBuilding.plz + " " +
                                                                         AppModel.Instance.OutScanBuilding.ort);
                                                }
                                                catch (Exception exLog)
                                                {
                                                    LogException("ScanBuildingOutView.CHECK-OUT.Log", exLog);
                                                }
                                            }

                                            try { await StopAsync(); }
                                            catch (Exception exStop) { LogException("ScanBuildingOutView.StopAsync(success)", exStop); }

                                            AppModel.Instance.UseExternHardware = false;

                                            try { func?.Invoke(); }
                                            catch (Exception exCb) { LogException("ScanBuildingOutView.func()", exCb); }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                                    "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
                                                    "OK");
                                            }
                                            catch (Exception exAlert) { LogException("ScanBuildingOutView.DisplayAlertAsync(wrong customer)", exAlert); }

                                            displayIsOpen = false;
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                                "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
                                                "OK");
                                        }
                                        catch (Exception exAlert) { LogException("ScanBuildingOutView.DisplayAlertAsync(invalid sp)", exAlert); }

                                        displayIsOpen = false;
                                    }
                                }
                                catch (Exception exInner)
                                {
                                    LogException("ScanBuildingOutView.OnDetectionFinished(inner)", exInner);
                                    try
                                    {
                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                            "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
                                            "OK");
                                    }
                                    catch (Exception exAlert) { LogException("ScanBuildingOutView.DisplayAlertAsync(inner catch)", exAlert); }

                                    displayIsOpen = false;
                                }
                            }
                        }
                        catch (Exception exOuter)
                        {
                            LogException("ScanBuildingOutView.OnDetectionFinished(outer)", exOuter);
                            try { displayIsOpen = false; } catch { }
                            try { await StopAsync(); } catch { }
                        }
                    });
                };
                cameraView.OnDetectionFinished += _detectionHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () =>
                    {
                        try { cameraView.TorchOn = !cameraView.TorchOn; }
                        catch (Exception ex) { LogException("ScanBuildingOutView.ToggleTorch", ex); }
                    }
                );

                grid.Children.Clear();
                grid.Children.Add(cameraView);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);

                cameraView.CameraEnabled = true;
            }
            catch (Exception ex)
            {
                LogException("ScanBuildingOutView", ex);
                try { await page.DisplayAlertAsync("Faild scan QRCode", ex.Message, "OK"); } catch { }
            }
            finally
            {
                _scanGate.Release();
            }
        }

        public async void ScanBuildingView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
            await _scanGate.WaitAsync();
            try
            {
                await StopAsync();

                cameraView = new CameraView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 0, 0, 0),
                    AutomationId = "barcodeScannerView",
                    BarcodeSymbologies = BarcodeFormats.All
                };

                _detectionHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if (!displayIsOpen && e.BarcodeResults?.Count > 0)
                            {
                                displayIsOpen = true;
                                var result = e.BarcodeResults.First();

                                try
                                {
                                    var sp = result.DisplayValue
                                        .Replace("http://www.ipm-cloud.de/?objektid=", "")
                                        .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

                                    if (sp != null && sp.Length > 0)
                                    {
                                        var CustomerNumber = sp.Length == 1 ? "1" : "" + sp[1];
                                        Int32 buildingid = Int32.Parse(sp[0]);

                                        if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                        {
                                            AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;

                                            if (buildingid > 0 && AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
                                            {
                                                AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
                                                AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;
                                                AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);
                                                try
                                                {
                                                    AppModel.Logger.Info("CHECK-IN: " + AppModel.Instance.LastBuilding.strasse + " " +
                                                                         AppModel.Instance.LastBuilding.hsnr + " " +
                                                                         AppModel.Instance.LastBuilding.plz + " " +
                                                                         AppModel.Instance.LastBuilding.ort);
                                                }
                                                catch (Exception exLog)
                                                {
                                                    LogException("ScanBuildingView.CHECK-IN.Log", exLog);
                                                }
                                            }

                                            AppModel.Instance.SettingModel.SaveSettings();

                                            try { await StopAsync(); }
                                            catch (Exception exStop) { LogException("ScanBuildingView.StopAsync(success)", exStop); }

                                            AppModel.Instance.UseExternHardware = false;

                                            try { func?.Invoke(); }
                                            catch (Exception exCb) { LogException("ScanBuildingView.func()", exCb); }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                                    "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
                                                    "OK");
                                            }
                                            catch (Exception exAlert) { LogException("ScanBuildingView.DisplayAlertAsync(wrong customer)", exAlert); }

                                            displayIsOpen = false;
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                                "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
                                                "OK");
                                        }
                                        catch (Exception exAlert) { LogException("ScanBuildingView.DisplayAlertAsync(invalid sp)", exAlert); }

                                        displayIsOpen = false;
                                    }
                                }
                                catch (Exception exInner)
                                {
                                    LogException("ScanBuildingView.OnDetectionFinished(inner)", exInner);
                                    try
                                    {
                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                            "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
                                            "OK");
                                    }
                                    catch (Exception exAlert) { LogException("ScanBuildingView.DisplayAlertAsync(inner catch)", exAlert); }

                                    displayIsOpen = false;
                                }
                            }
                        }
                        catch (Exception exOuter)
                        {
                            LogException("ScanBuildingView.OnDetectionFinished(outer)", exOuter);
                            try { displayIsOpen = false; } catch { }
                            try { await StopAsync(); } catch { }
                        }
                    });
                };
                cameraView.OnDetectionFinished += _detectionHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () =>
                    {
                        try { cameraView.TorchOn = !cameraView.TorchOn; }
                        catch (Exception ex) { LogException("ScanBuildingView.ToggleTorch", ex); }
                    }
                );

                grid.Children.Clear();
                grid.Children.Add(cameraView);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);

                cameraView.CameraEnabled = true;
            }
            catch (Exception ex)
            {
                LogException("ScanBuildingView", ex);
                try { await page.DisplayAlertAsync("Faild scan building QRCode", ex.Message, "OK"); } catch { }
            }
            finally
            {
                _scanGate.Release();
            }
        }

        public async void ScanRegView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
            await _scanGate.WaitAsync();
            try
            {
                await StopAsync();

                cameraView = new CameraView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    AutomationId = "barcodeScannerView",
                    Margin = new Thickness(0, 0, 0, 0),
                    BarcodeSymbologies = BarcodeFormats.All,
                    ForceInverted = true
                };

                _detectionHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if (!displayIsOpen && e.BarcodeResults?.Count > 0)
                            {
                                displayIsOpen = true;
                                var result = e.BarcodeResults.First();

                                try
                                {
                                    var sp = result.DisplayValue
                                        .Replace("https://", "http://")
                                        .Replace("httpss://", "https://")
                                        .Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                                    if (sp.Length < 3)
                                        throw new Exception("QR-Format ungültig.");

                                    var newScanSettings = new SettingDTO
                                    {
                                        ServerUrl = sp[0],
                                        CustomerNumber = sp[1],
                                        CustomerName = sp[2]
                                    };

                                    if (result.DisplayValue.IndexOf("###", StringComparison.Ordinal) > -1 &&
                                        !string.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                        !string.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                        !string.IsNullOrWhiteSpace(newScanSettings.CustomerName))
                                    {
                                        string directoryPath = Path.Combine(
                                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            "ipm/" + newScanSettings.CustomerNumber);

                                        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                                        AppModel.Instance.SettingModel.SettingDTO = newScanSettings;
                                        AppModel.Instance.SettingModel.SaveSettings();
                                        Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

                                        try { await StopAsync(); }
                                        catch (Exception exStop) { LogException("ScanRegView.StopAsync(success)", exStop); }

                                        AppModel.Instance.UseExternHardware = false;

                                        try { func?.Invoke(); }
                                        catch (Exception exCb) { LogException("ScanRegView.func()", exCb); }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                                "Dieser QR-Code kann für die Registrierung mit der iPM-Cloud nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
                                                "OK");
                                        }
                                        catch (Exception exAlert) { LogException("ScanRegView.DisplayAlertAsync(invalid)", exAlert); }

                                        displayIsOpen = false;
                                    }
                                }
                                catch (Exception exInner)
                                {
                                    LogException("ScanRegView.OnDetectionFinished(inner)", exInner);
                                    try
                                    {
                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                            "Dieser QR-Code kann für die Registrierung mit der iPM-Cloud nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
                                            "OK");
                                    }
                                    catch (Exception exAlert) { LogException("ScanRegView.DisplayAlertAsync(inner catch)", exAlert); }

                                    displayIsOpen = false;
                                }
                            }
                        }
                        catch (Exception exOuter)
                        {
                            LogException("ScanRegView.OnDetectionFinished(outer)", exOuter);
                            try { displayIsOpen = false; } catch { }
                            try { await StopAsync(); } catch { }
                        }
                    });
                };
                cameraView.OnDetectionFinished += _detectionHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () =>
                    {
                        try { cameraView.TorchOn = !cameraView.TorchOn; }
                        catch (Exception ex) { LogException("ScanRegView.ToggleTorch", ex); }
                    }
                );

                grid.Children.Clear();
                grid.Children.Add(cameraView);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);

                cameraView.CameraEnabled = true;
            }
            catch (Exception ex)
            {
                LogException("ScanRegView", ex);
                try { await page.DisplayAlertAsync("Faild scan reg QRCode", ex.Message, "OK"); } catch { }
            }
            finally
            {
                _scanGate.Release();
            }
        }

        public async void ScanAddRegView(ContentPage page, StackLayout scanContainer, Func<bool> func, Func<bool> funcfaild)
        {
            await _scanGate.WaitAsync();
            try
            {
                await StopAsync();

                cameraView = new CameraView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 0, 0, 0),
                    AutomationId = "barcodeScannerView",
                    BarcodeSymbologies = BarcodeFormats.All
                };

                _detectionHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if (!displayIsOpen && e.BarcodeResults?.Count > 0)
                            {
                                displayIsOpen = true;
                                var result = e.BarcodeResults.First();

                                try
                                {
                                    var sp = result.DisplayValue
                                        .Replace("https://", "http://")
                                        .Replace("httpss://", "https://")
                                        .Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                                    if (sp.Length < 3)
                                        throw new Exception("QR-Format ungültig.");

                                    var newScanSettings = new SettingDTO
                                    {
                                        ServerUrl = sp[0],
                                        CustomerNumber = sp[1],
                                        CustomerName = sp[2]
                                    };

                                    if (result.DisplayValue.IndexOf("###", StringComparison.Ordinal) > -1 &&
                                        !string.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                        !string.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                        !string.IsNullOrWhiteSpace(newScanSettings.CustomerName) &&
                                        newScanSettings.CustomerNumber != AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                    {
                                        Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

                                        string directoryPath = Path.Combine(
                                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            "ipm/" + newScanSettings.CustomerNumber);

                                        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                                        AppModel.Instance.SettingModel.SettingDTO = newScanSettings;
                                        AppModel.Instance.SettingModel.SaveSettings();

                                        try { await StopAsync(); }
                                        catch (Exception exStop) { LogException("ScanAddRegView.StopAsync(success)", exStop); }

                                        AppModel.Instance.UseExternHardware = false;

                                        try { func?.Invoke(); }
                                        catch (Exception exCb) { LogException("ScanAddRegView.func()", exCb); }
                                    }
                                    else
                                    {
                                        // invalid / already registered -> stop scan to avoid races
                                        try { await StopAsync(); }
                                        catch (Exception exStop) { LogException("ScanAddRegView.StopAsync(invalid)", exStop); }

                                        // optional: funcfaild?.Invoke();
                                    }
                                }
                                catch (Exception exInner)
                                {
                                    LogException("ScanAddRegView.OnDetectionFinished(inner)", exInner);

                                    try
                                    {
                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!",
                                            "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.",
                                            "OK");
                                    }
                                    catch (Exception exAlert) { LogException("ScanAddRegView.DisplayAlertAsync(inner catch)", exAlert); }

                                    try { await StopAsync(); }
                                    catch (Exception exStop) { LogException("ScanAddRegView.StopAsync(after inner catch)", exStop); }

                                    try { funcfaild?.Invoke(); }
                                    catch (Exception exCb) { LogException("ScanAddRegView.funcfaild()", exCb); }
                                }
                            }
                        }
                        catch (Exception exOuter)
                        {
                            LogException("ScanAddRegView.OnDetectionFinished(outer)", exOuter);
                            try { displayIsOpen = false; } catch { }
                            try { await StopAsync(); } catch { }
                        }
                    });
                };
                cameraView.OnDetectionFinished += _detectionHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () =>
                    {
                        try { cameraView.TorchOn = !cameraView.TorchOn; }
                        catch (Exception ex) { LogException("ScanAddRegView.ToggleTorch", ex); }
                    }
                );

                grid.Children.Clear();
                grid.Children.Add(cameraView);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);

                cameraView.CameraEnabled = true;
            }
            catch (Exception ex)
            {
                LogException("ScanAddRegView", ex);
                try { await page.DisplayAlertAsync("Faild scan addreg QRCode", ex.Message, "OK"); } catch { }
            }
            finally
            {
                _scanGate.Release();
            }
        }

        public void Btn_FlashlightTapped(object sender, EventArgs e)
        {
            try
            {
                if (cameraView != null)
                    cameraView.TorchOn = !cameraView.TorchOn;
            }
            catch (Exception ex)
            {
                LogException("Btn_FlashlightTapped", ex);
            }
        }

        public async void Btn_FlashlightAloneTapped(object sender, EventArgs e)
        {
            try
            {
                if (AppModel.Instance.isFlashLigthAloneON)
                {
                    AppModel.Instance.isFlashLigthAloneON = false;
                    await Flashlight.Default.TurnOffAsync();
                }
                else
                {
                    AppModel.Instance.isFlashLigthAloneON = true;
                    await Flashlight.Default.TurnOnAsync();
                }
            }
            catch (Exception ex)
            {
                LogException("Btn_FlashlightAloneTapped", ex);
            }
        }

        public void FlashON_Handle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                if (cameraView != null)
                    cameraView.TorchOn = !cameraView.TorchOn;

            }
            catch (Exception ex)
            {
                LogException("Btn_FlashlightAloneTapped", ex);
            }
        }


    }
}