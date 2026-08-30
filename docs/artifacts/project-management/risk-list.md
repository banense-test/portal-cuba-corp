## Document Control

| Field | Value |
|---|---|
| Phase | Transition |
| Status | Active — Updated for Transition Iter 2 Close-Out (T3 pending) |
| Milestone Target | Product Release (PR) — **NOT YET ACHIEVED — stakeholder sanction REFUSED (T2); T3 iteration required** |
| Iteration | 2 (Cycle 1) |
| Date | 2026-08-30 |
| Author | Project Manager (Project Management Discipline) |
| Prior Phase | Construction C4 Cycle 1 — R003 ACCEPTED (mock-auth); R004 deferred to Transition; R007 RESOLVED; R008 COMPLETE |
| Evolution | T2 Risk List evolved from T1. RL-F6 (Major) RESOLVED: R003 formally accepted risk with residual stated per STK-001 directive — 8 TCs covered by mock, proven at deployment time, mock-auth expiry 2026-12-31, owner Software Architect. R004 CLOSED — NFR-001 measured 0.14s (threshold 3s) PASS, NFR-002 measured 0.003s (threshold 1s) PASS, production-site validation deferred. R008 CLOSED — all 3 binding conditions met, stakeholder re-review pending. NEW in T2: R011 (HIGH) — cross-artifact data integrity governance gap (MR-T2-002). Stakeholder: "Nobody owns the consistency of a single fact across artifacts." |
| Stakeholder Directive | STK-001: "An accepted risk is a decision; 'unverified' is a wound left open." R003 is a formally accepted risk, not an open verification item. Mock-auth expiry must have a date and owner — "a mock that unblocks 8 tests and has no expiry becomes the permanent implementation." T2: "an ambiguous safeguard is not a safeguard" — date must be canonical, one home, cited from everywhere. |
| Finding RL-F6 | **RESOLVED in T2** — R003 converted to FORMALLY ACCEPTED risk with residual stated; R004 CLOSED with measured values; R008 CLOSED with all 3 binding conditions met. API closure gap noted — explicit resolution may be required. |

## Risk Classification

Risks are classified by **Probability (P) × Impact (I) = Exposure**, yielding a **Magnitude** rating. The scale is 1–3 for both probability and impact, producing exposure values from 1 to 9.

| Exposure | Magnitude | Action |
|---|---|---|
| 9 | HIGH | Must be confronted in the earliest possible iteration; mitigation plan mandatory |
| 6–8 | SIGNIFICANT | Active mitigation required; monitor each iteration |
| 4–5 | MODERATE | Mitigation plan prepared; monitor for escalation |
| 3 | MINOR | Accept with awareness; review each phase |
| 1–2 | LOW | Accept; review if situation changes |

```plantuml
@startuml
title Risk List — T2 Close-Out Classification

skinparam classAttributeIconSize 0

class R001 as "R001\nAD LDAP\nInconsistency" {
  P=3, I=3, Exp=9
  Magnitude: HIGH
  Strategy: Accept
  Status: OPEN
  Mitigation: PoC verified
  in Elaboration
}

class R002 as "R002\nClocking\nAdoption" {
  P=3, I=2, Exp=6
  Magnitude: SIGNIFICANT
  Strategy: Accept
  Status: OPEN
  Mitigation: User docs
  ready, training plan
}

class R003 as "R003\nOIDC\nIntegration" {
  P=2, I=3, Exp=6
  Magnitude: SIGNIFICANT
  Strategy: ACCEPT
  Status: ACCEPTED
  Residual: 8 TCs mock
  Proven at deployment
}

class R004 as "R004\nNFR Performance" {
  P=2, I=3, Exp=6
  Magnitude: SIGNIFICANT
  Strategy: Accept
  Status: CLOSED
  NFR-001: 0.14s PASS
  NFR-002: 0.003s PASS
}

class R005 as "R005\nUI Design\nCompliance" {
  P=2, I=2, Exp=4
  Magnitude: MODERATE
  Strategy: Accept
  Status: OPEN
}

class R006 as "R006\nOffline\nResilience" {
  P=2, I=2, Exp=4
  Magnitude: MODERATE
  Strategy: Accept
  Status: OPEN
}

class R007 as "R007\nCI Build\nIntegrity" {
  P=1, I=3, Exp=3
  Magnitude: MINOR
  Strategy: Accept
  Status: RESOLVED
  CI GREEN
}

class R008 as "R008\nBinding\nConditions" {
  P=2, I=3, Exp=6
  Magnitude: SIGNIFICANT
  Strategy: Accept
  Status: CLOSED
  3 BCs met T2
}

class R009 as "R009\nDeployment\nVerification" {
  P=3, I=2, Exp=6
  Magnitude: SIGNIFICANT
  Strategy: Accept
  Status: OPEN
  No Windows Server
  Declared in Release Notes
}

class R010 as "R010\nAcceptance\nCriteria" {
  P=3, I=3, Exp=9
  Magnitude: HIGH
  Strategy: Accept
  Status: OPEN
  AC-001..005 not
  verified in production
}

class R011 as "R011\nCross-Artifact\nData Integrity" {
  P=3, I=3, Exp=9
  Magnitude: HIGH
  Strategy: MITIGATE
  Status: NEW (T2)
  Root cause: no role owns
  consistency of single fact
  across artifacts
}

R011 --> R003 : relates to\n(mock-auth date)
R010 --> R008 : depends on
R009 --> R010 : blocks

@enduml
```

