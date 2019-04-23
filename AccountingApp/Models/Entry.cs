using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AccountingApp.Models
{
    public class Entry
    {
        public List<TransactionTable> transactions { get; set; }
        public int entryID { get; set; }
        public string status { get; set; }
        public string comment { get; set; }

        public string AccountantUsername { get; set; }
        public string ManagerUsername { get; set; }
        public string AccountantComment { get; set; }
        public string ManagerComment { get; set; }
        public string Entry_Type { get; set; }
        public int PostReference { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime DateReviewed { get; set; }

        public DateTime submitDate;
        public List<String> accountNames { get; set; }
        public List<Decimal> debits { get; set; }
        public List<Decimal> credits { get; set; }
        public List<DocumentsTable> files { get; set; }
        public List<String> fileNames { get; set; }

        public Entry()
        {
            transactions = new List<TransactionTable>();
            accountNames = new List<String>();
            debits = new List<Decimal>();
            credits = new List<Decimal>();
            files = new List<DocumentsTable>();
            fileNames = new List<String>();
        }

        public Entry(int entryID, string status, DateTime submitDate, string comment)
        {
            this.entryID = entryID;
            this.status = status;
            this.submitDate = submitDate;
            this.comment = comment;
            transactions = new List<TransactionTable>();
            accountNames = new List<String>();
            debits = new List<Decimal>();
            credits = new List<Decimal>();
            files = new List<DocumentsTable>();
            fileNames = new List<String>();
        }

        public string FormattedFileNames() {

            string formatted = "";
            List<String> AllFiles = new List<String>();

            foreach (DocumentsTable f in files)
            {
                //AllFiles.Add("<a href=\"../Accountant/Download?file=" + f.FileName + "\" target=\"_blank\">" + f.FileName + "</a>");
                AllFiles.Add(f.FileName);
            }

            formatted = string.Join(", ", AllFiles);

            return formatted;
        }

        public string FormattedAccountNames() {

            string formatted = "";
            int i = findIndexOfCredit();
            int j = 1;
            foreach (string account in accountNames)
            {
                if (formatted != "")
                {
                    if (j != i)
                    {
                        formatted += "\r\n";
                        j++;
                    }
                    else
                        formatted += "\r\n\t";
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

        private int findIndexOfCredit()
        {
            int index = 0;
            foreach (Decimal amount in credits)
            {
                if (amount == 0)
                {
                    index++;
                    continue;
                }
                else
                    break;
            }
            return index;
        }
    }
}