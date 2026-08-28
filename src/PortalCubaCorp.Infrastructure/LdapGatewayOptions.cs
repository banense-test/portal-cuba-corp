namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// Configuration options for the LDAP gateway (COMP-005).
/// Connection details for the corporate Active Directory server.
/// </summary>
public class LdapGatewayOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 389;
    public string BindDn { get; set; } = string.Empty;
    public string BindPassword { get; set; } = string.Empty;
    public string SearchBase { get; set; } = string.Empty;
}