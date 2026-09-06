using Microsoft.Data.SqlClient;
using System;
using System.Data;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    /// <summary>
    /// HCM documents in the multi-level approval workflow.
    /// </summary>
    public static class clsHcmApprovalDocuments
    {
        public const int TypePayroll = 23;
        public const int TypeEmployeeContract = 27;
        public const int TypeEmployeeSalaryElement = 28;
        public const int TypePayrollPeriod = 29;
        public const int TypeEmployeeShiftAssignment = 30;
        public const int TypeLeaveRequest = 31;

        public static readonly int[] HcmTypeIds =
        {
            TypePayroll,
            TypeEmployeeContract,
            TypeEmployeeSalaryElement,
            TypePayrollPeriod,
            TypeEmployeeShiftAssignment,
            TypeLeaveRequest,
        };

        public static bool IsHcmType(int documentTypeId) =>
            Array.IndexOf(HcmTypeIds, documentTypeId) >= 0;

        public static bool TryGetDocumentMeta(
            int documentTypeId,
            string documentGuid,
            int companyId,
            SqlTransaction trn,
            out int branchId,
            out decimal amount,
            out string documentNumber,
            out int currentStatus,
            out int submittedBy)
        {
            branchId = 0;
            amount = 0;
            documentNumber = "";
            currentStatus = (int)DocumentStatus.Draft;
            submittedBy = 0;

            if (string.IsNullOrWhiteSpace(documentGuid)) return false;

            string tableName;
            string numberColumn;
            string amountColumn;
            string branchColumn;
            string creationUserColumn;

            switch (documentTypeId)
            {
                case TypeEmployeeContract:
                    tableName = "tbl_EmployeeContract";
                    numberColumn = "ContractNumber";
                    amountColumn = "BasicSalary";
                    branchColumn = "BranchID";
                    creationUserColumn = "CreationUserID";
                    break;
                case TypeEmployeeSalaryElement:
                    tableName = "tbl_EmployeeSalaryElements";
                    numberColumn = "ID";
                    amountColumn = "AssignedValue";
                    branchColumn = null;
                    creationUserColumn = "CreationUserId";
                    break;
                case TypePayrollPeriod:
                    tableName = "tbl_PayrollPeriod";
                    numberColumn = "PeriodAName";
                    amountColumn = null;
                    branchColumn = null;
                    creationUserColumn = "CreationUserID";
                    break;
                case TypePayroll:
                    tableName = "tbl_PayrollHeader";
                    numberColumn = "ID";
                    amountColumn = "NetSalary";
                    branchColumn = null;
                    creationUserColumn = "CreationUserID";
                    break;
                case TypeEmployeeShiftAssignment:
                    tableName = "tbl_EmployeeShiftAssignment";
                    numberColumn = "ID";
                    amountColumn = null;
                    branchColumn = null;
                    creationUserColumn = "CreationUserID";
                    break;
                case TypeLeaveRequest:
                    tableName = "tbl_LeaveRequest";
                    numberColumn = "ID";
                    amountColumn = "Days";
                    branchColumn = "BranchID";
                    creationUserColumn = "CreationUserID";
                    break;
                default:
                    return false;
            }

            clsSQL sql = new clsSQL();
            string amountExpr = amountColumn == null ? "0" : $"ISNULL([{amountColumn}],0)";
            string branchExpr = branchColumn == null ? "0" : $"ISNULL([{branchColumn}],0)";
            string numberExpr = numberColumn == "ID"
                ? "CAST(ID AS NVARCHAR(50))"
                : $"ISNULL(CAST([{numberColumn}] AS NVARCHAR(100)),'')";

            string query = $@"
SELECT {branchExpr} AS BranchID,
       {amountExpr} AS Amount,
       {numberExpr} AS DocumentNumber,
       ISNULL(DocumentStatus, 2) AS DocumentStatus,
       ISNULL([{creationUserColumn}], 0) AS CreationUserID
FROM {tableName}
WHERE Guid = @Guid AND CompanyID = @CompanyID";

            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            DataTable dt = sql.ExecuteQueryStatement(
                query,
                sql.CreateDataBaseConnectionString(companyId),
                prm,
                trn);

            if (dt == null || dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            branchId = Simulate.Integer32(row["BranchID"]);
            amount = Simulate.Decimal(row["Amount"]);
            documentNumber = Simulate.String(row["DocumentNumber"]);
            currentStatus = Simulate.Integer32(row["DocumentStatus"]);
            submittedBy = Simulate.Integer32(row["CreationUserID"]);
            return true;
        }

        public static void SetDocumentStatus(
            int documentTypeId,
            string documentGuid,
            int status,
            int userId,
            int companyId,
            SqlTransaction trn)
        {
            string tableName = GetTableName(documentTypeId);
            if (tableName == null) return;

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = status },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            sql.ExecuteNonQueryStatement($@"
UPDATE {tableName}
SET DocumentStatus = @DocumentStatus,
    PostedDate = CASE WHEN @DocumentStatus = 2 THEN GETDATE() ELSE PostedDate END,
    PostedByUserId = CASE WHEN @DocumentStatus = 2 THEN @UserId ELSE PostedByUserId END,
    SubmittedByUserId = CASE WHEN @DocumentStatus = 1 THEN @UserId ELSE SubmittedByUserId END,
    SubmittedDate = CASE WHEN @DocumentStatus = 1 THEN GETDATE() ELSE SubmittedDate END
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                prm,
                trn);
        }

        public static bool PostDocument(
            int documentTypeId,
            string documentGuid,
            int userId,
            int companyId,
            SqlTransaction trn)
        {
            switch (documentTypeId)
            {
                case TypeEmployeeContract:
                    return PostEmployeeContract(documentGuid, userId, companyId, trn);
                case TypeEmployeeSalaryElement:
                    return PostEmployeeSalaryElement(documentGuid, userId, companyId, trn);
                case TypePayrollPeriod:
                    return PostPayrollPeriod(documentGuid, userId, companyId, trn);
                case TypePayroll:
                    return new clsPayrollPostingService().PostPayrollHeaderByGuid(documentGuid, userId, companyId, trn);
                case TypeEmployeeShiftAssignment:
                    return PostEmployeeShiftAssignment(documentGuid, userId, companyId, trn);
                case TypeLeaveRequest:
                    return PostLeaveRequest(documentGuid, userId, companyId, trn);
                default:
                    return false;
            }
        }

        static string GetTableName(int documentTypeId)
        {
            switch (documentTypeId)
            {
                case TypeEmployeeContract: return "tbl_EmployeeContract";
                case TypeEmployeeSalaryElement: return "tbl_EmployeeSalaryElements";
                case TypePayrollPeriod: return "tbl_PayrollPeriod";
                case TypePayroll: return "tbl_PayrollHeader";
                case TypeEmployeeShiftAssignment: return "tbl_EmployeeShiftAssignment";
                case TypeLeaveRequest: return "tbl_LeaveRequest";
                default: return null;
            }
        }

        static bool PostEmployeeContract(string documentGuid, int userId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] sel =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ID, EmployeeID, ISNULL(IsActive,0) AS IsActive
FROM tbl_EmployeeContract
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), sel, trn);

            if (dt == null || dt.Rows.Count == 0) return false;

            int id = Simulate.Integer32(dt.Rows[0]["ID"]);
            int employeeId = Simulate.Integer32(dt.Rows[0]["EmployeeID"]);
            bool shouldActivate = Simulate.Bool(dt.Rows[0]["IsActive"]);

            if (shouldActivate)
            {
                new clsEmployeeContract().DeactivateActiveContracts(employeeId, companyId, userId, trn);
            }

            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = shouldActivate },
            };

            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_EmployeeContract
