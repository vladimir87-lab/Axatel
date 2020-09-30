using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Axatel.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using Axatel.Models;

namespace Axatel.Controllers
{
    public class AlfacrmController : Controller
    {
        Context _db = new Context();
        AlfacrmFunction func = new AlfacrmFunction();
        // GET: Alfacrm
        [HttpPost]
        public ActionResult Zvonok(string ALFACRM_URL, string PHONE_NUMBER, string AXATEL_GUID, string CALL_GUID, string TYPE)
        {
            AlfaCompan co = _db.alfaCompans.Where(p => p.PortalName == ALFACRM_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            List<AlfacrmFunction.Custumer> userinf =  func.GetCostumers(PHONE_NUMBER);                   
            func.Zvonok(CALL_GUID, TYPE, PHONE_NUMBER);
           // return Json(new { CREATED_ENTITY = "1", INNERNUMB = "301", CRM_INFO = new { Id = userinf.Id, Name = userinf.FullName BierzDay = userinf.BierzDay,  } }, JsonRequestBehavior.AllowGet);
            return Json(new { CREATED_ENTITY = "1", INNERNUMB = "300", CRM_INFO = userinf }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Active(string ALFACRM_URL, string PHONE_NUMBER, string AXATEL_GUID, string CALL_GUID, string TYPE)
        {
            AlfaCompan co = _db.alfaCompans.Where(p => p.PortalName == ALFACRM_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            func.Active(CALL_GUID, TYPE, PHONE_NUMBER);
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Close(string ALFACRM_URL, string PHONE_NUMBER, string AXATEL_GUID, string CALL_GUID, string TYPE, string USER_PHONE_INNER, string DURATION, string VI_STATUS, string finish_reason, string RECORD_URL)
        {
            AlfaCompan co = _db.alfaCompans.Where(p => p.PortalName == ALFACRM_URL && p.AxatelGuid == AXATEL_GUID).FirstOrDefault();
            if (co == null) { return Content("Портал не найден"); }
            func.Close(CALL_GUID, TYPE, PHONE_NUMBER,  USER_PHONE_INNER, DURATION,  VI_STATUS, finish_reason, RECORD_URL);
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Test()
        {
            // string text = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("/text.txt"));
            var data = new
            {
                ALFACRM_URL = "legionsport.s20.online",
                PHONE_NUMBER = "+375(44)719-66-49",  // номер звонящего в случае входящего звонка, или исходящий номер.
                AXATEL_GUID = "ceb9906c-0a24-4d7b-a8f4-f6e8f381c355",
                CALL_GUID = "asd9906c-0ad4-4d7b-ada4-f6e8fasdc353", // постоянный ID звонка во внешней системе
                TYPE = "in"  // направление звонка, может быть либо in либо out;
            };
            string contentText2 = JsonConvert.SerializeObject(data).ToString();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Post("http://localhost:62415/method/alfacrm/alert", contentText2, "application/json").ToString();
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

            return Content(content);

        }

    }
}