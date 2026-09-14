using Application.Features.Pay;
using Application.Features.Pay.PayrollDocuments;
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

            // A withholding is taken off what this employee is owed, and an employer tax is a
            // cost of employing them, so both counter-accounts come from this line's own
            // earnings rather than from the document header.
            var expenseWeights = PayrollPostingAccountAllocator.ExpenseWeights(line);
            var payableWeights = PayrollPostingAccountAllocator.PayableWeights(line);

            foreach (var tax in line.TaxLines.OrderBy(x => x.TaxDefinition.Code))
            {
                var isEmployerTax = tax.TaxDefinition.TaxType == PayrollTaxTypeConst.Employer;
                var shares = PayrollPostingAccountAllocator.Allocate(
                    isEmployerTax ? expenseWeights : payableWeights,
                    tax.Amount,
                    isEmployerTax ? document.SalaryExpenseAccountId : document.SalaryPayableAccountId);

                foreach (var share in shares)
                {
                    AddSignedEntry(
                        entries,
                        share.AccountId,
                        tax.LiabilityAccountId,
                        share.Amount,
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

            var expenseWeights = PayrollPostingAccountAllocator.ExpenseWeights(line);
            var payableWeights = PayrollPostingAccountAllocator.PayableWeights(line);

            foreach (var tax in line.TaxLines.Where(x => x.Amount != 0m))
            {
                if (tax.LiabilityAccountId <= 0)
                    return Task.FromResult(SharedKernel.Results.Result.Failure(
                        PayrollErrors.StoredPostingAccountMissing("taxLiabilityAccountId", tax.Id)));

                // The tax is debited to the accounts this line's earnings used; the document
                // header is only needed when the line carries no earnings to weigh.
                var isEmployerTax = tax.TaxDefinition.TaxType == PayrollTaxTypeConst.Employer;
                var hasWeights = (isEmployerTax ? expenseWeights : payableWeights).Count > 0;
                var fallbackAccountId = isEmployerTax
                    ? document.SalaryExpenseAccountId
                    : document.SalaryPayableAccountId;
                if (!hasWeights && !fallbackAccountId.HasValue)
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
