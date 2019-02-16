using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class EditUserModel
    {
        public int ID { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.Date)]
        [Required]
        public Nullable<System.DateTime> Date_Modified { get; set; }

        [DisplayName("First Name")]
        [ValidField(ErrorMessageID = 1)]
        [Required]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [ValidField(ErrorMessageID = 2)]
        [Required]
        public string LastName { get; set; }

        [DisplayName("Email Address")]
        [DataType(DataType.EmailAddress)]
        [ValidField(ErrorMessageID = 3)]
        [Required]
        public string Email { get; set; }

        [ValidField(ErrorMessageID = 4)]       
        public string Username { get; set; }

        [ValidPassword]
        [Required]
        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; }

        [ValidField(ErrorMessageID = 7)]
        [Required]
        public string Phone { get; set; }

        [DisplayName("Allow Access")]
        public bool Active { get; set; }

        [ValidField(ErrorMessageID = 9)]
        [Required]
        public string Address { get; set; }

        [ValidField(ErrorMessageID = 10)]
        [Required]
        public string City { get; set; }

        public string State { get; set; }

        [DisplayName("ZIP Code")]
        [ValidField(ErrorMessageID = 12)]
        [Required]
        public string ZIP_Code { get; set; }
    }
}