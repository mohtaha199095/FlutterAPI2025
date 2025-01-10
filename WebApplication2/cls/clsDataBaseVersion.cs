using DocumentFormat.OpenXml.Math;
using FastReport.Table;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;
namespace WebApplication2.cls
{
    public class clsDataBaseVersion
    {


		public void checkDatabaseUpdates(decimal versionNumber,int CompanyId) {
			try
            {
                clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                clsSQL ClsSQL= new clsSQL();
            if (versionNumber < Simulate.decimal_(1.2))
            {
                //cls.AddColumnToTable(CompanyId, "tbl_POSSetting", "IsCumulative", SQLColumnDataType.Bit);
                //cls.AddColumnToTable(CompanyId, "tbl_POSSetting", "DefaultPaymentMethodID", SQLColumnDataType.Integer);
                //cls.AddColumnToTable(CompanyId, "tbl_POSSetting", "PrinterName", SQLColumnDataType.VarChar);
                //cls.InsertDataBaseVersion(Simulate.decimal_(1.2), CompanyId);
        
                clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(17, "دفعات نقاط البيع", "POS Cash Payment", 0, CompanyId);
                clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(18, "مقبوضات نقاط البيع", "POS Cash Recivable", 0, CompanyId);
                InsertDataBaseVersion(Simulate.decimal_(1.2), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(1.3))
                {
                    AddColumnToTable(CompanyId, "tbl_employee", "Email", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "Tel1", SQLColumnDataType.VarChar);
                    InsertDataBaseVersion(Simulate.decimal_(1.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(1.4))
                {
					CreateTable("tbl_ForgotPasswordRequest", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_ForgotPasswordRequest", "CompanyId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ForgotPasswordRequest", "Email", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_ForgotPasswordRequest", "Tel1", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_ForgotPasswordRequest", "EmployeeID", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_ForgotPasswordRequest", "GeneratedPassword", SQLColumnDataType.VarChar);
                    ClsSQL.ExecuteQueryStatement(@"

CREATE 
TRIGGER [dbo].[trg_SendEmailOnInsert]
ON [dbo].[tbl_ForgotPasswordRequest]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Variables to store inserted values
    DECLARE @GeneratedPassword NVARCHAR(MAX);
    DECLARE @Email NVARCHAR(MAX);
    

    -- Check the inserted table
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        -- Get the values of the newly inserted record
        SELECT 
            @GeneratedPassword = ISNULL(GeneratedPassword, 'N/A'),
            @Email = ISNULL(Email, 'N/A')            
        FROM inserted;

        -- Construct the email body
        DECLARE @Body NVARCHAR(MAX) = 
            'Please use this mail and password to login for your account, This is a one time password so you have to change it once you login:' + CHAR(13) + CHAR(10) +
            'GeneratedPassword: ' + @GeneratedPassword + CHAR(13) + CHAR(10) +
            'Email: ' + @Email + CHAR(13) + CHAR(10) ;

        -- Send the email
        EXEC msdb.dbo.sp_send_dbmail
            @profile_name = 'MTMail', -- Replace with your Database Mail profile
            @recipients = @Email, -- Replace with the recipient's email address
            @subject = 'One Time Password',
            @body = @Body;
    END;
END;
", ClsSQL.CreateDataBaseConnectionString(CompanyId));

                    InsertDataBaseVersion(Simulate.decimal_(1.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(1.5))
                {
                    AddColumnToTable(CompanyId, "tbl_POSDay", "CashDrawerID", SQLColumnDataType.Integer);
            
                    InsertDataBaseVersion(Simulate.decimal_(1.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(1.6))
                {
					CreateTable("tbl_BranchFloors", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_BranchFloors", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloors", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloors", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloors", "ModificationDate", SQLColumnDataType.DateTime);
					AddColumnToTable(CompanyId, "tbl_BranchFloors", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_BranchFloors", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_BranchFloors", "CompanyID", SQLColumnDataType.Integer);
                    


                    CreateTable("tbl_BranchFloorsTables", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "FloorID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "Shape", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "Color", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "ChairsCount", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "PositionX", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "PositionY", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BranchFloorsTables", "Width", SQLColumnDataType.Decimal);
					
					AddColumnToTable(CompanyId, "tbl_CashDrawer", "PosTypeID", SQLColumnDataType.Integer);

                    

                    clsForms clsForms = new clsForms();
				//
					clsForms.InsertForm(77,"BranchFloorsMain", "طوابق الفروع الرئيسيه", "Branch Floors Main", 52, true, true, false, false, false, false, CompanyId);
                    //
                    clsForms.InsertForm(78,"BranchFloorsAdd", "اضافة طابق للفروع ", "Branch Floors Add", 52, true, true, true, true, true, false, CompanyId);
                    //
                    clsForms.InsertForm(79,"BranchFloorsTablesMain", "طاولات الفروع الرئيسيه", "Branch Floors Main", 52, true, true, false, false, false, false, CompanyId);
                    //
                    clsForms.InsertForm(80,"BranchFloorsTablesAdd", "اضافة طاولات للفروع ", "Branch Floors Add", 52, true, true, true, true, true, false, CompanyId);
                    clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(19, "طلبات طاولات نقاط البيع", "POS Sales Table Order", 0, CompanyId);


                 // AddColumnToTable(CompanyId, "tbl_CashDrawer", "Status", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_InvoiceHeader", "Status", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceHeader", "TableID", SQLColumnDataType.Integer);



                    InsertDataBaseVersion(Simulate.decimal_(1.6), CompanyId);
                }
				if (versionNumber < Simulate.decimal_(1.7))
				{
					clsForms clsForms = new clsForms();
				 
				 
                    clsForms.InsertForm(81,"Filter CostCenter","صلاحيات مركز الكلفة","Filter Cost Center",0,true,false, false, false, false, false, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(1.7), CompanyId);

                }


            }
            catch (Exception ex)
            {

                throw;
            }
        }
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
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_DataBaseVersion where (VersionNumber=@VersionNumber or @VersionNumber=0 ) order by VersionNumber desc  ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                  
                return dt;
            }
            catch (Exception)
            {
                throw;
            }


        }

		public bool CreateTable(String TableName,int CompanyID) {

			try
			{
              clsSQL  clssql= new clsSQL();
                string createTableQuery = $@"
            CREATE TABLE [{TableName}] (
                ID INT PRIMARY KEY IDENTITY(1,1),
                CreationDate DATETIME DEFAULT GETDATE()
            );";



                DataTable dt = clssql.ExecuteQueryStatement(createTableQuery, clssql.CreateDataBaseConnectionString(CompanyID));
                if (dt != null && dt.Rows.Count > 0)
                {

                    return true;
                }
                else
                {
                    return false;
                }

                return true;
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
                    sqlColumnType = varcharLength.HasValue ? $"NVARCHAR({varcharLength.Value})" : "NVARCHAR(MAX)";
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

            int columnExists = (int)clssql.ExecuteScalar(checkColumnQuery, clssql.CreateDataBaseConnectionString(CompanyID), null);

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
	public	bool CreateDataBase(string DataBaseName , int CompanyID) {


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
	[ID] [int] IDENTITY(1,1) NOT NULL,
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
GO
SET IDENTITY_INSERT [dbo].[tbl_AccountNature] ON 

GO
INSERT [dbo].[tbl_AccountNature] ([ID], [AName], [EName], [CompanyID]) VALUES (1, N'مدين', N'Debit', NULL)
GO
INSERT [dbo].[tbl_AccountNature] ([ID], [AName], [EName], [CompanyID]) VALUES (2, N'دائن', N'Credit', NULL)
GO
SET IDENTITY_INSERT [dbo].[tbl_AccountNature] OFF
GO
SET IDENTITY_INSERT [dbo].[tbl_ReportingType] ON 

GO
INSERT [dbo].[tbl_ReportingType] ([ID], [AName], [EName], [ParentID], [CompanyID]) VALUES (1, N'قائمة دخل', N'Income Statment', 0, NULL)
GO
INSERT [dbo].[tbl_ReportingType] ([ID], [AName], [EName], [ParentID], [CompanyID]) VALUES (2, N'ميزانيه ', N'Balance Sheet', 0, NULL)
GO
SET IDENTITY_INSERT [dbo].[tbl_ReportingType] OFF
GO




GO
SET IDENTITY_INSERT [dbo].[tbl_Forms] ON 

GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (1, N'cashvouchermain', 8, N'شاشة رئيسية سندات نقدية', N'cash voucher main', 1, 0, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (2, N'cashvoucherpageadd', 8, N'شاشة اضافة سندات نقدية', N'cash voucher add', NULL, NULL, 1, 1, 1, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (3, N'journalvouchermain', 8, N'شاشة رئيسية قيود مالية', N'journal voucher main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (4, N'journalvoucherpageadd', 8, N'شاشة اضافة قيود مالية', N'journal voucher page add', NULL, 0, 1, 1, 1, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (5, N'accountstatement', 8, N'كشف حساب', N'account statement', 1, 0, 1, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (6, N'invoicereport', 8, N'تقرير الفواتير', N'invoice report', 1, 0, 1, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (7, N'trialbalance', 8, N'ميزان المراجعة', N'trial balance', 1, 0, 1, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (8, N'accountingpage', 53, N'المحاسبة', N'accounting page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (9, N'dashboard', 11, N'لوحة التقارير', N'dashboard', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (10, N'financingAdd', 11, N'شاشة اضافة تمويلات', N'financing Add', NULL, 0, 1, 1, 1, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (11, N'financingMain', 53, N'شاشة رئيسية تمويلات', N'financing Main', 1, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (12, N'invoicemain', 14, N'شاشة رئيسية فواتير', N'invoice main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (13, N'invoicepageadd', 14, N'شاشة اضافة فواتير', N'invoice page add', NULL, 0, 1, 1, 1, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (14, N'inventorypage', 53, N'شاشة رئيسية مستودعات', N'inventory page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (15, N'inventoyreport', 17, N'تقارير المستودعات', N'inventoy report', 1, 1, 0, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (16, N'itemtransactionreport', 17, N'تقرير حركة مادة', N'item transaction report', 1, 1, NULL, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (17, N'posfirstpage', 53, N'نقاط بيع شاشة اولى', N'pos first page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (18, N'posmain', 17, N'نقاط بيع رئيسية', N'pos main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (19, N'possetting', 52, N'اعدادات نقاط البيع', N'pos setting', 1, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (20, N'cashreport', 17, N'تقرير النقدية', N'cash report', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (21, N'accountsetting', 52, N'اعدادات الحسابات', N'account setting', 1, NULL, 1, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (22, N'bankspageadd', 52, N'اضافة بنوك', N'banks page add', 0, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (23, N'bankspagemain', 52, N'شاشة بنوك رئيسية', N'banks page main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (24, N'branchpageadd', 52, N'اضافة فروع', N'branch page add', NULL, 0, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (25, N'branchpagemain', 52, N'شاشة فروع رئيسية', N'branch page main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (26, N'businesspartnermain', 52, N' شاشة مورد/عميل', N'business partner main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (27, N'businesspartnerpageadd', 52, N'اضافة مورد/عميل', N'business partner page add', NULL, 0, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (28, N'cashdraweradd', 52, N'اضافة صناديق', N'cash drawer add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (29, N'cashdrawermain', 52, N'شاشة رئيسية صناديق', N'cash drawer main', 0, 0, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (30, N'accountadd', 52, N'اضافة حساب', N'account add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (31, N'accountmain', 52, N'شاشة شجرة الحسابات', N'account main', 1, 0, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (32, N'companypageadd', 52, N'معلومات الشركة', N'company page add', 1, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (33, N'costcenterpageadd', 52, N'اضافة مركز كلفة', N'cost center page add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (34, N'costcenterpagemain', 52, N'شاشة رئيسية مراكز الكلفة', N'cost center page main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (35, N'countriesmainpage', 52, N'شاشة رئيسية دول', N'countries main page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (36, N'countriespageadd', 52, N'اضافة دول', N'countries page add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (37, N'itempageadd', 52, N'اضافة مادة', N'item page add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (38, N'itemspagemain', 52, N'شاشة رئيسية مواد', N'items page main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (39, N'itemsboxtypemainpage', 52, N'شاشة رئيسية تعبئة', N'items box type main page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (40, N'itemsboxtypepageadd', 52, N'اضافة تعبئة', N'items box type page add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (41, N'itemscategoryadd', 52, N'اضافة مجموعات مواد', N'items category add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (42, N'itemscategorymain', 52, N'شاشة رئيسية مجموعات مواد', N'items category main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (43, N'possessionstypemain', 52, N'شاشة رئيسية جلسات نقاط البيع', N'pos sessions type main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (44, N'possessionstypepageadd', 52, N'اضافة جلسات نقاط البيع', N'pos sessions type page add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (45, N'storepageadd', 52, N'شاشة رئيسية مستودعات', N'store page add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (46, N'storepagemain', 52, N'اضافة مستودعات', N'store page main', 1, 0, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (47, N'taxpageadd', 52, N'اضافة ضرائب', N'tax page add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (48, N'taxpagemain', 52, N'شاشة رئيسية ضرائب', N'tax page main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (49, N'userAuthorizationAdd', 52, N'شاشة صلاحيات المستخدمين', N'user Authorization Add', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (50, N'userspageadd', 52, N'اضافة مستخدمين', N'users page add', 0, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (51, N'userspagemain', 52, N'شاشة رئيسية مستخدمين', N'users page main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (52, N'setting_page', 53, N'شاشة الإعدادات ', N'setting page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (53, N'homepage', 0, N'الشاشة الرئيسية', N'home page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (54, N'financingpage', 53, N'شاشه التمويلات', N'financing page', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (55, N'financingReport', 54, N'شاشه تقرير التمويلات', N'financing Report', 1, 1, NULL, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (56, N'loanType', 52, N'انواع القروض', N'loanType', 1, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (57, N'loanTypeAdd', 52, N'اضافه قروض', N'loanTypeAdd', NULL, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (58, N'AgingReport', 8, N'تقرير اعمار الذمم', N'AgingReport', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (59, N'CashLoanMain', 53, N'شاشه قروض النقديه', N'CashLoanMain', 1, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (60, N'CashLoanAdd', 53, N'اضافه قرض نقدي', N'CashLoanAdd', NULL, NULL, 1, 1, 1, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (61, N'BusinessPartnerBalanceReport', 53, N'تقرير ارصدة المتعاملين', N'BusinessPartnerBalanceReport', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (62, N'
LoanScheduling', 53, N'جدولة القيود', N'
LoanschedulingMain', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (63, N'
LoanschedulingAdd', 53, N'اضافة جدولة قروض', N'
LoanschedulingAdd', 1, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (64, N'SubscriptionsAdd', 53, N'اضافة إشتراك', N'SubscriptionsAdd', 1, NULL, 1, NULL, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (65, N'Loan RJ Reports', 53, N'تقارير الملكية', N'Loan RJ Reports', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (66, N'ReportingTypeNodes', 52, N'تفاصيل التقارير المحاسبيه', N'ReportingTypeNodes', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (67, N'ReportingTypeNodesAdd', 52, N'اضافه تفاصيل التقارير', N'ReportingTypeNodesAdd', 1, NULL, 1, 1, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (68, N'BalanceSheet', 8, N'تقرير الميزانيه', N'Balances Sheet', 1, NULL, NULL, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (69, N'IncomeStatement', 8, N'تقرير الأرباح والخسائر', N'IncomeStatement', 1, NULL, NULL, NULL, NULL, 1)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (70, N'Filter Vendors', 0, N'صلاحيات الموردين', N'Filter Vendors', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (71, N'Filter Branch', 0, N'صلاحيات الفروع', N'Filter Branch', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (72, N'ReconciliationMain', 8, N'المطابقه', N'ReconciliationMain', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (73, N'ReconciliationAdd', 8, N'المطابقه', N'ReconciliationAdd', 1, NULL, 1, NULL, 1, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (74, N'Signuture', 54, N'التواقيع', N'Signuture', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (75, N'Payment Method Main', 52, N'شاشه طرق الدفع', N'Payment Method Main', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_Forms] ([ID], [FrmName], [ParentID], [AName], [EName], [IsAccess], [IsSearch], [IsAdd], [IsEdit], [IsDelete], [IsPrint]) VALUES (76, N'Payment Method Add', 52, N'شاشه اضافه طريقه دفع', N'Payment Method Add', 1, 1, 1, 1, 1, NULL)
GO
SET IDENTITY_INSERT [dbo].[tbl_Forms] OFF
GO


GO
SET IDENTITY_INSERT [dbo].[tbl_DataBaseVersion] ON 

GO
INSERT [dbo].[tbl_DataBaseVersion] ([ID], [VersionNumber], [CreationDate]) VALUES (1, CAST(1.100000 AS Decimal(18, 6)), CAST(N'2024-12-02 22:51:50.907' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[tbl_DataBaseVersion] OFF
GO
GO
SET IDENTITY_INSERT [dbo].[tbl_ItemReadType] ON 

GO
INSERT [dbo].[tbl_ItemReadType] ([ID], [AName], [EName], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate]) VALUES (1, N'حبه', NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[tbl_ItemReadType] ([ID], [AName], [EName], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate]) VALUES (2, N'ميزان', NULL, NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[tbl_ItemReadType] OFF
GO


GO
SET IDENTITY_INSERT [dbo].[tbl_JournalVoucherTypes] ON 

GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (1, N'قيد يدوي', N'Manual JV', 0)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (2, N'فاتورة مشتريات', N'Purchase Invoice', 1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (3, N'فاتورة مبيعات', N'Sales Invoice', -1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (4, N'مردود مبيعات', N'Sales Refund', 1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (5, N'عرض سعر', N'Sales Offer', 0)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (6, N'طلب شراء', N'Purchase Offer', 0)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (7, N'مردود مشتريات', N'Purchase Refund', -1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (8, N'سند ادخال', N'Good Recipt', 1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (9, N'سند اخراج', N'Good Issue', -1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (10, N'فاتوره نقاط بيع', N'POS Sales Invoice', -1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (11, N'مردود نقاط بيع', N'POS Sales Invoice Refund', 1)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (12, N'سند صرف', N'Cash Payment', 0)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (13, N'سند قبض', N'Cash Recivable', 0)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (14, N'تمويل', N'Financing', 0)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (15, N'جدولة القروض', N'Loan Scheduling', 0)
GO
INSERT [dbo].[tbl_JournalVoucherTypes] ([ID], [AName], [EName], [QTYFactor]) VALUES (16, N'كشف الملكيه', N'RJBulkUpload', 0)
GO
SET IDENTITY_INSERT [dbo].[tbl_JournalVoucherTypes] OFF
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

              clsCompany clsCompany= new clsCompany();
				clsCompany.UpdateCompanyDataBaseName(CompanyID, DataBaseName);
				checkDatabaseUpdates(0, CompanyID);
                   // InsertDataBaseVersion(Simulate.decimal_("1.0") , CompanyID);

                    return true;
               
              
            }
            catch (Exception ex)
            {
                return false;

            }
        }
}
}