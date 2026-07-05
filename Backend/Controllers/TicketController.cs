using MobileService.vo;
using MobileService.vo.ticket;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MobileService.Controllers
{
    public class TicketController : ApiController
    {
        public Root _root = new Root();

        public TicketController()
        {
        }

        #region Existing Methods

        [Route("~/api/GetTickets")]
        [HttpPost]
        public TicketResponse GetTickets(TicketRequest value)
        {
            TicketResponse result = new TicketResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                result = GetTickets(value.gruppeid, 0, 0, value.personid, -1, "", "DATE1", value.token);
            }
            return result;
        }

        [Route("~/api/GetTicketCounts")]
        [HttpPost]
        public TicketResponse GetTicketCounts(TicketRequest value)
        {
            TicketResponse result = new TicketResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                result.counts = LoadTicketsCount(value.gruppeid, value.token, 0, 0, value.personid);
            }
            return result;
        }

        [Route("~/api/GetTicket")]
        [HttpPost]
        public TicketResponse GetTicket(TicketRequest value)
        {
            TicketResponse result = new TicketResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                result.ticket = LoadTicket(value.ticketid);
            }
            return result;
        }

        #endregion

        #region New Chat Methods

        /// <summary>
        /// Lädt die komplette Chat-Historie eines Tickets
        /// </summary>
        [Route("~/api/GetTicketChats")]
        [HttpPost]
        public TicketChatResponse GetTicketChats(TicketChatRequest value)
        {
            TicketChatResponse result = new TicketChatResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                result.chats = TicketChat.LoadAll(_root._connectionString, value.ticketid);
                result.success = true;
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Fügt eine neue Chat-Nachricht zu einem Ticket hinzu
        /// </summary>
        [Route("~/api/AddTicketChatMessage")]
        [HttpPost]
        public TicketChatResponse AddTicketChatMessage(TicketChatRequest value)
        {
            TicketChatResponse result = new TicketChatResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    // Ticket laden
                    var ticket = Ticket.Load(_root._connectionString, value.ticketid);
                    if (ticket == null)
                    {
                        result.success = false;
                        result.message = "Ticket nicht gefunden";
                        return result;
                    }

                    // Neue Chat-Nachricht erstellen
                    var newChat = new TicketChat
                    {
                        ticketid = value.ticketid,
                        personid = value.personid,
                        typ = string.IsNullOrEmpty(value.typ) ? "info" : value.typ,
                        t = value.nachricht,
                        intern = value.intern,
                        personname = value.personname
                    };

                    // Chat-Nachricht speichern
                    bool success = TicketChat.AddTicketChat(_root._connectionString, newChat, value.token, value.intern);

                    if (success)
                    {
                        // Aktualisierte Chat-Liste zurückgeben
                        result.chats = TicketChat.LoadAll(_root._connectionString, value.ticketid);
                        result.success = true;
                        result.message = "Nachricht erfolgreich hinzugefügt";
                    }
                    else
                    {
                        result.success = false;
                        result.message = "Fehler beim Hinzufügen der Nachricht";
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Aktualisiert eine Chat-Nachricht
        /// </summary>
        [Route("~/api/UpdateTicketChatMessage")]
        [HttpPost]
        public TicketChatResponse UpdateTicketChatMessage(TicketChatRequest value)
        {
            TicketChatResponse result = new TicketChatResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    var chat = TicketChat.LoadOne(_root._connectionString, value.chatid);
                    if (chat == null)
                    {
                        result.success = false;
                        result.message = "Chat-Nachricht nicht gefunden";
                        return result;
                    }

                    chat.t = value.nachricht;
                    chat.intern = value.intern;

                    bool success = TicketChat.Update(_root._connectionString, chat);

                    if (success)
                    {
                        result.chats = TicketChat.LoadAll(_root._connectionString, value.ticketid);
                        result.success = true;
                        result.message = "Nachricht erfolgreich aktualisiert";
                    }
                    else
                    {
                        result.success = false;
                        result.message = "Fehler beim Aktualisieren der Nachricht";
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Löscht eine Chat-Nachricht (setzt del=2)
        /// </summary>
        [Route("~/api/DeleteTicketChatMessage")]
        [HttpPost]
        public TicketChatResponse DeleteTicketChatMessage(TicketChatRequest value)
        {
            TicketChatResponse result = new TicketChatResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    bool success = TicketChat.Delete(_root._connectionString, value.chatid);

                    if (success)
                    {
                        result.chats = TicketChat.LoadAll(_root._connectionString, value.ticketid);
                        result.success = true;
                        result.message = "Nachricht erfolgreich gelöscht";
                    }
                    else
                    {
                        result.success = false;
                        result.message = "Fehler beim Löschen der Nachricht";
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Ändert den intern-Status einer Chat-Nachricht
        /// </summary>
        [Route("~/api/ToggleTicketChatIntern")]
        [HttpPost]
        public TicketChatResponse ToggleTicketChatIntern(TicketChatRequest value)
        {
            TicketChatResponse result = new TicketChatResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    bool success = TicketChat.UpdateIntern(_root._connectionString, value.chatid);

                    if (success)
                    {
                        result.chats = TicketChat.LoadAll(_root._connectionString, value.ticketid);
                        result.success = true;
                        result.message = "Intern-Status erfolgreich geändert";
                    }
                    else
                    {
                        result.success = false;
                        result.message = "Fehler beim Ändern des Intern-Status";
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Ändert den Status eines Tickets und fügt optional einen Chat-Eintrag hinzu
        /// </summary>
        [Route("~/api/UpdateTicketStatus")]
        [HttpPost]
        public TicketResponse UpdateTicketStatus(TicketStatusRequest value)
        {
            TicketResponse result = new TicketResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    bool success = Ticket.UpdateStatus(
                        _root._connectionString, 
                        value.ticketid, 
                        value.status, 
                        value.intern, 
                        value.token
                    );

                    if (success)
                    {
                        result.ticket = LoadTicket(value.ticketid);
                        result.success = true;
                        result.message = "Status erfolgreich aktualisiert";
                    }
                    else
                    {
                        result.success = false;
                        result.message = "Fehler beim Aktualisieren des Status";
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Aktualisiert den Besitzer-Status eines Tickets
        /// </summary>
        [Route("~/api/UpdateTicketBesitzerStatus")]
        [HttpPost]
        public TicketResponse UpdateTicketBesitzerStatus(TicketStatusRequest value)
        {
            TicketResponse result = new TicketResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    string query = "UPDATE tickets SET " +
                        "besitzerstatus = " + value.besitzerstatus + ", " +
                        "lastbesitzerupdate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                        "WHERE id = " + value.ticketid + ";";

                    bool success = Utils.ActionMySQL(_root._connectionString, query);

                    if (success)
                    {
                        result.ticket = LoadTicket(value.ticketid);
                        result.success = true;
                        result.message = "Besitzer-Status erfolgreich aktualisiert";
                    }
                    else
                    {
                        result.success = false;
                        result.message = "Fehler beim Aktualisieren des Besitzer-Status";
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Markiert alle Ticket-Chats als gelesen für einen Benutzer
        /// </summary>
        [Route("~/api/MarkTicketChatsAsRead")]
        [HttpPost]
        public TicketChatResponse MarkTicketChatsAsRead(TicketChatRequest value)
        {
            TicketChatResponse result = new TicketResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    // Logik zum Markieren als gelesen (z.B. besitzerstatus aktualisieren)
                    string query = "UPDATE tickets SET " +
                        "besitzerstatus = 0 " + // 0 = Gesehen
                        "WHERE id = " + value.ticketid + " AND besitzerid = " + value.personid + ";";

                    bool success = Utils.ActionMySQL(_root._connectionString, query);

                    if (success)
                    {
                        result.success = true;
                        result.message = "Ticket als gelesen markiert";
                    }
                    else
                    {
                        result.success = false;
                        result.message = "Fehler beim Markieren als gelesen";
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        /// <summary>
        /// Lädt ungelesene Ticket-Anzahl für einen Benutzer
        /// </summary>
        [Route("~/api/GetUnreadTicketCount")]
        [HttpPost]
        public TicketResponse GetUnreadTicketCount(TicketRequest value)
        {
            TicketResponse result = new TicketResponse();
            if (Utils.CheckPersonCredential(_root._connectionString, value.token))
            {
                try
                {
                    // Zähle Tickets mit besitzerstatus = -1 (Noch nicht gesehen)
                    int count = Utils.ActionCountMySQL(_root._connectionString,
                        "SELECT COUNT(*) FROM tickets WHERE besitzerid = " + value.personid + 
                        " AND besitzerstatus = -1 AND del = 0;");

                    result.unreadCount = count;
                    result.success = true;
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "Fehler: " + ex.Message;
                }
            }
            else
            {
                result.success = false;
                result.message = "Authentifizierung fehlgeschlagen";
            }
            return result;
        }

        #endregion

        #region Private Helper Methods

        private TicketResponse GetTickets(Int32 gruppeid, Int32 personid, Int32 objektid, Int32 besitzerid, int status,
            string likeStr, string sort, string sessionid)
        {
            List<Ticket> tickets = new List<Ticket>();
            TicketResponse response = new TicketResponse();

            tickets = Ticket.LoadAll(_root._connectionString, likeStr, status, 0, personid, objektid, besitzerid);
            if (personid > 0)
            {
                response.counts = Ticket.LoadPersonCounts(_root._connectionString, personid);
            }
            else if (objektid > 0)
            {
                response.counts = Ticket.LoadObjektCounts(_root._connectionString, objektid);
            }
            else if (besitzerid > 0)
            {
                response.counts = Ticket.LoadBesitzerCounts(_root._connectionString, besitzerid);
            }
            else
            {
                response.counts = Ticket.LoadCounts(_root._connectionString);
            }

            if (sort == "DATE1")
            {
                response.tickets = tickets.OrderByDescending(_ => _.start).ToList();
            }
            else if (sort == "DATE2")
            {
                response.tickets = tickets.OrderBy(_ => _.start).ToList();
            }
            else if (sort == "PRIO1")
            {
                response.tickets = tickets.OrderBy(_ => _.prio).ToList();
            }
            else if (sort == "PRIO2")
            {
                response.tickets = tickets.OrderByDescending(_ => _.prio).ToList();
            }
            else if (sort == "STARTAB1")
            {
                response.tickets = tickets.OrderByDescending(_ => _.startab).ToList();
            }
            else if (sort == "STARTAB2")
            {
                response.tickets = tickets.OrderBy(_ => _.startab).ToList();
            }
            else if (sort == "ENDBIS1")
            {
                response.tickets = tickets.OrderByDescending(_ => _.endbis).ToList();
            }
            else if (sort == "ENDBIS2")
            {
                response.tickets = tickets.OrderBy(_ => _.endbis).ToList();
            }
            else if (sort == "BESITZER1")
            {
                response.tickets = tickets.OrderBy(_ => _.besitzername).ToList();
            }
            else if (sort == "BESITZER2")
            {
                response.tickets = tickets.OrderByDescending(_ => _.besitzername).ToList();
            }
            else if (sort == "ERSTELLER1")
            {
                response.tickets = tickets.OrderBy(_ => _.erstellername).ToList();
            }
            else if (sort == "ERSTELLER2")
            {
                response.tickets = tickets.OrderByDescending(_ => _.erstellername).ToList();
            }
            else if (sort == "STATUS")
            {
                response.tickets = tickets.OrderBy(_ => _.status).ToList();
            }

            return response;
        }

        private List<int> LoadTicketsCount(
            Int32 gruppeid, string sessionid, Int32 personid = 0, Int32 objektid = 0, Int32 besitzerid = 0)
        {
            if (personid == 0 && objektid == 0 && besitzerid == 0)
            {
                return Ticket.LoadCounts(_root._connectionString);
            }
            if (personid > 0 && objektid == 0 && besitzerid == 0)
            {
                return Ticket.LoadPersonCounts(_root._connectionString, personid);
            }
            if (personid == 0 && objektid > 0 && besitzerid == 0)
            {
                return Ticket.LoadObjektCounts(_root._connectionString, objektid);
            }
            if (personid == 0 && objektid > 0 && besitzerid > 0)
            {
                return Ticket.LoadBesitzerCounts(_root._connectionString, besitzerid);
            }
            return new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        private Ticket LoadTicket(Int32 ticketid)
        {
            var r = Ticket.Load(_root._connectionString, ticketid);
            if (r.auftragid > 0)
            {
                r.auftrag = GetAuftragToTicketFromDB(r.auftragid).FirstOrDefault();
            }
            return r;
        }

        public List<AuftragWSO> GetAuftragToTicketFromDB(Int32 auftragid)
        {
            var dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            List<AuftragWSO> resultList = new List<AuftragWSO>();
            List<AuftragWSO> auftragList =
                GetAuftragListFromDB(_root._connectionString,
                "SELECT * FROM objekt_auftrag WHERE id =" + auftragid + " AND del=0 AND (status='Aktiv' OR (status='Beendet' AND enddatum > '" +
                JavaScriptDateConverter.Convert(dt) + "')) ORDER BY id; ");

            foreach (var auftrag in auftragList)
            {
                int countMobile = 0;
                auftrag.kategorien = GetKategorieLeistungList(auftrag.id);
                if (auftrag.kategorien != null && auftrag.kategorien.Count > 0)
                {
                    foreach (var item in auftrag.kategorien)
                    {
                        if (item.leistungen != null && item.leistungen.Count > 0)
                        {
                            countMobile++;
                        }
                    }
                }
                if (countMobile > 0) { resultList.Add(auftrag); }
            }
            return resultList;
        }

        public List<AuftragWSO> GetAuftragListFromDB(string objektids = null)
        {
            var allKats = GetAllKategorieLeistungList(objektids);
            var dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            List<AuftragWSO> resultList = new List<AuftragWSO>();
            List<AuftragWSO> auftragList = objektids != null ?
                GetAuftragListFromDB(_root._connectionString,
                "SELECT * FROM objekt_auftrag WHERE objektid in (" + objektids + ") AND del=0 AND (status='Aktiv' OR (status='Beendet' AND enddatum > '" +
                JavaScriptDateConverter.Convert(dt) + "')) ORDER BY id; ") :
                GetAuftragListFromDB(_root._connectionString,
                "SELECT * FROM objekt_auftrag WHERE del=0 AND (status='Aktiv' OR (status='Beendet' AND enddatum > '" +
                JavaScriptDateConverter.Convert(dt) + "')) ORDER BY id; ");

            foreach (var auftrag in auftragList)
            {
                int countMobile = 0;
                auftrag.kategorien = allKats.FindAll(f => f.auftragid == auftrag.id);
                if (auftrag.kategorien != null && auftrag.kategorien.Count > 0)
                {
                    foreach (var item in auftrag.kategorien)
                    {
                        if (item.leistungen != null && item.leistungen.Count > 0)
                        {
                            countMobile++;
                        }
                    }
                }
                if (countMobile > 0) { resultList.Add(auftrag); }
            }
            return resultList;
        }

        public List<AuftragWSO> GetAuftragListFromDB(Int32 objektid)
        {
            var dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            List<AuftragWSO> auftragList = GetAuftragListFromDB(_root._connectionString,
                "SELECT * FROM objekt_auftrag WHERE del=0 AND objektid=" + objektid +
                " AND (status='Aktiv' OR (status='Beendet' AND enddatum > '" +
                JavaScriptDateConverter.Convert(dt) + "')) ORDER BY id; ");
            foreach (var auftrag in auftragList) { auftrag.kategorien = GetKategorieLeistungList(auftrag.id); }
            return auftragList;
        }

        private List<AuftragWSO> GetAuftragListFromDB(string _connectionString, string commandString)
        {
            List<AuftragWSO> auftragList = new List<AuftragWSO>();
            MySqlConnection connection = new MySqlConnection(_connectionString);
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = commandString;
                MySqlDataReader Reader;
                connection.Open();
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    AuftragWSO auftrag = new AuftragWSO();
                    auftrag = AuftragWSO.ParseSQLDataToObject(Reader);
                    auftragList.Add(auftrag);
                }
                connection.Close();
            }
            catch (Exception e)
            {
                connection.Close();
                Console.Write(e.Message);
            }
            return auftragList;
        }

        public List<KategorieWSO> GetAllKategorieLeistungList(string objektids = null)
        {
            List<KategorieWSO> kategorieList = GetKategorieListFromDB(_root._connectionString,
                objektids != null ?
                "SELECT * FROM auftragleistungen_kategorie WHERE objektid in (" + objektids + ") AND del=0 AND mobil=1 ORDER BY indexa " :
                "SELECT * FROM auftragleistungen_kategorie WHERE del=0 AND mobil=1 ORDER BY indexa ");
            List<LeistungWSO> leistungen = GetAllLeistungList(objektids);

            foreach (KategorieWSO kategorie in kategorieList)
            {
                kategorie.leistungen = leistungen.FindAll(f => f.kategorieid == kategorie.id);
                if (kategorie.leistungen != null || kategorie.leistungen.Count > 0)
                {
                    for (int i = 0; i < kategorie.leistungen.Count; i++)
                    {
                        if (kategorie.type == "1")
                        {
                            kategorie.leistungen[i].type = "1";
                        }
                    }
                }
            }
            return kategorieList;
        }

        public List<LeistungWSO> GetAllLeistungList(string objektids = null)
        {
            List<LeistungWSO> leistungen = GetLeistungListFromDB(_root._connectionString,
                objektids != null ?
                "SELECT * FROM auftragleistungen WHERE objektid in (" + objektids + ") AND del=0 AND mobil=1 ORDER BY indexa " :
                "SELECT * FROM auftragleistungen WHERE del=0 AND mobil=1 ORDER BY indexa ");
            List<LeistungExt> leistungExts = LeistungExt.LoadAllInAuftrag(_root._connectionString);
            if (leistungen != null || leistungen.Count > 0)
            {
                for (int i = 0; i < leistungen.Count; i++)
                {
                    leistungen[i].ext = LeistungExtWSO.ToWSO(leistungExts.Find(f => f.leistungid == leistungen[i].id));
                }
            }
            return leistungen;
        }

        public List<KategorieWSO> GetKategorieLeistungList(Int32 auftragid)
        {
            List<KategorieWSO> kategorieList = new List<KategorieWSO>();
            kategorieList = GetKategorieListFromDB(_root._connectionString,
                "SELECT * FROM auftragleistungen_kategorie WHERE " +
                " del=0 AND mobil=1 AND auftragid=" + auftragid + " ORDER BY indexa ");
            foreach (KategorieWSO kategorie in kategorieList)
            {
                kategorie.leistungen = GetLeistungListFromDB(_root._connectionString,
                    "SELECT * FROM auftragleistungen WHERE " +
                    " del=0 AND mobil=1 AND kategorieid=" + kategorie.id + " ORDER BY indexa ");
                if (kategorie.leistungen != null || kategorie.leistungen.Count > 0)
                {
                    for (int i = 0; i < kategorie.leistungen.Count; i++)
                    {
                        kategorie.leistungen[i].ext = LeistungExtWSO
                            .ToWSO(LeistungExt.LoadInAuftrag(_root._connectionString, kategorie.leistungen[i].id));
                        if (kategorie.type == "1")
                        {
                            kategorie.leistungen[i].type = "1";
                        }
                    }
                }
            }
            return kategorieList;
        }

        private List<KategorieWSO> GetKategorieListFromDB(string _connectionString, string commandString)
        {
            List<KategorieWSO> kategorien = new List<KategorieWSO>();
            MySqlConnection connection = new MySqlConnection(_connectionString);
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = commandString;
                MySqlDataReader Reader;
                connection.Open();
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    KategorieWSO kategorie = new KategorieWSO();
                    kategorie = KategorieWSO.ParseSQLDataToKategorie(Reader);
                    kategorien.Add(kategorie);
                }
                connection.Close();
            }
            catch (Exception e)
            {
                connection.Close();
                Console.Write(e.Message);
            }
            return kategorien;
        }

        private List<LeistungWSO> GetLeistungListFromDB(string _connectionString, string commandString)
        {
            List<LeistungWSO> leistungen = new List<LeistungWSO>();
            MySqlConnection connection = new MySqlConnection(_connectionString);
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = commandString;
                MySqlDataReader Reader;
                connection.Open();
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    LeistungWSO leistung = new LeistungWSO();
                    leistung = LeistungWSO.ParseSQLDataToLeistung(Reader);
                    if (leistung.muell == 1)
                    {
                        leistung.inout = InOutWSO.ToWSO(InOut.Load(_root._connectionString, leistung.auftragid, leistung.id));
                    }
                    leistungen.Add(leistung);
                }
                connection.Close();
            }
            catch (Exception e)
            {
                connection.Close();
                Console.Write(e.Message);
            }
            return leistungen;
        }

        #endregion
    }

    #region Request/Response Classes

    [Serializable]
    public class TicketRequest
    {
        public string token = "";
        public Int32 ticketid = 0;
        public Int32 personid = 0;
        public Int32 gruppeid = 0;
        public int year = 2021;
        public int month = 1;
        public string gruppeids = "";

        public TicketRequest()
        {
        }
    }

    [Serializable]
    public class TicketResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = "";
        public Ticket ticket { get; set; } = null;
        public List<Ticket> tickets { get; set; } = null;
        public List<int> counts { get; set; } = null;
        public int unreadCount { get; set; } = 0;
    }

    /// <summary>
    /// Request-Klasse für Chat-Operationen
    /// </summary>
    [Serializable]
    public class TicketChatRequest
    {
        public string token { get; set; } = "";
        public Int32 ticketid { get; set; } = 0;
        public Int32 chatid { get; set; } = 0;
        public Int32 personid { get; set; } = 0;
        public string personname { get; set; } = "";
        public string nachricht { get; set; } = "";
        public string typ { get; set; } = "info";
        public bool intern { get; set; } = true;

        public TicketChatRequest()
        {
        }
    }

    /// <summary>
    /// Response-Klasse für Chat-Operationen
    /// </summary>
    [Serializable]
    public class TicketChatResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = "";
        public List<TicketChat> chats { get; set; } = null;

        public TicketChatResponse()
        {
        }
    }

    /// <summary>
    /// Request-Klasse für Status-Änderungen
    /// </summary>
    [Serializable]
    public class TicketStatusRequest
    {
        public string token { get; set; } = "";
        public Int32 ticketid { get; set; } = 0;
        public int status { get; set; } = 1;
        public int besitzerstatus { get; set; } = -1;
        public bool intern { get; set; } = true;

        public TicketStatusRequest()
        {
        }
    }

    #endregion
}
