//using System;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Maui.ApplicationModel;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Devices;
//using Microsoft.Maui.Storage;
//using ZXing.Net.Maui;
//using ZXing.Net.Maui.Controls;

//namespace iPMCloud.Mobile.vo
//{
//    public class Scanner
//    {
//        public Scanner()
//        {
//        }

//        // Thread-safe guard so StopAsync() is idempotent and never runs concurrently with itself
//        private int _isStopping = 0;

//        // Stored handler so we can unsubscribe (no inline-lambda-only subscriptions)
//        private EventHandler<BarcodeDetectionEventArgs> _barcodeHandler;

//        // Delay to let in-flight Android CameraManager Runnables finish before removing the view
//        // from the visual tree / reconnecting.
//        // Increased to 1200ms to ensure Android native camera resources are fully released
//        // This prevents NullReferenceException in CameraManager.Connect when rapidly switching views
//        private const int CameraDrainDelayMs = 1200;

//        public bool displayIsOpen = false;

//        public CameraBarcodeReaderView zxing;

//        // Eigenes Overlay erstellen (ZXingDefaultOverlay existiert nicht mehr)
//        public ContentView overlayz;

//        // NOTE: This grid is shared on the singleton Scanner instance; we therefore must be
//        // very careful to tear down before reconnecting.
//        public Grid grid = new Grid
//        {
//            VerticalOptions = LayoutOptions.Fill,
//            HorizontalOptions = LayoutOptions.Fill,
//        };

//        public Image img = new Image
//        {
//            VerticalOptions = LayoutOptions.Fill,
//            HorizontalOptions = LayoutOptions.Fill,
//        };

//        private static void LogException(string context, Exception ex)
//        {
//            try
//            {
//                // Ensure stacktrace is included even if logger formatting changes.
//                AppModel.Logger.Error(ex, $"ERROR: {context} | {ex.Message} | {ex.StackTrace}");
//            }
//            catch
//            {
//                // never throw from logging
//            }
//        }

//        // Hilfsmethode zum Erstellen eines Custom Overlays
//        private ContentView CreateOverlay(string topText, string bottomText, bool showFlashButton, Action onFlashButtonClicked)
//        {
//            var overlayGrid = new Grid
//            {
//                VerticalOptions = LayoutOptions.Fill,
//                HorizontalOptions = LayoutOptions.Fill,
//                BackgroundColor = Colors.Transparent
//            };

//            overlayGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
//            overlayGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
//            overlayGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

//            // Top Text
//            var topLabel = new Label
//            {
//                Text = topText,
//                TextColor = Colors.White,
//                FontSize = 16,
//                HorizontalOptions = LayoutOptions.Center,
//                VerticalOptions = LayoutOptions.End,
//                Margin = new Thickness(0, 0, 0, 30),
//                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.7f, Radius = 7, Offset = new Point(3, 3) },
//            };
//            Grid.SetRow(topLabel, 0);
//            overlayGrid.Children.Add(topLabel);

//            // Scanner Frame (Mitte)
//            var scanFrame = new Border
//            {
//                Stroke = Colors.White,
//                BackgroundColor = Colors.Transparent,
//                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
//                WidthRequest = 250,
//                HeightRequest = 250,
//                HorizontalOptions = LayoutOptions.Center,
//                VerticalOptions = LayoutOptions.Center,
//                Padding = 0,
//                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.3f, Radius = 5, Offset = new Point(2, 2) },
//            };
//            Grid.SetRow(scanFrame, 1);
//            overlayGrid.Children.Add(scanFrame);

//            // Bottom Stack
//            var bottomStack = new StackLayout
//            {
//                VerticalOptions = LayoutOptions.Start,
//                HorizontalOptions = LayoutOptions.Center,
//                Spacing = 10,
//                Margin = new Thickness(0, 30, 0, 0)
//            };

//            var bottomLabel = new Label
//            {
//                Text = bottomText,
//                TextColor = Colors.White,
//                FontSize = 18,
//                HorizontalOptions = LayoutOptions.Center,
//                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.7f, Radius = 7, Offset = new Point(3, 3) },
//            };
//            bottomStack.Children.Add(bottomLabel);

