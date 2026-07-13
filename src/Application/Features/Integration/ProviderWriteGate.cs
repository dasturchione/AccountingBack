using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Integration;

/// <summary>
/// Central fail-closed gate for provider write operations whose contract is not verified.
/// It performs no I/O and intentionally returns only a safe error code/message.
/// </summary>
public static class ProviderWriteGate
{
    public static Result RequireContract(Provider provider) => provider switch
    {
        Provider.Didox or Provider.AslBelgi => Result.Failure(
            Error.Business("Integration.ProviderWriteBlocked", "Provider write operations are blocked until the contract is verified.")),
        _ => Result.Success()
    };
}
