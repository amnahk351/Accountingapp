using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace AccountingApp.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View("Login");
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

                    if (userDetails.Role == "Admin")
                    {
                        return View("~/Views/Admin/AdminIndex.cshtml"); //takes user to admin page
                    }
                    else if (userDetails.Role == "Accountant")
                    {
                        return View("~/Views/Home/Index.cshtml"); //takes user to accountant page, probably should make this one go to a manager page
                    }
                }
            }
            catch (Exception exception)
            {
                Response.Write("<script language=javascript>alert('" + exception.Message + "'); window.location = 'Login';</script>");
            }
            
            return View("~/Views/Admin/AdminIndex.cshtml"); //just a default page to end up at if neither option above was used, probably should make this an accountant

        }

        public ActionResult ForgotPassword()
        {
            var PartForgot = new ForgotPasswordModel();
            return PartialView(PartForgot);
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel ForgotPass)
        {
            string Em = ForgotPass.Email;
            string message = "If an account uses that email, a password reset link has been sent.";
            using (Database1Entities4 dc = new Database1Entities4())
            {
                var account = dc.CreateUsers.Where(a => a.Email == Em).FirstOrDefault();
                if (account != null)
                {
                    //send email
                    string resetCode = Guid.NewGuid().ToString();
                    SendEmail(Em,resetCode);
                    account.ResetPasswordCode = resetCode;

                    //dc.Configuration.ValidateOnSaveEnabled = false;

                    dc.SaveChanges();
                }
            }
            ViewBag.Message = message;
            //var PartForgot = new ForgotPasswordModel();
            //return PartialView(PartForgot);
            //return Json(new { message });
            //LoginModel Lm = new LoginModel();

            return View();
        }

        [NonAction]
        public void SendEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/Account/ResetPassword/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("objectiveaccteam@gmail.com", "Objective Accounting");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "Application_2019"; // Actual password

            string subject = "";
            string body = "";

            subject = "Reset Password";
            body = "Hello,<br/>We got a password request for an account linked to this email. Please click on the below link to reset your password" +
                "<br/><br/><a href=" + link + ">Reset Password link</a>";


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        //private bool EmailExists(string Email)
        //{
        //    bool found = false;
        //    SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
        //    SqlCommand cmd = new SqlCommand("Select count(*) from CreateUsers where Email=@email", con);
        //    cmd.Parameters.AddWithValue("@email", Email);
        //    con.Open();
        //    int result = (int)cmd.ExecuteScalar();
        //    if (result != 0)
        //    {
        //        found = true;
        //    }
        //    else
        //    {
        //        found = false;
        //    }
        //    con.Close();

        //    return found;
        //}

        public ActionResult ResetPassword(string id)
        {

            using (Database1Entities4 dc = new Database1Entities4())
            {
                var user = dc.CreateUsers.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }

            //var ResetPass = new ResetPasswordModel();
            //return PartialView(ResetPass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (Database1Entities4 dc = new Database1Entities4())
                {
                    var user = dc.CreateUsers.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        OldPasswordHandler PassHand = new OldPasswordHandler();
                        PassHand.AdjustOldPasswords(user.Password, user.ID);

                        user.Password = model.Password;
                        user.ResetPasswordCode = "";

                        dc.SaveChanges();
                        message = "Password updated successfully.";
                    }
                }
            }
            else
            {
                message = "An error occurred.";
            }

            ViewBag.Message = message;
            return View(model);
        }
    }
}
