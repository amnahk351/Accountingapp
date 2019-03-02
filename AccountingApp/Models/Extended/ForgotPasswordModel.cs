using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    [Validator(typeof(ForgotPasswordValidator))]
    public class ForgotPasswordModel
    {
        [DisplayName("Email:")]        
        public string Email { get; set; }
    }

    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordModel>
    {
        ErrorController ErrorFinder = new ErrorController();

        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(3));
            RuleFor(x => x.Email).EmailAddress().WithMessage(ErrorFinder.GetErrorMessage(29));
        }
    }
}