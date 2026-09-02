using Application.Features.Warehouses;

namespace UnitTests;

public sealed class WarehouseValidatorTests
{
    [Fact]
    public void ListFilter_DefaultIsValidAndInvalidBoundsAreRejected()
    {
        var validator = new WarehouseListFilterValidator();

        Assert.True(validator.Validate(new WarehouseListFilter()).IsValid);
        Assert.False(validator.Validate(new WarehouseListFilter { BranchId = 0 }).IsValid);
        Assert.False(validator.Validate(new WarehouseListFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new WarehouseListFilter { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new WarehouseListFilter { Search = new string('x', 251) }).IsValid);
    }

    [Fact]
    public void BaseValidatorMatchesDomainStringAndReferenceBounds()
    {
        var validator = new WarehouseBaseDtoValidator();
        var dto = new WarehouseBaseDto
        {
            BranchId = 1,
            ResponsibleUserId = 1,
            Code = new string('c', 100),
            Name = "Warehouse",
            Address = new string('a', 1000)
        };

        Assert.True(validator.Validate(dto).IsValid);
        dto.BranchId = 0;
        dto.ResponsibleUserId = 0;
        dto.Code = new string('c', 101);
        dto.Address = new string('a', 1001);
        Assert.False(validator.Validate(dto).IsValid);
    }
}
