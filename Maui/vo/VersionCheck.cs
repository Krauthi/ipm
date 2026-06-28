using System;
using System.Linq;
using System.Xml;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class VersionCheck
    {
        public VersionCheck()
        {
        }

        public String versionIOS = "";
        public String againIOS = "";

        public String versionAndroid = "";
        public String againAndroid = "";

        public String versionurl = "";
        public String playstoreUrl = "";
        public String appstoreUrl = "";

        public AppControll AppControll = new AppControll();

    }
}