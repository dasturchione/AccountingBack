using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Contracts;

public static class ProviderContractReconciliationRules
{
    public const string EdocsProviderCode = "EDOCS";

    public static Error? Validate(
        ProviderContractReconciliationCreateDto dto,
        short? languageId = null)
    {
        if (!dto.Confirm)
            return ProviderContractReconciliationErrors.ConfirmationRequired(languageId);

        if (dto.CounterpartyId <= 0)
            return ProviderContractReconciliationErrors.CounterpartyRequired(languageId);

        if (!string.Equals(dto.ProviderCode?.Trim(), EdocsProviderCode, StringComparison.OrdinalIgnoreCase))
            return ProviderContractReconciliationErrors.ProviderInvalid(languageId);

        var providerNumber = NormalizeProviderNumber(dto.ProviderContractNumber);
        if (providerNumber is null)
            return ProviderContractReconciliationErrors.ProviderNumberInvalid(languageId);

        if (dto.ProviderContractDate == default)
            return ProviderContractReconciliationErrors.ProviderDateRequired(languageId);

        if (dto.ContractTypeId <= 0 || dto.ContractDate == default)
            return ProviderContractReconciliationErrors.HeaderRequired(languageId);

        if (dto.StartDate == default || dto.EndDate == default || dto.EndDate.Date < dto.StartDate.Date)
            return ProviderContractReconciliationErrors.DateIntervalInvalid(languageId);

        if (dto.Comment is not null && dto.Comment.Length > 1000)
            return ProviderContractReconciliationErrors.CommentInvalid(languageId);

        return null;
    }

    public static string? NormalizeProviderNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        if (normalized.Length > 100 || normalized.Any(char.IsControl))
            return null;

        return normalized;
    }

    public static string BuildIdempotencyKey(
        int organizationId,
        int counterpartyId,
        string providerCode,
        string providerContractNumber,
        DateOnly providerContractDate)
    {
        var source = $"EDO_CONTRACT|{organizationId}|{counterpartyId}|{providerCode.Trim().ToUpperInvariant()}|{providerContractNumber}|{providerContractDate:yyyy-MM-dd}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
        return $"EDO_CONTRACT_{hash}";
    }

    public static string ClassifyExistingIdentity(IEnumerable<Contract> contracts) =>
        contracts.Any(x => x.StateId == SharedKernel.Constants.StateIdConst.ACTIVE)
            ? "ALREADY_EXISTS"
            : contracts.Any()
                ? "INACTIVE"
                : "CREATE";
}
