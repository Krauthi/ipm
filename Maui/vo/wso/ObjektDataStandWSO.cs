using System;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class ObjektDataStandWSO
    {
        public Int32 id;
        public Int32 gruppeid;
        public Int32 objektdataid;
        public String datum;
        public String stand;
        public int del;
        public String lastchange;
        public String typ;
        public String nr;
        public String firstStand;
        public String lastStand;
        public String standGeaendertAm = "";
        public String ablesegrund = "";// Erstablesung, Jahresendeablesung, Zaeherlwechsel, Zwischenstand, Öl
        public String eich = ""; // "eich" in DB übertragen in ObjektData


        public string guid = "";
        public ObjektDataStandWSO()
        {
            this.guid = Guid.NewGuid().ToString();
        }
    }
}