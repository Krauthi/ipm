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
    }

    /// <summary>
    /// Request-Klasse für Ticket-Chat-Operationen
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
    /// Response-Klasse für Ticket-Chat-Operationen
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
}
