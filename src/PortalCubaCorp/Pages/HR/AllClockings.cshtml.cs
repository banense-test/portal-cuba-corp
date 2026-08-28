using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.HR;

/// <summary>
/// All clockings page model (V003) — UC-003 view all employee clockings + UC-004 CSV export.
/// HR-only access. Shows all employees' clockings for a selected month.
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

/// <summary>
/// CSV export handler — UC-004 export monthly clocking report.
/// Returns a CSV file download.
/// </summary>
[Authorize(Roles = "hr")]
public class ExportModel : PageModel
{
    private readonly IClockingService _clockingService;

    public ExportModel(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    public IActionResult OnGet(int year, int month)
    {
        var range = DateRange.ForMonth(year, month);
        var stream = _clockingService.ExportCsv(range);
        var fileName = $"clockings-{year}-{month:D2}.csv";
        return File(stream, "text/csv", fileName);
    }
}