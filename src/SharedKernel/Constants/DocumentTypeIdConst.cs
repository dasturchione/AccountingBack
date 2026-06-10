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
        public const short PURCHASE = 1;

        /// <summary>
        /// Sales document for goods or services.
        /// </summary>
        public const short SALE = 2;

        /// <summary>
        /// Bank transaction document.
        /// </summary>
        public const short BANKOPERATION = 3;

        /// <summary>
        /// Cash transaction document.
        /// </summary>
        public const short CASHOPERATION = 4;

        /// <summary>
        /// Payroll document.
        /// </summary>
        public const short SALARY = 5;

        /// <summary>
        /// Expense document.
        /// </summary>
        public const short EXPENSE = 6;
    }
}
