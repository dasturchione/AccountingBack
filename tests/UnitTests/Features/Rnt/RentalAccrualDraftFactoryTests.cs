using Application.Features.Rnt.RentalAccruals;
using Domain.Entities;

namespace UnitTests.Features.Rnt;

public sealed class RentalAccrualDraftFactoryTests
{
    [Fact]
    public void CreateCalculatesCalendarMonthAccrualsIncludingPartialFirstAndLastMonths()
    {
        var contract = new RentalContract
        {
            Id = 1,
            OrganizationId = 1,
            CurrencyId = 1,
            EndDate = new DateTime(2024, 10, 9)
        };
        var contractObject = new RentalContractObject
        {
            Id = 10,
            PeriodAmount = 470000m,
            TaxBaseAmount = 470000m,
            TaxRate = 0m
        };
        var periodStarts = new[]
        {
            new DateTime(2024, 7, 9),
            new DateTime(2024, 8, 1),
            new DateTime(2024, 9, 1),
            new DateTime(2024, 10, 1)
        };
        var sources = periodStarts
            .Select(start => new RentalAccrualDraftSource(
                contractObject,
                RentalAccrualSchedule.GetPeriod(start, "MONTH", contract.EndDate!.Value)))
            .ToArray();

        var document = RentalAccrualDraftFactory.Create(
            contract,
            "1",
            contract.EndDate.Value,
            sources,
            1);

        Assert.Equal(1425161.29m, document.ContractAmount);
        Assert.Equal(1425161.29m, document.TaxBaseAmount);
        Assert.Equal(348709.68m, document.Items.OrderBy(x => x.PeriodFrom).First().ContractAmount);
        Assert.Equal(136451.61m, document.Items.OrderBy(x => x.PeriodFrom).Last().ContractAmount);
        Assert.Equal(136451.61m, document.Items.OrderBy(x => x.PeriodFrom).Last().TaxBaseAmount);
    }

    [Fact]
    public void CreateRejectsFreeRentalContract()
    {
        var contract = new RentalContract
        {
            Id = 1,
            OrganizationId = 1,
            CurrencyId = 1,
            IsFreeOfCharge = true
        };
        var contractObject = new RentalContractObject
        {
            Id = 10,
            PeriodAmount = 0,
            TaxBaseAmount = 0,
            TaxRate = 0
        };
        var source = new RentalAccrualDraftSource(
            contractObject,
            new RentalAccrualPeriod(new DateTime(2026, 9, 1), new DateTime(2026, 9, 30), new DateTime(2026, 10, 1)));

        Assert.Throws<InvalidOperationException>(() => RentalAccrualDraftFactory.Create(
            contract,
            "1",
            new DateTime(2026, 9, 30),
            [source],
            1));
    }
}
