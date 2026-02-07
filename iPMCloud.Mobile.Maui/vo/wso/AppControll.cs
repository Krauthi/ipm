using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.IO;

namespace iPMCloud.Mobile
{
    public class AppControll
    {
        public int id { get; set; }
        public int gruppeid { get; set; }
        public bool scanbmp { get; set; } = true;
        public bool cleansaves { get; set; } = false;
        public bool scanbmpview { get; set; } = true;
        public bool scanbmpviewinperson { get; set; } = false;
        public bool scanbmpviewprotokoll { get; set; } = true;
        public bool showPersonTimes { get; set; } = false;
        public bool showObjektPlans { get; set; } = false;
        public bool showTickets { get; set; } = false;
        public bool showChecks { get; set; } = false;
        public bool filterKategories { get; set; } = false;
        public bool translation { get; set; } = false;
        public string lang { get; set; } = "de";

        /// <summary>
        /// Version im Format: 2.1.43 => 2001043
        /// </summary>
        public int version { get; set; } = 2001050; //2.1.50 [--2],[--1],[-50]

        public bool direktBuchenMuell { get; set; } = true;
        public bool direktBuchenWinter { get; set; } = true;
        public bool direktBuchenPos { get; set; } = false;

        /// <summary>
        /// Standort ins Bild einbetten
        /// </summary>
        public bool imageIncludeLocation { get; set; } = false;

        /// <summary>
        /// PersonWSO Extensions - Kategorie-Filter ignorieren
        /// </summary>
        public bool ignoreKategorieFilterByPerson { get; set; } = false;

        public AppControll() { }

        #region Save/Load/Delete Methods

        /// <summary>
        /// Speichert die AppControll-Einstellungen
        /// </summary>
        public static bool Save(AppModel model, AppControll ac)
        {
            try
            {
                if (model == null || ac == null)
                {
                    AppModel.Logger?.Error("Save AppControll: model or ac is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save AppControll: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "appcontroll.ipm");

                // JSON Serialisierung
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(ac, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save AppControll");
                return false;
            }
        }

        /// <summary>
        /// Lädt die AppControll-Einstellungen
        /// </summary>
        public static AppControll Load(AppModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return new AppControll();
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "appcontroll.ipm");

                if (File.Exists(filePath))
                {
                    try
                    {
                        // JSON laden
                        string jsonString = File.ReadAllText(filePath);

                        if (string.IsNullOrWhiteSpace(jsonString))
                        {
                            return new AppControll();
                        }

                        var jsonSettings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Include,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        AppControll ac = JsonConvert.DeserializeObject<AppControll>(jsonString, jsonSettings);
                        return ac ?? new AppControll();
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON Fehler - könnte alte BinaryFormatter Datei sein
                        AppModel.Logger?.Warn(jsonEx, "Failed to deserialize AppControll JSON, attempting migration");

                        if (TryMigrateLegacyAppControll(filePath, out AppControll migratedAc))
                        {
                            // Nach erfolgreicher Migration neu speichern
                            Save(model, migratedAc);
                            return migratedAc;
                        }

                        return new AppControll();
                    }
                }
                else
                {
                    return new AppControll();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load AppControll");
                return new AppControll();
            }
        }

        private static bool TryMigrateLegacyAppControll(string filePath, out AppControll appControll)
        {
            appControll = null;

            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    AppModel.Logger?.Info($"Legacy AppControll file backed up to: {backupPath}");
                }

                // In .NET MAUI kann BinaryFormatter nicht mehr verwendet werden
                // Die alte Datei wird gesichert und muss manuell konvertiert werden
                appControll = new AppControll();
                return true; // Gib neue leere AppControll zurück
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacyAppControll()");
                return false;
            }
        }

