using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Rnt.RentalAccruals;

public static class RentalAccrualDraftFactory
{
    public static RentalAccrualDoc Create(
        RentalContract contract,
        string docNumber,
        DateTime docDate,
        IReadOnlyCollection<RentalAccrualDraftSource> sources,
        int? createdByUserId)
    {
        if (contract.IsFreeOfCharge)
            throw new InvalidOperationException("A free rental contract cannot produce accrual documents.");
        if (sources.Count == 0)
            throw new ArgumentException("At least one due rental object is required.", nameof(sources));
        if (sources
            .GroupBy(x => new { x.ContractObject.Id, x.Period.PeriodFrom, x.Period.PeriodTo })
            .Any(group => group.Count() > 1))
            throw new ArgumentException("A rental object period can be included only once.", nameof(sources));

        var document = new RentalAccrualDoc
        {
            OrganizationId = contract.OrganizationId,
            ContractId = contract.Id,
            DocNumber = docNumber,
            DocDate = DateTime.SpecifyKind(docDate, DateTimeKind.Unspecified),
            CurrencyId = contract.CurrencyId,
            ExchangeRate = 1m,
            LessorPayableAccountId = contract.LessorPayableAccountId,
            TaxPayableAccountId = contract.TaxPayableAccountId,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now,
            CreatedByUserId = createdByUserId
        };

        foreach (var source in sources)
        {
            var amounts = RentalAccrualCalculator.Calculate(
                source.ContractObject.ContractAmount,
                source.ContractObject.TaxBaseAmount,
                source.ContractObject.TaxRate);

            document.Items.Add(new RentalAccrualDocItem
            {
                ContractObjectId = source.ContractObject.Id,
                PeriodFrom = source.Period.PeriodFrom,
                PeriodTo = source.Period.PeriodTo,
                ContractAmount = source.ContractObject.ContractAmount,
                TaxBaseAmount = source.ContractObject.TaxBaseAmount,
                TaxRate = source.ContractObject.TaxRate,
                TaxAmount = amounts.TaxAmount,
                PayableAmount = amounts.PayableAmount,
                Amount = amounts.Amount,
                ExpenseAccountId = source.ContractObject.ExpenseAccountId
            });
        }

        document.ContractAmount = document.Items.Sum(x => x.ContractAmount);
        document.TaxBaseAmount = document.Items.Sum(x => x.TaxBaseAmount);
        document.TaxAmount = document.Items.Sum(x => x.TaxAmount);
        document.PayableAmount = document.Items.Sum(x => x.PayableAmount);
        document.Amount = document.Items.Sum(x => x.Amount);
        return document;
    }
}

public readonly record struct RentalAccrualDraftSource(
    RentalContractObject ContractObject,
    RentalAccrualPeriod Period);
