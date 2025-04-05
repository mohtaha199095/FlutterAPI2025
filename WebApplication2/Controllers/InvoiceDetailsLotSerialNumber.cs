using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/InvoiceDetailsLotsSerialNumber")]
    public class InvoiceDetailsLotsSerialNumberController : Controller
    {
        [HttpGet]
        [Route("SelectInvoiceDetailsLotsSerialNumber")]
        public string SelectInvoiceDetailsLotsSerialNumber(Guid guid, Guid invoiceDetailsGuid, Guid itemGuid, int companyID)
        {
            try
            {
                clsInvoiceDetailsLotsSerialNumber invoiceDetailsLotsSerialNumber = new clsInvoiceDetailsLotsSerialNumber();
                DataTable dt = invoiceDetailsLotsSerialNumber.SelectInvoiceDetailsLotSerialNumber(guid, invoiceDetailsGuid, itemGuid, companyID);

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
        [Route("DeleteInvoiceDetailsLotsSerialNumberByGuid")]
        public bool DeleteInvoiceDetailsLotsSerialNumberByGuid(Guid guid, int companyID)
        {
            try
            {
                clsInvoiceDetailsLotsSerialNumber invoiceDetailsLotsSerialNumber = new clsInvoiceDetailsLotsSerialNumber();
                return invoiceDetailsLotsSerialNumber.DeleteInvoiceDetailsLotSerialNumberByGuid(guid, companyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("InsertInvoiceDetailsLotsSerialNumber")]
        public string InsertInvoiceDetailsLotsSerialNumber(
            Guid invoiceDetailsGuid,
            Guid itemGuid,
            int invoiceType,
            Guid invoiceGuid,
            Guid lotGuid,
            string serialNumber,
            bool status,
            int companyID,
            int creationUserID)
        {
            try
            {
                clsInvoiceDetailsLotsSerialNumber invoiceDetailsLotsSerialNumber = new clsInvoiceDetailsLotsSerialNumber();
                int result = invoiceDetailsLotsSerialNumber.InsertInvoiceDetailsLotSerialNumber(
                    invoiceDetailsGuid,
                    itemGuid,
                    invoiceType,
                    invoiceGuid,
                    lotGuid,
                    serialNumber,
                    status,
                    companyID,
                    creationUserID);

                return result > 0 ? "Success" : "Failure";
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Error = ex.Message });
            }
        }
    }
}