SET IsActive = @IsActive,
    DocumentStatus = 2,
    PostedDate = GETDATE(),
    PostedByUserId = @UserId,
    ModificationUserID = @UserId,
    ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);

            if (shouldActivate && employeeId > 0)
            {
                try
                {
                    new clsLeave().SeedBalancesFromContract(employeeId, companyId, userId, trn);
                }
                catch { /* best-effort */ }
            }

            return id > 0;
        }

        static bool PostEmployeeSalaryElement(string documentGuid, int userId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
            };

            int rows = sql.ExecuteNonQueryStatement(@"
UPDATE tbl_EmployeeSalaryElements
SET IsActive = 1,
    DocumentStatus = 2,
    PostedDate = GETDATE(),
    PostedByUserId = @UserId,
    ModificationUserID = @UserId,
    ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);

            return rows > 0;
        }

        static bool PostPayrollPeriod(string documentGuid, int userId, int companyId, SqlTransaction trn)
        {
            SetDocumentStatus(TypePayrollPeriod, documentGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
            return true;
        }

        static bool PostEmployeeShiftAssignment(string documentGuid, int userId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
            };

            int rows = sql.ExecuteNonQueryStatement(@"
UPDATE tbl_EmployeeShiftAssignment
SET IsActive = 1,
    DocumentStatus = 2,
    PostedDate = GETDATE(),
    PostedByUserId = @UserId,
    ModificationUserID = @UserId,
    ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);

            return rows > 0;
        }

        static bool PostLeaveRequest(string documentGuid, int userId, int companyId, SqlTransaction trn)
        {
            return new clsLeave().ApproveLeaveRequest(documentGuid, userId, companyId, trn);
        }

        public static string SelectGuidById(int documentTypeId, int id, int companyId, SqlTransaction trn = null)
        {
            string tableName = GetTableName(documentTypeId);
            if (tableName == null || id <= 0) return "";

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            object scalar = sql.ExecuteScalar(
                $"SELECT CAST(Guid AS NVARCHAR(50)) FROM {tableName} WHERE ID = @ID AND CompanyID = @CompanyID",
                prm,
                sql.CreateDataBaseConnectionString(companyId),
                trn);

            return Simulate.String(scalar);
        }

        public static int GetDocumentStatusByGuid(int documentTypeId, string documentGuid, int companyId, SqlTransaction trn = null)
        {
            if (!TryGetDocumentMeta(documentTypeId, documentGuid, companyId, trn,
                    out _, out _, out _, out int status, out _))
                return (int)DocumentStatus.Posted;

            return status;
        }
    }
}
