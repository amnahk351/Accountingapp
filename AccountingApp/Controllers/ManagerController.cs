using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountingApp.Controllers
{
    public class ManagerController : Controller
    {
        private Database1Entities7 db = new Database1Entities7();
        // GET: Manager
        public ActionResult ManagerIndex()
        {
            return View();
        }
        public ActionResult GeneralJournal()
        {
            var allTransactions = db.Transactions.ToList();
            return View(allTransactions);
        }
    }
}