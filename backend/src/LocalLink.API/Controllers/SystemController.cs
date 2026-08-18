using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalLink.Application.DTOs;
using LocalLink.Infrastructure.Persistence;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SystemController> _logger;

    public SystemController(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        ILogger<SystemController> logger)
    {
        _dbContext = dbContext;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<SystemStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var dbStatus = "unknown";
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            dbStatus = canConnect ? "connected" : "disconnected";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to database during system status check.");
            dbStatus = "unavailable";
        }

        var status = new SystemStatusDto
        {
            Application = "LocalLink",
            Status = "running",
            Environment = _environment.EnvironmentName,
            Database = dbStatus,
            TimestampUtc = DateTime.UtcNow,
            Version = "1.0.0"
        };

        return Ok(status);
    }
}
