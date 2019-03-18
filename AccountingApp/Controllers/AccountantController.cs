using AccountingApp.Models;
using AccountingApp.DBAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.IO;

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
           // Database1Entities3 db = new Database1Entities3();
            //List<ChartOfAcc> getaccountslist = db.ChartOfAccs.ToList();
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            List<SelectListItem> sliAccountList = new List<SelectListItem>();

            //IEnumerable<ChartOfAcc> accounts = new List<ChartOfAcc> { new ChartOfAcc { AccountNumber = 1234, AccountName = "Test" } };

            //foreach (ChartOfAcc coa in getaccountslist)
            //{
            //    SelectListItem item = new SelectListItem
            //    {
            //        Text = coa.AccountName,
            //        Value = coa.AccountNumber.ToString()
            //    };
            //    sliAccountList.Add(item);
            //}
            foreach (ChartOfAcc coa in listAccounts)
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
            //Database1Entities7 entities = new Database1Entities7();

            return View();
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


            //Database1Entities3 db = new Database1Entities3();
            //List<ChartOfAcc> getaccountslist = db.ChartOfAccs.ToList();
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            List<SelectListItem> sliAccountList = new List<SelectListItem>();
            

            //IEnumerable<ChartOfAcc> accounts = new List<ChartOfAcc> { new ChartOfAcc { AccountNumber = 1234, AccountName = "Test" } };

            //foreach (ChartOfAcc coa in getaccountslist)
            //{
            //    SelectListItem item = new SelectListItem
            //    {
            //        Text = coa.AccountName,
            //        Value = coa.AccountNumber.ToString()
            //    };
            //    sliAccountList.Add(item);
            //}

            foreach (ChartOfAcc coa in listAccounts)
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


        public double GetAccountNumber(string AccName) {

            //query COA and get number
            var coaDB = new Database1Entities3();

            var Account = coaDB.ChartOfAccs.Where(x => x.AccountName == AccName).FirstOrDefault();
            
            return Account.AccountNumber;
        }


        [HttpPost]
        public JsonResult InsertJournal(Transaction[] transactions)
        {
            using (Database1Entities7 entities = new Database1Entities7())
            {
                if (transactions == null)
                {
                    throw new Exception("NO TRANSACTIONS");
                }

                var mostRecentEntryID = entities.Transactions.ToList().Select(eID => eID.EntryId).LastOrDefault();

                var coaDB = new Database1Entities3();
                int insertedRecords = 0;

                for (int i = 1; i < transactions.Length; i++)
                {
                    //query COA
                    //Trace.WriteLine("---------------------" + transactions[i].AccountName);

                    //var coa = coaDB.ChartOfAccs.Find(transactions[i].AccountName);  //needs primary key


                    var AccName = transactions[i].AccountName;
                    var coa = coaDB.ChartOfAccs.Where(x => x.AccountName == AccName).FirstOrDefault();

                    if (coa == null)
                        Trace.WriteLine("Could not find COA");

                    if (transactions[i].Debit == null)
                        transactions[i].Debit = 0;

                    if (transactions[i].Credit == null)
                        transactions[i].Credit = 0;

                    if (coa.NormalSide.ToLower() == "debit")
                    {
                        coa.CurrentBalance += transactions[i].Debit.Value;
                        coa.CurrentBalance -= transactions[i].Credit.Value;
                    }
                    else //normal side is credit
                    {
                        coa.CurrentBalance += transactions[i].Credit.Value;
                        coa.CurrentBalance -= transactions[i].Debit.Value;
                    }
                    coaDB.SaveChanges();

                    Transaction tran = new Transaction();
                    tran.DateSubmitted = transactions[i].DateSubmitted;
                    tran.AccountName = transactions[i].AccountName;
                    tran.AccountNumber = GetAccountNumber(AccName);
                    tran.Debit = transactions[i].Debit;
                    tran.Credit = transactions[i].Credit;

                    if (i == 1) {
                        tran.Comment = transactions[i].Comment;
                    }

                    tran.EntryId = mostRecentEntryID + 1;

                    //tran.Status = "pending";
                    tran.Status = transactions[i].Status;
                    entities.Transactions.Add(tran);

                    //transactions[i].AccountNumber = GetAccountNumber(AccName);
                    //transactions[i].EntryId = mostRecentEntryID + 1;
                    //transactions[i].Status = "pending";
                    //entities.Transactions.Add(transactions[i]);  //this line adds everything that is already filled like debit/credit and account
                    entities.SaveChanges();
                    insertedRecords++;
                }

                //Transaction tran = new Transaction();
                

                //for(int i = 1; i < transactions.Length; i++)
                //{
                //    tran.DateSubmitted = transactions[i].DateSubmitted;
                //    tran.AccountNumber = transactions[i].AccountNumber;
                //    tran.Debit = transactions[i].Debit;
                //    tran.Credit = transactions[i].Credit;

                //    entities.Transactions.Add(tran);
                //    entities.SaveChanges();
                //    insertedRecords++;
                //}
                return Json(insertedRecords);
            }
        }


        [HttpPost]
        public ActionResult UploadFiles()
        {
            Database1Entities7 entities = new Database1Entities7();
            var mostRecentEntryID = entities.Transactions.ToList().Select(eID => eID.EntryId).LastOrDefault() + 1;  //add a 1 to match folder with EntryId
            string foldername = mostRecentEntryID.ToString();

            string folder = Server.MapPath(string.Format("~/User_Uploads/{0}/", foldername));

            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            string location = "~/User_Uploads/" + foldername + "/";

            string path = Server.MapPath(location);
            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFileBase file = files[i];
                file.SaveAs(path + file.FileName);
            }
            return Json(files.Count + " Files Uploaded!");
        }


    }
}