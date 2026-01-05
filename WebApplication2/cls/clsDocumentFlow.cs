using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsDocumentFlow
    {
        // =========================
        // 1) HEADER
        // =========================

        public DataTable SelectFlowHeader(
            int ID,
            Guid TransactionGuidFrom,
            Guid TransactionGuidTo,
            int FlowTypeID,
            int FlowActionID,
            int StatusID,
            int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },

                    new SqlParameter("@TransactionGuidFrom", SqlDbType.UniqueIdentifier) { Value = TransactionGuidFrom },
                    new SqlParameter("@TransactionGuidTo", SqlDbType.UniqueIdentifier) { Value = TransactionGuidTo },

                    new SqlParameter("@FlowTypeID", SqlDbType.Int) { Value = FlowTypeID },
                    new SqlParameter("@FlowActionID", SqlDbType.Int) { Value = FlowActionID },
                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = StatusID },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();

                string sql = @"
select *
from tbl_DocumentFlowHeader
where (ID=@ID or @ID=0)
  and (TransactionGuidFrom=@TransactionGuidFrom or @TransactionGuidFrom='00000000-0000-0000-0000-000000000000')
  and (TransactionGuidTo=@TransactionGuidTo or @TransactionGuidTo='00000000-0000-0000-0000-000000000000')
  and (FlowTypeID=@FlowTypeID or @FlowTypeID=0)
  and (FlowActionID=@FlowActionID or @FlowActionID=0)
  and (StatusID=@StatusID or @StatusID=0)
  and (CompanyID=@CompanyID or @CompanyID=0)
order by ID desc
";
                return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertFlowHeader(
            int FlowTypeID,
            int FlowActionID,
            int StatusID,
            Guid TransactionGuidFrom,
            Guid TransactionGuidTo,
            int TransactionTypeIDFrom,
            int TransactionTypeIDTo,
            string ReferenceNo,
            string Notes,
            int CompanyID,
            int CreationUserID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@FlowTypeID", SqlDbType.Int) { Value = FlowTypeID },
                    new SqlParameter("@FlowActionID", SqlDbType.Int) { Value = FlowActionID },
                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = StatusID },

                    new SqlParameter("@TransactionGuidFrom", SqlDbType.UniqueIdentifier) { Value = TransactionGuidFrom },
                    new SqlParameter("@TransactionGuidTo", SqlDbType.UniqueIdentifier) { Value = TransactionGuidTo },

                    new SqlParameter("@TransactionTypeIDFrom", SqlDbType.Int) { Value = TransactionTypeIDFrom },
                    new SqlParameter("@TransactionTypeIDTo", SqlDbType.Int) { Value = TransactionTypeIDTo },

                    new SqlParameter("@ReferenceNo", SqlDbType.NVarChar, -1) { Value = (object)ReferenceNo ?? "" },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = (object)Notes ?? "" },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_DocumentFlowHeader
(
    FlowTypeID, FlowActionID, StatusID,
    TransactionGuidFrom, TransactionGuidTo,
    TransactionTypeIDFrom, TransactionTypeIDTo,
    ReferenceNo, Notes,
    CompanyID, CreationDate, CreationUserID
)
output inserted.ID
values
(
    @FlowTypeID, @FlowActionID, @StatusID,
    @TransactionGuidFrom, @TransactionGuidTo,
    @TransactionTypeIDFrom, @TransactionTypeIDTo,
    @ReferenceNo, @Notes,
    @CompanyID, @CreationDate, @CreationUserID
)
";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateFlowHeaderStatus(
            int HeaderID,
            int StatusID,
            int ModificationUserID,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderID", SqlDbType.Int) { Value = HeaderID },
                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = StatusID },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
update tbl_DocumentFlowHeader set
   StatusID=@StatusID,
   ModificationDate=@ModificationDate,
   ModificationUserID=@ModificationUserID
