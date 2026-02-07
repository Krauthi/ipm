using iPMCloud.Mobile.vo;
using System;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class IpmLoginRequest
    {
        public string bn = "";
        public string pw = "";

        public IpmLoginRequest()
        {
        }
    }

    [Serializable]
    public class IpmLoginSmallRequest
    {
        public string token = "";

        public IpmLoginSmallRequest()
        {
        }
    }

    [Serializable]
    public class IpmLoginResponse
    {
        public bool success = false;
        public string message = "";
        public string sessionkey = "";
        public bool active = true;
        public PersonWSO person = null;
        public VersionCheck versionCheck = new VersionCheck();

        public IpmLoginResponse()
        {
        }
    }



    [Serializable]
    public class CheckSettingsRequest
    {
        public string id = "";
        public string bn = "";
        public string pw = "";
        public SettingDTO settings;

        public CheckSettingsRequest()
        {
        }
    }
    [Serializable]
    public class CheckSettingsResponse
    {
        public bool success = false;
        public string message = "";
        public SettingDTO data = null;

        public CheckSettingsResponse()
        {
        }
    }
}
