using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using AccountingApp.DBAccess;
using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    [Validator(typeof(NewUserValidator))]
    public class NewUserModel
    {
        public int ID { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }
        
        public string Email { get; set; }
        
        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; }

        public string Phone { get; set; }

        [DisplayName("Date")]
        public Nullable<System.DateTime> Date_Created { get; set; }

        [DisplayName("Allow Access")]
        public bool Active { get; set; }

        public string Old_Passwords { get; set; }
        
        public string Address { get; set; }
       
        public string City { get; set; }
        
        public string State { get; set; }

        [DisplayName("ZIP Code")]
        public string ZIP_Code { get; set; }
    }

    public class NewUserValidator : AbstractValidator<NewUserModel>
    {
        ErrorController ErrorFinder = new ErrorController();

        public NewUserValidator()
        {
            RuleFor(x => x.Date_Created).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(8));
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(1));
            RuleFor(x => x.LastName).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(2));

            RuleFor(x => x.Username).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(4));
            RuleFor(x => x.Username).Must(UsernameIsFound).WithMessage(ErrorFinder.GetErrorMessage(20));  //works on server side
            RuleFor(x => x.Password).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(23)).MinimumLength(8).WithMessage(ErrorFinder.GetErrorMessage(15));
            RuleFor(x => x.Password).Matches("^.*((?=.*[!@#$%^&*()\\-_=+{};:,<.>]){1})(?=.*\\d)((?=.*[a-z]){1})((?=.*[A-Z]){1}).*$").WithMessage(ErrorFinder.GetErrorMessage(28));
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(24)).Equal(x => x.Password).WithMessage(ErrorFinder.GetErrorMessage(24));

            RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(3));
            RuleFor(x => x.Email).EmailAddress().WithMessage(ErrorFinder.GetErrorMessage(29));
            RuleFor(x => x.Email).Must(EmailIsFound).WithMessage(ErrorFinder.GetErrorMessage(47));  //works on server side
            RuleFor(x => x.Phone).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(7)).Length(10, 10).WithMessage(ErrorFinder.GetErrorMessage(17)).Matches("^[0-9]*$").WithMessage(ErrorFinder.GetErrorMessage(16));
            RuleFor(x => x.Address).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(9));
            RuleFor(x => x.City).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(10));
            RuleFor(x => x.ZIP_Code).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(12)).Length(5, 5).WithMessage(ErrorFinder.GetErrorMessage(18)).Matches("^[0-9]*$").WithMessage(ErrorFinder.GetErrorMessage(16));

        }

        private bool UsernameIsFound(string UsernameChoosen)
        {            
            bool found = false;
            if (UsernameChoosen != null)
            {
                SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
                SqlCommand cmd = new SqlCommand("Select count(*) from UserTable where Username= @Username", con);
                //    SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
                //SqlCommand cmd = new SqlCommand("Select count(*) from CreateUsers where Username= @Username", con);
                cmd.Parameters.AddWithValue("@Username", UsernameChoosen);
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

        private bool EmailIsFound(string Email)
        {
            bool found = false;
            if (Email != null)
            {
                SqlConnection con = new SqlConnection(SqlAccess.GetConnectionString());
                SqlCommand cmd = new SqlCommand("Select count(*) from UserTable where Email= @Email", con);
                cmd.Parameters.AddWithValue("@Email", Email);
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