using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// LDAP gateway interface (INT-006, COMP-005).
/// Read-only access to Active Directory over LDAP (CON-005, CON-010).
/// </summary>
public interface ILdapGateway
{
    List<LdapSearchResult> SearchEntries(string filter);
    LdapSearchResult? GetEntryByUserId(string adUserId);
    Dictionary<string, string> ResolveNames(List<string> adUserIds);
}