using Azure;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using FastReport.Barcode;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
 
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using System.Security.Principal;
using WebApplication2.DataBaseTable;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;
using WebApplication2.cls.Reports;
namespace WebApplication2.cls
{
    public class clsDataBaseVersion
    {


		public void checkDatabaseUpdates(decimal versionNumber,int CompanyId) {
			try
            {
                clsForms clsForms = new clsForms();
                clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                clsSQL ClsSQL= new clsSQL();
                #region OldVersions

              
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
					 


                    clsForms.InsertForm(81, "Filter CostCenter", "صلاحيات مركز الكلفة", "Filter Cost Center", 0, true, false, false, false, false, false, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(1.7), CompanyId);

                }

                if (versionNumber < Simulate.decimal_(1.8))
                {

					CreateTable("tbl_Currency",CompanyId);
					 
					AddColumnToTable(CompanyId, "tbl_Currency", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Currency", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Currency", "Code", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Currency", "PartAName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Currency", "PartEName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Currency", "DecimalPlaces", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Currency", "Symbol", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Currency", "ExchangeRate", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_Currency", "IsBase", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_Currency", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_Currency", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Currency", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_Currency", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Currency", "CompanyID", SQLColumnDataType.Integer);
                   
                    clsForms.InsertForm(82, "CurrenyMain", "العملات رئيسي", "Currency Main", 52, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(83, "CurrenyAdd", "اضافه عملات", "Currency Add", 52, false, true, true, true, true, false, CompanyId);
					clsCurrency clsCurrency = new clsCurrency();
					clsCurrency.InsertCurrency("دينار أردني", "Jordanian Dinnar", "JOD", "فلس", "Fils"
						,3,"JD",1,true,true,0,CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(1.8), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(1.9))
                {
                    
                    AddColumnToTable(CompanyId, "tbl_JournalVoucherDetails", "CurrencyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_JournalVoucherDetails", "CurrencyRate", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_JournalVoucherDetails", "CurrencyBaseAmount", SQLColumnDataType.Decimal);
					string a = "update tbl_JournalVoucherDetails set CurrencyID= (select id from tbl_currency where isbase=1) , CurrencyRate=1,CurrencyBaseAmount=total where isnull (CurrencyID,0)=0  ";
					ClsSQL.ExecuteQueryStatement(a, ClsSQL.CreateDataBaseConnectionString(CompanyId));
               
                    AddColumnToTable(CompanyId, "tbl_InvoiceHeader", "CurrencyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceHeader", "CurrencyRate", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_InvoiceHeader", "CurrencyBaseAmount", SQLColumnDataType.Decimal);
                      a = "update tbl_InvoiceHeader set CurrencyID= (select id from tbl_currency where isbase=1) , CurrencyRate=1,CurrencyBaseAmount=TotalInvoice where isnull (CurrencyID,0)=0  ";
                    ClsSQL.ExecuteQueryStatement(a, ClsSQL.CreateDataBaseConnectionString(CompanyId));
                    clsForms.InsertForm(84, "Social Solidarity Fund", "صندوق تكافل وظيفي", "Social Solidarity Fund", 0, true, false, false, false, false, false, CompanyId);


                    InsertDataBaseVersion(Simulate.decimal_(1.9), CompanyId);
                }
				if (versionNumber < Simulate.decimal_(2.0))
				{
                    AddColumnToTable(CompanyId, "tbl_Items", "AVGCostPerUnit", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "AVGCostPerUnit", SQLColumnDataType.Decimal);
					string a = @"
declare @companyID int = "+ CompanyId+@"
CREATE TABLE [dbo].[tbl_DashboardWidgets](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[WidgetType] [nvarchar](50) NOT NULL,
	[GroupName] [nvarchar](max) NULL,
	[Title] [nvarchar](255) NOT NULL,
	[SQLQuery] [nvarchar](max) NOT NULL,
	[ChartConfig] [nvarchar](max) NULL,
	[Icon] [nvarchar](100) NULL,
	[Color] [nvarchar](50) NULL,
	[SectionName] [nvarchar](max) NULL,
	[SectionIndex] [int] NULL,
	[CreationDate] [datetime] NULL CONSTRAINT [DF__Dashboard__Creat__1B9317B3]  DEFAULT (getdate()),
	[CreationUserID] [int] NULL,
	[ModificationDate] [datetime] NULL CONSTRAINT [DF__Dashboard__Updat__1C873BEC]  DEFAULT (getdate()),
	[ModificationUserID] [int] NULL,
	[CompanyID] [int] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK__Dashboar__ADFD3012C2B40457] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

 
SET IDENTITY_INSERT [dbo].[tbl_DashboardWidgets] ON 

 
INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (1, -1, N'KPI', N'Finance', N'Month To Day Revenue ', N'WITH CurrentMonth AS (
    SELECT 
        SUM(Total * -1) AS Total 
    FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
    ) 
    AND YEAR(DueDate) = YEAR(GETDATE()) 
    AND MONTH(DueDate) = MONTH(GETDATE())
),
PreviousMonth AS (
    SELECT 
        SUM(Total * -1) AS Total 
    FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
    ) 
    AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
)
SELECT 
    COALESCE(CM.Total, 0) AS Total,
   -- COALESCE(PM.Total, 0) AS PreviousMonthTotal,
    CASE 
        WHEN COALESCE(PM.Total, 0) = 0 THEN NULL
        ELSE (COALESCE(CM.Total, 0) - COALESCE(PM.Total, 0)) * 100.0 / COALESCE(PM.Total, 1)
    END AS PercentageChange
FROM CurrentMonth CM, PreviousMonth PM;
', NULL, N'0xf155', N'#2ecc71', N'rightWidgets', 343, CAST(N'2025-01-22 23:06:41.750' AS DateTime), NULL, CAST(N'2025-01-25 22:38:37.777' AS DateTime), 1, @companyID, 1)
 
INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (2, -1, N'KPI', N'Finance', N'Total Credit Amount', N'SELECT SUM(Credit) AS Total FROM tbl_JournalVoucherDetails', NULL, N'0xe85d', N'#f39c12', N'leftWidgets', 643, CAST(N'2025-01-22 23:06:41.750' AS DateTime), NULL, CAST(N'2025-01-25 22:43:09.043' AS DateTime), 1, @companyID, 1)
 
INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (3, -1, N'KPI', N'Finance', N'Total Journal Vouchers', N'SELECT COUNT(*) AS Total FROM tbl_JournalVoucherHeader', NULL, N'0xe850', N'#3498db', N'rightWidgets', 151, CAST(N'2025-01-22 23:06:41.750' AS DateTime), NULL, CAST(N'2025-01-25 22:38:29.407' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (4, -1, N'KPI', N'Finance', N'Total Accounts', N'SELECT COUNT(*) AS Total FROM tbl_Accounts', NULL, N'0xe30b', N'#9b59b6', N'leftWidgets', 545, CAST(N'2025-01-22 23:06:41.750' AS DateTime), NULL, CAST(N'2025-01-25 22:38:34.537' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (5, -1, N'BarChart', N'Finance', N'Debit by Account', N'SELECT A.EName AS Name, SUM(D.Debit) AS Total 
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_Accounts A ON D.AccountID = A.ID 
     GROUP BY A.EName', N'{ ""xAxis"": ""AccountName"", ""yAxis"": ""TotalDebit"", ""barColor"": ""#1abc9c"" }', N'0xe6cb', N'#e74c3c', N'midWidgets', 3455, CAST(N'2025-01-22 23:06:47.283' AS DateTime), NULL, CAST(N'2025-01-25 22:39:05.850' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (6, -1, N'BarChart', N'Finance', N'Credit by Account', N'SELECT A.EName AS Name, SUM(D.Credit) AS Total
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_Accounts A ON D.AccountID = A.ID 
     GROUP BY A.EName', N'{ ""xAxis"": ""AccountName"", ""yAxis"": ""TotalCredit"", ""barColor"": ""#f39c12"" }', N'0xe85d', N'#f39c12', N'midWidgets', 3155, CAST(N'2025-01-22 23:06:47.283' AS DateTime), NULL, CAST(N'2025-01-25 22:39:03.913' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (7, -1, N'LineChart', N'Finance', N'Monthly Debit Trend', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, SUM(D.Debit) AS Total
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
     ORDER BY Name', N'{ ""xAxis"": ""Month"", ""yAxis"": ""TotalDebit"", ""lineColor"": ""#3498db"" }', N'0xe6cb', N'#e74c3c', N'midWidgets', 491, CAST(N'2025-01-22 23:06:52.590' AS DateTime), NULL, CAST(N'2025-01-25 22:38:44.000' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (8, -1, N'LineChart', N'Finance', N'Monthly Credit Trend', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, SUM(D.Credit) AS Total FROM tbl_JournalVoucherDetails D
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
     ORDER BY Name', N'{ ""xAxis"": ""Month"", ""yAxis"": ""TotalCredit"", ""lineColor"": ""#e74c3c"" }', N'0xe85d', N'#f39c12', N'midWidgets', 791, CAST(N'2025-01-22 23:06:52.603' AS DateTime), NULL, CAST(N'2025-01-25 22:38:50.700' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (9, -1, N'PieChart', N'Finance', N'Debit Distribution by Cost Center', N'SELECT A.EName AS Name, SUM(D.Debit) AS Total
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_Accounts A ON D.AccountID = A.ID
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY A.EName', N'{ ""labels"": ""CostCenter"", ""values"": ""TotalDebit"", ""pieColors"": [""#3498db"", ""#e74c3c"", ""#2ecc71""] }', N'0xe6cb', N'#e74c3c', N'midWidgets', 151, CAST(N'2025-01-22 23:06:58.810' AS DateTime), NULL, CAST(N'2025-01-25 22:43:09.043' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (10, -1, N'Table', N'Finance', N'Recent Journal Entries', N'SELECT TOP 10 JVNumber, VoucherDate, BranchID, CostCenterID 
     FROM tbl_JournalVoucherHeader 
     ORDER BY VoucherDate DESC', N'{ ""columns"": [""JVNumber"", ""VoucherDate"", ""BranchID"", ""CostCenterID""] }', N'0xf128', N'#1abc9c', N'midWidgets', 2647, CAST(N'2025-01-22 23:07:05.980' AS DateTime), NULL, CAST(N'2025-01-25 22:39:01.213' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (11, -1, N'BarChart', N'Finance', N'Monthly Debit vs Credit', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, 
            SUM(D.Debit) AS Total, 
            SUM(D.Credit) AS TotalCredit
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
     ORDER BY Name', N'{ ""xAxis"": ""Month"", ""yAxis"": [""TotalDebit"", ""TotalCredit""], ""barColors"": [""#1abc9c"", ""#e74c3c""] }', N'0xe85d', N'#f39c12', N'midWidgets', 2347, CAST(N'2025-01-22 23:07:12.433' AS DateTime), NULL, CAST(N'2025-01-25 22:38:59.580' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (12, -1, N'BarChart', N'Finance', N'Best Performing Accounts by Credit', N'SELECT TOP 5 A.EName AS Name, SUM(D.Credit) AS Total 
      FROM tbl_JournalVoucherDetails D
      JOIN tbl_Accounts A ON D.AccountID = A.ID 
      GROUP BY A.EName 
      ORDER BY SUM(D.Credit) DESC', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""barColor"": ""#e67e22"" }', N'0xe85d', N'#f39c12', N'midWidgets', 151, CAST(N'2025-01-25 20:43:31.597' AS DateTime), NULL, CAST(N'2025-01-25 22:43:09.040' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (13, -1, N'PieChart', N'Customers', N'Top 5 Customers by Invoice Amount', N'SELECT TOP 5 BP.EName AS Name, SUM(I.TotalInvoice) AS Total
      FROM tbl_InvoiceHeader I
      JOIN tbl_BusinessPartner BP ON I.BusinessPartnerID = BP.ID
      GROUP BY BP.EName
      ORDER BY SUM(I.TotalInvoice) DESC', N'{ ""labels"": ""Name"", ""values"": ""Total"", ""pieColors"": [""#1abc9c"", ""#3498db"", ""#9b59b6"", ""#f1c40f"", ""#e74c3c""] }', N'0xf007', N'#e67e22', N'midWidgets', 451, CAST(N'2025-01-25 20:43:31.600' AS DateTime), NULL, CAST(N'2025-01-25 22:38:55.677' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (14, -1, N'LineChart', N'Sales', N'Monthly Sales Trend', N'SELECT FORMAT(InvoiceDate, ''yyyy-MM'') AS Name, SUM(TotalInvoice) AS Total
      FROM tbl_InvoiceHeader 
      GROUP BY FORMAT(InvoiceDate, ''yyyy-MM'')
      ORDER BY Name', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""lineColor"": ""#2ecc71"" }', N'0xf080', N'#2980b9', N'midWidgets', 151, CAST(N'2025-01-25 20:43:31.600' AS DateTime), NULL, CAST(N'2025-01-25 22:38:36.680' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (15, -1, N'BarChart', N'Sales', N'Branch-wise Sales', N'SELECT B.EName AS Name, SUM(I.TotalInvoice) AS Total 
      FROM tbl_InvoiceHeader I
      JOIN tbl_Branch B ON I.BranchID = B.ID
      GROUP BY B.EName
      ORDER BY SUM(I.TotalInvoice) DESC', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""barColor"": ""#3498db"" }', N'0xf080', N'#2980b9', N'midWidgets', 151, CAST(N'2025-01-25 20:43:31.600' AS DateTime), NULL, CAST(N'2025-01-25 22:38:46.060' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (16, -1, N'Table', N'Sales', N'Recent Transactions', N'SELECT TOP 10 InvoiceNo AS Name, TotalInvoice AS Total
      FROM tbl_InvoiceHeader 
      ORDER BY InvoiceDate DESC', N'{ ""columns"": [""Name"", ""Total""] }', N'0xf07a', N'#16a085', N'leftWidgets', 285, CAST(N'2025-01-25 20:43:31.600' AS DateTime), NULL, CAST(N'2025-01-25 22:38:47.590' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (17, -1, N'KPI', N'Finance', N'Account Balances Overview', N'SELECT SUM(Debit) - SUM(Credit) AS Total, ''Account Balance'' AS Name
      FROM tbl_JournalVoucherDetails', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', 151, CAST(N'2025-01-25 20:43:31.600' AS DateTime), NULL, CAST(N'2025-01-25 22:38:31.483' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (18, -1, N'LineChart', N'Finance', N'Monthly Expense Trend', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, SUM(D.Debit) AS Total
      FROM tbl_JournalVoucherDetails D
      JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
      GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
      ORDER BY Name', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""lineColor"": ""#e74c3c"" }', N'0xf0d6', N'#c0392b', N'midWidgets', 1953, CAST(N'2025-01-25 20:43:31.600' AS DateTime), NULL, CAST(N'2025-01-25 22:38:57.447' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (19, -1, N'KPI', N'Warehouse', N'Total Products', N'SELECT COUNT(*) AS Total, ''Products'' AS Name FROM tbl_Items', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', 151, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:35.580' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (20, -1, N'KPI', N'Admin', N'Total Branches', N'SELECT COUNT(*) AS Total, ''Branches'' AS Name FROM tbl_Branch', NULL, N'0xf19c', N'#f1c40f', N'leftWidgets', 383, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:30.290' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (21, -1, N'KPI', N'Admin', N'Total Transactions', N'SELECT COUNT(*) AS Total, ''Transactions'' AS Name FROM tbl_CashVoucherHeader', NULL, N'0xf07a', N'#16a085', N'rightWidgets', 151, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:43:09.037' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (22, -1, N'KPI', N'Customers', N'Active Customers', N'SELECT COUNT(*) AS Total, ''Active Customers'' AS Name FROM tbl_BusinessPartner WHERE Active = 1', NULL, N'0xf007', N'#e67e22', N'leftWidgets', 1185, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:52.370' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (23, -1, N'KPI', N'Finance', N'Total Revenue', N'SELECT SUM(TotalInvoice) AS Total, ''Revenue'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf155', N'#2ecc71', N'rightWidgets', 173, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:33.300' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (24, -1, N'KPI', N'Finance', N'Total Expenses', N'SELECT SUM(Debit) AS Total, ''Expenses'' AS Name FROM tbl_JournalVoucherDetails', NULL, N'0xf0d6', N'#c0392b', N'leftWidgets', 727, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:40.167' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (25, -1, N'KPI', N'Finance', N'Profit Margin', N'SELECT (SUM(TotalInvoice) - SUM(Debit)) AS Total, ''Profit'' AS Name FROM tbl_InvoiceHeader I 
      JOIN tbl_JournalVoucherDetails J ON I.CompanyID = J.CompanyID', NULL, N'0xf0e7', N'#8e44ad', N'rightWidgets', 451, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:41.640' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (26, -1, N'KPI', N'Sales', N'Avg Sales per Customer', N'SELECT AVG(TotalInvoice) AS Total, ''Avg Sale'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf080', N'#2980b9', N'leftWidgets', 1377, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:53.530' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (27, -1, N'KPI', N'Sales', N'Highest Sale', N'SELECT MAX(TotalInvoice) AS Total, ''Highest Sale'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', 1569, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:55.190' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (28, -1, N'KPI', N'Sales', N'Lowest Sale', N'SELECT MIN(TotalInvoice) AS Total, ''Lowest Sale'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf128', N'#1abc9c', N'leftWidgets', 751, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:38:49.163' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (29, -1, N'KPI', N'Sales', N'Pending Invoices', N'SELECT COUNT(*) AS Total, ''Pending Invoices'' AS Name FROM tbl_InvoiceHeader WHERE Status = 0', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', 572, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:43:09.040' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (30, -1, N'KPI', N'Admin', N'Active Users', N'SELECT COUNT(*) AS Total, ''Users'' AS Name FROM tbl_employee WHERE IsSystemUser = 1', NULL, N'0xf128', N'#1abc9c', N'leftWidgets', 343, CAST(N'2025-01-25 21:00:13.963' AS DateTime), NULL, CAST(N'2025-01-25 22:43:09.040' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (31, -1, N'KPI', N'Admin', N'Total Cost Centers', N'SELECT COUNT(*) AS Total, ''Cost Centers'' AS Name FROM tbl_CostCenter', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', 451, CAST(N'2025-01-25 21:00:13.967' AS DateTime), NULL, CAST(N'2025-01-25 22:43:09.043' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (86, -1, N'KPI', N'Finance', N'Month Gross Profit Margin', N'select (q.totalRevenue-q.totalCost)/ q.totalRevenue *100 as Total,



    CASE 
        WHEN COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/ q.totalRevenuePastMonth *100 ), 0) = 0 THEN NULL
        ELSE (COALESCE(((q.totalRevenue-q.totalCost)/ q.totalRevenue *100 ), 0) - COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/ q.totalRevenuePastMonth *100 ), 0)) * 100.0 / COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/ q.totalRevenuePastMonth *100 ), 1)
    END AS PercentageChange
 
 
 
  from (select 

( select isnull(SUM(Total * -1),0) AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
		 
    ) 
    AND YEAR(DueDate) = YEAR(GETDATE()) 
    AND MONTH(DueDate) = MONTH(GETDATE())) as totalRevenue 
	,
	( select isnull(SUM(Total  ),0) AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
       
		 
    ) 
    AND YEAR(DueDate) = YEAR(GETDATE()) 
    AND MONTH(DueDate) = MONTH(GETDATE())) as totalCost,
	( select isnull( SUM(Total * -1) ,0) AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
		 
    ) 
     AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
	) as totalRevenuePastMonth 
	,
	( select isnull(SUM(Total  ) ,0)AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
       
		 
    ) 
      AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
	) as totalCostPastMonth) as q', NULL, N'0xf0e7', N'#8e44ad', N'leftWidgets', 151, CAST(N'2025-01-26 20:41:24.303' AS DateTime), 1, CAST(N'2025-01-26 20:41:24.303' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (91, -1, N'KPI', N'Finance', N'Inventory Turnover Rate/YTD', N'SELECT 
    (q.totalCost * -1) / 
    NULLIF((q.totalStock - q.totalStockPastYear) / 2, 0) AS Total,
    
    (q.totalCostPastYear * -1) / 
    NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0) AS TotalPast,

    CASE 
        WHEN COALESCE(
            (q.totalCostPastYear * -1) / 
            NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0), 
        0) = 0 
        THEN NULL
        ELSE 
            (COALESCE(
                (q.totalCost * -1) / 
                NULLIF((q.totalStock - q.totalStockPastYear) / 2, 0), 
            0) 
            - 
            COALESCE(
                (q.totalCostPastYear * -1) / 
                NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0), 
            0)
            ) * 100.0 / 
            COALESCE(
                (q.totalCostPastYear * -1) / 
                NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0), 
            1)
    END AS PercentageChange
FROM 
(
    SELECT 
        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) = YEAR(GETDATE())) AS totalCost,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) AS totalCostPastYear,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 8 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         )) AS totalStock,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 8 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) <= YEAR(DATEADD(YEAR, -1, GETDATE()))) AS totalStockPastYear,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 8 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) <= YEAR(DATEADD(YEAR, -2, GETDATE()))) AS totalStockPastYearOpenning
) AS q;
', NULL, N'0xe8cc', N'#1abc9c', N'rightWidgets', 151, CAST(N'2025-01-26 21:47:01.620' AS DateTime), 1, CAST(N'2025-01-26 21:47:01.620' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (96, -1, N'KPI', N'Customers', N'Customer Retention Rate', N'SELECT 
    q.Total, 
   
    CASE 
        WHEN q.TotalPastMonth = 0 THEN NULL 
        ELSE CAST(q.Total AS DECIMAL(18,2)) *100 / CAST(q.TotalPastMonth AS DECIMAL(18,2)) 
    END AS PercentageChange
FROM (
    SELECT 
        (SELECT COUNT(id) 
         FROM tbl_BusinessPartner 
         WHERE [type] = 1 
           AND Active = 1  
           AND YEAR(CreationDate) = YEAR(GETDATE()) 
           AND MONTH(CreationDate) = MONTH(GETDATE())
        ) AS Total,

        (SELECT COUNT(id) 
         FROM tbl_BusinessPartner 
         WHERE [type] = 1 
           AND Active = 1  
           AND YEAR(CreationDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
           AND MONTH(CreationDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
        ) AS TotalPastMonth
) AS q;
', NULL, N'0xf128', N'#1abc9c', N'leftWidgets', 151, CAST(N'2025-01-26 23:12:22.667' AS DateTime), 1, CAST(N'2025-01-26 23:12:22.667' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (101, -1, N'BarChart', N'Sales', N'Monthly Sales Revenue by Product Category', N' 
select  tbl_ItemsCategory.AName Name ,sum(TotalLine) Total
 from tbl_InvoiceDetails 
 left join tbl_InvoiceHeader on tbl_InvoiceHeader.Guid = tbl_InvoiceDetails.HeaderGuid
 left join tbl_Items on tbl_Items.Guid = tbl_InvoiceDetails.ItemGuid
left join tbl_ItemsCategory on tbl_ItemsCategory.ID = tbl_Items.CategoryID

where 
tbl_InvoiceHeader.InvoiceTypeID = 3 and 
    YEAR(tbl_InvoiceHeader.InvoiceDate) = YEAR(DATEADD(MONTH, 0, GETDATE())) 
    AND MONTH(tbl_InvoiceHeader.InvoiceDate) = MONTH(DATEADD(MONTH, 0, GETDATE()))
group by tbl_ItemsCategory.AName

 

 ', NULL, N'0xf155', N'#2ecc71', N'midWidgets', 151, CAST(N'2025-01-26 23:25:48.283' AS DateTime), 1, CAST(N'2025-01-26 23:25:48.283' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (103, -1, N'BarChart', N'Customers', N'Top 10 Customers by Revenue', N'SELECT Top 10
    SUM(q.Total) AS Total,
   tbl_BusinessPartner.AName  Name
FROM (
    SELECT 
        TotalInvoice AS Total, 
        tbl_InvoiceHeader.BusinessPartnerID AS BusinessPartnerID 
    FROM tbl_InvoiceHeader 
    WHERE InvoiceTypeID in (3,10) and YEAR( tbl_InvoiceHeader.InvoiceDate)= Year(GETDATE()) 

    UNION ALL 

    SELECT 
        TotalAmount AS Total, 
        BusinessPartnerID AS BusinessPartnerID 
    FROM tbl_FinancingHeader where YEAR( tbl_FinancingHeader.VoucherDate)= Year(GETDATE()) 
) AS q left join tbl_BusinessPartner on tbl_BusinessPartner.id = q.BusinessPartnerID
GROUP BY q.BusinessPartnerID ,
   tbl_BusinessPartner.AName
ORDER BY Total DESC;', NULL, N'0xf155', N'#2ecc71', N'midWidgets', 183, CAST(N'2025-01-26 23:46:00.613' AS DateTime), NULL, CAST(N'2025-01-26 23:46:00.613' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (108, -1, N'BarChart', N'Sales', N'Product Return Rate by Category.', N' 
select  tbl_ItemsCategory.AName Name ,sum(TotalLine) Total
 from tbl_InvoiceDetails 
 left join tbl_InvoiceHeader on tbl_InvoiceHeader.Guid = tbl_InvoiceDetails.HeaderGuid
 left join tbl_Items on tbl_Items.Guid = tbl_InvoiceDetails.ItemGuid
left join tbl_ItemsCategory on tbl_ItemsCategory.ID = tbl_Items.CategoryID

where 
tbl_InvoiceHeader.InvoiceTypeID = 4 and 
    YEAR(tbl_InvoiceHeader.InvoiceDate) = YEAR(DATEADD(MONTH, 0, GETDATE())) 
    AND MONTH(tbl_InvoiceHeader.InvoiceDate) = MONTH(DATEADD(MONTH, 0, GETDATE()))
group by tbl_ItemsCategory.AName', NULL, N'0xf128', N'#1abc9c', N'midWidgets', 1, CAST(N'2025-01-28 21:55:58.437' AS DateTime), 1, CAST(N'2025-01-28 21:55:58.437' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (111, -1, N'BarChart', N'Sales', N'Outstanding Invoices by Age (0-30, 30-60, 60+ Days).', N'WITH ReconciledAmounts AS (
    SELECT 
        JVDetailsGuid,
        SUM(Amount) AS Reconciled
    FROM 
        tbl_Reconciliation 
    GROUP BY 
        JVDetailsGuid
),
AgingBuckets AS (
    SELECT 
        CASE 
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) BETWEEN 0 AND 30 THEN ''0-30 Days''
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) BETWEEN 31 AND 60 THEN ''31-60 Days''
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) > 60 THEN ''60+ Days''
        END AS Name,
        total - ISNULL(R.Reconciled, 0) AS Amount
    FROM 
        tbl_JournalVoucherDetails JVD
    LEFT JOIN 
        tbl_BusinessPartner BP ON BP.ID = JVD.SubAccountID
    LEFT JOIN 
        ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE 
        DueDate <= GETDATE() and SubAccountID>0 and  accountid =
		 (	select top 1 AccountID from tbl_AccountSetting where AccountRefID = 7order by CreationDate desc)
)
SELECT 
    Name,
    SUM(Amount) AS Total
FROM 
    AgingBuckets
GROUP BY 
    Name;


', NULL, N'0xf128', N'#1abc9c', N'midWidgets', 1, CAST(N'2025-01-28 22:36:05.283' AS DateTime), 1, CAST(N'2025-01-28 22:36:05.283' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (182, 1, N'KPI', N'Finance', N'Month To Day Revenue ', N'WITH CurrentMonth AS (
    SELECT 
        SUM(Total * -1) AS Total 
    FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
    ) 
    AND YEAR(DueDate) = YEAR(GETDATE()) 
    AND MONTH(DueDate) = MONTH(GETDATE())
),
PreviousMonth AS (
    SELECT 
        SUM(Total * -1) AS Total 
    FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
    ) 
    AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
)
SELECT 
    COALESCE(CM.Total, 0) AS Total,
   -- COALESCE(PM.Total, 0) AS PreviousMonthTotal,
    CASE 
        WHEN COALESCE(PM.Total, 0) = 0 THEN NULL
        ELSE (COALESCE(CM.Total, 0) - COALESCE(PM.Total, 0)) * 100.0 / COALESCE(PM.Total, 1)
    END AS PercentageChange
FROM CurrentMonth CM, PreviousMonth PM;
', NULL, N'0xf155', N'#2ecc71', N'rightWidgets', -1002, CAST(N'2025-01-30 21:14:21.467' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (183, 1, N'KPI', N'Finance', N'Total Credit Amount', N'SELECT SUM(Credit) AS Total FROM tbl_JournalVoucherDetails', NULL, N'0xe85d', N'#f39c12', N'leftWidgets', -654, CAST(N'2025-01-30 21:14:21.860' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (184, 1, N'KPI', N'Finance', N'Total Journal Vouchers', N'SELECT COUNT(*) AS Total FROM tbl_JournalVoucherHeader', NULL, N'0xe850', N'#3498db', N'rightWidgets', -2238, CAST(N'2025-01-30 21:14:22.210' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (185, 1, N'KPI', N'Finance', N'Total Accounts', N'SELECT COUNT(*) AS Total FROM tbl_Accounts', NULL, N'0xe30b', N'#9b59b6', N'leftWidgets', -860, CAST(N'2025-01-30 21:14:22.930' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.737' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (186, 1, N'BarChart', N'Finance', N'Debit by Account', N'SELECT A.EName AS Name, SUM(D.Debit) AS Total 
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_Accounts A ON D.AccountID = A.ID 
     GROUP BY A.EName', N'{ ""xAxis"": ""AccountName"", ""yAxis"": ""TotalDebit"", ""barColor"": ""#1abc9c"" }', N'0xe6cb', N'#e74c3c', N'midWidgets', 2337, CAST(N'2025-01-30 21:14:23.380' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.757' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (187, 1, N'BarChart', N'Finance', N'Credit by Account', N'SELECT A.EName AS Name, SUM(D.Credit) AS Total
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_Accounts A ON D.AccountID = A.ID 
     GROUP BY A.EName', N'{ ""xAxis"": ""AccountName"", ""yAxis"": ""TotalCredit"", ""barColor"": ""#f39c12"" }', N'0xe85d', N'#f39c12', N'midWidgets', 2037, CAST(N'2025-01-30 21:14:24.440' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.757' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (188, 1, N'LineChart', N'Finance', N'Monthly Debit Trend', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, SUM(D.Debit) AS Total
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
     ORDER BY Name', N'{ ""xAxis"": ""Month"", ""yAxis"": ""TotalDebit"", ""lineColor"": ""#3498db"" }', N'0xe6cb', N'#e74c3c', N'midWidgets', 555, CAST(N'2025-01-30 21:14:24.863' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.757' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (189, 1, N'LineChart', N'Finance', N'Monthly Credit Trend', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, SUM(D.Credit) AS Total FROM tbl_JournalVoucherDetails D
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
     ORDER BY Name', N'{ ""xAxis"": ""Month"", ""yAxis"": ""TotalCredit"", ""lineColor"": ""#e74c3c"" }', N'0xe85d', N'#f39c12', N'midWidgets', 949, CAST(N'2025-01-30 21:14:25.217' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.757' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (190, 1, N'PieChart', N'Finance', N'Debit Distribution by Cost Center', N'SELECT A.EName AS Name, SUM(D.Debit) AS Total
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_Accounts A ON D.AccountID = A.ID
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY A.EName', N'{ ""labels"": ""CostCenter"", ""values"": ""TotalDebit"", ""pieColors"": [""#3498db"", ""#e74c3c"", ""#2ecc71""] }', N'0xe6cb', N'#e74c3c', N'midWidgets', -1638, CAST(N'2025-01-30 21:14:25.607' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (191, 1, N'Table', N'Finance', N'Recent Journal Entries', N'SELECT TOP 10 JVNumber, VoucherDate, BranchID, CostCenterID 
     FROM tbl_JournalVoucherHeader 
     ORDER BY VoucherDate DESC', N'{ ""columns"": [""JVNumber"", ""VoucherDate"", ""BranchID"", ""CostCenterID""] }', N'0xf128', N'#1abc9c', N'midWidgets', 461, CAST(N'2025-01-30 21:14:26.730' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (192, 1, N'BarChart', N'Finance', N'Monthly Debit vs Credit', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, 
            SUM(D.Debit) AS Total, 
            SUM(D.Credit) AS TotalCredit
     FROM tbl_JournalVoucherDetails D
     JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
     GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
     ORDER BY Name', N'{ ""xAxis"": ""Month"", ""yAxis"": [""TotalDebit"", ""TotalCredit""], ""barColors"": [""#1abc9c"", ""#e74c3c""] }', N'0xe85d', N'#f39c12', N'midWidgets', 1737, CAST(N'2025-01-30 21:14:27.093' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.757' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (193, 1, N'BarChart', N'Finance', N'Best Performing Accounts by Credit', N'SELECT TOP 5 A.EName AS Name, SUM(D.Credit) AS Total 
      FROM tbl_JournalVoucherDetails D
      JOIN tbl_Accounts A ON D.AccountID = A.ID 
      GROUP BY A.EName 
      ORDER BY SUM(D.Credit) DESC', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""barColor"": ""#e67e22"" }', N'0xe85d', N'#f39c12', N'midWidgets', -1338, CAST(N'2025-01-30 21:14:27.467' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.743' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (194, 1, N'KPI', N'Finance', N'Account Balances Overview', N'SELECT SUM(Debit) - SUM(Credit) AS Total, ''Account Balance'' AS Name
      FROM tbl_JournalVoucherDetails', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', -2032, CAST(N'2025-01-30 21:14:28.410' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (195, 1, N'LineChart', N'Finance', N'Monthly Expense Trend', N'SELECT FORMAT(H.VoucherDate, ''yyyy-MM'') AS Name, SUM(D.Debit) AS Total
      FROM tbl_JournalVoucherDetails D
      JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
      GROUP BY FORMAT(H.VoucherDate, ''yyyy-MM'')
      ORDER BY Name', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""lineColor"": ""#e74c3c"" }', N'0xf0d6', N'#c0392b', N'midWidgets', 1343, CAST(N'2025-01-30 21:14:28.777' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.757' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (196, 1, N'KPI', N'Finance', N'Total Revenue', N'SELECT SUM(TotalInvoice) AS Total, ''Revenue'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf155', N'#2ecc71', N'rightWidgets', -1208, CAST(N'2025-01-30 21:14:29.150' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (197, 1, N'KPI', N'Finance', N'Total Expenses', N'SELECT SUM(Debit) AS Total, ''Expenses'' AS Name FROM tbl_JournalVoucherDetails', NULL, N'0xf0d6', N'#c0392b', N'leftWidgets', -425, CAST(N'2025-01-30 21:14:29.923' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (198, 1, N'KPI', N'Finance', N'Profit Margin', N'SELECT (SUM(TotalInvoice) - SUM(Debit)) AS Total, ''Profit'' AS Name FROM tbl_InvoiceHeader I 
      JOIN tbl_JournalVoucherDetails J ON I.CompanyID = J.CompanyID', NULL, N'0xf0e7', N'#8e44ad', N'rightWidgets', -796, CAST(N'2025-01-30 21:14:31.030' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (199, 1, N'KPI', N'Finance', N'Month Gross Profit Margin', N'select (q.totalRevenue-q.totalCost)/ q.totalRevenue *100 as Total,



    CASE 
        WHEN COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/ q.totalRevenuePastMonth *100 ), 0) = 0 THEN NULL
        ELSE (COALESCE(((q.totalRevenue-q.totalCost)/ q.totalRevenue *100 ), 0) - COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/ q.totalRevenuePastMonth *100 ), 0)) * 100.0 / COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/ q.totalRevenuePastMonth *100 ), 1)
    END AS PercentageChange
 
 
 
  from (select 

( select isnull(SUM(Total * -1),0) AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
		 
    ) 
    AND YEAR(DueDate) = YEAR(GETDATE()) 
    AND MONTH(DueDate) = MONTH(GETDATE())) as totalRevenue 
	,
	( select isnull(SUM(Total  ),0) AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
       
		 
    ) 
    AND YEAR(DueDate) = YEAR(GETDATE()) 
    AND MONTH(DueDate) = MONTH(GETDATE())) as totalCost,
	( select isnull( SUM(Total * -1) ,0) AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
        UNION ALL
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
		 
    ) 
     AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
	) as totalRevenuePastMonth 
	,
	( select isnull(SUM(Total  ) ,0)AS Totalrevenue   FROM tbl_JournalVoucherDetails 
    WHERE AccountID IN (
        SELECT TOP 1 AccountID 
        FROM tbl_AccountSetting 
        WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
        ORDER BY CreationDate DESC
       
		 
    ) 
      AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
	) as totalCostPastMonth) as q', NULL, N'0xf0e7', N'#8e44ad', N'leftWidgets', -2238, CAST(N'2025-01-30 21:14:31.463' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.733' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (200, 1, N'KPI', N'Finance', N'Inventory Turnover Rate/YTD', N'SELECT 
    (q.totalCost * -1) / 
    NULLIF((q.totalStock - q.totalStockPastYear) / 2, 0) AS Total,
    
    (q.totalCostPastYear * -1) / 
    NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0) AS TotalPast,

    CASE 
        WHEN COALESCE(
            (q.totalCostPastYear * -1) / 
            NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0), 
        0) = 0 
        THEN NULL
        ELSE 
            (COALESCE(
                (q.totalCost * -1) / 
                NULLIF((q.totalStock - q.totalStockPastYear) / 2, 0), 
            0) 
            - 
            COALESCE(
                (q.totalCostPastYear * -1) / 
                NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0), 
            0)
            ) * 100.0 / 
            COALESCE(
                (q.totalCostPastYear * -1) / 
                NULLIF((q.totalStockPastYear - q.totalStockPastYearOpenning) / 2, 0), 
            1)
    END AS PercentageChange
FROM 
(
    SELECT 
        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) = YEAR(GETDATE())) AS totalCost,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) AS totalCostPastYear,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 8 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         )) AS totalStock,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 8 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) <= YEAR(DATEADD(YEAR, -1, GETDATE()))) AS totalStockPastYear,

        (SELECT ISNULL(SUM(Total * -1), 0) 
         FROM tbl_JournalVoucherDetails 
         WHERE AccountID IN (
            SELECT TOP 1 AccountID 
            FROM tbl_AccountSetting 
            WHERE AccountRefID = 8 AND AccountID > 0 AND Active = 1 
            ORDER BY CreationDate DESC
         ) 
         AND YEAR(DueDate) <= YEAR(DATEADD(YEAR, -2, GETDATE()))) AS totalStockPastYearOpenning
) AS q;
', NULL, N'0xe8cc', N'#1abc9c', N'rightWidgets', -1826, CAST(N'2025-01-30 21:14:31.830' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (201, 1, N'PieChart', N'Customers', N'Top 5 Customers by Invoice Amount', N'SELECT TOP 5 BP.EName AS Name, SUM(I.TotalInvoice) AS Total
      FROM tbl_InvoiceHeader I
      JOIN tbl_BusinessPartner BP ON I.BusinessPartnerID = BP.ID
      GROUP BY BP.EName
      ORDER BY SUM(I.TotalInvoice) DESC', N'{ ""labels"": ""Name"", ""values"": ""Total"", ""pieColors"": [""#1abc9c"", ""#3498db"", ""#9b59b6"", ""#f1c40f"", ""#e74c3c""] }', N'0xf007', N'#e67e22', N'midWidgets', 255, CAST(N'2025-01-30 21:14:33.637' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.753' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (202, 1, N'KPI', N'Customers', N'Active Customers', N'SELECT COUNT(*) AS Total, ''Active Customers'' AS Name FROM tbl_BusinessPartner WHERE Active = 1', NULL, N'0xf007', N'#e67e22', N'leftWidgets', -13, CAST(N'2025-01-30 21:14:34.043' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (203, 1, N'BarChart', N'Customers', N'Top 10 Customers by Revenue', N'SELECT Top 10
    SUM(q.Total) AS Total,
   tbl_BusinessPartner.AName  Name
FROM (
    SELECT 
        TotalInvoice AS Total, 
        tbl_InvoiceHeader.BusinessPartnerID AS BusinessPartnerID 
    FROM tbl_InvoiceHeader 
    WHERE InvoiceTypeID in (3,10) and YEAR( tbl_InvoiceHeader.InvoiceDate)= Year(GETDATE()) 

    UNION ALL 

    SELECT 
        TotalAmount AS Total, 
        BusinessPartnerID AS BusinessPartnerID 
    FROM tbl_FinancingHeader where YEAR( tbl_FinancingHeader.VoucherDate)= Year(GETDATE()) 
) AS q left join tbl_BusinessPartner on tbl_BusinessPartner.id = q.BusinessPartnerID
GROUP BY q.BusinessPartnerID ,
   tbl_BusinessPartner.AName
ORDER BY Total DESC;', NULL, N'0xf155', N'#2ecc71', N'midWidgets', -44, CAST(N'2025-01-30 21:14:34.830' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.753' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (204, 1, N'KPI', N'Customers', N'Customer Retention Rate', N'SELECT 
    q.Total, 
   
    CASE 
        WHEN q.TotalPastMonth = 0 THEN NULL 
        ELSE CAST(q.Total AS DECIMAL(18,2)) *100 / CAST(q.TotalPastMonth AS DECIMAL(18,2)) 
    END AS PercentageChange
FROM (
    SELECT 
        (SELECT COUNT(id) 
         FROM tbl_BusinessPartner 
         WHERE [type] = 1 
           AND Active = 1  
           AND YEAR(CreationDate) = YEAR(GETDATE()) 
           AND MONTH(CreationDate) = MONTH(GETDATE())
        ) AS Total,

        (SELECT COUNT(id) 
         FROM tbl_BusinessPartner 
         WHERE [type] = 1 
           AND Active = 1  
           AND YEAR(CreationDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) 
           AND MONTH(CreationDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
        ) AS TotalPastMonth
) AS q;
', NULL, N'0xf128', N'#1abc9c', N'leftWidgets', -2009, CAST(N'2025-01-30 21:14:35.460' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.737' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (205, 1, N'LineChart', N'Sales', N'Monthly Sales Trend', N'SELECT FORMAT(InvoiceDate, ''yyyy-MM'') AS Name, SUM(TotalInvoice) AS Total
      FROM tbl_InvoiceHeader 
      GROUP BY FORMAT(InvoiceDate, ''yyyy-MM'')
      ORDER BY Name', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""lineColor"": ""#2ecc71"" }', N'0xf080', N'#2980b9', N'midWidgets', -1038, CAST(N'2025-01-30 21:14:36.960' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.743' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (206, 1, N'BarChart', N'Sales', N'Branch-wise Sales', N'SELECT B.EName AS Name, SUM(I.TotalInvoice) AS Total 
      FROM tbl_InvoiceHeader I
      JOIN tbl_Branch B ON I.BranchID = B.ID
      GROUP BY B.EName
      ORDER BY SUM(I.TotalInvoice) DESC', N'{ ""xAxis"": ""Name"", ""yAxis"": ""Total"", ""barColor"": ""#3498db"" }', N'0xf080', N'#2980b9', N'midWidgets', -644, CAST(N'2025-01-30 21:14:37.790' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.753' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (207, 1, N'Table', N'Sales', N'Recent Transactions', N'SELECT TOP 10 InvoiceNo AS Name, TotalInvoice AS Total
      FROM tbl_InvoiceHeader 
      ORDER BY InvoiceDate DESC', N'{ ""columns"": [""Name"", ""Total""] }', N'0xf07a', N'#16a085', N'leftWidgets', -1780, CAST(N'2025-01-30 21:14:38.213' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.737' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (208, 1, N'KPI', N'Sales', N'Avg Sales per Customer', N'SELECT AVG(TotalInvoice) AS Total, ''Avg Sale'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf080', N'#2980b9', N'leftWidgets', 192, CAST(N'2025-01-30 21:14:38.577' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (209, 1, N'KPI', N'Sales', N'Highest Sale', N'SELECT MAX(TotalInvoice) AS Total, ''Highest Sale'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', -178, CAST(N'2025-01-30 21:14:38.993' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.763' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (210, 1, N'KPI', N'Sales', N'Lowest Sale', N'SELECT MIN(TotalInvoice) AS Total, ''Lowest Sale'' AS Name FROM tbl_InvoiceHeader', NULL, N'0xf128', N'#1abc9c', N'leftWidgets', -219, CAST(N'2025-01-30 21:14:39.853' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (211, 1, N'KPI', N'Sales', N'Pending Invoices', N'SELECT COUNT(*) AS Total, ''Pending Invoices'' AS Name FROM tbl_InvoiceHeader WHERE Status = 0', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', -384, CAST(N'2025-01-30 21:14:40.490' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.763' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (212, 1, N'BarChart', N'Sales', N'Monthly Sales Revenue by Product Category', N' 
select  tbl_ItemsCategory.AName Name ,sum(TotalLine) Total
 from tbl_InvoiceDetails 
 left join tbl_InvoiceHeader on tbl_InvoiceHeader.Guid = tbl_InvoiceDetails.HeaderGuid
 left join tbl_Items on tbl_Items.Guid = tbl_InvoiceDetails.ItemGuid
left join tbl_ItemsCategory on tbl_ItemsCategory.ID = tbl_Items.CategoryID

where 
tbl_InvoiceHeader.InvoiceTypeID = 3 and 
    YEAR(tbl_InvoiceHeader.InvoiceDate) = YEAR(DATEADD(MONTH, 0, GETDATE())) 
    AND MONTH(tbl_InvoiceHeader.InvoiceDate) = MONTH(DATEADD(MONTH, 0, GETDATE()))
group by tbl_ItemsCategory.AName

 

 ', NULL, N'0xf155', N'#2ecc71', N'midWidgets', -344, CAST(N'2025-01-30 21:14:41.040' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.753' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (213, 1, N'BarChart', N'Sales', N'Product Return Rate by Category.', N' 
select  tbl_ItemsCategory.AName Name ,sum(TotalLine) Total
 from tbl_InvoiceDetails 
 left join tbl_InvoiceHeader on tbl_InvoiceHeader.Guid = tbl_InvoiceDetails.HeaderGuid
 left join tbl_Items on tbl_Items.Guid = tbl_InvoiceDetails.ItemGuid
left join tbl_ItemsCategory on tbl_ItemsCategory.ID = tbl_Items.CategoryID

where 
tbl_InvoiceHeader.InvoiceTypeID = 4 and 
    YEAR(tbl_InvoiceHeader.InvoiceDate) = YEAR(DATEADD(MONTH, 0, GETDATE())) 
    AND MONTH(tbl_InvoiceHeader.InvoiceDate) = MONTH(DATEADD(MONTH, 0, GETDATE()))
group by tbl_ItemsCategory.AName', NULL, N'0xf128', N'#1abc9c', N'midWidgets', -2238, CAST(N'2025-01-30 21:14:42.137' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (214, 1, N'BarChart', N'Sales', N'Outstanding Invoices by Age (0-30, 30-60, 60+ Days).', N'WITH ReconciledAmounts AS (
    SELECT 
        JVDetailsGuid,
        SUM(Amount) AS Reconciled
    FROM 
        tbl_Reconciliation 
    GROUP BY 
        JVDetailsGuid
),
AgingBuckets AS (
    SELECT 
        CASE 
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) BETWEEN 0 AND 30 THEN ''0-30 Days''
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) BETWEEN 31 AND 60 THEN ''31-60 Days''
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) > 60 THEN ''60+ Days''
        END AS Name,
        total - ISNULL(R.Reconciled, 0) AS Amount
    FROM 
        tbl_JournalVoucherDetails JVD
    LEFT JOIN 
        tbl_BusinessPartner BP ON BP.ID = JVD.SubAccountID
    LEFT JOIN 
        ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE 
        DueDate <= GETDATE() and SubAccountID>0 and  accountid =
		 (	select top 1 AccountID from tbl_AccountSetting where AccountRefID = 7order by CreationDate desc)
)
SELECT 
    Name,
    SUM(Amount) AS Total
FROM 
    AgingBuckets
GROUP BY 
    Name;


', NULL, N'0xf128', N'#1abc9c', N'midWidgets', -1938, CAST(N'2025-01-30 21:14:42.763' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.740' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (215, 1, N'KPI', N'Warehouse', N'Total Products', N'SELECT COUNT(*) AS Total, ''Products'' AS Name FROM tbl_Items', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', -1620, CAST(N'2025-01-30 21:14:44.443' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (216, 1, N'KPI', N'Admin', N'Total Branches', N'SELECT COUNT(*) AS Total, ''Branches'' AS Name FROM tbl_Branch', NULL, N'0xf19c', N'#f1c40f', N'leftWidgets', -1066, CAST(N'2025-01-30 21:14:45.930' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.737' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (217, 1, N'KPI', N'Admin', N'Total Transactions', N'SELECT COUNT(*) AS Total, ''Transactions'' AS Name FROM tbl_CashVoucherHeader', NULL, N'0xf07a', N'#16a085', N'rightWidgets', -1414, CAST(N'2025-01-30 21:14:46.390' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (218, 1, N'KPI', N'Admin', N'Active Users', N'SELECT COUNT(*) AS Total, ''Users'' AS Name FROM tbl_employee WHERE IsSystemUser = 1', NULL, N'0xf128', N'#1abc9c', N'leftWidgets', -1272, CAST(N'2025-01-30 21:14:46.823' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.737' AS DateTime), 1, @companyID, 1)

INSERT [dbo].[tbl_DashboardWidgets] ([ID], [UserId], [WidgetType], [GroupName], [Title], [SQLQuery], [ChartConfig], [Icon], [Color], [SectionName], [SectionIndex], [CreationDate], [CreationUserID], [ModificationDate], [ModificationUserID], [CompanyID], [IsActive]) VALUES (219, 1, N'KPI', N'Admin', N'Total Cost Centers', N'SELECT COUNT(*) AS Total, ''Cost Centers'' AS Name FROM tbl_CostCenter', NULL, N'0xf128', N'#1abc9c', N'rightWidgets', -590, CAST(N'2025-01-30 21:14:47.260' AS DateTime), 1, CAST(N'2025-01-30 21:20:30.760' AS DateTime), 1, @companyID, 1)

SET IDENTITY_INSERT [dbo].[tbl_DashboardWidgets] OFF
delete from tbl_DashboardWidgets where userid >0
";
                    ClsSQL.ExecuteQueryStatement(a, ClsSQL.CreateDataBaseConnectionString(CompanyId));
                    string b = @"
SET IDENTITY_INSERT [dbo].[tbl_BusinessPartnerType] ON


-- Insert Customer if not exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_BusinessPartnerType] WHERE [ID] = 1)
BEGIN
    INSERT INTO [dbo].[tbl_BusinessPartnerType] ([ID], [AName], [EName]) 
    VALUES (1, N'عميل', N'Customer')
END


-- Insert Vendor if not exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_BusinessPartnerType] WHERE [ID] = 2)
BEGIN
    INSERT INTO [dbo].[tbl_BusinessPartnerType] ([ID], [AName], [EName]) 
    VALUES (2, N'مورد', N'Vendor')
