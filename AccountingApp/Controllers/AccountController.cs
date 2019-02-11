﻿using AccountingApp.Models;
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
                    userLoggingIn.Empty = true;
                    return View("LogIn", userLoggingIn);
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