using System;
using System.Collections.Generic;

namespace iPMCloud.Mobile
{
    public class CheckLeistungAntwortBem
    {
        public Int32 id = 0;
        public Int32 a_id = 0;
        public string datum = "";
        public string text = "";
        public string guid = "";
        public string antwort_guid = "";
        public List<CheckLeistungAntwortBemImg> imgs { get; set; } = new List<CheckLeistungAntwortBemImg>();
    }
}