//            // Flash Button
//            if (showFlashButton)
//            {
//                var flashButton = new Button
//                {
//                    ImageSource = "Flashlight.png",
//                    Padding = 5,
//                    BackgroundColor = Color.FromRgb(20, 77, 147),
//                    CornerRadius = 0,
//                    WidthRequest = 70,
//                    HeightRequest = 70,
//                    Margin = new Thickness(0, 20, 0, 0),
//                    HorizontalOptions = LayoutOptions.Center
//                };
//                flashButton.Clicked += (s, e) =>
//                {
//                    try { onFlashButtonClicked?.Invoke(); }
//                    catch (Exception ex) { LogException("CreateOverlay.flashButton.Clicked", ex); }
//                };
//                bottomStack.Children.Add(flashButton);
//            }

//            Grid.SetRow(bottomStack, 2);
//            overlayGrid.Children.Add(bottomStack);

//            return new ContentView { Content = overlayGrid };
//        }

//        /// <summary>
//        /// Asynchronously stops the scanner in a thread-safe, idempotent way.
//        /// - Unsubscribes the barcode handler
//        /// - Disables detection / torch
//        /// - Hard-disconnects the MAUI handler (important to avoid Android Connect races)
//        /// - Waits briefly for in-flight native callbacks to drain
//        /// - Clears the grid
//        /// All UI work is guaranteed to run on the MainThread.
//        /// </summary>
//        public Task StopAsync()
//        {
//            if (Interlocked.CompareExchange(ref _isStopping, 1, 0) != 0)
//                return Task.CompletedTask;

//            return MainThread.InvokeOnMainThreadAsync(async () =>
//            {
//                try
//                {
//                    // Block re-entry while tearing down (synchronized with other UI callbacks)
//                    displayIsOpen = true;

//                    try
//                    {
//                        if (zxing != null)
//                        {
//                            if (_barcodeHandler != null)
//                            {
//                                try { zxing.BarcodesDetected -= _barcodeHandler; }
//                                catch (Exception ex) { LogException("StopAsync.unsubscribe", ex); }
//                                _barcodeHandler = null;
//                            }

//                            try { zxing.IsDetecting = false; }
//                            catch (Exception ex) { LogException("StopAsync.IsDetecting=false", ex); }

//                            try { zxing.IsTorchOn = false; }
//                            catch (Exception ex) { LogException("StopAsync.IsTorchOn=false", ex); }

//                            // Critical for Connect-NRE: detach native view/handler
//                            try { zxing.Handler?.DisconnectHandler(); }
//                            catch (Exception ex) { LogException("StopAsync.DisconnectHandler", ex); }
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        LogException("StopAsync.inner", ex);
//                    }

//                    await Task.Delay(CameraDrainDelayMs);

//                    try { grid?.Children.Clear(); }
//                    catch (Exception ex) { LogException("StopAsync.grid.Children.Clear", ex); }

//                    zxing = null;
//                }
//                finally
//                {
//                    displayIsOpen = false;
//                    Interlocked.Exchange(ref _isStopping, 0);
//                }
//            });
//        }

//        /// <summary>
//        /// Fire-and-forget wrapper around StopAsync(). Safe to call from any thread.
//        /// </summary>
//        public void Stop()
//        {
//            StopAsync().ContinueWith(
//                t => { _ = t.Exception; },
//                TaskContinuationOptions.OnlyOnFaulted);
//        }

//        public async void ScanBuildingOutView(ContentPage page, StackLayout scanContainer, Func<bool> func)
//        {
//            try
//            {
//                await StopAsync();

//                var opts = new BarcodeReaderOptions
//                {
//                    Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
//                    AutoRotate = true,
//                    Multiple = false
//                };

