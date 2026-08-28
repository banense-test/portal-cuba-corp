using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Abstraction over a raw LDAP connection (COMP-005).
/// Allows the LdapGateway to be unit-tested with mocks (R001 risk retirement).
/// The real implementation wraps Novell.Directory.Ldap.LdapConnection.
/// </summary>
public interface ILdapConnection
{
    void Connect(string host, int port);
    void Bind(string bindDn, string password);
    List<LdapRawEntry> Search(string searchBase, string filter, string[] attributes);
    void Disconnect();
}

/// <summary>
/// Raw LDAP entry — attribute name to value mapping.
/// Missing attributes are simply absent from the dictionary (R001 fallback).
/// </summary>
public class LdapRawEntry
{
    public string Dn { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new();

    public string? GetAttribute(string name)
    {
        return Attributes.TryGetValue(name, out var value) ? value : null;
    }
}