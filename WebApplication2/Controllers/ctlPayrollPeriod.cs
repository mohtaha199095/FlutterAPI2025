using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayrollPeriod")]
    public class ctlPayrollPeriod : Controller
    {
        // ==========================================================
        // SELECT
        // ==========================================================
        [HttpGet]
        [Route("SelectPayrollPeriod")]
        public string SelectPayrollPeriod(int ID, string AName, int IsClosed, int CompanyID)
        {
            try
            {
                clsPayrollPeriod obj = new clsPayrollPeriod();

                DataTable dt = obj.SelectPayrollPeriod(
                    ID,  Simulate.String( AName),   IsClosed,
                    CompanyID
                );

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT
        // ==========================================================
        [HttpPost]
        [Route("InsertPayrollPeriod")]
        public int InsertPayrollPeriod(
            string AName,
            string EName,
            DateTime StartDate,
            DateTime EndDate, bool IsClosed,
            int CompanyID,
            int CreationUserID
        )
        {
            try
            {
                clsPayrollPeriod obj = new clsPayrollPeriod();
                clsApprovalEngine approvalEngine = new clsApprovalEngine();
                int documentStatus = approvalEngine.ResolveInitialDocumentStatus(
                    CompanyID, clsHcmApprovalDocuments.TypePayrollPeriod, 0, 0);

                int newID = obj.InsertPayrollPeriod(
                    Simulate.String(AName),
                    Simulate.String(EName),
                    StartDate,
                    EndDate, IsClosed,
                    CompanyID,
                    CreationUserID,
                    null,
                    documentStatus);

                if (documentStatus == (int)DocumentStatus.Posted && newID > 0)
                {
                    string guid = clsHcmApprovalDocuments.SelectGuidById(
                        clsHcmApprovalDocuments.TypePayrollPeriod, newID, CompanyID);
                    if (!string.IsNullOrWhiteSpace(guid))
                        clsHcmApprovalDocuments.PostDocument(
                            clsHcmApprovalDocuments.TypePayrollPeriod,
                            guid, CreationUserID, CompanyID, null);
                }

                return newID;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE
        // ==========================================================
        [HttpPost]
        [Route("UpdatePayrollPeriod")]
        public int UpdatePayrollPeriod(
            int ID,
            string AName,
            string EName,
            DateTime StartDate,
            DateTime EndDate, bool IsClosed,
            int ModificationUserID,
            int CompanyID
        )
        {
            try
            {
                clsPayrollPeriod obj = new clsPayrollPeriod();
                string existingGuid = clsHcmApprovalDocuments.SelectGuidById(
                    clsHcmApprovalDocuments.TypePayrollPeriod, ID, CompanyID);
                if (!string.IsNullOrWhiteSpace(existingGuid))
                {
                    int existingStatus = clsHcmApprovalDocuments.GetDocumentStatusByGuid(
                        clsHcmApprovalDocuments.TypePayrollPeriod, existingGuid, CompanyID);
                    if (existingStatus == (int)DocumentStatus.PendingApproval ||
                        existingStatus == (int)DocumentStatus.Posted)
                        throw new InvalidOperationException(
                            "This payroll period cannot be edited while pending approval or after posting.");
                }

                int A = obj.UpdatePayrollPeriod(
                    ID,
                    Simulate.String(AName),
                    Simulate.String(EName),
                    StartDate,
                    EndDate, IsClosed,
                    ModificationUserID,
                    CompanyID
                );

                return A;
            }
            catch
            {
                throw;
            }
        }
    }
}
