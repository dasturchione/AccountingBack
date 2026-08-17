using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.Hr.Files;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Text.RegularExpressions;

namespace Application.Features.Hr.Absences;

public sealed class HrAbsenceService : BaseService, IHrAbsenceService
{
    private readonly ILogger<HrAbsenceService> _logger;
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IHrFileStorage _fileStorage;
    private readonly ITelegramFileArchive _telegramArchive;
    private readonly IQueryRepository<HrAbsence> _absenceQuery;
    private readonly ICommandRepository<HrAbsence> _absenceCommand;
    private readonly IQueryRepository<HrAbsenceAttachment> _attachmentQuery;
    private readonly ICommandRepository<HrAbsenceAttachment> _attachmentCommand;
    private readonly IQueryRepository<HrAbsenceType> _typeQuery;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly IQueryRepository<PayTimesheetLine> _timesheetLineQuery;

    public HrAbsenceService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        IHrFileStorage fileStorage,
        ITelegramFileArchive telegramArchive,
        IQueryRepository<HrAbsence> absenceQuery,
        ICommandRepository<HrAbsence> absenceCommand,
        IQueryRepository<HrAbsenceAttachment> attachmentQuery,
        ICommandRepository<HrAbsenceAttachment> attachmentCommand,
        IQueryRepository<HrAbsenceType> typeQuery,
        IQueryRepository<PayEmployee> employeeQuery,
        IQueryRepository<PayEmployment> employmentQuery,
        IQueryRepository<PayTimesheetLine> timesheetLineQuery,
        ILogger<HrAbsenceService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _logger = logger;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _fileStorage = fileStorage;
        _telegramArchive = telegramArchive;
        _absenceQuery = absenceQuery;
        _absenceCommand = absenceCommand;
        _attachmentQuery = attachmentQuery;
        _attachmentCommand = attachmentCommand;
        _typeQuery = typeQuery;
        _employeeQuery = employeeQuery;
        _employmentQuery = employmentQuery;
        _timesheetLineQuery = timesheetLineQuery;
    }

    public Task<Result<PagedResponse<HrAbsenceListDto>>> GetAllAsync(
        HrAbsenceListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();

            var specification = _queryBuilder.For<HrAbsence>()
                .Where(x =>
                    x.StateId == StateIdConst.ACTIVE &&
                    (!filter.EmployeeId.HasValue || x.EmployeeId == filter.EmployeeId.Value) &&
                    (!filter.AbsenceTypeId.HasValue || x.AbsenceTypeId == filter.AbsenceTypeId.Value) &&
                    (!filter.DateFrom.HasValue || x.EndDate >= filter.DateFrom.Value) &&
                    (!filter.DateTo.HasValue || x.StartDate <= filter.DateTo.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.DocNumber.ToLower().Contains(search) ||
                     x.Employee.EmployeeNumber.ToLower().Contains(search) ||
                     x.Employee.FirstName.ToLower().Contains(search) ||
                     x.Employee.LastName.ToLower().Contains(search) ||
                     (x.Note != null && x.Note.ToLower().Contains(search))))
                .As(x => new HrAbsenceListDto
                {
                    Id = x.Id,
                    DocNumber = x.DocNumber,
                    DocDate = x.DocDate,
                    EmployeeId = x.EmployeeId,
                    EmployeeNumber = x.Employee.EmployeeNumber,
                    EmployeeName = x.Employee.LastName + " " + x.Employee.FirstName +
                                   (x.Employee.MiddleName != null ? " " + x.Employee.MiddleName : ""),
                    AbsenceTypeId = x.AbsenceTypeId,
                    AbsenceTypeCode = x.AbsenceType.Code,
                    AbsenceTypeName = x.AbsenceType.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    AttachmentCount = x.Attachments.Count,
                    Note = x.Note
                })
                .OrderBy(x => x.OrderByDescending(y => y.DocDate).ThenByDescending(y => y.Id))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();

            var paged = await _absenceQuery.GetPagedAsync(specification, ct);
            foreach (var item in paged.Items)
                item.CalendarDays = item.EndDate.DayNumber - item.StartDate.DayNumber + 1;
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<HrAbsenceDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<HrAbsenceDto>(HrErrors.NotFound("Absence", id))
                : Result.Success(dto);
        });

    public Task<Result<List<HrAbsenceTypeDto>>> GetTypesAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTypesAsync), async () =>
        {
            var query = _queryBuilder.For<HrAbsenceType>()
                .Where(x => x.StateId == StateIdConst.ACTIVE)
                .As(x => new HrAbsenceTypeDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    TimesheetCategory = x.TimesheetCategory,
                    IsPaid = x.IsPaid
                })
                .Build();
            var types = await _typeQuery.GetAllAsync(query, ct);
            return Result.Success(types.OrderBy(x => x.Id).ToList());
        });

    public Task<Result<long>> CreateAsync(HrAbsenceCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            var validation = await ValidateAsync(dto, null, ct);
            if (!validation.IsSuccess)
                return Result.Failure<long>(validation.Error);

            var organizationId = _userContext.OrganizationId!.Value;
            var documentNumberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.HRABSENCE,
                dto.DocDate.ToDateTime(TimeOnly.MinValue),
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var entity = new HrAbsence
            {
                OrganizationId = organizationId,
                EmployeeId = dto.EmployeeId,
                AbsenceTypeId = dto.AbsenceTypeId,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = dto.DocDate,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Note = NormalizeNote(dto.Note),
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                CreatedByUserId = _userContext.Id
            };

            await _absenceCommand.CreateAsync(entity, ct);
            if (dto.Files.Count > 0)
            {
                var filesResult = await AddAttachmentsCoreAsync(entity, dto.Files, ct);
                if (!filesResult.IsSuccess)
                    return Result.Failure<long>(filesResult.Error);
            }

            _auditLogService.SetNewValues((await GetDtoInternalAsync(entity.Id, ct))!);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.HrAbsence,
                entity.Id.ToString(),
                AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, HrAbsenceUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeAttachments: false, ct);
            if (entity is null)
                return Result.Failure(HrErrors.NotFound("Absence", id));

            if (await IsLockedByPostedTimesheetAsync(entity.EmployeeId, entity.StartDate, entity.EndDate, ct))
                return Result.Failure(HrErrors.Conflict(
                    "AbsenceLocked",
                    "Ushbu yo‘qlik hujjati tasdiqlangan tabelda ishlatilgan, shuning uchun uni o‘zgartirib bo‘lmaydi."));

            var validation = await ValidateAsync(dto, id, ct);
            if (!validation.IsSuccess)
                return validation;

            _auditLogService.SetOldValues((await GetDtoInternalAsync(id, ct))!);
            entity.EmployeeId = dto.EmployeeId;
            entity.AbsenceTypeId = dto.AbsenceTypeId;
            entity.DocDate = dto.DocDate;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Note = NormalizeNote(dto.Note);
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _absenceCommand.UpdateAsync(entity, ct);

            _auditLogService.SetNewValues((await GetDtoInternalAsync(id, ct))!);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.HrAbsence,
                id.ToString(),
                AuditLogOperationTypeConst.Update);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var entity = await GetEntityAsync(id, includeAttachments: true, ct);
            if (entity is null)
                return Result.Failure(HrErrors.NotFound("Absence", id));

            if (await IsLockedByPostedTimesheetAsync(entity.EmployeeId, entity.StartDate, entity.EndDate, ct))
                return Result.Failure(HrErrors.Conflict(
                    "AbsenceLocked",
                    "Ushbu yo‘qlik hujjati tasdiqlangan tabelda ishlatilgan, shuning uchun uni o‘chirib bo‘lmaydi."));

            _auditLogService.SetOldValues((await GetDtoInternalAsync(id, ct))!);
            await _absenceCommand.DeleteAsync(entity, ct);
            await CleanupFilesAsync(entity.Attachments, CancellationToken.None);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.HrAbsence,
                id.ToString(),
                AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    public Task<Result<List<HrAbsenceAttachmentDto>>> AddAttachmentsAsync(
        long id,
        IReadOnlyCollection<HrFileUpload> files,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(AddAttachmentsAsync), async () =>
        {
            if (files.Count == 0)
                return Result.Failure<List<HrAbsenceAttachmentDto>>(
                    HrErrors.Business("EmptyAttachments", "Kamida bitta fayl tanlanishi kerak."));

            var entity = await GetEntityAsync(id, includeAttachments: false, ct);
            if (entity is null)
                return Result.Failure<List<HrAbsenceAttachmentDto>>(HrErrors.NotFound("Absence", id));
            if (await IsLockedByPostedTimesheetAsync(entity.EmployeeId, entity.StartDate, entity.EndDate, ct))
                return Result.Failure<List<HrAbsenceAttachmentDto>>(HrErrors.Conflict(
                    "AbsenceLocked",
                    "Yo‘qlik hujjati tasdiqlangan tabelda ishlatilgandan keyin uning fayllarini o‘zgartirib bo‘lmaydi."));

            return await AddAttachmentsCoreAsync(entity, files, ct);
        }, ct);

    public Task<Result<HrAttachmentDownload>> DownloadAttachmentAsync(
        long absenceId,
        long attachmentId,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(DownloadAttachmentAsync), async () =>
        {
            var attachment = await GetAttachmentAsync(absenceId, attachmentId, ct);
            if (attachment is null)
                return Result.Failure<HrAttachmentDownload>(HrErrors.NotFound("AbsenceAttachment", attachmentId));

            var local = await _fileStorage.OpenAsync(attachment.FilePath, ct);
            if (local is null)
            {
                try
                {
                    await using var telegram = await _telegramArchive.DownloadAsync(attachment.TelegramFileId, ct);
                    await _fileStorage.RestoreAsync(attachment.FilePath, telegram, ct);
                    local = await _fileStorage.OpenAsync(attachment.FilePath, ct);
                }
                catch
                {
                    return Result.Failure<HrAttachmentDownload>(
                        HrErrors.FileStorage("Fayl serverda ham, Telegram arxivida ham topilmadi."));
                }
            }

            return local is null
                ? Result.Failure<HrAttachmentDownload>(HrErrors.FileStorage("Faylni qayta tiklab bo‘lmadi."))
                : Result.Success(new HrAttachmentDownload(local, attachment.ContentType, attachment.OriginalFileName));
        });

    public Task<Result> DeleteAttachmentAsync(
        long absenceId,
        long attachmentId,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAttachmentAsync), async () =>
        {
            var attachment = await GetAttachmentAsync(absenceId, attachmentId, ct);
            if (attachment is null)
                return Result.Failure(HrErrors.NotFound("AbsenceAttachment", attachmentId));
            var absence = await GetEntityAsync(absenceId, includeAttachments: false, ct);
            if (absence is null)
                return Result.Failure(HrErrors.NotFound("Absence", absenceId));
            if (await IsLockedByPostedTimesheetAsync(absence.EmployeeId, absence.StartDate, absence.EndDate, ct))
                return Result.Failure(HrErrors.Conflict(
                    "AbsenceLocked",
                    "Yo‘qlik hujjati tasdiqlangan tabelda ishlatilgandan keyin uning fayllarini o‘zgartirib bo‘lmaydi."));

            await _attachmentCommand.DeleteAsync(attachment, ct);
            await CleanupFilesAsync([attachment], CancellationToken.None);
            return Result.Success();
        }, ct);

    private async Task<Result<List<HrAbsenceAttachmentDto>>> AddAttachmentsCoreAsync(
        HrAbsence absence,
        IReadOnlyCollection<HrFileUpload> files,
        CancellationToken ct)
    {
        var created = new List<HrAbsenceAttachment>();
        var localPaths = new List<string>();
        var telegramMessageIds = new List<int>();
        var stage = "LocalFileSave";
        HrFileUpload? currentFile = null;
        try
        {
            foreach (var file in files)
            {
                currentFile = file;
                stage = "LocalFileSave";
                var stored = await _fileStorage.SaveAsync(absence.OrganizationId, absence.Id, file, ct);
                localPaths.Add(stored.RelativePath);
                stage = "LocalFileOpen";
                await using var local = await _fileStorage.OpenAsync(stored.RelativePath, ct)
                    ?? throw new IOException("Saqlangan faylni qayta ochib bo‘lmadi.");
                stage = "TelegramUpload";
                var telegram = await _telegramArchive.UploadAsync(
                    local,
                    file.FileName,
                    file.ContentType,
                    $"HR absence {absence.DocNumber}; employeeId={absence.EmployeeId}",
                    ct);
                telegramMessageIds.Add(telegram.MessageId);

                created.Add(new HrAbsenceAttachment
                {
                    OrganizationId = absence.OrganizationId,
                    AbsenceId = absence.Id,
                    FilePath = stored.RelativePath,
                    OriginalFileName = Truncate(Path.GetFileName(file.FileName), 255),
                    ContentType = Truncate(
                        string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                        150),
                    FileSize = file.Length,
                    TelegramFileId = telegram.FileId,
                    TelegramMessageId = telegram.MessageId,
                    CreatedDate = DateTime.Now,
                    CreatedByUserId = _userContext.Id
                });
            }

            currentFile = null;
            stage = "AttachmentDatabaseSave";
            await _attachmentCommand.CreateAsync(created, ct);
            return Result.Success(created.Select(MapAttachment).ToList());
        }
        catch (Exception ex)
        {
            var extension = currentFile is null
                ? null
                : Path.GetExtension(Path.GetFileName(currentFile.FileName));
            if (stage == "TelegramUpload")
            {
                _logger.LogError(
                    "HR attachment save failed; Stage={Stage}; OrganizationId={OrganizationId}; AbsenceId={AbsenceId}; " +
                    "FileExtension={FileExtension}; FileSize={FileSize}; FileCount={FileCount}; " +
                    "ExceptionType={ExceptionType}; Error={Error}",
                    stage,
                    absence.OrganizationId,
                    absence.Id,
                    extension,
                    currentFile?.Length,
                    files.Count,
                    ex.GetType().Name,
                    SanitizeTelegramLogMessage(ex.Message));
            }
            else
            {
                _logger.LogError(
                    ex,
                    "HR attachment save failed; Stage={Stage}; OrganizationId={OrganizationId}; AbsenceId={AbsenceId}; " +
                    "FileExtension={FileExtension}; FileSize={FileSize}; FileCount={FileCount}",
                    stage,
                    absence.OrganizationId,
                    absence.Id,
                    extension,
                    currentFile?.Length,
                    files.Count);
            }

            await CleanupArtifactsAsync(localPaths, telegramMessageIds, CancellationToken.None);
            return Result.Failure<List<HrAbsenceAttachmentDto>>(
                HrErrors.FileStorage("Faylni AppData va Telegram arxiviga saqlab bo‘lmadi."));
        }
    }

    private async Task<Result> ValidateAsync(HrAbsenceSaveDto dto, long? currentId, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
        if (dto.EndDate < dto.StartDate)
            return Result.Failure(HrErrors.Business("InvalidAbsenceDates", "Tugash sanasi boshlanish sanasidan oldin bo‘lishi mumkin emas."));
        if (dto.EndDate.DayNumber - dto.StartDate.DayNumber > 731)
            return Result.Failure(HrErrors.Business("AbsenceRangeTooLarge", "Bitta yo‘qlik hujjati davri ikki yildan oshmasligi kerak."));
        if (dto.Note?.Length > 1000)
            return Result.Failure(HrErrors.Business("AbsenceNoteTooLong", "Izoh 1000 ta belgidan oshmasligi kerak."));

        if (!await _employeeQuery.AnyAsync(x =>
                x.Id == dto.EmployeeId &&
                x.OrganizationId == _userContext.OrganizationId.Value &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(HrErrors.NotFound("Employee", dto.EmployeeId));
        if (!await _typeQuery.AnyAsync(x => x.Id == dto.AbsenceTypeId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(HrErrors.NotFound("AbsenceType", dto.AbsenceTypeId));
        if (!await _employmentQuery.AnyAsync(x =>
                x.EmployeeId == dto.EmployeeId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StartDate <= dto.StartDate &&
                (!x.EndDate.HasValue || x.EndDate.Value >= dto.EndDate), ct))
            return Result.Failure(HrErrors.Business(
                "AbsenceOutsideEmployment",
                "Yo‘qlik davri xodimning amaldagi ishga qabul davri ichida bo‘lishi kerak."));
        if (await _absenceQuery.AnyAsync(x =>
                x.EmployeeId == dto.EmployeeId &&
                x.StateId == StateIdConst.ACTIVE &&
                (!currentId.HasValue || x.Id != currentId.Value) &&
                x.StartDate <= dto.EndDate &&
                x.EndDate >= dto.StartDate, ct))
            return Result.Failure(HrErrors.Conflict(
                "AbsenceOverlap",
                "Tanlangan davrda xodim uchun boshqa yo‘qlik hujjati mavjud."));
        if (await IsLockedByPostedTimesheetAsync(dto.EmployeeId, dto.StartDate, dto.EndDate, ct))
            return Result.Failure(HrErrors.Conflict(
                "PostedTimesheet",
                "Tasdiqlangan tabel mavjud bo‘lgan davr uchun yo‘qlik hujjatini yaratish yoki o‘zgartirish mumkin emas."));

        return Result.Success();
    }

    private Task<bool> IsLockedByPostedTimesheetAsync(
        long employeeId,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct) =>
        _timesheetLineQuery.AnyAsync(x =>
            x.EmployeeId == employeeId &&
            x.Timesheet.StatusId == DocumentStatusIdConst.POSTED &&
            x.Timesheet.Period.StartDate <= dateTo &&
            x.Timesheet.Period.EndDate >= dateFrom, ct);

    private async Task<HrAbsence?> GetEntityAsync(long id, bool includeAttachments, CancellationToken ct)
    {
        var query = _queryBuilder.For<HrAbsence>()
            .Where(x => x.Id == id && x.StateId == StateIdConst.ACTIVE)
            .Build();
        if (includeAttachments)
            query.AddIncludes(x => x.Include(absence => absence.Attachments));
        return await _absenceQuery.GetAsync(query, ct);
    }

    private async Task<HrAbsenceAttachment?> GetAttachmentAsync(
        long absenceId,
        long attachmentId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<HrAbsenceAttachment>()
            .Where(x => x.Id == attachmentId && x.AbsenceId == absenceId)
            .Build();
        return await _attachmentQuery.GetAsync(query, ct);
    }

    private async Task<HrAbsenceDto?> GetDtoInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<HrAbsence>()
            .Where(x => x.Id == id && x.StateId == StateIdConst.ACTIVE)
            .As(x => new HrAbsenceDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                DocNumber = x.DocNumber,
                DocDate = x.DocDate,
                EmployeeId = x.EmployeeId,
                EmployeeNumber = x.Employee.EmployeeNumber,
                EmployeeName = x.Employee.LastName + " " + x.Employee.FirstName +
                               (x.Employee.MiddleName != null ? " " + x.Employee.MiddleName : ""),
                AbsenceTypeId = x.AbsenceTypeId,
                AbsenceTypeCode = x.AbsenceType.Code,
                AbsenceTypeName = x.AbsenceType.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                AttachmentCount = x.Attachments.Count,
                Note = x.Note,
                StateId = x.StateId,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                Attachments = x.Attachments
                    .OrderBy(attachment => attachment.Id)
                    .Select(attachment => new HrAbsenceAttachmentDto
                    {
                        Id = attachment.Id,
                        OriginalFileName = attachment.OriginalFileName,
                        ContentType = attachment.ContentType,
                        FileSize = attachment.FileSize,
                        CreatedDate = attachment.CreatedDate,
                        DownloadUrl = "/api/hr/absences/" + x.Id + "/attachments/" + attachment.Id
                    })
                    .ToList()
            })
            .Build();
        var dto = await _absenceQuery.GetAsync(query, ct);
        if (dto is not null)
            dto.CalendarDays = dto.EndDate.DayNumber - dto.StartDate.DayNumber + 1;
        return dto;
    }

    private async Task CleanupFilesAsync(
        IEnumerable<HrAbsenceAttachment> attachments,
        CancellationToken ct)
    {
        var list = attachments.ToList();
        await CleanupArtifactsAsync(
            list.Select(x => x.FilePath),
            list.Select(x => x.TelegramMessageId),
            ct);
    }

    private async Task CleanupArtifactsAsync(
        IEnumerable<string> localPaths,
        IEnumerable<int> telegramMessageIds,
        CancellationToken ct)
    {
        foreach (var path in localPaths)
        {
            try
            {
                await _fileStorage.DeleteAsync(path, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "HR attachment local cleanup failed; FilePath={FilePath}",
                    path);
                // Database consistency has priority; an orphaned local file is recoverable.
            }
        }

        foreach (var messageId in telegramMessageIds)
        {
            try
            {
                await _telegramArchive.DeleteMessageAsync(messageId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "HR attachment Telegram cleanup failed; MessageId={MessageId}; ExceptionType={ExceptionType}; Error={Error}",
                    messageId,
                    ex.GetType().Name,
                    SanitizeTelegramLogMessage(ex.Message));
                // Telegram cleanup failure must not corrupt the HR transaction.
            }
        }
    }

    private static string SanitizeTelegramLogMessage(string message) =>
        Regex.Replace(
            message,
            @"\d{5,}:[A-Za-z0-9_-]{10,}",
            "[REDACTED]",
            RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100));

    private static HrAbsenceAttachmentDto MapAttachment(HrAbsenceAttachment x) =>
        new()
        {
            Id = x.Id,
            OriginalFileName = x.OriginalFileName,
            ContentType = x.ContentType,
            FileSize = x.FileSize,
            CreatedDate = x.CreatedDate,
            DownloadUrl = $"/api/hr/absences/{x.AbsenceId}/attachments/{x.Id}"
        };

    private static string? NormalizeNote(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
