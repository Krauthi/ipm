using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    /// <summary>
    /// Request-Klasse für Ticket-API-Aufrufe
    /// </summary>
    [Serializable]
    public class TicketRequest
    {
        public string token { get; set; } = "";
        public Int32 ticketid { get; set; } = 0;
        public Int32 personid { get; set; } = 0;
        public Int32 gruppeid { get; set; } = 0;
        public int year { get; set; } = DateTime.Now.Year;
        public int month { get; set; } = DateTime.Now.Month;
        public string gruppeids { get; set; } = "";
        public bool inclChats { get; set; } = true;

        public TicketRequest()
        {
        }
    }

    /// <summary>
    /// Response-Klasse für Ticket-API-Aufrufe
    /// </summary>
    [Serializable]
    public class TicketResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = "";
        public Ticket ticket { get; set; } = null;
        public List<Ticket> tickets { get; set; } = null;
        public List<int> counts { get; set; } = null;
        public int unreadCount { get; set; } = 0;

        /// <summary>
        /// True, wenn diese Response aus dem lokalen Offline-Cache stammt und nicht live vom Server geholt wurde.
        /// </summary>
        public bool fromCache { get; set; } = false;

        /// <summary>
        /// Zeitpunkt, zu dem die Daten zuletzt erfolgreich LIVE vom Server geholt wurden.
        /// </summary>
        public DateTime? lastUpdated { get; set; } = null;
    }

    [Serializable]

    public class TicketStatusResponse
    {
        public bool success { get; set; } = true;
        public string message { get; set; } = "";
        public TicketStatusResponse() { }
    }

    [Serializable]

    public class TicketStatusRequest
    {
        public string token { get; set; } = "";
        public Int32 ticketid { get; set; } = 0;
        public int status { get; set; } = 0;
        public int besitzerstatus { get; set; } = 0;
        public string guid { get; set; } = "";
        public TicketStatusRequest() { }
    }

    [Serializable]

    public class TicketChatResponse
    {
        public bool success { get; set; } = true;
        public string message { get; set; } = "";
        public Int32 ticketchatid { get; set; } = 0;
        public TicketChatResponse() { }
    }

    [Serializable]
    public class TicketChatRequest
    {
        public string token { get; set; } = "";
        public Int32 gruppeid { get; set; } = 0;
        public Int32 ticketchatid { get; set; } = 0;
        public TicketChat tc { get; set; } = null;

        public TicketChatRequest()
        {
        }
    }


}
