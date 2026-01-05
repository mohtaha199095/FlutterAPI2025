using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlDocumentFlowService")]
    public class ctlDocumentFlowService : Controller
    {
        // =========================
        // Constants
        // =========================
        // Doc Types (ثبتهم)
        private const int DocType_Invoice = 100;
        private const int DocType_CashVoucher = 200;

        // Flow Types
        private const int FlowType_Finance = 1;

        // Flow Actions
        private const int FlowAction_PaymentAllocation = 30;

        // Status
        private const int Status_Open = 1;
        private const int Status_Closed = 2;

        // ==========================================================
        // 1) Allocate CashVoucher to Multiple Invoices (DocumentFlow only)
        // Body JSON:
        // {
        //   "CashVoucherGuid":"...",
        //   "ReferenceNo":"PAY-001",
        //   "Notes":"Allocation notes",
        //   "CompanyID":1,
        //   "UserID":5,
        //   "Rows":[
        //     {"InvoiceGuid":"...","Amount":250,"CurrencyID":784,"Rate":1,"Notes":"Part 1"},
        //     {"InvoiceGuid":"...","Amount":450,"CurrencyID":784,"Rate":1,"Notes":"Part 2"}
        //   ]
        // }
        // ==========================================================
        [HttpPost]
        [Route("AllocateCashVoucherToInvoices")]
        public bool AllocateCashVoucherToInvoices([FromBody] dynamic body)
        {
            try
            {
                if (body == null) throw new Exception("Body is null");

                string json = body.ToString();
                var obj = JObject.Parse(json);

                Guid cashVoucherGuid = Guid.Parse(obj["CashVoucherGuid"]!.ToString());

                string referenceNo = obj["ReferenceNo"] != null ? obj["ReferenceNo"]!.ToString() : "";
                string notes = obj["Notes"] != null ? obj["Notes"]!.ToString() : "";

                int companyID = obj["CompanyID"] != null ? Simulate.Integer32(obj["CompanyID"]) : 0;
                int userID = obj["UserID"] != null ? Simulate.Integer32(obj["UserID"]) : 0;

                if (companyID <= 0) throw new Exception("CompanyID is required");
                if (userID <= 0) throw new Exception("UserID is required");

                var rowsArr = obj["Rows"] as JArray;
                if (rowsArr == null || rowsArr.Count == 0) throw new Exception("Rows is empty");

                // Prepare rows
                List<AllocationRow> rows = new List<AllocationRow>();
                foreach (var r in rowsArr)
                {
                    Guid invoiceGuid = Guid.Parse(r["InvoiceGuid"]!.ToString());
                    decimal amount = Simulate.decimal_(r["Amount"]);

                    int currencyID = r["CurrencyID"] != null ? Simulate.Integer32(r["CurrencyID"]) : 0;
                    decimal rate = r["Rate"] != null ? Simulate.decimal_(r["Rate"]) : 1;
                    string rowNotes = r["Notes"] != null ? r["Notes"]!.ToString() : "";

                    if (invoiceGuid == Guid.Empty) throw new Exception("InvoiceGuid is empty");
                    if (amount <= 0) throw new Exception("Amount must be > 0");

                    rows.Add(new AllocationRow
                    {
                        InvoiceGuid = invoiceGuid,
                        Amount = amount,
                        CurrencyID = currencyID,
                        Rate = rate == 0 ? 1 : rate,
                        Notes = rowNotes
                    });
                }

                // Do atomic insert with SqlTransaction
                clsSQL clsSQL = new clsSQL();
                using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID)))
                {
                    con.Open();
                    using (SqlTransaction trn = con.BeginTransaction())
                    {
                        try
                        {
                            clsDocumentFlow flow = new clsDocumentFlow();

                            foreach (var row in rows)
                            {
                                int headerID = flow.InsertFlowHeader(
                                    FlowType_Finance,
                                    FlowAction_PaymentAllocation,
                                    Status_Open,
                                    row.InvoiceGuid,
                                    cashVoucherGuid,
                                    DocType_Invoice,
                                    DocType_CashVoucher,
                                    referenceNo,
                                    string.IsNullOrEmpty(notes) ? row.Notes : notes,
                                    companyID,
                                    userID,
                                    trn
                                );

                                flow.InsertFlowDetailAmount(
                                    headerID,
                                    row.Amount,
                                    row.CurrencyID,
                                    row.Rate,
                                    companyID,
                                    userID,
                                    row.Notes,
                                    trn
                                );
                            }

                            trn.Commit();
                            return true;
                        }
                        catch
                        {
                            trn.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // 2) Get Invoice Settlement Status (DocumentFlow only)
        // Returns JSON:
        // { "InvoiceGuid":"...", "TotalInvoice":123, "PaidAmount":50, "Remaining":73, "CalcStatusID":1 }
        // CalcStatusID: 1=Open, 2=Closed
        // ==========================================================
        [HttpGet]
        [Route("GetInvoiceSettlementStatus")]
        public string GetInvoiceSettlementStatus(string InvoiceGuid, int CompanyID)
        {
            try
            {
                Guid invGuid = string.IsNullOrEmpty(InvoiceGuid) ? Guid.Empty : Guid.Parse(InvoiceGuid);
                if (invGuid == Guid.Empty) throw new Exception("InvoiceGuid is required");
                if (CompanyID <= 0) throw new Exception("CompanyID is required");

                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@InvoiceGuid", SqlDbType.UniqueIdentifier) { Value = invGuid },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@FlowActionID", SqlDbType.Int) { Value = FlowAction_PaymentAllocation },
                };

                string sql = @"
SELECT
    inv.Guid AS InvoiceGuid,
    ISNULL(inv.TotalInvoice,0) AS TotalInvoice,
    ISNULL(Paid.PaidAmount,0) AS PaidAmount,
    (ISNULL(inv.TotalInvoice,0) - ISNULL(Paid.PaidAmount,0)) AS Remaining,
    CASE WHEN (ISNULL(inv.TotalInvoice,0) - ISNULL(Paid.PaidAmount,0)) <= 0 THEN 2 ELSE 1 END AS CalcStatusID
FROM tbl_InvoiceHeader inv
OUTER APPLY (
    SELECT ISNULL(SUM(d.Amount * ISNULL(NULLIF(d.Rate,0),1)),0) AS PaidAmount
    FROM tbl_DocumentFlowHeader h
    INNER JOIN tbl_DocumentFlowDetail d ON d.HeaderID = h.ID
    WHERE h.CompanyID = @CompanyID
      AND h.TransactionGuidFrom = inv.Guid
      AND h.FlowActionID = @FlowActionID
      AND d.ValueTypeID = 2
) Paid
WHERE inv.Guid = @InvoiceGuid
  AND (inv.CompanyID = @CompanyID OR @CompanyID = 0);
";

                DataTable dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // =========================
        // Private helper model
        // =========================
        private class AllocationRow
        {
            public Guid InvoiceGuid { get; set; }
            public decimal Amount { get; set; }
            public int CurrencyID { get; set; }
            public decimal Rate { get; set; }
            public string Notes { get; set; } = "";
        }
    }
}
