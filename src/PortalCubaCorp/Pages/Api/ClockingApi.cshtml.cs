using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using System.Text.Json.Serialization;

namespace PortalCubaCorp.Pages.Api;

[Authorize]
public class ClockingApiModel : PageModel
{
    private readonly IClockingService _clockingService;

    public ClockingApiModel(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    public class ClockingRequest
    {
        [JsonPropertyName("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

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

        var result = _clockingService.RecordClocking(
            request.EmployeeId,
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
