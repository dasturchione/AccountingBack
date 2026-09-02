using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Authentication;
using Application.Features.BankOperations;
using Application.Features.Cmn.Documents;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class DocumentRegistryTests
{
    [Fact]
    public void AppDbContext_MapsUniversalDocumentIdentity()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_metadata_only")
            .Options;

        using var context = new AppDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(DocumentRegistry));

        Assert.NotNull(entityType);
        Assert.Equal("cmn_document_registry", entityType!.GetTableName());

        var uniqueIdentity = Assert.Single(entityType.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(DocumentRegistry.DocumentTypeId), nameof(DocumentRegistry.DocumentId)]));

        Assert.Equal(
            "uq_cmn_document_registry_document",
            uniqueIdentity.GetDatabaseName());
        Assert.NotNull(entityType.FindProperty(nameof(DocumentRegistry.OrganizationId)));
    }

    [Fact]
    public void RelatedDocumentPolicy_AcceptsActiveDocumentFromCurrentOrganization()
    {
        var document = new DocumentRegistry
        {
            Id = 71,
            OrganizationId = 12,
            StateId = StateIdConst.ACTIVE
        };

        var result = BankOperationRelatedDocumentPolicy.Validate(document, organizationId: 12);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(13, StateIdConst.ACTIVE, "BankOperation.RelatedDocumentOrganizationMismatch")]
    [InlineData(12, StateIdConst.PASSIVE, "BankOperation.RelatedDocumentInactive")]
    public void RelatedDocumentPolicy_RejectsForeignOrInactiveDocument(
        int organizationId,
        short stateId,
        string expectedErrorCode)
    {
        var document = new DocumentRegistry
        {
            Id = 71,
            OrganizationId = organizationId,
            StateId = stateId
        };

        var result = BankOperationRelatedDocumentPolicy.Validate(document, organizationId: 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public void Projection_ReturnsFieldsNeededToSelectRelatedDocument()
    {
        var entity = new DocumentRegistry
        {
            Id = 81,
            OrganizationId = 12,
            DocumentTypeId = DocumentTypeIdConst.SALARY,
            DocumentId = 91,
            DocNumber = "PAY-12",
            DocDate = new DateTime(2026, 8, 28, 14, 15, 0),
            Amount = 4_200_000m,
            CurrencyId = 1,
            StatusId = DocumentStatusIdConst.POSTED,
            StateId = StateIdConst.ACTIVE,
            DocumentType = new DocumentType { Code = "salary", Name = "Salary" },
            Currency = new Currency { Code = "UZS", Name = "So'm" },
            Status = new DocumentStatus { Code = "posted", Name = "Posted" },
            State = new State { FullName = "Active" }
        };

        var dto = new DocumentRegistryDtoProjection(new TestUserContext()).Build().Compile()(entity);

        Assert.Equal(81, dto.Id);
        Assert.Equal(DocumentTypeIdConst.SALARY, dto.DocumentTypeId);
        Assert.Equal(91, dto.DocumentId);
        Assert.Equal("PAY-12", dto.DocNumber);
        Assert.Equal(4_200_000m, dto.Amount);
        Assert.Equal("UZS", dto.CurrencyCode);
        Assert.Equal("posted", dto.StatusCode);
    }

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => null;
        public int? TenantId => 1;
        public int? OrganizationId => 1;
        public List<int> AllowedOrganizationIds => [1];
        public int? BranchId => null;
    }
}
