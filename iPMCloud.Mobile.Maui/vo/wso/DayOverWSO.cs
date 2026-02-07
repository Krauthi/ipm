using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert einen Tagesabschluss (Check-In/Check-Out)
    /// </summary>
    public class DayOverWSO
    {
        public int gruppeid { get; set; } = 0;
        public int personid { get; set; } = 0;
        public string guid { get; set; } = string.Empty;
        public long endticks { get; set; } = 0;

        // GPS Check-In
        public string latin { get; set; } = string.Empty;
        public string lonin { get; set; } = string.Empty;
        public string messagein { get; set; } = string.Empty;

        // GPS Check-Out
        public string latout { get; set; } = string.Empty;
        public string lonout { get; set; } = string.Empty;
        public string messageout { get; set; } = string.Empty;

        public DayOverWSO()
        {
            this.guid = Guid.NewGuid().ToString();
        }

        #region Save/Load/Delete Methods

        /// <summary>
        /// Speichert einen Tagesabschluss
        /// </summary>
        public static bool Save(AppModel model, DayOverWSO day)
        {
            try
            {
                if (model == null || day == null)
                {
                    AppModel.Logger?.Error("Save DayOverWSO: model or day is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save DayOverWSO: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{day.endticks}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(day, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save DayOverWSO");
                return false;
            }
        }

        /// <summary>
        /// Lädt den letzten (neuesten) Tagesabschluss
        /// </summary>
        public static DayOverWSO LoadLast(AppModel model)
        {
            try
            {
                var list = LoadAll(model);
                return list.FirstOrDefault(); // Bereits sortiert
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadLast DayOverWSO");
                return null;
            }
        }

        /// <summary>
        /// Lädt alle Tagesabschlüsse (maximal 20, automatisches Cleanup)
        /// </summary>
        public static List<DayOverWSO> LoadAll(AppModel model)
        {
            List<DayOverWSO> list = new List<DayOverWSO>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var day = Load(model, file);
                            if (day != null)
                            {
                                list.Add(day);
                            }
                        }
                    }

                    // Nach Datum sortieren (neueste zuerst)
                    list = list.OrderByDescending(d => d.endticks).ToList();

                    // Automatisches Cleanup: Nur die letzten 20 behalten
                    if (list.Count > 20)
                    {
                        for (int i = 20; i < list.Count; i++)
                        {
                            Delete(model, list[i]);
                        }

                        // Nach Cleanup neu laden
                        list = list.Take(20).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAll DayOverWSO");
            }

            return list;
        }

        /// <summary>
        /// Lädt einen einzelnen Tagesabschluss
        /// </summary>
        private static DayOverWSO Load(AppModel model, string filename)
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

                return JsonConvert.DeserializeObject<DayOverWSO>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Load DayOverWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht einen Tagesabschluss
        /// </summary>
        public static bool Delete(AppModel model, DayOverWSO day)
        {
            try
            {
                if (day == null || model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/" + day.endticks + ".ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: Delete DayOverWSO");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Tagesabschlüsse
        /// </summary>
        public static bool DeleteAll(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayover/"
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
                AppModel.Logger?.Error(ex, "ERROR: DeleteAll DayOverWSO");
                return false;
            }
        }

        #endregion

        #region Upload Stack Management

        /// <summary>
        /// Fügt einen Tagesabschluss zum Upload-Stack hinzu
        /// </summary>
        public static bool ToUploadStack(AppModel model, DayOverWSO day)
        {
            try
            {
                if (model == null || day == null)
                {
                    AppModel.Logger?.Error("ToUploadStack DayOverWSO: model or day is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{day.endticks}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(day, jsonSettings);
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
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack DayOverWSO");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der Tagesabschlüsse im Upload-Stack
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack DayOverWSO");
                return 0;
            }
        }

        /// <summary>
        /// Lädt alle Tagesabschlüsse aus dem Upload-Stack
        /// </summary>
        public static List<DayOverWSO> LoadAllFromUploadStack(AppModel model)
        {
            List<DayOverWSO> list = new List<DayOverWSO>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var day = LoadFromUploadStack(model, file);
                            if (day != null)
                            {
                                list.Add(day);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack DayOverWSO");
            }

            return list;
        }

        /// <summary>
        /// Lädt einen einzelnen Tagesabschluss aus dem Upload-Stack
        /// </summary>
        private static DayOverWSO LoadFromUploadStack(AppModel model, string filename)
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

                return JsonConvert.DeserializeObject<DayOverWSO>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromUploadStack DayOverWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht einen Tagesabschluss aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(AppModel model, DayOverWSO day)
        {
            try
            {
                if (day == null || model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/" + day.endticks + ".ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack DayOverWSO");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Tagesabschlüsse aus dem Upload-Stack
        /// </summary>
        public static bool ClearUploadStack(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/dayoverupload/"
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
                AppModel.Logger?.Error(ex, "ERROR: ClearUploadStack DayOverWSO");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gibt das End-Datum als DateTime zurück
        /// </summary>
        public DateTime GetEndDateTime()
        {
            return new DateTime(endticks);
        }

        /// <summary>
        /// Setzt das End-Datum von einem DateTime
        /// </summary>
        public void SetEndDateTime(DateTime dateTime)
        {
            endticks = dateTime.Ticks;
        }

        /// <summary>
        /// Prüft ob GPS Check-In Daten vorhanden sind
        /// </summary>
        public bool HasCheckInGPS()
        {
            return !string.IsNullOrWhiteSpace(latin) && !string.IsNullOrWhiteSpace(lonin);
        }

        /// <summary>
        /// Prüft ob GPS Check-Out Daten vorhanden sind
        /// </summary>
        public bool HasCheckOutGPS()
        {
            return !string.IsNullOrWhiteSpace(latout) && !string.IsNullOrWhiteSpace(lonout);
        }

        /// <summary>
        /// Gibt die Check-In GPS-Koordinaten zurück
        /// </summary>
        public (double latitude, double longitude)? GetCheckInCoordinates()
        {
            if (HasCheckInGPS() &&
                double.TryParse(latin, out double lat) &&
                double.TryParse(lonin, out double lon))
            {
                return (lat, lon);
            }
            return null;
        }

        /// <summary>
        /// Gibt die Check-Out GPS-Koordinaten zurück
        /// </summary>
        public (double latitude, double longitude)? GetCheckOutCoordinates()
        {
            if (HasCheckOutGPS() &&
                double.TryParse(latout, out double lat) &&
                double.TryParse(lonout, out double lon))
            {
                return (lat, lon);
            }
            return null;
        }

        /// <summary>
        /// Prüft ob der Tagesabschluss gültig ist
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(guid) &&
                   endticks > 0 &&
                   personid > 0;
        }

        #endregion
    }
}