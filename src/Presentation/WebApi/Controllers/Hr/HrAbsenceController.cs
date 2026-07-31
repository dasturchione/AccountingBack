using Application.Features.Hr.Absences;
using Application.Features.Hr.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/hr/absences")]
[ApiController]
[Authorize]
public sealed class HrAbsenceController : ControllerBase
{
    private readonly IHrAbsenceService _service;

    public HrAbsenceController(IHrAbsenceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceView)]
    public async Task<IResult> GetAllAsync(
        [FromQuery] HrAbsenceListFilter filter,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("types")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceView)]
    public async Task<IResult> GetTypesAsync(CancellationToken ct = default)
    {
        var result = await _service.GetTypesAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceView)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceCreate)]
    public async Task<IResult> CreateAsync(
        [FromForm] HrAbsenceCreateRequest request,
        CancellationToken ct = default)
    {
        var uploads = OpenUploads(request.Files);
        try
        {
            var result = await _service.CreateAsync(new HrAbsenceCreateDto
            {
                EmployeeId = request.EmployeeId,
                AbsenceTypeId = request.AbsenceTypeId,
                DocDate = request.DocDate,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Note = request.Note,
                Files = uploads
            }, ct);
            return result.Match(Results.Ok, CustomResults.Problem);
        }
        finally
        {
            await DisposeUploadsAsync(uploads);
        }
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] HrAbsenceUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/attachments")]
    [Consumes("multipart/form-data")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceUpdate)]
    public async Task<IResult> AddAttachmentsAsync(
        [FromRoute] long id,
        [FromForm] List<IFormFile> files,
        CancellationToken ct = default)
    {
        var uploads = OpenUploads(files);
        try
        {
            var result = await _service.AddAttachmentsAsync(id, uploads, ct);
            return result.Match(Results.Ok, CustomResults.Problem);
        }
        finally
        {
            await DisposeUploadsAsync(uploads);
        }
    }

    [HttpGet("{absenceId:long}/attachments/{attachmentId:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceView)]
    public async Task<IResult> DownloadAttachmentAsync(
        [FromRoute] long absenceId,
        [FromRoute] long attachmentId,
        CancellationToken ct = default)
    {
        var result = await _service.DownloadAttachmentAsync(absenceId, attachmentId, ct);
        return result.Match(
            file => Results.File(
                file.Content,
                file.ContentType,
                file.FileName,
                enableRangeProcessing: true),
            CustomResults.Problem);
    }

    [HttpDelete("{absenceId:long}/attachments/{attachmentId:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrAbsenceUpdate)]
    public async Task<IResult> DeleteAttachmentAsync(
        [FromRoute] long absenceId,
        [FromRoute] long attachmentId,
        CancellationToken ct = default)
    {
        var result = await _service.DeleteAttachmentAsync(absenceId, attachmentId, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    private static List<HrFileUpload> OpenUploads(IEnumerable<IFormFile>? files) =>
        (files ?? [])
        .Select(file => new HrFileUpload(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length))
        .ToList();

    private static async Task DisposeUploadsAsync(IEnumerable<HrFileUpload> uploads)
    {
        foreach (var upload in uploads)
            await upload.Content.DisposeAsync();
    }
}

public sealed class HrAbsenceCreateRequest
{
    public long EmployeeId { get; set; }
    public short AbsenceTypeId { get; set; }
    public DateOnly DocDate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Note { get; set; }
    public List<IFormFile> Files { get; set; } = [];
}
