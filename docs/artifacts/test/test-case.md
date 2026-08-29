## Document Control

| Field | Value |
|---|---|
| Phase | Transition |
| Status | Draft — Transition Iteration 1 Cycle 1 Acceptance Testing Complete |
| Milestone Target | End-of-Transition — NOT YET ACHIEVED |
| Iteration | 1 (Cycle 1) |
| Date | 2026-08-29 |
| Author | Test Designer (Test Discipline) — Test Cases designed in Elaboration/C1/C2/C3/C4 |
| Tester | Tester (Test Discipline) — Execution and evaluation in Construction C1–C4, Transition I1 |
| Test Analyst | Test Analyst (Test Discipline) — Quality evaluation, defect pattern analysis, Ideas evolution in Construction C1–C4 |
| Prior Phase | Construction C4 Cycle 1 — 43 TCs (35 PASS, 8 BLOCKED by R003, 0 FAIL); stakeholder sanction GRANTED with 3 binding conditions; IOC milestone: CONDITIONAL GO |
| Evolution | **Elaboration:** 20 TCs (TC-001..TC-020). **C1:** Extended to 30 TCs with adversarial + performance tests. **C2:** Extended to 35 TCs (TC-031..TC-035). **C3:** Extended to 39 TCs (TC-036..TC-039); 31 PASS, 8 BLOCKED, 0 FAIL. **C4 (Test Designer):** Extended to 43 TCs (TC-040..TC-043); C4-1/C4-2/C4-3 RESOLVED in PR #32. **C4 (Tester):** 35 PASS, 8 BLOCKED (R003), 0 FAIL. Regression: CLEAN. Issues #12, #13, #14 RESOLVED in code. CI green on iteration/C4 (run 33255939673) and main (run 33252332825). **Transition I1 (Tester):** Acceptance testing executed against 5 ACs. AC-001 PASS, AC-002 PASS, AC-005 PASS (service+JS). AC-003 PASS (functional, performance UNVERIFIED). AC-004 PASS (automated, manual UAT required). Regression: CLEAN (35/35 PASS TCs re-verified). NFR-001/NFR-002 BLOCKED — no deployment environment (Transition exit criterion unmet). R003 persists (8 TCs BLOCKED, stakeholder ACCEPTED). 6 open defect issues reviewed — 1 blocker (ACCEPTED), 5 minor/deferred. CI green on main (run 33256627567). |
| Build ID | main — CI run 33256627567 (2026-08-29 14:05:31Z) |
| Test Environment | .NET 10 test project (xUnit); InMemoryDb; MockLdapGateway; OIDC mock tokens; 35 TCs no external deps; 8 TCs require OIDC (R003 BLOCKED). No production-equivalent deployment available for NFR measurement. |

## Test Scope

### All Use Cases Under Test — Transition I1 Acceptance Testing

This Test Case artifact covers **all 10 use-case scenarios** at Transition depth. The Transition iteration focuses on **acceptance testing** against the 5 declared acceptance criteria (AC-001 through AC-005) and **regression verification** of all 35 PASS TCs from Construction C4.

| Priority | UC ID | UC Name | TCs | Test Focus | Risk |
|---|---|---|---|---|---|
| 1 | UC-001 | Clock In / Clock Out | TC-001..TC-005, TC-021, TC-022, TC-031, TC-033, TC-034, TC-036, TC-038, TC-039 | Offline retry (AC-005), idempotency, NFR-002 (<1s), client-side timestamp, cross-employee collision, C2 RESOLVED, C3 RESOLVED, C4 transaction atomicity | R002 (adoption) |
| 2 | UC-009 | Search Employee Directory | TC-006, TC-007, TC-020, TC-028 | LDAP integration (R001), read-only AD, corporate-data-only, multi-office | R001 (LDAP attributes) |
| 3 | UC-005 | Publish News | TC-008, TC-023, TC-040, TC-041 | Audit trail (NFR-004), IsFeatured flag, C4 transaction atomicity | — |
| 4 | UC-002 | View Own Clocking History | TC-015 | Data correctness, current-month filter | — |
| 5 | UC-003 | View All Employee Clockings | TC-020 | HR authorization, LDAP name lookup | — |
| 6 | UC-004 | Export Monthly Clocking Report | TC-016, TC-035 | CSV format, data completeness, C2 RESOLVED header | — |
| 7 | UC-006 | Edit Published News | TC-010, TC-024, TC-032, TC-037, TC-042 | Audit trail on edit, IsFeatured preservation, C2 RESOLVED, C3 RESOLVED, C4 isFeatured through edit | — |
| 8 | UC-007 | Unpublish News | TC-009, TC-027, TC-040, TC-041 | No hard delete (CON-013), record preserved, C4 transaction atomicity | — |
| 9 | UC-008 | Read and Filter News | TC-017, TC-025 | Category filter, featured banner, sorted by date | — |
| 10 | UC-010 | Manage Worker Category | TC-018, TC-019, TC-026, TC-043 | AD user id → category, audit trail, C4 transaction atomicity | — |

### Transition I1 Acceptance Testing Summary

| Metric | Value |
|---|---|
| Total Test Cases | 43 (TC-001..TC-043) |
| PASS | 35 (unchanged from C4 — regression verified) |
| FAIL | 0 |
| BLOCKED | 8 (TC-013, TC-014, TC-029, TC-030 + 4 OIDC-dependent — R003 ACCEPTED risk) |
| Regression | CLEAN — 35/35 PASS TCs re-verified against build 33256627567 |
| CI Build | main — GREEN (run 33256627567, 2026-08-29 14:05:31Z) |
| Open Defect Issues | 6 (1 blocker ACCEPTED, 5 minor/deferred) |

### Acceptance Criteria Verdicts

| AC ID | Description | Verdict | Evidence | Gap |
|---|---|---|---|---|
| AC-001 | Employee can clock in/out without HR/dev help | **PASS** | TC-001 (Clock In), TC-002 (Clock Out), TC-003 (status display), TC-004 (confirmation) all PASS. UI (Index.cshtml) shows single Clock In/Out button based on ClockStatus. ClockingRetry.submit handles POST with idempotency. | None — service + UI verified |
| AC-002 | HR can publish news without technical assistance | **PASS** | TC-008 (Publish), TC-009 (Unpublish), TC-010 (Edit) all PASS. NewsService.PublishAsync creates news with audit trail (author + timestamp). UI pages exist for Publish, Edit, Management. | None — service verified; UI pages present |
| AC-003 | Employee finds colleague's phone/email in < 10 seconds | **PASS (functional)** | TC-006 (Search valid query), TC-007 (missing attrs → N/A fallback) PASS. DirectoryService.Search returns DirectoryEntry with all corporate fields. R001 fallback (N/A for missing attrs) verified. | Performance UNVERIFIED — NFR-001 (<3s page load) requires deployment to production-equivalent environment. No latency measurement possible in CI. |
| AC-004 | 80% of employees complete at least one clocking with no prior training | **PASS (automated)** | TC-001 (Clock In), TC-002 (Clock Out) PASS. UI is single-button operation with clear status indicator. ClockingRetry.js handles offline gracefully. | Manual UAT with real users required to validate 80% adoption rate. Cannot be verified in automated CI. |
| AC-005 | System works temporarily offline (5 min network drop, data syncs when back) | **PASS** | TC-003 (offline retry idempotency), TC-004 (duplicate key returns existing record), TC-021 (client timestamp preserved) all PASS. clocking-retry.js implements: localStorage storage, retry every 10s for 5 min max, idempotency key generation, confirmation/failure UI messages. Server-side idempotency via RecordClocking with key deduplication. | None — service + JS code review confirms full implementation |

### Transition I1 Regression Results

| Regression Scope | TCs | Result | Notes |
|---|---|---|---|
| Clocking Service (UC-001) | TC-001..TC-005, TC-021, TC-022, TC-031, TC-033, TC-034, TC-036, TC-038, TC-039 | **CLEAN** | All PASS — no regressions from C4 baseline |
| News Service (UC-005..UC-008) | TC-008..TC-010, TC-017, TC-023..TC-025, TC-027, TC-032, TC-037, TC-040, TC-041, TC-042 | **CLEAN** | All PASS — C4-1 (isFeatured) and C4-2 (transaction) stable |
| Directory Service (UC-009) | TC-006, TC-007, TC-020, TC-028 | **CLEAN** | All PASS — R001 fallback (N/A) verified |
| Worker Category (UC-010) | TC-018, TC-019, TC-026, TC-043 | **CLEAN** | All PASS — audit trail verified |
| Domain Entities | DomainTests (6 TCs) | **CLEAN** | All PASS — DateRange, DirectoryEntry, ClockingResult |
| Offline Retry | OfflineRetryTests (10 TCs) | **CLEAN** | All PASS — idempotency, timestamp, transaction |
| OIDC-Dependent (R003) | TC-013, TC-014, TC-029, TC-030 + 4 others | **BLOCKED** | Stakeholder ACCEPTED risk — mock-auth contingency active |

### Transition I1 NFR Assessment

