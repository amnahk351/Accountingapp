using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using AccountingApp.Controllers;
using System.Text.RegularExpressions;

namespace AccountingApp.Models
{
    public class ValidField : ValidationAttribute
    {
        public int ErrorMessageID { get; set; }
        ErrorController ErrorFinder = new ErrorController();

        protected override ValidationResult
                IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(ErrorMessageID));
            }
            else if (IsEmptyOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult
                    (ErrorFinder.GetErrorMessage(ErrorMessageID));
            }
            else
            {
                return ValidationResult.Success;
            }
        }
        private bool IsEmptyOrWhiteSpace(string value)
        {
            return value.All(char.IsWhiteSpace);
        }
    }
}