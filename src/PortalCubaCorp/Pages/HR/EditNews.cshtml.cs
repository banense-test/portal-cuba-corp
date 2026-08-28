using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.HR;

/// <summary>
/// Edit news page model (V005) — UC-006 edit published news.
/// HR-only access. Every edit is audited (NFR-004).
/// </summary>
[Authorize(Roles = "hr")]
public class EditNewsModel : PageModel
{
    private readonly INewsService _newsService;

    public NewsItem? NewsItem { get; private set; }
    public List<NewsCategory> Categories { get; } = Enum.GetValues<NewsCategory>().ToList();
    public string? Message { get; private set; }

    public EditNewsModel(INewsService newsService)
    {
        _newsService = newsService;
    }

    public void OnGet(Guid id)
    {
        NewsItem = _newsService.GetById(id);
    }

    public IActionResult OnPost(Guid id, string title, string body, NewsCategory category)
    {
        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? string.Empty;
        var updated = _newsService.Edit(id, title, body, category, authorId);
        NewsItem = updated;
        Message = "News item updated successfully.";
        return Page();
    }
}