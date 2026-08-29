using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for ClockingService (COMP-002).
/// Black-box: verify IClockingService contract — record clocking, get status, history, CSV export.
/// White-box: exercise idempotency deduplication branch (per-employee scoped), empty input validation, status determination logic.
/// </summary>
public class ClockingServiceTests
{
    private static (ClockingService service, InMemoryPersistence persistence) CreateService()
    {
        var persistence = new InMemoryPersistence();
        var service = new ClockingService(persistence);
        return (service, persistence);
    }

    // --- Black-box: RecordClocking ---

    [Fact]
    public void RecordClocking_NewKey_ReturnsSuccess()
    {
        var (service, _) = CreateService();
        var result = service.RecordClocking("emp1", DateTime.UtcNow, ClockType.In, "key-001");

        Assert.True(result.Success);
        Assert.False(result.IsDuplicate);
        Assert.NotNull(result.Record);
        Assert.Equal("emp1", result.Record!.EmployeeId);
        Assert.Equal(ClockType.In, result.Record.Type);
        Assert.Equal("key-001", result.Record.IdempotencyKey);
    }

    [Fact]
    public void RecordClocking_DuplicateKey_ReturnsExistingRecord()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;
        service.RecordClocking("emp1", ts, ClockType.In, "key-dup");
        var result = service.RecordClocking("emp1", ts, ClockType.In, "key-dup");

        Assert.True(result.IsDuplicate);
        Assert.True(result.Success);
        Assert.NotNull(result.Record);
        Assert.Equal("key-dup", result.Record!.IdempotencyKey);
    }

    // --- White-box: CR #11 — idempotency key scoped per employee ---

    [Fact]
    public void RecordClocking_SameKeyDifferentEmployee_BothSucceed()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;
        var key = "shared-key-001";

        // Same idempotency key but different employees — both should succeed
        // because idempotency is scoped per (employeeId, key), not globally
        var first = service.RecordClocking("emp1", ts, ClockType.In, key);
        var second = service.RecordClocking("emp2", ts, ClockType.In, key);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.False(first.IsDuplicate);
        Assert.False(second.IsDuplicate);
        Assert.NotEqual(first.Record!.Id, second.Record!.Id);
    }

    [Fact]
    public void RecordClocking_EmptyEmployeeId_ReturnsFail()
    {
        var (service, _) = CreateService();
        var result = service.RecordClocking("", DateTime.UtcNow, ClockType.In, "key-001");

        Assert.False(result.Success);
        Assert.Equal("Employee ID is required", result.Error);
    }

    [Fact]
    public void RecordClocking_EmptyIdempotencyKey_ReturnsFail()
    {
        var (service, _) = CreateService();
        var result = service.RecordClocking("emp1", DateTime.UtcNow, ClockType.In, "");

        Assert.False(result.Success);
        Assert.Equal("Idempotency key is required", result.Error);
    }

    // --- Black-box: GetCurrentStatus ---

    [Fact]
    public void GetCurrentStatus_NoHistory_ReturnsClockedOut()
    {
        var (service, _) = CreateService();
        var status = service.GetCurrentStatus("emp1");
        Assert.Equal(ClockStatus.ClockedOut, status);
    }

    [Fact]
    public void GetCurrentStatus_LastClockIn_ReturnsClockedIn()
    {
        var (service, _) = CreateService();
        service.RecordClocking("emp1", DateTime.UtcNow, ClockType.In, "key-001");
        var status = service.GetCurrentStatus("emp1");
        Assert.Equal(ClockStatus.ClockedIn, status);
    }

    [Fact]
    public void GetCurrentStatus_LastClockOut_ReturnsClockedOut()
    {
        var (service, _) = CreateService();
        var now = DateTime.UtcNow;
        service.RecordClocking("emp1", now.AddMinutes(-30), ClockType.In, "key-001");
        service.RecordClocking("emp1", now, ClockType.Out, "key-002");
        var status = service.GetCurrentStatus("emp1");
        Assert.Equal(ClockStatus.ClockedOut, status);
    }

    // --- Black-box: GetHistory ---

    [Fact]
    public void GetHistory_ReturnsEmployeeClockings()
    {
        var (service, _) = CreateService();
        var now = DateTime.UtcNow;
        service.RecordClocking("emp1", now, ClockType.In, "key-001");
        service.RecordClocking("emp1", now.AddMinutes(30), ClockType.Out, "key-002");

        var range = DateRange.ForMonth(now.Year, now.Month);
        var history = service.GetHistory("emp1", range);

        Assert.Equal(2, history.Count);
    }

    [Fact]
    public void GetHistory_NoClockings_ReturnsEmptyList()
    {
        var (service, _) = CreateService();
        var range = DateRange.ForMonth(2026, 1);
        var history = service.GetHistory("emp1", range);
        Assert.Empty(history);
    }

    // --- Black-box: GetAllClockings ---

    [Fact]
    public void GetAllClockings_ReturnsAllEmployees()
    {
        var (service, _) = CreateService();
        var now = DateTime.UtcNow;
        service.RecordClocking("emp1", now, ClockType.In, "key-001");
        service.RecordClocking("emp2", now, ClockType.In, "key-002");

        var range = DateRange.ForMonth(now.Year, now.Month);
        var all = service.GetAllClockings(range);

        Assert.Equal(2, all.Count);
    }

    // --- Black-box: ExportCsv ---

    [Fact]
    public void ExportCsv_WithClockings_ReturnsCsvStream()
    {
        var (service, _) = CreateService();
        var now = DateTime.UtcNow;
        var range = DateRange.ForMonth(now.Year, now.Month);
        service.RecordClocking("emp1", now, ClockType.In, "key-001");
        service.RecordClocking("emp1", now.AddMinutes(30), ClockType.Out, "key-002");

        var stream = service.ExportCsv(range);
        var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        Assert.Contains("Employee,Date,TimeIn,TimeOut,Direction", content);
        Assert.Contains("emp1", content);
        Assert.Contains("IN", content);
        Assert.Contains("OUT", content);
    }

    [Fact]
    public void ExportCsv_NoClockings_ReturnsHeaderOnly()
    {
        var (service, _) = CreateService();
        var range = DateRange.ForMonth(2026, 1);

        var stream = service.ExportCsv(range);
        var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        var lines = content.Trim().Split('\n');
        Assert.Single(lines);
        Assert.Contains("Employee,Date,TimeIn,TimeOut,Direction", lines[0]);
    }
}
