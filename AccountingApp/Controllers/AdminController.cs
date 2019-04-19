using AccountingApp.Models;
using AccountingApp.DBAccess;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

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


        public ActionResult AccountRequests() {
            List<UserRequestsModel> items;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                items = db.Query<UserRequestsModel>($"Select * from dbo.UserRequestsTable").ToList();
            }
            return View(items);
        }

        public ActionResult UserStatistics()
        {
            List<UserStatsModel> stats = new List<UserStatsModel>();

            List<CreateUser> listUser;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                listUser = db.Query<CreateUser>($"Select * from dbo.UserTable").ToList();
            }

            for (int i = 0; i < listUser.Count; i++) {
                UserStatsModel mod = new UserStatsModel();
                mod.ID = listUser[i].ID;
                mod.Username = listUser[i].Username;
                mod.Date = listUser[i].Date;
                mod.DateModified = listUser[i].DateModified;
                mod.LastLogin = listUser[i].LastLogin;
                mod.LastSignout = listUser[i].LastSignout;
                mod.LoginAmount = listUser[i].LoginAmount;
                mod.LoginFails = listUser[i].LoginFails;


                stats.Add(mod);
            }

            return View(stats);
        }


        public ActionResult NewAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewAccount(ChartOfAcc model)
        {
            var sessionUser = Session["Username"] as string;

            ChartOfAcc tb2 = new ChartOfAcc();
            tb2.AccountNumber = model.AccountNumber;
            tb2.AccountName = model.AccountName;
            tb2.AccountType = model.AccountType;
            tb2.NormalSide = model.NormalSide;
            tb2.OriginalBalance = model.OriginalBalance;
            tb2.CurrentBalance = model.CurrentBalance;
            tb2.AccountDescription = model.AccountDescription;
            tb2.CreatedBy = sessionUser;
            tb2.Active = model.Active;
            

            if (ModelState.IsValid)
            {
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = $"Insert into dbo.ChartOfAccounts (AccountNumber, AccountName, " +
                        "AccountType, NormalSide, OriginalBalance, CurrentBalance, AccountDescription, CreatedBy, Active, DateCreated)" +
                        "values(@AccountNumber, @AccountName, @AccountType,@NormalSide,@OriginalBalance," +
                        "@CurrentBalance,@AccountDescription,@CreatedBy,@Active,@Date)";
                    db.Execute(sql, new
                    {

                        AccountNumber = model.AccountNumber,
                        AccountName = model.AccountName,
                        AccountType = model.AccountType,
                        NormalSide = model.NormalSide,
                        OriginalBalance = model.OriginalBalance,
                        CurrentBalance = model.CurrentBalance,
                        AccountDescription = model.AccountDescription,
                        CreatedBy = sessionUser,
                        Active = model.Active,
                        Date = DateTime.Now
                    });
                }

            //    if (ModelState.IsValid)
            //{
            //    db.ChartOfAccs.Add(tb2);

            //    db.SaveChanges();
            //    var item = db.ChartOfAccs.ToList();
                TempData["Message"] = "A new account was successfully created !";


                return RedirectToAction("ChartOfAccounts");
            }

            return View("NewAccount", new ChartOfAcc());
        }
       
     

        
        public ActionResult ChartOfAccounts()
        {
            List<ChartOfAcc> listAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {                
                listAccounts = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts").ToList();
            }
            return View(listAccounts);
        }


        public ActionResult EditAccount(double id)
        {
            List<ChartOfAcc> editAccount;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                editAccount = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountNumber = @ID", new { ID = id }).ToList();
            }
            //return View(editAccount);
            return View(editAccount[0]);
            //var item = db.ChartOfAccs.Where(x => x.AccountNumber == id).First();
            //return View(item);
        }

        [HttpPost]
        public ActionResult EditAccount(ChartOfAcc model)
        {
            if (ModelState.IsValid)
            {
                List<ChartOfAcc> CurrentAccount;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    CurrentAccount = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountNumber = @ID", new { ID = model.AccountNumber }).ToList();
                }             

                CurrentAccount[0].AccountNumber = model.AccountNumber;
                CurrentAccount[0].AccountName = model.AccountName;
                CurrentAccount[0].AccountType = model.AccountType;
                CurrentAccount[0].NormalSide = model.NormalSide;
                CurrentAccount[0].OriginalBalance = model.OriginalBalance;
                CurrentAccount[0].AccountDescription = model.AccountDescription;
                CurrentAccount[0].Active = model.Active;
                CurrentAccount[0].CreatedBy = model.CreatedBy;
                CurrentAccount[0].CurrentBalance = model.CurrentBalance;

                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {

                    string sql = "Update dbo.ChartOfAccounts set AccountName = @AccountName, " +
                        "AccountType = @AccountType, AccountDescription = @AccountDescription," +
                        "Active = @Active Where AccountNumber = @AccountNumber";


                    db.Execute(sql, new
                    {
                        AccountNumber = CurrentAccount[0].AccountNumber,
                        AccountName = CurrentAccount[0].AccountName,
                        AccountType = CurrentAccount[0].AccountType,
                        AccountDescription = CurrentAccount[0].AccountDescription,
                        Active = CurrentAccount[0].Active


                    });
                }
                //if (ModelState.IsValid)
                //{
                //    var item = db.ChartOfAccs.Where(x => x.AccountNumber == model.AccountNumber).First();

                //    item.AccountNumber = model.AccountNumber;
                //    item.AccountName = model.AccountName;
                //    item.AccountType = model.AccountType;
                //    item.NormalSide = model.NormalSide;
                //    item.OriginalBalance = model.OriginalBalance;
                //    item.AccountDescription = model.AccountDescription;
                //    item.Active = model.Active;
                //    item.CreatedBy = model.CreatedBy;
                //    item.CurrentBalance = model.CurrentBalance;


                //    db.SaveChanges();
                //    var item2 = db.ChartOfAccs.ToList();
                TempData["Message"] = "Your entry was successfully updated!";

                return RedirectToAction("ChartOfAccounts");
            
            }
                return View(model);
        }


        public ActionResult Delete(int id)
        {
            List<ChartOfAcc> remainingAccounts;
            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                string sql = $"Delete from dbo.Usertable where ID = @ID";

                db.Execute(sql, new { ID = id });
                remainingAccounts = db.Query<ChartOfAcc>($"Select * from dbo.Usertable").ToList();

            }
            return View("ChartOfAccounts", remainingAccounts);
            //var item = db.ChartOfAccs.Where(x => x.AccountNumber == id).First();
            //db.ChartOfAccs.Remove(item);
            //db.SaveChanges();
            //var item2 = db.ChartOfAccs.ToList();
            //return View("ChartOfAccounts", item2);
        }
    }
}
