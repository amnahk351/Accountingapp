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
        //private Database1Entities7 db = new Database1Entities7();
        //private Database1Entities3 coaDB = new Database1Entities3();
        // GET: Manager
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

        //public ActionResult DisapproveEntry(List<Transaction>)

        public ActionResult GeneralJournal()
        {

            List<Transaction> allTransactions;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                allTransactions = db.Query<Transaction>($"Select * From dbo.TransactionTable").ToList();
            }
            return View(allTransactions);
        }

        public ActionResult TrialBalance()
        {

            List<ChartOfAcc> coa;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts").ToList();
            }

            return View(coa);
        }

        private Entries getAllEntriesOfStatus(string s)
        {

            List<Transaction> allPendingTransactions;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                allPendingTransactions = db.Query<Transaction>($"Select * From dbo.TransactionTable Where Status = @status", new { status = s }).ToList();
            }


            Entries entries = new Entries();
            List<int> ids = new List<int>();
            foreach (Transaction t in allPendingTransactions)
            {
                int id = t.EntryId.Value;
                string status = t.Status;

                if (ids.Contains(id))
                    continue;
                else
                    ids.Add(id);

                Entry e = new Entry(id, status);
                foreach (Transaction t2 in allPendingTransactions)
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