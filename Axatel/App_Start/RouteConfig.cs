using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Axatel
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapRoute(
            name: "Default",
            url: "",
            defaults: new { controller = "Home", action = "Index" }
        );
            routes.MapRoute(
            name: "Test",
            url: "test",
            defaults: new { controller = "Home", action = "Test" }
            );
            routes.MapRoute(
            name: "Test2",
            url: "test2",
            defaults: new { controller = "Home", action = "Test2" }
            );
            routes.MapRoute(
            name: "Test3",
            url: "test3",
            defaults: new { controller = "Home", action = "Test3" }
            );
            routes.MapRoute(
            name: "Test4",
            url: "test4",
            defaults: new { controller = "Home", action = "Test4" }
            );
            routes.MapRoute(
            name: "Test5",
            url: "test5",
            defaults: new { controller = "Home", action = "Test5" }
            );

            routes.MapRoute(
            name: "TestUpFile",
            url: "testupfile",
            defaults: new { controller = "Home", action = "TestUpFile" }
            );
            
            routes.MapRoute(
            name: "Reciver",
            url: "reciver",
            defaults: new { controller = "Home", action = "Reciver" }
            );
            routes.MapRoute(
            name: "reg",
            url: "method/reg",
            defaults: new { controller = "Home", action = "Reg" }
            );

            routes.MapRoute(
            name: "Showgroup",
            url: "method/showgroup",
            defaults: new { controller = "Home", action = "Showgroup" }
            );

            routes.MapRoute(
            name: "Finish",
            url: "method/finish",
            defaults: new { controller = "Home", action = "Finish" }
            );
            routes.MapRoute(
            name: "Record",
            url: "method/recordatt",
            defaults: new { controller = "Home", action = "Record" }
            );
            routes.MapRoute(
            name: "Hidegroup",
            url: "method/hidegroup",
            defaults: new { controller = "Home", action = "Hidegroup" }
            );
            routes.MapRoute(
            name: "Transfer",
            url: "method/showgroup/transfer",
            defaults: new { controller = "Home", action = "Transfer" }
            );
            routes.MapRoute(
            name: "ClickToCall",
            url: "clicktocall",
            defaults: new { controller = "Home", action = "ClickToCall" }
            );
            routes.MapRoute(
            name: "BackToCall",
            url: "backtocall",
            defaults: new { controller = "Home", action = "BackToCall" }
            );
            
            routes.MapRoute(
            name: "Licenzia",
            url: "licenzia",
            defaults: new { controller = "Home", action = "Licenzia" }
            );

            routes.MapRoute(
            name: "Uslovie",
            url: "uslovie",
            defaults: new { controller = "Home", action = "Uslovie" }
            );
            routes.MapRoute(
            name: "GetOperators",
            url: "getoperators",
            defaults: new { controller = "Home", action = "GetOperators" }
            );

            routes.MapRoute(
            name: "FaxAtt",
            url: "method/faxatt",
            defaults: new { controller = "Home", action = "FaxAtt" }
            );
            routes.MapRoute(
            name: "RegLead",
            url: "method/reglead",
            defaults: new { controller = "Home", action = "RegLead" }
            );
            routes.MapRoute(
            name: "RegaCall",
            url: "method/regacall",
            defaults: new { controller = "Home", action = "RegaCall" }
            );
            


            routes.MapRoute(
             name: "EmailSend",
             url: "method/emailsend",
             defaults: new { controller = "Home", action = "EmailSend" }
             );
            routes.MapRoute(
             name: "SmsSend",
             url: "method/smssend",
             defaults: new { controller = "Home", action = "SmsSend" }
             );
            routes.MapRoute(
             name: "Notify",
             url: "method/notify",
             defaults: new { controller = "Home", action = "Notify" }
             );
            routes.MapRoute(
             name: "BlackList",
             url: "blacklist",
             defaults: new { controller = "Home", action = "BlackList" }
             );
            routes.MapRoute(
              name: "AddNumb",
              url: "addnumb",
              defaults: new { controller = "Home", action = "AddNumb" }
              );
            routes.MapRoute(
               name: "DellBlackList",
               url: "dellblacklist",
               defaults: new { controller = "Home", action = "DellBlackList" }
               );
            routes.MapRoute(
           name: "Tiket",
           url: "tiket",
           defaults: new { controller = "Home", action = "Tiket" }
           );
            routes.MapRoute(
            name: "Intro",
            url: "intro",
            defaults: new { controller = "Home", action = "Intro" }
            );
            routes.MapRoute(
             name: "ActivPortal",
             url: "activportal/{id}",
             defaults: new { controller = "Home", action = "ActivPortal", id = UrlParameter.Optional }
             );
            routes.MapRoute(
           name: "DealApp",
           url: "method/dealapp",
           defaults: new { controller = "Home", action = "DealApp" }
           );

            routes.MapRoute(
           name: "Log",
           url: "route.by/clicktocall",
           defaults: new { controller = "Home", action = "Log" }
           );

            routes.MapRoute(
            name: "LoadImgScript",
            url: "Content/addscript/{idport}",
            defaults: new { controller = "Home", action = "LoadImgScript",idport = UrlParameter.Optional }
            );
            routes.MapRoute(
            name: "ParsCont",
            url: "tablabon",
            defaults: new { controller = "ParsCont", action = "Index" }
            );
            routes.MapRoute(
            name: "SearchLead",
            url: "searchlead",
            defaults: new { controller = "ParsCont", action = "SearchLead" }
            );
            routes.MapRoute(
            name: "SearchCont",
            url: "searchcont",
            defaults: new { controller = "ParsCont", action = "SearchCont" }
            );
            routes.MapRoute(
            name: "SearchDeal",
            url: "searchdeal",
            defaults: new { controller = "ParsCont", action = "SearchDeal" }
            );
            routes.MapRoute(
            name: "SearchComp",
            url: "searchcomp",
            defaults: new { controller = "ParsCont", action = "SearchComp" }
            );

            routes.MapRoute(
            name: "AddComps",
            url: "addcomps",
            defaults: new { controller = "ParsCont", action = "AddComps" }
            );
            routes.MapRoute(
            name: "AddDeals",
            url: "adddeals",
            defaults: new { controller = "ParsCont", action = "AddDeals" }
            );
            routes.MapRoute(
            name: "AddConts",
            url: "addconts",
            defaults: new { controller = "ParsCont", action = "AddConts" }
            );
            routes.MapRoute(
            name: "AddLeads",
            url: "addleads",
            defaults: new { controller = "ParsCont", action = "AddLeads" }
            );
            routes.MapRoute(
            name: "Number",
            url: "method/callmanager/number",
            defaults: new { controller = "ParsCont", action = "Number" }
            );

            routes.MapRoute(
            name: "CallmanagerFinish",
            url: "method/callmanager/finish",
            defaults: new { controller = "ParsCont", action = "Finish" }
            );
            routes.MapRoute(
            name: "Onworkabon",
            url: "onworkabon",
            defaults: new { controller = "ParsCont", action = "Onworkabon" }
            );
            routes.MapRoute(
            name: "ClearData",
            url: "cleardata",
            defaults: new { controller = "ParsCont", action = "ClearData" }
            );
            routes.MapRoute(
            name: "ClearIsgetStat",
            url: "clearisgetstat",
            defaults: new { controller = "ParsCont", action = "ClearIsgetStat" }
            );
            routes.MapRoute(
            name: "GetStat",
            url: "getstat",
            defaults: new { controller = "ParsCont", action = "GetStat" }
            );
            routes.MapRoute(
            name: "ExCSV",
            url: "excsv",
            defaults: new { controller = "ParsCont", action = "ExCSV" }
            );
            routes.MapRoute(
            name: "InpCsv",
            url: "inpcsv",
            defaults: new { controller = "ParsCont", action = "InpCSV" }
            );
            routes.MapRoute(
            name: "SaveVoiceBot",
            url: "savevoicebot",
            defaults: new { controller = "ParsCont", action = "SaveVoiceBot" }
            );
            


            routes.MapRoute(
            name: "CountdealsProg",
            url: "countdealsprog",
            defaults: new { controller = "ParsCont", action = "CountdealsProg" }
            );
            routes.MapRoute(
            name: "DellTabl",
            url: "delltabl",
            defaults: new { controller = "ParsCont", action = "DellTabl" }
            );
            routes.MapRoute(
            name: "SendnewTable",
            url: "sendnewtable",
            defaults: new { controller = "ParsCont", action = "SendnewTable" }
            );
            routes.MapRoute(
            name: "SetTable",
            url: "settable",
            defaults: new { controller = "ParsCont", action = "SetTable" }
            );
            routes.MapRoute(
            name: "Roistat",
            url: "method/roistat",
            defaults: new { controller = "Home", action = "Roistat" }
            );






























            routes.MapRoute(
           name: "AmoDefault",
           url: "amo/index",
           defaults: new { controller = "Amo", action = "Index" }
           );
            routes.MapRoute(
            name: "AmoReg",
            url: "method/amo/finish",
            defaults: new { controller = "Amo", action = "Finish" }
            );
            routes.MapRoute(
            name: "AmoShowEvent",
            url: "method/amo/showgroup",
            defaults: new { controller = "Amo", action = "ShowEvent" }
            );
            routes.MapRoute(
            name: "AmoLoadUser",
            url: "amo/loaduser",
            defaults: new { controller = "Amo", action = "LoadUser" }
            );
            routes.MapRoute(
            name: "AmoClickToCall",
            url: "amo/clicktocall",
            defaults: new { controller = "Amo", action = "ClickToCall" }
            );
            routes.MapRoute(
            name: "AmoTest",
            url: "amo/test",
            defaults: new { controller = "Amo", action = "Test" }
            );

            routes.MapRoute(
            name: "AmoRegContact",
            url: "method/amo/reg",
            defaults: new { controller = "Amo", action = "RegContact" }
            );
            routes.MapRoute(
            name: "AmoCreatDeal",
            url: "method/amo/creatdeal",
            defaults: new { controller = "Amo", action = "CreatDeal" }
            );
            routes.MapRoute(
            name: "GetParam",
            url: "method/amo/getparam",
            defaults: new { controller = "Amo", action = "GetParam"}
            );
            routes.MapRoute(
            name: "SetParam",
            url: "method/amo/setparam",
            defaults: new { controller = "Amo", action = "SetParam" }
            );
            




            routes.MapRoute(
            name: "AlfacrmZvonok",
            url: "method/alfacrm/alert",
            defaults: new { controller = "Alfacrm", action = "Zvonok" }
            );
            routes.MapRoute(
            name: "AlfacrmActive",
            url: "method/alfacrm/active",
            defaults: new { controller = "Alfacrm", action = "Active" }
            );
            routes.MapRoute(
                name: "AlfacrmClose",
                url: "method/alfacrm/release",
                defaults: new { controller = "Alfacrm", action = "Close" }
                );
            
            routes.MapRoute(
            name: "AlfacrmZvonokTest",
            url: "method/alfacrm/test",
            defaults: new { controller = "Alfacrm", action = "Test" }
            );

















            routes.MapRoute(
            name: "OnScript",
            url: "onscript",
            defaults: new { controller = "Home", action = "OnScript" }
            );

            
            routes.MapRoute(
            name: "Script",
            url: "script/{member_id}/{id}",
            defaults: new { controller = "Home", action = "Script", id = UrlParameter.Optional, member_id = UrlParameter.Optional }
            );
             
            routes.MapRoute(
            name: "DellScript",
            url: "dellscript",
            defaults: new { controller = "Home", action = "DellScript" }
            );
            
            routes.MapRoute(
            name: "Writer",
            url: "writer",
            defaults: new { controller = "Home", action = "Writer" }
            );
            routes.MapRoute(
            name: "AddScript",
            url: "addscript",
            defaults: new { controller = "Home", action = "AddScript" }
            );
            
            routes.MapRoute(
            name: "MainPage",
            url: "mainpage",
            defaults: new { controller = "Home", action = "MainPage" }
            );

            



        }
    }
}
