using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AccountingApp.Models;

namespace AccountingApp.Controllers
{
    public class EventLogHandler
    {
        Database1Entities6 db = new Database1Entities6();

        public void LogNewUser(string Username)
        {
            var sessionUserID = HttpContext.Current.Session["UserID"] as string;
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.User_ID = Convert.ToInt32(sessionUserID);
            model.From = null;
            model.To = "User Created: " + Username;
            model.IP_Address = ip;
            model.Screen = "NewUser";
            model.Access_Level = "All";

            db.EventLogs.Add(model);
            db.SaveChanges();
        }

        public void LogEditUser(int UserId, string Username, string Original, string Updated)
        {
            var sessionUserID = HttpContext.Current.Session["UserID"] as string;
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.User_ID = UserId;
            model.From = Original;
            model.To = Updated;
            model.IP_Address = ip;
            model.Screen = "Edit";
            model.Access_Level = "All";

            db.EventLogs.Add(model);
            db.SaveChanges();
        }

        public void LogForgotPassword(string Email)
        {
            Database1Entities5 db2 = new Database1Entities5();

            var user = db2.CreateUsers.Where(x => x.Email == Email).FirstOrDefault();

            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.User_ID = Convert.ToInt32(user.ID);
            model.From = "";
            model.To = "Requested Password Reset Link to: " + Email;
            model.IP_Address = ip;
            model.Screen = "ForgotPassword";
            model.Access_Level = "All";           

            db.EventLogs.Add(model);
            db.SaveChanges();
        }

        public void LogPasswordReset(int UserID, string Username)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.User_ID = UserID;
            model.From = null;
            model.To = "Password Reset for: " + Username;
            model.IP_Address = ip;
            model.Screen = "ResetPassword";
            model.Access_Level = "All";

            db.EventLogs.Add(model);
            db.SaveChanges();
        }

        public void LogPasswordChange()
        {
            var sessionUserID = HttpContext.Current.Session["UserID"] as string;
            var sessionUsername = HttpContext.Current.Session["Username"] as string;
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.User_ID = Convert.ToInt32(sessionUserID);
            model.From = null;
            model.To = "Password Changed for: " + sessionUsername;
            model.IP_Address = ip;
            model.Screen = "ChangePassword";
            model.Access_Level = "All";

            db.EventLogs.Add(model);
            db.SaveChanges();
        }

        public void LogAccountLocked(int UserID, string Username)  //When user fails login 10 times
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.User_ID = UserID;
            model.From = "Account Unlocked for: " + Username;
            model.To = "Account Locked for: " + Username;
            model.IP_Address = ip;
            model.Screen = "Login";
            model.Access_Level = "All";

            db.EventLogs.Add(model);
            db.SaveChanges();
        }

        public void LogAccountRecovered(int UserId, string Username)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            EventLog model = new EventLog();
            model.Date = DateTime.Now;
            model.User_ID = UserId;
            model.From = "Account Locked for: " + Username;
            model.To = "Account Unlocked for: " + Username;
            model.IP_Address = ip;
            model.Screen = "AnswerQuestions";
            model.Access_Level = "All";

            db.EventLogs.Add(model);
            db.SaveChanges();
        }
    }
}