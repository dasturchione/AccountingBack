using Application.Abstractions.Integration;
using Application.Abstractions;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using Application.Features.Cmn.Taxes.Integration.Mappers;
using Application.Features.Integration;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Query;
using SharedKernel.Results;
using SharedKernel.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Features.Cmn.Taxes.Integration.Services;

public sealed class TaxIntegrationService : ITaxIntegrationService
{
    private readonly ITaxProviderFactory _factory;
    private readonly IDidoxAuthClient _didoxAuthClient;
    private readonly IDidoxDocumentClient _didoxDocumentClient;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<SaleDoc> _saleDocQuery;
    private readonly IQueryRepository<OrganizationTaxSetting> _taxSettingQuery;
    private readonly IQueryRepository<DidoxMxikCatalog> _mxikCatalogQuery;
    private readonly IQueryRepository<UnitDidoxPackage> _unitPackageQuery;
    private readonly IQueryRepository<ProductTableDidoxOrigin> _productTableOriginQuery;
    private readonly IQueryRepository<ProductDidoxProfile> _productProfileQuery;
    private readonly IQueryRepository<CounterpartyDidoxProfile> _counterpartyProfileQuery;
    private readonly IQueryRepository<DidoxOrigin> _didoxOriginQuery;
    private readonly IQueryRepository<DidoxVatRegStatus> _didoxVatStatusQuery;
    private readonly IOrganizationScopeResolver _scopeResolver;
    private readonly IProviderCredentialStore _credentialStore;
    private readonly IProviderSessionStore _sessionStore;
    private readonly ISecretProtector _secretProtector;
    private readonly IUnitOfWork _unitOfWork;

