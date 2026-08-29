using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Novell LDAP connection adapter (COMP-005).
/// Wraps Novell.Directory.Ldap.LdapConnection to implement ILdapConnection.
///
/// [DEFERRED — requires integration testing with real AD server (R001)]
/// All methods throw NotImplementedException until integration testing with
/// the real Active Directory server provided by STK-003 (Infrastructure team).
/// The LdapGateway is fully unit-tested via MockLdapGateway (ILdapGateway mock);
/// this adapter is the production implementation that will be validated during
/// integration testing when the AD server is available.
/// </summary>
public class NovellLdapConnectionAdapter : ILdapConnection
{
    // [DEFERRED — requires integration testing with real AD server (R001)]

    public void Connect(string host, int port)
    {
        throw new NotImplementedException("LDAP connection requires real AD server (R001 — deferred to integration testing)");
    }

    public void Bind(string bindDn, string password)
    {
        throw new NotImplementedException("LDAP bind requires real AD server (R001 — deferred to integration testing)");
    }

    public List<LdapRawEntry> Search(string searchBase, string filter, string[] attributes)
    {
        throw new NotImplementedException("LDAP search requires real AD server (R001 — deferred to integration testing)");
    }

    public void Disconnect()
    {
        throw new NotImplementedException("LDAP disconnect requires real AD server (R001 — deferred to integration testing)");
    }
}