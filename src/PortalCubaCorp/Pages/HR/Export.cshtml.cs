using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.HR;

/// <summary>
/// CSV export page model — UC-004 export monthly clocking report.
/// HR-only access. Returns a CSV file download.
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