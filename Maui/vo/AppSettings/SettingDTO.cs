using Newtonsoft.Json;
using System.Text;

namespace iPMCloud.Mobile.vo
{
    public class SettingDTO
    {
        // Backing fields für die Properties, die Encoding-Korrektur benötigen
        private string _serverUrl = "";
        private string _customerNumber = "";
        private string _customerName = "";

        public string ServerUrl 
        { 
            get => _serverUrl; 
            set => _serverUrl = value; 
        }

        public string CustomerNumber 
        { 
            get => _customerNumber; 
            set => _customerNumber = value; 
        }

        public string CustomerName 
        { 
            get => _customerName; 
            set => _customerName = value; 
        }

        public string LoginName { get; set; } = ""; //*** to fixServer
        public string LoginPassword { get; set; } = ""; //*** to fixServer
        public string LoginToken { get; set; } = "";
        public string LastTokenDateTimeTicks { get; set; } = null;
        public bool Autologin { get; set; } = false;
        public string FontSize { get; set; } = "NORMAL";
        public string PNToken { get; set; } = ""; //*** to fixServer
        public int LastBuildingIdScanned { get; set; } = -1;
        public string LastBuildingSyncedDateTimeTicks { get; set; } = null;
        public bool RunBackground { get; set; } = false;
        public bool GPSInfoHasShow { get; set; } = false;
        public int SyncTimeHours { get; set; } = 12;

        public SettingDTO()
        {
            LastTokenDateTimeTicks = null;
            FontSize = "NORMAL";
            PNToken = ""; 
            LastBuildingIdScanned = -1;
            LastBuildingSyncedDateTimeTicks = null;
            GPSInfoHasShow = false;
            _customerNumber = "";
            _customerName = "";
            _serverUrl = "";
            LoginName = "";
            LoginPassword = "";
            LoginToken = "";
            Autologin = false;
            RunBackground = false;
            SyncTimeHours = 12;
        }


        //public string FixQrCodeEncoding(string text)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //        return text;

        //    // Wenn bereits "?" enthalten ist und der Text sonst ASCII ist, 
        //    // dann sind die Daten bereits beim Scannen verloren gegangen
        //    if (text.Contains('?'))
        //    {
        //        // Versuche verschiedene Encoding-Strategien
        //        try
        //        {
        //            // Strategie 1: UTF-8 Bytes die als Latin1 interpretiert wurden
        //            byte[] latin1Bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
        //            string utf8Test = Encoding.UTF8.GetString(latin1Bytes);
        //            if (!utf8Test.Contains('\uFFFD') && utf8Test.Any(c => c > 127))
        //            {
        //                return utf8Test;
        //            }

        //            // Strategie 2: Windows-1252 (erweiterte Latin1)
        //            byte[] win1252Bytes = Encoding.GetEncoding("windows-1252").GetBytes(text);
        //            string utf8Test2 = Encoding.UTF8.GetString(win1252Bytes);
        //            if (!utf8Test2.Contains('\uFFFD') && utf8Test2.Any(c => c > 127))
        //            {
        //                return utf8Test2;
        //            }
        //        }
        //        catch
        //        {
        //            // Fehler ignorieren
        //        }

        //        // Wenn nichts funktioniert hat, gib Original zurück
        //        return text;
        //    }

        //    // Normaler Fall: Versuche UTF-8 Korrektur wenn nicht-ASCII Zeichen vorhanden
        //    try
        //    {
        //        // Prüfe ob überhaupt nicht-ASCII Zeichen vorhanden sind
        //        if (!text.Any(c => c > 127))
        //        {
        //            return text; // Reiner ASCII, keine Korrektur nötig
        //        }

        //        // Versuche UTF-8 Korrektur: Konvertiere falsches Latin1 zurück zu Bytes und dann zu UTF-8
        //        byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
        //        string corrected = Encoding.UTF8.GetString(bytes);

        //        // Prüfe ob die Korrektur sinnvoll ist (keine Replacement-Characters)
        //        if (!corrected.Contains('\uFFFD'))
        //        {
        //            return corrected;
        //        }
        //    }
        //    catch
        //    {
        //        // Bei Fehler, original zurückgeben
        //    }

        //    return text;
        //}


    }
}