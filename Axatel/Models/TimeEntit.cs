using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Axatel.Models
{
    public class TimeEntit
    {
        public int Id { get; set; }
        public int IdEntit { get; set; }
        public string PortalName { get; set; }
        public string Telefon { get; set; }        
        public string Type { get; set; }
    }
}