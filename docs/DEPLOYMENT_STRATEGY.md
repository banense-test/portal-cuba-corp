# Portal Cuba Corp — Deployment Strategy

**Phase:** Inception
**Status:** Draft
**Date:** 2026-08-28

## Deployment Mode

**Custom-built** — Portal Cuba Corp is a bespoke internal application deployed to a single
organization (Cuba Corp, 200 employees, 3 offices). It is not shrink-wrapped or downloadable;
it runs on internal infrastructure managed by the Infrastructure team (STK-003).

## Target Environments

| Environment | Description | Status |
|---|---|---|
| Production (single) | Internal Windows Server (CON-006) hosting .NET 10 application + PostgreSQL on the same node | Target topology defined |
| Keycloak (external) | Already running, maintained by STK-003. Portal registers as OIDC client (CON-004) | External — not deployed by this project |
| Active Directory (external) | Already running, maintained by STK-003. Portal reads via LDAP (CON-005, CON-009) | External — not deployed by this project |

## Target User Community

- **STK-004** Cuba Corp Employees — 200 people across 3 offices; daily use for clocking (FR-001, FR-002), news reading (FR-008), and directory lookup (FR-009)
- **STK-001** HR Director and HR staff — use clocking reports (FR-003, FR-004), news management (FR-005, FR-006, FR-007), and worker category management (FR-010)

## Rollout Approach

Single-server deployment to the internal Windows Server. No phased rollout or canary releases
are needed — the user base is 200 employees on a corporate intranet with extended working hours
(NFR-003: Mon–Fri 7:00–19:00). Deployment occurs during a maintenance window outside working hours.

**Beta testing** is planned for the Transition phase: a subset of employees across the 3 offices
will test clocking, news, and directory before full rollout. Beta feedback mechanism and success
criteria will be defined in Transition.

## Rollback Criteria

| Criterion | Action |
|---|---|
| Smoke test failure after deployment | Restore previous build from backup; restart application pool |
| OIDC authentication failure (Keycloak unreachable) | Verify network path to Keycloak; if Keycloak is down, escalate to STK-003 — portal cannot operate without auth |
| LDAP connection failure (AD unreachable) | Verify network path to AD; if AD is down, escalate to STK-003 — directory feature degrades, clocking still works |
| Page load exceeds NFR-001 (3s) on corporate network | Investigate; rollback if performance regression confirmed |
| Clocking response exceeds NFR-002 (1s) | Investigate; rollback if latency regression confirmed |

## Deployment Constraints

| Constraint | Impact on Deployment |
|---|---|
| CON-006 | Internal Windows Server only — no cloud deployment options |
| CON-007 | No external access — no public-facing endpoints, no reverse proxy for internet |
| CON-001 | .NET 10 runtime required on target server |
| CON-003 | PostgreSQL must be installed and configured on the target server |
| CON-004 | Keycloak OIDC client registration must exist before deployment (STK-003 responsibility) |
| CON-005 | LDAP connectivity to AD must be verified before deployment (STK-003 responsibility) |
| NFR-003 | Deployment window: outside Mon–Fri 7:00–19:00 |

## Deployment Risks

| Risk | Severity | Mitigation |
|---|---|---|
| R001 — AD LDAP attribute inconsistency across 3 offices | High (9) | Validate LDAP attributes in Elaboration Iter 1 (PoC). Directory may show gaps if attributes are not filled consistently. |
| R002 — Employee adoption resistance | Medium (6) | Beta program in Transition; communicate change; training material as first-class deliverable. |
| Keycloak client not registered before deployment | Medium | Coordinate with STK-003 early in Elaboration; deployment blocked without OIDC client. |
| PostgreSQL not installed on target server | Low | Infrastructure prerequisite — verify during Elaboration. |
