using Application.Common.Pagination;
using Application.Features.Manual;
using SharedKernel.Results;

namespace Application.Features.Acc.DocumentAccountSettings;

public interface IDocumentAccountSettingService
{
    Task<Result<PagedResponse<DocumentAccountSettingListDto>>> GetAllAsync(DocumentAccountSettingListFilter filter, CancellationToken ct = default);
    Task<Result<DocumentAccountSettingDto>> GetByDocumentTypeIdAsync(short documentTypeId, CancellationToken ct = default);
    Task<Result<List<long>>> SaveAsync(DocumentAccountRuleSettingSaveDto dto, CancellationToken ct = default);
    Task<Result<List<ChartAccountSelectListDto>>> GetSelectListAsync(short documentTypeId, short? documentRoleId, string? documentRoleCode, CancellationToken ct = default);
}