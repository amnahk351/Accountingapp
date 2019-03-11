using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Configuration;
namespace AccountingApp.DBAccess
{
    public static class SqlAccess
    {
        public static string GetConnectionString(string connectionName = "Razor")
        {

            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            //Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Quillan\Documents\GitHub\MVCAccountingapp\AccountingApp\App_Data\Database1.mdf; Integrated Security = True; MultipleActiveResultSets = True; Application Name = EntityFramework;
            // =data source=(LocalDB)\MSSQLLocalDB;attachdbfilename=|DataDirectory|\Database1.mdf;integrated security=True;multipleactiveresultsets=True;application name=EntityFramework&quot;" providerName = "System.Data.EntityClient" />

        }

        public static List<T> LoadData<T>(string sql)
        {
            using (IDbConnection cnn = new SqlConnection(GetConnectionString()))
            {
                return cnn.Query<T>(sql).ToList();
            }
        }

        public static int SaveData<T>(string sql, T data)
        {
            using (IDbConnection cnn = new SqlConnection(GetConnectionString()))
            {
                return cnn.Execute(sql, data);
            }
        }

        public static void ExecuteWithoutReturn(string procedureName, DynamicParameters par = null)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString("Razor")))
            {
                con.Open();
                con.Execute(procedureName, par, commandType: CommandType.StoredProcedure);

            }
        }

        public static T ExecuteReturnScalar<T>(string procedureName, DynamicParameters par = null)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString("Razor")))
            {
                con.Open();
                return (T)Convert.ChangeType(con.ExecuteScalar(procedureName, par, commandType: CommandType.StoredProcedure), typeof(T));

            }
        }

        // SqlAccess.ReturnList<ErrorMessageModel> <= IEnumerable<ErrorMessageModel>
        public static IEnumerable<T> ReturnList<T>(string procedureName, DynamicParameters par = null)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString("Razor")))
            {
                con.Open();
                return con.Query<T>(procedureName, par, commandType: CommandType.StoredProcedure);

            }
        }
    }
}