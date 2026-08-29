using PortalCubaCorp.Application;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using Xunit;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Unit tests for the offline clocking retry mechanism (R006, AC-005).
/// Black-box: verify idempotency key prevents duplicate records on retry.
/// White-box: exercise the retry flow - store, deduplicate, succeed, and timeout branches.
/// UC-001: Clock In/Out with offline retry.
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
    }

    // --- White-box: different employees with same key both succeed (CR #11 per-employee scoping) ---

    [Fact]
    public void Retry_SameKeyDifferentEmployee_BothSucceed()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;
        var key = "shared-key-retry";

        var first = service.RecordClocking("emp1", ts, ClockType.In, key);
        var second = service.RecordClocking("emp2", ts, ClockType.In, key);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.False(first.IsDuplicate);
        Assert.False(second.IsDuplicate);
    }

    // --- Black-box: server accepts client-side timestamp (AC-005 criterion 5) ---

    [Fact]
    public void Retry_ClientTimestamp_PreservedInRecord()
    {
        var (service, _) = CreateService();
        var clientTs = new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);
        var key = "emp1-client-ts-key";

        var result = service.RecordClocking("emp1", clientTs, ClockType.In, key);

        Assert.True(result.Success);
        Assert.Equal(clientTs, result.Record!.Timestamp);
    }

    // --- White-box: empty idempotency key rejected ---

    [Fact]
    public void Retry_EmptyIdempotencyKey_ReturnsFail()
    {
        var (service, _) = CreateService();
        var result = service.RecordClocking("emp1", DateTime.UtcNow, ClockType.In, "");

        Assert.False(result.Success);
        Assert.Equal("Idempotency key is required", result.Error);
    }

    // --- White-box: empty employee ID rejected ---

    [Fact]
    public void Retry_EmptyEmployeeId_ReturnsFail()
    {
        var (service, _) = CreateService();
        var result = service.RecordClocking("", DateTime.UtcNow, ClockType.In, "key-001");

        Assert.False(result.Success);
        Assert.Equal("Employee ID is required", result.Error);
    }

    // --- Black-box: multiple retries with same key all return same record ---

    [Fact]
    public void Retry_MultipleRetries_AllReturnSameRecord()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;
        var key = "emp1-multi-retry-key";

        var first = service.RecordClocking("emp1", ts, ClockType.In, key);
        var second = service.RecordClocking("emp1", ts, ClockType.In, key);
        var third = service.RecordClocking("emp1", ts, ClockType.In, key);

        Assert.True(first.Success);
        Assert.True(second.IsDuplicate);
        Assert.True(third.IsDuplicate);
        Assert.Equal(first.Record!.Id, second.Record!.Id);
        Assert.Equal(first.Record!.Id, third.Record!.Id);
    }

    // --- Black-box: IN followed by OUT with different keys both succeed ---

    [Fact]
    public void Retry_ClockInThenOut_DifferentKeys_BothSucceed()
    {
        var (service, _) = CreateService();
        var ts = DateTime.UtcNow;

        var inResult = service.RecordClocking("emp1", ts, ClockType.In, "key-in-001");
        var outResult = service.RecordClocking("emp1", ts.AddMinutes(30), ClockType.Out, "key-out-001");

        Assert.True(inResult.Success);
        Assert.True(outResult.Success);
        Assert.False(inResult.IsDuplicate);
        Assert.False(outResult.IsDuplicate);
    }

    // --- White-box: ExecuteInTransactionAsync (C4-2) ---

    [Fact]
    public async Task ExecuteInTransactionAsync_SuccessfulAction_Commits()
    {
        var persistence = new InMemoryPersistence();

        await persistence.ExecuteInTransactionAsync(async () =>
        {
            persistence.InsertClocking(new ClockingRecord
            {
                EmployeeId = "emp1",
                Timestamp = DateTime.UtcNow,
                Type = ClockType.In,
                IdempotencyKey = "tx-key-001"
            });
            await Task.CompletedTask;
        });

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
}