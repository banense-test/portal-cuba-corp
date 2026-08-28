using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Pages;
using PortalCubaCorp.Pages.HR;
using System.Security.Claims;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for presentation layer page models.
/// Black-box: verify page model contract — correct properties populated on GET/POST.
/// White-box: exercise category filter, empty query, HR role guard, audit trail calls.
/// </summary>
public class PresentationTests
{
    // --- IndexModel (V001) — UC-001 clocking + UC-008 news ---

    [Fact]
    public void IndexModel_OnGet_PopulatesNewsAndStatus()
    {
        var clockingSvc = new Mock<IClockingService>();
        clockingSvc.Setup(s => s.GetCurrentStatus(It.IsAny<string>())).Returns(ClockStatus.ClockedOut);
        var newsSvc = new Mock<INewsService>();
        var newsItems = new List<NewsItem>
        {
            new() { Title = "Test News", Body = "Body", Category = NewsCategory.General, Status = NewsStatus.Published }
        };
        newsSvc.Setup(s => s.GetPublishedNews(null)).Returns(newsItems);
        newsSvc.Setup(s => s.GetFeaturedNews()).Returns(new List<NewsItem>());

        var model = new IndexModel(clockingSvc.Object, newsSvc.Object);
        SetupUser(model, "emp123");

        model.OnGet(null);

        Assert.Equal(ClockStatus.ClockedOut, model.CurrentStatus);
        Assert.Single(model.News);
        Assert.Empty(model.FeaturedNews);
    }

    [Fact]
    public void IndexModel_OnGet_WithCategory_FiltersNews()
    {
        var clockingSvc = new Mock<IClockingService>();
        clockingSvc.Setup(s => s.GetCurrentStatus(It.IsAny<string>())).Returns(ClockStatus.ClockedIn);
        var newsSvc = new Mock<INewsService>();
        newsSvc.Setup(s => s.GetPublishedNews(NewsCategory.HR)).Returns(new List<NewsItem>
        {
            new() { Title = "HR News", Category = NewsCategory.HR }
        });
        newsSvc.Setup(s => s.GetFeaturedNews()).Returns(new List<NewsItem>());

        var model = new IndexModel(clockingSvc.Object, newsSvc.Object);
        SetupUser(model, "emp123");

        model.OnGet(NewsCategory.HR);

        Assert.Equal(NewsCategory.HR, model.SelectedCategory);
        Assert.Single(model.News);
        Assert.Equal("HR News", model.News[0].Title);
    }

    [Fact]
    public void IndexModel_OnGet_PopulatesEmployeeIdFromClaims()
    {
        var clockingSvc = new Mock<IClockingService>();
        var newsSvc = new Mock<INewsService>();
        newsSvc.Setup(s => s.GetPublishedNews(null)).Returns(new List<NewsItem>());
        newsSvc.Setup(s => s.GetFeaturedNews()).Returns(new List<NewsItem>());

        var model = new IndexModel(clockingSvc.Object, newsSvc.Object);
        SetupUser(model, "user-oidc-123");

        model.OnGet(null);

        Assert.Equal("user-oidc-123", model.EmployeeId);
    }

    // --- ClockingModel (V002) — UC-001 + UC-002 ---

    [Fact]
    public void ClockingModel_OnGet_PopulatesStatusAndHistory()
    {
        var clockingSvc = new Mock<IClockingService>();
        clockingSvc.Setup(s => s.GetCurrentStatus(It.IsAny<string>())).Returns(ClockStatus.ClockedIn);
        var history = new List<ClockingRecord>
        {
            new() { EmployeeId = "emp1", Type = ClockType.In, Timestamp = DateTime.UtcNow }
        };
        clockingSvc.Setup(s => s.GetHistory(It.IsAny<string>(), It.IsAny<DateRange>())).Returns(history);

        var model = new ClockingModel(clockingSvc.Object);
        SetupUser(model, "emp1");

        model.OnGet();

        Assert.Equal(ClockStatus.ClockedIn, model.CurrentStatus);
        Assert.Single(model.History);
    }

    [Fact]
    public void ClockingModel_OnGet_NoHistory_ReturnsEmptyList()
    {
        var clockingSvc = new Mock<IClockingService>();
        clockingSvc.Setup(s => s.GetCurrentStatus(It.IsAny<string>())).Returns(ClockStatus.ClockedOut);
        clockingSvc.Setup(s => s.GetHistory(It.IsAny<string>(), It.IsAny<DateRange>())).Returns(new List<ClockingRecord>());

        var model = new ClockingModel(clockingSvc.Object);
        SetupUser(model, "emp1");

        model.OnGet();

        Assert.Empty(model.History);
    }

