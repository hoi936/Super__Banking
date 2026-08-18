using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    /// <summary>
    /// Get aggregated dashboard metrics (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "STAFF,ADMIN")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboardMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _adminDashboardService.GetDashboardMetricsAsync(cancellationToken);
        return Ok(metrics);
    }
}
