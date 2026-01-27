using Microsoft.Data.SqlClient;
using System;
using System.Data;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsUOM
    {
        public DataTable SelectUOM(int Id, string AName, string EName, string Symbol, int CompanyId, int IsActive)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@Symbol", SqlDbType.NVarChar, -1) { Value = Symbol },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                    new SqlParameter("@IsActive", SqlDbType.Int) { Value = IsActive }, // -1 = All
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
select * from tbl_UOM
where (Id=@Id or @Id=0)
  and (AName=@AName or @AName='')
  and (EName=@EName or @EName='')
  and (Symbol=@Symbol or @Symbol='')
  and (CompanyId=@CompanyId or @CompanyId=0)
  and (IsActive=@IsActive or @IsActive=-1)
", clsSQL.CreateDataBaseConnectionString(CompanyId), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteUOMByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                };

                clsSQL.ExecuteNonQueryStatement(@"delete from tbl_UOM where (Id=@Id)", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertUOM(string AName, string EName, string Symbol, int DecimalPlaces,
            bool IsActive, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Symbol", SqlDbType.NVarChar,-1) { Value = Symbol },
                    new SqlParameter("@DecimalPlaces", SqlDbType.Int) { Value = DecimalPlaces },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"
insert into tbl_UOM(AName,EName,Symbol,DecimalPlaces,IsActive,CompanyID,CreationUserId,CreationDate)
OUTPUT INSERTED.ID
values(@AName,@EName,@Symbol,@DecimalPlaces,@IsActive,@CompanyID,@CreationUserId,@CreationDate)
";

                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateUOM(int Id, string AName, string EName, string Symbol, int DecimalPlaces,
            bool IsActive, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Symbol", SqlDbType.NVarChar,-1) { Value = Symbol },
                    new SqlParameter("@DecimalPlaces", SqlDbType.Int) { Value = DecimalPlaces },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },

                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                int A = clsSQL.ExecuteNonQueryStatement(@"
update tbl_UOM set
AName=@AName,
EName=@EName,
Symbol=@Symbol,
DecimalPlaces=@DecimalPlaces,
IsActive=@IsActive,
ModificationUserId=@ModificationUserId,
ModificationDate=@ModificationDate
where Id=@Id
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
