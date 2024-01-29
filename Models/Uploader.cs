using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class Uploader
    {
        public string title { get;  set; }
        public string yrr { get; set; }
        public string semm { get; set; }
        public string subb { get;  set;}
        public string upload_type { get; set; }
        public HttpPostedFileBase filee { get; set; }
        public string lastDateOnly { get; set; }
        public string lastTimeOnly { get; set; }
    }
    public class UploadFromSubjects
    {
        public int subId { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public HttpPostedFileBase filee { get; set; }
        public string lastDateOnly { get; set; }
        public string lastTimeOnly { get; set; }
    }
}