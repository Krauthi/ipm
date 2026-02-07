using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert eine Person im Ticket-Verlauf
    /// </summary>
    public class TicketPerson
    {
        public enum PersonStatus
        {
            Abgelehnt,
            Angenommen,
            Erledigt,
            Weitergeleitet,
            InArbeit,
            Gesehen,
            Neu,
            Kein
        }

        public int besitzerId { get; set; } = 0;
        public string besitzerName { get; set; } = string.Empty;
        public DateTime? aenderungsDatum { get; set; } = null;
        public DateTime? zugewiesenDatum { get; set; } = null;
        public PersonStatus status { get; set; } = PersonStatus.Kein;

        public TicketPerson() { }

        public TicketPerson(int besitzerId, string besitzerName, PersonStatus status)
        {
            this.besitzerId = besitzerId;
            this.besitzerName = besitzerName;
            this.status = status;
            this.zugewiesenDatum = DateTime.Now;
            this.aenderungsDatum = DateTime.Now;
        }

        /// <summary>
        /// Gibt den Status als lesbaren Text zurück
        /// </summary>
        public string GetStatusText()
        {
            return status switch
            {
                PersonStatus.Abgelehnt => "Abgelehnt",
                PersonStatus.Angenommen => "Angenommen",
                PersonStatus.Erledigt => "Erledigt",
                PersonStatus.Weitergeleitet => "Weitergeleitet",
                PersonStatus.InArbeit => "In Arbeit",
                PersonStatus.Gesehen => "Gesehen",
                PersonStatus.Neu => "Neu",
                PersonStatus.Kein => "Kein Status",
                _ => "Unbekannt"
            };
        }
    }

    /// <summary>
    /// Repräsentiert ein Ticket im System
    /// </summary>
    public class Ticket
    {
        public enum TicketStatus
        {
            Abgelehnt,
            Angenommen,
            Erledigt,
            Weitergeleitet,
            InArbeit,
            Gesehen,
            Neu,
            Kein
        }

        public int id { get; set; }
        public DateTime? letztesAenderungsDatum { get; set; } = null;
        public DateTime? datum { get; set; } = null; // Erstell Datum
        public string titel { get; set; } = string.Empty;
        public string beschreibung { get; set; } = string.Empty;
        public int erstellerId { get; set; } = 0;
        public string erstellerName { get; set; } = string.Empty;
        public int besitzerId { get; set; } = 0;
        public string besitzerName { get; set; } = string.Empty;
        public List<TicketPerson> histPersons { get; set; } = new List<TicketPerson>();

        public Ticket()
        {
            datum = DateTime.Now;
            letztesAenderungsDatum = DateTime.Now;
        }

        public Ticket(string titel, string beschreibung, int erstellerId, string erstellerName)
        {
            this.titel = titel;
            this.beschreibung = beschreibung;
            this.erstellerId = erstellerId;
            this.erstellerName = erstellerName;
            this.datum = DateTime.Now;
            this.letztesAenderungsDatum = DateTime.Now;
        }

        #region Save/Load/Delete Methods

        /// <summary>
        /// Speichert ein Ticket
        /// </summary>
        public static bool Save(Ticket t)
        {
            try
            {
                if (t == null)
                {
                    AppModel.Logger?.Error("Save Ticket: t is null");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    AppModel.Logger?.Error("Save Ticket: CustomerNumber is null");
                    return false;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{t.id}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat
                };

                string jsonString = JsonConvert.SerializeObject(t, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: Save Ticket");
                return false;
            }
        }

        /// <summary>
        /// Lädt alle Tickets (sortiert nach letzter Änderung absteigend)
        /// </summary>
        public static List<Ticket> LoadAll()
        {
            List<Ticket> list = new List<Ticket>();

            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return list;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(file);

                            if (int.TryParse(fileName, out int ticketId))
                            {
                                var loadedTicket = Load(ticketId);
                                if (loadedTicket != null)
                                {
                                    list.Add(loadedTicket);
                                }
                            }
                        }
                    }

                    // Nach letzter Änderung sortieren (neueste zuerst)
                    list = list.OrderByDescending(d => d.letztesAenderungsDatum).ToList();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAll Ticket");
            }

            return list;
        }

        /// <summary>
        /// Lädt ein spezifisches Ticket anhand der ID
        /// </summary>
        public static Ticket Load(int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return null;
                }

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/"
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

                    return JsonConvert.DeserializeObject<Ticket>(jsonString, jsonSettings);
                }

                return null;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Load Ticket - {id}");
                return null;
            }
        }

        /// <summary>
        /// Löscht ein Ticket
        /// </summary>
        public static bool Delete(int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber))
                {
                    return false;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/" + id + ".ipm"
                );

                if (File.Exists(filePath))
                {
                    // Optional: Backup vor dem Löschen
                    string backupPath = filePath + $".deleted_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(filePath, backupPath, true);

                    File.Delete(filePath);

                    AppModel.Logger?.Info($"Ticket deleted: {id}. Backup: {backupPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, $"ERROR: Delete Ticket - {id}");
                return false;
            }
        }

        /// <summary>
        /// Löscht alle Tickets
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
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/"
                );

                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.ipm");

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    AppModel.Logger?.Info($"All tickets deleted: {files.Length} files");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteAll Ticket");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Prüft ob das Ticket gültig ist
        /// </summary>
        public bool IsValid()
        {
            return id > 0 && !string.IsNullOrWhiteSpace(titel);
        }

        /// <summary>
        /// Gibt den aktuellen Status basierend auf dem Besitzer zurück
        /// </summary>
        public TicketStatus GetCurrentStatus()
        {
            if (histPersons != null && histPersons.Count > 0)
            {
                var lastPerson = histPersons.OrderByDescending(p => p.aenderungsDatum).FirstOrDefault();
                if (lastPerson != null)
                {
                    return (TicketStatus)lastPerson.status;
                }
            }
            return TicketStatus.Neu;
        }

        /// <summary>
        /// Gibt das Alter des Tickets zurück
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
        /// Gibt die Zeit seit der letzten Änderung zurück
        /// </summary>
        public TimeSpan? GetTimeSinceLastChange()
        {
            if (letztesAenderungsDatum.HasValue)
            {
                return DateTime.Now - letztesAenderungsDatum.Value;
            }
            return null;
        }

        /// <summary>
        /// Weist das Ticket einem neuen Besitzer zu
        /// </summary>
        public void AssignTo(int besitzerId, string besitzerName, TicketPerson.PersonStatus status = TicketPerson.PersonStatus.Neu)
        {
            this.besitzerId = besitzerId;
            this.besitzerName = besitzerName;
            this.letztesAenderungsDatum = DateTime.Now;

            var ticketPerson = new TicketPerson(besitzerId, besitzerName, status);
            histPersons.Add(ticketPerson);
        }

        /// <summary>
        /// Ändert den Status des Tickets
        /// </summary>
        public void ChangeStatus(TicketPerson.PersonStatus newStatus)
        {
            if (histPersons.Count > 0)
            {
                var lastPerson = histPersons.OrderByDescending(p => p.aenderungsDatum).FirstOrDefault();
                if (lastPerson != null)
                {
                    lastPerson.status = newStatus;
                    lastPerson.aenderungsDatum = DateTime.Now;
                }
            }

            this.letztesAenderungsDatum = DateTime.Now;
        }

        /// <summary>
        /// Gibt eine formatierte Zeitangabe für das Alter zurück
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
            else
            {
                return datum.Value.ToString("dd.MM.yyyy");
            }
        }

        /// <summary>
        /// Gibt eine Zusammenfassung des Tickets zurück
        /// </summary>
        public override string ToString()
        {
            var status = GetCurrentStatus();
            return $"Ticket #{id}: {titel} [{status}] ({GetFormattedAge()})";
        }

        #endregion

        #region Filter/Query Methods

        /// <summary>
        /// Lädt Tickets nach Status
        /// </summary>
        public static List<Ticket> LoadByStatus(TicketStatus status)
        {
            return LoadAll().Where(t => t.GetCurrentStatus() == status).ToList();
        }

        /// <summary>
        /// Lädt Tickets eines bestimmten Besitzers
        /// </summary>
        public static List<Ticket> LoadByOwner(int besitzerId)
        {
            return LoadAll().Where(t => t.besitzerId == besitzerId).ToList();
        }

        /// <summary>
        /// Lädt Tickets eines bestimmten Erstellers
        /// </summary>
        public static List<Ticket> LoadByCreator(int erstellerId)
        {
            return LoadAll().Where(t => t.erstellerId == erstellerId).ToList();
        }

        /// <summary>
        /// Lädt neue Tickets
        /// </summary>
        public static List<Ticket> LoadNew()
        {
            return LoadByStatus(TicketStatus.Neu);
        }

        /// <summary>
        /// Lädt Tickets in Arbeit
        /// </summary>
        public static List<Ticket> LoadInProgress()
        {
            return LoadByStatus(TicketStatus.InArbeit);
        }

        /// <summary>
        /// Lädt erledigte Tickets
        /// </summary>
        public static List<Ticket> LoadCompleted()
        {
            return LoadByStatus(TicketStatus.Erledigt);
        }

        /// <summary>
        /// Zählt Tickets nach Status
        /// </summary>
        public static int CountByStatus(TicketStatus status)
        {
            return LoadByStatus(status).Count;
        }

        #endregion
    }
}