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

        public ActionResult GeneralJournal()
        {
            var allTransactions = db.Transactions.ToList();
            return View(allTransactions);
        }
    }
}