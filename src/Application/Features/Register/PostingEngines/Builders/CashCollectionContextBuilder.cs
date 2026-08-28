using Application.Features.CashCollections;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines.Builders;

public sealed class CashCollectionContextBuilder : IPostingContextBuilder<CashCollectionDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public CashCollectionContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(CashCollectionDoc document) =>
    [
        new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHCOLLECTION,
            AccountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId),
            DocumentId = document.Id,
            CurrencyId = document.CurrencyId,
            DocDate = document.DocDate,
            JournalNumber = document.DocNumber,
            Entries =
            [
                new PostingEntryContext
                {
                    DebitAccountId = document.CashInTransitAccountId,
                    CreditAccountId = document.CashChartAccountId,
                    Amount = document.Amount,
                    Content = "Cash collection sent to bank"
                }
            ],
            Subkontos =
            [
                new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                    EntityId = document.CashBoxId,
                    DisplayValue = document.CashBox.Name,
                    SortOrder = 1,
                    AppliesToAccountId = document.CashChartAccountId
                },
                new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.BankAccounts,
                    EntityId = document.BankAccountId,
                    DisplayValue = document.BankAccount.AccountNumber,
                    SortOrder = 2,
                    AppliesToAccountId = document.CashInTransitAccountId
                }
            ]
        }
    ];
}
