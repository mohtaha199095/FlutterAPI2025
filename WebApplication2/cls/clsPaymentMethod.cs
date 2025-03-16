 
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.DataSet;
namespace WebApplication2.cls
{
    public class clsPaymentMethod
    {





        public DataTable SelectPaymentMethodByID(int Id, string AName, string EName, int CompanyID,SqlTransaction trn=null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_PaymentMethod where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeletePaymentMethodByID(int Id,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm1 =
                { new SqlParameter("@PaymentMethodID", SqlDbType.Int) { Value = Id },

                };
                string a = @" 
select PaymentMethodTypeID from tbl_CashVoucherHeader  where PaymentMethodTypeID=@PaymentMethodID
union all
select PaymentMethodID from tbl_InvoiceHeader where PaymentMethodID=@PaymentMethodID
";
                
  DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm1);

                if(dt!= null&& dt.Rows.Count >0)
                {
                    return false;

                }
                else
                {
                    SqlParameter[] prm =
                                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                    int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_PaymentMethod where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                    return true;
                }

            
            }
            catch (Exception)
            {

                return false;
            }


        }
        public int InsertPaymentMethod(string AName, string EName, int BranchID, int GLAccountID, int GLSubAccountID,
            bool IsCash, bool IsBank, bool IsDebit, int CompanyID, int CreationUserId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },


                                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },


  new SqlParameter("@GLAccountID", SqlDbType.Int) { Value = GLAccountID },
    new SqlParameter("@GLSubAccountID", SqlDbType.Int) { Value = GLSubAccountID },
    new SqlParameter("@IsCash", SqlDbType.Bit) { Value = IsCash },
    new SqlParameter("@IsBank", SqlDbType.Bit) { Value = IsBank },
        new SqlParameter("@IsDebit", SqlDbType.Bit) { Value = IsDebit },

                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_PaymentMethod(AName,EName,BranchID,GLAccountID,GLSubAccountID,IsCash,IsBank,IsDebit,CompanyID,CreationUserId,CreationDate)
                           OUTPUT INSERTED.ID values(@AName,@EName,@BranchID,@GLAccountID,@GLSubAccountID,@IsCash,@IsBank,@IsDebit,@CompanyID,@CreationUserId,@CreationDate)";

                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdatePaymentMethod(int ID, string AName, string EName, int BranchID, int GLAccountID, int GLSubAccountID,
            bool IsCash, bool IsBank, bool IsDebit, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },



                                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                                      new SqlParameter("@GLAccountID", SqlDbType.Int) { Value = GLAccountID },
    new SqlParameter("@GLSubAccountID", SqlDbType.Int) { Value = GLSubAccountID },
    new SqlParameter("@IsCash", SqlDbType.Bit) { Value = IsCash },
    new SqlParameter("@IsBank", SqlDbType.Bit) { Value = IsBank },
        new SqlParameter("@IsDebit", SqlDbType.Bit) { Value = IsDebit },

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_PaymentMethod set 
                       AName=@AName,
                       EName=@EName,
 BranchID=@BranchID,
GLAccountID=@GLAccountID,
GLSubAccountID=@GLSubAccountID,
IsCash=@IsCash,
IsBank=@IsBank,
IsDebit=@IsDebit,




                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where id =@id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}

