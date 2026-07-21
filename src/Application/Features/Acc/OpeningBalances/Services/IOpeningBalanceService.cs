using SharedKernel.Results;

namespace Application.Features.Acc.OpeningBalances
{
    public interface IOpeningBalanceService
    {
        Task<Result<OpeningBalanceDto>> GetAsync(CancellationToken ct = default);
        Task<Result<OpeningBalanceDetailDto>> GetDetailAsync(long id, long openingBalanceAccountId, CancellationToken ct = default);
        Task<Result<long>> CreateAsync(OpeningBalanceCreateDto dto, CancellationToken ct = default);
        Task<Result> UpdateAsync(long id, OpeningBalanceUpdateDto dto, CancellationToken ct = default);
        Task<Result> SaveAccountAsync(long id, OpeningBalanceAccountSaveDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    }
}
