using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
// TODO: ZXing.Net.Mobile.Forms not MAUI-compatible - use ZXing.Net.Maui
// using ZXing.Net.Mobile.Forms;

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
        // TODO: ZXing.Net.Mobile.Forms not MAUI-compatible - use ZXing.Net.Maui
        // public ZXingScannerView zxing;
        // public ZXingScannerView zxingAlone = new ZXingScannerView();
        // public ZXingDefaultOverlay overlayz;
        public Grid grid = new Grid
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
        };
        public Image img = new Image
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
        };


        public async void ScanBuildingOutView(ContentPage page, StackLayout scanContainer, Func<bool> func)
        {
            try
            {
                var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
                var opts = new ZXing.Mobile.MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }
                };
                zxing = new ZXingScannerView
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    AutomationId = "zxingScannerView",
                };
                zxing.Options = opts;
                zxing.OnScanResult += (result) =>
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen)
                        {
                            displayIsOpen = true;
                            try
                            {
                                var sp = result.Text.Replace("http://www.ipm-cloud.de/?objektid=", "").Split(new String[] { "_" }, System.StringSplitOptions.RemoveEmptyEntries);
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
                                        zxing.IsTorchOn = false;// flashlight off;
                                        zxing.IsAnalyzing = false;
                                        zxing.IsScanning = false;
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
                            catch (Exception e)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    }
                );

                overlayz = new ZXingDefaultOverlay
                {
                    TopText = "Richten Sie die Kamera auf den QR-Code",
                    BottomText = "Das Scannen erfolgt automatisch",
                    ShowFlashButton = false,// zxing.HasTorch,
                    AutomationId = "zxingDefaultOverlay",
                };
                overlayz.FlashButtonClicked += (sender, e) =>
                {
                    zxing.IsTorchOn = !zxing.IsTorchOn;
                };

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsScanning = true;
                zxing.IsAnalyzing = true;
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
                var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
                var opts = new ZXing.Mobile.MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }
                };
                zxing = new ZXingScannerView
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    AutomationId = "zxingScannerView",
                };
                zxing.Options = opts;
                zxing.OnScanResult += (result) =>
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen)
                        {
                            displayIsOpen = true;
                            try
                            {
                                var sp = result.Text.Replace("http://www.ipm-cloud.de/?objektid=", "").Split(new String[] { "_" }, System.StringSplitOptions.RemoveEmptyEntries);
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
                                            // Zurücksetzten aller States für die Auswahl der Ausführungen
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
                                        zxing.IsTorchOn = false;// flashlight off;
                                        zxing.IsAnalyzing = false;
                                        zxing.IsScanning = false;
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
                            catch (Exception e)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    }
                );

                overlayz = new ZXingDefaultOverlay
                {
                    TopText = "Richten Sie die Kamera auf den QR-Code",
                    BottomText = "Das Scannen erfolgt automatisch",
                    ShowFlashButton = false,// zxing.HasTorch,
                    AutomationId = "zxingDefaultOverlay",
                };
                overlayz.FlashButtonClicked += (sender, e) =>
                {
                    zxing.IsTorchOn = !zxing.IsTorchOn;
                };

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsScanning = true;
                zxing.IsAnalyzing = true;
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
                var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
                var opts = new ZXing.Mobile.MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }
                };

                //opts.TryHarder = true;
                //opts.AutoRotate = true;
                //opts.TryInverted = true;
                //opts.UseNativeScanning = true;
                //opts.DisableAutofocus = true;

                zxing = new ZXingScannerView
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    AutomationId = "zxingScannerView",
                };
                zxing.Options = opts;
                zxing.OnScanResult += (result) =>
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen)
                        {
                            displayIsOpen = true;
                            try
                            {
                                var sp = result.Text.Replace("https://", "http://").Replace("httpss://", "https://").Split(new String[] { "###" }, System.StringSplitOptions.RemoveEmptyEntries);

                                var newScanSettings = new SettingDTO();
                                newScanSettings.ServerUrl = sp[0];
                                newScanSettings.CustomerNumber = sp[1]; // MandantId
                                newScanSettings.CustomerName = sp[2];

                                if (result.Text.IndexOf("###") > -1 && !String.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerName))
                                {

                                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + newScanSettings.CustomerNumber + "");
                                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                                    model.SettingModel.SettingDTO = newScanSettings;
                                    model.SettingModel.SaveSettings();
                                    Company.AddUpdateCompany(model, model.SettingModel.SettingDTO);

                                    zxing.IsTorchOn = false;// flashlight off;
                                    zxing.IsAnalyzing = false;
                                    zxing.IsScanning = false;
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
                            catch (Exception e)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung mit der iPM-Cloud nicht verwendet werden. Bitte Probieren Sie es noch einmal.", "OK");
                                displayIsOpen = false;
                            }
                        }
                    }
                );

                overlayz = new ZXingDefaultOverlay
                {
                    TopText = "Richten Sie die Kamera auf den QR-Code",
                    BottomText = "Das Scannen erfolgt automatisch",
                    ShowFlashButton = false,// zxing.HasTorch,
                    AutomationId = "zxingDefaultOverlay",
                };
                overlayz.FlashButtonClicked += (sender, e) =>
                {
                    zxing.IsTorchOn = !zxing.IsTorchOn;
                };

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsScanning = true;
                zxing.IsAnalyzing = true;
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
                var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
                var opts = new ZXing.Mobile.MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }
                };
                zxing = new ZXingScannerView
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    AutomationId = "zxingScannerView",
                };
                zxing.Options = opts;
                zxing.OnScanResult += (result) =>
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!displayIsOpen)
                        {
                            displayIsOpen = true;
                            try
                            {
                                var sp = result.Text.Replace("https://", "http://").Replace("httpss://", "https://").Split(new String[] { "###" }, System.StringSplitOptions.RemoveEmptyEntries);

                                var newScanSettings = new SettingDTO();
                                newScanSettings.ServerUrl = sp[0];
                                newScanSettings.CustomerNumber = sp[1]; // MandantId
                                newScanSettings.CustomerName = sp[2];

                                if (result.Text.IndexOf("###") > -1 && !String.IsNullOrWhiteSpace(newScanSettings.ServerUrl) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerNumber) &&
                                    !String.IsNullOrWhiteSpace(newScanSettings.CustomerName) && newScanSettings.CustomerNumber != model.SettingModel.SettingDTO.CustomerNumber)
                                {
                                    // Vorherige aktive Company/SettingDTO speichern
                                    Company.AddUpdateCompany(model, model.SettingModel.SettingDTO);

                                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ipm/" + newScanSettings.CustomerNumber + "");
                                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                                    model.SettingModel.SettingDTO = newScanSettings;
                                    model.SettingModel.SaveSettings();

                                    zxing.IsTorchOn = false;// flashlight off;
                                    zxing.IsAnalyzing = false;
                                    zxing.IsScanning = false;
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
                                    zxing.IsTorchOn = false;// flashlight off;
                                    zxing.IsAnalyzing = false;
                                    zxing.IsScanning = false;
                                    grid.Children.Clear();
                                    displayIsOpen = false;
                                    funcfaild.Invoke();
                                }
                            }
                            catch (Exception e)
                            {
                                await page.DisplayAlert("QR-Code nicht erkannt!", "Dieser QR-Code kann für die Registrierung eines weiteren Unternehmens mit der iPM-Cloud-App nicht verwendet werden.", "OK");

                                zxing.IsTorchOn = false;// flashlight off;
                                zxing.IsAnalyzing = false;
                                zxing.IsScanning = false;
                                grid.Children.Clear();
                                displayIsOpen = false;
                                funcfaild.Invoke();
                            }
                        }
                    }
                );

                overlayz = new ZXingDefaultOverlay
                {
                    TopText = "Richten Sie die Kamera auf den QR-Code",
                    BottomText = "Das Scannen erfolgt automatisch",
                    ShowFlashButton = false,// zxing.HasTorch,
                    AutomationId = "zxingDefaultOverlay",
                };
                overlayz.FlashButtonClicked += (sender, e) =>
                {
                    zxing.IsTorchOn = !zxing.IsTorchOn;
                };

                grid.Children.Clear();
                grid.Children.Add(zxing);
                grid.Children.Add(overlayz);
                scanContainer.Children.Add(grid);
                zxing.IsScanning = true;
                zxing.IsAnalyzing = true;
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
                    await Flashlight.TurnOffAsync();
                }
                else
                {
                    AppModel.Instance.isFlashLigthAloneON = true;
                    await Flashlight.TurnOnAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }
        public void FlashON_Handle_Clicked(object sender, System.EventArgs e)
        {
            zxing.IsTorchOn = !zxing.IsTorchOn;

            //if (isFlashlight)
            //{
            //    btn_flashlight_regscan.Text = "Licht aus";
            //    try
            //    {
            //        // Turn On Flashlight  
            //        await Flashlight.TurnOnAsync();
            //    }
            //    catch (FeatureNotSupportedException fnsEx)
            //    {
            //        await DisplayAlert("(FNS) Faild by take ON Flashlight", fnsEx.Message, "Ok");
            //    }
            //    catch (PermissionException pEx)
            //    {
            //        await DisplayAlert("(P) Faild by take ON Flashlight", pEx.Message, "Ok");
            //    }
            //    catch (Exception ex)
            //    {
            //        await DisplayAlert("(R) Faild by take ON Flashlight", ex.Message, "Ok");
            //    }
            //}
            //else
            //{
            //    btn_flashlight_regscan.Text = "Licht an";
            //    try
            //    {
            //        // Turn Off Flashlight  
            //        await Flashlight.TurnOffAsync();
            //    }
            //    catch (FeatureNotSupportedException fnsEx)
            //    {
            //        await DisplayAlert("(FNS)´Faild by take OFF Flashlight", fnsEx.Message, "Ok");
            //    }
            //    catch (PermissionException pEx)
            //    {
            //        await DisplayAlert("(P) Faild by take OFF Flashlight", pEx.Message, "Ok");
            //    }
            //    catch (Exception ex)
            //    {
            //        await DisplayAlert("(R) Faild by take OFF Flashlight", ex.Message, "Ok");
            //    }
            //}
        }





    }
}