//                zxing = new CameraBarcodeReaderView
//                {
//                    HorizontalOptions = LayoutOptions.Fill,
//                    VerticalOptions = LayoutOptions.Fill,
//                    AutomationId = "zxingScannerView",
//                    Margin = new Thickness(0, 0, 0, 0),
//                    Options = opts
//                };

//                _barcodeHandler = (sender, e) =>
//                {
//                    MainThread.BeginInvokeOnMainThread(async () =>
//                    {
//                        try
//                        {
//                            if (!displayIsOpen && e.Results?.Length > 0)
//                            {
//                                displayIsOpen = true;
//                                var result = e.Results[0];

//                                try
//                                {
//                                    var sp = result.Value
//                                        .Replace("http://www.ipm-cloud.de/?objektid=", "")
//                                        .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

//                                    if (sp != null && sp.Length > 0)
//                                    {
//                                        AppModel.Instance.OutScanBuilding = null;

//                                        var CustomerNumber = sp.Length == 1 ? "1" : "" + sp[1];
//                                        Int32 buildingid = Int32.Parse(sp[0]);

//                                        if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
//                                        {
//                                            if (AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
//                                            {
//                                                AppModel.Instance.OutScanBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);
//                                                try
//                                                {
//                                                    AppModel.Logger.Info("CHECK-OUT: " + AppModel.Instance.OutScanBuilding.strasse + " " +
//                                                                         AppModel.Instance.OutScanBuilding.hsnr + " " +
//                                                                         AppModel.Instance.OutScanBuilding.plz + " " +
//                                                                         AppModel.Instance.OutScanBuilding.ort);
//                                                }
//                                                catch (Exception exLog)
//                                                {
//                                                    LogException("ScanBuildingOutView.CHECK-OUT.Log", exLog);
//                                                }
//                                            }

//                                            try { await StopAsync(); }
//                                            catch (Exception exStop) { LogException("ScanBuildingOutView.StopAsync(success)", exStop); }

//                                            AppModel.Instance.UseExternHardware = false;

//                                            try { func?.Invoke(); }
//                                            catch (Exception exCb) { LogException("ScanBuildingOutView.func()", exCb); }
//                                        }
//                                        else
//                                        {
//                                            try
//                                            {
//                                                await page.DisplayAlertAsync("QR-Code nicht erkannt!",
//                                                    "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
//                                                    "OK");
//                                            }
//                                            catch (Exception exAlert) { LogException("ScanBuildingOutView.DisplayAlertAsync(wrong customer)", exAlert); }

//                                            displayIsOpen = false;
//                                        }
//                                    }
//                                    else
//                                    {
//                                        try
//                                        {
//                                            await page.DisplayAlertAsync("QR-Code nicht erkannt!",
//                                                "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
//                                                "OK");
//                                        }
//                                        catch (Exception exAlert) { LogException("ScanBuildingOutView.DisplayAlertAsync(invalid sp)", exAlert); }

//                                        displayIsOpen = false;
//                                    }
//                                }
//                                catch (Exception exInner)
//                                {
//                                    LogException("ScanBuildingOutView.BarcodesDetected(inner)", exInner);
//                                    try
//                                    {
//                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!",
//                                            "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
//                                            "OK");
//                                    }
//                                    catch (Exception exAlert) { LogException("ScanBuildingOutView.DisplayAlertAsync(inner catch)", exAlert); }

//                                    displayIsOpen = false;
//                                }
//                            }
//                        }
//                        catch (Exception exOuter)
//                        {
//                            LogException("ScanBuildingOutView.BarcodesDetected(outer)", exOuter);
//                            try { displayIsOpen = false; } catch { }
//                            try { await StopAsync(); } catch { }
//                        }
//                    });
//                };
//                zxing.BarcodesDetected += _barcodeHandler;

//                overlayz = CreateOverlay(
//                    "Richten Sie die Kamera auf den QR-Code",
//                    "Das Scannen erfolgt automatisch",
//                    true,
//                    () =>
//                    {
//                        try { zxing.IsTorchOn = !zxing.IsTorchOn; }
//                        catch (Exception ex) { LogException("ScanBuildingOutView.ToggleTorch", ex); }
//                    }
//                );

