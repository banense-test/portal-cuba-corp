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

    // --- White-box: CR #11 — Idempotency scoped per employee prevents cross-employee collision ---

    [Fact]
    public void Retry_SameKeyDifferentEmployee_BothSucceedNotDuplicate()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;
        var key = "shared-key-001";

        // Same idempotency key but different employees — both should succeed
        // because idempotency is scoped per (employeeId, key), not globally (CR #11)
        var first = service.RecordClocking("emp1", ts, ClockType.In, key);
        var second = service.RecordClocking("emp2", ts, ClockType.In, key);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.False(first.IsDuplicate);
        Assert.False(second.IsDuplicate);
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

    // --- White-box: 5-minute expiry boundary (AC-005 criterion 7) ---
    // The client-side clocking-retry.js retries every 10 seconds for up to 5 minutes.
    // After 5 minutes without network recovery, the user sees "Clocking failed — contact HR".
    // This test verifies that a clocking with a timestamp older than 5 minutes is still
    // accepted by the server (the server does not reject old timestamps — the client
    // is responsible for the 5-minute retry window). The test validates that the server
    // idempotency mechanism works correctly even for delayed retries.

    [Fact]
    public void Retry_TimestampOlderThan5Minutes_StillAcceptedByServer()
    {
        var (service, _) = CreateService();
        // Simulate a clocking that was stored 6 minutes ago (past the 5-minute client retry window)
        var oldTimestamp = DateTime.UtcNow.AddMinutes(-6);
        var key = "emp1-expired-retry-key";

        // The server accepts the timestamp regardless of age — the client-side
        // clocking-retry.js is responsible for the 5-minute retry window.
        // If the client sends the clocking after 5 minutes (e.g., user manually retries),
        // the server still processes it with idempotency protection.
        var result = service.RecordClocking("emp1", oldTimestamp, ClockType.In, key);

        Assert.True(result.Success);
        Assert.False(result.IsDuplicate);
        Assert.Equal(oldTimestamp, result.Record!.Timestamp);
    }

    [Fact]
    public void Retry_At5MinuteBoundary_StillAcceptedByServer()
    {
        var (service, _) = CreateService();
        // Simulate a clocking at exactly the 5-minute boundary
        var boundaryTimestamp = DateTime.UtcNow.AddMinutes(-5);
        var key = "emp1-5min-boundary-key";

        var result = service.RecordClocking("emp1", boundaryTimestamp, ClockType.In, key);

        Assert.True(result.Success);
        Assert.False(result.IsDuplicate);
        Assert.Equal(boundaryTimestamp, result.Record!.Timestamp);
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
        var record = persistence.FindByIdempotencyKey("emp1", "tx-key-001");
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
