using Application.Features.CounterpartyBankAccounts;

namespace UnitTests;

public sealed class CounterpartyBankAccountValidatorTests
{
    [Fact]
    public void ListFilter_DefaultIsValidAndInvalidBoundsAreRejected()
    {
        var validator = new CounterpartyBankAccountListFilterValidator();

        Assert.True(validator.Validate(new CounterpartyBankAccountListFilter()).IsValid);
        Assert.False(validator.Validate(new CounterpartyBankAccountListFilter { CounterpartyId = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyBankAccountListFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyBankAccountListFilter { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyBankAccountListFilter { Search = new string('x', 51) }).IsValid);
    }

    [Fact]
    public void BaseValidator_MatchesRequestShape()
    {
        var validator = new CounterpartyBankAccountBaseDtoValidator();
        var valid = new CounterpartyBankAccountBaseDto
        {
            CounterpartyId = 1,
            BankId = 1,
            BankBranchId = 1,
            AccountNumber = new string('1', 50),
            CurrencyId = 1
        };

        Assert.True(validator.Validate(valid).IsValid);
        valid.AccountNumber = new string('1', 51);
        Assert.False(validator.Validate(valid).IsValid);
    }
}