## Risk Register

| ID | Risk | P | I | Exp | Magnitude | Strategy | Status | Owner | Mitigation | Contingency |
|---|---|---|---|---|---|---|---|---|---|---|
| R001 | AD LDAP attribute inconsistency across 3 offices | 3 | 3 | 9 | HIGH | Accept | OPEN | Project Manager | PoC verified in Elaboration — LDAP attributes readable. Residual: production AD may have gaps in some offices. | Directory shows gaps for some employees — escalate to Infrastructure team (STK-003) to fill AD attributes. |
| R002 | Employees keep using Excel for clocking | 3 | 2 | 6 | SIGNIFICANT | Accept | OPEN | Project Manager | User Documentation publication-ready; training plan documented for post-deployment. | Low adoption after 3 months — HR Director (STK-001) communicates mandate; measure BG-003 adoption rate. |
| R003 | OIDC integration unverified — Keycloak out of scope | 2 | 3 | 6 | SIGNIFICANT | **ACCEPT** | **ACCEPTED** | Software Architect | **FORMALLY ACCEPTED per STK-001 directive.** 8 TCs covered by mock-auth. Mock-auth expiry: 2026-12-31, owner: Software Architect. Residual: proven against real OIDC client at deployment time only. | If real OIDC fails at deployment — debug and fix OIDC client registration with STK-003; mock-auth provides test coverage until then. |
| R004 | NFR-001/NFR-002 performance thresholds not met | 2 | 3 | 6 | SIGNIFICANT | Accept | **CLOSED** | Project Manager | **CLOSED — measured in CI.** NFR-001: 0.14s (threshold 3s) PASS. NFR-002: 0.003s (threshold 1s) PASS. Production-site validation deferred (no Windows Server). | If production performance differs — measure on deployment target and optimize. |
| R005 | UI does not match mandatory design (CON-011) | 2 | 2 | 4 | MODERATE | Accept | OPEN | Designer | Design Model implements CON-011 design. Verified by Code Reviewer. | If gaps found — Designer corrects against docs/inputs/employee-portal-design.html. |
| R006 | Offline resilience (AC-005) fails under network drop | 2 | 2 | 4 | MODERATE | Accept | OPEN | Software Architect | PoC decision recorded in Elaboration — localStorage retry for 5 minutes. Code implemented. | If offline retry fails — fix localStorage persistence and retry logic. |
| R007 | CI build failures block integration | 1 | 3 | 3 | MINOR | Accept | **RESOLVED** | Integrator | **RESOLVED — CI GREEN on main** (run 33263001739). Integrator role mandated in Construction C3. | If CI breaks — Integrator fixes and re-runs. |
| R008 | Binding conditions unmet — PR sanction refused | 2 | 3 | 6 | SIGNIFICANT | Accept | **CLOSED** | Project Manager | **CLOSED — all 3 binding conditions met in T2.** NFR measured, R003 accepted, mock-auth documented. BUT: date inconsistency introduced new risk (R011). | If stakeholder refuses again — T3 must canonicalize mock-auth date and resolve 4 Major findings. |
| R009 | Deployment on Windows Server (CON-006) not performed | 3 | 2 | 6 | SIGNIFICANT | Accept | OPEN | Deployment Manager | No Windows Server environment available. Explicitly stated in Release Notes per STK-001 directive. | When environment becomes available — deploy and verify. |
| R010 | Acceptance criteria (AC-001..AC-005) not verified in production | 3 | 3 | 9 | HIGH | Accept | OPEN | Project Manager | All 10 FRs implemented, CI GREEN, NFRs measured. ACs require production deployment + user interaction. | If ACs fail post-deployment — fix and re-verify; measure BG-003 adoption rate. |
| R011 | **Cross-artifact data integrity — single fact has multiple values** | 3 | 3 | 9 | **HIGH** | **MITIGATE** | **NEW (T2)** | Project Manager | **Root cause:** no role owns consistency of a single fact across artifacts. Mock-auth expiry date appears as 2026-11-29, 2026-12-31, and 2027-01-31 across 7 artifacts with 2 owners. **Mitigation:** T3 establishes ONE canonical value (one home, cited from everywhere, never copied). Process protocol: canonical-value ownership assigned to the role that creates the value; all other artifacts reference it by citation. | If canonicalization fails — stakeholder refuses PR sanction again; T4 required. |