    // --- DirectoryModel (V007) — UC-009 ---

    [Fact]
    public void DirectoryModel_OnGet_WithQuery_PopulatesResults()
    {
        var dirSvc = new Mock<IDirectoryService>();
        dirSvc.Setup(s => s.Search("john")).Returns(new List<DirectoryEntry>
        {
            new() { AdUserId = "jdoe", DisplayName = "John Doe" }
        });

        var model = new DirectoryModel(dirSvc.Object);

        model.OnGet("john");

        Assert.Equal("john", model.Query);
        Assert.Single(model.Results);
        Assert.Equal("John Doe", model.Results[0].DisplayName);
    }

    [Fact]
    public void DirectoryModel_OnGet_EmptyQuery_ReturnsEmptyResults()
    {
        var dirSvc = new Mock<IDirectoryService>();

        var model = new DirectoryModel(dirSvc.Object);

        model.OnGet("");

        Assert.Equal("", model.Query);
        Assert.Empty(model.Results);
        dirSvc.Verify(s => s.Search(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void DirectoryModel_OnGet_NullQuery_ReturnsEmptyResults()
    {
        var dirSvc = new Mock<IDirectoryService>();

        var model = new DirectoryModel(dirSvc.Object);

        model.OnGet(null);

        Assert.Equal("", model.Query);
        Assert.Empty(model.Results);
    }

    // --- AllClockingsModel (V003) — UC-003 ---

    [Fact]
    public void AllClockingsModel_OnGet_DefaultsToCurrentMonth()
    {
        var clockingSvc = new Mock<IClockingService>();
        clockingSvc.Setup(s => s.GetAllClockings(It.IsAny<DateRange>())).Returns(new List<ClockingRecord>());

        var model = new AllClockingsModel(clockingSvc.Object);

        model.OnGet(null, null);

        var now = DateTime.UtcNow;
        Assert.Equal(now.Year, model.Year);
        Assert.Equal(now.Month, model.Month);
    }

    [Fact]
    public void AllClockingsModel_OnGet_WithYearMonth_PopulatesClockings()
    {
        var clockingSvc = new Mock<IClockingService>();
        var clockings = new List<ClockingRecord>
        {
            new() { EmployeeId = "emp1", Type = ClockType.In, Timestamp = new DateTime(2026, 1, 15, 9, 0, 0, DateTimeKind.Utc) }
        };
        clockingSvc.Setup(s => s.GetAllClockings(It.IsAny<DateRange>())).Returns(clockings);

        var model = new AllClockingsModel(clockingSvc.Object);

        model.OnGet(2026, 1);

        Assert.Equal(2026, model.Year);
        Assert.Equal(1, model.Month);
        Assert.Single(model.Clockings);
    }

    // --- PublishNewsModel (V004) — UC-005 ---

    [Fact]
    public void PublishNewsModel_OnPost_PublishesNewsWithAudit()
    {
        var newsSvc = new Mock<INewsService>();
        var publishedItem = new NewsItem { Title = "Test", Body = "Body", Category = NewsCategory.HR };
        newsSvc.Setup(s => s.Publish("Test", "Body", NewsCategory.HR, It.IsAny<string>()))
               .Returns(publishedItem);

        var model = new PublishNewsModel(newsSvc.Object);
        SetupUser(model, "hr-user-1");

        model.OnPost("Test", "Body", NewsCategory.HR, false);

        newsSvc.Verify(s => s.Publish("Test", "Body", NewsCategory.HR, "hr-user-1"), Times.Once);
        Assert.NotNull(model.Message);
    }

    [Fact]
    public void PublishNewsModel_OnGet_PopulatesCategories()
    {
        var newsSvc = new Mock<INewsService>();
        var model = new PublishNewsModel(newsSvc.Object);

        model.OnGet();

        Assert.Equal(4, model.Categories.Count);
        Assert.Contains(NewsCategory.General, model.Categories);
        Assert.Contains(NewsCategory.HR, model.Categories);
        Assert.Contains(NewsCategory.IT, model.Categories);
        Assert.Contains(NewsCategory.Events, model.Categories);
    }

    // --- EditNewsModel (V005) — UC-006 ---

    [Fact]
    public void EditNewsModel_OnGet_PopulatesNewsItem()
    {
        var newsSvc = new Mock<INewsService>();
        var item = new NewsItem { Id = Guid.NewGuid(), Title = "Test", Body = "Body", Category = NewsCategory.IT };
        newsSvc.Setup(s => s.GetById(item.Id)).Returns(item);

        var model = new EditNewsModel(newsSvc.Object);

        model.OnGet(item.Id);

        Assert.NotNull(model.NewsItem);
        Assert.Equal("Test", model.NewsItem!.Title);
    }

    [Fact]
    public void EditNewsModel_OnGet_NonExistent_ReturnsNull()
    {
        var newsSvc = new Mock<INewsService>();
        newsSvc.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((NewsItem?)null);

        var model = new EditNewsModel(newsSvc.Object);

        model.OnGet(Guid.NewGuid());

        Assert.Null(model.NewsItem);
    }

    [Fact]
    public void EditNewsModel_OnPost_UpdatesAndAudits()
    {
        var newsSvc = new Mock<INewsService>();
        var id = Guid.NewGuid();
        var updated = new NewsItem { Id = id, Title = "Updated", Body = "New Body", Category = NewsCategory.Events };
        newsSvc.Setup(s => s.Edit(id, "Updated", "New Body", NewsCategory.Events, It.IsAny<string>()))
               .Returns(updated);

        var model = new EditNewsModel(newsSvc.Object);
        SetupUser(model, "hr-user-1");

        model.OnPost(id, "Updated", "New Body", NewsCategory.Events);

        newsSvc.Verify(s => s.Edit(id, "Updated", "New Body", NewsCategory.Events, "hr-user-1"), Times.Once);
        Assert.Equal("Updated", model.NewsItem!.Title);
        Assert.NotNull(model.Message);
    }

    // --- NewsManagementModel (V006) — UC-007 ---

    [Fact]
    public void NewsManagementModel_OnGet_PopulatesAllNews()
    {
        var newsSvc = new Mock<INewsService>();
        var allNews = new List<NewsItem>
        {
            new() { Title = "News 1", Status = NewsStatus.Published },
            new() { Title = "News 2", Status = NewsStatus.Unpublished }
        };
        newsSvc.Setup(s => s.ListAll()).Returns(allNews);

        var model = new NewsManagementModel(newsSvc.Object);

        model.OnGet();

        Assert.Equal(2, model.AllNews.Count);
    }

    [Fact]
    public void NewsManagementModel_OnPostUnpublish_CallsUnpublishAndRefreshes()
    {
        var newsSvc = new Mock<INewsService>();
        var id = Guid.NewGuid();
        newsSvc.Setup(s => s.ListAll()).Returns(new List<NewsItem>());

        var model = new NewsManagementModel(newsSvc.Object);
        SetupUser(model, "hr-user-1");

        model.OnPostUnpublish(id);

        newsSvc.Verify(s => s.Unpublish(id, "hr-user-1"), Times.Once);
        newsSvc.Verify(s => s.ListAll(), Times.Exactly(2));
    }

    // --- WorkerCategoryModel (V008) — UC-010 ---

    [Fact]
    public void WorkerCategoryModel_OnGet_NoSearch_PopulatesCategoriesOnly()
    {
        var wcSvc = new Mock<IWorkerCategoryService>();
        wcSvc.Setup(s => s.ListCategories()).Returns(new List<WorkerCategory>
        {
            new() { AdUserId = "jdoe", Category = "IT" }
        });

        var model = new WorkerCategoryModel(wcSvc.Object);

        model.OnGet(null);

        Assert.Single(model.Categories);
        Assert.Empty(model.SearchResults);
        Assert.Equal("", model.SearchQuery);
    }

    [Fact]
    public void WorkerCategoryModel_OnGet_WithSearch_PopulatesResults()
    {
        var wcSvc = new Mock<IWorkerCategoryService>();
        wcSvc.Setup(s => s.ListCategories()).Returns(new List<WorkerCategory>());
        wcSvc.Setup(s => s.LookupAdUser("john")).Returns(new List<DirectoryEntry>
        {
            new() { AdUserId = "jdoe", DisplayName = "John Doe" }
        });

        var model = new WorkerCategoryModel(wcSvc.Object);

        model.OnGet("john");

        Assert.Equal("john", model.SearchQuery);
        Assert.Single(model.SearchResults);
    }

    [Fact]
    public void WorkerCategoryModel_OnPostAssign_CallsAssignAndRefreshes()
    {
        var wcSvc = new Mock<IWorkerCategoryService>();
        wcSvc.Setup(s => s.ListCategories()).Returns(new List<WorkerCategory>());

        var model = new WorkerCategoryModel(wcSvc.Object);
        SetupUser(model, "hr-user-1");

        model.OnPostAssign("jdoe", "Operations");

        wcSvc.Verify(s => s.AssignCategory("jdoe", "Operations", "hr-user-1"), Times.Once);
        wcSvc.Verify(s => s.ListCategories(), Times.Once);
    }

    // --- Helper: setup user claims on PageModel ---

    private static void SetupUser(PageModel model, string userId)
    {
        var claims = new[] { new Claim("sub", userId) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        model.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}