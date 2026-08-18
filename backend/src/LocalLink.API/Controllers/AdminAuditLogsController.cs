using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Roles = "ADMIN")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly IAdminAuditService _adminAuditService;

    public AdminAuditLogsController(IAdminAuditService adminAuditService)
    {
        _adminAuditService = adminAuditService;
    }

    /// <summary>
    /// Get paginated and searchable list of audit logs (Requires ADMIN role)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminAuditService.GetAuditLogsAsync(
            page, pageSize, action, entityType, userId, fromDate, toDate, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get audit log detail (Requires ADMIN role)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogDetail(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _adminAuditService.GetAuditLogDetailAsync(id, cancellationToken);
        return Ok(detail);
    }
}
