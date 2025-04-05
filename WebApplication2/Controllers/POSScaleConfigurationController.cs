using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Collections.Generic;
using WebApplication2.cls;
using WebApplication2.Models;
using Newtonsoft.Json;

namespace WebApplication2.Controllers
{
    [Route("api/POSScale")]
    [ApiController]
    public class POSScaleConfigurationController : ControllerBase
    {
        private readonly clsPOSScaleConfiguration _scaleService;

        public POSScaleConfigurationController()
        {
            _scaleService = new clsPOSScaleConfiguration();
        }

        // GET: Fetch all POS Scale Configurations
        [HttpGet]
        [Route("GetAllScales")]
        public IActionResult GetAllScales(int CompanyID)
        {
            try
            {
                DataTable dt = _scaleService.SelectScales(0, "", "", CompanyID);
                return Ok(dt);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: Fetch a specific POS Scale Configuration by ID
        [HttpGet]
        [Route("SelectScaleByID")]
        public string SelectScaleByID(int ID, int CompanyID)
        {
            try
            {
                DataTable dt = _scaleService.SelectScales(ID, "", "", CompanyID);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                } else {

 
                    return "";
                }
                

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // POST: Insert a new POS Scale Configuration
        [HttpPost]
        [Route("InsertScale")]
        public IActionResult InsertScale(  [FromBody] TblPOSScaleConfiguration scale)
        {
            try
            {
                int newId = _scaleService.InsertScale(
                    scale.ScaleName, scale.ScaleType, scale.ConnectionType, scale.PortName,
                    scale.BaudRate, scale.DataBits, scale.Parity, scale.StopBits, scale.BarcodePrefix,
                    scale.SKULength,
                    scale.AutoDetect, scale.DefaultPrintType, scale.Divisionfactor, scale.Status, scale.CompanyID, scale.CreationUserID
                );

                return Ok(new { ID = newId, Message = "Scale inserted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: Update an existing POS Scale Configuration
        [HttpPost]
        [Route("UpdateScale")]
        public IActionResult UpdateScale([FromBody] TblPOSScaleConfiguration scale)
        {
            try
            {
                int rowsAffected = _scaleService.UpdateScale(
                    scale.ID, scale.ScaleName, scale.ScaleType, scale.ConnectionType, scale.PortName,
                    scale.BaudRate, scale.DataBits, scale.Parity, scale.StopBits, scale.BarcodePrefix,
                     scale.SKULength,
                    scale.AutoDetect, scale.DefaultPrintType, scale.Divisionfactor, scale.Status, scale.ModificationUserID, scale.CompanyID
                );

                if (rowsAffected == 0)
                    return NotFound("Scale configuration not found.");

                return Ok(new { Message = "Scale updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: Remove a POS Scale Configuration
        [HttpPost]
        [Route("DeleteScaleByID")]
        public IActionResult DeleteScaleByID(int ID, int CompanyID)
        {
            try
            {
                bool isDeleted = _scaleService.DeleteScaleByID(ID, CompanyID);

                if (!isDeleted)
                    return NotFound("Scale configuration not found.");

                return Ok(new { Message = "Scale deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
 

namespace WebApplication2.Models
{
    public class TblPOSScaleConfiguration
    {
        public int ID { get; set; }
        public string ScaleName { get; set; }
        public string ScaleType { get; set; }
        public string ConnectionType { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public string Parity { get; set; }
        public int StopBits { get; set; }
        public string BarcodePrefix { get; set; }
        public int SKULength { get; set; }
        
        public bool AutoDetect { get; set; }
        public string DefaultPrintType { get; set; }
        public int Divisionfactor { get; set; }
        public bool Status { get; set; }
        public int CompanyID { get; set; }
        public int CreationUserID { get; set; }
        public DateTime CreationDate { get; set; }
        public int ModificationUserID { get; set; }
        public DateTime ModificationDate { get; set; }

        public TblPOSScaleConfiguration()
        {
            // Ensuring default values are assigned
            ID = 0;
            ScaleName = string.Empty;
            ScaleType = string.Empty;
            ConnectionType = string.Empty;
            PortName = string.Empty;
            BaudRate = 9600; // Default standard baud rate for serial scales
            DataBits = 8; // Default data bits
            Parity = "None"; // Default parity setting
            StopBits = 1; // Default stop bits
            BarcodePrefix = string.Empty;
            SKULength = 0;
            AutoDetect = false;
            DefaultPrintType = "Weight"; // Default behavior
            Status = true; // Default status active
            CompanyID = 0;
            CreationUserID = 0;
            CreationDate = DateTime.Now;
            ModificationUserID = 0;
            ModificationDate = DateTime.Now;
        }
    }
}
