using DocumentFormat.OpenXml.Presentation;
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsJournalVoucherHeader
    {
        private const string AllCostCenterNamesSql = @"
, COALESCE(
    NULLIF((
        SELECT STUFF(
            (SELECT ', ' + cc.AName
             FROM (
                 SELECT DISTINCT d.CostCenterID
                 FROM tbl_JournalVoucherDetails d
                 WHERE d.ParentGuid = tbl_JournalVoucherHeader.Guid
                   AND ISNULL(d.CostCenterID, 0) > 0
             ) dc
             INNER JOIN tbl_CostCenter cc ON cc.ID = dc.CostCenterID
             ORDER BY cc.AName
             FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '')
    ), ''),
    (
        SELECT TOP 1 cc.AName
        FROM tbl_CostCenter cc
        WHERE cc.ID = tbl_JournalVoucherHeader.CostCenterID
          AND ISNULL(tbl_JournalVoucherHeader.CostCenterID, 0) > 0
    ),
    ''
) AS AllCostCenterNames";

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
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select *  ,
(select sum(debit) from tbl_JournalVoucherDetails
where tbl_JournalVoucherDetails.ParentGuid = tbl_JournalVoucherHeader.Guid) as TotalAmount" + AllCostCenterNamesSql + @"

from tbl_JournalVoucherHeader
where (tbl_JournalVoucherHeader.guid=@guid or @guid='00000000-0000-0000-0000-000000000000' )
and (tbl_JournalVoucherHeader.BranchID=@BranchID or @BranchID=0 )
and (tbl_JournalVoucherHeader.CostCenterID=@CostCenterID or @CostCenterID=0 )
and (tbl_JournalVoucherHeader.JVTypeID=@JVTypeID or @JVTypeID=0 )
and (tbl_JournalVoucherHeader.CompanyID=@CompanyID or @CompanyID=0 )
and (cast(tbl_JournalVoucherHeader.VoucherDate as date) between cast( @date1 as date) and cast( @date2 as date))
and (tbl_JournalVoucherHeader.Notes=@Notes or @Notes='' )
and (tbl_JournalVoucherHeader.JVNumber=@JVNumber or @JVNumber='' ) order by jvnumber asc", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public DataTable SelectJournalVoucherHeaderForScheduling(string guid, int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, int CompanyID, DateTime Date1, DateTime Date2)
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
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select *  ,
(select sum(debit) from tbl_JournalVoucherDetails
where tbl_JournalVoucherDetails.ParentGuid = tbl_JournalVoucherHeader.Guid) as TotalAmount
, (select top 1 tbl_BusinessPartner.AName from tbl_JournalVoucherDetails
left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID
 where ParentGuid = tbl_JournalVoucherHeader.Guid) BusinessPartnerName
 ,
(SELECT STUFF(
        (SELECT ', ' + tbl_FinancingDetails.Description
         FROM tbl_FinancingDetails 
         WHERE tbl_FinancingDetails.HeaderGuid = tbl_JournalVoucherHeader.RelatedFinancingHeaderGuid or 
		 tbl_FinancingDetails.Guid = tbl_JournalVoucherHeader.RelatedFinancingHeaderGuid
         FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '+')) AS AllDescriptions" + AllCostCenterNamesSql + @"
from tbl_JournalVoucherHeader
where (tbl_JournalVoucherHeader.guid=@guid or @guid='00000000-0000-0000-0000-000000000000' )
and (tbl_JournalVoucherHeader.BranchID=@BranchID or @BranchID=0 )
and (tbl_JournalVoucherHeader.CostCenterID=@CostCenterID or @CostCenterID=0 )
and (tbl_JournalVoucherHeader.JVTypeID=@JVTypeID or @JVTypeID=0 )
and (tbl_JournalVoucherHeader.CompanyID=@CompanyID or @CompanyID=0 )
and (cast(tbl_JournalVoucherHeader.VoucherDate as date) between cast( @date1 as date) and cast( @date2 as date))
and (tbl_JournalVoucherHeader.Notes=@Notes or @Notes='' )
and (tbl_JournalVoucherHeader.JVNumber=@JVNumber or @JVNumber='' ) order by jvnumber asc", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public DataTable SelectJournalVoucherHeaderForPrint(string guid, int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, int CompanyID, DateTime Date1, DateTime Date2)
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
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select tbl_JournalVoucherHeader.*,tbl_Branch.aname as BranchName,tbl_CostCenter.aname as CostCenterName 
,tbl_employee.AName as EmployeeAName
from tbl_JournalVoucherHeader 
 left join tbl_Branch on tbl_Branch.ID = tbl_JournalVoucherHeader.BranchID
  left join tbl_CostCenter on tbl_CostCenter.ID = tbl_JournalVoucherHeader.CostCenterID
  left join tbl_employee on tbl_employee.ID = tbl_JournalVoucherHeader.CreationUserId
where (tbl_JournalVoucherHeader.guid=@guid or @guid='00000000-0000-0000-0000-000000000000' )
and (tbl_JournalVoucherHeader.BranchID=@BranchID or @BranchID=0 )
and (tbl_JournalVoucherHeader.CostCenterID=@CostCenterID or @CostCenterID=0 )
and (tbl_JournalVoucherHeader.JVTypeID=@JVTypeID or @JVTypeID=0 )
and (tbl_JournalVoucherHeader.CompanyID=@CompanyID or @CompanyID=0 )
and (cast(tbl_JournalVoucherHeader.VoucherDate as date) between cast( @date1 as date) and cast( @date2 as date))
and (tbl_JournalVoucherHeader.Notes=@Notes or @Notes='' )
and (tbl_JournalVoucherHeader.JVNumber=@JVNumber or @JVNumber='' ) order by jvnumber asc", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public bool DeleteJournalVoucherHeaderByID(string guid, int CompanyID,SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid ) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_JournalVoucherHeader where (guid=@guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                if (A > 0)
                    clsAuditService.LogDelete(0, CompanyID, "JournalVoucher", "tbl_JournalVoucherHeader", 0, guid);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, int CompanyID, DateTime VoucherDate, int CreationUserId,string RelatedFinancingHeaderGuid, int RelatedLoanTypeID,SqlTransaction trn = null, int documentStatus = 2)
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
                       new SqlParameter("@RelatedFinancingHeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( RelatedFinancingHeaderGuid ) },

                      new SqlParameter("@RelatedLoanTypeID", SqlDbType.Int) { Value = RelatedLoanTypeID },
                      new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = documentStatus },
                };

                string a = @"insert into tbl_JournalVoucherHeader(Notes,BranchID,CostCenterID,JVNumber,JVTypeID,CompanyID,VoucherDate,CreationUserId,CreationDate,RelatedFinancingHeaderGuid,RelatedLoanTypeID,DocumentStatus) 
                                       OUTPUT INSERTED.guid values(@Notes,@BranchID,@CostCenterID,@JVNumber,@JVTypeID,@CompanyID,@VoucherDate,@CreationUserId,@CreationDate,@RelatedFinancingHeaderGuid,@RelatedLoanTypeID,@DocumentStatus)";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                {
                    var insertedGuid = Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                    if (!string.IsNullOrEmpty(insertedGuid))
                        clsAuditService.LogInsert(CreationUserId, CompanyID, "JournalVoucher", "tbl_JournalVoucherHeader", 0, JVNumber);
                    return insertedGuid;
                }
                else
                {
                    var insertedGuid = Simulate.String(clsSQL.ExecuteScalar(a,  prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
                    if (!string.IsNullOrEmpty(insertedGuid))
                        clsAuditService.LogInsert(CreationUserId, CompanyID, "JournalVoucher", "tbl_JournalVoucherHeader", 0, JVNumber);
                    return insertedGuid;
                }

            }
            catch (Exception)
            {

                throw;
            }


        }
        public string UpdateJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, DateTime VoucherDate, string guid, int ModificationUserId, string RelatedFinancingHeaderGuid, int RelatedLoanTypeID,int CompanyID, SqlTransaction trn)
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
                           new SqlParameter("@RelatedFinancingHeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( RelatedFinancingHeaderGuid ) },

                      new SqlParameter("@RelatedLoanTypeID", SqlDbType.Int) { Value = RelatedLoanTypeID },
                };
                int rows = clsSQL.ExecuteNonQueryStatement(@"update tbl_JournalVoucherHeader set 
Notes=@Notes,
JVNumber=@JVNumber,
BranchID=@BranchID,
CostCenterID=@CostCenterID,
JVTypeID=@JVTypeID,
 
VoucherDate=@VoucherDate,



ModificationDate=@ModificationDate,
ModificationUserId=@ModificationUserId ,
 RelatedFinancingHeaderGuid=@RelatedFinancingHeaderGuid,
 RelatedLoanTypeID=@RelatedLoanTypeID
where guid =@guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                if (rows > 0)
                    clsAuditService.LogUpdate(ModificationUserId, CompanyID, "JournalVoucher", "tbl_JournalVoucherHeader", 0, JVNumber);

                return Simulate.String(rows);
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
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select isnull(max(CONVERT(INT, JVNumber)),0) from tbl_JournalVoucherHeader where 
(guid=@guid or @guid='00000000-0000-0000-0000-000000000000' )
 
and (JVTypeID=@JVTypeID or @JVTypeID=0 )
and (CompanyID=@CompanyID or @CompanyID=0 )
 ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public bool CheckJVMatch(string JVID, int CompanyID,SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId(JVID, 0, 0,0,0, 0, CompanyID, trn);
                if (dt != null && dt.Rows.Count > 0)
                {

                    decimal TotalDebit = 0;
                    decimal TotalCredit = 0;
                    decimal TotalLine = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Simulate.Integer32(dt.Rows[i]["Accountid"]) > 0) { 
                        TotalDebit = TotalDebit + Simulate.decimal_(dt.Rows[i]["Debit"]);
                        TotalCredit = TotalCredit + Simulate.decimal_(dt.Rows[i]["Credit"]);
                        TotalLine = TotalLine + Simulate.decimal_(dt.Rows[i]["Total"]);
                        }

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

        public string SelectLatestJournalVoucherGuid(int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    select top 1 cast(Guid as nvarchar(50)) as Guid
                    from tbl_JournalVoucherHeader
                    where CompanyID = @CompanyID
                    order by VoucherDate desc, CreationDate desc
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);

                if (dt != null && dt.Rows.Count > 0)
                    return Simulate.String(dt.Rows[0]["Guid"]);

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool UpdateDocumentStatus(string guid, int documentStatus, int userId, int companyId, SqlTransaction trn = null)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(guid) },
                new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = documentStatus },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            string sql = @"