## Risk Mitigation and Contingency

### T2 Close-Out Mitigation Actions

| Risk | Action | Owner | Due | Status |
|---|---|---|---|---|
| R011 | Establish canonical mock-auth expiry date: ONE value, ONE owner, ONE home. All 7 artifacts cite it, never copy it. | Project Manager | T3 | **PENDING** |
| R011 | Define cross-artifact canonical-value protocol for evolution cycle: canonical value has one home, cited from everywhere else | Project Manager | T3 | **PENDING** |
| R003 | Ensure mock-auth expiry date is consistent across Risk List, Release Notes, Test Case, Vision, Supplementary Spec, Review Record, and MockAuthHandler.cs | Software Architect | T3 | **PENDING** |
| R009 | Maintain explicit "NOT PERFORMED" statement in Release Notes for Windows Server deployment | Deployment Manager | Ongoing | **MET** |
| R010 | Plan post-deployment AC verification: AC-001..AC-005 require production environment + user interaction | Project Manager | Post-deployment | **PLANNED** |

### Cumulative Mitigation History

| Risk | Iteration | Action | Result |
|---|---|---|---|
| R001 | Elaboration 1 | PoC: LDAP query against test AD | Attributes readable — residual: production gaps possible |
| R003 | Construction C4 | Mock-auth implemented for 8 TCs | Tests pass — residual: real OIDC unverified |
| R003 | Transition T1 | STK-001 directive: convert to accepted risk | R003 formally accepted in T2 |
| R004 | Transition T2 | Execute NFR-001/NFR-002 load tests in CI | NFR-001: 0.14s PASS, NFR-002: 0.003s PASS |
| R007 | Construction C3 | Integrator role mandated, CI fixes | CI GREEN — RESOLVED |
| R008 | Transition T1 | Close 3 binding conditions | All 3 met in T2 — but date inconsistency introduced R011 |

## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| R001 | CON-005, CON-009, STK-003 | Derives | SAD COMP-003 (LDAP), Architectural PoC |
| R002 | BG-003, AC-004 | Derives | T5 (user docs), T6 (assessment) |
| R003 | CON-004, STK-003, STK-001 binding condition #2 | Derives | SAD COMP-001 (OIDC), Iteration Assessment (formally accepted) |
| R004 | NFR-001, NFR-002, STK-001 binding condition #1 | Derives | T1 (load testing), SAD COMP-006, Iteration Assessment (measured) |
| R005 | CON-011, CON-002 | Derives | Design Model V010, T4 (deployment) |
| R006 | AC-005, SAD Process View | Derives | T4 (deployment), Architectural PoC |
| R007 | Review Record C2 + C4 findings | Derives | CI build (run 33263001739) |
| R008 | Stakeholder sanction (IOC), STK-001 PR refusal | Derives | T6 (assessment), PR milestone, Iteration Assessment |
| R009 | CON-006, CON-007, STK-001 directive | Derives | Release Notes (explicit deployment status) |
| R010 | AC-001..AC-005, BG-003, R008 | Derives | T6 (assessment), PR milestone review |
| R011 | MR-T2-002, RR-F1, STK-001 T2 directive | Derives | T3: canonical mock-auth date, cross-artifact protocol |
| RL-F6 (RESOLVED) | Review Record T1 RL-F6 | Resolved by | R003 formally accepted; R004 measured and CLOSED; R008 CLOSED with 3 BCs met |