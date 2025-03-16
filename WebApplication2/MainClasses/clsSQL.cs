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
        //       public string conString = "Data Source=MAIN;Initial Catalog=WEBERP;User ID=sa;Password=123456789jo;";


        public string MainDataBaseconString = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "//sqlcon.txt").ToString();
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
                Command.Parameters.AddRange(Parameter);
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
                Command.Parameters.AddRange(Parameter);
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
                Command.Parameters.AddRange(Parameter);
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
                Command.Parameters.AddRange(Parameter);
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
                Command.Parameters.AddRange(Parameter);
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
                Command.Parameters.AddRange(Parameter);
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

                Command.Parameters.AddRange(Parameter);
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

                Command.Parameters.AddRange(Parameter);
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

                Command.Parameters.AddRange(Parameter);
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
                    Command.Parameters.AddRange(Prm);

                }
                con.Open();
                int RowEffected = Command.ExecuteNonQuery();
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
                Command.Parameters.AddRange(Parameter);
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
