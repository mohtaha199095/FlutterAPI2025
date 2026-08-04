using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;

namespace WebApplication2
{
    public class clsSQL
    {
        #region Declarations
        SqlCommand Command;
        SqlDataAdapter Adapter;
        public SqlTransaction Transaction;

        static SqlParameter[] CloneParameters(SqlParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return parameters;
            var cloned = new SqlParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                SqlParameter source = parameters[i];
                cloned[i] = new SqlParameter(source.ParameterName, source.SqlDbType)
                {
                    Value = source.Value ?? DBNull.Value,
                    Size = source.Size,
                    Precision = source.Precision,
                    Scale = source.Scale,
                };
            }
            return cloned;
        }
        //       public string conString = "Data Source=MAIN;Initial Catalog=WEBERP;User ID=sa;Password=123456789jo;";

        private static readonly Lazy<string> _mainDataBaseConString = new Lazy<string>(LoadMainConnectionString);
        public string MainDataBaseconString => _mainDataBaseConString.Value;

        private static string LoadMainConnectionString()
        {
            // `Directory.GetCurrentDirectory()` is not stable in ASP.NET hosting scenarios.
            // We search a few likely roots for `sqlcon.txt` / `SqlCon.txt`.
            string[] candidateFileNames = { "sqlcon.txt", "SqlCon.txt" };
            string[] startingDirs = { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };

            foreach (var start in startingDirs)
            {
                if (string.IsNullOrWhiteSpace(start)) continue;

                var dirInfo = new DirectoryInfo(start);
                for (int i = 0; i < 8 && dirInfo != null; i++, dirInfo = dirInfo.Parent)
                {
                    foreach (var fileName in candidateFileNames)
                    {
                        var path = Path.Combine(dirInfo.FullName, fileName);
                        if (File.Exists(path))
                        {
                            return File.ReadAllText(path).Trim();
                        }

                        // Common layout: project folder contains `sqlcon.txt`
                        var projectPath = Path.Combine(dirInfo.FullName, "WebApplication2", fileName);
                        if (File.Exists(projectPath))
                        {
                            return File.ReadAllText(projectPath).Trim();
                        }
                    }
                }
            }

            throw new FileNotFoundException(
                "Main database connection-string file not found. Expected `sqlcon.txt` (or `SqlCon.txt`) near the application folder.",
                "sqlcon.txt");
        }
        #endregion

