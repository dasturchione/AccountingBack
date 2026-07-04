namespace Application.Features.Register.PostingEngines;

public interface IOrganizationAccountingPolicyResolver
{
    Task<short> ResolveAsync(int organizationId, CancellationToken ct = default);
}
