namespace Application.Abstractions;

public interface IDocNumberGenerator
{
    Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default);
}
