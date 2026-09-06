using System;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsMO
    {
        // =========================================================
        // HEADER
        // =========================================================
        public DataTable SelectMOProgress(string moGuid, int companyID)
        {
            clsSQL clsSQL = new clsSQL();
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID))) // <-- use your connection string holder
            using (SqlCommand cmd = new SqlCommand(@"
DECLARE @MOGuid UNIQUEIDENTIFIER = @pMOGuid;
DECLARE @CompanyID INT = @pCompanyID;

DECLARE @INPUT_ID  INT = 25;
DECLARE @OUTPUT_ID INT = 26;

;WITH Planned AS
(
    SELECT
        d.ItemGuid,
        MAX(d.ItemName) AS ItemName,
        CASE
            WHEN d.LineTypeID = @INPUT_ID THEN 'INPUT'
            WHEN d.LineTypeID = @OUTPUT_ID THEN 'OUTPUT'
            ELSE 'OTHER'
        END AS [Type],
        SUM(ISNULL(d.PlannedQty,0)) AS PlannedQty
    FROM dbo.tbl_MODetails d
    WHERE d.HeaderGuid = @MOGuid
      AND d.CompanyID = @CompanyID
      AND d.LineTypeID IN (@INPUT_ID, @OUTPUT_ID)
    GROUP BY d.ItemGuid, d.LineTypeID
),
Actual AS
(
    SELECT
        det.ItemGuid,
        MAX(det.ItemName) AS ItemName,
        CASE
            WHEN l.LinkTypeID = @INPUT_ID THEN 'INPUT'
            WHEN l.LinkTypeID = @OUTPUT_ID THEN 'OUTPUT'
            ELSE 'OTHER'
        END AS [Type],
        SUM(ISNULL(det.Qty,0)) AS ActualQty,
        SUM(CASE WHEN l.LinkTypeID = @INPUT_ID THEN ISNULL(det.TotalLine,0) ELSE 0 END) AS ActualCost,
        SUM(CASE WHEN l.LinkTypeID = @INPUT_ID THEN ISNULL(det.TotalLine,0) ELSE 0 END)
            / NULLIF(SUM(CASE WHEN l.LinkTypeID = @INPUT_ID THEN ISNULL(det.Qty,0) ELSE 0 END), 0) AS ActualUnitCost
    FROM dbo.tbl_MOInvoiceLink l
    INNER JOIN dbo.tbl_InvoiceHeader h
        ON h.Guid = l.InvoiceHeaderGuid
       AND h.CompanyID = @CompanyID
    INNER JOIN dbo.tbl_InvoiceDetails det
        ON det.HeaderGuid = h.Guid
       AND det.CompanyID = @CompanyID
    WHERE (l.MOGuid = @MOGuid or  @MOGuid='00000000-0000-0000-0000-000000000000') 
      AND l.CompanyID = @CompanyID
      AND l.LinkTypeID IN (@INPUT_ID, @OUTPUT_ID)
    GROUP BY det.ItemGuid, l.LinkTypeID
)
SELECT
    COALESCE(p.ItemGuid, a.ItemGuid) AS ItemGuid,
    COALESCE(p.ItemName, a.ItemName) AS ItemName,
    COALESCE(p.[Type], a.[Type]) AS [Type],
    CAST(ISNULL(p.PlannedQty,0) AS DECIMAL(18,3)) AS PlannedQty,
    CAST(ISNULL(a.ActualQty,0) AS DECIMAL(18,3)) AS ActualQty,
    CAST(
        CASE
          WHEN COALESCE(p.[Type], a.[Type]) = 'INPUT'
            THEN ISNULL(p.PlannedQty,0) * ISNULL(a.ActualUnitCost,0)
          ELSE 0
        END
    AS DECIMAL(18,3)) AS PlannedCost,
    CAST(ISNULL(a.ActualCost,0) AS DECIMAL(18,3)) AS ActualCost
FROM Planned p
FULL OUTER JOIN Actual a
    ON a.ItemGuid = p.ItemGuid
   AND a.[Type] = p.[Type]
WHERE COALESCE(p.[Type], a.[Type]) IN ('INPUT','OUTPUT')
ORDER BY
    CASE COALESCE(p.[Type], a.[Type]) WHEN 'INPUT' THEN 25 ELSE 26 END,
    COALESCE(p.ItemName, a.ItemName);
", con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@pMOGuid", SqlDbType.UniqueIdentifier).Value = Simulate.Guid(moGuid);
                cmd.Parameters.Add("@pCompanyID", SqlDbType.Int).Value = companyID;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable SelectMOHeader(string Guid, string MOCode, string MOName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.NVarChar, -1) { Value = Guid ?? "" },
                    new SqlParameter("@MOCode", SqlDbType.NVarChar, -1) { Value = MOCode ?? "" },
                    new SqlParameter("@MOName", SqlDbType.NVarChar, -1) { Value = MOName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_MOHeader
                    where (cast([Guid] as nvarchar(max))=@Guid or @Guid='')
                      and (MOCode=@MOCode or @MOCode='')
                      and (MOName=@MOName or @MOName='')
                      and (CompanyID=@CompanyID or @CompanyID=0)
                    order by CreationDate desc
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteMOByGuid(string Guid, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(Guid) },
                }; SqlParameter[] prm1 =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(Guid) },
                }; SqlParameter[] prm2 =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(Guid) },
                };

                if (trn == null)
                {
                    // children first
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MODetails where HeaderGuid=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MOInvoiceLink where MOGuid=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm1);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MOHeader where [Guid]=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm2);
                }
                else
                {
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MODetails where HeaderGuid=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MOInvoiceLink where MOGuid=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm1, trn);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MOHeader where [Guid]=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm2, trn);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string InsertMOHeader(
            string MOCode,
            string MOName,
            int BOMID,
            decimal PlannedQty,
            decimal BatchQty,
            DateTime? MODate,
            DateTime? PlannedStartDate,
            DateTime? PlannedEndDate,
            int StatusID,
            int BranchID,
            int StoreID,
            string Notes,
            bool IsActive,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                Guid newGuid = Guid.NewGuid();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = newGuid },
                    new SqlParameter("@MOCode", SqlDbType.NVarChar, -1) { Value = MOCode ?? "" },
                    new SqlParameter("@MOName", SqlDbType.NVarChar, -1) { Value = MOName ?? "" },

                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID },

                    new SqlParameter("@PlannedQty", SqlDbType.Decimal) { Value = PlannedQty },
                    new SqlParameter("@BatchQty", SqlDbType.Decimal) { Value = BatchQty },

                    new SqlParameter("@MODate", SqlDbType.DateTime) { Value = (object?)MODate ?? DBNull.Value },
                    new SqlParameter("@PlannedStartDate", SqlDbType.DateTime) { Value = (object?)PlannedStartDate ?? DBNull.Value },
                    new SqlParameter("@PlannedEndDate", SqlDbType.DateTime) { Value = (object?)PlannedEndDate ?? DBNull.Value },

                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = StatusID },

                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },

                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    insert into tbl_MOHeader
                    ([Guid],MOCode,MOName,BOMID,PlannedQty,BatchQty,MODate,PlannedStartDate,PlannedEndDate,StatusID,BranchID,StoreID,Notes,IsActive,CompanyID,CreationUserId,CreationDate)
                    output inserted.[Guid]
                    values
                    (@Guid,@MOCode,@MOName,@BOMID,@PlannedQty,@BatchQty,@MODate,@PlannedStartDate,@PlannedEndDate,@StatusID,@BranchID,@StoreID,@Notes,@IsActive,@CompanyID,@CreationUserId,@CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();
                object o;
                if (trn == null)
                    o = clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID));
                else
                    o = clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn);

                return Simulate.String(o);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateMOHeader(
            string Guid,
            string MOCode,
            string MOName,
            int BOMID,
            decimal PlannedQty,
            decimal BatchQty,
            DateTime? MODate,
            DateTime? PlannedStartDate,
            DateTime? PlannedEndDate,
            int StatusID,
            int BranchID,
            int StoreID,
            string Notes,
            bool IsActive,
            int ModificationUserId,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(Guid) },

                    new SqlParameter("@MOCode", SqlDbType.NVarChar, -1) { Value = MOCode ?? "" },
                    new SqlParameter("@MOName", SqlDbType.NVarChar, -1) { Value = MOName ?? "" },

                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID },

                    new SqlParameter("@PlannedQty", SqlDbType.Decimal) { Value = PlannedQty },
                    new SqlParameter("@BatchQty", SqlDbType.Decimal) { Value = BatchQty },

                    new SqlParameter("@MODate", SqlDbType.DateTime) { Value = (object?)MODate ?? DBNull.Value },
                    new SqlParameter("@PlannedStartDate", SqlDbType.DateTime) { Value = (object?)PlannedStartDate ?? DBNull.Value },
                    new SqlParameter("@PlannedEndDate", SqlDbType.DateTime) { Value = (object?)PlannedEndDate ?? DBNull.Value },

                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = StatusID },

                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },

                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },

                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    update tbl_MOHeader set
                        MOCode=@MOCode,
                        MOName=@MOName,
                        BOMID=@BOMID,
                        PlannedQty=@PlannedQty,
                        BatchQty=@BatchQty,
                        MODate=@MODate,
                        PlannedStartDate=@PlannedStartDate,
                        PlannedEndDate=@PlannedEndDate,
                        StatusID=@StatusID,
                        BranchID=@BranchID,
                        StoreID=@StoreID,
                        Notes=@Notes,
                        IsActive=@IsActive,
                        ModificationUserId=@ModificationUserId,
                        ModificationDate=@ModificationDate
                    where [Guid]=@Guid
                ";

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }


        // =========================================================
        // DETAILS
        // =========================================================

        public DataTable SelectMODetailsByMOGuid(string MOGuid, int CompanyID, int LineTypeID = 0)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(MOGuid) },
                    new SqlParameter("@LineTypeID", SqlDbType.Int) { Value = LineTypeID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_MODetails
                    where HeaderGuid=@HeaderGuid
                      and (LineTypeID=@LineTypeID or @LineTypeID=0)
                      and (CompanyID=@CompanyID or @CompanyID=0)
                    order by LineTypeID, RowIndex, CreationDate
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteMODetailsByMOGuid(string MOGuid, int CompanyID, int LineTypeID = 0, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(MOGuid) },
                    new SqlParameter("@LineTypeID", SqlDbType.Int) { Value = LineTypeID },
                };

                string q = @"
                    delete from tbl_MODetails
                    where HeaderGuid=@HeaderGuid
                      and (LineTypeID=@LineTypeID or @LineTypeID=0)
                ";

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string InsertMODetail(
            string MOGuid,
            int RowIndex,
            int LineTypeID,
            Guid ItemGuid,
            string ItemName,
            decimal PlannedQty,
            int UOMID,
            decimal ScrapPercent,
            decimal CostSharePercent,
            int BOMLineNo,
            int BranchID,
            int StoreID,
            string Notes,
            bool TrackLot,
            bool TrackSerial,
            bool TrackExpiryDate,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                Guid lineGuid = Guid.NewGuid();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = lineGuid },
                    new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(MOGuid) },

                    new SqlParameter("@RowIndex", SqlDbType.Int) { Value = RowIndex },
                    new SqlParameter("@LineTypeID", SqlDbType.Int) { Value = LineTypeID },

                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                    new SqlParameter("@ItemName", SqlDbType.NVarChar, -1) { Value = ItemName ?? "" },

                    new SqlParameter("@PlannedQty", SqlDbType.Decimal) { Value = PlannedQty },
                    new SqlParameter("@UOMID", SqlDbType.Int) { Value = UOMID },

                    new SqlParameter("@ScrapPercent", SqlDbType.Decimal) { Value = ScrapPercent },
                    new SqlParameter("@CostSharePercent", SqlDbType.Decimal) { Value = CostSharePercent },

                    new SqlParameter("@BOMLineNo", SqlDbType.Int) { Value = BOMLineNo },

                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },

                    new SqlParameter("@TrackLot", SqlDbType.Bit) { Value = TrackLot },
                    new SqlParameter("@TrackSerial", SqlDbType.Bit) { Value = TrackSerial },
                    new SqlParameter("@TrackExpiryDate", SqlDbType.Bit) { Value = TrackExpiryDate },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    insert into tbl_MODetails
                    ([Guid],HeaderGuid,RowIndex,LineTypeID,ItemGuid,ItemName,PlannedQty,UOMID,ScrapPercent,CostSharePercent,BOMLineNo,BranchID,StoreID,Notes,TrackLot,TrackSerial,TrackExpiryDate,CompanyID,CreationUserId,CreationDate)
                    output inserted.[Guid]
                    values
                    (@Guid,@HeaderGuid,@RowIndex,@LineTypeID,@ItemGuid,@ItemName,@PlannedQty,@UOMID,@ScrapPercent,@CostSharePercent,@BOMLineNo,@BranchID,@StoreID,@Notes,@TrackLot,@TrackSerial,@TrackExpiryDate,@CompanyID,@CreationUserId,@CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();
                object o;
                if (trn == null)
                    o = clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID));
                else
                    o = clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn);

                return Simulate.String(o);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateMODetail(
            string Guid,
            int RowIndex,
            int LineTypeID,
            Guid ItemGuid,
            string ItemName,
            decimal PlannedQty,
            int UOMID,
            decimal ScrapPercent,
            decimal CostSharePercent,
            int BOMLineNo,
            int BranchID,
            int StoreID,
            string Notes,
            bool TrackLot,
            bool TrackSerial,
            bool TrackExpiryDate,
            int ModificationUserId,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(Guid) },

                    new SqlParameter("@RowIndex", SqlDbType.Int) { Value = RowIndex },
                    new SqlParameter("@LineTypeID", SqlDbType.Int) { Value = LineTypeID },

                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                    new SqlParameter("@ItemName", SqlDbType.NVarChar, -1) { Value = ItemName ?? "" },

                    new SqlParameter("@PlannedQty", SqlDbType.Decimal) { Value = PlannedQty },
                    new SqlParameter("@UOMID", SqlDbType.Int) { Value = UOMID },

                    new SqlParameter("@ScrapPercent", SqlDbType.Decimal) { Value = ScrapPercent },
                    new SqlParameter("@CostSharePercent", SqlDbType.Decimal) { Value = CostSharePercent },

                    new SqlParameter("@BOMLineNo", SqlDbType.Int) { Value = BOMLineNo },

                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },

                    new SqlParameter("@TrackLot", SqlDbType.Bit) { Value = TrackLot },
                    new SqlParameter("@TrackSerial", SqlDbType.Bit) { Value = TrackSerial },
                    new SqlParameter("@TrackExpiryDate", SqlDbType.Bit) { Value = TrackExpiryDate },

                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    update tbl_MODetails set
                        RowIndex=@RowIndex,
                        LineTypeID=@LineTypeID,
                        ItemGuid=@ItemGuid,
                        ItemName=@ItemName,
                        PlannedQty=@PlannedQty,
                        UOMID=@UOMID,
                        ScrapPercent=@ScrapPercent,
                        CostSharePercent=@CostSharePercent,
                        BOMLineNo=@BOMLineNo,
                        BranchID=@BranchID,
                        StoreID=@StoreID,
                        Notes=@Notes,
                        TrackLot=@TrackLot,
                        TrackSerial=@TrackSerial,
                        TrackExpiryDate=@TrackExpiryDate,
                        ModificationUserId=@ModificationUserId,
                        ModificationDate=@ModificationDate
                    where [Guid]=@Guid
                ";

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteMODetailByGuid(string Guid, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(Guid) },
                };

                if (trn == null)
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MODetails where [Guid]=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MODetails where [Guid]=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // MO ↔ Invoice Link
        // =========================================================

        public DataTable SelectMOInvoiceLinks(string MOGuid, int CompanyID, int LinkTypeID = 0)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@MOGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(MOGuid) },
                    new SqlParameter("@LinkTypeID", SqlDbType.Int) { Value = LinkTypeID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_MOInvoiceLink
                    where MOGuid=@MOGuid
                      and (LinkTypeID=@LinkTypeID or @LinkTypeID=0)
                      and (CompanyID=@CompanyID or @CompanyID=0)
                    order by CreationDate desc
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string InsertMOInvoiceLink(
            string MOGuid,
            string InvoiceHeaderGuid,
            int LinkTypeID,
            string Notes,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                // FG receipt only after all planned materials issued (before link insert)
                if (LinkTypeID == (int)clsEnum.VoucherType.manufacturingOrderOutput)
                {
                    new clsManufacturingOps().AssertInputsFullyIssued(MOGuid, CompanyID, trn);
                }

                Guid linkGuid = Guid.NewGuid();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = linkGuid },
                    new SqlParameter("@MOGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(MOGuid) },
                    new SqlParameter("@InvoiceHeaderGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(InvoiceHeaderGuid) },
                    new SqlParameter("@LinkTypeID", SqlDbType.Int) { Value = LinkTypeID },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    insert into tbl_MOInvoiceLink
                    ([Guid],MOGuid,InvoiceHeaderGuid,LinkTypeID,Notes,CompanyID,CreationUserId,CreationDate)
                    output inserted.[Guid]
                    values
                    (@Guid,@MOGuid,@InvoiceHeaderGuid,@LinkTypeID,@Notes,@CompanyID,@CreationUserId,@CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();
                object o;
                if (trn == null)
                    o = clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID));
                else
                    o = clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn);

                // First material issue promotes Released -> In Progress
                if (LinkTypeID == (int)clsEnum.VoucherType.manufacturingOrderInput)
                {
                    new clsManufacturingOps().PromoteMoInProgress(MOGuid, CompanyID, trn);
                }

                return Simulate.String(o);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteMOInvoiceLinkByGuid(string Guid, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(Guid) },
                };

                if (trn == null)
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MOInvoiceLink where [Guid]=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_MOInvoiceLink where [Guid]=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // REPORT 1: MO SUMMARY (Header + totals of planned inputs/outputs)
        // =========================================================
        public DataTable SelectMOSummary(string MOGuid, string MOCode, string MOName, int CompanyID,
                                 string DateFrom, string DateTo)
        {
            try
            {
                SqlParameter[] prm =
                {
            new SqlParameter("@MOGuid", SqlDbType.NVarChar,-1) { Value = Simulate.String(MOGuid) },
            new SqlParameter("@MOCode", SqlDbType.NVarChar,-1) { Value = Simulate.String(MOCode) },
            new SqlParameter("@MOName", SqlDbType.NVarChar,-1) { Value = Simulate.String(MOName) },
            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

            new SqlParameter("@DateFrom", SqlDbType.NVarChar,-1) { Value = Simulate.String(DateFrom) },
            new SqlParameter("@DateTo", SqlDbType.NVarChar,-1) { Value = Simulate.String(DateTo) },

            new SqlParameter("@InputTypeID", SqlDbType.Int) { Value = (int)clsEnum.VoucherType.manufacturingOrderInput },
            new SqlParameter("@OutputTypeID", SqlDbType.Int) { Value = (int)clsEnum.VoucherType.manufacturingOrderOutput },
        };

                string sql = @"
SELECT
    h.Guid AS MOGuid,
    h.MOCode,
    h.MOName,
    h.MODate,
    h.StatusID,
    h.BranchID,
    h.StoreID,
    h.PlannedQty,
    h.BatchQty,
    (ISNULL(h.PlannedQty,0) * ISNULL(h.BatchQty,0)) AS TotalMultiplier,

    SUM(CASE WHEN d.LineTypeID = @InputTypeID  THEN ISNULL(d.PlannedQty,0) ELSE 0 END) AS PlannedInputQty,
    SUM(CASE WHEN d.LineTypeID = @OutputTypeID THEN ISNULL(d.PlannedQty,0) ELSE 0 END) AS PlannedOutputQty,

    COUNT(CASE WHEN d.LineTypeID = @InputTypeID  THEN 1 END) AS InputLines,
    COUNT(CASE WHEN d.LineTypeID = @OutputTypeID THEN 1 END) AS OutputLines

FROM tbl_MOHeader h
LEFT JOIN tbl_MODetails d
    ON d.HeaderGuid = h.Guid
    AND (d.CompanyID = h.CompanyID OR @CompanyID = 0)

WHERE
    (h.CompanyID = @CompanyID OR @CompanyID = 0)
    AND (@MOGuid = '' OR CAST(h.Guid AS nvarchar(50)) = @MOGuid)
    AND (@MOCode = '' OR h.MOCode LIKE '%' + @MOCode + '%')
    AND (@MOName = '' OR h.MOName LIKE '%' + @MOName + '%')
    AND (@DateFrom = '' OR h.MODate >= TRY_CONVERT(datetime, @DateFrom))
    AND (@DateTo   = '' OR h.MODate <  DATEADD(day,1, TRY_CONVERT(datetime, @DateTo)))

GROUP BY
    h.Guid, h.MOCode, h.MOName, h.MODate, h.StatusID, h.BranchID, h.StoreID, h.PlannedQty, h.BatchQty
ORDER BY h.MODate DESC, h.MOCode DESC;
";

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }


        // =========================================================
        // REPORT 2: MO PROGRESS (Planned vs Actual by Item)
        // Actual based on TblMOInvoiceLink + Invoice Header/Details
        // =========================================================


        // =========================================================
        // REPORT 3: MO VOUCHERS DETAILS (Links + Invoice Header/Lines)
        // =========================================================
        public DataTable SelectMOVouchers(string MOGuid, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
            new SqlParameter("@MOGuid", SqlDbType.NVarChar,-1) { Value = Simulate.String(MOGuid) },
            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
        };

                string sql = @"
SELECT
    l.Guid AS LinkGuid,
    l.MOGuid,
    l.InvoiceHeaderGuid,
    l.LinkTypeID,
    l.Notes AS LinkNotes,
    l.CreationDate AS LinkCreationDate,

    ih.Guid AS InvoiceGuid,
    ih.InvoiceNo,
    ih.InvoiceDate,
    ih.InvoiceTypeID,
    ih.Status AS InvoiceStatus,
    ih.IsPosted,
    ih.BranchID,
    ih.StoreID,
    ih.Note AS InvoiceNote,
    ih.TotalInvoice,

    id.Guid AS LineGuid,
    id.RowIndex,
    id.ItemGuid,
    id.ItemName,
    id.Qty,
    id.UOMID,
    id.PriceBeforeTax,
    id.TaxAmount,
    id.SpecialTaxAmount,
    id.PriceAfterTaxPcs,
    id.TotalLine,
    id.AVGCostPerUnit

FROM tbl_MOInvoiceLink l
INNER JOIN tbl_InvoiceHeader ih
    ON ih.Guid = l.InvoiceHeaderGuid
    AND (ih.CompanyID = l.CompanyID OR @CompanyID = 0)

INNER JOIN tbl_InvoiceDetails id
    ON id.HeaderGuid = ih.Guid
    AND (id.CompanyID = ih.CompanyID OR @CompanyID = 0)

WHERE
    (l.CompanyID = @CompanyID OR @CompanyID = 0)
    AND (@MOGuid = '' OR CAST(l.MOGuid AS nvarchar(50)) = @MOGuid)

ORDER BY ih.InvoiceDate DESC, ih.InvoiceNo DESC, id.RowIndex ASC;
";

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // MO ROUTING
        // =========================================================
        public DataTable SelectMORoutingByMOGuid(string moGuid, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@MOGuid", SqlDbType.NVarChar, -1) { Value = moGuid ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    SELECT * FROM tbl_MORouting
                    WHERE (CAST(MOGuid AS nvarchar(50)) = @MOGuid OR @MOGuid = '')
                      AND CompanyID = @CompanyID
                    ORDER BY LineNo
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteMORoutingByMOGuid(string moGuid, int companyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@MOGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(moGuid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                if (trn == null)
                {
                    clsSQL.ExecuteNonQueryStatement(
                        "DELETE FROM tbl_MORouting WHERE MOGuid = @MOGuid AND CompanyID = @CompanyID",
                        clsSQL.CreateDataBaseConnectionString(companyID), prm);
                }
                else
                {
                    clsSQL.ExecuteNonQueryStatement(
                        "DELETE FROM tbl_MORouting WHERE MOGuid = @MOGuid AND CompanyID = @CompanyID",
                        clsSQL.CreateDataBaseConnectionString(companyID), prm, trn);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string InsertMORouting(
            string moGuid,
            int lineNo,
            int workCenterID,
            string operationName,
            decimal plannedHours,
            decimal actualHours,
            int statusID,
            DateTime? plannedStart,
            DateTime? plannedEnd,
            string notes,
            int companyID,
            int creationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                string newGuid = Guid.NewGuid().ToString();
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid(newGuid) },
                    new SqlParameter("@MOGuid", SqlDbType.UniqueIdentifier) { Value = new Guid(moGuid) },
                    new SqlParameter("@LineNo", SqlDbType.Int) { Value = lineNo },
                    new SqlParameter("@WorkCenterID", SqlDbType.Int) { Value = workCenterID },
                    new SqlParameter("@OperationName", SqlDbType.NVarChar, -1) { Value = operationName ?? "" },
                    new SqlParameter("@PlannedHours", SqlDbType.Decimal) { Value = plannedHours },
                    new SqlParameter("@ActualHours", SqlDbType.Decimal) { Value = actualHours },
                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = statusID },
                    new SqlParameter("@PlannedStart", SqlDbType.DateTime) { Value = (object)plannedStart ?? DBNull.Value },
                    new SqlParameter("@PlannedEnd", SqlDbType.DateTime) { Value = (object)plannedEnd ?? DBNull.Value },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = notes ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = creationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    INSERT INTO tbl_MORouting
                    (Guid, MOGuid, LineNo, WorkCenterID, OperationName, PlannedHours, ActualHours,
                     StatusID, PlannedStart, PlannedEnd, Notes, CompanyID, CreationUserId, CreationDate)
                    VALUES
                    (@Guid, @MOGuid, @LineNo, @WorkCenterID, @OperationName, @PlannedHours, @ActualHours,
                     @StatusID, @PlannedStart, @PlannedEnd, @Notes, @CompanyID, @CreationUserId, @CreationDate)";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyID), prm);
                }
                else
                {
                    clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyID), prm, trn);
                }

                return newGuid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // MRP SUGGESTIONS
        // =========================================================
        public DataTable SelectMRPSuggestions(int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                string sql = @"
;WITH Stock AS (
    SELECT
        d.ItemGuid,
        SUM(CASE WHEN h.InvoiceTypeID IN (8, 26) THEN ISNULL(d.Qty, 0)
                 WHEN h.InvoiceTypeID IN (9, 25) THEN -ISNULL(d.Qty, 0)
                 ELSE 0 END) AS CurrentQty
    FROM tbl_InvoiceDetails d
    INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid AND h.CompanyID = d.CompanyID
    WHERE d.CompanyID = @CompanyID AND h.IsPosted = 1
    GROUP BY d.ItemGuid
),
BOMOut AS (
    SELECT
        bh.ID AS BOMID,
        bh.BOMCode,
        bo.OutputItemGuid,
        MAX(i.AName) AS ItemName,
        MAX(bo.Qty) AS OutputQtyPerBatch,
        MAX(bh.BatchQty) AS BatchQty
    FROM tbl_BOMHeader bh
    INNER JOIN tbl_BOMOutput bo ON bo.BOMID = bh.ID AND bo.CompanyID = bh.CompanyID
    INNER JOIN tbl_Items i ON i.Guid = bo.OutputItemGuid
    WHERE bh.CompanyID = @CompanyID AND bh.IsActive = 1
    GROUP BY bh.ID, bh.BOMCode, bo.OutputItemGuid
)
SELECT
    b.BOMID,
    b.BOMCode,
    b.OutputItemGuid AS ItemGuid,
    b.ItemName,
    CAST(ISNULL(s.CurrentQty, 0) AS DECIMAL(18,3)) AS CurrentQty,
    CAST(ISNULL(i.MinimumLimit, 0) AS DECIMAL(18,3)) AS RequiredQty,
    CAST(
        CASE
            WHEN ISNULL(s.CurrentQty, 0) >= ISNULL(i.MinimumLimit, 0) THEN 0
            ELSE CEILING((ISNULL(i.MinimumLimit, 0) - ISNULL(s.CurrentQty, 0)) / NULLIF(b.OutputQtyPerBatch / NULLIF(b.BatchQty, 0), 0))
        END
    AS DECIMAL(18,3)) AS SuggestedMOQty
FROM BOMOut b
INNER JOIN tbl_Items i ON i.Guid = b.OutputItemGuid
LEFT JOIN Stock s ON s.ItemGuid = b.OutputItemGuid
WHERE ISNULL(s.CurrentQty, 0) < ISNULL(i.MinimumLimit, 0)
ORDER BY b.BOMCode, b.ItemName";

                return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // SCHEDULING BOARD
        // =========================================================
        public DataTable SelectMOSchedulingBoard(int companyID, int branchID, int workCenterID, string dateFrom, string dateTo)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchID },
                    new SqlParameter("@WorkCenterID", SqlDbType.Int) { Value = workCenterID },
                    new SqlParameter("@DateFrom", SqlDbType.NVarChar, -1) { Value = dateFrom ?? "" },
                    new SqlParameter("@DateTo", SqlDbType.NVarChar, -1) { Value = dateTo ?? "" },
                };

                clsSQL clsSQL = new clsSQL();
                string sql = @"
