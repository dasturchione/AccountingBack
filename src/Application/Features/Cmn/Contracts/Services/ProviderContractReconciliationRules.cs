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
            return Error.Conflict("EDO_CONTRACT_CONFIRMATION_REQUIRED", "Explicit contract reconciliation confirmation is required.");

        if (dto.CounterpartyId <= 0)
            return Error.Problem("EDO_CONTRACT_COUNTERPARTY_REQUIRED", "A valid counterparty is required.");

        if (!string.Equals(dto.ProviderCode?.Trim(), EdocsProviderCode, StringComparison.OrdinalIgnoreCase))
            return Error.Problem("EDO_CONTRACT_PROVIDER_INVALID", "Only the active EDOCS provider identity is accepted by this endpoint.");

        var providerNumber = NormalizeProviderNumber(dto.ProviderContractNumber);
        if (providerNumber is null)
            return Error.Problem("EDO_CONTRACT_PROVIDER_NUMBER_INVALID", "A non-empty provider contract number is required.");

        if (dto.ProviderContractDate == default)
            return Error.Problem("EDO_CONTRACT_PROVIDER_DATE_REQUIRED", "A provider contract date is required.");

        if (dto.ContractTypeId <= 0 || dto.ContractDate == default)
            return Error.Problem("EDO_CONTRACT_HEADER_REQUIRED", "Contract type and contract date are required.");

        if (dto.StartDate == default || dto.EndDate == default || dto.EndDate.Date < dto.StartDate.Date)
            return Error.Problem("EDO_CONTRACT_DATE_INTERVAL_INVALID", "The contract date interval is invalid.");

        if (dto.Comment is not null && dto.Comment.Length > 1000)
            return Error.Problem("EDO_CONTRACT_COMMENT_INVALID", "The contract comment is too long.");

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
