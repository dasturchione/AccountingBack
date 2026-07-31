using Application.Features.Inv.OpeningInventories;

namespace UnitTests;

public sealed class OpeningInventoryValidationTests
{
    [Fact]
    public async Task CreateValidator_AcceptsValidDocument()
    {
        var dto = ValidCreateDto();

        var result = await new OpeningInventoryCreateDtoValidator().ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateValidator_RejectsMissingLines()
    {
        var dto = ValidCreateDto();
        dto.Lines.Clear();

        var result = await new OpeningInventoryCreateDtoValidator().ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(dto.Lines));
    }

    [Fact]
    public async Task CreateValidator_RejectsNonPositiveQuantityAndNegativePrice()
    {
        var dto = ValidCreateDto();
        dto.Lines[0].Quantity = 0m;
        dto.Lines[0].UnitPrice = -1m;

        var result = await new OpeningInventoryCreateDtoValidator().ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName.EndsWith(nameof(OpeningInventoryProductBaseDto.Quantity)));
        Assert.Contains(result.Errors, x => x.PropertyName.EndsWith(nameof(OpeningInventoryProductBaseDto.UnitPrice)));
    }

    [Fact]
    public async Task UpdateValidator_RequiresCompleteReplacementLines()
    {
        var dto = new OpeningInventoryUpdateDto
        {
            DocDate = new DateTime(2026, 7, 30),
            CounterpartyId = 1,
            WarehouseId = 2,
            TotalAmount = 0m
        };

        var result = await new OpeningInventoryUpdateDtoValidator().ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(dto.Lines));
    }

    private static OpeningInventoryCreateDto ValidCreateDto() =>
        new()
        {
            DocDate = new DateTime(2026, 7, 30),
            CounterpartyId = 1,
            WarehouseId = 2,
            TotalAmount = 20m,
            Lines =
            [
                new OpeningInventoryProductCreateDto
                {
                    ProductId = 3,
                    Quantity = 2m,
                    UnitId = 1,
                    UnitPrice = 10m,
                    Amount = 20m,
                    DebitAccountId = 4
                }
            ]
        };
}
