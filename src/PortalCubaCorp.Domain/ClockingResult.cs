namespace PortalCubaCorp.Domain;

/// <summary>
/// Result of a clocking operation (IClockingService.RecordClocking).
/// Carries the clocking record and flags whether this was a duplicate
/// (idempotency key already existed) or a new insertion.
/// </summary>
public class ClockingResult
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Whether the idempotency key was a duplicate — the clocking was already recorded.
    /// </summary>
    public bool IsDuplicate { get; init; }

    /// <summary>
    /// The clocking record (existing if duplicate, newly created otherwise).
    /// </summary>
    public ClockingRecord? Record { get; init; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? Error { get; init; }

    public static ClockingResult Ok(ClockingRecord record) => new() { Success = true, Record = record };

    public static ClockingResult Duplicate(ClockingRecord existing) => new() { Success = true, IsDuplicate = true, Record = existing };

    public static ClockingResult Fail(string error) => new() { Success = false, Error = error };
}