        #region Query
        public DataTable Query(string StoredProcedure,string conString)
        {
            try
            {
                SqlConnection con = new SqlConnection(conString);
                Command = new SqlCommand(StoredProcedure, con);
                Command.CommandType = CommandType.StoredProcedure;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Adapter = new SqlDataAdapter(Command);
                DataTable dt = new DataTable();
                dt.Clear();
                Adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable Query(string StoredProcedure, string conString, SqlTransaction Transaction)
        {
            try
            {
                SqlConnection con = new SqlConnection(conString);
                Command = new SqlCommand(StoredProcedure, con, Transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Adapter = new SqlDataAdapter(Command);
                DataTable dt = new DataTable();
                dt.Clear();
                Adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable Query(string StoredProcedure, SqlParameter[] Parameter, SqlTransaction Transaction)
        {
            try
            {
                Command = new SqlCommand(StoredProcedure, Transaction.Connection, Transaction);
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandTimeout = 6000000;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.Parameters.AddRange(CloneParameters(Parameter));
                Adapter = new SqlDataAdapter(Command);
                DataTable dt = new DataTable();
                dt.Clear();
                Adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable Query(string StoredProcedure, SqlParameter[] Parameter, string conString)
        {
            try
            {

                SqlConnection con = new SqlConnection(conString);
                Command = new SqlCommand(StoredProcedure, con);
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandTimeout = 6000000;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.Parameters.AddRange(CloneParameters(Parameter));
                Adapter = new SqlDataAdapter(Command);
                DataTable dt = new DataTable();
                dt.Clear();
                Adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region NonQuery
        public int NonQuery(string StoredProcedure,  SqlParameter[] Parameter, string conString)
        {
            SqlConnection con = new SqlConnection(conString);
            try
            {

                Command = new SqlCommand(StoredProcedure, con, Transaction);
                Command.CommandType = CommandType.StoredProcedure;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.Parameters.AddRange(CloneParameters(Parameter));
                con.Open();
                int RowsEffected = Command.ExecuteNonQuery();
                con.Close();
                return RowsEffected;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public int NonQuery(string StoredProcedure, SqlParameter[] Parameter, SqlTransaction Transactions)
        {
            try
            {
                Command = new SqlCommand(StoredProcedure, Transactions.Connection, Transactions);
                Command.CommandType = CommandType.StoredProcedure;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.Parameters.AddRange(CloneParameters(Parameter));
                int RowsEffected = Command.ExecuteNonQuery();
                return RowsEffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int NonQuery(string StoredProcedure, SqlTransaction Transactions)
        {
            try
            {
                Command = new SqlCommand(StoredProcedure, Transactions.Connection, Transactions);
                Command.CommandType = CommandType.StoredProcedure;
                int RowsEffected = Command.ExecuteNonQuery();
                return RowsEffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region NonQueryWithReturnedValue
        public int NonQueryWithReturnedValue(string StoredProcedure, SqlParameter[] Parameter, string conString)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                Command = new SqlCommand(StoredProcedure, con);
                Command.CommandType = CommandType.StoredProcedure;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.Parameters.AddRange(CloneParameters(Parameter));
                con.Open();
                int RowsEffected = Command.ExecuteNonQuery();
                con.Close();
                return int.Parse(Parameter[Parameter.Length - 1].Value.ToString());
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public int NonQueryWithReturnedValue(string StoredProcedure, SqlParameter[] Parameter, SqlTransaction Transactions)
        {
            try
            {
                Command = new SqlCommand(StoredProcedure, Transactions.Connection, Transactions);
                Command.CommandType = CommandType.StoredProcedure;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.Parameters.AddRange(CloneParameters(Parameter));
                int RowsEffected = Command.ExecuteNonQuery();
                return int.Parse(Parameter[Parameter.Length - 1].Value.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ExecuteScalar
        public object ExecuteScalar(string StoredProcedure, string conString)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                Command = new SqlCommand(StoredProcedure, con);
                Command.CommandType = CommandType.StoredProcedure;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                con.Open();
                object ReturnedValue = Command.ExecuteScalar();
                con.Close();
                return ReturnedValue;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public object ExecuteScalar(string Text, SqlParameter[] Parameter, string conString, SqlTransaction trn)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                Command = new SqlCommand(Text, trn.Connection, trn);
                Command.CommandType = CommandType.Text;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();

                Command.Parameters.AddRange(CloneParameters(Parameter));
                //    clsConnections.con.Open ( );
                object ReturnedValue = Command.ExecuteScalar();
                //    clsConnections.con.Close ( );
                return ReturnedValue;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public object ExecuteScalar(string Text, string conString, SqlTransaction trn)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                if (trn != null) {
                    Command = new SqlCommand(Text, trn.Connection, trn);
                } else {
                    con.Open();
                   Command = new SqlCommand(Text, con);
                }
               
                Command.CommandType = CommandType.Text;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();


                //    clsConnections.con.Open ( );
                object ReturnedValue = Command.ExecuteScalar();
                if (trn == null ) {
                    con.Close();
                }
                //    clsConnections.con.Close ( );
                return ReturnedValue;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public object ExecuteScalarText(string Text, SqlParameter[] Parameter, string conString)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                Command = new SqlCommand(Text, con);
                Command.CommandType = CommandType.Text;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();

                Command.Parameters.AddRange(CloneParameters(Parameter));
                con.Open();
                object ReturnedValue = Command.ExecuteScalar();
                con.Close();
                return ReturnedValue;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public object ExecuteScalar(string StoredProcedure, SqlParameter[] Parameter, string conString )
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                Command = new SqlCommand(StoredProcedure, con);
                Command.CommandType = CommandType.Text;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();

                Command.Parameters.AddRange(CloneParameters(Parameter));
                con.Open();
                object ReturnedValue = Command.ExecuteScalar();
                con.Close();
                return ReturnedValue;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public object ExecuteScalarCommandText(string CommandText, string conString)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                Command = new SqlCommand(CommandText, con);
                Command.CommandType = CommandType.Text;
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();

                //   Command.Parameters.AddRange(Parameter);
                con.Open();
                object ReturnedValue = Command.ExecuteScalar();
                con.Close();
                return ReturnedValue;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        #endregion

        #region ExecuteStatement
        public int ExecuteNonQueryStatement(string SqlStatement, string conString, SqlParameter[] Prm = null, SqlTransaction trn = null)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {
                if (trn == null)
                    Command = new SqlCommand(SqlStatement, con);
                else
                    Command = new SqlCommand(SqlStatement, trn.Connection, trn);
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.CommandType = CommandType.Text;
                Command.CommandText = SqlStatement;
                if (Prm != null)
                {
                    Command.Parameters.AddRange(CloneParameters(Prm));

                }
                if (trn == null)
                    con.Open();
                int RowEffected = Command.ExecuteNonQuery();
                if (trn == null)
                    con.Close();
                return RowEffected;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                throw ex;
            }
        }
        public DataTable ExecuteQueryStatement(string SqlStatement, string conString, SqlTransaction trn = null)
        {

            SqlConnection con = new SqlConnection(conString);

            try
            {
                if (trn != null)
                    Command = new SqlCommand(SqlStatement, trn.Connection, trn);
                else
                    Command = new SqlCommand(SqlStatement, con);
                if (Command.Parameters.Count > 0)
                    Command.Parameters.Clear();
                Command.CommandType = CommandType.Text;
                SqlDataAdapter Adapter = new SqlDataAdapter(Command);






                DataTable dt = new DataTable();
                dt.Clear();
                Adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable ExecuteQueryStatement(string SqlStatement, string conString, SqlParameter[] Parameter, SqlTransaction trn = null)
        {
            SqlConnection con = new SqlConnection(conString);

            try
            {

                if (trn != null)
                    Command = new SqlCommand(SqlStatement, trn.Connection, trn);
                else
                    Command = new SqlCommand(SqlStatement, con);
                if (Command.Parameters.Count > 0)

                    Command.Parameters.Clear();
                Command.CommandTimeout = 0;  
                Command.CommandType = CommandType.Text;
                Command.Parameters.AddRange(CloneParameters(Parameter));
                SqlDataAdapter Adapter = new SqlDataAdapter(Command);

                DataTable dt = new DataTable();
                dt.Clear();
                Adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {



            }
        }
        public string CreateDataBaseConnectionString( int CompanyID) {
            DataTable dt= ExecuteQueryStatement("select * from tbl_company where id="+ Simulate.String( CompanyID),MainDataBaseconString);

            if (CompanyID == 0) {
                 
                return MainDataBaseconString ;

            }
            else if (dt != null && dt.Rows.Count > 0)
            {
                // Create a SqlConnectionStringBuilder to extract settings from MainDataBaseconString
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(MainDataBaseconString);

                // Set the database name dynamically from the DataTable
                builder.InitialCatalog = Simulate.String(dt.Rows[0]["DataBaseName"]);
                string newConnectionString = builder.ToString();
               // string a = "Data Source=DESKTOP-4462NTN;Initial Catalog=" + Simulate.String(dt.Rows[0]["DataBaseName"]) + " ;Integrated Security=true;User ID=sa;Password=P@ssw0rd;";

                return newConnectionString;


            }
            else{
            
            return "";
            }
        }
        #endregion
    }
}
