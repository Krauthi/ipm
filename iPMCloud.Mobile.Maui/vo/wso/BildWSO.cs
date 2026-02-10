using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert ein Bild-Objekt
    /// </summary>
    public class BildWSO
    {
        public string name { get; set; } = string.Empty;
        public byte[] bytes { get; set; }
        public string guid { get; set; } = string.Empty;
        public string mainguid { get; set; } = string.Empty;
        public int bemId { get; set; } = -1;
        public string filename { get; set; } = string.Empty;

        [JsonIgnore] // Nicht serialisieren
        public StackLayout stack { get; set; }

        public BildWSO()
        {
            this.guid = Guid.NewGuid().ToString();
        }

        public BildWSO(string mainguid)
        {
            this.mainguid = mainguid;
            this.guid = Guid.NewGuid().ToString();
        }

        #region UI Helper Methods

        /// <summary>
        /// Erstellt ein UI-Element für ein Attachment
        /// </summary>
        public static StackLayout GetAttachmentForNoticeElement(ImageSource source, string text, ICommand func = null)
        {
            var deleteButton = new Border
            {
                BackgroundColor = Color.FromArgb("#041d43"),
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Opacity = 0.5f,
                    Radius = 5,
                    Offset = new Point(2, 2)
                },
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(1),
                Margin = new Thickness(3, 0, 3, 0),
                Content = new StackLayout
                {
                    BackgroundColor = Color.FromArgb("#042d53"),
                    HeightRequest = 40,
                    WidthRequest = 40,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    Spacing = 0,
                    Children =
                    {
                        new Image
                        {
                            Margin = new Thickness(0),
                            HeightRequest = 30,
                            WidthRequest = 30,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            Source = AppModel.Instance.imagesBase.Trash,
                        }
                    }
                }
            };

            // Command hinzufügen wenn vorhanden
            if (func != null)
            {
                var tapGesture = new TapGestureRecognizer { Command = func };
                deleteButton.GestureRecognizers.Add(tapGesture);
            }

            return new StackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Children =
                {
                    new StackLayout
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 10,
                        Padding = new Thickness(0),
                        Margin = new Thickness(5, 2, 5, 2),
                        Children =
                        {
                            new Image
                            {
                                Margin = new Thickness(0, 0, 20, 0),
                                HeightRequest = 60,
                                HorizontalOptions = LayoutOptions.Start,
                                VerticalOptions = LayoutOptions.Center,
                                Source = source,
                            },
                            new Label
                            {
                                Text = text,
                                Padding = new Thickness(0),
                                Margin = new Thickness(0),
                                HorizontalOptions = LayoutOptions.Start,
                                VerticalOptions = LayoutOptions.Center,
                                FontSize = 14,
                                TextColor = Colors.White,
                                LineBreakMode = LineBreakMode.WordWrap
                            },
                            deleteButton
                        }
                    },
                    new BoxView
                    {
                        BackgroundColor = Colors.Gray,
                        HeightRequest = 1,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Fill,
                        Margin = new Thickness(0)
                    }
                }
            };
        }

        #endregion

        #region Save/Load/Delete Methods (Temporary Storage)

        /// <summary>
        /// Speichert ein Bild temporär
        /// </summary>
        public static bool Save(AppModel model, BildWSO b)
        {
            try
            {
                if (model == null || b == null)
                {
                    AppModel.Logger?.Error("Save BildWSO: model or b is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save BildWSO: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbi/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"_{b.guid}_{b.mainguid}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(b, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save BildWSO");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Lädt alle Bilder zu einer bestimmten GUID
        /// </summary>
        public static List<BildWSO> LoadFromGuid(AppModel model, string guid)
        {
            List<BildWSO> list = new List<BildWSO>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(guid))
                {
                    return list;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbi/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            if (file.Contains(guid))
                            {
                                var bild = Load(model, file);
                                if (bild != null)
                                {
                                    list.Add(bild);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromGuid BildWSO - {guid}");
            }

            return list;
        }

        /// <summary>
        /// Lädt ein einzelnes Bild
        /// </summary>
        public static BildWSO Load(AppModel model, string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    return null;
                }

                string jsonString = File.ReadAllText(filename);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return null;
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var bildwso = JsonConvert.DeserializeObject<BildWSO>(jsonString, jsonSettings);

                if (bildwso != null)
                {
                    bildwso.filename = filename;
                }

                return bildwso;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Load BildWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht ein Bild
        /// </summary>
        public static bool Delete(AppModel model, BildWSO b)
        {
            try
            {
                if (b == null || model == null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbi/_" + b.guid + "_" + b.mainguid + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete BildWSO");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Löscht alle temporären Bilder
        /// </summary>
        public static bool DeleteAllTemp()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbi/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteAllTemp BildWSO");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        #endregion

        #region Upload Stack Management

        /// <summary>
        /// Speichert ein Bild im Upload-Stack
        /// </summary>
        public static bool SaveToStack(AppModel model, BildWSO b)
        {
            try
            {
                if (model == null || b == null)
                {
                    AppModel.Logger?.Error("SaveToStack BildWSO: model or b is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/nbis/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"_{b.guid}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(b, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                // UI Update
                if (AppModel.Instance?.MainPage != null)
                {
                    AppModel.Instance.MainPage.SetAllSyncState();
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: SaveToStack BildWSO");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Lädt alle Bilder aus dem Upload-Stack
        /// </summary>
        public static List<BildWSO> LoadAllFromStack()
        {
            List<BildWSO> list = new List<BildWSO>();

            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbis/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var bild = Load(AppModel.Instance, file);
                            if (bild != null)
                            {
                                list.Add(bild);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromStack BildWSO");
            }

            return list;
        }

        /// <summary>
        /// Zählt die Anzahl der Bilder im Upload-Stack
        /// </summary>
        public static int CountFromStack()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return 0;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbis/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack BildWSO");
                return 0;
            }
        }

        /// <summary>
        /// Löscht ein Bild aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromStack(BildWSO pic)
        {
            try
            {
                if (pic == null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbis/_" + pic.guid + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    // UI Update
                    if (AppModel.Instance?.MainPage != null)
                    {
                        AppModel.Instance.MainPage.SetAllSyncState();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromStack BildWSO");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Bilder aus dem Upload-Stack
        /// </summary>
        public static bool ClearStack()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/nbis/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    // UI Update
                    if (AppModel.Instance?.MainPage != null)
                    {
                        AppModel.Instance.MainPage.SetAllSyncState();
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ClearStack BildWSO");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob das Bild gültig ist
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(guid) &&
                   bytes != null &&
                   bytes.Length > 0;
        }

        /// <summary>
        /// Gibt die Größe des Bildes in Bytes zurück
        /// </summary>
        public int GetImageSize()
        {
            return bytes?.Length ?? 0;
        }

        /// <summary>
        /// Gibt die Größe des Bildes in KB zurück
        /// </summary>
        public double GetImageSizeKB()
        {
            return GetImageSize() / 1024.0;
        }

        /// <summary>
        /// Gibt die Größe des Bildes in MB zurück
        /// </summary>
        public double GetImageSizeMB()
        {
            return GetImageSizeKB() / 1024.0;
        }

        /// <summary>
        /// Gibt ImageSource für MAUI zurück
        /// </summary>
        public ImageSource GetImageSource()
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }

        /// <summary>
        /// Speichert das Bild in eine Datei
        /// </summary>
        public bool SaveToFile(string filePath)
        {
            try
            {
                if (bytes != null && bytes.Length > 0)
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllBytes(filePath, bytes);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: SaveToFile BildWSO");
                return false;
            }
        }

        /// <summary>
        /// Gibt eine Beschreibung des Objekts zurück
        /// </summary>
        public override string ToString()
        {
            return $"BildWSO [Name: {name}, GUID: {guid}, Size: {GetImageSizeKB():F2} KB]";
        }

        #endregion
    }
}