        /// <summary>
        /// Lädt die AppControll-Einstellungen als JSON-String
        /// </summary>
        public static string Load_AsJson()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return "{\"Error\": \"CustomerNumber not set\"}";
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "appcontroll.ipm");

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        return "{\"Error\": \"File is empty\"}";
                    }

                    // JSON ist bereits vorhanden, einfach zurückgeben
                    // Optional: Neu formatieren
                    var ac = JsonConvert.DeserializeObject<AppControll>(jsonString);
                    return JsonConvert.SerializeObject(ac, Formatting.Indented);
                }
                else
                {
                    return "{\"Info\": \"File not exist\"}";
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load_AsJson AppControll");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        /// <summary>
        /// Löscht die AppControll-Einstellungen
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
                    "ipm/" + model.SettingModel.SettingDTO.CustomerNumber + "/appcontroll/appcontroll.ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

                    AppModel.Logger?.Info($"AppControll deleted. Backup: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete AppControll");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Erstellt Standard-AppControll-Einstellungen
        /// </summary>
        public static AppControll CreateDefault()
        {
            return new AppControll
            {
                scanbmp = true,
                scanbmpview = true,
                scanbmpviewprotokoll = true,
                direktBuchenMuell = true,
                direktBuchenWinter = true,
                lang = "de",
                version = 2001050
            };
        }

        /// <summary>
        /// Setzt alle Einstellungen auf Standard zurück
        /// </summary>
        public void ResetToDefaults()
        {
            scanbmp = true;
            cleansaves = false;
            scanbmpview = true;
            scanbmpviewinperson = false;
            scanbmpviewprotokoll = true;
            showPersonTimes = false;
            showObjektPlans = false;
            showTickets = false;
            showChecks = false;
            filterKategories = false;
            translation = false;
            lang = "de";
            version = 2001050;
            direktBuchenMuell = true;
            direktBuchenWinter = true;
            direktBuchenPos = false;
            imageIncludeLocation = false;
            ignoreKategorieFilterByPerson = false;
        }

        /// <summary>
        /// Kopiert Einstellungen von einer anderen AppControll-Instanz
        /// </summary>
        public void CopyFrom(AppControll source)
        {
            if (source == null) return;

            id = source.id;
            gruppeid = source.gruppeid;
            scanbmp = source.scanbmp;
            cleansaves = source.cleansaves;
            scanbmpview = source.scanbmpview;
            scanbmpviewinperson = source.scanbmpviewinperson;
            scanbmpviewprotokoll = source.scanbmpviewprotokoll;
            showPersonTimes = source.showPersonTimes;
            showObjektPlans = source.showObjektPlans;
            showTickets = source.showTickets;
            showChecks = source.showChecks;
            filterKategories = source.filterKategories;
            translation = source.translation;
            lang = source.lang;
            version = source.version;
            direktBuchenMuell = source.direktBuchenMuell;
            direktBuchenWinter = source.direktBuchenWinter;
            direktBuchenPos = source.direktBuchenPos;
            imageIncludeLocation = source.imageIncludeLocation;
            ignoreKategorieFilterByPerson = source.ignoreKategorieFilterByPerson;
        }

        /// <summary>
        /// Gibt die Version als lesbaren String zurück (z.B. "2.1.50")
        /// </summary>
        public string GetVersionString()
        {
            int major = version / 1000000;
            int minor = (version / 1000) % 1000;
            int patch = version % 1000;
            return $"{major}.{minor}.{patch}";
        }

        /// <summary>
        /// Setzt die Version aus einem String (z.B. "2.1.50")
        /// </summary>
        public void SetVersionFromString(string versionString)
        {
            try
            {
                var parts = versionString.Split('.');
                if (parts.Length == 3)
                {
                    int major = int.Parse(parts[0]);
                    int minor = int.Parse(parts[1]);
                    int patch = int.Parse(parts[2]);
                    version = (major * 1000000) + (minor * 1000) + patch;
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: SetVersionFromString({versionString})");
            }
        }

        #endregion
    }
}