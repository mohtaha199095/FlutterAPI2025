using Microsoft.Data.SqlClient;
using System.Data;
using System;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;

namespace WebApplication2.cls
{
    public class clsLoanTypes
    {
        public DataTable SelectLoanTypes(int Id,string LoanMainType, string AName, string EName, string Code, int CompanyID)
        {
            try
            {
             
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
              new SqlParameter("@Code", SqlDbType.NVarChar,-1) { Value = Code },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
         
                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_LoanTypes where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' )  and (Code=@Code or @Code='' ) 
and (CompanyID=@CompanyID or CompanyID=-1 or @CompanyID=0 )
and (MainTypeID in (" + LoanMainType + ") )  ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteLoanTypesByID(int Id,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_LoanTypes where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertLoanTypes(string AName, string EName,string Code, bool IsReturned, 
            int PaymentAccountID,int ReceivableAccountID,decimal DefaultAmount,int DevidedMonths
            ,bool IsActive,decimal InterestRate,int MainTypeID,int ProfitAccount, bool IsStopBP
            , int CompanyID, int CreationUserId,bool IsShowInMonthlyReports)
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
                 new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                 new SqlParameter("@InterestRate", SqlDbType.Decimal) { Value = InterestRate },
                 new SqlParameter("@MainTypeID", SqlDbType.Int) { Value = MainTypeID },
                new SqlParameter("@ProfitAccount", SqlDbType.Int) { Value = ProfitAccount },  
                 new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                 new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                 new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                  new SqlParameter("@IsStopBP", SqlDbType.Bit) { Value = IsStopBP },
                   new SqlParameter("@IsShowInMonthlyReports", SqlDbType.Bit) { Value = IsShowInMonthlyReports },
                  
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
IsActive,
InterestRate,
MainTypeID,
ProfitAccount,
CompanyID,
CreationUserId,
CreationDate,
IsStopBP,
IsShowInMonthlyReports)
                        OUTPUT INSERTED.ID values
(@AName,
@EName,
@Code,
@IsReturned,
@PaymentAccountID,
@ReceivableAccountID,
@DefaultAmount,
@DevidedMonths,
@IsActive,
@InterestRate,
@MainTypeID,
@ProfitAccount,
@CompanyID,
@CreationUserId,
@CreationDate,
@IsStopBP,
@IsShowInMonthlyReports)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateLoanTypes(int ID,
            string AName, string EName, string Code, bool IsReturned,
            int PaymentAccountID, int ReceivableAccountID, decimal DefaultAmount, int DevidedMonths,
            bool IsActive,decimal InterestRate,int MainTypeID,int ProfitAccount,bool IsStopBP,
            int ModificationUserId,bool IsShowInMonthlyReports,int CompanyID)
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

                 new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                 new SqlParameter("@InterestRate", SqlDbType.Decimal) { Value = InterestRate },
                 new SqlParameter("@MainTypeID", SqlDbType.Int) { Value = MainTypeID },
                 new SqlParameter("@ProfitAccount", SqlDbType.Int) { Value = ProfitAccount },
                 
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                       new SqlParameter("@IsStopBP", SqlDbType.Bit) { Value = IsStopBP },
               new SqlParameter("@IsShowInMonthlyReports", SqlDbType.Bit) { Value = IsShowInMonthlyReports },

                       

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
                       IsActive=@IsActive,
                       InterestRate=@InterestRate,
                       MainTypeID=@MainTypeID,
                       ProfitAccount=@ProfitAccount,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId,
                       IsStopBP =@IsStopBP,
IsShowInMonthlyReports=@IsShowInMonthlyReports
                   where id =@id",  clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
