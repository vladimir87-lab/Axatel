using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;

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
                string[] iduser = new string[2];
                iduser[0] = obj2._embedded.items[0].responsible_user_id.ToString(); // ответственный
                iduser[1] = obj2._embedded.items[0].name.ToString(); // имя контакта
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