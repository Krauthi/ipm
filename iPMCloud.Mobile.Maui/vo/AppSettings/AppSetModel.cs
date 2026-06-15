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
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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

                if (!File.Exists(FilePath))
                {
                    // Datei existiert nicht - neue Instanz erstellen
                    AppModel.Instance.AppSetModel = new AppSetModel();
                    return;
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                if (PersistedJsonMigration.TryLoadWithLegacyMigration(
                    FilePath,
                    jsonSettings,
                    "Load AppSet",
                    migratedAppSet =>
                    {
                        AppModel.Instance.AppSetModel = migratedAppSet;
                        return Save();
                    },
                    out AppSetModel appSetModel))
                {
                    AppModel.Instance.AppSetModel = appSetModel;
                }
                else
                {
                    // Both JSON load and legacy migration failed.
                    // Determine whether the file is empty or contains unrecognized/corrupted data.
                    long fileSize = new FileInfo(FilePath).Length;
                    if (fileSize == 0)
                    {
                        AppModel.Logger?.Warn($"Load AppSet: File is empty, resetting to defaults - {FilePath}");
                    }
                    else
                    {
                        // File has content but cannot be read - back it up and start fresh.
                        AppModel.Logger?.Warn(
                            $"Load AppSet: File is corrupted or uses an unsupported format " +
                            $"(size={fileSize} bytes). Backing up and resetting to defaults - {FilePath}");
                        TryBackupCorruptedFile();
                    }

                    AppModel.Instance.AppSetModel = new AppSetModel();
                    Save(); // Persist a clean default so next launch succeeds without warnings.
                    AppModel.Logger?.Info($"Load AppSet: Recovered with default AppSet, clean file written - {FilePath}");
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

        private static void TryBackupCorruptedFile()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return;

                string backupPath = FilePath + $".corrupted_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}";
                File.Copy(FilePath, backupPath, overwrite: true);
                File.Delete(FilePath);
                AppModel.Logger?.Info($"Load AppSet: Corrupted file backed up to {backupPath}");
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Warn(ex, $"Load AppSet: Failed to back up corrupted file - {FilePath}");
            }
        }
    }
}
