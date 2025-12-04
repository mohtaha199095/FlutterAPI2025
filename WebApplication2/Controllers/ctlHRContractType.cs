using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{

    [Route("api/ctlHRContractType")]
    public class ctlHRContractType : Controller
    {
         
        [HttpGet]
        [Route("SelectHRContractTypeByID")]
        public string SelectHRContractTypeByID(int ID, int CompanyID)
        {
            try
            {

                clsHRContractType clsHRContractType = new clsHRContractType();
                DataTable dt = clsHRContractType.SelectHRContractTypeByID(ID, "", "", CompanyID);
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
        [Route("DeleteHRContractTypeByID")]
        public bool DeleteHRContractTypeByID(int ID, int CompanyID)
        {
            try
            {
                clsHRContractType Countries = new clsHRContractType();
                bool A = Countries.DeleteHRContractTypeByID(ID, CompanyID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertHRContractType")]
        public int InsertHRContractType(string AName, string EName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsHRContractType clsCountries = new clsHRContractType();
                int A = clsCountries.InsertHRContractType(Simulate.String(AName), Simulate.String(EName),
                     CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateHRContractType")]
        public int UpdateHRContractType(int ID, string AName, string EName, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsHRContractType clsCountries = new clsHRContractType();
                int A = clsCountries.UpdateHRContractType(ID, Simulate.String(AName), Simulate.String(EName),

                    ModificationUserId, CompanyID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
