using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iPMCloud.Mobile
{
    public interface IBackgroundDataInfo
    {
        /// <summary>
        /// Prüft, ob die Hintergrunddatennutzung für die App vom Betriebssystem eingeschränkt ist
        /// (z. B. Android "Hintergrunddatennutzung zulassen" ist deaktiviert).
        /// </summary>
        bool IsBackgroundDataRestricted();

        /// <summary>
        /// Öffnet die System-Einstellungsseite, auf der der Nutzer die Hintergrunddatennutzung
        /// für die App aktivieren kann.
        /// </summary>
        void StartSetting();
    }
}
