using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AccountingApp.Controllers
{
    public class OldPasswordHandler
    {
        public void AdjustOldPasswords(string Password, int UserId) {

            string FoundPasswords = "";
            string PasswordsToInsert = "";
            SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            sqlCon.Open();

            string query = "SELECT Old_Passwords FROM CreateUsers WHERE ID = '" + UserId + "'";
            SqlCommand conData = new SqlCommand(query, sqlCon);
            SqlDataReader myReader;

            myReader = conData.ExecuteReader();

            while (myReader.Read())
            {
                FoundPasswords = myReader.GetString(0);
            }

            myReader.Close();
            sqlCon.Close();

            if (FoundPasswords == null)
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

            //SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");

            string Upd = "UPDATE CreateUsers SET Old_Passwords=@Passwords WHERE ID='" + UserId + "';";
            sqlCon.Open();
            SqlCommand sqlUpdate = new SqlCommand(Upd, sqlCon);
            sqlUpdate.Parameters.AddWithValue("@Passwords", PasswordsToInsert);

            sqlUpdate.ExecuteNonQuery();

            sqlCon.Close();

        }

        //private void InsertPassword(string pass) {
        //SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");

        //string Upd = "UPDATE CreateUsers SET FirstName=@firstname WHERE UserID='" + UserIDTextbox.Text + "';";
        //sqlCon.Open();
        //    SqlCommand sqlUpdate = new SqlCommand(Upd, sqlCon);
        //sqlUpdate.Parameters.AddWithValue("@firstname", FirstnameTextbox.Text.Trim());
        //    sqlUpdate.Parameters.AddWithValue("@lastname", LastnameTextbox.Text.Trim());

        //    sqlUpdate.ExecuteNonQuery();

        //    sqlCon.Close();
        //}
    }
}