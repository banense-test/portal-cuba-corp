namespace PortalCubaCorp.Domain;

/// <summary>
/// Clocking record entity (CLS-016, maps to T1 clockings table).
/// Records a single clock in/out event with idempotency key for offline retry (AC-005).
/// </summary>
public class ClockingRecord
{
    /// <summary>
    /// Database-generated primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// OIDC subject identifier of the employee who clocked.
    /// </summary>
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Server-side timestamp of when the clocking was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Direction — Clock In or Clock Out.
    /// </summary>
    public ClockType Type { get; set; }

    /// <summary>
    /// Client-generated UUID for idempotency — prevents duplicate inserts
    /// when the offline retry mechanism resends the same clocking (AC-005).
    /// Unique index enforced by PostgreSQL.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;
}