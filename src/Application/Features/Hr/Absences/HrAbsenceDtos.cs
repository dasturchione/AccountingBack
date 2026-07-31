using Application.Features.Hr.Files;
using SharedKernel.Filters;

namespace Application.Features.Hr.Absences;

public class HrAbsenceSaveDto
{
    public long EmployeeId { get; set; }
    public short AbsenceTypeId { get; set; }
    public DateOnly DocDate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Note { get; set; }
}

public sealed class HrAbsenceCreateDto : HrAbsenceSaveDto
{
    public List<HrFileUpload> Files { get; set; } = [];
}

public sealed class HrAbsenceUpdateDto : HrAbsenceSaveDto;

public sealed class HrAbsenceListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public long? EmployeeId { get; set; }
    public short? AbsenceTypeId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public class HrAbsenceListDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateOnly DocDate { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public short AbsenceTypeId { get; set; }
    public string AbsenceTypeCode { get; set; } = null!;
    public string AbsenceTypeName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int CalendarDays { get; set; }
    public int AttachmentCount { get; set; }
    public string? Note { get; set; }
}

public sealed class HrAbsenceDto : HrAbsenceListDto
{
    public int OrganizationId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<HrAbsenceAttachmentDto> Attachments { get; set; } = [];
}

public sealed class HrAbsenceAttachmentDto
{
    public long Id { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public DateTime CreatedDate { get; set; }
    public string DownloadUrl { get; set; } = null!;
}

public sealed record HrAttachmentDownload(
    Stream Content,
    string ContentType,
    string FileName);

public sealed class HrAbsenceTypeDto
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string TimesheetCategory { get; set; } = null!;
    public bool IsPaid { get; set; }
}
