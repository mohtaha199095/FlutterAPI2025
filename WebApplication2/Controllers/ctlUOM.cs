using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlUOM")]
    public class ctlUOM : Controller
    {
        [HttpGet]
        [Route("SelectUOM")]
        public string SelectUOM(int ID, string AName, string EName, string Code, int CompanyID, int IsActive)
        {
            try
            {
                clsUOM clsUOM = new clsUOM();
                DataTable dt = clsUOM.SelectUOM(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(Code), CompanyID, IsActive);

                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("DeleteUOMByID")]
        public bool DeleteUOMByID(int ID, int CompanyID)
        {
            try
            {
                clsUOM clsUOM = new clsUOM();
                bool A = clsUOM.DeleteUOMByID(ID, CompanyID);
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertUOM")]
        public int InsertUOM(string AName, string EName, string Symbol, int DecimalPlaces, bool IsActive, int CompanyID, int CreationUserId)
        {
            try
            {
                clsUOM clsUOM = new clsUOM();
                int A = clsUOM.InsertUOM(
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(Symbol),
                    Simulate.Integer32(DecimalPlaces),
                    Simulate.Bool(IsActive),
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
        [Route("UpdateUOM")]
        public int UpdateUOM(int ID, string AName, string EName, string Symbol, int DecimalPlaces, bool IsActive, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsUOM clsUOM = new clsUOM();
                int A = clsUOM.UpdateUOM(
                    Simulate.Integer32(ID),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(Symbol),
                    Simulate.Integer32(DecimalPlaces),
                    Simulate.Bool(IsActive),
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
    }
}
