using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountingApp.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult LogIn()
        {
            return View("LogIn");
        }
        [HttpPost]
        public ActionResult Authenticate(AccountingApp.Models.CreateUser userLoggingIn)
        {

            var db = new Database1Entities2();

            try
            {

                var userDetails = db.CreateUsers.Where(validUser => validUser.Username == userLoggingIn.Username &&
                                                                    validUser.Password == userLoggingIn.Password).FirstOrDefault();

                if (userDetails == null)
                {
                    return View("LogIn", userLoggingIn);
                    //TempData["invalidAuthMessage"] = "Invalid Credentials.";
                    //ViewBag.Message = "Invalid Credentials";
                    //Response.Write("<script language='javascript'>window.alert('Invalid Credentials');window.location='~/Views/Account/LogIn.cshtml';</script>");
                    //Response.Redirect("LogIn");
                }
            }
            catch (Exception exception)
            {
                Response.Write(exception);
            }

            return View("~/Views/Home/Index.cshtml");
        }
    }
}