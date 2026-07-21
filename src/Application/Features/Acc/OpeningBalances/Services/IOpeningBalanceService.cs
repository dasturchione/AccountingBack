using SharedKernel.Results;

namespace Application.Features.Acc.OpeningBalances
{
    public interface IOpeningBalanceService
    {
        Task<Result<OpeningBalanceDto>> GetAsync();
        Task<Result<OpeningBalanceDetailDto>> GetDetailAsync(long id, long openingBalanceAccountId);
        Task<Result<long>> CreateAsync(OpeningBalanceCreateDto dto);
        Task<Result> UpdateAsync(long id, OpeningBalanceUpdateDto dto);
        Task<Result> SaveAccountAsync(long id, OpeningBalanceAccountSaveDto dto);
        Task<Result> DeleteAsync(long id);
    }
}
