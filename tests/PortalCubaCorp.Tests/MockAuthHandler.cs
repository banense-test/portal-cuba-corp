using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Mock authentication handler for integration and performance tests.
///
/// ═══════════════════════════════════════════════════════════════════════════
/// MOCK-AUTH EXPIRY — Binding Condition #3 (Stakeholder Sanction T2)
/// ═══════════════════════════════════════════════════════════════════════════
///
/// Expiry date:  2027-01-31
/// Owner:        Infrastructure team (STK-003)
/// Created:      2026-08-29 (Transition T2)
///
/// Purpose:
///   Unblocks 8 test cases that require OIDC authentication by providing
///   a test auth handler that bypasses Keycloak. These tests validate the
///   application logic behind authenticated endpoints without requiring a
///   running Keycloak instance.
///
/// Residual risk (formally accepted — stakeholder directive T2):
///   8 test cases are covered by mock and will only be proven against the
///   real OIDC client at deployment time on the internal Windows Server.
///   Keycloak is out of project scope (CON-004). STK-003 operates Keycloak.
///   The mock is a testing convenience, NOT a production authentication
///   mechanism — production uses real Keycloak OIDC as configured in
///   Program.cs (AddOpenIdConnect).
///
/// Expiry condition:
///   If deployment to the internal Windows Server has not occurred by
///   2027-01-31, this mock must be re-evaluated: either extend the expiry
///   with stakeholder approval, or replace the mock with real OIDC
///   integration tests against a staging Keycloak instance.
///
/// Tests covered by this mock (8):
///   - PerformanceTests.NFR001_PageLoad_Under3Seconds
///   - PerformanceTests.NFR002_ClockInResponse_Under1Second
///   - (6 additional integration tests that would require authenticated HTTP)
///
/// Disposition: Formally accepted risk — stakeholder sanction T2.
/// ═══════════════════════════════════════════════════════════════════════════
/// </summary>
public class MockAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthScheme = "TestAuth";
    public const string TestEmployeeId = "test-emp-001";

    /// <summary>
    /// MOCK-AUTH EXPIRY: 2027-01-31 — Owner: STK-003 (Infrastructure team)
    /// </summary>
    public static readonly DateTime ExpiryDate = new(2027, 1, 31);

    public MockAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim("sub", TestEmployeeId),
            new Claim(ClaimTypes.Name, TestEmployeeId),
            new Claim(ClaimTypes.Role, "Employee")
        };
        var identity = new ClaimsIdentity(claims, AuthScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
