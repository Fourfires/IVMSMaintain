using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IVMSMaintain.Models;
using Models;
using Newtonsoft.Json;
using Webdiyer.WebControls.Mvc;

namespace IVMSMaintain.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult IvmsQuery(string searchkey,int pageIndex = 1)
        {
            ViewBag.Message = "站点查询.";
            if (string.IsNullOrEmpty(searchkey))
                searchkey = string.Empty;
            using (var db = new IvmsmaintainDB("localhost_ivmsmaintain"))
            {
                var totalList = (from p in db.TServerManager
                    where p.StationName.Contains(searchkey) || p.Ip.Contains(searchkey)
                    select p).ToPagedList(pageIndex,20);

                if (Request.IsAjaxRequest())
                {
                    return PartialView("Pager", totalList);
                }
                return View(totalList);
            }
        }
        public JsonResult GetStationList(int limit,int offset = 1)
        {
            ViewBag.Message = "站点查询.";
            using (var db = new IvmsmaintainDB("localhost_ivmsmaintain"))
            {
                var totalList = (from p in db.TServerManager
                    //where p.StationName.Contains(searchkey) || p.Ip.Contains(searchkey)
                    select p).ToList();
                totalList = totalList.Skip(offset).Take(limit).ToList();
                return Json(new {total = totalList.Count, rows = totalList}, JsonRequestBehavior.AllowGet);
            }
        }
    }
}