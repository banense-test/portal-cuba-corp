using PortalCubaCorp.Domain;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for Domain entities and value objects.
/// Black-box: verify entity creation and mapping behavior per specification.
/// White-box: exercise every branch in DirectoryEntry.FromLdapAttributes and DateRange.
/// </summary>
public class DomainTests
{
    // --- DirectoryEntry.FromLdapAttributes (R001 fallback) ---

    [Fact]
    public void FromLdapAttributes_AllPresent_ReturnsAllValues()
    {
        var entry = DirectoryEntry.FromLdapAttributes(
            "jdoe", "John Doe", "Developer", "IT", "Havana", "jdoe@cuba.cu", "1234");

        Assert.Equal("jdoe", entry.AdUserId);
        Assert.Equal("John Doe", entry.DisplayName);
        Assert.Equal("Developer", entry.JobTitle);
        Assert.Equal("IT", entry.Department);
        Assert.Equal("Havana", entry.Office);
        Assert.Equal("jdoe@cuba.cu", entry.Email);
        Assert.Equal("1234", entry.Extension);
    }

    [Fact]
    public void FromLdapAttributes_AllNull_ReturnsNA()
    {
        var entry = DirectoryEntry.FromLdapAttributes("jdoe", null, null, null, null, null, null);

        Assert.Equal("jdoe", entry.AdUserId);
        Assert.Equal("N/A", entry.DisplayName);
        Assert.Equal("N/A", entry.JobTitle);
        Assert.Equal("N/A", entry.Department);
        Assert.Equal("N/A", entry.Office);
        Assert.Equal("N/A", entry.Email);
        Assert.Equal("N/A", entry.Extension);
    }

    [Fact]
    public void FromLdapAttributes_AllWhitespace_ReturnsNA()
    {
        var entry = DirectoryEntry.FromLdapAttributes("jdoe", "   ", "\t", " ", "", "  ", "\n");

        Assert.Equal("N/A", entry.DisplayName);
        Assert.Equal("N/A", entry.JobTitle);
        Assert.Equal("N/A", entry.Department);
        Assert.Equal("N/A", entry.Office);
        Assert.Equal("N/A", entry.Email);
        Assert.Equal("N/A", entry.Extension);
    }

    [Fact]
    public void FromLdapAttributes_MixedPresentAndMissing_ReturnsValuesAndNA()
    {
        var entry = DirectoryEntry.FromLdapAttributes(
            "jdoe", "John Doe", null, "IT", "   ", "jdoe@cuba.cu", null);

        Assert.Equal("John Doe", entry.DisplayName);
        Assert.Equal("N/A", entry.JobTitle);
        Assert.Equal("IT", entry.Department);
        Assert.Equal("N/A", entry.Office);
        Assert.Equal("jdoe@cuba.cu", entry.Email);
        Assert.Equal("N/A", entry.Extension);
    }

    // --- DateRange ---

    [Fact]
    public void DateRange_ForMonth_ReturnsCorrectRange()
    {
        var range = DateRange.ForMonth(2026, 3);

        Assert.Equal(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), range.Start);
        Assert.Equal(new DateTime(2026, 3, 31, 23, 59, 59, 9999999, DateTimeKind.Utc), range.End);
    }

    [Fact]
    public void DateRange_ForMonth_December_ReturnsCorrectRange()
    {
        var range = DateRange.ForMonth(2026, 12);

        Assert.Equal(new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc), range.Start);
        Assert.Equal(new DateTime(2026, 12, 31, 23, 59, 59, 9999999, DateTimeKind.Utc), range.End);
    }

    [Fact]
    public void DateRange_ForMonth_January_ReturnsCorrectRange()
    {
        var range = DateRange.ForMonth(2026, 1);

        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), range.Start);
        Assert.Equal(new DateTime(2026, 1, 31, 23, 59, 59, 9999999, DateTimeKind.Utc), range.End);
    }

    // --- ClockingResult ---

    [Fact]
    public void ClockingResult_Ok_SetsSuccessTrue()
    {
        var record = new ClockingRecord { Id = 1, EmployeeId = "emp1" };
        var result = ClockingResult.Ok(record);

        Assert.True(result.Success);
        Assert.False(result.IsDuplicate);
        Assert.Same(record, result.Record);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ClockingResult_Duplicate_SetsIsDuplicateTrue()
    {
        var record = new ClockingRecord { Id = 1, EmployeeId = "emp1", IdempotencyKey = "key1" };
        var result = ClockingResult.Duplicate(record);

        Assert.True(result.Success);
        Assert.True(result.IsDuplicate);
        Assert.Same(record, result.Record);
    }

    [Fact]
    public void ClockingResult_Fail_SetsSuccessFalse()
    {
        var result = ClockingResult.Fail("error");

        Assert.False(result.Success);
        Assert.Equal("error", result.Error);
        Assert.Null(result.Record);
    }
}