using Application.Abstractions;
using Application.Features.CashOperations;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines.Builders;

public class CashOperationContextBuilder : IPostingContextBuilder<CashOperation>
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PaymentType> _paymentTypeQuery;
    private readonly IQueryRepository<PaymentPurpose> _paymentPurposeQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public CashOperationContextBuilder(
        IQueryBuilder queryBuilder,
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<PaymentPurpose> paymentPurposeQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _queryBuilder = queryBuilder;
        _paymentTypeQuery = paymentTypeQuery;
        _paymentPurposeQuery = paymentPurposeQuery;
        _counterpartyQuery = counterpartyQuery;
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(CashOperation document)
    {
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var context = new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            AccountingPolicyId = accountingPolicyId,
            DocumentId = document.Id,
            CurrencyId = document.CurrencyId,
            DocDate = document.DocDate,
            JournalNumber = document.DocNumber,
            RuleId = GetRuleId(document),
            RequiredDebitAlias = await ResolveRequiredDebitAliasAsync(document),
            RequiredCreditAlias = await ResolveRequiredCreditAliasAsync(document),
            AllowedAliases = await ResolveAllowedAliasesAsync(document),
            Amounts = new Dictionary<string, decimal>
            {
                [AmountSourceConst.Total] = document.Amount
            },
            PaymentMethod = await GetPaymentMethodAsync(document.PaymentTypeId),
            Subkontos = new List<SubkontoValue>()
        };

        AddAmountSubkonto(context, document);
        await AddCounterparty(context, document);

        return new List<PostingContext> { context };
    }

    private static short GetRuleId(CashOperation operation) =>
        operation.OperationTypeId == OperationTypeIdConst.OUT
            ? PostingRuleIdConst.CREDIT_OPERATION
            : operation.OperationTypeId == OperationTypeIdConst.TRANSFER
                ? PostingRuleIdConst.CASH_TRANSFER
                : PostingRuleIdConst.DEBIT_OPERATION;

    private async Task<string[]> ResolveAllowedAliasesAsync(CashOperation document)
    {
        var alias = await GetPaymentPurposeAliasAsync(document.PaymentPurposeId);
        return string.IsNullOrWhiteSpace(alias) ? Array.Empty<string>() : [alias];
    }

    private async Task<string?> ResolveRequiredDebitAliasAsync(CashOperation document)
    {
        if (document.OperationTypeId == OperationTypeIdConst.TRANSFER)
            return null;

        var alias = await GetPaymentPurposeAliasAsync(document.PaymentPurposeId);
        if (string.IsNullOrWhiteSpace(alias))
            return null;

        return document.OperationTypeId == OperationTypeIdConst.IN
            ? AliasConst.PaymentAccount
            : alias;
    }

    private async Task<string?> ResolveRequiredCreditAliasAsync(CashOperation document)
    {
        if (document.OperationTypeId == OperationTypeIdConst.TRANSFER)
            return null;

        var alias = await GetPaymentPurposeAliasAsync(document.PaymentPurposeId);
        if (string.IsNullOrWhiteSpace(alias))
            return null;

        return document.OperationTypeId == OperationTypeIdConst.IN
            ? alias
            : AliasConst.PaymentAccount;
    }

    private static void AddAmountSubkonto(PostingContext context, CashOperation document)
    {
        if (document.OperationTypeId == OperationTypeIdConst.TRANSFER)
        {
            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.CASH_BOX,
                DisplayValue = $"{RegisterDefaultsConst.SourceCashBoxDisplayPrefix}:{document.CashBoxId}",
                EntityId = document.CashBoxId,
                SortOrder = 1
            });

            if (document.DestinationCashBoxId.HasValue)
            {
                context.Subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.CASH_BOX,
                    DisplayValue = $"{RegisterDefaultsConst.DestinationCashBoxDisplayPrefix}:{document.DestinationCashBoxId}",
                    EntityId = document.DestinationCashBoxId.Value,
                    SortOrder = 2
                });
            }

            return;
        }

        context.Subkontos.Add(new SubkontoValue
        {
            SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
            DisplayValue = JsonSerializer.Serialize(new { type = RegisterDefaultsConst.CashOperation, id = document.Id }),
            EntityId = document.Id,
            SortOrder = 1
        });
    }

    private async Task<string> GetPaymentMethodAsync(short? paymentTypeId)
    {
        if (paymentTypeId is null)
            return RegisterDefaultsConst.DefaultDimensionValue;

        var query = _queryBuilder.For<PaymentType>()
            .Where(x => x.Id == paymentTypeId.Value)
            .As(x => x.Code)
            .Build();

        var paymentTypeCode = await _paymentTypeQuery.GetAsync(query);
        return paymentTypeCode ?? RegisterDefaultsConst.DefaultDimensionValue;
    }

    private async Task AddCounterparty(PostingContext context, CashOperation document)
    {
        if (!document.CounterpartyId.HasValue)
            return;

        var name = await GetCounterpartyNameAsync(document.CounterpartyId.Value);
        context.Subkontos.Add(new SubkontoValue
        {
            SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
            DisplayValue = name,
            EntityId = document.CounterpartyId.Value,
            SortOrder = 2
        });
    }

    private async Task<string> GetCounterpartyNameAsync(int counterpartyId)
    {
        var query = _queryBuilder.For<CounterpartyCard>()
            .Where(x => x.Id == counterpartyId)
            .As(x => x.ShortName)
            .Build();

        var name = await _counterpartyQuery.GetAsync(query);
        return name ?? string.Empty;
    }

    private async Task<string?> GetPaymentPurposeAliasAsync(short paymentPurposeId)
    {
        if (paymentPurposeId <= 0)
            return null;

        var query = _queryBuilder.For<PaymentPurpose>()
            .Where(x => x.Id == paymentPurposeId)
            .As(x => x.Alias.Code)
            .Build();

        return await _paymentPurposeQuery.GetAsync(query);
    }
}
