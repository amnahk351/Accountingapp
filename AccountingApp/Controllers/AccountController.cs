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
        public ActionResult Authenticate(CreateUser userLoggingIn)
        {
            ErrorController GetErr = new ErrorController();
            string inv = GetErr.GetErrorMessage(19);
            string denied = GetErr.GetErrorMessage(21);
            var db = new Database1Entities4();

            var userDetails = db.CreateUsers.Where(validUser => validUser.Username == userLoggingIn.Username &&
                                                                    validUser.Password == userLoggingIn.Password).FirstOrDefault();

            try
            {
                

                if (userDetails == null)
                    throw new Exception(inv);

                else if (userDetails.Active == false)
                    throw new Exception(denied);
                else {
                    //The account is allowed
                    System.Web.HttpContext.Current.Session["FirstNameofUser"] = userDetails.FirstName;
                    System.Web.HttpContext.Current.Session["UserRole"] = userDetails.Role;  //UserRole is stored in session ID, helpful link https://code.msdn.microsoft.com/How-to-create-and-access-447ada98
                }
            }
            catch (Exception exception)
            {
                Response.Write("<script language=javascript>alert('" + exception.Message + "'); window.location = 'LogIn';</script>");
            }

            if (userDetails.Role == "Admin") {
                return View("~/Views/Admin/AdminIndex.cshtml"); //takes user to admin page
            }

            else if (userDetails.Role == "Accountant") {
                return View("~/Views/Home/Index.cshtml"); //takes user to accountant page, probably should make this one go to a manager page
            }
            return View("~/Views/Admin/AdminIndex.cshtml"); //just a default page to end up at if neither option above was used, probably should make this an accountant




        }
    }
}
