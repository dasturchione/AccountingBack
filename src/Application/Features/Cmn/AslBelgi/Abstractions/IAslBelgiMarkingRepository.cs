using Domain.Entities;

namespace Application.Features.Cmn.AslBelgi.Abstractions;

/// <summary>Product info needed to build a marking order.</summary>
public sealed record AslBelgiProductMarkingInfo(int ProductId, string? Gtin);

/// <summary>
/// Read port for the marking flow (keeps EF/async-query concerns in Infrastructure so the service
/// stays unit-testable). Writes go through <see cref="Application.Abstractions.ICommandRepository{ProductTable}"/>.
/// </summary>
public interface IAslBelgiMarkingRepository
{
    Task<AslBelgiProductMarkingInfo?> GetProductForMarkingAsync(int productId, int organizationId, CancellationToken ct = default);

    /// <summary>Returns the subset of <paramref name="markings"/> already stored for the organization.</summary>
    Task<IReadOnlyCollection<string>> GetExistingMarkingsAsync(int organizationId, IReadOnlyCollection<string> markings, CancellationToken ct = default);
}
