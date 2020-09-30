using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Axatel.Models
{
    public class AlfaCompan
    {
        public int Id { get; set; }
        public string PortalName { get; set; } //
        public DateTime DTSetApp { get; set; }  

        public string AcesTok { get; set; } // токен интеграции апи кей - acf112e1-2de8-11e9-9333-0cc47a6ca50e
        public string AcesTokTel { get; set; } // токен интеграции для телефонии - c4ca4238a0b923820dcc509a6f75849b
        public string AxatelGuid { get; set; }  // ключ телефонной станции


    }
}