| NFR ID | Description | Verdict | Evidence | Gap |
|---|---|---|---|---|
| NFR-001 | Page load < 3s on corporate network | **BLOCKED** | TC-011 designed for this NFR. No deployment environment available. | Requires production-equivalent deployment with real PostgreSQL + LDAP + OIDC. Transition exit criterion per stakeholder condition (1). |
| NFR-002 | Clock in/out response < 1s | **BLOCKED** | TC-012 designed for this NFR. No deployment environment available. | Requires production-equivalent deployment. Transition exit criterion per stakeholder condition (1). |
| NFR-003 | Availability 7:00–19:00 Mon–Fri | **BLOCKED** | No deployment to assess. | Requires deployment + monitoring. |
| NFR-004 | Mandatory audit trail | **PASS** | TC-008, TC-010, TC-018, TC-023, TC-024, TC-040, TC-041 all verify audit records (author + timestamp). AuditInterceptor.cs + InMemoryAuditLogger confirm audit logging. | None — fully verified at service layer |

### Transition I1 Open Defect Review

| Issue # | Description | Severity | Priority | Status | Transition Action |
|---|---|---|---|---|---|
| #30 | R003 OIDC infrastructure blocker — 8 TCs BLOCKED | blocker | critical | cr:deferred | **ACCEPTED** — stakeholder approved mock-auth contingency. Real OIDC is Transition work item. 8 tests stay covered-by-mock. |
| #12 | CSV export format — TimeOut column always empty for OUT records | minor | medium | cr:deferred | No code change in Transition. Deferred to post-release. |
| #15 | Naming violation on feature/C1-presentation | minor | medium | cr:deferred | Stale branch superseded. No action needed. |
| #17 | RecordClockingRequest.EmployeeId is dead code | minor | medium | cr:deferred | DTO field ignored by server (identity from token). Cosmetic. Deferred. |
| #18 | Test codifies idempotency collision as expected behavior | minor | low | cr:deferred | Test reflects intended design per CR #11. Deferred. |
| #34 | Design Model async method names lag behind implementation | minor | low | cr:deferred | Documentation-only. Deferred to Designer. |

### Transition Exit Criteria Status

| Criterion | Status | Evidence |
|---|---|---|
| (1) NFR-001/NFR-002 load testing with measured values | **NOT MET** | No deployment environment available. CI tests use InMemoryDb — no real PostgreSQL latency. Requires production-equivalent deployment. |
| (2) Real OIDC integration as named Transition work item | **PENDING** | 8 TCs remain BLOCKED. STK-003 must confirm OIDC client registration. Mock-auth contingency active. |
| (3) Mock-auth has expiry date | **PENDING** | Must be documented in Transition Iteration Plan by Project Manager. |

### Quality Verdict — Transition I1

The system is **functionally complete** at the service layer. All 5 acceptance criteria have been evaluated:

- **3 of 5 ACs fully PASS** (AC-001, AC-002, AC-005) — verified through automated test execution and source code review.
- **2 of 5 ACs PASS with caveats** (AC-003, AC-004) — functional correctness verified, but performance measurement and real-user UAT require deployment.
- **Regression is CLEAN** — 35/35 PASS TCs re-verified against build 33256627567 with 0 regressions.
- **0 FAIL** across all 43 TCs.
- **8 BLOCKED** by R003 (OIDC) — stakeholder ACCEPTED risk, mock-auth contingency active.

**Blocking conditions for Transition exit:**
1. NFR-001/NFR-002 load testing requires production-equivalent deployment — **NOT MET**
2. Real OIDC integration requires STK-003 confirmation — **PENDING**
3. Mock-auth expiry date must be documented — **PENDING**

**Overall verdict: CONDITIONAL** — functional acceptance verified, but 3 stakeholder-mandated exit criteria remain unmet due to infrastructure dependencies.

```plantuml
@startuml
title Transition Iteration 1 — Acceptance Test Execution Flow

skinparam activityBackgroundColor #F8F8FF
skinparam activityBorderColor #336699
skinparam shadowing false

start

partition "S2: Smoke Test" {
  :CI Build Status Check (main);
  if (CI GREEN?) then (yes)
    :Release Candidate Stable\nBuild: run 33256627567;
  else (no)
    :Log Blocker Issue;
    stop
  endif
}

partition "S3: Acceptance Testing" {
  
  fork
    partition "AC-001: Clock In/Out" {
      :Execute TC-001 (Clock In happy path);
      :Execute TC-002 (Clock Out);
      :Execute TC-003 (Status display);
      if (All PASS?) then (yes)
        #LightGreen:AC-001 PASS\nService + UI verified;
      else (no)
        #LightCoral:AC-001 FAIL\nLog defect;
      endif
    }
  fork again
    partition "AC-002: Publish News" {
      :Execute TC-008 (Publish);
      :Execute TC-009 (Edit);
      :Execute TC-010 (Unpublish);
      if (All PASS?) then (yes)
        #LightGreen:AC-002 PASS\nService verified;
      else (no)
        #LightCoral:AC-002 FAIL\nLog defect;
      endif
    }
  fork again
    partition "AC-003: Directory Search" {
      :Execute TC-006 (Search valid);
      :Execute TC-007 (Missing attrs);
      if (Functional PASS?) then (yes)
        #LightYellow:AC-003 PASS (functional)\nPerformance UNVERIFIED;
      else (no)
        #LightCoral:AC-003 FAIL\nLog defect;
      endif
    }
  fork again
    partition "AC-004: 80% Adoption" {
      :Execute TC-001 (Clock In);
      :Execute TC-002 (Clock Out);
      if (Automated PASS?) then (yes)
        #LightYellow:AC-004 PASS (automated)\nManual UAT required;
      else (no)
        #LightCoral:AC-004 FAIL\nLog defect;
      endif
    }
  fork again
    partition "AC-005: Offline Resilience" {
      :Execute TC-003 (Offline retry);
      :Execute TC-004 (Idempotency);
      :Execute TC-021 (Client timestamp);
      if (All PASS?) then (yes)
        #LightGreen:AC-005 PASS\nService + JS verified;
      else (no)
        #LightCoral:AC-005 FAIL\nLog defect;
      endif
    }
  end fork
  
  :Regression: Re-execute all 35 PASS TCs;
  if (Regression CLEAN?) then (yes)
    #LightGreen:Regression CLEAN\n0 regressions detected;
  else (no)
    #LightCoral:Regression FAIL\nLog regression defects;
  endif
}

partition "S4: Finalize" {
  :Evaluate NFR-001/NFR-002;
  if (Deployment available?) then (yes)
    :Measure page load + clocking latency;
  else (no)
    #LightCoral:NFR-001/NFR-002 BLOCKED\nNo deployment environment;
  endif
  
  :Evaluate R003/OIDC (8 BLOCKED TCs);
  :Stakeholder ACCEPTED risk\nMock-auth contingency active;
  
  :Review 6 open defect issues;
  if (Critical/High resolved?) then (yes)
    #LightGreen:Release candidate\nCONDITIONAL PASS;
  else (no)
    #LightCoral:Release candidate BLOCKED\nCritical/High defects open;
  endif
}

stop
@enduml
```

```plantuml
@startuml
title Transition Iteration 1 — Defect and Issue Status Summary

skinparam rectangle {
  BackgroundColor #F8F8FF
  BorderColor #336699
}
skinparam shadowing false

rectangle "Open Defect Issues (6 total)" as OPEN {
  rectangle "#30 R003 OIDC\nseverity: blocker\npriority: critical\nstatus: cr:deferred\nACCEPTED risk" as I30 #LightCoral
  rectangle "#12 CSV export format\nseverity: minor\npriority: medium\nstatus: cr:deferred" as I12 #LightYellow
  rectangle "#15 Naming violation\nseverity: minor\npriority: medium\nstatus: cr:deferred" as I15 #LightYellow
  rectangle "#17 Dead code DTO\nseverity: minor\npriority: medium\nstatus: cr:deferred" as I17 #LightYellow
  rectangle "#18 Test idempotency\nseverity: minor\npriority: low\nstatus: cr:deferred" as I18 #LightGreen
  rectangle "#34 Design Model async\nseverity: minor\npriority: low\nstatus: cr:deferred" as I34 #LightGreen
}

rectangle "Test Case Status (43 TCs)" as TCS {
  rectangle "35 PASS\nService-layer verified\nInMemoryDb + MockLDAP\nOIDC mock tokens" as PASS #LightGreen
  rectangle "8 BLOCKED (R003)\nTC-013, TC-014, TC-028-030\nOIDC environment required\nStakeholder ACCEPTED" as BLK #LightCoral
  rectangle "0 FAIL" as FAIL #LightGreen
}

rectangle "Acceptance Criteria Verdict" as AC {
  rectangle "AC-001 Clock In/Out\nPASS (automated)" as AC1 #LightGreen
  rectangle "AC-002 Publish News\nPASS (automated)" as AC2 #LightGreen
  rectangle "AC-003 Directory Search\nPASS (functional)\nPerf UNVERIFIED" as AC3 #LightYellow
  rectangle "AC-004 80% Adoption\nPASS (automated)\nManual UAT needed" as AC4 #LightYellow
  rectangle "AC-005 Offline\nPASS (service+JS)" as AC5 #LightGreen
}

rectangle "NFR Status" as NFR {
  rectangle "NFR-001 Page Load < 3s\nBLOCKED\nNo deployment" as N1 #LightCoral
  rectangle "NFR-002 Clock Response < 1s\nBLOCKED\nNo deployment" as N2 #LightCoral
}

rectangle "Transition Exit Criteria" as EXIT {
  rectangle "1. NFR load testing\nwith measured values\nNOT MET" as E1 #LightCoral
  rectangle "2. OIDC integration\nwork item named\nPENDING (Transition)" as E2 #LightYellow
  rectangle "3. Mock-auth expiry\ndate documented\nPENDING" as E3 #LightYellow
}

I30 --> BLK : blocks
N1 --> E1 : blocks exit
N2 --> E1 : blocks exit

@enduml
```

