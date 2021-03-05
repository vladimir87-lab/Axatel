using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Text;

namespace Axatel.Service
{
    public class amoFunction
    {
        public static string ClienID = "cd4781a4-fe45-418d-8013-d1746b1751f5";
        public static string ClientSecret = "RVQISkEGZm5vGN2w8i0Jfjhdux1O7cvHwHGc9nA2me5KkYPSUOZtwEVFKKCwSSI8";
        public static string RedirectUrl = "https://service-axatel.ru:8099/amo/index";

        public void RegisterCall(string portal, string acectok, string tel, string userid, int status, string type, string link , string CALL_FINISH_DATE="", int? duration=0)
        {
            if (status == 6) { duration = 0; CALL_FINISH_DATE = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");  }
            string result = "";
            if (status == 4) { result = "Звонок cостоялся"; }
            else if (status == 6) { result = "Пропущеный"; }
            // DateTime td = DateTimeю.Parse(CALL_START_DATE);
         //   DateTimeOffset td = DateTimeOffset.Now;
            DateTimeOffset tdn = DateTimeOffset.Parse(CALL_FINISH_DATE);
            Guid guid = Guid.NewGuid();
            var data = new
            {
                add = new[] {
                    new
                    {
                        uniq = guid.ToString(),
                        phone_number = tel,
                        source = ClienID,
                          created_at = tdn.ToUnixTimeSeconds(),
                          created_by = userid,
                          duration = duration,
                          call_status = status,
                          call_result = result,
                          direction = type,
                          link = link, //"https://service-axatel.ru:8099/content/zapisvobrabotke.mp3",
                          responsible_user_id = userid
                    }
                }
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);

                content = request.Post("https://" + portal + "/api/v2/calls", contentText2, "application/json").ToString();
            }

        }

        public void ShowEvent(string portal, string acectok, string tel, string useid)
        {

            var data = new
            {
                add = new[] {
                    new
                    {
                        type = "phone_call",
                        phone_number = tel,
                        users =  new[] { useid }
                    }
                }
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                content = request.Post("https://" + portal + "/api/v2/events", contentText2, "application/json").ToString();
            }

        }
        // получсаем ид ответственного
        public string[] isHaveCont(string portal, string acectok, string tel)
        {
            string respc;
            xNet.HttpResponse resp;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                resp = request.Get("https://" + portal + "/api/v2/contacts?query=" + tel);
                respc = resp.ToString();
            }
            var converter = new ExpandoObjectConverter();
            if (resp.StatusCode != xNet.HttpStatusCode.OK)
            {
                return null;
            }
            else
            {

                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(respc, converter);
                string[] iduser = new string[3];
                iduser[0] = obj2._embedded.items[0].responsible_user_id.ToString(); // ответственный
                iduser[1] = obj2._embedded.items[0].name.ToString(); // имя контакта
                iduser[2] = obj2._embedded.items[0].id.ToString(); // ид контакта
                return iduser;
            }
        }

        public string CreatCont(string portal, string acectok, string tel, string userid)
        {
            string content2;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                content2 = request.Get("https://" + portal + "/api/v2/account?with=custom_fields").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);
            string idtelef = "0";
            foreach (var item in obj._embedded.custom_fields.contacts)
            {
                if (item.Value.name == "Телефон") { idtelef = item.Value.id.ToString(); break; }

            }

