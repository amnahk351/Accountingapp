using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult NewAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewAccount(ChartOfAcc model)
        {
            ChartOfAcc tb2 = new ChartOfAcc();
            tb2.AccountNumber = model.AccountNumber;
            tb2.AccountName = model.AccountName;
            tb2.AccountType = model.AccountType;
            tb2.NormalSide = model.NormalSide;
            tb2.OriginalBalance = model.OriginalBalance;
            tb2.CurrentBalance = model.CurrentBalance;
            tb2.AccountDescription = model.AccountDescription;
            tb2.CreatedBy = model.CreatedBy;
            tb2.Active = model.Active;
           
            if (ModelState.IsValid)
            {
                db.ChartOfAccs.Add(tb2);

                db.SaveChanges();
                var item = db.ChartOfAccs.ToList();
                TempData["Message"] = "A new account was successfully created !";


                return RedirectToAction("ChartOfAccounts");
            }

            return View("NewAccount", new ChartOfAcc());
        }
       
     

        
        public ActionResult ChartOfAccounts()
        {
            var item = db.ChartOfAccs.ToList();
            return View(item);

            
        }

        public ActionResult EditAccount(int id)
        {
            var item = db.ChartOfAccs.Where(x => x.AccountNumber == id).First();
            return View(item);
        }

        [HttpPost]
        public ActionResult EditAccount(ChartOfAcc model)
        {
            if (ModelState.IsValid)
            {
                var item = db.ChartOfAccs.Where(x => x.AccountNumber == model.AccountNumber).First();

                item.AccountNumber = model.AccountNumber;
                item.AccountName = model.AccountName;
                item.AccountType = model.AccountType;
                item.NormalSide = model.NormalSide;
                item.OriginalBalance = model.OriginalBalance;
                item.AccountDescription = model.AccountDescription;
                item.Active = model.Active;
                item.CreatedBy = model.CreatedBy;
                item.CurrentBalance = model.CurrentBalance;
               

                db.SaveChanges();
                var item2 = db.ChartOfAccs.ToList();
                TempData["Message"] = "Your entry was successfully updated!";

                return RedirectToAction("ChartOfAccounts");
            }
            return View(model);
        }


        public ActionResult Delete(int id)
        {
            var item = db.ChartOfAccs.Where(x => x.AccountNumber == id).First();
            db.ChartOfAccs.Remove(item);
            db.SaveChanges();
            var item2 = db.ChartOfAccs.ToList();
            return View("ChartOfAccounts", item2);
        }
    }
}
