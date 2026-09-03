using Application.Features.OrganizationSetup;

namespace UnitTests;

public sealed class OrganizationSetupDtoContractTests
{
    [Fact]
    public void PublicDtoProperties_AreStable()
    {
        AssertProperties<OrganizationSetupDto>(
            ("AccountingCompleted", typeof(bool)),
            ("AccountingPolicy", typeof(OrganizationSetupAccountingPolicyDto)),
            ("CompanyProfile", typeof(OrganizationSetupCompanyProfileDto)),
            ("CompletedAt", typeof(DateTime?)),
            ("CostingCondition", typeof(OrganizationSetupCostingConditionDto)),
            ("CurrentStep", typeof(string)),
            ("Defaults", typeof(OrganizationSetupDefaultsDto)),
            ("DefaultsCompleted", typeof(bool)),
            ("IsCompleted", typeof(bool)),
            ("OrganizationCompleted", typeof(bool)),
            ("OrganizationId", typeof(int)),
            ("PricingCondition", typeof(OrganizationSetupPricingConditionDto)),
            ("SetupStatus", typeof(string)),
            ("TaxCompleted", typeof(bool)),
            ("TaxSettings", typeof(OrganizationSetupTaxSettingsDto)));

        AssertProperties<OrganizationSetupCompanyProfileDto>(
            ("Address", typeof(string)),
            ("DefaultLanguageId", typeof(short?)),
            ("Director", typeof(string)),
            ("DistrictId", typeof(int?)),
            ("Email", typeof(string)),
            ("FullName", typeof(string)),
            ("Inn", typeof(string)),
            ("Oked", typeof(string)),
            ("PhoneNumber", typeof(string)),
            ("RegionId", typeof(int)),
            ("ShortName", typeof(string)),
            ("Website", typeof(string)));

        AssertProperties<OrganizationSetupTaxSettingsDto>(
            ("EffectiveFrom", typeof(DateOnly)),
            ("EffectiveTo", typeof(DateOnly?)),
            ("IsVatPayer", typeof(bool)),
            ("StateId", typeof(short)),
            ("TaxTypeId", typeof(short)),
            ("VatRegistrationNumber", typeof(string)));

        AssertProperties<OrganizationSetupAccountingPolicyDto>(
            ("AccountingPolicyId", typeof(short?)),
            ("AccountingStartDate", typeof(DateOnly?)),
            ("BaseCurrencyId", typeof(short?)),
            ("FiscalYearStartMonth", typeof(short)),
            ("InventoryValuationMethod", typeof(string)));

        AssertProperties<OrganizationSetupCostingConditionDto>(
            ("InventoryValuationMethod", typeof(string)));

        AssertProperties<OrganizationSetupPricingConditionDto>(
            ("EndDate", typeof(DateTime?)),
            ("Id", typeof(long)),
            ("PricingMethodCode", typeof(string)),
            ("PricingMethodId", typeof(short)),
            ("PricingMethodName", typeof(string)),
            ("PricingValue", typeof(decimal)),
            ("RoundingMethodCode", typeof(string)),
            ("RoundingMethodId", typeof(short)),
            ("RoundingMethodName", typeof(string)),
            ("RoundingPrecision", typeof(decimal)),
            ("StartDate", typeof(DateTime)));

        AssertProperties<OrganizationSetupDefaultsDto>(
            ("BankAccountId", typeof(int?)),
            ("BankAccountingAccountId", typeof(int?)),
            ("BranchId", typeof(int?)),
            ("CashAccountId", typeof(int?)),
            ("CashBoxId", typeof(int?)),
            ("CogsAccountId", typeof(int?)),
            ("ExpenseAccountId", typeof(int?)),
            ("InventoryAccountId", typeof(int?)),
            ("PayableAccountId", typeof(int?)),
            ("ReceivableAccountId", typeof(int?)),
            ("RevenueAccountId", typeof(int?)),
            ("WarehouseId", typeof(int?)));
    }

    private static void AssertProperties<T>(params (string Name, Type Type)[] expected)
    {
        var actual = typeof(T).GetProperties()
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(property => property.Name)
            .ToArray();

        Assert.Equal(expected.OrderBy(property => property.Name), actual);
    }
}
