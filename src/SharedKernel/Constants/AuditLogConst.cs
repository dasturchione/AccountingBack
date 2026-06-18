namespace SharedKernel.Constants;

public static class AuditLogTableConst
{
    public const string PurchaseDoc = "pur_doc";
    public const string SaleDoc = "sale_doc";
    public const string BankOperation = "bank_operation";
    public const string CashOperation = "cash_operation";
}

public static class AuditLogOperationTypeConst
{
    public const string Create = "INSERT";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
}
