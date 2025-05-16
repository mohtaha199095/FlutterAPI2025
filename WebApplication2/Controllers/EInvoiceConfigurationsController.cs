using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/EInvoice")]
    public class EInvoiceConfigurationsController : Controller
    {
        [HttpGet]
        [Route("Select")]
        public string Select(int ID, string Country, string UserCode, int CompanyID)
        {
            try
            {
                clsEInvoiceConfigurations cls = new clsEInvoiceConfigurations();
                DataTable dt = cls.SelectEInvoiceConfigurations(Simulate.Integer32( ID),Simulate.String( Country)
                    ,Simulate.String( UserCode), CompanyID);

                if (dt != null)
                {
                    string json = JsonConvert.SerializeObject(dt);
                    return json;
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Insert")]
        public int Insert(string Country, string UserCode, string SecretKey, string ActivityNumber,string TaxNumber, bool Active, int CompanyID)
        {
            try
            {
                clsEInvoiceConfigurations cls = new clsEInvoiceConfigurations();
                return cls.InsertEInvoiceConfigurations(Country, UserCode, SecretKey, ActivityNumber, TaxNumber, Active, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        public int Update(int ID, string Country, string UserCode, string SecretKey,string ActivityNumber,string TaxNumber, bool Active, int CompanyID)
        {
            try
            {
                clsEInvoiceConfigurations cls = new clsEInvoiceConfigurations();
                return cls.UpdateEInvoiceConfigurations(ID, Country, UserCode, SecretKey, ActivityNumber, TaxNumber, Active, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public bool Delete(int ID, int CompanyID)
        {
            try
            {
                clsEInvoiceConfigurations cls = new clsEInvoiceConfigurations();
                return cls.DeleteEInvoiceConfigurationsByID(ID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
