using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.News;

/// <summary>
/// Edit news page (UC-006: Edit Published News).
/// C2-MAJ-1 fix: BindProperty names match form field names via [BindProperty(Name = ...)].
/// </summary>
[Authorize(Roles = "hr,HR")]
public class EditModel : PageModel
{
    private readonly INewsService _newsService;

    public EditModel(INewsService newsService)
    {
        _newsService = newsService;
    }

    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NewsCategory Category { get; set; }

    public IActionResult OnGet(Guid id)
    {
        var item = _newsService.GetById(id);
        if (item == null)
        {
            TempData["ErrorMessage"] = "News item not found.";
            return RedirectToPage("/News/Management");
        }

        Id = item.Id;
        Title = item.Title;
        Body = item.Body;
        Category = item.Category;
        return Page();
    }

    // C2-MAJ-1 fix: Name parameter matches form field name="title"
    [BindProperty(Name = "title")]
    public string EditTitle { get; set; } = string.Empty;

    // C2-MAJ-1 fix: Name parameter matches form field name="body"
    [BindProperty(Name = "body")]
    public string EditBody { get; set; } = string.Empty;

    // C2-MAJ-1 fix: Name parameter matches form field name="category"
    [BindProperty(Name = "category")]
    public NewsCategory EditCategory { get; set; }

    public IActionResult OnPost(Guid id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        _newsService.Edit(id, EditTitle, EditBody, EditCategory, authorId);
        TempData["SuccessMessage"] = "News item updated successfully.";
        return RedirectToPage("/News/Management");
    }
}