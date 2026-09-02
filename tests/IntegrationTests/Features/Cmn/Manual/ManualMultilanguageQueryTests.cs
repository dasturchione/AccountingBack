using Application.Abstractions.Authentication;
using Application.Features.Manual;
using Domain.Entities;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Constants;

namespace IntegrationTests.Features.Cmn.Manual;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class ManualMultilanguageQueryTests(PostgreSqlIntegrationFixture fixture)
{
    private static readonly DateTime SeedDate = new(2026, 9, 2);

    [Fact]
    public async Task TranslationBackedLookupsUseRequestedLanguageFallbackActiveFilterAndNameOrder()
    {
        await SeedDirectNameLookupsAsync();
        await using var provider = ApplicationQueryTestServiceProvider.Create(
            fixture,
            User(),
            services => services.AddScoped<IManualService, ManualService>());
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IManualService>();

        AssertLookup(await service.GetCurrenciesAsync(), 25002, 25001, 25003, "AA RU Currency", "BB Base Currency");
        AssertLookup(await service.GetDocumentStatusesAsync(), 25012, 25011, 25013, "AA RU Document status", "BB Base Document status");
        AssertLookup(await service.GetPaymentTypesAsync(), 25022, 25021, 25023, "AA RU Payment type", "BB Base Payment type");
        AssertLookup(await service.GetCostingMethodsAsync(), 25032, 25031, null, "AA RU Costing method", "BB Base Costing method");
        AssertLookup(await service.GetDocumentTypesAsync(), 25042, 25041, 25043, "AA RU Document type", "BB Base Document type");
        AssertLookup(await service.GetOperationTypesAsync(), 25052, 25051, 25053, "AA RU Operation type", "BB Base Operation type");
        AssertLookup(await service.GetMovementDirectionsAsync(), 25062, 25061, null, "AA RU Movement direction", "BB Base Movement direction");
        AssertLookup(await service.GetContractTypesAsync(), 25072, 25071, 25073, "AA RU Contract type", "BB Base Contract type");
        AssertLookup(await service.GetAccountTypesAsync(), 25082, 25081, 25083, "AA RU Account type", "BB Base Account type");
    }

    [Fact]
    public async Task ExistingTranslatedLookupsRemainLocalizedWithFallbackAndStableOrder()
    {
        await SeedExistingTranslatedLookupsAsync();
        await using var provider = ApplicationQueryTestServiceProvider.Create(
            fixture,
            User(),
            services => services.AddScoped<IManualService, ManualService>());
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IManualService>();

        AssertLookup(await service.GetUserKindsAsync(), 25102, 25101, null, "AA RU User kind", "BB Base User kind");
        AssertLookup(await service.GetBankOperationCategoriesAsync(), 25112, 25111, 25113, "AA RU Bank category", "BB Base Bank category");
        AssertLookup(await service.GetFaReceiptTypesAsync(), 25122, 25121, null, "AA RU FA receipt", "BB Base FA receipt");
        AssertLookup(await service.GetFaDisposalTypesAsync(), 25132, 25131, null, "AA RU FA disposal", "BB Base FA disposal");
        AssertLookup(await service.GetRentalObjectTypesAsync(), 25142, 25141, 25143, "AA RU Rental object", "MROF");
        AssertLookup(await service.GetProductGroupsAsync(), 25152, 25151, 25153, "AA RU Product group", "BB Base Product group");
        AssertLookup(await service.GetPaymentAcceptancePointTypesAsync(), 25162, 25161, 25163, "AA RU Acceptance point", "BB Base Acceptance point");
        AssertLookup(await service.GetPaymentMethodsAsync(), 25172, 25171, null, "AA RU Payment method", "BB Base Payment method");
        AssertLookup(await service.GetFiscalCashRegisterTypesAsync(), 25182, 25181, null, "AA RU Fiscal type", "BB Base Fiscal type");
        AssertLookup(await service.GetSubkontoTypesAsync(), 25192, 25191, 25193, "AA RU Subkonto type", "BB Base Subkonto type");
    }

    private static void AssertLookup(
        IReadOnlyCollection<SelectListDto> values,
        long translatedId,
        long fallbackId,
        long? passiveId,
        string translatedName,
        string fallbackName)
    {
        var ownedIds = passiveId.HasValue
            ? new HashSet<long> { translatedId, fallbackId, passiveId.Value }
            : new HashSet<long> { translatedId, fallbackId };
        var owned = values.Where(value => ownedIds.Contains(value.Id)).ToList();

        Assert.Collection(
            owned,
            value =>
            {
                Assert.Equal(translatedId, value.Id);
                Assert.Equal(translatedName, value.Name);
            },
            value =>
            {
                Assert.Equal(fallbackId, value.Id);
                Assert.Equal(fallbackName, value.Name);
            });
        Assert.Equal(owned.Count, owned.Select(value => value.Id).Distinct().Count());
        if (passiveId.HasValue)
            Assert.DoesNotContain(values, value => value.Id == passiveId.Value);
    }

