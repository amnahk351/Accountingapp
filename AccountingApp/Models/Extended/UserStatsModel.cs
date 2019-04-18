using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountingApp.Models.Extended
{
    public class UserStatsModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }        
        public Nullable<System.DateTime> LastLogin { get; set; }
        public Nullable<System.DateTime> LastSignout { get; set; }
        public Nullable<int> LoginAmount { get; set; }
        public Nullable<int> LoginFails { get; set; }        
    }
}