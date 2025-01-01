using System.Data.SqlClient;
using System.Data;
using System;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace WebApplication2.cls
{
    public class clsForms
    { 
            public DataTable SelectForms(int ID, string FrmName, string AName, string EName, int ParentID, bool? IsAccess, bool? IsSearch, int CompanyID)
            {
                try
                {
                    SqlParameter[] prm =
                    {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@FrmName", SqlDbType.NVarChar, -1) { Value = FrmName },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                    new SqlParameter("@IsAccess", SqlDbType.Bit) { Value = IsAccess ?? (object)DBNull.Value },
                    new SqlParameter("@IsSearch", SqlDbType.Bit) { Value = IsSearch ?? (object)DBNull.Value }
                };

                    clsSQL clsSQL = new clsSQL();
                    DataTable dt = clsSQL.ExecuteQueryStatement(
                        @"SELECT * FROM tbl_Forms WHERE 
                      (ID = @ID OR @ID = 0) AND 
                      (FrmName = @FrmName OR @FrmName = '') AND 
                      (AName = @AName OR @AName = '') AND 
                      (EName = @EName OR @EName = '') AND 
                      (ParentID = @ParentID OR @ParentID = 0) AND 
                      (IsAccess = @IsAccess OR @IsAccess IS NULL) AND 
                      (IsSearch = @IsSearch OR @IsSearch IS NULL)",
                        clsSQL.CreateDataBaseConnectionString(CompanyID),
                        prm);

                    return dt;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            public bool DeleteFormByID(int ID, int CompanyID)
            {
                try
                {
                    clsSQL clsSQL = new clsSQL();

                    SqlParameter[] prm =
                    {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                    clsSQL.ExecuteNonQueryStatement("DELETE FROM tbl_Forms WHERE ID = @ID", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            public int InsertForm(int ID, string FrmName, string AName, string EName, int ParentID, bool IsAccess, bool IsSearch, bool IsAdd, bool IsEdit, bool IsDelete, bool IsPrint ,int CompanyID)
            {
                try
            {
                SqlParameter[] prm =
                    {    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@FrmName", SqlDbType.NVarChar, -1) { Value = FrmName },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                    new SqlParameter("@IsAccess", SqlDbType.Bit) { Value = IsAccess },
                    new SqlParameter("@IsSearch", SqlDbType.Bit) { Value = IsSearch },
                    new SqlParameter("@IsAdd", SqlDbType.Bit) { Value = IsAdd },
                    new SqlParameter("@IsEdit", SqlDbType.Bit) { Value = IsEdit },
                    new SqlParameter("@IsDelete", SqlDbType.Bit) { Value = IsDelete },
                    new SqlParameter("@IsPrint", SqlDbType.Bit) { Value = IsPrint },
                    
                };

                    string query = @"    SET IDENTITY_INSERT tbl_Forms ON;
            INSERT INTO tbl_Forms 
            (ID, FrmName, AName, EName, ParentID, IsAccess, IsSearch, IsAdd, IsEdit, IsDelete, IsPrint) 
            OUTPUT INSERTED.ID 
            VALUES (@ID, @FrmName, @AName, @EName, @ParentID, @IsAccess, @IsSearch, @IsAdd, @IsEdit, @IsDelete, @IsPrint);
            SET IDENTITY_INSERT tbl_Forms OFF;";

                    clsSQL clsSQL = new clsSQL();
                    return Convert.ToInt32(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            public int UpdateForm(int ID, string FrmName, string AName, string EName, int ParentID, bool IsAccess, bool IsSearch, bool IsAdd, bool IsEdit, bool IsDelete, bool IsPrint,  int CompanyID)
            {
                try
                {
                    SqlParameter[] prm =
                    {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@FrmName", SqlDbType.NVarChar, -1) { Value = FrmName },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                    new SqlParameter("@IsAccess", SqlDbType.Bit) { Value = IsAccess },
                    new SqlParameter("@IsSearch", SqlDbType.Bit) { Value = IsSearch },
                    new SqlParameter("@IsAdd", SqlDbType.Bit) { Value = IsAdd },
                    new SqlParameter("@IsEdit", SqlDbType.Bit) { Value = IsEdit },
                    new SqlParameter("@IsDelete", SqlDbType.Bit) { Value = IsDelete },
                    new SqlParameter("@IsPrint", SqlDbType.Bit) { Value = IsPrint },
               
                };

                    string query = @"UPDATE tbl_Forms SET 
                                FrmName = @FrmName, 
                                AName = @AName, 
                                EName = @EName, 
                                ParentID = @ParentID, 
                                IsAccess = @IsAccess, 
                                IsSearch = @IsSearch, 
                                IsAdd = @IsAdd, 
                                IsEdit = @IsEdit, 
                                IsDelete = @IsDelete, 
                                IsPrint = @IsPrint 
                                WHERE ID = @ID";

                    clsSQL clsSQL = new clsSQL();
                    return clsSQL.ExecuteNonQueryStatement(query, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }

 