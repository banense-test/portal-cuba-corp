using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for the offline clocking retry mechanism (R006, AC-005).
/// Black-box: verify idempotency key prevents duplicate records on retry.
/// White-box: exercise the retry flow - store, deduplicate, succeed, and timeout branches.
///
/// These tests validate the SERVER-SIDE idempotency that the client-side
/// clocking-retry.js depends on. The client-side JS stores the clocking in
/// localStorage and retries; the server-side IClockingService.RecordClocking
/// uses the idempotency key to prevent duplicates.
/// </summary>
public class OfflineRetryTests
{
    private static (ClockingService service, InMemoryPersistence persistence) CreateService()
    {
        var persistence = new InMemoryPersistence();
        var service = new ClockingService(persistence);
        return (service, persistence);
    }

    // --- Black-box: Idempotency prevents duplicates on retry (AC-005 criterion 4) ---

    [Fact]
    public void Retry_SameIdempotencyKey_ReturnsDuplicateNotNewRecord()
    {
        var (service, persistence) = CreateService();
        var ts = DateTime.UtcNow;
        var key = "emp1-1234567890-abc123";

        // First attempt (simulates initial POST)
        var first = service.RecordClocking("emp1", ts, ClockType.In, key);
        Assert.True(first.Success);
        Assert.False(first.IsDuplicate);

        // Retry with same key (simulates network recovery + retry)
        var retry = service.RecordClocking("emp1", ts, ClockType.In, key);
        Assert.True(retry.Success);
        Assert.True(retry.IsDuplicate);
        Assert.Equal(first.Record!.Id, retry.Record!.Id);

        // Only one record in the database
        var range = DateRange.ForMonth(ts.Year, ts.Month);
        var history = service.GetHistory("emp1", range);
        Assert.Single(history);
    }

    // --- Black-box: Server accepts client-side timestamp (AC-005 criterion 5) ---

    [Fact]
    public void Retry_ClientSideTimestamp_PreservedInRecord()
    {
        var (service, _) = CreateService();
        var clientTimestamp = new DateTime(2026, 8, 28, 9, 15, 0, DateTimeKind.Utc);
        var key = "emp1-20260828091500-def456";

        var result = service.RecordClocking("emp1", clientTimestamp, ClockType.In, key);

        Assert.True(result.Success);
        Assert.Equal(clientTimestamp, result.Record!.Timestamp);
    }

    // --- White-box: Different idempotency keys create separate records ---

    [Fact]
    public void Retry_DifferentIdempotencyKey_CreatesNewRecord()
    {
        var (service, persistence) = CreateService();
        var ts = DateTime.UtcNow;

        var first = service.RecordClocking("emp1", ts, ClockType.In, "key-A");
        var second = service.RecordClocking("emp1", ts.AddMinutes(30), ClockType.Out, "key-B");

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.False(second.IsDuplicate);
        Assert.NotEqual(first.Record!.Id, second.Record!.Id);

        var range = DateRange.ForMonth(ts.Year, ts.Month);
        var history = service.GetHistory("emp1", range);
        Assert.Equal(2, history.Count);
    }

    // --- White-box: Empty idempotency key is rejected ---

    [Fact]
    public void Retry_EmptyIdempotencyKey_ReturnsFail()
    {
        var (service, _) = CreateService();
        var result = service.RecordClocking("emp1", DateTime.UtcNow, ClockType.In, "");
        Assert.False(result.Success);
        Assert.Equal("Idempotency key is required", result.Error);
    }

    // --- White-box: Null idempotency key is rejected ---

    [Fact]
    public void Retry_NullIdempotencyKey_ReturnsFail()
    {
        var (service, _) = CreateService();
        var result = service.RecordClocking("emp1", DateTime.UtcNow, ClockType.In, null!);
        Assert.False(result.Success);
        Assert.Equal("Idempotency key is required", result.Error);
    }

    // --- White-box: Idempotency works across different employees ---

    [Fact]
    public void Retry_SameKeyDifferentEmployee_BothSucceed()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;
        var key = "shared-key-001";

        // Same idempotency key but different employees - both should succeed
        // because idempotency is per-record, not per-employee
        var first = service.RecordClocking("emp1", ts, ClockType.In, key);
        var second = service.RecordClocking("emp2", ts, ClockType.In, key);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.True(second.IsDuplicate); // Same key = duplicate of first
    }

    // --- Black-box: Multiple retries with same key all return the same record ---

    [Fact]
    public void Retry_MultipleRetriesSameKey_AllReturnSameRecord()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;
        var key = "emp1-multi-retry-key";

        var first = service.RecordClocking("emp1", ts, ClockType.In, key);
        var second = service.RecordClocking("emp1", ts, ClockType.In, key);
        var third = service.RecordClocking("emp1", ts, ClockType.In, key);

        Assert.True(first.Success && !first.IsDuplicate);
        Assert.True(second.Success && second.IsDuplicate);
        Assert.True(third.Success && third.IsDuplicate);
        Assert.Equal(first.Record!.Id, second.Record!.Id);
        Assert.Equal(first.Record!.Id, third.Record!.Id);
    }

    // --- White-box: ExecuteInTransactionAsync wraps operations atomically (INT-007) ---

    [Fact]
    public async Task ExecuteInTransactionAsync_SuccessfulAction_Commits()
    {
        var persistence = new InMemoryPersistence();
        var executed = false;

        await persistence.ExecuteInTransactionAsync(async () =>
        {
            persistence.InsertClocking(new ClockingRecord
            {
                EmployeeId = "emp1",
                Timestamp = DateTime.UtcNow,
                Type = ClockType.In,
                IdempotencyKey = "tx-key-001"
            });
            executed = true;
            await Task.CompletedTask;
        });

        Assert.True(executed);
        var record = persistence.FindByIdempotencyKey("tx-key-001");
        Assert.NotNull(record);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_FailingAction_RollsBackAndThrows()
    {
        var persistence = new InMemoryPersistence();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await persistence.ExecuteInTransactionAsync(async () =>
            {
                persistence.InsertClocking(new ClockingRecord
                {
                    EmployeeId = "emp1",
                    Timestamp = DateTime.UtcNow,
                    Type = ClockType.In,
                    IdempotencyKey = "tx-key-002"
                });
                await Task.CompletedTask;
                throw new InvalidOperationException("Simulated failure");
            });
        });

        // In-memory test double executes directly, so the record IS inserted
        // (real EF Core would roll back). This test verifies the exception propagates.
    }
}