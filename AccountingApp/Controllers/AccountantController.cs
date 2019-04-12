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

        public ActionResult EditJournal(double id)
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


        [HttpGet]
        public JsonResult GetAllTransactions(int id)
        {
            List<TransactionTable> transactions = new List<TransactionTable>();

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                transactions = db.Query<TransactionTable>($"Select * From dbo.TransactionTable Where EntryId = @ID", new { ID = id }).ToList();
            }
                        
            var result = JsonConvert.SerializeObject(transactions);
            System.Diagnostics.Debug.WriteLine("json: " + result);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllFiles(int id)
        {
            List<DocumentsTable> files = new List<DocumentsTable>();

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                files = db.Query<DocumentsTable>($"Select FileName From dbo.DocumentsTable Where FK_EntryId = @ID", new { ID = id }).ToList();
            }

            var result = JsonConvert.SerializeObject(files);
            System.Diagnostics.Debug.WriteLine("json: " + result);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

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

            string type = "";

            if (transactions[0].Status == "pending")
            {
                type = "Submitted";
            }
            else {
                type = "Suspended";
            }

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                for (int i = 1; i < transactions.Length; i++)
                {
                    //if was submitted today added the current time to the database

                    //if submitted on another day, default the time to 12

                    if (i == 1)
                    {
                        var DatetoUse = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine("Date Now: " + DatetoUse);
                        System.Diagnostics.Debug.WriteLine("Date: " + transactions[i].DateSubmitted);

                        var TodayString = DatetoUse.ToString();
                        string[] Pieces = TodayString.Split(' ');
                        string JustDate = Pieces[0];


                        string SavedDate = transactions[i].DateSubmitted.ToString();
                        string[] Pieces2 = SavedDate.Split(' ');
                        string JustDate2 = Pieces2[0];

                        if (JustDate != JustDate2)
                        {
                            System.Diagnostics.Debug.WriteLine("Other date: " + (DateTime)transactions[i].DateSubmitted);
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
        public JsonResult GetLatestEntryIdforFile() {

            int EntryId;

            SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
            SqlCommand cmd = new SqlCommand($"SELECT TOP 1 EntryId FROM dbo.TransactionTable ORDER BY TransactionID DESC", con);

            con.Open();
            string s = cmd.ExecuteScalar().ToString();  //Stores the latest EntryId in the table

            con.Close();
            EntryId = Int32.Parse(s);

            
            var result = JsonConvert.SerializeObject(EntryId + 1);
            System.Diagnostics.Debug.WriteLine("json latest entry id: " + result);

            return Json(result, JsonRequestBehavior.AllowGet);
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
            List<DocumentsTable> fileList = new List<DocumentsTable>();

            

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


            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                fileList = db.Query<DocumentsTable>($"Select * From dbo.DocumentsTable").ToList();
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
                        for (int i = 0; i < fileList.Count; i++)
                        {

                            if (fileList[i].FK_EntryId == t2.EntryId) {
                                e.files.Add(fileList[i]);
                            }
                        }
                        

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
