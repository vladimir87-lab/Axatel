using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using xNet;

namespace Axatel.Service
{
    public class Ansver
    {
        public string CALL_GUID { get; set; }
        public string CRM_OWNER_NUMBER { get; set; }
        public string SIP_INFO { get; set; }
        public string CREATED_ENTITY { get; set; }

    }

    public class Function
    {
        //public Dictionary<string, string> usernamber = new Dictionary<string, string>();

        //Function()
        //{
        //    usernamber.Add("1", "0");
        //    usernamber.Add("2", "205");
        //}

        public int ShowCard(string B24_URL, string actok, string[] USER_PHONE_INNER, string CALL_GUID)
        {
            string[] usersids = GetIdUsersB24(B24_URL, actok, USER_PHONE_INNER).ToArray();
            var data = new
            {
                CALL_ID = CALL_GUID,
                USER_ID = usersids
              
            };
            string content = JsonConvert.SerializeObject(data).ToString();           
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                string cont = request.Post("https://" + B24_URL + "/rest/telephony.externalcall.show.json?auth=" + actok, content, "application/json").ToString();
            }
            return 1;
        }

        public int HideCard(string B24_URL, string actok, string[] USER_PHONE_INNER, string CALL_GUID)
        {
            string[] usersids = GetIdUsersB24(B24_URL, actok, USER_PHONE_INNER).ToArray();
            var data = new
            {
                CALL_ID = CALL_GUID,
                USER_ID = usersids

            };
            string content = JsonConvert.SerializeObject(data).ToString();
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                string cont = request.Post("https://" + B24_URL + "/rest/telephony.externalcall.hide.json?auth=" + actok, content, "application/json").ToString();
            }
            return 1;
        }

        public int Transfer(string B24_URL, string actok, string CALL_GUID, string NUMBER_HIDE, string NUMBER_SHOW)
        {
            string hideuser = GetIdUserB24(B24_URL, actok, NUMBER_HIDE);
            System.Threading.Thread.Sleep(100);
            string showuser = GetIdUserB24(B24_URL, actok, NUMBER_SHOW);
            System.Threading.Thread.Sleep(100);

            var data = new
            {
                CALL_ID = CALL_GUID,
                USER_ID = hideuser

            };
            string content = JsonConvert.SerializeObject(data).ToString();
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                string cont = request.Post("https://" + B24_URL + "/rest/telephony.externalcall.hide.json?auth=" + actok, content, "application/json").ToString();
            }
            System.Threading.Thread.Sleep(100);
            var data2 = new
            {
                CALL_ID = CALL_GUID,
                USER_ID = showuser

            };
            string content2 = JsonConvert.SerializeObject(data2).ToString();
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                string cont = request.Post("https://" + B24_URL + "/rest/telephony.externalcall.show.json?auth=" + actok, content2, "application/json").ToString();
            }

            return 1;
        }

        // определяем пользователя битрикс24
        public List<string> GetIdUsersB24(string B24_URL, string actok, string[] USER_PHONE_INNER)
        {
            List<string> operb24 = new List<string>();
         
                var data7 = new
                {
                    filter = new
                    {
                        UF_PHONE_INNER = USER_PHONE_INNER

                    }

                };
                string contentText7 = JsonConvert.SerializeObject(data7).ToString();

                string content7;
                using (xNet.HttpRequest request7 = new xNet.HttpRequest())
                {
                    content7 = request7.Post("https://" + B24_URL + "/rest/user.get.json?auth=" + actok, contentText7, "application/json").ToString();
                }
                var converter7 = new ExpandoObjectConverter();
                dynamic otvetst7 = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(content7, converter7);
            foreach( var item in  otvetst7.result)
            {
                operb24.Add(item.ID);
            }                       
            return operb24;
        }
        //получаем ид оператора битрикс24
        public string GetIdUserB24(string B24_URL, string actok, string USER_PHONE_INNER)
        {
            string operb24 = "1";
            if (USER_PHONE_INNER != "0")
            {
                var data7 = new
                {
                    filter = new
                    {
                        UF_PHONE_INNER = USER_PHONE_INNER

                    }

                };
                string contentText7 = JsonConvert.SerializeObject(data7).ToString();

                string content7;
                using (xNet.HttpRequest request7 = new xNet.HttpRequest())
                {
                    content7 = request7.Post("https://" + B24_URL + "/rest/user.get.json?auth=" + actok, contentText7, "application/json").ToString();
                }
                var converter7 = new ExpandoObjectConverter();
                dynamic otvetst7 = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(content7, converter7);
                try { operb24 = otvetst7.result[0].ID.ToString(); }
                catch { operb24 = "1"; }

            }
            return operb24;
        }

        public string GetUserInnerNumb(string B24_URL, string actok, string USER_ID)
        {
            string usernumb = "";
            string content = "";
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Get("https://" + B24_URL + "/rest/user.get.json?id=" + USER_ID + "&auth=" + actok).ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            return usernumb = obj.result[0].UF_PHONE_INNER.ToString();

        }
        public string GetUserMobNumb(string B24_URL, string actok, string USER_ID)
        {
            string usernumb = "";
            string content = "";
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Get("https://" + B24_URL + "/rest/user.get.json?id=" + USER_ID + "&auth=" + actok).ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            return usernumb = obj.result[0].PERSONAL_MOBILE.ToString();

        }
        // ложим трубку
        public int FinishCall(string CALL_Guid, string B24_URL, string INNER_PHONE, string DURATION, string VI_STATUS, string FAILED_REASON, string VOTE, string CALL_FINISH_DATE, string actok, string PROCES_STATUS)
        {
            string operb24 = GetIdUserB24(B24_URL, actok, INNER_PHONE);
            object data;
            if (VI_STATUS == "304")
            {
                data = new
                {
                    CALL_ID = CALL_Guid,
                    USER_ID = operb24,
                   
                    STATUS_CODE = VI_STATUS,
                    FAILED_REASON = FAILED_REASON,
                    VOTE = VOTE
                  
                };
            }
            else
            {
                data = new
                {
                    CALL_ID = CALL_Guid,
                    USER_ID = operb24,
                    DURATION = DURATION,
                    STATUS_CODE = VI_STATUS,
                    FAILED_REASON = FAILED_REASON,
                    VOTE = VOTE,
                    RECORD_URL = "https://service-axatel.ru:8099/content/zapisvobrabotke.mp3"
                };
            }
            
            string contentText = JsonConvert.SerializeObject(data).ToString();
            string content = "";
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://" + B24_URL + "/rest/telephony.externalcall.finish.json?auth=" + actok, contentText, "application/json").ToString();
            }
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/logb24.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--FinishCallB24--Content: " + content + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--FinishCallB24--Ошибка логирования!--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            return 1;
        }

        // прикрепление записи
        public int Record(string CALL_Guid, string B24_URL, string FILE_CONTENT, string URL, string Ip, string AcesTok)
        {

            if (URL.IndexOf("http") == -1)
            {
                URL = "http://" + Ip + URL;
            }

            var data2 = new
            {
                CALL_ID = CALL_Guid,
                FILENAME = CALL_Guid + ".mp3",
                RECORD_URL = URL
            };
            string contentText2 = JsonConvert.SerializeObject(data2).ToString();        
            using (xNet.HttpRequest request2 = new xNet.HttpRequest())
            {
                string content = request2.Post("https://" + B24_URL + "/rest/telephony.externalCall.attachRecord.json?auth=" + AcesTok, contentText2, "application/json").ToString();
            }

            return 1;
        }

        public string RegLead(string B24_URL, string PHONE_NUMBER, string NameLead, string actok, string USER_PHONE_INNER)
        {
            string operb24 = GetIdUserB24(B24_URL, actok, USER_PHONE_INNER);
            var data = new
            {

                fields = new
                {
                    TITLE = NameLead,
                    NAME = "Имя лида "+ PHONE_NUMBER,
                    SECOND_NAME = "",
                    LAST_NAME = "",
                    STATUS_ID = "NEW",
                    OPENED = "Y",
                    ASSIGNED_BY_ID = operb24,
                    CURRENCY_ID = "BYN",

                    PHONE = new[]
                    {
                         new {VALUE=PHONE_NUMBER , VALUE_TYPE = "WORK"}
                    }

                },
                @params = new
                {
                    REGISTER_SONET_EVENT = "Y"
                }

            };
            string contentText = JsonConvert.SerializeObject(data).ToString();

            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://" + B24_URL + "/rest/crm.lead.add?auth=" + actok, contentText, "application/json").ToString();
            }
            var converter2 = new ExpandoObjectConverter();
            dynamic idlead = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(content, converter2);
            return idlead.result.ToString();
        }

        public string  RegaCall(string B24_URL, string LeadID, string Comment, string actok, string USER_PHONE_INNER, string PHONE_NUMBER)
        {
           // string operb24 = GetIdUserB24(B24_URL, actok, USER_PHONE_INNER);
            var data = new
            {

                fields = new
                {
                    OWNER_TYPE_ID = 1,
                    OWNER_ID = LeadID,
                    TYPE_ID = 2,
                    COMMUNICATIONS = new[]
                    {
                       new{ VALUE = PHONE_NUMBER }
                    },
                    SUBJECT= "Новое дело",
                    COMPLETED = "N",
                    RESPONSIBLE_ID ="1",
                    DESCRIPTION = Comment

                }

            };
            string contentText = JsonConvert.SerializeObject(data).ToString();

            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://" + B24_URL + "/rest/crm.activity.add?auth=" + actok, contentText, "application/json").ToString();
            }

            return "ok";
        }


        // регистрация звонка
        public Ansver RegisterCall(string B24_URL, string PHONE_NUMBER, string CALL_START_DATE, string TYPE, string CRM_CREATE, string PROCES_STATUS, string actok, string USER_PHONE_INNER = "0", string LINE_NUMBER="")
        {
            string operb24 = GetIdUserB24(B24_URL, actok, USER_PHONE_INNER);
            
            Ansver ans = new Ansver();
            //var data5 = new
            //{
            //    filter = new
            //    {
            //        PHONE = PHONE_NUMBER

            //    },
            //    select = new[] { "ID", "NAME", "LAST_NAME", "SECOND_NAME", "PHONE" } //
            //};
            //string contentText = JsonConvert.SerializeObject(data5).ToString();

            //string content5;
            //using (xNet.HttpRequest request5 = new xNet.HttpRequest())
            //{
            //    content5 = request5.Post("https://" + B24_URL + "/rest/crm.contact.list?auth=" + actok, contentText, "application/json").ToString();
            //}
            //var converter2 = new ExpandoObjectConverter();
            //dynamic contakt = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(content5, converter2);

            //string contid;
            //try
            //{
            //    contid = contakt.result[0].ID; //ид контакта. если контакт есть то берем первый попавшийся

            //}
            //catch { contid = "0"; }

            //if (contid == "0") // ели контакта нет то добовляем
            //{
            //    var data3 = new
            //    {

            //        fields = new
            //        {
            //            NAME = PHONE_NUMBER,
            //            LAST_NAME = "Контакт",
            //            OPENED = "Y",
            //            ASSIGNED_BY_ID = operb24,
            //            TYPE_ID = "CLIENT",
            //            PHONE = new[]
            //            {
            //                new { VALUE = PHONE_NUMBER , VALUE_TYPE = "WORK" }
            //            }
            //        },
            //        @params = new
            //        {
            //            REGISTER_SONET_EVENT = "Y"
            //        }

            //    };
            //    string contentText3 = JsonConvert.SerializeObject(data3).ToString();
            //    string content3;
            //    using (xNet.HttpRequest request3 = new xNet.HttpRequest())
            //    {
            //        content3 = request3.Post("https://" + B24_URL + "/rest/crm.contact.add?auth=" + actok, contentText3, "application/json").ToString();
            //    }
            //}
            //System.Threading.Thread.Sleep(100);

            // if (dt != null) { date = dt; }
           
            var data = new
            {
                CALL_START_DATE = CALL_START_DATE,
                USER_ID = operb24,
                PHONE_NUMBER = PHONE_NUMBER,
                TYPE = TYPE,
                SHOW = 0,
                CRM_CREATE = CRM_CREATE,
                LINE_NUMBER = LINE_NUMBER,

            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://" + B24_URL + "/rest/telephony.externalcall.register.json?auth=" + actok, contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            System.Threading.Thread.Sleep(100);
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            List<object> resentry = obj.result.CRM_CREATED_ENTITIES;
            if (resentry.Count == 0)
            {
                ans.CREATED_ENTITY = "0";
            }
            else
            {
                ans.CREATED_ENTITY = "1";
            }
            if (string.IsNullOrEmpty(obj.result.CALL_ID)) { return null; }
            ans.CALL_GUID = obj.result.CALL_ID.ToString();

            string idotv = "";
            if (obj.result.CRM_ENTITY_TYPE.ToString() == "CONTACT")
            {
                ans.SIP_INFO = obj.result.CRM_ENTITY_TYPE;
                string idcont = obj.result.CRM_ENTITY_ID.ToString();
                string content2 = "";
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content2 = request.Get("https://" + B24_URL + "/rest/crm.contact.get.json?id=" + idcont + "&auth=" + actok).ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);
                idotv = obj2.result.ASSIGNED_BY_ID;
            }else if (obj.result.CRM_ENTITY_TYPE.ToString() == "LEAD")
            {
                ans.SIP_INFO = obj.result.CRM_ENTITY_TYPE;
                string idlead = obj.result.CRM_ENTITY_ID.ToString();
                string content2 = "";
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content2 = request.Get("https://" + B24_URL + "/rest/crm.lead.get.json?id=" + idlead + "&auth=" + actok).ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);
                idotv = obj2.result.ASSIGNED_BY_ID;

            }else if (obj.result.CRM_ENTITY_TYPE.ToString() == "COMPANY")
            {
                ans.SIP_INFO = obj.result.CRM_ENTITY_TYPE;
                string idcoo = obj.result.CRM_ENTITY_ID.ToString();
                string content2 = "";
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content2 = request.Get("https://" + B24_URL + "/rest/crm.company.get.json?id=" + idcoo + "&auth=" + actok).ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);
                idotv = obj2.result.ASSIGNED_BY_ID;

            }
            try
            {
                System.Threading.Thread.Sleep(100);
                string content6 = "";
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content6 = request.Get("https://" + B24_URL + "/rest/user.get.json?id=" + idotv + "&auth=" + actok).ToString();
                }
                dynamic obj6 = JsonConvert.DeserializeObject<ExpandoObject>(content6, converter);
                ans.CRM_OWNER_NUMBER = obj6.result[0].UF_PHONE_INNER.ToString();
            }
            catch
            {
                ans.SIP_INFO = "BITRIX_USER_NUMBER";
                ans.CRM_OWNER_NUMBER = "";
            }
            return ans;
        }
    }
}