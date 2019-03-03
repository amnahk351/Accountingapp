using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Data;

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
            string locked = GetErr.GetErrorMessage(30);
            string attempts = GetErr.GetErrorMessage(31);
            var db = new Database1Entities5();
            
            //checks username and password both exists for an account, left for reference
            //var userDetails = db.CreateUsers.Where(validUser => validUser.Username == userLoggingIn.Username && validUser.Password == userLoggingIn.Password).FirstOrDefault();
                        
            var userDetails = db.CreateUsers.Where(validUser => validUser.Username == userLoggingIn.Username).FirstOrDefault();  //get the account for the typed username

            try
            {
                if (userDetails == null)
                {
                    throw new Exception(inv);  //the username does not exist                    
                }

                else if (userLoggingIn.Password != userDetails.Password)
                {
                    //usernames exists, but password is wrong                    
                    if (userDetails.Login_Attempts == 1)
                    {
                        userDetails.Account_Locked = true;
                        db.SaveChanges();
                        throw new Exception(locked);
                    }

                    userDetails.Login_Fails++;
                    userDetails.Login_Attempts--;
                    db.SaveChanges();

                    throw new Exception(attempts + " " + userDetails.Login_Attempts.ToString());
                }

                else if (userDetails.Active == false)
                    throw new Exception(denied);
                else if (userDetails.Account_Locked == true)
                    throw new Exception(locked);
                else if (userDetails.Security_Question1 == null) {
                    //Not answered security questions
                    System.Web.HttpContext.Current.Session["FirstNameofUser"] = userDetails.FirstName;
                    System.Web.HttpContext.Current.Session["Username"] = userDetails.Username;
                    System.Web.HttpContext.Current.Session["UserRole"] = userDetails.Role;
                                        
                    userDetails.Login_Amount++;
                    db.SaveChanges();

                    System.Diagnostics.Debug.WriteLine("Went to security questions.");
                    return Redirect("~/Account/SecurityQuestions");
                }
                else
                {
                    //The account is allowed
                    System.Web.HttpContext.Current.Session["FirstNameofUser"] = userDetails.FirstName;
                    System.Web.HttpContext.Current.Session["Username"] = userDetails.Username;
                    System.Web.HttpContext.Current.Session["UserRole"] = userDetails.Role;  //UserRole is stored in session ID, helpful link https://code.msdn.microsoft.com/How-to-create-and-access-447ada98

                    userDetails.Login_Attempts = 10;
                    userDetails.Login_Amount++;
                    db.SaveChanges();

                    if (userDetails.Role == "Admin")
                    {
                        return Redirect("~/Admin/AdminIndex"); //takes user to admin page
                        //return View("~/Views/Admin/AdminIndex.cshtml"); //takes user to admin page
                    }
                    else if (userDetails.Role == "Accountant")
                    {
                        return Redirect("~/Accountant/AccountantIndex");  //takes user to accountant page, probably should make this one go to a manager page
                        //return View("~/Views/Home/Index.cshtml"); //takes user to accountant page, probably should make this one go to a manager page
                    }
                }
            }
            catch (Exception exception)
            {
                Response.Write("<script language=javascript>alert('" + exception.Message + "'); window.location = 'Login';</script>");
            }

            //return Redirect("~/Admin/AdminIndex");  //just a default page to end up at if neither option above was used, probably should make this an accountant
            return View("~/Views/Admin/AdminIndex.cshtml"); //just a default page to end up at if neither option above was used, probably should make this an accountant

        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            var PartForgot = new ForgotPasswordModel();
            return PartialView(PartForgot);
        }

        [HttpPost]
        public void ForgotPassword(ForgotPasswordModel ForgotPass, string Email)
        {
            string Em = Email;
            
            using (Database1Entities5 dc = new Database1Entities5())
            {                
                var account = dc.CreateUsers.Where(a => a.Email == Em).FirstOrDefault();

                if (account != null)
                {
                    //send email
                    string resetCode = Guid.NewGuid().ToString();
                    SendEmail(Em, resetCode);
                    account.ResetPasswordCode = resetCode;
                    dc.SaveChanges();

                    System.Diagnostics.Debug.WriteLine("Email was sent");
                }
                else {
                    System.Diagnostics.Debug.WriteLine("Fail, no email sent");
                }
            }
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

        public ActionResult ResetPassword(string id)
        {

            using (Database1Entities5 dc = new Database1Entities5())
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
            if (ModelState.IsValid)
            {
                using (Database1Entities5 dc = new Database1Entities5())
                {
                    var user = dc.CreateUsers.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        OldPasswordHandler PassHand = new OldPasswordHandler();
                        PassHand.AdjustOldPasswords(user.Password, user.ID);

                        user.Password = model.Password;
                        user.ResetPasswordCode = "";

                        dc.SaveChanges();
                        var message = "Password updated successfully.";
                        ViewBag.Message = message;
                    }
                }
            }
            return View(model);
        }

        public void GetErrors()
        {
            List<String> Errors = new List<String>();

            using (SqlConnection connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True"))
            {
                connection.Open();
                string query = "SELECT Description FROM ErrorMessages";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Errors.Add(reader.GetString(0));
                        }
                    }
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Response.Write(js.Serialize(Errors));
        }

        public ActionResult ChangePassword()
        {
            ChangePasswordModel ChgePass = new ChangePasswordModel();
            return View(ChgePass);
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            //have to add code to replace password still
            //use old passsword handler

            using (Database1Entities5 dc = new Database1Entities5())
            {
                var sessionUser = Session["Username"] as string;

                var user = dc.CreateUsers.Where(a => a.Username == sessionUser).FirstOrDefault();
                if (user != null)
                {
                    OldPasswordHandler PassHand = new OldPasswordHandler();
                    PassHand.AdjustOldPasswords(model.CurrentPassword, user.ID);

                    user.Password = model.NewPassword;
                    dc.SaveChanges();
                                       
                    var message = "Password updated successfully.";
                    ViewBag.Message = message;
                }
            }
            
            return View(model);
        }

        public ActionResult SecurityQuestions()
        {
            var Security = new SecurityQuestionsModel();
            return View(Security);           
        }

        [HttpPost]
        public ActionResult SecurityQuestions(SecurityQuestionsModel model)
        {
            var sessionUser = Session["Username"] as string;
            var sessionRole = Session["UserRole"] as string;

            using (Database1Entities5 dc = new Database1Entities5())
            {
                var account = dc.CreateUsers.Where(a => a.Username == sessionUser).FirstOrDefault();

                account.Security_Question1 = model.Security_Question1;
                account.Answer_1 = model.Answer_1;
                account.Security_Question2 = model.Security_Question2;
                account.Answer_2 = model.Answer_2;
                dc.SaveChanges();                               
                
            }

            if (sessionRole == "Admin")
            {
                return Redirect("~/Admin/AdminIndex");
            }
            else if (sessionRole == "Accountant")
            {
                return Redirect("~/Accountant/AccountantIndex");
            }

            return Redirect("~/Admin/AdminIndex");
        }

        public ActionResult AccountRecovery()
        {
            var v = new AccountRecoveryModel();
            return View(v);
        }

        [HttpPost]
        public ActionResult AccountRecovery(AccountRecoveryModel model) {
            ErrorController ErrorFinder = new ErrorController();
            string mess = ErrorFinder.GetErrorMessage(35);

            Database1Entities5 db = new Database1Entities5();
            var userDetails = db.CreateUsers.Where(validUser => validUser.Username == model.Username && validUser.Email == model.Email).FirstOrDefault();

            if (userDetails == null)
            {

                ViewBag.Message = mess;
            }
            else {
                //store in session variables the username and email
                System.Web.HttpContext.Current.Session["Username"] = userDetails.Username;
                System.Web.HttpContext.Current.Session["Email"] = userDetails.Email;

                //go to security questions page for answerings and unlocking

                return Redirect("~/Account/AnswerQuestions");
            }

            return View();
        }

        public ActionResult AnswerQuestions()
        {
            var CustomView = new AnswerQuestionsModel();
            var sessionUser = Session["Username"] as string;
            var sessionEmail = Session["Email"] as string;

            Database1Entities5 db = new Database1Entities5();
            var userDetails = db.CreateUsers.Where(validUser => validUser.Username == sessionUser && validUser.Email == sessionEmail).FirstOrDefault();

            ViewBag.Question_1 = userDetails.Security_Question1;
            ViewBag.Question_2 = userDetails.Security_Question2;

            return View(CustomView);
        }
    }
}
