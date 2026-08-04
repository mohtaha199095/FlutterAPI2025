using Microsoft.Data.SqlClient;
using System;
using System.Data;
using WebApplication2.cls.Reports;

namespace WebApplication2.cls
{
    /// <summary>
    /// Per-company transaction print definitions (page + engine + template/frx).
    /// </summary>
    public class clsTransactionReport
    {
        public DataTable SelectTransactionReportByID(int id, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_TransactionReport
                    where ID = @Id and CompanyID = @CompanyID
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectTransactionReportList(string pageName, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value = pageName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select ID, CompanyID, PageName, ReportName, AName, EName,
                           ReportEngine, FastReportFileName, ReportTemplateID,
                           case when ReportFrxXml is null or ReportFrxXml = '' then null else N'1' end as ReportFrxXml,
                           IsDefault, IsActive, SortOrder,
                           CreationUserID, CreationDate, ModificationUserID, ModificationDate
                    from tbl_TransactionReport
                    where CompanyID = @CompanyID
                      and (PageName = @PageName or @PageName = '')
                      and IsActive = 1
                    order by IsDefault desc, SortOrder asc, ID asc
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectTransactionReportByPageAndName(
            string pageName, string reportName, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value = pageName ?? "" },
                    new SqlParameter("@ReportName", SqlDbType.NVarChar, -1) { Value = reportName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select top 1 *
                    from tbl_TransactionReport
                    where CompanyID = @CompanyID
                      and PageName = @PageName
                      and ReportName = @ReportName
                      and IsActive = 1
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Same lookup without IsActive filter (used when seeding defaults).</summary>
        public DataTable SelectTransactionReportByPageAndNameAny(
            string pageName, string reportName, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value = pageName ?? "" },
                    new SqlParameter("@ReportName", SqlDbType.NVarChar, -1) { Value = reportName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select top 1 *
                    from tbl_TransactionReport
                    where CompanyID = @CompanyID
                      and PageName = @PageName
                      and ReportName = @ReportName
                    order by IsActive desc, ID asc
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectDefaultTransactionReport(string pageName, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value = pageName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select top 1 *
                    from tbl_TransactionReport
                    where CompanyID = @CompanyID
                      and PageName = @PageName
                      and IsActive = 1
                    order by IsDefault desc, SortOrder asc, ID asc
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertTransactionReport(
            string pageName,
            string reportName,
            string aName,
            string eName,
            string reportEngine,
            string fastReportFileName,
            int? reportTemplateID,
            bool isDefault,
            bool isActive,
            int sortOrder,
            int companyID,
            int creationUserID)
        {
            try
            {
                if (isDefault)
                    ClearDefaultForPage(pageName, companyID);

                SqlParameter[] prm =
                {
                    new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value = pageName ?? "" },
                    new SqlParameter("@ReportName", SqlDbType.NVarChar, -1) { Value = reportName ?? "" },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = aName ?? "" },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = eName ?? "" },
                    new SqlParameter("@ReportEngine", SqlDbType.NVarChar, -1) { Value = reportEngine ?? "" },
                    new SqlParameter("@FastReportFileName", SqlDbType.NVarChar, -1) { Value = fastReportFileName ?? "" },
                    new SqlParameter("@ReportTemplateID", SqlDbType.Int)
                    {
                        Value = reportTemplateID.HasValue ? reportTemplateID.Value : DBNull.Value
                    },
                    new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = isDefault },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive },
                    new SqlParameter("@SortOrder", SqlDbType.Int) { Value = sortOrder },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = creationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    insert into tbl_TransactionReport
                        (CompanyID, PageName, ReportName, AName, EName, ReportEngine, FastReportFileName,
                         ReportTemplateID, IsDefault, IsActive, SortOrder, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    values
                        (@CompanyID, @PageName, @ReportName, @AName, @EName, @ReportEngine, @FastReportFileName,
                         @ReportTemplateID, @IsDefault, @IsActive, @SortOrder, @CreationUserID, @CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyID)));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateTransactionReport(
            int id,
            string pageName,
            string reportName,
            string aName,
            string eName,
            string reportEngine,
            string fastReportFileName,
            int? reportTemplateID,
            bool isDefault,
            bool isActive,
            int sortOrder,
            int modificationUserID,
            int companyID)
        {
            try
            {
                if (isDefault)
                    ClearDefaultForPage(pageName, companyID, id);

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value = pageName ?? "" },
                    new SqlParameter("@ReportName", SqlDbType.NVarChar, -1) { Value = reportName ?? "" },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = aName ?? "" },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = eName ?? "" },
                    new SqlParameter("@ReportEngine", SqlDbType.NVarChar, -1) { Value = reportEngine ?? "" },
                    new SqlParameter("@FastReportFileName", SqlDbType.NVarChar, -1) { Value = fastReportFileName ?? "" },
                    new SqlParameter("@ReportTemplateID", SqlDbType.Int)
                    {
                        Value = reportTemplateID.HasValue ? reportTemplateID.Value : DBNull.Value
                    },
                    new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = isDefault },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive },
                    new SqlParameter("@SortOrder", SqlDbType.Int) { Value = sortOrder },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = modificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(@"
                    update tbl_TransactionReport set
                        PageName = @PageName,
                        ReportName = @ReportName,
                        AName = @AName,
                        EName = @EName,
                        ReportEngine = @ReportEngine,
                        FastReportFileName = @FastReportFileName,
                        ReportTemplateID = @ReportTemplateID,
                        IsDefault = @IsDefault,
                        IsActive = @IsActive,
                        SortOrder = @SortOrder,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    where ID = @ID and CompanyID = @CompanyID
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateReportFrxXml(int id, string reportFrxXml, int modificationUserID, int companyID)
        {
            try
            {
                object frxValue = string.IsNullOrWhiteSpace(reportFrxXml)
                    ? (object)DBNull.Value
                    : reportFrxXml;

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@ReportFrxXml", SqlDbType.NVarChar, -1)
                    {
                        Value = frxValue
                    },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = modificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(@"
                    update tbl_TransactionReport set
                        ReportFrxXml = @ReportFrxXml,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    where ID = @ID and CompanyID = @CompanyID
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Clears a company customization so print falls back to the shared Reports\ .frx base.
        /// </summary>
        public int ClearReportFrxXml(int id, int modificationUserID, int companyID)
        {
            return UpdateReportFrxXml(id, null, modificationUserID, companyID);
        }

        public bool DeleteTransactionReportByID(int id, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                clsSQL.ExecuteNonQueryStatement(@"
                    delete from tbl_TransactionReport
                    where ID = @Id and CompanyID = @CompanyID
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int SetAsDefaultTransactionReport(int id, int modificationUserID, int companyID)
        {
            try
            {
                DataTable dt = SelectTransactionReportByID(id, companyID);
                if (dt == null || dt.Rows.Count == 0)
                    return 0;

                string pageName = Simulate.String(dt.Rows[0]["PageName"]);
                ClearDefaultForPage(pageName, companyID, id);

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = modificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(@"
                    update tbl_TransactionReport set
                        IsDefault = 1,
                        IsActive = 1,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    where ID = @ID and CompanyID = @CompanyID
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int LinkJsonTemplateReport(
            int transactionReportID,
            string pageName,
            string reportName,
            string aName,
            string eName,
            int reportTemplateID,
            bool setAsDefault,
            int companyID,
            int userId)
        {
            try
            {
                if (reportTemplateID <= 0)
                    return 0;

                clsReportTemplate tpl = new clsReportTemplate();
                tpl.SetActive(reportTemplateID, true, userId, companyID);

                int id = transactionReportID;
                if (id > 0)
                {
                    UpdateTransactionReport(
                        id,
                        pageName,
                        reportName,
                        aName,
                        eName,
                        "JsonTemplate",
                        "",
                        reportTemplateID,
                        setAsDefault,
                        true,
                        0,
                        userId,
                        companyID);
                }
                else
                {
                    id = InsertTransactionReport(
                        pageName,
                        reportName,
                        aName,
                        eName,
                        "JsonTemplate",
                        "",
                        reportTemplateID,
                        setAsDefault,
                        true,
                        0,
                        companyID,
                        userId);
                }

                if (setAsDefault && id > 0)
                    SetAsDefaultTransactionReport(id, userId, companyID);

                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string SelectLatestHeaderGuidForPage(string pageName, int companyID)
        {
            if (pageName == "JournalVoucherAdd")
            {
                clsJournalVoucherHeader cls = new clsJournalVoucherHeader();
                return cls.SelectLatestJournalVoucherGuid(companyID);
            }

            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyID);
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
            };

            string printPage = clsTransactionReportDefaults.ResolvePrintPageName(pageName);
            string query = printPage switch
            {
                "InvoicePageAdd" => @"
                    select top 1 Guid from tbl_InvoiceHeader
                    where CompanyID = @CompanyID
                    order by InvoiceDate desc, CreationDate desc",
                "CashVoucherAdd" => @"
                    select top 1 Guid from tbl_CashVoucherHeader
                    where CompanyID = @CompanyID
                    order by VoucherDate desc, CreationDate desc",
                "CreditNotePageAdd" => @"
                    select top 1 Guid from tbl_CreditNoteHeader
                    where CompanyID = @CompanyID
                    order by VoucherDate desc, CreationDate desc",
                "FinancingHeaderAdd" => @"
                    select top 1 Guid from tbl_FinancingHeader
                    where CompanyID = @CompanyID
                    order by VoucherDate desc, CreationDate desc",
                "EmployeeContractAdd" => @"
                    select top 1 cast(ID as nvarchar(50)) from tbl_EmployeeContract
                    where CompanyID = @CompanyID
                    order by CreationDate desc",
                _ => "",
            };

            if (string.IsNullOrEmpty(query))
                return "";

            object scalar = sql.ExecuteScalar(query, prm, conn);
            return Simulate.String(scalar);
        }

        private void ClearDefaultForPage(string pageName, int companyID, int exceptId = 0)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value = pageName ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                new SqlParameter("@ExceptId", SqlDbType.Int) { Value = exceptId },
            };

            clsSQL clsSQL = new clsSQL();
            clsSQL.ExecuteNonQueryStatement(@"
                update tbl_TransactionReport set IsDefault = 0
                where CompanyID = @CompanyID
                  and PageName = @PageName
                  and (ID <> @ExceptId or @ExceptId = 0)
            ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
        }
    }
}
