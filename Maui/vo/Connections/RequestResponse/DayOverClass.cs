using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class DayOverRequest
    {
        public string token = "";
        public List<DayOverWSO> dayOvers = new List<DayOverWSO>();

        public DayOverRequest()
        {
        }
    }

    [Serializable]
    public class DayOverResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public List<DayOverWSO> dayOvers;

        public DayOverResponse()
        {
        }
    }
}