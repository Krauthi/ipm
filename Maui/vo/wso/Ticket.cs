using iPMCloud.Mobile.vo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Repräsentiert eine Person im Ticket-System (vereinfacht für Mobile)
    /// </summary>
    public class TicketPerson
    {
        public Int32 id { get; set; } = 0;
        public int rolle { get; set; } = 0;
        public string anrede { get; set; } = "";
        public string firma { get; set; } = "";
        public string vorname { get; set; } = "";
        public string name { get; set; } = "";
        public string mobile { get; set; } = "";
        public string telefon { get; set; } = "";
        public string mail { get; set; } = "";
        public Int32 personid { get; set; } = 0;
        public byte[] userIcon { get; set; } = null;

        public TicketPerson() { }

        /// <summary>
        /// Gibt den vollständigen Namen zurück
        /// </summary>
        public string GetFullName()
        {
            return anrede == "Firma" ? firma : $"{vorname} {name}".Trim();
        }
    }

    /// <summary>
    /// Repräsentiert ein Objekt im Ticket-System
    /// </summary>
    public class TicketObjekt
    {
        public Int32 id { get; set; } = 0;
        public Int32 personid { get; set; } = 0;
        public string objektnr { get; set; } = "";
        public string objektname { get; set; } = "";
        public string type { get; set; } = "";
        public string status { get; set; } = "";
        public string adresse { get; set; } = "";
        public string plz { get; set; } = "";
        public string ort { get; set; } = "";
        public int del { get; set; } = 0;

        public TicketObjekt() { }

        /// <summary>
        /// Gibt die vollständige Adresse zurück
        /// </summary>
        public string GetFullAddress()
        {
            return $"{adresse} {plz} {ort}".Trim();
        }
    }

    /// <summary>
    /// Repräsentiert eine Chat-Nachricht im Ticket-Verlauf
    /// </summary>
    public class TicketChat
    {
        public Int32 id { get; set; } = 0;
        public Int32 ticketid { get; set; } = 0;
        public Int32 personid { get; set; } = 0;
        public string typ { get; set; } = "info";
        /*  
            typ = 
                'new' - Ticket erstellt
                'info' - Info-Nachricht
                'mailto' - Mail gesendet
                'notificationto' - Benachrichtigung gesendet
                'statuschange' - Status geändert
                'besitzerchange' - Besitzer geändert
                'rueckfrage' - Rückfrage
                'erledigt' - Erledigt
                'geschlossen' - Geschlossen
         */
        public string t { get; set; } = ""; // Text/Nachricht
        public int del { get; set; } = 0;
        public string personname { get; set; } = "";
        public string info { get; set; } = "";
        public string updateat { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public bool intern { get; set; } = true;

        public TicketChat() { }

        public TicketChat(int ticketid, int personid, string personname, string text)
        {
            this.ticketid = ticketid;
            this.personid = personid;
            this.personname = personname;
            this.t = text;
            this.updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Gibt formatierte Zeit zurück (z.B. "14:30" oder "Gestern 14:30")
        /// </summary>
        public string GetFormattedTime()
        {
            if (DateTime.TryParse(updateat, out DateTime datum))
            {
                var now = DateTime.Now;
                var diff = now - datum;

                if (diff.TotalDays < 1 && now.Date == datum.Date)
                {
                    return datum.ToString("HH:mm");
                }
                else if (diff.TotalDays < 2 && now.Date.AddDays(-1) == datum.Date)
                {
                    return $"Gestern {datum:HH:mm}";
                }
                else if (diff.TotalDays < 7)
                {
                    return datum.ToString("dddd HH:mm");
                }
                else
                {
                    return datum.ToString("dd.MM.yyyy HH:mm");
                }
            }
            return updateat;
        }

        /// <summary>
        /// Konvertiert zu DateTime
        /// </summary>
        public DateTime GetDateTime()
        {
            if (DateTime.TryParse(updateat, out DateTime result))
            {
                return result;
            }
            return DateTime.Now;
        }
    }

    /// <summary>
    /// Repräsentiert ein Ticket im System (angepasst an Backend-Struktur)
    /// </summary>
    public class Ticket
    {
        public enum TicketStatus
        {
            Neu = 1,            // Neu (noch nicht zugewiesen)
            Offen = 2,          // Offen (zugewiesen)
            Wartend = 3,        // Wartend
            InArbeit = 4,       // In Arbeit
            Rueckfrage = 5,     // Rückfrage
            Erledigt = 9,       // Erledigt / Rechnung freigeben
            Geschlossen = 10    // Geschlossen
        }

        public enum BesitzerStatus
        {
            NochNichtGesehen = -1,  // Noch nicht gesehen
            Gesehen = 0,            // Gesehen/Geöffnet
            Gestartet = 1,          // Gestartet/In Arbeit
            Rueckfrage = 2,         // Rückfrage
            Erledigt = 9            // Erledigt
        }

        // IDs
        public Int32 id { get; set; } = 0;
        public Int32 gruppeid { get; set; } = 0;
        public Int32 personid { get; set; } = 0;
        public Int32 besitzerid { get; set; } = 0;
        public Int32 erstellerid { get; set; } = 0;
        public Int32 aspid { get; set; } = 0;
        public Int32 objektid { get; set; } = 0;
        public Int32 auftragid { get; set; } = 0;

        // Texte
        public string text { get; set; } = "";          // Beschreibung (im Backend als BLOB)
        public string titel { get; set; } = "";

        // Datumsangaben (als Unix-Timestamp-Strings)
        public string start { get; set; } = "0";        // Erstelldatum
        public string end { get; set; } = "0";          // Enddatum
        public string startab { get; set; } = "0";      // Start ab (Zeitfenster)
        public string endbis { get; set; } = "0";       // Ende bis (Zeitfenster)
        public string updateat { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string lastbesitzerupdate { get; set; } = null;

        // Status und Flags
        public int status { get; set; } = 1;            // Ticket-Status (1=Neu, 2=Offen, etc.)
        public int besitzerstatus { get; set; } = -1;   // Status beim Besitzer (-1=Noch nicht gesehen)
        public int del { get; set; } = 0;               // Löschen-Flag (0=aktiv, 1=gelöscht, 5=vom Kunde gelöscht)
        public bool intern { get; set; } = true;        // Intern/Extern
        public int prio { get; set; } = 1;              // Priorität

        // Objekt-Referenzen (werden vom Backend gefüllt)
        public TicketPerson kunde { get; set; }
        public string kundename { get; set; }
        public TicketPerson besitzer { get; set; }
        public string besitzername { get; set; }
        public TicketPerson ersteller { get; set; }
        public string erstellername { get; set; }
        public TicketPerson asp { get; set; }
        public TicketObjekt objekt { get; set; }
        public string objektname { get; set; }

        // Chat und Aufträge
        public TicketChat newchat { get; set; } = new TicketChat();
        public List<TicketChat> chats { get; set; } = new List<TicketChat>();

        // Legacy-Properties (für Kompatibilität mit bestehendem Mobile-Code)
        [JsonIgnore]
        public DateTime? datum
        {
            get => ConvertStringToDateTime(start);
            set => start = value.HasValue ? ConvertDateTimeToString(value.Value) : "0";
        }

        [JsonIgnore]
        public DateTime? letztesAenderungsDatum
        {
            get
            {
                if (DateTime.TryParse(updateat, out DateTime result))
                    return result;
                return null;
            }
            set => updateat = value?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [JsonIgnore]
        public string beschreibung
        {
            get => text;
            set => text = value;
        }

        public Ticket()
        {
            updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            start = ConvertDateTimeToString(DateTime.Now);
        }

        public Ticket(string titel, string beschreibung, int erstellerId, string erstellerName)
        {
            this.titel = titel;
            this.text = beschreibung;
            this.erstellerid = erstellerId;
            this.erstellername = erstellerName;
            this.start = ConvertDateTimeToString(DateTime.Now);
            this.updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }







        public static async Task<TicketResponse> LoadTicketsFromBackendAsync()
        {
            try
            {
                AppModel.Instance.TicketResponse
                    = await AppModel.Instance.Connections.GetTickets();

                if (AppModel.Instance.TicketResponse.success && AppModel.Instance.TicketResponse.tickets != null)
                {
                    // Badge-Count vom Backend verwenden, falls verfügbar
                    if (AppModel.Instance.TicketResponse.counts != null && AppModel.Instance.TicketResponse.counts.Count > 0)
                    {
                        int totalCount = AppModel.Instance.TicketResponse.counts.Sum();
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            AppModel.Instance.MainPage.UpdateTicketBadgeCount(totalCount);
                        });
                    }

                    return AppModel.Instance.TicketResponse;
                }
                else
                {
                    AppModel.Logger.Warn($"LoadTicketsFromBackendAsync: {AppModel.Instance.TicketResponse.message}");
                    return new TicketResponse();
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error($"LoadTicketsFromBackendAsync: {ex.Message}");
                return new TicketResponse();
            }
        }






        #region DateTime Conversion Helper

        /// <summary>
        /// Konvertiert einen Timestamp-String in DateTime (Unix-Timestamp in Millisekunden)
        /// </summary>
        private DateTime? ConvertStringToDateTime(string timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestamp) || timestamp == "0" || timestamp == "-1")
                return null;

            if (long.TryParse(timestamp, out long ticks))
            {
                try
                {
                    // Unix-Timestamp (Millisekunden seit 1.1.1970)
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    return epoch.AddMilliseconds(ticks).ToLocalTime();
                }
                catch
                {
                    return null;
                }
            }

            // Fallback: Versuche direktes DateTime-Parsing
            if (DateTime.TryParse(timestamp, out DateTime result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Konvertiert ein DateTime in einen Timestamp-String (Unix-Timestamp in Millisekunden)
        /// </summary>
        private string ConvertDateTimeToString(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan span = dateTime.ToUniversalTime() - epoch;
            return ((long)span.TotalMilliseconds).ToString();
        }

        #endregion

        #region Status Helper Methods

        /// <summary>
        /// Gibt den Status als lesbaren Text zurück
        /// </summary>
        public string GetStatusText()
        {
            return ((TicketStatus)status) switch
            {
                TicketStatus.Neu => "NEU",
                TicketStatus.Offen => "OFFEN",
                TicketStatus.Wartend => "WARTEND",
                TicketStatus.InArbeit => "IN ARBEIT",
                TicketStatus.Rueckfrage => "RÜCKFRAGE",
                TicketStatus.Erledigt => "ERLEDIGT",
                TicketStatus.Geschlossen => "GESCHLOSSEN",
                _ => "*"
            };
        }

        /// <summary>
        /// Gibt den Besitzerstatus als lesbaren Text zurück
        /// </summary>
        public string GetBesitzerStatusText()
        {
            return ((BesitzerStatus)besitzerstatus) switch
            {
                BesitzerStatus.NochNichtGesehen => "Noch nicht gesehen",
                BesitzerStatus.Gesehen => "Gesehen",
                BesitzerStatus.Gestartet => "Gestartet",
                BesitzerStatus.Rueckfrage => "Rückfrage",
                BesitzerStatus.Erledigt => "Erledigt",
                _ => "Unbekannt"
            };
        }

        /// <summary>
        /// Gibt den aktuellen Status zurück
        /// </summary>
        public TicketStatus GetCurrentStatus()
        {
            if (Enum.IsDefined(typeof(TicketStatus), status))
            {
                return (TicketStatus)status;
            }
            return TicketStatus.Neu;
        }

        #endregion

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
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ipm/" + AppModel.Instance.SettingModel.SettingDTO.CustomerNumber + "/ticket/"
                );

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{id}.ipm");

                if (File.Exists(filePath))
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    };

                    string jsonString = File.ReadAllText(filePath);
                    if (string.IsNullOrWhiteSpace(jsonString))
                        return null;
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
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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

        #region Chat Helper Methods

        /// <summary>
        /// Fügt eine neue Chat-Nachricht zum Ticket hinzu
        /// </summary>
        public void AddChatMessage(int personid, string personname, string nachricht, string typ = "info", bool intern = true)
        {
            var chat = new TicketChat
            {
                ticketid = this.id,
                personid = personid,
                personname = personname,
                t = nachricht,
                typ = typ,
                intern = intern,
                updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            chats.Add(chat);
            this.updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Legacy-Methode für Kompatibilität (verwendet AddChatMessage)
        /// </summary>
        public void AddMessage(int absenderId, string absenderName, string nachricht)
        {
            AddChatMessage(absenderId, absenderName, nachricht, "info", true);
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
        /// Weist das Ticket einem neuen Besitzer zu (Backend-kompatibel)
        /// </summary>
        public void AssignTo(int newBesitzerId, string newBesitzerName)
        {
            this.besitzerid = newBesitzerId;
            this.besitzername = newBesitzerName;
            this.besitzerstatus = -1; // Noch nicht gesehen
            this.updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Optional: Chat-Eintrag für die Zuweisung
            AddChatMessage(
                personid: newBesitzerId,
                personname: newBesitzerName,
                nachricht: $"Ticket wurde {newBesitzerName} zugewiesen",
                typ: "besitzerchange",
                intern: true
            );
        }

        /// <summary>
        /// Ändert den Status des Tickets (Backend-kompatibel)
        /// </summary>
        public void ChangeStatus(TicketStatus newStatus, string changeReason = "")
        {
            int oldStatus = this.status;
            this.status = (int)newStatus;
            this.updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Optional: Chat-Eintrag für die Statusänderung
            if (!string.IsNullOrEmpty(changeReason))
            {
                AddChatMessage(
                    personid: this.besitzerid,
                    personname: this.besitzername ?? "System",
                    nachricht: changeReason,
                    typ: "statuschange",
                    intern: true
                );
            }
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
            return $"Ticket #{id}: {titel} [{status.ToString()}] ({GetFormattedAge()})";
        }

        #endregion

        #region Filter/Query Methods

        /// <summary>
        /// Lädt Tickets nach Status (Backend-kompatibel)
        /// </summary>
        public static List<Ticket> LoadByStatus(TicketStatus status)
        {
            return LoadAll().Where(t => t.status == (int)status).ToList();
        }

        /// <summary>
        /// Lädt Tickets eines bestimmten Besitzers (Backend-kompatibel)
        /// </summary>
        public static List<Ticket> LoadByOwner(int besitzerId)
        {
            return LoadAll().Where(t => t.besitzerid == besitzerId).ToList();
        }

        /// <summary>
        /// Lädt Tickets eines bestimmten Erstellers (Backend-kompatibel)
        /// </summary>
        public static List<Ticket> LoadByCreator(int erstellerId)
        {
            return LoadAll().Where(t => t.erstellerid == erstellerId).ToList();
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
