using SharedKernel.Constants;

namespace Application.Features.AccountingPolicies.DTOs;

public sealed class AccountingPolicyValueDto<T>
{
    public T Value { get; init; } = default!;

    public AccountingPolicySourceStatus SourceStatus { get; init; }

    public string? SourceEvidence { get; init; }

    public bool RequiresBusinessDecision { get; init; }

    public string? AffectedModule { get; init; }

    public string? ImplementationDependency { get; init; }
}

public sealed class AccountingPolicyGeneralDto
{
    public AccountingPolicyValueDto<DateOnly?> AccountingStartDate { get; init; } = new();

    public AccountingPolicyValueDto<short?> FiscalYearStartMonth { get; init; } = new();
}

public sealed class AccountingPolicyInventoryDto
{
    public AccountingPolicyValueDto<string?> InventoryValuationMethod { get; init; } = new();
}

public sealed class AccountingPolicyCurrencyDto
{
    public AccountingPolicyValueDto<short?> BaseCurrencyId { get; init; } = new();

    public AccountingPolicyValueDto<string?> BaseCurrencyCode { get; init; } = new();

    public AccountingPolicyValueDto<string?> CurrencyRevaluationService { get; init; } = new();

    public AccountingPolicyValueDto<string?> RevaluationScope { get; init; } = new();
}

public sealed class AccountingPolicyVatDto
{
    public AccountingPolicyValueDto<bool?> IsVatPayer { get; init; } = new();

    public AccountingPolicyValueDto<short?> TaxTypeId { get; init; } = new();

    public AccountingPolicyValueDto<string?> VatTaxPeriod { get; init; } = new();

    public AccountingPolicyValueDto<string?> VatBaseMoment { get; init; } = new();

    public AccountingPolicyValueDto<string?> ActiveVatRateCatalog { get; init; } = new();
}

public sealed class AccountingPolicyPayrollDto
{
    public AccountingPolicyValueDto<string?> PayrollComponentModel { get; init; } = new();

    public AccountingPolicyValueDto<string?> IndividualTaxPolicy { get; init; } = new();

    public AccountingPolicyValueDto<string?> SocialTaxPolicy { get; init; } = new();
}

public sealed class AccountingPolicyProductionDto
{
    public AccountingPolicyValueDto<bool?> ProductionEnabled { get; init; } = new();

    public AccountingPolicyValueDto<int?> ProductionOutputAccountId { get; init; } = new();

    public AccountingPolicyValueDto<string?> OutputAccountCode { get; init; } = new();
}

public sealed class AccountingPolicyCostingDto
{
    public AccountingPolicyValueDto<string?> CostAllocationMethod { get; init; } = new();
}

public sealed class AccountingPolicyAccountsDto
{
    public AccountingPolicyValueDto<int?> DocumentAccountSettingsCount { get; init; } = new();
}

public sealed class AccountingPolicyGovernanceDto
{
    public AccountingPolicyValueDto<DateOnly?> EffectiveFrom { get; init; } = new();

    public AccountingPolicyValueDto<DateOnly?> EffectiveTo { get; init; } = new();

    public AccountingPolicyValueDto<bool?> PolicyVersioning { get; init; } = new();

    public AccountingPolicyValueDto<string?> ClosedPeriodPolicy { get; init; } = new();
}

public sealed class AccountingPolicyCurrentDto
{
    public int OrganizationId { get; init; }

    public AccountingPolicyGeneralDto General { get; init; } = new();

    public AccountingPolicyInventoryDto Inventory { get; init; } = new();

    public AccountingPolicyCurrencyDto Currency { get; init; } = new();

    public AccountingPolicyVatDto Vat { get; init; } = new();

    public AccountingPolicyPayrollDto Payroll { get; init; } = new();

    public AccountingPolicyProductionDto Production { get; init; } = new();

    public AccountingPolicyCostingDto Costing { get; init; } = new();

    public AccountingPolicyAccountsDto Accounts { get; init; } = new();

    public AccountingPolicyGovernanceDto Governance { get; init; } = new();

    public string? InventoryValuationMethod { get; init; }

    public short? BaseCurrencyId { get; init; }

    public string? BaseCurrencyCode { get; init; }

    public DateOnly? AccountingStartDate { get; init; }

    public short? FiscalYearStartMonth { get; init; }

    public bool? IsVatPayer { get; init; }

    public short? TaxTypeId { get; init; }

    public string? VatTaxPeriod { get; init; }

    public string? VatBaseMoment { get; init; }

    public bool? ProductionEnabled { get; init; }

    public bool? ForeignCurrencyEnabled { get; init; }

    public string? ForeignCurrency { get; init; }

    public string? CostAllocationMethod { get; init; }

    public DateOnly? EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public bool? PolicyVersioning { get; init; }

    public string? ClosedPeriodPolicy { get; init; }

    public AccountingPolicySourceStatus SourceStatus { get; init; }

    public string? SourceEvidence { get; init; }

    public bool RequiresBusinessDecision { get; init; }

    public string? AffectedModule { get; init; }

    public string? ImplementationDependency { get; init; }
}

public sealed class AccountingPolicyHistoryItemDto
{
    public int OrganizationId { get; init; }

    public int Version { get; init; }

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public string? InventoryValuationMethod { get; init; }

    public short? BaseCurrencyId { get; init; }

    public bool? VatPayer { get; init; }

    public short? TaxTypeId { get; init; }

    public string? VatTaxPeriod { get; init; }

    public string? VatBaseMoment { get; init; }

    public string? ClosedPeriodPolicy { get; init; }
}

public sealed class AccountingPolicyHistoryDto
{
    public int OrganizationId { get; init; }

    public IReadOnlyList<AccountingPolicyHistoryItemDto> Items { get; init; } = Array.Empty<AccountingPolicyHistoryItemDto>();

    public AccountingPolicySourceStatus SourceStatus { get; init; }

    public string? SourceEvidence { get; init; }

    public bool RequiresBusinessDecision { get; init; }

    public string? AffectedModule { get; init; }

    public string? ImplementationDependency { get; init; }
}

public sealed class AccountingPolicyImpactDto
{
    public int OrganizationId { get; init; }

    public DateOnly EffectiveOn { get; init; }

    public string? DocumentType { get; init; }

    public AccountingPolicyCurrentDto? Policy { get; init; }

    public bool ExistingDocumentsRecalculated { get; init; }

    public AccountingPolicySourceStatus SourceStatus { get; init; }

    public string? SourceEvidence { get; init; }

    public bool RequiresBusinessDecision { get; init; }

    public string? AffectedModule { get; init; }

    public string? ImplementationDependency { get; init; }
}

public class AccountingPolicyUpdateRequestDto
{
    public string? InventoryValuationMethod { get; init; }

    public short? BaseCurrencyId { get; init; }

    public bool? VatPayer { get; init; }

    public short? TaxTypeId { get; init; }

    public string? VatTaxPeriod { get; init; }

    public string? VatBaseMoment { get; init; }

    public DateOnly? EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public bool? ProductionEnabled { get; init; }

    public bool? ForeignCurrencyEnabled { get; init; }

    public string? CostAllocationMethod { get; init; }

    public string? ClosedPeriodPolicy { get; init; }
}

// Application-facing alias retained for the service contract described by the
// approved policy plan. It intentionally shares the request shape and does not
// introduce a second update model.
public sealed class AccountingPolicyUpdateDto : AccountingPolicyUpdateRequestDto
{
}
