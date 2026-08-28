using Application.Features.CashCollections;
using Application.Features.BankOperations;
using Application.Features.Register.PostingEngines;
using Application.Features.Register.PostingEngines.Builders;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class CashCollectionTests
{
    [Fact]
    public void BankLinkPolicy_AcceptsExactInTransitCollection()
    {
        var document = CreateInTransitDocument();

        var result = CashCollectionBankLinkPolicy.Validate(
            document,
            organizationId: 10,
            bankAccountId: 20,
            directionId: MovementDirectionIdConst.IN,
            currencyId: 1,
            amount: 125_000m);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(11, 20, 1, 1, 125_000, "CashCollection.OrganizationMismatch")]
    [InlineData(10, 21, 1, 1, 125_000, "CashCollection.BankAccountMismatch")]
    [InlineData(10, 20, -1, 1, 125_000, "CashCollection.DirectionMustBeIn")]
    [InlineData(10, 20, 1, 2, 125_000, "CashCollection.CurrencyMismatch")]
    [InlineData(10, 20, 1, 1, 124_999, "CashCollection.AmountMismatch")]
    public void BankLinkPolicy_RejectsMismatchedBankOperation(
        int organizationId,
        int bankAccountId,
        short directionId,
        short currencyId,
        decimal amount,
        string expectedErrorCode)
    {
        var result = CashCollectionBankLinkPolicy.Validate(
            CreateInTransitDocument(),
            organizationId,
            bankAccountId,
            directionId,
            currencyId,
            amount);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public void BankLinkPolicy_RejectsDocumentThatIsNotInTransit()
    {
        var document = CreateInTransitDocument();
        document.StatusId = DocumentStatusIdConst.DRAFT;

        var result = CashCollectionBankLinkPolicy.Validate(
            document,
            10,
            20,
            MovementDirectionIdConst.IN,
            1,
            125_000m);

        Assert.False(result.IsSuccess);
        Assert.Equal("CashCollection.NotInTransit", result.Error.Code);
    }

    [Fact]
    public void CancellationPolicy_RejectsCollectionWithActiveBankOperation()
    {
        var document = CreateInTransitDocument();

        var result = CashCollectionBankLinkPolicy.ValidateCancellation(document, hasActiveBankOperation: true);

        Assert.False(result.IsSuccess);
        Assert.Equal("CashCollection.ActiveBankOperation", result.Error.Code);
    }

    [Fact]
    public void BankLinkPolicy_AppliesInternalSettlementAccountsAndClearsCounterpartyLinks()
    {
        var document = CreateInTransitDocument();
        var registry = new DocumentRegistry
        {
            Id = 77,
            DocumentTypeId = DocumentTypeIdConst.CASHCOLLECTION,
            DocumentId = document.Id
        };
        var operation = new BankOperation
        {
            CounterpartyId = 31,
            CounterpartyBankAccountId = 32,
            ContractId = 33
        };

        CashCollectionBankLinkPolicy.Apply(operation, registry, document, cashCollectionCategoryId: 4);

        Assert.Equal(registry.Id, operation.RelatedDocumentId);
        Assert.Equal(document.BankChartAccountId, operation.BankChartAccountId);
        Assert.Equal(document.CashInTransitAccountId, operation.OffsetAccountId);
        Assert.Equal((short)4, operation.ClassificationCategoryId);
        Assert.Equal(PaymentTypeIdConst.BANK, operation.PaymentTypeId);
        Assert.Null(operation.CounterpartyId);
        Assert.Null(operation.CounterpartyBankAccountId);
        Assert.Null(operation.ContractId);
    }

    [Fact]
    public async Task AccountingContext_PostsCashToTransitOnly()
    {
        var document = CreateInTransitDocument();
        document.CashBox = new CashBox { Id = 12, Name = "Main cash box" };
        document.BankAccount = new BankAccount { Id = 20, AccountNumber = "20208000123456789001" };
        var builder = new CashCollectionContextBuilder(new StubAccountingPolicyResolver(7));

        var context = Assert.Single(await builder.BuildAsync(document));
        var entry = Assert.Single(context.Entries);

        Assert.Equal(DocumentTypeIdConst.CASHCOLLECTION, context.DocumentTypeId);
        Assert.Equal(302, entry.DebitAccountId);
        Assert.Equal(301, entry.CreditAccountId);
        Assert.Equal(125_000m, entry.Amount);
        Assert.DoesNotContain(context.Entries, x => x.DebitAccountId == 303 || x.CreditAccountId == 303);
    }

    [Fact]
    public void InTransitProjection_ReturnsCompactUserFacingData()
    {
        var document = CreateInTransitDocument();
        document.DocNumber = "7";
        document.DocDate = new DateTime(2026, 8, 28, 10, 30, 0);
        document.CashBox = new CashBox { Id = 12, Name = "Main cash box" };
        document.BankAccount = new BankAccount
        {
            Id = 20,
            AccountNumber = "20208000123456789001",
            Bank = new Bank { Name = "Trustbank" }
        };
        document.Currency = new Currency { Id = 1, Code = "UZS", Name = "So'm" };

        var dto = new CashCollectionInTransitDtoProjection().Build().Compile()(document);

        Assert.Equal(document.Id, dto.Id);
        Assert.Equal("7", dto.DocNumber);
        Assert.Equal("Main cash box", dto.CashBoxName);
        Assert.Equal("20208000123456789001", dto.BankAccountNumber);
        Assert.Equal("Trustbank", dto.BankName);
        Assert.Equal("UZS", dto.CurrencyCode);
        Assert.Equal(125_000m, dto.Amount);
    }

    [Fact]
    public void BaseDtoValidator_RejectsInvalidAmountRateAndDate()
    {
        var result = new CashCollectionBaseDtoValidator().Validate(new CashCollectionBaseDto
        {
            CashBoxId = 1,
            BankAccountId = 1,
            CurrencyId = 1,
            Amount = 0,
            ExchangeRate = 0,
            DocDate = default
        });

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CashCollectionBaseDto.Amount));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CashCollectionBaseDto.ExchangeRate));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CashCollectionBaseDto.DocDate));
    }

    [Fact]
    public void BankOperationProjections_ReturnUniversalDocumentLink()
    {
        var entity = new BankOperation
        {
            RelatedDocumentId = 77,
            RelatedDocument = new DocumentRegistry
            {
                Id = 77,
                DocumentTypeId = DocumentTypeIdConst.SALARY,
                DocumentId = 91,
                DocNumber = "PAY-12",
                DocDate = new DateTime(2026, 8, 28),
                DocumentType = new DocumentType { Name = "Salary" }
            },
            Organization = new Organization(),
            BankAccount = new BankAccount { Bank = new Bank() },
            Direction = new MovementDirection(),
            Currency = new Currency(),
            Status = new DocumentStatus(),
            State = new State()
        };

        var detail = new BankOperationDtoProjection().Build().Compile()(entity);
        var list = new BankOperationListDtoProjection().Build().Compile()(entity);

        Assert.Equal(77, detail.RelatedDocumentId);
        Assert.Equal(DocumentTypeIdConst.SALARY, detail.RelatedDocumentTypeId);
        Assert.Equal(91, detail.RelatedDocumentEntityId);
        Assert.Equal("PAY-12", detail.RelatedDocumentNumber);
        Assert.Equal("Salary", detail.RelatedDocumentTypeName);
        Assert.Equal(77, list.RelatedDocumentId);
        Assert.Equal("PAY-12", list.RelatedDocumentNumber);
    }

    private static CashCollectionDoc CreateInTransitDocument() => new()
    {
        Id = 45,
        OrganizationId = 10,
        CashBoxId = 12,
        BankAccountId = 20,
        CurrencyId = 1,
        Amount = 125_000m,
        ExchangeRate = 1m,
        CashChartAccountId = 301,
        CashInTransitAccountId = 302,
        BankChartAccountId = 303,
        StatusId = DocumentStatusIdConst.IN_TRANSIT,
        StateId = StateIdConst.ACTIVE
    };

    private sealed class StubAccountingPolicyResolver(short value) : IOrganizationAccountingPolicyResolver
    {
        public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
            Task.FromResult(value);
    }
}
