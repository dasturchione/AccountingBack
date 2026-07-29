using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines;

public class BankOperationContextBuilder :
    IPostingContextBuilder<BankOperation>,
    IPostingContextBuilder<List<BankOperation>>
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly IQueryRepository<PaymentType> _paymentTypeQuery;
    private readonly IQueryRepository<PayPaymentBatch> _payrollPaymentQuery;
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public BankOperationContextBuilder(
        IQueryBuilder queryBuilder,
        IQueryRepository<Contract> contractQuery,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<PayPaymentBatch> payrollPaymentQuery,
        IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _queryBuilder = queryBuilder;
        _contractQuery = contractQuery;
        _bankAccountQuery = bankAccountQuery;
        _paymentTypeQuery = paymentTypeQuery;
        _counterpartyQuery = counterpartyQuery;
        _payrollPaymentQuery = payrollPaymentQuery;
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public Task<List<PostingContext>> BuildAsync(BankOperation document)
        => BuildAsync(new List<BankOperation> { document });

    public async Task<List<PostingContext>> BuildAsync(List<BankOperation> documents)
    {
        var result = new List<PostingContext>();
        if (documents.Count == 0)
            return result;

        var bankAccountIds = documents.Select(document => document.BankAccountId).Distinct().ToList();
        var counterpartyIds = documents
            .Select(document => document.CounterpartyId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var contractIds = documents
            .Where(document => document.ContractId.HasValue)
            .Select(document => document.ContractId!.Value)
            .Distinct()
            .ToList();
        var paymentTypeIds = documents
            .Select(document => document.PaymentTypeId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var bankAccountMap = await GetBankAccountMapAsync(bankAccountIds);
        var counterpartyMap = await GetCounterpartyMapAsync(counterpartyIds);
        var contractMap = await GetContractMapAsync(contractIds);
        var paymentTypeMap = await GetPaymentTypeMapAsync(paymentTypeIds);
        var payrollPaymentMap = await GetPayrollPaymentMapAsync(documents.Select(x => x.Id).ToList());
        var accountingPolicyMap = new Dictionary<int, short>();
        foreach (var organizationId in documents.Select(operation => operation.OrganizationId).Distinct())
            accountingPolicyMap[organizationId] = await _accountingPolicyResolver.ResolveAsync(organizationId);

        foreach (var operation in documents)
        {
            if (operation.Amount == 0)
                continue;

            if (payrollPaymentMap.TryGetValue(operation.Id, out var payrollPayment))
            {
                result.AddRange(BuildPayrollPaymentContexts(
                    operation,
                    payrollPayment,
                    paymentTypeMap,
                    bankAccountMap,
                    counterpartyMap,
                    contractMap,
                    accountingPolicyMap[operation.OrganizationId]));
                continue;
            }

            result.Add(new PostingContext
            {
                OrganizationId = operation.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                AccountingPolicyId = accountingPolicyMap[operation.OrganizationId],
                DocumentId = operation.Id,
                SourceLineId = null,
                DocDate = operation.DocDate,
                CurrencyId = operation.CurrencyId,
                JournalNumber = operation.DocNumber,
                Entries = new List<PostingEntryContext>
                {
                    BuildEntry(operation, paymentTypeMap)
                },
                Subkontos = BuildSubkontos(operation, bankAccountMap, counterpartyMap, contractMap)
            });
        }

        return result;
    }

    private static PostingEntryContext BuildEntry(BankOperation operation, Dictionary<short, string> paymentTypeMap)
        => BuildEntry(operation, paymentTypeMap, operation.Amount, null);

    private static PostingEntryContext BuildEntry(
        BankOperation operation,
        Dictionary<short, string> paymentTypeMap,
        decimal amount,
        long? sourceLineId)
    {
        var content = ResolvePaymentMethod(operation.PaymentTypeId, paymentTypeMap);

        return operation.OperationTypeId switch
        {
            OperationTypeIdConst.IN => new PostingEntryContext
            {
                DebitAccountId = operation.BankChartAccountId,
                CreditAccountId = operation.OffsetAccountId,
                Amount = amount,
                Content = content,
                SourceLineId = sourceLineId
            },
            OperationTypeIdConst.OUT => new PostingEntryContext
            {
                DebitAccountId = operation.OffsetAccountId,
                CreditAccountId = operation.BankChartAccountId,
                Amount = amount,
                Content = content,
                SourceLineId = sourceLineId
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation.OperationTypeId),
                operation.OperationTypeId,
                "Unsupported bank operation type for accounting posting.")
        };
    }

    private static List<PostingContext> BuildPayrollPaymentContexts(
        BankOperation operation,
        PayPaymentBatch payment,
        Dictionary<short, string> paymentTypeMap,
        Dictionary<int, string> bankAccountMap,
        Dictionary<int, string> counterpartyMap,
        Dictionary<long, (string Number, DateTime Date)> contractMap,
        short accountingPolicyId)
    {
        ValidatePayrollPayment(operation, payment);

        return payment.Lines
            .Where(line => line.Amount != 0m)
            .Select(line =>
            {
                var subkontos = BuildSubkontos(operation, bankAccountMap, counterpartyMap, contractMap);
                subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.OrganizationEmployees,
                    EntityId = line.EmployeeId,
                    DisplayValue = $"{line.Employee.EmployeeNumber} - {line.Employee.LastName} {line.Employee.FirstName}",
                    SortOrder = 3,
                    AppliesToAccountId = operation.OffsetAccountId
                });

                return new PostingContext
                {
                    OrganizationId = operation.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                    AccountingPolicyId = accountingPolicyId,
                    DocumentId = operation.Id,
                    SourceLineId = line.Id,
                    DocDate = operation.DocDate,
                    CurrencyId = operation.CurrencyId,
                    JournalNumber = operation.DocNumber,
                    Entries = new List<PostingEntryContext>
                    {
                        BuildEntry(operation, paymentTypeMap, line.Amount, line.Id)
                    },
                    Subkontos = subkontos
                };
            })
            .ToList();
    }

    private static void ValidatePayrollPayment(BankOperation operation, PayPaymentBatch payment)
    {
        if (payment.OrganizationId != operation.OrganizationId ||
            payment.CurrencyId != operation.CurrencyId ||
            payment.SourceType != PayrollPaymentSourceConst.Bank ||
            payment.TotalAmount != operation.Amount ||
            payment.Lines.Sum(line => line.Amount) != operation.Amount)
        {
            throw new InvalidOperationException(
                $"Payroll payment {payment.Id} does not match bank operation {operation.Id}.");
        }
    }

    private static string ResolvePaymentMethod(short? paymentTypeId, Dictionary<short, string> paymentTypeMap) =>
        paymentTypeId is { } id && paymentTypeMap.TryGetValue(id, out var value)
            ? value.ToLowerInvariant()
            : "bank";

    private static List<SubkontoValue> BuildSubkontos(
        BankOperation operation,
        Dictionary<int, string> bankAccountMap,
        Dictionary<int, string> counterpartyMap,
        Dictionary<long, (string Number, DateTime Date)> contractMap)
    {
        var subkontos = new List<SubkontoValue>
        {
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.BankAccounts,
                DisplayValue = bankAccountMap.GetValueOrDefault(operation.BankAccountId),
                EntityId = operation.BankAccountId,
                SortOrder = 1
            },
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.CounterpartySettlementDocuments,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    number = operation.DocNumber,
                    date = operation.DocDate
                }),
                EntityId = operation.Id,
                SortOrder = 2
            }
        };

        if (operation.CounterpartyId.HasValue && counterpartyMap.TryGetValue(operation.CounterpartyId.Value, out var counterpartyName))
        {
            subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
                DisplayValue = counterpartyName,
                EntityId = operation.CounterpartyId.Value,
                SortOrder = 3
            });
        }

        if (operation.ContractId.HasValue && contractMap.TryGetValue(operation.ContractId.Value, out var contractData))
        {
            subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.Contracts,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    number = contractData.Number,
                    date = contractData.Date
                }),
                EntityId = operation.ContractId.Value,
                SortOrder = 4
            });
        }

        return subkontos;
    }

    private async Task<Dictionary<int, string>> GetBankAccountMapAsync(List<int> ids)
    {
        if (ids.Count == 0)
            return new Dictionary<int, string>();

        var query = _queryBuilder.For<BankAccount>()
            .Where(account => ids.Contains(account.Id))
            .As(account => new
            {
                account.Id,
                Name = string.IsNullOrWhiteSpace(account.Name)
                    ? account.AccountNumber
                    : $"{account.Name} ({account.AccountNumber})"
            })
            .Build();

        var items = await _bankAccountQuery.GetAllAsync(query);
        return items.ToDictionary(item => item.Id, item => item.Name);
    }

    private async Task<Dictionary<int, string>> GetCounterpartyMapAsync(List<int> ids)
    {
        if (ids.Count == 0)
            return new Dictionary<int, string>();

        var query = _queryBuilder.For<CounterpartyCard>()
            .Where(counterparty => ids.Contains(counterparty.Id))
            .As(counterparty => new
            {
                counterparty.Id,
                counterparty.ShortName
            })
            .Build();

        var items = await _counterpartyQuery.GetAllAsync(query);
        return items.ToDictionary(item => item.Id, item => item.ShortName);
    }

    private async Task<Dictionary<long, (string Number, DateTime Date)>> GetContractMapAsync(List<long> ids)
    {
        if (ids.Count == 0)
            return new Dictionary<long, (string Number, DateTime Date)>();

        var query = _queryBuilder.For<Contract>()
            .Where(contract => ids.Contains(contract.Id))
            .As(contract => new ContractData
            {
                Id = contract.Id,
                Number = contract.ContractNumber,
                Date = contract.ContractDate
            })
            .Build();

        var items = await _contractQuery.GetAllAsync(query);
        return items.ToDictionary(item => item.Id, item => (item.Number, item.Date));
    }

    private async Task<Dictionary<short, string>> GetPaymentTypeMapAsync(List<short> ids)
    {
        if (ids.Count == 0)
            return new Dictionary<short, string>();

        var query = _queryBuilder.For<PaymentType>()
            .Where(paymentType => ids.Contains(paymentType.Id))
            .As(paymentType => new
            {
                paymentType.Id,
                paymentType.Code
            })
            .Build();

        var items = await _paymentTypeQuery.GetAllAsync(query);
        return items.ToDictionary(item => item.Id, item => item.Code);
    }

    private async Task<Dictionary<long, PayPaymentBatch>> GetPayrollPaymentMapAsync(List<long> bankOperationIds)
    {
        if (bankOperationIds.Count == 0)
            return new Dictionary<long, PayPaymentBatch>();

        var query = _queryBuilder.For<PayPaymentBatch>()
            .Where(payment =>
                payment.BankOperationId.HasValue &&
                bankOperationIds.Contains(payment.BankOperationId.Value) &&
                payment.StateId == StateIdConst.ACTIVE)
            .Build();
        query.AddIncludes(x => x.Include(payment => payment.Lines).ThenInclude(line => line.Employee));

        var payments = await _payrollPaymentQuery.GetAllAsync(query);
        return payments.ToDictionary(payment => payment.BankOperationId!.Value);
    }

    private sealed class ContractData
    {
        public long Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
