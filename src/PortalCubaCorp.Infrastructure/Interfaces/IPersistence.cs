using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Persistence gateway interface (INT-007, COMP-006).
/// Centralizes all database access via EF Core + PostgreSQL.
/// Transaction management via callback pattern (ExecuteInTransactionAsync)
/// rather than exposing DbContext.Database.BeginTransaction() directly.
/// UC-001..UC-004: Clocking operations. UC-005..UC-008: News operations. UC-010: Worker category.
/// </summary>
public interface IPersistence
{
    // Clocking operations (UC-001..UC-004)
    List<ClockingRecord> GetClockingsByEmployee(string empId, DateRange range);
    List<ClockingRecord> GetAllClockingsForMonth(DateRange range);
    ClockingRecord InsertClocking(ClockingRecord record);
    ClockingRecord? FindByIdempotencyKey(string employeeId, string key);

    // News operations (UC-005..UC-008)
    NewsItem? GetNewsItem(Guid id);
    NewsItem SaveNewsItem(NewsItem item);
    NewsItem UpdateNewsItem(Guid id, string title, string body, NewsCategory category, bool isFeatured);
    NewsItem UpdateNewsStatus(Guid id, NewsStatus status);
    List<NewsItem> GetPublishedNews(NewsCategory? category);
    List<NewsItem> GetFeaturedNews();
    List<NewsItem> GetAllNewsItems();

    // Worker category operations (UC-010)
    WorkerCategory UpsertWorkerCategory(string adUserId, string category);
    List<WorkerCategory> GetAllWorkerCategories();

    // Audit operations (NFR-004)
    void InsertAuditRecord(AuditRecord record);

    // Transaction support — callback pattern wraps EF Core transaction (INT-007)
    Task ExecuteInTransactionAsync(Func<Task> action);
}
