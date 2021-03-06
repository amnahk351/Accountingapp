﻿using AccountingApp.Controllers;
using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    [Validator(typeof(AnswerQuestionsValidator))]
    public class AnswerQuestionsModel
    {
        public string Security_Question1 { get; set; }

        [DisplayName("Answer")]
        public string Answer_1 { get; set; }

        public string Security_Question2 { get; set; }

        [DisplayName("Answer")]
        public string Answer_2 { get; set; }

    }

    public class AnswerQuestionsValidator : AbstractValidator<AnswerQuestionsModel>
    {
        ErrorController ErrorFinder = new ErrorController();

        public AnswerQuestionsValidator()
        {
            RuleFor(x => x.Answer_1).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(32));

            RuleFor(x => x.Answer_2).NotEmpty().WithMessage(ErrorFinder.GetErrorMessage(33));
        }
    }
}