where ID=@HeaderID
";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteFlowHeaderByID(int HeaderID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                // Usually delete details first
                DeleteFlowDetailsByHeaderID(HeaderID, CompanyID, trn);

                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderID", SqlDbType.Int) { Value = HeaderID }
                };

                string sql = @"delete from tbl_DocumentFlowHeader where ID=@HeaderID";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================
        // 2) DETAIL
        // =========================

        public DataTable SelectFlowDetailsByHeaderID(int HeaderID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderID", SqlDbType.Int) { Value = HeaderID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL clsSQL = new clsSQL();

                string sql = @"
select *
from tbl_DocumentFlowDetail
where (HeaderID=@HeaderID or @HeaderID=0)
  and (CompanyID=@CompanyID or @CompanyID=0)
order by ID
";
                return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertFlowDetailQty(
            int HeaderID,
            Guid TransactionLineGuidFrom,
            Guid TransactionLineGuidTo,
            Guid ItemGuid,
            decimal Qty,
            int CompanyID,
            int CreationUserID,
            string Notes = "",
            SqlTransaction trn = null)
        {
            try
            {
                // ValueTypeID: 1 = Qty
                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderID", SqlDbType.Int) { Value = HeaderID },
                    new SqlParameter("@ValueTypeID", SqlDbType.Int) { Value = 1 },

                    new SqlParameter("@TransactionLineGuidFrom", SqlDbType.UniqueIdentifier) { Value = TransactionLineGuidFrom },
                    new SqlParameter("@TransactionLineGuidTo", SqlDbType.UniqueIdentifier) { Value = TransactionLineGuidTo },
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },

                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = Qty },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = DBNull.Value },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = (object)Notes ?? "" },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_DocumentFlowDetail
(
    HeaderID, ValueTypeID, Notes,
    TransactionLineGuidFrom, TransactionLineGuidTo,
    ItemGuid, Qty, Amount,
    CompanyID, CreationDate, CreationUserID
)
output inserted.ID
values
(
    @HeaderID, @ValueTypeID, @Notes,
    @TransactionLineGuidFrom, @TransactionLineGuidTo,
    @ItemGuid, @Qty, @Amount,
    @CompanyID, @CreationDate, @CreationUserID
)
";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertFlowDetailAmount(
            int HeaderID,
            decimal Amount,
            int CurrencyID,
            decimal Rate,
            int CompanyID,
            int CreationUserID,
            string Notes = "",
            SqlTransaction trn = null)
        {
            try
            {
                // ValueTypeID: 2 = Amount
                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderID", SqlDbType.Int) { Value = HeaderID },
                    new SqlParameter("@ValueTypeID", SqlDbType.Int) { Value = 2 },

                    new SqlParameter("@TransactionLineGuidFrom", SqlDbType.UniqueIdentifier) { Value = DBNull.Value },
                    new SqlParameter("@TransactionLineGuidTo", SqlDbType.UniqueIdentifier) { Value = DBNull.Value },
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = DBNull.Value },

                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = DBNull.Value },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = Amount },

                    new SqlParameter("@CurrencyID", SqlDbType.Int) { Value = CurrencyID },
                    new SqlParameter("@Rate", SqlDbType.Decimal) { Value = Rate },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = (object)Notes ?? "" },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_DocumentFlowDetail
