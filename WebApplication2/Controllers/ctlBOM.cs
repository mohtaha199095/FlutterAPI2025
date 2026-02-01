using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlBOM")]
    public class ctlBOM : Controller
    {
        // =========================================================
        // HEADER
        // =========================================================

        [HttpGet]
        [Route("SelectBOMHeader")]
        public string SelectBOMHeader(int ID, string BOMCode, string BOMName, int CompanyID)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                DataTable dt = clsBOM.SelectBOMHeader(
                    Simulate.Integer32(ID),
                    Simulate.String(BOMCode),
                    Simulate.String(BOMName),
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
        [Route("DeleteBOMByID")]
        public bool DeleteBOMByID(int ID, int CompanyID)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                bool A = clsBOM.DeleteBOMByID(Simulate.Integer32(ID), CompanyID);
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertBOMHeader")]
        public int InsertBOMHeader(
            string BOMCode,
            string BOMName,
            decimal BatchQty,
            int VersionNo,
            bool IsDefault,
            bool IsActive,
            DateTime? EffectiveFrom,
            DateTime? EffectiveTo,
            string Notes,
            int CompanyID,
            int CreationUserId)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();

                int A = clsBOM.InsertBOMHeader(
                    Simulate.String(BOMCode),
                    Simulate.String(BOMName),
                    Simulate.decimal_(BatchQty),
                    Simulate.Integer32(VersionNo),
                    IsDefault,
                    IsActive,
                    EffectiveFrom,
                    EffectiveTo,
                    Simulate.String(Notes),
                    CompanyID,
                    CreationUserId
                );

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("UpdateBOMHeader")]
        public int UpdateBOMHeader(
            int ID,
            string BOMCode,
            string BOMName,
            decimal BatchQty,
            int VersionNo,
            bool IsDefault,
            bool IsActive,
            DateTime? EffectiveFrom,
            DateTime? EffectiveTo,
            string Notes,
            int ModificationUserId,
            int CompanyID)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();

                int A = clsBOM.UpdateBOMHeader(
                    Simulate.Integer32(ID),
                    Simulate.String(BOMCode),
                    Simulate.String(BOMName),
                    Simulate.decimal_(BatchQty),
                    Simulate.Integer32(VersionNo),
                    IsDefault,
                    IsActive,
                    EffectiveFrom,
                    EffectiveTo,
                    Simulate.String(Notes),
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
        // INPUTS
        // =========================================================

        [HttpGet]
        [Route("SelectBOMInputsByBOMID")]
        public string SelectBOMInputsByBOMID(int BOMID, int CompanyID)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                DataTable dt = clsBOM.SelectBOMInputsByBOMID(Simulate.Integer32(BOMID), CompanyID);

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
        [Route("DeleteBOMInputsByBOMID")]
        public int DeleteBOMInputsByBOMID(int BOMID, int CompanyID)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                return clsBOM.DeleteBOMInputsByBOMID(Simulate.Integer32(BOMID), CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertBOMInput")]
        public int InsertBOMInput(
            int BOMID,
            Guid ComponentItemGuid,
            decimal Qty,
            int UOMID,
            int LineNo,
            decimal ScrapPercent,
            string Notes,
            int CompanyID,
            int CreationUserId)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                int A = clsBOM.InsertBOMInput(
                    Simulate.Integer32(BOMID),
                    ComponentItemGuid,
                    Simulate.decimal_(Qty),
                    Simulate.Integer32(UOMID),
                    Simulate.Integer32(LineNo),
                    Simulate.decimal_(ScrapPercent),
                    Simulate.String(Notes),
                    CompanyID,
                    CreationUserId
                );
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // OUTPUTS
        // =========================================================

        [HttpGet]
        [Route("SelectBOMOutputsByBOMID")]
        public string SelectBOMOutputsByBOMID(int BOMID, int CompanyID)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                DataTable dt = clsBOM.SelectBOMOutputsByBOMID(Simulate.Integer32(BOMID), CompanyID);

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
        [Route("DeleteBOMOutputsByBOMID")]
        public int DeleteBOMOutputsByBOMID(int BOMID, int CompanyID)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                return clsBOM.DeleteBOMOutputsByBOMID(Simulate.Integer32(BOMID), CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertBOMOutput")]
        public int InsertBOMOutput(
            int BOMID,
            Guid OutputItemGuid,
            decimal Qty,
            int UOMID,
            decimal CostSharePercent,
            int LineNo,
            string Notes,
            int CompanyID,
            int CreationUserId)
        {
            try
            {
                clsBOM clsBOM = new clsBOM();
                int A = clsBOM.InsertBOMOutput(
                    Simulate.Integer32(BOMID),
                    OutputItemGuid,
                    Simulate.decimal_(Qty),
                    Simulate.Integer32(UOMID),
                    Simulate.decimal_(CostSharePercent),
                    Simulate.Integer32(LineNo),
                    Simulate.String(Notes),
                    CompanyID,
                    CreationUserId
                );
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // OPTIONAL: Save FULL BOM (Header + Inputs + Outputs) in one request
        // payload JSON comes as string like your style
        // =========================================================

        public class BOMSaveDto
        {
            public int ID { get; set; } // 0=new
            public string BOMCode { get; set; }
            public string BOMName { get; set; }
            public decimal BatchQty { get; set; }
            public int VersionNo { get; set; }
            public bool IsDefault { get; set; }
            public bool IsActive { get; set; }
            public DateTime? EffectiveFrom { get; set; }
            public DateTime? EffectiveTo { get; set; }
            public string Notes { get; set; }

            // arrays
            public BOMInputDto[] Inputs { get; set; }
            public BOMOutputDto[] Outputs { get; set; }
        }

        public class BOMInputDto
        {
            public Guid ComponentItemGuid { get; set; }
            public decimal Qty { get; set; }
            public int UOMID { get; set; }
            public int LineNo { get; set; }
            public decimal ScrapPercent { get; set; }
            public string Notes { get; set; }
        }

        public class BOMOutputDto
        {
            public Guid OutputItemGuid { get; set; }
            public decimal Qty { get; set; }
            public int UOMID { get; set; }
            public decimal CostSharePercent { get; set; }
            public int LineNo { get; set; }
            public string Notes { get; set; }
        } 

            [HttpGet]
            [Route("SaveBOMFull")]
            public int SaveBOMFull(string BOMJson, int CompanyID, int UserId)
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
                            BOMSaveDto dto = JsonConvert.DeserializeObject<BOMSaveDto>(BOMJson);
                            clsBOM clsBOM = new clsBOM();

                            // Basic validations (optional but recommended)
                            if (dto == null) throw new Exception("Invalid BOMJson");
                            if (dto.Outputs == null || dto.Outputs.Length == 0)
                                throw new Exception("BOM must have at least one output item.");

                            int bomId = 0;

                            // 1) Header
                            if (dto.ID == 0)
                            {
                                bomId = clsBOM.InsertBOMHeader(
                                    Simulate.String(dto.BOMCode),
                                    Simulate.String(dto.BOMName),
                                    Simulate.decimal_(dto.BatchQty),
                                    Simulate.Integer32(dto.VersionNo),
                                    dto.IsDefault,
                                    dto.IsActive,
                                    dto.EffectiveFrom,
                                    dto.EffectiveTo,
                                    Simulate.String(dto.Notes),
                                    CompanyID,
                                    UserId,
                                    trn
                                );
                            }
                            else
                            {
                                bomId = Simulate.Integer32(dto.ID);

                                clsBOM.UpdateBOMHeader(
                                    bomId,
                                    Simulate.String(dto.BOMCode),
                                    Simulate.String(dto.BOMName),
                                    Simulate.decimal_(dto.BatchQty),
                                    Simulate.Integer32(dto.VersionNo),
                                    dto.IsDefault,
                                    dto.IsActive,
                                    dto.EffectiveFrom,
                                    dto.EffectiveTo,
                                    Simulate.String(dto.Notes),
                                    UserId,
                                    CompanyID,
                                    trn
                                );
                            }

                            // 2) Replace Inputs
                            clsBOM.DeleteBOMInputsByBOMID(bomId, CompanyID, trn);

                            if (dto.Inputs != null)
                            {
                                foreach (var r in dto.Inputs)
                                {
                                    clsBOM.InsertBOMInput(
                                        bomId,
                                        r.ComponentItemGuid,
                                        Simulate.decimal_(r.Qty),
                                        Simulate.Integer32(r.UOMID),
                                        Simulate.Integer32(r.LineNo),
                                        Simulate.decimal_(r.ScrapPercent),
                                        Simulate.String(r.Notes),
                                        CompanyID,
                                        UserId,
                                        trn
                                    );
                                }
                            }

                            // 3) Replace Outputs
                            clsBOM.DeleteBOMOutputsByBOMID(bomId, CompanyID, trn);

                            if (dto.Outputs != null)
                            {
                                foreach (var r in dto.Outputs)
                                {
                                    clsBOM.InsertBOMOutput(
                                        bomId,
                                        r.OutputItemGuid,
                                        Simulate.decimal_(r.Qty),
                                        Simulate.Integer32(r.UOMID),
                                        Simulate.decimal_(r.CostSharePercent),
                                        Simulate.Integer32(r.LineNo),
                                        Simulate.String(r.Notes),
                                        CompanyID,
                                        UserId,
                                        trn
                                    );
                                }
                            }

                            trn.Commit();
                            return   bomId;
                        }
                        catch (Exception ex)
                        {
                            try { trn.Rollback(); } catch { }
                            throw;
                        }
                    }
                }
            }
        }
}

 
 
 
