using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Axatel.Models
{
    public class Compan
    {
        public int Id { get; set; }
        public string Portal { get; set; }
        public DateTime DTSetApp { get; set; }
        public string RefTok { get; set; }
        public DateTime DTRefTok { get; set; }
        public string AcesTok { get; set; }
        public string MemberId { get; set; }
        public string AxatelGuid { get; set; }
        public string BackToken { get; set; }
        public string Type { get; set; }
        public string BackIp { get; set; }
        public int? InerNumb { get; set; }
        public int IdFolder { get; set; }
        public int Activ { get; set; }
    }
}