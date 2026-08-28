using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages;

/// <summary>
/// Clocking page model (V002) — UC-001 clock in/out + UC-002 view own history.
/// Shows the clocking button and the employee's clocking history for the current month.
/// </summary>
[Authorize]
public class ClockingModel : PageModel
{
    private readonly IClockingService _clockingService;

    public ClockStatus CurrentStatus { get; private set; }
    public string EmployeeId { get; private set; } = string.Empty;
    public List<ClockingRecord> History { get; private set; } = new();

    public ClockingModel(IClockingService clockingService)
    {
        _clockingService = clockingService;
    }

    public void OnGet()
    {
        EmployeeId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? string.Empty;
        CurrentStatus = _clockingService.GetCurrentStatus(EmployeeId);
        History = _clockingService.GetHistory(EmployeeId, DateRange.CurrentMonth());
    }
}