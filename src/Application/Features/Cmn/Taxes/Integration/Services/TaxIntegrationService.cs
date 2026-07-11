using Application.Abstractions.Integration;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes.Integration.Services;

public sealed class TaxIntegrationService : ITaxIntegrationService
{
    private readonly ITaxProviderFactory _factory;
    private readonly IDidoxAuthClient _didoxAuthClient;
    private readonly IDidoxDocumentClient _didoxDocumentClient;

    public TaxIntegrationService(
        ITaxProviderFactory factory,
        IDidoxAuthClient didoxAuthClient,
        IDidoxDocumentClient didoxDocumentClient)
    {
        _factory = factory;
        _didoxAuthClient = didoxAuthClient;
        _didoxDocumentClient = didoxDocumentClient;
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
        => await ExecuteDocumentAsync("DIDOX", "submit", request, ct);

    public async Task<Result<TaxDocumentResultDto>> GetDidoxStatusAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
        => await ExecuteDocumentAsync("DIDOX", "status", request, ct);

    public async Task<Result<TaxDocumentResultDto>> CancelDidoxAsync(TaxDocumentRequestDto request, CancellationToken ct = default)
        => await ExecuteDocumentAsync("DIDOX", "cancel", request, ct);

    public async Task<Result<DidoxTokenResultDto>> GetDidoxTokenBySignatureAsync(DidoxAuthSignatureRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.TaxId))
            return Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.TaxIdRequired", "Tax id is required."));

        if (string.IsNullOrWhiteSpace(request.Signature))
            return Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.SignatureRequired", "E-IMZO signature is required."));

        var result = await _didoxAuthClient.GetTokenBySignatureAsync(request.TaxId, request.Signature, request.Locale, ct);
        return result.IsSuccessful
            ? Result.Success(result)
            : Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.AuthFailed", result.Message ?? "Didox authentication failed."));
    }

    public async Task<Result<DidoxTokenResultDto>> GetDidoxTokenByPasswordAsync(DidoxAuthPasswordRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.TaxId))
            return Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.TaxIdRequired", "Tax id is required."));

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.PasswordRequired", "Password is required."));

        var result = await _didoxAuthClient.GetTokenByPasswordAsync(request.TaxId, request.Password, request.Locale, ct);
        return result.IsSuccessful
            ? Result.Success(result)
            : Result.Failure<DidoxTokenResultDto>(Error.Problem("Didox.AuthFailed", result.Message ?? "Didox authentication failed."));
    }

    public async Task<Result<TaxDocumentResultDto>> SignDidoxAsync(DidoxSignRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentId))
            return Result.Failure<TaxDocumentResultDto>(Error.Problem("Didox.DocumentIdRequired", "Document id is required."));

        if (string.IsNullOrWhiteSpace(request.Signature))
            return Result.Failure<TaxDocumentResultDto>(Error.Problem("Didox.SignatureRequired", "Signature is required (produced by the frontend E-IMZO flow)."));

        var response = await _didoxDocumentClient.SignAsync(request.DocumentId, request.Signature, request.CompanyToken, ct);
        return Result.Success(ToDocumentResult(response));
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

        var model = new TaxProviderOperationRequestDto
        {
            ProviderCode = providerCode,
            OrganizationId = request.OrganizationId,
            DocumentNumber = request.DocumentNumber,
            Payload = request.Payload,
            ExternalDocumentId = request.ExternalDocumentId,
            CompanyToken = request.CompanyToken
        };

        var response = operation switch
        {
            "submit" => await documentProvider.SubmitAsync(model, ct),
            "status" => await documentProvider.GetDocumentStatusAsync(model, ct),
            "cancel" => await documentProvider.CancelAsync(model, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        return Result.Success(ToDocumentResult(response));
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
}
