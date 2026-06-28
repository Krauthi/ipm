using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    public class CheckKategorie
    {
        public Int32 id = 0;//id bigint(20) 
        public Int32 gruppeid;//gruppeid bigint(20) 
        public Int32 objektid;//gruppeid bigint(20) 
        public Int32 checkid;//gruppeid bigint(20) 
        public int indexa;//indexa int(11) 
        public String type = "0";
        public String titel = "";//titel varchar(100) 
        public int del = 0;//TINYINT(4)
        public String lastchange = "";//varchar(50)
        public int mobil = 1;//gruppeid bigint(20)
        public String art = "Leistung";//varchar(45) 
        public String notiz = "";//varchar(500) 

        public String saison = "0";//varchar(2) 
        public String startsaison = "0";//varchar(60) 
        public String endesaison = "0";//varchar(60) 

        //public PosOption po = null;

        public List<CheckLeistung> leistungen = new List<CheckLeistung>();
        public int anzahlLeistungen = 0;


        public int plangruppeid = 1; // 1 In Planung / 2 SetAll Time

        public int winterservice = 0;

        /*
        public Boolean aussetzen = false;
        public Boolean selected = false;
        public String kategorieTitel = "";
        public Boolean inwork = false;
        public Boolean faelligeLeistungen = false;

        public Int32 intbiga = 0;
        public Int32 intbigb = 0;
        public Int32 intbigc = 0;
        public String stra = "";
        public String strb = "";
        public String strc = "";
        public int inta = 0;
        public int intb = 0;
        public int intc = 0;
        public double da = 0;
        public double db = 0;
        public double dc = 0;
        */

    }
}