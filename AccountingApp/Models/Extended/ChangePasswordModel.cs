using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    [Validator(typeof(ChangePasswordValidator))]
    public class ChangePasswordModel
    {
        [DisplayName("Current Password")]
        public string CurrentPassword { get; set; }

        [DisplayName("New Password")]
        public string NewPassword { get; set; }

        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordModel>
    {
        ErrorController ErrorFinder = new ErrorController();
        string User = (string)HttpContext.Current.Session["Username"];

        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(22));
            RuleFor(x => x.CurrentPassword).Must(CheckCurrentPassword).WithMessage(ErrorFinder.GetErrorMessage(25));  //works on server side

            RuleFor(x => x.NewPassword).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(23)).MinimumLength(8).WithMessage(ErrorFinder.GetErrorMessage(13));
            RuleFor(x => x.NewPassword).Matches("^.*((?=.*[!@#$%^&*()\\-_=+{};:,<.>]){1})(?=.*\\d)((?=.*[a-z]){1})((?=.*[A-Z]){1}).*$").WithMessage(ErrorFinder.GetErrorMessage(28));
            RuleFor(x => x.NewPassword).NotEqual(x => x.CurrentPassword).WithMessage(ErrorFinder.GetErrorMessage(27));
            RuleFor(x => x.NewPassword).Must(CheckOldPassword).WithMessage(ErrorFinder.GetErrorMessage(26));  //works on server side

            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(24)).Equal(x => x.NewPassword).WithMessage(ErrorFinder.GetErrorMessage(24));                       
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
            else {            
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
                System.Diagnostics.Debug.WriteLine("current pass value: " + SavedString);
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
            System.Diagnostics.Debug.WriteLine("matches value: " + matches);
            return matches;
        }
    }
}