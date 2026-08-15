using Application.Abstractions;
using Application.Abstractions.Integration.Edo;
using Integration.Edocs.Facturas;
using Integration.Edocs.Http;
using SharedKernel.Constants;

namespace Integration.Edocs.Historical;

public sealed class EdocsHistoricalDocumentSource(
    EdocsEdoOperations operations,
    EdocsTokenCache tokenCache,
    IOrganizationSourceReader organizationSourceReader) : IEdoHistoricalDocumentSource
{
    private const string HistoricalListOperation = "EDOCS_HISTORICAL_LIST";
    private const string HistoricalDetailOperation = "EDOCS_HISTORICAL_DETAIL";

    public EdoProviderCode ProviderCode => EdoProviderCode.EDOCS;

    public async ValueTask<EdoHistoricalSourceReadinessDto> CheckReadinessAsync(
        EdoHistoricalExecutionContextDto context,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (context.OrganizationId <= 0)
            return NotConfigured();

        var organizationInn = await organizationSourceReader.GetOrganizationInnAsync(context.OrganizationId, ct);
        if (string.IsNullOrWhiteSpace(organizationInn))
            return NotConfigured();

        var isSessionReady = tokenCache.TryGet(
            context.OrganizationId,
            IntegrationProviderConst.Edocs,
            out _);
        return new EdoHistoricalSourceReadinessDto
        {
            ProviderCode = ProviderCode,
            IsProviderRegistered = true,
            IsSessionReady = isSessionReady,
            State = isSessionReady
                ? EdoHistoricalReadState.COMPLETE
                : EdoHistoricalReadState.WAITING_AUTH,
            SafeFailureCode = isSessionReady
                ? null
                : Edo.Historical.EdoHistoricalSourceSupport.AuthRequired
        };
    }

    public async Task<EdoHistoricalPageResultDto> ReadInboxPageAsync(
        EdoHistoricalExecutionContextDto context,
        EdoHistoricalPageRequestDto request,
        CancellationToken ct = default)
    {
        var readiness = await CheckReadinessAsync(context, ct);
        if (!readiness.IsSessionReady)
            return UnavailablePage(request, readiness);

        try
        {
            return await operations.ListHistoricalSignedInboxAsync(context.OrganizationId, request, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Edo.Historical.EdoHistoricalSourceSupport.BuildFailurePage(
                ProviderCode,
                request,
                exception,
                HistoricalListOperation);
        }
    }

    public async Task<EdoHistoricalDetailResultDto> ReadDetailAsync(
        EdoHistoricalExecutionContextDto context,
        EdoHistoricalDetailRequestDto request,
        CancellationToken ct = default)
    {
        var readiness = await CheckReadinessAsync(context, ct);
        if (!readiness.IsSessionReady)
            return UnavailableDetail(readiness);

        try
        {
            var organizationInn = await organizationSourceReader.GetOrganizationInnAsync(context.OrganizationId, ct);
            if (string.IsNullOrWhiteSpace(organizationInn))
                return UnavailableDetail(NotConfigured());

            var document = await operations.GetHistoricalDocumentDetailsAsync(
                context.OrganizationId,
                request.Item.DocumentType,
                request.Item.ProviderDocumentId,
                ct);
            return Edo.Historical.EdoHistoricalSourceSupport.ValidateAndMapDetail(
                ProviderCode,
                organizationInn,
                request,
                document);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Edo.Historical.EdoHistoricalSourceSupport.BuildFailureDetail(
                ProviderCode,
                exception,
                HistoricalDetailOperation);
        }
    }

    private EdoHistoricalSourceReadinessDto NotConfigured() => new()
    {
        ProviderCode = ProviderCode,
        IsProviderRegistered = true,
        IsSessionReady = false,
        State = EdoHistoricalReadState.TERMINAL_PROVIDER_FAILURE,
        SafeFailureCode = Edo.Historical.EdoHistoricalSourceSupport.OrganizationNotConfigured
    };

    private EdoHistoricalPageResultDto UnavailablePage(
        EdoHistoricalPageRequestDto request,
        EdoHistoricalSourceReadinessDto readiness) => new()
    {
        ProviderCode = ProviderCode,
        Page = request.Page,
        PageSize = request.PageSize,
        IsCompletenessConfirmed = false,
        RequiresOverlapRescan = true,
        State = readiness.State,
        SafeFailureCode = readiness.SafeFailureCode
    };

    private EdoHistoricalDetailResultDto UnavailableDetail(EdoHistoricalSourceReadinessDto readiness) => new()
    {
        ProviderCode = ProviderCode,
        State = readiness.State,
        IsImportReady = false,
        SafeFailureCode = readiness.SafeFailureCode
    };
}
