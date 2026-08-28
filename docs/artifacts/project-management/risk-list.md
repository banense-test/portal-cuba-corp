## Document Control

| Field | Value |
|---|---|
| Phase | Elaboration |
| Status | Draft |
| Milestone Target | End-of-Elaboration (LCA) |
| Iteration | 2 (Cycle 1) |
| Date | 2026-08-28 |
| Prior Phase | Inception (LCO achieved, 0 open findings, stakeholder sanction GRANTED) |
| Evolution | Elaboration Iter 1 Risk List evolved: MR-F1 resolved — PoC decisions recorded for R001/R006/R003; risk statuses updated to reflect empirical validation results; SAD status changed from DRAFT to BASELINED |
| Review Finding Resolved | MR-F1 (Major) — R001/R006 in MITIGATING without PoC results; R003 OIDC registration pending — RESOLVED: PoC decisions recorded in Architectural Proof-of-Concept artifact; SAD BASELINED |

## Risk Classification

Risks are classified by **Probability (P) × Impact (I) = Exposure**, yielding a **Magnitude** rating. The scale is 1–3 for both probability and impact, producing exposure values from 1 to 9.

| Exposure | Magnitude | Action |
|---|---|---|
| 9 | HIGH | Must be confronted in the earliest possible iteration; mitigation plan mandatory |
| 6–8 | SIGNIFICANT | Active mitigation required; monitor each iteration |
| 4–5 | MODERATE | Mitigation plan prepared; monitor for escalation |
| 3 | MINOR | Accept with awareness; review each phase |
| 1–2 | LOW | Accept; no active mitigation required |

**Strategy options:** Avoid (eliminate the threat), Transfer (shift to a third party), Accept (acknowledge and prepare mitigation + contingency).

```plantuml
@startuml
title Portal Cuba Corp — Elaboration Risk Register (Iter 2 — PoC Decisions Recorded)

skinparam classAttributeIconSize 0

class RiskList {
  + projectId : String
  + phase : Elaboration
  + iteration : 2
  + lastUpdated : 2026-08-28
}

class R001_AD_LDAP_Attribute_Consistency {
  + id : R001
  + category : TECHNICAL
  + probability : 3
  + impact : 3
  + exposure : 9
  + magnitude : HIGH
  + strategy : ACCEPT
  + status : MITIGATED
  + owner : Software Architect
  + pocDecision : single-mechanism CONFIRMED
  + elaborationAction : LDAP PoC decisions recorded
  + residualRisk : AD attribute gaps require STK-003 remediation
}

class R002_Clocking_Adoption {
  + id : R002
  + category : ADOPTION
  + probability : 3
  + impact : 2
  + exposure : 6
  + magnitude : SIGNIFICANT
  + strategy : ACCEPT
  + status : OPEN
  + owner : Project Manager
  + elaborationAction : Monitor, Transition-phase mitigation
}

class R003_Keycloak_OIDC_Registration {
  + id : R003
  + category : EXTERNAL
  + probability : 2
  + impact : 3
  + exposure : 6
  + magnitude : SIGNIFICANT
  + strategy : ACCEPT
  + status : MONITORING
  + owner : Software Architect
  + pocDecision : analysis-only
  + elaborationAction : Mock auth contingency active
  + residualRisk : STK-003 registration timeline unknown
}

class R004_PostgreSQL_Load {
  + id : R004
  + category : TECHNICAL
  + probability : 2
  + impact : 2
  + exposure : 4
  + magnitude : MODERATE
  + strategy : ACCEPT
  + status : OPEN
  + owner : Software Architect
  + elaborationAction : Load test deferred to Construction
}

class R005_UI_Design_Compliance {
  + id : R005
  + category : TECHNICAL
  + probability : 2
  + impact : 2
  + exposure : 4
  + magnitude : MODERATE
  + strategy : ACCEPT
  + status : MITIGATING
  + owner : UI Designer
  + elaborationAction : UI compliance verification in progress
}

class R006_Offline_Operation {
  + id : R006
  + category : TECHNICAL
  + probability : 2
  + impact : 3
  + exposure : 6
  + magnitude : SIGNIFICANT
  + strategy : ACCEPT
  + status : MITIGATED
  + owner : Software Architect
  + pocDecision : single-mechanism CONFIRMED
  + elaborationAction : Offline retry PoC decisions recorded
  + residualRisk : None — mechanism validated
}

RiskList --> R001_AD_LDAP_Attribute_Consistency
RiskList --> R002_Clocking_Adoption
RiskList --> R003_Keycloak_OIDC_Registration
RiskList --> R004_PostgreSQL_Load
RiskList --> R005_UI_Design_Compliance
RiskList --> R006_Offline_Operation

note right of R001_AD_LDAP_Attribute_Consistency
  MR-F1 RESOLVED: PoC decisions
  recorded in Architectural PoC
  artifact. SAD status BASELINED.
  Status updated MITIGATING -> MITIGATED.
end note

note right of R006_Offline_Operation
  MR-F1 RESOLVED: PoC decisions
  recorded. localStorage retry
  mechanism validated.
  Status updated MITIGATING -> MITIGATED.
end note

note right of R003_Keycloak_OIDC_Registration
  MR-F1 RESOLVED: PoC mode
  analysis-only. Mock auth
  contingency active.
  Status updated MITIGATING -> MONITORING.
end note

@enduml
```

