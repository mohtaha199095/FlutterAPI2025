using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsCashVoucherDetails
    {


        public DataTable SelectCashVoucherDetailsByHeaderGuid(string HeaderGuid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },

        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_CashVoucherDetails where   (HeaderGuid=@HeaderGuid or @HeaderGuid='00000000-0000-0000-0000-000000000000' )    and (CompanyID=@CompanyID or @CompanyID=0  )  order by rowindex asc
                     ", prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteCashVoucherDetailsByHeaderGuid(string HeaderGuid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_CashVoucherDetails where (HeaderGuid=@HeaderGuid  )", prm, trn);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }
        public string InsertCashVoucherDetails(DBCashVoucherDetails dBCashVoucherDetails, string HeaderGuid, SqlTransaction trn)
        {
            try
            {
                SqlParameter[] prm =
                   { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(HeaderGuid)},
                    new SqlParameter("@IsUpper", SqlDbType.Bit) { Value = dBCashVoucherDetails.IsUpper },                    new SqlParameter("@RowIndex", SqlDbType.Int) { Value = dBCashVoucherDetails.RowIndex },


                new SqlParameter("@AccountID", SqlDbType.Int) { Value = dBCashVoucherDetails.AccountID },
                new SqlParameter("@SubAccountID", SqlDbType.Int) { Value = dBCashVoucherDetails.SubAccountID },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = dBCashVoucherDetails.BranchID },
                new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = dBCashVoucherDetails.CostCenterID },
                    new SqlParameter("@Debit", SqlDbType.Decimal) { Value = dBCashVoucherDetails.Debit },
                    new SqlParameter("@Credit", SqlDbType.Decimal) { Value = dBCashVoucherDetails.Credit },
                    new SqlParameter("@Total", SqlDbType.Decimal) { Value = dBCashVoucherDetails.Total },
                    new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = dBCashVoucherDetails.Note },
                new SqlParameter("@VoucherType", SqlDbType.Int) { Value = dBCashVoucherDetails.VoucherType },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = dBCashVoucherDetails.CompanyID },
                };

                string a = @"insert into tbl_CashVoucherDetails ( HeaderGuid,IsUpper,RowIndex,AccountID,SubAccountID,BranchID,
                                                                 CostCenterID,Debit,Credit, Total,Note,VoucherType,CompanyID)  
OUTPUT INSERTED.Guid  
values ( @HeaderGuid,@IsUpper,@RowIndex,@AccountID,@SubAccountID,@BranchID,
                                                                 @CostCenterID,@Debit,@Credit, @Total,@Note,@VoucherType,@CompanyID)";
                clsSQL clsSQL = new clsSQL();
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(a, prm, trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }


    }
    public class DBCashVoucherDetails
    {


        public Guid Guid { get; set; }
        public Guid HeaderGuid { get; set; }
        public bool IsUpper { get; set; }
        public int RowIndex { get; set; }
        public int AccountID { get; set; }
        public int SubAccountID { get; set; }
        public int BranchID { get; set; }
        public int CostCenterID { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Total { get; set; }
        public string Note { get; set; }
        public int VoucherType { get; set; }
        public int CompanyID { get; set; }





    }
}
