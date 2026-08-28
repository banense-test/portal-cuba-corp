# Portal Cuba Corp — Deployment Strategy

## Deployment Mode

**Custom-built** — internal intranet application deployed to a single Windows Server.

- No cloud, no public-facing endpoints (CON-006, CON-007)
- Single node sufficient for 200 users across 3 offices
- NFR-003: Extended working hours Mon–Fri 7:00–19:00, no 24/7 requirement

## Target Environments

| Environment | Description |
|---|---|
| Development | Developer workstation / local IIS |
| Production (single) | Internal Windows Server — .NET 10 app + PostgreSQL on same node |

## Topology

- **Client:** Chrome / Edge browser (CON-008), Razor Pages server-rendered (CON-002)
- **Application Server:** Windows Server (CON-006), .NET 10 REST API (CON-001)
- **Database:** PostgreSQL (CON-003), co-located on same server
- **Authentication:** Keycloak OIDC (CON-004) — external, already running, managed by STK-003
- **Directory:** Active Directory over LDAP (CON-005) — external, already running, read-only

## Rollout Approach

1. Deploy to internal Windows Server
2. Register OIDC client in Keycloak (STK-003 prerequisite)
3. Confirm LDAP connectivity and attribute coverage (STK-003, R001)
4. Beta test with subset of employees
5. Full cutover — all 3 offices access same server

## Rollback Criteria

- Portal unavailable >30 min during business hours (NFR-003)
- Clocking data corruption or loss
- Keycloak OIDC integration failure
- AD LDAP connectivity failure

## Offline Behavior (AC-005)

- Clocking POST retried client-side via localStorage for up to 5 minutes
- Server accepts client timestamp, rejects duplicates by idempotency key
- Directory and news show "no connection" when offline
- No PWA, no service worker, no client cache of other data
