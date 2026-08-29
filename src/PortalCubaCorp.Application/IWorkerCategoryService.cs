using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Application;

/// <summary>
/// Worker category service interface (INT-004, COMP-004).
/// Manages AD user id → category mapping (CON-009).
/// All changes audited (NFR-004) and wrapped in transactions (C4-2).
/// UC-010: Manage Worker Category.
/// </summary>
public interface IWorkerCategoryService
{
    Task<WorkerCategory> AssignCategoryAsync(string adUserId, string category, string authorId);
    List<WorkerCategory> ListCategories();
    List<DirectoryEntry> LookupAdUser(string query);
}
