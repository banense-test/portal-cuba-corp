using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Application;

/// <summary>
/// News service interface (INT-002, COMP-003).
/// Manages news lifecycle: publish, edit, unpublish (CON-013 no hard delete).
/// All operations audited (NFR-004).
/// </summary>
public interface INewsService
{
    NewsItem Publish(string title, string body, NewsCategory category, string authorId);
    NewsItem Edit(Guid id, string title, string body, NewsCategory category, string authorId);
    NewsItem Unpublish(Guid id, string authorId);
    NewsItem? GetById(Guid id);
    List<NewsItem> GetPublishedNews(NewsCategory? category);
    List<NewsItem> GetFeaturedNews();
    List<NewsItem> ListAll();
}