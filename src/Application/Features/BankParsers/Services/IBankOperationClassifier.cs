using SharedKernel.Results;

namespace Application.Features.BankParsers;

public interface IBankOperationClassifier
{
    Task<Result<BankExportDto>> ClassifyAsync(
        BankExportDto export,
        int bankId,
        CancellationToken ct = default);
}
