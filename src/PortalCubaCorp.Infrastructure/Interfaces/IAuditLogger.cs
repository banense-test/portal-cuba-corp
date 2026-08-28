using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Audit logger interface (INT-005, COMP-008).
/// Append-only audit trail — records who, what, and when (NFR-004).
/// Method named LogAudit to avoid collision with Microsoft.Extensions.Logging.ILogger.Log().
/// </summary>
public interface IAuditLogger
{
    void LogAudit(string entityType, string entityId, AuditAction action, string author, DateTime timestamp);
}