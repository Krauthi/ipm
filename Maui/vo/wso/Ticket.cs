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
        public string besitzerStatus { get; set; } = "";
        public string mandantStatus { get; set; } = "";
        public string erstellerStatus { get; set; } = "";
        public string guid { get; set; } = Guid.NewGuid().ToString();

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

        #region Upload Stack Management

        private static string GetUploadStackDirectory()
        {
            string customerNumber = AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber;
            if (string.IsNullOrWhiteSpace(customerNumber))
            {
                return null;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ipm/" + customerNumber + "/ticketchatupload/"
            );
        }

        /// <summary>
        /// Fügt einen TicketChat-Eintrag zum Upload-Stack hinzu (wird später erneut hochgeladen)
        /// </summary>
        public static bool ToUploadStack(TicketChat tc)
        {
            try
            {
                if (tc == null)
                {
                    AppModel.Logger?.Error("ToUploadStack TicketChat: tc is null");
                    return false;
                }

                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null)
                {
                    return false;
                }

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (string.IsNullOrWhiteSpace(tc.guid))
                {
                    tc.guid = Guid.NewGuid().ToString();
                }

                string filePath = Path.Combine(directoryPath, $"{tc.guid}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(tc, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                if (AppModel.Instance?.MainPage != null)
                {
                    AppModel.Instance.SetAllSyncStateSafe();
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack TicketChat");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der TicketChat-Einträge im Upload-Stack
        /// </summary>
        public static int CountFromStackFrom(Int32 customerId)
        {
            try
            {
                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ipm/" + customerId + "/ticketchatupload/"
                );
                if (directoryPath == null || !Directory.Exists(directoryPath))
                {
                    return 0;
                }

                return Directory.GetFiles(directoryPath, "*.ipm").Length;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStackFrom TicketChat");
                return 0;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der TicketChat-Einträge im Upload-Stack
        /// </summary>
        public static int CountFromStack()
        {
            try
            {
                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null || !Directory.Exists(directoryPath))
                {
                    return 0;
                }

                return Directory.GetFiles(directoryPath, "*.ipm").Length;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack TicketChat");
                return 0;
            }
        }

        /// <summary>
        /// Lädt alle TicketChat-Einträge aus dem Upload-Stack
        /// </summary>
        public static List<TicketChat> LoadAllFromUploadStack()
        {
            List<TicketChat> list = new List<TicketChat>();

            try
            {
                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null || !Directory.Exists(directoryPath))
                {
                    return list;
                }

                var files = Directory.GetFiles(directoryPath, "*.ipm");
                foreach (var file in files)
                {
                    try
                    {
                        string jsonString = File.ReadAllText(file);
                        if (string.IsNullOrWhiteSpace(jsonString))
                            continue;

                        var tc = JsonConvert.DeserializeObject<TicketChat>(jsonString);
                        if (tc != null)
                        {
                            list.Add(tc);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppModel.Logger?.Error(ex, $"ERROR: LoadAllFromUploadStack TicketChat - {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack TicketChat");
            }

            return list;
        }

        /// <summary>
        /// Entfernt einen TicketChat-Eintrag aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(TicketChat tc)
        {
            try
            {
                if (tc == null || string.IsNullOrWhiteSpace(tc.guid))
                {
                    return false;
                }

                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null)
                {
                    return false;
                }

                string filePath = Path.Combine(directoryPath, $"{tc.guid}.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack TicketChat");
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Repräsentiert eine ausstehende Besitzerstatus-Änderung eines Tickets im Upload-Stack
    /// </summary>
    public class TicketBesitzerStatusUpload
    {
        public string guid { get; set; } = Guid.NewGuid().ToString();
        public Int32 ticketid { get; set; } = 0;
        public int status { get; set; } = 0;

        public TicketBesitzerStatusUpload() { }

        public TicketBesitzerStatusUpload(Int32 ticketid, int status)
        {
            this.ticketid = ticketid;
            this.status = status;
            this.guid = Guid.NewGuid().ToString();
        }

        #region Upload Stack Management

        private static string GetUploadStackDirectory()
        {
            string customerNumber = AppModel.Instance?.SettingModel?.SettingDTO?.CustomerNumber;
            if (string.IsNullOrWhiteSpace(customerNumber))
            {
                return null;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ipm/" + customerNumber + "/ticketbesitzerstatusupload/"
            );
        }

        /// <summary>
        /// Fügt eine Besitzerstatus-Änderung zum Upload-Stack hinzu (wird später erneut hochgeladen)
        /// </summary>
        public static bool ToUploadStack(TicketBesitzerStatusUpload tbs)
        {
            try
            {
                if (tbs == null)
                {
                    AppModel.Logger?.Error("ToUploadStack TicketBesitzerStatusUpload: tbs is null");
                    return false;
                }

                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null)
                {
                    return false;
                }

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Priorisierung: Pro TicketId darf nur der höchste Status im Stack verbleiben.
                // Bereits vorhandene Einträge mit gleichem oder höherem Status machen den neuen Eintrag überflüssig.
                // Vorhandene Einträge mit niedrigerem Status werden entfernt, da sie überholt sind.
                var files = Directory.GetFiles(directoryPath, "*.ipm");
                foreach (var file in files)
                {
                    try
                    {
                        string existingJson = File.ReadAllText(file);
                        if (string.IsNullOrWhiteSpace(existingJson))
                            continue;

                        var existing = JsonConvert.DeserializeObject<TicketBesitzerStatusUpload>(existingJson);
                        if (existing == null || existing.ticketid != tbs.ticketid)
                            continue;

                        if (existing.status >= tbs.status)
                        {
                            // Ein bereits h\u00f6herer oder gleicher Status ist schon im Stack -> neuen Eintrag verwerfen
                            return true;
                        }
                        else
                        {
                            // Vorhandener Eintrag hat niedrigeren Status -> entfernen
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppModel.Logger?.Error(ex, $"ERROR: ToUploadStack TicketBesitzerStatusUpload - Priorisierung von {file}");
                    }
                }

                if (string.IsNullOrWhiteSpace(tbs.guid))
                {
                    tbs.guid = Guid.NewGuid().ToString();
                }

                string filePath = Path.Combine(directoryPath, $"{tbs.guid}.ipm");

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(tbs, jsonSettings);
                File.WriteAllText(filePath, jsonString);

                if (AppModel.Instance?.MainPage != null)
                {
                    AppModel.Instance.SetAllSyncStateSafe();
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: ToUploadStack TicketBesitzerStatusUpload");
                return false;
            }
        }

        /// <summary>
        /// Zählt die Anzahl der Besitzerstatus-Änderungen im Upload-Stack
        /// </summary>
        public static int CountFromStack()
        {
            try
            {
                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null || !Directory.Exists(directoryPath))
                {
                    return 0;
                }

                return Directory.GetFiles(directoryPath, "*.ipm").Length;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: CountFromStack TicketBesitzerStatusUpload");
                return 0;
            }
        }

        /// <summary>
        /// Lädt alle Besitzerstatus-Änderungen aus dem Upload-Stack
        /// </summary>
        public static List<TicketBesitzerStatusUpload> LoadAllFromUploadStack()
        {
            List<TicketBesitzerStatusUpload> list = new List<TicketBesitzerStatusUpload>();

            try
            {
                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null || !Directory.Exists(directoryPath))
                {
                    return list;
                }

                var files = Directory.GetFiles(directoryPath, "*.ipm");
                foreach (var file in files)
                {
                    try
                    {
                        string jsonString = File.ReadAllText(file);
                        if (string.IsNullOrWhiteSpace(jsonString))
                            continue;

                        var tbs = JsonConvert.DeserializeObject<TicketBesitzerStatusUpload>(jsonString);
                        if (tbs != null)
                        {
                            list.Add(tbs);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppModel.Logger?.Error(ex, $"ERROR: LoadAllFromUploadStack TicketBesitzerStatusUpload - {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: LoadAllFromUploadStack TicketBesitzerStatusUpload");
            }

            return list;
        }

        /// <summary>
        /// Entfernt eine Besitzerstatus-Änderung aus dem Upload-Stack
        /// </summary>
        public static bool DeleteFromUploadStack(TicketBesitzerStatusUpload tbs)
        {
            try
            {
                if (tbs == null || string.IsNullOrWhiteSpace(tbs.guid))
                {
                    return false;
                }

                string directoryPath = GetUploadStackDirectory();
                if (directoryPath == null)
                {
                    return false;
                }

                string filePath = Path.Combine(directoryPath, $"{tbs.guid}.ipm");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error(ex, "ERROR: DeleteFromUploadStack TicketBesitzerStatusUpload");
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Repräsentiert ein Ticket im System (angepasst an Backend-Struktur)
    /// </summary>
    public class Ticket : ICloneable
    {

        public object Clone()
        {
            return this.MemberwiseClone();
        }

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
            NochNichtGesehen = -1,  
            Gesehen = 0,            
            Gestartet = 1,          
            Rueckfrage = 2,         
            Abgeschlossen = 8       
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
        public string startab { get; set; } = "-1";      // Start ab (Zeitfenster)
        public string endbis { get; set; } = "0";       // Ende bis (Zeitfenster)
        public string updateat { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string lastbesitzerupdate { get; set; } = null;

        // Status und Flags
        public int status { get; set; } = 1;            // Ticket-Status (1=Neu, 2=Offen, etc.)
        public int besitzerstatus { get; set; } = -1;   // Status beim Besitzer (-1=Noch nicht gesehen)
        public string mandantStatus { get; set; } = "";
        public string reminder { get; set; } = null ;// null = nicht aktiv sonst day,std,min
        public int del { get; set; } = 0;               // Löschen-Flag (0=aktiv, 1=gelöscht, 5=vom Kunde gelöscht)
        public bool intern { get; set; } = true;        // Intern/Extern
        public int prio { get; set; } = 1;              // Priorität

        // Objekt-Referenzen (werden vom Backend gefüllt)
        public TicketPerson kunde { get; set; }
        public string kundename { get; set; }
        public TicketPerson besitzer { get; set; }
        public string besitzername { get; set; }
        public string besitzerinfo { get; set; }
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



        /// <summary>
        /// Gibt die Farbe für einen Ticket-Status zurück
        /// </summary>
        public static string GetStatusColor(int status)
        {
            return status switch
            {
                1 => "#6B7280", // Neue - Grau
                2 => "#3B82F6", // Offene - Blau
                4 => "#F59E0B", // In Arbeit - Gelb/Orange
                5 => "#F97316", // Rückfrage - Orange
                9 => "#DC2626", // Erledigte - Rot/Weinrot
                10 => "#4B5563", // Archiv - Dunkelgrau
                _ => "#ffffff"  // Unbekannt - Weiß
            };
        }

        /// <summary>
        /// Gibt den Text für einen Ticket-Status zurück
        /// </summary>
        public static string GetStatusText(int status)
        {
            return status switch
            {
                1 => "Neue",
                2 => "Offen",
                4 => "In Arbeit",
                5 => "Rückfrage",
                9 => "Erledigte",
                10 => "Archiv",
                _ => "Unbekannt"
            };
        }

        public static Border CreateTicketCard(Ticket ticket, double screenWidthDp)
        {
            // Statusfarbe aus ticket.status ermitteln
            string statusColor = GetStatusColor(ticket.status);

            var border = new Border
            {
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(0),
                BackgroundColor = Color.FromArgb("#2a2a2a"),
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#444444"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(5) }
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto }, // Status-Indikator
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Content
                    new ColumnDefinition { Width = GridLength.Auto } // ID Badge
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }, // Titel
                    new RowDefinition { Height = GridLength.Auto }, // Beschreibung (optional)
                    new RowDefinition { Height = GridLength.Auto }, // Info (Zeit, Prio, Chat)
                    new RowDefinition { Height = GridLength.Auto }  // Status + Start/End
                }
            };

            // Status-Indikator (genau am linken Rand, 6px breit, volle Höhe, abgerundete Ecken)
            var statusIndicator = new Border
            {
                BackgroundColor = Color.FromArgb(statusColor),
                WidthRequest = 6,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = new CornerRadius(5, 0, 0, 5) // Nur linke Ecken abgerundet
                }
            };
            grid.Add(statusIndicator, 0, 0);
            Grid.SetRowSpan(statusIndicator, 4); // Über alle 4 Zeilen

            // Titel-Zeile mit ID vorne
            var titleStack = new HorizontalStackLayout
            {
                Margin = new Thickness(12, 8, 8, 2),
                Spacing = 8,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            // Ticket-ID Badge (blauer abgerundeter Hintergrund vor dem Titel)
            var idBadge = new Border
            {
                BackgroundColor = Color.FromArgb("#0078d7"),
                Padding = new Thickness(6, 3),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = $"#{ticket.id}",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#ffffff"),
                    FontAttributes = FontAttributes.Bold
                }
            };
            titleStack.Children.Add(idBadge);

            // Titel
            var titleLabel = new Label
            {
                Text = ticket.titel ?? "Ohne Titel",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#ffffff"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                WidthRequest = screenWidthDp - 150,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            titleStack.Children.Add(titleLabel);

            grid.Add(titleStack, 1, 0);

            // Rechte Spalte: Prio oben, Zeit darunter
            var rightStack = new VerticalStackLayout
            {
                Margin = new Thickness(0, 8, 8, 0),
                Spacing = 5,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            };

            // Priorität als Chip darstellen
            if (ticket.prio >= 0)
            {
                string prioText;
                string prioBackgroundColor;
                string prioTextColor;

                if (ticket.prio == 0)
                {
                    prioText = "Gering";
                    prioBackgroundColor = "#4472C4";
                    prioTextColor = "#FFFFFF";
                }
                else if (ticket.prio == 1)
                {
                    prioText = "Normal";
                    prioBackgroundColor = "#009900";
                    prioTextColor = "#ffffff";
                }
                else if (ticket.prio == 2)
                {
                    prioText = "Hoch";
                    prioBackgroundColor = "#aa5500";
                    prioTextColor = "#FFFFFF";
                }
                else
                {
                    prioText = "NOTFALL";
                    prioBackgroundColor = "#990000";
                    prioTextColor = "#FFFFFF";
                }

                var prioChip = new Border
                {
                    BackgroundColor = Color.FromArgb(prioBackgroundColor),
                    Padding = new Thickness(8, 4),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(4) },
                    HorizontalOptions = LayoutOptions.End,
                    Content = new Label
                    {
                        Text = prioText,
                        FontSize = 11,
                        TextColor = Color.FromArgb(prioTextColor),
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };
                rightStack.Children.Add(prioChip);
            }

            // Zeitstempel unter der Prio
            DateTime.TryParse(ticket.updateat, out DateTime updateDate);
            var timeLabel = new Label
            {
                Text = FormatTicketTime(updateDate),
                FontSize = 10,
                TextColor = Color.FromArgb("#888888"),
                HorizontalOptions = LayoutOptions.End
            };
            rightStack.Children.Add(timeLabel);

            grid.Add(rightStack, 2, 0);
            Grid.SetRowSpan(rightStack, 2); // Über 2 Zeilen

            // Beschreibung
            //if (!string.IsNullOrEmpty(ticket.text))
            //{
            //    var descLabel = new Label
            //    {
            //        Text = ticket.text,
            //        FontSize = 12,
            //        TextColor = Color.FromArgb("#cccccc"),
            //        Margin = new Thickness(10, 0, 0, 2),
            //        LineBreakMode = LineBreakMode.TailTruncation,
            //        MaxLines = 2
            //    };
            //    grid.Add(descLabel, 0, 1);
            //    Grid.SetColumnSpan(descLabel, 2);
            //}

            // Chat-Badge (wenn neue Nachrichten vorhanden)
            //int newChatCount = ticket.chats?.Count(c => c.id == ticket.newchat?.id) ?? 0;
            //if (newChatCount > 0)
            //{
            //    var chatBadge = new Border
            //    {
            //        BackgroundColor = Color.FromArgb("#ff0000"),
            //        Padding = new Thickness(6, 2),
            //        Margin = new Thickness(12, 2, 8, 2),
            //        HorizontalOptions = LayoutOptions.Start,
            //        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
            //        Content = new Label
            //        {
            //            Text = $"{newChatCount} neu",
            //            FontSize = 10,
            //            TextColor = Color.FromArgb("#ffffff"),
            //            FontAttributes = FontAttributes.Bold
            //        }
            //    };
            //    grid.Add(chatBadge, 1, 2);
            //}

            // Zeile 3: Status-Chip und Start/End-Zeiten
            var statusAndDatesStack = new HorizontalStackLayout
            {
                Margin = new Thickness(12, 2, 8, 8),
                Spacing = 10
            };

            // Status-Chip links
            var statusChip = new Border
            {
                BackgroundColor = Color.FromArgb(statusColor),
                Padding = new Thickness(8, 4),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(4) },
                Content = new Label
                {
                    Text = GetStatusText(ticket.status),
                    FontSize = 10,
                    TextColor = Color.FromArgb("#ffffff"),
                    FontAttributes = FontAttributes.Bold
                }
            };
            statusAndDatesStack.Children.Add(statusChip);

            // Start/End-Zeiten vertikal
            var datesStack = new VerticalStackLayout
            {
                Spacing = 5
            };

            // Start-Datum/Status (basierend auf ticket.startab)
            if (!string.IsNullOrEmpty(ticket.startab))
            {
                string startValueText = "";
                DateTime? startDateTime = null;

                if (ticket.startab == "0")
                {
                    startValueText = "Ohne Angaben";
                }
                else if (ticket.startab == "-1")
                {
                    startValueText = "SOFORT";
                }
                else
                {
                    // Datum aus ticket.startab verwenden (UTC -> Lokalzeit)
                    if (long.TryParse(ticket.startab, out long startValue) && startValue > 0)
                    {
                        startDateTime = DateTimeOffset.FromUnixTimeMilliseconds(startValue).ToLocalTime().DateTime;
                        startValueText = $"{startDateTime.Value:dd.MM.yyyy HH:mm}";
                    }
                }

                if (!string.IsNullOrEmpty(startValueText))
                {
                    var startRowStack = new HorizontalStackLayout
                    {
                        Spacing = 8
                    };

                    var startLabel = new Label
                    {
                        FontSize = 10,
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span { Text = "START: ", TextColor = Color.FromArgb("#ffffff"), FontAttributes = FontAttributes.Bold },
                                new Span { Text = startValueText, TextColor = Color.FromArgb("#ffcc00") }
                            }
                        }
                    };
                    startRowStack.Children.Add(startLabel);

                    // Countdown bis zum Start (wenn in der Zukunft)
                    if (startDateTime.HasValue && startDateTime.Value > DateTime.Now)
                    {
                        var timeSpan = startDateTime.Value - DateTime.Now;
                        string countdownText = "";

                        if (timeSpan.TotalDays >= 1)
                        {
                            countdownText = $"(in {(int)timeSpan.TotalDays} Tag{(timeSpan.TotalDays >= 2 ? "en" : "")})";
                        }
                        else if (timeSpan.TotalHours >= 1)
                        {
                            countdownText = $"(in {(int)timeSpan.TotalHours} Std.)";
                        }
                        else if (timeSpan.TotalMinutes >= 1)
                        {
                            countdownText = $"(in {(int)timeSpan.TotalMinutes} Min.)";
                        }

                        if (!string.IsNullOrEmpty(countdownText))
                        {
                            var countdownLabel = new Label
                            {
                                Text = countdownText,
                                FontSize = 9,
                                TextColor = Color.FromArgb("#88ff88"),
                                VerticalOptions = LayoutOptions.Center
                            };
                            startRowStack.Children.Add(countdownLabel);
                        }
                    }

                    datesStack.Children.Add(startRowStack);
                }
            }

            // End-Datum/Status (basierend auf ticket.endbis)
            if (!string.IsNullOrEmpty(ticket.endbis))
            {
                string endValueText = "";

                if (ticket.endbis == "0")
                {
                    endValueText = "Ohne Angaben";
                }
                else if (ticket.endbis == "-1")
                {
                    endValueText = "SOFORT";
                }
                else
                {
                    // Datum aus ticket.endbis verwenden (UTC -> Lokalzeit)
                    if (long.TryParse(ticket.endbis, out long endValue) && endValue > 0)
                    {
                        DateTime endDate = DateTimeOffset.FromUnixTimeMilliseconds(endValue).ToLocalTime().DateTime;
                        endValueText = $"{endDate:dd.MM.yyyy HH:mm}";
                    }
                }

                if (!string.IsNullOrEmpty(endValueText))
                {
                    var endLabel = new Label
                    {
                        FontSize = 10,
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span { Text = "ENDE: ", TextColor = Color.FromArgb("#ffffff"), FontAttributes = FontAttributes.Bold },
                                new Span { Text = endValueText, TextColor = Color.FromArgb("#ffcc00") }
                            }
                        }
                    };
                    datesStack.Children.Add(endLabel);
                }
            }

            // Nur hinzufügen, wenn mindestens ein Datum vorhanden ist
            if (datesStack.Children.Count > 0)
            {
                statusAndDatesStack.Children.Add(datesStack);
            }

            // Nur hinzufügen, wenn Status oder Daten vorhanden sind
            if (statusAndDatesStack.Children.Count > 0)
            {
                grid.Add(statusAndDatesStack, 1, 3);
                Grid.SetColumnSpan(statusAndDatesStack, 2);
            }

            border.Content = grid;

            // Tap-Handler zum Öffnen des Tickets
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => AppModel.Instance.MainPage.OnTicketCardTapped(ticket);
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }

        public static string FormatTicketTime(DateTime dateTime)
        {
            var now = DateTime.Now;
            var diff = now - dateTime;

            if (diff.TotalMinutes < 1)
                return "Gerade eben";
            if (diff.TotalMinutes < 60)
                return $"vor {(int)diff.TotalMinutes} Min";
            if (diff.TotalHours < 24)
                return $"vor {(int)diff.TotalHours} Std";
            if (diff.TotalDays < 7)
                return $"vor {(int)diff.TotalDays} Tagen";

            return dateTime.ToString("dd.MM.yyyy");
        }



        public static string DecodeBase64RichText(string base64Text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64Text))
                {
                    return string.Empty;
                }

                // Versuche Base64-Dekodierung
                byte[] data = Convert.FromBase64String(base64Text);
                string decodedText = System.Text.Encoding.UTF8.GetString(data);

                // Trim und logge den echten Inhalt
                string trimmedText = decodedText.Trim();

                // Prüfe, ob es bereits HTML ist
                if (trimmedText.StartsWith("<"))
                {
                    return trimmedText;
                }

                // Wenn es Plain Text ist, konvertiere zu HTML
                string escapedText = System.Security.SecurityElement.Escape(trimmedText);
                escapedText = escapedText.Replace("\r\n", "<br>").Replace("\n", "<br>");
                return $"<p>{escapedText}</p>";
            }
            catch (FormatException ex)
            {
                //AppModel.Logger?.Warn($"DecodeBase64RichText: Not Base64, treating as plain text: {ex.Message}");
                // Kein Base64, behandle als normalen Text
                if (string.IsNullOrWhiteSpace(base64Text))
                    return string.Empty;

                string trimmedPlain = base64Text.Trim();

                // Bereits HTML (z.B. lokal erzeugte <div>/<p>/<img>-Nachrichten) - unverändert übernehmen
                if (trimmedPlain.StartsWith("<"))
                {
                    return trimmedPlain;
                }

                string escapedText = System.Security.SecurityElement.Escape(trimmedPlain);
                escapedText = escapedText.Replace("\r\n", "<br>").Replace("\n", "<br>");
                return $"<p>{escapedText}</p>";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Dekodieren: {ex.Message}");
                AppModel.Logger?.Error($"DecodeBase64RichText: Error = {ex.Message}");
                return $"<p style='color: #ff6b6b;'>Fehler beim Dekodieren der Beschreibung.</p>";
            }
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
                BesitzerStatus.Abgeschlossen => "Abgeschlossen",
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
                t = WrapAsRichText(nachricht),
                typ = typ,
                intern = intern,
                updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            if (chats == null)
            {
                chats = new List<TicketChat>();
            }

            chats.Add(chat);
            this.updateat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Stellt sicher, dass der Nachrichtentext als RTF/HTML (mindestens von einem &lt;div&gt; umschlossen)
        /// vorliegt, damit er im FrontendWeb korrekt als Richtext eingesetzt wird
        /// </summary>
        private static string WrapAsRichText(string nachricht)
        {
            if (string.IsNullOrWhiteSpace(nachricht))
            {
                return "<div></div>";
            }

            string trimmed = nachricht.Trim();

            // Bereits HTML (z.B. <div>, <p>, <img> aus Bild-Upload) - unverändert übernehmen
            if (trimmed.StartsWith("<"))
            {
                return trimmed;
            }

            return $"<div>{trimmed}</div>";
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
