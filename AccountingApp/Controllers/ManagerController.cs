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

namespace AccountingApp.Controllers
{
    public class ManagerController : Controller
    {
        public ActionResult ManagerIndex()
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

        [HttpGet]
        public ActionResult GeneralJournal(string status)
        {
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
            decimal revenueTotal = 0;
            decimal expenseTotal = 0;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();
                //foreach (ChartOfAcc c in coa)
                //{
                //    if (c.AccountType.ToLower() == "revenue")
                //        revenueTotal += c.CurrentBalance.Value;
                //    if (c.AccountType.ToLower() == "expense")
                //        expenseTotal += c.CurrentBalance.Value;


                //}

                //ViewBag.RevenueTotal = revenueTotal;
                //ViewBag.ExpenseTotal = expenseTotal;
                //ViewBag.NetIncome_Loss = revenueTotal - expenseTotal;
            }

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
                DateTime date = t.DateSubmitted.GetValueOrDefault();
                string comment = t.AccountantComment;

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
    }
}