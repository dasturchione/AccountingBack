namespace SharedKernel.Constants
{
    /// <summary>
    /// Defines available fields that can be used as quantity sources in posting rules.
    /// These values are stored in the database and indicate which document quantity
    /// should be used when generating accounting entries.
    /// </summary>
    public class PostingQuantityFields
    {
        /// <summary>
        /// Document quantity (number of items / units).
        /// </summary>
        public const string Quantity = "quantity";
    }
}
