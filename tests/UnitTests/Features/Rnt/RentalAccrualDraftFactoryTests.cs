using Application.Features.Rnt.RentalAccruals;
using Domain.Entities;

namespace UnitTests.Features.Rnt;

public sealed class RentalAccrualDraftFactoryTests
{
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
            ContractAmount = 0,
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
