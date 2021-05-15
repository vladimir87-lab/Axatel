using Axatel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Axatel.Service;
using xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Drawing;
using System.IO;
using System.Net.Mail;


namespace Axatel.Controllers
{
    public class HomeController : Controller
    {
        Context _db = new Context();
        Service.Function func = new Service.Function();
        public static string client_id = "app.5e76175e7ed6c9.84942186";
        public static string client_secret = "DQAPkjKmX3uwA24Pf1NExrYwQ6GugYSC1r55V17Etbkea57UUV";


        public ActionResult Licenzia()
        {
            return View();
        }
        public ActionResult Uslovie()
        {
            return View();
        }

        public ActionResult Index(string member_id = "")
        {

           //  return Content("ok");
            return RedirectToAction("MainPage", new { member_id = member_id });
        }
        public ActionResult MainPage(string member_id)
        {
            // if (string.IsNullOrEmpty(member_id)) return Content("<h2>Ошибка доступа<h2>");
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            // обрываем парсинг
            Axatel.Controllers.ParsContController.Statuspars stat = ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault();
            if (stat == null)
            {
                ParsContController.stadeal.Add(new ParsContController.Statuspars { Portal = comp.Portal, flagdeal = false });
            }
            ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = false;
            // обрываем парсинг

            RefSetToken(comp.Portal);
            ViewBag.Memb = member_id;
            return View(comp);
        }
        [HttpPost]
        public ActionResult MainPage(string member_id, string AXATEL_GUID, string backurl, string backip, int? innumb, string Type)
        {
            // if (string.IsNullOrEmpty(member_id)) return Content("<h2>Ошибка доступа<h2>");
            backip = backip.Trim();
            AXATEL_GUID = AXATEL_GUID.Trim();
            backurl = backurl.Trim();
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2>Ошибка доступа<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            if (!string.IsNullOrEmpty(backurl))
            {
                comp.BackToken = backurl;
                comp.BackIp = backip;
                comp.Type = Type;
                comp.InerNumb = innumb.GetValueOrDefault(0);
            }
            comp.AxatelGuid = AXATEL_GUID;
            _db.Entry(comp).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();

            var data = new
            {
                entity = "check",
                action = "check",
                options = new
                {
                    B24_URL = comp.Portal,
                    AXATEL_GUID = AXATEL_GUID,
                }
            };
            string contentText = JsonConvert.SerializeObject(data).ToString();
            string content;
            try
            {
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Post("http://" + backip + "/api/token/v1/" + backurl, contentText, "application/json").ToString();
                }
                var convert = new ExpandoObjectConverter();
                dynamic otvet = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(content, convert);
                if (otvet.success == true)
                {
                    ViewBag.Check = "True";
                }
                else if (otvet.success == false)
                {
                    ViewBag.Check = "False";
                }
            }
            catch
            {
                ViewBag.Check = "False";
            }
            ViewBag.Memb = member_id;
            return View(comp);
        }



        // регистрация звонка
        public ActionResult Reg(string INTAPP, string AXATEL_GUID, string B24_URL, string PHONE_NUMBER, string CALL_START_DATE, string TYPE, string CRM_CREATE, string PROCES_STATUS, string USER_PHONE_INNER = "0", string LINE_NUMBER = "")
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--RegCall--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--PHONE_NUMBER:" + PHONE_NUMBER + "--CALL_START_DATE:" + CALL_START_DATE + "--TYPE:" + TYPE + "--CRM_CREATE:" + CRM_CREATE + "--PROCES_STATUS:" + PROCES_STATUS + "--USER_PHONE_INNER:" + USER_PHONE_INNER + "--LINE_NUMBER:" + LINE_NUMBER + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--RegCall--Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();

            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            if (TYPE == "2")
            {
                BlackList bl = _db.BlackLists.Where(i => i.PortalId == co.Id).Where(n => n.Numb == PHONE_NUMBER.Trim()).FirstOrDefault();
                if (bl != null)
                {
                    return Json(new { BLACKLIST = "1" }, JsonRequestBehavior.AllowGet);
                }
            }
            RefSetToken(B24_URL);
            Ansver ans = func.RegisterCall(B24_URL, PHONE_NUMBER, CALL_START_DATE, TYPE, CRM_CREATE, PROCES_STATUS, co.AcesTok, USER_PHONE_INNER, LINE_NUMBER);

