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
        /// Retail sale document.
        /// </summary>
        public const short RETAIL_SALE = 7;

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

        /// <summary>
        /// Opening inventory receipt. It affects warehouse stock and opening
        /// balances, but never creates accounting postings.
        /// </summary>
        public const short OPENINGINVENTORY = 16;

        /// <summary>
        /// Fixed asset commissioning document.
        /// </summary>
        public const short FACOMMISSIONING = 17;

        /// <summary>
        /// Warehouse transfer document.
        /// </summary>
        public const short WAREHOUSETRANSFER = 18;

        /// <summary>
        /// Shipment created for a sale document.
        /// </summary>
        public const short SALESHIPMENT = 19;

        /// <summary>
        /// Payroll timesheet document.
        /// </summary>
        public const short PAYROLLTIMESHEET = 20;

        /// <summary>
        /// Payroll payment document.
        /// </summary>
        public const short PAYROLLPAYMENT = 21;

        /// <summary>
        /// Employee absence document.
        /// </summary>
        public const short HRABSENCE = 22;

        /// <summary>
        /// Money transfer between a fiscal cash register and the main cash box.
        /// </summary>
        public const short CASHFISCALTRANSFER = 23;

        /// <summary>
        /// Cash collection from a cash box through cash in transit to a bank account.
        /// </summary>
        public const short CASHCOLLECTION = 24;

        /// <summary>
        /// Movement of funds at a payment acceptance point.
        /// </summary>
        public const short PAYMENT_ACCEPTANCE_POINT_OPERATION = 25;

        /// <summary>
        /// Rental accrual for an individual lessor.
        /// </summary>
        public const short RENTAL_ACCRUAL = 26;
    }
}
