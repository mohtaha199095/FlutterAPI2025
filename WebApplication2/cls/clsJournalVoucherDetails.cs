using Microsoft.CodeAnalysis.Operations;
using System;
using System.Data;
using System.Data.SqlClient;
using WebApplication2.DataSet;

namespace WebApplication2.cls
{
    public class clsJournalVoucherDetails
    {
        public DataTable SelectJournalVoucherDetailsByParentId(string ParentGuid, int AccountId, int SubAccountid, int branchID, int costcenterID,int CreationUserID ,SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( ParentGuid ) },
                     new SqlParameter("@costcenterID", SqlDbType.Int) { Value = costcenterID },    new SqlParameter("@branchID", SqlDbType.Int) { Value = branchID },
                        new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
    new SqlParameter("@accountid", SqlDbType.Int) { Value = AccountId },    new SqlParameter("@SubAccountid", SqlDbType.Int) { Value = SubAccountid },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_JournalVoucherDetails where (CreationUserID=@CreationUserID or @CreationUserID=0 ) and (branchID=@branchID or @branchID=0 ) and (costcenterID=@costcenterID or @costcenterID=0 ) and (accountid=@accountid or @accountid=0 ) and (SubAccountid=@SubAccountid or @SubAccountid=0 ) and (ParentGuid=@ParentGuid or @ParentGuid='00000000-0000-0000-0000-000000000000' )   order by rowindex asc", prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public dsJVDetails SelectJournalVoucherDetailsByParentIdForPrint(string ParentGuid, int AccountId, int SubAccountid, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( ParentGuid ) },
    new SqlParameter("@accountid", SqlDbType.Int) { Value = AccountId },    new SqlParameter("@SubAccountid", SqlDbType.Int) { Value = SubAccountid },
                };
                DataTable dtDetails = clsSQL.ExecuteQueryStatement(@"select tbl_JournalVoucherDetails.*,tbl_Branch.aname as BranchName,tbl_CostCenter.aname as CostCenterName 
,tbl_Accounts.AName as AccountName
,tbl_BusinessPartner.AName as SubAccountName
from tbl_JournalVoucherDetails 
 left join tbl_Branch on tbl_Branch.ID = tbl_JournalVoucherDetails.BranchID
  left join tbl_CostCenter on tbl_CostCenter.ID = tbl_JournalVoucherDetails.CostCenterID
     left join tbl_Accounts on tbl_Accounts.ID = tbl_JournalVoucherDetails.AccountID
	      left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID

where (tbl_JournalVoucherDetails.accountid=@accountid or @accountid=0 ) 
and (tbl_JournalVoucherDetails.SubAccountid=@SubAccountid or @SubAccountid=0 )
and (tbl_JournalVoucherDetails.ParentGuid=@ParentGuid or @ParentGuid='00000000-0000-0000-0000-000000000000' )   order by rowindex asc", prm, trn);

                dsJVDetails ds = new dsJVDetails();

                if (dtDetails != null && dtDetails.Rows.Count > 0) {


                 

                    if (dtDetails != null && dtDetails.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtDetails.Rows.Count; i++)
                        {
                            ds.JVDetails.Rows.Add();

                            ds.JVDetails.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                            ds.JVDetails.Rows[i]["ParentGuid"] = Simulate.String(dtDetails.Rows[i]["ParentGuid"]);
                            ds.JVDetails.Rows[i]["RowIndex"] = Simulate.String(dtDetails.Rows[i]["RowIndex"]);
                            ds.JVDetails.Rows[i]["AccountID"] = Simulate.String(dtDetails.Rows[i]["AccountID"]);
                            ds.JVDetails.Rows[i]["AccountName"] = Simulate.String(dtDetails.Rows[i]["AccountName"]);
                            ds.JVDetails.Rows[i]["SubAccountID"] = Simulate.String(dtDetails.Rows[i]["SubAccountID"]);
                            ds.JVDetails.Rows[i]["SubAccountName"] = Simulate.String(dtDetails.Rows[i]["SubAccountName"]);
                            ds.JVDetails.Rows[i]["Debit"] = Simulate.decimal_(dtDetails.Rows[i]["Debit"]);
                            ds.JVDetails.Rows[i]["Credit"] = Simulate.decimal_(dtDetails.Rows[i]["Credit"]);
                            ds.JVDetails.Rows[i]["Total"] = Simulate.decimal_(dtDetails.Rows[i]["Total"]);
                            ds.JVDetails.Rows[i]["BranchID"] = Simulate.String(dtDetails.Rows[i]["BranchID"]);
                            ds.JVDetails.Rows[i]["BranchName"] = Simulate.String(dtDetails.Rows[i]["BranchName"]);
                            ds.JVDetails.Rows[i]["CostCenterID"] = Simulate.String(dtDetails.Rows[i]["CostCenterID"]);
                            ds.JVDetails.Rows[i]["CostCenterName"] = Simulate.String(dtDetails.Rows[i]["CostCenterName"]);
                            ds.JVDetails.Rows[i]["DueDate"] = Simulate.StringToDate(dtDetails.Rows[i]["DueDate"]).ToString("yyyy-MM-dd");
                            ds.JVDetails.Rows[i]["Note"] = Simulate.String(dtDetails.Rows[i]["Note"]);





                        }
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public bool DeleteJournalVoucherDetailsByParentId(string ParentGuid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                string a = @"delete from tbl_Reconciliation where TransactionGuid in (

select TransactionGuid from tbl_Reconciliation where JVDetailsGuid in (select guid from tbl_JournalVoucherDetails where ParentGuid = @ParentGuid))";
                SqlParameter[] prm1 =
                 { new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value =  Simulate.Guid( ParentGuid ) },

                };
                clsSQL.ExecuteNonQueryStatement(a, prm1,trn);



                SqlParameter[] prm =
                 { new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value =  Simulate.Guid( ParentGuid ) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_JournalVoucherDetails where (ParentGuid=@ParentGuid  )", prm, trn);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertJournalVoucherDetails(string ParentGuid, int RowIndex, int AccountID, int SubAccountID,
            decimal Debit,
            decimal Credit,
            decimal Total,
            int BranchID,
            int CostCenterID,
            DateTime DueDate,
            string Note,
            int CompanyID,
                      int CreationUserId, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value =  Simulate.Guid( ParentGuid ) },
                      new SqlParameter("@AccountID", SqlDbType.Int) { Value =  AccountID },  new SqlParameter("@RowIndex", SqlDbType.Int) { Value =  RowIndex },
                       new SqlParameter("@SubAccountID", SqlDbType.Int) { Value = SubAccountID },
                      new SqlParameter("@Debit", SqlDbType.Decimal) { Value = Debit },
                       new SqlParameter("@Credit", SqlDbType.Decimal) { Value = Credit },
                        new SqlParameter("@Total", SqlDbType.Decimal) { Value = Total },
                          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                           new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = CostCenterID },
                             new SqlParameter("@DueDate", SqlDbType.DateTime) { Value = DueDate },
                               new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = Note },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                                 new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                                  new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                clsSQL clsSQL = new clsSQL();
                string a = @"insert into tbl_JournalVoucherDetails(ParentGuid,RowIndex,AccountID,SubAccountID,Debit,Credit,Total,BranchID,CostCenterID,DueDate,Note,CompanyID,CreationUserId,CreationDate) 
                                       OUTPUT INSERTED.Guid values(@ParentGuid,@RowIndex,@AccountID,@SubAccountID,@Debit,@Credit,@Total,@BranchID,@CostCenterID,@DueDate,@Note,@CompanyID,@CreationUserId,@CreationDate)";
                if (trn == null)
                    return Simulate.String(clsSQL.ExecuteScalar(a, prm));
                else
                    return Simulate.String(clsSQL.ExecuteScalar(a, prm, trn));

            }
            catch (Exception)
            {

                throw;
            }


        }

    }
}
