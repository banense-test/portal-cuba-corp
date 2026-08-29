using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.News;

/// <summary>
/// Edit news page (UC-006: Edit Published News).
/// C2-MAJ-1 fix: BindProperty names match form field names via [BindProperty(Name = ...)].
/// C4-1: isFeatured checkbox added to edit form (CR-010).
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
    public bool IsFeatured { get; set; }

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
        IsFeatured = item.IsFeatured;
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

    // C4-1: isFeatured bindable from edit form (CR-010)
    [BindProperty(Name = "isFeatured")]
    public bool EditIsFeatured { get; set; }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        await _newsService.EditAsync(id, EditTitle, EditBody, EditCategory, EditIsFeatured, authorId);
        TempData["SuccessMessage"] = "News item updated successfully.";
        return RedirectToPage("/News/Management");
    }
}
