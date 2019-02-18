using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "The Email Address Field is Required.")]
        [DisplayName("Email:")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}