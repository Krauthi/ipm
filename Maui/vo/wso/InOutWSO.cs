using System;


namespace iPMCloud.Mobile
{
    [Serializable]
    public class InOutWSO
    {
        public Int32 id { get; set; } = 0;
        //public Int32 auftragid { get; set; } = 0;
        //public Int32 leistungid { get; set; } = 0;
        public int inout { get; set; } = 0;
        //public int newinout { get; set; } = 0;
        //public String bemerkung { get; set; } = "";
        public String last { get; set; } = "0";

        public InOutWSO() { }
    }
}