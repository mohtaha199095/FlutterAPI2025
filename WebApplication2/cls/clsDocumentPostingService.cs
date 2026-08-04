using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsDocumentPostingService
    {
        private readonly clsJournalVoucherHeader _jvHeader = new clsJournalVoucherHeader();
        private readonly clsJournalVoucherDetails _jvDetails = new clsJournalVoucherDetails();
        private readonly clsCashVoucherHeader _cashHeader = new clsCashVoucherHeader();
        private readonly clsCashVoucherDetails _cashDetails = new clsCashVoucherDetails();
        private readonly clsCreditNoteHeader _creditNoteHeader = new clsCreditNoteHeader();
        private readonly clsCreditNoteDetails _creditNoteDetails = new clsCreditNoteDetails();

        public bool PostDocument(int documentTypeId, string documentGuid, int userId, int companyId, SqlTransaction trn)
        {
            if (IsCashVoucherType(documentTypeId))
                return PostCashVoucher(documentGuid, userId, companyId, trn);

            if (IsCreditNoteType(documentTypeId))
                return PostCreditNote(documentGuid, userId, companyId, trn);

            if (clsApprovalDocumentTypes.IsInvoiceHeaderType(documentTypeId))
                return PostInvoice(documentGuid, userId, companyId, trn);

            if (clsApprovalDocumentTypes.IsHcmType(documentTypeId))
                return clsHcmApprovalDocuments.PostDocument(documentTypeId, documentGuid, userId, companyId, trn);

            return PostJournalVoucher(documentGuid, userId, companyId, trn);
        }

        public bool PostJournalVoucher(string jvGuid, int userId, int companyId, SqlTransaction trn)
        {
            if (string.IsNullOrWhiteSpace(jvGuid)) return false;

            if (!_jvHeader.CheckJVMatch(jvGuid, companyId, trn))
                return false;

            return _jvHeader.UpdateDocumentStatus(
                jvGuid,
                (int)DocumentStatus.Posted,
                userId,
                companyId,
                trn);
        }

        public bool PostCashVoucher(string cashGuid, int userId, int companyId, SqlTransaction trn)
        {
            DataTable dt = _cashHeader.SelectCashVoucherHeaderByGuid(
                cashGuid,
                Simulate.StringToDate("1900-01-01"),
                Simulate.StringToDate("2300-01-01"),
                0, 0, companyId,
                "00000000-0000-0000-0000-000000000000",
                trn);

            if (dt == null || dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            int status = Simulate.Integer32(row["DocumentStatus"]);
            if (status == (int)DocumentStatus.Posted) return true;

            string existingJvGuid = Simulate.String(row["JVGuid"]);
            if (!string.IsNullOrWhiteSpace(existingJvGuid) &&
                existingJvGuid != "00000000-0000-0000-0000-000000000000")
            {
                _jvHeader.UpdateDocumentStatus(existingJvGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
                return _cashHeader.UpdateDocumentStatus(cashGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
            }

            int branchId = Simulate.Integer32(row["BranchID"]);
            int costCenterId = Simulate.Integer32(row["CostCenterID"]);
            int accountId = Simulate.Integer32(row["AccountID"]);
            int cashId = Simulate.Integer32(row["CashID"]);
            decimal amount = Simulate.Decimal(row["Amount"]);
            string note = Simulate.String(row["Note"]);
            DateTime voucherDate = Simulate.StringToDate(row["VoucherDate"]);
            DateTime dueDate = Simulate.StringToDate(row["DueDate"]);
            int voucherType = Simulate.Integer32(row["VoucherType"]);
            int creationUserId = Simulate.Integer32(row["CreationUserID"]);

            DataTable dtDetails = _cashDetails.SelectCashVoucherDetailsByHeaderGuid(cashGuid, companyId, trn);
            var details = new List<DBCashVoucherDetails>();
            if (dtDetails != null)
            {
                foreach (DataRow dRow in dtDetails.Rows)
                {
                    details.Add(new DBCashVoucherDetails
                    {
                        AccountID = Simulate.Integer32(dRow["AccountID"]),
                        SubAccountID = Simulate.Integer32(dRow["SubAccountID"]),
                        Debit = Simulate.Decimal(dRow["Debit"]),
                        Credit = Simulate.Decimal(dRow["Credit"]),
                        BranchID = Simulate.Integer32(dRow["BranchID"]),
                        CostCenterID = Simulate.Integer32(dRow["CostCenterID"]),
                        Note = Simulate.String(dRow["Note"]),
                        CompanyID = companyId,
                    });
                }
            }

            bool posted = _cashHeader.InsertCashVoucherJournalVoucher(
                cashGuid, accountId, branchId, costCenterId, cashId, amount, note,
                voucherDate, dueDate, details, "", voucherType, companyId, creationUserId, trn);

            if (!posted) return false;

            DataTable dtAfter = _cashHeader.SelectCashVoucherHeaderByGuid(
                cashGuid,
                Simulate.StringToDate("1900-01-01"),
                Simulate.StringToDate("2300-01-01"),
                0, 0, companyId,
                "00000000-0000-0000-0000-000000000000",
                trn);

            if (dtAfter != null && dtAfter.Rows.Count > 0)
            {
                string jvGuid = Simulate.String(dtAfter.Rows[0]["JVGuid"]);
                if (!string.IsNullOrWhiteSpace(jvGuid))
                    _jvHeader.UpdateDocumentStatus(jvGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
            }

            return _cashHeader.UpdateDocumentStatus(cashGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
        }

        public bool PostCreditNote(string creditNoteGuid, int userId, int companyId, SqlTransaction trn)
        {
            DataTable dt = _creditNoteHeader.SelectCreditNoteHeaderByGuid(
                creditNoteGuid,
                Simulate.StringToDate("1900-01-01"),
                Simulate.StringToDate("2300-01-01"),
                0, 0, companyId,
                trn);

            if (dt == null || dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            int status = Simulate.Integer32(row["DocumentStatus"]);
            if (status == (int)DocumentStatus.Posted) return true;

            int branchId = Simulate.Integer32(row["BranchID"]);
            int costCenterId = Simulate.Integer32(row["CostCenterID"]);
            int accountId = Simulate.Integer32(row["AccountID"]);
            int subAccountId = Simulate.Integer32(row["SubAccountID"]);
            decimal amount = Simulate.Decimal(row["Amount"]);
            string note = Simulate.String(row["Note"]);
            DateTime voucherDate = Simulate.StringToDate(row["VoucherDate"]);
            int voucherType = Simulate.Integer32(row["VoucherType"]);
            int creationUserId = Simulate.Integer32(row["CreationUserID"]);
            string existingJvGuid = Simulate.String(row["JVGuid"]);

            DataTable dtDetails = _creditNoteDetails.SelectCreditNoteDetailsByHeaderGuid(creditNoteGuid, companyId, trn);
            var details = new List<DBCreditNoteDetails>();
            if (dtDetails != null)
            {
                foreach (DataRow dRow in dtDetails.Rows)
                {
                    details.Add(new DBCreditNoteDetails
                    {
                        AccountID = Simulate.Integer32(dRow["AccountID"]),
                        SubAccountID = Simulate.Integer32(dRow["SubAccountID"]),
                        Debit = Simulate.Decimal(dRow["Debit"]),
                        Credit = Simulate.Decimal(dRow["Credit"]),
                        BranchID = Simulate.Integer32(dRow["BranchID"]),
                        CostCenterID = Simulate.Integer32(dRow["CostCenterID"]),
                        Note = Simulate.String(dRow["Note"]),
                        CompanyID = companyId,
                    });
                }
            }

            bool posted = _creditNoteHeader.InsertCreditNoteJournalVoucher(
                creditNoteGuid, accountId, subAccountId, branchId, costCenterId, amount, note,
                voucherDate, details, existingJvGuid, voucherType, companyId, creationUserId, trn,
                (int)DocumentStatus.Posted);

            if (!posted) return false;

            DataTable dtAfter = _creditNoteHeader.SelectCreditNoteHeaderByGuid(
                creditNoteGuid,
                Simulate.StringToDate("1900-01-01"),
                Simulate.StringToDate("2300-01-01"),
                0, 0, companyId,
                trn);

            if (dtAfter != null && dtAfter.Rows.Count > 0)
            {
                string jvGuid = Simulate.String(dtAfter.Rows[0]["JVGuid"]);
                if (!string.IsNullOrWhiteSpace(jvGuid) &&
                    jvGuid != "00000000-0000-0000-0000-000000000000")
                {
                    _jvHeader.UpdateDocumentStatus(jvGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
                }
            }

            return _creditNoteHeader.UpdateDocumentStatus(
                creditNoteGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
        }

        public bool PostInvoice(string invoiceGuid, int userId, int companyId, SqlTransaction trn)
        {
            return new clsInvoiceHeader().PostInvoiceDocument(invoiceGuid, userId, companyId, trn);
        }

        public static bool IsCashVoucherType(int documentTypeId)
        {
            return documentTypeId == (int)VoucherType.CashPayment ||
                   documentTypeId == (int)VoucherType.Cashrecivable ||
                   documentTypeId == (int)VoucherType.POSCashPayment ||
                   documentTypeId == (int)VoucherType.POSCashRecipt;
        }

        public static bool IsCreditNoteType(int documentTypeId)
        {
            return documentTypeId == (int)VoucherType.creditNote ||
                   documentTypeId == (int)VoucherType.debitNote;
        }

        public static bool IsMvpApprovalType(int documentTypeId)
        {
            return clsApprovalDocumentTypes.IsSupported(documentTypeId);
        }
    }
}
