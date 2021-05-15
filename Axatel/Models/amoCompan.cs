using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Axatel.Models
{
    public class amoCompan
    {
        public int Id { get; set; }
        public string PortalName { get; set; }
        public DateTime DTSetApp { get; set; }
        public string RefTok { get; set; }
        public DateTime DTRefTok { get; set; }
        public string AcesTok { get; set; }
        public string AxatelGuid { get; set; }
        public string BackToken { get; set; }
        public string Type { get; set; }
        public string BackIp { get; set; }

        public string InCall { get; set; }
        public string OutCall { get; set; }
        public string BadCall { get; set; }
        public int TagCall { get; set; }
        public string IdDopFildDeal { get; set; }
        public string IdDopFildDeal2 { get; set; }
        public string IdDopFildDeal3 { get; set; }
        public string IdTypeTask { get; set; }
        public string IdOtvetstv { get; set; }


    }
}