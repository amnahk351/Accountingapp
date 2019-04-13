using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountingApp.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using AccountingApp.DBAccess;

namespace AccountingApp.Controllers
{
    public class EventLogHandler
    {
        Database1Entities6 db = new Database1Entities6();

        //public void LogNewUser(string Username)
        //{
        //    var sessionUserID = HttpContext.Current.Session["UserID"] as string;
        //    string ip = HttpContext.Current.Request.UserHostAddress;

        //    EventLog model = new EventLog();
        //    model.Date = DateTime.Now;
        //    model.User_ID = Convert.ToInt32(sessionUserID);
        //    model.From = null;
        //    model.To = "User Created: " + Username;
        //    model.IP_Address = ip;
        //    model.Screen = "NewUser";
        //    model.Access_Level = "All";

        //    using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
        //    {

        //        string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
        //            "[From], [To], IPAddress, Screen, AccessLevel) values" +
        //            "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
        //        db.Execute(sql, new
        //        {

        //            Date = model.Date,
        //            UserID = model.User_ID,
        //            From = model.From,
        //            To = model.To,
        //            IPAddress = model.IP_Address,
        //            Screen = model.Screen,
        //            AccessLevel = model.Access_Level

        //        });
        //    }
        //    //db.EventLogs.Add(model);
        //    //db.SaveChanges();
        //}

        private int FindUserId(string Username) {
            List<CreateUser> user;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                user = db.Query<CreateUser>("Select * from dbo.UserTable where Username = @username;",
                    new { username = Username }).ToList();
            }

            return user[0].ID;
        }

        public void LogEditUser(int UserId, string Username, string Original, string Updated)
        {
            var sessionUserID = HttpContext.Current.Session["UserID"] as string;
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.UserID = UserId;
            model.From = Original;
            model.To = Updated;
            model.IPAddress = ip;
            model.Screen = "EditUser";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }

            //db.EventLogs.Add(model);
            //db.SaveChanges();
        }

        public void LogForgotPassword(string Email)
        {
            Database1Entities5 db2 = new Database1Entities5();

            List<CreateUser> user;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                

                user = db.Query<CreateUser>("Select * from dbo.UserTable where Email = @Email;", new { Email }).ToList();
            }

            //var user = db2.CreateUsers.Where(x => x.Email == Email).FirstOrDefault();

            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.UserID = Convert.ToInt32(user[0].ID);

            //model.User_ID = Convert.ToInt32(user.ID);
            model.From = "";
            model.To = "Requested Password Reset Link to: " + Email;
            model.IPAddress = ip;
            model.Screen = "ForgotPassword";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }
            //db.EventLogs.Add(model);
            //db.SaveChanges();
        }

        public void LogPasswordReset(int UserID, string Username)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.UserID = UserID;
            model.From = null;
            model.To = "Password Reset for: " + Username;
            model.IPAddress = ip;
            model.Screen = "ResetPassword";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }
            //db.EventLogs.Add(model);
            //db.SaveChanges();
        }

        public void LogPasswordChange()
        {
            var sessionUserID = HttpContext.Current.Session["UserID"] as string;
            var sessionUsername = HttpContext.Current.Session["Username"] as string;
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.UserID = Convert.ToInt32(sessionUserID);
            model.From = null;
            model.To = "Password Changed for: " + sessionUsername;
            model.IPAddress = ip;
            model.Screen = "ChangePassword";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }
            //db.EventLogs.Add(model);
            //db.SaveChanges();
        }

        public void LogAccountLocked(int UserID, string Username)  //When user fails login 10 times
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.UserID = UserID;
            model.From = "Account Unlocked for: " + Username;
            model.To = "Account Locked for: " + Username;
            model.IPAddress = ip;
            model.Screen = "Login";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }
            //db.EventLogs.Add(model);
            //db.SaveChanges();
        }

        public void LogAccountRecovered(int UserId, string Username)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.UserID = UserId;
            model.From = "Account Locked for: " + Username;
            model.To = "Account Unlocked for: " + Username;
            model.IPAddress = ip;
            model.Screen = "AnswerQuestions";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }

            //db.EventLogs.Add(model);
            //db.SaveChanges();
        }

        public void LogJournalEntrySubmitted(string Username, string EntryID, string type)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;
            EventLog model = new EventLog();
            

            model.Date = DateTime.Now;
            model.UserID = FindUserId(Username);
            model.From = "";
            model.To = Username + " " + type + " Journal Entry, " + EntryID;
            model.IPAddress = ip;
            model.Screen = "Journalize";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }            
        }


        public void LogEditedJournalEntry(string Username, string EntryID, string type)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;
            EventLog model = new EventLog();


            model.Date = DateTime.Now;
            model.UserID = FindUserId(Username);
            model.From = "";
            model.To = Username + " " + type + " Journal Entry, " + EntryID;
            model.IPAddress = ip;
            model.Screen = "EditJournal";
            model.Access_Level = "All";

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                string sql = $"Insert into dbo.EventLogTable (Date, UserID, " +
                    "[From], [To], IPAddress, Screen, AccessLevel) values" +
                    "(@Date, @UserID, @From, @To, @IPAddress, @Screen,@AccessLevel)";
                db.Execute(sql, new
                {

                    Date = model.Date,
                    UserID = model.UserID,
                    From = model.From,
                    To = model.To,
                    IPAddress = model.IPAddress,
                    Screen = model.Screen,
                    AccessLevel = model.Access_Level

                });
            }
        }

    }
}