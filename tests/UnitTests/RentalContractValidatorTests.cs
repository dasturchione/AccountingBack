using Application.Features.Rnt.RentalAccruals;
using Application.Features.Rnt.RentalContracts;

namespace UnitTests;

public sealed class RentalContractValidatorTests
{
    [Fact]
    public void CreateValidator_RejectsMissingLessorIdentity()
    {
        var dto = ValidContract();
        dto.LessorInn = null;
        dto.LessorPinfl = null;

        var result = new RentalContractCreateDtoValidator().Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_RejectsObjectOutsideContractAndTaxBaseBelowContractAmount()
    {
        var dto = ValidContract();
        dto.Objects[0].EndDate = dto.EndDate.AddDays(1);
        dto.Objects[0].TaxBaseAmount = dto.Objects[0].ContractAmount - 1m;

        var result = new RentalContractCreateDtoValidator().Validate(dto);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }

    [Fact]
    public void AccrualUpdateValidator_RejectsDuplicateItemIds()
    {
        var dto = new RentalAccrualUpdateDto
        {
            ExchangeRate = 1m,
            LessorPayableAccountId = 201,
            TaxPayableAccountId = 202,
            Items =
            [
                new RentalAccrualItemAccountDto { ItemId = 10, ExpenseAccountId = 203 },
                new RentalAccrualItemAccountDto { ItemId = 10, ExpenseAccountId = 204 }
            ]
        };

        Assert.False(new RentalAccrualUpdateDtoValidator().Validate(dto).IsValid);
    }

    private static RentalContractCreateDto ValidContract() => new()
    {
        LessorFullName = "Ali Valiyev",
        LessorPinfl = "12345678901234",
        ContractNumber = "1",
        ContractDate = new DateTime(2026, 8, 1),
        StartDate = new DateTime(2026, 8, 1),
        EndDate = new DateTime(2027, 7, 31),
        CurrencyId = 1,
        Objects =
        [
            new RentalContractObjectInputDto
            {
                RentalObjectTypeId = 1,
                ObjectName = "Office",
                StartDate = new DateTime(2026, 8, 1),
                EndDate = new DateTime(2027, 7, 31),
                PeriodUnit = "MONTH",
                PeriodValue = 1,
                ContractAmount = 5_000_000m,
                TaxBaseAmount = 6_000_000m,
                TaxRate = 12m
            }
        ]
    };
}
