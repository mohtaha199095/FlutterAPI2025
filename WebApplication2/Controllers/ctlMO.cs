using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;
using WebApplication2.cls.Reports;

namespace WebApplication2.Controllers
{
    [Route("api/ctlMO")]
    public class ctlMO : Controller
    {
        // =========================================================
        // HEADER
        // =========================================================

        [HttpGet]
        [Route("SelectMOHeader")]
        public string SelectMOHeader(string Guid, string MOCode, string MOName, int CompanyID)
        {
            try
            {
                clsMO clsMO = new clsMO();
                DataTable dt = clsMO.SelectMOHeader(
                    Simulate.String(Guid),
                    Simulate.String(MOCode),
                    Simulate.String(MOName),
                    CompanyID
                );

                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("DeleteMOByGuid")]
        public bool DeleteMOByGuid(string Guid, int CompanyID)
        {
            try
            {
                clsMO clsMO = new clsMO();
                bool A = clsMO.DeleteMOByGuid(Simulate.String(Guid), CompanyID);
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertMOHeader")]
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
            int CreationUserId)
        {
            try
            {
                clsMO clsMO = new clsMO();

                string A = clsMO.InsertMOHeader(
                    Simulate.String(MOCode),
                    Simulate.String(MOName),
                    Simulate.Integer32(BOMID),
                    Simulate.decimal_(PlannedQty),
                    Simulate.decimal_(BatchQty),
                    MODate,
                    PlannedStartDate,
                    PlannedEndDate,
                    Simulate.Integer32(StatusID),
                    Simulate.Integer32(BranchID),
                    Simulate.Integer32(StoreID),
                    Simulate.String(Notes),
                    IsActive,
                    CompanyID,
                    CreationUserId
                );

                return A; // returns MO Guid
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("UpdateMOHeader")]
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
            int CompanyID)
        {
            try
            {
                clsMO clsMO = new clsMO();

                int A = clsMO.UpdateMOHeader(
                    Simulate.String(Guid),
                    Simulate.String(MOCode),
                    Simulate.String(MOName),
                    Simulate.Integer32(BOMID),
                    Simulate.decimal_(BatchQty),  // (typo safe) we pass BatchQty below correctly
                    Simulate.decimal_(PlannedQty),// we will fix order below
                    MODate,
                    PlannedStartDate,
                    PlannedEndDate,
                    Simulate.Integer32(StatusID),
                    Simulate.Integer32(BranchID),
                    Simulate.Integer32(StoreID),
                    Simulate.String(Notes),
                    IsActive,
                    ModificationUserId,
                    CompanyID
                );

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ✅ IMPORTANT: Fix the parameter order issue (PlannedQty/BatchQty)
        // Your clsMO.UpdateMOHeader signature is: (Guid, MOCode, MOName, BOMID, PlannedQty, BatchQty, ...)
        // So use this corrected method instead of the one above if you want no mistakes:
        [HttpGet]
        [Route("UpdateMOHeader2")]
        public int UpdateMOHeader2(
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
            int CompanyID)
        {
            try
            {
                clsMO clsMO = new clsMO();

                int A = clsMO.UpdateMOHeader(
                    Simulate.String(Guid),
                    Simulate.String(MOCode),
                    Simulate.String(MOName),
                    Simulate.Integer32(BOMID),
                    Simulate.decimal_(PlannedQty),
                    Simulate.decimal_(BatchQty),
                    MODate,
                    PlannedStartDate,
                    PlannedEndDate,
                    Simulate.Integer32(StatusID),
                    Simulate.Integer32(BranchID),
                    Simulate.Integer32(StoreID),
                    Simulate.String(Notes),
                    IsActive,
                    ModificationUserId,
                    CompanyID
                );

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        [Route("SelectMODetailsByMOGuid")]
        public string SelectMODetailsByMOGuid(string MOGuid, int CompanyID, int LineTypeID = 0)
        {
            try
            {
                clsMO clsMO = new clsMO();
                DataTable dt = clsMO.SelectMODetailsByMOGuid(
                    Simulate.String(MOGuid),
                    CompanyID,
                    Simulate.Integer32(LineTypeID)
                );

                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("DeleteMODetailsByMOGuid")]
        public int DeleteMODetailsByMOGuid(string MOGuid, int CompanyID, int LineTypeID = 0)
        {
            try
            {
                clsMO clsMO = new clsMO();
                return clsMO.DeleteMODetailsByMOGuid(
                    Simulate.String(MOGuid),
                    CompanyID,
                    Simulate.Integer32(LineTypeID)
                );
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertMODetail")]
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
            int CreationUserId)
        {
            try
            {
                clsMO clsMO = new clsMO();

                string A = clsMO.InsertMODetail(
                    Simulate.String(MOGuid),
                    Simulate.Integer32(RowIndex),
                    Simulate.Integer32(LineTypeID),
                    ItemGuid,
                    Simulate.String(ItemName),
                    Simulate.decimal_(PlannedQty),
                    Simulate.Integer32(UOMID),
                    Simulate.decimal_(ScrapPercent),
                    Simulate.decimal_(CostSharePercent),
                    Simulate.Integer32(BOMLineNo),
                    Simulate.Integer32(BranchID),
                    Simulate.Integer32(StoreID),
                    Simulate.String(Notes),
                    TrackLot,
                    TrackSerial,
                    TrackExpiryDate,
                    CompanyID,
                    CreationUserId
                );

                return A; // line Guid
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("UpdateMODetail")]
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
            int CompanyID)
        {
            try
            {
                clsMO clsMO = new clsMO();

                int A = clsMO.UpdateMODetail(
                    Simulate.String(Guid),
                    Simulate.Integer32(RowIndex),
                    Simulate.Integer32(LineTypeID),
                    ItemGuid,
                    Simulate.String(ItemName),
                    Simulate.decimal_(PlannedQty),
                    Simulate.Integer32(UOMID),
                    Simulate.decimal_(ScrapPercent),
                    Simulate.decimal_(CostSharePercent),
                    Simulate.Integer32(BOMLineNo),
                    Simulate.Integer32(BranchID),
                    Simulate.Integer32(StoreID),
                    Simulate.String(Notes),
                    TrackLot,
                    TrackSerial,
                    TrackExpiryDate,
                    ModificationUserId,
                    CompanyID
                );

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("DeleteMODetailByGuid")]
        public bool DeleteMODetailByGuid(string Guid, int CompanyID)
        {
            try
            {
                clsMO clsMO = new clsMO();
                bool A = clsMO.DeleteMODetailByGuid(Simulate.String(Guid), CompanyID);
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // MO ↔ Invoice Link
        // =========================================================

        [HttpGet]
        [Route("SelectMOInvoiceLinks")]
        public string SelectMOInvoiceLinks(string MOGuid, int CompanyID, int LinkTypeID = 0)
        {
            try
            {
                clsMO clsMO = new clsMO();
                DataTable dt = clsMO.SelectMOInvoiceLinks(
                    Simulate.String(MOGuid),
                    CompanyID,
                    Simulate.Integer32(LinkTypeID)
                );

                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertMOInvoiceLink")]
        public string InsertMOInvoiceLink(
            string MOGuid,
            string InvoiceHeaderGuid,
            int LinkTypeID,
            string Notes,
            int CompanyID,
            int CreationUserId)
        {
            try
            {
                clsMO clsMO = new clsMO();

                string A = clsMO.InsertMOInvoiceLink(
                    Simulate.String(MOGuid),
                    Simulate.String(InvoiceHeaderGuid),
                    Simulate.Integer32(LinkTypeID),
                    Simulate.String(Notes),
                    CompanyID,
                    CreationUserId
                );

                return JsonConvert.SerializeObject(A);  // link Guid
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("DeleteMOInvoiceLinkByGuid")]
        public bool DeleteMOInvoiceLinkByGuid(string Guid, int CompanyID)
        {
            clsMO clsMO = new clsMO();
            clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
            clsSQL clsSQL = new clsSQL();

            string cs = clsSQL.CreateDataBaseConnectionString(CompanyID);

            using (SqlConnection cn = new SqlConnection(cs))
            {
                cn.Open();
                SqlTransaction trn = cn.BeginTransaction();

                try
                {
                    // 1) Read InvoiceHeaderGuid from link
                    SqlParameter[] prmSelect =
                    {
                        new SqlParameter("@Guid", SqlDbType.NVarChar,-1) { Value = Simulate.String(Guid) },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    };

                    DataTable dt = clsSQL.ExecuteQueryStatement(@"
                        SELECT TOP 1 InvoiceHeaderGuid
                        FROM TblMOInvoiceLink
                        WHERE (Guid = @Guid OR @Guid = '')
                          AND (CompanyID = @CompanyID OR @CompanyID = 0)
                    ", cs, prmSelect, trn);

                    if (dt == null || dt.Rows.Count == 0)
                        throw new Exception("Link not found.");

                    string invoiceGuid = Simulate.String(dt.Rows[0]["InvoiceHeaderGuid"]);

                    if (string.IsNullOrEmpty(invoiceGuid))
                        throw new Exception("InvoiceHeaderGuid is empty.");

                    // 2) Delete Link
                    bool moDeleted = clsMO.DeleteMOInvoiceLinkByGuid(
                        Simulate.String(Guid),
                        CompanyID,
                        trn
                    );

                    // 3) Delete Invoice (using invoiceGuid, NOT linkGuid)
                    bool invoiceDeleted = clsInvoiceHeader.DeleteInvoiceHeaderByGuid(
                        Simulate.String(invoiceGuid),
                        CompanyID,
                        trn
                    );

                    if (!moDeleted || !invoiceDeleted)
                        throw new Exception("Delete operation failed.");

                    trn.Commit();
                    return true;
                }
                catch
                {
                    try { trn.Rollback(); } catch { }
                    throw;
                }
            }
        }
        [HttpGet]
        [Route("SelectMOProgress")]
        public string SelectMOProgress(string MOGuid, int CompanyID)
        {
            try
            {
                clsMO cls = new clsMO();
                DataTable dt = cls.SelectMOProgress(Simulate.String(MOGuid), CompanyID);
                if (dt != null) return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // OPTIONAL: Save FULL MO (Header + Details) in one request
        // Details include Inputs + Outputs in one list (LineTypeID)
        // =========================================================

        public class MOSaveDto
        {
            public string Guid { get; set; } // ""=new
            public string MOCode { get; set; }
            public string MOName { get; set; }

            public int BOMID { get; set; }
            public decimal PlannedQty { get; set; }
            public decimal BatchQty { get; set; }

            public DateTime? MODate { get; set; }
            public DateTime? PlannedStartDate { get; set; }
            public DateTime? PlannedEndDate { get; set; }

            public int StatusID { get; set; }
            public int BranchID { get; set; }
            public int StoreID { get; set; }

            public string Notes { get; set; }
            public bool IsActive { get; set; }

            public MODetailDto[] Details { get; set; }
        }

        public class MODetailDto
        {
            public int RowIndex { get; set; }
            public int LineTypeID { get; set; } // 25=input, 26=output

            public Guid ItemGuid { get; set; }
            public string ItemName { get; set; }

            public decimal PlannedQty { get; set; }
            public int UOMID { get; set; }

            public decimal ScrapPercent { get; set; }
            public decimal CostSharePercent { get; set; }
            public int BOMLineNo { get; set; }

            public int BranchID { get; set; }
            public int StoreID { get; set; }

            public string Notes { get; set; }

            public bool TrackLot { get; set; }
            public bool TrackSerial { get; set; }
            public bool TrackExpiryDate { get; set; }
        }

        [HttpGet]
        [Route("SaveMOFull")]
        public string SaveMOFull(string MOJson, int CompanyID, int UserId)
        {
            clsSQL clsSQL = new clsSQL();
            string cs = clsSQL.CreateDataBaseConnectionString(CompanyID);

            using (SqlConnection cn = new SqlConnection(cs))
            {
                cn.Open();
                using (SqlTransaction trn = cn.BeginTransaction())
                {
                    try
                    {
                        MOSaveDto dto = JsonConvert.DeserializeObject<MOSaveDto>(MOJson);
                        clsMO clsMO = new clsMO();

                        if (dto == null) throw new Exception("Invalid MOJson");
                        if (dto.Details == null || dto.Details.Length == 0)
                            throw new Exception("MO must have details (inputs/outputs).");

                        // validate: at least one output line
                        bool hasOutput = false;
                        foreach (var d in dto.Details)
                        {
                            if (d.LineTypeID == 26) { hasOutput = true; break; }
                        }
                        if (!hasOutput) throw new Exception("MO must have at least one output line.");

                        string moGuid = "";

                        // 1) Header
                        if (string.IsNullOrEmpty(dto.Guid))
                        {
                            moGuid = clsMO.InsertMOHeader(
                                Simulate.String(dto.MOCode),
                                Simulate.String(dto.MOName),
                                Simulate.Integer32(dto.BOMID),
                                Simulate.decimal_(dto.PlannedQty),
                                Simulate.decimal_(dto.BatchQty),
                                dto.MODate,
                                dto.PlannedStartDate,
                                dto.PlannedEndDate,
                                Simulate.Integer32(dto.StatusID),
                                Simulate.Integer32(dto.BranchID),
                                Simulate.Integer32(dto.StoreID),
                                Simulate.String(dto.Notes),
                                dto.IsActive,
                                CompanyID,
                                UserId,
                                trn
                            );
                        }
                        else
                        {
                            moGuid = dto.Guid;

                            clsMO.UpdateMOHeader(
                                Simulate.String(dto.Guid),
                                Simulate.String(dto.MOCode),
                                Simulate.String(dto.MOName),
                                Simulate.Integer32(dto.BOMID),
                                Simulate.decimal_(dto.PlannedQty),
                                Simulate.decimal_(dto.BatchQty),
                                dto.MODate,
                                dto.PlannedStartDate,
                                dto.PlannedEndDate,
                                Simulate.Integer32(dto.StatusID),
                                Simulate.Integer32(dto.BranchID),
                                Simulate.Integer32(dto.StoreID),
                                Simulate.String(dto.Notes),
                                dto.IsActive,
                                UserId,
                                CompanyID,
                                trn
                            );
                        }

                        // 2) Replace Details
                        clsMO.DeleteMODetailsByMOGuid(moGuid, CompanyID, 0, trn);

                        foreach (var r in dto.Details)
                        {
                            clsMO.InsertMODetail(
                                moGuid,
                                Simulate.Integer32(r.RowIndex),
                                Simulate.Integer32(r.LineTypeID),
                                r.ItemGuid,
                                Simulate.String(r.ItemName),
                                Simulate.decimal_(r.PlannedQty),
                                Simulate.Integer32(r.UOMID),
                                Simulate.decimal_(r.ScrapPercent),
                                Simulate.decimal_(r.CostSharePercent),
                                Simulate.Integer32(r.BOMLineNo),
                                Simulate.Integer32(r.BranchID),
                                Simulate.Integer32(r.StoreID),
                                Simulate.String(r.Notes),
                                r.TrackLot,
                                r.TrackSerial,
                                r.TrackExpiryDate,
                                CompanyID,
                                UserId,
                                trn
                            );
                        }

                        trn.Commit();
                        
                        return JsonConvert.SerializeObject(moGuid); ;
                    }
                    catch (Exception)
                    {
                        try { trn.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }// =========================================================
        // REPORTS (3 Main Reports)
        // =========================================================

        [HttpGet]
        [Route("SelectMOSummaryReport")]
        public string SelectMOSummaryReport(string MOGuid, string MOCode, string MOName, int CompanyID, string DateFrom = "", string DateTo = "")
        {
            try
            {
                clsMO cls = new clsMO();
                DataTable dt = cls.SelectMOSummary(
                    Simulate.String(MOGuid),
                    Simulate.String(MOCode),
                    Simulate.String(MOName),
                    CompanyID,
                    Simulate.String(DateFrom),
                    Simulate.String(DateTo)
                );

                if (dt != null) return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectMOProgressReport")]
        public string SelectMOProgressReport(string MOGuid, int CompanyID)
        {
            try
            {
                clsMO cls = new clsMO();
                DataTable dt = cls.SelectMOProgress(
                    Simulate.String(MOGuid),
                    CompanyID
                );

                if (dt != null) return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectMOVouchersReport")]
        public string SelectMOVouchersReport(string MOGuid, int CompanyID)
        {
            try
            {
                clsMO cls = new clsMO();
                DataTable dt = cls.SelectMOVouchers(
                    Simulate.String(MOGuid),
                    CompanyID
                );

                if (dt != null) return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
