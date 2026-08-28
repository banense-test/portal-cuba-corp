using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;

namespace PortalCubaCorp.Application;

/// <summary>
/// Directory service implementation (COMP-001).
/// UC-009: Search employee directory from AD via LDAP (CON-005).
/// Missing attributes default to "N/A" (R001 fallback).
/// Corporate data only — no private personal information (CON-012).
/// </summary>
public class DirectoryService : IDirectoryService
{
    private readonly ILdapGateway _ldapGateway;

    public DirectoryService(ILdapGateway ldapGateway)
    {
        _ldapGateway = ldapGateway;
    }

    public List<DirectoryEntry> Search(string query, string? office = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<DirectoryEntry>();

        // Build LDAP filter for name, department, or office search
        var escapedQuery = EscapeLdapFilter(query);
        var filter = $"(|(cn=*{escapedQuery}*)(department=*{escapedQuery}*)(physicalDeliveryOfficeName=*{escapedQuery}*))";

        // If office filter is specified, add it as an AND condition (MINOR-1 fix)
        if (!string.IsNullOrWhiteSpace(office))
        {
            var escapedOffice = EscapeLdapFilter(office);
            filter = $"(&{filter}(physicalDeliveryOfficeName=*{escapedOffice}*))";
        }

        var results = _ldapGateway.SearchEntries(filter);

        // Map to DirectoryEntry with R001 fallback (missing attributes → "N/A")
        return results
            .Select(r => DirectoryEntry.FromLdapAttributes(
                r.AdUserId,
                r.DisplayName,
                r.JobTitle,
                r.Department,
                r.Office,
                r.Email,
                r.Extension))
            .ToList();
    }

    private static string EscapeLdapFilter(string value)
    {
        return value.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");
    }
}
