using Application.Features.Contracts;
using Domain.Entities;

public sealed class ProviderContractReconciliationRulesTests
{
    [Fact]
    public void RequiresExplicitConfirmationAndCompleteDateInterval()
    {
        var dto = ValidDto() with { Confirm = false };

        var error = ProviderContractReconciliationRules.Validate(dto);

        Assert.NotNull(error);
        Assert.Equal("EDO_CONTRACT_CONFIRMATION_REQUIRED", error!.Code);
    }

    [Fact]
    public void RejectsInvalidIntervalAndNonEdocsProvider()
    {
        var providerError = ProviderContractReconciliationRules.Validate(ValidDto() with { ProviderCode = "DIDOX" });
        var intervalError = ProviderContractReconciliationRules.Validate(ValidDto() with
        {
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2026, 3, 1)
        });

        Assert.Equal("EDO_CONTRACT_PROVIDER_INVALID", providerError!.Code);
        Assert.Equal("EDO_CONTRACT_DATE_INTERVAL_INVALID", intervalError!.Code);
    }

    [Fact]
    public void IdempotencyKeyIsStableAndDoesNotExposeIdentity()
    {
        var first = ProviderContractReconciliationRules.BuildIdempotencyKey(2, 5, "edocs", "3", new DateOnly(2026, 3, 3));
        var second = ProviderContractReconciliationRules.BuildIdempotencyKey(2, 5, "EDOCS", "3", new DateOnly(2026, 3, 3));

        Assert.Equal(first, second);
        Assert.StartsWith("EDO_CONTRACT_", first);
        Assert.DoesNotContain("|", first);
        Assert.Equal("EDO_CONTRACT_".Length + 64, first.Length);
    }

    [Fact]
    public void ExistingIdentityIsReusedOnlyWhenActive()
    {
        var active = new Contract { StateId = 1 };
        var inactive = new Contract { StateId = 2 };

        Assert.Equal("ALREADY_EXISTS", ProviderContractReconciliationRules.ClassifyExistingIdentity([active]));
        Assert.Equal("INACTIVE", ProviderContractReconciliationRules.ClassifyExistingIdentity([inactive]));
        Assert.Equal("CREATE", ProviderContractReconciliationRules.ClassifyExistingIdentity([]));
    }

    private static ProviderContractReconciliationCreateDto ValidDto() => new()
    {
        Confirm = true,
        CounterpartyId = 5,
        ProviderCode = "EDOCS",
        ProviderContractNumber = "3",
        ProviderContractDate = new DateOnly(2026, 3, 3),
        ContractTypeId = 1,
        ContractDate = new DateTime(2026, 3, 3),
        StartDate = new DateTime(2026, 1, 1),
        EndDate = new DateTime(2026, 12, 31)
    };
}
