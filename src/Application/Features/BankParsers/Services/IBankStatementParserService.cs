using SharedKernel.Results;

namespace Application.Features.BankParsers;

public interface IBankStatementParserService
{
    Task<Result<BankExportDto>> ParseAsync(Stream stream, CancellationToken ct = default);
}
