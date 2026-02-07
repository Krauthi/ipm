using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile
{

    [Serializable]
    public class PlanPersonMobile
    {
        public Int32 id { get; set; } = 0;
        public Int32 objektid { get; set; } = 0;
        public Int32 auftragid { get; set; } = 0;
        public Int32 personid { get; set; } = 0;
        public int katid { get; set; } = 0;
        public string katname { get; set; } = "";
        public string katids { get; set; }
        public int day { get; set; } = 0;
        public int planday { get; set; } = 0;
        public string std { get; set; } = "";
        public string eur { get; set; } = "";
        public string name { get; set; } = "";
        public string lastwork { get; set; } = "0";
        public string lastworker { get; set; } = "";
        public int haswork { get; set; } = 0;
        public int kwnum { get; set; } = 0;
        public int i { get; set; } = -1;
        public string datum { get; set; }
        public Int32 vonpersonid { get; set; } = 0;
        public Int32 muelltoid { get; set; } = 0;
        public string info { get; set; } = "";
        public string plz { get; set; } = "";
        public string ort { get; set; } = "";
        public string strasse { get; set; } = "";
        public string hsnr { get; set; } = "";
        public int winterservice { get; set; } = 0;


        public int mobil { get; set; } = 1;
        public Int32 leiid { get; set; } = 0;
        public string guid { get; set; } = "";
        public string sort { get; set; } = "";
        public string aw { get; set; } = "";

        public string stdD { get; set; } = "0,00";
        public string eurD { get; set; } = "0,00";
        public string stdW { get; set; } = "0,00";
        public string eurW { get; set; } = "0,00";

        public List<PlanPersonMobile> more { get; set; } = new List<PlanPersonMobile>();

        
        public LeistungInWorkWSO lei { get; set; } = null;


        public bool suscces = false;
        public bool isSelected = false;
        public StackLayout view;

        public static PlanPersonMobile ToNewPlanPersonMobile(PlanPersonMobile p)
        {
            return new PlanPersonMobile
            {
                id = p.id,
                objektid = p.objektid,
                auftragid = p.auftragid,
                personid = p.personid,
                katid = p.katid,
                katname = p.katname,
                katids = p.katids,
                day = p.day,
                planday = p.planday,
                std = p.std,
                eur = p.eur,
                name = p.name,
                lastwork = p.lastwork,
                lastworker = p.lastworker,
                haswork = p.haswork,
                kwnum = p.kwnum,
                i = p.i,
                datum = p.datum,
                vonpersonid = p.vonpersonid,
                muelltoid = p.muelltoid,
                mobil = p.mobil,
                info = p.info,
                plz = p.plz,
                ort = p.ort,
                strasse = p.strasse,
                hsnr = p.hsnr,
                leiid = p.leiid,
                guid = p.guid,
                sort = p.sort,
                aw = p.aw,
                stdD = p.stdD,
                eurD = p.eurD,
                stdW = p.stdW,
                eurW = p.eurW,
            };
        }
    }
}