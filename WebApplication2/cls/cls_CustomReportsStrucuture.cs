using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Text;
using FastReport;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication2.cls
{
    public class cls_CustomReportsStrucuture
    {
        

            public DataTable SelectAvailableReport(string PageName,  int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@PageName", SqlDbType.NVarChar,-1) { Value = PageName },
     

        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select distinct ReportName
 from tbl_CustomReportsStrucuture
where          
(PageName=@PageName or @PageName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable DeleteReportByName(string PageName, string ReportName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@PageName", SqlDbType.NVarChar,-1) { Value = PageName },
                 
                 new SqlParameter("@ReportName", SqlDbType.NVarChar,-1) { Value = ReportName },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"delete  
 from tbl_CustomReportsStrucuture
where      (ReportName=@ReportName) and 
(PageName=@PageName   )   and (CompanyID=@CompanyID )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectCustomReportsStrucuture(  string PageName, string ReportName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@PageName", SqlDbType.NVarChar,-1) { Value = PageName },
      new SqlParameter("@ReportName", SqlDbType.NVarChar,-1) { Value = ReportName },
 
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };


                clsSQL clsSQL = new clsSQL();
                if (ReportName == "Default")
                {
                    DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_CustomReportsStrucuture
where          (ReportName= (select top 1 ReportName from tbl_CustomReportsStrucuture where (PageName=@PageName or @PageName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )  ) ) and
(PageName=@PageName or @PageName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                    return dt;

                }
                else {
                    DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_CustomReportsStrucuture
where          (ReportName=@ReportName or @ReportName='' ) and
(PageName=@PageName or @PageName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                    return dt;
                }
              
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteCustomReportsStrucuture(string PageName, string ReportName, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
              { new SqlParameter("@PageName", SqlDbType.NVarChar,-1) { Value = PageName },
      new SqlParameter("@ReportName", SqlDbType.NVarChar,-1) { Value = ReportName },

        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_CustomReportsStrucuture 
where    
(ReportName=@ReportName or @ReportName='' ) and
(PageName=@PageName or @PageName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )",
clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertCustomReportStructure(
            string pageName,
            string reportName,
            int rowIndex,
            string type,
            string parameter,
            string otherValue,
            string font,
            decimal fontSize,
            string fontWeight,
            bool withBorder,
            string horizontalAlignment,
            string verticalAlignment,
            string fontColor,
            string backColor,
            decimal height,
            decimal? width,
            int companyID,
            int creationUserID,int widgetIndex,
            SqlTransaction trn = null)
        {
            try
            {
                // Prepare parameters converting strings to byte arrays (for varbinary columns).
                SqlParameter[] prm =
                {
                    new SqlParameter("@PageName", SqlDbType.NVarChar, -1) { Value =Simulate.String(pageName) },
                    new SqlParameter("@ReportName", SqlDbType.NVarChar, -1) { Value = Simulate.String(reportName) },
                    new SqlParameter("@RowIndex", SqlDbType.Int) { Value = Simulate.Integer32(rowIndex) },
                    new SqlParameter("@Type", SqlDbType.NVarChar, -1) { Value = Simulate.String(type)    },
                    new SqlParameter("@Parameter", SqlDbType.NVarChar, -1) { Value = Simulate.String(parameter)  },
                    new SqlParameter("@OtherValue", SqlDbType.NVarChar, -1) { Value = Simulate.String(otherValue)   },
                    new SqlParameter("@Font", SqlDbType.NVarChar, -1) { Value = Simulate.String(font)    },
                    new SqlParameter("@FontSize", SqlDbType.Decimal) { Value = Simulate.decimal_(fontSize)   },
                    new SqlParameter("@FontWeight", SqlDbType.NVarChar, -1) { Value = Simulate.String(fontWeight)    },
                    new SqlParameter("@WithBoarder", SqlDbType.NVarChar, -1) { Value =Simulate.String(withBorder)      },
                    new SqlParameter("@HorizontalAlignment", SqlDbType.NVarChar, -1) { Value =Simulate.String(horizontalAlignment)   },
                    new SqlParameter("@VerticalAlignment", SqlDbType.NVarChar, -1) { Value = Simulate.String(verticalAlignment)     },
                    new SqlParameter("@FontColor", SqlDbType.NVarChar, -1) { Value = Simulate.String(fontColor)      },
                    new SqlParameter("@BackColor", SqlDbType.NVarChar, -1) { Value = Simulate.String(backColor)      },
                    new SqlParameter("@Height", SqlDbType.Decimal) { Value =Simulate.decimal_(height)   },
                    new SqlParameter("@Width", SqlDbType.Decimal) { Value =Simulate.decimal_(width)    },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value =Simulate.Integer32(companyID)      },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = Simulate.Integer32(creationUserID)       },
                      new SqlParameter("@WidgetIndex", SqlDbType.Int) { Value = Simulate.Integer32(widgetIndex)       }


                    
                };

                // SQL query with OUTPUT clause to return the inserted ID.
                string sql = @"
                INSERT INTO tbl_CustomReportsStrucuture
                (
                    PageName,
                    ReportName,
                    RowIndex,
                    Type,
                    Parameter,
                    OtherValue,
                    Font,
                    FontSize,
                    FontWeight,
                    WithBoarder,
                    HorizontalAlignment,
                    VerticalAlignment,
                    FontColor,
                    BackColor,
                    Height,
                    Width,
                    CompanyID,
                    CreationUserID,
WidgetIndex
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @PageName,
                    @ReportName,
                    @RowIndex,
                    @Type,
                    @Parameter,
                    @OtherValue,
                    @Font,
                    @FontSize,
                    @FontWeight,
                    @WithBoarder,
                    @HorizontalAlignment,
                    @VerticalAlignment,
                    @FontColor,
                    @BackColor,
                    @Height,
                    @Width,
                    @CompanyID,
                    @CreationUserID,
@WidgetIndex
                )";

                clsSQL clsSQL = new clsSQL();
                int insertedId;
                if (trn == null)
                {
                    insertedId = Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyID)));
                }
                else
                {
                    insertedId = Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyID), trn));
                }
                return insertedId;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
    
 