# Branching Strategy — Portal Cuba Corp

**Document Control**

| Field | Value |
|---|---|
| Phase | Transition |
| Status | Active |
| Milestone Target | End of Transition (Product Release) |
| Owner | Configuration Manager |
| Last Updated | 2026-08-30 |
| Prior Phase | Construction — C4 baseline TAGGED (baseline-construction-C4-v1) |
| Current Iteration | Transition Iter 3 (T3) |
| C1 Baseline Status | **TAGGED** — `baseline-construction-C1-v1` @ SHA 16608668ed7a80c05afe8ee08b55bf2945b7b1eb |
| C2 Baseline Status | **BLOCKED** — PR #21 review state NONE (Issue #26); superseded by C3 rework via PR #28 |
| C3 Baseline Status | **BLOCKED** — PR #29 review state NONE (Issue #31); CI main GREEN; superseded by C4 rework via PR #32 |
| C4 Baseline Status | **TAGGED** — `baseline-construction-C4-v1` @ SHA bf0903a846f50f6532f0b4eaac788cff2fe7dae2 |
| T1 Baseline Status | **TAGGED** — `baseline-transition-T1-v1` (PR #35 APPROVED, CI main GREEN) — stakeholder sanction REFUSED (3 binding conditions unmet) |
| T2 Baseline Status | **TAGGED** — `baseline-transition-T2-v1` @ SHA c6a1304b2d22fdbcd9fb6918b3febdde0284a421 (PR #38 APPROVED, CI main GREEN run 33262804733) — stakeholder re-review REFUSED (mock-auth date inconsistency across 7 artifacts) |
| T3 Baseline Status | **TAGGED** — `baseline-transition-T3-v1` @ SHA 66efe297bdae781ad085ac87b1e05f3237798ea8 (PR #41 APPROVED, CI main GREEN run 33310078920) — stakeholder re-review PENDING |

---

## 1. Purpose

This document defines the canonical branching model, naming conventions, baseline
procedure, and change-control integration for the Portal Cuba Corp project. It is
**config-as-code**: it lives in the repository and is consumed directly by the
Integrator, Implementer, Code Reviewer, and Configuration Manager.

Updates to this file are committed **directly to `main`** via `scm_commit_files` —
no pull request is required. The file is documentation; a PR would gate a Markdown
change behind a Reviewer with nothing actionable to inspect, and the file would
not reach downstream consumers (Integrator, Implementer, Reviewer) until the PR
is handled. PRs are for source code; docs are commits.

---

## 2. Naming Conventions

| CI Type | Pattern | Example |
|---|---|---|
| Feature branch (Construction) | `feature/C{n}-{uc-id}-{subject}` | `feature/C1-uc001-clock-in` |
| Feature branch (Elaboration) | `feature/E{n}-{risk-id}[-{mechanism}]` | `feature/E1-architectural-infrastructure` |
| Integration branch | `iteration/E{n}` \| `iteration/C{n}` | `iteration/C1` |
| Hotfix branch (Transition) | `hotfix/{issue-id}` \| `hotfix/T{n}-defect-fixes` | `hotfix/T3-defect-fixes` |
| Chore branch | `chore/{subject}` | `chore/update-ci-config` |
| Baseline tag | `baseline-{phase}{n}-v{x}` | `baseline-transition-T3-v1` |

**Phase encoding in tags:** `elaboration` \| `construction` \| `transition`.
`<n>` is the iteration number (integer). `<x>` is the patch version starting at 1;
re-tag `v2, v3…` only after an explicit rollback or post-baseline critical fix.

---

## 3. Branching Model

### 3.1 Workspace Hierarchy

```plantuml
@startuml Branching_Topology
component "main" as main
component "iteration/E1" as iterE1
component "iteration/C1" as iterC1
component "iteration/C2" as iterC2
component "iteration/C3" as iterC3
component "iteration/C4" as iterC4
component "feature/E1-arch-infra" as featE1
component "feature/C1-presentation" as featC1
component "feature/C2-presentation" as featC2
component "feature/C3-presentation" as featC3
component "feature/C4-rework" as featC4
component "hotfix/T1-defect-fixes" as hotfixT1
component "hotfix/T2-defect-fixes" as hotfixT2
component "hotfix/T3-defect-fixes" as hotfixT3

featE1 --> iterE1 : PR #4 (APPROVED)
iterE1 --> main : PR #7 (APPROVED) — LAM
featC1 --> iterC1 : PR #8 (APPROVED)
iterC1 --> main : PR #9 (APPROVED) — IOC
featC2 --> iterC2 : PR #19 (APPROVED)
iterC2 --> main : PR #21 (BLOCKED → superseded)
featC3 --> iterC3 : PR #28 (APPROVED)
iterC3 --> main : PR #29 (BLOCKED → superseded)
featC4 --> iterC4 : PR #32 (APPROVED)
iterC4 --> main : PR #33 (APPROVED) — GA
hotfixT1 --> main : PR #35 (APPROVED) — T1 baseline
hotfixT2 --> main : PR #38 (APPROVED) — T2 baseline
hotfixT3 --> main : PR #41 (APPROVED) — T3 baseline

note right of main
  Baseline tags on main:
  baseline-elaboration-E1-v1
  baseline-construction-C1-v1
  baseline-construction-C4-v1
  baseline-transition-T1-v1
  baseline-transition-T2-v1
  baseline-transition-T3-v1 (CURRENT)
end note
@enduml
```

### 3.2 Cross-Phase Invariants

- Only the Integrator writes `iteration/*` and `main` (no other role pushes there).
- `ready-for-review` is the Implementer → Code Reviewer handoff label.
- A baseline tag freezes only an APPROVED + CI-green commit.
- Hotfix branches in Transition: `hotfix/T{n}-defect-fixes` from `main`, express review, merge to `main` with a patch baseline tag.
- `docs/BRANCHING_STRATEGY.md` updates go directly to `main` via `scm_commit_files` — no PR.

---

## 4. Baseline Pedigree

```plantuml
@startuml Baseline_Pedigree_Transition
title Baseline Pedigree — Portal Cuba Corp (Transition T3)

state "Inception I1" as I1
state "Inception I2" as I2
state "Elaboration E1" as E1
state "Construction C1" as C1
state "Construction C2" as C2
state "Construction C3" as C3
state "Construction C4" as C4
state "Transition T1" as T1
state "Transition T2" as T2
state "Transition T3" as T3

I1 --> I2 : docs only
I2 --> E1 : LCO
E1 --> C1 : LAM\nbaseline-elaboration-E1-v1
C1 --> C2 : IOC\nbaseline-construction-C1-v1
C2 --> C3 : rework\n(BLOCKED — superseded)
C3 --> C4 : rework\n(BLOCKED — superseded)
C4 --> T1 : GA\nbaseline-construction-C4-v1
T1 --> T2 : stakeholder\nsanction REFUSED\n(3 binding conditions)\nbaseline-transition-T1-v1
T2 --> T3 : stakeholder\nre-review REFUSED\n(mock-auth date\ninconsistency)\nbaseline-transition-T2-v1
T3 : **baseline-transition-T3-v1**\nPR #41 APPROVED\nCI GREEN (run 33310078920)\nSHA 66efe297\nStakeholder re-review PENDING

note right of T3
  T3 resolves 3 binding T2 directives:
  1. Canonical mock-auth expiry date
  2. Change Request updated to Transition
  3. Development Case unfrozen
  
  4 open Major findings (other roles):
  MR-T2-002, CR-F1, TC-F3, RL-F6
  
  Handoff: Issue #42
end note

@enduml
```

### 4.1 Baseline Audit Summary

| Baseline | Tag | PR | Review State | CI Run | SHA | Stakeholder Sanction |
|---|---|---|---|---|---|---|
| Elaboration E1 | `baseline-elaboration-E1-v1` | #7 | APPROVED | GREEN | (LAM) | N/A |
| Construction C1 | `baseline-construction-C1-v1` | #9 | APPROVED | GREEN | 16608668ed7a80c05afe8ee08b55bf2945b7b1eb | N/A |
| Construction C2 | — | #21 | NONE (BLOCKED) | — | — | Superseded by C3 |
| Construction C3 | — | #29 | NONE (BLOCKED) | — | — | Superseded by C4 |
| Construction C4 | `baseline-construction-C4-v1` | #33 | APPROVED | GREEN | bf0903a846f50f6532f0b4eaac788cff2fe7dae2 | N/A |
| Transition T1 | `baseline-transition-T1-v1` | #35 | APPROVED | GREEN | (T1) | REFUSED (3 binding conditions) |
| Transition T2 | `baseline-transition-T2-v1` | #38 | APPROVED | GREEN (33262804733) | c6a1304b2d22fdbcd9fb6918b3febdde0284a421 | REFUSED (mock-auth date inconsistency) |
| Transition T3 | `baseline-transition-T3-v1` | #41 | APPROVED | GREEN (33310078920) | 66efe297bdae781ad085ac87b1e05f3237798ea8 | PENDING |

---

## 5. Configuration Items — Final Inventory (Transition T3)

| CI Category | Items | Storage |
|---|---|---|
| Source Code | `src/` — .NET 10 REST API + Razor Pages frontend | Git `main` |
| Database Schema | PostgreSQL DDL scripts | Git `main` |
| Test Code | Unit + integration tests (xUnit) | Git `main` |
| CI Configuration | GitHub Actions workflow | Git `main` |
| Documentation | RUP artifacts (16 artifacts across disciplines) | Git `main` (SCM-backed) |
| Branching Strategy | `docs/BRANCHING_STRATEGY.md` | Git `main` |
| UI Design | `docs/inputs/employee-portal-design.html` | Git `main` |
| Release Notes | Release Notes artifact (Transition) | Git `main` (SCM-backed) |
| User Documentation | User Documentation artifact (Transition) | Git `main` (SCM-backed) |

---

## 6. Change Control Integration

- Change Requests are tracked as GitHub Issues with labels `cr:new`, `cr:approved`, `cr:complete`.
- The Change Control Manager (CCM) owns the CR state machine and CCB triage.
- The Configuration Manager consumes CCM-triaged outcomes via branches and PRs.
- Naming-convention violations are filed as Issues with `severity:minor` + `nature:defect` + `naming-violation`.
- Gate failures (missing approval, red CI) are filed as Issues with `severity:blocker` + `nature:defect`.

---

## 7. Transition Hotfix Workflow

```plantuml
@startuml Hotfix_Workflow_Transition
title Transition Hotfix Workflow

state "Defect identified" as s1
state "hotfix/T{n}-defect-fixes\nbranch from main" as s2
state "Code Reviewer\nexpress review" as s3
state "PR to main" as s4
state "CM gate check:\nreview == APPROVED\nAND CI == GREEN" as s5
state "Tag baseline-transition-T{n}-v{x}" as s6
state "Stakeholder\nre-review" as s7

s1 --> s2 : branch
s2 --> s3 : ready-for-review label
s3 --> s4 : APPROVED
s4 --> s5 : merged to main
s5 --> s6 : both gates GREEN
s5 --> "File blocker Issue" : gate FAILS
s6 --> s7 : tag written
s7 --> "Product Release\nsanction" : APPROVED
s7 --> s1 : REFUSED — iterate

@enduml
```

---

## 8. Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| baseline-elaboration-E1-v1 | PR #7 (APPROVED) | Realizes | LAM milestone |
| baseline-construction-C1-v1 | PR #9 (APPROVED) | Realizes | IOC milestone |
| baseline-construction-C4-v1 | PR #33 (APPROVED) | Realizes | GA milestone |
| baseline-transition-T1-v1 | PR #35 (APPROVED), CI GREEN | Realizes | T1 release (sanction REFUSED) |
| baseline-transition-T2-v1 | PR #38 (APPROVED), CI GREEN (33262804733) | Realizes | T2 release (sanction REFUSED) |
| baseline-transition-T3-v1 | PR #41 (APPROVED), CI GREEN (33310078920) | Realizes | T3 release (sanction PENDING) |
| T3 release handoff | Issue #42 (handoff:release-notes) | Refines | DeploymentManager release deployment |
| T3 hotfix defect fixes | PR #41 (hotfix/T3-defect-fixes → main) | Realizes | Transition T3 release baseline |
| T3 stakeholder directives | STK-001 directive (T2 refusal) | Refines | (1) Canonical mock-auth expiry, (2) CR updated to Transition, (3) Development Case unfrozen |
| T3 stakeholder sanction | STK-001, AC-001..AC-005 | Refines | PENDING re-review with T3 evidence |
| CR-T2-001 (Minor) | Review Record (T2) | Derives | Mock-auth expiry date consistency (documentation) — ADDRESSED in T3 |
| DM-F2 (Open Minor) | Review Record (T1) | Derives | Design Model traceability update (Designer owns) |