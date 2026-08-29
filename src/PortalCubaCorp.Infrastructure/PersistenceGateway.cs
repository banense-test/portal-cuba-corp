using Microsoft.EntityFrameworkCore;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// EF Core implementation of IPersistence (COMP-006).
/// All database access centralized through this gateway.
/// </summary>
public class PersistenceGateway : IPersistence
{
    private readonly PortalDbContext _db;

    public PersistenceGateway(PortalDbContext db)
    {
        _db = db;
    }

    // Clocking operations

    public List<ClockingRecord> GetClockingsByEmployee(string empId, DateRange range)
    {
        return _db.Clockings
            .Where(c => c.EmployeeId == empId && c.Timestamp >= range.Start && c.Timestamp <= range.End)
            .OrderByDescending(c => c.Timestamp)
            .ToList();
    }

    public List<ClockingRecord> GetAllClockingsForMonth(DateRange range)
    {
        return _db.Clockings
            .Where(c => c.Timestamp >= range.Start && c.Timestamp <= range.End)
            .OrderBy(c => c.EmployeeId).ThenByDescending(c => c.Timestamp)
            .ToList();
    }

    public ClockingRecord InsertClocking(ClockingRecord record)
    {
        _db.Clockings.Add(record);
        _db.SaveChanges();
        return record;
    }

    public ClockingRecord? FindByIdempotencyKey(string employeeId, string key)
    {
        return _db.Clockings.FirstOrDefault(c => c.EmployeeId == employeeId && c.IdempotencyKey == key);
    }

    // News operations

    public NewsItem? GetNewsItem(Guid id)
    {
        return _db.NewsItems.FirstOrDefault(n => n.Id == id);
    }

    public NewsItem SaveNewsItem(NewsItem item)
    {
        _db.NewsItems.Add(item);
        _db.SaveChanges();
        return item;
    }

    public NewsItem UpdateNewsItem(Guid id, string title, string body, NewsCategory category)
    {
        var item = _db.NewsItems.FirstOrDefault(n => n.Id == id)
            ?? throw new InvalidOperationException(string.Format("NewsItem {0} not found", id));
        item.Title = title;
        item.Body = body;
        item.Category = category;
        item.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return item;
    }

    public NewsItem UpdateNewsStatus(Guid id, NewsStatus status)
    {
        var item = _db.NewsItems.FirstOrDefault(n => n.Id == id)
            ?? throw new InvalidOperationException(string.Format("NewsItem {0} not found", id));
        item.Status = status;
        item.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return item;
    }

    public List<NewsItem> GetPublishedNews(NewsCategory? category)
    {
        var query = _db.NewsItems.Where(n => n.Status == NewsStatus.Published);
        if (category.HasValue)
            query = query.Where(n => n.Category == category.Value);
        return query.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public List<NewsItem> GetFeaturedNews()
    {
        return _db.NewsItems
            .Where(n => n.Status == NewsStatus.Published && n.IsFeatured)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    public List<NewsItem> GetAllNewsItems()
    {
        return _db.NewsItems.OrderByDescending(n => n.CreatedAt).ToList();
    }

    // Worker category operations

    public WorkerCategory UpsertWorkerCategory(string adUserId, string category)
    {
        var existing = _db.WorkerCategories.FirstOrDefault(wc => wc.AdUserId == adUserId);
        if (existing != null)
        {
            existing.Category = category;
        }
        else
        {
            _db.WorkerCategories.Add(new WorkerCategory { AdUserId = adUserId, Category = category });
        }
        _db.SaveChanges();
        return existing ?? new WorkerCategory { AdUserId = adUserId, Category = category };
    }

    public List<WorkerCategory> GetAllWorkerCategories()
    {
        return _db.WorkerCategories.ToList();
    }

    // Audit operations

    public void InsertAuditRecord(AuditRecord record)
    {
        _db.AuditRecords.Add(record);
        _db.SaveChanges();
    }

    // Transaction support — callback pattern (INT-007 corrected)

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
