using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.News;

/// <summary>
/// News management page (UC-005..UC-007: Publish, Edit, Unpublish).
/// C4-2: OnPostUnpublishAsync calls UnpublishAsync (transaction-wrapped).
/// </summary>
[Authorize(Roles = "hr,HR")]
public class ManagementModel : PageModel
{
    private readonly INewsService _newsService;

    public ManagementModel(INewsService newsService)
    {
        _newsService = newsService;
    }

    public List<NewsItem> AllNews { get; set; } = new();

    public void OnGet()
    {
        AllNews = _newsService.ListAll();
    }

    public async Task<IActionResult> OnPostUnpublishAsync(Guid id)
    {
        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        await _newsService.UnpublishAsync(id, authorId);
        TempData["SuccessMessage"] = "News item unpublished. Record preserved for audit trail.";
        return RedirectToPage();
    }
}