UPDATE tbl_JournalVoucherHeader SET
    DocumentStatus = @DocumentStatus,
    ModificationUserId = @UserId,
    ModificationDate = GETDATE(),
    PostedDate = CASE WHEN @DocumentStatus = 2 THEN GETDATE() ELSE PostedDate END,
    PostedByUserId = CASE WHEN @DocumentStatus = 2 THEN @UserId ELSE PostedByUserId END,
    SubmittedByUserId = CASE WHEN @DocumentStatus = 1 THEN @UserId ELSE SubmittedByUserId END,
    SubmittedDate = CASE WHEN @DocumentStatus = 1 THEN GETDATE() ELSE SubmittedDate END
WHERE Guid = @Guid AND CompanyID = @CompanyID";

            int rows = clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
            return rows > 0;
        }

        public decimal GetJournalVoucherAmount(string guid, int companyId, SqlTransaction trn = null)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(guid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return Simulate.Decimal(clsSQL.ExecuteScalar(@"
SELECT ISNULL(SUM(Debit), 0)
FROM tbl_JournalVoucherDetails
WHERE ParentGuid = @Guid AND CompanyID = @CompanyID",
                prm, clsSQL.CreateDataBaseConnectionString(companyId), trn));
        }

        public DataTable SelectJournalVoucherHeaderByGuid(string guid, int companyId, SqlTransaction trn = null)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(guid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return clsSQL.ExecuteQueryStatement(
                "SELECT TOP 1 * FROM tbl_JournalVoucherHeader WHERE Guid = @Guid AND CompanyID = @CompanyID",
                clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
        }
    }
}
