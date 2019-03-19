using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class Entries
    {
        public List<Entry> entries { get; set; }

        public Entries()
        {
            entries = new List<Entry>();
        }
    }
}