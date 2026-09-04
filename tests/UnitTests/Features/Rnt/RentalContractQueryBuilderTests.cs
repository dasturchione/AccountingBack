using Application.Features.Rnt.RentalContracts;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests.Features.Rnt;

public sealed class RentalContractQueryBuilderTests
{
    [Fact]
    public void DateFromUsesEarliestContractEndOrTerminationDate()
    {
        var predicate = new RentalContractCriteriaBuilder()
            .Build(new RentalContractListFilter { DateFrom = new DateTime(2026, 9, 1) })
            .Compile();
        var contract = new RentalContract
        {
            StateId = StateIdConst.ACTIVE,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 8, 31),
            TerminationDate = new DateTime(2026, 9, 20)
        };

        Assert.False(predicate(contract));
    }

    [Fact]
    public void DateFromIncludesIndefiniteActiveContract()
    {
        var predicate = new RentalContractCriteriaBuilder()
            .Build(new RentalContractListFilter { DateFrom = new DateTime(2030, 1, 1) })
            .Compile();
        var contract = new RentalContract
        {
            StateId = StateIdConst.ACTIVE,
            StartDate = new DateTime(2026, 1, 1)
        };

        Assert.True(predicate(contract));
    }
}
