using System;

namespace iPMCloud.Mobile
{
    public class CheckLeistung
    {
        public Int32 id = 0;//id bigint(20) 
        public Int32 gruppeid;//gruppeid bigint(20) 
        public Int32 kategorieid;//gruppeid bigint(20) 
        public Int32 checkid;//gruppeid bigint(20) 
        public Int32 objektid;//gruppeid bigint(20) 
        public Int32 indexa = 0;//indexa int(11) 
        public Int32 indexb = 0;// ......  für Fragen (Antwort)  1 == Ja - 2 == Nein  - 0 == unbeantwortet
        public Int32 indexc = 0;//indexa int(11) 
        public String type = ""; //type varchar(45)         
        public String beschreibung = "";//text text      
        public String timeval = "1x wöchentlich";//titel varchar(100) 
        public int del = 0;//TINYINT(4)
        public String lastchange = "";//varchar(50)
        public int mobil = 1;//gruppeid bigint(20) 
        public int timevaldays = 7;
        public String lastwork = "0";

        //public PosOption po = null;

        public int dstd = 0;
        public int dmin = 0;

        public String preis = "0,00";//Netto
        public String anzahl = "1,00";// auch halbe z.B. 0,5 (50%)
        public String ve = "1,00";// auch halbe z.B. 0,5 (50%)
        public String einheit = "std";// Stunde,Stück,Kg,...
        public String notiz = "";
        public String art = "Leistung";// Leistung oder Produkt oder Texte
        public String kurz = "";// Keyname
        public String kategoriename = "";
        public String workat = "";

        public int nichtpauschal = 0;//TINYINT(4)
        public int muell = 0;//TINYINT(4)
        public int muellcolor = 0;


        public CheckLeistungChecks clc = null;
        public CheckLeistungAntwort cla = null;

        public int winterservice = 0;

        public CheckKategorie kategorie;


        public Boolean aussetzen = false;
        public Boolean selected = false;
        public String kategorieTitel = "";
        public Boolean inwork = false;
        public Boolean faelligeLeistungen = false;

        public Int32 zuverbucheneanzahl = 1;
        public Int32 lastanzahl = 1;// Produkte zuletzt verwendet ... am = lastwork !




        /*
        public Int32 intbiga = 0;
        public Int32 intbigb = 0;
        public Int32 intbigc = 0;
        public String stra = "";
        public String strb = "";
        public String strj = "";
        public int inta = 0;
        public int intb = 0;
        public int intc = 0;
        public double da = 0;
        public double db = 0;
        public double dc = 0;
        */
        //public List<ObjektGeo> geos; 





    }
}