END


SET IDENTITY_INSERT [dbo].[tbl_BusinessPartnerType] OFF
"; 
                    
                    ClsSQL.ExecuteQueryStatement(b, ClsSQL.CreateDataBaseConnectionString(CompanyId));
                    InsertDataBaseVersion(Simulate.decimal_(2.0), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.1))
                {

                    ClsSQL.ExecuteQueryStatement("delete from tbl_DashboardWidgets where Title like 'Month Gross Profit Margin' and userid>0", ClsSQL.CreateDataBaseConnectionString(CompanyId));
                    ClsSQL.ExecuteQueryStatement("update  tbl_DashboardWidgets set SQLQuery='select (q.totalRevenue-q.totalCost)/ (case when q.totalRevenue= 0 then 1 else q.totalRevenue end ) *100 as Total,\r\n\r\n\r\n\r\n    CASE \r\n        WHEN COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)\r\n\t\t/   (case when q.totalRevenuePastMonth= 0 then 1 else q.totalRevenuePastMonth end )    *100 ), 0) = 0 THEN 1\r\n        ELSE (COALESCE(((q.totalRevenue-q.totalCost)/ (case when q.totalRevenue= 0 then 1 else q.totalRevenue end ) *100 ), 0) -\r\n\t\t COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/  \r\n\t\t (case when q.totalRevenuePastMonth= 0 then 1 else q.totalRevenuePastMonth end )    *100 ), 0)) * 100.0 \r\n\t\t \r\n\t\t / \r\n\t\t COALESCE( ((q.totalRevenuePastMonth-q.totalCostPastMonth)/ (case when q.totalRevenuePastMonth= 0 then 1 else q.totalRevenuePastMonth end )   *100 ), 1)\r\n    END AS PercentageChange\r\n \r\n \r\n \r\n  from (select \r\n\r\n( select isnull(SUM(Total * -1),0) AS Totalrevenue   FROM tbl_JournalVoucherDetails \r\n    WHERE AccountID IN (\r\n        SELECT TOP 1 AccountID \r\n        FROM tbl_AccountSetting \r\n        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 \r\n        ORDER BY CreationDate DESC\r\n        UNION ALL\r\n        SELECT TOP 1 AccountID \r\n        FROM tbl_AccountSetting \r\n        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 \r\n        ORDER BY CreationDate DESC\r\n\t\t \r\n    ) \r\n    AND YEAR(DueDate) = YEAR(GETDATE()) \r\n    AND MONTH(DueDate) = MONTH(GETDATE())) as totalRevenue \r\n\t,\r\n\t( select isnull(SUM(Total  ),0) AS Totalrevenue   FROM tbl_JournalVoucherDetails \r\n    WHERE AccountID IN (\r\n        SELECT TOP 1 AccountID \r\n        FROM tbl_AccountSetting \r\n        WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 \r\n        ORDER BY CreationDate DESC\r\n       \r\n\t\t \r\n    ) \r\n    AND YEAR(DueDate) = YEAR(GETDATE()) \r\n    AND MONTH(DueDate) = MONTH(GETDATE())) as totalCost,\r\n\t( select isnull( SUM(Total * -1) ,0) AS Totalrevenue   FROM tbl_JournalVoucherDetails \r\n    WHERE AccountID IN (\r\n        SELECT TOP 1 AccountID \r\n        FROM tbl_AccountSetting \r\n        WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1 \r\n        ORDER BY CreationDate DESC\r\n        UNION ALL\r\n        SELECT TOP 1 AccountID \r\n        FROM tbl_AccountSetting \r\n        WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1 \r\n        ORDER BY CreationDate DESC\r\n\t\t \r\n    ) \r\n     AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) \r\n    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))\r\n\t) as totalRevenuePastMonth \r\n\t,\r\n\t( select isnull(SUM(Total  ) ,0)AS Totalrevenue   FROM tbl_JournalVoucherDetails \r\n    WHERE AccountID IN (\r\n        SELECT TOP 1 AccountID \r\n        FROM tbl_AccountSetting \r\n        WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1 \r\n        ORDER BY CreationDate DESC\r\n       \r\n\t\t \r\n    ) \r\n      AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE())) \r\n    AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))\r\n\t) as totalCostPastMonth) as q'  where Title like 'Month Gross Profit Margin' ", ClsSQL.CreateDataBaseConnectionString(CompanyId));


                    InsertDataBaseVersion(Simulate.decimal_(2.1), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.2))
                {

                    CreateTable("tbl_attachemnts", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_attachemnts", "TransactionId", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_attachemnts", "FormName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_attachemnts", "FileName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_attachemnts", "FileType", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_attachemnts", "FileData", SQLColumnDataType.varbinarymax);
                    AddColumnToTable(CompanyId, "tbl_attachemnts", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_attachemnts", "CreationUserID", SQLColumnDataType.Integer);
                    InsertDataBaseVersion(Simulate.decimal_(2.2), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.3))
                {
                    clsForms.DeleteFormByID(84, CompanyId);
                    clsForms.DeleteFormByID(85, CompanyId);
                    clsForms.DeleteFormByID(86, CompanyId);
                    clsForms.DeleteFormByID(87, CompanyId);
                    DropTable("tbl_CreditNoteHeader", CompanyId);
                    DropTable("tbl_CreditNoteDetails", CompanyId);
                    CreateTable("tbl_CreditNoteHeader", CompanyId);
                    DropColumn("tbl_CreditNoteHeader", "ID", CompanyId);
                    clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(20, "اشعار دائن", "Credit Note", 0, CompanyId);
                    clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(21, "اشعار مدين", "Debit Note", 0, CompanyId);  
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "CompanyID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "Guid", SQLColumnDataType.guid,0,true);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "VoucherDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "CostCenterID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "AccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "SubAccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "Amount", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "Note", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "JVGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "VoucherNo", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "VoucherType", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "CreationUserID", SQLColumnDataType.Integer);
                     AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteHeader", "DueDate", SQLColumnDataType.DateTime);
                    CreateTable("tbl_CreditNoteDetails", CompanyId);
                    DropColumn("tbl_CreditNoteDetails", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "Guid", SQLColumnDataType.guid,0, true);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "RowIndex", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "HeaderGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "AccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "SubAccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "CostCenterID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "Debit", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "Credit", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "Total", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "Note", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "VoucherType", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CreditNoteDetails", "ModificationDate", SQLColumnDataType.DateTime);
                    clsForms.InsertForm(84, "Debit Note Main", "اشعار مدين رئيسيه ", "Debit Note Main", 8, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(85, "Debit Note Add", "اشعار مدين اضافه ", "Debit Note Add", 8, true, true, true, true, true, true, CompanyId);
                    clsForms.InsertForm(86, "Credit Note Main", "اشعار دائن رئيسيه ", "Credit Note Main", 8, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(87, "Credit Note Add", "اشعار دائن اضافه ", "Credit Note Add", 8, true, true, true, true, true, true, CompanyId);

                    DropTable("tbl_CustomReportsStrucuture", CompanyId);
                    CreateTable("tbl_CustomReportsStrucuture", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "PageName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "ReportName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "RowIndex",SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "Index", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "Type", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "Parameter", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "OtherValue", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "Font", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "FontSize", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "FontWeight", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "WithBoarder", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "HorizontalAlignment", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "VerticalAlignment", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "FontColor", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "BackColor", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "Height", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "Width", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "CreationDate", SQLColumnDataType.DateTime);
                    InsertDataBaseVersion(Simulate.decimal_(2.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.4))
                {
                    AddColumnToTable(CompanyId, "tbl_CustomReportsStrucuture", "widgetIndex", SQLColumnDataType.Integer);
                    InsertDataBaseVersion(Simulate.decimal_(2.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.5))
                {
                    AddColumnToTable(CompanyId, "tbl_FinancingHeader", "InvoiceHeaderGuid", SQLColumnDataType.guid);
                    

                 clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(22, "فاتوره شراء تابعه لتمويل", "PurchaseInvoiceFromFinancing", 1, CompanyId);


                    InsertDataBaseVersion(Simulate.decimal_(2.5), CompanyId);
                }
                #endregion //OldVersions
                if (versionNumber < Simulate.decimal_(2.6))
                {
                    AddColumnToTable(CompanyId, "tbl_FinancingHeader", "PurchaseInvoiceRefNumber", SQLColumnDataType.VarChar);


 

                    InsertDataBaseVersion(Simulate.decimal_(2.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.7))
                {
                    CreateTable("tbl_POSScaleConfiguration", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "ScaleName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "ScaleType", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "ConnectionType", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "PortName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "BaudRate", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "DataBits", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "Parity", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "StopBits", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "BarcodePrefix", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "AutoDetect", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "DefaultPrintType", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "Status", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "ModificationUserID", SQLColumnDataType.Integer);


                    InsertDataBaseVersion(Simulate.decimal_(2.7), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.8))
                {
                    clsForms.InsertForm(88, "POSScaleMain", "اعدادات موازين نقاط البيع", "POS Scale Main", 52, true, true, false, false, false, false, CompanyId);
                    clsForms.InsertForm(89, "POSScaleAdd", "اضافه ميزان", "POS Scale Add", 52, true, true, true, true, true, true, CompanyId);



                    InsertDataBaseVersion(Simulate.decimal_(2.8), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(2.9))
                {
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "SKULength", SQLColumnDataType.Integer);


 

                    InsertDataBaseVersion(Simulate.decimal_(2.9), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(3.0))
                {
                    AddColumnToTable(CompanyId, "tbl_POSScaleConfiguration", "Divisionfactor", SQLColumnDataType.Integer);




                    InsertDataBaseVersion(Simulate.decimal_(3.0), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.1))
                {
                    DropTable("tbl_InvoiceDetailsLotsTracking", CompanyId);
                    CreateTable("tbl_InvoiceDetailsLotsTracking", CompanyId);
                    DeleteColumnFromTable("tbl_InvoiceDetailsLotsTracking", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "Guid", SQLColumnDataType.guid,0,true);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "InvoiceDetailsGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "ItemGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "InvoiceType", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "InvoiceGuid", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "LotNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "ExpiryDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "QTY", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "CreationDate", SQLColumnDataType.DateTime);
                    DropTable("tbl_InvoiceDetailsLotsSerialNumber", CompanyId);
                    CreateTable("tbl_InvoiceDetailsLotsSerialNumber", CompanyId);
                    DeleteColumnFromTable("tbl_InvoiceDetailsLotsSerialNumber", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "Guid", SQLColumnDataType.guid, 0, true);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "InvoiceDetailsGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "ItemGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "InvoiceType", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "InvoiceGuid", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "LotID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "SerialNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "Status", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "CreationDate", SQLColumnDataType.DateTime);
                    InsertDataBaseVersion(Simulate.decimal_(3.1), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.2))
                {
                  
                     

                    AddColumnToTable(CompanyId, "tbl_Items", "TrackLot", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_Items", "TrackSerial", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_Items", "TrackExpiryDate", SQLColumnDataType.Bit);

                    InsertDataBaseVersion(Simulate.decimal_(3.2), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.3))
                {

          
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "TrackLot", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "TrackSerial", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "TrackExpiryDate", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "LotDetails", SQLColumnDataType.VarChar);


                    InsertDataBaseVersion(Simulate.decimal_(3.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.4))
                {
                     
                    DeleteColumnFromTable("tbl_InvoiceDetailsLotsTracking", "InvoiceGuid", CompanyId);
           
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsTracking", "InvoiceGuid", SQLColumnDataType.guid);

                    
                    
                    InsertDataBaseVersion(Simulate.decimal_(3.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.5))
                {

                    DeleteColumnFromTable("tbl_InvoiceDetailsLotsSerialNumber", "InvoiceGuid", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "InvoiceGuid", SQLColumnDataType.guid);



                    InsertDataBaseVersion(Simulate.decimal_(3.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.6))
                {

                    DeleteColumnFromTable("tbl_InvoiceDetailsLotsSerialNumber", "LotID", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_InvoiceDetailsLotsSerialNumber", "LotGuid", SQLColumnDataType.guid);



                    InsertDataBaseVersion(Simulate.decimal_(3.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.7))
                {
                    CreateTable("tbl_EInvoiceConfigurations",CompanyId);
 

                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "Country", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "UserCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "SecretKey", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "Active", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_InvoiceHeader", "IsPosted", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_FinancingHeader", "IsPosted", SQLColumnDataType.Bit);
                    CreateTable("tbl_EInvoiceTransactions", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceTransactions", "VoucherType", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceTransactions", "InvoiceGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceTransactions", "FinancingGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceTransactions", "Response", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceTransactions", "EInvoiceConfigurationsID", SQLColumnDataType.VarChar);
                    InsertDataBaseVersion(Simulate.decimal_(3.7), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.8))
                {
                    clsForms.InsertForm(90, "EInvoiceConfigurationsMain", "اعدادات نظام الفوتره الحكومي", "EInvoiceConfigurations Main", 52, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(91, "EInvoiceConfigurationsAdd", "اضافه ربط نظام الفوتره", "EInvoiceConfigurations Add", 52, true, true, true, true, true, true, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(3.8), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(3.9))
                {
                    
                    AddColumnToTable(CompanyId, "tbl_InvoiceHeader", "EInvoiceQRCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_FinancingHeader", "EInvoiceQRCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "ActivityNumber", SQLColumnDataType.VarChar);

                    InsertDataBaseVersion(Simulate.decimal_(3.9), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(4.0))
                {
 
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "TaxNumber", SQLColumnDataType.VarChar);

                    InsertDataBaseVersion(Simulate.decimal_(4.0), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.1))
                {

                    clsForms.InsertForm(92, "EInvoicePosting", "ترحيل نظام الفوتره", "EInvoicePosting", 52, true, false, false, false, false, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(4.1), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.2))
                {
                    AddColumnToTable(CompanyId, "tbl_BusinessPartner", "BankName", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_BusinessPartner", "BankAccountNumber", SQLColumnDataType.VarChar);
                    InsertDataBaseVersion(Simulate.decimal_(4.2), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.3))
                {
                    AddColumnToTable(CompanyId, "tbl_Accounts", "IsSubLedger", SQLColumnDataType.Bit);

               
                    InsertDataBaseVersion(Simulate.decimal_(4.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.4))
                {
                    clsForms.InsertForm(93, "CustomerLoansSummary", "تقرير ملخص العميل", "CustomerLoansSummary", 54, true, false, false, false, false, false, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(4.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.5))
                {
                    clsForms.InsertForm(94, "HRMainPage", "الموارد البشريه الرئيسيه", "HRMainPage", 54, true, false, false, false, false, false, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(4.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.6))
                {
                    AddColumnToTable(CompanyId, "tbl_employee", "EmployeeCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "Tel2", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "Address", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "CountryID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_employee", "CityID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_employee", "NationalityID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_employee", "NationalNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "IDNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "IDIssueDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_employee", "IDExpireDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_employee", "PassportNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "PassportIssueDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_employee", "PassportExpireDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_employee", "EducationalLevelID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_employee", "HireDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_employee", "BankName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "IBAN", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "SWIFTCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "BankAccountNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "SocialSecurityNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "SocialSecurityProgramID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_employee", "MedicalInsuranceNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_employee", "MedicalInsuranceProgramID", SQLColumnDataType.Integer);
                    CreateTable("tbl_JobTitle", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_JobTitle", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_JobTitle", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_JobTitle", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_JobTitle", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_JobTitle", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_JobTitle", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_JobTitle", "ModificationDate", SQLColumnDataType.DateTime);
                    CreateTable("tbl_Department", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_Department", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Department", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Department", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Department", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Department", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_Department", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Department", "ModificationDate", SQLColumnDataType.DateTime);
                    CreateTable("tbl_Position", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_Position", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Position", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Position", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Position", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Position", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_Position", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Position", "ModificationDate", SQLColumnDataType.DateTime);



                    InsertDataBaseVersion(Simulate.decimal_(4.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.7))
                {
                    AddColumnToTable(CompanyId, "tbl_Countries", "NationalityAName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Countries", "NationalityEName", SQLColumnDataType.VarChar);



                    InsertDataBaseVersion(Simulate.decimal_(4.7), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(4.8))
                {
                    CreateTable("tbl_City", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_City", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_City", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_City", "CountryID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_City", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_City", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_City", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_City", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_City", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(4.8), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(4.9))
                {
                    clsForms.InsertForm(95, "CityMainPage", "المدن", "CityMainPage", 54, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(96, "CityPageADD", "المدن", "CityPageADD", 54, true, true, true, true, true, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(4.9), CompanyId);
                }
                
                if (versionNumber < Simulate.decimal_(5.0))
                {

                    clsForms.InsertForm(97, "DepartmentMainPage", "الاقسام الرئيسيه", "DepartmentMainPage", 54, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(98, "DepartmentPageADD", "اضافه الااقسام", "DepartmentPageADD", 54, true, true, true, true, true, false, CompanyId);


                    clsForms.InsertForm(99, "JobTitleMainPage", "المسميات الوظيفيه الرئيسيه", "JobTitleMainPage", 54, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(100, "JobTitlePageADD", "اضافه مسميات وظيفيه", "JobTitlePageADD", 54, true, true, true, true, true, false, CompanyId);


                    clsForms.InsertForm(101, "HRContractTypeMainPage", "انواع عقود الموظفين الرئيسيه", "HRContractTypeMainPage", 54, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(102, "HRContractTypePageADD", "اضافه انوع عقود موظفين", "HRContractTypePageADD", 54, true, true, true, true, true, false, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(5.0), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(5.1))
                {
                    CreateTable("tbl_HRContractType", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_HRContractType", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_HRContractType", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_HRContractType", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_HRContractType", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_HRContractType", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_HRContractType", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_HRContractType", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(5.1), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(5.2))
                {
                    // ===============================
                    // Salary Elements Master Table
                    // ===============================
                    CreateTable("tbl_SalariesElements", CompanyId);

                    // -------------------------------
                    // Basic Information
                    // -------------------------------
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "Code", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "EName", SQLColumnDataType.VarChar);

                    // -------------------------------
                    // Classification
                    // -------------------------------
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "ElementTypeID", SQLColumnDataType.Integer);     // Earning / Deduction / Contribution
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "SalariesElementCategoryID", SQLColumnDataType.Integer);        // Basic / Allowance / Overtime / Tax / Loan

                    // -------------------------------
                    // Calculation Method
                    // -------------------------------
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "CalcTypeID", SQLColumnDataType.Integer);        // Fixed / Percentage / Formula
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "DefaultValue", SQLColumnDataType.Decimal);      // Default entry
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "PercentageOfElementID", SQLColumnDataType.Integer); // FK to another element
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "FormulaText", SQLColumnDataType.VarChar);    // Formula expression

                    // -------------------------------
                    // Payroll Flags
                    // -------------------------------
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "IsTaxable", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "IsAffectSocialSecurity", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "IsRecurring", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "IsSystemElement", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "IsEditable", SQLColumnDataType.Bit);

                    // -------------------------------
                    // Effective Dating (Version Control)
                    // -------------------------------
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "StartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "EndDate", SQLColumnDataType.DateTime);

                    // -------------------------------
                    // Accounting Integration
                    // -------------------------------
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "EmployeeDebitAccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "EmployeeCreditAccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "CompanyDebitAccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "CompanyCreditAccountID", SQLColumnDataType.Integer);

                    // -------------------------------
                    // Audit + Tenant
                    // -------------------------------
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "ModificationDate", SQLColumnDataType.DateTime);

                    clsForms.InsertForm(103, "HRSalariesElementsMainPage", "عناصر الرواتب الرئيسيه", "HRSalariesElementsMainPage", 54, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(104, "HRSalariesElementsPageADD", "اضافه عناصر الرواتب", "HRSalariesElementsPageADD", 54, true, true, true, true, true, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(5.2), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(5.3))
                {
                    CreateTable("tbl_EmployeeSalaryElements", CompanyId);

          
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "EmployeeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "SalaryElementID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "CalcTypeID", SQLColumnDataType.Integer);          // Fixed / % / Formula
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "AssignedValue", SQLColumnDataType.Decimal);       // This is the fixed value or % number
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "IsCalculated", SQLColumnDataType.Bit);            // Formula result stored?
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "StartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "EndDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "ModificationDate", SQLColumnDataType.DateTime);
                    clsForms.InsertForm(105, "EmployeeSalaryElementsMainPage", "حركات الرواتب الرئيسيه", "EmployeeSalaryElementsMainPage", 54, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(106, "EmployeeSalaryElementsPageADD", "اضافه حركات الرواتب", "EmployeeSalaryElementsPageADD", 54, true, true, true, true, true, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(5.3), CompanyId);
                }
                 
                if (versionNumber < Simulate.decimal_(5.5))
                {
                    // ============================================================
                    // 1️⃣ PAYROLL PERIOD TABLE (Monthly Payroll Cycle)
                    // ============================================================
                    CreateTable("tbl_PayrollPeriod", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "PeriodAName", SQLColumnDataType.VarChar); // e.g., Jan-2025
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "PeriodEName", SQLColumnDataType.VarChar); // e.g., Jan-2025
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "StartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "EndDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "IsClosed", SQLColumnDataType.Bit);

                    // Audit
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollPeriod", "ModificationDate", SQLColumnDataType.DateTime);

                    // 📌 Forms 107–108
                    clsForms.InsertForm(107, "PayrollPeriodMainPage", "فترات الرواتب", "PayrollPeriodMainPage", 54, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(108, "PayrollPeriodAddPage", "اضافة فترة رواتب", "PayrollPeriodAddPage", 54, true, true, true, true, true, false, CompanyId);


                    // ============================================================
                    // 2️⃣ PAYROLL HEADER TABLE (One record per employee per period)
                    // ============================================================
                    CreateTable("tbl_PayrollHeader", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "PayrollPeriodID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "EmployeeID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "BasicSalary", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "TotalEarnings", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "TotalDeductions", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "NetSalary", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "Status", SQLColumnDataType.Integer); // 1=Draft, 2=Approved, 3=Posted

                    // Audit
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "ModificationDate", SQLColumnDataType.DateTime);

                    // 📌 Form 109 — Payroll Run
                    clsForms.InsertForm(109, "PayrollHeaderMainPage", "معالجة الرواتب", "PayrollHeaderMainPage", 54, true, false, false, false, false, false, CompanyId);


                    // ============================================================
                    // 3️⃣ PAYROLL DETAILS TABLE (Calculated salary elements)
                    // ============================================================
                    CreateTable("tbl_PayrollDetails", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "PayrollHeaderID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "SalaryElementID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "ElementTypeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "CalcTypeID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "AssignedValue", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "CalculatedAmount", SQLColumnDataType.Decimal);

                    // Audit
                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_PayrollDetails", "CreationDate", SQLColumnDataType.DateTime);

                    // 📌 Form 110 — Payroll Details View
                    clsForms.InsertForm(110, "PayrollDetailsViewPage", "تفاصيل الرواتب", "PayrollDetailsViewPage", 54, true, false, false, false, true, false, CompanyId);


                    // ============================================================
                    // VERSION UPDATE
                    // ============================================================
                    InsertDataBaseVersion(Simulate.decimal_(5.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(5.6))
                {
                    clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(23, "صرف رواتب", "Payroll", 0, CompanyId); 
                    InsertDataBaseVersion(Simulate.decimal_(5.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(5.7))
                {
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "IsPosted", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_SalariesElements", "SortIndex", SQLColumnDataType.Integer);
                  
                    InsertDataBaseVersion(Simulate.decimal_(5.7), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(5.8))
                {
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "PostedDate", SQLColumnDataType.DateTime);
 

                    InsertDataBaseVersion(Simulate.decimal_(5.8), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(5.9))
                {
                    AddColumnToTable(CompanyId, "tbl_PayrollHeader", "JVGuid", SQLColumnDataType.guid);


                    InsertDataBaseVersion(Simulate.decimal_(5.9), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.0))
                {
                    AddColumnToTable(CompanyId, "tbl_employee", "DepartmentID", SQLColumnDataType.Integer);


                    InsertDataBaseVersion(Simulate.decimal_(6.0), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.1))
                {
                    CreateTable("tbl_Shifts", CompanyId);

                   
                    AddColumnToTable(CompanyId, "tbl_Shifts", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "StartTime", SQLColumnDataType.Time);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "EndTime", SQLColumnDataType.Time);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "BreakMinutes", SQLColumnDataType.Integer);
           
                    AddColumnToTable(CompanyId, "tbl_Shifts", "GraceLateMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "GraceEarlyMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "ShiftType", SQLColumnDataType.Integer); // 1=Normal 2=Ramadan 3=Flexible

                    AddColumnToTable(CompanyId, "tbl_Shifts", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Shifts", "ModificationDate", SQLColumnDataType.DateTime);
                    InsertDataBaseVersion(Simulate.decimal_(6.1), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(6.2))
                {
                    CreateTable("tbl_EmployeeShift", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "EmployeeID", SQLColumnDataType.Integer);  // ⭐ Required
                    AddColumnToTable(CompanyId, "tbl_Shifts", "IsUseDetails", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "ShiftID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "StartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "EndDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "Priority", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShift", "ModificationDate", SQLColumnDataType.DateTime);
                    InsertDataBaseVersion(Simulate.decimal_(6.2), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.3))
                {
                    CreateTable("tbl_AttendanceRawPunch", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "EmployeeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "PunchTime", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "PunchType", SQLColumnDataType.Integer); // 0=IN, 1=OUT
                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "MachineID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRawPunch", "ModificationDate", SQLColumnDataType.DateTime);
                    InsertDataBaseVersion(Simulate.decimal_(6.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.4))
                {
                    CreateTable("tbl_ShiftDetails", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "ShiftID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "SegmentNo", SQLColumnDataType.Integer); // 1,2,3...
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "StartTime", SQLColumnDataType.Time);
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "EndTime", SQLColumnDataType.Time);

                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "BreakMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "IsOvernight", SQLColumnDataType.Bit); // e.g., 23:00–07:00

                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ShiftDetails", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(6.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.5))
                {
                    CreateTable("tbl_EmployeeShiftAssignment", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "EmployeeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "ShiftID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "WeekDay", SQLColumnDataType.Integer);
                    // 0=All days, 1=Mon, 2=Tue...7=Sun

                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "StartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "EndDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeShiftAssignment", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(6.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.6))
                {
                    CreateTable("tbl_AttendanceDay", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "EmployeeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "WorkDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "ShiftID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "FirstIn", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "LastOut", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "WorkedMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "LateMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "EarlyLeaveMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "OvertimeMinutes", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "StatusID", SQLColumnDataType.Integer);
                    // 1=Present, 2=Absent, 3=Leave, 4=Offday

                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceDay", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(6.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.7))
                {
                    CreateTable("tbl_AttendanceRuleHeader", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "EName", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "RuleType", SQLColumnDataType.Integer);
                    // 1 = Late
                    // 2 = EarlyLeave
                    // 3 = Absence
                    // 4 = Overtime Normal Day
                    // 5 = Overtime Weekend
                    // 6 = Overtime Holiday

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleHeader", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(6.7), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.8))
                {
                    CreateTable("tbl_AttendanceRuleDetails", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "RuleHeaderID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "FromMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "ToMinutes", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "CalcType", SQLColumnDataType.Integer);
                    // 1 = Fixed Amount
                    // 2 = Percentage
                    // 3 = Per Minute Rate
                    // 4 = Formula
                    // 5 = Convert To Salary Element Amount

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "FixedValue", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "PercentageValue", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "FormulaText", SQLColumnDataType.VarChar);
                    // Example: (BasicSalary / 30 / ShiftHours) * LateMinutes

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "SalaryElementID", SQLColumnDataType.Integer);
                    // Optional — Only if CalcType = 5

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "Priority", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleDetails", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(6.8), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(6.9))
                {
                    CreateTable("tbl_AttendanceRuleAssignment", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "RuleHeaderID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "LevelType", SQLColumnDataType.Integer);
                    // 1 = Company
                    // 2 = Department
                    // 3 = Shift
                    // 4 = Employee

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "RefID", SQLColumnDataType.Integer);
                    // companyId, departmentId, shiftId, employeeId depending on level

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "StartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "EndDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleAssignment", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(6.9), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.0))
                {
                    CreateTable("tbl_AttendanceToPayroll", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceToPayroll", "AttendanceDayID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceToPayroll", "SalaryElementID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceToPayroll", "Amount", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_AttendanceToPayroll", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceToPayroll", "CreationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(7.0), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.1))
                {
                    CreateTable("tbl_AttendanceRuleGroups", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "EName", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "CompanyID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleGroups", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(7.1), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.2))
                {
                    CreateTable("tbl_AttendanceRules", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "RuleName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "RuleGroupID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "RuleTypeID", SQLColumnDataType.Integer);
                    // 1=Late, 2=EarlyLeave, 3=Overtime, 4=Absence, 5=WeekendOT, 6=HolidayOT

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "CalculationTypeID", SQLColumnDataType.Integer);
                    // 1=Fixed, 2=Percentage, 3=RatePerHour, 4=Formula

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "Value", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "FormulaText", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "SalaryElementID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "MinAmount", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "MaxAmount", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "RoundTypeID", SQLColumnDataType.Integer);
                    // 1=None, 2=Nearest15, 3=Up15, 4=Down15

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "CompanyID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRules", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(7.2), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.3))
                {
                    CreateTable("tbl_AttendanceRuleConditions", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "RuleID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "LeftOperand", SQLColumnDataType.VarChar);
                    // Examples: 'LateMinutes', 'WorkedMinutes', 'ShiftType'

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "Operator", SQLColumnDataType.VarChar);
                    // >, >=, ==, <, <=, !=

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "RightOperand", SQLColumnDataType.VarChar);
                    // Examples: 'GraceLateMinutes', '480', 'ScheduleStart'

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "ValueType", SQLColumnDataType.Integer);
                    // 1=Numeric, 2=Time, 3=PropertyReference

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "CompanyID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleConditions", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(7.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.4))
                {
                    CreateTable("tbl_AttendanceRuleMapping", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "RuleID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "RuleGroupID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "DepartmentID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "ShiftID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "EmployeeID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "Priority", SQLColumnDataType.Integer);
                    // Higher priority overrides lower priority

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "CompanyID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceRuleMapping", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(7.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.5))
                {
                    AddColumnToTable(CompanyId, "tbl_Shifts", "IsOvernight", SQLColumnDataType.Bit);


                    InsertDataBaseVersion(Simulate.decimal_(7.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.6))
                {
                    CreateTable("tbl_ReportTemplate", CompanyId);

                    // Core
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "TemplateName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "TemplateType", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "EntityName", SQLColumnDataType.VarChar);

                    // JSON payload (designer output)
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "TemplateJson", SQLColumnDataType.VarChar);

                    // Flags + audit
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ReportTemplate", "ModificationDate", SQLColumnDataType.DateTime);

                    InsertDataBaseVersion(Simulate.decimal_(7.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.7))
                {
                    // =========================
                    // 1) Document Flow Header
                    // =========================
                    CreateTable("tbl_DocumentFlowHeader", CompanyId);

                    // Process family: 1=P2P, 2=O2C ...
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "FlowTypeID", SQLColumnDataType.Integer);

                    // 1=Open, 2=Closed, 3=Cancelled
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "StatusID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "Notes", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "CompanyID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "ModificationUserID", SQLColumnDataType.Integer);

                    // From/To document headers
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "TransactionGuidFrom", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "TransactionGuidTo", SQLColumnDataType.guid);

                    // From/To document types
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "TransactionTypeIDFrom", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "TransactionTypeIDTo", SQLColumnDataType.Integer);

                    // Step/action
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "FlowActionID", SQLColumnDataType.Integer);

                    // Optional
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowHeader", "ReferenceNo", SQLColumnDataType.VarChar);

                    // =========================
                    // 2) Document Flow Detail
                    // =========================
                    CreateTable("tbl_DocumentFlowDetail", CompanyId);

                    // FK to header (assumes header has integer ID as PK created by CreateTable)
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "HeaderID", SQLColumnDataType.Integer);

                    // 1=Qty, 2=Amount, 3=Both
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "ValueTypeID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "Notes", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "TransactionLineGuidFrom", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "TransactionLineGuidTo", SQLColumnDataType.guid);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "ItemGuid", SQLColumnDataType.guid);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "Qty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "Amount", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "CurrencyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "Rate", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "CompanyID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_DocumentFlowDetail", "ModificationUserID", SQLColumnDataType.Integer);

                    InsertDataBaseVersion(Simulate.decimal_(7.7), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(7.8))
                {  // ----------------------------------------------------------
                   // A) Extend tbl_Items (keep it lean: identity + defaults + flags)
                   // ----------------------------------------------------------

                    // Internal SKU / Item Code (Unique in UI/logic; DB unique index later if you have helper)
                    AddColumnToTable(CompanyId, "tbl_Items", "ItemCode", SQLColumnDataType.VarChar); // ex: "ITM-000123"

                    // Item Type:
                    // 1=Stock, 2=Service, 3=Consumable, 4=Bundle/Kit, 5=Recipe (restaurant)
                    AddColumnToTable(CompanyId, "tbl_Items", "ItemTypeID", SQLColumnDataType.Integer);

                    // Brand / Manufacturer / Model (optional, but useful)
                    AddColumnToTable(CompanyId, "tbl_Items", "BrandID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Items", "ManufacturerID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Items", "ModelNo", SQLColumnDataType.VarChar);

                    // Base UoM & default sales/purchase UoM (even if you don't implement UoM now, keep hook)
                    AddColumnToTable(CompanyId, "tbl_Items", "BaseUOMID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Items", "SalesUOMID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Items", "PurchaseUOMID", SQLColumnDataType.Integer);

                    // Costing hooks
                    AddColumnToTable(CompanyId, "tbl_Items", "StandardCost", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_Items", "LastPurchaseCost", SQLColumnDataType.Decimal);

                    // POS / Scale / price behavior
                    AddColumnToTable(CompanyId, "tbl_Items", "IsWeightedItem", SQLColumnDataType.Bit); // USB scale
                    AddColumnToTable(CompanyId, "tbl_Items", "IsOpenPrice", SQLColumnDataType.Bit);    // cashier enters price

                    // Stock controls (extra future-proof)
                    AddColumnToTable(CompanyId, "tbl_Items", "AllowNegativeStock", SQLColumnDataType.Bit);

                    // Expiry control (you already have track flags in code; add DB columns to persist)
                    AddColumnToTable(CompanyId, "tbl_Items", "TrackLot", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_Items", "TrackSerial", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_Items", "TrackExpiryDate", SQLColumnDataType.Bit);

                    // Shelf life & warning (works great with TrackExpiryDate)
                    AddColumnToTable(CompanyId, "tbl_Items", "ShelfLifeDays", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Items", "ExpiryWarningDays", SQLColumnDataType.Integer);

                    // ----------------------------------------------------------
                    // B) Master: Units of Measure
                    // ----------------------------------------------------------
                    CreateTable("tbl_UOM", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_UOM", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_UOM", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_UOM", "Symbol", SQLColumnDataType.VarChar); // EA, KG, L

                    AddColumnToTable(CompanyId, "tbl_UOM", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_UOM", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_UOM", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_UOM", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_UOM", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_UOM", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // C) Item UoM conversions (packaging: 1 Box = 12 EA)
                    // ----------------------------------------------------------
                    CreateTable("tbl_ItemUOM", CompanyId);
                    DropColumn("tbl_ItemUOM", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "ItemGuid", SQLColumnDataType.guid,0,true);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "FromUOMID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "ToUOMID", SQLColumnDataType.Integer);

                    // Factor: Qty(To) = Qty(From) * Factor
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "Factor", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "IsDefaultSales", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "IsDefaultPurchase", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // D) Multiple Barcodes per item (optionally per UoM)
                    // ----------------------------------------------------------
                    CreateTable("tbl_ItemBarcodes", CompanyId);
                    DropColumn("tbl_ItemBarcodes", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "ItemGuid", SQLColumnDataType.guid,0,true);
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "Barcode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "UOMID", SQLColumnDataType.Integer); // 0 or NULL if not used
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "IsDefault", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // E) Item Taxes mapping (supports multiple taxes; replaces fixed SalesTaxID/SpecialSalesTaxID later)
                    // ----------------------------------------------------------
                    // UsageType:
                    // 1 = Sales
                    // 2 = Purchase
                    CreateTable("tbl_ItemTax", CompanyId);
                    DropColumn("tbl_ItemTax", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "ItemGuid", SQLColumnDataType.guid, 0, true);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "TaxID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "UsageType", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "SortOrder", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_ItemTax", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemTax", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemTax", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // F) Vendor purchasing setup (preferred vendor + lead time + MOQ)
                    // ----------------------------------------------------------
                    CreateTable("tbl_ItemVendor", CompanyId);
                    DropColumn("tbl_ItemVendor", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "ItemGuid", SQLColumnDataType.guid, 0, true);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "VendorID", SQLColumnDataType.Integer); // tbl_people.PeopleID (supplier)
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "VendorItemCode", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "IsPreferred", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "LeadTimeDays", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "MinOrderQty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "OrderMultipleQty", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // G) Reorder policy (min/max/reorder qty/safety stock)
                    // ----------------------------------------------------------
                    // PolicyType:
                    // 1 = MinMax
                    // 2 = ReorderPoint
                    CreateTable("tbl_ItemReorder", CompanyId);
                    DropColumn("tbl_ItemReorder", "ID", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "ItemGuid", SQLColumnDataType.guid, 0, true);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "WarehouseID", SQLColumnDataType.Integer); // optional, keep for future

                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "PolicyType", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "ReorderPointQty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "ReorderQty", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "MinQty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "MaxQty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "SafetyStockQty", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // H) Accounting mapping (optional now, critical later)
                    // ----------------------------------------------------------
                    // If you have chart of accounts table, these will reference it.
                    CreateTable("tbl_ItemAccounts", CompanyId);
                  
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "ItemGuid", SQLColumnDataType.guid);

                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "RevenueAccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "COGSAccountID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "InventoryAccountID", SQLColumnDataType.Integer);

                    // ValuationMethod:
                    // 1 = FIFO
                    // 2 = AVG
                    // 3 = STANDARD
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "ValuationMethod", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemAccounts", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // I) Images gallery (optional - keep your Picture as main image, add multi-image support)
                    // ----------------------------------------------------------
                    CreateTable("tbl_ItemImages", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_ItemImages", "ItemGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_ItemImages", "ImageData", SQLColumnDataType.Binary);
                    AddColumnToTable(CompanyId, "tbl_ItemImages", "SortOrder", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemImages", "IsDefault", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemImages", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemImages", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_ItemImages", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemImages", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_ItemImages", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_ItemImages", "ModificationDate", SQLColumnDataType.DateTime);

                    // ----------------------------------------------------------
                    // Done
                    // ----------------------------------------------------------


                    InsertDataBaseVersion(Simulate.decimal_(7.8), CompanyId); 
                }

                if (versionNumber < Simulate.decimal_(7.9))
                {
                AddColumnToTable(CompanyId, "tbl_UOM", "DecimalPlaces", SQLColumnDataType.Decimal);

                InsertDataBaseVersion(Simulate.decimal_(7.9), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.0))
                {
                  
                    DropPrimaryKeyConstraintFromColumn(CompanyId, "tbl_ItemVendor", "ItemGuid");
                    
                    AddColumnToTable(CompanyId, "tbl_ItemVendor", "ID", SQLColumnDataType.guid, 0, true);
                    InsertDataBaseVersion(Simulate.decimal_(8.0), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.1))
                {

                    DropPrimaryKeyConstraintFromColumn(CompanyId, "tbl_ItemUOM", "ItemGuid");

                    AddColumnToTable(CompanyId, "tbl_ItemUOM", "ID", SQLColumnDataType.guid, 0, true);
                    InsertDataBaseVersion(Simulate.decimal_(8.1), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.3))
                {

                    DropPrimaryKeyConstraintFromColumn(CompanyId, "tbl_ItemBarcodes", "ItemGuid");

                    AddColumnToTable(CompanyId, "tbl_ItemBarcodes", "ID", SQLColumnDataType.guid, 0, true);
                    InsertDataBaseVersion(Simulate.decimal_(8.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.4))
                {

                    DropPrimaryKeyConstraintFromColumn(CompanyId, "tbl_ItemImages", "ItemGuid");

                    AddColumnToTable(CompanyId, "tbl_ItemImages", "ID", SQLColumnDataType.guid, 0, true);
                    InsertDataBaseVersion(Simulate.decimal_(8.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.5))
                {

                    DropPrimaryKeyConstraintFromColumn(CompanyId, "tbl_ItemReorder", "ItemGuid");

                    AddColumnToTable(CompanyId, "tbl_ItemReorder", "ID", SQLColumnDataType.guid, 0, true);
                    InsertDataBaseVersion(Simulate.decimal_(8.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.6))
                {

                    DropColumn( "tbl_ItemImages", "ImageData", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_ItemImages", "ImageData", SQLColumnDataType.varbinarymax);

                    InsertDataBaseVersion(Simulate.decimal_(8.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.7))
                {
                    clsSQL cls = new clsSQL();


                    AddColumnToTable(CompanyId, "tbl_Items", "ParentGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_Items", "BaseFactor", SQLColumnDataType.Decimal);
                    cls.ExecuteNonQueryStatement( @"
        UPDATE tbl_Items
        SET BaseFactor = 1
        WHERE BaseFactor IS NULL OR BaseFactor = 0
    ",cls.CreateDataBaseConnectionString(CompanyId));
                    InsertDataBaseVersion(Simulate.decimal_(8.7), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.8))
                {
                    clsSQL cls = new clsSQL();


                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "UOMQTY", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "UOMID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_InvoiceDetails", "UOMFactor", SQLColumnDataType.Decimal);
                    cls.ExecuteNonQueryStatement(@"UPDATE d
SET 
  d.UOMQTY = d.qty,
  d.UOMFactor = 1,
  d.UOMID = ISNULL(i.BaseUOMID, 0)
FROM tbl_InvoiceDetails d
LEFT JOIN tbl_Items i ON i.Guid = d.ItemGuid;
    ", cls.CreateDataBaseConnectionString(CompanyId));
                    InsertDataBaseVersion(Simulate.decimal_(8.8), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(8.9))
                {
                    // =========================================================
                    // tbl_BOMHeader
                    // =========================================================
                    CreateTable("tbl_BOMHeader", CompanyId);

                

                      // Core info
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "BOMCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "BOMName", SQLColumnDataType.VarChar);

                    // Batch / version
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "BatchQty", SQLColumnDataType.Decimal);     // default 1 in code
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "VersionNo", SQLColumnDataType.Integer);    // default 1 in code

                    // Flags
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "IsDefault", SQLColumnDataType.Bit);        // default 0
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "IsActive", SQLColumnDataType.Bit);         // default 1

                    // Effective dates (optional)
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "EffectiveFrom", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "EffectiveTo", SQLColumnDataType.DateTime);

                    // Notes
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "Notes", SQLColumnDataType.VarChar);

                    // Audit
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "CompanyID", SQLColumnDataType.Integer);

                    // =========================================================
                    // tbl_BOMInput (Components / Raw Materials)
                    // =========================================================
                    CreateTable("tbl_BOMInput", CompanyId);

                    
              

                    // Link
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "BOMID", SQLColumnDataType.Integer);

                    // Component item
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "ComponentItemGuid", SQLColumnDataType.guid);

                    // Qty / UOM
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "Qty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "UOMID", SQLColumnDataType.Integer);

                    // Order
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "LineNo", SQLColumnDataType.Integer);

                    // Optional: scrap% per line
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "ScrapPercent", SQLColumnDataType.Decimal);

                    // Notes
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "Notes", SQLColumnDataType.VarChar);

                    // Audit
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_BOMInput", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "CompanyID", SQLColumnDataType.Integer);

                    // =========================================================
                    // tbl_BOMOutput (Multi outputs: 1..N)
                    // =========================================================
                    CreateTable("tbl_BOMOutput", CompanyId);

                 

                    // Link
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "BOMID", SQLColumnDataType.Integer);

                    // Output item
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "OutputItemGuid", SQLColumnDataType.guid);

                    // Qty / UOM
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "Qty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "UOMID", SQLColumnDataType.Integer);

                    // Optional: allocate cost between outputs (0-100)
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "CostSharePercent", SQLColumnDataType.Decimal);

                    // Order
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "LineNo", SQLColumnDataType.Integer);

                    // Notes
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "Notes", SQLColumnDataType.VarChar);

                    // Audit
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "CompanyID", SQLColumnDataType.Integer);

                    // =========================================================
                    // DB Version
                    // =========================================================
                    InsertDataBaseVersion(Simulate.decimal_(8.9), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(9.1))
                {

                    // LineOrder
                    AddColumnToTable(CompanyId, "tbl_BOMInput", "LineOrder", SQLColumnDataType.Integer);

                    // Audit
                  
                    InsertDataBaseVersion(Simulate.decimal_(9.1), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(9.2))
                {

                    // LineOrder
                    AddColumnToTable(CompanyId, "tbl_BOMOutput", "LineOrder", SQLColumnDataType.Integer);

                    // Audit

                    InsertDataBaseVersion(Simulate.decimal_(9.2), CompanyId);
                }
                // =========================================================
                // Manufacturing Orders (MO) + Link to Invoice documents
                // Supports long period GI/GR posting (multi issue + multi receipt)
                // =========================================================

                if (versionNumber < Simulate.decimal_(9.3))
                {
                    // =========================================================
                    // tbl_MOHeader
                    // =========================================================
                    CreateTable("tbl_MOHeader", CompanyId);
                    DeleteColumnFromTable("tbl_MOHeader", "id", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "Guid", SQLColumnDataType.guid, null, true);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "MOCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "MOName", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "BOMID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "PlannedQty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "BatchQty", SQLColumnDataType.Decimal);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "MODate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "PlannedStartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "PlannedEndDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "StatusID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "StoreID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "Notes", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "IsActive", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MOHeader", "ModificationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_MOHeader", "CompanyID", SQLColumnDataType.Integer);


                    // =========================================================
                    // tbl_MODetails
                    // =========================================================
                    CreateTable("tbl_MODetails", CompanyId);
                    DeleteColumnFromTable("tbl_MODetails", "id", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "Guid", SQLColumnDataType.guid, null, true);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "HeaderGuid", SQLColumnDataType.guid);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "RowIndex", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "LineTypeID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "ItemGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "ItemName", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "PlannedQty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "UOMID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "ScrapPercent", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "CostSharePercent", SQLColumnDataType.Decimal);

                    // Optional: map line to BOM (very useful when MO created from BOM)
                    AddColumnToTable(CompanyId, "tbl_MODetails", "BOMLineNo", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "StoreID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "Notes", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "TrackLot", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "TrackSerial", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "TrackExpiryDate", SQLColumnDataType.Bit);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "CreationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MODetails", "ModificationDate", SQLColumnDataType.DateTime);

                    AddColumnToTable(CompanyId, "tbl_MODetails", "CompanyID", SQLColumnDataType.Integer);


                    // =========================================================
                    // tbl_MOInvoiceLink
                    // =========================================================
                    CreateTable("tbl_MOInvoiceLink", CompanyId);
                    DeleteColumnFromTable("tbl_MOInvoiceLink", "id", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "Guid", SQLColumnDataType.guid, null, true);
                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "MOGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "InvoiceHeaderGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "LinkTypeID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "Notes", SQLColumnDataType.VarChar);

                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_MOInvoiceLink", "CompanyID", SQLColumnDataType.Integer);

                    // =========================================================
                    // DB Version
                    // =========================================================
                    InsertDataBaseVersion(Simulate.decimal_(9.3), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(9.4))
                {

                    // LineOrder
                    clsForms.InsertForm(111, "UOMPage", "وحدات القياس", "UOMPage", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(112, "UOMPageAdd", "  اضافة وحدات القياس", "UOMPageAdd", 54, true, true, true, true, true, false, CompanyId);
                    clsForms.InsertForm(113, "BOMPageMain", "خلطات مواد التصنيع", "BOMPageMain", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(114, "BOMPageAdd", "اضافة خلطات مواد التصنيع  ", "BOMPageAdd", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(115, "Manufacturing", "التصنيع", "Manufacturing", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(116, "Manufacturing Order", "امر تصنيع", "PayrollDetailsViewPage", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(117, "MO Summary Report", " تقرير ملخص اوامر التصنيع", "PayrollDetailsViewPage", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(118, "MO Progress Report", "تقرير تقدم اوامر التصنيع", "PayrollDetailsViewPage", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(119, "MO Vouchers Report", "تقرير مستندات اوامر التصنيع", "PayrollDetailsViewPage", 54, true, false, false, false, true, false, CompanyId);
                    clsForms.InsertForm(120, "MOPageAdd", "اضافه اوامر تصنيع", "PayrollDetailsViewPage", 54, true, false, false, false, true, false, CompanyId);
               
                    // Audit

                    InsertDataBaseVersion(Simulate.decimal_(9.4), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(9.5))
                {
 
              
                    clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(24, "امر تصنيع", "Manufacturing Order", 0, CompanyId);
                    clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(25, "مدخلات الانتاج ", "Manufacturing Input", -1, CompanyId);
                    clsJournalVoucherTypes.Inserttbl_JournalVoucherTypes(26, "مخرجات الانتاج", "Manufacturing Output", 1, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(9.5), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(9.6))
                {
                    clsSQL clssql = new clsSQL();
                  
                    string b = @"IF OBJECT_ID('dbo.vw_JVSourceTransaction', 'V') IS NOT NULL
    DROP VIEW dbo.vw_JVSourceTransaction;";
                    ClsSQL.ExecuteNonQueryStatement(b, clssql.CreateDataBaseConnectionString(CompanyId));
                    string a = @"
  Create VIEW dbo.vw_JVSourceTransaction
AS
select JVGuid,InvoiceNo as VoucherNumber ,guid from tbl_InvoiceHeader
union all
select JVGuid,VoucherNumber,guid from tbl_FinancingHeader
union all
select tbl_FinancingDetails.JVGuid,f.VoucherNumber,f.guid  from tbl_FinancingDetails 
left join tbl_FinancingHeader f on f.Guid= tbl_FinancingDetails.HeaderGuid
union all
select JVGuid,VoucherNo as VoucherNumber ,guid from tbl_CashVoucherHeader
union all
select JVGuid,VoucherNo as VoucherNumber,guid from tbl_CreditNoteHeader

;
"; 
                    ClsSQL.ExecuteNonQueryStatement(a, clssql.CreateDataBaseConnectionString(CompanyId));              
                    InsertDataBaseVersion(Simulate.decimal_(9.6), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(9.7))
                {
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "SubmitSalesInvoices", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "SubmitSalesReturnInvoices", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "SubmitPOSSalesInvoices", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EInvoiceConfigurations", "SubmitPOSSalesReturnInvoices", SQLColumnDataType.Integer);

                    InsertDataBaseVersion(Simulate.decimal_(9.7), CompanyId);
                }
                if (versionNumber < Simulate.decimal_(9.8))
                {
                    // HR: Attendance Machines (biometric devices registry)
                    CreateTable("tbl_AttendanceMachines", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "Model", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "IPAddress", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "Port", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "Password", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "MachineName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AttendanceMachines", "ModificationDate", SQLColumnDataType.DateTime);

                    // HR: Live attendance log buffer (consumed by Live Logs dashboard)
                    CreateTable("tbl_LiveLogs", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_LiveLogs", "EmployeeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_LiveLogs", "MachineID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_LiveLogs", "PunchTime", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_LiveLogs", "PunchType", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_LiveLogs", "CompanyID", SQLColumnDataType.Integer);

                    InsertDataBaseVersion(Simulate.decimal_(9.8), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.9))
                {
                    // HR: Employee contracts (hire / renewal / probation history)
                    CreateTable("tbl_EmployeeContract", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "EmployeeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "ContractTypeID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "JobTitleID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "DepartmentID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "ContractNumber", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "StartDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "EndDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "IsOpenEnded", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "ProbationMonths", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "WorkingHoursPerWeek", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "BasicSalary", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "Notes", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "ModificationDate", SQLColumnDataType.DateTime);

                    clsForms.InsertForm(130, "HireEmployeePage", "توظيف موظف جديد (انشاء عقد)", "HireEmployeePage", 54, true, false, true, false, false, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(9.9), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.91))
                {
                    // HR: contract leave entitlements (Jordan Labour Law defaults: annual 14→21 after 5 yrs; sick 14+14)
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "AnnualLeaveDaysPerYear", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "AnnualLeaveDaysAfter5Years", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "SickLeaveFullPayDaysPerYear", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_EmployeeContract", "SickLeaveExtendedDaysPerYear", SQLColumnDataType.Integer);

                    clsSQL clssql = new clsSQL();
                    string upd = @"
UPDATE tbl_EmployeeContract SET
  AnnualLeaveDaysPerYear = 14,
  AnnualLeaveDaysAfter5Years = 21,
  SickLeaveFullPayDaysPerYear = 14,
  SickLeaveExtendedDaysPerYear = 14
WHERE AnnualLeaveDaysPerYear IS NULL OR AnnualLeaveDaysAfter5Years IS NULL
   OR SickLeaveFullPayDaysPerYear IS NULL OR SickLeaveExtendedDaysPerYear IS NULL";
                    clssql.ExecuteNonQueryStatement(upd, clssql.CreateDataBaseConnectionString(CompanyId));
                    InsertDataBaseVersion(Simulate.decimal_(9.91), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.92))
                {
                    // Link employee salary element rows to a contract (agreement package + traceability)
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "EmployeeContractID", SQLColumnDataType.Integer);
                    InsertDataBaseVersion(Simulate.decimal_(9.92), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.93))
                {
                    // Flag: include this assignment on the printed employment contract PDF
                    AddColumnToTable(CompanyId, "tbl_EmployeeSalaryElements", "IncludeOnContractPrint", SQLColumnDataType.Bit);
                    clsSQL clssql2 = new clsSQL();
                    string conn2 = clssql2.CreateDataBaseConnectionString(CompanyId);
                    clssql2.ExecuteNonQueryStatement(
                        "UPDATE tbl_EmployeeSalaryElements SET IncludeOnContractPrint = 0 WHERE IncludeOnContractPrint IS NULL",
                        conn2);
                    clssql2.ExecuteNonQueryStatement(
                        "UPDATE tbl_EmployeeSalaryElements SET IncludeOnContractPrint = 1 WHERE EmployeeContractID IS NOT NULL",
                        conn2);
                    InsertDataBaseVersion(Simulate.decimal_(9.93), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.94))
                {
                    // Per-company transaction print definitions (JV, invoice, cash voucher, etc.)
                    CreateTable("tbl_TransactionReport", CompanyId);

                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "PageName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "ReportName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "EName", SQLColumnDataType.VarChar);

                    // FastReport | JsonTemplate
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "ReportEngine", SQLColumnDataType.VarChar);
                    // FastReport file name without extension, e.g. rptJV
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "FastReportFileName", SQLColumnDataType.VarChar);
                    // Optional link to tbl_ReportTemplate when ReportEngine = JsonTemplate
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "ReportTemplateID", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "IsDefault", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "SortOrder", SQLColumnDataType.Integer);

                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "ModificationDate", SQLColumnDataType.DateTime);

                    clsSQL seedSql = new clsSQL();
                    string seedConn = seedSql.CreateDataBaseConnectionString(CompanyId);
                    SqlParameter[] seedPrm =
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyId },
                    };
                    string seedJv = @"
IF NOT EXISTS (
    SELECT 1 FROM tbl_TransactionReport
    WHERE CompanyID = @CompanyID AND PageName = 'JournalVoucherAdd' AND ReportName = 'DefaultJV'
)
BEGIN
    INSERT INTO tbl_TransactionReport
        (CompanyID, PageName, ReportName, AName, EName, ReportEngine, FastReportFileName,
         ReportTemplateID, IsDefault, IsActive, SortOrder, CreationUserID, CreationDate)
    VALUES
        (@CompanyID, 'JournalVoucherAdd', 'DefaultJV',
         N'قيد يومية - افتراضي', 'Journal Voucher - Default',
         'FastReport', 'rptJV', NULL, 1, 1, 1, 0, GETDATE());
END";
                    seedSql.ExecuteNonQueryStatement(seedJv, seedConn, seedPrm);

                    InsertDataBaseVersion(Simulate.decimal_(9.94), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.95))
                {
                    // Optional inline FastReport layout (parsed at runtime when set)
                    AddColumnToTable(CompanyId, "tbl_TransactionReport", "ReportFrxXml", SQLColumnDataType.VarChar);

                    string defaultFrxPath = System.IO.Path.Combine(
                        Environment.CurrentDirectory, "Reports", "rptJV.frx");
                    if (System.IO.File.Exists(defaultFrxPath))
                    {
                        string defaultFrxXml = System.IO.File.ReadAllText(defaultFrxPath);
                        clsSQL frxSql = new clsSQL();
                        string frxConn = frxSql.CreateDataBaseConnectionString(CompanyId);
                        SqlParameter[] frxPrm =
                        {
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyId },
                            new SqlParameter("@ReportFrxXml", SqlDbType.NVarChar, -1)
                            {
                                Value = defaultFrxXml ?? ""
                            },
                        };
                        frxSql.ExecuteNonQueryStatement(@"
UPDATE tbl_TransactionReport
SET ReportFrxXml = @ReportFrxXml
WHERE CompanyID = @CompanyID
  AND PageName = 'JournalVoucherAdd'
  AND ReportName = 'DefaultJV'
  AND (ReportFrxXml IS NULL OR ReportFrxXml = '')",
                            frxConn, frxPrm);
                    }

                    InsertDataBaseVersion(Simulate.decimal_(9.95), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.96))
                {
                    // Clear truncated/invalid inline FRX (use file path instead)
                    clsSQL fixSql = new clsSQL();
                    string fixConn = fixSql.CreateDataBaseConnectionString(CompanyId);
                    fixSql.ExecuteNonQueryStatement(@"
UPDATE tbl_TransactionReport
SET ReportFrxXml = NULL
WHERE PageName = 'JournalVoucherAdd'
  AND ReportName = 'DefaultJV'
  AND (
        ReportFrxXml IS NULL
        OR LEN(ReportFrxXml) < 500
        OR ReportFrxXml NOT LIKE '%</Report>%'
      )",
                        fixConn);

                    InsertDataBaseVersion(Simulate.decimal_(9.96), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.97))
                {
                    clsSQL seedSql = new clsSQL();
                    string seedConn = seedSql.CreateDataBaseConnectionString(CompanyId);
                    SqlParameter[] seedPrm =
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyId },
                    };

                    string seedReports = @"
IF NOT EXISTS (SELECT 1 FROM tbl_TransactionReport WHERE CompanyID=@CompanyID AND PageName='InvoicePageAdd' AND ReportName='DefaultInvoice')
INSERT INTO tbl_TransactionReport (CompanyID,PageName,ReportName,AName,EName,ReportEngine,FastReportFileName,ReportTemplateID,IsDefault,IsActive,SortOrder,CreationUserID,CreationDate)
VALUES (@CompanyID,'InvoicePageAdd','DefaultInvoice',N'فاتورة - افتراضي','Invoice - Default (Sales / Purchase)','FastReport','rptInvoice',NULL,1,1,1,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM tbl_TransactionReport WHERE CompanyID=@CompanyID AND PageName='InvoicePageAdd' AND ReportName='DefaultInvoicePOS')
INSERT INTO tbl_TransactionReport (CompanyID,PageName,ReportName,AName,EName,ReportEngine,FastReportFileName,ReportTemplateID,IsDefault,IsActive,SortOrder,CreationUserID,CreationDate)
VALUES (@CompanyID,'InvoicePageAdd','DefaultInvoicePOS',N'فاتورة نقطة بيع','POS Invoice - Default','FastReport','rptInvoicePOS',NULL,0,1,2,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM tbl_TransactionReport WHERE CompanyID=@CompanyID AND PageName='CashVoucherAdd' AND ReportName='DefaultCashVoucher')
INSERT INTO tbl_TransactionReport (CompanyID,PageName,ReportName,AName,EName,ReportEngine,FastReportFileName,ReportTemplateID,IsDefault,IsActive,SortOrder,CreationUserID,CreationDate)
VALUES (@CompanyID,'CashVoucherAdd','DefaultCashVoucher',N'سند صندوق - افتراضي','Cash Voucher - Default','FastReport','rptCashVoucher',NULL,1,1,1,0,GETDATE());

IF NOT EXISTS (SELECT 1 FROM tbl_TransactionReport WHERE CompanyID=@CompanyID AND PageName='CreditNotePageAdd' AND ReportName='DefaultCreditNote')
INSERT INTO tbl_TransactionReport (CompanyID,PageName,ReportName,AName,EName,ReportEngine,FastReportFileName,ReportTemplateID,IsDefault,IsActive,SortOrder,CreationUserID,CreationDate)
VALUES (@CompanyID,'CreditNotePageAdd','DefaultCreditNote',N'إشعار دائن/مدين - افتراضي','Credit / Debit Note - Default','FastReport','rptCashVoucher',NULL,1,1,1,0,GETDATE());";

                    seedSql.ExecuteNonQueryStatement(seedReports, seedConn, seedPrm);
                    InsertDataBaseVersion(Simulate.decimal_(9.97), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.98))
                {
                    clsDashboardWidgets.ApplyDashboardWidgetSqlFixes(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(9.98), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(9.99))
                {
                    clsDashboardWidgets.ApplyDashboardWidgetTypeFixes(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(9.99), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.00))
                {
                    clsTransactionReportDefaults.ApplyDefaultSeeds(CompanyId, 0);
                    InsertDataBaseVersion(Simulate.decimal_(10.00), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.01))
                {
                    clsTransactionReportDefaults.ApplyStandardFrxFromFiles(CompanyId, 0);
                    InsertDataBaseVersion(Simulate.decimal_(10.01), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.02))
                {
                    clsTransactionReportDefaults.ApplyDefaultSeeds(CompanyId, 0);
                    clsTransactionReportDefaults.ApplyStandardFrxFromFiles(CompanyId, 0);
                    InsertDataBaseVersion(Simulate.decimal_(10.02), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.03))
                {
                    clsDashboardWidgets.ApplyBusinessDashboardDefaults(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.03), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.04))
                {
                    clsDashboardWidgets.ApplyFinancingDashboardDefaults(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.04), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.05))
                {
                    clsDashboardWidgets.ApplyAdvancedAnalyticsDashboardDefaults(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.05), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.06))
                {
                    CreateTable("tbl_UserMenuPreferences", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_UserMenuPreferences", "UserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_UserMenuPreferences", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_UserMenuPreferences", "ModuleNamespace", SQLColumnDataType.VarChar, 100);
                    AddColumnToTable(CompanyId, "tbl_UserMenuPreferences", "PinnedKeys", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_UserMenuPreferences", "ExpandedGroups", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_UserMenuPreferences", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_UserMenuPreferences", "ModificationDate", SQLColumnDataType.DateTime);
                    ClsSQL.ExecuteNonQueryStatement(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_UserMenuPreferences_User_Company_Module'
      AND object_id = OBJECT_ID(N'tbl_UserMenuPreferences')
)
BEGIN
    CREATE UNIQUE INDEX UX_UserMenuPreferences_User_Company_Module
    ON tbl_UserMenuPreferences (UserId, CompanyID, ModuleNamespace);
END", ClsSQL.CreateDataBaseConnectionString(CompanyId), null);
                    InsertDataBaseVersion(Simulate.decimal_(10.06), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.07))
                {
                    clsDashboardWidgets.ApplyDashboardWidgetSqlFixes(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.07), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.08))
                {
                    clsDashboardWidgets.ApplyDashboardWidgetSqlFixes(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.08), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.09))
                {
                    clsDashboardWidgets.ApplyDashboardWidgetSqlFixes(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.09), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.10))
                {
                    // CRM: pipeline, journey stages, opportunities, history, activities
                    CreateTable("tbl_CRMPipeline", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "IsDefault", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMPipeline", "ModificationDate", SQLColumnDataType.DateTime);

                    CreateTable("tbl_CRMJourneyStage", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "PipelineID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "StageOrder", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "Color", SQLColumnDataType.VarChar, 20);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "IsWon", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "IsLost", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "IsDefault", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMJourneyStage", "ModificationDate", SQLColumnDataType.DateTime);

                    CreateTable("tbl_CRMOpportunity", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "PipelineID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "StageID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Title", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Tel1", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Email", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Country", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Source", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Notes", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "BusinessPartnerID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "AssignedUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "ExpectedValue", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "CurrencyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Probability", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "ExpectedCloseDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "Priority", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMOpportunity", "ModificationDate", SQLColumnDataType.DateTime);

                    CreateTable("tbl_CRMStageHistory", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_CRMStageHistory", "OpportunityID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMStageHistory", "FromStageID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMStageHistory", "ToStageID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMStageHistory", "MovedByUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMStageHistory", "MovedDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CRMStageHistory", "CompanyID", SQLColumnDataType.Integer);

                    CreateTable("tbl_CRMActivity", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "OpportunityID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "ActivityType", SQLColumnDataType.VarChar, 50);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "Subject", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "DueDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "IsDone", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "Notes", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "AssignedUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_CRMActivity", "ModificationDate", SQLColumnDataType.DateTime);

                    // tbl_Leads in company DB for public lead capture sync
                    CreateTable("tbl_Leads", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_Leads", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Leads", "Tel1", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Leads", "Email", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Leads", "Country", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Leads", "Note", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_Leads", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Leads", "CRMOpportunityID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Leads", "CreationDate", SQLColumnDataType.DateTime);

                    clsSQL crmSql = new clsSQL();
                    string crmConn = crmSql.CreateDataBaseConnectionString(CompanyId);

                    crmSql.ExecuteNonQueryStatement(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CRMOpportunity_Company_Pipeline_Stage' AND object_id = OBJECT_ID(N'tbl_CRMOpportunity'))
    CREATE INDEX IX_CRMOpportunity_Company_Pipeline_Stage ON tbl_CRMOpportunity (CompanyID, PipelineID, StageID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CRMJourneyStage_Company_Pipeline_Order' AND object_id = OBJECT_ID(N'tbl_CRMJourneyStage'))
    CREATE INDEX IX_CRMJourneyStage_Company_Pipeline_Order ON tbl_CRMJourneyStage (CompanyID, PipelineID, StageOrder);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CRMStageHistory_Opportunity' AND object_id = OBJECT_ID(N'tbl_CRMStageHistory'))
    CREATE INDEX IX_CRMStageHistory_Opportunity ON tbl_CRMStageHistory (OpportunityID, MovedDate DESC);", crmConn, null);

                    clsCRMPipeline clsPipeline = new clsCRMPipeline();
                    clsPipeline.EnsureDefaultPipeline(CompanyId, 1);

                    clsForms.InsertForm(131, "CRMModule", "إدارة علاقات العملاء", "CRM Module", 53, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(132, "CRMPipelineBoard", "لوحة مسار المبيعات", "CRM Pipeline Board", 131, true, true, false, false, false, false, CompanyId);
                    clsForms.InsertForm(133, "CRMOpportunityAdd", "إضافة فرصة", "CRM Opportunity Add", 131, false, false, true, true, true, false, CompanyId);
                    clsForms.InsertForm(134, "CRMJourneyStages", "مراحل رحلة العميل", "CRM Journey Stages", 131, true, false, true, true, true, false, CompanyId);
                    clsForms.InsertForm(135, "CRMActivity", "أنشطة CRM", "CRM Activity", 131, true, false, true, true, true, false, CompanyId);
                    clsForms.InsertForm(136, "CRMOpportunityMain", "قائمة الفرص", "CRM Opportunity Main", 131, true, true, false, false, false, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(10.10), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.11))
                {
                    clsForms.InsertForm(137, "CRMDashboard", "لوحة CRM", "CRM Dashboard", 131, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(138, "CRMReports", "تقارير CRM", "CRM Reports", 131, true, false, false, false, false, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(10.11), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.12))
                {
                    ApplyMainDatabaseAuditSchema();

                    CreateTable("tbl_AuditActionType", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_AuditActionType", "Code", SQLColumnDataType.VarChar, 50);
                    AddColumnToTable(CompanyId, "tbl_AuditActionType", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AuditActionType", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AuditActionType", "Category", SQLColumnDataType.VarChar, 30);
                    AddColumnToTable(CompanyId, "tbl_AuditActionType", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditActionType", "CreationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditActionType", "CreationDate", SQLColumnDataType.DateTime);
                    SeedAuditActionTypes(CompanyId);

                    CreateTable("tbl_AuditUserSession", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "SessionGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "UserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "UserName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "LoginTime", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "LogoutTime", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "LogoutReason", SQLColumnDataType.VarChar, 50);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "LastActivityTime", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "DurationMinutes", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "IPAddress", SQLColumnDataType.VarChar, 100);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "DeviceInfo", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "AppVersion", SQLColumnDataType.VarChar, 50);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "Platform", SQLColumnDataType.VarChar, 30);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditUserSession", "CreationUserID", SQLColumnDataType.Integer);

                    CreateTable("tbl_AuditEvent", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "SessionID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "UserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "ActionTypeCode", SQLColumnDataType.VarChar, 50);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "ModuleName", SQLColumnDataType.VarChar, 100);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "EntityTable", SQLColumnDataType.VarChar, 100);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "RecordID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "RecordReference", SQLColumnDataType.VarChar, 200);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "Description", SQLColumnDataType.VarChar, 500);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "FormID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "IPAddress", SQLColumnDataType.VarChar, 100);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "DeviceInfo", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "OldValuesJson", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "NewValuesJson", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "EventDateTime", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_AuditEvent", "CreationUserID", SQLColumnDataType.Integer);

                    PrepareAuditEventColumnsForIndexes(CompanyId);
                    CreateAuditIndexes(CompanyId);

                    clsForms.InsertForm(139, "AuditReportsHub", "تدقيق وأمان", "Audit & Security", 0, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(140, "AuditSessions", "جلسات المستخدمين", "User Sessions", 139, true, false, false, false, false, false, CompanyId);
                    clsForms.InsertForm(141, "AuditEvents", "سجل النشاط", "Activity Log", 139, true, false, false, false, false, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(10.12), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.13))
                {
                    AddColumnToTable(CompanyId, "tbl_Company", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Subscriptions", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_Subscriptions", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_UserAuthorization", "ModificationUserID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_UserAuthorization", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_BOMHeader", "ModificationDate", SQLColumnDataType.DateTime);
                    InsertDataBaseVersion(Simulate.decimal_(10.13), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.14))
                {
                    clsForms mfgFormsUpdate = new clsForms();
                    mfgFormsUpdate.UpdateForm(116, "Manufacturing Order", "امر تصنيع", "MOPageMain", 54, true, false, false, false, true, false, CompanyId);
                    mfgFormsUpdate.UpdateForm(117, "MO Summary Report", " تقرير ملخص اوامر التصنيع", "MOSummaryReportPage", 54, true, false, false, false, true, false, CompanyId);
                    mfgFormsUpdate.UpdateForm(118, "MO Progress Report", "تقرير تقدم اوامر التصنيع", "MOProgressReportPage", 54, true, false, false, false, true, false, CompanyId);
                    mfgFormsUpdate.UpdateForm(119, "MO Vouchers Report", "تقرير مستندات اوامر التصنيع", "MOVouchersReportPage", 54, true, false, false, false, true, false, CompanyId);
                    mfgFormsUpdate.UpdateForm(120, "MOPageAdd", "اضافه اوامر تصنيع", "MOPageAdd", 54, true, false, false, false, true, false, CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.14), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.15))
                {
                    CreateTable("tbl_WorkCenter", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "WorkCenterCode", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "AName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "EName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "CapacityPerDay", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "IsActive", SQLColumnDataType.Bit);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "Notes", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_WorkCenter", "CompanyID", SQLColumnDataType.Integer);

                    CreateTable("tbl_MORouting", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "Guid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "MOGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "LineNo", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "WorkCenterID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "OperationName", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "PlannedHours", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "ActualHours", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "StatusID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "PlannedStart", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "PlannedEnd", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "Notes", SQLColumnDataType.VarChar);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "CreationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "CreationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "ModificationUserId", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "ModificationDate", SQLColumnDataType.DateTime);
                    AddColumnToTable(CompanyId, "tbl_MORouting", "CompanyID", SQLColumnDataType.Integer);

                    clsForms mfgFormsInsert = new clsForms();
                    mfgFormsInsert.InsertForm(121, "WorkCenterPageMain", "مراكز العمل", "WorkCenterPageMain", 54, true, false, false, false, true, false, CompanyId);
                    mfgFormsInsert.InsertForm(122, "WorkCenterPageAdd", "اضافة مركز عمل", "WorkCenterPageAdd", 54, true, false, false, false, true, false, CompanyId);
                    mfgFormsInsert.InsertForm(123, "MOSchedulingPage", "جدولة الانتاج", "MOSchedulingPage", 54, true, false, false, false, false, false, CompanyId);
                    mfgFormsInsert.InsertForm(124, "MRPSuggestionsPage", "تخطيط متطلبات المواد", "MRPSuggestionsPage", 54, true, false, false, false, false, false, CompanyId);
                    mfgFormsInsert.InsertForm(125, "MODashboardPage", "لوحة التصنيع", "MODashboardPage", 54, true, false, false, false, false, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(10.15), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.16))
                {
                    // HR attendance + shift forms referenced by Flutter (126-129 were never seeded).
                    // IDs 121-125 remain manufacturing (work center, MRP, MO dashboard).
                    clsForms hrForms = new clsForms();
                    const int hrParent = 94;

                    hrForms.InsertFormIfNotExists(126, "AttendanceRulesMain", "قواعد الحضور", "Attendance Rules", hrParent, true, false, true, true, true, false, CompanyId);
                    hrForms.InsertFormIfNotExists(127, "TimesheetMonthPage", "جدول الحضور الشهري", "Monthly Timesheet", hrParent, true, false, false, false, false, false, CompanyId);
                    hrForms.InsertFormIfNotExists(128, "AttendanceMachinePage", "أجهزة الحضور", "Attendance Machines", hrParent, true, false, true, true, true, false, CompanyId);
                    hrForms.InsertFormIfNotExists(129, "LiveLogDashboard", "سجل الحضور المباشر", "Live Attendance Logs", hrParent, true, false, false, false, false, false, CompanyId);

                    hrForms.InsertFormIfNotExists(142, "ShiftMainPage", "إدارة الورديات", "Shift Management", hrParent, true, false, true, true, true, false, CompanyId);
                    hrForms.InsertFormIfNotExists(143, "EmployeeShiftAssignmentPage", "تعيين ورديات الموظفين", "Employee Shift Assignment", hrParent, true, false, true, true, true, false, CompanyId);
                    hrForms.InsertFormIfNotExists(144, "ShiftWeeklyPage", "تقويم الورديات الأسبوعي", "Weekly Shift Calendar", hrParent, true, false, false, false, false, false, CompanyId);
                    hrForms.InsertFormIfNotExists(145, "FingerprintReviewPage", "مراجعة بصمات الحضور", "Fingerprint Punch Review", hrParent, true, true, false, false, true, false, CompanyId);
                    hrForms.InsertFormIfNotExists(146, "AttendanceDayViewerPage", "حساب الحضور اليومي", "Attendance Calculation", hrParent, true, false, false, false, false, false, CompanyId);

                    InsertDataBaseVersion(Simulate.decimal_(10.16), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.17))
                {
                    clsDashboardWidgets.ApplyDashboardLeadershipKpiDefaults(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.17), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.18))
                {
                    clsDashboardWidgets.ApplyDashboardChartTypeUpgrades(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.18), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.19))
                {
                    AddColumnToTable(CompanyId, "tbl_BusinessPartner", "Note", SQLColumnDataType.VarChar);
                    InsertDataBaseVersion(Simulate.decimal_(10.19), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.20))
                {
                    ApplyApprovalWorkflowSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.20), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.21))
                {
                    ApplyApprovalPolicyLevelAmountSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.21), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.22))
                {
                    ApplyApprovalPolicyLevelGroupSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.22), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.23))
                {
                    ApplyApprovalPolicyDefaultFlagsSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.23), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.24))
                {
                    ApplyCreditNoteApprovalSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.24), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.25))
                {
                    ApplyApprovalPolicyCreditDebitSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.25), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.26))
                {
                    ApplyInvoiceApprovalSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.26), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.27))
                {
                    ApplyHcmApprovalSchema(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.27), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.28))
                {
                    // Stock balance snapshot: a per item/store cache of on-hand quantity and
                    // average cost so reports/lookups don't have to re-aggregate the whole
                    // tbl_InvoiceDetails every time. Maintained via clsStockBalance.
                    CreateTable("tbl_StockBalance", CompanyId);
                    AddColumnToTable(CompanyId, "tbl_StockBalance", "ItemGuid", SQLColumnDataType.guid);
                    AddColumnToTable(CompanyId, "tbl_StockBalance", "StoreID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_StockBalance", "BranchID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_StockBalance", "CompanyID", SQLColumnDataType.Integer);
                    AddColumnToTable(CompanyId, "tbl_StockBalance", "OnHandQty", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_StockBalance", "AvgCost", SQLColumnDataType.Decimal);
                    AddColumnToTable(CompanyId, "tbl_StockBalance", "LastUpdated", SQLColumnDataType.DateTime);
                    ClsSQL.ExecuteNonQueryStatement(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_StockBalance_Item_Store_Company'
      AND object_id = OBJECT_ID(N'tbl_StockBalance')
)
BEGIN
    CREATE UNIQUE INDEX UX_StockBalance_Item_Store_Company
    ON tbl_StockBalance (ItemGuid, StoreID, CompanyID);
END", ClsSQL.CreateDataBaseConnectionString(CompanyId), null);
                    InsertDataBaseVersion(Simulate.decimal_(10.28), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.29))
                {
                    BackupAndDeleteZeroAmountReconciliationRows(CompanyId);
                    InsertDataBaseVersion(Simulate.decimal_(10.29), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.30))
                {
                    // Ensure every company database has the full set of standard FastReport defaults.
                    try
                    {
                        clsTransactionReportPrint.TryEnsureTransactionReportSchema(CompanyId);
                        clsTransactionReportDefaults.ApplyDefaultSeeds(CompanyId, 0);
                        clsTransactionReportDefaults.ApplyStandardFrxFromFiles(CompanyId, 0);
                    }
                    catch
                    {
                        // Schema/seed failures must not block other version upgrades.
                    }
                    InsertDataBaseVersion(Simulate.decimal_(10.30), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.31))
                {
                    // Shared Reports\*.frx is the base for all customers.
                    // Clear seeded DB copies that match the global standard so company/custom
                    // file updates (e.g. Employee Loans / rptCutomerLoansReport) are used.
                    // Real uploads that differ from the global file are kept.
                    try
                    {
                        clsTransactionReportPrint.TryEnsureTransactionReportSchema(CompanyId);
                        clsTransactionReportDefaults.ClearSeededFrxMatchingGlobalStandard(CompanyId, 0);
                    }
                    catch
                    {
                        // Non-blocking
                    }
                    InsertDataBaseVersion(Simulate.decimal_(10.31), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.32))
                {
                    // Touch-screen POS login on the landing page (in-app keyboard). Default off.
                    AddColumnToTable(CompanyId, "tbl_Company", "EnableTouchScreenPosLogin", SQLColumnDataType.Bit);
                    ClsSQL.ExecuteNonQueryStatement(
                        @"UPDATE tbl_Company SET EnableTouchScreenPosLogin = 0 WHERE EnableTouchScreenPosLogin IS NULL",
                        ClsSQL.CreateDataBaseConnectionString(CompanyId), null);
                    InsertDataBaseVersion(Simulate.decimal_(10.32), CompanyId);
                }

                if (versionNumber < Simulate.decimal_(10.33))
                {
                    // POS-only users: after login they are limited to the POS screens.
                    AddColumnToTable(CompanyId, "tbl_employee", "IsPOSOnly", SQLColumnDataType.Bit);
                    ClsSQL.ExecuteNonQueryStatement(
                        @"UPDATE tbl_employee SET IsPOSOnly = 0 WHERE IsPOSOnly IS NULL",
                        ClsSQL.CreateDataBaseConnectionString(CompanyId), null);
                    InsertDataBaseVersion(Simulate.decimal_(10.33), CompanyId);
                }


            }

            catch (Exception ex)
            {

                throw;
            }
        }
        bool DropPrimaryKeyConstraintFromColumn(int CompanyID, string tableName, string columnName)
        {
            try
            {
                clsSQL clssql = new clsSQL();

                // Check if PK exists on this table AND includes this column
                string findPkQuery = $@"
DECLARE @tableName SYSNAME = N'{tableName}';
DECLARE @columnName SYSNAME = N'{columnName}';

SELECT TOP 1 kc.name
FROM sys.key_constraints kc
INNER JOIN sys.index_columns ic 
    ON ic.object_id = kc.parent_object_id
    AND ic.index_id = kc.unique_index_id
INNER JOIN sys.columns c
    ON c.object_id = ic.object_id
    AND c.column_id = ic.column_id
WHERE kc.[type] = 'PK'
  AND kc.parent_object_id = OBJECT_ID(@tableName)
  AND c.name = @columnName;
";

                object pkNameObj = clssql.ExecuteScalar(findPkQuery, clssql.CreateDataBaseConnectionString(CompanyID), null);
                string pkName = pkNameObj == null ? "" : pkNameObj.ToString();

                if (string.IsNullOrEmpty(pkName))
                {
                    // No PK found that includes this column
                    return false;
                }

                // Drop the PK constraint
                string dropPkQuery = $@"
DECLARE @tableName SYSNAME = N'{tableName}';
DECLARE @pkName SYSNAME = N'{pkName}';
DECLARE @sql NVARCHAR(MAX);

SET @sql = N'ALTER TABLE ' + QUOTENAME(@tableName) + N' DROP CONSTRAINT ' + QUOTENAME(@pkName);
EXEC sp_executesql @sql;
";

                DataTable dt = clssql.ExecuteQueryStatement(dropPkQuery, clssql.CreateDataBaseConnectionString(CompanyID));

                // In your pattern: if dt != null and has rows => true (but ALTER may return empty result)
                // So we return true if no exception occurred and PK existed.
                return true;
            }
            catch (Exception)
            {
                return false;
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
            catch (TypeInitializationException ex)
            {
                Console.WriteLine(ex.ToString());
                // or 
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }


        }
        public bool DropTable(string tableName, int companyId)
        {
            try
            {
                clsSQL clssql = new clsSQL();

                // Safeguard: Only drop the table if it exists
                string dropTableQuery = $@"
            IF OBJECT_ID('{tableName}', 'U') IS NOT NULL
            BEGIN
                DROP TABLE [{tableName}];
            END
        ";

                // Execute the drop statement
                DataTable dt = clssql.ExecuteQueryStatement(
                    dropTableQuery,
                    clssql.CreateDataBaseConnectionString(companyId)
                );

                // For DDL, the result set is typically empty (no rows).
                // Return true if we haven't hit any exceptions.
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool DropColumn(string tableName, string columnName, int companyId)
        {
            try
            {
                clsSQL clssql = new clsSQL();

                // Step 1: Dynamically drop any key constraint that includes the column.
                string dropConstraintSQL = $@"
DECLARE @constraintName NVARCHAR(200);
DECLARE @sql NVARCHAR(MAX);
-- Compute the quoted table name.
DECLARE @QuotedTableName NVARCHAR(128) = QUOTENAME(N'{tableName}');

-- Retrieve the constraint name that involves the column.
SELECT @constraintName = kc.name
FROM sys.key_constraints kc
INNER JOIN sys.index_columns ic 
    ON kc.parent_object_id = ic.object_id 
    AND kc.unique_index_id = ic.index_id
INNER JOIN sys.columns c 
    ON c.object_id = ic.object_id 
    AND c.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID(N'{tableName}')
  AND c.name = N'{columnName}';

-- If a constraint exists, build and execute the dynamic SQL to drop it.
IF @constraintName IS NOT NULL
BEGIN
    SET @sql = N'ALTER TABLE ' + @QuotedTableName + N' DROP CONSTRAINT ' + QUOTENAME(@constraintName);
    EXEC sp_executesql @sql;
END
";

                clssql.ExecuteQueryStatement(
                    dropConstraintSQL,
                    clssql.CreateDataBaseConnectionString(companyId)
                );

                // Step 2: Drop the column if it exists.
                string dropColumnSQL = $@"
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE Name = N'{columnName}' 
      AND Object_ID = OBJECT_ID(N'{tableName}')
)
BEGIN
    ALTER TABLE {tableName} DROP COLUMN {columnName};
END
";

                clssql.ExecuteQueryStatement(
                    dropColumnSQL,
                    clssql.CreateDataBaseConnectionString(companyId)
                );

                return true;
            }
            catch (Exception ex)
            {
                // Optionally log the exception details here.
                throw;
            }
        }


        public bool CreateTable(String TableName,int CompanyID) {

			try
			{
              clsSQL  clssql= new clsSQL();
                string createTableQuery = $@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{TableName}')
BEGIN
    CREATE TABLE [{TableName}] (
        ID INT PRIMARY KEY IDENTITY(1,1),
        CreationDate DATETIME DEFAULT GETDATE()
    );
END";



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
        
        public bool AddColumnToTable(int CompanyID,string tableName, string columnName, SQLColumnDataType columnType, int? varcharLength = null,bool? isPrimary=false)
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
                    case SQLColumnDataType.guid:
                        sqlColumnType = "UNIQUEIDENTIFIER";
                        break;
                    case SQLColumnDataType.Bit:
                    sqlColumnType = "BIT";
                    break;
                    case SQLColumnDataType.Time:
                        sqlColumnType = "TIME";
                        break;
                    case SQLColumnDataType.Binary:
                        sqlColumnType = "BINARY";
                        break;

                    case SQLColumnDataType.varbinarymax:
                        sqlColumnType = "varbinary(MAX)"; break;

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
                    if (Simulate.Bool( isPrimary) && columnType==SQLColumnDataType.guid) {
                          addColumnQuery = $@"
ALTER TABLE [{tableName}] 
ADD [{columnName}] {sqlColumnType} NOT NULL DEFAULT NEWID();

ALTER TABLE [{tableName}] 
ADD CONSTRAINT [PK_{tableName}_{columnName}] PRIMARY KEY([{columnName}]);
";
                    }
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
            string query = $@"DECLARE @tableName NVARCHAR(MAX) = '"+ tableName + @"';
DECLARE @columnName NVARCHAR(MAX) = '"+ columnName + @"';
DECLARE @constraintName NVARCHAR(MAX);
DECLARE @sql NVARCHAR(MAX);

/* STEP 1: Drop Foreign Key Constraints */
DECLARE cur CURSOR FOR 
SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID(@tableName) 
   OR referenced_object_id = OBJECT_ID(@tableName);

OPEN cur;
FETCH NEXT FROM cur INTO @constraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE ' + @tableName + ' DROP CONSTRAINT ' + @constraintName;
    EXEC sp_executesql @sql;
    FETCH NEXT FROM cur INTO @constraintName;
END

CLOSE cur;
DEALLOCATE cur;

/* STEP 2: Drop Primary Key Constraint */
SELECT @constraintName = name 
FROM sys.key_constraints 
WHERE type = 'PK' AND parent_object_id = OBJECT_ID(@tableName);

IF @constraintName IS NOT NULL
BEGIN
    SET @sql = 'ALTER TABLE ' + @tableName + ' DROP CONSTRAINT ' + @constraintName;
    EXEC sp_executesql @sql;
END

/* STEP 3: Drop Indexes on the Column */
DECLARE cur CURSOR FOR 
SELECT name 
FROM sys.indexes 
WHERE object_id = OBJECT_ID(@tableName) 
AND name IS NOT NULL;

OPEN cur;
FETCH NEXT FROM cur INTO @constraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'DROP INDEX ' + @constraintName + ' ON ' + @tableName;
    EXEC sp_executesql @sql;
    FETCH NEXT FROM cur INTO @constraintName;
END

CLOSE cur;
DEALLOCATE cur;

/* STEP 4: Drop the Column */
SET @sql = 'ALTER TABLE ' + @tableName + ' DROP COLUMN ' + @columnName;
EXEC sp_executesql @sql;
";

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

        /// <summary>
        /// Idempotent approval schema updates (amount bands, approver groups).
        /// Safe to call before approval API operations when DB migration has not run yet.
        /// Cached per company so POS / invoice saves do not re-run DDL and full-table UPDATEs.
        /// </summary>
        private static readonly ConcurrentDictionary<int, byte> _approvalSchemaReady =
            new ConcurrentDictionary<int, byte>();

        public void EnsureApprovalWorkflowSchema(int companyId)
        {
            if (_approvalSchemaReady.ContainsKey(companyId))
                return;

            lock (_approvalSchemaReady)
            {
                if (_approvalSchemaReady.ContainsKey(companyId))
                    return;

                clsSQL sql = new clsSQL();
                string con = sql.CreateDataBaseConnectionString(companyId);
                int policyTableExists = Simulate.Integer32(sql.ExecuteScalar(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_ApprovalPolicy'",
                    con, null));

                if (policyTableExists == 0)
                {
                    ApplyApprovalWorkflowSchema(companyId);
                }
                else
                {
                    ApplyApprovalPolicyLevelAmountSchema(companyId);
                    ApplyApprovalPolicyLevelGroupSchema(companyId);
                    ApplyCreditNoteApprovalSchema(companyId);
                    ApplyApprovalPolicyCreditDebitSchema(companyId);
                    ApplyInvoiceApprovalSchema(companyId);
                    ApplyHcmApprovalSchema(companyId);
                }

                _approvalSchemaReady[companyId] = 1;
            }
        }

        void ApplyHcmApprovalSchema(int companyId)
        {
            clsJournalVoucherTypes jvt = new clsJournalVoucherTypes();
            jvt.Inserttbl_JournalVoucherTypes(27, "عقد موظف", "Employee Contract", 0, companyId);
            jvt.Inserttbl_JournalVoucherTypes(28, "عنصر راتب موظف", "Employee Salary Element", 0, companyId);
            jvt.Inserttbl_JournalVoucherTypes(29, "فترة رواتب", "Payroll Period", 0, companyId);
            jvt.Inserttbl_JournalVoucherTypes(30, "تعيين وردية", "Employee Shift Assignment", 0, companyId);

            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);

            foreach (string table in new[]
            {
                "tbl_EmployeeContract",
                "tbl_EmployeeSalaryElements",
                "tbl_PayrollPeriod",
                "tbl_PayrollHeader",
                "tbl_EmployeeShiftAssignment",
            })
            {
                // Backfill only for newly added columns — avoid locking large tables on every ensure.
                bool addedGuid = AddColumnToTable(companyId, table, "Guid", SQLColumnDataType.guid);
                bool addedStatus = AddColumnToTable(companyId, table, "DocumentStatus", SQLColumnDataType.Integer);
                AddColumnToTable(companyId, table, "SubmittedByUserId", SQLColumnDataType.Integer);
                AddColumnToTable(companyId, table, "SubmittedDate", SQLColumnDataType.DateTime);
                AddColumnToTable(companyId, table, "PostedByUserId", SQLColumnDataType.Integer);
                AddColumnToTable(companyId, table, "PostedDate", SQLColumnDataType.DateTime);

                if (addedGuid)
                {
                    sql.ExecuteNonQueryStatement(
                        $"UPDATE {table} SET Guid = NEWID() WHERE Guid IS NULL;",
                        con, null);
                }
                if (addedStatus)
                {
                    sql.ExecuteNonQueryStatement(
                        $"UPDATE {table} SET DocumentStatus = 2 WHERE DocumentStatus IS NULL;",
                        con, null);
                }
            }

            int policyTableExists = Simulate.Integer32(sql.ExecuteScalar(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_ApprovalPolicy'",
                con, null));
            if (policyTableExists == 0) return;

            sql.ExecuteNonQueryStatement(@"
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 23)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (23, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 27)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (27, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 28)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (28, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 29)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (29, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 30)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (30, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
", con, new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        void AddHcmApprovalColumns(int companyId, string tableName)
        {
            AddColumnToTable(companyId, tableName, "Guid", SQLColumnDataType.guid);
            AddColumnToTable(companyId, tableName, "DocumentStatus", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, tableName, "SubmittedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, tableName, "SubmittedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, tableName, "PostedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, tableName, "PostedDate", SQLColumnDataType.DateTime);
        }

        void ApplyInvoiceApprovalSchema(int companyId)
        {
            bool addedStatus = AddColumnToTable(companyId, "tbl_InvoiceHeader", "DocumentStatus", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_InvoiceHeader", "SubmittedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_InvoiceHeader", "SubmittedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_InvoiceHeader", "PostedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_InvoiceHeader", "PostedDate", SQLColumnDataType.DateTime);

            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            // Only backfill when the column was just added — never on every invoice save.
            if (addedStatus)
            {
                sql.ExecuteNonQueryStatement(
                    @"UPDATE tbl_InvoiceHeader SET DocumentStatus = 2 WHERE DocumentStatus IS NULL;",
                    con, null);
            }

            int policyTableExists = Simulate.Integer32(sql.ExecuteScalar(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_ApprovalPolicy'",
                con, null));
            if (policyTableExists == 0) return;

            sql.ExecuteNonQueryStatement(@"
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 2)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (2, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 3)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (3, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 4)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (4, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 5)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (5, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 6)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (6, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 7)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (7, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 8)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (8, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 9)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (9, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 10)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (10, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 11)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (11, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 22)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (22, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
", con, new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        void ApplyApprovalPolicyCreditDebitSchema(int companyId)
        {
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            int policyTableExists = Simulate.Integer32(sql.ExecuteScalar(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_ApprovalPolicy'",
                con, null));
            if (policyTableExists == 0) return;

            sql.ExecuteNonQueryStatement(@"
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 20)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (20, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 21)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (21, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
", con, new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        void ApplyCreditNoteApprovalSchema(int companyId)
        {
            AddColumnToTable(companyId, "tbl_CreditNoteHeader", "DocumentStatus", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_CreditNoteHeader", "SubmittedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_CreditNoteHeader", "SubmittedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_CreditNoteHeader", "PostedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_CreditNoteHeader", "PostedDate", SQLColumnDataType.DateTime);

            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(
                @"UPDATE tbl_CreditNoteHeader SET DocumentStatus = 2 WHERE DocumentStatus IS NULL;",
                sql.CreateDataBaseConnectionString(companyId), null);
        }

        void ApplyApprovalWorkflowSchema(int companyId)
        {
            clsForms forms = new clsForms();
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);

            AddColumnToTable(companyId, "tbl_JournalVoucherHeader", "DocumentStatus", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_JournalVoucherHeader", "SubmittedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_JournalVoucherHeader", "SubmittedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_JournalVoucherHeader", "PostedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_JournalVoucherHeader", "PostedDate", SQLColumnDataType.DateTime);

            AddColumnToTable(companyId, "tbl_CashVoucherHeader", "DocumentStatus", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_CashVoucherHeader", "SubmittedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_CashVoucherHeader", "SubmittedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_CashVoucherHeader", "PostedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_CashVoucherHeader", "PostedDate", SQLColumnDataType.DateTime);

            AddColumnToTable(companyId, "tbl_UserAuthorization", "IsApprove", SQLColumnDataType.Bit);
            AddColumnToTable(companyId, "tbl_UserAuthorization", "IsReject", SQLColumnDataType.Bit);

            CreateTable("tbl_ApprovalPolicy", companyId);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "DocumentTypeID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "BranchID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "IsEnabled", SQLColumnDataType.Bit);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "MinAmount", SQLColumnDataType.Decimal);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "MaxAmount", SQLColumnDataType.Decimal);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "AllowSelfApproval", SQLColumnDataType.Bit);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "CompanyID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "CreationUserID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "CreationDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "ModificationUserID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicy", "ModificationDate", SQLColumnDataType.DateTime);

            CreateTable("tbl_ApprovalPolicyLevel", companyId);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "PolicyID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "LevelNo", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "LevelName", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "ApproverUserID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "MinApproversRequired", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "CompanyID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "CreationUserID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "CreationDate", SQLColumnDataType.DateTime);

            CreateTable("tbl_ApprovalRequest", companyId);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "Guid", SQLColumnDataType.guid);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "PolicyID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "DocumentTypeID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "DocumentGuid", SQLColumnDataType.guid);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "DocumentNumber", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "CurrentLevel", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "Status", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "TotalAmount", SQLColumnDataType.Decimal);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "BranchID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "SubmittedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "SubmittedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "FinalApprovedByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "FinalApprovedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "Comments", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "CompanyID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalRequest", "CreationDate", SQLColumnDataType.DateTime);

            CreateTable("tbl_ApprovalAction", companyId);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "RequestGuid", SQLColumnDataType.guid);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "DocumentGuid", SQLColumnDataType.guid);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "LevelNo", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "LevelName", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "ActionType", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "ActionByUserId", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "ActionDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "Comments", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalAction", "CompanyID", SQLColumnDataType.Integer);

            CreateTable("tbl_ApprovalNotification", companyId);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "UserID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "Title", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "Body", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "EntityType", SQLColumnDataType.VarChar);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "EntityGuid", SQLColumnDataType.guid);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "IsRead", SQLColumnDataType.Bit);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "CreatedDate", SQLColumnDataType.DateTime);
            AddColumnToTable(companyId, "tbl_ApprovalNotification", "CompanyID", SQLColumnDataType.Integer);

            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_JournalVoucherHeader SET DocumentStatus = 2 WHERE DocumentStatus IS NULL;
UPDATE tbl_CashVoucherHeader SET DocumentStatus = 2 WHERE DocumentStatus IS NULL;
UPDATE tbl_UserAuthorization SET IsApprove = 0 WHERE IsApprove IS NULL;
UPDATE tbl_UserAuthorization SET IsReject = 0 WHERE IsReject IS NULL;

IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 1)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (1, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 12)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (12, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 13)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (13, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 20)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (20, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_ApprovalPolicy WHERE CompanyID = @CompanyID AND DocumentTypeID = 21)
    INSERT INTO tbl_ApprovalPolicy (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval, CompanyID, CreationUserID, CreationDate)
    VALUES (21, 0, 0, 0, 0, 0, @CompanyID, 0, GETDATE());
", con, new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });

            forms.InsertFormIfNotExists(147, "ApprovalInboxPage", "صندوق الموافقات", "Approval Inbox", 52, true, false, false, false, false, false, companyId);
            forms.InsertFormIfNotExists(148, "ApprovalPolicySettingsPage", "سياسات الموافقة", "Approval Policies", 52, true, false, true, true, true, false, companyId);
            forms.InsertFormIfNotExists(149, "ApprovalNotificationsPage", "إشعارات الموافقة", "Approval Notifications", 52, true, false, false, false, false, false, companyId);
        }

        void ApplyApprovalPolicyLevelAmountSchema(int companyId)
        {
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "MinAmount", SQLColumnDataType.Decimal);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "MaxAmount", SQLColumnDataType.Decimal);
        }

        void ApplyApprovalPolicyLevelGroupSchema(int companyId)
        {
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);

            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevel", "RequireAllApprovers", SQLColumnDataType.Bit);

            CreateTable("tbl_ApprovalPolicyLevelMember", companyId);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevelMember", "PolicyLevelID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevelMember", "ApproverUserID", SQLColumnDataType.Integer);
            AddColumnToTable(companyId, "tbl_ApprovalPolicyLevelMember", "CompanyID", SQLColumnDataType.Integer);

            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_ApprovalPolicyLevel SET RequireAllApprovers = 0 WHERE RequireAllApprovers IS NULL;

