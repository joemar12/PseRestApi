using Microsoft.AspNetCore.Mvc;
using PseRestApi.Core.Common.Interfaces;

namespace PseRestApi.Host.Controllers;

/// <summary>
/// Health check endpoint for monitoring and container orchestration (Docker, Kubernetes).
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly IAppDbContext _appDbContext;

    public HealthController(IAppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    /// <summary>
    /// Simple health check endpoint.
    /// Returns 200 OK if the service is running.
    /// Used by Docker health checks and load balancers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connectivity
            var result = await _appDbContext.Database.CanConnectAsync();
            if (!result)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "unhealthy", reason = "database_connection_failed" });
            }

            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "unhealthy", reason = ex.Message });
        }
    }

    /// <summary>
    /// Detailed health check endpoint.
    /// Returns comprehensive health information including database and external dependencies.
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDetailed()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            checks = await GetHealthChecks()
        };

        var allHealthy = await ValidateHealthChecks();
        if (!allHealthy)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
        }

        return Ok(health);
    }

    private async Task<object> GetHealthChecks()
    {
        var database = false;
        var databaseMessage = string.Empty;

        try
        {
            database = await _appDbContext.Database.CanConnectAsync();
            databaseMessage = database ? "Connected" : "Connection failed";
        }
        catch (Exception ex)
        {
            databaseMessage = $"Error: {ex.Message}";
        }

        return new
        {
            database = new { status = database ? "healthy" : "unhealthy", message = databaseMessage }
        };
    }

    private async Task<bool> ValidateHealthChecks()
    {
        try
        {
            return await _appDbContext.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}
