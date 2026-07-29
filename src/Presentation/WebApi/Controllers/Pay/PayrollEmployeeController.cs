using Application.Features.Pay.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payroll/employees")]
[ApiController]
[Authorize]
public sealed class PayrollEmployeeController : ControllerBase
{
    private readonly IPayrollEmployeeService _service;

    public PayrollEmployeeController(IPayrollEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeView)]
    public async Task<IResult> GetAllAsync([FromQuery] PayrollEmployeeListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeView)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeCreate)]
    public async Task<IResult> CreateAsync([FromBody] PayrollEmployeeCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] PayrollEmployeeUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/employments")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeUpdate)]
    public async Task<IResult> AddEmploymentAsync(
        [FromRoute] long employeeId,
        [FromBody] PayrollEmploymentSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.AddEmploymentAsync(employeeId, dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{employeeId:long}/employments/{employmentId:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeUpdate)]
    public async Task<IResult> UpdateEmploymentAsync(
        [FromRoute] long employeeId,
        [FromRoute] long employmentId,
        [FromBody] PayrollEmploymentSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateEmploymentAsync(employeeId, employmentId, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{employeeId:long}/components")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeUpdate)]
    public async Task<IResult> AssignComponentAsync(
        [FromRoute] long employeeId,
        [FromBody] PayrollEmployeeComponentSaveDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.AssignComponentAsync(employeeId, dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpDelete("{employeeId:long}/components/{assignmentId:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollEmployeeUpdate)]
    public async Task<IResult> RemoveComponentAsync(
        [FromRoute] long employeeId,
        [FromRoute] long assignmentId,
        CancellationToken ct = default)
    {
        var result = await _service.RemoveComponentAsync(employeeId, assignmentId, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
