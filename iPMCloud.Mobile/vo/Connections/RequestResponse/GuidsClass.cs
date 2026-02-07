using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class GuidsCheckRequest
    {
        public string token;
        public string[] guids;

        public GuidsCheckRequest()
        {
        }
    }
    [Serializable]
    public class GuidsCheckResponse
    {
        public string token;
        public string[] guids;

        public GuidsCheckResponse()
        {
        }
    }
}