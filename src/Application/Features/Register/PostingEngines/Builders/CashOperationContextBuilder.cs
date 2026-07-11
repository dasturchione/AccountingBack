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
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public CashOperationContextBuilder(
        IQueryBuilder queryBuilder,
        IQueryRepository<PaymentType> paymentTypeQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _queryBuilder = queryBuilder;
        _paymentTypeQuery = paymentTypeQuery;
        _counterpartyQuery = counterpartyQuery;
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(CashOperation document)
    {
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var paymentMethod = await GetPaymentMethodAsync(document.PaymentTypeId);
        var context = new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            AccountingPolicyId = accountingPolicyId,
            DocumentId = document.Id,
            CurrencyId = document.CurrencyId,
            DocDate = document.DocDate,
            JournalNumber = document.DocNumber,
            Entries = new List<PostingEntryContext>
            {
                BuildEntry(document, paymentMethod)
            },
            Subkontos = new List<SubkontoValue>()
        };

        AddAmountSubkonto(context, document);
        await AddCounterparty(context, document);

        return new List<PostingContext> { context };
    }

    private static PostingEntryContext BuildEntry(CashOperation document, string paymentMethod) =>
        document.OperationTypeId switch
        {
            OperationTypeIdConst.IN => new PostingEntryContext
            {
                DebitAccountId = document.CashChartAccountId,
                CreditAccountId = document.OffsetAccountId,
                Amount = document.Amount,
                Content = paymentMethod
            },
            OperationTypeIdConst.OUT => new PostingEntryContext
            {
                DebitAccountId = document.OffsetAccountId,
                CreditAccountId = document.CashChartAccountId,
                Amount = document.Amount,
                Content = paymentMethod
            },
            OperationTypeIdConst.TRANSFER => new PostingEntryContext
            {
                DebitAccountId = document.OffsetAccountId,
                CreditAccountId = document.CashChartAccountId,
                Amount = document.Amount,
                Content = RegisterDefaultsConst.CashOperation
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(document.OperationTypeId),
                document.OperationTypeId,
                "Unsupported cash operation type for accounting posting.")
        };

    private static void AddAmountSubkonto(PostingContext context, CashOperation document)
    {
        if (document.OperationTypeId == OperationTypeIdConst.TRANSFER)
        {
            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                DisplayValue = $"{RegisterDefaultsConst.SourceCashBoxDisplayPrefix}:{document.CashBoxId}",
                EntityId = document.CashBoxId,
                SortOrder = 1,
                AppliesToAccountId = document.CashChartAccountId
            });

            if (document.DestinationCashBoxId.HasValue)
            {
                context.Subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                    DisplayValue = $"{RegisterDefaultsConst.DestinationCashBoxDisplayPrefix}:{document.DestinationCashBoxId}",
                    EntityId = document.DestinationCashBoxId.Value,
                    SortOrder = 2,
                    AppliesToAccountId = document.OffsetAccountId
                });
            }

            return;
        }

        context.Subkontos.Add(new SubkontoValue
        {
            SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
            DisplayValue = JsonSerializer.Serialize(new { type = RegisterDefaultsConst.CashOperation, id = document.Id }),
            EntityId = document.CashBoxId,
            SortOrder = 1,
            AppliesToAccountId = document.CashChartAccountId
        });

        context.Subkontos.Add(new SubkontoValue
        {
            SubkontoTypeId = SubkontoTypeIdConst.CounterpartySettlementDocuments,
            DisplayValue = JsonSerializer.Serialize(new
            {
                number = document.DocNumber,
                date = document.DocDate
            }),
            EntityId = document.Id,
            SortOrder = 2
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
            SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
            DisplayValue = name,
            EntityId = document.CounterpartyId.Value,
            SortOrder = 3
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
