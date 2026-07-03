using Application.Abstractions;
using Domain.Entities;
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
    private readonly IQueryRepository<PaymentPurpose> _paymentPurposeQuery;
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public BankOperationContextBuilder(
        IQueryBuilder queryBuilder,
        IQueryRepository<Contract> contractQuery,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<PaymentPurpose> paymentPurposeQuery,
        IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _queryBuilder = queryBuilder;
        _contractQuery = contractQuery;
        _bankAccountQuery = bankAccountQuery;
        _paymentTypeQuery = paymentTypeQuery;
        _counterpartyQuery = counterpartyQuery;
        _paymentPurposeQuery = paymentPurposeQuery;
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public Task<List<PostingContext>> BuildAsync(BankOperation document)
        => BuildAsync(new List<BankOperation> { document });

    public async Task<List<PostingContext>> BuildAsync(List<BankOperation> documents)
    {
        var result = new List<PostingContext>();

        if (documents.Count == 0)
            return result;

        var bankAccountIds = documents
            .Select(document => document.BankAccountId)
            .Distinct()
            .ToList();

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

        var paymentPurposeIds = documents
            .Select(operation => operation.PaymentPurposeId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var bankAccountMap = await GetBankAccountMapAsync(bankAccountIds);
        var counterpartyMap = await GetCounterpartyMapAsync(counterpartyIds);
        var contractMap = await GetContractMapAsync(contractIds);
        var paymentTypeMap = await GetPaymentTypeMapAsync(paymentTypeIds);
        var paymentPurposeAliasMap = await GetPaymentPurposeAliasMapAsync(paymentPurposeIds);
        var accountingPolicyMap = new Dictionary<int, short>();
        foreach (var organizationId in documents.Select(operation => operation.OrganizationId).Distinct())
            accountingPolicyMap[organizationId] = await _accountingPolicyResolver.ResolveAsync(organizationId);

        foreach (var operation in documents)
        {
            if (operation.Amount == 0)
                continue;

            var paymentPurposeAlias = paymentPurposeAliasMap.GetValueOrDefault(operation.PaymentPurposeId);
            var counterpartyId = operation.CounterpartyId;

            result.Add(new PostingContext
            {
                OrganizationId = operation.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
                AccountingPolicyId = accountingPolicyMap[operation.OrganizationId],
                RuleId = ResolveRuleId(operation.OperationTypeId),
                DocumentId = operation.Id,
                SourceLineId = null,
                DocDate = operation.DocDate,
                CurrencyId = operation.CurrencyId,
                JournalNumber = operation.DocNumber,
                PaymentMethod = ResolvePaymentMethod(operation.PaymentTypeId, paymentTypeMap),
                RequiredDebitAlias = ResolveRequiredDebitAlias(operation.OperationTypeId, paymentPurposeAlias),
                RequiredCreditAlias = ResolveRequiredCreditAlias(operation.OperationTypeId, paymentPurposeAlias),
                AllowedAliases = string.IsNullOrWhiteSpace(paymentPurposeAlias)
                    ? Array.Empty<string>()
                    : new[] { paymentPurposeAlias },
                Amounts = new Dictionary<string, decimal>
                {
                    [AmountSourceConst.Total] = operation.Amount
                },
                Subkontos = BuildSubkontos(operation, counterpartyId, bankAccountMap, counterpartyMap, contractMap)
            });
        }

        return result;
    }

    private static short ResolveRuleId(short operationTypeId) =>
        operationTypeId switch
        {
            OperationTypeIdConst.IN => PostingRuleIdConst.DEBIT_OPERATION,
            OperationTypeIdConst.OUT => PostingRuleIdConst.CREDIT_OPERATION,
            _ => throw new ArgumentOutOfRangeException(
                nameof(operationTypeId),
                operationTypeId,
                "Unsupported bank operation type for accounting posting.")
        };

    private static string? ResolveRequiredDebitAlias(short operationTypeId, string? paymentPurposeAlias) =>
        operationTypeId switch
        {
            OperationTypeIdConst.IN when !string.IsNullOrWhiteSpace(paymentPurposeAlias) => AliasConst.PaymentAccount,
            OperationTypeIdConst.OUT when !string.IsNullOrWhiteSpace(paymentPurposeAlias) => paymentPurposeAlias,
            _ => null
        };

    private static string? ResolveRequiredCreditAlias(short operationTypeId, string? paymentPurposeAlias) =>
        operationTypeId switch
        {
            OperationTypeIdConst.IN when !string.IsNullOrWhiteSpace(paymentPurposeAlias) => paymentPurposeAlias,
            OperationTypeIdConst.OUT when !string.IsNullOrWhiteSpace(paymentPurposeAlias) => AliasConst.PaymentAccount,
            _ => null
        };

    private static string ResolvePaymentMethod(short? paymentTypeId, Dictionary<short, string> paymentTypeMap) =>
        paymentTypeId is { } id && paymentTypeMap.TryGetValue(id, out var value)
            ? value.ToLowerInvariant()
            : "bank";

    private static List<SubkontoValue> BuildSubkontos(
        BankOperation operation,
        int? counterpartyId,
        Dictionary<int, string> bankAccountMap,
        Dictionary<int, string> counterpartyMap,
        Dictionary<long, (string Number, DateTime Date)> contractMap)
    {
        var subkontos = new List<SubkontoValue>
        {
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.BANK_ACCOUNT,
                DisplayValue = bankAccountMap.GetValueOrDefault(operation.BankAccountId),
                EntityId = operation.BankAccountId,
                SortOrder = 1
            },
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.BANK_OPERATION,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    number = operation.DocNumber,
                    date = operation.DocDate
                }),
                EntityId = operation.Id,
                SortOrder = 2
            }
        };

        if (counterpartyId.HasValue && counterpartyMap.TryGetValue(counterpartyId.Value, out var counterpartyName))
        {
            subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                DisplayValue = counterpartyName,
                EntityId = counterpartyId.Value,
                SortOrder = 3
            });
        }

        if (operation.ContractId.HasValue && contractMap.TryGetValue(operation.ContractId.Value, out var contractData))
        {
            subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.CONTRACT,
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

    private async Task<Dictionary<short, string>> GetPaymentPurposeAliasMapAsync(List<short> ids)
    {
        if (ids.Count == 0)
            return new Dictionary<short, string>();

        var query = _queryBuilder.For<PaymentPurpose>()
            .Where(paymentPurpose => ids.Contains(paymentPurpose.Id))
            .As(paymentPurpose => new
            {
                paymentPurpose.Id,
                AliasCode = paymentPurpose.Alias.Code
            })
            .Build();

        var items = await _paymentPurposeQuery.GetAllAsync(query);
        return items
            .Where(item => !string.IsNullOrWhiteSpace(item.AliasCode))
            .ToDictionary(item => item.Id, item => item.AliasCode);
    }

    private sealed class ContractData
    {
        public long Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
