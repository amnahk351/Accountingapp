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
        Database1Entities db = new Database1Entities();
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
             return View();
        }
        [HttpPost]
        public ActionResult NewUser(CreateUser model)
        {
            CreateUser tbl = new CreateUser();
            tbl.FirstName = model.FirstName;
            tbl.LastName = model.LastName;
            tbl.Username = model.Username;
            tbl.Password = model.Password;
            tbl.Role = model.Role;
            tbl.Phone = model.Phone;
            tbl.Email = model.Email;
            tbl.Date = model.Date;

            db.CreateUsers.Add(tbl);

            db.SaveChanges();
            var item = db.CreateUsers.ToList();
            
            if (ModelState.IsValid)
            {
                return RedirectToAction("ShowUserData");
            }
            return View("ShowUserData",item);
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
            var item = db.CreateUsers.Where(x => x.ID == model.ID).First();
            item.FirstName = model.FirstName;
            item.LastName = model.LastName;
            item.Email = model.Email;
            item.Username = model.Username;
            item.Password = model.Password;
            item.Role = model.Role;
            item.Phone = model.Phone;
            item.Date = model.Date;

            db.SaveChanges();
            var item2 = db.CreateUsers.ToList();

            return View("ShowUserData", item2);
        }

    }
}