using SharedKernel.Results;

namespace Application.Features.Pay.PayrollDocuments;

public interface IPayrollAccountResolver
{
    Task<Result<IReadOnlyDictionary<string, int>>> ResolveAsync(
        int organizationId,
        IEnumerable<string> roleCodes,
        CancellationToken ct = default);
}
