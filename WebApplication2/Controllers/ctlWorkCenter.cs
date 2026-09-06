using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlWorkCenter")]
    public class ctlWorkCenter : Controller
    {
        [HttpGet]
        [Route("SelectWorkCenter")]
        public string SelectWorkCenter(int ID, string WorkCenterCode, string AName, int CompanyID)
        {
            try
            {
                clsWorkCenter cls = new clsWorkCenter();
                DataTable dt = cls.SelectWorkCenter(ID, WorkCenterCode, AName, CompanyID);
                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("DeleteWorkCenterByID")]
        public bool DeleteWorkCenterByID(int ID, int CompanyID)
        {
            try
            {
                clsWorkCenter cls = new clsWorkCenter();
                return cls.DeleteWorkCenterByID(ID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertWorkCenter")]
        public int InsertWorkCenter(
            string WorkCenterCode,
            string AName,
            string EName,
            int BranchID,
            decimal CapacityPerDay,
            bool IsActive,
            string Notes,
            int CompanyID,
            int CreationUserId,
            decimal HourlyRate = 0)
        {
            try
            {
                clsWorkCenter cls = new clsWorkCenter();
                return cls.InsertWorkCenter(
                    Simulate.String(WorkCenterCode),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    BranchID,
                    CapacityPerDay,
                    IsActive,
                    Simulate.String(Notes),
                    CompanyID,
                    CreationUserId,
                    null,
                    HourlyRate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("UpdateWorkCenter")]
        public int UpdateWorkCenter(
            int ID,
            string WorkCenterCode,
            string AName,
            string EName,
            int BranchID,
            decimal CapacityPerDay,
            bool IsActive,
            string Notes,
            int ModificationUserId,
            int CompanyID,
            decimal HourlyRate = 0)
        {
            try
            {
                clsWorkCenter cls = new clsWorkCenter();
                return cls.UpdateWorkCenter(
                    ID,
                    Simulate.String(WorkCenterCode),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    BranchID,
                    CapacityPerDay,
                    IsActive,
                    Simulate.String(Notes),
                    ModificationUserId,
                    CompanyID,
                    HourlyRate);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
