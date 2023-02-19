namespace WebApplication2.MainClasses
{
    public class clsEnum
    {

        public enum VoucherType : int
        {
            ManualJV = 1,
            PurchaseInvoice = 2,
            SalesInvoice = 3,
            SalesRefund = 4,
            SalesOffer = 5,
            PurchaseOffer = 6,
            PurchaseRefund = 7,
            GoodRecipt = 8,
            GoodIssue = 9, POSSalesInvoice = 10, POSSalesInvoicereturn = 11, CashPayment = 12, Cashrecivable = 13, Finance=14,
        }
        public enum AccountMainSetting : int
        {
            PurchaseAccount = 1,
            SalesAccount = 2,
            SalesReturnAccount = 3,
            PurchaseReturnAccount = 4,
            CashAccount = 5,
            VendorAccount = 6,
            CustomerAccount = 7,
            Inventory = 8,
            SalesTaxAccount = 9,
            SpecialSalesTaxAccount = 10,
            PurchaseTaxAccount = 11,
            SpecialPurchaseTaxAccount = 12,
            SalesDiscount = 13,
            PurchaseDiscount = 14,
            Banks = 15,
            IncomingCheuqesUPC = 16,
            OutgoingCheuqesUPC = 17,
            Employees = 18,


        }
        public enum PaymentMethod : int
        {
            Cash = 1,
            Debit = 2,
        }
        public enum BusinessPartner : int
        {
            Customer = 1,
            Vendor = 2,
        }
    }
}
