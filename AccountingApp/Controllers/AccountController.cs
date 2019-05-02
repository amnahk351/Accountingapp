using AccountingApp.Models;
using AccountingApp.DBAccess;
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
using Dapper;

namespace AccountingApp.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View("Login");
        }
        [HttpPost]
        public ActionResult Authenticate(LoginModel userLoggingIn)
        {
            EventLogHandler Logger = new EventLogHandler();
            ErrorController GetErr = new ErrorController();
            string inv = GetErr.GetErrorMessage(19);
            string denied = GetErr.GetErrorMessage(21);
            string locked = GetErr.GetErrorMessage(30);
            string attempts = GetErr.GetErrorMessage(31);
            //var db = new Database1Entities5();

            //checks username and password both exists for an account, left for reference
            //var userDetails = db.CreateUsers.Where(validUser => validUser.Username == userLoggingIn.Username && validUser.Password == userLoggingIn.Password).FirstOrDefault();

            //var userDetails = db.CreateUsers.Where(validUser => validUser.Username == userLoggingIn.Username).FirstOrDefault();  //get the account for the typed username
            //List<CreateUser> validateLogin;
            List<UserModel> userDetails;


            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                userDetails = db.Query<UserModel>("Select * from dbo.UserTable Where Username = @Username", new { Username = userLoggingIn.Username }).ToList();

            }

            try
            {
                if (userDetails == null)
                {
                    throw new Exception(inv);  //the username does not exist                    
                }

                else if (userDetails.Count == 0)
                {
                    throw new Exception(inv);
                }

                else if (userDetails[0].Active == false)
                {
                    throw new Exception(denied);
                }

                else if (userDetails[0].AccountLocked == true)
                {
                    throw new Exception(locked);
                }


                if (userLoggingIn.Password != userDetails[0].Password)
                {
                    //usernames exists, but password is wrong

                    if (userDetails[0].LoginAttempts == 1)
                    {

                        //Adjust fails and login attempts for last attempt
                        using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                        {
                            string sql = "Update dbo.UserTable set LoginFails = @fails, LoginAttempts = @attempts Where Username = @name;";

                            db.Execute(sql, new
                            {
                                fails = userDetails[0].LoginFails + 1,
                                attempts = userDetails[0].LoginAttempts - 1,
                                name = userDetails[0].Username

                            });
                        }

                        //Lock account for too many invalid login attempts;
                        bool NewLock = true;

                        using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                        {
                            string sql = "Update dbo.UserTable set AccountLocked = @Lock Where Username = @name;";

                            db.Execute(sql, new
                            {
                                Lock = NewLock,
                                name = userDetails[0].Username

                            });
                        }

                        Logger.LogAccountLocked(userDetails[0].ID, userDetails[0].Username);
                        throw new Exception(locked);
                    }


                    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                    {
                        string sql = "Update dbo.UserTable set LoginFails = @fails, LoginAttempts = @attempts Where Username = @name;";

                        db.Execute(sql, new
                        {
                            fails = userDetails[0].LoginFails + 1,
                            attempts = userDetails[0].LoginAttempts - 1,
                            name = userDetails[0].Username

                        });
                    }

                    int AmountRemaining = (int)userDetails[0].LoginAttempts - 1;


                    throw new Exception(attempts + " " + AmountRemaining.ToString());

                    //throw new Exception(attempts + " " + validateLogin[0].Login_Attempts.ToString());
                }


                else if (userDetails[0].SecurityQuestion1 == null)
                {
                    System.Web.HttpContext.Current.Session["FirstNameofUser"] = userDetails[0].FirstName;
                    System.Web.HttpContext.Current.Session["Username"] = userDetails[0].Username;
                    System.Web.HttpContext.Current.Session["UserRole"] = userDetails[0].Role;



                    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                    {
                        string sql = "Update dbo.UserTable set LoginAmount = @amount Where Username = @name;";

                        db.Execute(sql, new
                        {
                            amount = userDetails[0].LoginAmount + 1,
                            name = userDetails[0].Username

                        });
                    }


                    return Redirect("~/Account/SecurityQuestions");
                }
                else
                {
                    //The account is allowed
                    System.Web.HttpContext.Current.Session["FirstNameofUser"] = userDetails[0].FirstName;
                    System.Web.HttpContext.Current.Session["Username"] = userDetails[0].Username;
                    System.Web.HttpContext.Current.Session["UserRole"] = userDetails[0].Role;

                    //UserRole is stored in session ID, helpful link https://code.msdn.microsoft.com/How-to-create-and-access-447ada98


                    int x = 10;

                    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                    {
                        string sql = "Update dbo.UserTable set LoginAmount = @amount, LoginAttempts = @attempts, LastLogin = @time Where Username = @name;";

                        db.Execute(sql, new
                        {
                            amount = userDetails[0].LoginAmount + 1,
                            attempts = x,
                            time = DateTime.Now,
                            name = userDetails[0].Username

                        });
                    }

                    Logger.LogUserLogin(userDetails[0].Username);

                    if (userDetails[0].Role == "Admin")
                    {
                        return Redirect("~/Admin/Dashboard"); //takes user to Admin view
                    }
                    else if (userDetails[0].Role == "Accountant")
                    {
                        return Redirect("~/Accountant/Dashboard");  //takes user to Accountant view
                    }
                    else if (userDetails[0].Role == "Manager")  //takes user to Manager view
                    {
                        return Redirect("~/Manager/ManagerIndex");
                    }


                }
            }



            //try
            //{
            //    if (userDetails == null)
            //    {
            //        throw new Exception(inv);  //the username does not exist                    
            //    }

            //    else if (userLoggingIn.Password != userDetails.Password)
            //    {
            //        //usernames exists, but password is wrong                    
            //        if (userDetails.Login_Attempts == 1)
            //        {
            //            userDetails.Account_Locked = true;
            //            db.SaveChanges();

            //            Logger.LogAccountLocked(userDetails.ID, userDetails.Username);
            //            Database1Entities6 db2 = new Database1Entities6();
            //            var events = db2.EventLogs.ToList();
            //            throw new Exception(locked);
            //        }

            //        userDetails.Login_Fails++;
            //        userDetails.Login_Attempts--;
            //        db.SaveChanges();

            //        throw new Exception(attempts + " " + userDetails.Login_Attempts.ToString());
            //    }

            //    else if (userDetails.Active == false)
            //        throw new Exception(denied);
            //    else if (userDetails.Account_Locked == true)
            //        throw new Exception(locked);
            //    else if (userDetails.Security_Question1 == null) {
            //        //Not answered security questions
            //        System.Web.HttpContext.Current.Session["FirstNameofUser"] = userDetails.FirstName;
            //        System.Web.HttpContext.Current.Session["Username"] = userDetails.Username;
            //        System.Web.HttpContext.Current.Session["UserRole"] = userDetails.Role;

            //        userDetails.Login_Amount++;
            //        db.SaveChanges();

            //        System.Diagnostics.Debug.WriteLine("Went to security questions.");
            //        return Redirect("~/Account/SecurityQuestions");
            //    }
            //    else
            //    {
            //        //The account is allowed
            //        System.Web.HttpContext.Current.Session["UserID"] = userDetails.ID;
            //        System.Web.HttpContext.Current.Session["FirstNameofUser"] = userDetails.FirstName;
            //        System.Web.HttpContext.Current.Session["Username"] = userDetails.Username;
            //        System.Web.HttpContext.Current.Session["UserRole"] = userDetails.Role;  //UserRole is stored in session ID, helpful link https://code.msdn.microsoft.com/How-to-create-and-access-447ada98

            //        userDetails.Login_Attempts = 10;
            //        userDetails.Login_Amount++;
            //        db.SaveChanges();

            //        if (userDetails.Role == "Admin")
            //        {
            //            return Redirect("~/Admin/AdminIndex"); //takes user to admin page
            //            //return View("~/Views/Admin/AdminIndex.cshtml"); //takes user to admin page
            //        }
            //        else if (userDetails.Role == "Manager")
            //        {
            //            return Redirect("~/Manager/ManagerIndex");
            //        }
            //        else if (userDetails.Role == "Accountant")
            //        {
            //            return Redirect("~/Accountant/AccountantIndex");  //takes user to accountant page, probably should make this one go to a manager page
            //            //return View("~/Views/Home/Index.cshtml"); //takes user to accountant page, probably should make this one go to a manager page
            //        }
            //    }
            //}
            catch (Exception exception)
            {
                Response.Write("<script language=javascript>alert('" + exception.Message + "'); window.location = 'Login';</script>");
            }

            //return Redirect("~/Admin/AdminIndex");  //just a default page to end up at if neither option above was used, probably should make this an accountant
            return new EmptyResult();//just a default page to end up at if neither option above was used, probably should make this an accountant

        }

        [HttpPost]
        public ActionResult LogOutUser(string username)
        {
            System.Diagnostics.Debug.WriteLine("user " + username);
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                //string sql = $"Insert into dbo.UserTable LastSignout = @time WHERE Username = @user";
                string sql = $"Update dbo.UserTable set LastSignout = @time WHERE Username = @user";

                db.Execute(sql, new
                {
                    time = DateTime.Now,
                    user = username
                });
            }
            //return View("Login");
            return Json("User Signed Out!");
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
            EventLogHandler Logger = new EventLogHandler();

            List<CreateUser> validateEmail;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                validateEmail = db.Query<CreateUser>("Select * from dbo.Usertable Where Email = @Email", new { Email = Em }).ToList();
            }

            if (validateEmail.Count > 0)
            {
                //send email
                string resetCode = Guid.NewGuid().ToString();
                SendEmail(Em, resetCode);
                validateEmail[0].ResetPasswordCode = resetCode;

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = "Update dbo.UserTable set ResetPasswordCode = @resetCode where Username = @Username";
                    db.Execute(sql, new { resetCode = resetCode, Username = validateEmail[0].Username });
                }
                Logger.LogForgotPassword(Em);
                System.Diagnostics.Debug.WriteLine("Email was sent");

            }

            else
            {
                System.Diagnostics.Debug.WriteLine("Fail, no email sent");
            }
            //using (Database1Entities5 dc = new Database1Entities5())
            //{                
            //    var account = dc.CreateUsers.Where(a => a.Email == Em).FirstOrDefault();

            //    if (account != null)
            //    {
            //        //send email
            //        string resetCode = Guid.NewGuid().ToString();
            //        SendEmail(Em, resetCode);
            //        account.ResetPasswordCode = resetCode;
            //        dc.SaveChanges();

            //        Logger.LogForgotPassword(Em);
            //        Database1Entities6 db2 = new Database1Entities6();
            //        var events = db2.EventLogs.ToList();
            //        System.Diagnostics.Debug.WriteLine("Email was sent");
            //    }
            //    else {
            //        System.Diagnostics.Debug.WriteLine("Fail, no email sent");
            //    }
            //}
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
            List<CreateUser> validatePasswordCode;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                validatePasswordCode = db.Query<CreateUser>($"Select * from dbo.Usertable Where ResetPasswordCode = @ResetCode", new { ResetCode = id }).ToList();

            }
            if (validatePasswordCode.Count() > 0)
            {
                ResetPasswordModel model = new ResetPasswordModel();
                model.ResetCode = id;
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
            //using (Database1Entities5 dc = new Database1Entities5())
            //{
            //    var user = dc.CreateUsers.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
            //    if (user != null)
            //    {
            //        ResetPasswordModel model = new ResetPasswordModel();
            //        model.ResetCode = id;
            //        return View(model);
            //    }
            //    else
            //    {
            //        return HttpNotFound();
            //    }
            //}

            //var ResetPass = new ResetPasswordModel();
            //return PartialView(ResetPass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            EventLogHandler Logger = new EventLogHandler();

            var message = "";
            if (ModelState.IsValid)
            {

                List<CreateUser> validatePasswordCode;

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    validatePasswordCode = db.Query<CreateUser>($"Select * from dbo.Usertable Where ResetPasswordCode = @ResetCode", new { ResetCode = model.ResetCode }).ToList();

                }

                if (validatePasswordCode.Count > 0)
                {
                    OldPasswordHandler PassHand = new OldPasswordHandler();
                    PassHand.AdjustOldPasswords(validatePasswordCode[0].Password, validatePasswordCode[0].ID);
                    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                    {

                        string sql = $"Update dbo.UserTable set Password = @Password, ResetPasswordCode = NULL where Username = @Username";
                        db.Execute(sql, new { Password = model.Password, Username = validatePasswordCode[0].Username });
                        message = "Password updated successfully.";
                        Logger.LogPasswordReset(validatePasswordCode[0].ID, validatePasswordCode[0].Username);
                        ViewBag.Message = message;
                    }
                }
            }


            return View(model);
            //if (ModelState.IsValid)
            //{
            //    using (Database1Entities5 dc = new Database1Entities5())
            //    {
            //        var user = dc.CreateUsers.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
            //        if (user != null)
            //        {
            //            OldPasswordHandler PassHand = new OldPasswordHandler();
            //            PassHand.AdjustOldPasswords(user.Password, user.ID);

            //            user.Password = model.Password;
            //            user.ResetPasswordCode = "";

            //            dc.SaveChanges();
            //            Logger.LogPasswordReset(user.ID, user.Username);
            //            Database1Entities6 db2 = new Database1Entities6();
            //            var events = db2.EventLogs.ToList();
            //            var message = "Password updated successfully.";
            //            ViewBag.Message = message;
            //        }
            //    }
            //}
            //return View(model);
        }

        public void GetErrors()
        {
            List<String> Errors = new List<String>();


            List<ErrorMessageModel> messages;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                messages = db.Query<ErrorMessageModel>("Select * from dbo.ErrorMessages").ToList();
            }
            //using (SqlConnection connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True"))
            //{
            //    connection.Open();
            //    string query = "SELECT Description FROM ErrorMessages";
            //    using (SqlCommand command = new SqlCommand(query, connection))
            //    {
            //        using (SqlDataReader reader = command.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                Errors.Add(reader.GetString(0));
            //            }
            //        }
            //    }
            //}
            foreach (var message in messages)
            {
                Errors.Add(message.Description);
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
            EventLogHandler Logger = new EventLogHandler();

            List<CreateUser> user;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                var sessionUser = Session["Username"] as string;

                user = db.Query<CreateUser>("Select * from dbo.UserTable where Username = @Username;", new { Username = sessionUser }).ToList();
            }
            if (user.Count() > 0)
            {
                OldPasswordHandler PassHand = new OldPasswordHandler();
                PassHand.AdjustOldPasswords(model.CurrentPassword, user[0].ID);
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = $"Update dbo.UserTable set Password = @Password where Username = @Username;";
                    db.Execute(sql, new { Password = user[0].Password, Username = user[0].Username });
                }
                Logger.LogPasswordChange();
                var message = "Password updated successfully.";
                ViewBag.Message = message;
            }
            // using (Database1Entities5 dc = new Database1Entities5())
            //{
            //    EventLogHandler Logger = new EventLogHandler();
            //    var sessionUser = Session["Username"] as string;

            //    var user = dc.CreateUsers.Where(a => a.Username == sessionUser).FirstOrDefault();
            //    if (user != null)
            //    {
            //        OldPasswordHandler PassHand = new OldPasswordHandler();
            //        PassHand.AdjustOldPasswords(model.CurrentPassword, user.ID);

            //        user.Password = model.NewPassword;
            //        dc.SaveChanges();

            //        Logger.LogPasswordChange();
            //        Database1Entities6 db2 = new Database1Entities6();
            //        var events = db2.EventLogs.ToList();
            //        var message = "Password updated successfully.";
            //        ViewBag.Message = message;
            //    }
            //}

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

            List<CreateUser> user;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {


                user = db.Query<CreateUser>("Select * from dbo.UserTable where Username = @Username", new { Username = sessionUser }).ToList();
                string sql = $"Update dbo.UserTable set SecurityQuestion1 = @Question1, Answer1 = @Ans1," +
                             "SecurityQuestion2 = @Question2, Answer2 = @Ans2 where Username = @Username";
                db.Execute(sql, new
                {
                    Question1 = model.Security_Question1,
                    Ans1 = model.Answer_1,
                    Question2 = model.Security_Question2,
                    Ans2 = model.Answer_2,
                    Username = sessionUser
                });
            }
            //using (Database1Entities5 dc = new Database1Entities5())
            //{
            //    var account = dc.CreateUsers.Where(a => a.Username == sessionUser).FirstOrDefault();

            //    account.Security_Question1 = model.Security_Question1;
            //    account.Answer_1 = model.Answer_1;
            //    account.Security_Question2 = model.Security_Question2;
            //    account.Answer_2 = model.Answer_2;
            //    dc.SaveChanges();                               

            //}

            if (sessionRole == "Admin")
            {
                return Redirect("~/Admin/Dashboard");
            }
            else if (sessionRole == "Accountant")
            {
                return Redirect("~/Accountant/Dashboard");
            }

            return Redirect("~/Admin/Dashboard");
        }

        public ActionResult AccountRecovery()
        {
            var v = new AccountRecoveryModel();
            return View(v);
        }

        [HttpPost]
        public ActionResult AccountRecovery(AccountRecoveryModel model)
        {
            ErrorController ErrorFinder = new ErrorController();
            List<CreateUser> user;



            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                user = db.Query<CreateUser>("Select * from dbo.UserTable where Username = @Username AND Email = @Email;",
                    new { Username = model.Username, Email = model.Email }).ToList();
            }


            if (user.Count() == 0)
                ViewBag.Message = ErrorFinder.GetErrorMessage(35);
            else if (user[0].AccountLocked == false)
                ViewBag.Message = ErrorFinder.GetErrorMessage(36);
            else
            {
                //store in session variables the username and email
                System.Web.HttpContext.Current.Session["Username"] = user[0].Username;
                System.Web.HttpContext.Current.Session["Email"] = user[0].Email;

                //    //go to security questions page for answerings and unlocking

                return Redirect("~/Account/AnswerQuestions");
            }

            //Database1Entities5 db = new Database1Entities5();
            //var userDetails = db.CreateUsers.Where(validUser => validUser.Username == model.Username && validUser.Email == model.Email).FirstOrDefault();

            //if (userDetails == null)
            //{

            //    ViewBag.Message = ErrorFinder.GetErrorMessage(35);
            //}
            //else if (userDetails.Account_Locked == false)
            //{
            //    ViewBag.Message = ErrorFinder.GetErrorMessage(36);
            //}
            //else
            //{
            //    //store in session variables the username and email
            //    System.Web.HttpContext.Current.Session["Username"] = userDetails.Username;
            //    System.Web.HttpContext.Current.Session["Email"] = userDetails.Email;

            //    //go to security questions page for answerings and unlocking

            //    return Redirect("~/Account/AnswerQuestions");
            //}

            return View();
        }

        public ActionResult AnswerQuestions()
        {
            var CustomView = new AnswerQuestionsModel();
            var sessionUser = Session["Username"] as string;
            var sessionEmail = Session["Email"] as string;

            List<CreateUser> user;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                user = db.Query<CreateUser>("Select * from dbo.UserTable where Username = @Username AND Email = @Email;",
                    new { Username = sessionUser, Email = sessionEmail }).ToList();
            }
            //Database1Entities5 db = new Database1Entities5();
            //var userDetails = db.CreateUsers.Where(validUser => validUser.Username == sessionUser && validUser.Email == sessionEmail).FirstOrDefault();

            ViewBag.Question_1 = user[0].SecurityQuestion1;
            ViewBag.Question_2 = user[0].SecurityQuestion2;

            return View(CustomView);
        }

        [HttpPost]
        public ActionResult AnswerQuestions(AnswerQuestionsModel model)
        {
            EventLogHandler Logger = new EventLogHandler();
            ErrorController ErrorFinder = new ErrorController();

            var sessionUser = Session["Username"] as string;
            var sessionEmail = Session["Email"] as string;

            List<CreateUser> user;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {


                user = db.Query<CreateUser>("Select * from dbo.UserTable where Username = @Username AND Email = @Email;",
                    new { Username = sessionUser, Email = sessionEmail }).ToList();
            }
            ViewBag.Question_1 = user[0].SecurityQuestion1;
            ViewBag.Question_2 = user[0].SecurityQuestion2;
            //Database1Entities5 db = new Database1Entities5();
            //var userDetails = db.CreateUsers.Where(validUser => validUser.Username == sessionUser && validUser.Email == sessionEmail).FirstOrDefault();

            //ViewBag.Question_1 = userDetails.Security_Question1;
            //ViewBag.Question_2 = userDetails.Security_Question2;
            if (model.Answer_1 == user[0].Answer1 && model.Answer_2 == user[0].Answer2)
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = $"Update dbo.UserTable set AccountLocked = @AccountLocked where Username = @Username;";
                    db.Execute(sql, new { AccountLocked = false, Username = user[0].Username });
                }
                //user[0].Account_Locked = false;
                //db.SaveChanges();

                Logger.LogAccountRecovered(user[0].ID, user[0].Username);
                //Database1Entities6 db2 = new Database1Entities6();
                //var events = db2.EventLogs.ToList();
                ViewBag.Message = "Account Unlocked Successfully.";
            }
            else
            {
                ViewBag.Error = ErrorFinder.GetErrorMessage(37);
            }

            //if (model.Answer_1 == userDetails.Answer_1 && model.Answer_2 == userDetails.Answer_2) {
            //    userDetails.Account_Locked = false;
            //    db.SaveChanges();

            //    Logger.LogAccountRecovered(userDetails.ID, userDetails.Username);
            //    Database1Entities6 db2 = new Database1Entities6();
            //    var events = db2.EventLogs.ToList();
            //    ViewBag.Message = "Account Unlocked Successfully.";
            //}
            //else
            //{
            //    ViewBag.Error = ErrorFinder.GetErrorMessage(37);
            //}



            return View();
        }
    }
}
