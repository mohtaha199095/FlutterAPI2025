using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/InvoiceDetailsLotsTracking")]
    public class InvoiceDetailsLotsTrackingController : Controller
    {
        [HttpGet]
        [Route("SelectInvoiceDetailsLotsTracking")]
        public string SelectInvoiceDetailsLotsTracking(Guid InvoiceDetailsGuid, Guid ItemGuid, int InvoiceType
            , string LotNumber, DateTime date1, DateTime date2,
            int CompanyID)
        {
            try
            {
                clsInvoiceDetailsLotsTracking invoiceDetailsLotsTracking = new clsInvoiceDetailsLotsTracking();
             
                DataTable dt = invoiceDetailsLotsTracking.SelectInvoiceDetailsLotsTracking( InvoiceDetailsGuid,  ItemGuid, 
                    InvoiceType,
       Simulate.String(       LotNumber),  date1,  date2,
             CompanyID);

                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteInvoiceDetailsLotsTrackingByGuid")]
        public bool DeleteInvoiceDetailsLotsTrackingByGuid(Guid guid, int companyID)
        {
            try
            {
                clsInvoiceDetailsLotsTracking invoiceDetailsLotsTracking = new clsInvoiceDetailsLotsTracking();
                return invoiceDetailsLotsTracking.DeleteInvoiceDetailsLotsTrackingByGuid(guid, companyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("InsertInvoiceDetailsLotsTracking")]
        public string InsertInvoiceDetailsLotsTracking(
            Guid invoiceDetailsGuid,
            Guid itemGuid,
            int invoiceType,
            Guid invoiceGuid,
            string lotNumber,
            DateTime expiryDate,
            decimal qty,
            int companyID,
            int creationUserID)
        {
            try
            {
                clsInvoiceDetailsLotsTracking invoiceDetailsLotsTracking = new clsInvoiceDetailsLotsTracking();
               
                string result = invoiceDetailsLotsTracking.InsertInvoiceDetailsLotsTracking(
                    invoiceDetailsGuid,
                    itemGuid,
                    invoiceType,
                    invoiceGuid,
                    lotNumber,
                    expiryDate,
                    qty,
                    companyID,
                    creationUserID);

                return result != "" ? "Success" : "Failure";
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Error = ex.Message });
            }
        }
    }
}
