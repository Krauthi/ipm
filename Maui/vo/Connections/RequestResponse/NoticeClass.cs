using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    [Serializable]
    public class NoticeRequest
    {
        public string token = "";
        public List<BemerkungWSO> bemerkungen = new List<BemerkungWSO>();

        public NoticeRequest()
        {
        }
    }

    [Serializable]
    public class NoticeResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public List<BemerkungWSO> bemerkungen;

        public NoticeResponse()
        {
        }
    }



    [Serializable]
    public class SingleNoticeRequest
    {
        public string token = "";
        public BemerkungWSO bemerkung;

        public SingleNoticeRequest(string t, BemerkungWSO bem, bool inclphotos)
        {
            var b = new BemerkungWSO
            {
                text = bem.text,
                prio = bem.prio,
                gruppeid = bem.gruppeid,
                objektid = bem.objektid,
                auftragid = bem.auftragid,
                leistungid = bem.leistungid,
                personid = bem.personid,
                datum = bem.datum,
                hasSend = bem.hasSend,
                guid = bem.guid,
                photos = inclphotos ? bem.photos : null
            };

            token = t;
            // b.photos = null;
            bemerkung = b;
        }
    }

    [Serializable]
    public class SingleNoticeResponse
    {
        public bool success = false;
        public string message;
        public bool active = true;
        public Int32 bemid = 0;

        public SingleNoticeResponse()
        {
        }
    }


    [Serializable]
    public class NoticeBildRequest
    {
        public string token = "";
        public Int32 gruppeid = 0;
        public BildWSO bild;

        public NoticeBildRequest()
        {
        }
    }

    [Serializable]
    public class NoticeBildResponse
    {
        public bool success = false;
        public string message;

        public NoticeBildResponse()
        {
        }
    }



}