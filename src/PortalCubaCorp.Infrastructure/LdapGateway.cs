using Novell.Directory.Ldap;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// LDAP gateway implementation (COMP-005).
/// Read-only access to Active Directory via Novell.Directory.Ldap (CON-005).
/// Never writes to AD (CON-010). Missing attributes return null (R001 fallback).
/// </summary>
public class LdapGateway : ILdapGateway
{
    private readonly LdapGatewayOptions _options;

    public LdapGateway(LdapGatewayOptions options)
    {
        _options = options;
    }

    public List<LdapSearchResult> SearchEntries(string filter)
    {
        var results = new List<LdapSearchResult>();

        using var conn = new LdapConnection();
        conn.Connect(_options.Host, _options.Port);
        conn.Bind(_options.BindDn, _options.BindPassword);

        var searchResults = conn.Search(
            _options.SearchBase,
            LdapConnection.SCOPE_SUB,
            filter,
            new[] { "sAMAccountName", "cn", "title", "department", "physicalDeliveryOfficeName", "mail", "telephoneNumber" },
            false);

        while (searchResults.HasMore())
        {
            var entry = searchResults.Next();
            results.Add(MapEntry(entry));
        }

        return results;
    }

    public LdapSearchResult? GetEntryByUserId(string adUserId)
    {
        var results = SearchEntries($"(sAMAccountName={EscapeFilter(adUserId)})");
        return results.FirstOrDefault();
    }

    public Dictionary<string, string> ResolveNames(List<string> adUserIds)
    {
        var mapping = new Dictionary<string, string>();
        foreach (var userId in adUserIds)
        {
            var entry = GetEntryByUserId(userId);
            mapping[userId] = entry?.DisplayName ?? userId;
        }
        return mapping;
    }

    private static LdapSearchResult MapEntry(LdapEntry entry)
    {
        return new LdapSearchResult
        {
            AdUserId = GetAttribute(entry, "sAMAccountName"),
            DisplayName = GetAttributeOrNull(entry, "cn"),
            JobTitle = GetAttributeOrNull(entry, "title"),
            Department = GetAttributeOrNull(entry, "department"),
            Office = GetAttributeOrNull(entry, "physicalDeliveryOfficeName"),
            Email = GetAttributeOrNull(entry, "mail"),
            Extension = GetAttributeOrNull(entry, "telephoneNumber")
        };
    }

    private static string GetAttribute(LdapEntry entry, string name)
    {
        var attr = entry.getAttribute(name);
        return attr?.StringValue ?? string.Empty;
    }

    private static string? GetAttributeOrNull(LdapEntry entry, string name)
    {
        var attr = entry.getAttribute(name);
        return attr?.StringValue;
    }

    private static string EscapeFilter(string value)
    {
        return value.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");
    }
}