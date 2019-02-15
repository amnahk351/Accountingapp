using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using AccountingApp.Controllers;
using System.Data.SqlClient;
using System.Diagnostics;

namespace AccountingApp.Models
{
    public class ValidUsername : ValidationAttribute
    {
        ErrorController ErrorFinder = new ErrorController();

        protected override ValidationResult
                IsValid(object value, ValidationContext validationContext)
        {
            

            if (value == null) {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(4));
            }

            //string Username = value.ToString();

            else if (UsernameIsFound(value.ToString()) == true)
            {               
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(20));
            }
            else
            {
                return ValidationResult.Success;
            }
        }

        private bool UsernameIsFound(string UsernameChoosen) {
            bool found = false;
            SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            SqlCommand cmd = new SqlCommand("Select count(*) from CreateUsers where Username= @Username", con);
            cmd.Parameters.AddWithValue("@Username", UsernameChoosen);
            con.Open();
            int result = (int)cmd.ExecuteScalar();
            if (result != 0)
            {
                found = true;
            }
            else {
                found = false;
            }
            con.Close();

            return found;
        }
    }
}