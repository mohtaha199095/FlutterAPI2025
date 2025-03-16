using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net;

namespace WebApplication2.cls
{
    public class clsCompany
    {
        public DataTable SelectCompany(int Id, string AName, string EName, string Tel1,int CompanyID,string PartOfTheName)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
           new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value =Simulate.String( Tel1) },
           new SqlParameter("@PartOfTheName", SqlDbType.NVarChar,-1) { Value =Simulate.String( PartOfTheName )},
           
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_Company where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' ) and (Tel1=@Tel1 or @Tel1='' ) 
    AND (AName LIKE N'%' + @PartOfTheName + '%' OR @PartOfTheName = '')

                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteCompanyByID(int Id, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                }; clsSQL clsSQL = new clsSQL();

                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Company where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertCompany(string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, byte[] Logo, string TradeName,string DataBaseName,string conString)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                        new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },
                        new SqlParameter("@ContactPerson", SqlDbType.NVarChar,-1) { Value = ContactPerson },
                        new SqlParameter("@ContactNumber", SqlDbType.NVarChar,-1) { Value = ContactNumber },
                        new SqlParameter("@TradeName", SqlDbType.NVarChar,-1) { Value = TradeName },
                     new SqlParameter("@Logo", SqlDbType.Binary) { Value = Logo!= null ? Logo: DBNull.Value },

                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@DataBaseName", SqlDbType.NVarChar,-1) { Value = DataBaseName },

                     
                };

                string a = @"insert into tbl_Company(AName,EName,Email,Address,Tel1,Tel2,ContactPerson,ContactNumber,TradeName,Logo,CreationDate,DataBaseName)
                        OUTPUT INSERTED.ID values(@AName,@EName,@Email,@Address,@Tel1,@Tel2,@ContactPerson,@ContactNumber,@TradeName,@Logo,@CreationDate,@DataBaseName)";
                clsSQL clsSQL = new clsSQL();

                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm,conString));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertCompanyWithID(int ID,string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, byte[] Logo, string TradeName, string DataBaseName, string conString)
        {
            try
            {
                SqlParameter[] prm =
                 {  new SqlParameter("@ID", SqlDbType.NVarChar,-1) { Value = ID },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                        new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },
                        new SqlParameter("@ContactPerson", SqlDbType.NVarChar,-1) { Value = ContactPerson },
                        new SqlParameter("@ContactNumber", SqlDbType.NVarChar,-1) { Value = ContactNumber },
                        new SqlParameter("@TradeName", SqlDbType.NVarChar,-1) { Value = TradeName },
                     new SqlParameter("@Logo", SqlDbType.Binary) { Value = Logo!= null ? Logo: DBNull.Value },

                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@DataBaseName", SqlDbType.NVarChar,-1) { Value = DataBaseName },


                };

                string a = @" SET IDENTITY_INSERT tbl_Company ON;

INSERT INTO tbl_Company (
    ID, AName, EName, Email, Address, Tel1, Tel2, ContactPerson, ContactNumber, TradeName, Logo, CreationDate, DataBaseName
)
OUTPUT INSERTED.ID 
VALUES (
    @ID, @AName, @EName, @Email, @Address, @Tel1, @Tel2, @ContactPerson, @ContactNumber, @TradeName, @Logo, @CreationDate, @DataBaseName
);

SET IDENTITY_INSERT tbl_Company OFF;";
                clsSQL clsSQL = new clsSQL();

                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, conString));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateCompanyDataBaseName(int ID, string DataBaseName) {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                  new SqlParameter("@DataBaseName", SqlDbType.NVarChar,-1) { Value = DataBaseName }, 
                }; clsSQL clsSQL = new clsSQL();

                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Company set 
                       DataBaseName=@DataBaseName 
                   where id =@id", clsSQL.CreateDataBaseConnectionString(ID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }
        }


      
        public int UpdateCompany(int ID, string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, byte[] Logo, string TradeName, int ModificationUserId,int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                  new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                        new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },
                        new SqlParameter("@ContactPerson", SqlDbType.NVarChar,-1) { Value = ContactPerson },
                        new SqlParameter("@ContactNumber", SqlDbType.NVarChar,-1) { Value = ContactNumber },
                        new SqlParameter("@TradeName", SqlDbType.NVarChar,-1) { Value = TradeName },
                     new SqlParameter("@Logo", SqlDbType.Binary) { Value = Logo!= null ? Logo: DBNull.Value },
                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                }; clsSQL clsSQL = new clsSQL();

                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Company set 
                       AName=@AName,
                       EName=@EName,
                       Email=@Email,
                       Address=@Address,
                       Tel1=@Tel1,
                       Tel2=@Tel2,
                       ContactPerson=@ContactPerson,
                       ContactNumber=@ContactNumber,
                       TradeName=@TradeName,
                       Logo=@Logo,
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
