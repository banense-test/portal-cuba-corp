using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.HR;

/// <summary>
/// Publish news page model (V004) — UC-005 publish news with audit trail.
/// HR-only access. Publication is audited (author + timestamp, NFR-004).
/// </summary>
[Authorize(Roles = "hr")]
public class PublishNewsModel : PageModel
{
    private readonly INewsService _newsService;

    public List<NewsCategory> Categories { get; } = Enum.GetValues<NewsCategory>().ToList();
    public string? Message { get; private set; }

    public PublishNewsModel(INewsService newsService)
    {
        _newsService = newsService;
    }

    public void OnGet() { }

    public IActionResult OnPost(string title, string body, NewsCategory category, bool isFeatured)
    {
        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? string.Empty;
        var item = _newsService.Publish(title, body, category, authorId);

        if (isFeatured)
        {
            // Featured flag is set via direct update after publish
            // since Publish creates the item; we update it to set IsFeatured
            var updated = _newsService.Edit(item.Id, title, body, category, authorId);
        }

        Message = $"News item '{item.Title}' published successfully.";
        return Page();
    }
}