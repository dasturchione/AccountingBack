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

        /// <summary>
        /// Warehouse transfer document.
        /// </summary>
        public const short WAREHOUSETRANSFER = 7;

        /// <summary>
        /// Inventory adjustment document.
        /// </summary>
        public const short INVENTORYADJUSTMENT = 8;

        /// <summary>
        /// Inventory count document.
        /// </summary>
        public const short INVENTORYCOUNT = 9;

        /// <summary>
        /// Currency revaluation document.
        /// </summary>
        public const short CURRENCYREVALUATION = 10;

        /// <summary>
        /// Fixed asset receipt document.
        /// </summary>
        public const short FARECEIPT = 11;

        /// <summary>
        /// Fixed asset movement document.
        /// </summary>
        public const short FAMOVEMENT = 12;

        /// <summary>
        /// Fixed asset depreciation run document.
        /// </summary>
        public const short FADEPRECIATION = 13;

        /// <summary>
        /// Fixed asset disposal document.
        /// </summary>
        public const short FADISPOSAL = 14;

        /// <summary>
        /// Fixed asset revaluation document.
        /// </summary>
        public const short FAREVALUATION = 15;
    }
}
