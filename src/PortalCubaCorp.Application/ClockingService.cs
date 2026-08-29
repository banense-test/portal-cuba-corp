using System.Globalization;
using System.Text;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;

namespace PortalCubaCorp.Application;

/// <summary>
/// Clocking service implementation (COMP-002).
/// UC-001: Clock In / Clock Out with idempotency key for offline retry (AC-005).
/// UC-002: View own clocking history.
/// UC-003: View all employee clockings (HR only).
/// UC-004: Export monthly clocking report as CSV.
/// </summary>
public class ClockingService : IClockingService
{
    private readonly IPersistence _persistence;

    public ClockingService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public ClockingResult RecordClocking(string employeeId, DateTime timestamp, ClockType type, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            return ClockingResult.Fail("Employee ID is required");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return ClockingResult.Fail("Idempotency key is required");

        // Check idempotency — scoped per employee (CR #11)
        // If (employeeId, key) exists, return the existing record (AC-005)
        var existing = _persistence.FindByIdempotencyKey(employeeId, idempotencyKey);
        if (existing != null)
            return ClockingResult.Duplicate(existing);

        // Insert new clocking record
        var record = new ClockingRecord
        {
            EmployeeId = employeeId,
            Timestamp = timestamp,
            Type = type,
            IdempotencyKey = idempotencyKey
        };
        _persistence.InsertClocking(record);
        return ClockingResult.Ok(record);
    }

    public ClockStatus GetCurrentStatus(string employeeId)
    {
        var now = DateTime.UtcNow;
        var monthRange = DateRange.ForMonth(now.Year, now.Month);
        var history = _persistence.GetClockingsByEmployee(employeeId, monthRange);

        if (history.Count == 0)
            return ClockStatus.ClockedOut;

        // History is ordered by timestamp DESC, so first is the most recent
        var mostRecent = history[0];
        return mostRecent.Type == ClockType.In ? ClockStatus.ClockedIn : ClockStatus.ClockedOut;
    }

    public List<ClockingRecord> GetHistory(string employeeId, DateRange month)
    {
        return _persistence.GetClockingsByEmployee(employeeId, month);
    }

    public List<ClockingRecord> GetAllClockings(DateRange month)
    {
        return _persistence.GetAllClockingsForMonth(month);
    }

    public Stream ExportCsv(DateRange month)
    {
        var clockings = _persistence.GetAllClockingsForMonth(month);
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        // C2-MIN-4 fix: header matches data columns — Employee, Date, Time, Direction
        writer.WriteLine("Employee,Date,Time,Direction");

        // Group by employee and pair in/out records
        var grouped = clockings.GroupBy(c => c.EmployeeId).OrderBy(g => g.Key);
        foreach (var group in grouped)
        {
            var sorted = group.OrderBy(c => c.Timestamp).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var record = sorted[i];
                var date = record.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var time = record.Timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                var direction = record.Type == ClockType.In ? "IN" : "OUT";
                writer.WriteLine($"{record.EmployeeId},{date},{time},{direction}");
            }
        }

        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}