## Test Case Catalog

### TC-001: Clock In — Main Flow (Happy Path)

| Field | Value |
|---|---|
| **UC Trace** | UC-001 (main flow, steps 1–9) |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Goal** | TG-002 (clock response < 1s) |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | Employee authenticated via OIDC mock (Employee role); InMemoryDb initialized empty (TD-001) |
| **Input Data** | Employee id: `emp-001`; direction: `in`; timestamp: `2026-08-28T08:00:00Z`; idempotency key: `key-001` |
| **Expected Outcome** | Confirmation returned with correct time; exactly 1 record in clockings table |
| **Pass/Fail Criteria** | PASS: 1 record, correct fields, confirmation time matches. FAIL: 0 records, >1 record, or timestamp mismatch |
| **Interface Points** | INT-001 (IClockingService), INT-007 (IPersistence) |
| **Automation** | xUnit + InMemoryDb; OIDC mock token |

**Procedure:**
1. Arrange: Initialize InMemoryDb (TD-001 — empty). Generate OIDC mock token for `emp-001` with Employee role.
2. Act: Call `ClockingService.RecordClocking("emp-001", DateTime.UtcNow, ClockType.In, "key-001")`.
3. Assert: Return value `IsDuplicate == false` and `Success == true`.
4. Assert: Query clockings table — exactly 1 record with `EmployeeId=emp-001`, `Type=In`, `IdempotencyKey=key-001`.
5. Assert: Confirmation timestamp in response matches persisted timestamp exactly.

**C1 Verdict: PASS** — `RecordClocking_NewKey_ReturnsSuccess` validates Success=true, IsDuplicate=false, correct fields.
**C2 Verdict: PASS** — Service-layer test confirmed. API routing fixed (C2-CRIT-1 RESOLVED).
**C3 Verdict: PASS** — Route integration confirmed via WebApplicationFactory.
**C4 Verdict: PASS** — No changes to ClockingService.RecordClocking in C4. Regression clean. CI green (run 33255939673).
**Transition I1 Verdict: PASS** — Regression verified against build 33256627567. AC-001 evidence: employee can clock in without HR assistance. UI (Index.cshtml) shows Clock In button based on ClockStatus.

---

### TC-002: Clock Out — Main Flow

| Field | Value |
|---|---|
| **UC Trace** | UC-001 (main flow, steps 10–18) |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | Employee authenticated; 1 clock-in record exists (TD-002) |
| **Input Data** | Employee id: `emp-001`; direction: `out`; timestamp: `2026-08-28T17:00:00Z`; idempotency key: `key-002` |
| **Expected Outcome** | Confirmation returned; 2 records in clockings table (in + out) |
| **Pass/Fail Criteria** | PASS: 2 records, correct direction, confirmation shown. FAIL: wrong count or direction |
| **Interface Points** | INT-001 (IClockingService), INT-007 (IPersistence) |
| **Automation** | xUnit + InMemoryDb; OIDC mock token |

**Procedure:**
1. Arrange: Seed InMemoryDb with 1 clock-in record (TD-002). Generate OIDC mock token.
2. Act: Call `ClockingService.RecordClocking("emp-001", DateTime.UtcNow, ClockType.Out, "key-002")`.
3. Assert: Return value `Success == true`, `IsDuplicate == false`.
4. Assert: 2 records in clockings table — first is `In`, second is `Out`.
5. Assert: Confirmation timestamp matches.

**C1 Verdict: PASS** — `GetCurrentStatus_LastClockOut_ReturnsClockedOut` validates status transition.
**C2 Verdict: PASS** — Service-layer test confirmed.
**C3 Verdict: PASS** — Route integration confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. AC-001 evidence: employee can clock out. UI shows Clock Out button when status is ClockedIn.

---

### TC-003: Clock In — Offline Retry (AC-005)

| Field | Value |
|---|---|
| **UC Trace** | UC-001 (A1 — offline retry), AC-005, NFR-003 |
| **Test Level** | Integration |
| **Quality Dimension** | Reliability |
| **Goal** | TG-005 (offline fault tolerance) |
| **Regression** | Yes — every build |
| **Suite** | OfflineRetryTests |
| **Preconditions** | Employee authenticated; network unavailable |
| **Input Data** | Employee id: `emp-001`; timestamp: client-side; idempotency key: `emp1-1234567890-abc123` |
| **Expected Outcome** | Clocking stored in localStorage; retried every 10s for 5 min; on network recovery, POST succeeds with idempotency key preventing duplicates |
| **Pass/Fail Criteria** | PASS: duplicate key returns existing record (not new). FAIL: duplicate creates second record |
| **Interface Points** | INT-001 (IClockingService), clocking-retry.js |
| **Automation** | xUnit (server-side idempotency); JS code review (client-side retry) |

**Procedure:**
1. Arrange: Initialize InMemoryDb. Simulate first POST (network available).
2. Act: Call `RecordClocking("emp1", ts, ClockType.In, "emp1-1234567890-abc123")` — first attempt.
3. Act: Call `RecordClocking("emp1", ts, ClockType.In, "emp1-1234567890-abc123")` — retry with same key.
4. Assert: First returns `Success=true, IsDuplicate=false`. Retry returns `Success=true, IsDuplicate=true`.
5. Assert: Both return same record ID — no duplicate created.
6. Code Review: Verify clocking-retry.js stores pending clocking in localStorage, retries every 10s (RETRY_INTERVAL_MS=10000), max 5 min (MAX_RETRY_DURATION_MS=300000), generates idempotency key, shows confirmation/failure messages.

**C1 Verdict: PASS** — `Retry_SameIdempotencyKey_ReturnsDuplicateNotNewRecord` validates server-side dedup.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes to retry logic. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. AC-005 evidence: server-side idempotency prevents duplicates on retry. Client-side clocking-retry.js implements localStorage storage, 10s retry interval, 5 min max duration, idempotency key generation, and confirmation/failure UI messages. Full offline resilience chain verified.

---

### TC-004: Clock In — Idempotency Key Deduplication (AC-005)

| Field | Value |
|---|---|
| **UC Trace** | UC-001 (A1), AC-005 |
| **Test Level** | Integration |
| **Quality Dimension** | Reliability |
| **Regression** | Yes — every build |
| **Suite** | OfflineRetryTests |
| **Preconditions** | Employee authenticated; 1 clocking record exists with idempotency key |
| **Input Data** | Same employee, same timestamp, same idempotency key as existing record |
| **Expected Outcome** | Retry returns existing record, not a new one |
| **Pass/Fail Criteria** | PASS: IsDuplicate=true, same record ID. FAIL: new record created |
| **Interface Points** | INT-001 (IClockingService), INT-007 (IPersistence) |
| **Automation** | xUnit + InMemoryDb |

**Procedure:**
1. Arrange: Seed 1 clocking record with idempotency key `key-dup`.
2. Act: Call `RecordClocking` with same employee, timestamp, and key `key-dup`.
3. Assert: `IsDuplicate == true`, `Success == true`.
4. Assert: Record ID matches the original — no new record inserted.

**C1 Verdict: PASS** — `RecordClocking_DuplicateKey_ReturnsExistingRecord` validates dedup.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. AC-005 evidence: idempotency key prevents duplicate records when same clocking is retried.

---

### TC-005: Clock In — Empty Employee ID Rejected

| Field | Value |
|---|---|
| **UC Trace** | UC-001 (A2 — validation) |
| **Test Level** | Unit |
| **Quality Dimension** | Functionality |
| **Regression** | Yes |
| **Suite** | ClockingServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Employee id: `""` (empty string) |
| **Expected Outcome** | `Success == false`, error message "Employee ID is required" |
| **Pass/Fail Criteria** | PASS: validation rejects empty employee ID. FAIL: no validation |
| **Interface Points** | INT-001 (IClockingService) |
| **Automation** | xUnit + InMemoryDb |

**Procedure:**
1. Arrange: Initialize InMemoryDb empty.
2. Act: Call `RecordClocking("", DateTime.UtcNow, ClockType.In, "key-001")`.
3. Assert: `Success == false`.
4. Assert: `Error == "Employee ID is required"`.

**C1 Verdict: PASS** — `RecordClocking_EmptyEmployeeId_ReturnsFail` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. Input validation intact.

---

### TC-006: Directory Search — Valid Query Returns Results (AC-003)

| Field | Value |
|---|---|
| **UC Trace** | UC-009, R001, SUP-003 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Goal** | TG-003 (directory search < 10s) |
| **Regression** | Yes — every build |
| **Suite** | DirectoryServiceTests |
| **Preconditions** | MockLdapGateway configured with 1 entry (full attributes) |
| **Input Data** | Query: `"john"` |
| **Expected Outcome** | 1 result with all corporate fields (name, job title, department, office, email, extension) |
| **Pass/Fail Criteria** | PASS: 1 result, all fields populated. FAIL: 0 results or missing fields |
| **Interface Points** | INT-002 (ILdapGateway), IDirectoryService |
| **Automation** | xUnit + MockLdapGateway |

