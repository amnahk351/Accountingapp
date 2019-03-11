using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class UserModel
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public Nullable<System.DateTime> Date_Created { get; set; }
        public bool Active { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public string OldPasswords { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP_Code { get; set; }
        public string ResetPasswordCode { get; set; }
        public Nullable<int> LoginAmount { get; set; }
        public Nullable<int> LoginFails { get; set; }
        public Nullable<bool> AccountLocked { get; set; }
        public Nullable<System.DateTime> LastSignout { get; set; }
        public string SecurityQuestion1 { get; set; }
        public string Answer1 { get; set; }
        public string SecurityQuestion { get; set; }
        public string Answer2 { get; set; }
        public Nullable<short> LoginAttempts { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }
        public Nullable<int> TotalOnlineMins { get; set; }
    }
}