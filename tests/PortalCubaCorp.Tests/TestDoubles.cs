using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;

namespace PortalCubaCorp.Tests;

/// <summary>
/// In-memory persistence implementation for unit testing.
/// Simulates the PostgreSQL database without requiring a real DB instance.
/// UC-001..UC-004: Clocking. UC-005..UC-008: News. UC-010: Worker category.
/// </summary>
public class InMemoryPersistence : IPersistence
{
    private readonly List<ClockingRecord> _clockings = new();
    private readonly List<NewsItem> _newsItems = new();
    private readonly List<WorkerCategory> _workerCategories = new();
    private readonly List<AuditRecord> _auditRecords = new();

    public List<ClockingRecord> GetClockingsByEmployee(string empId, DateRange range)
    {
        return _clockings
            .Where(c => c.EmployeeId == empId && c.Timestamp >= range.Start && c.Timestamp <= range.End)
            .OrderByDescending(c => c.Timestamp)
            .ToList();
    }

    public List<ClockingRecord> GetAllClockingsForMonth(DateRange range)
    {
        return _clockings
            .Where(c => c.Timestamp >= range.Start && c.Timestamp <= range.End)
            .OrderBy(c => c.EmployeeId).ThenByDescending(c => c.Timestamp)
            .ToList();
    }

    public ClockingRecord InsertClocking(ClockingRecord record)
    {
        record.Id = _clockings.Count + 1;
        _clockings.Add(record);
        return record;
    }

    public ClockingRecord? FindByIdempotencyKey(string employeeId, string key)
    {
        return _clockings.FirstOrDefault(c => c.EmployeeId == employeeId && c.IdempotencyKey == key);
    }

    public NewsItem? GetNewsItem(Guid id)
    {
        return _newsItems.FirstOrDefault(n => n.Id == id);
    }

    public NewsItem SaveNewsItem(NewsItem item)
    {
        if (item.Id == Guid.Empty)
            item.Id = Guid.NewGuid();
        _newsItems.Add(item);
        return item;
    }

    // C4-1: isFeatured parameter added (CR-010)
    public NewsItem UpdateNewsItem(Guid id, string title, string body, NewsCategory category, bool isFeatured)
    {
        var item = _newsItems.FirstOrDefault(n => n.Id == id)
            ?? throw new InvalidOperationException(string.Format("NewsItem {0} not found", id));
        item.Title = title;
        item.Body = body;
        item.Category = category;
        item.IsFeatured = isFeatured;
        item.UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public NewsItem UpdateNewsStatus(Guid id, NewsStatus status)
    {
        var item = _newsItems.FirstOrDefault(n => n.Id == id)
            ?? throw new InvalidOperationException(string.Format("NewsItem {0} not found", id));
        item.Status = status;
        item.UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public List<NewsItem> GetPublishedNews(NewsCategory? category)
    {
        var query = _newsItems.Where(n => n.Status == NewsStatus.Published);
        if (category.HasValue)
            query = query.Where(n => n.Category == category.Value);
        return query.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public List<NewsItem> GetFeaturedNews()
    {
        return _newsItems
            .Where(n => n.Status == NewsStatus.Published && n.IsFeatured)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    public List<NewsItem> GetAllNewsItems()
    {
        return _newsItems.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public WorkerCategory UpsertWorkerCategory(string adUserId, string category)
    {
        var existing = _workerCategories.FirstOrDefault(wc => wc.AdUserId == adUserId);
        if (existing != null)
        {
            existing.Category = category;
            return existing;
        }
        var newEntry = new WorkerCategory { AdUserId = adUserId, Category = category };
        _workerCategories.Add(newEntry);
        return newEntry;
    }

    public List<WorkerCategory> GetAllWorkerCategories()
    {
        return _workerCategories.ToList();
    }

    public void InsertAuditRecord(AuditRecord record)
    {
        record.Id = _auditRecords.Count + 1;
        _auditRecords.Add(record);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        // In-memory test double — just execute the action directly
        await action();
    }
}

/// <summary>
/// In-memory audit logger for unit testing.
/// Captures audit records for verification in tests.
/// </summary>
public class InMemoryAuditLogger : IAuditLogger
{
    public List<AuditRecord> Records { get; } = new();

    public void LogAudit(string entityType, string entityId, AuditAction action, string author, DateTime timestamp)
    {
        Records.Add(new AuditRecord
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Author = author,
            Timestamp = timestamp
        });
    }
}

/// <summary>
/// Mock LDAP gateway for unit testing.
/// Returns pre-configured entries regardless of filter (unit tests verify mapping logic, not LDAP filtering).
/// </summary>
public class MockLdapGateway : ILdapGateway
{
    public List<LdapSearchResult> Entries { get; } = new();

    public List<LdapSearchResult> SearchEntries(string filter)
    {
        return Entries.ToList();
    }

    public LdapSearchResult? GetEntryByUserId(string adUserId)
    {
        return Entries.FirstOrDefault(e => e.AdUserId == adUserId);
    }

    public Dictionary<string, string> ResolveNames(List<string> adUserIds)
    {
        return adUserIds.ToDictionary(
            id => id,
            id => Entries.FirstOrDefault(e => e.AdUserId == id)?.DisplayName ?? id);
    }
}