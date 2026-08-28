using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Audit logger interface (INT-005, COMP-008).
/// Append-only audit trail — records who, what, and when (NFR-004).
/// </summary>
public interface IAuditLogger
{
    void Log(string entityType, string entityId, AuditAction action, string author, DateTime timestamp);
}