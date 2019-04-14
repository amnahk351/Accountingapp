using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;
using Dapper;
using AccountingApp.Models;
using AccountingApp.DBAccess;

namespace AccountingApp.Controllers
{
    public class ErrorController
    {
        public string GetErrorMessage(int id)
        {
            List<ErrorMessageModel> errors = new List<ErrorMessageModel>();

            using (IDbConnection db = new SqlConnection(SqlAccess.GetConnectionString()))
            {

                
                errors.Add(db.Query<ErrorMessageModel>("Select * from dbo.ErrorMessages Where Error_ID = @ID", new { ID = id }).FirstOrDefault());

            }
            return errors[0].Description.ToString();
            //string error = "";
            //SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database1.mdf;Integrated Security=True");
            //sqlCon.Open();

            //string query = "SELECT Description FROM ErrorMessages WHERE Error_ID = '" + id + "'";
            //SqlCommand conData = new SqlCommand(query, sqlCon);
            //SqlDataReader myReader;

            //myReader = conData.ExecuteReader();

            //while (myReader.Read())
            //{
            //    error = myReader.GetString(0);
            //}

            //myReader.Close();
            //sqlCon.Close();
            //return error;
        }
    }
}