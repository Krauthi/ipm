using System;
using System.Collections.Generic;
using System.Text;

namespace iPMCloud.Mobile
{
    public class PersonTimeRequest
    {
        public string token { get; set; } = string.Empty;
        public int personid { get; set; } = 0;
        public int gruppeid { get; set; } = 0;
        public int year { get; set; } = DateTime.Now.Year;
        public int month { get; set; } = DateTime.Now.Month;
        public string gruppeids { get; set; } = string.Empty;

        public PersonTimeRequest() { }

        // Optional: Constructor mit Parametern
        public PersonTimeRequest(string token, int personid, int year, int month)
        {
            this.token = token;
            this.personid = personid;
            this.year = year;
            this.month = month;
        }
    }


    public class PersonTimeResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = string.Empty;
        public List<PersonTime> times { get; set; } = new List<PersonTime>();

        public PersonTimeResponse() { }

        // Optional: Constructor für Success-Response
        public PersonTimeResponse(bool success, string message, List<PersonTime> times = null)
        {
            this.success = success;
            this.message = message;
            this.times = times ?? new List<PersonTime>();
        }
    }

    public class PersonTime
    {
        public long tick { get; set; }
        public string tag { get; set; } = string.Empty;
        public string tagname { get; set; } = string.Empty;
        public string monat { get; set; } = string.Empty;
        public string monatname { get; set; } = string.Empty;
        public string jahr { get; set; } = string.Empty;
        public string start { get; set; } = string.Empty;
        public string end { get; set; } = string.Empty;
        public string pause { get; set; } = string.Empty;
        public string dauer { get; set; } = string.Empty;
        public string all { get; set; } = string.Empty;
        public string art { get; set; } = string.Empty;

        /// <summary>
        /// AddFahrtZuschlag
        /// </summary>
        public string fahrt { get; set; } = string.Empty;

        /// <summary>
        /// L u. G ... Grund
        /// </summary>
        public string top { get; set; } = string.Empty;

        /// <summary>
        /// AddFahrtZuschlag
        /// </summary>
        public string grund { get; set; } = string.Empty;

        public PersonTime() { }
    }
}