    public TaxIntegrationService(
        ITaxProviderFactory factory,
        IDidoxAuthClient didoxAuthClient,
        IDidoxDocumentClient didoxDocumentClient,
        IQueryBuilder queryBuilder,
        IQueryRepository<SaleDoc> saleDocQuery,
        IQueryRepository<OrganizationTaxSetting> taxSettingQuery,
        IQueryRepository<DidoxMxikCatalog> mxikCatalogQuery,
        IQueryRepository<UnitDidoxPackage> unitPackageQuery,
        IQueryRepository<ProductTableDidoxOrigin> productTableOriginQuery,
        IQueryRepository<ProductDidoxProfile> productProfileQuery,
        IQueryRepository<CounterpartyDidoxProfile> counterpartyProfileQuery,
        IQueryRepository<DidoxOrigin> didoxOriginQuery,
        IQueryRepository<DidoxVatRegStatus> didoxVatStatusQuery,
        IOrganizationScopeResolver scopeResolver,
        IProviderCredentialStore credentialStore,
        IProviderSessionStore sessionStore,
        ISecretProtector secretProtector,
        IUnitOfWork unitOfWork)
    {
        _factory = factory;
        _didoxAuthClient = didoxAuthClient;
        _didoxDocumentClient = didoxDocumentClient;
        _queryBuilder = queryBuilder;
        _saleDocQuery = saleDocQuery;
        _taxSettingQuery = taxSettingQuery;
        _mxikCatalogQuery = mxikCatalogQuery;
        _unitPackageQuery = unitPackageQuery;
        _productTableOriginQuery = productTableOriginQuery;
        _productProfileQuery = productProfileQuery;
        _counterpartyProfileQuery = counterpartyProfileQuery;
        _didoxOriginQuery = didoxOriginQuery;
        _didoxVatStatusQuery = didoxVatStatusQuery;
        _scopeResolver = scopeResolver;
        _credentialStore = credentialStore;
        _sessionStore = sessionStore;
        _secretProtector = secretProtector;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<IReadOnlyCollection<TaxProviderInfoDto>>> GetSupportedProvidersAsync(CancellationToken ct = default)
        => Task.FromResult(Result.Success(_factory.GetSupportedProviders()));

    public Task<Result<IReadOnlyCollection<TaxProviderInfoDto>>> GetLookupProvidersAsync(CancellationToken ct = default)
        => Task.FromResult(Result.Success(_factory.GetSupportedLookupProviders()));

    public Task<Result<IReadOnlyCollection<TaxProviderInfoDto>>> GetDocumentProvidersAsync(CancellationToken ct = default)
        => Task.FromResult(Result.Success(_factory.GetSupportedDocumentProviders()));

    public async Task<Result<IReadOnlyCollection<TaxLookupItemDto>>> SearchMxikAsync(TaxLookupRequestDto request, CancellationToken ct = default)
        => await ExecuteLookupAsync("MXIK", request, ct);

    public async Task<Result<TaxLookupItemDto?>> GetMxikByCodeAsync(string code, CancellationToken ct = default)
    {
        var provider = _factory.Resolve("MXIK");
        if (provider is not ITaxLookupProvider lookupProvider)
            return Result.Failure<TaxLookupItemDto?>(Error.NotFound("Tax.ProviderNotFound", "MXIK provider was not found."));

        var result = await lookupProvider.GetByCodeAsync(code, ct);
        return Result.Success(result is null
            ? null
            : new TaxLookupItemDto
            {
                Code = result.Code,
                Name = result.Name,
                Description = result.Description,
                IsActive = result.IsActive,
                Metadata = result.Metadata
            });
    }

    public async Task<Result<IReadOnlyCollection<TaxLookupItemDto>>> SearchSoliqAsync(TaxLookupRequestDto request, CancellationToken ct = default)
        => await ExecuteLookupAsync("SOLIQ_API", request, ct);

    public async Task<Result<TaxDocumentResultDto>> SubmitEFakturaAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
        => await ExecuteDocumentAsync("E_FAKTURA", "submit", request, ct);

    public async Task<Result<TaxDocumentResultDto>> GetEFakturaStatusAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
        => await ExecuteDocumentAsync("E_FAKTURA", "status", request, ct);

    public async Task<Result<TaxDocumentResultDto>> CancelEFakturaAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
        => await ExecuteDocumentAsync("E_FAKTURA", "cancel", request, ct);

    public async Task<Result<TaxDocumentResultDto>> SubmitDidoxAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var writeGate = ProviderWriteGate.RequireContract(Provider.Didox);
        if (!writeGate.IsSuccess)
            return Result.Failure<TaxDocumentResultDto>(writeGate.Error);

        if (!string.IsNullOrWhiteSpace(request.Payload))
            return await ExecuteDocumentAsync("DIDOX", "submit", request, ct);

        var mapped = await BuildDidoxPayloadFromSaleDocAsync(request, ct);
        if (!mapped.IsSuccess)
            return Result.Failure<TaxDocumentResultDto>(mapped.Error);

        var mappedRequest = new TaxDocumentRequestDto
        {
            ProviderCode = request.ProviderCode,
            DocumentNumber = request.DocumentNumber,
            ExternalDocumentId = request.ExternalDocumentId,
            Payload = mapped.Value
        };
        return await ExecuteDocumentAsync("DIDOX", "submit", mappedRequest, ct);
    }

    public async Task<Result<TaxDocumentResultDto>> GetDidoxStatusAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
        => await ExecuteDocumentAsync("DIDOX", "status", request, ct);

