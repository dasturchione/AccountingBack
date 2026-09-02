using Application.Features.CounterpartyContacts;

namespace UnitTests;

public sealed class CounterpartyContactValidatorTests
{
    [Fact]
    public void ListFilter_DefaultIsValidAndInvalidBoundsAreRejected()
    {
        var validator = new CounterpartyContactListFilterValidator();

        Assert.True(validator.Validate(new CounterpartyContactListFilter()).IsValid);
        Assert.False(validator.Validate(new CounterpartyContactListFilter { CounterpartyId = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyContactListFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyContactListFilter { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyContactListFilter { Search = new string('x', 101) }).IsValid);
    }

    [Fact]
    public void BaseValidator_AllowsSchemaEmailLengthAndRejectsOverflow()
    {
        var validator = new CounterpartyContactBaseDtoValidator();
        var dto = new CounterpartyContactBaseDto
        {
            CounterpartyId = 1,
            FullName = "Valid contact",
            Email = $"{new string('e', 237)}@example.com"
        };

        Assert.True(validator.Validate(dto).IsValid);
        dto.Email = $"{new string('e', 239)}@example.com";
        Assert.False(validator.Validate(dto).IsValid);
    }
}
