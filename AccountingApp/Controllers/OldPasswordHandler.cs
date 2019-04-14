using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;
using Dapper;
using AccountingApp.DBAccess;

namespace AccountingApp.Controllers
{
    public class OldPasswordHandler
    {
        public void AdjustOldPasswords(string Password, int UserId) {

            string FoundPasswords = "";
            string PasswordsToInsert = "";
            //SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            //SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            SqlConnection sqlCon = new SqlConnection(SqlAccess.GetConnectionString());

            sqlCon.Open();

            //string query = "SELECT Old_Passwords FROM CreateUsers WHERE ID = '" + UserId + "'";
            string query = "SELECT OldPasswords FROM dbo.UserTable WHERE ID = '" + UserId + "'";

            SqlCommand conData = new SqlCommand(query, sqlCon);
            SqlDataReader myReader;

            myReader = conData.ExecuteReader();

            while (myReader.Read())
            {
                //FoundPasswords = myReader.GetString(0);
                int x = myReader.GetOrdinal("OldPasswords");

                FoundPasswords = myReader.IsDBNull(x) ? string.Empty : myReader.GetString(0);
            }

            myReader.Close();
            sqlCon.Close();

            if (FoundPasswords == null || FoundPasswords == "")
            {
                //no passwords found, store the 1 password
                PasswordsToInsert = Password;
            }
            else if (FoundPasswords.Contains('|') == false)
            {
                //only 1 password
                PasswordsToInsert = FoundPasswords + "|" + Password;
            }
            else
            {
                //more than 1 password
                string[] passes = FoundPasswords.Split('|');
                if (passes.Length == 6)  //max old password limit reached
                {
                    List<string> passlist = passes.ToList();
                    passlist.Add(Password); //now length 7
                    passlist.RemoveAt(0);  //now length 6
                    PasswordsToInsert = string.Join("|", passlist.Select(x => x.ToString()).ToArray());
                }
                else
                {   //less than 6 passwords
                    PasswordsToInsert = FoundPasswords + "|" + Password;
                }
            }

            //string Upd = "UPDATE CreateUsers SET Old_Passwords=@Passwords WHERE ID='" + UserId + "';";
            string Upd = "UPDATE UserTable SET OldPasswords=@Passwords WHERE ID='" + UserId + "';";

            sqlCon.Open();
            SqlCommand sqlUpdate = new SqlCommand(Upd, sqlCon);
            sqlUpdate.Parameters.AddWithValue("@Passwords", PasswordsToInsert);

            sqlUpdate.ExecuteNonQuery();

            sqlCon.Close();

        }
    }
}
