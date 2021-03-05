using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Axatel.Models
{
    public class TaskAbonent
    {
        public int Id { get; set; }
        public string PortalName { get; set; }
        public string Guid { get; set; }
        public string NameTable { get; set; }
        public int isWork {get;set;}
        public int isFilterNumb { get; set; }

    }
}