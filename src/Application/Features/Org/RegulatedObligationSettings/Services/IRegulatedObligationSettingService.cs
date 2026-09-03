using SharedKernel.Results;

namespace Application.Features.RegulatedObligationSettings;

public interface IRegulatedObligationSettingService
{
    Task<Result<List<RegulatedObligationSettingDto>>> GetAllAsync(RegulatedObligationSettingListFilter filter, CancellationToken ct = default);
    Task<Result<RegulatedObligationSettingDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<RegulatedObligationSettingDto>> GetByCodeAsync(string code, DateOnly? choosedDate = null, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(RegulatedObligationSettingCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, RegulatedObligationSettingUpdateDto dto, CancellationToken ct = default);
}
