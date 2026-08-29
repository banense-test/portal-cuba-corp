using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for DirectoryService (COMP-001).
/// Black-box: verify IDirectoryService contract — search returns DirectoryEntry list from LDAP.
/// White-box: exercise empty query branch, R001 fallback (missing attributes → "N/A"), office filter.
/// UC-009: Search Employee Directory.
/// </summary>
public class DirectoryServiceTests
{
    private static (DirectoryService service, MockLdapGateway ldap) CreateService()
    {
        var ldap = new MockLdapGateway();
        var service = new DirectoryService(ldap);
        return (service, ldap);
    }

    // --- Black-box: Search (UC-009) ---

    [Fact]
    public void Search_ValidQuery_ReturnsResults()
    {
        var (service, ldap) = CreateService();
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

        var results = service.Search("john");

        Assert.Single(results);
        Assert.Equal("John Doe", results[0].DisplayName);
        Assert.Equal("Developer", results[0].JobTitle);
    }

    [Fact]
    public void Search_MultipleResults_ReturnsAll()
    {
        var (service, ldap) = CreateService();
        ldap.Entries.AddRange(new[]
        {
            new LdapSearchResult { AdUserId = "jdoe", DisplayName = "John Doe" },
            new LdapSearchResult { AdUserId = "jsmith", DisplayName = "John Smith" }
        });

        var results = service.Search("john");

        Assert.Equal(2, results.Count);
    }

    // --- White-box: R001 fallback ---

    [Fact]
    public void Search_MissingAttributes_ReturnsNA()
    {
        var (service, ldap) = CreateService();
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

        var results = service.Search("john");

        Assert.Single(results);
        Assert.Equal("N/A", results[0].JobTitle);
        Assert.Equal("N/A", results[0].Department);
        Assert.Equal("N/A", results[0].Office);
        Assert.Equal("N/A", results[0].Email);
        Assert.Equal("N/A", results[0].Extension);
    }

    [Fact]
    public void Search_AllAttributesMissing_ReturnsAllNA()
    {
        var (service, ldap) = CreateService();
        ldap.Entries.Add(new LdapSearchResult
        {
            AdUserId = "jdoe",
            DisplayName = null,
            JobTitle = null,
            Department = null,
            Office = null,
            Email = null,
            Extension = null
        });

        var results = service.Search("jdoe");

        Assert.Single(results);
        Assert.Equal("N/A", results[0].DisplayName);
        Assert.Equal("N/A", results[0].JobTitle);
        Assert.Equal("N/A", results[0].Department);
        Assert.Equal("N/A", results[0].Office);
        Assert.Equal("N/A", results[0].Email);
        Assert.Equal("N/A", results[0].Extension);
    }

    // --- White-box: empty query ---

    [Fact]
    public void Search_EmptyQuery_ReturnsEmptyList()
    {
        var (service, _) = CreateService();
        var results = service.Search("");
        Assert.Empty(results);
    }

    [Fact]
    public void Search_NullQuery_ReturnsEmptyList()
    {
        var (service, _) = CreateService();
        var results = service.Search(null!);
        Assert.Empty(results);
    }

    [Fact]
    public void Search_WhitespaceQuery_ReturnsEmptyList()
    {
        var (service, _) = CreateService();
        var results = service.Search("   ");
        Assert.Empty(results);
    }

    // --- Black-box: no results ---
    // Issue #13 fix: test name and assertion now match — empty results when no match
    [Fact]
    public void Search_NoMatchingEntries_ReturnsEmptyList()
    {
        var (service, ldap) = CreateService();
        // Empty mock — no entries configured, simulates no LDAP match
        var results = service.Search("nonexistent");
        Assert.Empty(results);
    }

    // --- White-box: office filter (MINOR-1 fix) ---

    [Fact]
    public void Search_WithOfficeFilter_BuildsCombinedFilter()
    {
        var (service, ldap) = CreateService();
        ldap.Entries.Add(new LdapSearchResult
        {
            AdUserId = "jdoe",
            DisplayName = "John Doe",
            Office = "Havana"
        });

        var results = service.Search("john", "Havana");

        Assert.Single(results);
        Assert.Equal("Havana", results[0].Office);
    }

    [Fact]
    public void Search_WithNullOfficeFilter_BehavesAsNoFilter()
    {
        var (service, ldap) = CreateService();
        ldap.Entries.Add(new LdapSearchResult
        {
            AdUserId = "jdoe",
            DisplayName = "John Doe",
            Office = "Havana"
        });

        var results = service.Search("john", null);

        Assert.Single(results);
    }

    [Fact]
    public void Search_WithEmptyOfficeFilter_BehavesAsNoFilter()
    {
        var (service, ldap) = CreateService();
        ldap.Entries.Add(new LdapSearchResult
        {
            AdUserId = "jdoe",
            DisplayName = "John Doe",
            Office = "Havana"
        });

        var results = service.Search("john", "");

        Assert.Single(results);
    }
}