using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert ein Bild für eine Check-Leistungs-Antwort-Bemerkung
    /// </summary>
    public class CheckLeistungAntwortBemImg
    {
        public int bem_id { get; set; } = 0;
        public string bem_guid { get; set; } = null;
        public string guid { get; set; } = null;
        public string url { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;

        public CheckLeistungAntwortBemImg()
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
        }

        #region Save/Load/Delete Methods (Temporary Storage)

        /// <summary>
        /// Speichert ein Bild temporär
        /// </summary>
        public static bool Save(CheckLeistungAntwortBemImg b)
        {
            try
            {
                if (b == null)
                {
                    AppModel.Logger?.Error("Save CheckLeistungAntwortBemImg: b is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save CheckLeistungAntwortBemImg: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"_{b.guid}_{b.bem_guid}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(b, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save CheckLeistungAntwortBemImg");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Lädt alle Bilder zu einer bestimmten GUID
        /// </summary>
        public static List<CheckLeistungAntwortBemImg> LoadFromGuid(string guid)
        {
            List<CheckLeistungAntwortBemImg> list = new List<CheckLeistungAntwortBemImg>();

            try
            {
                if (string.IsNullOrWhiteSpace(guid))
                {
                    return list;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/"
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
                                var img = Load(file);
                                if (img != null)
                                {
                                    list.Add(img);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromGuid CheckLeistungAntwortBemImg - {guid}");
            }

            return list;
        }

        /// <summary>
        /// Lädt ein einzelnes Bild
        /// </summary>
        public static CheckLeistungAntwortBemImg Load(string filename)
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

                var imgWso = JsonConvert.DeserializeObject<CheckLeistungAntwortBemImg>(jsonString, jsonSettings);

                if (imgWso != null)
                {
                    imgWso.filename = filename;
                }

                return imgWso;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Load CheckLeistungAntwortBemImg - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht ein Bild
        /// </summary>
        public static bool Delete(CheckLeistungAntwortBemImg b)
        {
            try
            {
                if (b == null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/_" + b.guid + "_" + b.bem_guid + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete CheckLeistungAntwortBemImg");
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbi/"
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
                AppModel.Logger?.Error(ex, "ERROR: DeleteAllTemp CheckLeistungAntwortBemImg");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        #endregion

        #region Upload Stack Management

        /// <summary>
        /// Speichert ein Bild im Upload-Stack
        /// </summary>
        public static bool SaveToStack(CheckLeistungAntwortBemImg b)
        {
            try
            {
                if (b == null)
                {
                    AppModel.Logger?.Error("SaveToStack CheckLeistungAntwortBemImg: b is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/"
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
                    DefaultValueHandling = DefaultValueHandling.Include
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
                AppModel.Logger?.Error(ex, "ERROR: SaveToStack CheckLeistungAntwortBemImg");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Lädt alle Bilder aus dem Upload-Stack
        /// </summary>
        public static List<CheckLeistungAntwortBemImg> LoadAllFromStack()
        {
            List<CheckLeistungAntwortBemImg> list = new List<CheckLeistungAntwortBemImg>();

            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var img = Load(file);
                            if (img != null)
                            {
                                list.Add(img);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromStack CheckLeistungAntwortBemImg");
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack CheckLeistungAntwortBemImg");
                return 0;
            }
        }

        /// <summary>
        /// Löscht ein Bild aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromStack(CheckLeistungAntwortBemImg pic)
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/_" + pic.guid + ".ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromStack CheckLeistungAntwortBemImg");
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/check_nbis/"
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
                AppModel.Logger?.Error(ex, "ERROR: ClearStack CheckLeistungAntwortBemImg");
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
            return !string.IsNullOrWhiteSpace(guid);
        }

        /// <summary>
        /// Prüft ob eine URL vorhanden ist
        /// </summary>
        public bool HasUrl()
        {
            return !string.IsNullOrWhiteSpace(url);
        }

        /// <summary>
        /// Prüft ob ein lokaler Dateiname vorhanden ist
        /// </summary>
        public bool HasFilename()
        {
            return !string.IsNullOrWhiteSpace(filename) && File.Exists(filename);
        }

        /// <summary>
        /// Gibt die Dateigröße zurück (falls Datei lokal vorhanden)
        /// </summary>
        public long? GetFileSize()
        {
            if (HasFilename())
            {
                try
                {
                    var fileInfo = new FileInfo(filename);
                    return fileInfo.Length;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        #endregion
    }
}