INSERT INTO tbl_ApprovalPolicyLevelMember (PolicyLevelID, ApproverUserID, CompanyID)
SELECT l.ID, l.ApproverUserID, l.CompanyID
FROM tbl_ApprovalPolicyLevel l
WHERE l.CompanyID = @CompanyID
  AND ISNULL(l.ApproverUserID, 0) > 0
  AND NOT EXISTS (
      SELECT 1 FROM tbl_ApprovalPolicyLevelMember m
      WHERE m.PolicyLevelID = l.ID AND m.ApproverUserID = l.ApproverUserID AND m.CompanyID = l.CompanyID
  );", con, new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        void ApplyApprovalPolicyDefaultFlagsSchema(int companyId)
        {
            ApplyApprovalPolicyLevelGroupSchema(companyId);

            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);

            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_ApprovalPolicy SET IsEnabled = 0 WHERE IsEnabled IS NULL;
UPDATE tbl_ApprovalPolicy SET AllowSelfApproval = 0 WHERE AllowSelfApproval IS NULL;
UPDATE tbl_ApprovalPolicyLevel SET RequireAllApprovers = 0 WHERE RequireAllApprovers IS NULL;

UPDATE p SET p.IsEnabled = 0
FROM tbl_ApprovalPolicy p
WHERE p.CompanyID = @CompanyID
  AND p.IsEnabled = 1
  AND NOT EXISTS (
      SELECT 1
      FROM tbl_ApprovalPolicyLevel l
      INNER JOIN tbl_ApprovalPolicyLevelMember m
          ON m.PolicyLevelID = l.ID AND m.CompanyID = l.CompanyID
      WHERE l.PolicyID = p.ID AND l.CompanyID = p.CompanyID
  );", con, new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        void ApplyMainDatabaseAuditSchema()
        {
            AddColumnToTable(0, "tbl_Company", "IsActive", SQLColumnDataType.Bit);
            AddColumnToTable(0, "tbl_Company", "IsSuspended", SQLColumnDataType.Bit);
            AddColumnToTable(0, "tbl_Company", "AdminNotes", SQLColumnDataType.VarChar);
            AddColumnToTable(0, "tbl_Company", "SubscriptionExpiry", SQLColumnDataType.DateTime);
            AddColumnToTable(0, "tbl_Company", "LastLoginDate", SQLColumnDataType.DateTime);
            AddColumnToTable(0, "tbl_Company", "LastActivityDate", SQLColumnDataType.DateTime);
            AddColumnToTable(0, "tbl_Company", "DBVersionCache", SQLColumnDataType.Decimal);
            AddColumnToTable(0, "tbl_Company", "ActiveUserCount", SQLColumnDataType.Integer);

            CreateTable("tbl_AuditCompanySnapshot", 0);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "CompanyID", SQLColumnDataType.Integer, isPrimary: true);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "CompanyAName", SQLColumnDataType.VarChar);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "CompanyEName", SQLColumnDataType.VarChar);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "DataBaseName", SQLColumnDataType.VarChar);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "LastLoginDate", SQLColumnDataType.DateTime);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "LastActivityDate", SQLColumnDataType.DateTime);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "ActiveSessionsToday", SQLColumnDataType.Integer);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "TotalUsers", SQLColumnDataType.Integer);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "DBVersion", SQLColumnDataType.Decimal);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "IsActive", SQLColumnDataType.Bit);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "IsSuspended", SQLColumnDataType.Bit);
            AddColumnToTable(0, "tbl_AuditCompanySnapshot", "UpdatedAt", SQLColumnDataType.DateTime);

            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_Company SET IsActive = 1 WHERE IsActive IS NULL;
