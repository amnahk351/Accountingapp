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
        // GET: Accountant
        //public ActionResult AccountantIndex()
        //{
        //    return View();
        //}
        public ActionResult AccountantIndex()
        {
           

            return View(getAllEntriesOfStatus("approved"));
        }

        [HttpPost]
        public ActionResult Journalize(Transaction transaction)
        {
            //check model
            //are both d/c 0? do all the d == c?
            //most recent entry id = SELECT TOP 1 * FROM Table ORDER BY ID DESC .entryID
            //foreach transaction entryID = most recent ++

            Trace.WriteLine(transaction.Debit);
            Trace.WriteLine(transaction.AccountNumber);


            //Database1Entities3 db = new Database1Entities3();
            //List<ChartOfAcc> getaccountslist = db.ChartOfAccs.ToList();
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            List<SelectListItem> sliAccountList = new List<SelectListItem>();


            //IEnumerable<ChartOfAcc> accounts = new List<ChartOfAcc> { new ChartOfAcc { AccountNumber = 1234, AccountName = "Test" } };

            //foreach (ChartOfAcc coa in getaccountslist)
            //{
            //    SelectListItem item = new SelectListItem
            //    {
            //        Text = coa.AccountName,
            //        Value = coa.AccountNumber.ToString()
            //    };
            //    sliAccountList.Add(item);
            //}

            foreach (ChartOfAcc coa in listAccounts)
            {
                SelectListItem item = new SelectListItem
                {
                    Text = coa.AccountName,
                    Value = coa.AccountNumber.ToString()
                };
                sliAccountList.Add(item);
            }

            //SelectList list = new SelectList(sliAccountList, "Value", "Text");
            ViewBag.accountlist = sliAccountList;
            //foreach (Transaction transaction in transactions)
            //{
            //    Trace.WriteLine(transaction.Debit);
            //}
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



            //using (Database1Entities7 entities = new Database1Entities7())
            //{
            //    if (transactions == null)
            //    {
            //        throw new Exception("NO TRANSACTIONS");
            //    }

            //    var mostRecentEntryID = entities.Transactions.ToList().Select(eID => eID.EntryId).LastOrDefault();

            //    var coaDB = new Database1Entities3();
            //    int insertedRecords = 0;

            //    for (int i = 1; i < transactions.Length; i++)
            //    {                   

            //        var AccName = transactions[i].AccountName;
            //        var coa = coaDB.ChartOfAccs.Where(x => x.AccountName == AccName).FirstOrDefault();

            //        if (coa == null)
            //            Trace.WriteLine("Could not find COA");

            //        if (transactions[i].Debit == null)
            //            transactions[i].Debit = 0;

            //        if (transactions[i].Credit == null)
            //            transactions[i].Credit = 0;

            //        if (coa.NormalSide.ToLower() == "debit")
            //        {
            //            coa.CurrentBalance += transactions[i].Debit.Value;
            //            coa.CurrentBalance -= transactions[i].Credit.Value;
            //        }
            //        else //normal side is credit
            //        {
            //            coa.CurrentBalance += transactions[i].Credit.Value;
            //            coa.CurrentBalance -= transactions[i].Debit.Value;
            //        }
            //        coaDB.SaveChanges();

            //        Transaction tran = new Transaction();
            //        tran.DateSubmitted = transactions[i].DateSubmitted;
            //        tran.AccountName = transactions[i].AccountName;
            //        tran.AccountNumber = GetAccountNumber(AccName);
            //        tran.Debit = transactions[i].Debit;
            //        tran.Credit = transactions[i].Credit;

            //        if (i == 1) {
            //            tran.Comment = transactions[i].Comment;
            //        }

            //        tran.EntryId = mostRecentEntryID + 1;

            //        //tran.Status = "pending";
            //        tran.Status = transactions[i].Status;
            //        entities.Transactions.Add(tran);

            //        //transactions[i].AccountNumber = GetAccountNumber(AccName);
            //        //transactions[i].EntryId = mostRecentEntryID + 1;
            //        //transactions[i].Status = "pending";
            //        //entities.Transactions.Add(transactions[i]);  //this line adds everything that is already filled like debit/credit and account
            //        entities.SaveChanges();
            //        insertedRecords++;
            //    }

            //    //Transaction tran = new Transaction();


            //    //for(int i = 1; i < transactions.Length; i++)
            //    //{
            //    //    tran.DateSubmitted = transactions[i].DateSubmitted;
            //    //    tran.AccountNumber = transactions[i].AccountNumber;
            //    //    tran.Debit = transactions[i].Debit;
            //    //    tran.Credit = transactions[i].Credit;

            //    //    entities.Transactions.Add(tran);
            //    //    entities.SaveChanges();
            //    //    insertedRecords++;
            //    //}
            //    return Json(insertedRecords);
            //}

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
                DateTime date = t.DateSubmitted.GetValueOrDefault();

                if (ids.Contains(id))
                    continue;
                else
                    ids.Add(id);

                Entry e = new Entry(id, status, date);
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
