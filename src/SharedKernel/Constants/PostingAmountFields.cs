namespace SharedKernel.Constants
{
    /// <summary>
    /// Defines available fields that can be used as amount sources in posting rules.
    /// These values are stored in the database and used to determine which document
    /// amount should be used when generating accounting entries.
    /// </summary>
    public class PostingAmountFields
    {
        /// <summary>
        /// Total document amount (standard amount).
        /// </summary>
        public const string Amount = "amount";

        /// <summary>
        /// Cost amount of goods (self-cost / себестоимость).
        /// </summary>
        public const string CostAmount = "cost_amount";

        /// <summary>
        /// VAT amount (value-added tax).
        /// </summary>
        public const string VatAmount = "vat_amount";
    }
}
