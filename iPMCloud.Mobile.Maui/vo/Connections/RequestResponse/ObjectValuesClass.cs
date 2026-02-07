using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class ObjectValuesRequest
    {
        public string token = "";
        public Int32 personid = 0;
        public List<ObjektDataWSO> objectValues = new List<ObjektDataWSO>();

        public ObjectValuesRequest()
        {
        }
    }

    [Serializable]
    public class ObjectValuesResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public List<ObjektDataWSO> objectValues;

        public ObjectValuesResponse()
        {
        }
    }
}