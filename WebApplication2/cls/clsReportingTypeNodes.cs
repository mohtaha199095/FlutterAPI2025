using Microsoft.Data.SqlClient;
using System.Data;
using System;

namespace WebApplication2.cls
{
    public class clsReportingTypeNodes
    {
        public DataTable SelectReportingTypeNodesByID(int Id, int ParentID,int ReportingTypeID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
       new SqlParameter("@ReportingTypeID", SqlDbType.Int) { Value = ReportingTypeID },
      
  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select tbl_ReportingTypeNodes.* ,
Parent.AName as ParentName , 
tbl_ReportingType.AName as  ReportingTypeName
from tbl_ReportingTypeNodes 
left join tbl_ReportingTypeNodes as Parent on Parent.id= tbl_ReportingTypeNodes.parentid
left join tbl_ReportingType   on tbl_ReportingType.id= tbl_ReportingTypeNodes.ReportingTypeID

where (tbl_ReportingTypeNodes.id=@Id or @Id=0 ) and  
(tbl_ReportingTypeNodes.ParentID=@ParentID or @ParentID=0 )   and 
(tbl_ReportingTypeNodes.ReportingTypeID=@ReportingTypeID or @ReportingTypeID=0 )   and 

(tbl_ReportingTypeNodes.CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteReportingTypeNodesByID(int Id,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_ReportingTypeNodes where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertReportingTypeNodes(string AName, string EName,int ReportingTypeID , int ParentID,int CompanyID, int CreationUserId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                           new SqlParameter("@ReportingTypeID", SqlDbType.Int) { Value = ReportingTypeID },
                                    new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                  
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_ReportingTypeNodes(AName,EName,ReportingTypeID,ParentID,CompanyID,CreationUserId,CreationDate)
                        OUTPUT INSERTED.ID values(@AName,@EName,@ReportingTypeID,@ParentID,@CompanyID,@CreationUserId,@CreationDate)";

                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateReportingTypeNodes(int ID, string AName, string EName,int ReportingTypeID, int ParentID, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                  new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                         new SqlParameter("@ReportingTypeID", SqlDbType.Int) { Value = ReportingTypeID },
                                    new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_ReportingTypeNodes set 
                       AName=@AName,
                       EName=@EName,
ReportingTypeID=@ReportingTypeID,
ParentID=@ParentID,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where id =@id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
