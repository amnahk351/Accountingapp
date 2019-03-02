using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    [Validator(typeof(AccountRecoveryValidator))]
    public class AccountRecoveryModel
    {
        public string Username { get; set; }

        public string Email { get; set; }
    }

    public class AccountRecoveryValidator : AbstractValidator<AccountRecoveryModel>
    {
        ErrorController ErrorFinder = new ErrorController();

        public AccountRecoveryValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(4));

            RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(3)).EmailAddress().WithMessage(ErrorFinder.GetErrorMessage(29));

            //check if an account exists


            //check if account is locked
        }
    }
}