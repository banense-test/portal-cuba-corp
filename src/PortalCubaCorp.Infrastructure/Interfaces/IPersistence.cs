using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Persistence gateway interface (INT-007, COMP-006).
/// Centralizes all database access via EF Core + PostgreSQL.
/// Transaction management via callback pattern (ExecuteInTransactionAsync)
/// rather than exposing DbContext.Database.BeginTransaction() directly.
/// </summary>
public interface IPersistence
{
    // Clocking operations
    List<ClockingRecord> GetClockingsByEmployee(string empId, DateRange range);
    List<ClockingRecord> GetAllClockingsForMonth(DateRange range);
    ClockingRecord InsertClocking(ClockingRecord record);
    ClockingRecord? FindByIdempotencyKey(string key);

    // News operations
    NewsItem? GetNewsItem(Guid id);
    NewsItem SaveNewsItem(NewsItem item);
    NewsItem UpdateNewsItem(Guid id, string title, string body, NewsCategory category);
    NewsItem UpdateNewsStatus(Guid id, NewsStatus status);
    List<NewsItem> GetPublishedNews(NewsCategory? category);
    List<NewsItem> GetFeaturedNews();
    List<NewsItem> GetAllNewsItems();

    // Worker category operations
    WorkerCategory UpsertWorkerCategory(string adUserId, string category);
    List<WorkerCategory> GetAllWorkerCategories();

    // Audit operations
    void InsertAuditRecord(AuditRecord record);

    // Transaction support — callback pattern wraps EF Core transaction
    Task ExecuteInTransactionAsync(Func<Task> action);
}