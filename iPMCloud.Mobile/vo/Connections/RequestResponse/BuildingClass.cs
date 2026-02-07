using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class IpmBuildingRequest
    {
        public string token { get; set; } = "";
        public string objids { get; set; } = "";
        public long lastsync { get; set; } = 0;

        public IpmBuildingRequest()
        {
        }
    }



    [Serializable]
    public class IpmBuildingResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = "";
        public List<BuildingWSO> builgings { get; set; }
        public List<Int32> deletedBuidlings { get; set; } = new List<Int32>();
        public AppControll AppControll { get; set; }

        public IpmBuildingResponse()
        {
        }
    }


    [Serializable]
    public class IpmNewSyncRequest
    {
        public string token { get; set; } = "";
        public string objids { get; set; } = "";
        public long lastsync { get; set; } = 0;

        public IpmNewSyncRequest()
        {
        }
    }



    [Serializable]
    public class IpmNewSyncResponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = "";
        public List<BuildingWSO> builgings { get; set; }
        public List<AuftragWSO> auftraege { get; set; }
        public List<Int32> deletedBuidlings { get; set; } = new List<Int32>();
        public AppControll AppControll { get; set; }

        public IpmNewSyncResponse()
        {
        }
    }


}