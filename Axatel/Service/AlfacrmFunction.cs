using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using xNet;


namespace Axatel.Service
{
    public class AlfacrmFunction
    {
        // получаем токен для альфа СРМ
        public string CreatLidAlfa()
        {

            var data = new
            {

                email = "vladimir@axata.by",
                api_key = "acf112e1-2de8-11e9-9333-0cc47a6ca50e"

            };
            string contentText = JsonConvert.SerializeObject(data).ToString();

            string tokenkey = "";
            using (xNet.HttpRequest reqtok = new xNet.HttpRequest())
            {
                string content = reqtok.Post("https://legionsport.s20.online/v2api/auth/login", contentText, "application/json").ToString();
                var converter = new ExpandoObjectConverter();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

                tokenkey = obj.token;
            }

            return tokenkey;

        }

        public string Zvonok(string call_id, string direction, string remote_number)
        {
          
            RequestParams reqpar = new RequestParams();            
            reqpar["event"] = "alert";
            reqpar["call_id"] = call_id;
            reqpar["direction"] = direction;
            reqpar["remote_number"] = remote_number;
            using (xNet.HttpRequest reqtok = new xNet.HttpRequest())
            {
                string content = reqtok.Post("https://legionsport.s20.online/api/1/sip/process?token=c4ca4238a0b923820dcc509a6f75849b", reqpar).ToString();
            }
            return "ok";
        }

        public string Active(string call_id, string direction, string remote_number)
        {

            RequestParams reqpar2 = new RequestParams();

            reqpar2["event"] = "active";
            reqpar2["call_id"] = call_id;
            reqpar2["direction"] = direction;
            reqpar2["remote_number"] = remote_number;
            using (xNet.HttpRequest request2 = new xNet.HttpRequest())
            {

                xNet.HttpResponse content2 = request2.Post("https://legionsport.s20.online/api/1/sip/process?token=c4ca4238a0b923820dcc509a6f75849b", reqpar2);
            }
            return "ok";
        }
        public string Close(string call_id, string direction, string remote_number, string USER_PHONE_INNER, string DURATION, string VI_STATUS, string finish_reason, string RECORD_URL)
        {

            RequestParams reqpar3 = new RequestParams();

            reqpar3["event"] = "release";
            reqpar3["call_id"] = call_id;
            reqpar3["direction"] = direction;
            reqpar3["local_number"] = USER_PHONE_INNER;
            reqpar3["duration"] = DURATION;
            reqpar3["is_success"] = VI_STATUS;
            reqpar3["remote_number"] = remote_number;
            reqpar3["finish_reason"] = finish_reason;
            reqpar3["record_url"] = RECORD_URL;
            using (xNet.HttpRequest request3 = new xNet.HttpRequest())
            {
                xNet.HttpResponse content3 = request3.Post("https://legionsport.s20.online/api/1/sip/process?token=c4ca4238a0b923820dcc509a6f75849b", reqpar3);
            }
            return "ok";
        }
        public List<Custumer> GetCostumers(string telef)
        {
            List<Custumer> listuser = new List<Custumer>();
            string token = CreatLidAlfa();
            var data = new
            {
                page = 0,
                phone = telef
            };
            string contentText = JsonConvert.SerializeObject(data).ToString();

            using (xNet.HttpRequest reqtok = new xNet.HttpRequest())
            {
                reqtok.AddHeader("X-ALFACRM-TOKEN", token);
                string content = reqtok.Post("https://legionsport.s20.online/v2api/1/customer/index", contentText, "application/json").ToString();
                var converter = new ExpandoObjectConverter();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

                foreach (var item in obj.items)
                {
                    Custumer user = new Custumer();
                    user.Id = item.id;
                    user.BierzDay = item.dob;
                    user.FullName = item.name;
                    user.Object = item.custom_obekt;
                    user.Gruop = item.custom_gruppa;
                    user.Telef1 = item.phone[0];
                    user.Telef2 = item.phone[1];
                    listuser.Add(user);
                }
            }
            return listuser;
        }

        public class Custumer
        {
            public long Id { get; set; }
            public string FullName { get; set; }
            public string BierzDay { get; set; }
            public string Gruop { get; set; }
            public string Object { get; set; }
            public string Telef1 { get; set; }
            public string Telef2 { get; set; }
        }
    }
}