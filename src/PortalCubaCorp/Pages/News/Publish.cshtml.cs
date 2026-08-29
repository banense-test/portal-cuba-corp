using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.News;

/// <summary>
/// Publish news page (UC-005: Publish News).
/// C4-2: OnPostAsync calls PublishAsync (transaction-wrapped).
/// </summary>
[Authorize(Roles = "hr,HR")]
public class PublishModel : PageModel
{
    private readonly INewsService _newsService;

    public PublishModel(INewsService newsService)
    {
        _newsService = newsService;
    }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string Body { get; set; } = string.Empty;

    [BindProperty]
    public NewsCategory Category { get; set; } = NewsCategory.General;

    [BindProperty]
    public bool IsFeatured { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        await _newsService.PublishAsync(Title, Body, Category, IsFeatured, authorId);
        TempData["SuccessMessage"] = "News item published successfully.";
        return RedirectToPage("/News/Management");
    }
}
