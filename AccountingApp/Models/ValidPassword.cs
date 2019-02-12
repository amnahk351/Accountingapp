using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using AccountingApp.Controllers;

namespace AccountingApp.Models
{
    public class ValidPassword : ValidationAttribute
    {
        ErrorController ErrorFinder = new ErrorController();

        protected override ValidationResult
                IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(5));
            }

            string pass = value.ToString();

            bool hasUpperCaseLetter = false;
            bool hasLowerCaseLetter = false;
            bool hasDecimalDigit = false;

            foreach (char c in pass)
            {
                if (char.IsUpper(c))
                {
                    hasUpperCaseLetter = true;
                }
                else if (char.IsLower(c))
                {
                    hasLowerCaseLetter = true;
                }
                else if (char.IsDigit(c))
                {
                    hasDecimalDigit = true;
                }
            }

            if (pass == null || pass == "")
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(4));
            }

            else if (pass.Length <= 8)
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(13));
            }

            else if (HasSpecialChars(pass) == false)
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(15));
            }


            else if (hasUpperCaseLetter == false)
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(17));
            }

            else if (hasLowerCaseLetter == false)
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(16));
            }

            else if (hasDecimalDigit == false)
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(14));
            }

            else
            {
                return ValidationResult.Success;
            }
        }

        private Boolean HasSpecialChars(string password)
        {
            return password.Any(ch => !Char.IsLetterOrDigit(ch));
        }
    }
}