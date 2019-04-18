using AccountingApp.Models;
using AccountingApp.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace AccountingApp.Controllers
{    
    public class HomeController : Controller
    {
        Database1Entities5 db = new Database1Entities5();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

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
                //db.CreateUsers.Ad
                //db.CreateUsers.Add(tbl);

                //db.SaveChanges();
                //var item = db.CreateUsers.ToList();
                TempData["Message"] = "Your entry was successfully added!";
                //Logger.LogNewUser(tbl.Username);

                return RedirectToAction("ShowUserData");
            }


            return View("NewUser", new NewUserModel());
            //if (ModelState.IsValid)
            //{
            //    db.CreateUsers.Add(tbl);

            //    db.SaveChanges();
            //    Logger.LogNewUser(tbl.Username);
            //    Database1Entities6 db2 = new Database1Entities6();
            //    var events = db2.EventLogs.ToList();
            //    var item = db.CreateUsers.ToList();
            //    TempData["Message"] = "Your entry was successfully added!";


            //    return RedirectToAction("ShowUserData");
            //}


           // return View("NewUser", new NewUserModel());
        }

        public ActionResult ShowUserData()
        {
            List<CreateUser> listUser;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listUser = db.Query<CreateUser>($"Select * from dbo.Usertable").ToList();
            }
            
            return View(listUser);
            //var item = db.CreateUsers.ToList();
            //return View(item);
        }

        //public ActionResult Delete(int id)
        //{
        //    var item = db.CreateUsers.Where(x => x.ID == id).First();
        //    db.CreateUsers.Remove(item);
        //    db.SaveChanges();
        //    var item2 = db.CreateUsers.ToList();
        //    return View("ShowUserData",item2);
        //}

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
            //CreateUser item = db.CreateUsers.Where(x => x.ID == id).First();            

            //EditUserModel EditView = new EditUserModel();
            //EditView.Date_Modified = DateTime.Now;
            //EditView.FirstName = item.FirstName;
            //EditView.LastName = item.LastName;
            //EditView.Email = item.Email;
            //EditView.Username = item.Username;
            //EditView.Role = item.Role;
            //EditView.Phone = item.Phone;
            //EditView.Active = item.Active;
            //EditView.Address = item.Address;
            //EditView.City = item.City;
            //EditView.State = item.State;
            //EditView.ZIP_Code = item.ZIP_Code;
            return View(EditView);
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
            //CreateUser CurrentUser = db.CreateUsers.Where(x => x.ID == value.ID).First();
            //int id = CurrentUser.ID;

            var Original = new List<string>();
            var Updated = new List<string>();

            string OriginalModel = "";
            string UpdatedModel = "";

            if (CurrentUser[0].DateModified != value.Date_Modified) {
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
                //Database1Entities6 db2 = new Database1Entities6();
                //var events = db2.EventLogs.ToList();
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
            //CurrentUser.Date_Modified = value.Date_Modified;
            //CurrentUser.FirstName = value.FirstName;
            //CurrentUser.LastName = value.LastName;
            //CurrentUser.Email = value.Email;
            //CurrentUser.Role = value.Role;
            //CurrentUser.Phone = value.Phone;
            //CurrentUser.Active = value.Active;
            //CurrentUser.Address = value.Address;
            //CurrentUser.City = value.City;
            //CurrentUser.State = value.State;
            //CurrentUser.ZIP_Code = value.ZIP_Code;

            //db.SaveChanges();
            //var item2 = db.CreateUsers.ToList();
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

            return RedirectToAction("ShowUserData");
          
        }

        public ActionResult EventLog()
        {
            List<EventLog> events;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                events = db.Query<EventLog>($"Select * from dbo.EventLogTable").ToList();
            }

            return View(events);
            //Database1Entities6 db2 = new Database1Entities6();
            //var events = db2.EventLogs.ToList();
            //return View(events);
        }

    }
}
