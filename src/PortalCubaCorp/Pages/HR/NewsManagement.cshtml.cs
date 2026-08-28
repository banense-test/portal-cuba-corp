using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.HR;

/// <summary>
/// News management page model (V006) — UC-007 unpublish news.
/// HR-only access. Unpublishing hides the item but never deletes it (CON-013).
/// Also lists all news items for management overview.
/// </summary>
[Authorize(Roles = "hr")]
public class NewsManagementModel : PageModel
{
    private readonly INewsService _newsService;

    public List<NewsItem> AllNews { get; private set; } = new();

    public NewsManagementModel(INewsService newsService)
    {
        _newsService = newsService;
    }

    public void OnGet()
    {
        AllNews = _newsService.ListAll();
    }

    public IActionResult OnPostUnpublish(Guid id)
    {
        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? string.Empty;
        _newsService.Unpublish(id, authorId);
        AllNews = _newsService.ListAll();
        return Page();
    }
}