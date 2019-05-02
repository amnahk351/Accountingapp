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

        public decimal GetAccountBalance(string name)
        {
            decimal result;

            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountName=@Value", new { Value = name }).ToList();
            }

            result = (decimal)listAccounts[0].CurrentBalance;

            return result;
        }

        public ActionResult GeneralLedger(string name, string PostReference)
        {
            if (name == null && PostReference == null)
            {
                ViewBag.AccountName = "Account Name";
                ViewBag.AccountNumber = "Account No. ";
                ViewBag.AccountBalance = "Balance: ";

                List<ChartOfAcc> listAccounts2;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    listAccounts2 = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Order By AccountName").ToList();
                }
                List<SelectListItem> sliAccountList2 = new List<SelectListItem>();


                foreach (ChartOfAcc coa in listAccounts2)
                {
                    SelectListItem item = new SelectListItem
                    {
                        Text = coa.AccountName,
                        Value = coa.AccountNumber.ToString()
                    };
                    sliAccountList2.Add(item);
                }

                ViewBag.accountlist = sliAccountList2;

                return View("GeneralLedger");
            }

            if (name != null)
            {
                List<TransactionTable> transactionList;
                string s = "approved";

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where Status = @status And AccountName = @Name Order By DateReviewed", new { status = s, Name = name }).ToList();
                }

                ViewBag.AccountName = name;

                int number = GetAccountNameNumber(name);
                ViewBag.AccountNumber = "Account No. " + number.ToString();

                decimal balance = GetAccountBalance(name);
                ViewBag.AccountBalance = "Balance: " + balance.ToString();

                List<ChartOfAcc> listAccounts;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Order By AccountName").ToList();
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


                foreach (TransactionTable t in transactionList)
                {
                    t.DebitString = String.Format("{0:n}", t.Debit);
                    t.CreditString = String.Format("{0:n}", t.Credit);
                    t.BeforeString = String.Format("{0:n}", t.BeforeBalance);
                    t.AfterString = String.Format("{0:n}", t.AfterBalance);
                }

                ViewBag.accountlist = sliAccountList;

                return View(transactionList);
            }

            if (PostReference != null)
            {
                string s = "approved";

                string AccName = "";

                List<TransactionTable> transactionListName;

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    transactionListName = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where PostReference = @Num", new { Num = PostReference }).ToList();
                }

                AccName = transactionListName[0].AccountName;

                List<TransactionTable> transactionList;

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where Status = @status And AccountName = @Name Order By DateReviewed", new { status = s, Name = AccName }).ToList();
                }

                ViewBag.AccountName = AccName;
                int number = GetAccountNameNumber(AccName);

                ViewBag.AccountNumber = "Account No. " + number.ToString();

                decimal balance = GetAccountBalance(AccName);
                ViewBag.AccountBalance = "Balance: " + balance.ToString();

                List<ChartOfAcc> listAccounts;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Order By AccountName").ToList();
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


                foreach (TransactionTable t in transactionList)
                {
                    t.DebitString = String.Format("{0:n}", t.Debit);
                    t.CreditString = String.Format("{0:n}", t.Credit);
                    t.BeforeString = String.Format("{0:n}", t.BeforeBalance);
                    t.AfterString = String.Format("{0:n}", t.AfterBalance);
                }

                ViewBag.accountlist = sliAccountList;

                return View(transactionList);
            }

            return View("GeneralLedger");
        }


        public ActionResult EventLog()
        {
            string access = "All";
            List<Models.EventLog> events;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                events = db.Query<Models.EventLog>($"Select * From dbo.EventLogTable Where AccessLevel = @Acc", new { Acc = access }).ToList();
            }

            return View(events);
        }



        public int GetLatestEntryId()
        {
            int EntryId;

            SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
            SqlCommand cmd = new SqlCommand($"SELECT TOP 1 EntryId FROM dbo.TransactionTable ORDER BY TransactionID DESC", con);
            string s = "1000";
            con.Open();
            try
            {
                s = cmd.ExecuteScalar().ToString();  //Stores the latest EntryId in the table
            }
            catch
            {

            }
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

                string EventLogTo = GenerateEventLogTransactionDetail(NewEntryId+1);

                Logger.LogJournalEntrySubmitted(sessionUser, EventLogTo, NewEntryId.ToString(), type);

            }

            return Json(insertedRecords);
        }


        //public int ModalID = 1002;

        //[HttpPost]
        //public ActionResult UpdateModalID(int id)
        //{
        //    ModalID = id;            

        //    return Json("Value Updated To: " + ModalID);
        //}

        public ActionResult TransactionSummary(int id) {

            //int ModalID = 1000;


            //ModalID = id;

            //List<TransactionTable> transactionList;
            //Entry EnModel = new Entry();
            //List<String> Names = new List<String>();
            //List<Decimal> Debits = new List<Decimal>();
            //List<Decimal> Credits = new List<Decimal>();
            //List<DocumentsTable> files;
            //List<String> NamesofFiles = new List<String>();

            //using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            //{
            //    transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId = @ID", new { ID = id }).ToList();
            //}

            //EnModel.entryID = (int)transactionList[0].EntryId;
            //EnModel.status = transactionList[0].Status;
            //EnModel.comment = transactionList[0].AccountantComment;
            //EnModel.submitDate = (DateTime) transactionList[0].DateSubmitted;

            //for (int i = 0; i < transactionList.Count; i++) {
            //    Names.Add(transactionList[i].AccountName);
            //    Debits.Add((Decimal)transactionList[i].Debit);
            //    Credits.Add((Decimal)transactionList[i].Credit);
            //}

            //EnModel.accountNames = Names;
            //EnModel.debits = Debits;
            //EnModel.credits = Credits;

            //using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            //{
            //    files = db.Query<DocumentsTable>($"Select FileName From dbo.DocumentsTable Where FK_EntryId = @ID", new { ID = id }).ToList();
            //}

            //for (int i = 0; i < files.Count; i++)
            //{
            //    NamesofFiles.Add(files[i].FileName);
            //}

            //EnModel.fileNames = NamesofFiles;
            //System.Diagnostics.Debug.WriteLine("modalid " + ModalID);
            
            return View(GetEntrySummary(id));
        }


        public string GenerateEventLogTransactionDetail(int id) {
            var sessionUser = Session["Username"] as string;
            List<TransactionTable> transactionList;
            List<string> Entries = new List<string>();
            List<string> TotalList = new List<string>();
            string AllEntries = "";
            string Resultant = "";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId = @ID", new { ID = id }).ToList();
            }

            TotalList.Add(transactionList[0].EntryId.ToString());
            TotalList.Add("Status: " + transactionList[0].Status);
            TotalList.Add("Type: " + transactionList[0].Entry_Type);

            for (int i = 0; i < transactionList.Count; i++)
            {

                string EntryAccountName = transactionList[i].AccountName;
                decimal DebitAmount = transactionList[i].Debit.GetValueOrDefault();
                decimal CreditAmount = transactionList[i].Credit.GetValueOrDefault();

                string line = "";

                if (CreditAmount == 0)
                {
                    //This line is a debit
                    line = EntryAccountName + "-Debit-" + DebitAmount;
                    Entries.Add(line);
                }

                if (DebitAmount == 0)
                {
                    //This line is a credit
                    line = EntryAccountName + "-Credit-" + CreditAmount;
                    Entries.Add(line);
                }

            }

            AllEntries = String.Join(",", Entries);
            TotalList.Add(AllEntries);
            TotalList.Add("Comment: " + transactionList[0].AccountantComment);
            Resultant = String.Join("|^|", TotalList);

            return Resultant;
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


            string EventLogFrom = GenerateEventLogTransactionDetail(EditedEntryID);

            //delete old entries here
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                string sql = $"DELETE FROM dbo.TransactionTable WHERE EntryId=@ID";

                db.Execute(sql, new
                {
                    ID = EditedEntryID
                });

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

            }

            string EventLogTo = GenerateEventLogTransactionDetail(EditedEntryID);

            Logger.LogEditedJournalEntry(sessionUser, EventLogFrom, EventLogTo, EditedEntryID, type);

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
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where Active=@Value Order By AccountName", new { Value = t }).ToList();
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
        public ActionResult RetrieveAccountantAndComment(int id)
        {
            List<TransactionTable> transactionList;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {                
                transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId=@ID", new { ID = id }).ToList();
            }

            string accountant = transactionList[0].AccountantUsername;
            string comment = transactionList[0].AccountantComment;
            string split = "|^|";
            string res = accountant + split + comment;
            var result = JsonConvert.SerializeObject(res);

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
            string str2 = str.Replace("/Manager/EditJournal/", "");
            int x = Int32.Parse(str2);


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
            List<TransactionTable> transactionList;
            string s = "approved";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where Status = @status", new { status = s }).ToList();
            }

            foreach (TransactionTable t in transactionList) {

                t.DebitString = String.Format("{0:n}", t.Debit);
                t.CreditString = String.Format("{0:n}", t.Credit);
                t.BeforeString = String.Format("{0:n}", t.BeforeBalance);
                t.AfterString = String.Format("{0:n}", t.AfterBalance);
            }

            return View(transactionList);
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
                NewE.Entry_Type = t.Entry_Type;


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


        private Entry GetEntrySummary(int id)
        {
            System.Diagnostics.Debug.WriteLine("id passed " + id);

            List<TransactionTable> transactionList;
            List<DocumentsTable> fileList = new List<DocumentsTable>();
            

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                transactionList = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId = @ID", new { ID = id }).ToList();
            }

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {                
                fileList = db.Query<DocumentsTable>($"Select * From dbo.DocumentsTable Where FK_EntryId = @ID", new { ID = id }).ToList();
            }

            System.Diagnostics.Debug.WriteLine("status error " + transactionList[0].Status);

            Entry NewE = new Entry();
            NewE.entryID = id;
            NewE.status = transactionList[0].Status;
            NewE.DateSubmitted = transactionList[0].DateSubmitted.GetValueOrDefault();
            NewE.DateReviewed = transactionList[0].DateReviewed.GetValueOrDefault();
            NewE.AccountantComment = transactionList[0].AccountantComment;
            NewE.ManagerComment = transactionList[0].ManagerComment;
            NewE.AccountantUsername = transactionList[0].AccountantUsername;
            NewE.ManagerUsername = transactionList[0].ManagerUsername;
            NewE.PostReference = transactionList[0].PostReference.GetValueOrDefault();


            foreach (TransactionTable t in transactionList) {
                NewE.accountNames.Add(t.AccountName);
                NewE.debits.Add(t.Debit.GetValueOrDefault());
                NewE.credits.Add(t.Credit.GetValueOrDefault());

            }

            foreach (DocumentsTable f in fileList)
            {
                NewE.fileNames.Add(f.FileName);

            }

            return NewE;
        }



        public ActionResult ChartOfAccounts()
        {
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }

            foreach (ChartOfAcc c in listAccounts)
            {
                c.CurrentBalString = String.Format("{0:n}", c.CurrentBalance);
                c.OriginalBalString = String.Format("{0:n}", c.OriginalBalance);
            }

            return View(listAccounts);
        }


        public ActionResult CloseAccountsEntry()
        {
            //Trace.WriteLine("Accounts Totally Closed");
            var sessionUser = Session["Username"] as string;
            EventLogHandler Logger = new EventLogHandler();

            //List<ChartOfAcc> revenueExpense;
            TransactionTable lastTransaction;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                lastTransaction = db.Query<TransactionTable>($"Select * from dbo.TransactionTable").LastOrDefault();

                int num = (int) lastTransaction.EntryId + 1;
                Logger.LogAccountantCloseAccountRequest(sessionUser, num);

                var revenueExpense = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountType = @revenue OR AccountType = @expense", new { revenue = "Revenue", expense = "Expense" }).ToList();
                lastTransaction = db.Query<TransactionTable>($"Select * from dbo.TransactionTable").LastOrDefault();
                decimal totalRevenues = 0;
                decimal totalExpenses = 0;
                string sqlstatement = $"Insert into dbo.TransactionTable (AccountantUsername, AccountantComment, "
    + "DateSubmitted, Status, AccountName, Debit, Credit, EntryId, Entry_Type)"
    + "values(@AccountantUsername,@AccountantComment,@DateSubmitted,@Status,@AccountName," + "@Debit,@Credit,@EntryId,@Entry_Type)";
                foreach (ChartOfAcc coa in revenueExpense)
                {
                    decimal amount = coa.CurrentBalance.Value;

                    if (coa.NormalSide == "Debit")
                    {
                        totalExpenses = totalExpenses + amount;


                        db.Execute(sqlstatement, new
                        {
                            AccountantUsername = "Accountant",
                            AccountantComment = "Closing Accounts",
                            DateSubmitted = DateTime.Now,
                            Status = "pending",
                            AccountName = coa.AccountName,
                            Debit = 0,
                            Credit = amount,
                            EntryId = lastTransaction.EntryId + 1,
                            Entry_Type = "Closing"
                        });
                    }
                    else
                    {
                        totalRevenues = totalRevenues + amount;

                        db.Execute(sqlstatement, new
                        {
                            AccountantUsername = "Accountant",
                            AccountantComment = "Closing Accounts",
                            DateSubmitted = DateTime.Now,
                            Status = "pending",
                            AccountName = coa.AccountName,
                            Debit = amount,
                            Credit = 0,
                            EntryId = lastTransaction.EntryId + 1,
                            Entry_Type = "Closing"
                        });
                    }
                    //string EventLogTo = GenerateEventLogTransactionDetail(lastTransaction.EntryId + 1);

                    //Logger.LogJournalEntrySubmitted(sessionUser, EventLogTo, lastTransaction.EntryId.ToString(), type);

                }
                db.Execute(sqlstatement, new
                {
                    AccountantUsername = "Accountant",
                    AccountantComment = "Closing Accounts",
                    DateSubmitted = DateTime.Now,
                    Status = "pending",
                    AccountName = "Retained Earnings",
                    Debit = 0,
                    Credit = totalRevenues-totalExpenses,
                    EntryId = lastTransaction.EntryId + 1,
                    Entry_Type = "Closing"
                });
            }

            return Redirect("ChartOfAccounts");
        }

        //colt's code
        public ActionResult TrialBalance(DateTime? until)
        {
            decimal debTotal = 0;
            decimal credTotal = 0;
            if (until == null)
            {
                ViewBag.DisplayDate = DateTime.Now.ToString();
                List<ChartOfAcc> coa;
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
            else
            {
                List<ChartOfAcc> coaAtDate = new List<ChartOfAcc>();
                ViewBag.DisplayDate = until.ToString();

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    List<TransactionTable> transactionsAtDate = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where DateReviewed <= @date AND status = @status",
                        new { date = until, status = "approved" }).ToList();
                    coaAtDate = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();

                    for (int i = 0; i < coaAtDate.Count; i++)
                    {
                        coaAtDate[i].CurrentBalance = coaAtDate[i].OriginalBalance;
                        Trace.WriteLine("---Before: " + coaAtDate[i].AccountName + ": " + coaAtDate[i].CurrentBalance);
                        for (int j = 0; j < transactionsAtDate.Count; j++)
                        {
                            if (transactionsAtDate[j].AccountName == coaAtDate[i].AccountName)
                            {
                                if (transactionsAtDate[j].Debit == null)
                                    transactionsAtDate[j].Debit = 0;

                                if (transactionsAtDate[j].Credit == null)
                                    transactionsAtDate[j].Credit = 0;

                                if (coaAtDate[i].NormalSide.ToLower() == "debit")
                                {
                                    coaAtDate[i].CurrentBalance += transactionsAtDate[j].Debit.Value;
                                    coaAtDate[i].CurrentBalance -= transactionsAtDate[j].Credit.Value;

                                }
                                else //normal side is credit
                                {
                                    coaAtDate[i].CurrentBalance += transactionsAtDate[j].Credit.Value;
                                    coaAtDate[i].CurrentBalance -= transactionsAtDate[j].Debit.Value;

                                }

                            }
                        }
                        Trace.WriteLine("---After: " + coaAtDate[i].AccountName + ": " + coaAtDate[i].CurrentBalance);
                    }
                }

                credTotal = 0;
                debTotal = 0;
                foreach (ChartOfAcc c in coaAtDate)
                {
                    if (c.NormalSide.ToLower() == "debit")
                        debTotal += c.CurrentBalance.Value;
                    else
                        credTotal += c.CurrentBalance.Value;
                }

                ViewBag.DebitTotal = debTotal;
                ViewBag.CreditTotal = credTotal;

                return View(coaAtDate);
            }

        }

        public ActionResult IncomeStatement(DateTime? until)
        {

            List<ChartOfAcc> coa;
            decimal revenueTotal = 0;
            decimal expenseTotal = 0;
            if (until == null)
            {
                until = DateTime.Now;
                ViewBag.DisplayDate = until.ToString();

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
            else
            {
                ViewBag.DisplayDate = until.ToString();
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    List<TransactionTable> transactionsAtDate = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where DateReviewed <= @date AND status = @status",
                        new { date = until, status = "approved" }).ToList();
                    List<ChartOfAcc> coaAtDate = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active AND AccountType = @revenue OR AccountType = @expense", new { active = true, revenue = "Revenue", expense = "Expense" }).ToList();

                    for (int i = 0; i < coaAtDate.Count; i++)
                    {
                        coaAtDate[i].CurrentBalance = coaAtDate[i].OriginalBalance;
                        Trace.WriteLine("--------Before Current: " + coaAtDate[i].CurrentBalance);
                        Trace.WriteLine("--------Before Original: " + coaAtDate[i].OriginalBalance);
                        for (int j = 0; j < transactionsAtDate.Count; j++)
                        {
                            if (transactionsAtDate[j].AccountName == coaAtDate[i].AccountName)
                            {
                                if (transactionsAtDate[j].Debit == null)
                                    transactionsAtDate[j].Debit = 0;

                                if (transactionsAtDate[j].Credit == null)
                                    transactionsAtDate[j].Credit = 0;

                                if (coaAtDate[i].NormalSide.ToLower() == "debit")
                                {
                                    coaAtDate[i].CurrentBalance += transactionsAtDate[j].Debit.Value;
                                    coaAtDate[i].CurrentBalance -= transactionsAtDate[j].Credit.Value;

                                }
                                else //normal side is credit
                                {
                                    coaAtDate[i].CurrentBalance += transactionsAtDate[j].Credit.Value;
                                    coaAtDate[i].CurrentBalance -= transactionsAtDate[j].Debit.Value;

                                }

                            }
                        }
                    }
                    revenueTotal = 0;
                    expenseTotal = 0;
                    
                    foreach (ChartOfAcc c in coaAtDate)
                    {
                        if (c.AccountType.ToLower() == "revenue")
                            revenueTotal += c.CurrentBalance.Value;
                        if (c.AccountType.ToLower() == "expense")
                            expenseTotal += c.CurrentBalance.Value;
                    }
                    ViewBag.RevenueTotal = revenueTotal;
                    ViewBag.ExpenseTotal = expenseTotal;
                    ViewBag.NetIncome_Loss = revenueTotal - expenseTotal;

                    return View(coaAtDate);
                }
            }
        }

        public ActionResult BalanceSheet(DateTime? until)
        {
            List<ChartOfAcc> coa;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();


                if (until == null)
                {
                    until = DateTime.Now;
                    ViewBag.DisplayDate = until.ToString();

                }
                else
                {
                    ViewBag.DisplayDate = until.ToString();

                    List<TransactionTable> transactionsAtDate = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where DateReviewed <= @date AND status = @status",
                            new { date = until, status = "approved" }).ToList();
                    for (int i = 0; i < coa.Count; i++)
                    {
                        coa[i].CurrentBalance = coa[i].OriginalBalance;
                        for (int j = 0; j < transactionsAtDate.Count; j++)
                        {
                            if (transactionsAtDate[j].AccountName == coa[i].AccountName)
                            {
                                if (transactionsAtDate[j].Debit == null)
                                    transactionsAtDate[j].Debit = 0;

                                if (transactionsAtDate[j].Credit == null)
                                    transactionsAtDate[j].Credit = 0;

                                if (coa[i].NormalSide.ToLower() == "debit")
                                {
                                    coa[i].CurrentBalance += transactionsAtDate[j].Debit.Value;
                                    coa[i].CurrentBalance -= transactionsAtDate[j].Credit.Value;

                                }
                                else //normal side is credit
                                {
                                    coa[i].CurrentBalance += transactionsAtDate[j].Credit.Value;
                                    coa[i].CurrentBalance -= transactionsAtDate[j].Debit.Value;

                                }

                            }
                        }
                    }
                }

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
            }
            return View(coa);
        }
        //colt's code


        public ActionResult RetainedEarnings(DateTime? until)
        {
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                List<ChartOfAcc> coa = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where Active = @active", new { active = true }).ToList();

                if (until == null)
                {
                    until = DateTime.Now;
                    ViewBag.DisplayDate = until.ToString();
                }
                else
                {
                    ViewBag.DisplayDate = until.ToString();
                    List<TransactionTable> transactionsAtDate = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where DateReviewed <= @date AND status = @status",
                            new { date = until, status = "approved" }).ToList();
                    for (int i = 0; i < coa.Count; i++)
                    {
                        coa[i].CurrentBalance = coa[i].OriginalBalance;
                        for (int j = 0; j < transactionsAtDate.Count; j++)
                        {
                            if (transactionsAtDate[j].AccountName == coa[i].AccountName)
                            {
                                if (transactionsAtDate[j].Debit == null)
                                    transactionsAtDate[j].Debit = 0;

                                if (transactionsAtDate[j].Credit == null)
                                    transactionsAtDate[j].Credit = 0;

                                if (coa[i].NormalSide.ToLower() == "debit")
                                {
                                    coa[i].CurrentBalance += transactionsAtDate[j].Debit.Value;
                                    coa[i].CurrentBalance -= transactionsAtDate[j].Credit.Value;

                                }
                                else //normal side is credit
                                {
                                    coa[i].CurrentBalance += transactionsAtDate[j].Credit.Value;
                                    coa[i].CurrentBalance -= transactionsAtDate[j].Debit.Value;

                                }

                            }
                        }
                    }
                }


                decimal revenueTotal = 0;
                decimal expenseTotal = 0;
                decimal earnings = 0;
                decimal divedends = 0;



                foreach (ChartOfAcc c in coa)
                {
                    if (c.AccountName == "Retained Earnings")
                        earnings = c.CurrentBalance.Value;
                    if (c.AccountName == "Divadends")
                        divedends = c.CurrentBalance.Value;
                    if (c.AccountType.ToLower() == "revenue")
                        revenueTotal += c.CurrentBalance.Value;
                    if (c.AccountType.ToLower() == "expense")
                        expenseTotal += c.CurrentBalance.Value;


                }

                ViewBag.RetainedEarnings = earnings;
                ViewBag.NetIncome = revenueTotal - expenseTotal;
                ViewBag.EarningsPlusIncome = earnings + revenueTotal - expenseTotal;
                ViewBag.Dividends = divedends;
                ViewBag.Total = earnings + revenueTotal - expenseTotal - divedends;
            }
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
