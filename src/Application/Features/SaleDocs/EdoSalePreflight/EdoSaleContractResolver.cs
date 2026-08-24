using Application.Abstractions.Integration.Edo;
using Domain.Entities;

namespace Application.Features.SaleDocs.EdoSalePreflight;

internal static class EdoSaleContractResolver
{
    private const string ProviderCode = "EDOCS";

    internal static EdoSaleMappingStatusDto Resolve(
        int organizationId,
        EdoOutboxProviderDocumentDetailDto detail,
        long? counterpartyId,
        IReadOnlyCollection<Contract> contracts,
        ISet<string> codes)
    {
        var providerNumber = Normalize(detail.ContractNumber);
        var providerDate = detail.ContractDate;
        var scopedCandidates = counterpartyId.HasValue
            ? contracts
                .Where(x => x.OrganizationId == organizationId
                    && x.CounterpartyId == counterpartyId.Value
                    && IsWithinDocumentDate(x, detail.DocumentDate))
                .Select(x => x.Id)
                .OrderBy(x => x)
                .ToArray()
            : [];

        EdoSaleMappingStatusDto Build(
            string status,
            string? code,
            string description,
            IReadOnlyCollection<long> candidateIds,
            long? selectedId = null) => new()
            {
                Status = status,
                SafeErrorCode = code,
                SelectedId = selectedId,
                ProviderCode = ProviderCode,
                ProviderContractNumber = providerNumber,
                ProviderContractDate = providerDate,
                CandidateIds = candidateIds,
                Description = description
            };

        if (!counterpartyId.HasValue)
        {
            codes.Add("CONTRACT_COUNTERPARTY_REQUIRED");
            return Build(
                "REQUIRES_SELECTION",
                "CONTRACT_COUNTERPARTY_REQUIRED",
                "Contract mapping requires a selected counterparty.",
                []);
        }

        if (string.IsNullOrWhiteSpace(providerNumber) || !providerDate.HasValue)
        {
            codes.Add("CONTRACT_PROVIDER_DATA_REQUIRED");
            return Build(
                "REQUIRES_SELECTION",
                "CONTRACT_PROVIDER_DATA_REQUIRED",
                "Provider contract number and date are unavailable; select a scoped local contract explicitly.",
                scopedCandidates);
        }

        var exactMatches = contracts
            .Where(x => x.OrganizationId == organizationId
                && x.CounterpartyId == counterpartyId.Value
                && string.Equals(x.ProviderCode, ProviderCode, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Normalize(x.ProviderContractNumber), providerNumber, StringComparison.Ordinal)
                && x.ProviderContractDate == providerDate.Value
                && IsWithinDocumentDate(x, detail.DocumentDate))
            .Select(x => x.Id)
            .OrderBy(x => x)
            .ToArray();

        if (exactMatches.Length == 1)
        {
            return Build(
                "READY",
                null,
                "One exact provider contract match exists within the organization, counterparty and date scope.",
                exactMatches,
                exactMatches[0]);
        }

        if (exactMatches.Length > 1)
        {
            codes.Add("CONTRACT_MAPPING_AMBIGUOUS");
            return Build(
                "REQUIRES_SELECTION",
                "CONTRACT_MAPPING_AMBIGUOUS",
                "More than one active contract matches the provider contract identity.",
                exactMatches);
        }

        codes.Add("CONTRACT_MAPPING_REQUIRED");
        return Build(
            "REQUIRES_SELECTION",
            "CONTRACT_MAPPING_REQUIRED",
            "No scoped active contract matches the provider contract identity.",
            scopedCandidates);
    }

    private static bool IsWithinDocumentDate(Contract contract, DateOnly documentDate) =>
        (contract.StartDate is null || DateOnly.FromDateTime(contract.StartDate.Value) <= documentDate)
        && (contract.EndDate is null || DateOnly.FromDateTime(contract.EndDate.Value) >= documentDate);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
