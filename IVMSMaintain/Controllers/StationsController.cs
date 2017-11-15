using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models;

namespace IVMSMaintain.Controllers
{
    public class StationsController : Controller
    {
        // GET: Stations
        public ActionResult Index()
        {
            using (var db = new IvmsmaintainDB("localhost_ivmsmaintain"))
            {
                var totalList = (from p in db.TServerManager
                   // where p.StationName.Contains(searchkey) || p.Ip.Contains(searchkey)
                    select p).ToList();
               
                return View(totalList);
            }
        }

        //// GET: Stations/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: Stations/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Stations/Create
        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Stations/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Stations/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Stations/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Stations/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
