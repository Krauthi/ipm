using System;

namespace iPMCloud.Mobile.vo
{
    [Serializable]
    public class SettingDTO
    {
        public string ServerUrl { get; set; } = ""; //*** to fixServer
        public string CustomerNumber { get; set; } = ""; //*** to fixServer
        public string CustomerName { get; set; } = ""; //*** to fixServer
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


        /*************** WICHTIG *****************************/
        /* Company Objekt anpassen wenn hier erweitert wird! */
        /*****************************************************/

        public SettingDTO()
        {
        }
    }
}
