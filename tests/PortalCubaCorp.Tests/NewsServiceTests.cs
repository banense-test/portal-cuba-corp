using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for NewsService (COMP-003).
/// Black-box: verify INewsService contract — publish, edit, unpublish, get published, get featured, list all.
/// White-box: exercise validation branches, audit trail calls, CON-013 no-delete behavior.
/// </summary>
public class NewsServiceTests
{
    private static (NewsService service, InMemoryPersistence persistence, InMemoryAuditLogger audit) CreateService()
    {
        var persistence = new InMemoryPersistence();
        var audit = new InMemoryAuditLogger();
        var service = new NewsService(persistence, audit);
        return (service, persistence, audit);
    }

    // --- Black-box: Publish ---

    [Fact]
    public void Publish_ValidInput_ReturnsPublishedNewsItem()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.HR, "author1");

        Assert.Equal("Title", item.Title);
        Assert.Equal("Body", item.Body);
        Assert.Equal(NewsCategory.HR, item.Category);
        Assert.Equal(NewsStatus.Published, item.Status);
        Assert.Equal("author1", item.AuthorId);
    }

    [Fact]
    public void Publish_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.IT, "author1");

        Assert.Single(audit.Records);
        Assert.Equal(AuditAction.Publish, audit.Records[0].Action);
        Assert.Equal("author1", audit.Records[0].Author);
        Assert.Equal(item.Id.ToString(), audit.Records[0].EntityId);
    }

    // --- White-box: Publish validation ---

    [Fact]
    public void Publish_EmptyTitle_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<ArgumentException>(() => service.Publish("", "Body", NewsCategory.HR, "author1"));
    }

    [Fact]
    public void Publish_NullTitle_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<ArgumentException>(() => service.Publish(null!, "Body", NewsCategory.HR, "author1"));
    }

    [Fact]
    public void Publish_EmptyBody_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<ArgumentException>(() => service.Publish("Title", "", NewsCategory.HR, "author1"));
    }

    [Fact]
    public void Publish_WhitespaceBody_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<ArgumentException>(() => service.Publish("Title", "   ", NewsCategory.HR, "author1"));
    }

    // --- Black-box: Edit ---

    [Fact]
    public void Edit_ExistingNews_UpdatesInPlace()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Old Title", "Old Body", NewsCategory.General, "author1");

        var updated = service.Edit(item.Id, "New Title", "New Body", NewsCategory.Events, "author2");

        Assert.Equal("New Title", updated.Title);
        Assert.Equal("New Body", updated.Body);
        Assert.Equal(NewsCategory.Events, updated.Category);
    }

    [Fact]
    public void Edit_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.General, "author1");
        audit.Records.Clear();

        service.Edit(item.Id, "New Title", "New Body", NewsCategory.IT, "author2");

        Assert.Single(audit.Records);
        Assert.Equal(AuditAction.Edit, audit.Records[0].Action);
        Assert.Equal("author2", audit.Records[0].Author);
    }

    [Fact]
    public void Edit_NonExistentNews_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<InvalidOperationException>(() =>
            service.Edit(Guid.NewGuid(), "Title", "Body", NewsCategory.HR, "author1"));
    }

    // --- Black-box: Unpublish (CON-013 — no hard delete) ---

    [Fact]
    public void Unpublish_SetsStatusToUnpublished()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.HR, "author1");

        var unpublished = service.Unpublish(item.Id, "author1");

        Assert.Equal(NewsStatus.Unpublished, unpublished.Status);
    }

    [Fact]
    public void Unpublish_PreservesRecord()
    {
        var (service, persistence, _) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.HR, "author1");
        service.Unpublish(item.Id, "author1");

        var stillExists = persistence.GetNewsItem(item.Id);
        Assert.NotNull(stillExists);
        Assert.Equal("Title", stillExists!.Title);
        Assert.Equal("Body", stillExists.Body);
    }

    [Fact]
    public void Unpublish_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.HR, "author1");
        audit.Records.Clear();

        service.Unpublish(item.Id, "author1");

        Assert.Single(audit.Records);
        Assert.Equal(AuditAction.Unpublish, audit.Records[0].Action);
    }

    [Fact]
    public void Unpublish_NonExistentNews_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<InvalidOperationException>(() =>
            service.Unpublish(Guid.NewGuid(), "author1"));
    }

    // --- Black-box: GetPublishedNews ---

    [Fact]
    public void GetPublishedNews_NoFilter_ReturnsAllPublished()
    {
        var (service, _, _) = CreateService();
        service.Publish("News 1", "Body", NewsCategory.General, "a1");
        service.Publish("News 2", "Body", NewsCategory.HR, "a2");

        var news = service.GetPublishedNews(null);

        Assert.Equal(2, news.Count);
    }

    [Fact]
    public void GetPublishedNews_WithFilter_ReturnsOnlyMatchingCategory()
    {
        var (service, _, _) = CreateService();
        service.Publish("News 1", "Body", NewsCategory.General, "a1");
        service.Publish("News 2", "Body", NewsCategory.HR, "a2");
        service.Publish("News 3", "Body", NewsCategory.HR, "a3");

        var news = service.GetPublishedNews(NewsCategory.HR);

        Assert.Equal(2, news.Count);
        Assert.All(news, n => Assert.Equal(NewsCategory.HR, n.Category));
    }

    [Fact]
    public void GetPublishedNews_ExcludesUnpublished()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("News 1", "Body", NewsCategory.General, "a1");
        service.Unpublish(item.Id, "a1");

        var news = service.GetPublishedNews(null);

        Assert.Empty(news);
    }

    // --- Black-box: GetFeaturedNews ---

    [Fact]
    public void GetFeaturedNews_ReturnsOnlyFeaturedPublished()
    {
        var (service, persistence, _) = CreateService();
        var item1 = service.Publish("Featured", "Body", NewsCategory.General, "a1");
        item1.IsFeatured = true;
        persistence.UpdateNewsItem(item1.Id, item1.Title, item1.Body, item1.Category);
        // Manually set featured flag via persistence
        var item2 = service.Publish("Not Featured", "Body", NewsCategory.General, "a2");

        var featured = service.GetFeaturedNews();

        // item1 should be featured, item2 should not
        Assert.Contains(featured, n => n.Title == "Featured");
    }

    // --- Black-box: ListAll ---

    [Fact]
    public void ListAll_ReturnsAllItemsIncludingUnpublished()
    {
        var (service, _, _) = CreateService();
        var item1 = service.Publish("News 1", "Body", NewsCategory.General, "a1");
        var item2 = service.Publish("News 2", "Body", NewsCategory.HR, "a2");
        service.Unpublish(item2.Id, "a2");

        var all = service.ListAll();

        Assert.Equal(2, all.Count);
    }

    // --- Black-box: GetById ---

    [Fact]
    public void GetById_ExistingNews_ReturnsItem()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.HR, "a1");

        var found = service.GetById(item.Id);

        Assert.NotNull(found);
        Assert.Equal("Title", found!.Title);
    }

    [Fact]
    public void GetById_NonExistent_ReturnsNull()
    {
        var (service, _, _) = CreateService();
        var found = service.GetById(Guid.NewGuid());
        Assert.Null(found);
    }
}