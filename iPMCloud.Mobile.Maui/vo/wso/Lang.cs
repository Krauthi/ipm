using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Observable Collection für gruppierte Lang-Items
    /// </summary>
    public class ObservableLangItemCollection<K, T> : ObservableCollection<T>
    {
        private readonly K _key;

        public ObservableLangItemCollection(IGrouping<K, T> group)
            : base(group)
        {
            _key = group.Key;
        }

        public K Key
        {
            get { return _key; }
        }
    }

    /// <summary>
    /// Repräsentiert Spracheinstellungen
    /// </summary>
    public class Lang
    {
        public string text { get; set; } = "Deutsch (Standard)";
        public string lang { get; set; } = "de";
        public string last { get; set; } = "0";

        public Lang() { }

        public Lang(string text, string lang)
        {
            this.text = text;
            this.lang = lang;
            this.last = DateTime.Now.Ticks.ToString();
        }

        #region Save/Load/Delete Methods

        /// <summary>
        /// Speichert die Spracheinstellungen
        /// </summary>
        public static bool Save(Lang pn)
        {
            try
            {
                if (pn == null)
                {
                    AppModel.Logger?.Error("Save Lang: pn is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save Lang: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "lang.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(pn, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save Lang");
                return false;
            }
        }

        /// <summary>
        /// Lädt die Spracheinstellungen
        /// </summary>
        public static Lang Load()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return new Lang();
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "lang.ipm");

                if (File.Exists(filePath))
                {
                    try
                    {
                        // JSON laden
                        string jsonString = File.ReadAllText(filePath);

                        if (string.IsNullOrWhiteSpace(jsonString))
                        {
                            return new Lang();
                        }

                        var jsonSettings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Include,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        Lang pn = JsonConvert.DeserializeObject<Lang>(jsonString, jsonSettings);
                        return pn ?? new Lang();
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON Fehler - könnte alte BinaryFormatter Datei sein
                        AppModel.Logger?.Warn(jsonEx, "Failed to deserialize Lang JSON, attempting migration");

                        if (TryMigrateLegacyLang(filePath, out Lang migratedLang))
                        {
                            // Nach erfolgreicher Migration neu speichern
                            Save(migratedLang);
                            return migratedLang;
                        }

                        return new Lang();
                    }
                }
                else
                {
                    return new Lang();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load Lang");
                return new Lang();
            }
        }

        private static bool TryMigrateLegacyLang(string filePath, out Lang lang)
        {
            lang = null;

            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    AppModel.Logger?.Info($"Legacy Lang file backed up to: {backupPath}");
                }

                // In .NET MAUI kann BinaryFormatter nicht mehr verwendet werden
                // Die alte Datei wird gesichert und Standard-Lang zurückgegeben
                lang = new Lang();
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacyLang()");
                return false;
            }
        }

        /// <summary>
        /// Lädt die Spracheinstellungen als JSON-String
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/lang.ipm"
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
                AppModel.Logger?.Error(ex, "ERROR: Load_AsJson Lang");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        /// <summary>
        /// Löscht die Spracheinstellungen
        /// </summary>
        public static bool Delete(string id = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/lang/lang.ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

                    AppModel.Logger?.Info($"Lang deleted. Backup: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete Lang");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob die Spracheinstellung gültig ist
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(text) &&
                   !string.IsNullOrWhiteSpace(lang);
        }

        /// <summary>
        /// Setzt den Last-Timestamp auf jetzt
        /// </summary>
        public void UpdateLastTimestamp()
        {
            last = DateTime.Now.Ticks.ToString();
        }

        /// <summary>
        /// Gibt den Last-Timestamp als DateTime zurück
        /// </summary>
        public DateTime? GetLastDateTime()
        {
            if (long.TryParse(last, out long ticks) && ticks > 0)
            {
                return new DateTime(ticks);
            }
            return null;
        }

        /// <summary>
        /// Erstellt eine Kopie der Spracheinstellung
        /// </summary>
        public Lang Clone()
        {
            return new Lang
            {
                text = this.text,
                lang = this.lang,
                last = this.last
            };
        }

        /// <summary>
        /// Gibt eine formatierte Beschreibung zurück
        /// </summary>
        public override string ToString()
        {
            return $"{text} ({lang})";
        }

        #endregion

        #region Predefined Languages

        /// <summary>
        /// Vordefinierte Sprachen
        /// </summary>
        public static class Languages
        {
            public static Lang German => new Lang("Deutsch (Standard)", "de");
            public static Lang English => new Lang("English", "en");
            public static Lang French => new Lang("Français", "fr");
            public static Lang Spanish => new Lang("Español", "es");
            public static Lang Italian => new Lang("Italiano", "it");
            public static Lang Portuguese => new Lang("Português", "pt");
            public static Lang Dutch => new Lang("Nederlands", "nl");
            public static Lang Polish => new Lang("Polski", "pl");
            public static Lang Turkish => new Lang("Türkçe", "tr");
            public static Lang Russian => new Lang("Русский", "ru");
            public static Lang Chinese => new Lang("中文", "zh");
            public static Lang Japanese => new Lang("日本語", "ja");
            public static Lang Korean => new Lang("한국어", "ko");
            public static Lang Arabic => new Lang("العربية", "ar");

            /// <summary>
            /// Gibt alle verfügbaren Sprachen zurück
            /// </summary>
            public static System.Collections.Generic.List<Lang> GetAll()
            {
                return new System.Collections.Generic.List<Lang>
                {
                    German,
                    English,
                    French,
                    Spanish,
                    Italian,
                    Portuguese,
                    Dutch,
                    Polish,
                    Turkish,
                    Russian,
                    Chinese,
                    Japanese,
                    Korean,
                    Arabic
                };
            }

            /// <summary>
            /// Findet eine Sprache anhand des Language-Codes
            /// </summary>
            public static Lang FindByCode(string langCode)
            {
                var all = GetAll();
                return all.FirstOrDefault(l => l.lang.Equals(langCode, StringComparison.OrdinalIgnoreCase))
                       ?? German;
            }
        }

        #endregion
    }
}