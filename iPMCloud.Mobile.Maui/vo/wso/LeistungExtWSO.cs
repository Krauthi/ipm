using System;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class LeistungExtWSO
    {
        //public Int32 id  = 0;
        //public Int32 auftragid  = 0;
        //public Int32 angebotid  = 0;
        //public Int32 stammid  = 0;
        //public Int32 leistungid  = 0;
        public int muellway = 2;

        // Kurzer Anweisungstext in unterschiedlichen Sprachen möglich
        public string anweisung = "";
        public string anweisungLang = "";
        //public string lang = "de";
        //public string translation = "";


        public LeistungExtWSO() { }


    }

}