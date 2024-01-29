using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class Login
    {
        public string uname { get; set; }
        public string password { get; set; }
        public dynamic remember { get; set; }
    }
}