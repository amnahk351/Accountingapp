using AccountingApp.Controllers;
using AccountingApp.DBAccess;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    [Validator(typeof(NewAccountValidator))]
    public class NewAccountModel
    {
        public string AccountNumber { get; set; }

        public string AccountName { get; set; }

        public string AccountType { get; set; }

        public string OriginalBalance { get; set; }

        public string AccountDescription { get; set; }

        public bool Active { get; set; }
    }

    public class NewAccountValidator : AbstractValidator<NewAccountModel>
    {
        ErrorController ErrorFinder = new ErrorController();

        public NewAccountValidator()
        {
            RuleFor(x => x.AccountNumber).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(51));
            RuleFor(x => x.AccountName).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(52));
            RuleFor(x => x.AccountType).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(53));
            RuleFor(x => x.OriginalBalance).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(55));

            RuleFor(x => x.AccountNumber).Must(AccountNumberIsFound).WithMessage(ErrorFinder.GetErrorMessage(57));  //works on server side
            RuleFor(x => x.AccountName).Must(AccountNameIsFound).WithMessage(ErrorFinder.GetErrorMessage(58));  //works on server side            
        }

        private bool AccountNumberIsFound(string NumberChoosen)
        {
            bool found = false;
            if (NumberChoosen != null)
            {
                SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
                SqlCommand cmd = new SqlCommand("Select count(*) from dbo.ChartOfAccounts where AccountNumber = @Num", con);
                //    SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
                //SqlCommand cmd = new SqlCommand("Select count(*) from CreateUsers where Username= @Username", con);
                cmd.Parameters.AddWithValue("@Num", NumberChoosen);
                con.Open();
                int result = (int)cmd.ExecuteScalar();
                if (result != 0)
                {
                    found = true;
                }
                else
                {
                    found = false;
                }
                con.Close();
            }
            return !found;
        }

        private bool AccountNameIsFound(string NameChoosen)
        {
            bool found = false;
            if (NameChoosen != null)
            {
                SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
                SqlCommand cmd = new SqlCommand("Select count(*) from dbo.ChartOfAccounts where AccountName = @Nam", con);
                cmd.Parameters.AddWithValue("@Nam", NameChoosen);
                con.Open();
                int result = (int)cmd.ExecuteScalar();
                if (result != 0)
                {
                    found = true;
                }
                else
                {
                    found = false;
                }
                con.Close();
            }
            return !found;
        }
    }
}