SELECT
    h.Guid AS MOGuid,
    h.MOCode,
    h.MOName,
    h.StatusID,
    h.BranchID,
    h.PlannedStartDate,
    h.PlannedEndDate,
    r.Guid AS RoutingGuid,
    r.LineNo,
    r.WorkCenterID,
    wc.AName AS WorkCenterName,
    ISNULL(wc.CapacityPerDay, 0) AS CapacityPerDay,
    r.OperationName,
    r.PlannedHours,
    r.ActualHours,
    r.StatusID AS RoutingStatusID,
    r.PlannedStart,
    r.PlannedEnd,
    CASE
        WHEN ISNULL(wc.CapacityPerDay, 0) <= 0 THEN CAST(0 AS bit)
        WHEN ISNULL(r.PlannedHours, 0) >
             ISNULL(wc.CapacityPerDay, 0) *
             NULLIF(DATEDIFF(day,
                ISNULL(r.PlannedStart, h.PlannedStartDate),
                ISNULL(r.PlannedEnd, h.PlannedEndDate)) + 1, 0)
            THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS IsOverloaded
FROM tbl_MOHeader h
LEFT JOIN tbl_MORouting r ON r.MOGuid = h.Guid AND r.CompanyID = h.CompanyID
LEFT JOIN tbl_WorkCenter wc ON wc.ID = r.WorkCenterID AND wc.CompanyID = h.CompanyID
WHERE h.CompanyID = @CompanyID
  AND (@BranchID = 0 OR h.BranchID = @BranchID)
  AND (@WorkCenterID = 0 OR r.WorkCenterID = @WorkCenterID)
  AND (@DateFrom = '' OR h.PlannedStartDate >= TRY_CONVERT(datetime, @DateFrom))
  AND (@DateTo = '' OR h.PlannedEndDate <= TRY_CONVERT(datetime, @DateTo))
