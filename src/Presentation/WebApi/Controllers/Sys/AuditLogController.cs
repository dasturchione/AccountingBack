using Application.Features.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;

namespace WebApi.Controllers;

[Route("api/audit-logs")]
[ApiController]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.AuditLogView)]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogFilter filter)
    {
        var result = await _service.GetByRecordAsync(filter);
        return Ok(result);
    }
}
