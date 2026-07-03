namespace SharedKernel.Constants;

public static class AuditLogTableConst
{
    public const string AccountingPeriod = "acc_accounting_period";
    public const string PurchaseDoc = "pur_doc";
    public const string SaleDoc = "sale_doc";
    public const string BankOperation = "bank_operation";
    public const string CashOperation = "cash_operation";
    public const string WarehouseTransferDoc = "inv_transfer_doc";
    public const string InventoryAdjustmentDoc = "inv_inventory_adjustment_doc";
    public const string InventoryCountDoc = "inv_inventory_count_doc";
}

public static class AuditLogOperationTypeConst
{
    public const string Create = "INSERT";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
}
