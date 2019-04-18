using AccountingApp.DBAccess;
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
                List<Transaction> allTransactionsWithEntryID;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    allTransactionsWithEntryID = db.Query<Transaction>($"Select * From dbo.TransactionTable Where EntryID = @EntryID", new { EntryID = id }).ToList();
                }

                foreach (Transaction t in allTransactionsWithEntryID)
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
            List<Transaction> allTransactionsWithEntryID;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                allTransactionsWithEntryID = db.Query<Transaction>($"Select * From dbo.TransactionTable Where EntryID = @EntryID", new { EntryID = id }).ToList();
            }

            foreach (Transaction t in allTransactionsWithEntryID)
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
        public ActionResult Journalize(Transaction transaction)
        {
            Trace.WriteLine(transaction.Debit);
            Trace.WriteLine(transaction.AccountNumber);

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

        private Entries getAllEntriesOfStatus(string s)
        {

            List<Transaction> transactionList;

            if (s == "all")
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    transactionList = db.Query<Transaction>($"Select * From dbo.TransactionTable").ToList();
                }
            }
            else
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    transactionList = db.Query<Transaction>($"Select * From dbo.TransactionTable Where Status = @status", new { status = s }).ToList();
                }
            }


            Entries entries = new Entries();
            List<int> ids = new List<int>();
            foreach (Transaction t in transactionList)
            {
                int id = t.EntryId.Value;
                string status = t.Status;
                string comment = t.AccountantComment;
                DateTime date = t.DateSubmitted.GetValueOrDefault();

                if (ids.Contains(id))
                    continue;
                else
                    ids.Add(id);

                Entry e = new Entry(id, status, date, comment);
                foreach (Transaction t2 in transactionList)
                {
                    if (t2.EntryId == id)
                    {
                        e.accountNames.Add(t2.AccountName);
                        e.debits.Add(t2.Debit.GetValueOrDefault());
                        e.credits.Add(t2.Credit.GetValueOrDefault());
                    }
                }
                entries.entries.Add(e);
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