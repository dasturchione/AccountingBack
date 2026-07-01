using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public class AccountingPostingValidatorTests
{
    private readonly AccountingPostingValidator _validator = new();

    [Fact]
    public void Validate_ShouldAcceptValidPairedEntries()
    {
        var result = _validator.Validate(new[]
        {
            ValidEntry()
        });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ShouldRejectMissingCreditAccount()
    {
        var entry = ValidEntry();
        entry.CreditAccountId = null;

        var result = _validator.Validate(new[] { entry });

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPosting.MissingAccount", result.Error.Code);
    }

    [Fact]
    public void Validate_ShouldRejectSameDebitAndCreditAccount()
    {
        var entry = ValidEntry();
        entry.CreditAccountId = entry.DebitAccountId;

        var result = _validator.Validate(new[] { entry });

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPosting.SameAccount", result.Error.Code);
    }

    [Fact]
    public void Validate_ShouldAllowSameDebitAndCreditAccountForCashBoxTransfer()
    {
        var entry = ValidEntry();
        entry.DebitAccountId = entry.CreditAccountId;
        entry.RegisterEntrySubkontos.Add(new RegisterEntrySubkonto
        {
            Side = SubkontoSideConst.DEBIT,
            SubkontoTypeId = SubkontoTypeIdConst.CASH_BOX,
            EntityId = 1
        });
        entry.RegisterEntrySubkontos.Add(new RegisterEntrySubkonto
        {
            Side = SubkontoSideConst.CREDIT,
            SubkontoTypeId = SubkontoTypeIdConst.CASH_BOX,
            EntityId = 2
        });

        var result = _validator.Validate(new[] { entry });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ShouldRejectInvalidQuantity()
    {
        var entry = ValidEntry();
        entry.DebitQuantity = 0;

        var result = _validator.Validate(new[] { entry });

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPosting.InvalidQuantity", result.Error.Code);
    }

    [Fact]
    public void Validate_ShouldRejectImbalancedQuantities()
    {
        var first = ValidEntry();
        var second = ValidEntry();
        first.DebitQuantity = 10m;
        first.CreditQuantity = null;
        second.DocumentId = 11;
        second.CreditQuantity = 5m;
        second.DebitQuantity = null;

        var result = _validator.Validate(new[] { first, second });

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPosting.BalanceMismatch", result.Error.Code);
    }

    [Fact]
    public void Validate_ShouldRejectInvalidCurrency()
    {
        var entry = ValidEntry();
        entry.CurrencyId = 0;

        var result = _validator.Validate(new[] { entry });

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPosting.InvalidCurrency", result.Error.Code);
    }

    [Fact]
    public void Validate_ShouldRejectInvalidDates()
    {
        var entry = ValidEntry();
        entry.DocDate = default;

        var result = _validator.Validate(new[] { entry });

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPosting.InvalidDate", result.Error.Code);
    }

    private static AccountingRegisterEntry ValidEntry() => new()
    {
        OrganizationId = 1,
        DocumentTypeId = 1,
        DocumentId = 10,
        DebitAccountId = 101,
        CreditAccountId = 601,
        CurrencyId = 1,
        Amount = 125.50m,
        DocDate = DateTime.Today,
        CreatedDate = DateTime.Today
    };
}