//                grid.Children.Clear();
//                grid.Children.Add(zxing);
//                grid.Children.Add(overlayz);

//                // Ensure grid is not already in the container to avoid duplicate parent issues
//                if (grid.Parent != scanContainer)
//                {
//                    scanContainer.Children.Add(grid);
//                }

//                // Extended delay before starting detection to ensure native camera resources are fully released
//                // This prevents NullReferenceException in ZXing.Net.Maui CameraManager.Connect on Android
//                await Task.Delay(300);

//                if (zxing != null)
//                {
//                    zxing.IsDetecting = true;
//                }
//                else
//                {
//                    throw new InvalidOperationException("Camera view was disposed during initialization");
//                }
//            }
//            catch (Exception ex)
//            {
//                LogException("ScanBuildingOutView", ex);
//                try { await page.DisplayAlertAsync("Faild scan QRCode", ex.Message, "OK"); } catch { }
//            }
//            finally
//            {
//            }
//        }

//        public async void ScanBuildingView(ContentPage page, StackLayout scanContainer, Func<bool> func)
//        {
//            try
//            {
//                await StopAsync();

//                // Extended delay after stop to ensure Android native camera resources are fully released
//                // Critical for preventing NullReferenceException in CameraManager.Connect
//                await Task.Delay(300);

//                var opts = new BarcodeReaderOptions
//                {
//                    Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
//                    AutoRotate = true,
//                    Multiple = false
//                };

//                zxing = new CameraBarcodeReaderView
//                {
//                    HorizontalOptions = LayoutOptions.Fill,
//                    VerticalOptions = LayoutOptions.Fill,
//                    Margin = new Thickness(0, 0, 0, 0),
//                    AutomationId = "zxingScannerView",
//                    Options = opts
//                };

//                _barcodeHandler = (sender, e) =>
//                {
//                    MainThread.BeginInvokeOnMainThread(async () =>
//                    {
//                        try
//                        {
//                            if (!displayIsOpen && e.Results?.Length > 0)
//                            {
//                                displayIsOpen = true;
//                                var result = e.Results[0];

//                                try
//                                {
//                                    var sp = result.Value
//                                        .Replace("http://www.ipm-cloud.de/?objektid=", "")
//                                        .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

//                                    if (sp != null && sp.Length > 0)
//                                    {
//                                        var CustomerNumber = sp.Length == 1 ? "1" : "" + sp[1];
//                                        Int32 buildingid = Int32.Parse(sp[0]);

//                                        if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
//                                        {
//                                            AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;

//                                            if (buildingid > 0 && AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
//                                            {
//                                                AppModel.Instance.SetAllObjectAndValuesToNoSelectedBuilding();
//                                                AppModel.Instance.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;
//                                                AppModel.Instance.LastBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);
//                                                try
//                                                {
//                                                    AppModel.Logger.Info("CHECK-IN: " + AppModel.Instance.LastBuilding.strasse + " " +
//                                                                         AppModel.Instance.LastBuilding.hsnr + " " +
//                                                                         AppModel.Instance.LastBuilding.plz + " " +
//                                                                         AppModel.Instance.LastBuilding.ort);
//                                                }
//                                                catch (Exception exLog)
//                                                {
//                                                    LogException("ScanBuildingView.CHECK-IN.Log", exLog);
//                                                }
//                                            }

//                                            AppModel.Instance.SettingModel.SaveSettings();

//                                            try { await StopAsync(); }
//                                            catch (Exception exStop) { LogException("ScanBuildingView.StopAsync(success)", exStop); }

//                                            AppModel.Instance.UseExternHardware = false;

//                                            try { func?.Invoke(); }
//                                            catch (Exception exCb) { LogException("ScanBuildingView.func()", exCb); }
//                                        }
//                                        else
//                                        {
//                                            try
//                                            {
//                                                await page.DisplayAlertAsync("QR-Code nicht erkannt!",
//                                                    "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.",
//                                                    "OK");
//                                            }
//                                            catch (Exception exAlert) { LogException("ScanBuildingView.DisplayAlertAsync(wrong customer)", exAlert); }

