﻿using AccountingApp.DBAccess;
using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using System.IO;
using Newtonsoft.Json;

namespace AccountingApp.Controllers
{
    public class ManagerController : Controller
    {
        //private Database1Entities7 db = new Database1Entities7();
        //private Database1Entities3 coaDB = new Database1Entities3();
        // GET: Manager
        public ActionResult ManagerIndex(string status)
        {
            
            ViewBag.pendingCount = getAllEntriesOfStatus("pending").entries.Count;
            ViewBag.approvedCount = getAllEntriesOfStatus("approved").entries.Count;
            return View();
            
        }

        public ActionResult ManagerApproval()
        { 
            return View(getAllEntriesOfStatus("pending"));
        }

        public void ApproveEntry(int? id)
        {
            if (ModelState.IsValid)
            {
                List<TransactionTable> allTransactionsWithEntryID;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    allTransactionsWithEntryID = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryID = @EntryID", new { EntryID = id }).ToList();
                }

                foreach (TransactionTable t in allTransactionsWithEntryID)
                {
                    t.Status = "approved";
                    Trace.WriteLine("----------" + t.EntryId + ":" + t.Status);

                    ChartOfAcc coa;
                    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                    {
                        coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where AccountName = @AccountName", new { AccountName = t.AccountName }).FirstOrDefault();
                    }

                    if (coa != null)
                        Trace.WriteLine("-------------" + coa.AccountName);

                    if (t.Debit == null)
                        t.Debit = 0;

                    if (t.Credit == null)
                        t.Credit = 0;

                    if (coa.NormalSide.ToLower() == "debit")
                    {
                        coa.CurrentBalance += t.Debit.Value;
                        coa.CurrentBalance -= t.Credit.Value;
                    }
                    else //normal side is credit
                    {
                        coa.CurrentBalance += t.Credit.Value;
                        coa.CurrentBalance -= t.Debit.Value;
                    }

                    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                    {
                        string sql = $"UPDATE dbo.TransactionTable SET Status = @status WHERE EntryID = @entryID";
                        db.Execute(sql, new { status = t.Status, entryID = t.EntryId });
                    }

                    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                    {
                        string sql = $"UPDATE dbo.ChartOfAccounts SET CurrentBalance = @currentBalance WHERE AccountName = @accountName";
                        db.Execute(sql, new { currentBalance = coa.CurrentBalance, accountName = coa.AccountName });
                    }
                }
            }
            else
                Trace.WriteLine("no");

            Response.Write("<script language=javascript>alert('journal updated'); window.location.href = '../ManagerApproval';</script>");
        }

        public ActionResult DisapproveEntry(int? id)
        {
            List<TransactionTable> allTransactionsWithEntryID;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                allTransactionsWithEntryID = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryID = @EntryID", new { EntryID = id }).ToList();
            }