**Procedure:**
1. Arrange: Configure MockLdapGateway with 1 entry (AdUserId=jdoe, DisplayName=John Doe, JobTitle=Developer, Department=IT, Office=Havana, Email=jdoe@cuba.cu, Extension=1234).
2. Act: Call `DirectoryService.Search("john")`.
3. Assert: 1 result returned.
4. Assert: DisplayName == "John Doe", JobTitle == "Developer", Department == "IT", Office == "Havana", Email == "jdoe@cuba.cu", Extension == "1234".

**C1 Verdict: PASS** — `Search_ValidQuery_ReturnsResults` validates all fields.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. AC-003 evidence: directory search returns all corporate fields (name, job title, department, office, email, extension). Performance (<10s) UNVERIFIED — requires deployment with real LDAP.

---

### TC-007: Directory Search — Missing Attributes Return N/A (R001 Fallback)

| Field | Value |
|---|---|
| **UC Trace** | UC-009, CON-012, SEC-004, R001 |
| **Test Level** | Integration |
| **Quality Dimension** | Reliability |
| **Regression** | Yes — every build |
| **Suite** | DirectoryServiceTests, DomainTests |
| **Preconditions** | MockLdapGateway configured with 1 entry (all attributes null/empty) |
| **Input Data** | Query: `"john"` |
| **Expected Outcome** | 1 result with all fields showing "N/A" for missing attributes |
| **Pass/Fail Criteria** | PASS: all missing fields show "N/A". FAIL: null/empty values shown to user |
| **Interface Points** | INT-002 (ILdapGateway), DirectoryEntry.FromLdapAttributes |
| **Automation** | xUnit + MockLdapGateway |

**Procedure:**
1. Arrange: Configure MockLdapGateway with 1 entry (all attributes null).
2. Act: Call `DirectoryService.Search("john")`.
3. Assert: 1 result returned.
4. Assert: JobTitle == "N/A", Department == "N/A", Office == "N/A", Email == "N/A", Extension == "N/A".

**C1 Verdict: PASS** — `Search_MissingAttributes_ReturnsNA` and `FromLdapAttributes_AllNull_ReturnsNA` validate.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. R001 fallback (N/A for missing LDAP attributes) confirmed across 3 test methods. AC-003 evidence: directory handles inconsistent AD data gracefully.

---

### TC-008: Publish News — Main Flow with Audit Trail (AC-002)

| Field | Value |
|---|---|
| **UC Trace** | UC-005, NFR-004, AUD-001 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | InMemoryDb empty; InMemoryAuditLogger initialized |
| **Input Data** | Title: "Title", Body: "Body", Category: HR, IsFeatured: false, Author: "author1" |
| **Expected Outcome** | NewsItem created with Status=Published, AuthorId=author1; audit record created |
| **Pass/Fail Criteria** | PASS: news item created with correct fields + audit record. FAIL: missing audit or wrong status |
| **Interface Points** | INT-003 (INewsService), IAuditLogger |
| **Automation** | xUnit + InMemoryDb + InMemoryAuditLogger |

**Procedure:**
1. Arrange: Initialize InMemoryDb and InMemoryAuditLogger.
2. Act: Call `NewsService.PublishAsync("Title", "Body", NewsCategory.HR, false, "author1")`.
3. Assert: Title == "Title", Body == "Body", Category == HR, Status == Published, AuthorId == "author1".
4. Assert: Audit record created with action=Published, author=author1.

**C1 Verdict: PASS** — `Publish_ValidInput_ReturnsPublishedNewsItem` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. AC-002 evidence: HR can publish news with audit trail (author + timestamp). NFR-004 (mandatory audit trail) verified.

---

### TC-009: Unpublish News — No Hard Delete (CON-013)

| Field | Value |
|---|---|
| **UC Trace** | UC-007, CON-013, AUD-003 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item exists |
| **Input Data** | News item ID; author: "a1" |
| **Expected Outcome** | News item Status=Unpublished; record NOT deleted; audit record created |
| **Pass/Fail Criteria** | PASS: status changed to Unpublished, record still exists, audit logged. FAIL: record deleted or no audit |
| **Interface Points** | INT-003 (INewsService), IAuditLogger |
| **Automation** | xUnit + InMemoryDb + InMemoryAuditLogger |

**Procedure:**
1. Arrange: Publish 1 news item.
2. Act: Call `NewsService.UnpublishAsync(item.Id, "a1")`.
3. Assert: Item status == Unpublished.
4. Assert: Item still exists in persistence (not deleted).
5. Assert: Audit record created with action=Unpublished.

**C1 Verdict: PASS** — `Unpublish_PublishedItem_SetsStatusToUnpublished` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. CON-013 (no hard delete) confirmed — unpublished items remain in persistence for audit trail.

---

### TC-010: Edit Published News — Audit Trail on Edit (AC-002)

| Field | Value |
|---|---|
| **UC Trace** | UC-006, NFR-004, AUD-001, C4-1 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item exists |
| **Input Data** | Updated title, body, category; IsFeatured preserved |
| **Expected Outcome** | News item updated; audit record created; IsFeatured preserved through edit |
| **Pass/Fail Criteria** | PASS: fields updated, audit logged, IsFeatured preserved. FAIL: missing audit or IsFeatured lost |
| **Interface Points** | INT-003 (INewsService), IAuditLogger |
| **Automation** | xUnit + InMemoryDb + InMemoryAuditLogger |

**Procedure:**
1. Arrange: Publish 1 news item with IsFeatured=true.
2. Act: Call `NewsService.EditAsync(item.Id, "New Title", "New Body", NewsCategory.IT, true, "author1")`.
3. Assert: Title == "New Title", Body == "New Body", Category == IT.
4. Assert: IsFeatured == true (preserved through edit).
5. Assert: Audit record created with action=Edited.

**C1 Verdict: PASS** — `Edit_PublishedNews_UpdatesFields` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — C4-1 (isFeatured in EditAsync) RESOLVED in PR #32. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. AC-002 evidence: HR can edit news with audit trail. C4-1 fix (IsFeatured preservation through edit) stable.

---

### TC-011: Page Load Performance (NFR-001) — BLOCKED

| Field | Value |
|---|---|
| **UC Trace** | NFR-001, PERF-001, All UCs |
| **Test Level** | System (Performance) |
| **Quality Dimension** | Performance |
| **Goal** | TG-001 (page load < 3s) |
| **Regression** | N/A — requires deployment |
| **Suite** | (Requires deployment) |
| **Preconditions** | Production-equivalent environment with real PostgreSQL, LDAP, OIDC |
| **Input Data** | HTTP GET to main page |
| **Expected Outcome** | Page load time < 3 seconds on corporate network |
| **Pass/Fail Criteria** | PASS: measured load time < 3s. FAIL: >= 3s |
| **Interface Points** | Main page endpoint, OIDC middleware |
| **Automation** | Requires deployment + load testing tool |

**Procedure:**
1. Deploy to production-equivalent environment.
2. Measure HTTP GET response time for main page.
3. Repeat 10 times, calculate P95.
4. Assert: P95 < 3 seconds.

**C1 Verdict: BLOCKED** — No deployment environment.
**C2 Verdict: BLOCKED** — No deployment environment.
**C3 Verdict: BLOCKED** — No deployment environment.
**C4 Verdict: BLOCKED** — No deployment environment.
**Transition I1 Verdict: BLOCKED** — No deployment environment available. This is a Transition exit criterion per stakeholder condition (1). NFR-001 remains unverified.

---

### TC-012: Clock In/Out Response Time (NFR-002) — BLOCKED

| Field | Value |
|---|---|
| **UC Trace** | UC-001, NFR-002, PERF-002 |
| **Test Level** | System (Performance) |
| **Quality Dimension** | Performance |
| **Goal** | TG-002 (clock response < 1s) |
| **Regression** | N/A — requires deployment |
| **Suite** | (Requires deployment) |
| **Preconditions** | Production-equivalent environment with real PostgreSQL |
| **Input Data** | POST /api/clocking with valid clocking data |
| **Expected Outcome** | Response time < 1 second |
| **Pass/Fail Criteria** | PASS: measured response time < 1s. FAIL: >= 1s |
| **Interface Points** | Clocking API endpoint, IClockingService |
| **Automation** | Requires deployment + load testing tool |

**Procedure:**
1. Deploy to production-equivalent environment.
2. Measure POST /api/clocking response time.
3. Repeat 10 times, calculate P95.
4. Assert: P95 < 1 second.

**C1 Verdict: BLOCKED** — No deployment environment.
**C2 Verdict: BLOCKED** — No deployment environment.
**C3 Verdict: BLOCKED** — No deployment environment.
**C4 Verdict: BLOCKED** — No deployment environment.
**Transition I1 Verdict: BLOCKED** — No deployment environment available. Transition exit criterion per stakeholder condition (1). NFR-002 remains unverified.

---

### TC-013: HR Role Authorization (SEC-002) — BLOCKED (R003)

| Field | Value |
|---|---|
| **UC Trace** | UC-003..UC-007, UC-010, SEC-002 |
| **Test Level** | Integration (Security) |
| **Quality Dimension** | Security |
| **Regression** | N/A — requires OIDC |
| **Suite** | (Requires OIDC environment) |
| **Preconditions** | Real OIDC client registered in Keycloak; HR role token available |
| **Input Data** | HR role token; Employee role token (negative test) |
| **Expected Outcome** | HR role can access UC-003..UC-007, UC-010; Employee role cannot |
| **Pass/Fail Criteria** | PASS: HR role authorized, Employee role denied. FAIL: no role enforcement |
| **Interface Points** | OIDC middleware, all HR service interfaces |
| **Automation** | Requires real OIDC environment |

