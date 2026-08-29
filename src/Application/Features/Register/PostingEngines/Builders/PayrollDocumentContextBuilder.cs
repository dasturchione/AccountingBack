using Application.Features.Pay.PayrollDocuments;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public sealed class PayrollDocumentContextBuilder :
    IPostingContextBuilder<PayPayrollDoc>,
    IPostingContextValidator<PayPayrollDoc>
{
    private static readonly string[] RequiredAccountRoles =
    [
        PayrollAccountRoleCodeConst.SalaryExpense,
        PayrollAccountRoleCodeConst.SalaryPayable,
        PayrollAccountRoleCodeConst.DeductionPayable,
        PayrollAccountRoleCodeConst.EmployerTaxExpense,
        PayrollAccountRoleCodeConst.EmployerTaxPayable,
        PayrollAccountRoleCodeConst.AdvanceReceivable
    ];

    private readonly IPayrollAccountResolver _accountResolver;
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public PayrollDocumentContextBuilder(
        IPayrollAccountResolver accountResolver,
        IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountResolver = accountResolver;
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(PayPayrollDoc document)
    {
        var accountsResult = await _accountResolver.ResolveAsync(document.OrganizationId, RequiredAccountRoles);
        if (!accountsResult.IsSuccess)
            throw new InvalidOperationException(accountsResult.Error.Description);

        var accounts = accountsResult.Value;
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var contexts = new List<PostingContext>();

        foreach (var line in document.Lines)
        {
            var entries = new List<PostingEntryContext>();
            foreach (var calc in line.CalcLines.OrderBy(x => x.Component.SortOrder))
            {
                switch (calc.Component.ComponentType)
                {
                    case PayrollComponentTypeConst.Earning:
                        AddSignedEntry(
                            entries,
                            calc.Component.ExpenseAccountId
                                ?? line.Employment.ExpenseAccountId
                                ?? accounts[PayrollAccountRoleCodeConst.SalaryExpense],
                            calc.Component.LiabilityAccountId
                                ?? accounts[PayrollAccountRoleCodeConst.SalaryPayable],
                            calc.Amount,
                            calc.Id,
                            calc.Component.Name);
                        break;

                    case PayrollComponentTypeConst.Deduction:
                        AddSignedEntry(
                            entries,
                            accounts[PayrollAccountRoleCodeConst.SalaryPayable],
                            calc.Component.LiabilityAccountId
                                ?? accounts[PayrollAccountRoleCodeConst.DeductionPayable],
                            calc.Amount,
                            calc.Id,
                            calc.Component.Name);
                        break;

                    case PayrollComponentTypeConst.EmployerTax:
                        AddSignedEntry(
                            entries,
                            calc.Component.ExpenseAccountId
                                ?? accounts[PayrollAccountRoleCodeConst.EmployerTaxExpense],
                            calc.Component.LiabilityAccountId
                                ?? accounts[PayrollAccountRoleCodeConst.EmployerTaxPayable],
                            calc.Amount,
                            calc.Id,
                            calc.Component.Name);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported payroll component type '{calc.Component.ComponentType}'.");
                }
            }

            if (line.AdvanceAmount != 0m)
            {
                AddSignedEntry(
                    entries,
                    accounts[PayrollAccountRoleCodeConst.SalaryPayable],
                    accounts[PayrollAccountRoleCodeConst.AdvanceReceivable],
                    line.AdvanceAmount,
                    line.Id,
                    "Payroll advance offset");
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

    public async Task<SharedKernel.Results.Result> ValidateAsync(
        PayPayrollDoc document,
        CancellationToken ct = default)
    {
        var accountsResult = await _accountResolver.ResolveAsync(
            document.OrganizationId,
            RequiredAccountRoles,
            ct);

        return accountsResult.IsSuccess
            ? SharedKernel.Results.Result.Success()
            : SharedKernel.Results.Result.Failure(accountsResult.Error);
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
