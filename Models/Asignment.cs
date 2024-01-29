using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class Asignment
    {
        public int docsID { get; set; }
        public HttpPostedFileBase assgnmentFile { get; set; }
    }
}