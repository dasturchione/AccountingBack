using Application.Features.Hr.Calendar;
using Application.Features.Hr.Schedules;
using Application.Features.Pay.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/hr/employees")]
[ApiController]
[Authorize]
public sealed class HrEmployeeController : ControllerBase
{
    private readonly IPayrollEmployeeService _employeeService;
    private readonly IHrWorkScheduleService _scheduleService;
    private readonly IHrEmployeeCalendarService _calendarService;

    public HrEmployeeController(
        IPayrollEmployeeService employeeService,
        IHrWorkScheduleService scheduleService,
        IHrEmployeeCalendarService calendarService)
    {
        _employeeService = employeeService;
        _scheduleService = scheduleService;
        _calendarService = calendarService;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeView)]
    public async Task<IResult> GetAllAsync(
        [FromQuery] PayrollEmployeeListFilter filter,
        CancellationToken ct = default)
    {
        var result = await _employeeService.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeView)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _employeeService.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeCreate)]
    public async Task<IResult> CreateAsync(
        [FromBody] PayrollEmployeeCreateDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] PayrollEmployeeUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _employeeService.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/employments")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeUpdate)]
    public async Task<IResult> AddEmploymentAsync(
        [FromRoute] long employeeId,
        [FromBody] PayrollEmploymentSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.AddEmploymentAsync(employeeId, dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{employeeId:long}/employments/{employmentId:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeUpdate)]
    public async Task<IResult> UpdateEmploymentAsync(
        [FromRoute] long employeeId,
        [FromRoute] long employmentId,
        [FromBody] PayrollEmploymentSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.UpdateEmploymentAsync(employeeId, employmentId, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("{employeeId:long}/history")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeView)]
    public async Task<IResult> GetHistoryAsync([FromRoute] long employeeId, CancellationToken ct = default)
    {
        var result = await _employeeService.GetHistoryAsync(employeeId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/transfer")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeUpdate)]
    public async Task<IResult> TransferAsync(
        [FromRoute] long employeeId,
        [FromBody] PayrollEmploymentTransferDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.TransferAsync(employeeId, dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/change-pay")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeUpdate)]
    public async Task<IResult> ChangePayAsync(
        [FromRoute] long employeeId,
        [FromBody] PayrollEmploymentPayChangeDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.ChangePayAsync(employeeId, dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/dismiss")]
    [ModuleAuthorize(PermissionCodeConst.HrEmployeeUpdate)]
    public async Task<IResult> DismissAsync(
        [FromRoute] long employeeId,
        [FromBody] PayrollEmploymentDismissDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.DismissAsync(employeeId, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/components")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeUpdate)]
    public async Task<IResult> AssignComponentAsync(
        [FromRoute] long employeeId,
        [FromBody] PayrollEmployeeComponentSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _employeeService.AssignComponentAsync(employeeId, dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpDelete("{employeeId:long}/components/{assignmentId:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeUpdate)]
    public async Task<IResult> RemoveComponentAsync(
        [FromRoute] long employeeId,
        [FromRoute] long assignmentId,
        CancellationToken ct = default)
    {
        var result = await _employeeService.RemoveComponentAsync(employeeId, assignmentId, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("{employeeId:long}/work-schedules")]
    [ModuleAuthorize(PermissionCodeConst.HrScheduleView)]
    public async Task<IResult> GetSchedulesAsync(
        [FromRoute] long employeeId,
        CancellationToken ct = default)
    {
        var result = await _scheduleService.GetAllAsync(employeeId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/work-schedules")]
    [ModuleAuthorize(PermissionCodeConst.HrScheduleManage)]
    public async Task<IResult> CreateScheduleAsync(
        [FromRoute] long employeeId,
        [FromBody] HrWorkScheduleSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _scheduleService.CreateAsync(employeeId, dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{employeeId:long}/work-schedules/{scheduleId:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrScheduleManage)]
    public async Task<IResult> UpdateScheduleAsync(
        [FromRoute] long employeeId,
        [FromRoute] long scheduleId,
        [FromBody] HrWorkScheduleSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _scheduleService.UpdateAsync(employeeId, scheduleId, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{employeeId:long}/work-schedules/{scheduleId:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrScheduleManage)]
    public async Task<IResult> DeleteScheduleAsync(
        [FromRoute] long employeeId,
        [FromRoute] long scheduleId,
        CancellationToken ct = default)
    {
        var result = await _scheduleService.DeleteAsync(employeeId, scheduleId, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("{employeeId:long}/calendar")]
    [ModuleAuthorize(PermissionCodeConst.HrCalendarView)]
    public async Task<IResult> GetCalendarAsync(
        [FromRoute] long employeeId,
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        CancellationToken ct = default)
    {
        var result = await _calendarService.GetAsync(employeeId, dateFrom, dateTo, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
