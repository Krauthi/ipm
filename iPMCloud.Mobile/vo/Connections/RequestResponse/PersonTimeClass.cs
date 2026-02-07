using System;
using System.Collections.Generic;
using System.Text;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class PersonTimeRequest
    {
        public string token = "";
        public Int32 personid = 0;
        public Int32 gruppeid = 0;
        public int year = 2021;
        public int month = 1;
        public string gruppeids = "";

        public PersonTimeRequest()
        {
        }
    }


    [Serializable]
    public class PersonTimeResponse
    {
        public bool success = false;
        public string message = "";
        public List<PersonTime> times;

        public PersonTimeResponse()
        {
        }
    }

    [Serializable]
    public class PersonTime
    {
        public long tick;
        public string tag;
        public string tagname;
        public string monat;
        public string monatname;
        public string jahr;
        public string start;
        public string end;
        public string pause;
        public string dauer;
        public string all;
        public string art;
        public string fahrt; // AddFahrtZuschlag
        public string top; // L & G ... Grund
        public string grund; // AddFahrtZuschlag


        public PersonTime()
        {
        }
    }
}
