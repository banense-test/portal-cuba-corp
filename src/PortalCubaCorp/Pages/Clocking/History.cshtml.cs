using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.Clocking;

/// <summary>
/// Clocking history page (UC-002: View Own Clocking History).
/// Employee views their clocking history for the current month.
/// </summary>
[Authorize]
public class HistoryModel : PageModel
{
    private readonly IClockingService _clockingService;

    public HistoryModel(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    public List<ClockingRecord> Clockings { get; set; } = new();
    public string MonthDisplay { get; set; } = string.Empty;

    public void OnGet(int? year, int? month)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var range = DateRange.ForMonth(y, m);

        var employeeId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        Clockings = _clockingService.GetHistory(employeeId, range);
        MonthDisplay = new DateTime(y, m, 1).ToString("MMMM yyyy");
    }
}