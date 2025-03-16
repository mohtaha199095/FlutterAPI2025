using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsCurrency
    {
        // Select Currency Records
        public DataTable SelectCurrency(int Id, string AName, string EName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"SELECT * FROM tbl_Currency WHERE (Id=@Id OR @Id=0) AND 
                        (AName=@AName OR @AName='') AND (EName=@EName OR @EName='') AND (CompanyID=@CompanyID OR @CompanyID=0)",
                        clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Delete Currency by ID
        public bool DeleteCurrencyByID(int Id, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id }
                };

                clsSQL clsSQL = new clsSQL();
                clsSQL.ExecuteNonQueryStatement(@"DELETE FROM tbl_Currency WHERE Id=@Id",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Insert New Currency
        public int InsertCurrency(string AName, string EName, string Code, string PartAName, string PartEName,
                                   int DecimalPlaces, string Symbol, decimal ExchangeRate, bool IsActive,bool IsBase,
                                   int CreationUserId, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@Code", SqlDbType.NVarChar, -1) { Value = Code },
                    new SqlParameter("@PartAName", SqlDbType.NVarChar, -1) { Value = PartAName },
                    new SqlParameter("@PartEName", SqlDbType.NVarChar, -1) { Value = PartEName },
                    new SqlParameter("@DecimalPlaces", SqlDbType.Int) { Value = DecimalPlaces },
                    new SqlParameter("@Symbol", SqlDbType.NVarChar, -1) { Value = Symbol },
                    new SqlParameter("@ExchangeRate", SqlDbType.Decimal) { Value = ExchangeRate },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@IsBase", SqlDbType.Bit) { Value = IsBase },
                    
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                string query = @"INSERT INTO tbl_Currency (AName, EName, Code, PartAName, PartEName, DecimalPlaces, Symbol, ExchangeRate, IsActive,IsBase, CreationUserId, CreationDate, CompanyID)
                                OUTPUT INSERTED.ID VALUES (@AName, @EName, @Code, @PartAName, @PartEName, @DecimalPlaces, @Symbol, @ExchangeRate, @IsActive, @IsBase,@CreationUserId, @CreationDate, @CompanyID)";

                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Update Currency
        public int UpdateCurrency(int Id, string AName, string EName, string Code, string PartAName, string PartEName,
                                   int DecimalPlaces, string Symbol, decimal ExchangeRate, bool IsActive,bool IsBase, int ModificationUserId, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@Code", SqlDbType.NVarChar, -1) { Value = Code },
                    new SqlParameter("@PartAName", SqlDbType.NVarChar, -1) { Value = PartAName },
                    new SqlParameter("@PartEName", SqlDbType.NVarChar, -1) { Value = PartEName },
                    new SqlParameter("@DecimalPlaces", SqlDbType.Int) { Value = DecimalPlaces },
                    new SqlParameter("@Symbol", SqlDbType.NVarChar, -1) { Value = Symbol },
                    new SqlParameter("@ExchangeRate", SqlDbType.Decimal) { Value = ExchangeRate },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@IsBase", SqlDbType.Bit) { Value = IsBase },
                    
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                string query = @"UPDATE tbl_Currency SET 
                                AName=@AName, EName=@EName, Code=@Code, PartAName=@PartAName, PartEName=@PartEName,
                                DecimalPlaces=@DecimalPlaces, Symbol=@Symbol, ExchangeRate=@ExchangeRate, IsActive=@IsActive, IsBase=@IsBase,
                                ModificationUserId=@ModificationUserId, ModificationDate=@ModificationDate
                                WHERE Id=@Id";

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(query, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
