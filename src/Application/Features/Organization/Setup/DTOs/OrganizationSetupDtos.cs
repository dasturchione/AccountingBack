namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupDto
{
    public int OrganizationId { get; set; }
    public string SetupStatus { get; set; } = null!;
    public string CurrentStep { get; set; } = null!;
    public bool OrganizationCompleted { get; set; }
    public bool TaxCompleted { get; set; }
    public bool AccountingCompleted { get; set; }
    public bool DefaultsCompleted { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public OrganizationSetupCompanyProfileDto CompanyProfile { get; set; } = null!;
    public OrganizationSetupTaxSettingsDto? TaxSettings { get; set; }
    public OrganizationSetupAccountingPolicyDto? AccountingPolicy { get; set; }
    public OrganizationSetupCostingConditionDto? CostingCondition { get; set; }
    public OrganizationSetupPricingConditionDto? PricingCondition { get; set; }
    public OrganizationSetupDefaultsDto? Defaults { get; set; }
}

public sealed class OrganizationSetupCompanyProfileDto
{
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? Director { get; set; }
    public short? DefaultLanguageId { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Oked { get; set; }
}

public sealed class OrganizationSetupTaxSettingsDto
{
    public short TaxTypeId { get; set; }
    public bool IsVatPayer { get; set; }
    public string? VatRegistrationNumber { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public short StateId { get; set; } = 1;
}

public sealed class OrganizationSetupAccountingPolicyDto
{
    public string InventoryValuationMethod { get; set; } = "fifo";
    public short? AccountingPolicyId { get; set; }
    public short? BaseCurrencyId { get; set; }
    public DateOnly? AccountingStartDate { get; set; }
    public short FiscalYearStartMonth { get; set; } = 1;
}

public sealed class OrganizationSetupCostingConditionDto
{
    public string InventoryValuationMethod { get; set; } = null!;
}

public sealed class OrganizationSetupPricingConditionDto
{
    public long Id { get; set; }
    public short PricingMethodId { get; set; }
    public string PricingMethodName { get; set; } = null!;
    public string PricingMethodCode { get; set; } = null!;
    public decimal PricingValue { get; set; }
    public short RoundingMethodId { get; set; }
    public string RoundingMethodName { get; set; } = null!;
    public string RoundingMethodCode { get; set; } = null!;
    public decimal RoundingPrecision { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class OrganizationSetupDefaultsDto
{
    public int? BranchId { get; set; }
    public int? WarehouseId { get; set; }
    public int? CashBoxId { get; set; }
    public int? BankAccountId { get; set; }
    public int? ReceivableAccountId { get; set; }
    public int? PayableAccountId { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? CashAccountId { get; set; }
    public int? BankAccountingAccountId { get; set; }
    public int? RevenueAccountId { get; set; }
    public int? ExpenseAccountId { get; set; }
    public int? CogsAccountId { get; set; }
}
