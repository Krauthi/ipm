using System;

namespace iPMCloud.Mobile
{
    public class CheckInfo : ICloneable
    {
        public Int32 id;
        public Int32 gruppeid;
        public Int32 objektid;
        public Int32 personid;
        public String bezeichnung;
        public String datum;
        public String p4;
        public int berechnunginterval = 1;
        public Int32 refid = 0;
        public Int32 checkA_id = 0; // Wenn offene bearbeitung ist die die RefId von Offene CheckA.Id 
        public Int32 bindingid = 0; // Auftrag
        public String gueltigbis = "";
        public String ausfuehrungam = "";
        public string lastStateOfCheck_a = "-";

        public int naeststeFaelligkeitDate = 0;

        public double percentToReady = 0;



        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}