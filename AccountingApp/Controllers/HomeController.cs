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
        public ActionResult LogIn()
        {
            return View("LogIn");
        }
        [HttpPost]
        public ActionResult LogIn(String username, String password)
        {
            return View("Index");
        }
        Database1Entities2 db = new Database1Entities2();

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
        public ActionResult NewUser(CreateUser model)
        {
            if (ModelState.IsValid)
            {
<<<<<<< HEAD
                db.CreateUsers.Add(model);

=======
                db.CreateUsers.Add(tbl);
>>>>>>> login page loaded by default
                db.SaveChanges();
                var item = db.CreateUsers.ToList();
                TempData["Message"] = "Your entry was successfully added!";


                return RedirectToAction("ShowUserData");
            }


            return View("NewUser", new CreateUser());
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
            var item = db.CreateUsers.Where(x => x.ID == id).First();
            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(CreateUser model)
        {
            if (ModelState.IsValid)
            {
                var item = db.CreateUsers.Where(x => x.ID == model.ID).First();
                item.Date_Modified = model.Date_Modified;
                item.FirstName = model.FirstName;
                item.LastName = model.LastName;
                item.Email = model.Email;
                item.Username = model.Username;
                item.Password = model.Password;
                item.Role = model.Role;
                item.Phone = model.Phone;
                item.Active = model.Active;
                item.Address = model.Address;
                item.City = model.City;
                item.State = model.State;
                item.ZIP_Code = model.ZIP_Code;
           
                db.SaveChanges();
                var item2 = db.CreateUsers.ToList();
                TempData["Message"] = "Your entry was successfully updated!";

                return RedirectToAction("ShowUserData");
            }
            return View(model);
        }

    }
}