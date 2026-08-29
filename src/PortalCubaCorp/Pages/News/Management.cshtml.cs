using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.News;

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

    public IActionResult OnPostUnpublish(Guid id)
    {
        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        _newsService.Unpublish(id, authorId);
        TempData["SuccessMessage"] = "News item unpublished. Record preserved for audit trail.";
        return RedirectToPage();
    }
}