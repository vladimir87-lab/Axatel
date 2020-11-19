using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Axatel.Models
{
    public class Script
    {
        public int Id { get; set;}
        public string Title { get; set; }
        public string Text { get; set; }
        public int IdPortal { get; set; }


    }
}