//                                            displayIsOpen = false;
//                                        }
//                                    }
//                                    else
//                                    {
//                                        try
//                                        {
//                                            await page.DisplayAlertAsync("QR-Code nicht erkannt!",
//                                                "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
//                                                "OK");
//                                        }
//                                        catch (Exception exAlert) { LogException("ScanBuildingView.DisplayAlertAsync(invalid sp)", exAlert); }

//                                        displayIsOpen = false;
//                                    }
//                                }
//                                catch (Exception exInner)
//                                {
//                                    LogException("ScanBuildingView.BarcodesDetected(inner)", exInner);
//                                    try
//                                    {
//                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!",
//                                            "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.",
//                                            "OK");
//                                    }
//                                    catch (Exception exAlert) { LogException("ScanBuildingView.DisplayAlertAsync(inner catch)", exAlert); }

//                                    displayIsOpen = false;
//                                }
//                            }
//                        }
//                        catch (Exception exOuter)
//                        {
//                            LogException("ScanBuildingView.BarcodesDetected(outer)", exOuter);
//                            try { displayIsOpen = false; } catch { }
//                            try { await StopAsync(); } catch { }
//                        }
//                    });
//                };
//                zxing.BarcodesDetected += _barcodeHandler;

//                overlayz = CreateOverlay(
//                    "Richten Sie die Kamera auf den QR-Code",
//                    "Das Scannen erfolgt automatisch",
//                    true,
//                    () =>
//                    {
//                        try { zxing.IsTorchOn = !zxing.IsTorchOn; }
//                        catch (Exception ex) { LogException("ScanBuildingView.ToggleTorch", ex); }
//                    }
//                );

//                grid.Children.Clear();
//                grid.Children.Add(zxing);
//                grid.Children.Add(overlayz);

//                // Ensure grid is not already in the container to avoid duplicate parent issues
//                if (grid.Parent != scanContainer)
//                {
//                    scanContainer.Children.Add(grid);
//                }

//                // Extended delay before starting detection to ensure native camera resources are fully released
//                // This prevents NullReferenceException in ZXing.Net.Maui CameraManager.Connect on Android
//                await Task.Delay(300);

//                if (zxing != null)
//                {
//                    zxing.IsDetecting = true;
//                }
//                else
//                {
//                    throw new InvalidOperationException("Camera view was disposed during initialization");
//                }
//            }
//            catch (Exception ex)
//            {
//                LogException("ScanBuildingView", ex);
//                try { await page.DisplayAlertAsync("Faild scan building QRCode", ex.Message, "OK"); } catch { }
//            }
//            finally
//            {
//            }
//        }

//        public void Btn_FlashlightTapped(object sender, EventArgs e)
//        {
//            try
//            {
//                if (zxing != null)
//                    zxing.IsTorchOn = !zxing.IsTorchOn;
//            }
//            catch (Exception ex)
//            {
//                LogException("Btn_FlashlightTapped", ex);
//            }
//        }

//        public async void Btn_FlashlightAloneTapped(object sender, EventArgs e)
//        {
//            try
//            {
//                if (AppModel.Instance.isFlashLigthAloneON)
//                {
//                    AppModel.Instance.isFlashLigthAloneON = false;
//                    await Flashlight.Default.TurnOffAsync();
//                }
//                else
//                {
//                    AppModel.Instance.isFlashLigthAloneON = true;
//                    await Flashlight.Default.TurnOnAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                LogException("Btn_FlashlightAloneTapped", ex);
//            }
//        }

//        public void FlashON_Handle_Clicked(object sender, System.EventArgs e)
//        {
//            try
//            {
//                if (zxing != null)
//                    zxing.IsTorchOn = !zxing.IsTorchOn;

//            }
//            catch (Exception ex)
//            {
//                LogException("Btn_FlashlightAloneTapped", ex);
//            }
//        }


//    }
//}