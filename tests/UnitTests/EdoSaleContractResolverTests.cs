using Application.Abstractions.Integration.Edo;
using Application.Features.SaleDocs.EdoSalePreflight;
using Domain.Entities;

public sealed class EdoSaleContractResolverTests
{
    [Fact]
    public void ExactProviderIdentityIsShownButNotAutomaticallySelected()
    {
        var contract = Contract(11, "EDOCS", "P-42", new DateOnly(2026, 2, 1));
        var codes = new HashSet<string>(StringComparer.Ordinal);

        var result = EdoSaleContractResolver.Resolve(
            2,
            Detail("P-42", new DateOnly(2026, 2, 1)),
            5,
            [contract],
            codes);

        Assert.Equal("REQUIRES_SELECTION", result.Status);
        Assert.Equal("CONTRACT_SELECTION_REQUIRED", result.SafeErrorCode);
        Assert.Null(result.SelectedId);
        Assert.Equal([contract.Id], result.CandidateIds);
        Assert.Contains("CONTRACT_SELECTION_REQUIRED", codes);
    }

    [Fact]
    public void ProviderIdentityMismatchDoesNotUseLocalContractNumber()
    {
        var contract = Contract(11, "EDOCS", "LOCAL-ONLY", new DateOnly(2026, 2, 1));
        var codes = new HashSet<string>(StringComparer.Ordinal);

        var result = EdoSaleContractResolver.Resolve(
            2,
            Detail("P-42", new DateOnly(2026, 2, 1)),
            5,
            [contract],
            codes);

        Assert.Equal("CONTRACT_MAPPING_REQUIRED", result.SafeErrorCode);
        Assert.Null(result.SelectedId);
        Assert.Equal([contract.Id], result.CandidateIds);
    }

    [Fact]
    public void MissingProviderIdentityReturnsSafeDataRequiredWithScopedCandidates()
    {
        var contract = Contract(11, null, null, new DateOnly(2026, 2, 1));
        var codes = new HashSet<string>(StringComparer.Ordinal);

        var result = EdoSaleContractResolver.Resolve(
            2,
            Detail(null, null),
            5,
            [contract],
            codes);

        Assert.Equal("CONTRACT_PROVIDER_DATA_REQUIRED", result.SafeErrorCode);
        Assert.Equal([contract.Id], result.CandidateIds);
        Assert.Null(result.SelectedId);
    }

    [Fact]
    public void OrganizationCounterpartyAndDateScopeAreRequiredForCandidates()
    {
        var matching = Contract(11, null, null, new DateOnly(2026, 2, 1));
        var wrongCounterparty = Contract(12, null, null, new DateOnly(2026, 2, 1));
        wrongCounterparty.CounterpartyId = 6;
        var outOfRange = Contract(11, null, null, new DateOnly(2024, 2, 1));
        outOfRange.EndDate = new DateTime(2025, 12, 31);
        var codes = new HashSet<string>(StringComparer.Ordinal);

        var result = EdoSaleContractResolver.Resolve(
            2,
            Detail(null, null),
            5,
            [matching, wrongCounterparty, outOfRange],
            codes);

        Assert.Equal([matching.Id], result.CandidateIds);
    }

    [Fact]
    public void CrossOrganizationContractIsNotAReconciliationCandidate()
    {
        var otherOrganization = Contract(11, null, null, null);
        otherOrganization.OrganizationId = 3;
        var codes = new HashSet<string>(StringComparer.Ordinal);

        var result = EdoSaleContractResolver.Resolve(
            2,
            Detail(null, null),
            5,
            [otherOrganization],
            codes);

        Assert.Empty(result.CandidateIds);
        Assert.Equal("CONTRACT_PROVIDER_DATA_REQUIRED", result.SafeErrorCode);
    }

    private static EdoOutboxProviderDocumentDetailDto Detail(
        string? contractNumber,
        DateOnly? contractDate) => new()
    {
        ProviderCode = EdoProviderCode.EDOCS,
        DocumentDate = new DateOnly(2026, 3, 3),
        ContractNumber = contractNumber,
        ContractDate = contractDate
    };

    private static Contract Contract(
        int counterpartyId,
        string? providerCode,
        string? providerNumber,
        DateOnly? providerDate) => new()
    {
        Id = counterpartyId * 10L,
        OrganizationId = 2,
        CounterpartyId = 5,
        ContractNumber = "LOCAL-" + counterpartyId,
        ProviderCode = providerCode,
        ProviderContractNumber = providerNumber,
        ProviderContractDate = providerDate,
        StateId = 1,
        StartDate = new DateTime(2026, 1, 1),
        EndDate = new DateTime(2026, 12, 31)
    };
}
