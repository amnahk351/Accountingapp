using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountingApp.Controllers
{
    public class ManagerController : Controller
    {
        private Database1Entities7 db = new Database1Entities7();
        private Database1Entities3 coaDB = new Database1Entities3();
        // GET: Manager
        public ActionResult ManagerIndex()
        {
            return View();
        }

        public ActionResult ManagerApproval()
        {
            var allPendingTransactions = db.Transactions.Where(t => t.Status == "pending").ToList();

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
                        e.debits.Add(t2.Debit.Value);
                        e.credits.Add(t2.Credit.Value);
                    }
                }
                entries.entries.Add(e);
            }

            return View(entries);
        }

        //public ActionResult ApproveEntry(int? id) {
        //    var allTransactionsWithEntryID = db.Transactions.Where(t => t.EntryId == id);
        //    return View(allTransactionsWithEntryID);
        //}

        public void ApproveEntry(int? id)
        {
            if (ModelState.IsValid)
            {
                var allTransactionsWithEntryID = db.Transactions.Where(t => t.EntryId == id);

                foreach (Transaction t in allTransactionsWithEntryID)
                {
                    t.Status = "approved";
                    Trace.WriteLine("----------" + t.EntryId + ":" + t.Status);
                    var coa = coaDB.ChartOfAccs.Where(a => a.AccountName == t.AccountName).FirstOrDefault();

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
                }
                coaDB.SaveChanges();
                db.SaveChanges();
            }
            else
                Trace.WriteLine("no");

            Response.Write("<script language=javascript>alert('journal updated'); window.location.href = '../ManagerApproval';</script>");
        }

        public ActionResult DisapproveEntry(int? id)
        {
            var allTransactionsWithEntryID = db.Transactions.Where(t => t.EntryId == id);

            foreach (Transaction t in allTransactionsWithEntryID)
            {
                t.Status = "disapproved";
            }
            db.SaveChanges();

            return View();
        }

        //public ActionResult DisapproveEntry(List<Transaction>)

        public ActionResult GeneralJournal()
        {
            var allTransactions = db.Transactions.ToList();
            return View(allTransactions);
        }

        public ActionResult TrialBalance()
        {
            var coa = coaDB.ChartOfAccs.ToList();
            return View(coa);
        }
    }
}