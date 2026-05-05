using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace iPMCloud.Mobile.vo
{
    public class Scanner
    {
        public Scanner()
        {
        }

        public bool displayIsOpen = false;

        private EventHandler<BarcodeDetectionEventArgs> _barcodeHandler;

        // Thread-safe guard so StopAsync() is idempotent and never runs concurrently with itself
        private int _isStopping = 0;

        public CameraBarcodeReaderView zxing;
        //public CameraBarcodeReaderView zxing9Alone = new CameraBarcodeReaderView();

        // Eigenes Overlay erstellen (ZXingDefaultOverlay existiert nicht mehr)
        public ContentView overlayz;

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
                var img = new Image();
                img.Source = "Flashlight.png";
                var flashButton = new Button
                {
                    ImageSource = "Flashlight.png",
                    Padding = 5, 
                    //Text = "🔦",
                    BackgroundColor = Color.FromRgb(20, 77, 147),
                    //TextColor = Colors.White,
                    CornerRadius = 0,
                    WidthRequest = 70,
                    HeightRequest = 70,Margin = new Thickness(0, 20, 0, 0),
                    //FontSize = 28,
                    HorizontalOptions = LayoutOptions.Center
                };
                flashButton.Clicked += (s, e) => onFlashButtonClicked?.Invoke();
                bottomStack.Children.Add(flashButton);
            }

            Grid.SetRow(bottomStack, 2);
            overlayGrid.Children.Add(bottomStack);

            return new ContentView { Content = overlayGrid };
        }

        // Delay in ms to let in-flight Android CameraManager Runnables finish before
        // removing the CameraBarcodeReaderView from the visual tree.
        private const int CameraDrainDelayMs = 150;

        /// <summary>
        /// Asynchronously stops the scanner in a thread-safe, idempotent way.
        /// Unsubscribes the barcode handler, disables detection, waits CameraDrainDelayMs for
        /// in-flight Android CameraManager callbacks to drain, then clears the grid.
        /// All UI work is guaranteed to run on the MainThread.
        /// </summary>
        public Task StopAsync()
        {
            // Idempotent: if already stopping, return immediately
            if (Interlocked.CompareExchange(ref _isStopping, 1, 0) != 0)
                return Task.CompletedTask;

            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    // Block late barcode callbacks from re-entering while we are tearing down.
                    // Set inside InvokeOnMainThreadAsync so it is synchronized with all
                    // other MainThread.BeginInvokeOnMainThread barcode-callback reads.
                    displayIsOpen = true;

                    try
                    {
                        if (zxing != null)
                        {
                            if (_barcodeHandler != null)
                            {
                                zxing.BarcodesDetected -= _barcodeHandler;
                                _barcodeHandler = null;
                            }
                            zxing.IsDetecting = false;
                            zxing.IsTorchOn = false;
                        }
                    }
                    catch { /* defensive: zxing may already be disposed/detached */ }

                    // Give in-flight Android CameraManager Runnables time to finish
                    // before we remove the view from the visual tree. Without this delay
                    // the native callback can dereference a freed object and cause a NRE.
                    await Task.Delay(CameraDrainDelayMs);

                    try { grid?.Children.Clear(); } catch { /* defensive */ }
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
        /// Exceptions from the async body are silently discarded since Stop() is best-effort.
        /// </summary>
        public void Stop()
        {
            StopAsync().ContinueWith(
                static t => { _ = t.Exception; },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public async void ScanBuildingOutView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
            // Stop any previous scanner instance before starting a new one
            await StopAsync();
            try
            {
                var opts = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
                    AutoRotate = true,
                    Multiple = false
                };

                zxing = new CameraBarcodeReaderView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    AutomationId = "zxingScannerView",
                    Margin = new Thickness(0, 0, 0, 0),
                    Options = opts
                };

                _barcodeHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen && e.Results?.Length > 0)
                        {
                            displayIsOpen = true;
                            var result = e.Results[0];

                            try
                            {
                                var sp = result.Value.Replace("http://www.ipm-cloud.de/?objektid=", "").Split(new String[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                                if (sp != null && sp.Length > 0)
                                {
                                    AppModel.Instance.OutScanBuilding = null;
                                    var CustomerNumber = "0";
                                    if (sp.Length == 1)
                                    {
                                        CustomerNumber = "1";
                                    }
                                    else
                                    {
                                        CustomerNumber = "" + sp[1];
                                    }
                                    Int32 buildingid = Int32.Parse(sp[0]);
                                    if (CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                    {
                                        if (AppModel.Instance.AllBuildings != null && AppModel.Instance.AllBuildings.Count > 0)
                                        {
                                            AppModel.Instance.OutScanBuilding = AppModel.Instance.AllBuildings.Find(bu => bu.id == buildingid);
                                            try
                                            {
                                                AppModel.Logger.Info("CHECK-OUT: " + AppModel.Instance.OutScanBuilding.strasse + " " + AppModel.Instance.OutScanBuilding.hsnr + AppModel.Instance.OutScanBuilding.plz + " " + AppModel.Instance.OutScanBuilding.ort);
                                            }
                                            catch (Exception) { }
                                        }
                                        await StopAsync();
                                        AppModel.Instance.UseExternHardware = false;
                                        func.Invoke();
                                    }
                                    else
                                    {
                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.", "OK");
                                        displayIsOpen = false;
                                    }
                                }
                                else
                                {
                                    await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                    displayIsOpen = false;
                                }
                            }
                            catch (Exception)
                            {
                                await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    });
                };
                zxing.BarcodesDetected += _barcodeHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () => { zxing.IsTorchOn = !zxing.IsTorchOn; }
                );

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsDetecting = true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: ScanBuildingOutView(...)");
                try
                {
                    await page.DisplayAlertAsync("Faild scan QRCode", ex.Message, "OK");
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }
        }

        public async void ScanBuildingView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
            // Stop any previous scanner instance before starting a new one
            await StopAsync();
            try
            {
                var opts = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
                    AutoRotate = true,
                    Multiple = false
                };

                zxing = new CameraBarcodeReaderView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 0, 0, 0),
                    AutomationId = "zxingScannerView",
                    Options = opts
                };

                _barcodeHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen && e.Results?.Length > 0)
                        {
                            displayIsOpen = true;
                            var result = e.Results[0];

                            try
                            {
                                var sp = result.Value.Replace("http://www.ipm-cloud.de/?objektid=", "").Split(new String[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                                if (sp != null && sp.Length > 0)
                                {
                                    var CustomerNumber = "0";
                                    if (sp.Length == 1)
                                    {
                                        CustomerNumber = "1";
                                    }
                                    else
                                    {
                                        CustomerNumber = "" + sp[1];
                                    }
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
                                                AppModel.Logger.Info("CHECK-IN: " + AppModel.Instance.LastBuilding.strasse + " " + AppModel.Instance.LastBuilding.hsnr + AppModel.Instance.LastBuilding.plz + " " + AppModel.Instance.LastBuilding.ort);
                                            }
                                            catch (Exception) { }
                                        }
                                        AppModel.Instance.SettingModel.SaveSettings();
                                        await StopAsync();
                                        AppModel.Instance.UseExternHardware = false;
                                        func.Invoke();
                                    }
                                    else
                                    {
                                        await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.", "OK");
                                        displayIsOpen = false;
                                    }
                                }
                                else
                                {
                                    await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                    displayIsOpen = false;
                                }
                            }
                            catch (Exception)
                            {
                                await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    });
                };
                zxing.BarcodesDetected += _barcodeHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () => { zxing.IsTorchOn = !zxing.IsTorchOn; }
                );

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsDetecting = true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: ScanBuildingView(...)");
                try
                {
                    await page.DisplayAlertAsync("Faild scan building QRCode", ex.Message, "OK");
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }
        }

        public async void ScanRegView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
            // Stop any previous scanner instance before starting a new one
            await StopAsync();
            try
            {
                var opts = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
                    AutoRotate = true,
                    Multiple = false,
                    TryHarder = true,
                    TryInverted = true
                };

                zxing = new CameraBarcodeReaderView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    AutomationId = "zxingScannerView", Margin = new Thickness(0,0,0,0),
                    Options = opts
                };

                _barcodeHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen && e.Results?.Length > 0)
                        {
                            displayIsOpen = true;
                            var result = e.Results[0];

                            try
                            {
                                var sp = result.Value.Replace("https://", "http://").Replace("httpss://", "https://").Split(new String[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                                if (sp.Length < 3)
                                    throw new Exception("QR-Format ungültig.");

                                var newScanSettings = new SettingDTO();
                                newScanSettings.ServerUrl = sp[0];
                                newScanSettings.CustomerNumber = sp[1];
                                newScanSettings.CustomerName = sp[2];

                                if (result.Value.IndexOf("###") > -1 && !String.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerName))
                                {
                                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ipm/" + newScanSettings.CustomerNumber + "");
                                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                                    AppModel.Instance.SettingModel.SettingDTO = newScanSettings;
                                    AppModel.Instance.SettingModel.SaveSettings();
                                    Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

                                    await StopAsync();
                                    AppModel.Instance.UseExternHardware = false;
                                    func.Invoke();
                                }
                                else
                                {
                                    await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung mit der iPM-Cloud nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                    displayIsOpen = false;
                                }
                            }
                            catch (Exception)
                            {
                                await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung mit der iPM-Cloud nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    });
                };
                zxing.BarcodesDetected += _barcodeHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () => { zxing.IsTorchOn = !zxing.IsTorchOn; }
                );

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsDetecting = true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: ScanRegView(...)");
                try
                {
                    await page.DisplayAlertAsync("Faild scan reg QRCode", ex.Message, "OK");
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }
        }

        public async void ScanAddRegView(ContentPage page, StackLayout scanContainer, Func<bool> func, Func<bool> funcfaild)
        {
            // Stop any previous scanner instance before starting a new one
            await StopAsync();
            try
            {
                var opts = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
                    AutoRotate = true,
                    Multiple = false
                };

                zxing = new CameraBarcodeReaderView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 0, 0, 0),
                    AutomationId = "zxingScannerView",
                    Options = opts
                };

                _barcodeHandler = (sender, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen && e.Results?.Length > 0)
                        {
                            displayIsOpen = true;
                            var result = e.Results[0];

                            try
                            {
                                var sp = result.Value.Replace("https://", "http://").Replace("httpss://", "https://").Split(new String[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                                if (sp.Length < 3)
                                    throw new Exception("QR-Format ungültig.");

                                var newScanSettings = new SettingDTO();
                                newScanSettings.ServerUrl = sp[0];
                                newScanSettings.CustomerNumber = sp[1];
                                newScanSettings.CustomerName = sp[2];

                                if (result.Value.IndexOf("###") > -1 && !String.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerName) && newScanSettings.CustomerNumber != AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                {
                                    Company.AddUpdateCompany(AppModel.Instance, AppModel.Instance.SettingModel.SettingDTO);

                                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ipm/" + newScanSettings.CustomerNumber + "");
                                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                                    AppModel.Instance.SettingModel.SettingDTO = newScanSettings;
                                    AppModel.Instance.SettingModel.SaveSettings();

                                    await StopAsync();
                                    AppModel.Instance.UseExternHardware = false;
                                    func.Invoke();
                                }
                                else
                                {
                                    //if (newScanSettings.CustomerNumber == AppModel.Instance.SettingModel.SettingDTO.CustomerNumber)
                                    //{
                                    //    await page.DisplayAlertAsync("Registrierung existiert schon!", "Diesen QR-Code haben Sie schon Registriert!", "OK");
                                    //}
                                    //else
                                    //{
                                    //    await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.", "OK");
                                    //}
                                    await StopAsync();
                                    //funcfaild.Invoke();
                                }
                            }
                            catch (Exception)
                            {
                                await page.DisplayAlertAsync("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.", "OK");

                                await StopAsync();
                                funcfaild.Invoke();
                            }
                        }
                    });
                };
                zxing.BarcodesDetected += _barcodeHandler;

                overlayz = CreateOverlay(
                    "Richten Sie die Kamera auf den QR-Code",
                    "Das Scannen erfolgt automatisch",
                    true,
                    () => { zxing.IsTorchOn = !zxing.IsTorchOn; }
                );

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsDetecting = true;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error(ex, "ERROR: ScanAddRegView(...)");
                try
                {
                    await page.DisplayAlertAsync("Faild scan addreg QRCode", ex.Message, "OK");
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }
        }

        public void Btn_FlashlightTapped(object sender, EventArgs e)
        {
            zxing.IsTorchOn = !zxing.IsTorchOn;
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
            catch (Exception)
            {
            }
        }

        public void FlashON_Handle_Clicked(object sender, System.EventArgs e)
        {
            zxing.IsTorchOn = !zxing.IsTorchOn;
        }
    }
}