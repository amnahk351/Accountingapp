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
    
    public partial class EventLog
    {
        public int EventID { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> UserID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string IPAddress { get; set; }
        public string Screen { get; set; }
        public string Access_Level { get; set; }
        public string DetailedFrom { get; set; }
        public string DetailedTo { get; set; }
    }
}
