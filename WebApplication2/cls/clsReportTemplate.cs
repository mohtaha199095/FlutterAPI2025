using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsReportTemplate
    {
        public DataTable SelectReportTemplateByID(int Id, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_ReportTemplate
                    where ID=@Id and CompanyID=@CompanyID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectReportTemplateList(string TemplateType, string EntityName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@TemplateType", SqlDbType.NVarChar, -1) { Value = TemplateType ?? "" },
                    new SqlParameter("@EntityName", SqlDbType.NVarChar, -1) { Value = EntityName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    select ID,CompanyID,TemplateName,TemplateType,EntityName,IsActive,CreationUserId,CreationDate,ModificationUserId,ModificationDate
                    from tbl_ReportTemplate
                    where CompanyID=@CompanyID
                      and (TemplateType=@TemplateType or @TemplateType='')
                      and (EntityName=@EntityName or @EntityName='')
                    order by ID desc
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ✅ Get the latest active template for a given type/entity
        public DataTable SelectLatestActiveTemplate(string TemplateType, string EntityName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@TemplateType", SqlDbType.NVarChar, -1) { Value = TemplateType ?? "" },
                    new SqlParameter("@EntityName", SqlDbType.NVarChar, -1) { Value = EntityName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    select top 1 * from tbl_ReportTemplate
                    where CompanyID=@CompanyID
                      and IsActive=1
                      and (TemplateType=@TemplateType or @TemplateType='')
                      and (EntityName=@EntityName or @EntityName='')
                    order by ID desc
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteReportTemplateByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                int A = clsSQL.ExecuteNonQueryStatement(@"
                    delete from tbl_ReportTemplate
                    where ID=@Id and CompanyID=@CompanyID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertReportTemplate(string TemplateName, string TemplateType, string EntityName, string TemplateJson,
            int CompanyID, int CreationUserId, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@TemplateName", SqlDbType.NVarChar, -1) { Value = TemplateName ?? "" },
                    new SqlParameter("@TemplateType", SqlDbType.NVarChar, -1) { Value = TemplateType ?? "" },
                    new SqlParameter("@EntityName", SqlDbType.NVarChar, -1) { Value = EntityName ?? "" },

                    // ✅ JSON is MAX
                    new SqlParameter("@TemplateJson", SqlDbType.NVarChar, -1) { Value = TemplateJson ?? "" },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true },
                };

                string sql = @"
                    insert into tbl_ReportTemplate
                    (TemplateName,TemplateType,EntityName,TemplateJson,CompanyID,IsActive,CreationUserId,CreationDate)
                    OUTPUT INSERTED.ID
                    values
                    (@TemplateName,@TemplateType,@EntityName,@TemplateJson,@CompanyID,@IsActive,@CreationUserId,@CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateReportTemplate(int ID, string TemplateName, string TemplateType, string EntityName, string TemplateJson,
            int ModificationUserId, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },

                    new SqlParameter("@TemplateName", SqlDbType.NVarChar, -1) { Value = TemplateName ?? "" },
                    new SqlParameter("@TemplateType", SqlDbType.NVarChar, -1) { Value = TemplateType ?? "" },
                    new SqlParameter("@EntityName", SqlDbType.NVarChar, -1) { Value = EntityName ?? "" },

                    // ✅ JSON is MAX
                    new SqlParameter("@TemplateJson", SqlDbType.NVarChar, -1) { Value = TemplateJson ?? "" },

                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                int A = clsSQL.ExecuteNonQueryStatement(@"
                    update tbl_ReportTemplate set
                        TemplateName=@TemplateName,
                        TemplateType=@TemplateType,
                        EntityName=@EntityName,
                        TemplateJson=@TemplateJson,
                        ModificationDate=@ModificationDate,
                        ModificationUserId=@ModificationUserId
                    where ID=@ID and CompanyID=@CompanyID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int SetActive(int ID, bool IsActive, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                int A = clsSQL.ExecuteNonQueryStatement(@"
                    update tbl_ReportTemplate set
                        IsActive=@IsActive,
                        ModificationDate=@ModificationDate,
                        ModificationUserId=@ModificationUserId
                    where ID=@ID and CompanyID=@CompanyID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }
         
    }
}
