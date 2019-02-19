using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountingApp.Controllers
{
    public class AccountantController : Controller
    {
        // GET: Accountant
        //public ActionResult AccountantIndex()
        //{
        //    return View();
        //}
        public ActionResult AccountantIndex()
        {
            Database1Entities3 db = new Database1Entities3();
            var getaccountslist = db.ChartOfAccs.ToList();
            SelectList list = new SelectList(getaccountslist,"AccountNumber", "AccountName");
            ViewBag.accountlist = list;
            return View();
        }
    }
}