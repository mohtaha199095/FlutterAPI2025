using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlCity")]
    public class ctlCity : Controller
    {
       

      


        [HttpGet]
        [Route("SelectCityByID")]
        public string SelectCityByID(int ID, int CountryID,int CompanyID)
        {
            try
            {

                clsCity clsCity = new clsCity();
                DataTable dt = clsCity.SelectCityByID(ID, "", "", CountryID,CompanyID);
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
        [Route("DeleteCityByID")]
        public bool DeleteCityByID(int ID, int CompanyID)
        {
            try
            {
                clsCity Countries = new clsCity();
                bool A = Countries.DeleteCityByID(ID, CompanyID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertCity")]
        public int InsertCity(string AName, string EName, int CountryID, int CompanyID, int CreationUserId)
        {
            try
            {
                clsCity clsCountries = new clsCity();
                int A = clsCountries.InsertCity(Simulate.String(AName), Simulate.String(EName),
                     Simulate.Integer32(CountryID), CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateCity")]
        public int UpdateCity(int ID, string AName, string EName, int CountryID, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsCity clsCountries = new clsCity();
                int A = clsCountries.UpdateCity(ID, Simulate.String(AName), Simulate.String(EName),
                    Simulate.Integer32(CountryID),
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
