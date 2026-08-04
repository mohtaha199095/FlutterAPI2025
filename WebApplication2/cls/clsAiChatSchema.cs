using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// Central ERP schema dictionary for AI chat SQL.
    /// Column logical keys map to physical names — update here when DB columns change.
    /// </summary>
    public static class clsAiChatSchema
    {
        public sealed class TableDef
        {
            public string Key { get; init; } = "";
            public string Name { get; init; } = "";
            public string DescriptionEn { get; init; } = "";
            public string DescriptionAr { get; init; } = "";
            public string[] Synonyms { get; init; } = Array.Empty<string>();
            public Dictionary<string, string> Columns { get; init; } = new();
            public string PkColumn => Columns.TryGetValue("Pk", out string pk) ? pk : "ID";

            public string Col(string logicalKey) =>
                Columns.TryGetValue(logicalKey, out string col) ? col : logicalKey;
        }

        public sealed class JoinDef
        {
            public string Key { get; init; } = "";
            public string FromTable { get; init; } = "";
            public string FromColumn { get; init; } = "";
            public string ToTable { get; init; } = "";
            public string ToColumn { get; init; } = "";
            public string JoinType { get; init; } = "INNER";
            public string Description { get; init; } = "";
        }

        public sealed class SubAccountRule
        {
            public int AccountRefId { get; init; }
            public string TargetTable { get; init; } = "";
            public string LabelEn { get; init; } = "";
            public string LabelAr { get; init; } = "";
        }

        public static readonly IReadOnlyDictionary<string, TableDef> Tables = BuildTables();
        public static readonly IReadOnlyList<JoinDef> Joins = BuildJoins();
        public static readonly IReadOnlyList<SubAccountRule> SubAccountRules = BuildSubAccountRules();

        /// <summary>Shared document type table — used for both JVTypeID and InvoiceTypeID.</summary>
        public static string DocTypeTable => T("JvTypes");

        public static class DocTypes
        {
            public static readonly int[] Sales = { (int)clsEnum.VoucherType.SalesInvoice, (int)clsEnum.VoucherType.POSSalesInvoice };
            public static readonly int[] SalesReturns = { (int)clsEnum.VoucherType.SalesRefund, (int)clsEnum.VoucherType.POSSalesInvoicereturn };
            public static readonly int[] Purchases = { (int)clsEnum.VoucherType.PurchaseInvoice, (int)clsEnum.VoucherType.PurchaseInvoiceFromFinancing };
            public static readonly int[] PurchaseReturns = { (int)clsEnum.VoucherType.PurchaseRefund };
            public static readonly int[] SalesOffers = { (int)clsEnum.VoucherType.SalesOffer };
            public static readonly int[] PurchaseOffers = { (int)clsEnum.VoucherType.PurchaseOffer };
            public static readonly int[] CashPayments = { (int)clsEnum.VoucherType.CashPayment, (int)clsEnum.VoucherType.POSCashPayment };
            public static readonly int[] CashReceipts = { (int)clsEnum.VoucherType.Cashrecivable, (int)clsEnum.VoucherType.POSCashRecipt };
            public static readonly int[] Financing = { (int)clsEnum.VoucherType.Finance, (int)clsEnum.VoucherType.LoanScheduling };

            public static string InClause(int[] ids) => string.Join(", ", ids);
        }

        public static class AccountRefs
        {
            public const int Cash = (int)clsEnum.AccountMainSetting.CashAccount;
            public const int Vendor = (int)clsEnum.AccountMainSetting.VendorAccount;
            public const int Customer = (int)clsEnum.AccountMainSetting.CustomerAccount;
            public const int Banks = (int)clsEnum.AccountMainSetting.Banks;
        }

        public static string T(string tableKey) =>
            Tables.TryGetValue(tableKey, out TableDef t) ? t.Name : tableKey;

        public static string C(string tableKey, string columnKey) =>
            Tables.TryGetValue(tableKey, out TableDef t) ? t.Col(columnKey) : columnKey;

        public static string Qualify(string tableKey, string columnKey, string alias = null)
        {
            string col = C(tableKey, columnKey);
            return string.IsNullOrWhiteSpace(alias) ? col : $"{alias}.{col}";
        }

        public static string GlBalanceExpression(string detailAlias = "d") =>
            $"ISNULL(SUM(ISNULL({detailAlias}.{C("JvDetail", "Total")}, ISNULL({detailAlias}.{C("JvDetail", "Debit")}, 0) - ISNULL({detailAlias}.{C("JvDetail", "Credit")}, 0))), 0)";

        public static string GlDebitExpression(string detailAlias = "d") =>
            $"ISNULL(SUM(ISNULL({detailAlias}.{C("JvDetail", "Debit")}, 0)), 0)";

        public static string GlCreditExpression(string detailAlias = "d") =>
            $"ISNULL(SUM(ISNULL({detailAlias}.{C("JvDetail", "Credit")}, 0)), 0)";

        public static string CompanyFilter(string alias, string paramName = "@CompanyId") =>
            $"({alias}.{C("JvDetail", "CompanyId")} = {paramName} OR {paramName} = 0)";

        public static string GetPromptSummary(string lang)
        {
            bool ar = clsAiChatLanguage.IsArabic(lang);
            var sb = new StringBuilder();

            if (ar)
            {
                sb.AppendLine("## نموذج البيانات (ERP)");
                sb.AppendLine("- **مصدر الأرصدة:** tbl_JournalVoucherHeader + tbl_JournalVoucherDetails (ParentGuid → Guid)");
                sb.AppendLine("- **SubAccountID:** يفلتر حسب نوع الحساب — عميل/مورد/بنك/صندوق");
                sb.AppendLine("- **الفواتير:** tbl_InvoiceHeader + tbl_InvoiceDetails، النوع عبر InvoiceTypeID → tbl_JournalVoucherTypes");
                sb.AppendLine("- **التمويل/القروض:** tbl_FinancingHeader + tbl_FinancingDetails");
                sb.AppendLine("- **التسوية:** tbl_Reconciliation يربط JVDetailsGuid بالمبالغ المسددة");
                sb.AppendLine($"- **مبيعات:** InvoiceTypeID IN ({DocTypes.InClause(DocTypes.Sales)})");
                sb.AppendLine($"- **مشتريات:** InvoiceTypeID IN ({DocTypes.InClause(DocTypes.Purchases)})");
            }
            else
            {
                sb.AppendLine("## ERP data model");
                sb.AppendLine("- **Balances source of truth:** tbl_JournalVoucherHeader + tbl_JournalVoucherDetails (ParentGuid → Guid)");
                sb.AppendLine("- **SubAccountID:** filters by sub-ledger — customer/vendor/bank/cash drawer depending on AccountID");
                sb.AppendLine("- **Invoices:** tbl_InvoiceHeader + tbl_InvoiceDetails; type via InvoiceTypeID → tbl_JournalVoucherTypes");
                sb.AppendLine("- **Financing / loans:** tbl_FinancingHeader + tbl_FinancingDetails");
                sb.AppendLine("- **Settlements:** tbl_Reconciliation maps JVDetailsGuid to settled amounts");
                sb.AppendLine($"- **Sales:** InvoiceTypeID IN ({DocTypes.InClause(DocTypes.Sales)})");
                sb.AppendLine($"- **Purchases:** InvoiceTypeID IN ({DocTypes.InClause(DocTypes.Purchases)})");
            }

            sb.AppendLine();
            sb.AppendLine(ar ? "### جداول رئيسية" : "### Core tables");
            foreach (TableDef table in Tables.Values.Where(t => t.Key is "JvHeader" or "JvDetail" or "Accounts" or "BusinessPartner" or "InvoiceHeader" or "FinancingHeader" or "Reconciliation"))
            {
                sb.AppendLine($"- **{table.Name}** — {(ar ? table.DescriptionAr : table.DescriptionEn)}");
            }

            return sb.ToString().Trim();
        }

        public static string GetSchemaJson() =>
            JsonConvert.SerializeObject(new
            {
                tables = Tables.Values.Select(t => new
                {
                    key = t.Key,
                    name = t.Name,
                    description = t.DescriptionEn,
                    synonyms = t.Synonyms,
                    columns = t.Columns
                }),
                joins = Joins.Select(j => new
                {
                    j.Key,
                    j.FromTable,
                    j.FromColumn,
                    j.ToTable,
                    j.ToColumn,
                    j.JoinType,
                    j.Description
                }),
                subAccountRules = SubAccountRules.Select(r => new
                {
                    r.AccountRefId,
                    r.TargetTable,
                    r.LabelEn
                }),
                documentTypes = new
                {
                    table = DocTypeTable,
                    sales = DocTypes.Sales,
                    salesReturns = DocTypes.SalesReturns,
                    purchases = DocTypes.Purchases,
                    purchaseReturns = DocTypes.PurchaseReturns,
                    financing = DocTypes.Financing
                },
                measures = new
                {
                    glBalance = GlBalanceExpression("d"),
                    invoiceTotal = $"SUM({C("InvoiceHeader", "TotalInvoice")})"
                }
            }, Formatting.Indented);

        private static Dictionary<string, TableDef> BuildTables() =>
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["JvHeader"] = new TableDef
                {
                    Key = "JvHeader",
                    Name = "tbl_JournalVoucherHeader",
                    DescriptionEn = "Journal voucher header — all GL transactions post here",
                    DescriptionAr = "رأس سند القيد — كل الحركات المحاسبية",
                    Synonyms = new[] { "journal voucher", "jv", "قيد", "سند قيد" },
                    Columns = Cols(
                        ("Pk", "Guid"), ("Date", "VoucherDate"), ("Number", "JVNumber"),
                        ("TypeId", "JVTypeID"), ("BranchId", "BranchID"), ("CostCenterId", "CostCenterID"),
                        ("Notes", "Notes"), ("CompanyId", "CompanyID"), ("FinancingGuid", "RelatedFinancingHeaderGuid"))
                },
                ["JvDetail"] = new TableDef
                {
                    Key = "JvDetail",
                    Name = "tbl_JournalVoucherDetails",
                    DescriptionEn = "Journal voucher lines — debit/credit/balance lines",
                    DescriptionAr = "تفاصيل سند القيد — مدين/دائن/رصيد",
                    Synonyms = new[] { "jv detail", "journal line", "قيد تفصيلي" },
                    Columns = Cols(
                        ("Pk", "Guid"), ("ParentFk", "ParentGuid"), ("RowIndex", "RowIndex"),
                        ("AccountId", "AccountID"), ("SubAccountId", "SubAccountID"),
                        ("Debit", "Debit"), ("Credit", "Credit"), ("Total", "Total"),
                        ("DueDate", "DueDate"), ("Note", "Note"), ("BranchId", "BranchID"),
                        ("CostCenterId", "CostCenterID"), ("CompanyId", "CompanyID"),
                        ("RelatedDetailGuid", "RelatedDetailsGuid"))
                },
                ["Accounts"] = new TableDef
                {
                    Key = "Accounts",
                    Name = "tbl_Accounts",
                    DescriptionEn = "Chart of accounts (GL master)",
                    DescriptionAr = "دليل الحسابات",
                    Synonyms = new[] { "chart of accounts", "gl account", "ledger", "حساب", "دليل الحسابات" },
                    Columns = Cols(
                        ("Pk", "ID"), ("ParentId", "ParentID"), ("Number", "AccountNumber"),
                        ("NameAr", "AName"), ("NameEn", "EName"), ("CompanyId", "CompanyID"),
                        ("NatureId", "AccountNatureID"), ("IsSubLedger", "IsSubLedger"))
                },
                ["AccountSetting"] = new TableDef
                {
                    Key = "AccountSetting",
                    Name = "tbl_AccountSetting",
                    DescriptionEn = "Maps logical account roles (AR, AP, Cash, Bank) to GL AccountID",
                    DescriptionAr = "ربط أدوار الحسابات (عملاء، موردين، صندوق، بنك) بمعرف الحساب",
                    Columns = Cols(
                        ("Pk", "ID"), ("AccountRefId", "AccountRefID"), ("AccountId", "AccountID"),
                        ("CompanyId", "CompanyID"), ("Active", "Active"))
                },
                ["BusinessPartner"] = new TableDef
                {
                    Key = "BusinessPartner",
                    Name = "tbl_BusinessPartner",
                    DescriptionEn = "Customers and vendors master data",
                    DescriptionAr = "بيانات العملاء والموردين",
                    Synonyms = new[] { "customer", "vendor", "client", "supplier", "عميل", "مورد" },
                    Columns = Cols(
                        ("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"),
                        ("CommercialName", "CommercialName"), ("Tel", "Tel"), ("Email", "Email"),
                        ("Active", "Active"), ("Type", "Type"), ("CompanyId", "CompanyID"))
                },
                ["InvoiceHeader"] = new TableDef
                {
                    Key = "InvoiceHeader",
                    Name = "tbl_InvoiceHeader",
                    DescriptionEn = "Sales/purchase/POS invoice headers",
                    DescriptionAr = "رأس الفاتورة (مبيعات/مشتريات/POS)",
                    Synonyms = new[] { "invoice", "sales invoice", "purchase invoice", "فاتورة" },
                    Columns = Cols(
                        ("Pk", "Guid"), ("Number", "InvoiceNo"), ("Date", "InvoiceDate"),
                        ("PartnerId", "BusinessPartnerID"), ("TypeId", "InvoiceTypeID"),
                        ("TotalInvoice", "TotalInvoice"), ("TotalTax", "TotalTax"),
                        ("JvGuid", "JVGuid"), ("Status", "Status"), ("BranchId", "BranchID"),
                        ("CashId", "CashID"), ("BankId", "BankID"), ("CompanyId", "CompanyID"),
                        ("IsPosted", "IsPosted"), ("RefNo", "RefNo"))
                },
                ["InvoiceDetail"] = new TableDef
                {
                    Key = "InvoiceDetail",
                    Name = "tbl_InvoiceDetails",
                    DescriptionEn = "Invoice line items",
                    DescriptionAr = "تفاصيل الفاتورة",
                    Columns = Cols(
                        ("Pk", "Guid"), ("HeaderFk", "HeaderGuid"), ("ItemGuid", "ItemGuid"),
                        ("RowIndex", "RowIndex"), ("Qty", "Qty"), ("TotalLine", "TotalLine"),
                        ("TypeId", "InvoiceTypeID"), ("PartnerId", "BusinessPartnerID"),
                        ("CompanyId", "CompanyID"))
                },
                ["JvTypes"] = new TableDef
                {
                    Key = "JvTypes",
                    Name = "tbl_JournalVoucherTypes",
                    DescriptionEn = "Document type lookup (JV + invoice types)",
                    DescriptionAr = "أنواع المستندات (قيود + فواتير)",
                    Columns = Cols(("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"), ("QtyFactor", "QTYFactor"))
                },
                ["FinancingHeader"] = new TableDef
                {
                    Key = "FinancingHeader",
                    Name = "tbl_FinancingHeader",
                    DescriptionEn = "Cash loans and installment sales headers",
                    DescriptionAr = "رأس التمويل — قروض نقدية ومبيعات بالتقسيط",
                    Synonyms = new[] { "financing", "loan", "installment", "تمويل", "قرض", "تقسيط" },
                    Columns = Cols(
                        ("Pk", "Guid"), ("Date", "VoucherDate"), ("Number", "VoucherNumber"),
                        ("PartnerId", "BusinessPartnerID"), ("VendorId", "VendorID"),
                        ("TotalAmount", "TotalAmount"), ("NetAmount", "NetAmount"),
                        ("DownPayment", "DownPayment"), ("LoanType", "LoanType"),
                        ("JvGuid", "JVGuid"), ("PaymentAccountId", "PaymentAccountID"),
                        ("PaymentSubAccountId", "PaymentSubAccountID"), ("CompanyId", "CompanyID"))
                },
                ["FinancingDetail"] = new TableDef
                {
                    Key = "FinancingDetail",
                    Name = "tbl_FinancingDetails",
                    DescriptionEn = "Installment schedule lines",
                    DescriptionAr = "جدول أقساط التمويل",
                    Columns = Cols(
                        ("Pk", "Guid"), ("HeaderFk", "HeaderGuid"), ("RowIndex", "RowIndex"),
                        ("Description", "Description"), ("InstallmentAmount", "InstallmentAmount"),
                        ("TotalWithInterest", "TotalAmountWithInterest"), ("PeriodMonths", "PeriodInMonths"),
                        ("FirstDate", "FirstInstallmentDate"), ("JvGuid", "JVGuid"), ("CompanyId", "CompanyID"))
                },
                ["Reconciliation"] = new TableDef
                {
                    Key = "Reconciliation",
                    Name = "tbl_Reconciliation",
                    DescriptionEn = "Settled debit/collection mapping to JV detail lines",
                    DescriptionAr = "تسوية المدين — ربط المبالغ المسددة بتفاصيل القيد",
                    Synonyms = new[] { "reconciliation", "settlement", "settled", "تسوية", "مسدد" },
                    Columns = Cols(
                        ("Pk", "Guid"), ("JvDetailFk", "JVDetailsGuid"), ("Amount", "Amount"),
                        ("VoucherNumber", "VoucherNumber"), ("TransactionGuid", "TransactionGuid"),
                        ("CompanyId", "CompanyID"))
                },
                ["Banks"] = new TableDef
                {
                    Key = "Banks",
                    Name = "tbl_Banks",
                    DescriptionEn = "Bank sub-ledger (SubAccountID target when account is bank GL)",
                    DescriptionAr = "دفتر البنوك الفرعي",
                    Columns = Cols(("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"), ("CompanyId", "CompanyID"))
                },
                ["CashDrawer"] = new TableDef
                {
                    Key = "CashDrawer",
                    Name = "tbl_CashDrawer",
                    DescriptionEn = "Cash drawer sub-ledger (SubAccountID target for cash GL)",
                    DescriptionAr = "دفتر الصندوق الفرعي",
                    Columns = Cols(("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"), ("CompanyId", "CompanyID"))
                },
                ["Items"] = new TableDef
                {
                    Key = "Items",
                    Name = "tbl_Items",
                    DescriptionEn = "Products / inventory items",
                    DescriptionAr = "الأصناف / المخزون",
                    Columns = Cols(("Pk", "Guid"), ("NameAr", "AName"), ("NameEn", "EName"), ("CompanyId", "CompanyID"))
                },
                ["CashVoucherHeader"] = new TableDef
                {
                    Key = "CashVoucherHeader",
                    Name = "tbl_CashVoucherHeader",
                    DescriptionEn = "Cash payment/receipt vouchers",
                    DescriptionAr = "سندات الصرف والقبض",
                    Columns = Cols(("Pk", "Guid"), ("JvGuid", "JVGuid"), ("CompanyId", "CompanyID"))
                },
                ["Employee"] = new TableDef
                {
                    Key = "Employee",
                    Name = "tbl_employee",
                    DescriptionEn = "Employees master",
                    DescriptionAr = "الموظفين",
                    Columns = Cols(("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"))
                },
                ["Branch"] = new TableDef
                {
                    Key = "Branch",
                    Name = "tbl_Branch",
                    DescriptionEn = "Branches",
                    DescriptionAr = "الفروع",
                    Columns = Cols(("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"))
                },
                ["Department"] = new TableDef
                {
                    Key = "Department",
                    Name = "tbl_Department",
                    DescriptionEn = "Departments",
                    DescriptionAr = "الأقسام",
                    Columns = Cols(("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"))
                },
                ["CostCenter"] = new TableDef
                {
                    Key = "CostCenter",
                    Name = "tbl_CostCenter",
                    DescriptionEn = "Cost centers",
                    DescriptionAr = "مراكز التكلفة",
                    Columns = Cols(("Pk", "ID"), ("NameAr", "AName"), ("NameEn", "EName"))
                }
            };

        private static List<JoinDef> BuildJoins() => new()
        {
            new JoinDef
            {
                Key = "JvHeader_Detail",
                FromTable = T("JvHeader"), FromColumn = C("JvHeader", "Pk"),
                ToTable = T("JvDetail"), ToColumn = C("JvDetail", "ParentFk"),
                JoinType = "INNER",
                Description = "Header to detail lines via Guid/ParentGuid"
            },
            new JoinDef
            {
                Key = "InvoiceHeader_Partner",
                FromTable = T("InvoiceHeader"), FromColumn = C("InvoiceHeader", "PartnerId"),
                ToTable = T("BusinessPartner"), ToColumn = C("BusinessPartner", "Pk"),
                JoinType = "INNER",
                Description = "Invoice to customer/vendor"
            },
            new JoinDef
            {
                Key = "InvoiceHeader_Type",
                FromTable = T("InvoiceHeader"), FromColumn = C("InvoiceHeader", "TypeId"),
                ToTable = T("JvTypes"), ToColumn = C("JvTypes", "Pk"),
                JoinType = "INNER",
                Description = "Invoice type lookup"
            },
            new JoinDef
            {
                Key = "InvoiceHeader_Jv",
                FromTable = T("InvoiceHeader"), FromColumn = C("InvoiceHeader", "JvGuid"),
                ToTable = T("JvHeader"), ToColumn = C("JvHeader", "Pk"),
                JoinType = "LEFT",
                Description = "Posted invoice to GL journal"
            },
            new JoinDef
            {
                Key = "InvoiceDetail_Header",
                FromTable = T("InvoiceDetail"), FromColumn = C("InvoiceDetail", "HeaderFk"),
                ToTable = T("InvoiceHeader"), ToColumn = C("InvoiceHeader", "Pk"),
                JoinType = "INNER",
                Description = "Invoice lines to header"
            },
            new JoinDef
            {
                Key = "JvDetail_Account",
                FromTable = T("JvDetail"), FromColumn = C("JvDetail", "AccountId"),
                ToTable = T("Accounts"), ToColumn = C("Accounts", "Pk"),
                JoinType = "INNER",
                Description = "JV line to chart of accounts"
            },
            new JoinDef
            {
                Key = "Reconciliation_JvDetail",
                FromTable = T("Reconciliation"), FromColumn = C("Reconciliation", "JvDetailFk"),
                ToTable = T("JvDetail"), ToColumn = C("JvDetail", "Pk"),
                JoinType = "INNER",
                Description = "Settlement to open JV line"
            },
            new JoinDef
            {
                Key = "FinancingDetail_Header",
                FromTable = T("FinancingDetail"), FromColumn = C("FinancingDetail", "HeaderFk"),
                ToTable = T("FinancingHeader"), ToColumn = C("FinancingHeader", "Pk"),
                JoinType = "INNER",
                Description = "Installment lines to financing header"
            },
            new JoinDef
            {
                Key = "FinancingHeader_Jv",
                FromTable = T("FinancingHeader"), FromColumn = C("FinancingHeader", "JvGuid"),
                ToTable = T("JvHeader"), ToColumn = C("JvHeader", "Pk"),
                JoinType = "LEFT",
                Description = "Financing document to GL"
            }
        };

        private static List<SubAccountRule> BuildSubAccountRules() => new()
        {
            new SubAccountRule { AccountRefId = AccountRefs.Customer, TargetTable = T("BusinessPartner"), LabelEn = "Customer AR sub-ledger", LabelAr = "عملاء — ذمم مدينة" },
            new SubAccountRule { AccountRefId = AccountRefs.Vendor, TargetTable = T("BusinessPartner"), LabelEn = "Vendor AP sub-ledger", LabelAr = "موردين — ذمم دائنة" },
            new SubAccountRule { AccountRefId = AccountRefs.Banks, TargetTable = T("Banks"), LabelEn = "Bank sub-ledger", LabelAr = "بنوك" },
            new SubAccountRule { AccountRefId = AccountRefs.Cash, TargetTable = T("CashDrawer"), LabelEn = "Cash drawer sub-ledger", LabelAr = "صندوق" }
        };

        private static Dictionary<string, string> Cols(params (string key, string col)[] pairs) =>
            pairs.ToDictionary(p => p.key, p => p.col, StringComparer.OrdinalIgnoreCase);
    }
}
