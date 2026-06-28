using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile.vo
{
    public class Translate
    {
        public Translate()
        {
        }
    }


    [Serializable]
    public class TransRequest
    {
        public string text = "";
        public string to = "";
        public Int32 leidid = 0;
        public Int32 katid = 0;
        public List<TransListItem> list = null;

        public TransRequest()
        {
        }
    }

    [Serializable]
    public class TransResponse
    {
        public bool success = false;
        public string text = "";
        public string to = "";
        public Int32 leidid = 0;
        public Int32 katid = 0;
        public List<TransListItem> list = null;


        public TransResponse()
        {
        }
    }
    [Serializable]
    public class TransOriginalResult
    {
        public string translatedText = "";

        public TransOriginalResult()
        {
        }
    }
    [Serializable]
    public class TransListItem
    {
        public string text = "";
        public Int32 leidid = 0;
        public Int32 katid = 0;

        public TransListItem()
        {
        }
    }



}
