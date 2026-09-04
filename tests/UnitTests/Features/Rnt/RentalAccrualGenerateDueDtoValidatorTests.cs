using Application.Features.Rnt.RentalAccruals;

namespace UnitTests.Features.Rnt;

public sealed class RentalAccrualGenerateDueDtoValidatorTests
{
    [Fact]
    public void ValidatorAcceptsValidYearAndMonth()
    {
        var dto = new RentalAccrualGenerateDueDto { Year = 2024, Month = 7 };

        var result = new RentalAccrualGenerateDueDtoValidator().Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(2024, 0)]
    [InlineData(2024, 13)]
    [InlineData(0, 7)]
    [InlineData(10000, 7)]
    public void ValidatorRejectsInvalidYearOrMonth(int year, int month)
    {
        var dto = new RentalAccrualGenerateDueDto { Year = year, Month = month };

        var result = new RentalAccrualGenerateDueDtoValidator().Validate(dto);

        Assert.False(result.IsValid);
    }
}
