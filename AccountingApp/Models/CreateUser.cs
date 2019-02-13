//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AccountingApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class CreateUser
    {
        //public int ID { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string Email { get; set; }
        //public string Username { get; set; }
        //public string Password { get; set; }
        //public string Role { get; set; }
        //public string Phone { get; set; }
        //public Nullable<System.DateTime> Date { get; set; }
        //public bool Active { get; set; }
        //public Nullable<System.DateTime> Date_Modified { get; set; }
        //public string Old_Passwords { get; set; }
        //public string Address { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string ZIP_Code { get; set; }
                

        public int ID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter First Name.")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Last Name.")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Email Address.")]
        public string Email { get; set; }

        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Username.")]
        [ValidUsername]
        public string Username { get; set; }

        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Password.")]
        [ValidPassword]
        public string Password { get; set; }

        [NotMapped]
        [Compare(nameof(Password), ErrorMessage = "Confirm Password and Password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Phone Number.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please Select a Date.")]
        public Nullable<System.DateTime> Date { get; set; }


        public bool Active { get; set; }


        //[Required(ErrorMessage = "Please Select a Date.")]   //TURNING THIS ON MESSES IT UP ON NEW USER PAGE
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> Date_Modified { get; set; }


        public string Old_Passwords { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter an Address.")]
        public string Address { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a City.")]
        public string City { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a State.")]
        public string State { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter a ZIP.")]
        [MinLength(5, ErrorMessage = "Minimum of 5 Characters is allowed for ZIP.")]
        public string ZIP_Code { get; set; }
    }
}
