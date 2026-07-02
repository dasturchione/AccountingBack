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
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;

    public CashOperationContextBuilder(
        IQueryBuilder queryBuilder,
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery)
    {
        _queryBuilder = queryBuilder;
        _paymentTypeQuery = paymentTypeQuery;
        _counterpartyQuery = counterpartyQuery;
    }

    public async Task<List<PostingContext>> BuildAsync(CashOperation document)
    {
        var context = new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            AccountingPolicyId = AccountingPolicyIdConst.STANDARD_UZ,
            DocumentId = document.Id,
            CurrencyId = document.CurrencyId,
            DocDate = document.DocDate,
            JournalNumber = document.DocNumber,
            RuleId = GetRuleId(document),
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

    private static void AddAmountSubkonto(PostingContext context, CashOperation document)
    {
        if (document.OperationTypeId == OperationTypeIdConst.TRANSFER)
        {
            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.CASH_BOX,
                DisplayValue = $"SourceCashBox:{document.CashBoxId}",
                EntityId = document.CashBoxId,
                SortOrder = 1
            });

            if (document.DestinationCashBoxId.HasValue)
            {
                context.Subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.CASH_BOX,
                    DisplayValue = $"DestinationCashBox:{document.DestinationCashBoxId}",
                    EntityId = document.DestinationCashBoxId.Value,
                    SortOrder = 2
                });
            }

            return;
        }

        context.Subkontos.Add(new SubkontoValue
        {
            SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
            DisplayValue = JsonSerializer.Serialize(new { type = "CASH_OPERATION", id = document.Id }),
            EntityId = document.Id,
            SortOrder = 1
        });
    }

    private async Task<string> GetPaymentMethodAsync(short? paymentTypeId)
    {
        if (paymentTypeId is null)
            return "_default";

        var query = _queryBuilder.For<PaymentType>()
            .Where(x => x.Id == paymentTypeId.Value)
            .As(x => x.Code)
            .Build();

        var paymentTypeCode = await _paymentTypeQuery.GetAsync(query);
        return paymentTypeCode ?? "_default";
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
}
