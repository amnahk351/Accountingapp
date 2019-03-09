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
                SelectListItem item = new SelectListItem
                {
                    Text = coa.AccountName,
                    Value = coa.AccountNumber.ToString()
                };
                sliAccountList.Add(item);
            }

            //SelectList list = new SelectList(sliAccountList, "Value", "Text");
            ViewBag.accountlist = sliAccountList;
            Database1Entities7 entities = new Database1Entities7();

            return View(entities.Transactions);
        }

        [HttpPost]
        public ActionResult Journalize(Transaction transaction)
        {
            //check model
            //are both d/c 0? do all the d == c?
            //most recent entry id = SELECT TOP 1 * FROM Table ORDER BY ID DESC .entryID
            //foreach transaction entryID = most recent ++

            Trace.WriteLine(transaction.Debit);
            Trace.WriteLine(transaction.AccountNumber);


            Database1Entities3 db = new Database1Entities3();
            List<ChartOfAcc> getaccountslist = db.ChartOfAccs.ToList();
            List<SelectListItem> sliAccountList = new List<SelectListItem>();

            //IEnumerable<ChartOfAcc> accounts = new List<ChartOfAcc> { new ChartOfAcc { AccountNumber = 1234, AccountName = "Test" } };

            foreach (ChartOfAcc coa in getaccountslist)
            {
                SelectListItem item = new SelectListItem
                {
                    Text = coa.AccountName,
                    Value = coa.AccountNumber.ToString()
                };
                sliAccountList.Add(item);
            }

            //SelectList list = new SelectList(sliAccountList, "Value", "Text");
            ViewBag.accountlist = sliAccountList;
            //foreach (Transaction transaction in transactions)
            //{
            //    Trace.WriteLine(transaction.Debit);
            //}
            return View("~/Views/Accountant/AccountantIndex.cshtml");
        }


        [HttpPost]
        public JsonResult InsertJournal(Transaction[] transactions)
        {
            System.Diagnostics.Debug.WriteLine("it got here0");
            using (Database1Entities7 entities = new Database1Entities7())
            {
                //Truncate Table to delete all old records.
                //entities.Database.ExecuteSqlCommand("TRUNCATE TABLE [Customers]");

                //Check for NULL.
                if (transactions == null)
                {
                    System.Diagnostics.Debug.WriteLine("it got here");
                    //transactions = new List<Transaction>();
                }

                //Loop and insert records.
                foreach (Transaction tran in transactions)
                {
                    System.Diagnostics.Debug.WriteLine("it got here2");
                    entities.Transactions.Add(tran);
                }
                int insertedRecords = entities.SaveChanges();
                System.Diagnostics.Debug.WriteLine("it got here3");
                return Json(insertedRecords);
            }
        }


        [HttpPost]
        public JsonResult InsertValue(Transaction[] itemlist)
        {
            System.Diagnostics.Debug.WriteLine("it got here5");

            foreach (Transaction i in itemlist)
            {
                //loop through the array and insert value into database.

            }
            return Json("Ok");
        }
    }
}