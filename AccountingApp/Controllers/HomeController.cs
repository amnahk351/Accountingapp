using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            CreateUser user = new CreateUser();
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
            tbl.Date_Created = model.Date_Created;
            tbl.Active = model.Active;
            tbl.Address = model.Address;
            tbl.City = model.City;
            tbl.State = model.State;
            tbl.ZIP_Code = model.ZIP_Code;
            tbl.Account_Locked = false;
            tbl.Login_Attempts = 10;
            tbl.Login_Amount = 0;
            tbl.Login_Fails = 0;

            if (ModelState.IsValid)
            {
                db.CreateUsers.Add(tbl);

                db.SaveChanges();
                Logger.LogNewUser(tbl.Username);
                Database1Entities6 db2 = new Database1Entities6();
                var events = db2.EventLogs.ToList();
                var item = db.CreateUsers.ToList();
                TempData["Message"] = "Your entry was successfully added!";


                return RedirectToAction("ShowUserData");
            }


            return View("NewUser", new NewUserModel());
        }

        public ActionResult ShowUserData()
        {
            var item = db.CreateUsers.ToList();
            return View(item);
        }

        //public ActionResult Delete(int id)
        //{
        //    var item = db.CreateUsers.Where(x => x.ID == id).First();
        //    db.CreateUsers.Remove(item);
        //    db.SaveChanges();
        //    var item2 = db.CreateUsers.ToList();
        //    return View("ShowUserData",item2);
        //}

        public ActionResult Edit(int id)
        {
            CreateUser item = db.CreateUsers.Where(x => x.ID == id).First();            

            EditUserModel EditView = new EditUserModel();
            EditView.Date_Modified = DateTime.Now;
            EditView.FirstName = item.FirstName;
            EditView.LastName = item.LastName;
            EditView.Email = item.Email;
            EditView.Username = item.Username;
            EditView.Role = item.Role;
            EditView.Phone = item.Phone;
            EditView.Active = item.Active;
            EditView.Address = item.Address;
            EditView.City = item.City;
            EditView.State = item.State;
            EditView.ZIP_Code = item.ZIP_Code;
            return View(EditView);
        }

        [HttpPost]
        public ActionResult Edit(EditUserModel value)
        {
            EventLogHandler Logger = new EventLogHandler();
            CreateUser CurrentUser = db.CreateUsers.Where(x => x.ID == value.ID).First();
            int id = CurrentUser.ID;

            var Original = new List<string>();
            var Updated = new List<string>();

            string OriginalModel = "";
            string UpdatedModel = "";

            if (CurrentUser.Date_Modified != value.Date_Modified) {
                Original.Add("Date Modified: " + CurrentUser.Date_Modified);
                Updated.Add("Date Modified: " + value.Date_Modified);
            }

            if (CurrentUser.FirstName != value.FirstName)
            {
                Original.Add("First Name: " + CurrentUser.FirstName);
                Updated.Add("First Name: " + value.FirstName);
            }

            if (CurrentUser.LastName != value.LastName)
            {
                Original.Add("Last Name: " + CurrentUser.LastName);
                Updated.Add("Last Name: " + value.LastName);
            }

            if (CurrentUser.Email != value.Email)
            {
                Original.Add("Email: " + CurrentUser.Email);
                Updated.Add("Email: " + value.Email);
            }

            if (CurrentUser.Role != value.Role)
            {
                Original.Add("Role: " + CurrentUser.Role);
                Updated.Add("Role: " + value.Role);
            }

            if (CurrentUser.Phone != value.Phone)
            {
                Original.Add("Phone: " + CurrentUser.Phone);
                Updated.Add("Phone: " + value.Phone);
            }

            if (CurrentUser.Active != value.Active)
            {
                Original.Add("Active: " + CurrentUser.Active);
                Updated.Add("Active: " + value.Active);
            }

            if (CurrentUser.Address != value.Address)
            {
                Original.Add("Address: " + CurrentUser.Address);
                Updated.Add("Address: " + value.Address);
            }

            if (CurrentUser.City != value.City)
            {
                Original.Add("City: " + CurrentUser.City);
                Updated.Add("City: " + value.City);
            }

            if (CurrentUser.State != value.State)
            {
                Original.Add("State: " + CurrentUser.State);
                Updated.Add("State: " + value.State);
            }

            if (CurrentUser.ZIP_Code != value.ZIP_Code)
            {
                Original.Add("ZIP Code: " + CurrentUser.ZIP_Code);
                Updated.Add("ZIP Code: " + value.ZIP_Code);
            }

            OriginalModel = String.Join(", ", Original);
            UpdatedModel = String.Join(", ", Updated);

            if (OriginalModel != "")
            {
                //A change has been done
                Logger.LogEditUser(CurrentUser.ID, CurrentUser.Username, OriginalModel, UpdatedModel);
                Database1Entities6 db2 = new Database1Entities6();
                var events = db2.EventLogs.ToList();
            }

            Original.Clear();
            Updated.Clear();

            CurrentUser.Date_Modified = value.Date_Modified;
            CurrentUser.FirstName = value.FirstName;
            CurrentUser.LastName = value.LastName;
            CurrentUser.Email = value.Email;
            CurrentUser.Role = value.Role;
            CurrentUser.Phone = value.Phone;
            CurrentUser.Active = value.Active;
            CurrentUser.Address = value.Address;
            CurrentUser.City = value.City;
            CurrentUser.State = value.State;
            CurrentUser.ZIP_Code = value.ZIP_Code;

            db.SaveChanges();
            var item2 = db.CreateUsers.ToList();
            TempData["Message"] = "Your entry was successfully updated!";

            return RedirectToAction("ShowUserData");
          
        }

        public ActionResult EventLog()
        {
            Database1Entities6 db2 = new Database1Entities6();
            var events = db2.EventLogs.ToList();
            return View(events);
        }

    }
}