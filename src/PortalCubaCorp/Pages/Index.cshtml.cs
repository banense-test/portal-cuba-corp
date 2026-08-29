using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages;

/// <summary>
/// Main page (UC-001: Clock In/Out status, UC-008: Read and Filter News).
/// Shows clocking button and news feed on the home page.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IClockingService _clockingService;
    private readonly INewsService _newsService;

    public IndexModel(IClockingService clockingService, INewsService newsService)
    {
        _clockingService = clockingService;
        _newsService = newsService;
    }

    public ClockStatus CurrentStatus { get; set; }
    public List<NewsItem> News { get; set; } = new();
    public List<NewsItem> FeaturedNews { get; set; } = new();
    public NewsCategory? SelectedCategory { get; set; }
    public string EmployeeId { get; set; } = string.Empty;

    public void OnGet(string? category = null)
    {
        EmployeeId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        CurrentStatus = _clockingService.GetCurrentStatus(EmployeeId);

        if (Enum.TryParse<NewsCategory>(category, out var cat))
        {
            SelectedCategory = cat;
            News = _newsService.GetPublishedNews(cat);
        }
        else
        {
            News = _newsService.GetPublishedNews(null);
        }

        FeaturedNews = _newsService.GetFeaturedNews();
    }
}