using Application.Common.Pagination;
using Application.Features.Hr.Files;
using SharedKernel.Results;

namespace Application.Features.Hr.Absences;

public interface IHrAbsenceService
{
    Task<Result<PagedResponse<HrAbsenceListDto>>> GetAllAsync(HrAbsenceListFilter filter, CancellationToken ct = default);
    Task<Result<HrAbsenceDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<List<HrAbsenceTypeDto>>> GetTypesAsync(CancellationToken ct = default);
    Task<Result<long>> CreateAsync(HrAbsenceCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, HrAbsenceUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<List<HrAbsenceAttachmentDto>>> AddAttachmentsAsync(long id, IReadOnlyCollection<HrFileUpload> files, CancellationToken ct = default);
    Task<Result<HrAttachmentDownload>> DownloadAttachmentAsync(long absenceId, long attachmentId, CancellationToken ct = default);
    Task<Result> DeleteAttachmentAsync(long absenceId, long attachmentId, CancellationToken ct = default);
}
