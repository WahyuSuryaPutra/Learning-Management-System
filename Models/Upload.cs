using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class Upload
    {
       public int subId { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public HttpPostedFileBase filee { get; set; }
        public string lastDateOnly { get; set; }
        public string lastTimeOnly { get; set; }
    }
}