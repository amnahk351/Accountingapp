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
        public int ID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter First Name.")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Last Name.")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Email Address.")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Username.")]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Password.")]
        [MinLength(6, ErrorMessage = "Minimum of 6 Characters is Required.")]
        public string Password { get; set; }

        ////DOES NOT WORK BECAUSE CONFIRM PASSWORD IS NOT IN DATABASE
        ////IT WANTS TO SAVE CONFIRMPASSWORD IN DATABASE BUT IT DOES NOT EXIST
        ////[DataType(DataType.Password)]
        //[NotMapped] //Attempted fix
        //[Compare("Password", ErrorMessage = "Confirm Password and Password do not match.")]  //This line specifically crashes it
        public string ConfirmPassword { get; set; }


        public string Role { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Phone Number.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please Select a Date.")]
        public System.DateTime Date { get; set; }


        public bool Active { get; set; }


        //[Required(ErrorMessage = "Please Select a Date.")]   //TURNING THIS ON MESSES IT UP ON NEW USER PAGE
        //[DataType(DataType.Date)]
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
