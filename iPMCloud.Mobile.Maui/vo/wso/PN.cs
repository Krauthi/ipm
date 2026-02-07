using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert eine Push-Benachrichtigung
    /// </summary>
    public class PN
    {
        public string id { get; set; }
        public DateTime? datum { get; set; } = null;
        public string titel { get; set; } = string.Empty;
        public string beschreibung { get; set; } = string.Empty;
        public string data { get; set; } = string.Empty;
        public string status { get; set; } = "Neu";

        public PN()
        {
            id = Guid.NewGuid().ToString();
            datum = DateTime.Now;
        }

        public PN(string titel, string beschreibung, string data = "")
        {
            this.id = Guid.NewGuid().ToString();
            this.datum = DateTime.Now;
            this.titel = titel;
            this.beschreibung = beschreibung;
            this.data = data;
            this.status = "Neu";
        }

        #region Save/Load/Delete Methods

        /// <summary>
        /// Speichert eine Push-Benachrichtigung
        /// </summary>
        public static bool Save(PN pn)
        {
            try
            {
                if (pn == null)
                {
                    AppModel.Logger?.Error("Save PN: pn is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save PN: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{pn.id}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat
                };

                string jsonString = JsonConvert.SerializeObject(pn, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save PN");
                return false;
            }
        }

        /// <summary>
        /// Lädt alle Push-Benachrichtigungen (sortiert nach Datum absteigend)
        /// </summary>
        public static List<PN> LoadAll()
        {
            List<PN> list = new List<PN>();

            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(file);
                            var loadedPN = Load(fileName);

                            if (loadedPN != null)
                            {
                                list.Add(loadedPN);
                            }
                        }
                    }

                    // Nach Datum sortieren (neueste zuerst)
                    list = list.OrderByDescending(d => d.datum).ToList();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAll PN");
            }

            return list;
        }

        /// <summary>
        /// Lädt eine spezifische Push-Benachrichtigung anhand der ID
        /// </summary>
        public static PN Load(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return null;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{id}.ipm");

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
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    };

                    return JsonConvert.DeserializeObject<PN>(jsonString, jsonSettings);
                }

                return null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Load PN - {id}");
                return null;
            }
        }

        /// <summary>
        /// Löscht eine Push-Benachrichtigung
        /// </summary>
        public static bool Delete(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/" + id + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

                    AppModel.Logger?.Info($"PN deleted: {id}. Backup: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Delete PN - {id}");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Push-Benachrichtigungen
        /// </summary>
        public static bool DeleteAll()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/pn/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    AppModel.Logger?.Info($"All PNs deleted: {files.Length} files");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteAll PN");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob die Push-Benachrichtigung gültig ist
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(id) &&
                   !string.IsNullOrWhiteSpace(titel);
        }

        /// <summary>
        /// Prüft ob die Benachrichtigung neu ist
        /// </summary>
        public bool IsNew()
        {
            return status == "Neu";
        }

        /// <summary>
        /// Markiert die Benachrichtigung als gelesen
        /// </summary>
        public void MarkAsRead()
        {
            status = "Gelesen";
        }

        /// <summary>
        /// Markiert die Benachrichtigung als archiviert
        /// </summary>
        public void MarkAsArchived()
        {
            status = "Archiviert";
        }

        /// <summary>
        /// Gibt das Alter der Benachrichtigung zurück
        /// </summary>
        public TimeSpan? GetAge()
        {
            if (datum.HasValue)
            {
                return DateTime.Now - datum.Value;
            }
            return null;
        }

        /// <summary>
        /// Prüft ob die Benachrichtigung älter als X Tage ist
        /// </summary>
        public bool IsOlderThan(int days)
        {
            var age = GetAge();
            return age.HasValue && age.Value.TotalDays > days;
        }

        /// <summary>
        /// Gibt eine formatierte Zeitangabe zurück (z.B. "vor 2 Stunden")
        /// </summary>
        public string GetFormattedAge()
        {
            var age = GetAge();

            if (!age.HasValue)
            {
                return "Unbekannt";
            }

            if (age.Value.TotalMinutes < 1)
            {
                return "Gerade eben";
            }
            else if (age.Value.TotalMinutes < 60)
            {
                return $"vor {(int)age.Value.TotalMinutes} Minute(n)";
            }
            else if (age.Value.TotalHours < 24)
            {
                return $"vor {(int)age.Value.TotalHours} Stunde(n)";
            }
            else if (age.Value.TotalDays < 7)
            {
                return $"vor {(int)age.Value.TotalDays} Tag(en)";
            }
            else if (age.Value.TotalDays < 30)
            {
                return $"vor {(int)(age.Value.TotalDays / 7)} Woche(n)";
            }
            else
            {
                return datum.Value.ToString("dd.MM.yyyy");
            }
        }

        /// <summary>
        /// Gibt eine Zusammenfassung der Benachrichtigung zurück
        /// </summary>
        public override string ToString()
        {
            return $"PN [{status}]: {titel} ({GetFormattedAge()})";
        }

        #endregion

        #region Filter/Query Methods

        /// <summary>
        /// Lädt nur neue Benachrichtigungen
        /// </summary>
        public static List<PN> LoadNew()
        {
            return LoadAll().Where(pn => pn.IsNew()).ToList();
        }

        /// <summary>
        /// Lädt Benachrichtigungen nach Status
        /// </summary>
        public static List<PN> LoadByStatus(string status)
        {
            return LoadAll().Where(pn => pn.status == status).ToList();
        }

        /// <summary>
        /// Lädt Benachrichtigungen der letzten X Tage
        /// </summary>
        public static List<PN> LoadRecent(int days = 7)
        {
            return LoadAll().Where(pn => !pn.IsOlderThan(days)).ToList();
        }

        /// <summary>
        /// Zählt die Anzahl neuer Benachrichtigungen
        /// </summary>
        public static int CountNew()
        {
            return LoadNew().Count;
        }

        /// <summary>
        /// Löscht alte Benachrichtigungen (älter als X Tage)
        /// </summary>
        public static int DeleteOld(int days = 30)
        {
            try
            {
                var allPNs = LoadAll();
                var oldPNs = allPNs.Where(pn => pn.IsOlderThan(days)).ToList();

                int deletedCount = 0;
                foreach (var pn in oldPNs)
                {
                    if (Delete(pn.id))
                    {
                        deletedCount++;
                    }
                }

                AppModel.Logger?.Info($"Deleted {deletedCount} old PNs (older than {days} days)");
                return deletedCount;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteOld PN");
                return 0;
            }
        }

        #endregion
    }
}