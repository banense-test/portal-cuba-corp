using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for NewsService (COMP-003).
/// Black-box: verify INewsService contract — publish, edit, unpublish, get published, get featured, list all.
/// White-box: exercise validation branches, audit trail calls, CON-013 no-delete behavior, isFeatured flag.
/// UC-005: Publish. UC-006: Edit. UC-007: Unpublish. UC-008: Read and Filter.
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

    // --- Black-box: Publish (UC-005) ---

    [Fact]
    public async Task Publish_ValidInput_ReturnsPublishedNewsItem()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.HR, false, "author1");

        Assert.Equal("Title", item.Title);
        Assert.Equal("Body", item.Body);
        Assert.Equal(NewsCategory.HR, item.Category);
        Assert.Equal(NewsStatus.Published, item.Status);
        Assert.Equal("author1", item.AuthorId);
    }

    [Fact]
    public async Task Publish_IsFeaturedTrue_SetsFeaturedFlag()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Featured Title", "Body", NewsCategory.General, true, "author1");

        Assert.True(item.IsFeatured);
    }

    [Fact]
    public async Task Publish_IsFeaturedFalse_DoesNotSetFeaturedFlag()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.General, false, "author1");

        Assert.False(item.IsFeatured);
    }

    [Fact]
    public async Task Publish_EmptyTitle_Throws()
    {
        var (service, _, _) = CreateService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.PublishAsync("", "Body", NewsCategory.General, false, "author1"));
    }

    [Fact]
    public async Task Publish_EmptyBody_Throws()
    {
        var (service, _, _) = CreateService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.PublishAsync("Title", "", NewsCategory.General, false, "author1"));
    }

    [Fact]
    public async Task Publish_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        await service.PublishAsync("Title", "Body", NewsCategory.HR, false, "author1");

        Assert.Single(audit.Records);
        Assert.Equal(AuditAction.Publish, audit.Records[0].Action);
        Assert.Equal("author1", audit.Records[0].Author);
    }

    // --- Black-box: Edit (UC-006) ---

    [Fact]
    public async Task Edit_ValidInput_ReturnsUpdatedNewsItem()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Original", "Body", NewsCategory.General, false, "author1");

        var updated = await service.EditAsync(item.Id, "Updated Title", "Updated Body", NewsCategory.HR, true, "author2");

        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Body", updated.Body);
        Assert.Equal(NewsCategory.HR, updated.Category);
        // C4-1: isFeatured is now updated by Edit (CR-010)
        Assert.True(updated.IsFeatured);
    }

    [Fact]
    public async Task Edit_NonExistentNews_Throws()
    {
        var (service, _, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EditAsync(Guid.NewGuid(), "Title", "Body", NewsCategory.General, false, "author1"));
    }

    [Fact]
    public async Task Edit_EmptyTitle_Throws()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.General, false, "author1");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.EditAsync(item.Id, "", "Body", NewsCategory.General, false, "author1"));
    }

    [Fact]
    public async Task Edit_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.General, false, "author1");
        await service.EditAsync(item.Id, "Updated", "Body", NewsCategory.HR, false, "author2");

        // Two audit records: one for publish, one for edit
        Assert.Equal(2, audit.Records.Count);
        Assert.Equal(AuditAction.Edit, audit.Records[1].Action);
        Assert.Equal("author2", audit.Records[1].Author);
    }

    // C4-1: Edit preserves isFeatured when set to true (CR-010)
    [Fact]
    public async Task Edit_IsFeaturedTrue_UpdatesFeaturedFlag()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.General, false, "author1");
        Assert.False(item.IsFeatured);

        var updated = await service.EditAsync(item.Id, "Title", "Body", NewsCategory.General, true, "author1");
        Assert.True(updated.IsFeatured);
    }

    // C4-1: Edit can unset isFeatured (CR-010)
    [Fact]
    public async Task Edit_IsFeaturedFalse_UnsetsFeaturedFlag()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.General, true, "author1");
        Assert.True(item.IsFeatured);

        var updated = await service.EditAsync(item.Id, "Title", "Body", NewsCategory.General, false, "author1");
        Assert.False(updated.IsFeatured);
    }

    // --- Black-box: Unpublish (UC-007) ---

    [Fact]
    public async Task Unpublish_PublishedNews_SetsStatusUnpublished()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.General, false, "author1");

        var updated = await service.UnpublishAsync(item.Id, "author1");

        Assert.Equal(NewsStatus.Unpublished, updated.Status);
    }

    [Fact]
    public async Task Unpublish_NonExistentNews_Throws()
    {
        var (service, _, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UnpublishAsync(Guid.NewGuid(), "author1"));
    }

    [Fact]
    public async Task Unpublish_CreatesAuditRecord()
    {
        var (service, _, audit) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.General, false, "author1");
        await service.UnpublishAsync(item.Id, "author1");

        Assert.Equal(2, audit.Records.Count);
        Assert.Equal(AuditAction.Unpublish, audit.Records[1].Action);
    }

    // CON-013: Unpublished news is NOT deleted — it stays in the list
    [Fact]
    public async Task Unpublish_PreservesRecordInListAll()
    {
        var (service, _, _) = CreateService();
        var item1 = await service.PublishAsync("News 1", "Body", NewsCategory.General, false, "a1");
        var item2 = await service.PublishAsync("News 2", "Body", NewsCategory.HR, false, "a2");
        await service.UnpublishAsync(item2.Id, "a2");

        var all = service.ListAll();

        Assert.Equal(2, all.Count);
    }

    // --- Black-box: GetById (UC-008) ---

    [Fact]
    public async Task GetById_ExistingNews_ReturnsItem()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Title", "Body", NewsCategory.HR, false, "a1");

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

    // --- Black-box: GetPublishedNews (UC-008) ---

    [Fact]
    public async Task GetPublishedNews_ReturnsOnlyPublished()
    {
        var (service, _, _) = CreateService();
        await service.PublishAsync("Published", "Body", NewsCategory.General, false, "a1");
        var item2 = await service.PublishAsync("To Unpublish", "Body", NewsCategory.HR, false, "a2");
        await service.UnpublishAsync(item2.Id, "a2");

        var published = service.GetPublishedNews(null);

        Assert.Single(published);
        Assert.Equal("Published", published[0].Title);
    }

    [Fact]
    public async Task GetPublishedNews_WithCategoryFilter_ReturnsMatchingCategory()
    {
        var (service, _, _) = CreateService();
        await service.PublishAsync("General News", "Body", NewsCategory.General, false, "a1");
        await service.PublishAsync("HR News", "Body", NewsCategory.HR, false, "a2");

        var hrNews = service.GetPublishedNews(NewsCategory.HR);

        Assert.Single(hrNews);
        Assert.Equal("HR News", hrNews[0].Title);
    }

    // --- Black-box: GetFeaturedNews (UC-008, FR-008 featured banner) ---

    [Fact]
    public async Task GetFeaturedNews_ReturnsOnlyFeaturedPublished()
    {
        var (service, _, _) = CreateService();
        await service.PublishAsync("Regular", "Body", NewsCategory.General, false, "a1");
        await service.PublishAsync("Featured", "Body", NewsCategory.General, true, "a2");

        var featured = service.GetFeaturedNews();

        Assert.Single(featured);
        Assert.Equal("Featured", featured[0].Title);
    }

    [Fact]
    public async Task GetFeaturedNews_ExcludesUnpublishedFeatured()
    {
        var (service, _, _) = CreateService();
        var item = await service.PublishAsync("Featured", "Body", NewsCategory.General, true, "a1");
        await service.UnpublishAsync(item.Id, "a1");

        var featured = service.GetFeaturedNews();

        Assert.Empty(featured);
    }

    // --- Black-box: ListAll (UC-005..UC-007 management view) ---

    [Fact]
    public async Task ListAll_ReturnsAllIncludingUnpublished()
    {
        var (service, _, _) = CreateService();
        var item1 = await service.PublishAsync("News 1", "Body", NewsCategory.General, false, "a1");
        var item2 = await service.PublishAsync("News 2", "Body", NewsCategory.HR, false, "a2");
        await service.UnpublishAsync(item2.Id, "a2");

        var all = service.ListAll();

        Assert.Equal(2, all.Count);
    }
}
