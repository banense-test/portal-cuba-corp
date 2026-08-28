using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for NewsService (COMP-003).
/// Black-box: verify INewsService contract — publish, edit, unpublish, get published, get featured, list all.
/// White-box: exercise validation branches, audit trail calls, CON-013 no-delete behavior, isFeatured flag.
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
        var item = service.Publish("Title", "Body", NewsCategory.HR, false, "author1");

        Assert.Equal("Title", item.Title);
        Assert.Equal("Body", item.Body);
        Assert.Equal(NewsCategory.HR, item.Category);
        Assert.Equal(NewsStatus.Published, item.Status);
        Assert.Equal("author1", item.AuthorId);
    }

    [Fact]
    public void Publish_IsFeaturedTrue_SetsFeaturedFlag()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Featured Title", "Body", NewsCategory.General, true, "author1");

        Assert.True(item.IsFeatured);
    }

    [Fact]
    public void Publish_IsFeaturedFalse_DoesNotSetFeaturedFlag()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Regular Title", "Body", NewsCategory.General, false, "author1");

        Assert.False(item.IsFeatured);
    }

    [Fact]
    public void Publish_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.IT, false, "author1");

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
        Assert.Throws<ArgumentException>(() => service.Publish("", "Body", NewsCategory.HR, false, "a1"));
    }

    [Fact]
    public void Publish_EmptyBody_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<ArgumentException>(() => service.Publish("Title", "", NewsCategory.HR, false, "a1"));
    }

    [Fact]
    public void Publish_WhitespaceTitle_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<ArgumentException>(() => service.Publish("   ", "Body", NewsCategory.HR, false, "a1"));
    }

    // --- Black-box: Edit ---

    [Fact]
    public void Edit_ExistingNews_UpdatesFields()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Original", "Body", NewsCategory.General, false, "a1");

        var updated = service.Edit(item.Id, "Updated Title", "Updated Body", NewsCategory.IT, "a2");

        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Body", updated.Body);
        Assert.Equal(NewsCategory.IT, updated.Category);
    }

    [Fact]
    public void Edit_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.General, false, "a1");
        audit.Records.Clear();

        service.Edit(item.Id, "New Title", "New Body", NewsCategory.HR, "a2");

        Assert.Single(audit.Records);
        Assert.Equal(AuditAction.Edit, audit.Records[0].Action);
        Assert.Equal("a2", audit.Records[0].Author);
    }

    [Fact]
    public void Edit_NonExistentNews_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<InvalidOperationException>(() => service.Edit(Guid.NewGuid(), "T", "B", NewsCategory.HR, "a1"));
    }

    // --- Black-box: Unpublish ---

    [Fact]
    public void Unpublish_PublishedNews_SetsStatusToUnpublished()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.General, false, "a1");

        var updated = service.Unpublish(item.Id, "a1");

        Assert.Equal(NewsStatus.Unpublished, updated.Status);
    }

    [Fact]
    public void Unpublish_PreservesRecord_NotDeleted()
    {
        var (service, persistence, _) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.General, false, "a1");
        service.Unpublish(item.Id, "a1");

        // Record still exists — CON-013: never hard-deleted
        var found = persistence.GetNewsItem(item.Id);
        Assert.NotNull(found);
        Assert.Equal(NewsStatus.Unpublished, found!.Status);
    }

    [Fact]
    public void Unpublish_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.General, false, "a1");
        audit.Records.Clear();

        service.Unpublish(item.Id, "a1");

        Assert.Single(audit.Records);
        Assert.Equal(AuditAction.Unpublish, audit.Records[0].Action);
    }

    [Fact]
    public void Unpublish_NonExistentNews_Throws()
    {
        var (service, _, _) = CreateService();
        Assert.Throws<InvalidOperationException>(() => service.Unpublish(Guid.NewGuid(), "a1"));
    }

    // --- Black-box: GetPublishedNews ---

    [Fact]
    public void GetPublishedNews_ReturnsOnlyPublishedItems()
    {
        var (service, _, _) = CreateService();
        service.Publish("News 1", "Body", NewsCategory.General, false, "a1");
        var item2 = service.Publish("News 2", "Body", NewsCategory.HR, false, "a2");
        service.Unpublish(item2.Id, "a2");

        var published = service.GetPublishedNews(null);

        Assert.Single(published);
        Assert.Equal("News 1", published[0].Title);
    }

    [Fact]
    public void GetPublishedNews_WithCategoryFilter_ReturnsFilteredResults()
    {
        var (service, _, _) = CreateService();
        service.Publish("News 1", "Body", NewsCategory.General, false, "a1");
        service.Publish("News 2", "Body", NewsCategory.HR, false, "a2");

        var hrNews = service.GetPublishedNews(NewsCategory.HR);

        Assert.Single(hrNews);
        Assert.Equal(NewsCategory.HR, hrNews[0].Category);
    }

    // --- Black-box: GetFeaturedNews (FR-008) ---

    [Fact]
    public void GetFeaturedNews_ReturnsOnlyFeaturedPublishedItems()
    {
        var (service, _, _) = CreateService();
        service.Publish("Regular", "Body", NewsCategory.General, false, "a1");
        service.Publish("Featured", "Body", NewsCategory.General, true, "a1");

        var featured = service.GetFeaturedNews();

        Assert.Single(featured);
        Assert.Equal("Featured", featured[0].Title);
        Assert.True(featured[0].IsFeatured);
    }

    [Fact]
    public void GetFeaturedNews_UnpublishedFeatured_NotReturned()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Featured", "Body", NewsCategory.General, true, "a1");
        service.Unpublish(item.Id, "a1");

        var featured = service.GetFeaturedNews();

        Assert.Empty(featured);
    }

    [Fact]
    public void GetFeaturedNews_NoFeaturedItems_ReturnsEmpty()
    {
        var (service, _, _) = CreateService();
        service.Publish("Regular", "Body", NewsCategory.General, false, "a1");

        var featured = service.GetFeaturedNews();

        Assert.Empty(featured);
    }

    // --- Black-box: ListAll ---

    [Fact]
    public void ListAll_ReturnsAllItemsIncludingUnpublished()
    {
        var (service, _, _) = CreateService();
        var item1 = service.Publish("News 1", "Body", NewsCategory.General, false, "a1");
        var item2 = service.Publish("News 2", "Body", NewsCategory.HR, false, "a2");
        service.Unpublish(item2.Id, "a2");

        var all = service.ListAll();

        Assert.Equal(2, all.Count);
    }

    // --- Black-box: GetById ---

    [Fact]
    public void GetById_ExistingNews_ReturnsItem()
    {
        var (service, _, _) = CreateService();
        var item = service.Publish("Title", "Body", NewsCategory.HR, false, "a1");

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
