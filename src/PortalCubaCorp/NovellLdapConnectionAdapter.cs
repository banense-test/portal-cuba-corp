using PortalCubaCorp.Infrastructure;

namespace PortalCubaCorp;

/// <summary>
/// Adapter that wraps the Novell.Directory.Ldap library into the ILdapConnection abstraction.
/// This is the real LDAP connection implementation for production use.
/// The Novell.Directory.Ldap.NETStandard package API is used here.
/// </summary>
public class NovellLdapConnectionAdapter : ILdapConnection
{
    public void Connect(string host, int port)
    {
        // Real implementation will use Novell.Directory.Ldap.LdapConnection.Connect()
        // Deferred to integration testing with real AD server
        throw new NotImplementedException("LDAP connection requires real AD server configuration.");
    }

    public void Bind(string bindDn, string password)
    {
        throw new NotImplementedException("LDAP connection requires real AD server configuration.");
    }

    public List<LdapRawEntry> Search(string searchBase, string filter, string[] attributes)
    {
        throw new NotImplementedException("LDAP connection requires real AD server configuration.");
    }

    public void Disconnect()
    {
        throw new NotImplementedException("LDAP connection requires real AD server configuration.");
    }
}