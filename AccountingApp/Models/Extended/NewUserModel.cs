using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingApp.Models
{
    public class NewUserModel
    {
        public int ID { get; set; }

        [ValidField(ErrorMessageID = 1)]
        public string FirstName { get; set; }

        [ValidField(ErrorMessageID = 2)]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [ValidField(ErrorMessageID = 3)]
        public string Email { get; set; }


        [ValidField(ErrorMessageID = 4)]
        [ValidUsername]
        public string Username { get; set; }


        [ValidPassword]
        public string Password { get; set; }

        [NotMapped]
        //[Compare(nameof(Password), ErrorMessage = "Confirm Password and Password do not match.")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; }

        [ValidField(ErrorMessageID = 7)]
        public string Phone { get; set; }

        [ValidField(ErrorMessageID = 8)]
        public Nullable<System.DateTime> Date { get; set; }


        public bool Active { get; set; }

        public string Old_Passwords { get; set; }

        [ValidField(ErrorMessageID = 9)]
        public string Address { get; set; }

        [ValidField(ErrorMessageID = 10)]
        public string City { get; set; }
        
        public string State { get; set; }

        [ValidField(ErrorMessageID = 12)]
        public string ZIP_Code { get; set; }
    }
}