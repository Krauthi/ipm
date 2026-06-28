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
            get => FixQrCodeEncoding(_serverUrl); 
            set => _serverUrl = value; 
        }

        public string CustomerNumber 
        { 
            get => FixQrCodeEncoding(_customerNumber); 
            set => _customerNumber = value; 
        }

        public string CustomerName 
        { 
            get => FixQrCodeEncoding(_customerName); 
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


        public string FixQrCodeEncoding(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            try
            {
                // Versuche UTF-8 Korrektur: Konvertiere falsches Latin1 zurück zu Bytes und dann zu UTF-8
                byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
                string corrected = Encoding.UTF8.GetString(bytes);

                // Prüfe ob die Korrektur sinnvoll ist (keine Replacement-Characters)
                if (!corrected.Contains('\uFFFD'))
                {
                    return corrected;
                }
            }
            catch
            {
                // Bei Fehler, original zurückgeben
            }

            return text;
        }


    }
}