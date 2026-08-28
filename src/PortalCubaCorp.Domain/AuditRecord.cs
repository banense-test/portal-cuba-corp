namespace PortalCubaCorp.Domain;

/// <summary>
/// Audit record entity (CLS-019, maps to T4 audit_records table).
/// Append-only — never updated or deleted (NFR-004).
/// Records who performed an action, what entity was affected, and when.
/// </summary>
public class AuditRecord
{
    /// <summary>
    /// Database-generated primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Type of entity affected (e.g., "NEWS_ITEM", "WORKER_CATEGORY").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the affected entity (Guid for news, string for worker category).
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Action performed — Publish, Edit, Unpublish, or CategoryChanged.
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// OIDC subject of the user who performed the action.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Server-side timestamp of when the action was performed.
    /// </summary>
    public DateTime Timestamp { get; set; }
}