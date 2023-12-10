using DocumentFormat.OpenXml.Wordprocessing;
using SixLabors.ImageSharp;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Emit;

namespace WebApplication2.cls
{
    public class clsBusinessPartner
    {
        public DataTable SelectBusinessPartner(int Id, int Type, string AName, string EName, int Active, int CompanyID,SqlTransaction trn=null)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                 new SqlParameter("@Type", SqlDbType.Int) { Value = Type },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
        new SqlParameter("@Active", SqlDbType.Int) { Value = Active },
                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_BusinessPartner where (id=@Id or @Id=0 ) and  
                  (Type=@Type or @Type=0) and   (AName=@AName or @AName='' )
and (EName=@EName or @EName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
and (active =@Active or @Active=-1)
                     ", prm,trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteBusinessPartnerByID(int Id)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BusinessPartner where (id=@Id  )", prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertBusinessPartner(string AName, string EName, string CommercialName, string Address, string Tel, bool Active, double Limit,
            string Email, int Type, int CompanyID, int CreationUserId, string EmpCode, string StreetName, string HouseNumber, string NationalNumber, string PassportNumber, int Nationality, string IDNumber,string TaxNumber)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@CommercialName", SqlDbType.NVarChar,-1) { Value = CommercialName },
                      new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                        new SqlParameter("@Tel", SqlDbType.NVarChar,-1) { Value = Tel },
                          new SqlParameter("@Active", SqlDbType.Bit) { Value = Active },
                          new SqlParameter("@Limit", SqlDbType.Decimal) { Value = Limit },
                                 new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Type", SqlDbType.Int) { Value = Type },
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                                           new SqlParameter("@EmpCode", SqlDbType.NVarChar,-1) { Value = EmpCode },
 new SqlParameter("@StreetName", SqlDbType.NVarChar,-1) { Value = StreetName },
  new SqlParameter("@HouseNumber", SqlDbType.NVarChar,-1) { Value = HouseNumber },
    new SqlParameter("@NationalNumber", SqlDbType.NVarChar,-1) { Value = NationalNumber },
      new SqlParameter("@PassportNumber", SqlDbType.NVarChar,-1) { Value = PassportNumber },
          new SqlParameter("@Nationality", SqlDbType.Int) { Value = Nationality },
              new SqlParameter("@IDNumber", SqlDbType.NVarChar,-1) { Value = IDNumber },
               new SqlParameter("@TaxNumber", SqlDbType.NVarChar,-1) { Value = TaxNumber },
                };

                string a = @"insert into tbl_BusinessPartner(AName,EName, CommercialName,  Address, Tel ,Active ,Limit ,Email ,Type,CompanyID,CreationUserId,CreationDate,EmpCode,StreetName,HouseNumber,NationalNumber,PassportNumber,Nationality,IDNumber,TaxNumber)
                        OUTPUT INSERTED.ID values         (@AName,@EName,@CommercialName,@Address,@Tel,@Active,@Limit,@Email,@Type,@CompanyID,@CreationUserId,@CreationDate,@EmpCode,@StreetName,@HouseNumber,@NationalNumber,@PassportNumber,@Nationality,@IDNumber,@TaxNumber)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateBusinessPartner(int ID, string AName, string EName, string CommercialName, string Address, string Tel, bool Active, double Limit,
            string Email, int Type, int ModificationUserId, string EmpCode, string StreetName, string HouseNumber, string NationalNumber, string PassportNumber, int Nationality, string IDNumber,string TaxNumber)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                  new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@CommercialName", SqlDbType.NVarChar,-1) { Value = CommercialName },
                      new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                        new SqlParameter("@Tel", SqlDbType.NVarChar,-1) { Value = Tel },
                          new SqlParameter("@Active", SqlDbType.Bit) { Value = Active },
                          new SqlParameter("@Limit", SqlDbType.Decimal) { Value = Limit },
                                 new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Type", SqlDbType.Int) { Value = Type },

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                                           new SqlParameter("@EmpCode", SqlDbType.NVarChar,-1) { Value = EmpCode },
 new SqlParameter("@StreetName", SqlDbType.NVarChar,-1) { Value = StreetName },
  new SqlParameter("@HouseNumber", SqlDbType.NVarChar,-1) { Value = HouseNumber },
    new SqlParameter("@NationalNumber", SqlDbType.NVarChar,-1) { Value = NationalNumber },
      new SqlParameter("@PassportNumber", SqlDbType.NVarChar,-1) { Value = PassportNumber },
          new SqlParameter("@Nationality", SqlDbType.Int) { Value = Nationality },
              new SqlParameter("@IDNumber", SqlDbType.NVarChar,-1) { Value = IDNumber },
                             new SqlParameter("@TaxNumber", SqlDbType.NVarChar,-1) { Value = TaxNumber },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_BusinessPartner set 
                       AName=@AName,
                       EName=@EName,
CommercialName =@CommercialName,
Address=@Address,
Tel=@Tel,
Active=@Active,
Limit=@Limit,
Email=@Email,
Type=@Type,

                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId,
EmpCode=@EmpCode,
StreetName=@StreetName,
HouseNumber=@HouseNumber,
NationalNumber=@NationalNumber,
PassportNumber=@PassportNumber,
Nationality=@Nationality,
IDNumber=@IDNumber,
TaxNumber=@TaxNumber
                   where id =@id", prm);
 
                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
