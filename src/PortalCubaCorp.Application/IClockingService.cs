using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Application;

/// <summary>
/// Clocking service interface (INT-001, COMP-002).
/// Handles clock in/out with idempotency key for offline retry (AC-005).
/// </summary>
public interface IClockingService
{
    ClockingResult RecordClocking(string employeeId, DateTime timestamp, ClockType type, string idempotencyKey);
    ClockStatus GetCurrentStatus(string employeeId);
    List<ClockingRecord> GetHistory(string employeeId, DateRange month);
    List<ClockingRecord> GetAllClockings(DateRange month);
    Stream ExportCsv(DateRange month);
}