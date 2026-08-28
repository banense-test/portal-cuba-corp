namespace PortalCubaCorp.Domain;

/// <summary>
/// News item entity (CLS-017, maps to T2 news_items table).
/// Lifecycle: Published → Unpublished (never deleted, CON-013).
/// State machine: once unpublished, the record is preserved for audit (NFR-004).
/// </summary>
public class NewsItem
{
    /// <summary>
    /// Database-generated unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// News title — non-empty per INewsService.Publish precondition.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// News body content — non-empty per INewsService.Publish precondition.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Category — General, HR, IT, or Events (FR-005).
    /// </summary>
    public NewsCategory Category { get; set; }

    /// <summary>
    /// Lifecycle status — Published or Unpublished (CON-013).
    /// </summary>
    public NewsStatus Status { get; set; } = NewsStatus.Published;

    /// <summary>
    /// Whether this news item appears as a featured banner (FR-008).
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Timestamp of original publication.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp of last modification (updated on edit, not on unpublish).
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// OIDC subject of the original author.
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;
}