(
    HeaderID, ValueTypeID, Notes,
    TransactionLineGuidFrom, TransactionLineGuidTo,
    ItemGuid, Qty, Amount,
    CurrencyID, Rate,
    CompanyID, CreationDate, CreationUserID
)
output inserted.ID
values
(
    @HeaderID, @ValueTypeID, @Notes,
    @TransactionLineGuidFrom, @TransactionLineGuidTo,
    @ItemGuid, @Qty, @Amount,
    @CurrencyID, @Rate,
    @CompanyID, @CreationDate, @CreationUserID
)
";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteFlowDetailsByHeaderID(int HeaderID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@HeaderID", SqlDbType.Int) { Value = HeaderID }
                };

                string sql = @"delete from tbl_DocumentFlowDetail where HeaderID=@HeaderID";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================
        // 3) QUICK QUERIES FOR UI (RELATED DOCS)
        // =========================

        public DataTable SelectRelatedDocsByTransactionGuid(Guid TransactionGuid, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@TransactionGuid", SqlDbType.UniqueIdentifier) { Value = TransactionGuid },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL clsSQL = new clsSQL();

                // This returns direct parents & children (1 level). We'll build recursion later.
                string sql = @"SELECT
    h.ID as FlowHeaderID,
    h.FlowTypeID,
    h.FlowActionID,
    h.StatusID,
    h.TransactionGuidFrom,
    h.TransactionGuidTo,
    h.TransactionTypeIDFrom,
    h.TransactionTypeIDTo,
    h.ReferenceNo,
    h.Notes,
    h.CreationDate,

    -- ===== FROM =====
    CASE 
        WHEN h.TransactionTypeIDFrom IN (12,13) THEN cvFrom.VoucherNo
        ELSE ihFrom.InvoiceNo
    END as DocNoFrom,
    CASE 
        WHEN h.TransactionTypeIDFrom IN (12,13) THEN cvFrom.VoucherDate
        ELSE ihFrom.InvoiceDate
    END as DocDateFrom,
    CASE 
        WHEN h.TransactionTypeIDFrom IN (12,13) THEN cvFrom.Amount
        ELSE ihFrom.TotalInvoice
    END as TotalFrom,
    CASE 
        WHEN h.TransactionTypeIDFrom IN (12,13) THEN jvtFromCV.AName
        ELSE jvtFromInv.AName
    END as TypeNameFrom,

    -- ===== TO =====
    CASE 
        WHEN h.TransactionTypeIDTo IN (12,13) THEN cvTo.VoucherNo
        ELSE ihTo.InvoiceNo
    END as DocNoTo,
    CASE 
        WHEN h.TransactionTypeIDTo IN (12,13) THEN cvTo.VoucherDate
        ELSE ihTo.InvoiceDate
    END as DocDateTo,
    CASE 
        WHEN h.TransactionTypeIDTo IN (12,13) THEN cvTo.Amount
        ELSE ihTo.TotalInvoice
    END as TotalTo,
    CASE 
        WHEN h.TransactionTypeIDTo IN (12,13) THEN jvtToCV.AName
        ELSE jvtToInv.AName
    END as TypeNameTo

FROM tbl_DocumentFlowHeader h

-- Invoice FROM / TO
LEFT JOIN tbl_InvoiceHeader ihFrom
       ON ihFrom.Guid = h.TransactionGuidFrom
LEFT JOIN tbl_JournalVoucherTypes jvtFromInv
       ON jvtFromInv.ID = ihFrom.InvoiceTypeID

LEFT JOIN tbl_InvoiceHeader ihTo
       ON ihTo.Guid = h.TransactionGuidTo
LEFT JOIN tbl_JournalVoucherTypes jvtToInv
       ON jvtToInv.ID = ihTo.InvoiceTypeID

-- CashVoucher FROM / TO (Aliased correctly)
LEFT JOIN tbl_CashVoucherHeader cvFrom
       ON cvFrom.Guid = h.TransactionGuidFrom
LEFT JOIN tbl_JournalVoucherTypes jvtFromCV
       ON jvtFromCV.ID = cvFrom.VoucherType  -- غيّرها إذا اسم الحقل مختلف

LEFT JOIN tbl_CashVoucherHeader cvTo
       ON cvTo.Guid = h.TransactionGuidTo
LEFT JOIN tbl_JournalVoucherTypes jvtToCV
       ON jvtToCV.ID = cvTo.VoucherType   
where (h.CompanyID=@CompanyID or @CompanyID=0)
  and (h.TransactionGuidFrom=@TransactionGuid or h.TransactionGuidTo=@TransactionGuid)
order by h.ID desc
";

                return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