ORDER BY h.PlannedStartDate, r.LineNo";

                return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // MANUFACTURING DASHBOARD
        // =========================================================
        public DataTable SelectMODashboardSummary(int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                string sql = @"
;WITH OpenMO AS (
    SELECT Guid, StatusID, PlannedEndDate, PlannedStartDate, MODate
    FROM tbl_MOHeader
    WHERE CompanyID = @CompanyID AND IsActive = 1
),
Progress AS (
    SELECT
        d.HeaderGuid AS MOGuid,
        d.LineTypeID,
        SUM(ISNULL(d.PlannedQty, 0)) AS PlannedQty
    FROM tbl_MODetails d
    INNER JOIN OpenMO h ON h.Guid = d.HeaderGuid
    WHERE d.CompanyID = @CompanyID AND d.LineTypeID IN (25, 26)
    GROUP BY d.HeaderGuid, d.LineTypeID
),
Actual AS (
    SELECT
        l.MOGuid,
        l.LinkTypeID,
        SUM(ISNULL(det.Qty, 0)) AS ActualQty,
        SUM(ISNULL(det.TotalLine, 0)) AS ActualCost
    FROM tbl_MOInvoiceLink l
    INNER JOIN tbl_InvoiceHeader ih ON ih.Guid = l.InvoiceHeaderGuid AND ih.CompanyID = l.CompanyID
    INNER JOIN tbl_InvoiceDetails det ON det.HeaderGuid = ih.Guid AND det.CompanyID = ih.CompanyID
    WHERE l.CompanyID = @CompanyID AND ih.IsPosted = 1 AND l.LinkTypeID IN (25, 26)
    GROUP BY l.MOGuid, l.LinkTypeID
),
RoutingAgg AS (
    SELECT
        r.MOGuid,
        SUM(ISNULL(r.PlannedHours, 0)) AS PlannedHours,
        SUM(ISNULL(r.ActualHours, 0)) AS ActualHours
    FROM tbl_MORouting r
    WHERE r.CompanyID = @CompanyID
    GROUP BY r.MOGuid
),
OpenWip AS (
    SELECT
        ISNULL(SUM(CASE WHEN a.LinkTypeID = 25 THEN a.ActualCost ELSE 0 END), 0)
      - ISNULL(SUM(CASE WHEN a.LinkTypeID = 26 THEN a.ActualCost ELSE 0 END), 0) AS WipValue
    FROM Actual a
    INNER JOIN OpenMO h ON h.Guid = a.MOGuid AND h.StatusID IN (1, 2)
),
YieldAgg AS (
    SELECT
        SUM(CASE WHEN p.LineTypeID = 26 THEN p.PlannedQty ELSE 0 END) AS PlannedOut,
        SUM(CASE WHEN a.LinkTypeID = 26 THEN a.ActualQty ELSE 0 END) AS ActualOut
    FROM OpenMO h
    LEFT JOIN Progress p ON p.MOGuid = h.Guid
    LEFT JOIN Actual a ON a.MOGuid = h.Guid
    WHERE h.StatusID IN (2, 3)
),
HoursAgg AS (
    SELECT
        SUM(ISNULL(r.PlannedHours, 0)) AS PlannedHours,
        SUM(ISNULL(r.ActualHours, 0)) AS ActualHours
    FROM RoutingAgg r
    INNER JOIN OpenMO h ON h.Guid = r.MOGuid AND h.StatusID IN (2, 3)
),
Overloaded AS (
    SELECT COUNT(*) AS Cnt
    FROM tbl_MOHeader h
    INNER JOIN tbl_MORouting r ON r.MOGuid = h.Guid AND r.CompanyID = h.CompanyID
    INNER JOIN tbl_WorkCenter wc ON wc.ID = r.WorkCenterID AND wc.CompanyID = h.CompanyID
    WHERE h.CompanyID = @CompanyID AND h.IsActive = 1 AND h.StatusID IN (1, 2)
      AND ISNULL(wc.CapacityPerDay, 0) > 0
      AND ISNULL(r.PlannedHours, 0) >
          ISNULL(wc.CapacityPerDay, 0) *
          NULLIF(DATEDIFF(day,
              ISNULL(r.PlannedStart, h.PlannedStartDate),
              ISNULL(r.PlannedEnd, h.PlannedEndDate)) + 1, 0)
)
SELECT 'Status' AS MetricGroup, 'Draft MO' AS MetricName,
       CAST((SELECT COUNT(*) FROM OpenMO WHERE StatusID = 0) AS DECIMAL(18,3)) AS MetricValue,
       'Count' AS MetricFormat
UNION ALL
SELECT 'Status', 'Released MO',
       CAST((SELECT COUNT(*) FROM OpenMO WHERE StatusID = 1) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Status', 'MO In Progress',
       CAST((SELECT COUNT(*) FROM OpenMO WHERE StatusID = 2) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Status', 'Active MO',
       CAST((SELECT COUNT(*) FROM OpenMO WHERE StatusID IN (1, 2)) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Status', 'Completed MO',
       CAST((SELECT COUNT(*) FROM OpenMO WHERE StatusID = 3) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Status', 'Cancelled MO',
       CAST((SELECT COUNT(*) FROM OpenMO WHERE StatusID = 4) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Status', 'Overdue MO',
       CAST((SELECT COUNT(*) FROM OpenMO
             WHERE StatusID IN (1, 2)
               AND PlannedEndDate IS NOT NULL
               AND PlannedEndDate < CAST(GETDATE() AS date)) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Status', 'Completed This Month',
       CAST((SELECT COUNT(*) FROM OpenMO
             WHERE StatusID = 3
               AND MODate >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Costing', 'WIP Value',
       CAST((SELECT ISNULL(WipValue, 0) FROM OpenWip) AS DECIMAL(18,3)), 'Money'
UNION ALL
SELECT 'Costing', 'Material Issued Cost',
       CAST((SELECT ISNULL(SUM(ActualCost), 0) FROM Actual WHERE LinkTypeID = 25) AS DECIMAL(18,3)), 'Money'
UNION ALL
SELECT 'Costing', 'FG Received Cost',
       CAST((SELECT ISNULL(SUM(ActualCost), 0) FROM Actual WHERE LinkTypeID = 26) AS DECIMAL(18,3)), 'Money'
UNION ALL
SELECT 'Costing', 'Open Issue Vouchers',
       CAST((SELECT COUNT(DISTINCT l.InvoiceHeaderGuid)
             FROM tbl_MOInvoiceLink l
             INNER JOIN OpenMO h ON h.Guid = l.MOGuid AND h.StatusID IN (1, 2)
             WHERE l.CompanyID = @CompanyID AND l.LinkTypeID = 25) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Costing', 'Open Receipt Vouchers',
       CAST((SELECT COUNT(DISTINCT l.InvoiceHeaderGuid)
             FROM tbl_MOInvoiceLink l
             INNER JOIN OpenMO h ON h.Guid = l.MOGuid AND h.StatusID IN (1, 2)
             WHERE l.CompanyID = @CompanyID AND l.LinkTypeID = 26) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Performance', 'Yield %',
       CAST(CASE WHEN ISNULL((SELECT PlannedOut FROM YieldAgg), 0) = 0 THEN 0
                 ELSE ISNULL((SELECT ActualOut FROM YieldAgg), 0) * 100.0
                      / NULLIF((SELECT PlannedOut FROM YieldAgg), 0) END AS DECIMAL(18,3)), 'Percent'
UNION ALL
SELECT 'Performance', 'Efficiency %',
       CAST(CASE WHEN ISNULL((SELECT PlannedHours FROM HoursAgg), 0) = 0 THEN 0
                 ELSE ISNULL((SELECT ActualHours FROM HoursAgg), 0) * 100.0
                      / NULLIF((SELECT PlannedHours FROM HoursAgg), 0) END AS DECIMAL(18,3)), 'Percent'
UNION ALL
SELECT 'Performance', 'Planned Hours',
       CAST(ISNULL((SELECT PlannedHours FROM HoursAgg), 0) AS DECIMAL(18,3)), 'Hours'
UNION ALL
SELECT 'Performance', 'Actual Hours',
       CAST(ISNULL((SELECT ActualHours FROM HoursAgg), 0) AS DECIMAL(18,3)), 'Hours'
UNION ALL
SELECT 'Performance', 'Overloaded Operations',
       CAST(ISNULL((SELECT Cnt FROM Overloaded), 0) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Master Data', 'Open BOM',
       CAST((SELECT COUNT(*) FROM tbl_BOMHeader WHERE CompanyID = @CompanyID AND IsActive = 1) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Master Data', 'Work Centers',
       CAST((SELECT COUNT(*) FROM tbl_WorkCenter WHERE CompanyID = @CompanyID AND IsActive = 1) AS DECIMAL(18,3)), 'Count'
UNION ALL
SELECT 'Master Data', 'BOM with Routing',
       CAST((SELECT COUNT(DISTINCT BOMID) FROM tbl_BOMRouting WHERE CompanyID = @CompanyID) AS DECIMAL(18,3)), 'Count'
";

                return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectMODashboardOverdue(int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };
                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
SELECT TOP 20
    h.Guid AS MOGuid,
    h.MOCode,
    h.MOName,
    h.StatusID,
    h.PlannedEndDate,
    DATEDIFF(day, h.PlannedEndDate, GETDATE()) AS DaysOverdue,
    ISNULL((
        SELECT SUM(ISNULL(d.PlannedQty, 0)) FROM tbl_MODetails d
        WHERE d.HeaderGuid = h.Guid AND d.LineTypeID = 26 AND d.CompanyID = h.CompanyID
    ), 0) AS PlannedOutputQty,
    ISNULL((
        SELECT SUM(ISNULL(det.Qty, 0))
        FROM tbl_MOInvoiceLink l
        INNER JOIN tbl_InvoiceHeader ih ON ih.Guid = l.InvoiceHeaderGuid AND ih.CompanyID = l.CompanyID
        INNER JOIN tbl_InvoiceDetails det ON det.HeaderGuid = ih.Guid
        WHERE l.MOGuid = h.Guid AND l.LinkTypeID = 26 AND ih.IsPosted = 1
    ), 0) AS ActualOutputQty
FROM tbl_MOHeader h
WHERE h.CompanyID = @CompanyID AND h.IsActive = 1 AND h.StatusID IN (1, 2)
  AND h.PlannedEndDate IS NOT NULL AND h.PlannedEndDate < CAST(GETDATE() AS date)
ORDER BY h.PlannedEndDate ASC
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}