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
using Newtonsoft.Json;

namespace AccountingApp.Controllers
{
    public class AccountantController : Controller
    {
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult EditJournal(double id)
        {
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


            return View();
        }


        [HttpGet]
        public JsonResult GetAllTransactions(int id)
        {
            List<TransactionTable> transactions = new List<TransactionTable>();

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                transactions = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId = @ID", new { ID = id }).ToList();
            }

            var result = JsonConvert.SerializeObject(transactions);
            //System.Diagnostics.Debug.WriteLine("json: " + result);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllFiles(int id)
        {
            List<DocumentsTable> files = new List<DocumentsTable>();
            List<String> names = new List<String>();

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                files = db.Query<DocumentsTable>($"Select FileName From dbo.DocumentsTable Where FK_EntryId = @ID", new { ID = id }).ToList();
            }

            for (int i = 0; i < files.Count; i++) {
                names.Add(files[i].FileName);
            }

            var result = JsonConvert.SerializeObject(names);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AccountantIndex(string status)
        {
            if (status == null || status == "")
                return View(getAllEntriesOfStatus("approved"));
            else
                return View(getAllEntriesOfStatus(status));
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
        public JsonResult InsertJournal(TransactionTable[] transactions)
        {
            int insertedRecords = 0;
            int NewEntryId = GetLatestEntryId();
            var sessionUser = Session["Username"] as string;
            EventLogHandler Logger = new EventLogHandler();

            string type = "";

            if (transactions[0].Status == "pending")
            {
                type = "Submitted";
            }
            else
            {
                type = "Suspended";
            }

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                for (int i = 0; i < transactions.Length; i++)
                {
                    //if was submitted today added the current time to the database
                    //if submitted on another day, default the time to 12
                    if (i == 0)
                    {
                        var DatetoUse = DateTime.Now;

                        var TodayString = DatetoUse.ToString();
                        string[] Pieces = TodayString.Split(' ');
                        string JustDate = Pieces[0];


                        string SavedDate = transactions[i].DateSubmitted.ToString();
                        string[] Pieces2 = SavedDate.Split(' ');
                        string JustDate2 = Pieces2[0];

                        if (JustDate != JustDate2)
                        {
                            DatetoUse = (DateTime)transactions[i].DateSubmitted;
                        }

                        string sql = $"Insert into dbo.TransactionTable (AccountantUsername, AccountantComment, " +
                        "DateSubmitted, Status, AccountName, Debit, Credit, EntryId, Entry_Type)" +
                        "values(@AccountantUsername,@AccountantComment,@DateSubmitted,@Status,@AccountName," +
                        "@Debit,@Credit,@EntryId,@Entry_Type)";

                        db.Execute(sql, new
                        {
                            AccountantUsername = sessionUser,
                            AccountantComment = transactions[i].AccountantComment,
                            DateSubmitted = DatetoUse,
                            Status = transactions[i].Status,
                            AccountName = transactions[i].AccountName,
                            Debit = transactions[i].Debit,
                            Credit = transactions[i].Credit,
                            EntryId = NewEntryId + 1,
                            Entry_Type = transactions[i].Entry_Type
                        });

                        insertedRecords++;

                    }
                    else
                    {
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

                    }
                }

                Logger.LogJournalEntrySubmitted(sessionUser, NewEntryId.ToString(), type);

            }

            return Json(insertedRecords);
        }

        public ActionResult TransactionSummary(int id) {

            List<TransactionTable> transactionList;
            Entry EnModel = new Entry();
            List<String> Names = new List<String>();
            List<Decimal> Debits = new List<Decimal>();
            List<Decimal> Credits = new List<Decimal>();
            List<DocumentsTable> files;
            List<String> NamesofFiles = new List<String>();

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId = @ID", new { ID = id }).ToList();
            }

            EnModel.entryID = (int)transactionList[0].EntryId;
            EnModel.status = transactionList[0].Status;
            EnModel.comment = transactionList[0].AccountantComment;
            EnModel.submitDate = (DateTime) transactionList[0].DateSubmitted;
            
            for (int i = 0; i < transactionList.Count; i++) {
                Names.Add(transactionList[i].AccountName);
                Debits.Add((Decimal)transactionList[i].Debit);
                Credits.Add((Decimal)transactionList[i].Credit);
            }

