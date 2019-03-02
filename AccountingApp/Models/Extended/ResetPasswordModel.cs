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

        [Required]
        public string ResetCode { get; set; }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
    {
        ErrorController ErrorFinder = new ErrorController();
        string User = (string)HttpContext.Current.Session["Username"];

        public ResetPasswordValidator()
        {
            RuleFor(x => x.Password).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(23)).MinimumLength(8).WithMessage(ErrorFinder.GetErrorMessage(13));
            RuleFor(x => x.Password).Matches("^.*((?=.*[!@#$%^&*()\\-_=+{};:,<.>]){1})(?=.*\\d)((?=.*[a-z]){1})((?=.*[A-Z]){1}).*$").WithMessage(ErrorFinder.GetErrorMessage(28));            
            RuleFor(x => x.Password).Must(CheckOldPassword).WithMessage(ErrorFinder.GetErrorMessage(26));  //works on server side
            RuleFor(x => x.Password).Must(CheckCurrentPassword).WithMessage(ErrorFinder.GetErrorMessage(26));  //works on server side

            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(24)).Equal(x => x.Password).WithMessage(ErrorFinder.GetErrorMessage(24));
        }

        private bool CheckOldPassword(string Password)
        {
            bool found = false;
            SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            SqlCommand cmd = new SqlCommand("Select Old_Passwords from CreateUsers where Username = @User", con);
            cmd.Parameters.AddWithValue("@User", User);
            con.Open();
            var nullableValue = cmd.ExecuteScalar();
            if (nullableValue == null || nullableValue == DBNull.Value)
            {
                return !found;
            }
            else
            {
                var SavedString = (string)cmd.ExecuteScalar();
                if (SavedString.Contains(Password))
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

        private bool CheckCurrentPassword(string Password)
        {
            bool matches = false;
            SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            SqlCommand cmd = new SqlCommand("Select Password from CreateUsers where Username = @User", con);
            cmd.Parameters.AddWithValue("@User", User);
            con.Open();
            var nullableValue = cmd.ExecuteScalar();
            if (nullableValue == null || nullableValue == DBNull.Value)
            {
                return matches;
            }
            else
            {
                var SavedString = (string)cmd.ExecuteScalar();
                if (SavedString == Password)
                {
                    matches = true;
                }
                else
                {
                    matches = false;
                }
                con.Close();
            }
            return matches;
        }
    }
}