            DateTimeOffset td = DateTimeOffset.Now;
            var data = new
            {
                add = new[] {
                    new
                    {
                        name = "Новый контакт",
                        responsible_user_id = userid,
                        created_by = userid,
                        created_at = td.ToUnixTimeSeconds(),
                        custom_fields = new[]{
                            new
                            {
                                id = idtelef,
                                values = new[]
                                {
                                    new {value = tel, @enum = "WORK"  }
                                }
                            }

                        }
                    }
                }
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                content = request.Post("https://" + portal + "/api/v2/contacts", contentText2, "application/json").ToString();
            }
            dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            string iduser = obj2._embedded.items[0].id.ToString();
            return iduser;
        }

        public int CreatDeal(string portal, string acectok, int cosht, int idcont, int idotv, string tag, string number)
        {
            var data = new[]
            {
                new
                {
                    name = "Новая сделка от "+number,
                     created_by = idotv,
                     responsible_user_id = idotv,
                     price = cosht,
                     _embedded = new
                     {

                         contacts = new[]
                         {
                             new{ id = idcont }
                         },
                         tags = new[]
                         {
                             new{name= tag}
                         }
                     }
                }
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                content = request.Post("https://" + portal + "/api/v4/leads", contentText2, "application/json").ToString();
            }
            return 1;
        }

        public List<string> StatusDeal(string portal, string acectok)
        {
            var converter = new ExpandoObjectConverter();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                content = request.Get("https://" + portal + "/api/v4/leads/pipelines").ToString();
            }
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            string idvoron = obj._embedded.pipelines[0].id.ToString();

            string content2;
            using (xNet.HttpRequest request2 = new xNet.HttpRequest())
            {
                request2.AddHeader("Authorization", " Bearer " + acectok);
                content2 = request2.Get("https://" + portal + "/api/v4/leads/pipelines/"+ idvoron + "/statuses").ToString();
            }
            dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);
            List<string> lstidstat = new List<string>();
            foreach (var item in obj2._embedded.statuses)
            {
                Encoding utf8 = Encoding.GetEncoding("UTF-8");
                Encoding win1251 = Encoding.GetEncoding("Windows-1251");
                byte[] utf8Bytes = win1251.GetBytes(item.name);
                byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);

                item.name = win1251.GetString(win1251Bytes);
                if ((item.name== "Первичный контакт") || (item.name == "Переговоры") || (item.name == "Принимают решение") || (item.name == "Согласование договора"))
                {

                    lstidstat.Add(item.id.ToString());
                }
            }
            return lstidstat;
        }

        public bool ifhaveDeals(string portal, string acectok, int idcont, List<string> lststatus)
        {
            var converter = new ExpandoObjectConverter();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                content = request.Get("https://" + portal + "/api/v4/contacts/"+ idcont + "?with=leads").ToString();
            }
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            List<int> lstdeals = new List<int>();
            foreach (var item in obj._embedded.leads)
            {
                lstdeals.Add(Convert.ToInt32(item.id));
            }
            bool flag = false;
            foreach(var item in lstdeals)
            {
                string content2;
                using (xNet.HttpRequest request2 = new xNet.HttpRequest())
                {
                    request2.AddHeader("Authorization", " Bearer " + acectok);
                    content2 = request2.Get("https://" + portal + "/api/v4/leads/"+ item).ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);
                if (lststatus.Contains(obj2.status_id.ToString()))
                {
                    flag = true;
                }
            }
            return flag;
        }
        public int CreatRazobrab(string portal, string acectok, int cosht, int idcont, int idotv, string tag, string number, string link, int duration )
        {
            var converter = new ExpandoObjectConverter();
            string content2;
            using (xNet.HttpRequest request2 = new xNet.HttpRequest())
            {
                request2.AddHeader("Authorization", " Bearer " + acectok);
                content2 = request2.Get("https://" + portal + "/api/v4/leads/pipelines").ToString();
            }
            dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);

            var data3 = new[]
            {
                new{name = tag }
            };
            string contentText3 = JsonConvert.SerializeObject(data3).ToString();
            string content3;
            using (xNet.HttpRequest request3 = new xNet.HttpRequest())
            {
                request3.AddHeader("Authorization", " Bearer " + acectok);
                content3 = request3.Post("https://" + portal + "/api/v4/leads/tags", contentText3, "application/json").ToString();
            }
            dynamic obj3 = JsonConvert.DeserializeObject<ExpandoObject>(content3, converter);
            int idtag = Convert.ToInt32(obj3._embedded.tags[0].id);
            int idvoron =Convert.ToInt32( obj2._embedded.pipelines[0].id);
            string guid1 = Guid.NewGuid().ToString();
            DateTimeOffset td = DateTimeOffset.Now;
            var data = new[]
           {
                new
                {
                    request_id = "123",
                     source_name = "Источник #1",
                     source_uid = guid1,
                     pipeline_id = idvoron,
                     created_at = td.ToUnixTimeSeconds(),
                     _embedded = new
                     {
                         leads = new[]
                         {
                             new{ name = "Новая сделка",
                                 price = cosht,
                                 _embedded = new{
                                        tags = new[]
                                        {
                                             new{id = idtag}
                                        }
                                    }
                             }                       
                         },
                         contacts = new[]
                         {
                             new{ id = idcont }
                         }
                        
                     },
                     metadata = new
                     {
                         is_call_event_needed = true,
                         duration = duration,
                        uniq = guid1,
                        service_code = "12345678",
                        link = link, //"https://service-axatel.ru:8099/content/zapisvobrabotke.mp3",
                        phone =  number,
                        called_at = td.ToUnixTimeSeconds(),
                        from = "клиента " + number
                     }
                }
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                content = request.Post("https://" + portal + "/api/v4/leads/unsorted/sip", contentText2, "application/json").ToString();
            }
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            return 1;
        }

        public class UserGetApi
        {
            public int id { get; set; }
            public string name { get; set; }
            public int InerNumb { get; set; }
            public bool isadmin { get; set; }
        }
        // получаем ид любого админа
        public string GetUsers(string portal, string acectok)
        {

            string resp;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                resp = request.Get("https://" + portal + "/api/v2/account?with=users").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(resp, converter);
            List<UserGetApi> userlst = new List<UserGetApi>();
            foreach (var item in obj._embedded.users)
            {
                UserGetApi user = new UserGetApi();
                user.id = Convert.ToInt32(item.Value.id);
                user.isadmin = item.Value.is_admin;
                userlst.Add(user);
            }
            string arruser = userlst.Where(i => i.isadmin == true).Select(q => q.id).FirstOrDefault().ToString();
            return arruser;
        }
        public string GetUserName(string portal, string acectok, int iduser)
        {

            string resp;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                resp = request.Get("https://" + portal + "/api/v2/account?with=users").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(resp, converter);
            string name = "";
            foreach (var item in obj._embedded.users)
            {
                if (Convert.ToInt32(item.Value.id) == iduser)
                {
                    name = item.Value.name;
                }
            }
            return name;
        }

        public List<UserGetApi> GetListApiUsers(string portal, string acectok)
        {

            string resp;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                request.AddHeader("Authorization", " Bearer " + acectok);
                resp = request.Get("https://" + portal + "/api/v2/account?with=users").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(resp, converter);
            List<UserGetApi> userlst = new List<UserGetApi>();
            foreach (var item in obj._embedded.users)
            {
                UserGetApi user = new UserGetApi();
                user.id = Convert.ToInt32(item.Value.id);
                user.isadmin = item.Value.is_admin;
                user.name = item.Value.name;
                userlst.Add(user);
            }
            return userlst;
        }
    }
}