using Novell.Directory.Ldap;
using PortalCubaCorp.Domain;
using DomainLdapSearchResult = PortalCubaCorp.Domain.LdapSearchResult;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// LDAP gateway implementation (COMP-005).
/// Read-only access to Active Directory via Novell.Directory.Ldap (CON-005).
/// Never writes to AD (CON-010). Missing attributes return null (R001 fallback).
/// </summary>
public class LdapGateway : ILdapGateway
{
    private readonly LdapGatewayOptions _options;

    // LDAP search scope constants (Novell.Directory.Ldap.NETStandard)
    private const int SCOPE_SUB = 2;

    public LdapGateway(LdapGatewayOptions options)
    {
        _options = options;
    }

    public List<DomainLdapSearchResult> SearchEntries(string filter)
    {
        var results = new List<DomainLdapSearchResult>();

        using var conn = new LdapConnection();
        conn.Connect(_options.Host, _options.Port);
        conn.Bind(_options.BindDn, _options.BindPassword);

        var searchResults = conn.Search(
            _options.SearchBase,
            SCOPE_SUB,
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

    public DomainLdapSearchResult? GetEntryByUserId(string adUserId)
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

    private static DomainLdapSearchResult MapEntry(LdapEntry entry)
    {
        var attrSet = entry.getAttributeSet();
        return new DomainLdapSearchResult
        {
            AdUserId = GetAttrValue(attrSet, "sAMAccountName") ?? string.Empty,
            DisplayName = GetAttrValue(attrSet, "cn"),
            JobTitle = GetAttrValue(attrSet, "title"),
            Department = GetAttrValue(attrSet, "department"),
            Office = GetAttrValue(attrSet, "physicalDeliveryOfficeName"),
            Email = GetAttrValue(attrSet, "mail"),
            Extension = GetAttrValue(attrSet, "telephoneNumber")
        };
    }

    private static string? GetAttrValue(LdapAttributeSet attrSet, string name)
    {
        return attrSet.getAttribute(name)?.StringValue;
    }

    private static string EscapeFilter(string value)
    {
        return value.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");
    }
}