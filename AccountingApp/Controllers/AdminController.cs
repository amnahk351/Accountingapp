using AccountingApp.Models;
using AccountingApp.DBAccess;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace AccountingApp.Controllers
{
    public class AdminController : Controller
    {
        Database1Entities3 db = new Database1Entities3();
        // GET: Admin
        public ActionResult AdminIndex()
        {
            return View();
        }

        public ActionResult Dashboard(string status)
        {
            return View();
        }

        public ActionResult NewUser()
        {
            NewUserModel user = new NewUserModel();
            user.Date_Created = DateTime.Now;
            return View(user);
        }

        [HttpPost]
        public ActionResult NewUser(NewUserModel model)
        {
            EventLogHandler Logger = new EventLogHandler();

            CreateUser tbl = new CreateUser();

            tbl.FirstName = model.FirstName;
            tbl.LastName = model.LastName;
            tbl.Username = model.Username;
            tbl.Password = model.Password;
            tbl.Role = model.Role;
            tbl.Phone = model.Phone;
            tbl.Email = model.Email;
            tbl.Date = model.Date_Created;
            tbl.Active = model.Active;
            tbl.Address = model.Address;
            tbl.City = model.City;
            tbl.State = model.State;
            tbl.ZIP_Code = model.ZIP_Code;
            tbl.AccountLocked = false;
            tbl.LoginAttempts = 10;
            tbl.LoginAmount = 0;
            tbl.LoginFails = 0;


            if (ModelState.IsValid)
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = $"Insert into dbo.UserTable (FirstName, LastName, " +
                        "Username, Password, Role, Phone, Email, Date, Active, Address, City, State, ZIP_Code," +
                        "AccountLocked, LoginAttempts, LoginAmount, LoginFails)" +
                        "values(@FirstName,@LastName,@Username,@Password,@Role," +
                        "@Phone,@Email,@Date,@Active,@Address,@City,@State,@ZIP_Code," +
                        "@AccountLocked, @LoginAttempts, @LoginAmount, @LoginFails)";
                    db.Execute(sql, new
                    {
                        FirstName = tbl.FirstName,
                        LastName = tbl.LastName,
                        Username = tbl.Username,
                        Password = tbl.Password,
                        Role = tbl.Role,
                        Phone = tbl.Phone,
                        Email = tbl.Email,
                        Date = tbl.Date,
                        Active = tbl.Active,
                        Address = tbl.Address,
                        City = tbl.City,
                        State = tbl.State,
                        ZIP_Code = tbl.ZIP_Code,
                        AccountLocked = tbl.AccountLocked,
                        LoginAttempts = tbl.LoginAttempts,
                        LoginAmount = tbl.LoginAmount,
                        LoginFails = tbl.LoginFails
                    });
                }
                TempData["Message"] = "Your entry was successfully added!";

                return RedirectToAction("AllUsers");
            }
            return View("NewUser", new NewUserModel());
        }


        public ActionResult EditUser(int id)
        {
            List<CreateUser> editUser;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                editUser = db.Query<CreateUser>($"Select * from dbo.Usertable Where ID = @ID", new { ID = id }).ToList();
            }

            EditUserModel EditView = new EditUserModel();
            EditView.Date_Modified = DateTime.Now;
            EditView.FirstName = editUser[0].FirstName;
            EditView.LastName = editUser[0].LastName;
            EditView.Email = editUser[0].Email;
            EditView.Username = editUser[0].Username;
            EditView.Role = editUser[0].Role;
            EditView.Phone = editUser[0].Phone;
            EditView.Active = editUser[0].Active;
            EditView.Address = editUser[0].Address;
            EditView.City = editUser[0].City;
            EditView.State = editUser[0].State;
            EditView.ZIP_Code = editUser[0].ZIP_Code;

            return View(EditView);
        }

        public ActionResult AllUsers()
        {
            List<CreateUser> listUser;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listUser = db.Query<CreateUser>($"Select * from dbo.Usertable").ToList();
            }

            return View(listUser);
        }

        [HttpPost]
        public ActionResult EditUser(EditUserModel value)
        {
            EventLogHandler Logger = new EventLogHandler();
            List<CreateUser> CurrentUser;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                CurrentUser = db.Query<CreateUser>($"Select * from dbo.Usertable Where ID = @ID", new { ID = value.ID }).ToList();
            }


            string CurrentPassword = CurrentUser[0].Password.ToString();
            int id = CurrentUser[0].ID;

            var Original = new List<string>();
            var Updated = new List<string>();

            string OriginalModel = "";
            string UpdatedModel = "";

            if (CurrentUser[0].DateModified != value.Date_Modified)
            {
                Original.Add("Date Modified: " + CurrentUser[0].DateModified);
                Updated.Add("Date Modified: " + value.Date_Modified);
            }

            if (CurrentUser[0].FirstName != value.FirstName)
            {
                Original.Add("First Name: " + CurrentUser[0].FirstName);
                Updated.Add("First Name: " + value.FirstName);
            }

            if (CurrentUser[0].LastName != value.LastName)
            {
                Original.Add("Last Name: " + CurrentUser[0].LastName);
                Updated.Add("Last Name: " + value.LastName);
            }

            if (CurrentUser[0].Email != value.Email)
            {
                Original.Add("Email: " + CurrentUser[0].Email);
                Updated.Add("Email: " + value.Email);
            }

            if (CurrentUser[0].Role != value.Role)
            {
                Original.Add("Role: " + CurrentUser[0].Role);
                Updated.Add("Role: " + value.Role);
            }

            if (CurrentUser[0].Phone != value.Phone)
            {
                Original.Add("Phone: " + CurrentUser[0].Phone);
                Updated.Add("Phone: " + value.Phone);
            }

            if (CurrentUser[0].Active != value.Active)
            {
                Original.Add("Active: " + CurrentUser[0].Active);
                Updated.Add("Active: " + value.Active);
            }

            if (CurrentUser[0].Address != value.Address)
            {
                Original.Add("Address: " + CurrentUser[0].Address);
                Updated.Add("Address: " + value.Address);
            }

            if (CurrentUser[0].City != value.City)
            {
                Original.Add("City: " + CurrentUser[0].City);
                Updated.Add("City: " + value.City);
            }

            if (CurrentUser[0].State != value.State)
            {
                Original.Add("State: " + CurrentUser[0].State);
                Updated.Add("State: " + value.State);
            }

            if (CurrentUser[0].ZIP_Code != value.ZIP_Code)
            {
                Original.Add("ZIP Code: " + CurrentUser[0].ZIP_Code);
                Updated.Add("ZIP Code: " + value.ZIP_Code);
            }

            OriginalModel = String.Join(", ", Original);
            UpdatedModel = String.Join(", ", Updated);

            if (OriginalModel != "")
            {
                //A change has been done
                Logger.LogEditUser(CurrentUser[0].ID, CurrentUser[0].Username, OriginalModel, UpdatedModel);
            }

            Original.Clear();
            Updated.Clear();

            CurrentUser[0].DateModified = value.Date_Modified;
            CurrentUser[0].FirstName = value.FirstName;
            CurrentUser[0].LastName = value.LastName;
            CurrentUser[0].Email = value.Email;
            CurrentUser[0].Role = value.Role;
            CurrentUser[0].Phone = value.Phone;
            CurrentUser[0].Active = value.Active;
            CurrentUser[0].Address = value.Address;
            CurrentUser[0].City = value.City;
            CurrentUser[0].State = value.State;
            CurrentUser[0].ZIP_Code = value.ZIP_Code;

            
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = "Update dbo.UserTable set FirstName = @FirstName, LastName = @LastName, " +
                    "Username = @Username, Password = @Password, Role = @Role, Phone = @Phone, " +
                    "Email = @Email, DateModified = @Date_Modified, Active = @Active, Address = @Address, " +
                    "City = @City, State = @State, ZIP_Code = @ZIP_Code Where ID = @ID;";

                db.Execute(sql, new
                {
                    FirstName = CurrentUser[0].FirstName,
                    LastName = CurrentUser[0].LastName,
                    Username = CurrentUser[0].Username,
                    Password = CurrentUser[0].Password,
                    Role = CurrentUser[0].Role,
                    Phone = CurrentUser[0].Phone,
                    Email = CurrentUser[0].Email,
                    Date_Modified = CurrentUser[0].DateModified,
                    Active = CurrentUser[0].Active,
                    Address = CurrentUser[0].Address,
                    City = CurrentUser[0].City,
                    State = CurrentUser[0].State,
                    ZIP_Code = CurrentUser[0].ZIP_Code,
                    ID = CurrentUser[0].ID

                });
            }
            TempData["Message"] = "Your entry was successfully updated!";

            return RedirectToAction("AllUsers");

        }

        public ActionResult EventLog()
        {
            List<Models.EventLog> events;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                events = db.Query<Models.EventLog>($"Select * from dbo.EventLogTable").ToList();
            }

            return View(events);
        }


        public ActionResult AccountRequests() {
            List<UserRequestsModel> items;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                items = db.Query<UserRequestsModel>($"Select * from dbo.UserRequestsTable").ToList();
            }
            return View(items);
        }

        public ActionResult UserStatistics()
        {
            List<UserStatsModel> stats = new List<UserStatsModel>();

            List<CreateUser> listUser;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listUser = db.Query<CreateUser>($"Select * from dbo.UserTable").ToList();
            }

            for (int i = 0; i < listUser.Count; i++) {
                UserStatsModel mod = new UserStatsModel();
                mod.ID = listUser[i].ID;
                mod.Username = listUser[i].Username;
                mod.Date = listUser[i].Date;
                mod.DateModified = listUser[i].DateModified;
                mod.LastLogin = listUser[i].LastLogin;
                mod.LastSignout = listUser[i].LastSignout;
                mod.LoginAmount = listUser[i].LoginAmount;
                mod.LoginFails = listUser[i].LoginFails;


                stats.Add(mod);
            }

            return View(stats);
        }


        public ActionResult NewAccount()
        {
            NewAccountModel m = new NewAccountModel();
            return View(m);
        }

        [HttpPost]
        public ActionResult NewAccount(NewAccountModel model)
        {
            var sessionUser = Session["Username"] as string;

            string Normal = "";
            if (model.AccountType == "Asset" || model.AccountType == "Liability")
            {
                Normal = "Debit";
            }
            else {
                Normal = "Credit";
            }

            if (ModelState.IsValid)
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = $"Insert into dbo.ChartOfAccounts (AccountNumber, AccountName, " +
                        "AccountType, NormalSide, OriginalBalance, CurrentBalance, AccountDescription, CreatedBy, Active, DateCreated)" +
                        "values(@AccountNumber, @AccountName, @AccountType,@NormalSide,@OriginalBalance," +
                        "@CurrentBalance,@AccountDescription,@CreatedBy,@Active,@Date)";
                    db.Execute(sql, new
                    {

                        AccountNumber = model.AccountNumber,
                        AccountName = model.AccountName,
                        AccountType = model.AccountType,
                        NormalSide = Normal,
                        OriginalBalance = model.OriginalBalance,
                        CurrentBalance = 0,
                        AccountDescription = model.AccountDescription,
                        CreatedBy = sessionUser,
                        Active = model.Active,
                        Date = DateTime.Now
                    });
                }

            //    if (ModelState.IsValid)
            //{
            //    db.ChartOfAccs.Add(tb2);

            //    db.SaveChanges();
            //    var item = db.ChartOfAccs.ToList();
                TempData["Message"] = "A new account was successfully created!";


                return RedirectToAction("ChartOfAccounts");
            }
            //ModelState.Clear();
            return View("NewAccount", new NewAccountModel());
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

        [HttpGet]
        public JsonResult GetAllAccountNumbers()
        {
            List<ChartOfAcc> listAccounts;
            List<int> nums = new List<int>();

            bool t = true;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where Active=@Value", new { Value = t }).ToList();
            }


            for (int i = 0; i < listAccounts.Count; i++)
            {
                nums.Add(listAccounts[i].AccountNumber);
            }

            var result = JsonConvert.SerializeObject(nums);

            return Json(result, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public ActionResult GetAccountType(int id)
        {
            List<ChartOfAcc> listAccounts;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                listAccounts = db.Query<ChartOfAcc>($"Select * From dbo.ChartOfAccounts Where AccountNumber=@ID", new { ID = id }).ToList();
            }

            string res = listAccounts[0].AccountType;
            var result = JsonConvert.SerializeObject(res);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAccount(int id)
        {
            List<ChartOfAcc> editAccount;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                editAccount = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountNumber = @ID", new { ID = id }).ToList();
            }
            //return View(editAccount);

            EditAccountModel e = new EditAccountModel();
            e.AccountNumber = editAccount[0].AccountNumber;
            e.AccountName = editAccount[0].AccountName;
            e.AccountType = editAccount[0].AccountType;
            e.AccountDescription = editAccount[0].AccountDescription;
            e.Active = editAccount[0].Active;

            return View(e);
            //var item = db.ChartOfAccs.Where(x => x.AccountNumber == id).First();
            //return View(item);
        }

        [HttpPost]
        public ActionResult EditAccount(EditAccountModel model)
        {
            if (ModelState.IsValid)
            {
                //List<ChartOfAcc> CurrentAccount;
                //using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                //{

                //    CurrentAccount = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountNumber = @ID", new { ID = model.AccountNumber }).ToList();
                //}             

                //CurrentAccount[0].AccountNumber = model.AccountNumber;
                //CurrentAccount[0].AccountName = model.AccountName;
                //CurrentAccount[0].AccountType = model.AccountType;
                //CurrentAccount[0].NormalSide = model.NormalSide;
                //CurrentAccount[0].OriginalBalance = model.OriginalBalance;
                //CurrentAccount[0].AccountDescription = model.AccountDescription;
                //CurrentAccount[0].Active = model.Active;
                //CurrentAccount[0].CreatedBy = model.CreatedBy;
                //CurrentAccount[0].CurrentBalance = model.CurrentBalance;

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = "Update dbo.ChartOfAccounts set AccountName = @AccountName, " +
                        "AccountType = @AccountType, AccountDescription = @AccountDescription," +
                        "Active = @Active Where AccountNumber = @AccountNumber";


                    db.Execute(sql, new
                    {
                        AccountNumber = model.AccountNumber,
                        AccountName = model.AccountName,
                        AccountType = model.AccountType,
                        AccountDescription = model.AccountDescription,
                        Active = model.Active
                        
                    });
                }
                //if (ModelState.IsValid)
                //{
                //    var item = db.ChartOfAccs.Where(x => x.AccountNumber == model.AccountNumber).First();

                //    item.AccountNumber = model.AccountNumber;
                //    item.AccountName = model.AccountName;
                //    item.AccountType = model.AccountType;
                //    item.NormalSide = model.NormalSide;
                //    item.OriginalBalance = model.OriginalBalance;
                //    item.AccountDescription = model.AccountDescription;
                //    item.Active = model.Active;
                //    item.CreatedBy = model.CreatedBy;
                //    item.CurrentBalance = model.CurrentBalance;


                //    db.SaveChanges();
                //    var item2 = db.ChartOfAccs.ToList();
                TempData["Message"] = "Your entry was successfully updated!";

                return RedirectToAction("ChartOfAccounts");
            
            }

            return View(model);
        }


        public ActionResult Delete(int id)
        {
            List<ChartOfAcc> remainingAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                string sql = $"Delete from dbo.Usertable where ID = @ID";

                db.Execute(sql, new { ID = id });
                remainingAccounts = db.Query<ChartOfAcc>($"Select * from dbo.Usertable").ToList();

            }
            return View("ChartOfAccounts", remainingAccounts);
            //var item = db.ChartOfAccs.Where(x => x.AccountNumber == id).First();
            //db.ChartOfAccs.Remove(item);
            //db.SaveChanges();
            //var item2 = db.ChartOfAccs.ToList();
            //return View("ChartOfAccounts", item2);
        }
    }
}
