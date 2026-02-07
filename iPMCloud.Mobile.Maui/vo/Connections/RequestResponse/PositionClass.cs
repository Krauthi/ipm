using System;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class PositionRequest
    {
        public string token = "";
        public LeistungPackWSO pack;

        public PositionRequest()
        {
        }
    }

    [Serializable]
    public class PositionResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;

        public LeistungPackWSO pack;
        public PlanPersonMobileWeek planweek;

        public PositionResponse()
        {
        }
    }
}