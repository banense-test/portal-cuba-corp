using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using System.Text.Json.Serialization;

namespace PortalCubaCorp.Pages.Api;

/// <summary>
/// Clocking API endpoint (UC-001: Clock In/Out).
/// Route: /api/clocking — matches JS fetch('/api/clocking') (C2-CRIT-1 fix).
/// [IgnoreAntiforgeryToken]: OIDC bearer auth + idempotency key provide replay protection (C2-MAJ-2 fix).
/// EmployeeId derived from OIDC token sub claim, NOT request body (C2-MIN-2 fix).
/// </summary>
[Authorize]
[IgnoreAntiforgeryToken]
public class ClockingApiModel : PageModel
{
    private readonly IClockingService _clockingService;

    public ClockingApiModel(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    public class ClockingRequest
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("clockType")]
        public string ClockType { get; set; } = string.Empty;

        [JsonPropertyName("idempotencyKey")]
        public string IdempotencyKey { get; set; } = string.Empty;
    }

    public class ClockingResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("isDuplicate")]
        public bool IsDuplicate { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public IActionResult OnPost()
    {
        var body = Request.Body;
        using var reader = new StreamReader(body);
        var json = reader.ReadToEnd();

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var request = System.Text.Json.JsonSerializer.Deserialize<ClockingRequest>(json, options);
        if (request == null)
        {
            return new JsonResult(new ClockingResponse { Success = false, Error = "Invalid request" });
        }

        if (!Enum.TryParse<ClockType>(request.ClockType, true, out var clockType))
        {
            return new JsonResult(new ClockingResponse { Success = false, Error = "Invalid clock type" });
        }

        // C2-MIN-2 fix: derive employeeId from OIDC token, not request body
        var employeeId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";

        var result = _clockingService.RecordClocking(
            employeeId,
            request.Timestamp,
            clockType,
            request.IdempotencyKey);

        return new JsonResult(new ClockingResponse
        {
            Success = result.Success,
            IsDuplicate = result.IsDuplicate,
            Error = result.Error
        });
    }
}