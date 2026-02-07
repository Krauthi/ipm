using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class UpdatePushTokenRequest
    {
        public string token = "";
        public PNWSO pn = new PNWSO();

        public UpdatePushTokenRequest()
        {
        }
    }

    [Serializable]
    public class UpdatePushTokenResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public PNWSO pn;

        public UpdatePushTokenResponse()
        {
        }
    }

}