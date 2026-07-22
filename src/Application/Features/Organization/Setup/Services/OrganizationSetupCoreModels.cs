namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupTaxSettingsWriteModel
{
    public short TaxTypeId { get; init; }
    public bool IsVatPayer { get; init; }
    public string? VatRegistrationNumber { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public short StateId { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class OrganizationSetupAccountingPolicyWriteModel
{
    public string InventoryValuationMethod { get; init; } = null!;
    public short? AccountingPolicyId { get; init; }
    public short? BaseCurrencyId { get; init; }
    public DateOnly? AccountingStartDate { get; init; }
    public short FiscalYearStartMonth { get; init; }
}

public sealed class OrganizationSetupDefaultsWriteModel
{
    public int? BranchId { get; init; }
    public int? WarehouseId { get; init; }
    public int? CashBoxId { get; init; }
    public int? BankAccountId { get; init; }
    public int? ReceivableAccountId { get; init; }
    public int? PayableAccountId { get; init; }
    public int? InventoryAccountId { get; init; }
    public int? CashAccountId { get; init; }
    public int? BankAccountingAccountId { get; init; }
    public int? RevenueAccountId { get; init; }
    public int? ExpenseAccountId { get; init; }
    public int? CogsAccountId { get; init; }
    public DateTime CreatedDate { get; init; }
}
