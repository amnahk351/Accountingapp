using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class ResetPasswordModel
    {
        [Required]
        [ValidPassword]
        [DisplayName("New Password")]
        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ResetCode { get; set; }
    }
}