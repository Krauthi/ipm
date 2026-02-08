using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;

namespace iPMCloud.Mobile.vo
{
    /// <summary>
    /// Model für App-Einstellungen
    /// </summary>
    public class AppSetModel
    {
        private int _viewOnlyMuell = 0;

        /// <summary>
        /// Letzter Stand für die Anzeige im Plan
        /// 0 = Beides, 1 = Plan, 2 = Müll
        /// </summary>
        [JsonProperty("viewOnlyMuell")]
        public int ViewOnlyMuell
        {
            get { return _viewOnlyMuell; }
            set
            {
                if (_viewOnlyMuell != value)
                {
                    _viewOnlyMuell = value;
                    AppSet.Save(); // Auto-Save bei Änderung
                }
            }
        }

        public AppSetModel() { }

        /// <summary>
        /// Gibt den View-Mode als Text zurück
        /// </summary>
        public string GetViewModeText()
        {
            return ViewOnlyMuell switch
            {
                0 => "Beides (Plan & Müll)",
                1 => "Nur Plan",
                2 => "Nur Müll",
                _ => "Unbekannt"
            };
        }

        /// <summary>
        /// Prüft ob Plan angezeigt werden soll
        /// </summary>
        public bool ShowPlan()
        {
            return ViewOnlyMuell == 0 || ViewOnlyMuell == 1;
        }

        /// <summary>
        /// Prüft ob Müll angezeigt werden soll
        /// </summary>
        public bool ShowMuell()
        {
            return ViewOnlyMuell == 0 || ViewOnlyMuell == 2;
        }

        /// <summary>
        /// Setzt den View-Mode
        /// </summary>
        public void SetViewMode(ViewMode mode)
        {
            ViewOnlyMuell = (int)mode;
        }

        /// <summary>
        /// Gibt den aktuellen View-Mode zurück
        /// </summary>
        public ViewMode GetViewMode()
        {
            return (ViewMode)ViewOnlyMuell;
        }

        public enum ViewMode
        {
            Both = 0,
            PlanOnly = 1,
            MuellOnly = 2
        }
    }

    /// <summary>
    /// Persistenz-Klasse für App-Einstellungen
    /// </summary>
    public static class AppSet
    {
        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ipm/appset/"
        );
        private static readonly string FilePath = Path.Combine(DirectoryPath, "set.ipm");

        /// <summary>
        /// Speichert die App-Einstellungen
        /// </summary>
        public static bool Save()
        {
            try
            {
                if (AppModel.Instance?.AppSetModel == null)
                {
                    AppModel.Logger?.Error("Save AppSet: AppSetModel is null");
                    return false;
                }

                if (!Directory.Exists(DirectoryPath))
                {
                    Directory.CreateDirectory(DirectoryPath);
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };

                string jsonString = JsonConvert.SerializeObject(AppModel.Instance.AppSetModel, jsonSettings);
                File.WriteAllText(FilePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save AppSet");
                Console.WriteLine($"Error saving AppSet: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lädt die App-Einstellungen
        /// </summary>
        public static void Load()
        {
            try
            {
                if (!Directory.Exists(DirectoryPath))
                {
                    Directory.CreateDirectory(DirectoryPath);
                }

                if (File.Exists(FilePath))
                {
                    try
                    {
                        string jsonString = File.ReadAllText(FilePath);

                        if (!string.IsNullOrWhiteSpace(jsonString))
                        {
                            var jsonSettings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Include,
                                DefaultValueHandling = DefaultValueHandling.Include,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            };

                            var obj = JsonConvert.DeserializeObject<AppSetModel>(jsonString, jsonSettings);

                            if (obj != null)
                            {
                                AppModel.Instance.AppSetModel = obj;
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON Fehler - könnte alte BinaryFormatter Datei sein
                        AppModel.Logger?.Warn(jsonEx, "Failed to deserialize AppSet JSON, attempting migration");

                        if (TryMigrateLegacyAppSet(FilePath, out AppSetModel migratedAppSet))
                        {
                            AppModel.Instance.AppSetModel = migratedAppSet;
                            Save(); // Neu speichern als JSON
                        }
                        else
                        {
                            // Fallback: Neue Instanz erstellen
                            AppModel.Instance.AppSetModel = new AppSetModel();
                        }
                    }
                }
                else
                {
                    // Datei existiert nicht - neue Instanz erstellen
                    AppModel.Instance.AppSetModel = new AppSetModel();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load AppSet");
                Console.WriteLine($"Error loading AppSet: {ex.Message}");

                // Fallback: Neue Instanz
                AppModel.Instance.AppSetModel = new AppSetModel();
            }
        }

        private static bool TryMigrateLegacyAppSet(string filePath, out AppSetModel appSetModel)
        {
            appSetModel = null;

            try
            {
                // Alte Datei sichern
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = filePath + $".old_binary_{timestamp}";

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    AppModel.Logger?.Info($"Legacy AppSet file backed up to: {backupPath}");
                }

                // In .NET MAUI kann BinaryFormatter nicht mehr verwendet werden
                // Die alte Datei wird gesichert und Standard-AppSet zurückgegeben
                appSetModel = new AppSetModel();
                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: TryMigrateLegacyAppSet()");
                return false;
            }
        }

        /// <summary>
        /// Lädt die App-Einstellungen als JSON-String
        /// </summary>
        public static string Load_AsJson()
        {
            try
            {
                if (!Directory.Exists(DirectoryPath))
                {
                    Directory.CreateDirectory(DirectoryPath);
                }

                if (File.Exists(FilePath))
                {
                    string jsonString = File.ReadAllText(FilePath);

                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        // JSON ist bereits vorhanden, einfach zurückgeben
                        // Optional: Neu formatieren
                        var obj = JsonConvert.DeserializeObject<AppSetModel>(jsonString);
                        return JsonConvert.SerializeObject(obj, Formatting.Indented);
                    }

                    return "{\"Info\": \"File is empty\"}";
                }
                else
                {
                    return "{\"Info\": \"File not exist\"}";
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Load_AsJson AppSet");
                return "{\"Error\": \"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        /// <summary>
        /// Löscht die App-Einstellungen
        /// </summary>
        public static bool Delete()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = FilePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(FilePath, backupPath, true);

                    File.Delete(FilePath);

                    AppModel.Logger?.Info($"AppSet deleted. Backup: {backupPath}");

                    // Neue Instanz erstellen
                    AppModel.Instance.AppSetModel = new AppSetModel();
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Delete AppSet");
                return false;
            }
        }

        /// <summary>
        /// Setzt alle Einstellungen auf Standard zurück
        /// </summary>
        public static bool Reset()
        {
            try
            {
                AppModel.Instance.AppSetModel = new AppSetModel();
                return Save();
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Reset AppSet");
                return false;
            }
        }

        /// <summary>
        /// Prüft ob eine Einstellungsdatei existiert
        /// </summary>
        public static bool Exists()
        {
            return File.Exists(FilePath);
        }

        /// <summary>
        /// Gibt den Pfad zur Einstellungsdatei zurück
        /// </summary>
        public static string GetFilePath()
        {
            return FilePath;
        }
    }
}