using System;
using System.Collections.Generic;
using System.IO;
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
        AppModel model = null;
        public Scanner(AppModel _model)
        {
            model = _model;
        }

        public bool displayIsOpen = false;

        public CameraBarcodeReaderView zxing;
        public CameraBarcodeReaderView zxingAlone = new CameraBarcodeReaderView();

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
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(topLabel, 0);
            overlayGrid.Children.Add(topLabel);

            // Scanner Frame (Mitte)
            var scanFrame = new Frame
            {
                BorderColor = Colors.White,
                BackgroundColor = Colors.Transparent,
                CornerRadius = 10,
                WidthRequest = 250,
                HeightRequest = 250,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = 0,
                HasShadow = false
            };
            Grid.SetRow(scanFrame, 1);
            overlayGrid.Children.Add(scanFrame);

            // Bottom Stack
            var bottomStack = new StackLayout
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 10,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var bottomLabel = new Label
            {
                Text = bottomText,
                TextColor = Colors.White,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center
            };
            bottomStack.Children.Add(bottomLabel);

            // Flash Button
            if (showFlashButton)
            {
                var flashButton = new Button
                {
                    Text = "🔦",
                    BackgroundColor = Colors.Gray.WithAlpha(0.7f),
                    TextColor = Colors.White,
                    CornerRadius = 25,
                    WidthRequest = 50,
                    HeightRequest = 50,
                    HorizontalOptions = LayoutOptions.Center
                };
                flashButton.Clicked += (s, e) => onFlashButtonClicked?.Invoke();
                bottomStack.Children.Add(flashButton);
            }

            Grid.SetRow(bottomStack, 2);
            overlayGrid.Children.Add(bottomStack);

            return overlayGrid;
        }

        public async void ScanBuildingOutView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
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
                    Options = opts
                };

                zxing.BarcodesDetected += (sender, e) =>
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
                                    model.OutScanBuilding = null;
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
                                    if (CustomerNumber == model.SettingModel.SettingDTO.CustomerNumber)
                                    {
                                        if (model.AllBuildings != null && model.AllBuildings.Count > 0)
                                        {
                                            model.OutScanBuilding = model.AllBuildings.Find(bu => bu.id == buildingid);
                                            try
                                            {
                                                AppModel.Logger.Info("CHECK-OUT: " + model.OutScanBuilding.strasse + " " + model.OutScanBuilding.hsnr + model.OutScanBuilding.plz + " " + model.OutScanBuilding.ort);
                                            }
                                            catch (Exception) { }
                                        }
                                        zxing.IsTorchOn = false;
                                        zxing.IsDetecting = false;
                                        grid.Children.Clear();
                                        displayIsOpen = false;

                                        model.UseExternHardware = false;
                                        func.Invoke();
                                    }
                                    else
                                    {
                                        await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.", "OK");
                                        displayIsOpen = false;
                                    }
                                }
                                else
                                {
                                    await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                    displayIsOpen = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    });
                };

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
                    await page.DisplayAlert("Faild scan QRCode", ex.Message, "OK");
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }
        }

        public async void ScanBuildingView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
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
                    Options = opts
                };

                zxing.BarcodesDetected += (sender, e) =>
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

                                    if (CustomerNumber == model.SettingModel.SettingDTO.CustomerNumber)
                                    {
                                        model.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;
                                        if (buildingid > 0 && model.AllBuildings != null && model.AllBuildings.Count > 0)
                                        {
                                            model.SetAllObjectAndValuesToNoSelectedBuilding();
                                            model.SettingModel.SettingDTO.LastBuildingIdScanned = buildingid;
                                            model.LastBuilding = model.AllBuildings.Find(bu => bu.id == buildingid);
                                            try
                                            {
                                                AppModel.Logger.Info("CHECK-IN: " + model.LastBuilding.strasse + " " + model.LastBuilding.hsnr + model.LastBuilding.plz + " " + model.LastBuilding.ort);
                                            }
                                            catch (Exception) { }
                                        }
                                        model.SettingModel.SaveSettings();
                                        zxing.IsTorchOn = false;
                                        zxing.IsDetecting = false;
                                        grid.Children.Clear();
                                        displayIsOpen = false;
                                        model.UseExternHardware = false;
                                        func.Invoke();
                                    }
                                    else
                                    {
                                        await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code ist zwar ein iPM-Cloud Code jedoch gehört er nicht zum Registrieten Unternehmen! Bitte Probieren Sie es noch einmal oder melden Sie sich in Ihrer Zentrale.", "OK");
                                        displayIsOpen = false;
                                    }
                                }
                                else
                                {
                                    await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                    displayIsOpen = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    });
                };

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
                    await page.DisplayAlert("Faild scan building QRCode", ex.Message, "OK");
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }
        }

        public async void ScanRegView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
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
                    AutomationId = "zxingScannerView",
                    Options = opts
                };

                zxing.BarcodesDetected += (sender, e) =>
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

                                var newScanSettings = new SettingDTO();
                                newScanSettings.ServerUrl = sp[0];
                                newScanSettings.CustomerNumber = sp[1];
                                newScanSettings.CustomerName = sp[2];

                                if (result.Value.IndexOf("###") > -1 && !String.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerName))
                                {
                                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + newScanSettings.CustomerNumber + "");
                                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                                    model.SettingModel.SettingDTO = newScanSettings;
                                    model.SettingModel.SaveSettings();
                                    Company.AddUpdateCompany(model, model.SettingModel.SettingDTO);

                                    zxing.IsTorchOn = false;
                                    zxing.IsDetecting = false;
                                    grid.Children.Clear();
                                    displayIsOpen = false;
                                    model.UseExternHardware = false;
                                    func.Invoke();
                                }
                                else
                                {
                                    await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung mit der iPM-Cloud nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                    displayIsOpen = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung mit der iPM-Cloud nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    });
                };

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
                    await page.DisplayAlert("Faild scan reg QRCode", ex.Message, "OK");
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }
        }

        public async void ScanAddRegView(ContentPage page, StackLayout scanContainer, Func<bool> func, Func<bool> funcfaild)
        {
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
                    Options = opts
                };

                zxing.BarcodesDetected += (sender, e) =>
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

                                var newScanSettings = new SettingDTO();
                                newScanSettings.ServerUrl = sp[0];
                                newScanSettings.CustomerNumber = sp[1];
                                newScanSettings.CustomerName = sp[2];

                                if (result.Value.IndexOf("###") > -1 && !String.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerName) && newScanSettings.CustomerNumber != model.SettingModel.SettingDTO.CustomerNumber)
                                {
                                    Company.AddUpdateCompany(model, model.SettingModel.SettingDTO);

                                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + newScanSettings.CustomerNumber + "");
                                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                                    model.SettingModel.SettingDTO = newScanSettings;
                                    model.SettingModel.SaveSettings();

                                    zxing.IsTorchOn = false;
                                    zxing.IsDetecting = false;
                                    grid.Children.Clear();
                                    displayIsOpen = false;
                                    model.UseExternHardware = false;
                                    func.Invoke();
                                }
                                else
                                {
                                    if (newScanSettings.CustomerNumber == model.SettingModel.SettingDTO.CustomerNumber)
                                    {
                                        await page.DisplayAlert("Registrierung existiert schon!", "Diesen QR-Code haben Sie schon Registriert!", "OK");
                                    }
                                    else
                                    {
                                        await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.", "OK");
                                    }
                                    zxing.IsTorchOn = false;
                                    zxing.IsDetecting = false;
                                    grid.Children.Clear();
                                    displayIsOpen = false;
                                    funcfaild.Invoke();
                                }
                            }
                            catch (Exception ex)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.", "OK");

                                zxing.IsTorchOn = false;
                                zxing.IsDetecting = false;
                                grid.Children.Clear();
                                displayIsOpen = false;
                                funcfaild.Invoke();
                            }
                        }
                    });
                };

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
                    await page.DisplayAlert("Faild scan addreg QRCode", ex.Message, "OK");
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
            catch (Exception ex)
            {
            }
        }

        public void FlashON_Handle_Clicked(object sender, System.EventArgs e)
        {
            zxing.IsTorchOn = !zxing.IsTorchOn;
        }
    }
}