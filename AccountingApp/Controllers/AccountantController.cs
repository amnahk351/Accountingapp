using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            List<ChartOfAcc> getaccountslist = db.ChartOfAccs.ToList();
            List<SelectListItem> sliAccountList = new List<SelectListItem>();

            //IEnumerable<ChartOfAcc> accounts = new List<ChartOfAcc> { new ChartOfAcc { AccountNumber = 1234, AccountName = "Test" } };

            foreach (ChartOfAcc coa in getaccountslist)
            {
                sliAccountList.Add(new SelectListItem() { Text = coa.AccountName, Value = coa.AccountNumber.ToString() });
            }

            //SelectList list = new SelectList(sliAccountList, "Value", "Text");
            ViewBag.accountlist = sliAccountList;
            return View();
        }

        public ActionResult Journalize(Transaction transaction)
        {
            //check model
            //are both d/c 0? do all the d == c?
            //most recent entry id = SELECT TOP 1 * FROM Table ORDER BY ID DESC .entryID
            //foreach transaction entryID = most recent ++

            Trace.WriteLine(transaction.Debit);

            //foreach (Transaction transaction in transactions)
            //{
            //    Trace.WriteLine(transaction.Debit);
            //}
            return View("~/Views/Accountant/AccountantIndex.cshtml");
        }
    }
}