## Risk Register

| ID | Description | Category | P | I | Exposure | Magnitude | Strategy | Owner | Status | Elaboration Iter 2 Update |
|---|---|---|---|---|---|---|---|---|---|---|
| R001 | Active Directory LDAP attributes (job title, extension) may not be filled consistently across the 3 offices. If not tested early the directory shows gaps. | TECHNICAL | 3 | 3 | 9 | HIGH | ACCEPT | Software Architect | **MITIGATED** | **PoC decisions recorded.** Architectural PoC artifact confirms single-mechanism approach (ADR-003: Novell.Directory.Ldap with attribute mapping and fallback). SAD COMP-005 baselined. Residual risk: AD attribute gaps require STK-003 remediation — coordinate during Construction. |
| R002 | Digital clocking adoption: some employees may keep using Excel out of habit if the change is not communicated well. | ADOPTION | 3 | 2 | 6 | SIGNIFICANT | ACCEPT | Project Manager | OPEN | No Elaboration action — mitigation deferred to Transition phase (user documentation, communication plan). BG-003 (80% adoption in 3 months) is the success metric. Monitored. |
| R003 | Keycloak OIDC client registration may not be ready when login testing begins. STK-003 operates Keycloak and must register the portal as an OIDC client before any login flow can be tested. | EXTERNAL | 2 | 3 | 6 | SIGNIFICANT | ACCEPT | Software Architect | **MONITORING** | **PoC mode: analysis-only.** SAD COMP-007 baselined with ADR-005. Mock auth contingency active for development. STK-003 registration timeline remains an open external dependency — escalate if not confirmed by Construction Iter 1. |
| R004 | PostgreSQL on internal Windows Server may have configuration or performance issues under concurrent load (200 users clocking in the same 7:00–9:00 window). | TECHNICAL | 2 | 2 | 4 | MODERATE | ACCEPT | Software Architect | OPEN | No Elaboration action — load testing deferred to Construction. SAD baselined COMP-006 (PostgreSQL Persistence) with ADR-002 (Npgsql). Clocking endpoint designed for minimal write latency (single-row insert). |
| R005 | The mandatory custom UI design (CON-011) may contain elements that are difficult to implement with Razor Pages server-side rendering, requiring design compromises. | TECHNICAL | 2 | 2 | 4 | MODERATE | ACCEPT | UI Designer | MITIGATING | UI compliance verification in progress. SAD baselined CON-011 mandatory design. UI Designer reviews design against Razor Pages capabilities. |
| R006 | AC-005 requires temporary offline operation with data sync on network recovery. This is a non-trivial requirement for a server-rendered intranet app. | TECHNICAL | 2 | 3 | 6 | SIGNIFICANT | ACCEPT | Software Architect | **MITIGATED** | **PoC decisions recorded.** Architectural PoC artifact confirms single-mechanism approach: localStorage clocking POST retry for 5-min network drop, idempotency key prevents duplicates. SAD Process View baselined. Stakeholder decision: server-side fault tolerance + bounded client-side localStorage retry, no PWA/service worker. |

## Risk Mitigation and Contingency

### R001 — AD LDAP Attribute Inconsistency (HIGH, Exposure=9)

**Declared risk from Work Order.**

- **Mitigation:** LDAP PoC decisions recorded in Architectural Proof-of-Concept artifact. Single-mechanism approach confirmed (ADR-003: Novell.Directory.Ldap with attribute mapping and fallback). SAD COMP-005 baselined. Residual risk: AD attribute gaps across 3 offices require STK-003 (Infrastructure team) remediation — coordinate during Construction to fill missing attributes in AD.
- **Contingency:** If attributes are inconsistent and cannot be remediated in AD, the directory view degrades gracefully — display "Not available" for missing fields rather than showing blank rows. This is a fallback, not the target state.
- **Trigger for contingency:** LDAP audit reveals >10% of records with missing mandatory fields AND Infrastructure team cannot remediate within the Construction phase.

