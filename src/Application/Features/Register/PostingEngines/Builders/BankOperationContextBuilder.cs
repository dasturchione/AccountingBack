using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines;

public class BankOperationContextBuilder : IPostingContextBuilder<BankOperation>
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PaymentType> _paymentTypeQuery;
    private readonly IQueryRepository<PaymentPurpose> _paymentPurposeQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;

    public BankOperationContextBuilder(
        IQueryBuilder queryBuilder,
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<PaymentPurpose> paymentPurposeQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<Contract> contractQuery,
        IQueryRepository<BankAccount> bankAccountQuery)
    {
        _queryBuilder = queryBuilder;
        _paymentTypeQuery = paymentTypeQuery;
        _paymentPurposeQuery = paymentPurposeQuery;
        _counterpartyQuery = counterpartyQuery;
        _contractQuery = contractQuery;
        _bankAccountQuery = bankAccountQuery;
    }

    public async Task<List<PostingContext>> BuildAsync(BankOperation document)
    {
        var primaryLine = document.BankOperationLines
            .OrderBy(x => x.OrderNumber)
            .FirstOrDefault();

        if (primaryLine == null)
            throw new InvalidOperationException($"Bank operation {document.Id} has no posting line.");

        var paymentPurposeAlias = await GetPaymentPurposeAliasAsync(primaryLine.PaymentPurposeId);
        var bankAccountNumber = await GetBankAccountNumberAsync(document.BankAccountId);

        var context = new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
            AccountingPolicyId = AccountingPolicyIdConst.STANDARD_UZ,
            DocumentId = document.Id,
            CurrencyId = document.CurrencyId,
            DocDate = document.DocDate,
            JournalNumber = document.DocNumber,
            RuleId = GetRuleId(document.OperationTypeId),
            Amounts = new Dictionary<string, decimal>
            {
                [AmountSourceConst.Total] = document.Amount
            },
            PaymentMethod = await GetPaymentMethodAsync(document.PaymentTypeId),
            AllowedAliases = string.IsNullOrWhiteSpace(paymentPurposeAlias) ? Array.Empty<string>() : [paymentPurposeAlias],
            Subkontos = new List<SubkontoValue>
            {
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.BANK_ACCOUNT,
                    DisplayValue = bankAccountNumber,
                    EntityId = document.BankAccountId,
                    SortOrder = 1
                },
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.BANK_OPERATION,
                    DisplayValue = JsonSerializer.Serialize(new
                    {
                        number = document.DocNumber,
                        date = document.DocDate
                    }),
                    EntityId = (int?)document.Id,
                    SortOrder = 2
                }
            }
        };

        if (document.CounterpartyId.HasValue)
        {
            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                DisplayValue = await GetCounterpartyNameAsync(document.CounterpartyId.Value),
                EntityId = document.CounterpartyId.Value,
                SortOrder = 3
            });
        }

        if (document.ContractId.HasValue)
        {
            var contractData = await GetContractDataAsync(document.ContractId.Value);
            if (contractData != null)
            {
                context.Subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.CONTRACT,
                    DisplayValue = JsonSerializer.Serialize(new
                    {
                        number = contractData.Value.ContractNumber,
                        date = contractData.Value.ContractDate
                    }),
                    EntityId = (int?)document.ContractId.Value,
                    SortOrder = 4
                });
            }
        }

        return [context];
    }

    private static short GetRuleId(short operationTypeId) =>
        operationTypeId == OperationTypeIdConst.OUT
            ? PostingRuleIdConst.CREDIT_OPERATION
            : PostingRuleIdConst.DEBIT_OPERATION;

    private async Task<string?> GetPaymentPurposeAliasAsync(short paymentPurposeId)
    {
        var query = _queryBuilder.For<PaymentPurpose>()
            .Where(x => x.Id == paymentPurposeId)
            .As(x => x.Alias.Code)
            .Build();

        return await _paymentPurposeQuery.GetAsync(query);
    }

    private async Task<string> GetPaymentMethodAsync(short? paymentTypeId)
    {
        if (paymentTypeId is null)
            return "_default";

        var query = _queryBuilder.For<PaymentType>()
            .Where(x => x.Id == paymentTypeId.Value)
            .As(x => x.Code)
            .Build();

        return await _paymentTypeQuery.GetAsync(query) ?? "_default";
    }

    private async Task<string> GetCounterpartyNameAsync(int counterpartyId)
    {
        var query = _queryBuilder.For<CounterpartyCard>()
            .Where(x => x.Id == counterpartyId)
            .As(x => x.ShortName)
            .Build();

        return await _counterpartyQuery.GetAsync(query) ?? string.Empty;
    }

    private async Task<(string ContractNumber, DateTime ContractDate)?> GetContractDataAsync(long contractId)
    {
        var query = _queryBuilder.For<Contract>()
            .Where(x => x.Id == contractId)
            .As(x => new ContractData
            {
                ContractNumber = x.ContractNumber,
                ContractDate = x.ContractDate
            })
            .Build();

        var contract = await _contractQuery.GetAsync(query);
        return contract == null ? null : (contract.ContractNumber, contract.ContractDate);
    }

    private async Task<string> GetBankAccountNumberAsync(int bankAccountId)
    {
        var query = _queryBuilder.For<BankAccount>()
            .Where(x => x.Id == bankAccountId)
            .As(x => x.AccountNumber)
            .Build();

        return await _bankAccountQuery.GetAsync(query) ?? string.Empty;
    }

    private sealed class ContractData
    {
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime ContractDate { get; set; }
    }
}
