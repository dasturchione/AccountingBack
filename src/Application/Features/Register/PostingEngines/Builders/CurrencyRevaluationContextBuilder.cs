using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public sealed class CurrencyRevaluationContextBuilder : IPostingContextBuilder<CurrencyRevaluation>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public CurrencyRevaluationContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(CurrencyRevaluation document)
    {
        var result = new List<PostingContext>();
        if (document.Lines.Count == 0)
            return result;

        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);

        foreach (var line in document.Lines.Where(x => x.DifferenceAmount != 0))
        {
            var amount = Math.Abs(line.DifferenceAmount);
            var isGain = line.DifferenceAmount > 0;

            result.Add(new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.CURRENCYREVALUATION,
                AccountingPolicyId = accountingPolicyId,
                RuleId = isGain
                    ? PostingRuleIdConst.CURRENCY_REVALUATION_GAIN
                    : PostingRuleIdConst.CURRENCY_REVALUATION_LOSS,
                DocumentId = document.Id,
                DocDate = document.RevaluationDate,
                CurrencyId = CurrencyIdConst.UZS,
                JournalNumber = document.Id.ToString(),
                SourceLineId = line.Id,
                Amounts = new Dictionary<string, decimal>
                {
                    [AmountSourceConst.Total] = amount
                }
            });
        }

        return result;
    }
}