    private static IntegrationTestUserContext User() => new()
    {
        Id = 25001,
        UserKind = CurrentUserKind.TenantUser,
        LanguageId = LanguageIdConst.RU,
        TenantId = 25001,
        OrganizationId = 25001,
        AllowedOrganizationIds = [25001]
    };

    private async Task SeedDirectNameLookupsAsync()
    {
        await using var context = fixture.CreateDbContext();

        await SeedCoreAsync(context);

        if (!await context.Currencies.AnyAsync(entity => entity.Id == 25001))
        {
            context.Currencies.AddRange(
                new Currency { Id = 25001, Code = "MCF", Name = "BB Base Currency", StateId = StateIdConst.ACTIVE },
                new Currency { Id = 25002, Code = "MCT", Name = "ZZ Base Currency Translated", StateId = StateIdConst.ACTIVE },
                new Currency { Id = 25003, Code = "MCP", Name = "CC Passive Currency", StateId = StateIdConst.PASSIVE });
            context.CurrencyTranslations.Add(new CurrencyTranslation
            {
                CurrencyId = 25002,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Currency"
            });
        }

        if (!await context.DocumentStatuses.AnyAsync(entity => entity.Id == 25011))
        {
            context.DocumentStatuses.AddRange(
                new DocumentStatus { Id = 25011, Code = "MDSF", Name = "BB Base Document status", StateId = StateIdConst.ACTIVE },
                new DocumentStatus { Id = 25012, Code = "MDST", Name = "ZZ Base Document status Translated", StateId = StateIdConst.ACTIVE },
                new DocumentStatus { Id = 25013, Code = "MDSP", Name = "CC Passive Document status", StateId = StateIdConst.PASSIVE });
            context.DocumentStatusTranslations.Add(new DocumentStatusTranslation
            {
                StatusId = 25012,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Document status"
            });
        }

        if (!await context.PaymentTypes.AnyAsync(entity => entity.Id == 25021))
        {
            context.PaymentTypes.AddRange(
                new PaymentType { Id = 25021, Code = "MPTF", Name = "BB Base Payment type", StateId = StateIdConst.ACTIVE },
                new PaymentType { Id = 25022, Code = "MPTT", Name = "ZZ Base Payment type Translated", StateId = StateIdConst.ACTIVE },
                new PaymentType { Id = 25023, Code = "MPTP", Name = "CC Passive Payment type", StateId = StateIdConst.PASSIVE });
            context.PaymentTypeTranslations.Add(new PaymentTypeTranslation
            {
                PaymentTypeId = 25022,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Payment type"
            });
        }

        if (!await context.CostingMethods.AnyAsync(entity => entity.Id == 25031))
        {
            context.CostingMethods.AddRange(
                new CostingMethod { Id = 25031, Code = "MCMF", Name = "BB Base Costing method" },
                new CostingMethod { Id = 25032, Code = "MCMT", Name = "ZZ Base Costing method Translated" });
            context.CostingMethodTranslations.Add(new CostingMethodTranslation
            {
                CostingMethodId = 25032,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Costing method"
            });
        }

        if (!await context.DocumentTypes.AnyAsync(entity => entity.Id == 25041))
        {
            context.DocumentTypes.AddRange(
                new DocumentType { Id = 25041, Code = "MDTF", Name = "BB Base Document type", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new DocumentType { Id = 25042, Code = "MDTT", Name = "ZZ Base Document type Translated", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new DocumentType { Id = 25043, Code = "MDTP", Name = "CC Passive Document type", StateId = StateIdConst.PASSIVE, CreatedDate = SeedDate });
            context.DocumentTypeTranslations.Add(new DocumentTypeTranslation
            {
                DocumentTypeId = 25042,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Document type"
            });
        }

        if (!await context.OperationTypes.AnyAsync(entity => entity.Id == 25051))
        {
            context.OperationTypes.AddRange(
                new OperationType { Id = 25051, Code = "MOTF", Name = "BB Base Operation type", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new OperationType { Id = 25052, Code = "MOTT", Name = "ZZ Base Operation type Translated", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new OperationType { Id = 25053, Code = "MOTP", Name = "CC Passive Operation type", StateId = StateIdConst.PASSIVE, CreatedDate = SeedDate });
            context.OperationTypeTranslations.Add(new OperationTypeTranslation
            {
                OperationTypeId = 25052,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Operation type"
            });
        }

        if (!await context.MovementDirections.AnyAsync(entity => entity.Id == 25061))
        {
            context.MovementDirections.AddRange(
                new MovementDirection { Id = 25061, Code = "MDF", Name = "BB Base Movement direction" },
                new MovementDirection { Id = 25062, Code = "MDT", Name = "ZZ Base Movement direction Translated" });
            context.MovementDirectionTranslations.Add(new MovementDirectionTranslation
            {
                MovementDirectionId = 25062,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Movement direction"
            });
        }

        if (!await context.ContractTypes.AnyAsync(entity => entity.Id == 25071))
        {
            context.ContractTypes.AddRange(
                new ContractType { Id = 25071, Code = "MCTYF", Name = "BB Base Contract type", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new ContractType { Id = 25072, Code = "MCTYT", Name = "ZZ Base Contract type Translated", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new ContractType { Id = 25073, Code = "MCTYP", Name = "CC Passive Contract type", StateId = StateIdConst.PASSIVE, CreatedDate = SeedDate });
            context.ContractTypeTranslations.Add(new ContractTypeTranslation
            {
                ContractTypeId = 25072,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Contract type"
            });
        }

        if (!await context.AccountTypes.AnyAsync(entity => entity.Id == 25081))
        {
            context.AccountTypes.AddRange(
                new AccountType { Id = 25081, Code = "MATF", Name = "BB Base Account type", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new AccountType { Id = 25082, Code = "MATT", Name = "ZZ Base Account type Translated", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new AccountType { Id = 25083, Code = "MATP", Name = "CC Passive Account type", StateId = StateIdConst.PASSIVE, CreatedDate = SeedDate });
            context.AccountTypeTranslations.Add(new AccountTypeTranslation
            {
                AccountTypeId = 25082,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Account type"
            });
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedExistingTranslatedLookupsAsync()
    {
        await using var context = fixture.CreateDbContext();

        await SeedCoreAsync(context);

        if (!await context.Set<UserKind>().AnyAsync(entity => entity.Id == 25101))
        {
            context.Set<UserKind>().AddRange(
                new UserKind { Id = 25101, Code = "MUKF", Name = "BB Base User kind" },
                new UserKind { Id = 25102, Code = "MUKT", Name = "ZZ Base User kind Translated" });
            context.Set<UserKindTranslation>().Add(new UserKindTranslation
            {
                UserKindId = 25102,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU User kind"
            });
        }

        if (!await context.BankOperationCategories.AnyAsync(entity => entity.Id == 25111))
        {
            context.BankOperationCategories.AddRange(
                new BankOperationCategory { Id = 25111, Code = "MBCF", Name = "BB Base Bank category", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new BankOperationCategory { Id = 25112, Code = "MBCT", Name = "ZZ Base Bank category Translated", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new BankOperationCategory { Id = 25113, Code = "MBCP", Name = "CC Passive Bank category", StateId = StateIdConst.PASSIVE, CreatedDate = SeedDate });
            context.BankOperationCategoryTranslations.Add(new BankOperationCategoryTranslation
            {
                CategoryId = 25112,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Bank category"
            });
        }

        if (!await context.Set<FaReceiptType>().AnyAsync(entity => entity.Id == 25121))
        {
            context.Set<FaReceiptType>().AddRange(
                new FaReceiptType { Id = 25121, Code = "MFRF", Name = "BB Base FA receipt" },
                new FaReceiptType { Id = 25122, Code = "MFRT", Name = "ZZ Base FA receipt Translated" });
            context.Set<FaReceiptTypeTranslation>().Add(new FaReceiptTypeTranslation
            {
                ReceiptTypeId = 25122,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU FA receipt"
            });
        }

        if (!await context.Set<FaDisposalType>().AnyAsync(entity => entity.Id == 25131))
        {
            context.Set<FaDisposalType>().AddRange(
                new FaDisposalType { Id = 25131, Code = "MFDF", Name = "BB Base FA disposal" },
                new FaDisposalType { Id = 25132, Code = "MFDT", Name = "ZZ Base FA disposal Translated" });
            context.Set<FaDisposalTypeTranslation>().Add(new FaDisposalTypeTranslation
            {
                DisposalTypeId = 25132,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU FA disposal"
            });
        }

        if (!await context.RentalObjectTypes.AnyAsync(entity => entity.Id == 25141))
        {
            context.RentalObjectTypes.AddRange(
                new RentalObjectType { Id = 25141, Code = "MROF", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new RentalObjectType { Id = 25142, Code = "MROT", StateId = StateIdConst.ACTIVE, CreatedDate = SeedDate },
                new RentalObjectType { Id = 25143, Code = "MROP", StateId = StateIdConst.PASSIVE, CreatedDate = SeedDate });
            context.RentalObjectTypeTranslations.Add(new RentalObjectTypeTranslation
            {
                RentalObjectTypeId = 25142,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Rental object"
            });
        }

        if (!await context.ProductGroups.AnyAsync(entity => entity.Id == 25151))
        {
            context.ProductGroups.AddRange(
                ProductGroup(25151, "MPGF", "BB Base Product group", StateIdConst.ACTIVE),
                ProductGroup(25152, "MPGT", "ZZ Base Product group Translated", StateIdConst.ACTIVE),
                ProductGroup(25153, "MPGP", "CC Passive Product group", StateIdConst.PASSIVE));
            context.Set<ProductGroupTranslation>().Add(new ProductGroupTranslation
            {
                ProductGroupId = 25152,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Product group"
            });
        }

        if (!await context.PaymentAcceptancePointTypes.AnyAsync(entity => entity.Id == 25161))
        {
            context.PaymentAcceptancePointTypes.AddRange(
                AcceptancePointType(25161, "MAPF", "BB Base Acceptance point", StateIdConst.ACTIVE),
                AcceptancePointType(25162, "MAPT", "ZZ Base Acceptance point Translated", StateIdConst.ACTIVE),
                AcceptancePointType(25163, "MAPP", "CC Passive Acceptance point", StateIdConst.PASSIVE));
            context.PaymentAcceptancePointTypeTranslations.Add(new PaymentAcceptancePointTypeTranslation
            {
                PaymentAcceptancePointTypeId = 25162,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Acceptance point"
            });
        }

        if (!await context.PaymentMethods.AnyAsync(entity => entity.Id == 25171))
        {
            context.PaymentMethods.AddRange(
                new PaymentMethod { Id = 25171, Code = "MPMF", Name = "BB Base Payment method" },
                new PaymentMethod { Id = 25172, Code = "MPMT", Name = "ZZ Base Payment method Translated" });
            context.PaymentMethodTranslations.Add(new PaymentMethodTranslation
            {
                PaymentMethodId = 25172,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Payment method"
            });
        }

        if (!await context.FiscalCashRegisterTypes.AnyAsync(entity => entity.Id == 25181))
        {
            context.FiscalCashRegisterTypes.AddRange(
                new FiscalCashRegisterType { Id = 25181, Code = "MFTF", Name = "BB Base Fiscal type" },
                new FiscalCashRegisterType { Id = 25182, Code = "MFTT", Name = "ZZ Base Fiscal type Translated" });
            context.FiscalCashRegisterTypeTranslations.Add(new FiscalCashRegisterTypeTranslation
            {
                CashRegisterTypeId = 25182,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Fiscal type"
            });
        }

        if (!await context.SubkontoTypes.AnyAsync(entity => entity.Id == 25191))
        {
            context.SubkontoTypes.AddRange(
                SubkontoType(25191, "MSTF", "BB Base Subkonto type", StateIdConst.ACTIVE),
                SubkontoType(25192, "MSTT", "ZZ Base Subkonto type Translated", StateIdConst.ACTIVE),
                SubkontoType(25193, "MSTP", "CC Passive Subkonto type", StateIdConst.PASSIVE));
            context.SubkontoTypeTranslations.Add(new SubkontoTypeTranslation
            {
                SubkontoTypeId = 25192,
                LanguageId = LanguageIdConst.RU,
                Name = "AA RU Subkonto type"
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCoreAsync(AppDbContext context)
    {

        if (!await context.States.AnyAsync(state => state.Id == StateIdConst.ACTIVE))
        {
            context.States.Add(new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "Active",
                FullName = "Active",
                CreatedDate = SeedDate
            });
        }

        if (!await context.States.AnyAsync(state => state.Id == StateIdConst.PASSIVE))
        {
            context.States.Add(new State
            {
                Id = StateIdConst.PASSIVE,
                ShortName = "Passive",
                FullName = "Passive",
                CreatedDate = SeedDate
            });
        }

        if (!await context.Languages.AnyAsync(language => language.Id == LanguageIdConst.RU))
        {
            context.Languages.Add(new Language
            {
                Id = LanguageIdConst.RU,
                Code = "ru",
                Name = "Russian",
                NativeName = "Русский",
                IsDefault = false,
                SortOrder = 2,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = SeedDate
            });
        }
    }

    private static ProductGroup ProductGroup(int id, string code, string name, short stateId) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        StateId = stateId,
        IsAssignable = true,
        SortOrder = id,
        CreatedDate = SeedDate
    };

    private static PaymentAcceptancePointType AcceptancePointType(short id, string code, string name, short stateId) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        StateId = stateId,
        CreatedDate = SeedDate
    };

    private static SubkontoType SubkontoType(short id, string code, string name, short stateId) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        SourceTable = "manual_test",
        StateId = stateId,
        CreatedDate = SeedDate
    };
}
