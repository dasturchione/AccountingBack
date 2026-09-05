using Application.Features.Rnt.RentalAccruals;
using Domain.Entities;

namespace UnitTests.Features.Rnt;

public sealed class RentalAccrualDraftFactoryTests
{
    [Theory]
    [InlineData(2026, 2, 1189285.71)]
    [InlineData(2026, 3, 3942972.35)]
    [InlineData(2026, 4, 3624408.61)]
    [InlineData(2026, 5, 2343333.33)]
    public void CreateAllocatesIjaraContractMonthAcrossCalendarMonths(
        int year,
        int month,
        decimal expectedAmount)
    {
        var contract = new RentalContract
        {
            Id = 1,
            OrganizationId = 1,
            CurrencyId = 1,
            StartDate = new DateTime(2026, 2, 20),
            EndDate = new DateTime(2026, 5, 19)
        };
        var contractObject = new RentalContractObject
        {
            Id = 10,
            StartDate = contract.StartDate,
            PeriodAmount = 3700000m,
            TaxBaseAmount = 3700000m,
            TaxRate = 0m
        };
        var period = RentalAccrualSchedule.GetPeriodForMonth(
            RentalAccrualSchedule.GetMonth(year, month),
            "MONTH",
            contract.StartDate,
            contract.EndDate.Value);

        Assert.NotNull(period);

        var document = RentalAccrualDraftFactory.Create(
            contract,
            "1",
            RentalAccrualSchedule.GetMonth(year, month).EndDate,
            [new RentalAccrualDraftSource(contractObject, period.Value)],
            1);

        Assert.Equal(expectedAmount, document.ContractAmount);
    }

    [Fact]
    public void CreateCalculatesCalendarMonthAccrualsIncludingPartialFirstAndLastMonths()
    {
        var contract = new RentalContract
        {
            Id = 1,
            OrganizationId = 1,
            CurrencyId = 1,
            StartDate = new DateTime(2024, 7, 9),
            EndDate = new DateTime(2024, 10, 9)
        };
        var contractObject = new RentalContractObject
        {
            Id = 10,
            StartDate = contract.StartDate,
            PeriodAmount = 470000m,
            TaxBaseAmount = 470000m,
            TaxRate = 0m
        };
        var months = new[]
        {
            RentalAccrualSchedule.GetMonth(2024, 7),
            RentalAccrualSchedule.GetMonth(2024, 8),
            RentalAccrualSchedule.GetMonth(2024, 9),
            RentalAccrualSchedule.GetMonth(2024, 10)
        };
        var sources = months
            .Select(month => new RentalAccrualDraftSource(
                contractObject,
                RentalAccrualSchedule.GetPeriodForMonth(
                    month,
                    "MONTH",
                    contract.StartDate,
                    contract.EndDate!.Value)!.Value))
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
        Assert.Equal(470000m, document.Items.Single(x => x.PeriodFrom.Month == 8).ContractAmount);
        Assert.Equal(465956.99m, document.Items.Single(x => x.PeriodFrom.Month == 9).ContractAmount);
        Assert.Equal(140494.62m, document.Items.OrderBy(x => x.PeriodFrom).Last().ContractAmount);
        Assert.Equal(140494.62m, document.Items.OrderBy(x => x.PeriodFrom).Last().TaxBaseAmount);
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
