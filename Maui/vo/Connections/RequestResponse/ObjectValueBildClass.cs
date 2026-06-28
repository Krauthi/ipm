using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class ObjectValueBildRequest
    {
        public string token = "";
        public Int32 personid = 0;
        public ObjektDatenBildWSO objectValueBild = null;

        public ObjectValueBildRequest()
        {
        }
    }

    [Serializable]
    public class ObjectValueBildResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public ObjektDatenBildWSO objectValueBild = null;

        public ObjectValueBildResponse()
        {
        }
    }
}