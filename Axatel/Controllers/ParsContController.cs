using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Axatel.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using xNet;
using CsvHelper;

namespace Axatel.Controllers
{
    public class ParsContController : Controller
    {
        
        Context _db = new Context();
        ExpandoObjectConverter converter = new ExpandoObjectConverter();
        public SqlConnection conn;
        public string connstr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //фильтр обработки номеров
        public string FilterNumber(string tel)
        {
           
            if (string.IsNullOrEmpty(tel)) { return ""; }
            string strtel = new string(tel.Where(t => char.IsDigit(t)).ToArray());
            if (strtel.Length < 7)
            {

                return "";
            }
            if (strtel.Length == 7)
            {
                strtel = "37517" + strtel;
                return strtel;
            }
            if (strtel.Substring(0, 3) == "375")
            {
                if (strtel.Length != 12) { return ""; } else { return strtel; }
            }
            if ((strtel.Substring(0, 2) == "80") && (strtel.Length == 11))
            {
                strtel = strtel.Replace("80", "375");
                return strtel;
            }
            else { return ""; }

        }
        public void ConnectSQLServer()
        {
            conn = new SqlConnection(connstr);
            conn.Open();
        }
        public List<object[]> SelAllData(string tablename)
        {
            ConnectSQLServer();
            SqlCommand cmd = new SqlCommand("SELECT * FROM [Axatel].[dbo].[" + tablename + "]", conn);
            SqlDataReader reader = cmd.ExecuteReader();

            List<object[]> result = new List<object[]>();
            while (reader.Read())
            {
                object[] str = new Object[reader.FieldCount];
                int fieldCount = reader.GetValues(str);
                result.Add(str);
            }
            reader.Close();           
            conn.Close();
            conn.Dispose();
            return result;
        }
        public object[] SelNumb(string tablename)
        {            
            ConnectSQLServer();
            SqlCommand cmd = new SqlCommand("SELECT TOP(1) * FROM [Axatel].[dbo].[" + tablename + "] WHERE [isGet] = 0", conn);
            SqlDataReader reader = cmd.ExecuteReader();

            object[] result = null;
            while (reader.Read())
            {
                object[] str = new Object[reader.FieldCount];
                int fieldCount = reader.GetValues(str);
                result = str;
            }
            reader.Close();
            SqlCommand cmd2 = new SqlCommand("UPDATE [Axatel].[dbo].[" + tablename + "] SET [isGet] = 1 WHERE [Id] = " + result[0], conn);
            cmd2.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            return result;
        }
        public int ClearStatisget(string tablename)
        {
            ConnectSQLServer();
            SqlCommand cmd2 = new SqlCommand("UPDATE [Axatel].[dbo].[" + tablename + "] SET [isGet] = 0 ", conn);
            cmd2.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            return 1;
        }

        public string CreateTable(int idportal)
        {
            string guidstr = Guid.NewGuid().ToString();
            string tablename = idportal + "-" + guidstr;
            string tablres = idportal + "-result-" + guidstr;
            ConnectSQLServer();
            SqlCommand cmd2 = new SqlCommand("CREATE TABLE [Axatel].[dbo].["+ tablename + "]([Id][int] IDENTITY(1, 1) NOT NULL,[Guid][nvarchar](50) NULL, [Telefon][nvarchar](20) NULL,[IdEntity][int] NULL,[TypeEntity][nvarchar](10) NULL,[PortalName][nvarchar](50) NULL, [isGet] [int] NOT NULL, CONSTRAINT[PK_" + tablename + "] PRIMARY KEY CLUSTERED([Id] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY] ALTER TABLE [dbo].["+ tablename + "] ADD  CONSTRAINT [DF_"+ tablename + "_isGet]  DEFAULT ((0)) FOR [isGet]", conn);
            cmd2.ExecuteNonQuery();
            SqlCommand cmd3 = new SqlCommand("CREATE TABLE [dbo].["+ tablres + "]([Id] [int] IDENTITY(1,1) NOT NULL,[PortalName] [nvarchar](100) NULL,[Date] [datetime] NULL,[CALLID] [nvarchar](50) NULL,[IdNumber] [nvarchar](50) NULL,[AbonentNumber] [nvarchar](20) NULL,[DateTimeStart] [datetime] NULL,[DateTimeStop] [datetime] NULL,[OperatorID] [nvarchar](50) NULL,[OperatorNumder] [int] NULL,[OperatorDisplayNane] [nvarchar](200) NULL,[LenQueue] [int] NULL,[LenTime] [int] NULL,[AbonentInfo] [nvarchar](max) NULL,[CallResult] [int] NULL,[URLRec] [nvarchar](500) NULL,CONSTRAINT [PK_" + tablres+"] PRIMARY KEY CLUSTERED ([Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]", conn);
            cmd3.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            return tablename;
        }

        public int Dellable(int idportal, string guidtable)
        {
           
            string tablename = idportal + "-" + guidtable;
            string tablres = idportal + "-result-" + guidtable;
            ConnectSQLServer();
            SqlCommand cmd2 = new SqlCommand("DROP TABLE [Axatel].[dbo].[" + tablename + "]", conn);
            cmd2.ExecuteNonQuery();
            SqlCommand cmd3 = new SqlCommand("DROP TABLE [dbo].[" + tablres + "]", conn);
            cmd3.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            return 1;
        }
        public int DellEntity(string tblportal, string type)
        {          
            ConnectSQLServer();
            SqlCommand cmd2 = new SqlCommand("DELETE FROM [Axatel].[dbo].[" + tblportal + "] WHERE [TypeEntity] ='"+ type + "'", conn);
            cmd2.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            return 1;
        }
        public int InsertEntity(string tblportal, List<TimeEntit> tentit)
        {
            
            ConnectSQLServer();
            foreach (var item in tentit)
            {
                string guidstr = Guid.NewGuid().ToString();
                SqlCommand cmd2 = new SqlCommand("INSERT INTO [Axatel].[dbo].[" + tblportal + "] (Guid, Telefon, [IdEntity], TypeEntity, PortalName) Values  ( '" + guidstr + "', '" + item.Telefon + "', " + item.IdEntit + ", '" + item.Type + "', '"+item.PortalName+"' )", conn);
                cmd2.ExecuteNonQuery();
            }
            conn.Close();
            conn.Dispose();
            return 1;
        }
        public int InsertDataFinish(string tblname, string porta , string DateTime, string CALLID,  string IdNumber, string AbonentNumber, string DateTimeStart, string DateTimeStop, string OperatorID, int OperatorNumder, string OperatorDisplayNane, int LenQueue, int LenTime, string AbonentInfo, string CallResult, string URLRec)
        {
           
            ConnectSQLServer();          
            string guidstr = Guid.NewGuid().ToString();
            SqlCommand cmd2 = new SqlCommand("INSERT INTO [Axatel].[dbo].[" + tblname + "] ([PortalName],[Date],[CALLID],[IdNumber],[AbonentNumber],[DateTimeStart],[DateTimeStop],[OperatorID],[OperatorNumder],[OperatorDisplayNane],[LenQueue],[LenTime],[AbonentInfo],[CallResult],[URLRec]) Values"+
                                                                                                "('" + porta + "', '" + DateTime + "', '" + CALLID + "', '" + IdNumber + "', '" + AbonentNumber + "','" + DateTimeStart + "', '" + DateTimeStop + "', '" + OperatorID + "', " + OperatorNumder + ", N'" + OperatorDisplayNane + "'," + LenQueue + ", " + LenTime + ", '" + AbonentInfo + "', '" + CallResult + "', '" + URLRec + "' )", conn);

            cmd2.ExecuteNonQuery();         
            conn.Close();
            conn.Dispose();
            return 1;
        }
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

