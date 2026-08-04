using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCRMActivity
    {
        public DataTable SelectCRMActivityByID(int ID, int OpportunityID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@OpportunityID", SqlDbType.Int) { Value = OpportunityID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT A.*, O.Title AS OpportunityTitle
                  FROM tbl_CRMActivity A
                  LEFT JOIN tbl_CRMOpportunity O ON O.ID = A.OpportunityID
                  WHERE (A.ID = @ID OR @ID = 0)
                    AND (A.OpportunityID = @OpportunityID OR @OpportunityID = 0)
                    AND A.CompanyID = @CompanyID
                  ORDER BY A.DueDate DESC, A.ID DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public int InsertCRMActivity(int OpportunityID, string ActivityType, string Subject,
            DateTime DueDate, bool IsDone, string Notes, int AssignedUserID,
            int CompanyID, int CreationUserID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@OpportunityID", SqlDbType.Int) { Value = OpportunityID },
                new SqlParameter("@ActivityType", SqlDbType.NVarChar, 50) { Value = ActivityType ?? "Task" },
                new SqlParameter("@Subject", SqlDbType.NVarChar, -1) { Value = Subject ?? "" },
                new SqlParameter("@DueDate", SqlDbType.DateTime) { Value = DueDate == DateTime.MinValue ? (object)DBNull.Value : DueDate },
                new SqlParameter("@IsDone", SqlDbType.Bit) { Value = IsDone },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },
                new SqlParameter("@AssignedUserID", SqlDbType.Int) { Value = AssignedUserID > 0 ? AssignedUserID : (object)DBNull.Value },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL clsSQL = new clsSQL();
            return Simulate.Integer32(clsSQL.ExecuteScalar(
                @"INSERT INTO tbl_CRMActivity
                  (OpportunityID, ActivityType, Subject, DueDate, IsDone, Notes, AssignedUserID, CompanyID, CreationUserID, CreationDate)
                  OUTPUT INSERTED.ID
                  VALUES (@OpportunityID, @ActivityType, @Subject, @DueDate, @IsDone, @Notes, @AssignedUserID, @CompanyID, @CreationUserID, @CreationDate)",
                prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
        }

        public int UpdateCRMActivity(int ID, int OpportunityID, string ActivityType, string Subject,
            DateTime DueDate, bool IsDone, string Notes, int AssignedUserID,
            int ModificationUserID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@OpportunityID", SqlDbType.Int) { Value = OpportunityID },
                new SqlParameter("@ActivityType", SqlDbType.NVarChar, 50) { Value = ActivityType ?? "Task" },
                new SqlParameter("@Subject", SqlDbType.NVarChar, -1) { Value = Subject ?? "" },
                new SqlParameter("@DueDate", SqlDbType.DateTime) { Value = DueDate == DateTime.MinValue ? (object)DBNull.Value : DueDate },
                new SqlParameter("@IsDone", SqlDbType.Bit) { Value = IsDone },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },
                new SqlParameter("@AssignedUserID", SqlDbType.Int) { Value = AssignedUserID > 0 ? AssignedUserID : (object)DBNull.Value },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_CRMActivity SET
                    OpportunityID=@OpportunityID, ActivityType=@ActivityType, Subject=@Subject,
                    DueDate=@DueDate, IsDone=@IsDone, Notes=@Notes, AssignedUserID=@AssignedUserID,
                    ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
                  WHERE ID=@ID AND CompanyID=@CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeleteCRMActivityByID(int ID, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL.ExecuteNonQueryStatement(
                @"DELETE FROM tbl_CRMActivity WHERE ID=@ID AND CompanyID=@CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }
    }
}
