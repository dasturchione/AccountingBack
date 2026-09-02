using Application.Abstractions.Authentication;
using Application.Features.Cmn.Documents;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace IntegrationTests.Features.Cmn.Documents;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class DocumentRegistryMultilanguageQueryTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);
    private const int OrganizationId = 36001;
    private const int OtherOrganizationId = 36002;
    private const short TranslatedDocumentTypeId = 28001;
    private const short FallbackDocumentTypeId = 28002;
    private const short TranslatedStatusId = 28001;
    private const short FallbackStatusId = 28002;
    private const short TranslatedCurrencyId = 28001;
    private const short FallbackCurrencyId = 28002;

    [Fact]
    public async Task ListAndDetailTranslateAllDisplayNamesWithIndependentFallbacksAndNulls()
    {
        await SeedAsync();
        var user = User(languageId: 2);
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IDocumentRegistryService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var projection = scope.ServiceProvider
            .GetRequiredService<IProjectionBuilder<DocumentRegistry, DocumentRegistryDto>>()
            .Build();

        var sql = context.DocumentRegistries.Select(projection).ToQueryString();
        var list = await service.GetAllAsync(new DocumentRegistryListFilter());
        var detail = await service.GetByIdAsync(36001);

        Assert.True(list.IsSuccess);
        Assert.Equal([36001L, 36002L, 36003L], list.Value.Select(document => document.Id));

        var translated = list.Value.Single(document => document.Id == 36001);
        Assert.Equal("Переведённый тип документа", translated.DocumentTypeName);
        Assert.Equal("Переведённый статус", translated.StatusName);
        Assert.Equal("Переведённая валюта", translated.CurrencyName);

        var fallback = list.Value.Single(document => document.Id == 36002);
        Assert.Equal("Base fallback document", fallback.DocumentTypeName);
        Assert.Equal("Base fallback status", fallback.StatusName);
        Assert.Equal("Base fallback currency", fallback.CurrencyName);

        var nullable = list.Value.Single(document => document.Id == 36003);
        Assert.Null(nullable.StatusId);
        Assert.Null(nullable.StatusName);
        Assert.Null(nullable.CurrencyId);
        Assert.Null(nullable.CurrencyName);

        Assert.True(detail.IsSuccess);
        Assert.Equal("Переведённый тип документа", detail.Value.DocumentTypeName);
        Assert.Equal("Переведённый статус", detail.Value.StatusName);
        Assert.Equal("Переведённая валюта", detail.Value.CurrencyName);

        Assert.Contains("cmn_document_type_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cmn_document_status_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cmn_currency_translation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListFiltersAndSearchesTranslatedProjectionInSqlWithoutPaginationWrapper()
    {
        await SeedAsync();
        var user = User(languageId: 2);
        await using var provider = CreateProvider(user);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IDocumentRegistryService>();

        var translatedSearch = await service.GetAllAsync(new DocumentRegistryListFilter
        {
            DocumentTypeCode = "DOC-I18N",
            Search = "переведённый тип документа"
        });
        var dateFiltered = await service.GetAllAsync(new DocumentRegistryListFilter
        {
            DateFrom = new DateTime(2026, 9, 1)
        });
        var baseNameSearch = await service.GetAllAsync(new DocumentRegistryListFilter
        {
            Search = "Base translated document"
        });
        var inaccessible = await service.GetByIdAsync(36004);

        Assert.True(translatedSearch.IsSuccess);
        Assert.Equal([36001L, 36003L], translatedSearch.Value.Select(document => document.Id));
        Assert.Equal(
            translatedSearch.Value.Count,
            translatedSearch.Value.Select(document => document.Id).Distinct().Count());

        Assert.True(dateFiltered.IsSuccess);
        Assert.Equal([36001L, 36002L], dateFiltered.Value.Select(document => document.Id));
        Assert.True(baseNameSearch.IsSuccess);
        Assert.Empty(baseNameSearch.Value);

        Assert.False(inaccessible.IsSuccess);
        Assert.Equal("DocumentRegistry.NotFound", inaccessible.Error.Code);

        var superAdmin = User(languageId: 2);
        superAdmin.UserKind = CurrentUserKind.SuperAdmin;
        await using var superAdminProvider = CreateProvider(superAdmin);
        await using var superAdminScope = superAdminProvider.CreateAsyncScope();
        var scopedSuperAdminService = superAdminScope.ServiceProvider.GetRequiredService<IDocumentRegistryService>();
        var scopedSuperAdminList = await scopedSuperAdminService.GetAllAsync(new DocumentRegistryListFilter());
        var scopedSuperAdminOtherDetail = await scopedSuperAdminService.GetByIdAsync(36004);

        Assert.True(scopedSuperAdminList.IsSuccess);
        Assert.DoesNotContain(scopedSuperAdminList.Value, document => document.OrganizationId != OrganizationId);
        Assert.False(scopedSuperAdminOtherDetail.IsSuccess);
    }

    private ServiceProvider CreateProvider(IntegrationTestUserContext user) =>
        ApplicationQueryTestServiceProvider.Create(
            fixture,
            user,
            services => services.AddScoped<IDocumentRegistryService, DocumentRegistryService>());

    private static IntegrationTestUserContext User(short languageId) => new()
    {
        Id = 36001,
        UserKind = CurrentUserKind.TenantUser,
        LanguageId = languageId,
        TenantId = 36001,
        OrganizationId = OrganizationId,
        AllowedOrganizationIds = [OrganizationId]
    };

    private async Task SeedAsync()
    {
        await using var context = fixture.CreateDbContext();

        if (!await context.States.AnyAsync(state => state.Id == 1))
        {
            context.States.Add(new State
            {
                Id = 1,
                ShortName = "Active",
                FullName = "Active",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Languages.AnyAsync(language => language.Id == 2))
        {
            context.Languages.Add(new Language
            {
                Id = 2,
                Code = "ru",
                Name = "Russian",
                NativeName = "Русский",
                IsDefault = false,
                SortOrder = 2,
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Regions.AnyAsync(region => region.Id == 36001))
        {
            context.Regions.Add(new Region
            {
                Id = 36001,
                ShortName = "Document registry test region",
                FullName = "Document registry test region",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.PlatformTenants.AnyAsync(tenant => tenant.Id == 36001))
        {
            context.PlatformTenants.Add(new PlatformTenant
            {
                Id = 36001,
                Name = "Document registry tenant",
                Slug = "document-registry-query-tenant",
                StateId = 1,
                CreatedDate = SeedDate
            });
        }

        if (!await context.Organizations.IgnoreQueryFilters().AnyAsync(organization => organization.Id == OrganizationId))
        {
            context.Organizations.AddRange(
                Organization(OrganizationId, "Document registry organization", "360000001"),
                Organization(OtherOrganizationId, "Other document registry organization", "360000002"));
        }

        if (!await context.DocumentTypes.AnyAsync(documentType => documentType.Id == TranslatedDocumentTypeId))
        {
            context.DocumentTypes.AddRange(
                DocumentType(TranslatedDocumentTypeId, "DOC-I18N", "Base translated document", "Переведённый тип документа"),
                DocumentType(FallbackDocumentTypeId, "DOC-FALLBACK", "Base fallback document"));
        }

        if (!await context.DocumentStatuses.AnyAsync(status => status.Id == TranslatedStatusId))
        {
            context.DocumentStatuses.AddRange(
                DocumentStatus(TranslatedStatusId, "STATUS-I18N", "Base translated status", "Переведённый статус"),
                DocumentStatus(FallbackStatusId, "STATUS-FALLBACK", "Base fallback status"));
        }

        if (!await context.Currencies.AnyAsync(currency => currency.Id == TranslatedCurrencyId))
        {
            context.Currencies.AddRange(
                Currency(TranslatedCurrencyId, "DRI18N", "Base translated currency", "Переведённая валюта"),
                Currency(FallbackCurrencyId, "DRFALL", "Base fallback currency"));
        }

        if (!await context.DocumentRegistries.IgnoreQueryFilters().AnyAsync(document => document.Id == 36001))
        {
            context.DocumentRegistries.AddRange(
                Registry(36001, OrganizationId, TranslatedDocumentTypeId, 1, "DOC-1", new DateTime(2026, 9, 2), TranslatedCurrencyId, TranslatedStatusId),
                Registry(36002, OrganizationId, FallbackDocumentTypeId, 2, "DOC-2", new DateTime(2026, 9, 1), FallbackCurrencyId, FallbackStatusId),
                Registry(36003, OrganizationId, TranslatedDocumentTypeId, 3, "DOC-3", new DateTime(2026, 8, 31), null, null),
                Registry(36004, OtherOrganizationId, TranslatedDocumentTypeId, 4, "DOC-4", new DateTime(2026, 9, 3), TranslatedCurrencyId, TranslatedStatusId));
        }

        await context.SaveChangesAsync();
    }

    private static Organization Organization(int id, string name, string inn) => new()
    {
        Id = id,
        ShortName = name,
        FullName = name,
        Inn = inn,
        RegionId = 36001,
        IsParent = false,
        StateId = 1,
        CreatedDate = SeedDate,
        TenantId = 36001,
        SetupStatus = "completed"
    };

    private static DocumentType DocumentType(short id, string code, string baseName, string? russianName = null)
    {
        var documentType = new DocumentType
        {
            Id = id,
            Code = code,
            Name = baseName,
            StateId = 1,
            CreatedDate = SeedDate
        };

        if (russianName is not null)
        {
            documentType.DocumentTypeTranslations.Add(new DocumentTypeTranslation
            {
                DocumentTypeId = id,
                LanguageId = 2,
                Name = russianName
            });
        }

        return documentType;
    }

    private static DocumentStatus DocumentStatus(short id, string code, string baseName, string? russianName = null)
    {
        var status = new DocumentStatus
        {
            Id = id,
            Code = code,
            Name = baseName,
            StateId = 1
        };

        if (russianName is not null)
        {
            status.DocumentStatusTranslations.Add(new DocumentStatusTranslation
            {
                StatusId = id,
                LanguageId = 2,
                Name = russianName
            });
        }

        return status;
    }

    private static Currency Currency(short id, string code, string baseName, string? russianName = null)
    {
        var currency = new Currency
        {
            Id = id,
            Code = code,
            Name = baseName,
            Symbol = code,
            StateId = 1
        };

        if (russianName is not null)
        {
            currency.CurrencyTranslations.Add(new CurrencyTranslation
            {
                CurrencyId = id,
                LanguageId = 2,
                Name = russianName
            });
        }

        return currency;
    }

    private static DocumentRegistry Registry(
        long id,
        int organizationId,
        short documentTypeId,
        long documentId,
        string number,
        DateTime date,
        short? currencyId,
        short? statusId) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        DocumentTypeId = documentTypeId,
        DocumentId = documentId,
        DocNumber = number,
        DocDate = date,
        Amount = id,
        CurrencyId = currencyId,
        StatusId = statusId,
        StateId = 1,
        CreatedDate = SeedDate
    };
}