UPDATE tbl_Company SET IsSuspended = 0 WHERE IsSuspended IS NULL;
", sql.MainDataBaseconString, null);
        }

        void SeedAuditActionTypes(int companyId)
        {
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            string seed = @"
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Login' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Login', N'تسجيل دخول', 'Login', 'Security', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Logout' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Logout', N'تسجيل خروج', 'Logout', 'Security', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'LoginFailed' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('LoginFailed', N'فشل تسجيل الدخول', 'Login Failed', 'Security', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Insert' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Insert', N'إضافة', 'Insert', 'Transaction', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Update' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Update', N'تعديل', 'Update', 'Transaction', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Delete' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Delete', N'حذف', 'Delete', 'Transaction', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Print' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Print', N'طباعة', 'Print', 'Report', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Report' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Report', N'تقرير', 'Report', 'Report', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'Export' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('Export', N'تصدير', 'Export', 'Report', @CompanyID, 0, GETDATE());
IF NOT EXISTS (SELECT 1 FROM tbl_AuditActionType WHERE Code = 'View' AND CompanyID = @CompanyID)
    INSERT INTO tbl_AuditActionType (Code, AName, EName, Category, CompanyID, CreationUserID, CreationDate)
    VALUES ('View', N'عرض', 'View', 'Transaction', @CompanyID, 0, GETDATE());
";
            SqlParameter[] prm = { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } };
            sql.ExecuteNonQueryStatement(seed, con, prm);
        }

        void PrepareAuditEventColumnsForIndexes(int companyId)
        {
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            sql.ExecuteNonQueryStatement(@"
IF OBJECT_ID('tbl_AuditEvent', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tbl_AuditEvent') AND name = 'ModuleName')
        ALTER TABLE tbl_AuditEvent ALTER COLUMN ModuleName NVARCHAR(100) NULL;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tbl_AuditEvent') AND name = 'EntityTable')
        ALTER TABLE tbl_AuditEvent ALTER COLUMN EntityTable NVARCHAR(100) NULL;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tbl_AuditEvent') AND name = 'RecordReference')
        ALTER TABLE tbl_AuditEvent ALTER COLUMN RecordReference NVARCHAR(200) NULL;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tbl_AuditEvent') AND name = 'Description')
        ALTER TABLE tbl_AuditEvent ALTER COLUMN Description NVARCHAR(500) NULL;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tbl_AuditEvent') AND name = 'ActionTypeCode')
        ALTER TABLE tbl_AuditEvent ALTER COLUMN ActionTypeCode NVARCHAR(50) NULL;
END
", con, null);
        }

        void BackupAndDeleteZeroAmountReconciliationRows(int companyId)
        {
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            sql.ExecuteNonQueryStatement(@"
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'tbl_Reconciliation_ZeroAmountBackup'
)
BEGIN
    CREATE TABLE [dbo].[tbl_Reconciliation_ZeroAmountBackup](
        [BackupID] [int] IDENTITY(1,1) NOT NULL,
        [Guid] [uniqueidentifier] NOT NULL,
        [VoucherNumber] [int] NULL,
        [JVDetailsGuid] [uniqueidentifier] NULL,
        [Amount] [decimal](18, 3) NULL,
        [CompanyID] [int] NULL,
        [CreationUserID] [int] NULL,
        [CreationDate] [datetime] NULL,
        [ModulleID] [int] NULL,
        [TransactionGuid] [uniqueidentifier] NULL,
        [BackedUpDate] [datetime] NOT NULL CONSTRAINT [DF_tbl_Reconciliation_ZeroAmountBackup_BackedUpDate] DEFAULT (GETDATE()),
        [MigrationVersion] [decimal](18, 2) NOT NULL,
        CONSTRAINT [PK_tbl_Reconciliation_ZeroAmountBackup] PRIMARY KEY CLUSTERED ([BackupID] ASC)
    );
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'tbl_Reconciliation')
BEGIN
    INSERT INTO tbl_Reconciliation_ZeroAmountBackup
        (Guid, VoucherNumber, JVDetailsGuid, Amount, CompanyID, CreationUserID, CreationDate, ModulleID, TransactionGuid, MigrationVersion)
    SELECT
        r.Guid, r.VoucherNumber, r.JVDetailsGuid, r.Amount, r.CompanyID, r.CreationUserID, r.CreationDate, r.ModulleID, r.TransactionGuid, 10.29
    FROM tbl_Reconciliation r
    WHERE ISNULL(r.Amount, 0) = 0
      AND (r.CompanyID = @CompanyID OR @CompanyID = 0)
      AND NOT EXISTS (
          SELECT 1 FROM tbl_Reconciliation_ZeroAmountBackup b WHERE b.Guid = r.Guid
      );

    DELETE r
    FROM tbl_Reconciliation r
    WHERE ISNULL(r.Amount, 0) = 0
      AND (r.CompanyID = @CompanyID OR @CompanyID = 0);
END
", con, prm);
        }

        void CreateAuditIndexes(int companyId)
        {
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            sql.ExecuteNonQueryStatement(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditUserSession_CompanyUserLogin' AND object_id = OBJECT_ID('tbl_AuditUserSession'))
    CREATE INDEX IX_AuditUserSession_CompanyUserLogin ON tbl_AuditUserSession (CompanyID, UserID, LoginTime DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditUserSession_SessionGuid' AND object_id = OBJECT_ID('tbl_AuditUserSession'))
    CREATE INDEX IX_AuditUserSession_SessionGuid ON tbl_AuditUserSession (SessionGuid);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditUserSession_CompanyActive' AND object_id = OBJECT_ID('tbl_AuditUserSession'))
    CREATE INDEX IX_AuditUserSession_CompanyActive ON tbl_AuditUserSession (CompanyID, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditEvent_CompanyDate' AND object_id = OBJECT_ID('tbl_AuditEvent'))
    CREATE INDEX IX_AuditEvent_CompanyDate ON tbl_AuditEvent (CompanyID, EventDateTime DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditEvent_CompanyUserDate' AND object_id = OBJECT_ID('tbl_AuditEvent'))
    CREATE INDEX IX_AuditEvent_CompanyUserDate ON tbl_AuditEvent (CompanyID, UserID, EventDateTime DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditEvent_CompanyActionDate' AND object_id = OBJECT_ID('tbl_AuditEvent'))
    CREATE INDEX IX_AuditEvent_CompanyActionDate ON tbl_AuditEvent (CompanyID, ActionTypeCode, EventDateTime DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditEvent_CompanyModuleRecord' AND object_id = OBJECT_ID('tbl_AuditEvent'))
    CREATE INDEX IX_AuditEvent_CompanyModuleRecord ON tbl_AuditEvent (CompanyID, ModuleName, RecordID);
", con, null);
        }
}
}