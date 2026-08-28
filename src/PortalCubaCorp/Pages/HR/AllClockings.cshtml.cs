using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.HR;

/// <summary>
/// All clockings page model (V003) — UC-003 view all employee clockings.
/// HR-only access. Shows all employees' clockings for a selected month.
/// CSV export is handled by ExportModel (UC-004).
/// </summary>
[Authorize(Roles = "hr")]
public class AllClockingsModel : PageModel
{
    private readonly IClockingService _clockingService;

    public List<ClockingRecord> Clockings { get; private set; } = new();
    public int Year { get; private set; }
    public int Month { get; private set; }

    public AllClockingsModel(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    public void OnGet(int? year, int? month)
    {
        var now = DateTime.UtcNow;
        Year = year ?? now.Year;
        Month = month ?? now.Month;
        var range = DateRange.ForMonth(Year, Month);
        Clockings = _clockingService.GetAllClockings(range);
    }
}