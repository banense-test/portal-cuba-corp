using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for WorkerCategoryService (COMP-004).
/// Black-box: verify IWorkerCategoryService contract — assign category, list categories, lookup AD user.
/// White-box: exercise validation branches, audit trail calls, empty query handling.
/// UC-010: Manage Worker Category.
/// </summary>
public class WorkerCategoryServiceTests
{
    private static (WorkerCategoryService service, InMemoryPersistence persistence, MockLdapGateway ldap, InMemoryAuditLogger audit) CreateService()
    {
        var persistence = new InMemoryPersistence();
        var ldap = new MockLdapGateway();
        var audit = new InMemoryAuditLogger();
        var service = new WorkerCategoryService(persistence, ldap, audit);
        return (service, persistence, ldap, audit);
    }

    // --- Black-box: AssignCategory (UC-010) ---

    [Fact]
    public async Task AssignCategory_NewUser_CreatesCategory()
    {
        var (service, persistence, _, _) = CreateService();
        var result = await service.AssignCategoryAsync("jdoe", "IT", "hr1");

        Assert.Equal("jdoe", result.AdUserId);
        Assert.Equal("IT", result.Category);
        var stored = persistence.GetAllWorkerCategories();
        Assert.Single(stored);
    }

    [Fact]
    public async Task AssignCategory_ExistingUser_UpdatesCategory()
    {
        var (service, _, _, _) = CreateService();
        await service.AssignCategoryAsync("jdoe", "IT", "hr1");
        var result = await service.AssignCategoryAsync("jdoe", "Operations", "hr1");

        Assert.Equal("Operations", result.Category);
    }

    [Fact]
    public async Task AssignCategory_CreatesAuditRecord()
    {
        var (service, _, _, audit) = CreateService();
        await service.AssignCategoryAsync("jdoe", "IT", "hr1");

        Assert.Single(audit.Records);
        Assert.Equal(AuditAction.CategoryChanged, audit.Records[0].Action);
        Assert.Equal("hr1", audit.Records[0].Author);
        Assert.Equal("jdoe", audit.Records[0].EntityId);
    }

    // --- White-box: validation ---

    [Fact]
    public async Task AssignCategory_EmptyAdUserId_Throws()
    {
        var (service, _, _, _) = CreateService();
        await Assert.ThrowsAsync<ArgumentException>(() => service.AssignCategoryAsync("", "IT", "hr1"));
    }

    [Fact]
    public async Task AssignCategory_EmptyCategory_Throws()
    {
        var (service, _, _, _) = CreateService();
        await Assert.ThrowsAsync<ArgumentException>(() => service.AssignCategoryAsync("jdoe", "", "hr1"));
    }

    // --- Black-box: ListCategories (UC-010) ---

    [Fact]
    public async Task ListCategories_ReturnsAllCategories()
    {
        var (service, _, _, _) = CreateService();
        await service.AssignCategoryAsync("jdoe", "IT", "hr1");
        await service.AssignCategoryAsync("jsmith", "HR", "hr1");

        var categories = service.ListCategories();

        Assert.Equal(2, categories.Count);
    }

    [Fact]
    public void ListCategories_NoCategories_ReturnsEmptyList()
    {
        var (service, _, _, _) = CreateService();
        var categories = service.ListCategories();
        Assert.Empty(categories);
    }

    // --- Black-box: LookupAdUser (UC-010) ---

    [Fact]
    public void LookupAdUser_ValidQuery_ReturnsResults()
    {
        var (service, _, ldap, _) = CreateService();
        ldap.Entries.Add(new LdapSearchResult
        {
            AdUserId = "jdoe",
            DisplayName = "John Doe",
            JobTitle = "Developer",
            Department = "IT",
            Office = "Havana",
            Email = "jdoe@cuba.cu",
            Extension = "1234"
        });

        var results = service.LookupAdUser("john");

        Assert.Single(results);
        Assert.Equal("John Doe", results[0].DisplayName);
    }

    [Fact]
    public void LookupAdUser_MissingAttributes_ReturnsNA()
    {
        var (service, _, ldap, _) = CreateService();
        ldap.Entries.Add(new LdapSearchResult
        {
            AdUserId = "jdoe",
            DisplayName = "John Doe",
            JobTitle = null,
            Department = null,
            Office = null,
            Email = null,
            Extension = null
        });

        var results = service.LookupAdUser("john");

        Assert.Single(results);
        Assert.Equal("N/A", results[0].JobTitle);
    }

    // --- White-box: empty query ---

    [Fact]
    public void LookupAdUser_EmptyQuery_ReturnsEmptyList()
    {
        var (service, _, _, _) = CreateService();
        var results = service.LookupAdUser("");
        Assert.Empty(results);
    }

    [Fact]
    public void LookupAdUser_NullQuery_ReturnsEmptyList()
    {
        var (service, _, _, _) = CreateService();
        var results = service.LookupAdUser(null!);
        Assert.Empty(results);
    }
}