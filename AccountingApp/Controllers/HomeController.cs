﻿using AccountingApp.Models;
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
            //CreateUser user = new CreateUser();
            return View();
        }
        [HttpPost]
        public ActionResult NewUser(NewUserModel model)
        {
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

            if (ModelState.IsValid)
            {
                db.CreateUsers.Add(tbl);

                db.SaveChanges();
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

        public ActionResult Delete(int id)
        {
            var item = db.CreateUsers.Where(x => x.ID == id).First();
            db.CreateUsers.Remove(item);
            db.SaveChanges();
            var item2 = db.CreateUsers.ToList();
            return View("ShowUserData",item2);
        }

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
            CreateUser CurrentUser = db.CreateUsers.Where(x => x.ID == value.ID).First();
            int id = CurrentUser.ID;

            CurrentUser.Date_Modified = value.Date_Modified;
            CurrentUser.FirstName = value.FirstName;
            CurrentUser.LastName = value.LastName;
            CurrentUser.Email = value.Email;
            
            //if (CurrentPassword != value.Password)
            //{
            //    //System.Diagnostics.Debug.WriteLine("value pass: " + value.Password);
            //    //System.Diagnostics.Debug.WriteLine("It got here");
            //    OldPasswordHandler PassHand = new OldPasswordHandler();
            //    PassHand.AdjustOldPasswords(CurrentPassword, id);
            //    //CurrentUser.Old_Passwords = CurrentPassword;
            //}
            //else {
            //    CurrentUser.Old_Passwords = null;
            //}

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

    }
}