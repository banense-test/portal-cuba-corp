## Document Control
| Field | Value |
|---|---|
| Phase | Construction |
| Status | Active |
| Milestone Target | End-of-Construction (IOC) |
| Iteration | 2 (Cycle 1) |
| Date | 2026-08-28 |
| CCB Composition | Change Control Manager (chair), Software Architect (for architectural CRs), Project Manager (for business CRs) |
| Source of Truth | SCM Issues — labels are the authoritative CR state; this artifact is the narrative audit ledger |
| Prior Iteration | Construction C1 — 13 CRs, 6 approved, 7 deferred, 0 complete |
| This Iteration | 5 new CRs registered, 6 approved, 3 completed, 7 deferred (carried) |
## Change Request Log
### Portfolio Summary

| Metric | Value |
|---|---|
| Total CRs (cumulative) | 18 (13 from C1 + 5 new in C2) |
| Approved (this iteration) | 6 (#22, #23, #24, #25, #26, #27) |
| Previously Approved (carried forward) | 2 (#1, #2 — assigned to software-architect, no PRs yet) |
| Deferred to Next Iteration | 7 (#3, #12, #13, #14, #15, #17, #18 — carried from C1) |
| Rejected | 0 |
| Completed (this iteration) | 3 (#6, #10, #11 — verified via merged PRs) |
| Closure Rate | 3/11 approved = 27% (up from 0% in C1) |

### CR State Distribution

```plantuml
@startuml CR_Portfolio_State_Distribution_C2
title CR Portfolio State Distribution — Construction C2 (2026-08-28)

skinparam backgroundColor #FEFEFE
skinparam shadowing false

rectangle "Approved (8)" as approved #LightGreen {
  note "CR-001: LDAP PoC (architect)\nCR-002: Offline Retry (architect)\nCR-022: Clocking API 404 (implementer)\nCR-023: Antiforgery token (implementer)\nCR-024: EmployeeId spoof (implementer)\nCR-025: Missing Razor Pages (implementer)\nCR-026: PR #21 approval (architect)\nCR-027: News/Edit mismatch (implementer)" as approved_note
}

rectangle "Deferred Next Iteration (7)" as deferred #LightYellow {
  note "CR-003: Audit trail validation\nCR-012: CSV export format\nCR-013: Test assertion\nCR-014: Placeholder test\nCR-015: Naming violation\nCR-017: Dead code DTO\nCR-018: Test codifies bug" as deferred_note
}

rectangle "Complete (3)" as complete #LightBlue {
  note "CR-006: Proto merge (PR #4)\nCR-010: IsFeatured (PR #20)\nCR-011: Idempotency (PR #20)" as complete_note
}

rectangle "Rejected (0)" as rejected #LightCoral

approved -[hidden]right-> deferred
deferred -[hidden]right-> complete
complete -[hidden]right-> rejected

note bottom of approved
  **Total CRs: 18** (13 prior + 5 new this iteration)
  Approved: 8 (44%)
  Deferred: 7 (39%)
  Complete: 3 (17%)
  Rejected: 0 (0%)
  Closure Rate: 3/11 approved = 27%
end note

@enduml
```

### Priority × Severity Matrix

```plantuml
@startuml CR_Priority_Severity_C2
title CR Priority x Severity Matrix — Construction C2 (2026-08-28)

skinparam backgroundColor #FEFEFE
skinparam shadowing false

object "critical / blocker" as cb {
  CR-022: Clocking API 404
  CR-026: PR #21 approval
  CR-006: Proto merge [COMPLETE]
}

object "critical / major" as cm {
  (none)
}

object "high / major" as hm {
  CR-001: LDAP PoC
  CR-002: Offline Retry
  CR-010: IsFeatured [COMPLETE]
  CR-011: Idempotency [COMPLETE]
  CR-023: Antiforgery token
  CR-025: Missing Razor Pages
  CR-027: News/Edit mismatch
}

object "high / minor" as hi {
  (none)
}

object "medium / major" as mm {
  CR-003: Audit trail [DEFERRED]
}

object "medium / minor" as mi {
  CR-012: CSV export [DEFERRED]
  CR-013: Test assertion [DEFERRED]
  CR-015: Naming violation [DEFERRED]
  CR-017: Dead code DTO [DEFERRED]
  CR-024: EmployeeId spoof
}

object "low / minor" as li {
  CR-018: Test codifies bug [DEFERRED]
}

object "low / trivial" as lt {
  CR-014: Placeholder test [DEFERRED]
}

cb -[hidden]right-> cm
cm -[hidden]right-> hm
hm -[hidden]right-> hi
hi -[hidden]down-> mm
mm -[hidden]right-> mi
mi -[hidden]down-> li
li -[hidden]right-> lt

note bottom of lt
  **Priority x Severity Distribution**
  Critical/Blocker: 3 (1 complete)
  High/Major: 7 (2 complete)
  Medium/Minor: 5 (4 deferred)
  Low: 2 (deferred)
end note

@enduml
```

### CR Lifecycle Activity

```plantuml
@startuml CR_Lifecycle_Activity_C2
title CR Lifecycle Activity — Construction C2 (2026-08-28)

skinparam backgroundColor #FEFEFE
skinparam shadowing false

start

:New CRs discovered (5 new);
:Issue #22 — Clocking API 404 (C2-CRIT-1);
:Issue #23 — Antiforgery token (C2-MAJ-2);
:Issue #24 — EmployeeId spoof (C2-MIN-2);
:Issue #25 — Missing Razor Pages;
:Issue #26 — PR #21 approval blocked;
:Issue #27 — News/Edit mismatch (C2-MAJ-1);

:Triage — classify on 4 axes;
:Priority / Severity / Nature / Impact;

if (Impact = architectural?) then (yes)
  :Park with needs-architect-review;
  :Wait for architect-concurred;
else (no)
  :Gate cleared;
endif

:CCB Decision;

if (Priority in {critical, high}?) then (yes)
  if (Fits C2 capacity?) then (yes)
    :APPROVE + assign executor;
  else (no)
    :DEFER to next iteration;
  endif
else (medium/low)
  if (Security or cross-cutting?) then (yes)
    :APPROVE + assign executor;
  else (no)
    :DEFER to next iteration;
  endif
endif

:Verify implementation;

if (Linked PR merged?) then (yes)
  :Transition to cr:complete;
  :Close issue;
else (no PR or open PR)
  :Remain cr:approved;
  :Next iteration re-check;
endif

stop

@enduml
```

### Detailed CR Ledger

| Issue # | CR ID | Title | State | Priority | Severity | Nature | Impact | Assigned | Origin |
|---|---|---|---|---|---|---|---|---|---|
| #1 | CR-001 | Execute LDAP Attribute Mapping PoC (R001) | cr:approved | high | major | enhancement | architectural | software-architect | Elaboration |
| #2 | CR-002 | Validate Offline Clocking Retry Design (AC-005) | cr:approved | high | major | enhancement | architectural | software-architect | Elaboration |
| #3 | CR-003 | Validate Audit Trail Pattern (NFR-004) | cr:deferred-next-iteration | medium | major | enhancement | cross-cutting | — | Elaboration |
| #6 | CR-006 | Architectural prototype PR #4 not merged | **cr:complete** | critical | blocker | defect | cross-cutting | implementer | C1 |
| #10 | CR-010 | IsFeatured not settable in NewsService.Publish | **cr:complete** | high | major | defect | local | implementer | C1 |
| #11 | CR-011 | Idempotency key not scoped per employee | **cr:complete** | high | major | defect | local | implementer | C1 |
| #12 | CR-012 | CSV export — TimeOut column always empty | cr:deferred-next-iteration | medium | minor | defect | local | — | C1 |
| #13 | CR-013 | Test assertion contradicts test name | cr:deferred-next-iteration | medium | minor | defect | local | — | C1 |
| #14 | CR-014 | Placeholder test UnitTest1.cs | cr:deferred-next-iteration | low | trivial | defect | local | — | C1 |
| #15 | CR-015 | Naming violation — missing UC identifiers | cr:deferred-next-iteration | medium | minor | defect | local | — | C1 |
| #17 | CR-017 | RecordClockingRequest.EmployeeId dead code | cr:deferred-next-iteration | medium | minor | defect | local | — | C1 |
| #18 | CR-018 | Test codifies idempotency collision as expected | cr:deferred-next-iteration | low | minor | defect | local | — | C1 |
| #22 | C2-CRIT-1 | Clocking API endpoint missing — 404 | cr:approved | critical | blocker | defect | local | implementer | C2 Review |
| #23 | C2-MAJ-2 | Missing antiforgery token on clocking POST | cr:approved | high | major | defect | local | implementer | C2 Review |
| #24 | C2-MIN-2 | EmployeeId spoofable from request body | cr:approved | medium | minor | defect | local | implementer | C2 Review |
| #25 | — | Missing Razor Pages for 9 of 10 UCs | cr:approved | high | major | defect | cross-cutting | implementer | C2 Review |
| #26 | — | C2 baseline blocked — missing Architect approval on PR #21 | cr:approved | critical | blocker | defect | cross-cutting | software-architect | CCM |
| #27 | C2-MAJ-1 | News/Edit form field names mismatch BindProperties | cr:approved | high | major | defect | local | implementer | C2 Review |
## Impact Analysis
### C2 New CRs — Impact Analysis

| Issue # | CR | Affected UCs/FRs | Affected Artifacts | Cost | Schedule Impact | Architectural? |
|---|---|---|---|---|---|---|
| #22 | C2-CRIT-1 | UC-001, FR-001, AC-001 | clocking-retry.js, Index.cshtml, ClockingApi.cshtml | Low — route fix | None — C2 rework | No |
| #23 | C2-MAJ-2 | UC-001, FR-001, AC-001 | clocking-retry.js, Index.cshtml | Low — token/attribute | None — C2 rework | No |
| #24 | C2-MIN-2 | UC-001, CON-004 (OIDC) | ClockingApi.cshtml.cs | Low — claim extraction | None — C2 rework | No |
| #25 | — | UC-002..UC-010, FR-002..FR-010 | All Razor Pages for 9 UCs, Design Model UI layer | Medium — 9 page pairs | Fits C2 rework | No |
| #26 | — | All UCs (baseline gate) | PR #21, C2 baseline | Low — review effort | Blocks C2 close | No |
| #27 | C2-MAJ-1 | UC-006, FR-006 | News/Edit.cshtml, News/Edit.cshtml.cs | Low — property rename | None — C2 rework | No |

### C1 Completed CRs — Impact Reconciliation

| Issue # | CR | Original Impact | Resolution | Verified Via |
|---|---|---|---|---|
| #6 | CR-006 | All 20 test cases blocked by unmerged prototype | PR #4 merged to main | PR #4 closed/merged |
| #10 | CR-010 | FR-008 featured banner broken | NewsService.Publish accepts isFeatured; full chain implemented | PR #20 closed/merged; Review Record MAJOR-1 RESOLVED |
| #11 | CR-011 | Cross-employee idempotency collision | FindByIdempotencyKey(employeeId, key) with composite unique index | PR #20 closed/merged; Review Record MINOR-3 RESOLVED |

### Deferred CRs — Impact Summary (carried from C1)

| Issue # | CR | Affected UCs/FRs | Impact | Deferral Rationale |
|---|---|---|---|---|
| #3 | CR-003 | UC-005, UC-006, UC-007, UC-010 (NFR-004) | Cross-cutting | Medium priority — audit trail validation deferred to integration testing |
| #12 | CR-012 | UC-004, FR-004 | Local | Medium priority — CSV format fix, not blocking UC functionality |
| #13 | CR-013 | UC-009, FR-009 | Local | Medium priority — test assertion fix, not blocking UC functionality |
| #14 | CR-014 | Test quality | Local | Low priority — placeholder test removal |
| #15 | CR-015 | UC-009, Design Model V007 | Local | Medium priority — naming convention, not blocking |
| #17 | CR-017 | UC-001, CON-004 | Local | Medium priority — dead code cleanup, related to #24 |
| #18 | CR-018 | CR-011 dependency | Local | Low priority — test behavior codification, resolved by #11 completion |
## Decisions and Status
### CCB Decisions — Construction C2 (2026-08-28)

| Issue # | CR | Decision | Rationale | CCB Members | Executor |
|---|---|---|---|---|---|
| #22 | C2-CRIT-1 | **APPROVED** | Critical blocker — UC-001 non-functional (404). Must fix in C2 rework. | CCM (chair) | implementer |
| #23 | C2-MAJ-2 | **APPROVED** | High priority — UC-001 POST rejected with 400. Must fix in C2 rework. | CCM (chair) | implementer |
| #24 | C2-MIN-2 | **APPROVED** | Security defect — identity must come from OIDC token (CON-004), not client. Approved despite medium priority due to security implications. | CCM (chair) | implementer |
| #25 | — | **APPROVED** | High priority — 9 of 10 UCs have no UI. Largest gap in C2 deliverable. Must implement in C2 rework. | CCM (chair) | implementer |
| #26 | — | **APPROVED** | Critical blocker — C2 baseline cannot close without Architect sign-off on PR #21. Process gate. | CCM (chair) | software-architect |
| #27 | C2-MAJ-1 | **APPROVED** | High priority — UC-006 non-functional (form binding mismatch). Must fix in C2 rework. | CCM (chair) | implementer |

### Completed CRs — Closure Record

| Issue # | CR | Closure Date | Verified Via | Closure Rationale |
|---|---|---|---|---|
| #6 | CR-006 | 2026-08-28 | PR #4 closed/merged | Architectural prototype on main; all 20 test cases unblocked |
| #10 | CR-010 | 2026-08-28 | PR #20 closed/merged; Review Record MAJOR-1 RESOLVED | IsFeatured flag fully implemented across NewsService, PersistenceGateway, and UI |
| #11 | CR-011 | 2026-08-28 | PR #20 closed/merged; Review Record MINOR-3 RESOLVED | Idempotency key scoped per employee with composite unique index |

### Carried-Forward Approved CRs (no PRs yet)

| Issue # | CR | State | Executor | Notes |
|---|---|---|---|---|
| #1 | CR-001 | cr:approved | software-architect | LDAP PoC — architect-concurred, awaiting implementation |
| #2 | CR-002 | cr:approved | software-architect | Offline Retry — architect-concurred, awaiting implementation |

### Deferred CRs — Next Iteration Pickup

| Issue # | CR | Priority | Notes |
|---|---|---|---|
| #3 | CR-003 | medium | Audit trail validation — pick up in integration testing |
| #12 | CR-012 | medium | CSV export format — minor formatting fix |
| #13 | CR-013 | medium | Test assertion fix — not blocking |
| #14 | CR-014 | low | Placeholder test — cleanup |
| #15 | CR-015 | medium | Naming violation — convention fix |
| #17 | CR-017 | medium | Dead code DTO — related to #24 resolution |
| #18 | CR-018 | low | Test codifies bug — resolved by #11 completion; may close next iteration |

### Process Health Metrics

| Metric | C1 | C2 | Trend |
|---|---|---|---|
| Total CRs | 13 | 18 (+5) | ↑ Volume increasing (expected in Construction) |
| Approved | 6 | 11 (+5) | ↑ Approval rate steady |
| Completed | 0 | 3 (+3) | ↑ Closure rate improving from 0% |
| Deferred | 7 | 7 (0) | → Stable backlog |
| Rejected | 0 | 0 | → No invalid CRs |
| Closure Rate | 0% | 27% | ↑ First closures — process working |
| Avg Age (approved, no PR) | N/A | 2 iterations (#1, #2) | ⚠ Architectural CRs aging — need PRs |
## Traceability
| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| CR-001 (#1) | R001, CON-005 | Derives | UC-009, NovellLdapConnectionAdapter.cs |
| CR-002 (#2) | AC-005, R006 (exposure=6) | Derives | UC-001, ClockingService, clocking-retry.js |
| CR-003 (#3) | NFR-004 | Derives | UC-005, UC-006, UC-007, UC-010 |
| CR-006 (#6) | PR #4, all UCs | DependsOn | PR #4 (merged), PR #9 (merged) |
| CR-010 (#10) | FR-008, Review Record MAJOR-1 | Derives | UC-005, UC-008, NewsService.cs, PublishNews.cshtml.cs |
| CR-011 (#11) | AC-005, Review Record MINOR-3 | Derives | UC-001, ClockingService.cs, IPersistence |
| CR-012 (#12) | FR-004 | Derives | UC-004, CSV export implementation |
| CR-013 (#13) | FR-009 | Derives | UC-009, DirectorySearchModel tests |
| CR-014 (#14) | Test quality | Tests | UnitTest1.cs |
| CR-015 (#15) | Review Record MINOR-1, Design Model V007 | Derives | Directory.cshtml.cs, branch naming |
| CR-017 (#17) | Review Record MINOR-2, CON-004 (OIDC) | Derives | UC-001, ClockingApiController.cs |
| CR-018 (#18) | Review Record MINOR-4, CR-011 | DependsOn | OfflineRetryTests.cs, CR-011 (complete) |
| C2-CRIT-1 (#22) | UC-001, FR-001, AC-001, Review Record C2-CRIT-1 | Derives | clocking-retry.js, Index.cshtml, ClockingApi.cshtml |
| C2-MAJ-2 (#23) | UC-001, FR-001, AC-001, Review Record C2-MAJ-2 | Derives | clocking-retry.js, Index.cshtml |
| C2-MIN-2 (#24) | CON-004 (OIDC), Review Record C2-MIN-2 | Derives | ClockingApi.cshtml.cs |
| #25 | UC-002..UC-010, FR-002..FR-010, Review Record PR #19 | Derives | All Razor Pages, Design Model UI layer |
| #26 | PR #21, Construction C2 baseline | DependsOn | PR #21, all UCs |
| C2-MAJ-1 (#27) | UC-006, FR-006, Review Record C2-MAJ-1 | Derives | News/Edit.cshtml, News/Edit.cshtml.cs, CLS-016..CLS-020 |
