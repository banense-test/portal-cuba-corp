namespace PortalCubaCorp.Domain;

/// <summary>
/// Read-only projection of an Active Directory user (CON-005, CON-012).
/// Contains corporate data only — no private personal information.
/// All fields come from AD over LDAP and are never written back (CON-010).
/// Missing attributes default to "N/A" as R001 fallback.
/// </summary>
public class DirectoryEntry
{
    /// <summary>
    /// AD user identifier (sAMAccountName).
    /// </summary>
    public string AdUserId { get; set; } = string.Empty;

    /// <summary>
    /// Full display name from AD cn attribute.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Job title from AD title attribute.
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Department from AD department attribute.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Office location from AD physicalDeliveryOfficeName attribute.
    /// </summary>
    public string Office { get; set; } = string.Empty;

    /// <summary>
    /// Email address from AD mail attribute.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Extension phone number from AD telephoneNumber attribute.
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Creates a DirectoryEntry from raw LDAP attribute values,
    /// defaulting missing attributes to "N/A" (R001 fallback).
    /// </summary>
    public static DirectoryEntry FromLdapAttributes(
        string adUserId,
        string? displayName,
        string? jobTitle,
        string? department,
        string? office,
        string? email,
        string? extension)
    {
        const string fallback = "N/A";
        return new DirectoryEntry
        {
            AdUserId = adUserId,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? fallback : displayName,
            JobTitle = string.IsNullOrWhiteSpace(jobTitle) ? fallback : jobTitle,
            Department = string.IsNullOrWhiteSpace(department) ? fallback : department,
            Office = string.IsNullOrWhiteSpace(office) ? fallback : office,
            Email = string.IsNullOrWhiteSpace(email) ? fallback : email,
            Extension = string.IsNullOrWhiteSpace(extension) ? fallback : extension
        };
    }
}