using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Application.Features.Integration.Edo;

public sealed class EdoProviderManagementService(
    IUserContext userContext,
    IEdoProviderRegistry providerRegistry,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoProviderConfiguration providerConfiguration) : IEdoProviderManagementService
{
    public async Task<EdoProviderDto> GetActiveProviderAsync(
        CancellationToken ct = default)
    {
        var organizationId = GetCurrentOrganizationId();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        var isConfigured = await IsConfiguredAsync(organizationId, provider.Code, ct);

        return Map(provider.Capabilities, isActive: true, isConfigured);
    }

    public async Task<EdoProviderDto> SetActiveProviderAsync(
        EdoActiveProviderRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = GetCurrentOrganizationId();
        var provider = providerRegistry.Resolve(request.ProviderCode);
        var isConfigured = await IsConfiguredAsync(
            organizationId,
            request.ProviderCode,
            ct);

        if (!isConfigured)
            throw new EdoCredentialNotConfiguredException(request.ProviderCode.ToString());

        await activeProviderResolver.SetActiveProviderAsync(request.ProviderCode, ct);

        return Map(provider.Capabilities, isActive: true, isConfigured: true);
    }

    public async Task<EdoCapabilitiesResponseDto> GetCapabilitiesAsync(
        CancellationToken ct = default)
    {
        var provider = await GetActiveProviderAsync(ct);
        return new EdoCapabilitiesResponseDto
        {
            Provider = provider.ProviderCode,
            DisplayName = provider.DisplayName,
            AuthModes = provider.AuthModes,
            SigningModes = provider.SigningModes,
            RawCapabilities = provider.Capabilities,
            Capabilities = MapFrontendCapabilities(provider.Capabilities),
            CategoryCapabilities = MapCategoryCapabilities(provider.ProviderCode, provider.Capabilities),
            StatusCapabilities = MapStatusCapabilities(provider.ProviderCode, provider.Capabilities),
            FilterCapabilities = MapFilterCapabilities(provider.ProviderCode, provider.Capabilities),
            StatusOptions = MapStatusOptions(provider.ProviderCode, provider.Capabilities)
        };
    }

    private async Task<bool> IsConfiguredAsync(
        int organizationId,
        EdoProviderCode providerCode,
        CancellationToken ct) =>
        await providerConfiguration.IsConfiguredAsync(providerCode, organizationId, ct);

    private int GetCurrentOrganizationId() =>
        userContext.OrganizationId
            ?? throw new EdoOrganizationScopeRequiredException();

    private static EdoProviderDto Map(
        EdoProviderCapabilityDto capability,
        bool isActive,
        bool isConfigured) =>
        new()
        {
            ProviderCode = capability.ProviderCode,
            DisplayName = capability.DisplayName,
            IsActive = isActive,
            IsConfigured = isConfigured,
            IsLocalConfigurationOnly = true,
            AuthModes = capability.AuthModes,
            SigningModes = capability.SigningModes,
            Capabilities = capability.Capabilities
        };

    private static EdoFrontendCapabilitiesDto MapFrontendCapabilities(
        IReadOnlyCollection<EdoCapabilityDto> capabilities)
    {
        var lookup = capabilities.ToDictionary(item => item.Kind, item => item.Status);
        var unknown = EdoCapabilityStatus.UNKNOWN;
        EdoCapabilityStatus Get(EdoCapabilityKind kind) =>
            lookup.TryGetValue(kind, out var status) ? status : unknown;
        var canAggregateAll = Get(EdoCapabilityKind.ListInbox) == EdoCapabilityStatus.SUPPORTED
            && Get(EdoCapabilityKind.ListOutbox) == EdoCapabilityStatus.SUPPORTED
            ? EdoCapabilityStatus.SUPPORTED
            : Get(EdoCapabilityKind.ListInbox) is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL
                || Get(EdoCapabilityKind.ListOutbox) is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL
                    ? EdoCapabilityStatus.PARTIAL
                    : Get(EdoCapabilityKind.ListInbox) == EdoCapabilityStatus.NOT_SUPPORTED
                        && Get(EdoCapabilityKind.ListOutbox) == EdoCapabilityStatus.NOT_SUPPORTED
                            ? EdoCapabilityStatus.NOT_SUPPORTED
                            : EdoCapabilityStatus.UNKNOWN;
        EdoCapabilityStatus GetAny(EdoCapabilityKind first, EdoCapabilityKind second)
        {
            var firstStatus = Get(first);
            var secondStatus = Get(second);
            if (firstStatus == EdoCapabilityStatus.SUPPORTED
                && secondStatus == EdoCapabilityStatus.SUPPORTED)
                return EdoCapabilityStatus.SUPPORTED;
            if (firstStatus is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL
                || secondStatus is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL)
                return EdoCapabilityStatus.PARTIAL;
            if (firstStatus == EdoCapabilityStatus.NOT_SUPPORTED
                && secondStatus == EdoCapabilityStatus.NOT_SUPPORTED)
                return EdoCapabilityStatus.NOT_SUPPORTED;
            return EdoCapabilityStatus.UNKNOWN;
        }

        return new EdoFrontendCapabilitiesDto
        {
            CanListInbox = Get(EdoCapabilityKind.ListInbox),
            CanListOutbox = Get(EdoCapabilityKind.ListOutbox),
            CanListDrafts = Get(EdoCapabilityKind.ListDrafts),
            CanListAll = Get(EdoCapabilityKind.ListAll),
            CanAggregateAll = canAggregateAll,
            CanGetDetail = Get(EdoCapabilityKind.GetDetail),
            CanGetFile = Get(EdoCapabilityKind.GetFile),
            CanGetStatus = GetAny(EdoCapabilityKind.GetInboxStatus, EdoCapabilityKind.GetOutboxStatus),
            CanCreate = Get(EdoCapabilityKind.CreateFactura),
            CanSign = Get(EdoCapabilityKind.SignOutbox),
            CanReject = Get(EdoCapabilityKind.RejectInbox),
            CanDelete = Get(EdoCapabilityKind.Delete),
            CanRestore = Get(EdoCapabilityKind.Restore),
            CanExport = Get(EdoCapabilityKind.Export),
            CanMarking = Get(EdoCapabilityKind.Marking)
        };
    }

    private static IReadOnlyCollection<EdoCategoryCapabilityDto> MapCategoryCapabilities(
        EdoProviderCode providerCode,
        IReadOnlyCollection<EdoCapabilityDto> capabilities)
    {
        var lookup = capabilities.ToDictionary(item => item.Kind, item => item.Status);
        EdoCapabilityStatus Get(EdoCapabilityKind kind) =>
            lookup.TryGetValue(kind, out var status) ? status : EdoCapabilityStatus.UNKNOWN;

        var statusCategory = providerCode is EdoProviderCode.EDOCS or EdoProviderCode.DIDOX
            ? EdoCapabilityStatus.SUPPORTED
            : EdoCapabilityStatus.UNKNOWN;

        return
        [
            new() { Category = EdoDocumentCategory.INBOX, Capability = Get(EdoCapabilityKind.ListInbox) },
            new() { Category = EdoDocumentCategory.OUTBOX, Capability = Get(EdoCapabilityKind.ListOutbox) },
            new() { Category = EdoDocumentCategory.DRAFTS, Capability = Get(EdoCapabilityKind.ListDrafts) },
            new() { Category = EdoDocumentCategory.REJECTED, Capability = statusCategory },
            new() { Category = EdoDocumentCategory.DELETED_ARCHIVED, Capability = statusCategory },
            new()
            {
                Category = EdoDocumentCategory.ALL,
                Capability = GetAggregateAllCapability(lookup)
            }
        ];
    }

    private static IReadOnlyCollection<EdoFilterCapabilityDto> MapFilterCapabilities(
        EdoProviderCode providerCode,
        IReadOnlyCollection<EdoCapabilityDto> capabilities)
    {
        var lookup = capabilities.ToDictionary(item => item.Kind, item => item.Status);
        var result = new List<EdoFilterCapabilityDto>();
        var inboxSupported = IsSupported(lookup, EdoCapabilityKind.ListInbox);
        var outboxSupported = IsSupported(lookup, EdoCapabilityKind.ListOutbox);
        var aggregateSupported = inboxSupported && outboxSupported;
        var statusFilterSupported = providerCode is EdoProviderCode.EDOCS or EdoProviderCode.DIDOX;

        AddListFilters(result, EdoDirection.INBOX, EdoDocumentCategory.INBOX, inboxSupported, statusFilterSupported);
        AddListFilters(result, EdoDirection.OUTBOX, EdoDocumentCategory.OUTBOX, outboxSupported, statusFilterSupported);
        AddListFilters(
            result,
            null,
            EdoDocumentCategory.ALL,
            aggregateSupported,
            statusFilterSupported && providerCode == EdoProviderCode.DIDOX);
        if (providerCode == EdoProviderCode.DIDOX)
        {
            AddFilter(result, null, EdoDocumentCategory.DRAFTS, "Status", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.REJECTED, "Status", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DELETED_ARCHIVED, "Status", aggregateSupported);
        }

        if (providerCode == EdoProviderCode.DIDOX)
        {
            AddFilter(result, EdoDirection.INBOX, EdoDocumentCategory.INBOX, "DateFrom", inboxSupported);
            AddFilter(result, EdoDirection.INBOX, EdoDocumentCategory.INBOX, "DateTo", inboxSupported);
            AddFilter(result, EdoDirection.OUTBOX, EdoDocumentCategory.OUTBOX, "DateFrom", outboxSupported);
            AddFilter(result, EdoDirection.OUTBOX, EdoDocumentCategory.OUTBOX, "DateTo", outboxSupported);
            AddFilter(result, null, EdoDocumentCategory.ALL, "DateFrom", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.ALL, "DateTo", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DRAFTS, "DateFrom", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DRAFTS, "DateTo", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.REJECTED, "DateFrom", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.REJECTED, "DateTo", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DELETED_ARCHIVED, "DateFrom", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DELETED_ARCHIVED, "DateTo", aggregateSupported);
            AddFilter(result, EdoDirection.INBOX, EdoDocumentCategory.INBOX, "HasMarks", inboxSupported);
            AddFilter(result, EdoDirection.OUTBOX, EdoDocumentCategory.OUTBOX, "HasMarks", outboxSupported);
            AddFilter(result, null, EdoDocumentCategory.ALL, "HasMarks", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DRAFTS, "HasMarks", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.REJECTED, "HasMarks", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DELETED_ARCHIVED, "HasMarks", aggregateSupported);
            AddFilter(result, EdoDirection.INBOX, EdoDocumentCategory.INBOX, "Search", inboxSupported);
            AddFilter(result, EdoDirection.OUTBOX, EdoDocumentCategory.OUTBOX, "Search", outboxSupported);
            AddFilter(result, null, EdoDocumentCategory.ALL, "Search", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DRAFTS, "Search", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.REJECTED, "Search", aggregateSupported);
            AddFilter(result, null, EdoDocumentCategory.DELETED_ARCHIVED, "Search", aggregateSupported);
        }

        return result;
    }

    private static void AddListFilters(
        ICollection<EdoFilterCapabilityDto> result,
        EdoDirection? direction,
        EdoDocumentCategory category,
        bool supported,
        bool statusSupported)
    {
        AddFilter(result, direction, category, "Page", supported);
        AddFilter(result, direction, category, "PageSize", supported);
        AddFilter(result, direction, category, "Status", supported && statusSupported);
    }

    private static void AddFilter(
        ICollection<EdoFilterCapabilityDto> result,
        EdoDirection? direction,
        EdoDocumentCategory category,
        string filter,
        bool supported)
    {
        if (!supported)
            return;

        result.Add(new EdoFilterCapabilityDto
        {
            Direction = direction,
            Category = category,
            Filter = filter,
            Capability = EdoCapabilityStatus.SUPPORTED
        });
    }

    private static IReadOnlyCollection<EdoStatusOptionCapabilityDto> MapStatusOptions(
        EdoProviderCode providerCode,
        IReadOnlyCollection<EdoCapabilityDto> capabilities)
    {
        var lookup = capabilities.ToDictionary(item => item.Kind, item => item.Status);
        var result = new List<EdoStatusOptionCapabilityDto>();
        var inboxSupported = IsSupported(lookup, EdoCapabilityKind.ListInbox);
        var outboxSupported = IsSupported(lookup, EdoCapabilityKind.ListOutbox);
        var draftsSupported = IsSupported(lookup, EdoCapabilityKind.ListDrafts);

        if (providerCode == EdoProviderCode.EDOCS)
        {
            AddStatusOptions(result, EdoDirection.INBOX, EdoDocumentCategory.INBOX, inboxSupported,
                (nameof(EdoDocumentStatusCode.SIGNED), true));
            AddStatusOptions(result, EdoDirection.OUTBOX, EdoDocumentCategory.OUTBOX, outboxSupported,
                (nameof(EdoDocumentStatusCode.DELETED), true));
        }
        else if (providerCode == EdoProviderCode.DIDOX)
        {
            AddStatusOptions(result, EdoDirection.INBOX, EdoDocumentCategory.INBOX, inboxSupported,
                ("ALL", false),
                (nameof(EdoDocumentStatusCode.PENDING_SIGNATURE), true),
                (nameof(EdoDocumentStatusCode.SIGNED), true),
                (nameof(EdoDocumentStatusCode.REJECTED), true),
                (nameof(EdoDocumentStatusCode.DELETED), true));
            AddStatusOptions(result, EdoDirection.OUTBOX, EdoDocumentCategory.OUTBOX, outboxSupported,
                (nameof(EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING), true),
                (nameof(EdoDocumentStatusCode.AGENT_SIGNATURE_PENDING), true),
                (nameof(EdoDocumentStatusCode.SIGNED), true),
                (nameof(EdoDocumentStatusCode.REJECTED), true),
                (nameof(EdoDocumentStatusCode.DELETED), true));
            AddStatusOptions(result, null, EdoDocumentCategory.DRAFTS, draftsSupported,
                (nameof(EdoDocumentStatusCode.DRAFT), true),
                (nameof(EdoDocumentStatusCode.DELETED), true));
        }

        return result;
    }

    private static void AddStatusOptions(
        ICollection<EdoStatusOptionCapabilityDto> result,
        EdoDirection? direction,
        EdoDocumentCategory category,
        bool supported,
        params (string Code, bool SendsProviderStatus)[] options)
    {
        if (!supported)
            return;

        foreach (var option in options)
        {
            result.Add(new EdoStatusOptionCapabilityDto
            {
                Direction = direction,
                Category = category,
                Code = option.Code,
                Capability = EdoCapabilityStatus.SUPPORTED,
                SendsProviderStatus = option.SendsProviderStatus
            });
        }
    }

    private static bool IsSupported(
        IReadOnlyDictionary<EdoCapabilityKind, EdoCapabilityStatus> lookup,
        EdoCapabilityKind kind) =>
        lookup.GetValueOrDefault(kind, EdoCapabilityStatus.UNKNOWN) == EdoCapabilityStatus.SUPPORTED;

    private static EdoCapabilityStatus GetAggregateAllCapability(
        IReadOnlyDictionary<EdoCapabilityKind, EdoCapabilityStatus> lookup)
    {
        var inbox = lookup.GetValueOrDefault(EdoCapabilityKind.ListInbox, EdoCapabilityStatus.UNKNOWN);
        var outbox = lookup.GetValueOrDefault(EdoCapabilityKind.ListOutbox, EdoCapabilityStatus.UNKNOWN);
        if (inbox == EdoCapabilityStatus.SUPPORTED && outbox == EdoCapabilityStatus.SUPPORTED)
            return EdoCapabilityStatus.SUPPORTED;
        if (inbox is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL
            || outbox is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL)
            return EdoCapabilityStatus.PARTIAL;
        if (inbox == EdoCapabilityStatus.NOT_SUPPORTED && outbox == EdoCapabilityStatus.NOT_SUPPORTED)
            return EdoCapabilityStatus.NOT_SUPPORTED;
        return EdoCapabilityStatus.UNKNOWN;
    }

    private static IReadOnlyCollection<EdoStatusCapabilityDto> MapStatusCapabilities(
        EdoProviderCode providerCode,
        IReadOnlyCollection<EdoCapabilityDto> capabilities)
    {
        if (providerCode == EdoProviderCode.EDOCS)
        {
            var edocsLookup = capabilities.ToDictionary(item => item.Kind, item => item.Status);
            var edocsResult = new List<EdoStatusCapabilityDto>();
            if (IsSupported(edocsLookup, EdoCapabilityKind.ListInbox))
                edocsResult.Add(new() { Status = EdoDocumentStatusCode.SIGNED, Capability = EdoCapabilityStatus.SUPPORTED });
            if (IsSupported(edocsLookup, EdoCapabilityKind.ListOutbox))
                edocsResult.Add(new() { Status = EdoDocumentStatusCode.DELETED, Capability = EdoCapabilityStatus.SUPPORTED });
            return edocsResult;
        }

        var lookup = capabilities.ToDictionary(item => item.Kind, item => item.Status);
        EdoCapabilityStatus Get(EdoCapabilityKind kind) =>
            lookup.TryGetValue(kind, out var status) ? status : EdoCapabilityStatus.UNKNOWN;

        var statusEndpoint = GetAny(lookup, EdoCapabilityKind.GetInboxStatus, EdoCapabilityKind.GetOutboxStatus);
        var filterStatus = Get(EdoCapabilityKind.SearchFilter);
        var result = Enum.GetValues<EdoDocumentStatusCode>()
            .Select(status => new EdoStatusCapabilityDto
            {
                Status = status,
                Capability = status switch
                {
                    EdoDocumentStatusCode.UNKNOWN => EdoCapabilityStatus.SUPPORTED,
                    EdoDocumentStatusCode.PENDING_SIGNATURE
                        or EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING
                        or EdoDocumentStatusCode.AGENT_SIGNATURE_PENDING
                        or EdoDocumentStatusCode.ARCHIVED
                        when providerCode == EdoProviderCode.DIDOX => EdoCapabilityStatus.SUPPORTED,
                    EdoDocumentStatusCode.DELETED
                        when providerCode is EdoProviderCode.EDOCS or EdoProviderCode.DIDOX
                            => EdoCapabilityStatus.SUPPORTED,
                    EdoDocumentStatusCode.DRAFT => Get(EdoCapabilityKind.ListDrafts),
                    EdoDocumentStatusCode.REJECTED
                        or EdoDocumentStatusCode.CANCELLED => filterStatus,
                    EdoDocumentStatusCode.SENT
                        or EdoDocumentStatusCode.SIGNED
                        or EdoDocumentStatusCode.RECEIVED
                        or EdoDocumentStatusCode.PENDING
                        or EdoDocumentStatusCode.COMPLETED
                        or EdoDocumentStatusCode.FAILED
                        or EdoDocumentStatusCode.RECONCILIATION_REQUIRED => statusEndpoint,
                    _ => EdoCapabilityStatus.UNKNOWN
                }
            })
            .Where(item => item.Status != EdoDocumentStatusCode.UNKNOWN
                && item.Capability == EdoCapabilityStatus.SUPPORTED)
            .ToArray();

        return result;
    }

    private static EdoCapabilityStatus GetAny(
        IReadOnlyDictionary<EdoCapabilityKind, EdoCapabilityStatus> lookup,
        EdoCapabilityKind first,
        EdoCapabilityKind second)
    {
        var firstStatus = lookup.GetValueOrDefault(first, EdoCapabilityStatus.UNKNOWN);
        var secondStatus = lookup.GetValueOrDefault(second, EdoCapabilityStatus.UNKNOWN);
        if (firstStatus == EdoCapabilityStatus.SUPPORTED
            && secondStatus == EdoCapabilityStatus.SUPPORTED)
            return EdoCapabilityStatus.SUPPORTED;
        if (firstStatus is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL
            || secondStatus is EdoCapabilityStatus.SUPPORTED or EdoCapabilityStatus.PARTIAL)
            return EdoCapabilityStatus.PARTIAL;
        if (firstStatus == EdoCapabilityStatus.NOT_SUPPORTED
            && secondStatus == EdoCapabilityStatus.NOT_SUPPORTED)
            return EdoCapabilityStatus.NOT_SUPPORTED;
        return EdoCapabilityStatus.UNKNOWN;
    }
}
