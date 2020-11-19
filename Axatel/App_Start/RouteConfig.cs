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
           name: "AmoDefault",
           url: "amo/index",
           defaults: new { controller = "Amo", action = "Index" }
           );
            routes.MapRoute(
            name: "AmoReg",
            url: "method/amo/finish",
            defaults: new { controller = "Amo", action = "Reg" }
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
            name: "Script",
            url: "script",
            defaults: new { controller = "Home", action = "Script" }
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