    public async Task<Result<TaxDocumentResultDto>> CancelDidoxAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var writeGate = ProviderWriteGate.RequireContract(Provider.Didox);
        return writeGate.IsSuccess
            ? await ExecuteDocumentAsync("DIDOX", "cancel", request, ct)
            : Result.Failure<TaxDocumentResultDto>(writeGate.Error);
    }

    public async Task<Result<DidoxTokenResultDto>> GetDidoxTokenBySignatureAsync(DidoxAuthSignatureRequestDto request, CancellationToken ct = default)
    {
        var writeGate = ProviderWriteGate.RequireContract(Provider.Didox);
        if (!writeGate.IsSuccess)
            return Result.Failure<DidoxTokenResultDto>(writeGate.Error);

        if (string.IsNullOrWhiteSpace(request.Signature))
            return Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.SignatureRequired", "E-IMZO signature is required."));

        var scope = await ResolveDidoxScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<DidoxTokenResultDto>(scope.Error);

        var result = await _didoxAuthClient.GetTokenBySignatureAsync(scope.Value.ExternalTin, request.Signature, request.Locale, ct);
        return await PersistDidoxTokenAsync(scope.Value, result, ct);
    }

    public async Task<Result<DidoxTokenResultDto>> GetDidoxTokenByPasswordAsync(DidoxAuthPasswordRequestDto request, CancellationToken ct = default)
    {
        var writeGate = ProviderWriteGate.RequireContract(Provider.Didox);
        if (!writeGate.IsSuccess)
            return Result.Failure<DidoxTokenResultDto>(writeGate.Error);

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.PasswordRequired", "Password is required."));

        var scope = await ResolveDidoxScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<DidoxTokenResultDto>(scope.Error);

        var result = await _didoxAuthClient.GetTokenByPasswordAsync(scope.Value.ExternalTin, request.Password, request.Locale, ct);
        return await PersistDidoxTokenAsync(scope.Value, result, ct);
    }

    public async Task<Result<TaxDocumentResultDto>> SignDidoxAsync(DidoxSignRequestDto request, CancellationToken ct = default)
    {
        var writeGate = ProviderWriteGate.RequireContract(Provider.Didox);
        if (!writeGate.IsSuccess)
            return Result.Failure<TaxDocumentResultDto>(writeGate.Error);

        if (string.IsNullOrWhiteSpace(request.DocumentId))
            return Result.Failure<TaxDocumentResultDto>(Error.Problem("Didox.DocumentIdRequired", "Document id is required."));

        if (string.IsNullOrWhiteSpace(request.Signature))
            return Result.Failure<TaxDocumentResultDto>(Error.Problem("Didox.SignatureRequired", "Signature is required (produced by the frontend E-IMZO flow)."));

        var scope = await ResolveDidoxScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<TaxDocumentResultDto>(scope.Error);

        var token = await LoadDidoxTokenAsync(scope.Value, ct);
        if (!token.IsSuccess)
            return Result.Failure<TaxDocumentResultDto>(token.Error);

        using (token.Value)
        {
            try
            {
                var response = await _didoxDocumentClient.SignAsync(request.DocumentId, request.Signature, token.Value.Token, ct);
                await RevokeDidoxOnUnauthorizedAsync(scope.Value, token.Value.Token, response.StatusCode, ct);
                return response.IsSuccessful
                    ? Result.Success(ToDidoxDocumentResult(response))
                    : Result.Failure<TaxDocumentResultDto>(Error.Problem("Tax.OperationFailed", "Tax provider operation failed."));
            }
            catch (IntegrationHttpException ex)
            {
                if (ex.StatusCode == 401)
                    await RevokeDidoxOnUnauthorizedAsync(scope.Value, token.Value.Token, "401", ct);
                return Result.Failure<TaxDocumentResultDto>(Error.Problem("Tax.OperationFailed", "Tax provider operation failed."));
            }
        }
    }

    public async Task<Result<IReadOnlyCollection<TaxProviderStatusDto>>> GetProviderStatusAsync(CancellationToken ct = default)
    {
        var providers = _factory.GetSupportedProviders();
        var statuses = new List<TaxProviderStatusDto>(providers.Count);

        foreach (var provider in providers)
        {
            ct.ThrowIfCancellationRequested();

            var instance = _factory.Resolve(provider.Code);
            if (instance is null)
            {
                statuses.Add(new TaxProviderStatusDto
                {
                    ProviderCode = provider.Code,
                    ProviderName = provider.Name,
                    IsEnabled = false,
                    IsConfigured = false,
                    CheckedAt = DateTime.UtcNow
                });
                continue;
            }

            statuses.Add(await instance.GetStatusAsync(ct));
        }

        return Result.Success<IReadOnlyCollection<TaxProviderStatusDto>>(statuses);
    }

    private async Task<Result<IReadOnlyCollection<TaxLookupItemDto>>> ExecuteLookupAsync(string providerCode, TaxLookupRequestDto request, CancellationToken ct)
    {
        var provider = _factory.Resolve(providerCode);
        if (provider is not ITaxLookupProvider lookupProvider)
            return Result.Failure<IReadOnlyCollection<TaxLookupItemDto>>(Error.NotFound("Tax.ProviderNotFound", $"{providerCode} provider was not found."));

        var result = await lookupProvider.SearchAsync(new TaxProviderLookupRequestDto
        {
            ProviderCode = providerCode,
            Query = request.Query,
            OrganizationId = request.OrganizationId,
            EffectiveDate = request.EffectiveDate
        }, ct);

        return Result.Success<IReadOnlyCollection<TaxLookupItemDto>>(result.Select(x => new TaxLookupItemDto
        {
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
            Metadata = x.Metadata
        }).ToList());
    }

    private async Task<Result<TaxDocumentResultDto>> ExecuteDocumentAsync(string providerCode, string operation, TaxDocumentRequestDto request, CancellationToken ct)
    {
        var provider = _factory.Resolve(providerCode);
        if (provider is not ITaxDocumentProvider documentProvider)
            return Result.Failure<TaxDocumentResultDto>(Error.NotFound("Tax.ProviderNotFound", $"{providerCode} provider was not found."));

        OrganizationScope? didoxScope = null;
        string? didoxToken = null;
        DidoxTokenLease? didoxLease = null;
        if (string.Equals(providerCode, "DIDOX", StringComparison.OrdinalIgnoreCase))
        {
            var scope = await ResolveDidoxScopeAsync(ct);
            if (!scope.IsSuccess)
                return Result.Failure<TaxDocumentResultDto>(scope.Error);

            var token = await LoadDidoxTokenAsync(scope.Value, ct);
            if (!token.IsSuccess)
                return Result.Failure<TaxDocumentResultDto>(token.Error);

            didoxScope = scope.Value;
            didoxLease = token.Value;
            didoxToken = token.Value.Token;
        }

        var model = new TaxProviderOperationRequestDto
        {
            ProviderCode = providerCode,
            OrganizationId = didoxScope?.OrganizationId ?? request.OrganizationId,
            DocumentNumber = request.DocumentNumber,
            Payload = request.Payload,
            ExternalDocumentId = request.ExternalDocumentId,
            CompanyToken = didoxScope is not null ? didoxToken : request.CompanyToken
        };

        TaxProviderOperationResultDto response;
        try
        {
            response = operation switch
            {
                "submit" => await documentProvider.SubmitAsync(model, ct),
                "status" => await documentProvider.GetDocumentStatusAsync(model, ct),
                "cancel" => await documentProvider.CancelAsync(model, ct),
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };
        }
        catch (IntegrationHttpException ex)
        {
            if (didoxScope is not null && didoxToken is not null && ex.StatusCode == 401)
                await RevokeDidoxOnUnauthorizedAsync(didoxScope, didoxToken, "401", ct);
            return Result.Failure<TaxDocumentResultDto>(Error.Problem("Tax.OperationFailed", "Tax provider operation failed."));
        }
        finally
        {
            didoxLease?.Dispose();
        }

        if (didoxScope is not null)
            await RevokeDidoxOnUnauthorizedAsync(didoxScope, didoxToken!, response.StatusCode, ct);

        return response.IsSuccessful
            ? Result.Success(string.Equals(providerCode, "DIDOX", StringComparison.OrdinalIgnoreCase)
                ? ToDidoxDocumentResult(response)
                : ToDocumentResult(response))
            : Result.Failure<TaxDocumentResultDto>(Error.Problem("Tax.OperationFailed", "Tax provider operation failed."));
    }

    private async Task<Result<string>> BuildDidoxPayloadFromSaleDocAsync(TaxDocumentRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
            return Result.Failure<string>(Error.Problem("Didox.DocumentNumberRequired", "DocumentNumber is required when building Didox payload from SaleDoc."));

        var scope = await ResolveDidoxScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<string>(scope.Error);

        var saleSpec = _queryBuilder.For<SaleDoc>()
            .Where(x => x.OrganizationId == scope.Value.OrganizationId && x.DocNumber == request.DocumentNumber)
            .Build();
        saleSpec.AddIncludes(x => x.Include(d => d.Organization));
        saleSpec.AddIncludes(x => x.Include(d => d.Organization).ThenInclude(o => o.BankAccounts).ThenInclude(a => a.Bank));
        saleSpec.AddIncludes(x => x.Include(d => d.Counterparty).ThenInclude(c => c.CounterpartyBankAccounts).ThenInclude(a => a.Bank));
        saleSpec.AddIncludes(x => x.Include(d => d.SaleDocProducts).ThenInclude(p => p.Product));
        saleSpec.AddIncludes(x => x.Include(d => d.SaleDocProducts).ThenInclude(p => p.Unit));
        saleSpec.AddIncludes(x => x.Include(d => d.SaleDocProducts).ThenInclude(p => p.VatRate));
        saleSpec.AddIncludes(x => x.Include(d => d.SaleDocProducts).ThenInclude(p => p.SaleDocTables).ThenInclude(t => t.ProductTable));
        saleSpec.AddIncludes(x => x.Include(d => d.Contract));

        var saleDoc = await _saleDocQuery.GetAsync(saleSpec, ct);
        if (saleDoc is null)
            return Result.Failure<string>(Error.NotFound("Didox.SaleDocNotFound", "SaleDoc was not found for Didox submit."));

        var taxSpec = _queryBuilder.For<OrganizationTaxSetting>()
            .Where(x => x.OrganizationId == saleDoc.OrganizationId
                && x.EffectiveFrom <= DateOnly.FromDateTime(saleDoc.DocDate)
                && (x.EffectiveTo == null || x.EffectiveTo >= DateOnly.FromDateTime(saleDoc.DocDate)))
            .Build();
        var taxSettings = await _taxSettingQuery.GetAllAsync(taxSpec, ct);
        var taxSetting = taxSettings.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();

        var enrichment = await ResolveDidoxMappingContextAsync(saleDoc, taxSetting, ct);
        if (!enrichment.IsSuccess)
            return Result.Failure<string>(enrichment.Error);

        var mapped = SaleDocToDidoxInvoiceMapper.Map(saleDoc, enrichment.Value);
        if (!mapped.IsSuccess)
            return Result.Failure<string>(mapped.Error);

        return Result.Success(JsonSerializer.Serialize(mapped.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }

    public static SaleDocToDidoxInvoiceMapper.MappingContext CreateMappingContext(
        SaleDoc saleDoc, OrganizationTaxSetting? taxSetting) =>
        new(
            taxSetting,
            saleDoc.Organization.BankAccounts.FirstOrDefault(x => x.IsMain),
            saleDoc.Counterparty.CounterpartyBankAccounts.FirstOrDefault(x => x.IsMain));

    private async Task<Result<SaleDocToDidoxInvoiceMapper.MappingContext>> ResolveDidoxMappingContextAsync(
        SaleDoc saleDoc,
        OrganizationTaxSetting? taxSetting,
        CancellationToken ct)
    {
        var organizationId = saleDoc.OrganizationId;
        var effectiveDate = DateOnly.FromDateTime(saleDoc.DocDate);

        if (saleDoc.Organization is null || saleDoc.Organization.Id != organizationId)
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(OrganizationMismatch("sale document organization"));
        if (saleDoc.Counterparty is null || saleDoc.Counterparty.OrganizationId != organizationId)
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(OrganizationMismatch("counterparty"));

        var counterpartyProfiles = await _counterpartyProfileQuery.GetAllAsync(new SharedKernel.Query.Specifications.QuerySpecification<CounterpartyDidoxProfile>
        {
            Criteria = x => x.OrganizationId == organizationId && x.CounterpartyId == saleDoc.CounterpartyId
                && x.EffectiveFrom <= effectiveDate && (!x.EffectiveTo.HasValue || x.EffectiveTo >= effectiveDate),
            OrderBy = q => q.OrderByDescending(x => x.EffectiveFrom)
        }, ct);
        var buyer = counterpartyProfiles.FirstOrDefault();
        if (buyer is null)
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing("buyer Didox profile"));
        if (string.IsNullOrWhiteSpace(buyer.VatRegCode))
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing("Buyer VatRegCode"));
        if (string.IsNullOrWhiteSpace(buyer.LegalAddress))
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing("Buyer legal address"));
        if (!await _didoxVatStatusQuery.AnyAsync(x => x.Code == buyer.VatRegStatusCode, ct))
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing($"Didox VAT registration status '{buyer.VatRegStatusCode}'"));

        var productLines = saleDoc.SaleDocProducts.OrderBy(x => x.Id).ToList();
        if (productLines.Count == 0)
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing("SaleDocProducts"));
        if (productLines.Any(x => x.Product is null || x.Product.OrganizationId != organizationId))
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(OrganizationMismatch("product"));

        var mxikCodes = productLines.Select(x => x.Product!.Mxik).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().Distinct().ToList();
        if (mxikCodes.Count != productLines.Select(x => x.Product!.Mxik).Distinct().Count() || mxikCodes.Count == 0)
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing("Product.Mxik"));
        var mxikCatalogs = await _mxikCatalogQuery.GetAllAsync(new SharedKernel.Query.Specifications.QuerySpecification<DidoxMxikCatalog>
        {
            Criteria = x => mxikCodes.Contains(x.MxikCode) && x.EffectiveFrom <= effectiveDate && (!x.EffectiveTo.HasValue || x.EffectiveTo >= effectiveDate),
            OrderBy = q => q.OrderByDescending(x => x.EffectiveFrom)
        }, ct);
        var mxikByCode = mxikCatalogs.GroupBy(x => x.MxikCode).ToDictionary(x => x.Key, x => x.First());

        var unitIds = productLines.Select(x => x.UnitId).Distinct().ToList();
        var packages = await _unitPackageQuery.GetAllAsync(new SharedKernel.Query.Specifications.QuerySpecification<UnitDidoxPackage>
        {
            Criteria = x => unitIds.Contains(x.UnitId) && x.EffectiveFrom <= effectiveDate && (!x.EffectiveTo.HasValue || x.EffectiveTo >= effectiveDate),
            OrderBy = q => q.OrderByDescending(x => x.EffectiveFrom)
        }, ct);
        var packageByUnit = packages.GroupBy(x => x.UnitId).ToDictionary(x => x.Key, x => x.First());

        var productIds = productLines.Select(x => x.ProductId).Distinct().ToList();
        var profiles = await _productProfileQuery.GetAllAsync(new SharedKernel.Query.Specifications.QuerySpecification<ProductDidoxProfile>
        {
            Criteria = x => x.OrganizationId == organizationId && productIds.Contains(x.ProductId)
                && x.EffectiveFrom <= effectiveDate && (!x.EffectiveTo.HasValue || x.EffectiveTo >= effectiveDate),
            OrderBy = q => q.OrderByDescending(x => x.EffectiveFrom)
        }, ct);
        var profileByProduct = profiles.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.First());

        var saleTables = productLines.SelectMany(x => x.SaleDocTables).ToList();
        if (saleTables.Any(x => x.ProductTable is null || x.ProductTable.OrganizationId != organizationId))
            return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(OrganizationMismatch("product table"));
        var productTableIds = saleTables.Select(x => x.ProductTableId).Distinct().ToList();
        var actualOrigins = productTableIds.Count == 0
            ? []
            : await _productTableOriginQuery.GetAllAsync(new SharedKernel.Query.Specifications.QuerySpecification<ProductTableDidoxOrigin>
            {
                Criteria = x => x.OrganizationId == organizationId && productTableIds.Contains(x.ProductTableId)
            }, ct);
        var actualOriginByTable = actualOrigins.ToDictionary(x => x.ProductTableId);
        var allOriginCodes = actualOrigins.Select(x => x.OriginCode).Concat(profiles.Select(x => x.DefaultOriginCode)).Distinct().ToList();
        var origins = allOriginCodes.Count == 0
            ? []
            : await _didoxOriginQuery.GetAllAsync(new SharedKernel.Query.Specifications.QuerySpecification<DidoxOrigin>
            {
                Criteria = x => allOriginCodes.Contains(x.Code)
            }, ct);
        var originByCode = origins.ToDictionary(x => x.Code);

        var resolved = new Dictionary<long, IReadOnlyList<SaleDocToDidoxInvoiceMapper.DidoxResolvedProductLine>>();
        foreach (var line in productLines)
        {
            var mxik = line.Product!.Mxik!;
            if (!mxikByCode.TryGetValue(mxik, out var catalog))
                return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing($"MXIK catalog '{mxik}'"));
            if (!packageByUnit.TryGetValue(line.UnitId, out var package))
                return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing($"Didox package for unit '{line.UnitId}'"));

            profileByProduct.TryGetValue(line.ProductId, out var fallbackProfile);
            var lineOrigins = new List<(SaleDocTable? Table, short Code)>();
            if (line.SaleDocTables.Count == 0)
            {
                if (fallbackProfile is null) return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing($"product Didox profile '{line.ProductId}'"));
                lineOrigins.Add((null, fallbackProfile.DefaultOriginCode));
            }
            else
            {
                foreach (var table in line.SaleDocTables)
                {
                    if (actualOriginByTable.TryGetValue(table.ProductTableId, out var actual)) lineOrigins.Add((table, actual.OriginCode));
                    else if (fallbackProfile is not null) lineOrigins.Add((table, fallbackProfile.DefaultOriginCode));
                    else return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing($"origin for product table '{table.ProductTableId}'"));
                }
            }

            var outputLines = new List<SaleDocToDidoxInvoiceMapper.DidoxResolvedProductLine>();
            foreach (var (table, originCode) in lineOrigins)
            {
                if (!originByCode.TryGetValue(originCode, out var origin))
                    return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing($"Didox origin '{originCode}'"));
                if (table is null)
                {
                    outputLines.Add(new(catalog.Name, package.PackageCode, package.PackageName, origin.Code, origin.Name, line.Quantity, line.UnitPrice, line.Amount, line.VatAmount, line.TotalAmount));
                    continue;
                }
                if (line.SaleDocTables.Count > 1 && line.Amount == 0m)
                    return Result.Failure<SaleDocToDidoxInvoiceMapper.MappingContext>(SourceMissing($"quantity allocation for SaleDocProduct '{line.Id}'"));
                var quantity = line.Amount == 0m ? line.Quantity : line.Quantity * table.Amount / line.Amount;
                outputLines.Add(new(catalog.Name, package.PackageCode, package.PackageName, origin.Code, origin.Name, quantity, line.UnitPrice, table.Amount, table.VatAmount, table.TotalAmount));
            }
            resolved[line.Id] = outputLines;
        }

        return Result.Success(CreateMappingContext(saleDoc, taxSetting) with
        {
            BuyerProfile = new(buyer.VatRegCode, buyer.VatRegStatusCode, buyer.LegalAddress),
            ProductLines = resolved
        });
    }

    private async Task<Result<OrganizationScope>> ResolveDidoxScopeAsync(CancellationToken ct)
    {
        var scope = await _scopeResolver.ResolveAsync(Provider.Didox, ct: ct);
        return scope.IsSuccess
            ? scope
            : Result.Failure<OrganizationScope>(scope.Error);
    }

    private async Task<Result<DidoxTokenResultDto>> PersistDidoxTokenAsync(
        OrganizationScope scope,
        DidoxTokenResultDto result,
        CancellationToken ct)
    {
        if (!result.IsSuccessful || string.IsNullOrWhiteSpace(result.Token))
            return Result.Failure<DidoxTokenResultDto>(DidoxAuthFailed);

        var token = result.Token;
        var expiresAtUtc = DidoxSessionPolicy.GetAccessExpiresAtUtc(DateTime.UtcNow);
        var tokenReference = _secretProtector.Protect(scope, token);
        if (!tokenReference.IsSuccess)
            return Result.Failure<DidoxTokenResultDto>(DidoxCredentialRequired);

        var credential = await _credentialStore.GetAsync(scope, CredentialKind.CompanyToken, ct);
        if (!credential.IsSuccess)
            return Result.Failure<DidoxTokenResultDto>(DidoxCredentialRequired);

        ProviderCredential? credentialEntity = credential.Value;
        if (credentialEntity is null)
        {
            var added = await _credentialStore.AddAsync(
                scope,
                CredentialKind.CompanyToken,
                tokenReference.Value,
                keyVersion: 1,
                validFromUtc: DateTime.UtcNow,
                expiresAtUtc: expiresAtUtc,
                createdByUserId: null,
                ct);
            if (!added.IsSuccess)
            {
                // A concurrent login may have won the unique scope race. Re-read the active
                // credential; never expose the store's details or the token to the caller.
                var raced = await _credentialStore.GetAsync(scope, CredentialKind.CompanyToken, ct);
                if (!raced.IsSuccess || raced.Value is null)
                    return Result.Failure<DidoxTokenResultDto>(DidoxCredentialRequired);
                credentialEntity = raced.Value;
            }
            else
            {
                credentialEntity = added.Value;
            }
        }

        var session = await _sessionStore.SetAsync(
            scope,
            new ProviderSessionMaterial(
                credentialEntity.Id,
                tokenReference.Value,
                RefreshTokenReference: null,
                TokenFingerprint: Fingerprint(token),
                AccessExpiresAtUtc: expiresAtUtc,
                RefreshExpiresAtUtc: null,
                Credential: credentialEntity),
            credentialEntity.KeyVersion,
            ct);
        if (!session.IsSuccess)
            return Result.Failure<DidoxTokenResultDto>(DidoxCredentialRequired);

        try
        {
            // Credential and session are staged in the same unit-of-work boundary.
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<DidoxTokenResultDto>(DidoxCredentialConflict);
        }
        catch (DbUpdateException)
        {
            return Result.Failure<DidoxTokenResultDto>(DidoxCredentialConflict);
        }

        // A Didox company token is never returned to the caller; the client only learns that the
        // connection succeeded and must use subsequent server-side operations.
        return Result.Success(new DidoxTokenResultDto
        {
            IsSuccessful = true,
            Token = null,
            Message = "Didox connection established."
        });
    }

    private async Task<Result<DidoxTokenLease>> LoadDidoxTokenAsync(
        OrganizationScope scope,
        CancellationToken ct)
    {
        var session = await _sessionStore.GetActiveAsync(scope, ct);
        if (!session.IsSuccess || session.Value is null)
            return Result.Failure<DidoxTokenLease>(DidoxCredentialRequired);

        var reference = EncryptedSecretReference.Create(session.Value.EncryptedAccessTokenReference);
        if (!reference.IsSuccess)
            return Result.Failure<DidoxTokenLease>(DidoxCredentialRequired);

        var unprotected = _secretProtector.Unprotect(scope, reference.Value);
        if (!unprotected.IsSuccess)
            return Result.Failure<DidoxTokenLease>(DidoxCredentialRequired);

        using (unprotected.Value)
            return Result.Success(new DidoxTokenLease(Encoding.UTF8.GetString(unprotected.Value.Value)));
    }

    private async Task RevokeDidoxOnUnauthorizedAsync(
        OrganizationScope scope,
        string token,
        string? statusCode,
        CancellationToken ct)
    {
        if (statusCode is not ("401" or "Unauthorized"))
            return;

        var removed = await _sessionStore.RemoveIfMatchesAsync(scope, Fingerprint(token), ct);
        if (removed.IsSuccess && removed.Value)
            await _unitOfWork.SaveChangesAsync(ct);
    }

    private static Error SourceMissing(string source) => Error.Business("Didox.SourceMissing", $"Required Didox source is missing: {source}.");
    private static Error OrganizationMismatch(string source) => Error.Forbidden("Didox.OrganizationMismatch", $"{source} does not belong to the sale document organization.");

    private static readonly Error DidoxCredentialRequired = Error.Unauthorized(
        "Didox.CredentialRequired", "Connect Didox for the current organization first.");
    private static readonly Error DidoxAuthFailed = Error.Problem(
        "Didox.AuthFailed", "Didox authentication failed.");
    private static readonly Error DidoxCredentialConflict = Error.Conflict(
        "Didox.CredentialConcurrencyConflict", "Didox credential changed concurrently. Please try again.");

    private static string Fingerprint(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private sealed class DidoxTokenLease(string token) : IDisposable
    {
        public string Token { get; private set; } = token;
        public void Dispose() => Token = string.Empty;
    }

    private static TaxDocumentResultDto ToDocumentResult(TaxProviderOperationResultDto response) => new()
    {
        ProviderCode = response.ProviderCode,
        Operation = response.Operation,
        ExternalDocumentId = response.ExternalDocumentId,
        StatusCode = response.StatusCode,
        StatusName = response.StatusName,
        IsSuccessful = response.IsSuccessful,
        Message = response.Message,
        RequestedAt = response.RequestedAt
    };

    private static TaxDocumentResultDto ToDidoxDocumentResult(TaxProviderOperationResultDto response) => new()
    {
        ProviderCode = response.ProviderCode,
        Operation = response.Operation,
        ExternalDocumentId = response.ExternalDocumentId,
        StatusCode = response.StatusCode,
        StatusName = response.StatusName,
        IsSuccessful = response.IsSuccessful,
        Message = response.IsSuccessful ? "Didox operation completed." : "Didox operation failed.",
        RequestedAt = response.RequestedAt
    };
}
