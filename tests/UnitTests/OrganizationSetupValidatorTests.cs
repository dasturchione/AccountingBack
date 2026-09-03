using Application.Features.OrganizationSetup;

namespace UnitTests;

public sealed class OrganizationSetupValidatorTests
{
    [Fact]
    public void CompanyProfileValidator_EnforcesRequestShape()
    {
        var validator = new OrganizationSetupCompanyProfileDtoValidator();

        Assert.True(validator.Validate(ValidCompanyProfile()).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupCompanyProfileDto
        {
            ShortName = "",
            FullName = "Company",
            Inn = "123",
            RegionId = 1
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupCompanyProfileDto
        {
            ShortName = "Company",
            FullName = "Company",
            Inn = "123",
            RegionId = 0
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupCompanyProfileDto
        {
            ShortName = "Company",
            FullName = "Company",
            Inn = "123",
            RegionId = 1,
            DistrictId = 0
        }).IsValid);
    }

    [Fact]
    public void TaxSettingsValidator_EnforcesIdsLengthsAndDateRange()
    {
        var validator = new OrganizationSetupTaxSettingsDtoValidator();
        var from = new DateOnly(2026, 1, 1);

        Assert.True(validator.Validate(new OrganizationSetupTaxSettingsDto
        {
            TaxTypeId = 1,
            EffectiveFrom = from,
            StateId = 1
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupTaxSettingsDto
        {
            TaxTypeId = 0,
            EffectiveFrom = from,
            StateId = 1
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupTaxSettingsDto
        {
            TaxTypeId = 1,
            EffectiveFrom = from,
            EffectiveTo = from.AddDays(-1),
            StateId = 1
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupTaxSettingsDto
        {
            TaxTypeId = 1,
            VatRegistrationNumber = new string('x', 101),
            EffectiveFrom = from,
            StateId = 1
        }).IsValid);
    }

    [Fact]
    public void AccountingPolicyValidator_EnforcesMethodReferencesAndFiscalMonth()
    {
        var validator = new OrganizationSetupAccountingPolicyDtoValidator();

        Assert.True(validator.Validate(new OrganizationSetupAccountingPolicyDto()).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupAccountingPolicyDto
        {
            InventoryValuationMethod = "",
            FiscalYearStartMonth = 1
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupAccountingPolicyDto
        {
            AccountingPolicyId = 0,
            FiscalYearStartMonth = 1
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupAccountingPolicyDto
        {
            BaseCurrencyId = 0,
            FiscalYearStartMonth = 1
        }).IsValid);
        Assert.False(validator.Validate(new OrganizationSetupAccountingPolicyDto
        {
            FiscalYearStartMonth = 13
        }).IsValid);
    }

    [Fact]
    public void DefaultsValidator_RejectsNonPositiveOptionalIds()
    {
        var validator = new OrganizationSetupDefaultsDtoValidator();

        Assert.True(validator.Validate(new OrganizationSetupDefaultsDto()).IsValid);

        foreach (var invalid in InvalidDefaults())
            Assert.False(validator.Validate(invalid).IsValid);
    }

    private static OrganizationSetupCompanyProfileDto ValidCompanyProfile() => new()
    {
        ShortName = "Company",
        FullName = "Company full name",
        Inn = "123456789",
        RegionId = 1
    };

    private static IEnumerable<OrganizationSetupDefaultsDto> InvalidDefaults()
    {
        yield return new OrganizationSetupDefaultsDto { BranchId = 0 };
        yield return new OrganizationSetupDefaultsDto { WarehouseId = 0 };
        yield return new OrganizationSetupDefaultsDto { CashBoxId = 0 };
        yield return new OrganizationSetupDefaultsDto { BankAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { ReceivableAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { PayableAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { InventoryAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { CashAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { BankAccountingAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { RevenueAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { ExpenseAccountId = 0 };
        yield return new OrganizationSetupDefaultsDto { CogsAccountId = 0 };
    }
}
