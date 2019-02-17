using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AccountingApp.Controllers
{
    public class ErrorController
    {
        public string GetErrorMessage(int id)
        {
            string error = "";
            SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            sqlCon.Open();

            string query = "SELECT Description FROM ErrorMessages WHERE Error_ID = '" + id + "'";
            SqlCommand conData = new SqlCommand(query, sqlCon);
            SqlDataReader myReader;

            myReader = conData.ExecuteReader();

            while (myReader.Read())
            {
                error = myReader.GetString(0);
            }

            myReader.Close();
            sqlCon.Close();
            return error;
        }
    }
}