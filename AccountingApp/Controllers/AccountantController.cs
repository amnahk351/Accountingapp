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
        [HttpGet]
        public ActionResult AccountantIndex(string status)
        {
            if (status == null || status =="")
                return View(getAllEntriesOfStatus("approved"));
            else
                return View(getAllEntriesOfStatus(status));
        }

        [HttpPost]
        public ActionResult Journalize(Transaction transaction)
        {
            Trace.WriteLine(transaction.Debit);
            Trace.WriteLine(transaction.AccountNumber);

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
            return View("~/Views/Accountant/AccountantIndex.cshtml");
        }


        public int GetLatestEntryId()
        {
            int EntryId;

            SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
            SqlCommand cmd = new SqlCommand($"SELECT TOP 1 EntryId FROM dbo.TransactionTable ORDER BY TransactionID DESC", con);
            
            con.Open();
            string s = cmd.ExecuteScalar().ToString();  //Stores the latest EntryId in the table

            con.Close();
            EntryId = Int32.Parse(s);

            return EntryId;
        }


        [HttpPost]
        public JsonResult InsertJournal(Transaction[] transactions)
        {
            int insertedRecords = 0;
            int NewEntryId = GetLatestEntryId();
            var sessionUser = Session["Username"] as string;
            EventLogHandler Logger = new EventLogHandler();

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                for (int i = 1; i < transactions.Length; i++)
                {
                    //var x = DateTime.Now;

                    //if was submitted today added the current time to the database

                    //if submitted on another day, default the time to 12


                    string sql = $"Insert into dbo.TransactionTable (AccountantUsername, AccountantComment, " +
                    "DateSubmitted, Status, AccountName, Debit, Credit, EntryId, Entry_Type)" +
                    "values(@AccountantUsername,@AccountantComment,@DateSubmitted,@Status,@AccountName," +
                    "@Debit,@Credit,@EntryId,@Entry_Type)";

                    db.Execute(sql, new
                    {
                        AccountantUsername = sessionUser,
                        AccountantComment = transactions[i].AccountantComment,
                        DateSubmitted = transactions[i].DateSubmitted,
                        Status = transactions[i].Status,
                        AccountName = transactions[i].AccountName,
                        Debit = transactions[i].Debit,
                        Credit = transactions[i].Credit,
                        EntryId = NewEntryId + 1,
                        Entry_Type = transactions[i].Entry_Type
                    });

                    insertedRecords++;
                    //Logger.LogJournalEntrySubmitted(sessionUser, NewEntryId.ToString());
                }
            }

            return Json(insertedRecords);
        }

        private static byte[] getBytes(string file)
        {
            using (SqlConnection cn = new SqlConnection(SqlAccess.GetConnectionString()))
            using (SqlCommand cm = cn.CreateCommand())
            {
                cm.CommandText = @"
            SELECT FileBytes
            FROM   dbo.DocumentsTable
            WHERE  FileName = @Name";
                cm.Parameters.AddWithValue("@Name", file);
                cn.Open();
                return cm.ExecuteScalar() as byte[];
            }
        }

        public FileResult Download(string file)
        {
            byte[] fileBytes = getBytes(file);
            var response = new FileContentResult(fileBytes, "application/octet-stream");
            response.FileDownloadName = file;
            return response;
        }

        [HttpPost]
        public ActionResult DeleteFile(string file) {

            int EntryID = GetLatestEntryId() + 1;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                string sql = $"DELETE FROM dbo.DocumentsTable WHERE FileName=@File AND FK_EntryId=@ID";
                
                db.Execute(sql, new
                {
                    File = file,
                    ID = EntryID
                });                

            }

            return Json("File, " + file + ", Deleted!");
        }

        //http://20fingers2brains.blogspot.com/2014/07/upload-multiple-files-to-database-using.html

        [HttpPost]
        public ActionResult UploadFiles()
        {
            //Database1Entities7 entities = new Database1Entities7();
            //var mostRecentEntryID = entities.Transactions.ToList().Select(eID => eID.EntryId).LastOrDefault() + 1;  //add a 1 to match folder with EntryId
            //string foldername = mostRecentEntryID.ToString();

            //string folder = Server.MapPath(string.Format("~/User_Uploads/{0}/", foldername));

            //if (!Directory.Exists(folder)) {
            //    Directory.CreateDirectory(folder);
            //}

            //string location = "~/User_Uploads/" + foldername + "/";

            //string path = Server.MapPath(location);

            //HttpFileCollectionBase files = Request.Files;
            //for (int i = 0; i < files.Count; i++)
            //{
            //    FileUploadService service = new FileUploadService();
            //    HttpPostedFileBase file = files[i];
            //    file.SaveAs(path + file.FileName);
            //}
            //return Json(files.Count + " Files Uploaded!");

            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++)
            {
                FileUploadService service = new FileUploadService();
                HttpPostedFileBase file = files[i];
                service.SaveFileDetails(file);
            }
            return Json(files.Count + " Files Uploaded!");

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

        public ActionResult ChartOfAccounts()
        {
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            return View(listAccounts);
            //var item = db.ChartOfAccs.ToList();
            //return View(item);


        }
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

                for (int i = 0; i < coa.Count; i++)
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



        public ActionResult RetainedEarnings()
        {
            return View();
        }

        public ActionResult PostClosingTrialBalance()
        {
            return View();
        }
    }

    //http://20fingers2brains.blogspot.com/2014/07/upload-multiple-files-to-database-using.html

    public class FileUploadService
    {
        public void SaveFileDetails(HttpPostedFileBase file)
        {
            int newID = GetLatestEntryId();
            //DocumentsTable newFile = new DocumentsTable();
            //UploadedFiles newFile = new UploadedFiles();
            //newFile.ContentType = file.ContentType;
            //newFile.FileBytes = ConvertToBytes(file);
            //newFile.FileName = file.FileName;
            //newFile.FK_EntryId = newID + 1;


            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.DocumentsTable (FileBytes, ContentType, " +
                    "FileName, FK_EntryId)" +
                    "values(@FileBytes,@ContentType,@FileName,@FK_EntryId)";
                db.Execute(sql, new
                {
                    FileBytes = ConvertToBytes(file),
                    ContentType = file.ContentType,
                    FileName = file.FileName,
                    FK_EntryId = newID + 1
                });
            }



            //using (FileUploadEntities dataContext = new FileUploadEntities())
            //{
            //    dataContext.UploadedFiles.AddObject(newFile);
            //    dataContext.SaveChanges();
            //}
        }

        public byte[] ConvertToBytes(HttpPostedFileBase file)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(file.InputStream);
            imageBytes = reader.ReadBytes((int)file.ContentLength);
            return imageBytes;
        }


        public int GetLatestEntryId()
        {
            int EntryId;

            SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
            SqlCommand cmd = new SqlCommand($"SELECT TOP 1 EntryId FROM dbo.TransactionTable ORDER BY TransactionID DESC", con);

            con.Open();
            string s = cmd.ExecuteScalar().ToString();  //Stores the latest EntryId in the table

            con.Close();
            EntryId = Int32.Parse(s);

            return EntryId;
        }
    }
}