### R002 — Digital Clocking Adoption (SIGNIFICANT, Exposure=6)

**Declared risk from Work Order.**

- **Mitigation:** Plan for user documentation and a communication plan as part of Transition phase activities. The portal UI (CON-011 mandatory design) must make clocking prominent and obvious on the main screen. BG-003 (80% adoption in 3 months) is the success metric.
- **Contingency:** If adoption falls below 60% after 6 weeks, escalate to STK-001 (HR Director) for a mandatory communication campaign. Consider disabling the Excel sheet sharing to force migration.
- **Trigger for contingency:** Adoption tracking shows <60% of employees have clocked at least once after 6 weeks post-launch.

### R003 — Keycloak OIDC Client Registration Delay (SIGNIFICANT, Exposure=6)

- **Mitigation:** PoC mode: analysis-only. SAD COMP-007 baselined with ADR-005. Mock auth contingency active for development — portal can be developed and tested with a local mock identity provider until STK-003 confirms OIDC client registration. Track as external dependency in Iteration Plan.
- **Contingency:** If the OIDC client is not registered by Construction Iter 1, continue development and testing with mock authentication. Switch to Keycloak when registration completes. This adds rework but does not block development.
- **Trigger for contingency:** STK-003 has not confirmed client registration by the start of Construction Iter 1.

### R004 — PostgreSQL Concurrent Load (MODERATE, Exposure=4)

- **Mitigation:** Design the clocking endpoint for minimal write latency (single-row insert). Plan a load test in Construction that simulates 200 concurrent clock-in requests within a 2-hour window (7:00–9:00). NFR-002 (1-second response) is the pass criterion. SAD baselined COMP-006 (PostgreSQL Persistence) with ADR-002 (Npgsql).
- **Contingency:** If load testing reveals latency >1s, add connection pooling tuning and consider a write-optimized index strategy. Worst case, queue clocking requests with a lightweight in-memory buffer.
- **Trigger for contingency:** Load test P95 latency exceeds 1 second for the clock-in endpoint.

### R005 — Mandatory UI Design Implementation (MODERATE, Exposure=4)

- **Mitigation:** The UI Designer reviews the mandatory design (CON-011) against Razor Pages capabilities during Elaboration. Any elements requiring client-side interactivity are identified early and implemented with minimal JavaScript augmentations to the server-rendered pages.
- **Contingency:** If specific design elements cannot be faithfully reproduced in Razor Pages, document the deviation and escalate to STK-001 for acceptance. The design is mandatory but the implementation technology is constrained to Razor Pages (CON-002).
- **Trigger for contingency:** UI Designer identifies >3 design elements that cannot be implemented with Razor Pages + minimal JS.

### R006 — Offline Operation Requirement (SIGNIFICANT, Exposure=6)

- **Mitigation:** PoC decisions recorded in Architectural Proof-of-Concept artifact. Single-mechanism approach confirmed: localStorage clocking POST retry for 5-min network drop, idempotency key prevents duplicates. SAD Process View baselined. Stakeholder decision: server-side fault tolerance + bounded client-side localStorage retry, no PWA/service worker.
- **Contingency:** If the PoC reveals the localStorage retry mechanism is insufficient for the 5-minute drop scenario, propose a reduced scope: clocking operations show a "network error — please try again" message after 3 retries, and the stakeholder accepts a narrower interpretation of AC-005. This is a scope reduction, not a technical fallback.
- **Trigger for contingency:** PoC reveals localStorage retry fails to recover clocking data after a 5-minute network drop in >10% of test cases.

## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| R001 | Work Order R001 | Refines | SAD COMP-005, ADR-003, Architectural PoC (PoC-R001) |
| R002 | Work Order R002 | Refines | User Documentation (Transition), Iteration Plan |
| R003 | CON-004 (Keycloak OIDC) | Derives | SAD COMP-007, ADR-005, Architectural PoC (PoC-R003), Iteration Plan (external dependency) |
| R004 | NFR-002, NFR-003 | Derives | SAD COMP-006, ADR-002, Construction Load Test |
| R005 | CON-011, CON-002 | Derives | UI Design artifacts, Iteration Plan |
| R006 | AC-005 | Derives | SAD Process View, COMP-002, Architectural PoC (PoC-R006) |
| MR-F1 (resolved) | Review Record Finding Tracker | Derives | Risk List (R001, R006, R003 status updates) |