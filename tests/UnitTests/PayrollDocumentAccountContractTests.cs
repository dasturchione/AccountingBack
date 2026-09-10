using Application.Features.Pay.PayrollDocuments;
using Domain.Entities;

namespace UnitTests;

public sealed class PayrollDocumentAccountContractTests
{
    [Fact]
    public void CalculateDto_ExposesOnlySalaryAccountInputs()
    {
        var propertyNames = typeof(PayrollCalculateDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains(nameof(PayrollCalculateDto.SalaryExpenseAccountId), propertyNames);
        Assert.Contains(nameof(PayrollCalculateDto.SalaryPayableAccountId), propertyNames);
        Assert.DoesNotContain("DeductionPayableAccountId", propertyNames);
        Assert.DoesNotContain("EmployerTaxExpenseAccountId", propertyNames);
        Assert.DoesNotContain("EmployerTaxPayableAccountId", propertyNames);
        Assert.DoesNotContain("AdvanceReceivableAccountId", propertyNames);
    }

    [Fact]
    public void PayrollDocumentEntity_DoesNotExposeRemovedAccountColumns()
    {
        var propertyNames = typeof(PayPayrollDoc)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("DeductionPayableAccountId", propertyNames);
        Assert.DoesNotContain("EmployerTaxExpenseAccountId", propertyNames);
        Assert.DoesNotContain("EmployerTaxPayableAccountId", propertyNames);
        Assert.DoesNotContain("AdvanceReceivableAccountId", propertyNames);
    }
}
