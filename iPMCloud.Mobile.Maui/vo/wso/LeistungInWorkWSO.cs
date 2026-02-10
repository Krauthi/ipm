using iPMCloud.Mobile.vo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class LeistungInWorkWSO
    {
        public Int32 id = 0;
        public Int32 gruppeid = 0;
        public Int32 kategorieid = 0;
        public Int32 auftragid = 0;
        public Int32 objektid = 0;
        public Int32 again = -1;

        public string guid = "";

        public string workat = "";
        public string lastwork = "";

        public string anzahl = "1,00"; // Anzahl wenn keine Einheit in Stunden!

        public int winterservice = 0;

        public InOutWSO inout = null;

        public PlanPersonMobile ppm = null;

        public List<BemerkungWSO> bemerkungen = new List<BemerkungWSO>();

        public LeistungInWorkWSO() {
            this.guid = Guid.NewGuid().ToString();
        }



        public static LeistungInWorkWSO ConvertLeistungTo(LeistungWSO l)
        {
            return new LeistungInWorkWSO
            {
                id = l.id,
                gruppeid = l.gruppeid,
                objektid = l.objektid,
                auftragid = l.auftragid,
                kategorieid = l.kategorieid,
                anzahl = Utils.formatDEStr(decimal.Parse(l.produktAnzahl) > 0 ? decimal.Parse(l.produktAnzahl) : 1),
                bemerkungen = null,
                inout = l.inout,
            };
        }

    }

}