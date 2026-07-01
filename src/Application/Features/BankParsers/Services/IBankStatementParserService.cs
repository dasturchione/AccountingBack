using SharedKernel.Results;

namespace Application.Features.BankParsers;

public interface IBankStatementParserService
{
    Task<Result<BankExportDto>> ParseExcelAsync(Stream stream, CancellationToken ct = default);
    Task<Result<BankExportDto>> EnrichAsync(BankExportDto export, CancellationToken ct = default);
    Task<Result<BankExportDto>> ParseAsync(Stream stream, CancellationToken ct = default);
}