            return Json(ans, JsonRequestBehavior.AllowGet);
        }
        // закрытие звонка
        public ActionResult Finish(string INTAPP, string AXATEL_GUID, string CALL_Guid, string B24_URL, string INNER_PHONE, string DURATION, string VI_STATUS, string FAILED_REASON, string VOTE, string CALL_FINISH_DATE, string PROCES_STATUS)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--FinishCall--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--CALL_Guid:" + CALL_Guid + "--INNER_PHONE:" + INNER_PHONE + "--DURATION:" + DURATION + "--VI_STATUS:" + VI_STATUS + "--PROCES_STATUS:" + PROCES_STATUS + "--FAILED_REASON:" + FAILED_REASON + "--VOTE:" + VOTE + "--CALL_FINISH_DATE:" + CALL_FINISH_DATE + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--FinishCall--Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }

            func.FinishCall(CALL_Guid, B24_URL, INNER_PHONE, DURATION, VI_STATUS, FAILED_REASON, VOTE, CALL_FINISH_DATE, co.AcesTok, PROCES_STATUS);



            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Record(string INTAPP, string AXATEL_GUID, string CALL_Guid, string B24_URL, string URL, string PROCES_STATUS, string FILE_CONTENT = "")
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            
            string filecont = "";
            if (FILE_CONTENT.Length > 10) { filecont = FILE_CONTENT.Substring(0, 10); } else { filecont = FILE_CONTENT; };
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--Record--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--FILE_CONTENT:" + filecont + "--PROCES_STATUS:" + PROCES_STATUS + "--URL:" + URL + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Record--Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();

            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            
            func.Record(CALL_Guid, B24_URL, FILE_CONTENT, URL, co.BackIp, co.AcesTok);
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Showgroup(string AXATEL_GUID, string B24_URL, string CALL_GUID, string[] INNER_PHONE)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--Showgroup--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--CALL_GUID:" + CALL_GUID + "--INNER_PHONE:" + string.Join(",", INNER_PHONE) + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Showgroup--Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }

            func.ShowCard(B24_URL, co.AcesTok, INNER_PHONE, CALL_GUID);

            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Hidegroup(string AXATEL_GUID, string B24_URL, string CALL_GUID, string[] INNER_PHONE)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--Hidegroup--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--CALL_GUID:" + CALL_GUID + "--INNER_PHONE:" + string.Join(",", INNER_PHONE) + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Hidegroup--Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }

            func.HideCard(B24_URL, co.AcesTok, INNER_PHONE, CALL_GUID);

            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Transfer(string AXATEL_GUID, string B24_URL, string CALL_GUID, string NUMBER_HIDE, string NUMBER_SHOW)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--Transfer--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--CALL_GUID:" + CALL_GUID + "--NUMBER_HIDE:" + string.Join(",", NUMBER_HIDE) + "--NUMBER_SHOW:" + string.Join(",", NUMBER_SHOW) + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Transfer--Ошибка логирования--\n\n");
            }
            myfile.Close();

            myfile.Dispose();
            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }

            func.Transfer(B24_URL, co.AcesTok, CALL_GUID, NUMBER_HIDE, NUMBER_SHOW);

            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        // Классы для битрикс КликТуКолл
        public class Auth
        {
            public string domain { get; set; }
            public string application_token { get; set; }
            public string member_id { get; set; }
        }
        public class Data
        {
            public string PHONE_NUMBER { get; set; }
            public string PHONE_NUMBER_INTERNATIONAL { get; set; }
            public string USER_ID { get; set; }
            public string CALL_LIST_ID { get; set; }
            public string CALL_ID { get; set; }
            public string CRM_ENTITY_TYPE { get; set; }
            public string CRM_ENTITY_ID { get; set; }
            public string IS_MOBILE { get; set; }
        }
        public ActionResult BackToCall(Data data, Auth auth, string ts, string @event)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--BackToCall--B24_GUID:" + data.CALL_ID + "--B24_URL:" + auth.domain + "--PHONE_NUMBER:" + data.PHONE_NUMBER + "--CRM_ENTITY_TYPE:" + data.CRM_ENTITY_TYPE + "--CRM_ENTITY_ID:" + data.CRM_ENTITY_ID + "--USER_ID:" + data.USER_ID + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--BackToCall--Ошибка логирования--\n\n");
            }
            myfile.Close();

            myfile.Dispose();
            // Compan co = _db.Compans.Where(p => p.Portal == auth.domain && p.MemberId == auth.member_id).FirstOrDefault();
            Compan co = _db.Compans.Where(p => p.Portal == auth.domain).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            RefSetToken(auth.domain);
            // string UserInnerNumb = func.GetUserInnerNumb(auth.domain, co.AcesTok, data.USER_ID);

            var datasend = new
            {
                entity = "callmanager",
                action = "init_call",
                options = new
                {

                    USER_PHONE_INNER = co.InerNumb,
                    PHONE_NUMBER = data.PHONE_NUMBER,
                    B24_URL = co.Portal,
                    AXATEL_GUID = co.AxatelGuid,
                    CRM_ENTITY_TYPE = data.CRM_ENTITY_TYPE,
                    CRM_ENTITY_ID = data.CRM_ENTITY_ID,
                    CALL_GUID = "externalCall.OnExternalCallBackStart",
                    TYPE = "CALLBACK"

                }
            };
            string contentText = JsonConvert.SerializeObject(datasend).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("http://" + co.BackIp + "/api/token/v1/" + co.BackToken, contentText, "application/json").ToString();
            }
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClickToCall(Data data, Auth auth, string ts, string @event)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--ClickToCall--B24_GUID:" + data.CALL_ID + "--B24_URL:" + auth.domain + "--PHONE_NUMBER:" + data.PHONE_NUMBER + "--CRM_ENTITY_TYPE:" + data.CRM_ENTITY_TYPE + "--CRM_ENTITY_ID:" + data.CRM_ENTITY_ID + "--USER_ID:" + data.USER_ID + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--ClickToCall--Ошибка логирования--\n\n");
            }
            myfile.Close();


            myfile.Dispose();
            // Compan co = _db.Compans.Where(p => p.Portal == auth.domain && p.MemberId == auth.member_id).FirstOrDefault();
            Compan co = _db.Compans.Where(p => p.Portal == auth.domain).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            RefSetToken(auth.domain);
            string UserInnerNumb = "";
            Object datasend;
            if (data.IS_MOBILE == "1")
            {
                UserInnerNumb = func.GetUserMobNumb(auth.domain, co.AcesTok, data.USER_ID);
                if (string.IsNullOrEmpty(UserInnerNumb))
                {
                    return Content("Not Mob Numb");
                }
                datasend = new
                {
                    entity = "callmanager",
                    action = "init_call",
                    options = new
                    {

                        USER_PHONE_INNER = UserInnerNumb,
                        PHONE_NUMBER = data.PHONE_NUMBER,
                        B24_URL = co.Portal,
                        AXATEL_GUID = co.AxatelGuid,
                        CRM_ENTITY_TYPE = data.CRM_ENTITY_TYPE,
                        CRM_ENTITY_ID = data.CRM_ENTITY_ID,
                        CALL_GUID = data.CALL_ID,
                        TYPE = "CALLBACK"

                    }
                };
            }
            else
            {
                UserInnerNumb = func.GetUserInnerNumb(auth.domain, co.AcesTok, data.USER_ID);
                datasend = new
                {
                    entity = "callmanager",
                    action = "init_call",
                    options = new
                    {

                        USER_PHONE_INNER = UserInnerNumb,
                        PHONE_NUMBER = data.PHONE_NUMBER,
                        B24_URL = co.Portal,
                        AXATEL_GUID = co.AxatelGuid,
                        CRM_ENTITY_TYPE = data.CRM_ENTITY_TYPE,
                        CRM_ENTITY_ID = data.CRM_ENTITY_ID,
                        CALL_GUID = data.CALL_ID,
                        TYPE = co.Type

                    }
                };
            }

            string contentText = JsonConvert.SerializeObject(datasend).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("http://" + co.BackIp + "/api/token/v1/" + co.BackToken, contentText, "application/json").ToString();
                // content = request.Post("https://webhook.site/a1ff4420-166e-42c8-9639-184561dcca43", contentText, "application/json").ToString();
            }
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);

        }

        [HttpHead]
        public ActionResult Reciver()
        {
            return Content("ok");
        }

        public ActionResult Reciver(string DOMAIN, string AUTH_ID, string REFRESH_ID, string member_id = "")
        {
            Compan co = _db.Compans.Where(p => p.Portal == DOMAIN).FirstOrDefault();
            if (co != null)
            {
                if (!string.IsNullOrEmpty(AUTH_ID))
                {
                    co.AcesTok = AUTH_ID;
                    co.DTRefTok = DateTime.Now.AddSeconds(3600);
                    co.RefTok = REFRESH_ID;
                    co.MemberId = member_id;
                    _db.Entry(co).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
            }
            else
            {

                string content;
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Get("https://" + DOMAIN + "/rest/event.bind.json?event=OnExternalCallStart&handler=http://service-axatel.ru:8098/clicktocall&auth=" + AUTH_ID).ToString();
                    System.Threading.Thread.Sleep(100);
                    content = request.Get("https://" + DOMAIN + "/rest/event.bind.json?event=OnExternalCallBackStart&handler=http://service-axatel.ru:8098/backtocall&auth=" + AUTH_ID).ToString();

                }


                Compan comp = new Compan();
                //if (content.Contains("true"))
                //{
                //    comp.EventSub = 1;
                //}
                //else { comp.EventSub = 0; }
                comp.AcesTok = AUTH_ID;
                comp.DTRefTok = DateTime.Now.AddSeconds(3600);
                comp.DTSetApp = DateTime.Now;
                comp.Portal = DOMAIN;
                comp.RefTok = REFRESH_ID;
                comp.MemberId = member_id;
                _db.Compans.Add(comp);
                _db.SaveChanges();

            }
            return RedirectToAction("MainPage", new { member_id = member_id });
        }

        //Закрытый метод для получения и записи Токенов авторизации
        public void RefSetToken(string portal)
        {
            Compan conm = _db.Compans.Where(i => i.Portal == portal).FirstOrDefault();

            // если токен свежий то выходим
            if (conm.DTRefTok.AddSeconds(-5) > DateTime.Now)
            {
                return;
            }

            xNet.HttpResponse resp2 = null;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {

                resp2 = request.Get("https://oauth.bitrix.info/oauth/token/?grant_type=refresh_token&client_id=" + client_id + "&client_secret=" + client_secret + "&refresh_token=" + conm.RefTok);

                //Если в ответ получаем статус-код отличный от 200, то это ошибка, вызываем исключение
                if (resp2.StatusCode != HttpStatusCode.OK)
                {
                    throw new FormatException("ErrorLogonBitrixOAuth");
                }
                else
                {
                    string stringLogonBitrixOAuth = resp2.ToString();
                    var converter = new ExpandoObjectConverter();
                    dynamic objLogonBitrixOAuth = JsonConvert.DeserializeObject<ExpandoObject>(stringLogonBitrixOAuth, converter);



                    //Записывем Токены авторизации в поля нашего класса из динамического объекта
                    conm.AcesTok = objLogonBitrixOAuth.access_token;
                    conm.RefTok = objLogonBitrixOAuth.refresh_token;
                    conm.DTRefTok = DateTime.Now.AddSeconds(objLogonBitrixOAuth.expires_in); //Добавляем к текущей дате количество секунд действия токена, обычно это плюс один час
                    _db.Entry(conm).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
            }
        }
        [HttpPost]
        public ActionResult GetOperators(string member_id)
        {
            if (string.IsNullOrEmpty(member_id)) return Content("err");
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("err");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            int count;
            List<dynamic> listoper = SearchDeals(out count, member_id);


            return PartialView(listoper);
        }

        public ActionResult RegLead(string AXATEL_GUID, string B24_URL, string PHONE_NUMBER, string LINE_NUMBER, string Lead_name, string INNER_PHONE)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--RegLead--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--PHONE_NUMBER:" + PHONE_NUMBER + "--LINE_NUMBER:" + LINE_NUMBER + "--INNER_PHONE:" + INNER_PHONE + "--Lead_name:" + Lead_name + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }

            string idlead = func.RegLead(B24_URL, PHONE_NUMBER, Lead_name, co.AcesTok, INNER_PHONE);

            return Json(new { result = "ok", leadid = idlead }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RegaCall(string AXATEL_GUID, string B24_URL, string LeadID, string Comment, string INNER_PHONE, string PHONE_NUMBER)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--RegaCall--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--LeadID:" + LeadID + "--Comment:" + Comment + "--INNER_PHONE:" + INNER_PHONE + "--PHONE_NUMBER" + PHONE_NUMBER + "\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }

            func.RegaCall(B24_URL, LeadID, Comment, co.AcesTok, INNER_PHONE, PHONE_NUMBER);

            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        List<dynamic> SearchDeals(out int count, string member_id)
        {
            List<dynamic> total = new List<dynamic>();
            count = 0;
            Compan comp = _db.Compans.Where(s => s.MemberId == member_id).FirstOrDefault();
            if (comp == null) return total;

            RefSetToken(comp.Portal);

            bool flag = true;
            int shag = 0;

            var data = new
            {
                filter = new
                {

                },
                select = new[] { "ID", "NAME", "LAST_NAME", "PERSONAL_MOBILE", "" }
            };
            string contentText = JsonConvert.SerializeObject(data).ToString();

            int i = 0;
            while (flag)
            {
                System.Threading.Thread.Sleep(100);
                string content;

                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Post("https://" + comp.Portal + "/rest/user.search?start=" + shag + "&auth=" + comp.AcesTok, contentText, "application/json").ToString();
                }
                System.Threading.Thread.Sleep(100);
                var converter = new ExpandoObjectConverter();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
                if (i == 0)
                {
                    count = Convert.ToInt32(obj.total);
                }
                i++;
                try
                {
                    if (obj.next == null)
                    {
                        flag = false;
                    }
                }
                catch
                {
                    flag = false;
                }
                shag = shag + 50;
                total.Add(obj);
            }
            return total;
        }
        [HttpPost]
        public ActionResult FaxAtt(string INTAPP, string AXATEL_GUID, string B24_URL, string CALL_GUID, string FILE_CONTENT, string PROCES_STATUS)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try {
                myfile.WriteLine(DateTime.Now.ToString() + "--FaxAtt--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--FILE_CONTENT:" + FILE_CONTENT.Substring(0, 10) + "--PROCES_STATUS:" + PROCES_STATUS + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();

            myfile.Dispose();
            RefSetToken(B24_URL);
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            // создаем папку
            var converter = new ExpandoObjectConverter();
            var addfold = new
            {
                id = 7,
                data = new { NAME = "FaxAxatel" }

            };
            try
            {
                string contentText2 = JsonConvert.SerializeObject(addfold).ToString();
                string content;

                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Post("https://" + co.Portal + "/rest/disk.storage.addfolder?auth=" + co.AcesTok, contentText2, "application/json").ToString();
                }

                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
                int idfold = Convert.ToInt32(obj.result.ID);
                co.IdFolder = idfold;
                _db.Entry(co).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            catch
            {

            }
            // заливаем файл
            string tdstr = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(FILE_CONTENT);
            string newb64 = "";
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                MemoryStream newms = new MemoryStream();
                Image image = Image.FromStream(ms, true);
                image.Save(newms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imagenewBytes = newms.ToArray();
                newb64 = Convert.ToBase64String(imagenewBytes);
            }

            var addfile = new
            {
                id = co.IdFolder,
                data = new { NAME = tdstr + ".jpg" },
                fileContent = new[] { tdstr + ".jpg", newb64 }
                // fileContent = new { DETAIL_PICTURE = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoHBwYIDAoMDAsKCwsNDhIQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQMEBAUEBQkFBQkUDQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBT/wAARCABpAo0DASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD9OqKWigBNtLRRQAUUlA6UAFLSUtACUm2nUUAJSbaGpaAEaloxR6UAGKWiigBKWiigBKKWigBMD0opaKAEwPSloooASilpKAFpKKWgAooooASsH/hAvDJtdYtT4d0k2usyNLqUP2GLZeyHktMNuJCfVs1v0UAYOgeBPDfhW7nudH8P6XpNxPEkEstjZRQu8aDCIxVQSqjgA8DtUGrfDbwnr13e3WqeF9G1K6volgup7vT4ZXuI1IKpIWUllBVSAcgED0rpaT0oAxpvBug3GtWGrzaHpsuq2ERhs75rWMz20ZGCsb43IMZ4BA5qxqXhvStXvtPvb7TLO9vdOdpLO4uIEkktmIwzRsQShI4JGM1pUUAczb/DXwjb2yW0PhbRIreO8/tBIY9OhCLdf89wu3Ak/wBvr71S1X4K/D3XtSuNR1PwH4a1DULhzJNd3ejW0s0rHqzOyEk+5NdnRQBh/wDCD+HVm0mUaBpgk0jI05/sce6yyMHyTt/d5HB24qxrnhvSPEgtP7X0qy1QWky3Nv8AbbdJvJlX7sibgdrDsw5FalFAFDWND07xFpk+m6rYWuqadcLsms7yFZYpF64ZGBBH1FNuPD+lXWivo0+m2k2kND9mbT5IEa3MWMeWYyNu3HG3GMVo0lAHFX3wR+HWqPA954C8M3bwRLBE0+j28hjjUYVFynCgcADgCuj0fw1pHh3SRpelaTY6bpiggWVnbJFCM9fkUBf0rSpaAOd0H4d+E/C+oS32i+GNG0e+m/1lxY6fFBI/1ZVBP41e1XwzpGuXun3mo6VY393p8vnWc91bJI9tJjG+NmBKNwORg8Vp0UAZq+F9GXXm1waTYjW2gFq2pC2T7SYQciMyY3bc87c4qnrXgLwz4k1B73VfDuk6neyW7Wb3N7YxTSNA33oizKSUPdeh9K3hRQBw9n8C/hvpt5Bd2nw+8K2t3BIssU8Gi2ySRupyrKwTIIIBBHQitS4+G/hK8t7iCfwrok0Fzef2hPHLp8LLLdf892BX5pP9s/N710tFAGbp/hvSdJ1G/wBQsdLsrO/1Ao15dW9ukctyUGFMjAZfA4Gc4FaVJiloASlFJRQAetG2looAaAPSlpaKAEwPSilooATijaPSlooASloooAKSlooAZTqMUUAFG2looASjA9KWigBv8VLRRQAtJiiloATA9KWiigBKKWigBKKWigAopM0CgBaKSigBaSlpPzoATdS9aqX2rWWmruu7y3tR6zSKv86y4/H3hp5PLXX9OZ+yi6jz/OtPZVJapM5J4zDU3yyqJP1R0FJioLO+tr9N9tcR3C/3o3DfyNTZB5zUyi4uzN4VIVFeDv6C0tN/OlqTQWim0tAC0UUlABRSbqWgBaKbnmmTTxwoXkkRE/vM2AP1ppNuyJlJQV5OxLRWFceOvDtpJsm13T43Bxta6TP86t6f4i0vVMfZNTtLrH/PKZW/kav2VRa8r+44447DSlyqor+qNKim9uOadWZ3dLhRRRQAUUUUAFFFFABRSbqOpoFtqFLSevt19Ky77xPpGlnF5qtnbH/prOq4/M1cYTlsjGpiKVH+JNL1NWisO18beH75ttvrdhM3QBLlDn9TWxHIsihkdXVuhU5H505U5x3RNLFUK38OcX8ySik6HmiszpDFLSZpaACkxRRmgAxRj3pabQA6kpuad2oAbS7qrXmp2enqXurmG3TrmRwo/Wsr/hPvDTSbP7e00N6fakz/ADrVUqkldROOpjMPTfLUqJP1N3NO/Gq1nqFrqSB7W5huVPO6Jw/8jVnPFRKLi7M6IVIVI3g7i0UU2pNB1JiigUALRSUtABRRTaAHUUnamSSrEu53WNO7MePzp67ITkoq8nZDqWsO68beH7OTZca3p8T5+69ygP8AOrNh4m0jU2AtNTtLontFOrH9K09jUSvZ/ccSx2GcuVVFf1RqUU0NnGPzzRmsjuvdC4paZk0ZoAdR+NNp1ABj3ox70tFABSYpKA1ADqKTP+NZ194k0nS/+P3U7S1K9fNnVf5mrjCctkYVK9Oj/Fml6uxoUHtWHb+OPDt3IEh13T5X/urcoSf1rahmjmj3xusiY4KkEfnmnKnOO6FSxVCs7U5p+juP7UtIOgpazOgSlptLQAtFFFACUtFFABRSYqO4mS2heWVtkaKWZiegFNbkykoxblsVda1q00CwkvL2ZYYIxksx6n0A9a8K8ZfGTWNZkeDSWbTbMcb+PNf3z2+g5pPHXiOfxhqpkLFLKLIgi9B3Y+5rmv7OIwMYxX1OCwdKnHnqq7Z+OZ9n2Kxc5UMLJxgtLrqc5dw3N9MZbqaa4lY5Z5WLMSeuc1F/Zp/u11H9m4HTp7Vr2fgPVbrTX1BLNjZxgu0jYX5R1PJ5717jrwgtdD89jllbEydryfXdnGac19pE6y2V1NZyr8ytExQ//Xr1bwV8ar21kjtdfX7VAeBeIvzr/vAdR7iuH/s3HBAo/s3viuevSo4hWmj18uxGNy2cZ0KjXk9j6gs7yHULWO4t5VlhkAKyIQQanrxL4a+KJvDt8tlcOx0+dsHdz5Tn+L6e1e28Y46dq+MxOHeHqcvQ/dcpzGOY0FO1pLdBS0zNO7VyHti0UlLQAlNZtoJyAByc9qfXmXxW8YS26nR7CQq7rmeReuP7gPYmt6NF15qCPNx+OhgaEq0+m3qVvHvxlTSZpNP0NFurpSQ9ywyiH0UfxH9BXi2uazrHiSYyalfz3BPRXc7R7BRxWr/Zm3gCpLfQ5ryZYoInllbgIi5Jr7TD0aGGjtqfhGZY3H5tVtOTt0iv61OS/s3ttp8di8TBkLIw5DKcEV6bF8I/EckauNOYAjOGkQH8ic1iX/hm80ic295bSW8w7OOv0rpjiqVR8sWjx6mS4rDx9pOEool8J/E7xD4ZlVGuX1G0XG6C4O8hfZuo/GvePCPjOw8YWPnWjFZUH72F/voff1HuK+eP7Pz2Bq/od1d+H9SivbOTy5UOTzwR3U+ua8zGYOliE5RVpH2GS51i8vkqdaTlTv1Ppmis3QNZh1/SoL2D5Q4+ZSfuN3WtEdK+QknF2e5+206ka0FUjswopaTFSaC0UUnNAeghrl/Gvj6w8G2o80me9cHyreM/Mfc+g9zV/wAWeI4/DOjy3TLvlPyxR/32PQf1r551M3OsX0t5dyGeeU5ZvX/63oK9XA4NYh89T4T4zP8AO55fH2OG+N9ewnir4jeIvFUjiS7e0tWzi2tyUGO2T941xzaeWbc2Wb+8etdUNNPpn/OK2Yfh1rk8aOmmTsjAEMqjBHrX1kalHDqysj8YqYbG5nNzm5TfzPPf7N79D9K29A8Q614ZlD6fqE8C55j3Eo31U8flXVf8K315eulXH/fIrLvdBn064e3uYWgnXqj9R/kVTrUqy5dGTHLsZgvfXNDzPV/Afxdg16SOx1VUsb88LIOI5Pz+6frXpHpXy0unlGBUEMDmvafhh4sk1S1/sy+ctcwAeW/eRPTPqK+Zx2BhT/eUdj9Y4ez6pXthsa7vo+53v5UtJ+OeetLXhH6IFJS0UAJRSUMyxozscIoySTjgDk0dRN2V29Crqep22j2Ul3dzLBbxjLO3b2968O8Z/GrU9UeS30NfsFryPOwDLJ7+i/zpPiD4nn8W6k0aMw0+E4iQcb/VjXJ/2b7V9RgsDTglUqq7Px/P8+xOKnLDYR8sVpddTnbwXepTNNd3Et1Ixz5krlyfxNQf2eewx+FdT/Z59Ku6V4WvdamaGytmuJFGSEA4Huc19A60KavskfnUcvrV6lruTfzOSsftemyiW0uZrWUHIeJ9p/SvUvBnxo1HTWjttcX7bbHgXCjEqe57MPpzXMah4dudKuGtrqIwzgAlGIOM9M1W/s/24rmrRo4iNmtz1cDVx2VVb0ZtW6M+ntP1C21azjurSYTQSAFWQ5qz+FeE/DrxRN4V1JYZnLabOf3kfXYc/fH417qrBgCpDL1yK+NxWHeHny9D90yjM45lQU9pLdDqKKK4j3RKKWigBKQnqeozzQf0rz34peLpdNtv7KsJCt1Mv72RTzGh9Pc9K2o03VmoxODHYyGBoSrT6FXx98YIdDd7DSVW7vgdrTH/AFUZ79PvH2rxLXfEGteJJS+o3802ekZfag+gHFaP9nZYnGT61Jb6PJczRwxIXlkYKqjqSfSvtMPQoYaNkte5+DZpjsdmtR802o9Ejkv7M5yBT109kYMuVYcgjgivQ1+G2vNz/ZVwP+A5plx8P9atYXmm06eOJBudiuAB3/lXV9bpPTmTPIWR4qn+8UJepR8L/EfxF4XkjVbt72zXANtdEuMf7JJyPz/CvePBnjqw8ZWpaA+RdR/621c/Mv49x718+/2cecrg96uaS9zouoQ3dpIYpoWBVu30PtXmYrCUq8W4q0j6rJc4xmWyUJy5qfZn00v0payPC+vx+I9Hiu0+SQ8SR/3WHX8K1c18hKMou0j9vo1Y1oKpB3TFalopak2CkFJSUbAO4rnfGPjbT/BliJrt987g+Vbp99z+PQe9XfEmuw+HdImvJfmK8JH3ZiOB/WvnjWLi71/UJr68dpJpDz2CjsBXp4LCe3fNP4T4/Ps5eXQ9nh/4j/APFnxO8ReJmdRcnT7NuBb2zFOPduprimsXkYs+XY925NdR/Zp9Pyo/s3jpjt04FfZU/ZUVamrI/D8SsVjZupXqOTZy403nOK19E1zWPDk4k0+/ntyDnYr5VvYr0rp9P8B6rqVuZ7exlaBQSZGG1fXvyay10/coOMg81PtqdX3XZihgMTg2qkbxfzR6v4C+MSatJHY60i214cKtwh/dufQ/3T/kV6eG3LkV8s/2dz05/wAK9h+Ffi6W7hGkX8m6WNcwSt/Eo6qfcV83j8FCN6lLY/WeHs9rVZLC4x69GejUtFFeAfpHoJRS0UAFFFFACVx/xK1BodHS0j+/cN83+6uDXY155461G0uPEkOmtcRi7S3EiwswDFWYjI/EV2YSPNVXlqeHnNZUcHK7tfQ8/XTeB8vFO/s0D+Gun/s0/rTjp3tmvofa2Z+afUdOa2hb8I+A7R7Yarq0saWqjeqMwA45yx9K6TxNqlprfwo1m807/jyk0+fyCq7RtCsAR7HGRXnfiPwm/iSzhsJr+6h07zPMms4ZNqT8dG9RntXoupafDpvwju7WBAkMemOqqOgATGBXl1780ZylfU+sy60aVSjSp2Sjv1bPIdO03/QbbjJ8tf5VP/Zvsa6HTNO/4l9qe/lr/IVY/s6vU9rbc+TjgbxRy40/B4XnIIP0r2LwfqB1DQrdnJMkX7p8n06H8QRXDf2cfce+OK6TwBfWssmo2cM8ck0BjeVI3BMe4EDd/wB81wY29Wk5dj6PJLYXFezcrc3Q7DbWLr3jTQvC95p9pquq2un3OoSeTaxTyBWmfjhQT7gZ9SBxkA7VeVfH/wCCdt8XvDqNCfI8QaerPYz7sBu/lt6AnGD2Iz614J+ho9V+lFfOH7Nvx2vtQ1Z/h14x8yPxNY74ra4kHNysYJZG6/OqgnP8QGeoNfSFAyve3QsrOa4bpGhb8q8KvLeTULua5mBaSVy5zz1Oa9c8eapbaT4bnkuplghdli3scDLMB/WuDhsxPGssbLJG3Ikj5Uj1zXtYBOEXNI+C4hnDEVY0E7WV7HNf2b/sVs+G/EL+CWu7wWT3imI4iiX94zAEgKT6mr/9nc9aX+zs54r0J1FUXKz5ulh6lCanT0ktihovxm8c32opcXXh7TbXSmI/0fzn+0Kueu77vA7Y/Gu6+LVvBJ4Lk1FgN9u0bqxGD8zBSPyb88Vg6R4fOpajFDt3DcCxI42jrmpvjhqS3FvpHhi2+efULhJJVB5WGJg5J9MsFH5158403WiqMbH0VKdf+z68sXNzvtfv5HDnTs9BSf2d9a6kabtXHXtR/ZvtXput1PmVgLrYvfDO6azvZrJifLmG9cn+If8A6/0r0Y5rzPS7i20fWdPeeeOBpZlij3tgszfLgV6aO47V4WNjapzW3P0HIqvNh3S5r8r+4q6lqlro1hcXt7cR2tpbxmWWaVtqIoGSWJ7f4VBoHiDTfFWk22p6RfQ6hp9wN0NxA+5XHrn1znjqADTfEnh+x8VaFe6PqcP2iwvImhmjz1U9vr0x9K+SbHWNc/Y9+Ih0y/abUvAWqyblYAkqOAZAM8SIPvL3GPavPPpD7JoOfWq+m6jb6vp9rfWkomtbqJZ4ZF6OjAMpH1BFWPqccUB6nlPxHum1TWhADmG1XaB6seSf5D865QaZ/s/pXS/bLXX728uLW4SfEzq4QglTk5B9Kk/s/wB/619VTl7GCgfkWKgsbXnWTvdnLf2YOPl9v1zW/N8WPF9jJHZ6b4bs7y3jVUikku2RnwAOmz1q3/Z1dJ4L0e1+1vdXLIDCR5asQMt1zWNapC3NONzoweGrqooUp8l+pfPjC78N+C59c8Xx2thLEhkNvbuWC8cJub7zHjpivENHuNU8VXWo6/qsfkSalL5kVvj/AFcQGEU++0DNew+PvhtJ8QdSsXudUJ0y0cTR6ekeEkkHRmbPzY7Y6Vgz6G9lM8EgAZSM49MY/KscJOlBNr4md2bUcTXlGM2/Zw2/vPuzkv7O/wBmrui79J1S3u4wQ0ThuO47itv+zT6UklkkMbvIyoijLMx4A9fau72nOnDueNTwvsZe1va2p6nHIJokkT7jgMPoafWX4XvodR8P2U9vKk8TR7VdDkHGR+PStWvlpx5ZNdj9aozVSnGa6oxLHxpoWp+I73QLTVbW41mxUNc2SSAyRg8glc/T8/cZ2v1r5g/aE+EuqeDPED/FHwLI9vqMEn2jULdeQePmkxkZU87l79fp618D/jHYfGbwf/aVuhtdQtWWC+tT/wAspSM8Hup6g+nHWoNj0WuX+IeoPZ6C8MR/eXBCDnsOTXU1wPxC1S0Gr2OnS3McdyYmljjc7S2Wxxnr0rqwsPaVUrXPHzasqODm27X0+887/szH8PFH9m/7NdQdPP4e1KNNOOT+lfR+21PzD6ipO6WhzUOjyXEyRRRl5HICqB1Nez+D/Dtt4UtIbQlTfXILuT1bGMgewyKy/DOi2+gWcmsaiywIilg0hwEXuTnv6Vz/AMOdf1Lx746vvEEqtBpEdu1vp9u4wShYFpD/AL2B+FeZiKssQnyv3UfU5bhaWXzhKcb1JvTyRzHjW3a4+KXiHOSqxW2F7D5Kp/2b/s102v2pm+JPiN/WO3xx/wBM6f8A2dnoK76dZRpwS7Hg4jBuWIqzevvM5b+z/Y1614D1Br3QY4nbMsH7onvjt+lcd/ZuO3HrXQeA7y3jv72xjnjecIsrRKdzLg4BOK5MY3VpN22PVyZLC4qMOa3MdnnLVjeI/GmieEPsR1rVLXTBeSiCA3MgXzH5+Uep/ljqK2l/P3rzf45fB2x+MXhf7JMwg1OzLPYXTDKxsQNyt/stgZ9MDr0rwT9H31PRwdwBBB+h4px718w/s7/GzVdF8SH4XeORJHrVq/2exuJcszEDIikbJzxyjZ5HB7V9O0dbARzzrbwvK5wqAsfoBmvCtUWTVtQnu5cmSRi3ToP/ANVeveNNSt9J8NX9zdSiC3VVR3YjA3EL/UV59DZpcRrJE6yRt0dDlT+NezgFyxdRHwnEUo1qkMNzedjmhpmP4f0rR8O6fs17TiBjFzGf1rYXT+Kt6TZiLVbNyQAsyE59MivRnWvFo+aw+C5asZ2tY6P4l/ES68A/2Ylno76xNfOyiNZRHs2gEnkV514n+MHiPxH4d1DTB4Olt2uoWiE32xTs3DGeBXqWqr4W8WalaCe/tbq6tGYRRx3IyCRhhgHr9azvEXgWGwt2u7MsY05aNjnA9jXl0JUIJKpF8x9TmFHH1nOWGqrktayt21PKrXS2jtYkcfOqAN9cVN/Zv+z7V066aO3T2pf7OPpmvX9trsfJrAKKRN8Obk6dqjWp/wBVcL0PZhkj869NrzC1mttFv7Ke4njg3Toi+Y20sxO3A9eten14eNj76nbc+8yGovYOjzX5X9xHd3UNjazXNxIkNvCheSWRtqooGSSegA659qoeGfFWk+MtLTU9F1CDU7B2ZVuLdw6kg4IyOhHoatatpdprel3mnX0Yms7qFoJo26FGXBB9jn8a+P7863+xz8QlngEupeAtWk+aLPzYB7ZPEidc/wAQ/Tzj6c+y8eowabVLQdcs/EmjWWq6dN59jeRJNDKARuVhxwenFXuKOgn5nmfxOu31DUIbJcmK3XcR/tH/ACPzri103b/D+ldTfX9preuamLa5jmlhuXglRW+ZGU7cEde1H9mkZHT2xX09JujTUT8lx0VjcTOqnfU5b+zcfw13fgPwHbyRnVdRRTbqf3cb8KcfxH27fUVRtdHNxcRRf32C/nXeeMfDM+veFzolneHTopwsM1wg+dYcfME9yMD6E+lc+JxDdoJ2uellmWJynXlG/LsulzzLxV8d73VL2fSfA2lR6jHCfKm1K6BW1U9CqD+P88Vzek6Pc2um28N0Ua4VQJDGDtJz2yScfjXrmk/CvQtN0tLPTneJYVChgVPT1wKwNT8PyaXeGBzkgbgy8ZX1rSjWow92nGz79zlxmBxtVqvipKS6JbI43+zfarGnwyafew3UW4PEwYY9jXRf2afSmtYrHyzbAo3Ek4wPWul1Ob3TgWF9i1O9rfgeoWlwt5axTIflkUMMe4/+tUmaxvBt9BqHh61ktpUnjUtGHQ8fKSv6EVt/hXy9SPLJqx+tYep7ajGone6CloptZnQOooooASvmX9qCzltvGGlXyFl8y08tJF4wVcnr9GzX03XlP7Q3hF/EXgn7bbo0lzpj+dtUZJTGHA/DDfhXr5VVjSxcXPZ6M+G40wVTHZLVjS+KPvaeR4NoXxe1/RVVHmW/t14CXQyfoGHP55rutM+PmnybVv8ASZ4G7tC4dfyOK8QHY45xRtA6DFfotTL8PX3R/LuE4mzPBaRnddnqfR9r8ZvCFwo3XU0BP8Mlu5/9BBr1DWtUtbv4TX99DJus3015Fk2n7uzOcHn868C+Dmo/Dqx8N3I8ZSacl6103lC8Tc3l7ExjjpnNevf8Lp+FQ0U6T/b+mf2b5XkfZRnZsxjbjHTHavh8fQjGryUoSdnvY/oPhvMKtfCOvjMRTXPHRXs0/M4aH40eErXT7fbcTzsI14jhb098Vjan+0DYR7hp2kSzt2aaQIPrgZP61lfGfxB8NZvDVlB4Lk019TN4iutmhDCPa+c8dM4ryf8AWvosFgcPiIe1lFryZ+YZ/wAQZnldf6rTqxatvDU7LxB8XPEOvqyC4Wxgbgx2g28ehb71es/sr2Mi6br2oOxInmjjDNnJKgkn/wAer502NIwVVLs3CqvU9OPxr7R+EvhU+DvA9hYygC5YedPgYy7fMR+A4/Csc89lhsL7KCtc6uAFjM0zh4uvNyjTTvd9Tsa8K/am+N2ofC/R9N0bw/ayz+JdeZobSVU3LCPukgfxPkjaPXntivdaguLC2vJIJJ7aGeS3fzIXkQMY2xjcpPQ47ivz8/p3zPD/ANmv9n9vh1Zf8JJ4lP2zxjfqXlkkYubVW6ruycuc/Mw+nSvdmzx2p3p7dKTtQB5/8dtPfUPhjqyoMmMJOeM8KwJP5fyr5Y0HxtrXhlv9BvXSPr5Mg3If+An+lfbWr6dHq2lXllPzDcwvE49mGDXw54k0G48M69faXdAia2laPp94dmHsetfccPzp1YSoTVz+ePEqjicLi6OY0ZOMWrO3dbHpmkfH6RVC6lpKyt3ktpNp/wC+Tn+ldZpfxn8NalNDAY7yGeVgqo0BYknjA2k8186cV6x8B9X8GeGby71fxNqNvZ3kbCO1+0HhcjlgO59+1ejjsFh6NOVRQbl5Hy3D/EWZ43GU8JVrJRe7lbRevc+i9Q1jS/hv4Xutb1aYW0McYd2Yc89FAxyc9hXh2i/Frw7qmsX3iTV79vt918kFuIJG8iEH5VGVxnHX3ruPE3xE+DfjSSCTWta07U1g5ijmlkaJW/vbPu7vfGa5fxr4k+DUfg7VP7Fm0Y6iLaT7MkSHd5m07ccdc18xhKaTtVpyu+vY/Xc6r1akU8FiaShBXs3q2iLUfj1oNup+yWN5dHtuAjH59f0rjtc+O2s3yslhaw6bGRjd/rH/ADOB+leYwtviRiBkgE8U4fKfavt6WWYeDva5+AYzizNMVeLqWXlod38L5L7xR8VNCkurmW7mW4EzSSMWwFBb8BkCvsgV85/sw+EWkvr/AMQzpiKFfs1vuHVjgsR+GB+Jr6M/Wvh88rQqYnkp7R0P6B8PMJXo5U8RiG26jvr2MTxt4rtvAvhLVfEF5FLPa6bbvO8UK5dwP4R9SR9M+1fJvws8D+IP2qPG0nj7xsHh8J27lLLT8nZKoOREvqgz8zfxHjnBx9lyRpNG0ciLJGylWVhkEHqCPSmWtnBY28dvbQR28Ea7EiiQKqL6ADgCvnj9RCGGO1hSGJFiijUIiKAAqgYAAHQVJnPHfp/OlpMde1GpMo8ya7nxH4qmvPCfjzWVtbiW0nhvJMNG2043ZHHcciuk0T45axYgJf28OpKOrY8t/wAxxn8K2P2lPCL6V4oi1qJD9l1BAkjAcLIBj9Rg/hXjv3ic1+qYaFDG4WEmuh/GuaVsw4fzStSpzcbN27NM970/48aBMq/a7K7tHbrgLJ+uR/KruofEfwF4htVhv7x3hDbvLkgkGCOP4Qa+d/0Xvzivo/w7Z/B7VfDem22o6ho5u7eFY5JZbn7NIzY+bJ3Lnn615uNwmHwiUlGTv26H2PD+d5pnLnSdSnHlX2tLnR/B/W/DFxrk9j4dvZ5AYDJJbbJRGACBuAZcA8jp61B8WviXpfgnxlBb3sckslxY+bshUdQ5Azk9+n4USfE74VfB/R5l0jULGV5DkW+myi5lmbsCwJ/U4r5j8UeMb/4ieLL/AMSXymATgRW8GcmGEE7QfU9c/WvLwOBeMxPtHFqHmfW5/wAQyyXKo0PaxniG/s6pHqGsftBSFSmm6SsJ7SXUmT+Q4/WvPfEPjrWvEzH7dfO8Q/5YR4SP/vkdawsfh7V0HgHwzN4y8W6bpkYJSSUNKw/gjBG4/kMV9YsNhsJBztsmfics3zPOa0MNKo3zNKy8z65+Fentpfw78P27cMLRHI9CwLY/8erq6jt4kt4UijG2NFCqPQAYFSV+VVZ+0m5dz+zcFQ+rYanRe8Ul9yPj/wCOXjzxT8bvifP8J/B0U1pp9nNt1W7bKhyMZLHtGPT+Mj3GfpD4W/DHSPhT4Xg0XSV3c7ri7IG+5l6Fm9PYdhwOK6iPT7WG8muo7aFLqYKss6xgO4XO0M3UgZOM9M1PWR2hXzr+1PYPHqug36HbvikiDjjaykEc/wDAv0r6KxXnfx28Jv4q8B3P2dN95YkXMQHJbH3l+uM16mW1lRxUJS2PjeLsDUx+T1qdL4krr5HzloPxY1/QVVBcLe269I7obuPTd96vQPD/AMf9Me7txq2mvaxb13yxP5igZGTjGfyzXhq0o+XpxX6PWy/D1k/dt6H8sYHibM8va5J3S6PU+rNQ/aT+FWqWv2W+1H7TCODFNp07rx7GPFdN8O/ix4H8b6jLp3hi4WW5hi8xoUs5IcJkDOWRR1NfFflr/dH5V6n+z34t0XwV4m1C81m+h061e22LJMcAtuBxx9K+cxmSU6FCU6cm32P1HI/EDFY7MKWHxUIRi3Zva3zPbfiF8ZvAngXVdQ0/UblIde8gOyLYySM2UOzLKhHT1NeUH9oS0NjA1tpE01y0Y3NKwjXdjngZOM/SuG+M3iTS/F3xS1bUtJvI76xaGBFmj+6SFINchtC8bcD0xXXl2U0fYxqVLtvozxOJeNMcsdVw+FaUItpNdfM7fxD8YPEOuK8aTrp1ueNtquGx6bjz+Vekfss2ckl14h1CQs4xHEWY53NyTz34xXgC55wM9h/n8a+w/gj4Rbwn4DtEnQpd3ZNzKpGCCw4B/wCA4H1qs59lhcI6cFZsOBPrmb50sRXm5Kmr6/gegV43+058bbn4M+Ebb+yrRrjX9Yka3sX2FkhIC5Y8EE/Nwvckehr2SoLzT7XUPK+1W0Nz5LiSPzow+xh0YZ6Eeor869T+o99T5+/Zp/Z/ufCufGnjPde+Lr7MqrcsXNoGJyzH/noRjJ/hzgc19ELRgelLQGvQ434waa2q/DXX4EGWFsZNvrtIYfyr5G0Hxhq3hqQHT72SJM5MR+ZD/wABP8xX3Jd26XlvLBKu5JFKEdiD1/rXw9418NzeEfE+oaVMrDyJCI2YcFCcq3vwR+Rr7Ph+VOop0Jo/n/xMo4nC1qOY0JNK3K7dzv8AR/j5PCEXU9LjuMdZLd9h/wC+TnP511dl8cPDFwAs6XdqzDkPFkf+Ok1884HpxXZfCZvDkfi5JfFFxb2+lxxN/wAfX+rdjhQDkY7k9e1e7isBh405VeXbsfA5TxPmtbEQwvtE+Z2vJafeemN4s+Gc28l4UkJLFhaSbgT3yF4Ne36Hf2d14FiullaWwNuziSXIYx84J3c9B3rzu30v4K2Fx/aC3+gFk+bH29XUY9EL4/SuD+N37Rum69oNz4W8GyNc/a18m51CMFIo4z95UJHJYfLuHAB9eK+SlR+uVIwowkvU/aKOO/sTD1q+YVqe2iju2SWPx90QaLZstpd3VyYlDcKg3d+Sf6Vz2tfHrVbwMmnWUOnqRgM58x/1wP0ryyGIQwpEDwoxT1719rTyvD09WrtH4Ji+LczxS5VU5Y+Wh1/g+61Dxd8RdEW8uprudryN8ysSQqncSOw4HavtMjBwa+ZP2Z/CL3/iS416ZMW9ipihYjhpGHP5D+dfTi18VntSDxCp01ZRP3jw5wlanlssVXbvUel+yM7xJrlv4Z8P6prF2JGtdOtpLqZYV3PsRCzAAd8CvjzwL4f8R/tf+Pn8U+IxNp/gWwkMdtZhiBIAQfLXpljkFn98DtX2oVDAgjI9Pr1qCzsLbTbZLe0t4rW3QYWGFAiKMk8AcDkn86+cP1kLCwttLsrezs4I7e1t0EcMUahVRRwAB2FTEcGl9KSgmWqaPjH4kfa/DfxN15raeS2nN3JMrRvtOHO7Pv1rS0P44a1pu2O+ig1KMd3Hlv8AmP8ACur/AGnvCT2+rWevwrmC4QW8+B0dclSfqCB+FeHHI/Gv1HBxo43Cxcl6/I/j3PKuYcP5xXo0puKcm16PU+h/CPxx0bUNZ0+C4sbmznknjQNkOgYsBkkY4BxXdfHi+jtdB0sXmoT6bpFxdrFdy26sSy7WZVO0EgEjr649a+QI3aORXQlWU5B/ungg/nX0x4M+Ovhbxj4bXRfGUtvZ3BQRSfbBiGcD+IN0BPXB6V4uY5b9XqxrUotxW9j7/hfimWZ0K2AxtRQqS+FvRP1F+GOueDIfE9pa+HryY3c25TCsc22QBSTu3ADt19q3PjR4+07wLrGgG/EsjXaXAEcIBLBdh5yR3P6mqdj4m+EXwnt7nU9P1PS1mYbc2tz9plbvtUBmPPtivnD4hfEK6+K3jCTWpYmtNPgj8iwt5OqpnJdvcntXNhcH9cxSmotQXc9jNs8eRZQ6U6sZV5PRR1SPRtW/aAJRl0zSAh/563Umcf8AAR/8VXn3iDx9rnij5by+YQEnFvCAkf4gdfxrntoHbmtbwr4fn8VeIrHSrcN5lzKEZlH3V6lvwGfyFfWrC4fDQcrbH4hUznNM2rRoSqO8nayPrP4Iae2m/C/RInUruiaUA/7Tlh+jCu67iq2m2MWmWNvZwDEUEaxIvQAAYA/SrVfldep7SrKfdn9l5dh3hMHRoSd3GKX4BSUtFYHoiUClpKADrTJo1mjZHUOjAqVboR3FPpMkZxxTV76EySaakrpnyR8ZvhXP4E1aS8s4mk0Wd8oyjPksedjeg9PavNRX3zqGnW+rWctrdwpcW0o2vHIAVZe4Oa8C8efs0yiWS78MTb0OSbC4YAj/AHH/AKMPxr7zLc6puCpYl6rqfzhxZwDiKdaeMyuPNB6uPX5Hz/JGknDorD/aGaZ9ig/54x/98iuh1jwPr3h6QpqGk3VuQeXMTFfwPNZCwyOwVUdn/uhST+VfUxrUZK8ZJn41UwOLoy9nKnJfJkC28UZBWJFI6EKKfz2/+tXR6D8O/EniSZUsdHupVJx5jpsQe5JwP1Ne2/D39nG30yaG/wDEcq306kMtnH/qgf8AaP8AER+H41w4rMsPho3lK77H0WUcKZrnFRezpuMespaJHN/AX4Sy6tfQeItVgK2EJ32sDjmZx/EQf4R+vWvpb7vA6U2GNYY1jjRUjUbVVQAAB0FPr82xmMqY2o6k/kf1dw/kOHyDCLDUtZdX3YtJS0VwH0wlIadSUAJXj/x2+E7+KrX+2tJiDapbqFliXgzxj0/2h29q9ipprpw+InhaiqU90eNm2V4fOMJPCYhaS/B9z8/pIzHIysCrg4IYEEfhUckSTAB1DAdAwzivrX4kfAvS/GrvfWR/szVGB3SouY5T6OPX3H414B4k+D/irwxIfP0qW4hH/LxajzEI/wCA5I/Kv0rB5tQxUVd8r7H8oZ1wbmeT1JPkc4dJR1+9dDhfscH/ADyj/wC+RR9jg/55R+v3RVya1mt22zQyRMP4XUqauab4d1XWJFjsdOurstx+6hY/r0r1XVppe9JHyEcLiZy5Y05X9GZwwoAHFdJ4D8C6h4+1yKxsoz5YIM9wQdkSepPr7V3vgv8AZv1vWJI59cYaTZ/eMYIaZx6Y6L+JNfRPhfwlpvg7S0sdJtlghXqRyzN/eY9Sa+dx+dUaMHCg7y7n6jwzwFjMfVjiMfF06S6Pd/Im8N+H7TwrolrpllGEt7dAox1Y45J9z1P1rTH50vuDx2or87lKU5OUnqz+oKNKFCnGlSjaMdBBR3FLRUmwUmKWigOpgeNfCNp428P3OlXa8SDMUneOQDhgfXn+dfGfizwnf+DdZl07UImjkQ/K4HyyLnhlPvX3Vjgiuf8AGHgXSfHenm01S3Dkf6uZR88ZPcN/SvdyvMngZcktYs/NeL+EYcQU1Voe7WitG9mfDm6oZrOC4YmSJHJ6kivXvGH7OniDQ5JJNKC6zZ5yvlkLKo9Cp4P4H8K811DRNR0mQx3thc2jDjE8TL/Sv0OjjMPiY80JfJn8xYzJMyy2o6dejKLXZfqZMen20LZjhRT9Ksenep7ayuLtwkEMs7n+GNC38hXaeF/gn4s8UTIU057C3Ygm4vAY1C+oB5P5VdTE0KMbzmkYYbK8wx1RQoUpSb8mcTb28l3cRwQRtLK7BURQSWY9AK+rvgj8L/8AhBdKa9vlX+2LsDzO/kpwQn9TVz4c/BnSPAKi6f8A0/VMc3UoHyZ7ID0+vevRNuOMY7Yr4bNc3+tL2NDSP5n9FcGcE/2TJY7Hq9Xou3qH6fWilpK+V8z9k16i0UmKWgBtBUNwQMHjDd+KWloE1zKz2PlP43fCeXwnqc2r6dDv0a4YsyoP+Pdjzggdjnj0ryj1Fffd5Zw6hay29xCk8EilGjdcqQeua8J8ffs1i4mlvfDEyxliWawnPHXPyN2+hr7nLM7hyqlidGup/OvFvANb20sZlkbxlq49fkfPOaRo1kGGAI9639Z8A+IvD8jJqGj3cAH/AC08osv/AH0uRWJ5T7gmxt/ZcZNfXRrUpr3ZJo/E6uBxVGXJUptNdLMhSFIc7FVe3yjFP7dh9a6DRPh/4i8RSBbDSLqUNj940e1B+JwP1r2j4ffs1pbTR33iaZZpBhxYQ52g/wC23U/QcVxYnMsNhYtylfyPfyrhbNM4qqFKm0u70SOU+Bvwll8UajDreqW5XSLdt0ccox9oYdMD+6D1r6kGBjHTtTLe3is7eOGCNYYYxtSNF2hR6ADpU1fmuOx1THVXOfyP6u4dyChw/hFQpayesn3f+QnpSU6krzz6oKWiigBteW/HD4Vnxvpa6jp8Y/ti0XKoBjz0/uk+vXH416n3FIfmznntXTh688NUVSm9UeXmeXUM0ws8LXV1I+AJoJbaaSKVGjkjba6uuCp7gg1GVVlwVBHoea+vfiR8FNI8eFruM/2dq+MC4jUYf/fHf618+eJvgt4s8MyPv02S+t16TWf70N+A5H4/nX6Tg82w2KSu+V+Z/KGd8F5lk1STpwc6fRr9UeeNpVm7Z+yx56/cqeGGOAfu0VPoMVZmtZrVik0MkTj+GRCDViw0PUdVkWOzsLm6djgLDGW/lXre1p2vzKx8U8Ji5yUPZyb9GUf0Nbng7wfqPjXWodO0+Lcx+aSRgdkSZ5Zj/Su98G/s6+INcaOfVgNHsurBvmlYem3+H8T+FfRfg/wXpXgfTFstLtgg4LzHl5DjqzDrXzuPzqlh4uNF80j9O4a4CxmY1I1cdH2dLez3ZJ4N8K2fgvQbTSrNcRxD5pOjSP8AxMfrW16dqUYx6iivzuc5VJOU3ds/qLD0aeGpRo0laKWgtJS0VJ0CUjU6ij1DyMfxR4Zs/Fuh3Wl3qb4Z1xuHVT2YehB718aeN/BOoeBtaksL6Ntu79zOq/LKnqPf2r7jrC8WeD9L8aaY9jqdsJ4znY+PmQ+qt2r3MszKeBlyy1iz864u4Tp8Q0VUg7VY7Pv6nwxTZI1mBDAMvcEV7B4z/Zy13RZJJ9GP9r2fJCbgsy/nw3868wv9A1TSXKXunXVoV6+dCy/zFfodHG0MRG8J3P5ex+QZnldT2eIotW6pafIx49NtYm3rboreuBVkYAx0HpUsNrNcyCOGGSdzxtjUsf0rsPDfwb8VeJpEMOlyWsBIzPd/u1A9eeT9AK0qYihSjeUkkc+HyzH46ap0aUpN+TOMjjeaRI40aSRiFVAMkk9OlfUvwJ+FLeEbH+2NTjxqt0oCxt96BCB8vsx71f8Ahv8AA3SfA/l3t039o6qB/r3UBIz/ALAPT6mvTMEdf06V8TmucLEr2OH+Hv3P6E4M4IeVzWOzBfvOkd7CZop1FfJH7SgzRRS0DG9hS0nYUlAD6SiloASilpp+8KAEZVbgqCB7U37PDnPlRk/Snr/FQtVzSWzMJUKUndxX3CBQvQAUo+979KGoXrSu3uaqKirJC8UtMp1IoWkpaZQA+ikpaAEopB9406gAphXOeKfRR5idnoyJreJusUZP0pURUPyoq/QU/wDiobvVc0u5j7CkndRX3BnmlpvpTqk2sFFFFAxKWmU6gBaSlplADs0U2l9KAFz7ZpjRpJ95Fb6inr96hqd2tiJQjP4lcYsESfdjVT9BTwPakX7wpW70Xb3ZMKcIfDFIWikpaRqFJS0ygB9FJS0AJRmhqbQA7ilpKWgBjKG6qCPcU37PDkHyUz9KmH3TR6VXNLozKVGnN3lFP5DFXaMBcD6Uv4UjfeNLU+bNFFQVooDRmkP3TSUDH0UlLQAUlLSNQAtJRQO9AC9sdqQ/QUtNal5iaT0YxoY2+9Gh+opwhRRhUVfoKc1Ieoq+aXcx+r0k7qK+4Slob7xpak3FooooASlplOoAWkpaRulACZpabTqAGnvSeUjZ3Irf7wFLSjrTTa2JlThP4lcYIY4/uxqv0AqRenAFIv3j9aX+I03Jvdkwo06esI2DpRTadUmgelLSdxS0AFFFFAH/2Q==" }
            };
            string contentsend3 = JsonConvert.SerializeObject(addfile).ToString();
            string content3;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content3 = request.Post("https://" + co.Portal + "/rest/disk.folder.uploadfile?auth=" + co.AcesTok, contentsend3, "application/json").ToString();
            }
            List<int> ids = new List<int>();

            string content4;
            using (xNet.HttpRequest request4 = new xNet.HttpRequest())
            {
                content4 = request4.Get("https://" + co.Portal + "/rest/user.search?auth=" + co.AcesTok).ToString();
            }
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content4, converter);
            foreach (var item in obj4.result)
            {
                ids.Add(Convert.ToInt32(item.ID));
            }

            foreach (var items in ids)
            {
                System.Threading.Thread.Sleep(200);
                using (xNet.HttpRequest request5 = new xNet.HttpRequest())
                {
                    string cont = request5.Get("https://" + co.Portal + "/rest/im.notify.json?to=" + items + "&message=Вам пришел факс!&auth=" + co.AcesTok).ToString();
                }
            }


            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EmailSend(string AXATEL_GUID, string B24_URL, string HTML_TEXT, string HTML_TITLE, string Email)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--EmailSend--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--FILE_CONTENT:" + HTML_TEXT.Substring(0, 10) + "--HTML_TITLE:" + HTML_TITLE + "--Email: " + Email + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential("axatel_no_reply@axata.by", "Ln83RzmB68mMRtn");
            MailMessage message = new MailMessage();
            message.From = new MailAddress("axatel_no_reply@axata.by", "Axatel");
            message.To.Add(new MailAddress(Email.Trim()));

            message.IsBodyHtml = true;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = HTML_TITLE;
            message.Body = HTML_TEXT;
            try
            {
                client.Send(message);
                return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", desc = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult SmsSend(string AXATEL_GUID, string B24_URL, string PHONE_NUMBER, string SMS_TEXT)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--SmsSend--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--PHONE_NUMBER:" + PHONE_NUMBER + "--SMS_TEXT:" + SMS_TEXT + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            try
            {
                string content;
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Get("https://userarea.sms-assistent.by/api/v1/send_sms/plain?user=Aksata&password=5VFH3a8y&recipient=" + PHONE_NUMBER.Trim() + "&message=" + SMS_TEXT.Trim() + "&sender=Axata").ToString();
                }
                return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", desc = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Notify(string AXATEL_GUID, string B24_URL, string[] USER_PHONE_INNER, string MESSAGE)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Notify--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--USER_PHONE_INNER:" + USER_PHONE_INNER + "--MESSAGE:" + MESSAGE + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            Compan co = _db.Compans.Where(p => p.Portal == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            RefSetToken(B24_URL);
            try
            {
                List<string> iduser = func.GetIdUsersB24(B24_URL, co.AcesTok, USER_PHONE_INNER);
                using (xNet.HttpRequest request5 = new xNet.HttpRequest())
                {
                    foreach (var item in iduser)
                    {
                        System.Threading.Thread.Sleep(300);
                        string cont = request5.Get("https://" + co.Portal + "/rest/im.notify.json?to=" + item + "&type=SYSTEM&message=" + MESSAGE + "&auth=" + co.AcesTok).ToString();
                    }
                }
                return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", desc = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BlackList(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();

            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            // обрываем парсинг
            Axatel.Controllers.ParsContController.Statuspars stat = ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault();
            if (stat == null)
            {
                ParsContController.stadeal.Add(new ParsContController.Statuspars { Portal = comp.Portal, flagdeal = false });
            }
            ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = false;
            // обрываем парсинг
            List<BlackList> blist = _db.BlackLists.Where(i => i.PortalId == comp.Id).OrderByDescending(d => d.Date).ToList();
            ViewBag.Memb = member_id;
            return View(blist);
        }
        public ActionResult AddNumb(string member_id, string numb)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            BlackList bl = new BlackList();
            bl.Date = DateTime.Now;
            bl.Numb = numb.Trim();
            bl.PortalId = comp.Id;

            _db.BlackLists.Add(bl);
            _db.SaveChanges();

            return Content("ok");
        }
        public ActionResult DellBlackList(string member_id, int id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            BlackList bl = _db.BlackLists.Where(i => i.Id == id).FirstOrDefault();
            _db.Entry(bl).State = System.Data.Entity.EntityState.Deleted;
            _db.SaveChanges();
            return Content("ok");
        }
        [HttpGet]
        public ActionResult Tiket(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();

            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            // обрываем парсинг
            Axatel.Controllers.ParsContController.Statuspars stat = ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault();
            if (stat == null)
            {
                ParsContController.stadeal.Add(new ParsContController.Statuspars { Portal = comp.Portal, flagdeal = false });
            }
            ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = false;
            // обрываем парсинг
            ViewBag.Memb = member_id;
            return View();
        }
        [HttpPost]
        public ActionResult Tiket(string email, string title, string text, string member_id)
        {
            if (email.IndexOf('@') == -1 || email.IndexOf('.') == -1 || email.Length < 6)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = "Укажите E-mail для обратной связи!";
                ViewBag.Title = title;
                ViewBag.Email = email;
                ViewBag.Text = text;
                return View();
            }
            if (title.Length < 3)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = "Укажите заголовок обращения!";
                ViewBag.Title = title;
                ViewBag.Email = email;
                ViewBag.Text = text;
                return View();
            }
            if (text.Length < 3)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = "Укажите текст обращения!";
                ViewBag.Title = title;
                ViewBag.Email = email;
                ViewBag.Text = text;
                return View();
            }
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            try
            {
                Tiket tiket = new Tiket();
                tiket.Date = DateTime.Now;
                tiket.Email = email;
                tiket.IdPortal = comp.Id;
                tiket.Text = text;
                tiket.Title = title;
                _db.Tikets.Add(tiket);
                _db.SaveChanges();

                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential("axatel_no_reply@axata.by", "Ln83RzmB68mMRtn");
                MailMessage message = new MailMessage();
                message.From = new MailAddress("axatel_no_reply@axata.by", "Axatel");
                message.To.Add(new MailAddress(email.Trim()));
                message.IsBodyHtml = true;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject = "Ваше обращение №" + tiket.Id + " Принято!";
                message.Body = "<p>Ваше обращение <u>" + title + "</u> принято. В ближайшее время мы рассмотрим сообщение и Вам ответим.</p>" +
                                "<p>Текст сообщения: </p>" +
                                "<p><i>" + text + "</i></p>" +
                                "<p>С уважением, команда <a href=\"axata.by\">Axata.by</a></p>";
                client.Send(message);

                MailMessage message2 = new MailMessage();
                message2.From = new MailAddress("axatel_no_reply@axata.by", "Axatel");
                message2.To.Add(new MailAddress("vladimir@axata.by"));
                message2.IsBodyHtml = true;
                message2.SubjectEncoding = System.Text.Encoding.UTF8;
                message2.BodyEncoding = System.Text.Encoding.UTF8;
                message2.Subject = "Поступило обращение №" + tiket.Id;
                message2.Body = "<p>Поступило обращение от портала " + comp.Portal + "</p>" +
                                "<p>Дата установки: <i>" + comp.DTSetApp + "</i></p>" +
                                "<p>Аксател гуид: <i>" + comp.AxatelGuid + "</i></p>" +
                                "<p>Обратный IP: <i>" + comp.BackIp + "</i></p>" +
                                "<p>Обратный токен: <i>" + comp.BackToken + "</i></p>" +
                                "<p>Тип: <i>" + comp.Type + "</i></p>" +
                                 "<p>Внутренний номер: <i>" + comp.InerNumb + "</i></p>" +
                                "<p>Email клиента: <i>" + email + "</i></p>" +
                                "<p>Заголовок: <i>" + title + "</i></p>" +
                                "<p>Текст сообщения: <i>" + text + "</i></p>" +

                                "<p>С уважением, команда <a href=\"axata.by\">Axata.by</a></p>";
                client.Send(message2);

                ViewBag.Memb = member_id;
                ViewBag.Ok = "Ваше обращение принято!";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = ex.Message;
                return View();
            }
        }
        [HttpGet]
        public ActionResult Intro(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 1)
            {
                return RedirectToAction("MainPage", new { member_id = member_id });
            }
            ViewBag.Memb = member_id;
            return View();
        }
        [HttpPost]
        public ActionResult Intro(string member_id, string fio, string email, string telef)
        {
            if (email.IndexOf('@') == -1 || email.IndexOf('.') == -1 || email.Length < 6)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = "Укажите E-mail для обратной связи!";
                ViewBag.Fio = fio;
                ViewBag.Email = email;
                ViewBag.Telef = telef;
                return View();
            }
            if (fio.Length < 7)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = "Укажите Фамилию Имя Отчество!";
                ViewBag.Fio = fio;
                ViewBag.Email = email;
                ViewBag.Telef = telef;
                return View();
            }
            if (telef.Length < 7)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = "Укажите свой номер телефона!";
                ViewBag.Fio = fio;
                ViewBag.Email = email;
                ViewBag.Telef = telef;
                return View();
            }

            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 1)
            {
                return RedirectToAction("MainPage", new { member_id = member_id });
            }
            try
            {
                RefSetToken(comp.Portal);
                var data = new
                {
                    fields = new
                    {
                        TITLE = "Заказ телефонии Axatel",
                        NAME = fio,
                        STATUS_ID = "NEW",
                        COMMENTS = "Имя портала:" + comp.Portal + "\r\n" + "Id портала:" + comp.Id + "\r\n" + "Сылка активации:(Не открывать!!!) https://service-axatel.ru:8099/activportal/" + comp.Id,
                        PHONE = new[] {
                    new{ VALUE = telef, VALUE_TYPE= "WORK"}
                    },
                        EMAIL = new[] {
                    new{ VALUE = email, VALUE_TYPE= "WORK"}
                    },

                    },
                    @params = new
                    {
                        REGISTER_SONET_EVENT = "Y"
                    }

                };
                string contentText2 = JsonConvert.SerializeObject(data).ToString();
                string content;
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Post("https://axata.bitrix24.ru/rest/1619/iwx9yian2g83d8i3/crm.lead.add", contentText2, "application/json").ToString();
                }
                SmtpClient client = new SmtpClient();

                MailMessage message2 = new MailMessage();

                message2.To.Add(new MailAddress("vladimir@axata.by"));
                message2.IsBodyHtml = true;
                message2.SubjectEncoding = System.Text.Encoding.UTF8;
                message2.BodyEncoding = System.Text.Encoding.UTF8;
                message2.Subject = "Заявка на портал Axatel";
                message2.Body = "<p>Поступила заявка от портала " + comp.Portal + "</p>" +
                               "<p>Id портала " + comp.Id + "</p>" +
                                "<p>ФИО клиента: <i>" + fio + "</i></p>" +
                                "<p>Email клиента: <i>" + email + "</i></p>" +
                                "<p>телефон: <i>" + telef + "</i></p>" +
                                "Сылка активации:(Не открывать!!!) https://service-axatel.ru:8099/activportal/" + comp.Id +
                                "<p>С уважением, команда <a href=\"axata.by\">Axata.by</a></p>";
                client.Send(message2);
                ViewBag.Ok = "Ваше обращение принято!";
                ViewBag.Memb = member_id;
                return View();
            } catch (Exception ex)
            {
                ViewBag.Memb = member_id;
                ViewBag.Error = ex.Message;
                return View();
            }
        }
        public ActionResult ActivPortal(int id)
        {
            Compan comp = _db.Compans.Where(i => i.Id == id).FirstOrDefault();
            if (comp == null)
            {
                return Content("<h2>Нету такого портала!</h2>");
            }
            comp.Activ = 1;
            _db.Entry(comp).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Content("<h2>Портал Активирован!</h2>");
        }

        public ActionResult DealApp(string URL, string authorization_token, string started_at, string operator_id, string client_id, string direction, string duration, string operator_phone_number, string client_phone_number, string audio_source_url)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--DealApp--URL:" + URL + "--authorization_token:" + authorization_token + "--started_at:" + started_at + "--operator_id:" + operator_id + "--client_id: " + client_id + "--direction: " + direction + "--duration:" + duration + " --operator_phone_number: " + operator_phone_number + "--client_phone_number: " + client_phone_number + "--audio_source_url: " + audio_source_url + " --\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            var datasend = new
            {
                authorization_token = authorization_token,
                started_at = started_at,
                operator_id = operator_id,
                client_id = client_id,
                direction = direction,
                duration = duration,
                operator_phone_number = operator_phone_number,
                client_phone_number = client_phone_number,
                audio_source_url = audio_source_url
            };

            string contentText = JsonConvert.SerializeObject(datasend).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post(URL, contentText, "application/json").ToString();
            }
            //string pach2 = System.Web.Hosting.HostingEnvironment.MapPath("/otvet.txt");
            //System.IO.StreamWriter myfile2 = new System.IO.StreamWriter(pach2, true);
            //try
            //{
            //    myfile2.WriteLine(" --\n\n--"+ content+ " --\n\n--");
            //}
            //catch
            //{
            //    myfile2.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            //}
            //myfile2.Close();
            //myfile2.Dispose();
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Log(string USER_PHONE_INNER, string PHONE_NUMBER, string AXATEL_GUID, string AXATEL_TOKEN, string TYPE)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Log--USER_PHONE_INNER:" + USER_PHONE_INNER + "--PHONE_NUMBER:" + PHONE_NUMBER + "--AXATEL_GUID:" + AXATEL_GUID + "--TYPE:" + TYPE + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OnScript(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp.IsScript == 0)
            {
                RefSetToken(comp.Portal);
                var data = new
                {
                    PLACEMENT = "CALL_CARD",
                    HANDLER = "https://service-axatel.ru:8099/script"
                };
                string contentText2 = JsonConvert.SerializeObject(data).ToString();
                string content;

                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Post("https://" + comp.Portal + "/rest/placement.bind?auth=" + comp.AcesTok, contentText2, "application/json").ToString();
                }
                comp.IsScript = 1;
                _db.Entry(comp).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Writer", new { member_id = member_id });
            }
            else
            {
                RefSetToken(comp.Portal);
                var data = new
                {
                    PLACEMENT = "CALL_CARD",
                    HANDLER = "https://service-axatel.ru:8099/script"
                };
                string contentText2 = JsonConvert.SerializeObject(data).ToString();
                string content;

                try
                {
                    using (xNet.HttpRequest request = new xNet.HttpRequest())
                    {
                        content = request.Post("https://" + comp.Portal + "/rest/placement.unbind?auth=" + comp.AcesTok, contentText2, "application/json").ToString();
                    }
                }
                catch {
                    return Content("Обновите приложение!<br>В левой колонке - Приложения - Установленные - Обновить.");
                }
                comp.IsScript = 0;
                _db.Entry(comp).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Writer", new { member_id = member_id });
            }

        }
        // страница создания скрипта
        public ActionResult Writer(string Id = "", string member_id = "", int status=0)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();

            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            // обрываем парсинг
            Axatel.Controllers.ParsContController.Statuspars stat = ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault();
            if (stat == null)
            {
                ParsContController.stadeal.Add(new ParsContController.Statuspars { Portal = comp.Portal, flagdeal = false });
            }
            ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = false;
            // обрываем парсинг

            List<Script> sc = _db.Scripts.Where(i => i.IdPortal == comp.Id).OrderBy(i => i.Title).ToList();
            if (Id != "")
            {
                ViewBag.Text = sc.Where(i => i.Id.ToString() == Id).FirstOrDefault().Text;
                ViewBag.Title = sc.Where(i => i.Id.ToString() == Id).FirstOrDefault().Title;
                ViewBag.Id = sc.Where(i => i.Id.ToString() == Id).FirstOrDefault().Id;
            }
            if (status== 1)
            {
                ViewBag.Error = "<p style=\"color: red; \">Заполните пустые поля!</p>";
            }
            ViewBag.Memb = member_id;
            ViewBag.IsOnscript = comp.IsScript;
            ViewBag.IdPortal = comp.Id;
            return View(sc);
        }
        // страница скрипта для загрузки в карточку
        public ActionResult Script(string member_id, string id="")
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();


            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            // обрываем парсинг
            Axatel.Controllers.ParsContController.Statuspars stat = ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault();
            if (stat == null)
            {
                ParsContController.stadeal.Add(new ParsContController.Statuspars { Portal = comp.Portal, flagdeal = false });
            }
            ParsContController.stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = false;
            // обрываем парсинг
            List<Script> sc = _db.Scripts.Where(i => i.IdPortal == comp.Id).OrderBy(i => i.Title).ToList();
            string Text = "";
            if (!string.IsNullOrEmpty(id))
            {
                Text = sc.Where(i => i.Id.ToString() == id).FirstOrDefault().Text;
            }
            ViewBag.Text = Text;
            ViewBag.Memb = member_id;


            return View(sc);
        }


        // удаляем скрипт
        public ActionResult DellScript(string Id, string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            Script script = _db.Scripts.Where(i => i.Id.ToString() == Id).FirstOrDefault();
            _db.Entry(script).State = System.Data.Entity.EntityState.Deleted;
            _db.SaveChanges();
            Directory.Delete(System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/" + script.Id),true);
            return Content("ok");

        }
        [ValidateInput(false)]
        public ActionResult AddScript( string Text, string Title, string member_id, string Id )
        {

            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            if ( Text.Length < 6 || string.IsNullOrEmpty(Title))
            {
                return RedirectToAction("Writer", new { member_id = member_id, status = 1 });
            }
            if (comp == null) return Content("<h2 style=\"text-aligncenter; text-align:center; margin-top: 30px; \">Доступ в приложение разрешен только лицу установившему приложение.<h2>");
            if (comp.Activ == 0)
            {
                return RedirectToAction("Intro", new { member_id = member_id });
            }
            if (Id != "")
            {
                
                Script script = _db.Scripts.Where(i => i.Id.ToString() == Id).FirstOrDefault();
                Text = Text.Replace("/Content/imgscript/now/"+ comp.Id+"/", "/Content/imgscript/"+Id+"/");
                script.Text = Text;
                script.Title = Title;
                _db.Entry(script).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                //создаем директорию под файлы соответств ид скрипта
                if (!System.IO.Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/" + script.Id)))
                {
                    System.IO.Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/" + script.Id));
                }
                string[] files = Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/now/" + comp.Id)); // читаем файлы из временной папки 
                foreach(var item in files)
                {
                    string filename = Path.GetFileName(item);
                    System.IO.File.Move(item, System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/" + script.Id+"/"+ filename));
                }

                return RedirectToAction("Writer", new { Id = script.Id, member_id = member_id });
            }
            else
            {
                Script nscript = new Script();
                nscript.Text = Text;
                nscript.Title = Title;
                nscript.IdPortal = comp.Id;

                _db.Scripts.Add(nscript);
                _db.SaveChanges();
                 nscript.Text = Text.Replace("/Content/imgscript/now/" + comp.Id + "/", "/Content/imgscript/" + nscript.Id + "/");
                _db.Entry(nscript).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                //создаем директорию под файлы соответств ид скрипта
                if (!System.IO.Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/" + nscript.Id)))
                {
                    System.IO.Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/" + nscript.Id));
                }
                string[] files = Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/now/" + comp.Id)); // читаем файлы из временной папки 
                foreach (var item in files)
                {
                    string filename = Path.GetFileName(item);
                    System.IO.File.Move(item, System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/" + nscript.Id + "/" + filename));
                }
                return RedirectToAction("Writer", new { Id = nscript.Id, member_id = member_id });
            }
        }
        
        public ActionResult LoadImgScript(HttpPostedFileBase upload, int idport=0)
        {
            
            if (upload.ContentLength > 5000000)
            {
                return Json(new { uploaded = 0, fileName = upload.FileName, url = upload.FileName, error = new { message = "Превышен размер файла! Максимальный размер 5Mb!" } });
            }

            string newfm = Guid.NewGuid().ToString();
            string sign = "";
            try
            {
                sign = upload.FileName.Split('.')[1];
            }
            catch
            {
                return Json(new { uploaded = 0, fileName = upload.FileName, url = upload.FileName, error = new { message = "Непонятное имя файла!" } });
            }
            string pacth = System.Web.Hosting.HostingEnvironment.MapPath("/Content/imgscript/now/"+ idport +"/");
            if (!System.IO.Directory.Exists(pacth))
            {
                System.IO.Directory.CreateDirectory(pacth);
            }
            string[] badname = new string[] { "JPG", "JPEG", "BMP", "TIFF", "PNG", "GIF" };
            if (!badname.Contains(sign.ToUpper()))
            {
                return Json(new { uploaded = 0, fileName = upload.FileName, url = upload.FileName, error = new { message = "Запрещенный файл!" } });
            }
            string urlfile = "/Content/imgscript/now/" + idport + "/" + newfm+"."+ sign;
            upload.SaveAs(pacth + newfm + "." + sign);
            
            return Json(new {uploaded=1, fileName = newfm + "." + sign, url = urlfile   });
        }















        public ActionResult Test()
        {
            //List<string> users = func.GetIdUsersB24("b24-r6qifz.bitrix24.by", "a09f745e00440798004487aa0000000100000356728105f4dd27831dc732462125ba64", new string[] { "205", "308" });
            //Service.Ansver ans = func.RegisterCall("0", "0", "b24-r6qifz.bitrix24.by", "+375442234242", "2020-03-19 21:54:05", "2", "1", "0", "7285735e00440798004487aa0000000100000344a11ebd3f77228aae53d524621da003", "205");
            // string users = func.GetIdUserB24("b24-r6qifz.bitrix24.by", "117c765e00459f96004487aa000000010000033020d8f792f4f4d4c38a95f8014e3911", "205");
            //func.FinishCall("externalCall.74c80b29f269526e3d63629f814904c5.1584821585", "b24-r6qifz.bitrix24.by", "205", "76", "200", "Звонок состоялся", "9", "2020-03-20 17:34:09", "117c765e00459f96004487aa000000010000033020d8f792f4f4d4c38a95f8014e3911", "1");

            //    var data = new
            //    {

            //        INTAPP= "N/A",
            //        AXATEL_GUID= "1cad413e-e3ff-4454-8010-979d5b6b6480",
            //        B24_URL= "b24-r6qifz.bitrix24.by",
            //        CALL_GUID= "externalCall.aa924b0b98d62b90e2f6a00294375a0a.1584868953",
            //        FILE_CONTENT = "AAAAAAAAAAAAAAAAAAAAAAAAAAA",
            //        PROCES_STATUS = "1"

            //};
            //    string contentText = JsonConvert.SerializeObject(data).ToString();
            //    string content;
            //    using (xNet.HttpRequest request = new xNet.HttpRequest())
            //    {
            //        content = request.Post("http://localhost:62415/metod/recordatt", contentText, "application/json").ToString();
            //    }
            //    return Content("ok");
            //    return Json("6", JsonRequestBehavior.AllowGet);



            var data = new
            {
                CALL_START_DATE = "2020-04-15 11:13:05",
                USER_ID = 1,
                PHONE_NUMBER = "+375291554486",
                TYPE = 2,
                SHOW = 0,
                CRM_CREATE = 1,
                LINE_NUMBER = "375173880981"

            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://b24-zs8f4b.bitrix24.by/rest/telephony.externalcall.register.json?auth=a712a85e00459f960046d88a00000001000003a61ee1f4e35989dbf680c374fe18e9b4", contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            List<object> resentry =  obj4.result.CRM_CREATED_ENTITIES;
            if (resentry.Count == 0)
            {
                return Content("Сущность есть");
            }
            else
            {
                return Content("Сущность создана");
            }
            //List<int> ids = new List<int>();
            //string content4;
            //using (xNet.HttpRequest request4 = new xNet.HttpRequest())
            //{
            //    content4 = request4.Get("https://b24-r6qifz.bitrix24.by/rest/user.search?auth=349fa65e00459f960046d88a0000000100000306b3101b984edea1bffa4ebc25d80e88").ToString();
            //}
            //dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content4, converter);
            //foreach (var item in obj4.result)
            //{
            //    ids.Add(Convert.ToInt32(item.ID));
            //}

            //foreach (var items in ids)
            //{
            //    System.Threading.Thread.Sleep(200);
            //    using (xNet.HttpRequest request5 = new xNet.HttpRequest())
            //    {
            //        string cont = request5.Get("https://b24-r6qifz.bitrix24.by/rest/im.notify.json?to=" + items + "&auth=845c985e00459f96004487aa00000001000003e157f1f6b34e54d9f772ddd9859b84c0").ToString();
            //    }
            //}
            //return Content("9");
        }

        public ActionResult Test2()
        {

            var data = new
            {
                CALL_ID = "externalCall.5fadeae90611fa01b7419ee44d8b1a44.1587976486",
                USER_ID = 1,
                DURATION = 30,
                STATUS_CODE = 200,
                FAILED_REASON = 1,
                RECORD_URL = "https://service-axatel.ru:8099/content/zapisvobrabotke.mp3"



            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://b24-zs8f4b.bitrix24.by/rest/telephony.externalcall.finish.json?auth=349fa65e00459f960046d88a0000000100000306b3101b984edea1bffa4ebc25d80e88", contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

            return Content("9");

        }

        public ActionResult Test3()
        {
           // string text = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("/text.txt"));
            var data = new
            {
                FILENAME = "123dfgdfhhhfgh45" + ".mp3",
                CALL_ID = "externalCall.5fadeae90611fa01b7419ee44d8b1a44.1587976486",
                RECORD_URL = "https://file-examples.com/wp-content/uploads/2017/11/file_example_MP3_5MG.mp3",                            
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://b24-zs8f4b.bitrix24.by/rest/telephony.externalCall.attachRecord.json?auth=75b1a65e00459f960046d88a00000001000003443ea803691b5d857bd958b526e4a42d", contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

            return Content("9");

        }
        public ActionResult Test4()
        {
            // string text = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("/text.txt"));
            var data = new
            {
                INTAPP = "N/A",
                AXATEL_GUID="ceb9906c-0a24-4d7b-a8f4-f6e8f381c362",
                B24_URL= "b24-zs8f4b.bitrix24.by",
                CALL_GUID= "externalCall.8b7dd0ba397d0232b5dbfbf65016f3a4.1587985399",
                FILE_CONTENT= "",
                URL= "http://service-axatel.ru:8098/50min.mp3",
                PROCES_STATUS= "1"
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("https://service-axatel.ru:8099/method/recordatt", contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

            return Content("9");

        }

        public ActionResult Test5()
        {
            // string text = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("/text.txt"));
            var data = new
            {
                AXATEL_GUID="ceb2354c-0a24-4d7b-a8f4-f6e8f381c225", 
                 B24_URL="b24.kzv.by",  
                 PHONE_NUMBER="+375291554493",
                 CALL_START_DATE="2020-04-15 11:13:05",
                 LINE_NUMBER="375173880981",  
                 TYPE="2",  
                 CRM_CREATE="1", 
                 PROCESS_STATUS="1"
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("http://localhost:62415/method/reg", contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

            return Content("9");

        }

        public class Auths
        {
            public string token { get; set; }
        }
        public class DataOtvet
        {
            public int timeout_seconds { get; set; }
            public string[] phones { get; set; }
            public string priority { get; set; }
        }
        public ActionResult Roistat(Auths auth, string numb)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Roistat--auth.token:" + auth.token + "--numb:" + numb + "\r\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--FinishCall--Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            RoisConfig rc = _db.RoisConfigs.Where(i => i.Token == auth.token).FirstOrDefault();
            if (rc == null)
            {
                return Json(new { success = "false", error = "Не верный токен авторизации!" });
            }

            RoistNumb roisnumb = _db.RoistNumb.Where(i => i.Number == numb.Trim()).FirstOrDefault();
            if (roisnumb ==null)
            {
                return Json( new { success ="false", error = "Ошибка! Нет соответствующих строк таблице номеров" });
            }
            List<RoisColler> roiscols = _db.RoisCollers.Where(i => i.IdRois == roisnumb.IdRois).ToList();
            if (roiscols.Count == 0)
            {
                return Json(new { success = "false", error = "Ошибка! Нет соответствующих строк таблице номеров переадресации" });
            }
            RoisData rd = _db.RoisDatas.Where(i => i.Id == roisnumb.IdRois).FirstOrDefault();

            int[] countgroup = roiscols.Select(i => i.IdGroup).Distinct().ToArray();

            List<RoisGroup> lstgroup = _db.RoisGroups.ToList();

            List<DataOtvet> datas = new List<DataOtvet>();
            foreach (var item in countgroup)
            {
                DataOtvet d_o = new DataOtvet();
                d_o.priority = lstgroup.Where(i => i.Id == item).FirstOrDefault().Priority.ToString();
                d_o.timeout_seconds = lstgroup.Where(i => i.Id == item).FirstOrDefault().TimeSec;
                d_o.phones = roiscols.Where(i => i.IdGroup == item).Select(q => q.Number.Trim()).ToArray();
                datas.Add(d_o);
            }

            return Json(new { groups = datas, auth = new { project = rd.Project, key = rd.Token } } );
        }


    }
}