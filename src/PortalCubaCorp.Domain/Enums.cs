namespace PortalCubaCorp.Domain;

/// <summary>
/// Direction of a clocking event — Clock In or Clock Out.
/// </summary>
public enum ClockType
{
    In,
    Out
}

/// <summary>
/// Current clocking status of an employee, derived from the most recent ClockingRecord.
/// </summary>
public enum ClockStatus
{
    ClockedIn,
    ClockedOut
}

/// <summary>
/// News category — matches the four categories declared in FR-005.
/// </summary>
public enum NewsCategory
{
    General,
    HR,
    IT,
    Events
}

/// <summary>
/// News lifecycle status — Published items are visible to employees;
/// Unpublished items are hidden but preserved for audit (CON-013).
/// </summary>
public enum NewsStatus
{
    Published,
    Unpublished
}

/// <summary>
/// Audit action type — records what operation was performed on what entity (NFR-004).
/// </summary>
public enum AuditAction
{
    Publish,
    Edit,
    Unpublish,
    CategoryChanged
}
