using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

namespace AccountingApp.Models
{
    [Validator(typeof(NewUserValidator))]
    public class NewUserModel
    {
        public int ID { get; set; }

        //[ValidField(ErrorMessageID = 1)]
        public string FirstName { get; set; }

        //[ValidField(ErrorMessageID = 2)]
        public string LastName { get; set; }

        //[DataType(DataType.EmailAddress)]
        //[ValidField(ErrorMessageID = 3)]
        public string Email { get; set; }


        //[ValidField(ErrorMessageID = 4)]
        //[ValidUsername]
        public string Username { get; set; }


        //[ValidPassword]
        public string Password { get; set; }

        //[NotMapped]
        //[Compare(nameof(Password), ErrorMessage = "Confirm Password and Password do not match.")]
        //[Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; }

        //[ValidField(ErrorMessageID = 7)]
        public string Phone { get; set; }

        //[ValidField(ErrorMessageID = 8)]
        public Nullable<System.DateTime> Date_Created { get; set; }


        public bool Active { get; set; }

        public string Old_Passwords { get; set; }

        //[ValidField(ErrorMessageID = 9)]
        public string Address { get; set; }

        //[ValidField(ErrorMessageID = 10)]
        public string City { get; set; }
        
        public string State { get; set; }

        //[ValidField(ErrorMessageID = 12)]
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
            RuleFor(x => x.Password).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(23)).MinimumLength(8).WithMessage(ErrorFinder.GetErrorMessage(13));
            RuleFor(x => x.Password).Matches("^.*((?=.*[!@#$%^&*()\\-_=+{};:,<.>]){1})(?=.*\\d)((?=.*[a-z]){1})((?=.*[A-Z]){1}).*$").WithMessage(ErrorFinder.GetErrorMessage(28));
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(24)).Equal(x => x.Password).WithMessage(ErrorFinder.GetErrorMessage(24));

            RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(3));
            RuleFor(x => x.Email).EmailAddress().WithMessage(ErrorFinder.GetErrorMessage(29));
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
                SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            SqlCommand cmd = new SqlCommand("Select count(*) from CreateUsers where Username= @Username", con);
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
    }
}