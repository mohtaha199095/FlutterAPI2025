using System.Data.SqlClient;
using System.Data;
using System;
using DocumentFormat.OpenXml.EMMA;

namespace WebApplication2.cls
{
    public class clsLoanTypes
    {
        public DataTable SelectLoanTypes(int Id, string AName, string EName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_LoanTypes where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteLoanTypesByID(int Id)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_LoanTypes where (id=@Id  )", prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertLoanTypes(string AName, string EName,string Code, bool IsReturned, 
            int PaymentAccountID,int ReceivableAccountID,decimal DefaultAmount,int DevidedMonths
            , int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 {
                 new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                 new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                 new SqlParameter("@Code", SqlDbType.NVarChar,-1) { Value = Code },

                 new SqlParameter("@IsReturned", SqlDbType.Bit) { Value = IsReturned },

                 new SqlParameter("@PaymentAccountID", SqlDbType.Int) { Value = PaymentAccountID },
                 new SqlParameter("@ReceivableAccountID", SqlDbType.Int) { Value = ReceivableAccountID },
                 new SqlParameter("@DefaultAmount", SqlDbType.Decimal) { Value = DefaultAmount },
                 new SqlParameter("@DevidedMonths", SqlDbType.Int) { Value = DevidedMonths },

                 new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                 new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                 new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_LoanTypes
(AName,
EName,
Code,
IsReturned,
PaymentAccountID,
ReceivableAccountID,
DefaultAmount,
DevidedMonths,
CompanyID,
CreationUserId,
CreationDate)
                        OUTPUT INSERTED.ID values
(@AName,
@EName,
@Code,
@IsReturned,
@PaymentAccountID,
@ReceivableAccountID,
@DefaultAmount,
@DevidedMonths,
@CompanyID,
@CreationUserId,
@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateLoanTypes(int ID,
            string AName, string EName, string Code, bool IsReturned,
            int PaymentAccountID, int ReceivableAccountID, decimal DefaultAmount, int DevidedMonths,
            int ModificationUserId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                 new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                 new SqlParameter("@Code", SqlDbType.NVarChar,-1) { Value = Code },
                 new SqlParameter("@IsReturned", SqlDbType.Bit) { Value = IsReturned },

                 new SqlParameter("@PaymentAccountID", SqlDbType.Int) { Value = PaymentAccountID },
                 new SqlParameter("@ReceivableAccountID", SqlDbType.Int) { Value = ReceivableAccountID },
                 new SqlParameter("@DefaultAmount", SqlDbType.Decimal) { Value = DefaultAmount },
                 new SqlParameter("@DevidedMonths", SqlDbType.Int) { Value = DevidedMonths },
                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_LoanTypes set 
                       AName=@AName,
                       EName=@EName,
                       Code=@Code,
                       IsReturned=@IsReturned,
                       PaymentAccountID=@PaymentAccountID,
                       ReceivableAccountID=@ReceivableAccountID,
                       DefaultAmount=@DefaultAmount,
                       DevidedMonths=@DevidedMonths,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where id =@id", prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
