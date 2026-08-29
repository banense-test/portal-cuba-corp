using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;

namespace PortalCubaCorp.Application;

/// <summary>
/// News service implementation (COMP-003).
/// UC-005: Publish news with audit trail (NFR-004).
/// UC-006: Edit published news — update in place, audit every edit.
/// UC-007: Unpublish news — hide, never delete (CON-013).
/// UC-008: Read and filter news — employees see published only.
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

    public NewsItem Publish(string title, string body, NewsCategory category, bool isFeatured, string authorId)
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

        _persistence.SaveNewsItem(item);

        // Audit trail (NFR-004)
        _auditLogger.LogAudit("NEWS_ITEM", item.Id.ToString(), AuditAction.Publish, authorId, now);

        return item;
    }

    public NewsItem Edit(Guid id, string title, string body, NewsCategory category, string authorId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required", nameof(body));

        var existing = _persistence.GetNewsItem(id)
            ?? throw new InvalidOperationException($"NewsItem {id} not found");

        var updated = _persistence.UpdateNewsItem(id, title, body, category);

        // Audit trail (NFR-004)
        _auditLogger.LogAudit("NEWS_ITEM", id.ToString(), AuditAction.Edit, authorId, DateTime.UtcNow);

        return updated;
    }

    public NewsItem Unpublish(Guid id, string authorId)
    {
        var existing = _persistence.GetNewsItem(id)
            ?? throw new InvalidOperationException($"NewsItem {id} not found");

        // Set status to Unpublished — record preserved, NOT deleted (CON-013)
        var updated = _persistence.UpdateNewsStatus(id, NewsStatus.Unpublished);

        // Audit trail (NFR-004)
        _auditLogger.LogAudit("NEWS_ITEM", id.ToString(), AuditAction.Unpublish, authorId, DateTime.UtcNow);

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
