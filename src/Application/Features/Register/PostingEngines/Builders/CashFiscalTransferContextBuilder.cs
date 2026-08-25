using Application.Features.CashFiscalTransfers;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines.Builders;

public sealed class CashFiscalTransferContextBuilder : IPostingContextBuilder<CashFiscalTransferDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public CashFiscalTransferContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(CashFiscalTransferDoc document)
    {
        var isFiscalOut = document.DirectionId == MovementDirectionIdConst.OUT;
        return new List<PostingContext>
        {
            new()
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.CASHFISCALTRANSFER,
                AccountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId),
                DocumentId = document.Id,
                CurrencyId = document.CurrencyId,
                DocDate = document.DocDate,
                JournalNumber = document.DocNumber,
                Entries = new List<PostingEntryContext>
                {
                    new()
                    {
                        DebitAccountId = isFiscalOut ? document.CashBoxAccountId : document.FiscalCashAccountId,
                        CreditAccountId = isFiscalOut ? document.FiscalCashAccountId : document.CashBoxAccountId,
                        Amount = document.Amount,
                        Content = "Transfer between fiscal register and cash box"
                    }
                },
                Subkontos = new List<SubkontoValue>
                {
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.OrganizationCashDesks,
                        EntityId = document.CashBoxId,
                        DisplayValue = document.CashBox.Name,
                        SortOrder = 1,
                        AppliesToAccountId = document.CashBoxAccountId
                    }
                }
            }
        };
    }
}
