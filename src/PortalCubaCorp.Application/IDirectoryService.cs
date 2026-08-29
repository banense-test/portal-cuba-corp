using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Application;

/// <summary>
/// Directory service interface (INT-003, COMP-001).
/// Read-only employee directory search from AD via LDAP (CON-005, CON-012).
/// Missing attributes default to "N/A" (R001 fallback).
/// </summary>
public interface IDirectoryService
{
    List<DirectoryEntry> Search(string query, string? office = null);
}
