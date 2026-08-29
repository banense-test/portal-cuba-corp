using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.WorkerCategory;

[Authorize(Roles = "hr,HR")]
public class ManageModel : PageModel
{
    private readonly IWorkerCategoryService _workerCategoryService;

    public ManageModel(IWorkerCategoryService workerCategoryService)
    {
        _workerCategoryService = workerCategoryService;
    }

    public List<global::PortalCubaCorp.Domain.WorkerCategory> Categories { get; set; } = new();

    public void OnGet()
    {
        Categories = _workerCategoryService.ListCategories();
    }

    public IActionResult OnPostAssign(string adUserId, string category)
    {
        if (string.IsNullOrWhiteSpace(adUserId) || string.IsNullOrWhiteSpace(category))
        {
            TempData["ErrorMessage"] = "AD User ID and Category are required.";
            return RedirectToPage();
        }

        var authorId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
        _workerCategoryService.AssignCategory(adUserId, category, authorId);
        TempData["SuccessMessage"] = $"Category '{category}' assigned to {adUserId}.";
        return RedirectToPage();
    }
}