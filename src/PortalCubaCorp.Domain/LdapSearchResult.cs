namespace PortalCubaCorp.Domain;

/// <summary>
/// Raw LDAP search result from ILdapGateway.SearchEntries.
/// Contains the raw attribute values extracted from an AD entry.
/// The DirectoryService maps these to DirectoryEntry with R001 fallback.
/// </summary>
public class LdapSearchResult
{
    /// <summary>
    /// AD user identifier (sAMAccountName).
    /// </summary>
    public string AdUserId { get; set; } = string.Empty;

    /// <summary>
    /// Display name (cn attribute).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Job title (title attribute).
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Department (department attribute).
    │   /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Office (physicalDeliveryOfficeName attribute).
    /// </summary>
    public string? Office { get; set; }

    /// <summary>
    /// Email (mail attribute).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Extension phone (telephoneNumber attribute).
    /// </summary>
    public string? Extension { get; set; }
}