using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsDocumentFlowService
    {
        // =========================
        // Constants (ثبتهم عندك)
        // =========================
        // Transaction Types (Doc types)
        // You can change numbers later, المهم يكون ثابت
        public const int DocType_Invoice = 100;
        public const int DocType_CashVoucher = 200;

        // Flow Types
        public const int FlowType_Finance = 1; // عام (إذا بدك P2P/O2C خليها لاحقاً)

        // Flow Actions
        public const int FlowAction_PaymentAllocation = 30;

        // Value Types
        public const int ValueType_Qty = 1;
        public const int ValueType_Amount = 2;

        // Header Status
        public const int Status_Open = 1;
        public const int Status_Closed = 2;
        public const int Status_Cancelled = 3;

        // =========================
        // Allocation Row Model
        // =========================
        public class PaymentAllocationRow
        {
            public Guid InvoiceGuid { get; set; }
            public decimal Amount { get; set; }
            public int CurrencyID { get; set; } = 0;
            public decimal Rate { get; set; } = 1;
            public string Notes { get; set; } = "";
        }

        // ==========================================================
        // STEP 1: Allocate ONE CashVoucher to MANY invoices
        // - creates one FlowHeader per invoice relation (Invoice -> CashVoucher)
        // - creates one FlowDetail (Amount) per relation
        // ==========================================================
        public bool AllocateCashVoucherToInvoices(
            Guid cashVoucherGuid,
            List<PaymentAllocationRow> allocations,
            int companyID,
            int userID,
            string referenceNo = "",
            string headerNotes = "")
        {
            if (cashVoucherGuid == Guid.Empty)
                throw new Exception("cashVoucherGuid is empty");

            if (allocations == null || allocations.Count == 0)
                throw new Exception("allocations is empty");

            clsSQL clsSQL = new clsSQL();

            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID)))
            {
                con.Open();

                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        clsDocumentFlow flow = new clsDocumentFlow();

                        // Validate allocations
                        foreach (var row in allocations)
                        {
                            if (row.InvoiceGuid == Guid.Empty)
                                throw new Exception("InvoiceGuid is empty in allocations");

                            if (row.Amount <= 0)
                                throw new Exception("Amount must be > 0 in allocations");
                        }

                        // Create header+detail per invoice
                        foreach (var row in allocations)
                        {
                            // 1) Insert FlowHeader (Invoice -> CashVoucher)
                            int headerID = flow.InsertFlowHeader(
                                FlowType_Finance,
                                FlowAction_PaymentAllocation,
                                Status_Open,
                                row.InvoiceGuid,
                                cashVoucherGuid,
                                DocType_Invoice,
                                DocType_CashVoucher,
                                referenceNo,
                                string.IsNullOrEmpty(headerNotes) ? row.Notes : headerNotes,
                                companyID,
                                userID,
                                trn
                            );

                            // 2) Insert FlowDetail Amount
                            flow.InsertFlowDetailAmount(
                                headerID,
                                row.Amount,
                                row.CurrencyID,
                                row.Rate == 0 ? 1 : row.Rate,
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
    }
}
