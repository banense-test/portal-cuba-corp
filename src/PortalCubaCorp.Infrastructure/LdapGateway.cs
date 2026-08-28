using PortalCubaCorp.Domain;
using DomainLdapSearchResult = PortalCubaCorp.Domain.LdapSearchResult;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// LDAP gateway implementation (COMP-005).
/// Read-only access to Active Directory via LDAP (CON-005).
/// Never writes to AD (CON-010). Missing attributes return null (R001 fallback).
/// Uses ILdapConnection abstraction for testability.
/// </summary>
public class LdapGateway : ILdapGateway
{
    private readonly LdapGatewayOptions _options;
    private readonly ILdapConnection _connection;

    private static readonly string[] DirectoryAttributes =
    {
        "sAMAccountName", "cn", "title", "department",
        "physicalDeliveryOfficeName", "mail", "telephoneNumber"
    };

    public LdapGateway(LdapGatewayOptions options, ILdapConnection connection)
    {
        _options = options;
        _connection = connection;
    }

    public List<DomainLdapSearchResult> SearchEntries(string filter)
    {
        _connection.Connect(_options.Host, _options.Port);
        _connection.Bind(_options.BindDn, _options.BindPassword);

        var rawEntries = _connection.Search(_options.SearchBase, filter, DirectoryAttributes);
        var results = rawEntries.Select(MapEntry).ToList();

        _connection.Disconnect();
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

    private static DomainLdapSearchResult MapEntry(LdapRawEntry entry)
    {
        return new DomainLdapSearchResult
        {
            AdUserId = entry.GetAttribute("sAMAccountName") ?? string.Empty,
            DisplayName = entry.GetAttribute("cn"),
            JobTitle = entry.GetAttribute("title"),
            Department = entry.GetAttribute("department"),
            Office = entry.GetAttribute("physicalDeliveryOfficeName"),
            Email = entry.GetAttribute("mail"),
            Extension = entry.GetAttribute("telephoneNumber")
        };
    }

    private static string EscapeFilter(string value)
    {
        return value.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");
    }
}