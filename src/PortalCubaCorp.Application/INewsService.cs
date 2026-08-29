using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Application;

/// <summary>
/// News service interface (INT-002, COMP-003).
/// Manages news lifecycle: publish, edit, unpublish (CON-013 no hard delete).
/// All operations audited (NFR-004) and wrapped in transactions (C4-2).
/// UC-005: Publish News, UC-006: Edit Published News, UC-007: Unpublish News, UC-008: Read and Filter News.
/// </summary>
public interface INewsService
{
    Task<NewsItem> PublishAsync(string title, string body, NewsCategory category, bool isFeatured, string authorId);
    Task<NewsItem> EditAsync(Guid id, string title, string body, NewsCategory category, bool isFeatured, string authorId);
    Task<NewsItem> UnpublishAsync(Guid id, string authorId);
    NewsItem? GetById(Guid id);
    List<NewsItem> GetPublishedNews(NewsCategory? category);
    List<NewsItem> GetFeaturedNews();
    List<NewsItem> ListAll();
}
