using AccountingApp.Controllers;
using AccountingApp.DBAccess;
using Dapper;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    [Validator(typeof(EditAccountValidator))]
    public class EditAccountModel
    {
        public int AccountNumber { get; set; }

        public string AccountName { get; set; }

        public string AccountType { get; set; }

        public string AccountDescription { get; set; }

        public bool Active { get; set; }
    }

    public class EditAccountValidator : AbstractValidator<EditAccountModel>
    {
        ErrorController ErrorFinder = new ErrorController();

        public EditAccountValidator()
        {            
            RuleFor(x => x.AccountName).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(52));
            
            RuleFor(x => x.AccountName).Must(AccountNameIsFound).WithMessage(ErrorFinder.GetErrorMessage(58));  //works on server side
            
        }

        private bool AccountNameIsFound(string NameChoosen)
        {
            bool found = false;
            if (NameChoosen != null)
            {
                string path_and_query = HttpContext.Current.Request.Url.PathAndQuery;
                string[] segments = path_and_query.Split('/');
                string AccountNumber = segments[3];


                List<ChartOfAcc> currentAccount;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    currentAccount = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountNumber = @Num", new { Num = AccountNumber }).ToList();
                }

                string CurrentAccountName = currentAccount[0].AccountName;

                List<ChartOfAcc> editAccount;
                using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
                {
                    editAccount = db.Query<ChartOfAcc>($"Select * from dbo.ChartOfAccounts Where AccountName != @Nam", new { Nam = CurrentAccountName }).ToList();
                }

                var list = new List<string>();

                // Fill the list
                foreach (ChartOfAcc c in editAccount)
                {
                    list.Add(c.AccountName);
                }

                list.Add(NameChoosen);
                
                if (list.Count != list.Distinct().Count())
                {
                    // Duplicates exist
                    found = true;                    
                }                

            }
            return !found;
        }
    }
}