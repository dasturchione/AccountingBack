namespace SharedKernel.Constants;

public static class AuditLogTableConst
{
    public const string AccountingPeriod = "acc_accounting_period";
    public const string PurchaseDoc = "pur_doc";
    public const string OpeningInventory = "inv_opening_inventory";
    public const string SaleDoc = "sale_doc";
    public const string BankOperation = "bank_operation";
    public const string CashOperation = "cash_operation";
    public const string CashFiscalTransfer = "cash_fiscal_transfer_doc";
    public const string CashCollection = "cash_collection_doc";
    public const string PaymentAcceptancePoint = "org_payment_acceptance_point";
    public const string PaymentAcceptancePointOperation = "payment_acceptance_point_operation";
    public const string WarehouseTransferDoc = "inv_transfer_doc";
    public const string InventoryAdjustmentDoc = "inv_inventory_adjustment_doc";
    public const string InventoryCountDoc = "inv_inventory_count_doc";
    public const string FaReceiptDoc = "fa_receipt_doc";
    public const string FaCommissioningDoc = "fa_commissioning_doc";
    public const string FaMovementDoc = "fa_movement_doc";
    public const string FaDepreciationRun = "fa_depreciation_run";
    public const string FaDisposalDoc = "fa_disposal_doc";
    public const string FaRevaluationDoc = "fa_revaluation_doc";
    public const string PayEmployee = "pay_employee";
    public const string PayComponent = "pay_component";
    public const string PayPeriod = "pay_period";
    public const string PayTimesheet = "pay_timesheet";
    public const string PayPayrollDoc = "pay_payroll_doc";
    public const string PayPaymentBatch = "pay_payment_batch";
    public const string HrEmployeeWorkSchedule = "hr_employee_work_schedule";
    public const string HrAbsence = "hr_absence";
    public const string AuthorizationBypass = "sys_authorization_bypass";
    public const string Notification = "sys_notification";
    public const string EdoImportJob = "edo_import_job";
    public const string EdoImportCandidate = "edo_import_candidate";
}

public static class AuditLogOperationTypeConst
{
    public const string Create = "INSERT";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
}
