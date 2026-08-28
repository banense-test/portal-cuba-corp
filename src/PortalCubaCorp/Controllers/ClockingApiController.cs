using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Controllers;

/// <summary>
/// REST API endpoint for clocking operations (UC-001, AC-005).
/// Receives POST from clocking-retry.js with employeeId, timestamp, type, idempotencyKey.
/// The idempotency key prevents duplicate records on offline retry (R006).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClockingApiController : ControllerBase
{
    private readonly IClockingService _clockingService;

    public ClockingApiController(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    /// <summary>
    /// POST /api/clocking — record a clock in/out event.
    /// Used by clocking-retry.js for both normal and offline-retry paths.
    /// EmployeeId is taken from the OIDC token subject claim.
    /// </summary>
    [HttpPost]
    public IActionResult RecordClocking([FromBody] RecordClockingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, error = "Invalid request" });

        // Extract employee ID from OIDC token subject claim
        var employeeId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(employeeId))
            return Unauthorized(new { success = false, error = "Employee identity not found" });

        var result = _clockingService.RecordClocking(
            employeeId,
            request.Timestamp,
            request.ClockType,
            request.IdempotencyKey);

        if (!result.Success)
            return BadRequest(new { success = false, error = result.Error });

        return Ok(new
        {
            success = true,
            isDuplicate = result.IsDuplicate,
            record = new
            {
                id = result.Record!.Id,
                employeeId = result.Record.EmployeeId,
                timestamp = result.Record.Timestamp,
                type = result.Record.Type.ToString(),
                idempotencyKey = result.Record.IdempotencyKey
            }
        });
    }
}

/// <summary>
/// Request body for POST /api/clocking.
/// Matches the JSON payload sent by clocking-retry.js.
/// </summary>
public class RecordClockingRequest
{
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ClockType ClockType { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}