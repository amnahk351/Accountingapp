using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class UserRequestsModel
    {
        public Nullable<System.DateTime> DateCreated { get; set; }
        public int AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string NormalSide { get; set; }
        public Nullable<decimal> OriginalBalance { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string Type { get; set; }        
    }
}