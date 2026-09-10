using Application.Features.Pay.Timesheets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payroll/timesheets")]
[ApiController]
[Authorize]
public sealed class PayrollTimesheetController : ControllerBase
{
    private readonly IPayrollTimesheetService _service;

    public PayrollTimesheetController(IPayrollTimesheetService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetView)]
    public async Task<IResult> GetAllAsync([FromQuery] PayrollTimesheetListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetView)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("calendar")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetView)]
    public async Task<IResult> GetEmployeeCalendarAsync(
        [FromQuery] long periodId,
        [FromQuery] long employeeId,
        CancellationToken ct = default)
    {
        var result = await _service.GetEmployeeCalendarAsync(periodId, employeeId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("attendance-status-options")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetView)]
    public async Task<IResult> GetAttendanceStatusOptionsAsync(CancellationToken ct = default)
    {
        var result = await _service.GetAttendanceStatusOptionsAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("calendar/table")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetView)]
    public async Task<IResult> GetCalendarTableAsync(
        [FromQuery] long periodId,
        CancellationToken ct = default)
    {
        var result = await _service.GetCalendarTableAsync(periodId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/calendar")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetView)]
    public async Task<IResult> GetDocumentCalendarAsync(
        [FromRoute] long id,
        CancellationToken ct = default)
    {
        var result = await _service.GetDocumentCalendarAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetCreate)]
    public async Task<IResult> CreateAsync([FromBody] PayrollTimesheetCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] PayrollTimesheetUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/initialize-days")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetUpdate)]
    public async Task<IResult> InitializeDaysAsync([FromRoute] long id, CancellationToken ct)
    {
        var result = await _service.InitializeDaysAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetConfirm)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.PayrollTimesheetCancel)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
