using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsJournalVoucherHeader
    {
        public DataTable SelectJournalVoucherHeader(string guid, int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, int CompanyID, DateTime Date1, DateTime Date2)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( guid )},
      new SqlParameter("@Notes", SqlDbType.NVarChar,-1) { Value = Notes },
       new SqlParameter("@JVNumber", SqlDbType.NVarChar,-1) { Value = JVNumber },
           new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
           new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = CostCenterID },
           new SqlParameter("@JVTypeID", SqlDbType.Int) { Value = JVTypeID },
           new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
           new SqlParameter("@Date1", SqlDbType.DateTime) { Value = Date1 },
           new SqlParameter("@Date2", SqlDbType.DateTime) { Value = Date2 },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_JournalVoucherHeader where (guid=@guid or @guid='00000000-0000-0000-0000-000000000000' )
and (BranchID=@BranchID or @BranchID=0 )
and (CostCenterID=@CostCenterID or @CostCenterID=0 )
and (JVTypeID=@JVTypeID or @JVTypeID=0 )
and (CompanyID=@CompanyID or @CompanyID=0 )
and (cast(VoucherDate as date) between cast( @date1 as date) and cast( @date2 as date))
and (Notes=@Notes or @Notes='' )
and (JVNumber=@JVNumber or @JVNumber='' ) order by jvnumber asc", prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteJournalVoucherHeaderByID(string guid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid ) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_JournalVoucherHeader where (guid=@guid  )", prm, trn);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, int CompanyID, DateTime VoucherDate, int CreationUserId, SqlTransaction trn = null)
        {

            try
            {
                SqlParameter[] prm =
                 {
      new SqlParameter("@Notes", SqlDbType.NVarChar,-1) { Value = Notes },
       new SqlParameter("@JVNumber", SqlDbType.NVarChar,-1) { Value = JVNumber },
           new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
           new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = CostCenterID },
           new SqlParameter("@JVTypeID", SqlDbType.Int) { Value = JVTypeID },
           new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
           new SqlParameter("@VoucherDate", SqlDbType.DateTime) { Value = VoucherDate },
                       new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_JournalVoucherHeader(Notes,BranchID,CostCenterID,JVNumber,JVTypeID,CompanyID,VoucherDate,CreationUserId,CreationDate) 
                                       OUTPUT INSERTED.guid values(@Notes,@BranchID,@CostCenterID,@JVNumber,@JVTypeID,@CompanyID,@VoucherDate,@CreationUserId,@CreationDate)";

                clsSQL clsSQL = new clsSQL();

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
        public string UpdateJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, DateTime VoucherDate, string guid, int ModificationUserId, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {  new SqlParameter("@Notes", SqlDbType.NVarChar,-1) { Value = Notes },
       new SqlParameter("@JVNumber", SqlDbType.NVarChar,-1) { Value = JVNumber },
           new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
           new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = CostCenterID },
           new SqlParameter("@JVTypeID", SqlDbType.Int) { Value = JVTypeID },

           new SqlParameter("@VoucherDate", SqlDbType.DateTime) { Value = VoucherDate },
                     new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid ) },
                           new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(@"update tbl_JournalVoucherHeader set 
Notes=@Notes,
JVNumber=@JVNumber,
BranchID=@BranchID,
CostCenterID=@CostCenterID,
JVTypeID=@JVTypeID,
 
VoucherDate=@VoucherDate,
 



ModificationDate=@ModificationDate,
ModificationUserId=@ModificationUserId 
where guid =@guid", prm, trn));

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }



        public DataTable SelectMaxJVNo(string guid, int JVTypeID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( guid )},

           new SqlParameter("@JVTypeID", SqlDbType.Int) { Value = JVTypeID },
           new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select max(JVNumber) from tbl_JournalVoucherHeader where 
(guid=@guid or @guid='00000000-0000-0000-0000-000000000000' )
 
and (JVTypeID=@JVTypeID or @JVTypeID=0 )
and (CompanyID=@CompanyID or @CompanyID=0 )
 ", prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public bool CheckJVMatch(string JVID, SqlTransaction trn)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId(JVID, 0, 0, trn);
                if (dt != null && dt.Rows.Count > 0)
                {

                    decimal TotalDebit = 0;
                    decimal TotalCredit = 0;
                    decimal TotalLine = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TotalDebit = TotalDebit + Simulate.decimal_(dt.Rows[i]["Debit"]);
                        TotalCredit = TotalCredit + Simulate.decimal_(dt.Rows[i]["Credit"]);
                        TotalLine = TotalLine + Simulate.decimal_(dt.Rows[i]["Total"]);


                    }
                    if ((TotalCredit == TotalDebit) && TotalLine == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {

                    return false;
                }


            }
            catch (Exception)
            {

                return false;
            }


        }
    }
}