            foreach (TransactionTable t in allTransactionsWithEntryID)
            {
                t.Status = "disapproved";

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    string sql = $"UPDATE dbo.TransactionTable SET Status = @status WHERE EntryID = @entryID";
                    db.Execute(sql, new { status = t.Status, entryID = t.EntryId });
                }
            }
            

            return View();
        }

        public ActionResult Journalize()
        {
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            List<SelectListItem> sliAccountList = new List<SelectListItem>();


            foreach (ChartOfAcc coa in listAccounts)
            {
                SelectListItem item = new SelectListItem
                {
                    Text = coa.AccountName,
                    Value = coa.AccountNumber.ToString()
                };
                sliAccountList.Add(item);
            }

            ViewBag.accountlist = sliAccountList;


            return View();
        }

        [HttpPost]
        public ActionResult Journalize(TransactionTable transaction)
        {
            Trace.WriteLine(transaction.Debit);
            //Trace.WriteLine(transaction.AccountNumber);

            List<ChartOfAcc> listAccounts;
            bool t = true;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where Active=@Value", new { Value = t }).ToList();
            }
            List<SelectListItem> sliAccountList = new List<SelectListItem>();

            foreach (ChartOfAcc coa in listAccounts)
            {
                SelectListItem item = new SelectListItem
                {
                    Text = coa.AccountName,
                    Value = coa.AccountNumber.ToString()
                };
                sliAccountList.Add(item);
            }

            ViewBag.accountlist = sliAccountList;
            return View("~/Views/Accountant/AccountantIndex.cshtml");
        }
        


        public ActionResult GeneralJournal(string status)
        {

            //List<Transaction> allTransactions;
            //using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            //{
            //    allTransactions = db.Query<Transaction>($"Select * From dbo.TransactionTable").ToList();
            //}
            if (status == null || status == "")
                return View(getAllEntriesOfStatus("approved"));
            else
                return View(getAllEntriesOfStatus(status));
        }

        public ActionResult TrialBalance()
        {

            List<ChartOfAcc> coa;
            decimal debTotal = 0;
            decimal credTotal = 0;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();
                foreach (ChartOfAcc c in coa)
                {
                    if (c.NormalSide.ToLower() == "debit")
                        debTotal += c.CurrentBalance.Value;
                    else
                        credTotal += c.CurrentBalance.Value;
                }

                ViewBag.DebitTotal = debTotal;
                ViewBag.CreditTotal = credTotal;
            }

            return View(coa);
        }

        public ActionResult IncomeStatement()
        {

            List<ChartOfAcc> coa;
            decimal revenueTotal = 0;
            decimal expenseTotal = 0;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();
                foreach (ChartOfAcc c in coa)
                {
                    if (c.AccountType.ToLower() == "revenue")
                        revenueTotal += c.CurrentBalance.Value;
                    if (c.AccountType.ToLower() == "expense")
                        expenseTotal += c.CurrentBalance.Value;


                }

                ViewBag.RevenueTotal = revenueTotal;
                ViewBag.ExpenseTotal = expenseTotal;
                ViewBag.NetIncome_Loss = revenueTotal - expenseTotal;
            }

            return View(coa);
        }

        public ActionResult BalanceSheet()
        {

            List<ChartOfAcc> coa;
            decimal totalCurrentAssets = 0;
            decimal totalAssets = 0;
            decimal propPlanEquipNet = 0;
            decimal totalCurrentLiabilities = 0;
            decimal totalLiabilities = 0;
            decimal retainedEarnings = 0;
            decimal totalStockHolderEquity = 0;
            decimal totalLiabilitesStockEquity = 0;
            decimal unearnedRevenue = 0;
            decimal contributedCapital = 0;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();
                
                for(int i = 0; i < coa.Count; i++)
                {
                    if (coa[i].AccountName.ToLower() == "cash" || coa[i].AccountName.ToLower() == "accounts receivable" || coa[i].AccountName.ToLower() == "supplies" || coa[i].AccountName.ToLower() == "prepaid insurance" || coa[i].AccountName.ToLower() == "prepaid rent")
                    {
                        totalCurrentAssets += coa[i].CurrentBalance.Value;
                    }
                    else if (coa[i].AccountName.ToLower() == "office supplies")
                    {
                        propPlanEquipNet += coa[i].CurrentBalance.Value;
                    }
                    else if (coa[i].AccountName.ToLower() == "accumulated depreciation equipment")
                    {
                        propPlanEquipNet -= coa[i].CurrentBalance.Value;
                    }
                    else if (coa[i].AccountName.ToLower() == "accounts payable" || coa[i].AccountName.ToLower() == "salaries payable")
                    {
                        totalCurrentLiabilities += coa[i].CurrentBalance.Value;
                    }
                    else if (coa[i].AccountName.ToLower() == "unearned revenue")
                    {
                        unearnedRevenue += coa[i].CurrentBalance.Value;
                    }
                    else if (coa[i].AccountName.ToLower() == "contributed capital")
                    {
                        contributedCapital += coa[i].CurrentBalance.Value;
                    }
                }
                
            }

            totalAssets = totalCurrentAssets + propPlanEquipNet;
            totalLiabilities = unearnedRevenue + totalCurrentLiabilities;

            retainedEarnings = totalAssets - totalLiabilities - contributedCapital;

            totalStockHolderEquity = retainedEarnings + contributedCapital;
            totalLiabilitesStockEquity = totalStockHolderEquity + totalLiabilities;

            ViewBag.totalCurrentAssets = totalCurrentAssets;
            ViewBag.totalAssets = totalAssets;
            ViewBag.propPlanEquipNet = propPlanEquipNet;
            ViewBag.totalCurrentLiabilities = totalCurrentLiabilities;
            ViewBag.totalLiabilities = totalLiabilities;
            ViewBag.retainedEarnings = retainedEarnings;
            ViewBag.totalStockHolderEquity = totalStockHolderEquity;
            ViewBag.totalLiabilitesStockEquity = totalLiabilitesStockEquity;

            return View(coa);
        }

        [HttpGet]
        public ActionResult RetrieveAccountBalanceAndStatus(string name)
        {
            System.Diagnostics.Debug.WriteLine("account name" + name);
            //List<ChartOfAcc> SpecificAccount;
            List<ChartOfAcc> Chart;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                Chart = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where AccountName = @N", new { N = name }).ToList();
            }

            //using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            //{
            //    SpecificAccount = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where AccountName=@Name", new { Name = name }).ToList();
            //}


            System.Diagnostics.Debug.WriteLine("count " + Chart.Count);
            decimal Num = (decimal)Chart[0].CurrentBalance;
            bool ActiveType = Chart[0].Active;
            //string comment = AllAccounts[0].AccountantComment;
            string split = "|^|";
            string res = Num + split + ActiveType;
            var result = JsonConvert.SerializeObject(res);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApproveSpecifiedEntry(int id, string comment)
        {
            System.Diagnostics.Debug.WriteLine("it got called");
            System.Diagnostics.Debug.WriteLine("id " + id);
            System.Diagnostics.Debug.WriteLine("comm " + comment);
            string s = "approved";
            var sessionUser = Session["Username"] as string;

            List<TransactionTable> transactionList;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId = @ID", new { ID = id }).ToList();
            }

                        
            for (int i = 0; i < transactionList.Count; i++) {

                //POST REFERENCE LOGIC
                //get the account name at i
                string AccountName = transactionList[i].AccountName;
                decimal RowDebit = (decimal) transactionList[i].Debit.GetValueOrDefault();
                decimal RowCredit = (decimal) transactionList[i].Credit.GetValueOrDefault();

                List<TransactionTable> TempList;

                //query transaction table database and get all records with that account name and status is approved
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    TempList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where AccountName = @Name And Status = @Stat", new { Name = AccountName, Stat = s }).ToList();
                }                

                //get post reference number at the selected query and put in a list<int>
                List<int> ReferenceNumbers = new List<int>();

                foreach (TransactionTable t in TempList) {
                    ReferenceNumbers.Add(t.PostReference.GetValueOrDefault());
                    //System.Diagnostics.Debug.WriteLine("current post refere " + t.PostReference.GetValueOrDefault());
                }

                int biggest = 0;

                if (ReferenceNumbers.Count != 0)
                {
                    biggest = ReferenceNumbers.Max();
                }
                
                //System.Diagnostics.Debug.WriteLine("biggest " + biggest);

                int PostReference;

                List<ChartOfAcc> Chart;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    Chart = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where AccountName = @Name", new { Name = AccountName }).ToList();
                }

                if (biggest == 0)
                {
                    //if list has null values, (this is first post reference for the account name)
                    int AccountNum = Chart[0].AccountNumber;

                    //take the account number and multiply it by 5055 and set that as the first post reference number    
                    PostReference = AccountNum * 5055;
                }
                else {
                    //take the maximum and add 1
                    PostReference = biggest + 1;
                }

                //System.Diagnostics.Debug.WriteLine("post ref " + PostReference);

                decimal CurrentAccountBalance = (decimal) Chart[0].CurrentBalance;
                string NormalSide = Chart[0].NormalSide;

                decimal NewBalance = CurrentAccountBalance;

                if (RowCredit == 0 && NormalSide == "Debit")
                {
                    //Debit has a value, it is an asset or expense account
                    //Debits increase asset and expense accounts.
                    NewBalance += RowDebit;
                }

                if (RowCredit == 0 && NormalSide == "Credit")
                {
                    //Debit has a value, it is a liability, equity, or revenue account
                    //Debits decrease liability, equity, and revenue accounts.
                    NewBalance -= RowDebit;
                }

                if (RowDebit == 0 && NormalSide == "Debit")
                {
                    //Credit has a value, it is an asset or expense account
                    //Credits decrease asset and expense accounts.
                    NewBalance -= RowCredit;
                }

                if (RowDebit == 0 && NormalSide == "Credit")
                {
                    //Credit has a value, it is a liability, equity, or revenue account
                    //Credits increase liability, equity, and revenue accounts.
                    NewBalance += RowCredit;
                }

                System.Diagnostics.Debug.WriteLine("current account " + AccountName);
                System.Diagnostics.Debug.WriteLine("new balance " + NewBalance);

                //update transaction table at i with the post reference, add before balance and after balance
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    string sql = $"UPDATE dbo.TransactionTable SET PostReference = @Post, BeforeBalance = @Be, AfterBalance = @Af WHERE EntryID = @entryID And AccountName = @AccName";
                    db.Execute(sql, new { Post = PostReference, Be = CurrentAccountBalance, Af = NewBalance, entryID = id, AccName = AccountName });
                }


                //update current balance in chart of accounts
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    string sql = $"UPDATE dbo.ChartOfAccounts SET CurrentBalance = @Cu WHERE AccountName = @AccName";
                    db.Execute(sql, new { Cu = NewBalance, AccName = AccountName });
                }

            }

            
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                string sql = $"UPDATE dbo.TransactionTable SET ManagerUsername = @User, ManagerComment = @Comm, DateReviewed = @Date, Status = @status WHERE EntryID = @entryID";
                db.Execute(sql, new { User = sessionUser, Comm = comment, Date = DateTime.Now, status = s, entryID = id });
            }


            return Json("Entry Approved.");
        }

        public ActionResult PendingTransactions()
        {

            return View(getAllEntriesOfStatus("pending"));
        }

        public ActionResult Transactions()
        {

            return View(getAllEntriesOfStatus("all"));
        }

        public ActionResult SuspendedTransactions()
        {

            return View(getAllEntriesOfStatus("suspended"));
        }

        public ActionResult DisapprovedTransactions()
        {

            return View(getAllEntriesOfStatus("disapproved"));
        }

        public ActionResult ApprovedTransactions()
        {

            return View(getAllEntriesOfStatus("approved"));
        }

        private Entries getAllEntriesOfStatus(string s)
        {

            List<TransactionTable> transactionList;
            //List<DocumentsTable> fileList = new List<DocumentsTable>();


            if (s == "all")
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable").ToList();
                }
            }
            else
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where Status = @status", new { status = s }).ToList();
                }
            }


            //using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            //{
            //    fileList = db.Query<DocumentsTable>($"Select * From dbo.DocumentsTable").ToList();
            //}


            Entries entries = new Entries();
            List<int> ids = new List<int>();
            foreach (TransactionTable t in transactionList)
            {
                int id = t.EntryId.Value;


                //string status = t.Status;
                //DateTime date = t.DateSubmitted.GetValueOrDefault();
                //string comment = t.AccountantComment;

                if (ids.Contains(id))
                    continue;
                else
                    ids.Add(id);

                //Entry e = new Entry(id, status, date, comment);

                Entry NewE = new Entry();
                NewE.entryID = id;
                NewE.status = t.Status;
                NewE.DateSubmitted = t.DateSubmitted.GetValueOrDefault();
                NewE.DateReviewed = t.DateReviewed.GetValueOrDefault();
                NewE.AccountantComment = t.AccountantComment;
                NewE.ManagerComment = t.ManagerComment;
                NewE.AccountantUsername = t.AccountantUsername;
                NewE.ManagerUsername = t.ManagerUsername;
                NewE.PostReference = t.PostReference.GetValueOrDefault();


                foreach (TransactionTable t2 in transactionList)
                {
                    if (t2.EntryId == id)
                    {
                        //for (int i = 0; i < fileList.Count; i++)
                        //{

                        //    if (fileList[i].FK_EntryId == t2.EntryId)
                        //    {
                        //        e.files.Add(fileList[i]);
                        //    }
                        //}

                        //e.accountNames.Add(t2.AccountName);
                        //e.debits.Add(t2.Debit.GetValueOrDefault());
                        //e.credits.Add(t2.Credit.GetValueOrDefault());

                        NewE.accountNames.Add(t2.AccountName);
                        NewE.debits.Add(t2.Debit.GetValueOrDefault());
                        NewE.credits.Add(t2.Credit.GetValueOrDefault());
                    }
                }
                entries.entries.Add(NewE);
            }

            return entries;
        }

        public ActionResult RetainedEarnings()
        {
            List<ChartOfAcc> coa;
            decimal revenueTotal = 0;
            decimal expenseTotal = 0;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();
                foreach (ChartOfAcc c in coa)
                {
                    if (c.AccountType.ToLower() == "revenue")
                        revenueTotal += c.CurrentBalance.Value;
                    if (c.AccountType.ToLower() == "expense")
                        expenseTotal += c.CurrentBalance.Value;


                }
            }
            ViewBag.NetIncome = revenueTotal - expenseTotal;
            return View();
        }

        public ActionResult PostClosingTrialBalance()
        {
            List<ChartOfAcc> coa;
            decimal debTotal = 0;
            decimal credTotal = 0;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active And Not AccountType = @revenue And Not AccountType = @expense",
                    new { active = true, revenue = "Revenue", expense = "Expense" }).ToList();
                foreach (ChartOfAcc c in coa)
                {
                    if (c.NormalSide.ToLower() == "debit")
                        debTotal += c.CurrentBalance.Value;
                    else
                        credTotal += c.CurrentBalance.Value;
                }

                ViewBag.DebitTotal = debTotal;
                ViewBag.CreditTotal = credTotal;
            }

            return View(coa);
        }

        public ActionResult ShowUserData()
        {
            List<CreateUser> listUser;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listUser = db.Query<CreateUser>($"Select * from dbo.Usertable").ToList();
            }

            return View(listUser);
            //var item = db.CreateUsers.ToList();
            //return View(item);
        }
        public ActionResult ChartOfAccounts()
        {
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            return View(listAccounts);
        }


        //Broderick's code
        public ActionResult EventLog()
        {
            List<Models.EventLog> events;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                events = db.Query<Models.EventLog>($"Select * from dbo.EventLogTable").ToList();
            }

            return View(events);
            //Database1Entities6 db2 = new Database1Entities6();
            //var events = db2.EventLogs.ToList();
            //return View(events);
        }
    }
}