                resp2 = request.Get("https://oauth.bitrix.info/oauth/token/?grant_type=refresh_token&client_id=" + HomeController.client_id + "&client_secret=" + HomeController.client_secret + "&refresh_token=" + conm.RefTok);

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
        public class Lead
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public DateTime Date { get; set; }
            public string Sourse { get; set; }
            public string Asingbyid { get; set; }
            public string Telef { get; set; }

        }
        public class Contact
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string TypeCont { get; set; }
            public string Asingbyid { get; set; }
            public string Telef { get; set; }

        }
        public class Deal
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public DateTime Date { get; set; }
            public string StadeCont { get; set; }
            public string Asingbyid { get; set; }
            public string Telef { get; set; }

        }
        public class Companiya
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public DateTime Date { get; set; }
            public string TypeComp { get; set; }
            public string Asingbyid { get; set; }
            public string Telef { get; set; }

        }
        [HttpPost]
        public JsonResult GetStat(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent taskab = _db.TaskAbonents.Where(a => a.PortalName == comp.Portal ).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
            var sqltot= "SELECT COUNT(*) FROM [Axatel].[dbo].[" + taskab.Guid + "]";
            var sqltotsend = "SELECT COUNT(*) FROM [Axatel].[dbo].[" + taskab.Guid + "] WHERE [isGet] = 1";
            int total = _db.Database.SqlQuery<int>(sqltot).Single();
            int totalsend = _db.Database.SqlQuery<int>(sqltotsend).Single();
            return Json(new { total, totalsend });
        }
        public class Statuspars
        {
            public string Portal { get; set; }
            public bool flagdeal { get; set; } // флаг работы парсинга
        }
        public static List<Statuspars> stadeal = new List<Statuspars>();// лист порталов в котором есть флаг работы парсинга
        // GET: ParsCont
        public ActionResult Index(string member_id)
        {
            
            Compan comp2 = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            
            RefSetToken(comp2.Portal);
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            Statuspars stat = stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault();
           if (stat == null)
            {
                stadeal.Add(new Statuspars { Portal = comp.Portal, flagdeal = false });
            }
            stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = false;
            if (comp == null) return Content("<h2>Ошибка доступа<h2>");
            TaskAbonent taskab = _db.TaskAbonents.Where(a => a.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
            int iswork = 0;
            int isfilter = 0;
           if (taskab == null)
            {
                string tablename = CreateTable(comp.Id);
                TaskAbonent abonent = new TaskAbonent();
                abonent.Guid = tablename;
                abonent.NameTable = "Новая таблица";
                abonent.PortalName = comp.Portal;
                abonent.isWork = 0;
                _db.TaskAbonents.Add(abonent);
                comp.IdMainTblAbon = abonent.Id;
                _db.Entry(comp).State = System.Data.Entity.EntityState.Modified;

                _db.SaveChanges();
            }
            else
            {
                iswork = taskab.isWork;
                isfilter = taskab.isFilterNumb;
            }

            string content;
            using (xNet.HttpRequest req = new xNet.HttpRequest())
            {
                content = req.Get("https://" + comp.Portal + "/rest/crm.status.list?auth=" + comp.AcesTok).ToString();              
            }
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);

            Dictionary<string, string> statuslid = new Dictionary<string, string>();            
            foreach (var item in obj.result)
            {
                if (item.ENTITY_ID ==  "STATUS")
                {
                    statuslid.Add(item.STATUS_ID.ToString(), item.NAME.ToString());
                }
            }

            Dictionary<string, string> SOURCElid = new Dictionary<string, string>();
            foreach (var item in obj.result)
            {
                if (item.ENTITY_ID == "SOURCE")
                {
                    SOURCElid.Add(item.STATUS_ID.ToString(), item.NAME.ToString());
                }
            }
            string content5;
            using (xNet.HttpRequest req5 = new xNet.HttpRequest())
            {
                content5 = req5.Get("https://" + comp.Portal + "/rest/crm.dealcategory.list?auth=" + comp.AcesTok).ToString();
            }
            dynamic obj5 = JsonConvert.DeserializeObject<ExpandoObject>(content5, converter);
            Dictionary<string, string> voronkiid = new Dictionary<string, string>();
            foreach(var item in obj5.result)
            {
                voronkiid.Add(item.ID.ToString(), item.NAME.ToString());
            }

            Dictionary<string, string> stadedeal = new Dictionary<string, string>();
            foreach (var item in obj.result)
            {
                if (item.ENTITY_ID.ToString().IndexOf("DEAL_STAGE") > -1)
                {
                    string name = "";
                    try
                    {
                        name = voronkiid.Where(i => i.Key == item.CATEGORY_ID.ToString()).FirstOrDefault().Value;
                    }
                    catch
                    {
                        name = "Основное";
                    }
                    if (string.IsNullOrEmpty(name)) { name = "Основное"; }
                    stadedeal.Add(item.STATUS_ID.ToString(), item.NAME.ToString() +"("+ name + ")");
                }
            }
            Dictionary<string, string> typecont = new Dictionary<string, string>();
            foreach (var item in obj.result)
            {
                if(item.ENTITY_ID == "CONTACT_TYPE")
                {
                    typecont.Add(item.STATUS_ID.ToString(), item.NAME.ToString());
                }
            }
            Dictionary<string, string> typecomp = new Dictionary<string, string>();
            foreach (var item in obj.result)
            {
                if (item.ENTITY_ID == "COMPANY_TYPE")
                {
                    typecomp.Add(item.STATUS_ID.ToString(), item.NAME.ToString());
                }
            }
                   
            Dictionary<string, string> mendic = new Dictionary<string, string>();
            string content2;
                                
            int ind = 0; int totcount = 0; int stap = 0; bool flag = true;
            List<Contact> lstcont = new List<Contact>();
            while (flag)
            {
                using (xNet.HttpRequest req = new xNet.HttpRequest())
                {
                    content2 = req.Get("https://" + comp.Portal + "/rest/user.search?start="+ stap + "&auth=" + comp.AcesTok).ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content2, converter);
                foreach (var item in obj2.result)
                {
                    mendic.Add(item.ID, item.NAME + " " + item.LAST_NAME);
                }
                if (ind == 0)
                {
                    int count = Convert.ToInt32(obj2.total);
                    totcount = count / 50;
                }
                ind++;
                stap += 50;
                if (ind > totcount)
                {
                    flag = false;
                }

            }
            List<object[]> alldate = SelAllData(taskab.Guid);
            int countlid = alldate.Where(l => l[4].ToString() == "lead").Count();
            int countdeal = alldate.Where(l => l[4].ToString() == "deal").Count();
            int countcomp = alldate.Where(l => l[4].ToString() == "comp").Count();
            int countcont = alldate.Where(l => l[4].ToString() == "cont").Count();
            int countimp = alldate.Where(l => l[4].ToString() == "insert").Count();


            ViewBag.countlid = countlid;
            ViewBag.countdeal = countdeal;
            ViewBag.countcomp = countcomp;
            ViewBag.countcont = countcont;
            ViewBag.countimp = countimp;



            List<TaskAbonent> lsttask = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).ToList();
            ViewBag.LastTask = lsttask;
            ViewBag.MainId = comp.IdMainTblAbon;
            ViewBag.MainNameId = lsttask.Where(i => i.Id == comp.IdMainTblAbon).FirstOrDefault().NameTable;
            ViewBag.Meneg = mendic;
            ViewBag.SOURCELid = SOURCElid;
            ViewBag.StatusLid = statuslid;
            ViewBag.TypeCont = typecont;
            ViewBag.TypeComp = typecomp;
            ViewBag.IsWork = iswork;
            ViewBag.Telbot = comp.VoiceBotNumber;
            ViewBag.Filternumb = isfilter;
            ViewBag.StadeDeal = stadedeal;
            ViewBag.memberid = member_id;
            return View();
        }

        public JsonResult DellTabl(string idtabl, string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent task = _db.TaskAbonents.Where(i => i.Id.ToString() == idtabl).FirstOrDefault();
            if (task.NameTable == "Новая таблица")
            {
                return Json("ok");
            }
            _db.Entry(task).State = System.Data.Entity.EntityState.Deleted;
            _db.SaveChanges();
            string delimStr = "-";
            char[] delimiter = delimStr.ToCharArray();
            string guidtabl = task.Guid.Split(delimiter,2)[1];
            Dellable(comp.Id, guidtabl);
            return Json("ok");
        }

        public JsonResult SetTable(string idtabl, string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            comp.IdMainTblAbon = Convert.ToInt32(idtabl);
            _db.Entry(comp).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json("ok");
        }

        public JsonResult SearchLead(string member_id, string[] status, string[] asingbyid, string[] sourseid, string dateot, string datedo)
        {
            Compan comp2 = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            HomeController hc = new HomeController();
            hc.RefSetToken(comp2.Portal);
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            //string[] status = new string[] { "1", "NEW", "IN_PROCESS" };// 
            //string[] asingbyid = new string[] { "1" };
            //string[] sourseid = new string[] { "ADVERTISING", "WEB", "5" };
            string statusstr = ""; // массив статуса
            if (status != null)
            {
                if (status.Length == 1)
                {
                    statusstr = status[0];
                }
                else
                {
                    statusstr = string.Join("\",\"", status);
                    statusstr = "[\"" + statusstr + "\"]";
                }
            }
            string asingbyidstr = "";// масив ответственного
            if (asingbyid != null)
            {
                if (asingbyid.Length == 1)
                {
                    asingbyidstr = asingbyid[0];
                }
                else
                {
                    asingbyidstr = string.Join("\",\"", asingbyid);
                    asingbyidstr = "[\"" + asingbyidstr + "\"]";
                }
            }
            string sourseidstr = ""; // массив источника
            if (sourseid != null)
            {
                if (sourseid.Length == 1)
                {
                    sourseidstr = sourseid[0];
                }
                else
                {
                    sourseidstr = string.Join("\",\"", sourseid);
                    sourseidstr = "[\"" + sourseidstr + "\"]";
                }
            }
            string dateotstr = "";
            if (dateot!="")
            {
                dateotstr = ">DATE_CREATE\":\"" + dateot + "T00:00:00+00:00\"";
            }
            string datedostr = "";
            if (datedo != "")
            {
                datedostr = "<DATE_CREATE\":\"" + datedo + "T00:00:00+00:00\"";
            }
            int totcount = 0;           
            int stap = 0;
            bool flag = true;
            int ind = 0;
            List<Lead> lstlead = new List<Lead>();
            while (flag)
            {
                
                string contentText = "{\"start\":"+ stap + ",\"filter\":{\"STATUS_ID\":" + statusstr + ",\"ASSIGNED_BY_ID\":" + asingbyidstr + ",\"SOURCE_ID\":" + sourseidstr + ",\"" + dateotstr + ", \"" + datedostr + "},\"select\":[\"ID\",\"TITLE\",\"DATE_CREATE\",\"STATUS_ID\",\"SOURCE_ID\",\"ASSIGNED_BY_ID\", \"PHONE\"]}";
                //var data = new
                //{
                //    filter = new { STATUS_ID = new[] { "1", "NEW", "IN_PROCESS" }, ASSIGNED_BY_ID = new[] { "1", "3", "7" }, SOURCE_ID= new[] { "ADVERTISING", "WEB", "5" }, DATE_CREATE = "2018-01-04T00:00:00+00:00" },
                //    select = new[] { "ID", "TITLE", "DATE_CREATE", "STATUS_ID", "SOURCE_ID", "ASSIGNED_BY_ID" }
                //};
                //string contentText = JsonConvert.SerializeObject(data).ToString();
                
                string contlid;
                using (xNet.HttpRequest req = new xNet.HttpRequest())
                {
                    contlid = req.Post("https://" + comp.Portal + "/rest/crm.lead.list?auth=" + comp.AcesTok, contentText, "application/json").ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(contlid, converter);
               foreach (var item in obj2.result)
                {
                    Lead lead = new Lead();
                    lead.Id =  item.ID;
                    lead.Asingbyid = item.ASSIGNED_BY_ID;
                    lead.Date = item.DATE_CREATE;
                    lead.Sourse = item.SOURCE_ID;
                    lead.Title = item.TITLE;
                    try{
                        lead.Telef = item.PHONE[0].VALUE;
                    }
                    catch
                    {
                        continue;
                    }
                    lstlead.Add(lead);

                }
                if (ind == 0)
                {
                    int count = Convert.ToInt32(obj2.total);
                    totcount = count / 50;
                }
                ind++;
                stap += 50;
                if (ind > totcount)
                {
                    flag = false;
                }                
                }
            List<TimeEntit> newenty = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).Where(n => n.Type == "lead").ToList();
            if (newenty.Count != 0)
            {
                foreach (var item in newenty)
                {
                    _db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                _db.SaveChanges();
            }
          //  List<TimeEntit> newentytotal = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).ToList();
           // TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).Where(q=>q.Id == comp.IdMainTblAbon).FirstOrDefault();
            
            foreach (var item in lstlead)
            {
                //string str = "";
                //if (task.isFilterNumb == 1)
                //{
                //    str = FilterNumber(item.Telef);
                //}
                //else
                //{
                //    str = item.Telef;
                //}
                //if (str == "") { continue; }
           //     if (newentytotal.Where(i => i.Telefon == str).FirstOrDefault() != null) { continue; }

                TimeEntit entin = new TimeEntit();
                entin.IdEntit = Convert.ToInt32(item.Id);
                entin.PortalName = comp.Portal;
                entin.Telefon = item.Telef;
                entin.Type = "lead";
                _db.TimeEntits.Add(entin);
               
            }
            _db.SaveChanges();
            string content = "";
            foreach (var item in lstlead)
            {
                content += "<input type=\"checkbox\" checked id=\"lead-" + item.Id + "\" class=\"checkitem\" value=\"" + item.Id + "\"><label for=\"lead-" + item.Id + "\"><a href=\"https://"+comp.Portal+"/crm/lead/details/"+ item.Id + "/\" target=\"_blank\">" + item.Id + "</a>  " + item.Title + " " + item.Telef + "</label>";
            }

            return Json(new { cont = content, count = lstlead.Count });
        }
        public JsonResult SearchCont(string member_id, string[] type, string[] asingbyid,  string dateot, string datedo)
        {
            Compan comp2 = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            HomeController hc = new HomeController();
            hc.RefSetToken(comp2.Portal);
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            //string[] status = new string[] { "1", "NEW", "IN_PROCESS" };// 
            //string[] asingbyid = new string[] { "1" };
            //string[] sourseid = new string[] { "ADVERTISING", "WEB", "5" };
            string typestr = ""; // массив статуса
            if (type != null)
            {
                if (type.Length == 1)
                {
                    if (type[0] == "")
                    {
                        typestr = "null";
                    }
                    else
                    {
                        typestr = type[0];
                    }
                }
                else
                {
                    typestr = string.Join("\",\"", type);
                    typestr = "[\"" + typestr + "\"]";
                }
            }
            string asingbyidstr = "";// масив ответственного
            if (asingbyid != null)
            {
                if (asingbyid.Length == 1)
                {
                    asingbyidstr = asingbyid[0];
                }
                else
                {
                    asingbyidstr = string.Join("\",\"", asingbyid);
                    asingbyidstr = "[\"" + asingbyidstr + "\"]";
                }
            }
            
            string dateotstr = "";
            if (dateot != "")
            {
                dateotstr = ">DATE_CREATE\":\"" + dateot + "T00:00:00+00:00\"";
            }
            string datedostr = "";
            if (datedo != "")
            {
                datedostr = "<DATE_CREATE\":\"" + datedo + "T00:00:00+00:00\"";
            }
            int totcount = 0;
            int stap = 0;
            bool flag = true;
            int ind = 0;
            List<Contact> lstcont = new List<Contact>();
            while (flag)
            {

                string contentText = "{\"start\":" + stap + ",\"filter\":{\"TYPE_ID\":" + typestr + ",\"ASSIGNED_BY_ID\":" + asingbyidstr + ",\"" + dateotstr + ", \"" + datedostr + "},\"select\":[\"ID\",\"NAME\",\"DATE_CREATE\",\"TYPE_ID\",\"ASSIGNED_BY_ID\", \"PHONE\"]}";
                //var data = new
                //{
                //    filter = new { STATUS_ID = new[] { "1", "NEW", "IN_PROCESS" }, ASSIGNED_BY_ID = new[] { "1", "3", "7" }, SOURCE_ID= new[] { "ADVERTISING", "WEB", "5" }, DATE_CREATE = "2018-01-04T00:00:00+00:00" },
                //    select = new[] { "ID", "TITLE", "DATE_CREATE", "STATUS_ID", "SOURCE_ID", "ASSIGNED_BY_ID" }
                //};
                //string contentText = JsonConvert.SerializeObject(data).ToString();

                string contlid;
                using (xNet.HttpRequest req = new xNet.HttpRequest())
                {
                    contlid = req.Post("https://" + comp.Portal + "/rest/crm.contact.list?auth=" + comp.AcesTok, contentText, "application/json").ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(contlid, converter);
                foreach (var item in obj2.result)
                {
                    Contact cont = new Contact();
                    cont.Id = item.ID;
                    cont.Asingbyid = item.ASSIGNED_BY_ID;
                    cont.Date = item.DATE_CREATE;
                    cont.TypeCont = item.TYPE_ID;
                    cont.Name = item.NAME;
                    try
                    {
                        cont.Telef = item.PHONE[0].VALUE;
                    }
                    catch
                    {
                        continue;
                    }
                    lstcont.Add(cont);

                }
                if (ind == 0)
                {
                    int count = Convert.ToInt32(obj2.total);
                    totcount = count / 50;
                }
                ind++;
                stap += 50;
                if (ind > totcount)
                {
                    flag = false;
                }
            }
            List<TimeEntit> newenty = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).Where(n => n.Type == "cont").ToList();
            
            if (newenty.Count != 0)
            {
                foreach (var item in newenty)
                {
                    _db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                _db.SaveChanges();
            }
          //  List<TimeEntit> newentytotal = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).ToList();
           // TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).FirstOrDefault();
            foreach (var item in lstcont)
            {
                //string str = "";
                //if (task.isFilterNumb == 1)
                //{
                //    str = FilterNumber(item.Telef);
                //}
                //else
                //{
                //    str = item.Telef;
                //}
                //if (str == "") { continue; }
            //    if (newentytotal.Where(i => i.Telefon == str).FirstOrDefault() != null) { continue; }
                TimeEntit entin = new TimeEntit();
                entin.IdEntit = Convert.ToInt32(item.Id);
                entin.PortalName = comp.Portal;
                entin.Telefon = item.Telef;
                entin.Type = "cont";
                _db.TimeEntits.Add(entin);
                
            }
            _db.SaveChanges();
            string content = "";
            foreach (var item in lstcont)
            {
                content += "<input type=\"checkbox\" checked id=\"cont-" + item.Id + "\" class=\"checkitem\" value=\"" + item.Id + "\"><label for=\"cont-" + item.Id + "\"><a href=\"https://" + comp.Portal + "/crm/contact/details/" + item.Id + "/\" target=\"_blank\">" + item.Id + "</a>   " + item.Name + " " + item.Telef + "</label>";
            }

            return Json(new { cont = content, count = lstcont.Count });
        }
        public class Countdeals
        {
            public List<Deal> lstdeal = new List<Deal>();
            public string Portal { get; set; }
        }
        public ActionResult CountdealsProg(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            int count;
            try
            {
                count = countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal.Count;
            } catch
            {
                count = 0;
            }
            return Content(count.ToString());
        }

       
        public static List<Countdeals> countdeals = new List<Countdeals>();
        public JsonResult SearchDeal(string member_id, string[] stade, string[] asingbyid, string dateot, string datedo)
        {
            Compan comp2 = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            HomeController hc = new HomeController();
            hc.RefSetToken(comp2.Portal);
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            //string[] status = new string[] { "1", "NEW", "IN_PROCESS" };// 
            //string[] asingbyid = new string[] { "1" };
            //string[] sourseid = new string[] { "ADVERTISING", "WEB", "5" };
            string stadestr = ""; // массив статуса
            if (stade != null)
            {
                if (stade.Length == 1)
                {
                    stadestr = stade[0];
                }
                else
                {
                    stadestr = string.Join("\",\"", stade);
                    stadestr = "[\"" + stadestr + "\"]";
                }
            }
            string asingbyidstr = "";// масив ответственного
            if (asingbyid != null)
            {
                if (asingbyid.Length == 1)
                {
                    asingbyidstr = asingbyid[0];
                }
                else
                {
                    asingbyidstr = string.Join("\",\"", asingbyid);
                    asingbyidstr = "[\"" + asingbyidstr + "\"]";
                }
            }

            string dateotstr = "";
            if (dateot != "")
            {
                dateotstr = ">DATE_CREATE\":\"" + dateot + "T00:00:00+00:00\"";
            }
            string datedostr = "";
            if (datedo != "")
            {
                datedostr = "<DATE_CREATE\":\"" + datedo + "T00:00:00+00:00\"";
            }
            int totcount = 0;
            int stap = 0;
            //bool flag = true;
            if (stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault() == null)
            {
                return Json("er");
            }
            stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = true;

            //flag = true;
            int ind = 0;
            //List<Deal> lstdeal = new List<Deal>();
            if (countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault() == null)
            {
                countdeals.Add(new Countdeals { lstdeal = new List<Deal>(), Portal = comp.Portal });
            }
            else
            {
                int countdealst = countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal.Count;
                countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal.RemoveRange(0, countdealst);
            }
            while (stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal)
            {

                string contentText = "{\"start\":" + stap + ",\"filter\":{\"STAGE_ID\":" + stadestr + ",\"ASSIGNED_BY_ID\":" + asingbyidstr + ",\"" + dateotstr + ", \"" + datedostr + "},\"select\":[\"ID\",\"TITLE\",\"DATE_CREATE\",\"STAGE_ID\",\"ASSIGNED_BY_ID\", \"CONTACT_ID\", \"COMPANY_ID\"]}";
                
                string contlid;
                RefSetToken(comp2.Portal);
                using (xNet.HttpRequest req = new xNet.HttpRequest())
                {
                    contlid = req.Post("https://" + comp.Portal + "/rest/crm.deal.list?auth=" + comp.AcesTok, contentText, "application/json").ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(contlid, converter);
                System.Threading.Thread.Sleep(500);
                foreach (var item in obj2.result)
                {
                    if (string.IsNullOrEmpty(item.CONTACT_ID))
                    {
                        if (string.IsNullOrEmpty(item.COMPANY_ID))
                        {
                            continue;
                        }
                        if (item.COMPANY_ID.ToString() == "0") { continue; }
                        Deal deal = new Deal();
                        deal.Id = item.ID;
                        deal.Asingbyid = item.ASSIGNED_BY_ID;
                        deal.Date = item.DATE_CREATE;
                        deal.StadeCont = item.STAGE_ID;
                        deal.Title = item.TITLE;
                        string cont3 = "";
                        using (xNet.HttpRequest request = new xNet.HttpRequest())
                        {
                            cont3 = request.Get("https://" + comp.Portal + "/rest/crm.company.get?id=" + item.COMPANY_ID + "&auth=" + comp.AcesTok).ToString();
                        }
                        dynamic obj3 = JsonConvert.DeserializeObject<ExpandoObject>(cont3, converter);
                        try
                        {
                            deal.Telef = obj3.result.PHONE[0].VALUE;
                        }
                        catch { continue; }
                        countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal.Add(deal);
                        System.Threading.Thread.Sleep(500);


                    }
                    else
                    {

                        Deal deal = new Deal();
                        deal.Id = item.ID;
                        deal.Asingbyid = item.ASSIGNED_BY_ID;
                        deal.Date = item.DATE_CREATE;
                        deal.StadeCont = item.STAGE_ID;
                        deal.Title = item.TITLE;
                        string cont3 = "";
                        using (xNet.HttpRequest request = new xNet.HttpRequest())
                        {
                            cont3 = request.Get("https://" + comp.Portal + "/rest/crm.contact.get?id=" + item.CONTACT_ID + "&auth=" + comp.AcesTok).ToString();
                        }
                        dynamic obj3 = JsonConvert.DeserializeObject<ExpandoObject>(cont3, converter);
                        try
                        {
                            deal.Telef = obj3.result.PHONE[0].VALUE;
                        }
                        catch { continue; }
                        countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal.Add(deal);
                        System.Threading.Thread.Sleep(500);
                    }
                    if (stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal == false)
                    {
                        break;
                    }
                }
                if (ind == 0)
                {
                    int count = Convert.ToInt32(obj2.total);
                    totcount = count / 50;
                }
                ind++;
                stap += 50;
                if (ind > totcount)
                {
                    stadeal.Where(p => p.Portal == comp.Portal).FirstOrDefault().flagdeal = false;
                }
            }
            List<TimeEntit> newenty = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).Where(n => n.Type == "deal").ToList();
           
            if (newenty.Count != 0)
            {
                foreach (var item in newenty)
                {
                    _db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                _db.SaveChanges();
            }
         //   List<TimeEntit> newentytotal = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).ToList();
          //  TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).FirstOrDefault();
            foreach (var item in countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal)
            {
                //string str = "";
                //if (task.isFilterNumb == 1)
                //{
                //    str = FilterNumber(item.Telef);
                //}
                //else
                //{
                //    str = item.Telef;
                //}
                //if (str == "") { continue; }
           //     if (newentytotal.Where(i => i.Telefon == str).FirstOrDefault() != null) { continue; }
                TimeEntit entin = new TimeEntit();
                entin.IdEntit = Convert.ToInt32(item.Id);
                entin.PortalName = comp.Portal;
                entin.Telefon = item.Telef;
                entin.Type = "deal";
                _db.TimeEntits.Add(entin);

            }
            _db.SaveChanges();
            string content = "";
            foreach (var item in countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal)
            {
                content += "<input type=\"checkbox\" checked id=\"deal-" + item.Id + "\" class=\"checkitem\" value=\"" + item.Id + "\"><label for=\"deal-" + item.Id + "\"><a href=\"https://" + comp.Portal + "/crm/deal/details/" + item.Id + "/\" target=\"_blank\">" + item.Id + "</a>   " + item.Title + " " + item.Telef + "</label>";
            }

            return Json(new { cont = content, count = countdeals.Where(p => p.Portal == comp.Portal).FirstOrDefault().lstdeal.Count });
        }

        public JsonResult SearchComp(string member_id, string[] status , string[] asingbyid, string dateot, string datedo)
        {
            Compan comp2 = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            HomeController hc = new HomeController();
            hc.RefSetToken(comp2.Portal);
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            string statusstr = ""; // массив статуса
            if (status != null)
            {
                if (status.Length == 1)
                {
                    if (status[0] == "")
                    {
                        statusstr = "null";
                    }
                    else
                    {
                        statusstr = status[0];
                    }
                }
                else
                {
                    statusstr = string.Join("\",\"", status);
                    statusstr = "[\"" + statusstr + "\"]";
                }
            }
            string asingbyidstr = "";// масив ответственного
            if (asingbyid != null)
            {
                if (asingbyid.Length == 1)
                {
                    asingbyidstr = asingbyid[0];
                }
                else
                {
                    asingbyidstr = string.Join("\",\"", asingbyid);
                    asingbyidstr = "[\"" + asingbyidstr + "\"]";
                }
            }

            string dateotstr = "";
            if (dateot != "")
            {
                dateotstr = ">DATE_CREATE\":\"" + dateot + "T00:00:00+00:00\"";
            }
            string datedostr = "";
            if (datedo != "")
            {
                datedostr = "<DATE_CREATE\":\"" + datedo + "T00:00:00+00:00\"";
            }
            int totcount = 0;
            int stap = 0;
            bool flag = true;
            int ind = 0;
            List<Companiya> lstcomp = new List<Companiya>();
            while (flag)
            {
                string contentText = "{\"start\":" + stap + ",\"filter\":{\"COMPANY_TYPE\":" + statusstr + ",\"ASSIGNED_BY_ID\":" + asingbyidstr + ",\"" + dateotstr + ", \"" + datedostr + "},\"select\":[\"ID\",\"TITLE\",\"DATE_CREATE\",\"COMPANY_TYPE\",\"ASSIGNED_BY_ID\", \"PHONE\"]}";
                string contlid;
                using (xNet.HttpRequest req = new xNet.HttpRequest())
                {
                    contlid = req.Post("https://" + comp.Portal + "/rest/crm.company.list?auth=" + comp.AcesTok, contentText, "application/json").ToString();
                }
                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(contlid, converter);
                foreach (var item in obj2.result)
                {
                    Companiya compan = new Companiya();
                    compan.Id = item.ID;
                    compan.Asingbyid = item.ASSIGNED_BY_ID;
                    compan.Date = item.DATE_CREATE;
                    compan.TypeComp = item.COMPANY_TYPE;
                    compan.Title = item.TITLE;
                    try
                    {
                        compan.Telef = item.PHONE[0].VALUE;
                    }
                    catch
                    {
                        continue;
                    }
                    lstcomp.Add(compan);

                }
                if (ind == 0)
                {
                    int count = Convert.ToInt32(obj2.total);
                    totcount = count / 50;
                }
                ind++;
                stap += 50;
                if (ind > totcount)
                {
                    flag = false;
                }
            }
            List<TimeEntit> newenty = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).Where(n => n.Type == "comp").ToList();
            
            if (newenty.Count != 0)
            {
                foreach (var item in newenty)
                {
                    _db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                _db.SaveChanges();
            }
         //   List<TimeEntit> newentytotal = _db.TimeEntits.Where(e => e.PortalName == comp.Portal).ToList();
         //   TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).FirstOrDefault();
            foreach (var item in lstcomp)
            {
                //string str = "";
                //if (task.isFilterNumb == 1)
                //{
                //    str = FilterNumber(item.Telef);
                //}
                //else
                //{
                //    str = item.Telef;
                //}
                //if (str == "") { continue; }
             //   if (newentytotal.Where(i => i.Telefon == str).FirstOrDefault() != null) { continue; }
                TimeEntit entin = new TimeEntit();
                entin.IdEntit = Convert.ToInt32(item.Id);
                entin.PortalName = comp.Portal;
                entin.Telefon = item.Telef;
                entin.Type = "comp";
                _db.TimeEntits.Add(entin);

            }
            _db.SaveChanges();
            string content = "";
            foreach (var item in lstcomp)
            {
                content += "<input type=\"checkbox\" checked id=\"comp-" + item.Id + "\" class=\"checkitem\" value=\"" + item.Id + "\"><label for=\"comp-" + item.Id + "\"><a href=\"https://" + comp.Portal + "/crm/company/details/" + item.Id + "/\" target=\"_blank\">" + item.Id + "</a>   " + item.Title + " " + item.Telef + "</label>";
            }

            return Json(new { cont = content, count = lstcomp.Count });
        }

        public JsonResult AddLeads(string member_id, int[] items)
        {
           
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
           
            DellEntity(task.Guid, "lead");
            List<TimeEntit> timeent = _db.TimeEntits.Where(p => p.PortalName == comp.Portal).Where(i => i.Type == "lead").Where(c => items.Contains(c.IdEntit)).ToList();
            List<object[]> alldate = SelAllData(task.Guid);
            List<TimeEntit> newtimeent = timeent.Where(i => alldate.Select(s => s[2]).Contains(i.Telefon)).ToList();
            foreach (var item in newtimeent)
            {
                timeent.Remove(item);
            }
           if (task.isFilterNumb == 1)
            {
               timeent.ToList().ForEach(x => x.Telefon = FilterNumber(x.Telefon));
               timeent = timeent.Where(t => t.Telefon != "").ToList();
            }
            
            InsertEntity(task.Guid, timeent);
            return Json(new { count = timeent.Count });

        }
        public JsonResult AddComps(string member_id, int[] items)
        {
           
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
            DellEntity(task.Guid, "comp");
            List<TimeEntit> timeent = _db.TimeEntits.Where(p => p.PortalName == comp.Portal).Where(i => i.Type == "comp").Where(c => items.Contains(c.IdEntit)).ToList();
            List<object[]> alldate = SelAllData(task.Guid);
            List<TimeEntit>  newtimeent = timeent.Where(i => alldate.Select(s => s[2]).Contains(i.Telefon)).ToList();
            foreach (var item in newtimeent)
            {
                timeent.Remove(item);
            }
            if (task.isFilterNumb == 1)
            {
                timeent.ToList().ForEach(x => x.Telefon = FilterNumber(x.Telefon));
                timeent = timeent.Where(t => t.Telefon != "").ToList();
            }
            InsertEntity(task.Guid, timeent);
            return Json(new { count = timeent.Count });
        }
        public JsonResult AddConts(string member_id, int[] items)
        {
            
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
            DellEntity(task.Guid, "cont");
            List<TimeEntit> timeent = _db.TimeEntits.Where(p => p.PortalName == comp.Portal).Where(i => i.Type == "cont").Where(c => items.Contains(c.IdEntit)).ToList();
            List<object[]> alldate = SelAllData(task.Guid);
            List<TimeEntit> newtimeent = timeent.Where(i => alldate.Select(s => s[2]).Contains(i.Telefon)).ToList();
            foreach (var item in newtimeent)
            {
                timeent.Remove(item);
            }
            if (task.isFilterNumb == 1)
            {
                timeent.ToList().ForEach(x => x.Telefon = FilterNumber(x.Telefon));
                timeent = timeent.Where(t => t.Telefon != "").ToList();
            }
            InsertEntity(task.Guid, timeent);
            return Json(new { count = timeent.Count });

        }
        public JsonResult AddDeals(string member_id, int[] items)
        {
            
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent task = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
            DellEntity(task.Guid, "deal");
            List<TimeEntit> timeent = _db.TimeEntits.Where(p => p.PortalName == comp.Portal).Where(i => i.Type == "deal").Where(c => items.Contains(c.IdEntit)).ToList();
            List<object[]> alldate = SelAllData(task.Guid);
            List<TimeEntit> newtimeent = timeent.Where(i => alldate.Select(s => s[2]).Contains(i.Telefon)).ToList();
            foreach (var item in newtimeent)
            {
                timeent.Remove(item);
            }
            if (task.isFilterNumb == 1)
            {
                timeent.ToList().ForEach(x => x.Telefon = FilterNumber(x.Telefon));
                timeent = timeent.Where(t => t.Telefon != "").ToList();
            }
            try
            {
                InsertEntity(task.Guid, timeent);
            } catch
            {
                return Json(new { count = "error" });
            }
            return Json(new { count = timeent.Count });

        }
        // выдача номеров
        public JsonResult Number(string AxatelGUID, string Type, string IdTask)
        {
            Compan comp = _db.Compans.Where(i => i.AxatelGuid == AxatelGUID).FirstOrDefault();
            if (comp == null) return Json(new { success = "false", error = "Не верный Guid" });
           
            string nametbl = comp.Id + "-" + IdTask;
            TaskAbonent task = _db.TaskAbonents.Where(i => i.Guid == nametbl).FirstOrDefault();
            if (task == null)
            {
                return Json(new { success = "false", error = "Не верный Guid задачи" });
            }
            if (task.isWork == 0)
            {
                return Json(new { success = "false", error = "Выдача номеров отключена настройками приложения" });
            }
            object[] row = null;
            try
            {
                row = SelNumb(nametbl);
            }
            catch(Exception ex)
            {
                if (ex.Message.IndexOf("Ссылка на объект не указывает на экземпляр объекта") >-1)
                {
                    return Json(new { success = "false", error = "Нет данных в таблице БД" });
                }
                if (ex.Message.IndexOf("Invalid object name") > -1)
                {
                    return Json(new { success = "false", error = "Не могу подключиться к таблице БД" });
                }
                return Json(new { success = "false", error = "Не известная ошибка" });
            }
            string td = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss+01:00");
            var data = new
            {
                AxatelGUID = AxatelGUID,
                Type = Type,
                IdTask = IdTask,
                DateTime = td,
                IdNumber = row[1],
                Number = row[2],
                VoiceBotNumber = comp.VoiceBotNumber
            };
            return Json(new { success = "true", data });
        }
       // сохраняем настройку телефона для бота
        public ActionResult SaveVoiceBot( string member_id, string Telef)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            comp.VoiceBotNumber = Telef.Trim();
            _db.Entry(comp).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Content("ok");
        }

        public JsonResult Finish(string AxatelGUID, string DateTime, string CALLID, string IdTask, string IdNumber, string AbonentNumber, string DateTimeStart, string DateTimeStop, string OperatorID, int OperatorNumder, string OperatorDisplayNane, int LenQueue, int LenTime, string AbonentInfo, string CallResult,  string URLRec)
        {
            Compan comp = _db.Compans.Where(i => i.AxatelGuid == AxatelGUID).FirstOrDefault();
            if (comp == null) return Json(new { success = "false", error = "Не верный Guid" });
            string nametbl = comp.Id + "-result-" + IdTask;
            DateTime dt = System.DateTime.Parse(DateTime);
            string dtstr = dt.ToString("yyyy-MM-ddTHH:mm:ss");
            DateTime dtstart = System.DateTime.Parse(DateTimeStart);
            string dtstartstr = dtstart.ToString("yyyy-MM-ddTHH:mm:ss");
            DateTime dtstop = System.DateTime.Parse(DateTimeStop);
            string dtstopstr = dtstop.ToString("yyyy-MM-ddTHH:mm:ss");
             try
            {
                InsertDataFinish(nametbl, comp.Portal, dtstr, CALLID, IdNumber, AbonentNumber, dtstartstr, dtstopstr, OperatorID, OperatorNumder, OperatorDisplayNane, LenQueue, LenTime, AbonentInfo, CallResult, URLRec);
            }
            catch (Exception ex)
            {               
                return Json(new { success = "false", error = "Ошибка записи данных", mess= ex.Message, trace = ex.StackTrace });
            }
            var data = new
            {
                IdTask = IdTask,
                RESULT = "OK"
            };
            return Json(new { success = "true", data });
        }
        // включить отключить работу абонентов
        public JsonResult Onworkabon(string type, string member_id)
        {           
           Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
           if (comp == null) return Json(new { success = "false", error = "MemberId" });
           TaskAbonent abonent = _db.TaskAbonents.Where(i => i.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();

           if (type == "work")
            {
                if(abonent.isWork ==0)
                {
                    abonent.isWork = 1;
                    _db.Entry(abonent).State = System.Data.Entity.EntityState.Modified;
                    
                }else
                {
                    abonent.isWork = 0;
                    _db.Entry(abonent).State = System.Data.Entity.EntityState.Modified;
                }
                _db.SaveChanges();
                return Json(new { type = type, status = abonent.isWork });
            }
           if (type == "filtnumb")
            {
                if (abonent.isFilterNumb == 0)
                {
                    abonent.isFilterNumb = 1;
                    _db.Entry(abonent).State = System.Data.Entity.EntityState.Modified;

                }
                else
                {
                    abonent.isFilterNumb = 0;
                    _db.Entry(abonent).State = System.Data.Entity.EntityState.Modified;
                }
                _db.SaveChanges();
                return Json(new { type = type, status = abonent.isFilterNumb });
            }
            return Json(new { success = "false", error = "Не изветный запрос" });
        }

        public ActionResult ClearData(string member_id)
        {           
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            string nametbl = _db.TaskAbonents.Where(p => p.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault().Guid;
            DellEntity(nametbl, "deal");
            DellEntity(nametbl, "cont");
            DellEntity(nametbl, "comp");
            DellEntity(nametbl, "lead");
            DellEntity(nametbl, "insert");
            return RedirectToAction("Index", new { member_id = member_id });
        }
        public ActionResult ClearIsgetStat(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            string nametbl = _db.TaskAbonents.Where(p => p.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault().Guid;
            ClearStatisget(nametbl);
            return RedirectToAction("Index", new { member_id = member_id });
        }

        // экспортируем в цсв
        public ActionResult ExCSV(string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent taskab = _db.TaskAbonents.Where(a => a.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
            List<object[]> obj = SelAllData(taskab.Guid);
            string pathCsvFile = System.Web.Hosting.HostingEnvironment.MapPath("/Content/csv/" + taskab.Guid + ".csv");
            using (StreamWriter streamWriter = new StreamWriter(pathCsvFile, false , System.Text.Encoding.UTF8))
            {
                streamWriter.WriteLine($"{"Телефон"};{"ID в Битрикс"};{"Тип сущности"};{"Статус обработки"}");
                foreach (var item in obj)
                {
                    //string tel = item[2].ToString().Replace("+", "");
                    streamWriter.WriteLine($"{item[2].ToString()};{item[3]};{item[4]};{item[6]}");
                }
                streamWriter.Close();
                streamWriter.Dispose();
            }
            return Redirect("/Content/csv/" + taskab.Guid + ".csv");

        }

        // импорт из цсв в таблицу
        public ActionResult InpCSV(string member_id, HttpPostedFileBase file)
        {
            if (file == null) { return Redirect("/tablabon?member_id="+ member_id); }
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent taskab = _db.TaskAbonents.Where(a => a.PortalName == comp.Portal).Where(q => q.Id == comp.IdMainTblAbon).FirstOrDefault();
            Stream streamfile = file.InputStream;
            List<TimeEntit> col = new List<TimeEntit>();            
            using (StreamReader rd = new StreamReader(streamfile, System.Text.Encoding.UTF8))
            {
                string[] str = rd.ReadToEnd().Split(new string[] { "\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach(var item in str)
                {
                    string fitem = new string(item.Where(t => char.IsDigit(t)).ToArray());
                    if (fitem ==""){ continue; }
                    TimeEntit te = new TimeEntit();
                    te.IdEntit = 0;
                    te.PortalName = comp.Portal;
                    te.Telefon = fitem.ToString();
                    te.Type = "insert";
                    col.Add(te);
                }

            }
            int allcount = col.Count;
            List<object[]> alldate = SelAllData(taskab.Guid);
            List<TimeEntit> newtimeent = col.Where(i => alldate.Select(s => s[2]).Contains(i.Telefon)).ToList();
            foreach (var item in newtimeent)
            {
                col.Remove(item);
            }
            if (taskab.isFilterNumb == 1)
            {
                col.ToList().ForEach(x => x.Telefon = FilterNumber(x.Telefon));
                col = col.Where(t => t.Telefon != "").ToList();
            }
            InsertEntity(taskab.Guid, col);
            int filtcount = col.Count;
            ViewBag.memberid = member_id;
            ViewBag.filtcount = filtcount;
            ViewBag.allcount = allcount;
            return View();
        }

        public JsonResult SendnewTable( string name, string member_id)
        {
            Compan comp = _db.Compans.Where(i => i.MemberId == member_id).FirstOrDefault();
            TaskAbonent taskab = new TaskAbonent();
            string tablename = CreateTable(comp.Id);
            TaskAbonent abonent = new TaskAbonent();
            abonent.Guid = tablename;
            abonent.NameTable = name;
            abonent.PortalName = comp.Portal;
            abonent.isWork = 0;
            abonent.isFilterNumb = 1;
            _db.TaskAbonents.Add(abonent);           
            _db.SaveChanges();

            return Json("ok");
        }
            





    }
}