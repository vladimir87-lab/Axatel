using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Axatel.Models
{
    public class BlackList
    {
        public int Id { get; set; }
        public string Numb { get; set; }
        public DateTime Date { get; set; }
        public int PortalId { get; set; }
    }
}