namespace Application.Abstractions;

public interface IOrganizationSourceReader
{
    Task<int?> GetProductOrganizationIdAsync(int productId, CancellationToken ct = default);
    Task<int?> GetCounterpartyOrganizationIdAsync(int counterpartyId, CancellationToken ct = default);
    Task<int?> GetProductTableOrganizationIdAsync(int productTableId, CancellationToken ct = default);
}
