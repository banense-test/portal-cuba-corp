using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.Clocking;

/// <summary>
/// All employee clockings page (UC-003: View All Employee Clockings, UC-004: Export Monthly Clocking Report).
/// HR-only view of all employees' clockings with CSV export capability.
/// </summary>
[Authorize(Roles = "hr,HR")]
public class AllModel : PageModel
{
    private readonly IClockingService _clockingService;

    public AllModel(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    public List<ClockingRecord> Clockings { get; set; } = new();
    public int Year { get; set; }
    public int Month { get; set; }

    public void OnGet(int? year, int? month, bool? export)
    {
        var now = DateTime.UtcNow;
        Year = year ?? now.Year;
        Month = month ?? now.Month;
        var range = DateRange.ForMonth(Year, Month);

        Clockings = _clockingService.GetAllClockings(range);

        if (export == true)
        {
            var stream = _clockingService.ExportCsv(range);
            var fileName = $"clockings_{Year}_{Month:D2}.csv";
            Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}";
            Response.ContentType = "text/csv";
            stream.CopyTo(Response.Body);
            Response.Body.Flush();
        }
    }
}