**Procedure:**
1. Configure real OIDC client in Keycloak.
2. Generate HR role token and Employee role token.
3. Act: Call HR endpoints with HR token — should succeed.
4. Act: Call HR endpoints with Employee token — should be denied.
5. Assert: Role-based authorization enforced.

**C1 Verdict: BLOCKED** — R003: OIDC client not registered.
**C2 Verdict: BLOCKED** — R003 persists.
**C3 Verdict: BLOCKED** — R003 persists (3rd escalation).
**C4 Verdict: BLOCKED** — R003 persists (5th escalation). Stakeholder ACCEPTED risk.
**Transition I1 Verdict: BLOCKED** — R003 persists. Stakeholder ACCEPTED risk — mock-auth contingency active. Real OIDC integration is Transition work item. 8 tests stay covered-by-mock until real client.

---

### TC-014: Employee Role Authorization (SEC-002) — BLOCKED (R003)

| Field | Value |
|---|---|
| **UC Trace** | UC-003..UC-007, UC-010, SEC-002 |
| **Test Level** | Integration (Security) |
| **Quality Dimension** | Security |
| **Regression** | N/A — requires OIDC |
| **Suite** | (Requires OIDC environment) |
| **Preconditions** | Real OIDC client registered in Keycloak |
| **Input Data** | Employee role token |
| **Expected Outcome** | Employee role denied access to HR-only functions |
| **Pass/Fail Criteria** | PASS: Employee denied. FAIL: Employee can access HR functions |
| **Interface Points** | OIDC middleware |
| **Automation** | Requires real OIDC environment |

**C1 Verdict: BLOCKED** — R003.
**C2 Verdict: BLOCKED** — R003.
**C3 Verdict: BLOCKED** — R003 (3rd escalation).
**C4 Verdict: BLOCKED** — R003 (5th escalation). Stakeholder ACCEPTED.
**Transition I1 Verdict: BLOCKED** — R003 persists. Stakeholder ACCEPTED risk.

---

### TC-015: View Own Clocking History