            EnModel.accountNames = Names;
            EnModel.debits = Debits;
            EnModel.credits = Credits;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                files = db.Query<DocumentsTable>($"Select FileName From dbo.DocumentsTable Where FK_EntryId = @ID", new { ID = id }).ToList();
            }

            for (int i = 0; i < files.Count; i++)
            {
                NamesofFiles.Add(files[i].FileName);
            }

            EnModel.fileNames = NamesofFiles;

            return View(EnModel);
        }


        [HttpPost]
        public JsonResult InsertEditedJournal(TransactionTable[] transactions, string id)
        {
            int insertedRecords = 0;
            int EditedEntryID = Int32.Parse(id);
            var sessionUser = Session["Username"] as string;
            EventLogHandler Logger = new EventLogHandler();
            
            string type = "";

            if (transactions[0].Status == "pending")
            {
                type = "Submitted";
            }
            else
            {
                type = "Suspended";
            }

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                for (int i = 0; i < transactions.Length; i++)
                {
                    //if was submitted today added the current time to the database

                    //if submitted on another day, default the time to 12

                    if (i == 0)
                    {
                        var DatetoUse = DateTime.Now;

                        var TodayString = DatetoUse.ToString();
                        string[] Pieces = TodayString.Split(' ');
                        string JustDate = Pieces[0];


                        string SavedDate = transactions[i].DateSubmitted.ToString();
                        string[] Pieces2 = SavedDate.Split(' ');
                        string JustDate2 = Pieces2[0];

                        if (JustDate != JustDate2)
                        {
                            DatetoUse = (DateTime)transactions[i].DateSubmitted;
                        }

                        string sql = $"Insert into dbo.TransactionTable (AccountantUsername, AccountantComment, " +
                        "DateSubmitted, Status, AccountName, Debit, Credit, EntryId, Entry_Type)" +
                        "values(@AccountantUsername,@AccountantComment,@DateSubmitted,@Status,@AccountName," +
                        "@Debit,@Credit,@EntryId,@Entry_Type)";

                        db.Execute(sql, new
                        {
                            AccountantUsername = sessionUser,
                            AccountantComment = transactions[i].AccountantComment,
                            DateSubmitted = DatetoUse,
                            Status = transactions[i].Status,
                            AccountName = transactions[i].AccountName,
                            Debit = transactions[i].Debit,
                            Credit = transactions[i].Credit,
                            EntryId = EditedEntryID,
                            Entry_Type = transactions[i].Entry_Type
                        });

                        insertedRecords++;

                    }
                    else
                    {
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
                            EntryId = EditedEntryID,
                            Entry_Type = transactions[i].Entry_Type
                        });

                        insertedRecords++;

                    }
                }

                Logger.LogEditedJournalEntry(sessionUser, EditedEntryID.ToString(), type);

            }

            return Json(insertedRecords);
        }


        private static byte[] getBytes(string file, int id)
        {
            using (SqlConnection cn = new SqlConnection(SqlAccess.GetConnectionString()))
            using (SqlCommand cm = cn.CreateCommand())
            {
                cm.CommandText = @"
            SELECT FileBytes
            FROM   dbo.DocumentsTable
            WHERE  FileName = @Name AND FK_EntryId = @ID";
                cm.Parameters.AddWithValue("@Name", file);
                cm.Parameters.AddWithValue("@ID", id);
                cn.Open();
                return cm.ExecuteScalar() as byte[];
            }
        }

        public FileResult Download(string file, int id)
        {
            byte[] fileBytes = getBytes(file, id);
            var response = new FileContentResult(fileBytes, "application/octet-stream");
            response.FileDownloadName = file;
            return response;
        }

        [HttpGet]
        public JsonResult GetLatestEntryIdforFile()
        {

            int EntryId;

            SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
            SqlCommand cmd = new SqlCommand($"SELECT TOP 1 EntryId FROM dbo.TransactionTable ORDER BY TransactionID DESC", con);

            con.Open();
            string s = cmd.ExecuteScalar().ToString();  //Stores the latest EntryId in the table

            con.Close();
            EntryId = Int32.Parse(s);


            var result = JsonConvert.SerializeObject(EntryId + 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RequestAccount(int number, string name, string type, string side, string balance, string comment)
        {
            var sessionUser = Session["Username"] as string;
            string s = "Request";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.UserRequestsTable (DateCreated, AccountNumber, AccountName, " +
                    "AccountType, NormalSide, OriginalBalance, Description, CreatedBy, Type)" +
                    "values(@Date, @AccountNumber, @AccountName, @AccountType, @NormalSide, @OriginalBalance," +
                    "@Description,@CreatedBy,@Type)";
                db.Execute(sql, new
                {
                    Date = DateTime.Now,
                    AccountNumber = number,
                    AccountName = name,
                    AccountType = type,
                    NormalSide = side,
                    OriginalBalance = balance,
                    Description = comment,
                    CreatedBy = sessionUser,
                    Type = s
                });
            }

            return Json("Account Request Submitted.");
        }

        [HttpGet]
        public JsonResult GetAllAccountNames()
        {
            List<ChartOfAcc> listAccounts;
            List<String> names = new List<String>();

            bool t = true;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where Active=@Value", new { Value = t }).ToList();
            }
            

            for (int i = 0; i < listAccounts.Count; i++)
            {
                names.Add(listAccounts[i].AccountName);
            }

            var result = JsonConvert.SerializeObject(names);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public int GetAccountNameNumber(string name) {
            int result;

            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountName=@Value", new { Value = name }).ToList();
            }

            result = listAccounts[0].AccountNumber;

            return result;
        }

        [HttpPost]
        public ActionResult RequestAccountUpdate(string name, string comment)
        {
            var sessionUser = Session["Username"] as string;
            string s = "Update";
            int AccNum = GetAccountNameNumber(name);


            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.UserRequestsTable (DateCreated, AccountNumber, AccountName, " +
                    "Description, CreatedBy, Type)" +
                    "values(@Date, @AccountNumber, @AccountName," +
                    "@Description,@CreatedBy,@Type)";
                db.Execute(sql, new
                {
                    Date = DateTime.Now,
                    AccountNumber = AccNum,
                    AccountName = name,                    
                    Description = comment,
                    CreatedBy = sessionUser,
                    Type = s
                });
            }

            return Json("Update Request Submitted.");
        }

        [HttpGet]
        public ActionResult RetreiveComment(int id)
        {
            List<TransactionTable> transactionList;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {                
                transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId=@ID", new { ID = id }).ToList();
            }

            string comment = transactionList[0].AccountantComment;
            var result = JsonConvert.SerializeObject(comment);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteFile(string file, int id)
        {

            //int EntryID = GetLatestEntryId() + 1;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                string sql = $"DELETE FROM dbo.DocumentsTable WHERE FileName=@File AND FK_EntryId=@ID";

                db.Execute(sql, new
                {
                    File = file,
                    ID = id
                });

            }

            return Json("File, " + file + ", Deleted!");
        }

        [HttpPost]
        public ActionResult DeleteEntriesforEdit(string id)
        {
            int x = Int32.Parse(id);
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                string sql = $"DELETE FROM dbo.TransactionTable WHERE EntryId=@ID";

                db.Execute(sql, new
                {
                    ID = x
                });

            }

            return Json("Entries Deleted!");
        }

        //http://20fingers2brains.blogspot.com/2014/07/upload-multiple-files-to-database-using.html

        [HttpPost]
        public ActionResult UploadFiles()
        {
            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++)
            {
                FileUploadService service = new FileUploadService();
                HttpPostedFileBase file = files[i];
                service.SaveFileDetails(file);
            }
            return Json(files.Count + " Files Uploaded!");

        }

        [HttpPost]
        public ActionResult UploadEditedFiles(string id)
        {
            string str = id.Replace("/Accountant/EditJournal/", "");
            int x = Int32.Parse(str);


            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++)
            {
                FileUploadService service = new FileUploadService();
                HttpPostedFileBase file = files[i];
                service.SaveEditedFileDetails(file, x);
            }
            return Json(files.Count + " Files Uploaded!");

        }

        public ActionResult Journalize()
        {
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


            return View();
        }

        public ActionResult PendingTransactions() {

            return View(getAllEntriesOfStatus("pending"));
        }

        public ActionResult DisapprovedTransactions()
        {

            return View(getAllEntriesOfStatus("disapproved"));
        }

        private Entries getAllEntriesOfStatus(string s)
        {

            List<TransactionTable> transactionList;
            List<DocumentsTable> fileList = new List<DocumentsTable>();


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

               

        public ActionResult ChartOfAccounts()
        {
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            return View(listAccounts);
        }


        //broderick's
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

        //colt's code
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
        //colt's code


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

        }

        public void SaveEditedFileDetails(HttpPostedFileBase file, int id)
        {
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
                    FK_EntryId = id
                });
            }

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
