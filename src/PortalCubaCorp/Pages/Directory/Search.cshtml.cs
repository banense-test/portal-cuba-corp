using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages.Directory;

/// <summary>
/// Employee directory search page (UC-009: Search Employee Directory).
/// Searches AD via LDAP for colleagues by name, department, or office (CON-005).
/// Read-only corporate data only (CON-012).
/// </summary>
[Authorize]
public class SearchModel : PageModel
{
    private readonly IDirectoryService _directoryService;

    public SearchModel(IDirectoryService directoryService)
    {
        _directoryService = directoryService;
    }

    public List<DirectoryEntry> Results { get; set; } = new();
    public string Query { get; set; } = string.Empty;
    public string? OfficeFilter { get; set; }

    public void OnGet(string? query, string? office)
    {
        Query = query ?? string.Empty;
        OfficeFilter = office;

        if (!string.IsNullOrWhiteSpace(query))
        {
            Results = _directoryService.Search(query, office);
        }
    }
}