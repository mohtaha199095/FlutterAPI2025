namespace WebApplication2.cls
{
    /// <summary>
    /// Central switches for inventory / costing accounting behaviour.
    ///
    /// These are intentionally simple static flags (no DB dependency) so the
    /// behaviour is explicit and easy to audit. They can later be promoted to a
    /// per-company setting if needed.
    /// </summary>
    public static class clsInventoryConfig
    {
        /// <summary>
        /// When TRUE the system posts a PERPETUAL inventory model: purchase
        /// invoices / purchase-from-financing debit the Inventory asset account
        /// (and purchase refunds credit it) instead of the Purchase expense /
        /// Purchase return accounts.
        ///
        /// DEFAULT IS FALSE (legacy periodic model) on purpose:
        ///  - Existing books were created under the periodic model; flipping the
        ///    default would change historical reporting semantics.
        ///  - If a company receives goods with a Good Receipt (which already
        ///    debits Inventory) and THEN records a Purchase Invoice for the same
        ///    goods, enabling this without a goods-received/invoice-received
        ///    (GR/IR) clearing account would DOUBLE COUNT inventory.
        ///
        /// Only enable this for companies that:
        ///   (a) post stock-in directly on the Purchase Invoice (no separate GR), OR
        ///   (b) have configured a clearing account for the GR -> PI flow.
        /// </summary>
        public static bool UsePerpetualInventory = false;

        /// <summary>
        /// Rounding tolerance used when asserting that a journal voucher is
        /// balanced (total debit == total credit).
        /// </summary>
        public const decimal JvBalanceEpsilon = 0.01m;
    }
}
