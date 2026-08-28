using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Pages;

/// <summary>
/// Directory search page model (V007) — UC-009 search employee directory.
/// Read-only search from AD via LDAP (CON-005, CON-012).
/// Missing attributes default to "N/A" (R001 fallback).
/// </summary>
[Authorize]
public class DirectoryModel : PageModel
{
    private readonly IDirectoryService _directoryService;

    public string Query { get; private set; } = string.Empty;
    public List<DirectoryEntry> Results { get; private set; } = new();

    public DirectoryModel(IDirectoryService directoryService)
    {
        _directoryService = directoryService;
    }

    public void OnGet(string? query = null)
    {
        Query = query ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(Query))
        {
            Results = _directoryService.Search(Query);
        }
    }
}