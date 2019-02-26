using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AccountingApp.Models
{
    [Validator(typeof(EditUserValidator))]
    public class EditUserModel
    {
        public int ID { get; set; }

        [DisplayName("Date")]
        //[DataType(DataType.Date)]
        public DateTime? Date_Modified { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Email Address")]
        public string Email { get; set; }
              
        public string Username { get; set; }

        public string Role { get; set; }
   
        public string Phone { get; set; }

        [DisplayName("Allow Access")]
        public bool Active { get; set; }

        public string Address { get; set; }
     
        public string City { get; set; }

        public string State { get; set; }

        [DisplayName("ZIP Code")]
        public string ZIP_Code { get; set; }
    }

    public class EditUserValidator : AbstractValidator<EditUserModel>
    {
        ErrorController ErrorFinder = new ErrorController();

        public EditUserValidator()
        {
            RuleFor(x => x.Date_Modified).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(8));
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(1));
            RuleFor(x => x.LastName).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(2));
            RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(3));
            RuleFor(x => x.Email).EmailAddress().WithMessage(ErrorFinder.GetErrorMessage(29));
            RuleFor(x => x.Phone).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(7));
            RuleFor(x => x.Address).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(9));
            RuleFor(x => x.City).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(10));
            RuleFor(x => x.ZIP_Code).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(12));

        }        
    }
}