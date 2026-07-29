using Application.Features.Pay.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payroll/reports")]
[ApiController]
[Authorize]
public sealed class PayrollReportController : ControllerBase
{
    private readonly IPayrollReportService _service;

    public PayrollReportController(IPayrollReportService service)
    {
        _service = service;
    }

    [HttpGet("register")]
    [ModuleAuthorize(PermissionCodeConst.PayrollReportView)]
    public async Task<IResult> GetRegisterAsync([FromQuery] long periodId, CancellationToken ct = default)
    {
        var result = await _service.GetRegisterAsync(periodId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("payslip")]
    [ModuleAuthorize(PermissionCodeConst.PayrollReportView)]
    public async Task<IResult> GetPayslipAsync(
        [FromQuery] long periodId,
        [FromQuery] long employeeId,
        CancellationToken ct = default)
    {
        var result = await _service.GetPayslipAsync(periodId, employeeId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
