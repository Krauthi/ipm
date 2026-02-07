using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert ein Leistungspaket mit mehreren Leistungen
    /// </summary>
    public class LeistungPackWSO
    {
        public long startticks { get; set; } = 0;
        public long endticks { get; set; } = 0;
        public int status { get; set; } = 0;
        public int personid { get; set; } = 0;
        public int diffObjekt { get; set; } = 0;
        public string guid { get; set; } = string.Empty;

        // GPS Check-In
        public string latin { get; set; } = string.Empty;
        public string lonin { get; set; } = string.Empty;
        public string messagein { get; set; } = "Kein GPS";

        // GPS Check-Out
        public string latout { get; set; } = string.Empty;
        public string lonout { get; set; } = string.Empty;
        public string messageout { get; set; } = "Kein GPS";

        public bool preview { get; set; } = true;
        public int winterservice { get; set; } = 0;

        public List<PlanPersonMobile> opwm { get; set; } = null;
        public List<LeistungInWorkWSO> leistungen { get; set; }

        public LeistungPackWSO()
        {
            this.guid = Guid.NewGuid().ToString();
            this.leistungen = new List<LeistungInWorkWSO>();
        }

        #region Save/Load/Delete Methods

        /// <summary>
        /// Speichert das aktuelle Leistungspaket
        /// </summary>
        public static bool Save(AppModel model, LeistungPackWSO pack)
        {
            try
            {
                if (model == null || pack == null)
                {
                    AppModel.Logger?.Error("Save LeistungPackWSO: model or pack is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save LeistungPackWSO: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/works/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "pack.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(pack, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save LeistungPackWSO");
                return false;
            }
        }

        /// <summary>
        /// Lädt das aktuelle Leistungspaket
        /// </summary>
        public static LeistungPackWSO Load(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return null;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/works/pack.ipm"
                );

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return null;
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    return JsonConvert.DeserializeObject<LeistungPackWSO>(jsonString, jsonSettings);
                }

                return null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load LeistungPackWSO");
                return null;
            }
        }

        /// <summary>
        /// Lädt das Leistungspaket als JSON-String
        /// </summary>
        public static string Load_AsJson()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return "{\"Error\": \"CustomerNumber not set\"}";
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/works/pack.ipm"
                );

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return "{\"Error\": \"File is empty\"}";
                    }

                    return jsonString;
                }
                else
                {
                    return "{\"Info\": \"File not exist\"}";
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load_AsJson LeistungPackWSO");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        /// <summary>
        /// Löscht das aktuelle Leistungspaket
        /// </summary>
        public static bool Delete(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/works/pack.ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: Delete LeistungPackWSO");
                return false;
            }
        }

        #endregion

        #region Upload Stack Management

        /// <summary>
        /// Fügt ein Leistungspaket zum Upload-Stack hinzu
        /// </summary>
        public static bool ToUploadStack(AppModel model, LeistungPackWSO pack)
        {
            try
            {
                if (model == null || pack == null)
                {
                    AppModel.Logger?.Error("ToUploadStack LeistungPackWSO: model or pack is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/worksupload/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{pack.startticks}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(pack, jsonSettings);
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
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack LeistungPackWSO");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der Leistungspakete im Upload-Stack
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/worksupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, "*.ipm").Length;
                }

                return 0;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack LeistungPackWSO");
                return 0;
            }
        }

        /// <summary>
        /// Lädt alle Leistungspakete aus dem Upload-Stack
        /// </summary>
        public static List<LeistungPackWSO> LoadAllFromUploadStack(AppModel model)
        {
            List<LeistungPackWSO> list = new List<LeistungPackWSO>();

            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/worksupload/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            var pack = LoadFromUploadStack(file);
                            if (pack != null)
                            {
                                list.Add(pack);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack LeistungPackWSO");
            }

            return list;
        }

        /// <summary>
        /// Lädt alle Leistungspakete aus dem Upload-Stack als JSON
        /// </summary>
        public static string LoadAllFromUploadStack_AsJson()
        {
            try
            {
                var list = LoadAllFromUploadStack(AppModel.Instance);
                return JsonConvert.SerializeObject(list, Formatting.Indented);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack_AsJson LeistungPackWSO");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        /// <summary>
        /// Lädt ein einzelnes Leistungspaket aus dem Upload-Stack
        /// </summary>
        private static LeistungPackWSO LoadFromUploadStack(string filename)
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

                return JsonConvert.DeserializeObject<LeistungPackWSO>(jsonString, jsonSettings);
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: LoadFromUploadStack LeistungPackWSO - {filename}");
                return null;
            }
        }

        /// <summary>
        /// Löscht ein Leistungspaket aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(AppModel model, LeistungPackWSO pack)
        {
            try
            {
                if (pack == null || model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/worksupload/" + pack.startticks + ".ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack LeistungPackWSO");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Leistungspakete aus dem Upload-Stack
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
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/worksupload/"
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
                AppModel.Logger?.Error(ex, "ERROR: ClearUploadStack LeistungPackWSO");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gibt das Start-Datum als DateTime zurück
        /// </summary>
        public DateTime GetStartDateTime()
        {
            return new DateTime(startticks);
        }

        /// <summary>
        /// Setzt das Start-Datum von einem DateTime
        /// </summary>
        public void SetStartDateTime(DateTime dateTime)
        {
            startticks = dateTime.Ticks;
        }

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
        /// Berechnet die Dauer des Leistungspakets
        /// </summary>
        public TimeSpan GetDuration()
        {
            if (endticks > 0 && startticks > 0)
            {
                return new TimeSpan(endticks - startticks);
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Prüft ob GPS Check-In Daten vorhanden sind
        /// </summary>
        public bool HasCheckInGPS()
        {
            return !string.IsNullOrWhiteSpace(latin) &&
                   !string.IsNullOrWhiteSpace(lonin) &&
                   messagein != "Kein GPS";
        }

        /// <summary>
        /// Prüft ob GPS Check-Out Daten vorhanden sind
        /// </summary>
        public bool HasCheckOutGPS()
        {
            return !string.IsNullOrWhiteSpace(latout) &&
                   !string.IsNullOrWhiteSpace(lonout) &&
                   messageout != "Kein GPS";
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
        /// Prüft ob das Leistungspaket gültig ist
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(guid) &&
                   startticks > 0 &&
                   personid > 0 &&
                   leistungen != null;
        }

        /// <summary>
        /// Gibt die Anzahl der Leistungen zurück
        /// </summary>
        public int GetLeistungenCount()
        {
            return leistungen?.Count ?? 0;
        }

        /// <summary>
        /// Prüft ob das Paket Leistungen enthält
        /// </summary>
        public bool HasLeistungen()
        {
            return leistungen != null && leistungen.Count > 0;
        }

        /// <summary>
        /// Prüft ob es sich um einen Winterdienst handelt
        /// </summary>
        public bool IsWinterservice()
        {
            return winterservice > 0;
        }

        #endregion
    }
}