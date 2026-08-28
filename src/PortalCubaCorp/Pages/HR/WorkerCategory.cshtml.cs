using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.HR;

/// <summary>
/// Worker category page model (V008) — UC-010 manage worker category.
/// HR-only access. Manages AD user id → category mapping (CON-009).
/// All changes audited (NFR-004).
/// </summary>
[Authorize(Roles = "hr")]
public class WorkerCategoryModel : PageModel
{
    private readonly IWorkerCategoryService _workerCategoryService;

    public List<WorkerCategory> Categories { get; private set; } = new();
    public List<DirectoryEntry> SearchResults { get; private set; } = new();
    public string SearchQuery { get; private set; } = string.Empty;

    public WorkerCategoryModel(IWorkerCategoryService workerCategoryService)
    {
        _workerCategoryService = workerCategoryService;
    }

    public void OnGet(string? searchQuery = null)
    {
        SearchQuery = searchQuery ?? string.Empty;
        Categories = _workerCategoryService.ListCategories();
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults = _workerCategoryService.LookupAdUser(SearchQuery);
        }
    }

    public IActionResult OnPostAssign(string adUserId, string category)
    {
        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? string.Empty;
        _workerCategoryService.AssignCategory(adUserId, category, authorId);
        Categories = _workerCategoryService.ListCategories();
        SearchQuery = string.Empty;
        return Page();
    }
}