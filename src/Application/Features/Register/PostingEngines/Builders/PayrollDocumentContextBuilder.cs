using Application.Features.Pay;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public sealed class PayrollDocumentContextBuilder :
    IPostingContextBuilder<PayPayrollDoc>,
    IPostingContextValidator<PayPayrollDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public PayrollDocumentContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(PayPayrollDoc document)
    {
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var contexts = new List<PostingContext>();

        foreach (var line in document.Lines)
        {
            var entries = new List<PostingEntryContext>();
            foreach (var calc in line.CalcLines.OrderBy(x => x.Component.SortOrder))
            {
                AddSignedEntry(
                    entries,
                    calc.DebitAccountId!.Value,
                    calc.CreditAccountId!.Value,
                    calc.Amount,
                    calc.Id,
                    calc.Component.Name);
            }

            foreach (var tax in line.TaxLines.OrderBy(x => x.TaxDefinition.Code))
            {
                var debitAccountId = tax.TaxDefinition.TaxType == PayrollTaxTypeConst.Employer
                    ? document.SalaryExpenseAccountId
                    : document.SalaryPayableAccountId;
                if (debitAccountId.HasValue)
                {
                    AddSignedEntry(
                        entries,
                        debitAccountId.Value,
                        tax.LiabilityAccountId,
                        tax.Amount,
                        tax.Id,
                        $"Tax: {tax.TaxDefinition.Name}");
                }
            }

            if (entries.Count == 0)
                continue;

            contexts.Add(new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.SALARY,
                DocumentId = document.Id,
                CurrencyId = document.CurrencyId,
                DocDate = document.DocDate,
                JournalNumber = document.DocNumber,
                SourceLineId = line.Id,
                AccountingPolicyId = accountingPolicyId,
                Entries = entries,
                Subkontos = BuildSubkontos(line)
            });
        }

        return contexts;
    }

    public Task<SharedKernel.Results.Result> ValidateAsync(
        PayPayrollDoc document,
        CancellationToken ct = default)
    {
        foreach (var line in document.Lines)
        {
            foreach (var calc in line.CalcLines.Where(x => x.Amount != 0m))
            {
                if (!calc.DebitAccountId.HasValue)
                    return Task.FromResult(SharedKernel.Results.Result.Failure(
                        PayrollErrors.StoredPostingAccountMissing("debitAccountId", calc.Id)));
                if (!calc.CreditAccountId.HasValue)
                    return Task.FromResult(SharedKernel.Results.Result.Failure(
                        PayrollErrors.StoredPostingAccountMissing("creditAccountId", calc.Id)));
            }

            foreach (var tax in line.TaxLines.Where(x => x.Amount != 0m))
            {
                if (tax.LiabilityAccountId <= 0)
                    return Task.FromResult(SharedKernel.Results.Result.Failure(
                        PayrollErrors.StoredPostingAccountMissing("taxLiabilityAccountId", tax.Id)));

                var debitAccountId = tax.TaxDefinition.TaxType == PayrollTaxTypeConst.Employer
                    ? document.SalaryExpenseAccountId
                    : document.SalaryPayableAccountId;
                if (!debitAccountId.HasValue)
                    return Task.FromResult(SharedKernel.Results.Result.Failure(
                        PayrollErrors.StoredPostingAccountMissing("taxDebitAccountId", tax.Id)));
            }

        }

        return Task.FromResult(SharedKernel.Results.Result.Success());
    }

    private static void AddSignedEntry(
        ICollection<PostingEntryContext> entries,
        int debitAccountId,
        int creditAccountId,
        decimal amount,
        long sourceLineId,
        string content)
    {
        if (amount == 0m)
            return;

        entries.Add(amount > 0m
            ? new PostingEntryContext
            {
                DebitAccountId = debitAccountId,
                CreditAccountId = creditAccountId,
                Amount = amount,
                SourceLineId = sourceLineId,
                Content = content
            }
            : new PostingEntryContext
            {
                DebitAccountId = creditAccountId,
                CreditAccountId = debitAccountId,
                Amount = Math.Abs(amount),
                SourceLineId = sourceLineId,
                Content = $"Correction: {content}"
            });
    }

    private static List<SubkontoValue> BuildSubkontos(PayPayrollLine line)
    {
        var values = new List<SubkontoValue>
        {
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.OrganizationEmployees,
                EntityId = line.EmployeeId,
                DisplayValue = $"{line.Employee.EmployeeNumber} - {line.Employee.LastName} {line.Employee.FirstName}",
                SortOrder = 1
            }
        };

        if (line.Employment.DepartmentId.HasValue)
        {
            values.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.SeparateDivisions,
                EntityId = line.Employment.DepartmentId.Value,
                DisplayValue = line.Employment.Department?.Name ?? line.Employment.DepartmentId.Value.ToString(),
                SortOrder = 2
            });
        }

        return values;
    }
}
