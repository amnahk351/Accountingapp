using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using AccountingApp.DBAccess;
using Dapper;
using System.Data;

namespace AccountingApp.Models
{
    [Validator(typeof(ResetPasswordValidator))]
    public class ResetPasswordModel
    {        
        [DisplayName("New Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
                
        public string ResetCode { get; set; }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
    {
        ErrorController ErrorFinder = new ErrorController();
        string User = (string)HttpContext.Current.Session["Username"];

        public ResetPasswordValidator()
        {
            RuleFor(x => x.Password).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(23)).MinimumLength(8).WithMessage(ErrorFinder.GetErrorMessage(15));
            RuleFor(x => x.Password).Matches("^.*((?=.*[!@#$%^&*()\\-_=+{};:,<.>]){1})(?=.*\\d)((?=.*[a-z]){1})((?=.*[A-Z]){1}).*$").WithMessage(ErrorFinder.GetErrorMessage(28));
            RuleFor(x => x.Password).Must(CheckNewPassword).WithMessage(ErrorFinder.GetErrorMessage(26)); //works on server side, don't tell user current password

            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(24)).Equal(x => x.Password).WithMessage(ErrorFinder.GetErrorMessage(24));

            RuleFor(x => x.ResetCode).NotEmpty();
        }


        private bool CheckNewPassword(string Password)
        {
            string path_and_query = HttpContext.Current.Request.Url.PathAndQuery;
            string[] segments = path_and_query.Split('/');

            string code = segments[(int)segments.Length - 1];

            bool matches = false;


            List<UserModel> userDetails;

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {
                userDetails = db.Query<UserModel>("Select * from dbo.UserTable Where ResetPasswordCode = @Code", new { Code = code }).ToList();
            }

            if (Password == null) {
                return true;
            }

            if (userDetails[0].Password == Password)
            {
                matches = true;
            }
            else if (userDetails[0].OldPasswords.Contains(Password))
            {
                matches = true;
            }
            else {
                matches = false;
            }


            return !matches;
        }
    }
}