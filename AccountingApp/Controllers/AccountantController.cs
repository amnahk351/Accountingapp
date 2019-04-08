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

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {


                for (int i = 1; i < transactions.Length; i++)
                {


                    string sql = $"Insert into dbo.TransactionTable (AccountantUsername, AccountantComment, " +
                    "DateSubmitted, Status, AccountName, Debit, Credit, EntryId)" +
                    "values(@AccountantUsername,@AccountantComment,@DateSubmitted,@Status,@AccountName," +
                    "@Debit,@Credit,@EntryId)";

                    db.Execute(sql, new
                    {
                        AccountantUsername = sessionUser,
                        AccountantComment = transactions[i].AccountantComment,
                        DateSubmitted = transactions[i].DateSubmitted,
                        Status = transactions[i].Status,
                        AccountName = transactions[i].AccountName,
                        Debit = transactions[i].Debit,
                        Credit = transactions[i].Credit,
                        EntryId = NewEntryId + 1
                    });

                    insertedRecords++;

                }
            }

            return Json(insertedRecords);
        }

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

    }

    //http://20fingers2brains.blogspot.com/2014/07/upload-multiple-files-to-database-using.html

    public class FileUploadService
    {
        public void SaveFileDetails(HttpPostedFileBase file)
        {
            //UploadedFiles newFile = new UploadedFiles();
            //newFile.ContentType = file.ContentType;
            //newFile.ImageBytes = ConvertToBytes(file);
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
    }
}
