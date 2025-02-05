using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Transactions;
using System;
using static WebApplication2.MainClasses.clsEnum;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WebApplication2.Controllers
{
    [Route("api/attachments")]
    [ApiController]
    public class AttachmentController : Controller
    {
      //  private readonly string _connectionString = "YourDatabaseConnectionString";

        /// <summary>
        /// Upload an attachment.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAttachment([FromForm] IFormFile file, [FromForm] string transactionId, [FromForm] string formName, [FromForm] string CreationUserID, [FromForm] string CompanyID)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    byte[] fileData = memoryStream.ToArray();
                    clsSQL clsSQL = new clsSQL();
                    using (var connection = new SqlConnection(clsSQL.CreateDataBaseConnectionString(Simulate.Integer32( CompanyID))))
                    {
                 

                        string query = @"INSERT INTO tbl_attachemnts (TransactionId, FormName, FileName, FileType, FileData, CreationUserID) 
                                     VALUES (@TransactionId, @FormName, @FileName, @FileType, @FileData, @CreationUserID)";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.Add("@TransactionId", SqlDbType.NVarChar).Value = transactionId;
                            command.Parameters.Add("@FormName", SqlDbType.NVarChar).Value = formName;
                            command.Parameters.Add("@FileName", SqlDbType.NVarChar).Value = file.FileName;
                            command.Parameters.Add("@FileType", SqlDbType.NVarChar).Value = file.ContentType;
                            command.Parameters.Add("@FileData", SqlDbType.VarBinary).Value = fileData;
                            command.Parameters.Add("@CreationUserID", SqlDbType.NVarChar).Value = CreationUserID;

                            connection.Open();
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                return Ok(new { message = "File uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get list of attachments for a transaction.
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetAttachments(string FormName, string transactionId,string CompanyID)
        {
            try
            {
                List<object> attachments = new List<object>();
                clsSQL clsSQL = new clsSQL();
                using (var connection = new SqlConnection(clsSQL.CreateDataBaseConnectionString(Simulate.Integer32( CompanyID))))
                {
                    string query = "SELECT Id, FileName, FileType, CreationDate FROM tbl_attachemnts WHERE FormName=@FormName  and  TransactionId = @TransactionId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@TransactionId", SqlDbType.NVarChar).Value = transactionId;
                        command.Parameters.Add("@FormName", SqlDbType.NVarChar).Value = FormName;

                        connection.Open();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                attachments.Add(new
                                {
                                    Id = reader["Id"],
                                    FileName = reader["FileName"],
                                    FileType = reader["FileType"],
                                    CreationDate = reader["CreationDate"]
                                });
                            }
                        }
                    }
                }

                return Ok(attachments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Download an attachment by ID.
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadAttachment(int id,string CompanyID)
        {
            try
            {
                clsSQL clsSQL=new clsSQL();
                using (var connection = new SqlConnection(clsSQL.CreateDataBaseConnectionString(Simulate.Integer32(CompanyID))))
                {
                    string query = "SELECT FileName, FileType, FileData FROM tbl_attachemnts WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                        connection.Open();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                string fileName = reader["FileName"].ToString();
                                string fileType = reader["FileType"].ToString();
                                byte[] fileData =   (byte[])reader["FileData"];

                                return File(fileData, fileType, fileName);
                            }
                        }
                    }
                }

                return NotFound("File not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
   
[HttpGet("Delete")]
        public async Task<IActionResult> DeleteAttachment(int id, string CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                 SqlParameter[] prm =
               { new SqlParameter("@Id", SqlDbType.Int) { Value = id },


                };
                    string query = "delete FROM tbl_attachemnts WHERE Id = @Id";
        var a =        clsSQL.ExecuteNonQueryStatement(query, clsSQL.CreateDataBaseConnectionString(Simulate.Integer32(CompanyID)), prm);


                return Ok(new { message = "File uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}