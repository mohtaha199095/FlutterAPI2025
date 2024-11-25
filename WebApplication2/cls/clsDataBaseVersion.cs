using DocumentFormat.OpenXml.Math;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;
namespace WebApplication2.cls
{
    public class clsDataBaseVersion
    {



        public int InsertDataBaseVersion(decimal VersionNumber,int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { 
                    new SqlParameter("@VersionNumber", SqlDbType.Decimal) { Value = VersionNumber },
                
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                string a = @"insert into tbl_DataBaseVersion(VersionNumber,CreationDate)
                        OUTPUT INSERTED.ID values(@VersionNumber,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a,prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            }
            catch (Exception)
            {

                throw;
            }
        }

        public DataTable SelectDataBaseVersion(decimal VersionNumber,int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@VersionNumber", SqlDbType.Decimal) { Value = VersionNumber },
      

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_DataBaseVersion where (VersionNumber=@VersionNumber or @VersionNumber=0 )  ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                  
                return dt;
            }
            catch (Exception)
            {
                throw;
            }


        }
        public bool AddColumnToTable(int CompanyID,string tableName, string columnName, SQLColumnDataType columnType, int? varcharLength = null)
        {
			try
			{
			 string sqlColumnType;

            // Map the enum to SQL data types
            switch (columnType)
            {
                case SQLColumnDataType.Integer:
                    sqlColumnType = "INT";
                    break;
                case SQLColumnDataType.Decimal:
                    sqlColumnType = "DECIMAL(18,2)";
                    break;
                case SQLColumnDataType.VarChar:
                    sqlColumnType = varcharLength.HasValue ? $"VARCHAR({varcharLength.Value})" : "VARCHAR(MAX)";
                    break;
                case SQLColumnDataType.DateTime:
                    sqlColumnType = "DATETIME";
                    break;
                case SQLColumnDataType.Bit:
                    sqlColumnType = "BIT";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }
            clsSQL clssql = new clsSQL();
            string checkColumnQuery = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";

            int columnExists = (int)clssql.ExecuteScalar(checkColumnQuery, clssql.CreateDataBaseConnectionString(CompanyID));

            if (columnExists == 0)
            {
                // Create the SQL command to add the column
                string addColumnQuery = $"ALTER TABLE {tableName} ADD {columnName} {sqlColumnType}";
                DataTable dt = clssql.ExecuteQueryStatement(addColumnQuery, clssql.CreateDataBaseConnectionString(CompanyID));
					if (dt != null && dt.Rows.Count > 0)
					{

						return true;
					}
					else {
                        return false;
                    }
				}
				else{
                    return false;
                }
               
            }
            catch (Exception)
            {

                return false;
            }


        }
        public DataTable DeleteColumnFromTable(string tableName, string columnName, int CompanyID)
        {
            // Construct the SQL command to delete the column
            string query = $"ALTER TABLE {tableName} DROP COLUMN {columnName}";

            clsSQL clssql = new clsSQL();
            DataTable dt = clssql.ExecuteQueryStatement(query, clssql.CreateDataBaseConnectionString(CompanyID));
            return dt;
        }
	public	bool CreateDataBase(string DataBaseName) {


			try
			{
            
		
		

        string a = @"GO

CREATE DATABASE [" + DataBaseName + @"]
GO

USE ["+ DataBaseName + @"]


GO
/****** Object:  Table [dbo].[tbl_AccountMain]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_AccountMain](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
 CONSTRAINT [PK_tbl_AccountMain] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_AccountNature]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_AccountNature](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
 CONSTRAINT [PK_tbl_AccountNature] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Accounts]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Accounts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [int] NULL,
	[AccountNumber] [nvarchar](max) NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[ReportingTypeID] [int] NULL,
	[CompanyID] [int] NULL,
	[AccountNatureID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[ReportingTypeNodeID] [int] NULL,
 CONSTRAINT [PK_tbl_Accounts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_AccountSetting]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_AccountSetting](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AccountRefID] [int] NULL,
	[AccountID] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModofocationDate] [datetime] NULL,
	[Active] [bit] NULL,
 CONSTRAINT [PK_tbl_AccountSetting] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Banks]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Banks](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[AccountNumber] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[CreationUserId] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserId] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_Banks] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Branch]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Branch](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CreationUserId] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[CompanyID] [int] NULL,
 CONSTRAINT [PK_tbl_Branch] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_BusinessPartner]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_BusinessPartner](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CommercialName] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[Tel] [nvarchar](max) NULL,
	[Active] [bit] NULL,
	[Limit] [decimal](18, 3) NULL,
	[Email] [nvarchar](max) NULL,
	[Type] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[EmpCode] [nvarchar](max) NULL,
	[StreetName] [nvarchar](max) NULL,
	[HouseNumber] [nvarchar](max) NULL,
	[NationalNumber] [nvarchar](max) NULL,
	[PassportNumber] [nvarchar](max) NULL,
	[Nationality] [int] NULL,
	[IDNumber] [nvarchar](max) NULL,
	[TaxNumber] [nvarchar](max) NULL,
	[Job] [nvarchar](max) NULL,
 CONSTRAINT [PK_tbl_BusinessPartner] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_BusinessPartnerType]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_BusinessPartnerType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
 CONSTRAINT [PK_tbl_BusinessPartnerType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_CashDrawer]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_CashDrawer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[BranchID] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_CashDrawer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_CashVoucherDetails]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_CashVoucherDetails](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_CashVoucherDetails_Guid]  DEFAULT (newid()),
	[RowIndex] [int] NULL,
	[HeaderGuid] [uniqueidentifier] NULL,
	[IsUpper] [bit] NULL,
	[AccountID] [int] NULL,
	[SubAccountID] [int] NULL,
	[BranchID] [int] NULL,
	[CostCenterID] [int] NULL,
	[Debit] [decimal](18, 3) NULL,
	[Credit] [decimal](18, 3) NULL,
	[Total] [decimal](18, 3) NULL,
	[Note] [nvarchar](max) NULL,
	[VoucherType] [int] NULL,
	[CompanyID] [int] NULL,
 CONSTRAINT [PK_tbl_CashVoucherDetails] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_CashVoucherHeader]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_CashVoucherHeader](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_CashVoucherHeader_Guid]  DEFAULT (newid()),
	[VoucherDate] [datetime] NULL,
	[BranchID] [int] NULL,
	[CostCenterID] [int] NULL,
	[AccountID] [int] NULL,
	[CashID] [int] NULL,
	[Amount] [decimal](18, 3) NULL,
	[JVGuid] [uniqueidentifier] NULL,
	[Note] [nvarchar](max) NULL,
	[VoucherNo] [int] NULL,
	[ManualNo] [nvarchar](max) NULL,
	[VoucherType] [int] NULL,
	[RelatedInvoiceGuid] [uniqueidentifier] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[PaymentMethodTypeID] [int] NULL,
	[DueDate] [datetime] NULL,
	[ChequeNote] [nvarchar](max) NULL,
	[ChequeName] [nvarchar](max) NULL,
	[RelatedFinancingGuid] [uniqueidentifier] NULL,
 CONSTRAINT [PK_tbl_CashVoucherHeader] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Company]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Company](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[Tel1] [nvarchar](max) NULL,
	[Tel2] [nvarchar](max) NULL,
	[ContactPerson] [nvarchar](max) NULL,
	[ContactNumber] [nvarchar](max) NULL,
	[Logo] [image] NULL,
	[TradeName] [nvarchar](max) NULL,
	[CreationDate] [datetime] NULL,
	[ModificationDate] [datetime] NULL,
	[ModificationUserId] [int] NULL,
	[DataBaseName] [nvarchar](max) NULL,



 CONSTRAINT [PK_tbl_Company] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_CostCenter]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_CostCenter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CreationUserId] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[CompanyID] [int] NULL,
 CONSTRAINT [PK_tbl_CostCenter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Countries]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Countries](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_Countries] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_DataBaseVersion]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_DataBaseVersion](
	[ID] [int] NOT NULL,
	[VersionNumber] [decimal](18, 6) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_DataBaseVersion] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_employee]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_employee](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[UserName] [nvarchar](max) NULL,
	[Password] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[CreationUserId] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserId] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[IsSystemUser] [bit] NULL,
	[Signuture] [image] NULL,
 CONSTRAINT [PK_tbl_employee] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_FinancingDetails]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_FinancingDetails](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_FinancingDetails_Guid]  DEFAULT (newid()),
	[HeaderGuid] [uniqueidentifier] NULL,
	[RowIndex] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[TotalAmount] [decimal](18, 3) NULL,
	[DownPayment] [decimal](18, 3) NULL,
	[FinancingAmount] [decimal](18, 3) NULL,
	[PeriodInMonths] [int] NULL,
	[InterestRate] [decimal](18, 3) NULL,
	[InterestAmount] [decimal](18, 3) NULL,
	[TotalAmountWithInterest] [decimal](18, 3) NULL,
	[FirstInstallmentDate] [datetime] NULL,
	[InstallmentAmount] [decimal](18, 3) NULL,
	[JVGuid] [uniqueidentifier] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[CompanyID] [int] NULL,
	[SerialNumber] [nvarchar](max) NULL,
	[PriceBeforeTax] [decimal](18, 3) NULL,
	[TaxID] [int] NULL,
	[TaxAmount] [decimal](18, 3) NULL,
 CONSTRAINT [PK_tbl_FinancingDetails] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_FinancingHeader]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_FinancingHeader](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_FinancingHeader_Guid]  DEFAULT (newid()),
	[VoucherDate] [datetime] NULL,
	[BranchID] [int] NULL,
	[BankCostCenterID] [int] NULL,
	[CostCenterID] [int] NULL,
	[VoucherNumber] [int] NULL,
	[BusinessPartnerID] [int] NULL,
	[Note] [nvarchar](max) NULL,
	[TotalAmount] [decimal](18, 3) NULL,
	[DownPayment] [decimal](18, 3) NULL,
	[NetAmount] [decimal](18, 3) NULL,
	[Grantor] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[CompanyID] [int] NULL,
	[LoanType] [int] NULL,
	[JVGuid] [uniqueidentifier] NULL,
	[IntrestRate] [decimal](18, 3) NULL,
	[IsAmountReturned] [bit] NULL,
	[MonthsCount] [int] NULL,
	[PaymentAccountID] [int] NULL,
	[PaymentSubAccountID] [int] NULL,
	[VendorID] [int] NULL,
	[IsShowInMonthlyReports] [bit] NULL,
	[SignutureGuid1] [uniqueidentifier] NULL,
	[SignutureGuid2] [uniqueidentifier] NULL,
	[SignutureGuid3] [uniqueidentifier] NULL,
	[SignutureGuid4] [uniqueidentifier] NULL,
	[SignutureGuid5] [uniqueidentifier] NULL,
	[SignutureGuid6] [uniqueidentifier] NULL,
	[SignutureGuid7] [uniqueidentifier] NULL,
	[SalesManID] [int] NULL,
 CONSTRAINT [PK_tbl_FinancingHeader] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Forms]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Forms](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FrmName] [nvarchar](max) NULL,
	[ParentID] [int] NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[IsAccess] [bit] NULL,
	[IsSearch] [bit] NULL,
	[IsAdd] [bit] NULL,
	[IsEdit] [bit] NULL,
	[IsDelete] [bit] NULL,
	[IsPrint] [bit] NULL,
 CONSTRAINT [PK_tbl_Forms] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_InvoiceDetails]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_InvoiceDetails](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_InvoiceDetails_Guid]  DEFAULT (newid()),
	[HeaderGuid] [uniqueidentifier] NULL,
	[ItemGuid] [uniqueidentifier] NULL,
	[RowIndex] [int] NULL,
	[ItemName] [nvarchar](max) NULL,
	[Qty] [decimal](18, 3) NULL,
	[PriceBeforeTax] [decimal](18, 3) NULL,
	[DiscountBeforeTaxAmountPcs] [decimal](18, 3) NULL,
	[DiscountBeforeTaxAmountAll] [decimal](18, 3) NULL,
	[TaxID] [int] NULL,
	[TaxPercentage] [decimal](18, 3) NULL,
	[TaxAmount] [decimal](18, 3) NULL,
	[SpecialTaxID] [int] NULL,
	[SpecialTaxPercentage] [decimal](18, 3) NULL,
	[SpecialTaxAmount] [decimal](18, 3) NULL,
	[PriceAfterTaxPcs] [decimal](18, 3) NULL,
	[DiscountAfterTaxAmountPcs] [decimal](18, 3) NULL,
	[DiscountAfterTaxAmountAll] [decimal](18, 3) NULL,
	[HeaderDiscountAfterTaxAmount] [decimal](18, 3) NULL,
	[HeaderDiscountTax] [decimal](18, 3) NULL,
	[FreeQty] [decimal](18, 3) NULL,
	[TotalQTY] [decimal](18, 3) NULL,
	[ServiceBeforeTax] [decimal](18, 3) NULL,
	[ServiceTaxAmount] [decimal](18, 3) NULL,
	[ServiceAfterTax] [decimal](18, 3) NULL,
	[TotalLine] [decimal](18, 3) NULL,
	[BranchID] [int] NULL,
	[StoreID] [int] NULL,
	[CompanyID] [int] NULL,
	[InvoiceTypeID] [int] NULL,
	[IsCounted] [bit] NULL,
	[InvoiceDate] [datetime] NULL,
	[BusinessPartnerID] [int] NULL,
	[ItemBatchsGuid] [uniqueidentifier] NULL,
	[CreationDate] [datetime] NULL,
	[CreationUserID] [int] NULL,
 CONSTRAINT [PK_tbl_InvoiceDetails] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_InvoiceHeader]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_InvoiceHeader](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_InvoiceHeader_guid]  DEFAULT (newid()),
	[InvoiceNo] [int] NULL,
	[InvoiceDate] [datetime] NULL,
	[PaymentMethodID] [int] NULL,
	[BranchID] [int] NULL,
	[Note] [nvarchar](max) NULL,
	[BusinessPartnerID] [int] NULL,
	[StoreID] [int] NULL,
	[InvoiceTypeID] [int] NULL,
	[IsCounted] [bit] NULL,
	[JVGuid] [uniqueidentifier] NULL,
	[TotalTax] [decimal](18, 3) NULL,
	[HeaderDiscount] [decimal](18, 3) NULL,
	[TotalDiscount] [decimal](18, 3) NULL,
	[TotalInvoice] [decimal](18, 3) NULL,
	[RefNo] [nvarchar](max) NULL,
	[RelatedInvoiceGuid] [uniqueidentifier] NULL,
	[CashID] [int] NULL,
	[BankID] [int] NULL,
	[POSDayGuid] [uniqueidentifier] NULL,
	[POSSessionGuid] [uniqueidentifier] NULL,
	[AccountID] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_InvoiceHeader] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_ItemReadType]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_ItemReadType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_ItemReadType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Items]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Items](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_Items_guid]  DEFAULT (newid()),
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[SalesPriceBeforeTax] [decimal](18, 3) NULL,
	[SalesPriceAfterTax] [decimal](18, 3) NULL,
	[CategoryID] [int] NULL,
	[SalesTaxID] [int] NULL,
	[SpecialSalesTaxID] [int] NULL,
	[PurchaseTaxID] [int] NULL,
	[SpecialPurchaseTaxID] [int] NULL,
	[Barcode] [nvarchar](max) NULL,
	[ReadType] [int] NULL,
	[OriginID] [int] NULL,
	[MinimumLimit] [decimal](18, 3) NULL,
	[Picture] [image] NULL,
	[IsActive] [bit] NULL,
	[IsPOS] [bit] NULL,
	[BoxTypeID] [int] NULL,
	[IsStockItem] [bit] NULL,
	[POSOrder] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_Items] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_ItemsBoxType]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_ItemsBoxType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[Qty] [decimal](18, 3) NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_ItemsBoxType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_ItemsCategory]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_ItemsCategory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CreationUserId] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[CompanyID] [int] NULL,
 CONSTRAINT [PK_tbl_ItemsCategory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_JournalVoucherDetails]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_JournalVoucherDetails](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_JournalVoucherDetails_Guid]  DEFAULT (newid()),
	[ParentGuid] [uniqueidentifier] NULL,
	[RowIndex] [int] NULL,
	[AccountID] [int] NULL,
	[SubAccountID] [int] NULL,
	[Debit] [decimal](18, 3) NULL,
	[Credit] [decimal](18, 3) NULL,
	[Total] [decimal](18, 3) NULL,
	[BranchID] [int] NULL,
	[CostCenterID] [int] NULL,
	[DueDate] [datetime] NULL,
	[Note] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[RelatedDetailsGuid] [uniqueidentifier] NULL,
 CONSTRAINT [PK_tbl_JournalVoucherDetails] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_JournalVoucherHeader]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_JournalVoucherHeader](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_JournalVoucherHeader_guid]  DEFAULT (newid()),
	[VoucherDate] [datetime] NULL,
	[BranchID] [int] NULL,
	[CostCenterID] [int] NULL,
	[Notes] [nvarchar](max) NULL,
	[JVNumber] [nvarchar](max) NULL,
	[JVTypeID] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[RelatedFinancingHeaderGuid] [uniqueidentifier] NULL,
	[RelatedLoanTypeID] [int] NULL,
 CONSTRAINT [PK_tbl_JournalVoucherHeader] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_JournalVoucherTypes]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_JournalVoucherTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[QTYFactor] [int] NULL,
 CONSTRAINT [PK_tbl_JournalVoucherTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_LoanTypes]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_LoanTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[Code] [nvarchar](max) NULL,
	[IsReturned] [bit] NULL,
	[PaymentAccountID] [int] NULL,
	[ReceivableAccountID] [int] NULL,
	[ProfitAccount] [int] NULL,
	[DefaultAmount] [decimal](18, 3) NULL,
	[DevidedMonths] [int] NULL,
	[IsActive] [bit] NULL,
	[InterestRate] [decimal](18, 3) NULL,
	[MainTypeID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[CompanyID] [int] NULL,
	[IsStopBP] [bit] NULL,
	[IsShowInMonthlyReports] [bit] NULL,
 CONSTRAINT [PK_tbl_LoadType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_PaymentMethod]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_PaymentMethod](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[BranchID] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[GLAccountID] [int] NULL,
	[GLSubAccountID] [int] NULL,
	[IsCash] [bit] NULL,
	[IsBank] [bit] NULL,
	[IsDebit] [bit] NULL,
 CONSTRAINT [PK_tbl_PaymentMethod] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tbl_POSDay]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tbl_POSDay](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Tbl_POSDay_Guid]  DEFAULT (newid()),
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[POSDate] [datetime] NULL,
	[Status] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_Tbl_POSDay] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_POSSessions]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_POSSessions](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_POSSessions_Guid]  DEFAULT (newid()),
	[POSDayGuid] [uniqueidentifier] NULL,
	[SessionTypeID] [int] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[CashDrawerID] [int] NULL,
	[Status] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_POSSessions] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_POSSessionsType]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_POSSessionsType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_POSSesstionType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_POSSetting]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_POSSetting](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CashDrawerID] [int] NULL,
	[POSSettingID] [int] NULL,
	[Value] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[CreationUserId] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserId] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_POSSetting] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_POSSettingMain]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_POSSettingMain](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
 CONSTRAINT [PK_tbl_POSSettingMain] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Reconciliation]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Reconciliation](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_Reconciliation_Guid]  DEFAULT (newid()),
	[VoucherNumber] [int] NULL,
	[JVDetailsGuid] [uniqueidentifier] NULL,
	[Amount] [decimal](18, 3) NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModulleID] [int] NULL,
	[TransactionGuid] [uniqueidentifier] NULL,
 CONSTRAINT [PK_tbl_Reconciliation] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_ReportingType]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_ReportingType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[ParentID] [int] NULL,
	[CompanyID] [int] NULL,
 CONSTRAINT [PK_tbl_ReportingType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_ReportingTypeNodes]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_ReportingTypeNodes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[ReportingTypeID] [int] NULL,
	[ParentID] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_ReportingTypeNodes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Signuture]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Signuture](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_Signuture_Guid]  DEFAULT (newid()),
	[Signuture] [image] NULL,
	[SourceGuid] [uniqueidentifier] NULL,
	[VoucherType] [int] NULL,
	[IsOpen] [bit] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[Terms] [nvarchar](max) NULL,
 CONSTRAINT [PK_tbl_Signuture] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Store]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Store](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[BranchID] [int] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_Store] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Subscriptions]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Subscriptions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BusinessPartnerID] [int] NULL,
	[SubscriptionTypeID] [int] NULL,
	[TransactionDate] [datetime] NULL,
	[TransactionStatusID] [int] NULL,
	[Amount] [decimal](18, 3) NULL,
	[CreationDate] [datetime] NULL,
	[CreationUserID] [int] NULL,
	[CompanyID] [int] NULL,
 CONSTRAINT [PK_tbl_] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_SubscriptionsStatus]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_SubscriptionsStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[IsNew] [bit] NULL,
	[IsStop] [bit] NULL,
	[CompanyID] [int] NULL,
	[Code] [nvarchar](max) NULL,
 CONSTRAINT [PK_tbl_SubscriptionsStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_SubscriptionsTypes]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_SubscriptionsTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[CompanyID] [int] NULL,
	[Code] [nvarchar](max) NULL,
 CONSTRAINT [PK_tbl_SubscriptionsTypes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Tax]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_Tax](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AName] [nvarchar](max) NULL,
	[EName] [nvarchar](max) NULL,
	[Value] [decimal](18, 4) NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
	[ModificationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL,
	[IsSalesTax] [bit] NULL,
	[IsPurchaseTax] [bit] NULL,
	[IsSalesSpecialTax] [bit] NULL,
	[IsSpecialPurchaseTax] [bit] NULL,
 CONSTRAINT [PK_tbl_Tax] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_UserAuthorization]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_UserAuthorization](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_UserAuthorization_Guid]  DEFAULT (newid()),
	[PageID] [int] NULL,
	[UserID] [int] NULL,
	[IsAccess] [bit] NULL,
	[IsSearch] [bit] NULL,
	[IsAdd] [bit] NULL,
	[IsEdit] [bit] NULL,
	[IsDelete] [bit] NULL,
	[IsPrint] [bit] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_UserAuthorization_1] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_UserAuthorizationModels]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_UserAuthorizationModels](
	[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_tbl_UserAuthorizationBranch_guid]  DEFAULT (newid()),
	[TypeID] [int] NULL,
	[ModelID] [int] NULL,
	[UserID] [int] NULL,
	[IsAccess] [bit] NULL,
	[IsDefault] [bit] NULL,
	[CompanyID] [int] NULL,
	[CreationUserID] [int] NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_tbl_UserAuthorizationBranch] PRIMARY KEY CLUSTERED 
(
	[Guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetAccountInParent]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create function [dbo].[GetAccountInParent](@ParentID int)
 RETURNS  TABLE  
 as 
 RETURN

 WITH ret AS(
        SELECT  id
        FROM    tbl_Accounts
        WHERE   ID = @ParentID
        UNION ALL
        SELECT  t.id
        FROM    tbl_Accounts t INNER JOIN
                ret r ON t.ParentID = r.ID
)
 
    SELECT  id
FROM    ret 


GO
/****** Object:  UserDefinedFunction [dbo].[GetChildAccounts]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create FUNCTION [dbo].[GetChildAccounts]
(
   @ParentID      int
)
RETURNS TABLE
AS
  RETURN (with tbParent as
(
   select * from tbl_Accounts where id= @ParentID 
   union all
   select tbl_Accounts.* from tbl_Accounts  join tbParent  on tbl_Accounts.ParentID=tbParent.id
)
 SELECT * FROM  tbParent
  );

GO
/****** Object:  UserDefinedFunction [dbo].[GetDueUnReconciledAmount]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[GetDueUnReconciledAmount]
(

 @date1 date ,
@date2 date ,
@compnayid int,
@BusinessPartnerID int 
   
)
RETURNS TABLE
AS
  RETURN ( select  Total -
isnull((select Amount from tbl_Reconciliation where JVDetailsGuid = tbl_JournalVoucherDetails.Guid),0) as UnReconciled
,Total from tbl_JournalVoucherDetails 
where 

tbl_JournalVoucherDetails.ParentGuid in (
select tbl_FinancingDetails.JVGuid 
from tbl_FinancingDetails inner join tbl_FinancingHeader on tbl_FinancingHeader.Guid 
= tbl_FinancingDetails.HeaderGuid 
where tbl_FinancingHeader.LoanType = 1

)
--=======================
and DueDate between @date1 and @date2 
and CompanyID =@compnayid
 and tbl_JournalVoucherDetails.Debit > 0
 and SubAccountID =@BusinessPartnerID
  );



GO
/****** Object:  UserDefinedFunction [dbo].[GetSumDueUnReconciledAmountByFinanceGuid]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--declare @Account int=826,@dueDate date ='2024-03-31' ,@SubAccountID int = 13 ,@CompanyID int =1022, @JournalVoucherHeaderGuid uniqueidentifier='00000000-0000-0000-0000-000000000000'
CREATE FUNCTION [dbo].[GetSumDueUnReconciledAmountByFinanceGuid]
(

@Account int ,
@dueDate date,
@SubAccountID int,
@CompanyID int ,
@JournalVoucherHeaderGuid uniqueidentifier ,
@RelatedLoanTypeID int 
   
)
RETURNS TABLE
AS
  RETURN( select sum(tt)as total from (
select tbl_JournalVoucherDetails.Total  as tt
 from tbl_JournalVoucherDetails
 --left join tbl_Reconciliation on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
 left join tbl_JournalVoucherHeader on  tbl_JournalVoucherDetails.ParentGuid = tbl_JournalVoucherHeader.Guid
  left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
  left join tbl_LoanTypes on tbl_FinancingHeader.LoanType = tbl_LoanTypes.ID
  left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
   left join tbl_FinancingHeader nH on tbl_FinancingDetails.HeaderGuid = nH.Guid
 where accountid = @Account 
 and (tbl_FinancingHeader.IsShowInMonthlyReports =1   or nH.IsShowInMonthlyReports =1 or tbl_JournalVoucherHeader.JVTypeID =15)
 and (ParentGuid	= @JournalVoucherHeaderGuid or @JournalVoucherHeaderGuid='00000000-0000-0000-0000-000000000000')
  and (tbl_JournalVoucherDetails.SubAccountID = @SubAccountID or @SubAccountID = 0)
 and (DueDate   <=  @dueDate )
 --and (tbl_FinancingHeader.LoanType= 1 or tbl_LoanTypes.IsShowInMonthlyReports = 1)
 and (tbl_JournalVoucherDetails.CompanyID = @CompanyID or @CompanyID =0)
 and ((@RelatedLoanTypeID =-1 and  tbl_JournalVoucherHeader.RelatedLoanTypeID>1) 
 or  tbl_JournalVoucherHeader.RelatedLoanTypeID = @RelatedLoanTypeID 
 or @RelatedLoanTypeID = 0)

 union all 
 select tbl_Reconciliation.Amount *-1   as tt
 from tbl_JournalVoucherDetails
 left join tbl_Reconciliation on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
 left join tbl_JournalVoucherHeader on  tbl_JournalVoucherDetails.ParentGuid = tbl_JournalVoucherHeader.Guid
 left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
  left join tbl_LoanTypes on tbl_FinancingHeader.LoanType = tbl_LoanTypes.ID
  left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
   left join tbl_FinancingHeader nH on tbl_FinancingDetails.HeaderGuid = nH.Guid
 where accountid = @Account 
 and (tbl_FinancingHeader.IsShowInMonthlyReports =1  or nH.IsShowInMonthlyReports =1  or tbl_JournalVoucherHeader.JVTypeID =15)
 --and (tbl_FinancingHeader.LoanType= 1 or tbl_LoanTypes.IsShowInMonthlyReports = 1)
 and (ParentGuid	= @JournalVoucherHeaderGuid or @JournalVoucherHeaderGuid='00000000-0000-0000-0000-000000000000')
  and (tbl_JournalVoucherDetails.SubAccountID = @SubAccountID or @SubAccountID = 0)
 and (DueDate   <=  @dueDate )
 and (tbl_JournalVoucherDetails.CompanyID = @CompanyID or @CompanyID =0)
 and ((@RelatedLoanTypeID =-1 and  tbl_JournalVoucherHeader.RelatedLoanTypeID>1) 
 or  tbl_JournalVoucherHeader.RelatedLoanTypeID = @RelatedLoanTypeID 
 or @RelatedLoanTypeID = 0)
 )as q);


GO
/****** Object:  UserDefinedFunction [dbo].[SplitInts]    Script Date: 10/16/2024 9:07:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[SplitInts]
(
   @List      VARCHAR(MAX),
   @Delimiter VARCHAR(255)
)
RETURNS TABLE
AS
  RETURN ( SELECT Item = CONVERT(INT, Item) FROM
      ( SELECT Item = x.i.value('(./text())[1]', 'varchar(max)')
        FROM ( SELECT [XML] = CONVERT(XML, '<i>'
        + REPLACE(@List, @Delimiter, '</i><i>') + '</i>').query('.')
          ) AS a CROSS APPLY [XML].nodes('i') AS x(i) ) AS y
      WHERE Item IS NOT NULL
  );


GO
";
                string[] sqlCommands = a.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
				clsSQL clsSQL = new clsSQL();
				
                using (SqlConnection connection = new SqlConnection(clsSQL.MainDataBaseconString))
                {
                    connection.Open();

                    foreach (string commandText in sqlCommands)
                    {
                        using (SqlCommand command = new SqlCommand(commandText, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    Console.WriteLine("Database and tables created successfully.");
                }

              
					//InsertDataBaseVersion(Simulate.decimal_("1.0") );

                    return true;
               
              
            }
            catch (Exception)
            {
                return false;

            }
        }
}
}