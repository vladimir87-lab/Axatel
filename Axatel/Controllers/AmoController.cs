using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Axatel.Models;
using Axatel.Service;
using xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace Axatel.Controllers
{
    public class AmoController : Controller
    {
        Context _db = new Context();
        amoFunction func = new amoFunction();

        [HttpGet]
        public ActionResult LoadUser()
        {

            return View();
        }
        [HttpPost]
        public ActionResult LoadUser(string Portal, string Guid, string Token, string Backip, int[] iduser = null, int[] inernumb = null)
        {
            //  List<Function.UserGetApi> listusapi = new List<Function.UserGetApi>();
            
            amoCompan co = _db.amoCompans.Where(p => p.PortalName == Portal).FirstOrDefault();
            co.AxatelGuid = Guid;
            co.BackIp = Backip;
            co.BackToken = Token;
            _db.Entry(co).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();


            if (iduser != null)
            {
                for (int i = 0; i < iduser.Length; i++)
                {
                    int userid = iduser[i];
                    amoOperator oper = _db.amoOperators.Where(q => q.AmoId == userid).FirstOrDefault();
                    if (oper == null)
                    {
                        amoOperator opernew = new amoOperator();
                        opernew.AmoId = iduser[i];
                        opernew.InerNumb = inernumb[i];
                        opernew.Name = "Имя";
                        opernew.PortalId = co.Id;
                        _db.amoOperators.Add(opernew);
                        _db.SaveChanges();
                    }
                    else
                    {
                        oper.InerNumb = inernumb[i];
                        _db.Entry(oper).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                    }
                }

            }
            if (co == null)
            {
                ViewBag.Text = "Портал не найден или не верный AxatelGuid";
            }
            //else
            //{
            //    RefSetToken(co.PortalName);
            //    List<Operator> lstoper = _db.Operators.Where(o => o.PortalId == co.Id).ToList();
            //    listusapi = func.GetListApiUsers(co.PortalName, co.AcesTok);
            //    foreach (var item in listusapi)
            //    {
            //        try
            //        { item.InerNumb = lstoper.Where(i => i.AmoId == item.id).FirstOrDefault().InerNumb; }
            //        catch { };
            //    }
            //    ViewBag.Guid = Portal;
            //    ViewBag.Cont = 1;
            //}
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
            //return View(listusapi);
        }
        [HttpPost]
        public ActionResult ClickToCall(int userid, string tel)
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/logamo.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--ClickToCall--userid:" + userid + "--tel:" + tel + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            amoOperator oper = _db.amoOperators.Where(i => i.AmoId == userid).FirstOrDefault();
            if (oper == null) { return Content("оператор не найден"); }
            amoCompan co = _db.amoCompans.Where(i => i.Id == oper.PortalId).FirstOrDefault();
            string guid = Guid.NewGuid().ToString();
            var datasend = new
            {
                entity = "callmanager",
                action = "init_call",
                options = new
                {

                    USER_PHONE_INNER = oper.InerNumb,
                    PHONE_NUMBER = tel,
                    AMO_URL = co.PortalName,
                    AXATEL_GUID = co.AxatelGuid,
                    CALL_GUID = guid,
                    TYPE = "INTERCOM"

                }
            };

            string contentText = JsonConvert.SerializeObject(datasend).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("http://" + co.BackIp + "/api/token/v1/" + co.BackToken, contentText, "application/json").ToString();
            }
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Index(string code = "", string referer = "", string client_id="", string from_widget="")
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/logamo.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--Install--code:" + code + "--referer:" + referer + "--client_id:" + client_id + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();

            if (!string.IsNullOrEmpty(code))
            {
                var data = new
                {
                    client_id = amoFunction.ClienID,
                    client_secret = amoFunction.ClientSecret,
                    grant_type = "authorization_code",
                    code = code,
                    redirect_uri = amoFunction.RedirectUrl

                };
                string contentText = JsonConvert.SerializeObject(data).ToString();
                string content;
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Post("https://" + referer + "/oauth2/access_token", contentText, "application/json").ToString();
                }
                var converter = new ExpandoObjectConverter();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

                amoCompan co = _db.amoCompans.Where(p => p.PortalName == referer).FirstOrDefault();
                if (co == null)
                {
                    amoCompan comp = new amoCompan();
                    comp.AcesTok = obj.access_token;
                    comp.DTRefTok = DateTime.Now.AddSeconds(86400);
                    comp.DTSetApp = DateTime.Now;
                    comp.PortalName = referer;
                    comp.RefTok = obj.refresh_token;
                    _db.amoCompans.Add(comp);
                    _db.SaveChanges();
                }
                else
                {
                    co.AcesTok = obj.access_token;
                    co.DTRefTok = DateTime.Now.AddSeconds(86400);
                    co.RefTok = obj.refresh_token;
                    _db.Entry(co).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
            }

            //  Response.Headers.Add("Access-Control-Allow-Origin", "https://service-axatel.ru");
            return View();
        }
        public ActionResult Reg(string AMO_URL, string PHONE_NUMBER, string AXATEL_GUID, int VI_STATUS, string TYPE, string CALL_FINISH_DATE="", int? DURATION= 0, int USER_PHONE_INNER = 0, string URL = "")
        {
            string pach = System.Web.Hosting.HostingEnvironment.MapPath("/logamo.txt");
            System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            try
            {
                myfile.WriteLine(DateTime.Now.ToString() + "--RegCall--AMO_URL:" + AMO_URL + "--PHONE_NUMBER:" + PHONE_NUMBER + "--VI_STATUS:" + VI_STATUS + "--USER_PHONE_INNER: " + USER_PHONE_INNER + "--DURATION:" + DURATION + "--CALL_FINISH_DATE:" + CALL_FINISH_DATE + "--\n\n");
            }
            catch
            {
                myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            }
            myfile.Close();
            myfile.Dispose();
            // portal = "officenewbrowsergamesru.amocrm.ru";
            // telefon = "45634565465";
            // userid = "6021190";
            //string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            //System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            //try
            //{
            //    myfile.WriteLine(DateTime.Now.ToString() + "--RegCall--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--PHONE_NUMBER:" + PHONE_NUMBER + "--CALL_START_DATE:" + CALL_START_DATE + "--TYPE:" + TYPE + "--CRM_CREATE:" + CRM_CREATE + "--PROCES_STATUS:" + PROCES_STATUS + "--USER_PHONE_INNER:" + USER_PHONE_INNER + "--LINE_NUMBER:" + LINE_NUMBER + "--\n\n");
            //}
            //catch
            //{
            //    myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            //}
            //myfile.Close();
            //myfile.Dispose();
            //RefSetToken(B24_URL);
            //  Compan co = _db.Compans.Where(p => p.PortalName == B24_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();

            RefSetToken(AMO_URL);
            amoCompan co = _db.amoCompans.Where(p => p.PortalName == AMO_URL).Where(a => a.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            string otv = "";
            if (USER_PHONE_INNER == 0)
            {
                otv = func.GetUsers(co.PortalName, co.AcesTok);
            }
            else
            {

                otv = _db.amoOperators.Where(i=>i.InerNumb == USER_PHONE_INNER).FirstOrDefault().AmoId.ToString();
            }
            if (func.isHaveCont(co.PortalName, co.AcesTok, PHONE_NUMBER) == null)
            {
                func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv);
            }
             
            //  if (co == null) { return Content("Портал не найден"); }

            //func.ShowEvent(co.PortalName, co.AcesTok, telefon, userid);
            URL = "http://" + co.BackIp + URL;
            func.RegisterCall(co.PortalName, co.AcesTok, PHONE_NUMBER, otv,  VI_STATUS, TYPE, URL, CALL_FINISH_DATE, DURATION);

            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }
        // входящий звонок
        public ActionResult ShowEvent(string AMO_URL, string PHONE_NUMBER, int[] INNER_PHONE, string AXATEL_GUID)
        {
            //userid = new string[2];
            //portal = "officenewbrowsergamesru.amocrm.ru";
            //telefon = "45634565465";
            //userid[0] = "6021190";
            //userid[1] = "6028612";
            //string pach = System.Web.Hosting.HostingEnvironment.MapPath("/log.txt");
            //System.IO.StreamWriter myfile = new System.IO.StreamWriter(pach, true);
            //try
            //{
            //    myfile.WriteLine(DateTime.Now.ToString() + "--RegCall--AXATEL_GUID:" + AXATEL_GUID + "--B24_URL:" + B24_URL + "--PHONE_NUMBER:" + PHONE_NUMBER + "--CALL_START_DATE:" + CALL_START_DATE + "--TYPE:" + TYPE + "--CRM_CREATE:" + CRM_CREATE + "--PROCES_STATUS:" + PROCES_STATUS + "--USER_PHONE_INNER:" + USER_PHONE_INNER + "--LINE_NUMBER:" + LINE_NUMBER + "--\n\n");
            //}
            //catch
            //{
            //    myfile.WriteLine(DateTime.Now.ToString() + "Ошибка логирования--\n\n");
            //}
            //myfile.Close();
            //myfile.Dispose();

            RefSetToken(AMO_URL);
            amoCompan co = _db.amoCompans.Where(p => p.PortalName == AMO_URL).Where(a => a.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            //string otv = func.GetUsers(co.PortalName, co.AcesTok);
            //if (!func.isHaveCont(co.PortalName, co.AcesTok, PHONE_NUMBER))
            //{
            //    func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv);
            //}
            List<string> lstnumbuser = _db.amoOperators.Where(i => INNER_PHONE.Contains(i.InerNumb)).Where(p => p.PortalId == co.Id).Select(s => s.AmoId.ToString()).ToList();
            foreach (var item in lstnumbuser)
            {
                func.ShowEvent(co.PortalName, co.AcesTok, PHONE_NUMBER, item);
            }

            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RegContact(string AMO_URL, string PHONE_NUMBER, string AXATEL_GUID)
        {
            RefSetToken(AMO_URL);
            amoCompan co = _db.amoCompans.Where(p => p.PortalName == AMO_URL).Where(a => a.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }

            string otv = func.GetUsers(co.PortalName, co.AcesTok); // получаем ид любого админа
            string[] datauser = new string[3];
            string[] have = func.isHaveCont(co.PortalName, co.AcesTok, PHONE_NUMBER);
            if (have == null)
            {
                datauser[0] = func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта
                datauser[1] = "Новый контакт";
                datauser[2] = "0";

              
            }
            else
            {
                int iduser = Convert.ToInt32(have[0]);
                datauser[0] = have[0];
                datauser[1] = have[1];
                try
                {
                    datauser[2] = _db.amoOperators.Where(i => i.AmoId == iduser).FirstOrDefault().InerNumb.ToString();
                }
                catch
                {
                    datauser[2] = "0";
                }
            }
            return Json(new { idcont = datauser[0], namecont = datauser[1], inernumb = datauser[2] }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreatDeal(string AMO_URL, string PHONE_NUMBER, string AXATEL_GUID, string Link, int Duration, string Type,  int Cosht=0, string Tag="")
        {
            RefSetToken(AMO_URL);
            amoCompan co = _db.amoCompans.Where(p => p.PortalName == AMO_URL).Where(a => a.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            string otv = func.GetUsers(co.PortalName, co.AcesTok); // получаем ид любого админа
            string[] have = func.isHaveCont(co.PortalName, co.AcesTok, PHONE_NUMBER); // 0- ответсвенный, 1- имя контакта, 2- ид контакта
            List<string> lststatus = func.StatusDeal(co.PortalName, co.AcesTok);// список ид пропускных статусов сделок

            switch (Type)
            {
                case "incoming":
                    if(co.InCall == "none")
                    {
                        break;
                    }else if (co.InCall == "cont")
                    {                       
                        if (have == null)
                        {
                            func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                    }else if (co.InCall == "deal")
                    {
                        if (have == null)
                        {
                            have[2] = func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                        bool havedeals2 = func.ifhaveDeals(co.PortalName, co.AcesTok, Convert.ToInt32(have[2]), lststatus); // true если есть активные сделки
                        if (havedeals2 == false)
                        {
                            if (co.TagCall == 0) { Tag = ""; }
                             func.CreatDeal(co.PortalName, co.AcesTok, Cosht, Convert.ToInt32(have[2]), Convert.ToInt32(otv), Tag, PHONE_NUMBER);                           
                        }
                    }
                    else if (co.InCall == "neraz")
                    {
                        if (have == null)
                        {
                            have[2] = func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                        bool havedeals2 = func.ifhaveDeals(co.PortalName, co.AcesTok, Convert.ToInt32(have[2]), lststatus); // true если есть активные сделки
                        if (havedeals2 == false)
                        {
                            if (co.TagCall == 0) { Tag = ""; }
                            func.CreatRazobrab(co.PortalName, co.AcesTok, Cosht, Convert.ToInt32(have[2]), Convert.ToInt32(otv), Tag, PHONE_NUMBER, Link, Duration);
                        }
                    }
                        break;
                case "outgoing":
                    if (co.OutCall == "none")
                    {
                        break;
                    }
                    else if (co.OutCall == "cont")
                    {
                        if (have == null)
                        {
                            func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                    }
                    else if (co.OutCall == "deal")
                    {
                        if (have == null)
                        {
                            have[2] = func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                        bool havedeals2 = func.ifhaveDeals(co.PortalName, co.AcesTok, Convert.ToInt32(have[2]), lststatus); // true если есть активные сделки
                        if (havedeals2 == false)
                        {
                            if (co.TagCall == 0) { Tag = ""; }
                            func.CreatDeal(co.PortalName, co.AcesTok, Cosht, Convert.ToInt32(have[2]), Convert.ToInt32(otv), Tag, PHONE_NUMBER);
                        }
                    }
                    else if (co.OutCall == "neraz")
                    {
                        if (have == null)
                        {
                            have[2] = func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                        bool havedeals2 = func.ifhaveDeals(co.PortalName, co.AcesTok, Convert.ToInt32(have[2]), lststatus); // true если есть активные сделки
                        if (havedeals2 == false)
                        {
                            if (co.TagCall == 0) { Tag = ""; }
                            func.CreatRazobrab(co.PortalName, co.AcesTok, Cosht, Convert.ToInt32(have[2]), Convert.ToInt32(otv), Tag, PHONE_NUMBER, Link, Duration);
                        }
                    }
                    break;
                case "missed":
                    if (co.BadCall == "none")
                    {
                        break;
                    }
                    else if (co.BadCall == "cont")
                    {
                        if (have == null)
                        {
                            func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                    }
                    else if (co.BadCall == "deal")
                    {
                        if (have == null)
                        {
                            have[2] = func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                        bool havedeals2 = func.ifhaveDeals(co.PortalName, co.AcesTok, Convert.ToInt32(have[2]), lststatus); // true если есть активные сделки
                        if (havedeals2 == false)
                        {
                            if (co.TagCall == 0) { Tag = ""; }
                            func.CreatDeal(co.PortalName, co.AcesTok, Cosht, Convert.ToInt32(have[2]), Convert.ToInt32(otv), Tag, PHONE_NUMBER);
                        }
                    }
                    else if (co.BadCall == "neraz")
                    {
                        if (have == null)
                        {
                            have[2] = func.CreatCont(co.PortalName, co.AcesTok, PHONE_NUMBER, otv); // получаем ид контакта                           
                        }
                        bool havedeals2 = func.ifhaveDeals(co.PortalName, co.AcesTok, Convert.ToInt32(have[2]), lststatus); // true если есть активные сделки
                        if (havedeals2 == false)
                        {
                            if (co.TagCall == 0) { Tag = ""; }
                            func.CreatRazobrab(co.PortalName, co.AcesTok, Cosht, Convert.ToInt32(have[2]), Convert.ToInt32(otv), Tag, PHONE_NUMBER, Link, Duration);
                        }
                    }
                    break;
                default:                   
                    break;
            }           
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }


        //Закрытый метод для получения и записи Токенов авторизации
        private void RefSetToken(string portal)
        {
            amoCompan conm = _db.amoCompans.Where(i => i.PortalName == portal).FirstOrDefault();

            // если токен свежий то выходим
            if (conm.DTRefTok.AddSeconds(-10) > DateTime.Now)
            {
                return;
            }

            xNet.HttpResponse resp2 = null;
            var data = new
            {
                client_id = amoFunction.ClienID,
                client_secret = amoFunction.ClientSecret,
                grant_type = "refresh_token",
                refresh_token = conm.RefTok,
                redirect_uri = amoFunction.RedirectUrl

            };
            string contentText = JsonConvert.SerializeObject(data).ToString();
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                resp2 = request.Post("https://" + portal + "/oauth2/access_token", contentText, "application/json");

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

        //public ActionResult Test()
        //{
        //    // string text = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("/text.txt"));
        //    var data = new
        //    {
        //        AMO_URL = "xecoli8508.amocrm.ru",
        //        PHONE_NUMBER = "80261691950",
        //        AXATEL_GUID = "ceb9906c-0a24-4d7b-a8f4-f6e8f381c355"
                

        //    };
        //    string contentText2 = JsonConvert.SerializeObject(data).ToString();
        //    string content;
        //    using (xNet.HttpRequest request = new xNet.HttpRequest())
        //    {
        //        content = request.Post("http://localhost:8098/amo/method/reg", contentText2, "application/json").ToString();
        //    }
        //    var converter = new ExpandoObjectConverter();
        //    dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

        //    return Content("9");

        //}
        public ActionResult Test()
        {
            // string text = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("/text.txt"));
            var data = new
            {
                AMO_URL = "xecoli8508.amocrm.ru",
                PHONE_NUMBER = "80291554499",
                AXATEL_GUID = "ceb9906c-0a24-4d7b-a8f4-f6e8f381c355",
                CALL_FINISH_DATE = "2020-03-20 17:34:09",
                DURATION = 65,
                VI_STATUS = 6,
                TYPE= "inbound",
                USER_PHONE_INNER = "300",
                URL= "/callrecords/domains/axata.a1.axatel.by/data/20200520/mtezmjywnjk0ma53d5.mp3"

            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("http://localhost:62415/method/amo/finish", contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

            return Content("9");

        }
    }
}