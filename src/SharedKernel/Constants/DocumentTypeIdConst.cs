namespace SharedKernel.Constants
{
    /// <summary>
    /// Document type identifiers.
    /// Values correspond to records in the cmn_document_type reference table.
    /// </summary>
    public static class DocumentTypeIdConst
    {
        /// <summary>
        /// Purchase document for goods or services.
        /// </summary>
        public const short Purchase = 1;

        /// <summary>
        /// Sales document for goods or services.
        /// </summary>
        public const short Sale = 2;

        /// <summary>
        /// Bank transaction document.
        /// </summary>
        public const short BankOperation = 3;

        /// <summary>
        /// Cash transaction document.
        /// </summary>
        public const short CashOperation = 4;

        /// <summary>
        /// Payroll document.
        /// </summary>
        public const short Salary = 5;

        /// <summary>
        /// Expense document.
        /// </summary>
        public const short Expense = 6;
    }
}
