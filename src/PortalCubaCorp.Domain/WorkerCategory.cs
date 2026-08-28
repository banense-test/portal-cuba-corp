namespace PortalCubaCorp.Domain;

/// <summary>
/// Worker category entity (CLS-018, maps to T3 worker_categories table).
/// Stores ONLY AD user id → category (CON-009). Two columns, nothing else.
/// All other employee data is projected from AD at read time — no sync.
/// </summary>
public class WorkerCategory
{
    /// <summary>
    /// Active Directory user identifier (sAMAccountName or DN).
    /// </summary>
    public string AdUserId { get; set; } = string.Empty;

    /// <summary>
    /// Worker category label (e.g., "Administrative", "Operations", "Management").
    /// </summary>
    public string Category { get; set; } = string.Empty;
}