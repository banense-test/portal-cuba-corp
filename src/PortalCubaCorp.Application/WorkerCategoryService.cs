using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;

namespace PortalCubaCorp.Application;

/// <summary>
/// Worker category service implementation (COMP-004).
/// UC-010: Manage worker category — AD user id to category (CON-009).
/// Bridges local DB (worker_categories) and LDAP (AD lookup).
/// All changes audited (NFR-004).
/// </summary>
public class WorkerCategoryService : IWorkerCategoryService
{
    private readonly IPersistence _persistence;
    private readonly ILdapGateway _ldapGateway;
    private readonly IAuditLogger _auditLogger;

    public WorkerCategoryService(IPersistence persistence, ILdapGateway ldapGateway, IAuditLogger auditLogger)
    {
        _persistence = persistence;
        _ldapGateway = ldapGateway;
        _auditLogger = auditLogger;
    }

    public WorkerCategory AssignCategory(string adUserId, string category, string authorId)
    {
        if (string.IsNullOrWhiteSpace(adUserId))
            throw new ArgumentException("AD user ID is required", nameof(adUserId));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category is required", nameof(category));

        // Upsert worker category — only 2 columns (CON-009)
        var result = _persistence.UpsertWorkerCategory(adUserId, category);

        // Audit trail (NFR-004)
        _auditLogger.LogAudit("WORKER_CATEGORY", adUserId, AuditAction.CategoryChanged, authorId, DateTime.UtcNow);

        return result;
    }

    public List<WorkerCategory> ListCategories()
    {
        return _persistence.GetAllWorkerCategories();
    }

    public List<DirectoryEntry> LookupAdUser(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<DirectoryEntry>();

        var escapedQuery = EscapeLdapFilter(query);
        var filter = string.Format("(|(cn=*{0}*)(sAMAccountName=*{0}*))", escapedQuery);

        var results = _ldapGateway.SearchEntries(filter);

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
        return value
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }
}