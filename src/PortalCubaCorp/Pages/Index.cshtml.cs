using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages;

/// <summary>
/// Main page model (V001) — UC-001 clocking button + UC-008 news reading.
/// Shows clock in/out button based on current status and published news with category filter.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IClockingService _clockingService;
    private readonly INewsService _newsService;

    public ClockStatus CurrentStatus { get; private set; }
    public string EmployeeId { get; private set; } = string.Empty;
    public List<NewsItem> News { get; private set; } = new();
    public List<NewsItem> FeaturedNews { get; private set; } = new();
    public NewsCategory? SelectedCategory { get; private set; }
    public List<NewsCategory> Categories { get; } = Enum.GetValues<NewsCategory>().ToList();

    public IndexModel(IClockingService clockingService, INewsService newsService)
    {
        _clockingService = clockingService;
        _newsService = newsService;
    }

    public void OnGet(NewsCategory? category = null)
    {
        EmployeeId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? string.Empty;
        CurrentStatus = _clockingService.GetCurrentStatus(EmployeeId);
        SelectedCategory = category;
        News = _newsService.GetPublishedNews(category);
        FeaturedNews = _newsService.GetFeaturedNews();
    }
}