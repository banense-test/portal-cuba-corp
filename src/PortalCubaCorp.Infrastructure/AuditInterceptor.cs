using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Audit interceptor implementation (COMP-008).
/// Cross-cutting concern: records author + timestamp for news ops and category changes.
/// Append-only — audit records are never updated or deleted (NFR-004).
/// </summary>
public class AuditInterceptor : IAuditLogger
{
    private readonly IPersistence _persistence;

    public AuditInterceptor(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public void LogAudit(string entityType, string entityId, AuditAction action, string author, DateTime timestamp)
    {
        var record = new AuditRecord
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Author = author,
            Timestamp = timestamp
        };
        _persistence.InsertAuditRecord(record);
    }
}