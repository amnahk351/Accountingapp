using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class Entry
    {
        public List<Transaction> transactions { get; set; }
        public int entryID { get; set; }
        public string status { get; set; }
        public List<String> accountNames { get; set; }
        public List<Decimal> debits { get; set; }
        public List<Decimal> credits { get; set; }

        public Entry(int entryID, string status)
        {
            this.entryID = entryID;
            this.status = status;
            transactions = new List<Transaction>();
            accountNames = new List<String>();
            debits = new List<Decimal>();
            credits = new List<Decimal>();
        }

        public string FormattedAccountNames() {

            string formatted = "";

            foreach (string account in accountNames)
            {
                if (formatted != "")
                {
                    formatted += "\r\n";
                }
                formatted += account;
            }

            return formatted;
        }

        public string FormattedDebits()
        {

            string formatted = "";

            foreach (Decimal debit in debits)
            {
                if (formatted != "")
                {
                    formatted += "\r\n";
                }
                formatted += debit.ToString();
            }

            return formatted;
        }

        public string FormattedCredits()
        {

            string formatted = "";

            foreach (Decimal credit in credits)
            {
                if (formatted != "")
                {
                    formatted += "\r\n";
                }
                formatted += credit.ToString();
            }

            return formatted;
        }
    }
}