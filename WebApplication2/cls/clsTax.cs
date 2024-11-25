using System;
using System.Data;
using System.Data.SqlClient;
namespace WebApplication2.cls
{
    public class clsTax
    {




        public DataTable SelectTaxByID(int Id, string AName, string EName, int CompanyID, 
            int IsSalesSpecialTax, int IsSalesTax, int IsPurchaseTax, int IsSpecialPurchaseTax)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@IsSalesSpecialTax", SqlDbType.Int) { Value = IsSalesSpecialTax },
                        new SqlParameter("@IsSalesTax", SqlDbType.Int) { Value = IsSalesTax },
                                new SqlParameter("@IsPurchaseTax", SqlDbType.Int) { Value = IsPurchaseTax },
                                        new SqlParameter("@IsSpecialPurchaseTax", SqlDbType.Int) { Value = IsSpecialPurchaseTax },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_Tax where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
 and (IsSalesSpecialTax=@IsSalesSpecialTax or @IsSalesSpecialTax =-1)
and (IsSalesTax =@IsSalesTax or @IsSalesTax=-1) 
and (IsPurchaseTax =@IsPurchaseTax or @IsPurchaseTax=-1) 
 and (IsSpecialPurchaseTax =@IsSpecialPurchaseTax or @IsSpecialPurchaseTax=-1) 
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteTaxByID(int Id,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Tax where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertTax(string AName, string EName, decimal value, bool IsSalesTax, bool IsPurchaseTax, bool IsSalesSpecialTax, bool IsSpecialPurchaseTax, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@value", SqlDbType.Decimal) { Value = value },


                     new SqlParameter("@IsSalesTax", SqlDbType.Bit) { Value = IsSalesTax },
                     new SqlParameter("@IsPurchaseTax", SqlDbType.Bit) { Value = IsPurchaseTax },
                     new SqlParameter("@IsSalesSpecialTax", SqlDbType.Bit) { Value = IsSalesSpecialTax },
                     new SqlParameter("@IsSpecialPurchaseTax", SqlDbType.Bit) { Value = IsSpecialPurchaseTax },

                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_Tax(AName,EName,value,IsSalesTax,IsPurchaseTax,IsSalesSpecialTax,IsSpecialPurchaseTax,CompanyID,CreationUserId,CreationDate)
                    OUTPUT INSERTED.ID values(@AName,@EName,@value,@IsSalesTax,@IsPurchaseTax,@IsSalesSpecialTax,@IsSpecialPurchaseTax,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();

                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateTax(int ID, string AName, string EName, decimal value, bool IsSalesTax, bool IsPurchaseTax, bool IsSalesSpecialTax, bool IsSpecialPurchaseTax, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@value", SqlDbType.Decimal) { Value = value },


                     new SqlParameter("@IsSalesTax", SqlDbType.Bit) { Value = IsSalesTax },
                     new SqlParameter("@IsPurchaseTax", SqlDbType.Bit) { Value = IsPurchaseTax },
                     new SqlParameter("@IsSalesSpecialTax", SqlDbType.Bit) { Value = IsSalesSpecialTax },
                     new SqlParameter("@IsSpecialPurchaseTax", SqlDbType.Bit) { Value = IsSpecialPurchaseTax },

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Tax set 
                       AName=@AName,
                       EName=@EName,
value=@value,
IsSalesTax=@IsSalesTax,
IsPurchaseTax=@IsPurchaseTax,
IsSalesSpecialTax=@IsSalesSpecialTax,
IsSpecialPurchaseTax=@IsSpecialPurchaseTax,

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
