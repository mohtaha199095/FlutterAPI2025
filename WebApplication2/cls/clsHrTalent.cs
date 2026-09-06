using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    /// <summary>Basic HR talent modules: recruitment, performance, disciplinary, documents.</summary>
    public class clsHrTalent
    {
        public DataTable SelectJobOpenings(int id, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_HrJobOpening
WHERE CompanyID=@CompanyID AND (@ID=0 OR ID=@ID)
ORDER BY ID DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int InsertJobOpening(string title, string department, string status, string notes,
            int companyId, int userId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Title", SqlDbType.NVarChar, -1) { Value = title ?? "" },
                new SqlParameter("@Department", SqlDbType.NVarChar, -1) { Value = department ?? "" },
                new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = status ?? "Open" },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = notes ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_HrJobOpening (Title, Department, Status, Notes, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@Title, @Department, @Status, @Notes, @CompanyID, @UserID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(companyId), null));
        }

        public DataTable SelectPerformanceReviews(int id, int employeeId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT p.*, ISNULL(e.AName,'') AS EmployeeName
FROM tbl_HrPerformanceReview p
LEFT JOIN tbl_employee e ON e.ID = p.EmployeeID AND e.CompanyID = p.CompanyID
WHERE p.CompanyID=@CompanyID
  AND (@ID=0 OR p.ID=@ID)
  AND (@EmployeeID=0 OR p.EmployeeID=@EmployeeID)
ORDER BY p.ReviewDate DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int InsertPerformanceReview(int employeeId, DateTime reviewDate, decimal rating,
            string summary, int companyId, int userId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@ReviewDate", SqlDbType.DateTime) { Value = reviewDate.Date },
                new SqlParameter("@Rating", SqlDbType.Decimal) { Value = rating },
                new SqlParameter("@Summary", SqlDbType.NVarChar, -1) { Value = summary ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_HrPerformanceReview (EmployeeID, ReviewDate, Rating, Summary, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@EmployeeID, @ReviewDate, @Rating, @Summary, @CompanyID, @UserID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(companyId), null));
        }

        public DataTable SelectDisciplinaryActions(int id, int employeeId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT d.*, ISNULL(e.AName,'') AS EmployeeName
FROM tbl_HrDisciplinaryAction d
LEFT JOIN tbl_employee e ON e.ID = d.EmployeeID AND e.CompanyID = d.CompanyID
WHERE d.CompanyID=@CompanyID
  AND (@ID=0 OR d.ID=@ID)
  AND (@EmployeeID=0 OR d.EmployeeID=@EmployeeID)
ORDER BY d.ActionDate DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int InsertDisciplinaryAction(int employeeId, DateTime actionDate, string actionType,
            string description, int companyId, int userId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@ActionDate", SqlDbType.DateTime) { Value = actionDate.Date },
                new SqlParameter("@ActionType", SqlDbType.NVarChar, 100) { Value = actionType ?? "" },
                new SqlParameter("@Description", SqlDbType.NVarChar, -1) { Value = description ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_HrDisciplinaryAction (EmployeeID, ActionDate, ActionType, Description, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@EmployeeID, @ActionDate, @ActionType, @Description, @CompanyID, @UserID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(companyId), null));
        }

        public DataTable SelectEmployeeDocuments(int id, int employeeId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT d.*, ISNULL(e.AName,'') AS EmployeeName
FROM tbl_HrEmployeeDocument d
LEFT JOIN tbl_employee e ON e.ID = d.EmployeeID AND e.CompanyID = d.CompanyID
WHERE d.CompanyID=@CompanyID
  AND (@ID=0 OR d.ID=@ID)
  AND (@EmployeeID=0 OR d.EmployeeID=@EmployeeID)
ORDER BY d.ExpiryDate, d.DocumentName",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int InsertEmployeeDocument(int employeeId, string documentName, string documentType,
            DateTime issueDate, DateTime expiryDate, string notes, int companyId, int userId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@DocumentName", SqlDbType.NVarChar, -1) { Value = documentName ?? "" },
                new SqlParameter("@DocumentType", SqlDbType.NVarChar, 100) { Value = documentType ?? "" },
                new SqlParameter("@IssueDate", SqlDbType.DateTime) { Value = issueDate.Date },
                new SqlParameter("@ExpiryDate", SqlDbType.DateTime) { Value = expiryDate.Date },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = notes ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_HrEmployeeDocument
  (EmployeeID, DocumentName, DocumentType, IssueDate, ExpiryDate, Notes, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@EmployeeID, @DocumentName, @DocumentType, @IssueDate, @ExpiryDate, @Notes, @CompanyID, @UserID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(companyId), null));
        }
    }
}
