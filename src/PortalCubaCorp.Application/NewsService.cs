using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;

namespace PortalCubaCorp.Application;

/// <summary>
/// News service implementation (COMP-003).
/// UC-005: Publish news with audit trail (NFR-004).
/// UC-006: Edit published news — update in place, audit every edit. isFeatured included (C4-1, CR-010).
/// UC-007: Unpublish news — hide, never delete (CON-013).
/// UC-008: Read and filter news — employees see published only.
/// All write operations wrapped in ExecuteInTransactionAsync for atomicity (C4-2, NFR-004).
/// </summary>
public class NewsService : INewsService
{
    private readonly IPersistence _persistence;
    private readonly IAuditLogger _auditLogger;

    public NewsService(IPersistence persistence, IAuditLogger auditLogger)
    {
        _persistence = persistence;
        _auditLogger = auditLogger;
    }

    public async Task<NewsItem> PublishAsync(string title, string body, NewsCategory category, bool isFeatured, string authorId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required", nameof(body));

        var now = DateTime.UtcNow;
        var item = new NewsItem
        {
            Title = title,
            Body = body,
            Category = category,
            IsFeatured = isFeatured,
            Status = NewsStatus.Published,
            CreatedAt = now,
            UpdatedAt = now,
            AuthorId = authorId
        };

        // C4-2: Wrap business op + audit in a transaction (NFR-004)
        NewsItem savedItem = item;
        await _persistence.ExecuteInTransactionAsync(() =>
        {
            savedItem = _persistence.SaveNewsItem(item);
            _auditLogger.LogAudit("NEWS_ITEM", savedItem.Id.ToString(), AuditAction.Publish, authorId, now);
            return Task.CompletedTask;
        });

        return savedItem;
    }

    public async Task<NewsItem> EditAsync(Guid id, string title, string body, NewsCategory category, bool isFeatured, string authorId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required", nameof(body));

        var existing = _persistence.GetNewsItem(id)
            ?? throw new InvalidOperationException($"NewsItem {id} not found");

        // C4-1: Pass isFeatured to UpdateNewsItem (CR-010)
        // C4-2: Wrap business op + audit in a transaction (NFR-004)
        NewsItem updated = existing;
        var now = DateTime.UtcNow;
        await _persistence.ExecuteInTransactionAsync(() =>
        {
            updated = _persistence.UpdateNewsItem(id, title, body, category, isFeatured);
            _auditLogger.LogAudit("NEWS_ITEM", id.ToString(), AuditAction.Edit, authorId, now);
            return Task.CompletedTask;
        });

        return updated;
    }

    public async Task<NewsItem> UnpublishAsync(Guid id, string authorId)
    {
        var existing = _persistence.GetNewsItem(id)
            ?? throw new InvalidOperationException($"NewsItem {id} not found");

        // Set status to Unpublished — record preserved, NOT deleted (CON-013)
        // C4-2: Wrap business op + audit in a transaction (NFR-004)
        NewsItem updated = existing;
        var now = DateTime.UtcNow;
        await _persistence.ExecuteInTransactionAsync(() =>
        {
            updated = _persistence.UpdateNewsStatus(id, NewsStatus.Unpublished);
            _auditLogger.LogAudit("NEWS_ITEM", id.ToString(), AuditAction.Unpublish, authorId, now);
            return Task.CompletedTask;
        });

        return updated;
    }

    public NewsItem? GetById(Guid id)
    {
        return _persistence.GetNewsItem(id);
    }

    public List<NewsItem> GetPublishedNews(NewsCategory? category)
    {
        return _persistence.GetPublishedNews(category);
    }

    public List<NewsItem> GetFeaturedNews()
    {
        return _persistence.GetFeaturedNews();
    }

    public List<NewsItem> ListAll()
    {
        return _persistence.GetAllNewsItems();
    }
}
