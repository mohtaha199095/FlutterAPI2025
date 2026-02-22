using FastReport.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using WebApplication2.cls;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Route("api/EInvoice")]
    [ApiController]
    public class EInvoiceController : Controller
    {
        [HttpPost]
        [Route("SubmitEInvoice")]
        public IActionResult SubmitEInvoice(int CompanyID,string InvoiceGuid,string FinancingGuid,string ReturnInvoiceNumber)
        {
            clsEInvoiceService clsEInvoiceService = new clsEInvoiceService();
            EInvoiceSubmitResult res =  clsEInvoiceService.SubmitEInvoice(CompanyID,  InvoiceGuid,  FinancingGuid,  ReturnInvoiceNumber);
            return StatusCode(res.HttpStatusCode, res);
        }


       
    }
}