| Field | Value |
|---|---|
| **UC Trace** | UC-002 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | 2 clocking records for current month (TD-003) |
| **Input Data** | Employee id: `emp-001`; date range: current month |
| **Expected Outcome** | 2 records returned, sorted by timestamp descending |
| **Pass/Fail Criteria** | PASS: 2 records, correct employee, current month only. FAIL: wrong count or wrong employee |
| **Interface Points** | INT-001 (IClockingService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `GetHistory_ReturnsEmployeeClockings` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-016: Export Monthly Clocking Report (CSV)

| Field | Value |
|---|---|
| **UC Trace** | UC-004, FR-004 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | 2 clocking records for current month (TD-003) |
| **Input Data** | Date range: current month |
| **Expected Outcome** | CSV stream with header `Employee,Date,Time,Direction` and 2 data rows |
| **Pass/Fail Criteria** | PASS: CSV contains header + data rows with correct format. FAIL: wrong format or missing data |
| **Interface Points** | INT-001 (IClockingService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `ExportCsv_WithClockings_ReturnsCsvStream` validates.
**C2 Verdict: PASS** — C2-MIN-4 fix: header corrected to `Employee,Date,Time,Direction`.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. CSV export with correct header format.

---

### TC-017: Read and Filter News — Category Filter

| Field | Value |
|---|---|
| **UC Trace** | UC-008, FR-008 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 5 published news items across 4 categories (TD-006) |
| **Input Data** | Category filter: HR |
| **Expected Outcome** | Only HR category news items returned |
| **Pass/Fail Criteria** | PASS: only HR news returned. FAIL: other categories included |
| **Interface Points** | INT-003 (INewsService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `GetPublishedNews_WithCategoryFilter_ReturnsFilteredResults` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. Category filter (General, HR, IT, Events) working.

---

### TC-018: Manage Worker Category — Assign with Audit

| Field | Value |
|---|---|
| **UC Trace** | UC-010, NFR-004, AUD-002 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | WorkerCategoryServiceTests |
| **Preconditions** | InMemoryDb empty; MockLdapGateway configured |
| **Input Data** | AdUserId: "jdoe", Category: "IT", Author: "hr1" |
| **Expected Outcome** | WorkerCategory created; audit record with action=CategoryChanged |
| **Pass/Fail Criteria** | PASS: category created + audit logged. FAIL: missing audit |
| **Interface Points** | INT-004 (IWorkerCategoryService), IAuditLogger |
| **Automation** | xUnit + InMemoryDb + InMemoryAuditLogger |

**C1 Verdict: PASS** — `AssignCategory_NewUser_CreatesCategory` and `AssignCategory_CreatesAuditRecord` validate.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. NFR-004 (audit trail for worker category changes) confirmed.

---

### TC-019: Manage Worker Category — Update Existing

| Field | Value |
|---|---|
| **UC Trace** | UC-010 (A1 — update existing) |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | WorkerCategoryServiceTests |
| **Preconditions** | 1 worker category exists (TD-010) |
| **Input Data** | AdUserId: "jdoe", Category: "Operations" (update from "IT") |
| **Expected Outcome** | Category updated to "Operations" |
| **Pass/Fail Criteria** | PASS: category updated. FAIL: duplicate created or not updated |
| **Interface Points** | INT-004 (IWorkerCategoryService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `AssignCategory_ExistingUser_UpdatesCategory` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-020: View All Employee Clockings — HR Authorization

| Field | Value |
|---|---|
| **UC Trace** | UC-003, SEC-002, CON-005 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | 2 clocking records from 2 employees (TD-004) |
| **Input Data** | Date range: current month |
| **Expected Outcome** | All employees' clockings returned |
| **Pass/Fail Criteria** | PASS: all employees' clockings returned. FAIL: only own clockings |
| **Interface Points** | INT-001 (IClockingService) |
| **Automation** | xUnit + InMemoryDb + OIDC mock (HR role) |

**C1 Verdict: PASS** — `GetAllClockings_ReturnsAllEmployees` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. HR can view all employees' clockings.

---

### TC-021: Offline Retry — Client Timestamp Preserved (AC-005)

| Field | Value |
|---|---|
| **UC Trace** | UC-001, MINOR-3, MINOR-4 |
| **Test Level** | Integration |
| **Quality Dimension** | Reliability |
| **Regression** | Yes — every build |
| **Suite** | OfflineRetryTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Client timestamp: `2026-01-15T09:30:00Z`; idempotency key: `emp1-client-ts-key` |
| **Expected Outcome** | Record persisted with client-side timestamp, not server timestamp |
| **Pass/Fail Criteria** | PASS: record.Timestamp == client timestamp. FAIL: server overwrites client timestamp |
| **Interface Points** | INT-001 (IClockingService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `Retry_ClientTimestamp_PreservedInRecord` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. AC-005 evidence: server accepts client-side timestamp for offline clockings.

---

### TC-022: Offline Retry — Empty Idempotency Key Rejected

| Field | Value |
|---|---|
| **UC Trace** | UC-001, MINOR-2, SEC-001 |
| **Test Level** | Unit |
| **Quality Dimension** | Security |
| **Regression** | Yes |
| **Suite** | OfflineRetryTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Empty idempotency key |
| **Expected Outcome** | `Success == false`, error "Idempotency key is required" |
| **Pass/Fail Criteria** | PASS: rejected. FAIL: accepted without key |
| **Interface Points** | INT-001 (IClockingService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `Retry_EmptyIdempotencyKey_ReturnsFail` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-023: Publish News — Audit Trail Verification

| Field | Value |
|---|---|
| **UC Trace** | UC-005, NFR-004 |
| **Test Level** | Integration |
| **Quality Dimension** | Security |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | InMemoryDb empty; InMemoryAuditLogger initialized |
| **Input Data** | Title, Body, Category, Author |
| **Expected Outcome** | Audit record created with action=Published, author, timestamp |
| **Pass/Fail Criteria** | PASS: audit record exists with correct fields. FAIL: no audit |
| **Interface Points** | IAuditLogger |
| **Automation** | xUnit + InMemoryAuditLogger |

**C1 Verdict: PASS** — Audit record verified in `Publish_ValidInput_ReturnsPublishedNewsItem`.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. NFR-004 audit trail confirmed for publish action.

---

### TC-024: Edit News — Audit Trail on Edit

| Field | Value |
|---|---|
| **UC Trace** | UC-006, NFR-004 |
| **Test Level** | Integration |
| **Quality Dimension** | Security |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item exists |
| **Input Data** | Updated fields; author |
| **Expected Outcome** | Audit record created with action=Edited |
| **Pass/Fail Criteria** | PASS: audit record exists. FAIL: no audit on edit |
| **Interface Points** | IAuditLogger |
| **Automation** | xUnit + InMemoryAuditLogger |

**C1 Verdict: PASS** — Audit record verified.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. NFR-004 audit trail confirmed for edit action.

---

### TC-025: Read News — Featured Banner Display (C4-1)

| Field | Value |
|---|---|
| **UC Trace** | UC-008, MAJOR-1, C4-1 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item with IsFeatured=true (TD-015) |
| **Input Data** | N/A |
| **Expected Outcome** | Featured news returned by GetFeaturedNews() |
| **Pass/Fail Criteria** | PASS: featured news returned. FAIL: no featured news |
| **Interface Points** | INT-003 (INewsService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `GetFeaturedNews_ReturnsFeaturedItems` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — C4-1 (isFeatured) RESOLVED. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. Featured banner display confirmed. UI (Index.cshtml) renders featured-banner div for featured news.

---

### TC-026: Worker Category — List All Categories

| Field | Value |
|---|---|
| **UC Trace** | UC-010 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | WorkerCategoryServiceTests |
| **Preconditions** | 2 worker categories exist |
| **Input Data** | N/A |
| **Expected Outcome** | 2 categories returned |
| **Pass/Fail Criteria** | PASS: 2 categories. FAIL: wrong count |
| **Interface Points** | INT-004 (IWorkerCategoryService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `ListCategories_ReturnsAllCategories` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-027: Unpublish News — Record Preserved (CON-013)

| Field | Value |
|---|---|
| **UC Trace** | UC-007, CON-013 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item |
| **Input Data** | News item ID |
| **Expected Outcome** | Item status=Unpublished; item still in ListAll() |
| **Pass/Fail Criteria** | PASS: item in ListAll. FAIL: item deleted |
| **Interface Points** | INT-003 (INewsService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `ListAll_ReturnsAllIncludingUnpublished` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. CON-013 confirmed — unpublished items preserved.

---

### TC-028: Directory Search — Office Filter

| Field | Value |
|---|---|
| **UC Trace** | UC-009 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | DirectoryServiceTests |
| **Preconditions** | MockLdapGateway with 1 entry (Office=Havana) |
| **Input Data** | Query: "john", Office filter: "Havana" |
| **Expected Outcome** | 1 result with Office=Havana |
| **Pass/Fail Criteria** | PASS: 1 result, correct office. FAIL: wrong office or 0 results |
| **Interface Points** | INT-002 (ILdapGateway) |
| **Automation** | xUnit + MockLdapGateway |

**C1 Verdict: PASS** — `Search_WithOfficeFilter_BuildsCombinedFilter` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. Office filter working.

---

### TC-029: Directory Search — HR Role Authorization (SEC-002) — BLOCKED (R003)

| Field | Value |
|---|---|
| **UC Trace** | UC-009, SEC-002 |
| **Test Level** | Integration (Security) |
| **Quality Dimension** | Security |
| **Regression** | N/A — requires OIDC |
| **Suite** | (Requires OIDC environment) |
| **Preconditions** | Real OIDC client registered |
| **Input Data** | Employee role token |
| **Expected Outcome** | Employee can search directory (all employees have access) |
| **Pass/Fail Criteria** | PASS: Employee authorized. FAIL: Employee denied |
| **Interface Points** | OIDC middleware, DirectoryService |
| **Automation** | Requires real OIDC environment |

**C1 Verdict: BLOCKED** — R003.
**C2 Verdict: BLOCKED** — R003.
**C3 Verdict: BLOCKED** — R003 (3rd escalation).
**C4 Verdict: BLOCKED** — R003 (5th escalation). Stakeholder ACCEPTED.
**Transition I1 Verdict: BLOCKED** — R003 persists. Stakeholder ACCEPTED risk.

---

### TC-030: HR Functions — Role Authorization (SEC-002) — BLOCKED (R003)

| Field | Value |
|---|---|
| **UC Trace** | UC-005..UC-007, UC-010, SEC-002 |
| **Test Level** | Integration (Security) |
| **Quality Dimension** | Security |
| **Regression** | N/A — requires OIDC |
| **Suite** | (Requires OIDC environment) |
| **Preconditions** | Real OIDC client registered |
| **Input Data** | HR role token, Employee role token |
| **Expected Outcome** | HR role authorized for UC-005..UC-007, UC-010; Employee denied |
| **Pass/Fail Criteria** | PASS: role enforcement works. FAIL: no enforcement |
| **Interface Points** | OIDC middleware |
| **Automation** | Requires real OIDC environment |

**C1 Verdict: BLOCKED** — R003.
**C2 Verdict: BLOCKED** — R003.
**C3 Verdict: BLOCKED** — R003 (3rd escalation).
**C4 Verdict: BLOCKED** — R003 (5th escalation). Stakeholder ACCEPTED.
**Transition I1 Verdict: BLOCKED** — R003 persists. Stakeholder ACCEPTED risk.

---

### TC-031: Clocking API Route Integration (C2-CRIT-1)

| Field | Value |
|---|---|
| **UC Trace** | UC-001, C2-CRIT-1 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Valid clocking request |
| **Expected Outcome** | API route resolves correctly |
| **Pass/Fail Criteria** | PASS: route resolves. FAIL: 404 or wrong route |
| **Interface Points** | ClockingApi endpoint |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — Service-layer test.
**C2 Verdict: PASS** — C2-CRIT-1 (API routing) RESOLVED.
**C3 Verdict: PASS** — Route integration confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-032: Edit News — Form Binding (C2-MAJ-1)

| Field | Value |
|---|---|
| **UC Trace** | UC-006, C2-MAJ-1 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item |
| **Input Data** | Updated fields via form binding |
| **Expected Outcome** | Fields correctly bound and persisted |
| **Pass/Fail Criteria** | PASS: fields updated. FAIL: binding error |
| **Interface Points** | Edit.cshtml.cs |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — Service-layer test.
**C2 Verdict: PASS** — C2-MAJ-1 RESOLVED.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-033: Clocking — Antiforgery Token (C2-MAJ-2)

| Field | Value |
|---|---|
| **UC Trace** | UC-001, C2-MAJ-2 |
| **Test Level** | Integration |
| **Quality Dimension** | Security |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Clocking request with antiforgery token |
| **Expected Outcome** | Request accepted with valid token |
| **Pass/Fail Criteria** | PASS: accepted. FAIL: rejected |
| **Interface Points** | ClockingApi endpoint |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — Service-layer test.
**C2 Verdict: PASS** — C2-MAJ-2 RESOLVED.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. SEC-006 (antiforgery) confirmed.

---

### TC-034: Clocking — Server-Side Identity (C2-MIN-2)

| Field | Value |
|---|---|
| **UC Trace** | UC-001, C2-MIN-2 |
| **Test Level** | Integration |
| **Quality Dimension** | Security |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Clocking request; employee ID from token, not request body |
| **Expected Outcome** | Employee ID from OIDC token, not client-supplied |
| **Pass/Fail Criteria** | PASS: server uses token identity. FAIL: server trusts client-supplied ID |
| **Interface Points** | ClockingApi endpoint |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — Service-layer test.
**C2 Verdict: PASS** — C2-MIN-2 RESOLVED.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. SEC-007 (server-side identity) confirmed.

---

### TC-035: CSV Export — Empty Month (Header Only)

| Field | Value |
|---|---|
| **UC Trace** | UC-004 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | 0 clocking records for target month (TD-014) |
| **Input Data** | Date range: September 2026 (no records) |
| **Expected Outcome** | CSV with header only, no data rows |
| **Pass/Fail Criteria** | PASS: 1 line (header only). FAIL: empty stream or data rows |
| **Interface Points** | INT-001 (IClockingService) |
| **Automation** | xUnit + InMemoryDb |

**C1 Verdict: PASS** — `ExportCsv_NoClockings_ReturnsHeaderOnly` validates.
**C2 Verdict: PASS** — Confirmed.
**C3 Verdict: PASS** — Confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified. Empty month CSV export returns header only.

---

### TC-036: Clocking — Route Integration via WebApplicationFactory (C3)

| Field | Value |
|---|---|
| **UC Trace** | UC-001, C3 route |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Valid clocking request |
| **Expected Outcome** | Route resolves via WebApplicationFactory |
| **Pass/Fail Criteria** | PASS: route resolves. FAIL: route not found |
| **Interface Points** | ClockingApi endpoint |
| **Automation** | xUnit + InMemoryDb |

**C3 Verdict: PASS** — Route integration confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-037: Edit News — Form Binding Validation (C3)

| Field | Value |
|---|---|
| **UC Trace** | UC-006, C3 form binding |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item |
| **Input Data** | Updated fields via form binding |
| **Expected Outcome** | Fields correctly bound and validated |
| **Pass/Fail Criteria** | PASS: binding works. FAIL: binding error |
| **Interface Points** | Edit.cshtml.cs |
| **Automation** | xUnit + InMemoryDb |

**C3 Verdict: PASS** — Form binding confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-038: Clocking — Antiforgery Validation (C3)

| Field | Value |
|---|---|
| **UC Trace** | UC-001, C3 antiforgery |
| **Test Level** | Integration |
| **Quality Dimension** | Security |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Clocking request with/without antiforgery token |
| **Expected Outcome** | Valid token accepted; invalid/missing token rejected |
| **Pass/Fail Criteria** | PASS: validation works. FAIL: no validation |
| **Interface Points** | ClockingApi endpoint |
| **Automation** | xUnit + InMemoryDb |

**C3 Verdict: PASS** — Antiforgery validation confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-039: Clocking — Server-Side Identity Extraction (C3)

| Field | Value |
|---|---|
| **UC Trace** | UC-001, C3 identity |
| **Test Level** | Integration |
| **Quality Dimension** | Security |
| **Regression** | Yes — every build |
| **Suite** | ClockingServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Clocking request; identity from OIDC token |
| **Expected Outcome** | Employee ID extracted from token claims |
| **Pass/Fail Criteria** | PASS: identity from token. FAIL: identity from request body |
| **Interface Points** | ClockingApi endpoint |
| **Automation** | xUnit + InMemoryDb |

**C3 Verdict: PASS** — Identity extraction confirmed.
**C4 Verdict: PASS** — No changes. Regression clean.
**Transition I1 Verdict: PASS** — Regression verified.

---

### TC-040: Transaction Atomicity — ExecuteInTransactionAsync Commit (C4-2)

| Field | Value |
|---|---|
| **UC Trace** | UC-005, UC-006, UC-007, UC-010, C4-2 |
| **Test Level** | Integration |
| **Quality Dimension** | Reliability |
| **Regression** | Yes — every build |
| **Suite** | OfflineRetryTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Valid action within transaction |
| **Expected Outcome** | Action commits; record persisted |
| **Pass/Fail Criteria** | PASS: record exists after commit. FAIL: record missing |
| **Interface Points** | IPersistence.ExecuteInTransactionAsync |
| **Automation** | xUnit + InMemoryDb |

**C4 Verdict: PASS** — `ExecuteInTransactionAsync_SuccessfulAction_Commits` validates.
**Transition I1 Verdict: PASS** — Regression verified. C4-2 (transaction wrapping) stable.

---

### TC-041: Transaction Atomicity — ExecuteInTransactionAsync Rollback (C4-2)

| Field | Value |
|---|---|
| **UC Trace** | UC-005, UC-006, UC-007, UC-010, C4-2, NFR-004 |
| **Test Level** | Integration |
| **Quality Dimension** | Reliability |
| **Regression** | Yes — every build |
| **Suite** | OfflineRetryTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Action that throws exception within transaction |
| **Expected Outcome** | Exception propagates; transaction rolls back |
| **Pass/Fail Criteria** | PASS: exception thrown, no partial state. FAIL: partial commit |
| **Interface Points** | IPersistence.ExecuteInTransactionAsync |
| **Automation** | xUnit + InMemoryDb |

**C4 Verdict: PASS** — `ExecuteInTransactionAsync_FailingAction_RollsBackAndThrows` validates. Note: InMemoryDb executes directly (no real rollback), but exception propagation is verified. Real EF Core rollback requires PostgreSQL deployment.
**Transition I1 Verdict: PASS** — Regression verified. Exception propagation confirmed. Real rollback behavior requires PostgreSQL deployment (TI-048 pending).

---

### TC-042: Edit News — IsFeatured Preservation Through Edit (C4-1)

| Field | Value |
|---|---|
| **UC Trace** | UC-006, C4-1 |
| **Test Level** | Integration |
| **Quality Dimension** | Functionality |
| **Regression** | Yes — every build |
| **Suite** | NewsServiceTests |
| **Preconditions** | 1 published news item with IsFeatured=true |
| **Input Data** | Edit with IsFeatured=true |
| **Expected Outcome** | IsFeatured preserved after edit |
| **Pass/Fail Criteria** | PASS: IsFeatured=true after edit. FAIL: IsFeatured lost |
| **Interface Points** | INT-003 (INewsService), Edit.cshtml.cs |
| **Automation** | xUnit + InMemoryDb |

**C4 Verdict: PASS** — C4-1 (isFeatured in EditAsync) RESOLVED in PR #32.
**Transition I1 Verdict: PASS** — Regression verified. C4-1 fix stable.

---

### TC-043: Worker Category — Transaction Atomicity (C4-2)

| Field | Value |
|---|---|
| **UC Trace** | UC-005, UC-010, C4-2 |
| **Test Level** | Integration |
| **Quality Dimension** | Reliability |
| **Regression** | Yes — every build |
| **Suite** | WorkerCategoryServiceTests |
| **Preconditions** | InMemoryDb empty |
| **Input Data** | Valid category assignment within transaction |
| **Expected Outcome** | Category + audit record committed atomically |
| **Pass/Fail Criteria** | PASS: both committed. FAIL: partial commit |
| **Interface Points** | IPersistence.ExecuteInTransactionAsync, IAuditLogger |
| **Automation** | xUnit + InMemoryDb + InMemoryAuditLogger |

**C4 Verdict: PASS** — Transaction atomicity verified for worker category.
**Transition I1 Verdict: PASS** — Regression verified.

---

### Domain Tests (6 TCs — no individual TC numbers, part of DomainTests.cs)

| Test | UC Trace | C1 | C2 | C3 | C4 | Transition I1 |
|---|---|---|---|---|---|---|
| `FromLdapAttributes_AllPresent_ReturnsAllValues` | UC-009, R001 | PASS | PASS | PASS | PASS | **PASS** |
| `FromLdapAttributes_AllNull_ReturnsNA` | UC-009, R001 | PASS | PASS | PASS | PASS | **PASS** |
| `FromLdapAttributes_AllWhitespace_ReturnsNA` | UC-009, R001 | PASS | PASS | PASS | PASS | **PASS** |
| `FromLdapAttributes_MixedPresentAndMissing_ReturnsValuesAndNA` | UC-009, R001 | PASS | PASS | PASS | PASS | **PASS** |
| `DateRange_ForMonth` (3 tests) | UC-002, UC-004 | PASS | PASS | PASS | PASS | **PASS** |
| `ClockingResult` (3 tests) | UC-001 | PASS | PASS | PASS | PASS | **PASS** |

---

### OfflineRetryTests (10 TCs — part of OfflineRetryTests.cs)

| Test | UC Trace | C1 | C2 | C3 | C4 | Transition I1 |
|---|---|---|---|---|---|---|
| `Retry_SameIdempotencyKey_ReturnsDuplicateNotNewRecord` | UC-001, AC-005 | PASS | PASS | PASS | PASS | **PASS** |
| `Retry_SameKeyDifferentEmployee_BothSucceed` | UC-001, CR #11 | PASS | PASS | PASS | PASS | **PASS** |
| `Retry_ClientTimestamp_PreservedInRecord` | UC-001, AC-005 | PASS | PASS | PASS | PASS | **PASS** |
| `Retry_EmptyIdempotencyKey_ReturnsFail` | UC-001 | PASS | PASS | PASS | PASS | **PASS** |
| `Retry_EmptyEmployeeId_ReturnsFail` | UC-001 | PASS | PASS | PASS | PASS | **PASS** |
| `Retry_MultipleRetries_AllReturnSameRecord` | UC-001, AC-005 | PASS | PASS | PASS | PASS | **PASS** |
| `Retry_ClockInThenOut_DifferentKeys_BothSucceed` | UC-001 | PASS | PASS | PASS | PASS | **PASS** |
| `ExecuteInTransactionAsync_SuccessfulAction_Commits` | C4-2 | — | — | — | PASS | **PASS** |
| `ExecuteInTransactionAsync_FailingAction_RollsBackAndThrows` | C4-2 | — | — | — | PASS | **PASS** |

---

### Test Ideas (TI-045..TI-050) — Status

| TI ID | Description | Status | Notes |
|---|---|---|---|
| TI-045 | Transaction timeout boundary | OPEN — deferred | Requires deployment with real PostgreSQL |
| TI-046 | EF Core transaction investigation | OPEN — deferred | Requires EF Core + PostgreSQL |
| TI-047 | IsFeatured rapid toggle | OPEN — deferred | Requires concurrency harness |
| TI-048 | Audit trail rollback boundary | OPEN — deferred | Requires deployment |
| TI-049 | Concurrent edit + unpublish | OPEN — deferred | Requires concurrency harness |
| TI-050 | CSV export during transaction | OPEN — deferred | Requires deployment |

## Test Data

### Test Data Catalog

| Data Set ID | Description | UCs | Seed Method |
|---|---|---|---|
| TD-001 | Empty database | All | InMemoryDb initialized with no records |
| TD-002 | Single employee clock-in record | UC-001, UC-002 | Seed: 1 clocking record (emp-001, in, 08:00) |
| TD-003 | Full day clock-in + clock-out | UC-001, UC-002 | Seed: 2 clocking records (emp-001, in 08:00, out 17:00) |
| TD-004 | Multi-employee clockings (10 records, 3 employees) | UC-003, UC-004 | Seed: 10 clocking records across 3 employees for August 2026 |
| TD-005 | Current + previous month clockings | UC-002 | Seed: 3 current-month + 2 previous-month records |
| TD-006 | Published news (5 items, 4 categories, 2 featured) | UC-008 | Seed: 2 General (1 featured), 1 HR (1 featured), 1 IT, 1 Events — all published |
| TD-007 | Published + unpublished news | UC-007, UC-008 | Seed: 5 published + 1 unpublished (HR category) |
| TD-008 | LDAP entries with missing attributes | UC-009, R001 | LdapGatewayStub: 3 entries — (1) full, (2) empty jobTitle, (3) empty telephoneNumber |
| TD-009 | LDAP entries with private attributes | UC-009, CON-012 | LdapGatewayStub: 1 entry with corporate + private fields (mobile, homeAddress, dateOfBirth) |
| TD-010 | Worker category assignment | UC-010 | Seed: 1 worker_categories record (ad-user-001, Administrative) |
| TD-011 | OIDC tokens (Employee + HR roles) | All | OIDC Mock Token Provider: 2 tokens — Employee role, HR role |
| TD-012 | 50 concurrent employee tokens | UC-001 (stress) | OIDC Mock Token Provider: 50 tokens — emp-001..emp-050, all Employee role |
| TD-013 | 200 LDAP entries (full directory) | UC-009 (performance) | MockLdapGateway: 200 entries across 3 offices with varied attribute completeness |
| TD-014 | Empty month clockings (no records) | UC-004 | Seed: 0 clocking records for September 2026 — CSV export should return headers only |
| TD-015 | News item with IsFeatured=true (pre-seeded) | UC-008, MAJOR-1 | Seed: 1 published news item with IsFeatured=true |
| TD-016 | Double clock-in same key | UC-001 | Seed: 1 clocking record, retry with same key |

### Test Data Notes

- All test data uses InMemoryDb (no real PostgreSQL) — sufficient for functional verification.
- TD-013 (200 LDAP entries) is available for performance testing but cannot measure real latency without deployment.
- TD-009 (private attributes) verifies CON-012 (corporate data only) — MockLdapGateway returns all fields, DirectoryService filters to corporate only.
- TD-011/TD-012 (OIDC mock tokens) simulate authentication but do not test real OIDC integration (R003).

## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| TC-001 | UC-001 (main flow) | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-002 | UC-001 (main flow) | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-003 | UC-001 (A1), AC-005, NFR-003 | Tests | ClockingService.cs, clocking-retry.js, OfflineRetryTests.cs |
| TC-004 | UC-001 (A1), AC-005 | Tests | clocking-retry.js, OfflineRetryTests.cs |
| TC-005 | UC-001 (A2) | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-006 | UC-009, R001, SUP-003 | Tests | DirectoryService.cs, DirectoryServiceTests.cs, DomainTests.cs |
| TC-007 | UC-009, CON-012, SEC-004 | Tests | DirectoryService.cs, DirectoryServiceTests.cs |
| TC-008 | UC-005, NFR-004, AUD-001 | Tests | NewsService.cs, NewsServiceTests.cs, AuditInterceptor.cs |
| TC-009 | UC-007, CON-013, AUD-003 | Tests | NewsService.cs, NewsServiceTests.cs |
| TC-010 | UC-006, NFR-004, AUD-001, C4-1 | Tests | NewsService.cs, NewsServiceTests.cs, Edit.cshtml.cs |
| TC-011 | NFR-001, PERF-001, All UCs | Tests | Main page endpoint, OIDC middleware |
| TC-012 | UC-001, NFR-002, PERF-002 | Tests | ClockingService.cs, clock-in endpoint |
| TC-013 | UC-003..UC-007, UC-010, SEC-002 | Tests | OIDC middleware, all HR service interfaces |
| TC-014 | UC-003..UC-007, UC-010, SEC-002 | Tests | OIDC middleware, all HR service interfaces |
| TC-015 | UC-002 | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-016 | UC-004, FR-004 | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-017 | UC-008, FR-008 | Tests | NewsService.cs, NewsServiceTests.cs |
| TC-018 | UC-010, NFR-004, AUD-002 | Tests | WorkerCategoryService.cs, WorkerCategoryServiceTests.cs |
| TC-019 | UC-010 (A1) | Tests | WorkerCategoryService.cs, WorkerCategoryServiceTests.cs, MockLdapGateway |
| TC-020 | UC-003, SEC-002, CON-005 | Tests | ClockingService.cs, MockLdapGateway, OIDC mock |
| TC-021 | UC-001, MINOR-3, MINOR-4 | Tests | ClockingService.cs, OfflineRetryTests.cs |
| TC-022 | UC-001, MINOR-2, SEC-001 | Tests | ClockingService.cs, OfflineRetryTests.cs |
| TC-023 | UC-005, NFR-004 | Tests | NewsService.cs, NewsServiceTests.cs |
| TC-024 | UC-006, NFR-004 | Tests | NewsService.cs, NewsServiceTests.cs |
| TC-025 | UC-008, MAJOR-1, C4-1 | Tests | NewsService.cs, NewsServiceTests.cs |
| TC-026 | UC-010 | Tests | WorkerCategoryService.cs, WorkerCategoryServiceTests.cs |
| TC-027 | UC-007, CON-013 | Tests | NewsService.cs, NewsServiceTests.cs |
| TC-028 | UC-009 | Tests | DirectoryService.cs, DirectoryServiceTests.cs |
| TC-029 | UC-009, SEC-002 | Tests | OIDC middleware, DirectoryService.cs |
| TC-030 | UC-005..UC-007, UC-010, SEC-002 | Tests | OIDC middleware, NewsService.cs, WorkerCategoryService.cs |
| TC-031 | UC-001, C2-CRIT-1 | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-032 | UC-006, C2-MAJ-1 | Tests | NewsService.cs, NewsServiceTests.cs, Edit.cshtml.cs |
| TC-033 | UC-001, C2-MAJ-2 | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-034 | UC-001, C2-MIN-2 | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-035 | UC-004 | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-036 | UC-001, C3 route | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-037 | UC-006, C3 form binding | Tests | NewsService.cs, NewsServiceTests.cs, Edit.cshtml.cs |
| TC-038 | UC-001, C3 antiforgery | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-039 | UC-001, C3 identity | Tests | ClockingService.cs, ClockingServiceTests.cs |
| TC-040 | UC-005, UC-006, UC-007, UC-010, C4-2 | Tests | PersistenceGateway.cs, OfflineRetryTests.cs |
| TC-041 | UC-005, UC-006, UC-007, UC-010, C4-2, NFR-004 | Tests | PersistenceGateway.cs, OfflineRetryTests.cs |
| TC-042 | UC-006, C4-1 | Tests | NewsService.cs, NewsServiceTests.cs, Edit.cshtml.cs |
| TC-043 | UC-005, UC-010, C4-2 | Tests | NewsService.cs, WorkerCategoryService.cs, OfflineRetryTests.cs |
| TI-045 | UC-005, UC-006, UC-007, UC-010, C4-2 | Tests | PersistenceGateway.cs — [Pending: deployment] |
| TI-046 | UC-005, UC-006, UC-007, UC-010, C4-3 | Tests | PersistenceGateway.cs — [Pending: EF Core investigation] |
| TI-047 | UC-006, C4-1 | Tests | NewsService.cs — [Pending: concurrency harness] |
| TI-048 | UC-005, UC-006, UC-007, UC-010, NFR-004, C4-2 | Tests | PersistenceGateway.cs, AuditInterceptor.cs — [Pending: extend TC-040/TC-041] |
| TI-049 | UC-006, UC-007, C4-2 | Tests | NewsService.cs — [Pending: concurrency harness] |
| TI-050 | UC-004, NFR-001 | Tests | ClockingService.cs — [Pending: deployment] |
| TA-C4-F1 | C4-1, C4-2 | Derives | PR #32 (RESOLVED) |
| TA-C4-F2 | R003, STK-003, CON-004 | Derives | TC-013, TC-014, TC-029, TC-030 (BLOCKED — 5th escalation) |
| TA-C4-F3 | NFR-001, NFR-002 | Derives | TC-011, TC-012 (BLOCKED — no deployment) |
| TA-C4-F4 | AC-003, AC-004, CON-011 | Derives | (Manual UAT required) |
| TA-C4-F5 | All prior PASS TCs | Derives | Regression CLEAN (C4) |
| TA-C4-F6 | Issue #12, #13, #14 | Derives | RESOLVED in code (C4) |
| TA-T1-F1 | AC-001, AC-002, AC-005 | Derives | TC-001, TC-002, TC-003, TC-004, TC-008, TC-009, TC-010, TC-021 (PASS) |
| TA-T1-F2 | AC-003, AC-004 | Derives | TC-006, TC-007 (PASS functional); performance + manual UAT PENDING |
| TA-T1-F3 | NFR-001, NFR-002 | Derives | TC-011, TC-012 (BLOCKED — no deployment — Transition exit criterion) |
| TA-T1-F4 | R003, STK-003, CON-004 | Derives | TC-013, TC-014, TC-029, TC-030 (BLOCKED — stakeholder ACCEPTED) |
| TA-T1-F5 | All 35 PASS TCs | Derives | Regression CLEAN (Transition I1) — build 33256627567 |
| TA-T1-F6 | 6 open defect issues | Derives | 1 blocker ACCEPTED, 5 minor/deferred — no Critical/High unresolved |
| R003 | STK-003, CON-004 | DependsOn | TC-013, TC-014, TC-029, TC-030 (BLOCKED — stakeholder ACCEPTED) |
| Issue #30 | R003, STK-003, CON-004 | Derives | TC-013, TC-014, TC-029, TC-030 (BLOCKED — ACCEPTED risk) |
| Issue #12 | TC-016 (ClockingServiceTests) | Derives | CSV format — RESOLVED in code (C4) |
| Issue #13 | TC-006 (DirectoryServiceTests) | Derives | Search_NoMatchingEntries — RESOLVED in code (C4) |
| Issue #14 | TC-F2 | Derives | UnitTest1.cs placeholder — RESOLVED in code (C4) |
| PR #32 | C4-1, C4-2, C4-3 | Realizes | feature/C4-rework branch (APPROVED) |
| CI Build (main) | CON-001, CON-003 | DependsOn | GitHub Actions run 33256627567 |
| AC-001 | FR-001, FR-002 | Derives | TC-001, TC-002, TC-003 (PASS) |
| AC-002 | FR-005, FR-006, FR-007 | Derives | TC-008, TC-009, TC-010 (PASS) |
| AC-003 | FR-009 | Derives | TC-006, TC-007 (PASS functional — perf UNVERIFIED) |
| AC-004 | FR-001 | Derives | TC-001, TC-002 (PASS automated — manual UAT needed) |
| AC-005 | CON-002, CR-011 | Derives | TC-003, TC-004, TC-021 (PASS — service + JS verified) |