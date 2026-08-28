## Document Control
| Field | Value |
|---|---|
| Phase | Construction |
| Status | Draft |
| Milestone Target | End-of-Construction |
| Iteration | 1 (Cycle 1) |
| Date | 2026-08-28 |
| Prior Phase | Elaboration (LCA achieved, 0 open Critical/Major findings, stakeholder sanction GRANTED) |
| Evolution | Construction C1: Designer class diagrams added (Portal.Services, Portal.Infrastructure, Portal.Domain) with full method signatures; NewsItem state machine added; subsystem interface dependency diagram added; testability entry points defined. UI Designer and Database Designer sections preserved. |
| Contributors | Designer (Analysis Classes, Use-Case Realizations, Design Classes, Interface Contracts, State Machines, Testability); User-Interface Designer (UI View/Controller Classes, UI Patterns, Boundary Classes and Navigation Map); Database Designer (Persistent Data Classes) |
## Design Overview
| Field | Value |
|---|---|
| Phase | Construction |
| Status | Draft |
| Milestone Target | End-of-Construction |
| Iteration | 2 (Cycle 1) |
| Date | 2026-08-28 |
| Contributors | Designer (Analysis Classes, Use-Case Realizations, Design Classes, Interface Contracts, State Machines, Testability); User-Interface Designer (UI View/Controller Classes, UI Patterns, Boundary Classes and Navigation Map); Database Designer (Persistent Data Classes) |

### Technology Stack Alignment

| Layer | Technology | Constraint | Design Mechanism |
|---|---|---|---|
| Presentation | Razor Pages (.NET 10) | CON-002 | Server-rendered HTML; no SPA; clocking-retry.js for offline retry only |
| Application Services | .NET 10 REST API | CON-001 | DI-injected services implementing component interfaces |
| Persistence | EF Core + PostgreSQL | CON-003 | Repository pattern via IPersistence; EF Core DbContext; transaction via ExecuteInTransactionAsync callback |
| Authentication | Keycloak OIDC client | CON-004 | OIDC middleware pipeline; role claims from token |
| Directory | AD over LDAP (Novell.Directory.Ldap) | CON-005, CON-009 | Read-only LDAP gateway; no local copy of employee data |
| Hosting | Internal Windows Server | CON-006, CON-007 | Single-server deployment; intranet-only |

### Layer Mapping

The design follows a three-layer architecture as defined in the SAD Logical View:

| Layer | Package | Design Classes | Interfaces |
|---|---|---|---|
| Presentation | Portal.UI | MainPageModel, ClockingPageModel, AllClockingsModel, PublishNewsModel, EditNewsModel, NewsManagementModel, DirectorySearchModel, WorkerCategoryModel | (Razor Page Models — see UI View/Controller Classes section) |
| Application Services | Portal.Services | ClockingService, NewsService, DirectoryService, WorkerCategoryService, AuditInterceptor | IClockingService, INewsService, IDirectoryService, IWorkerCategoryService, IAuditLogger |
| Infrastructure | Portal.Infrastructure | LdapGateway, PersistenceGateway, PortalDbContext, LdapSettings, LdapConnectionPool | ILdapGateway, IPersistence |
| Domain | Portal.Domain | ClockingRecord, NewsItem, WorkerCategory, AuditRecord, DirectoryEntry, DateRange, ClockingResult, LdapSearchResult, ClockType, ClockStatus, NewsCategory, NewsStatus, AuditAction | (no interfaces — value objects and enums) |

### Design Mechanism Resolution (Three-Level Chain)

| Analysis Mechanism | Design Mechanism (Pattern + Properties) | Implementation Mechanism | Component |
|---|---|---|---|
| Persistence | Repository + Unit of Work via EF Core DbContext; transactional, with unique index on clockings.idempotency_key | EF Core 10 + Npgsql (PostgreSQL) | COMP-006 |
| LDAP Directory Access | Gateway pattern; read-only; connection pooling; attribute mapping with fallback for missing fields (R001) | Novell.Directory.Ldap.NETStandard | COMP-005 |
| Authentication | OIDC client; token validation; role extraction from claims; no local user store | Keycloak (existing) + ASP.NET Core OIDC middleware | COMP-007 |
| Audit Trail | Interceptor pattern; append-only; same DB transaction as business operation; author from OIDC token | EF Core SaveInterceptor + audit_records table; `IAuditLogger.LogAudit()` called within `IPersistence.ExecuteInTransactionAsync()` callback | COMP-008 |
| Offline Retry | Client-side localStorage + POST retry with idempotency key; 5-min window; server accepts client timestamp | clocking-retry.js + IClockingService idempotencyKey param | COMP-002 |
| CSV Export | Streaming response; HR-only; date-range filtered | IClockingService.ExportCsv returns Stream → Razor Page writes to Response.Body | COMP-002 |

### Construction C2 — Design Model Evolution Summary

This iteration evolves the Design Model to align with implementation divergences discovered during source code inspection. Per the lesson learned ("Design Model must be updated when implementation diverges for good reason — silent divergence is always a finding"), the following changes bring the design contracts in sync with the implemented code. No Review Record findings targeted the Design Model (all 8 document artifacts passed with zero findings); the changes resolve implementation-design divergences proactively.

| Change | Rationale | Affected Sections |
|---|---|---|
| INT-002 method names: `PublishNews`→`Publish`, `EditNews`→`Edit`, `UnpublishNews`→`Unpublish`, `GetAllNewsItems`→`ListAll`, `GetNewsById`→`GetById` | Implementation uses concise .NET-idiomatic names; design updated to match | Interface Contracts, Design Packages and Classes, Use-Case Realizations |
| INT-001 method name: `GetAllClockingsForMonth`→`GetAllClockings` | Implementation uses shorter name; IPersistence retains `GetAllClockingsForMonth` | Interface Contracts, Design Packages and Classes |
| INT-005 `entityId` type: `Guid`→`string` | Implementation passes `item.Id.ToString()` for news and `adUserId` (string) for worker categories — `string` accommodates both | Interface Contracts, Design Packages and Classes |
| NewsStatus enum: removed `Draft` state | Implementation creates NewsItem directly as `Published` (UC-005 flow); no draft/approval workflow in scope | Domain Model, Capsules (State Machine) |
| NewsItem: `CreatedBy`→`AuthorId`, no `UpdatedBy` field | Implementation uses `AuthorId`; editor identity captured via `LogAudit` author parameter, not a separate field | Design Packages and Classes, Persistent Data Classes |
| AuditAction enum values: `NewsPublished`→`Publish`, `NewsEdited`→`Edit`, `NewsUnpublished`→`Unpublish` | Implementation uses concise enum names matching operation semantics | Design Packages and Classes, Use-Case Realizations |
| `isFeatured` parameter in INT-002 `Publish` and `Edit` | Design already had `isFeatured` in C1; implementation missing it (MAJOR-1, CR-010). Design is CORRECT — implementation must be fixed. Design Model retains `isFeatured` param. | Interface Contracts, Use-Case Realizations |
| `ExecuteInTransactionAsync` in audit operations | Design specifies wrapping business op + audit in transaction (M2 fix). Implementation calls `LogAudit` outside transaction. Design is CORRECT — implementation must be updated. | Use-Case Realizations (SEQ-005/006/007/010) |

### Prior Iteration Resolution Summary (C1)

| Finding | Design Model Change | Affected Sections |
|---|---|---|
| M1 — IAuditLogger signature mismatch | `Log()` → `LogAudit()` (avoids .NET `ILogger.Log()` collision) | Interface Contracts (INT-005), Use-Case Realizations (SEQ-005/006/007/010), Design Overview |
| M2 — IPersistence transaction API mismatch | Removed `BeginTransaction()`/`CommitTransaction()`; added `ExecuteInTransactionAsync(Func<Task> action)` callback pattern | Interface Contracts (INT-007), Use-Case Realizations (SEQ-005/006/007/010), Design Overview |
## Domain Model
Analysis classes identify the boundary, control, and entity stereotypes for each architecturally significant use case. These are the bridge from the Use-Case Model to design classes — each analysis class will be refined into one or more design classes in the Design Packages and Classes section.

### Analysis Class Catalog

| ID | Name | Stereotype | UC | Responsibility | SAD Component |
|---|---|---|---|---|---|
| ACL-001 | ClockingUI | <<boundary>> | UC-001 | Display clock in/out button; capture timestamp; show confirmation; manage localStorage retry | COMP-002 |
| ACL-002 | ClockingController | <<control>> | UC-001, UC-002, UC-003, UC-004 | Record clocking with idempotency; get current status; get history; get all clockings; export CSV | COMP-002 |
| ACL-003 | ClockingRecord | <<entity>> | UC-001 | Persist clocking entry: employeeId, timestamp, clockType, idempotencyKey | COMP-006 |
| ACL-004 | DirectorySearchUI | <<boundary>> | UC-009 | Display search form; display results; warn about missing AD attributes | COMP-001 |
| ACL-005 | DirectoryController | <<control>> | UC-009 | Search AD via LDAP; map LDAP attributes to DirectoryEntry; handle missing attributes (R001) | COMP-001, COMP-005 |
| ACL-006 | DirectoryEntry | <<entity>> | UC-009 | Value object: name, jobTitle, department, office, email, extension — projected from AD at read time | COMP-005 |
| ACL-007 | NewsUI | <<boundary>> | UC-005, UC-006, UC-007 | Display publish/edit forms; display news list; confirm unpublish | COMP-003 |
| ACL-008 | NewsController | <<control>> | UC-005, UC-006, UC-007 | Publish, edit, unpublish news; integrate audit trail; list published and all | COMP-003, COMP-008 |
| ACL-009 | NewsItem | <<entity>> | UC-005, UC-006, UC-007, UC-008 | News content: title, body, category, status, createdBy, createdAt, isFeatured | COMP-006 |
| ACL-010 | AuditRecord | <<entity>> | UC-005, UC-006, UC-007, UC-010 | Append-only audit: entityType, entityId, action, author, timestamp | COMP-008 |
| ACL-011 | CategoryUI | <<boundary>> | UC-010 | Display category list; display assign form; show confirmation | COMP-004 |
| ACL-012 | CategoryController | <<control>> | UC-010 | Assign category; list categories; lookup AD user | COMP-004, COMP-005 |
| ACL-013 | WorkerCategory | <<entity>> | UC-010 | AD user id → category link (two columns, nothing else) | COMP-006 |
| ACL-014 | NewsFeedUI | <<boundary>> | UC-008 | Display news feed; filter by category; display featured banners | COMP-003 |
| ACL-015 | NewsFeedController | <<control>> | UC-008 | Get published news; get featured news | COMP-003 |

### Analysis Class Diagram

```plantuml
@startuml
title Portal Cuba Corp — Analysis Classes (Elaboration)

skinparam classAttributeIconSize 0
skinparam packageStyle rectangle

package "UC-001: Clock In / Clock Out" {
  class "ClockingUI" as ACL001 <<boundary>> {
    + displayClockButton(status)
    + captureTimestamp()
    + showConfirmation(record)
    + storeLocalForRetry(data)
    + retryPost()
  }
  class "ClockingController" as ACL002 <<control>> {
    + recordClocking(empId, timestamp, type, idempotencyKey)
    + getCurrentStatus(empId)
    + getHistory(empId, month)
    + getAllClockings(month)
    + exportCsv(month)
  }
  class "ClockingRecord" as ACL003 <<entity>> {
    + employeeId : string
    + timestamp : DateTime
    + clockType : ClockType
    + idempotencyKey : string
  }
}

package "UC-009: Search Employee Directory" {
  class "DirectorySearchUI" as ACL004 <<boundary>> {
    + displaySearchForm()
    + displayResults(entries)
    + displayMissingAttrWarning()
  }
  class "DirectoryController" as ACL005 <<control>> {
    + search(query) : List<DirectoryEntry>
    + mapLdapAttributes(entry) : DirectoryEntry
  }
  class "DirectoryEntry" as ACL006 <<entity>> {
    + name : string
    + jobTitle : string
    + department : string
    + office : string
    + email : string
    + extension : string
  }
}

package "UC-005/006/007: News Lifecycle" {
  class "NewsUI" as ACL007 <<boundary>> {
    + displayPublishForm()
    + displayEditForm(item)
    + displayNewsList(items)
    + confirmUnpublish(id)
  }
  class "NewsController" as ACL008 <<control>> {
    + publish(title, body, category, authorId)
    + edit(id, title, body, category, authorId)
    + unpublish(id, authorId)
    + listPublished()
    + listAll()
  }
  class "NewsItem" as ACL009 <<entity>> {
    + id : Guid
    + title : string
    + body : string
    + category : NewsCategory
    + status : NewsStatus
    + createdBy : string
    + createdAt : DateTime
    + isFeatured : bool
  }
  class "AuditRecord" as ACL010 <<entity>> {
    + entityType : string
    + entityId : Guid
    + action : AuditAction
    + author : string
    + timestamp : DateTime
  }
}

package "UC-010: Manage Worker Category" {
  class "CategoryUI" as ACL011 <<boundary>> {
    + displayCategoryList()
    + displayAssignForm()
    + showConfirmation()
  }
  class "CategoryController" as ACL012 <<control>> {
    + assignCategory(adUserId, category, authorId)
    + listCategories()
    + lookupAdUser(query)
  }
  class "WorkerCategory" as ACL013 <<entity>> {
    + adUserId : string
    + category : string
  }
}

package "UC-008: Read and Filter News" {
  class "NewsFeedUI" as ACL014 <<boundary>> {
    + displayNewsFeed(items)
    + filterByCategory(cat)
    + displayFeatured(items)
  }
  class "NewsFeedController" as ACL015 <<control>> {
    + getPublishedNews(category?)
    + getFeaturedNews()
  }
}

ACL001 --> ACL002
ACL002 --> ACL003
ACL004 --> ACL005
ACL005 --> ACL006
ACL007 --> ACL008
ACL008 --> ACL009
ACL008 --> ACL010
ACL011 --> ACL012
ACL012 --> ACL013
ACL014 --> ACL015
ACL015 --> ACL009

note right of ACL005
  R001: LDAP attribute
  consistency risk —
  fallback to "N/A" for
  missing fields
end note

note right of ACL002
  AC-005: offline retry
  via localStorage +
  idempotency key
  NFR-002: <1s response
end note

note right of ACL008
  NFR-004: audit trail
  for publish/edit/unpublish
  AuditRecord is append-only
end note

@enduml
```

### Design Mechanism Resolution Summary

Each analysis mechanism from Inception is resolved to a design mechanism (pattern + properties). Implementation mechanisms are specified only where the stakeholder declared the technology.

| Analysis Mechanism | Design Mechanism | Properties | Implementation (where declared) |
|---|---|---|---|
| Persistence | Repository + Unit of Work (EF Core DbContext) | Transactional; unique index on clockings.idempotency_key; append-only audit_records | EF Core 10 + Npgsql (CON-001, CON-003) |
| LDAP Directory Access | Gateway (read-only) | Connection pooling; attribute mapping with fallback; no writes to AD | Novell.Directory.Ldap.NETStandard (CON-005) |
| Authentication | OIDC Client | Token validation; role extraction from claims; no local user store | Keycloak existing (CON-004) |
| Audit Trail | Interceptor (same transaction) | Append-only; author from OIDC token; timestamp from server; never hard-delete news | EF Core SaveInterceptor (CON-001) |
| Offline Retry | localStorage + POST Retry | 5-min window; idempotency key prevents duplicates; server accepts client timestamp; clocking only | clocking-retry.js (CON-002) |
| CSV Export | Streaming Response | HR-only; date-range filtered; streaming to Response.Body | .NET 10 FileStreamResult (CON-001) |
## Use-Case Realizations
Each architecturally significant use case is realized as a collaboration of design objects. Sequence diagrams show the message flow between boundary (UI), control (service), and entity (repository) objects for each UC's main flow and key alternative/error flows.

> **Construction C2 — Implementation Alignment:** All sequence diagrams updated to reflect actual implementation method names (Publish, Edit, Unpublish, GetById, ListAll, GetAllClockings, Search, AssignCategory, LookupAdUser). Error paths and validation failures shown explicitly. `isFeatured` parameter included in UC-005/006 (CR-010). `ExecuteInTransactionAsync` shown as design intent — implementation pending.

### Realization Index

| UC ID | UC Name | Priority | Seq ID | Key Risks/NFRs |
|---|---|---|---|---|
| UC-001 | Clock In / Clock Out | 2 | SEQ-001 | AC-005 offline retry, NFR-002 <1s |
| UC-002 | View Own Clocking History | 6 | SEQ-002 | — |
| UC-003 | View All Employee Clockings | 7 | SEQ-003 | LDAP name resolution (CON-009) |
| UC-004 | Export Monthly Clocking Report | 5 | SEQ-004 | PERF-004 streaming CSV |
| UC-005 | Publish News | 3 | SEQ-005 | NFR-004 audit trail |
| UC-006 | Edit Published News | 8 | SEQ-006 | NFR-004 audit trail |
| UC-007 | Unpublish News | 9 | SEQ-007 | CON-013 no hard delete |
| UC-008 | Read and Filter News | 10 | SEQ-008 | — |
| UC-009 | Search Employee Directory | 1 | SEQ-009 | R001 LDAP attribute risk |
| UC-010 | Manage Worker Category | 4 | SEQ-010 | Bridges DB + LDAP, NFR-004 audit |

### SEQ-001: UC-001 — Clock In / Clock Out

```plantuml
@startuml
title UC-001: Clock In / Clock Out (Construction C2 — Aligned with Implementation)

actor Employee as EMP
participant "Clocking UI\n+ clocking-retry.js" as UI
participant "ClockingService\n(CLS-001, COMP-002)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
database "PostgreSQL" as PG

EMP -> UI : Press Clock In/Out button
UI -> UI : Capture timestamp +\ngenerate idempotency key (UUID)

alt Network available (normal path)
  UI -> SVC : RecordClocking(employeeId,\n  timestamp, type, idempotencyKey)

  SVC -> SVC : Validate employeeId and\n  idempotencyKey non-empty

  alt Validation fails
    SVC --> UI : ClockingResult.Fail(msg)
    UI --> EMP : Show error
  end

  SVC -> DB : FindByIdempotencyKey(key)
  DB -> PG : SELECT WHERE\n  idempotency_key = ?

  alt Duplicate key found
    PG --> DB : Existing record
    DB --> SVC : ClockingRecord
    SVC --> UI : ClockingResult.Duplicate(record)
    UI --> EMP : "Already clocked"\n  (same result as original)
  else New entry
    PG --> DB : null
    DB --> SVC : null
    SVC -> DB : InsertClocking(record)
    DB -> PG : INSERT INTO clockings
    PG --> DB : Saved
    DB --> SVC : ClockingRecord
    SVC --> UI : ClockingResult.Ok(record)
    UI --> EMP : Show confirmation\n  (time + direction)
  end

else Network down (AC-005 offline)
  UI -> UI : Store in localStorage:\n  {employeeId, timestamp, type,\n  idempotencyKey}
  UI -> UI : Start retry timer

  alt Network restored within 5 min
    UI -> SVC : POST /api/clocking\n      (same payload)
    SVC -> DB : FindByIdempotencyKey(key)
    DB -> PG : SELECT
    alt Duplicate (already synced)
      SVC --> UI : ClockingResult.Duplicate
      UI -> UI : Clear localStorage
      UI --> EMP : "Already clocked"
    else New
      SVC -> DB : InsertClocking(record)
      DB -> PG : INSERT
      SVC --> UI : ClockingResult.Ok
      UI -> UI : Clear localStorage
      UI --> EMP : Show confirmation
    end
  else 5 min elapsed
    UI -> UI : Stop retry
    UI --> EMP : "Could not sync —\n  please contact HR"
  end
end

note right of SVC
  C2 UPDATE: Method name
  RecordClocking (unchanged).
  Returns ClockingResult with
  Ok/Duplicate/Fail factory methods.
  Server-side validation for empty
  employeeId and idempotencyKey
  (MINOR-3 fix, CR-011).
end note

@enduml
```

### SEQ-002: UC-002 — View Own Clocking History

```plantuml
@startuml
title UC-002: View Own Clocking History (Construction C2)

actor Employee as EMP
participant "ClockingPage UI\n(V002)" as UI
participant "ClockingService\n(CLS-001, COMP-002)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
database "PostgreSQL" as PG

EMP -> UI : Navigate to "My Clockings"
UI -> SVC : GetCurrentStatus(employeeId)
SVC -> DB : GetClockingsByEmployee(\n  employeeId, currentMonthRange)
DB -> PG : SELECT FROM clockings\n  WHERE employee_id = ?\n  AND timestamp BETWEEN ? AND ?\n  ORDER BY timestamp DESC
PG --> DB : List<ClockingRecord>
DB --> SVC : List<ClockingRecord>
SVC -> SVC : Determine status from\n  most recent record
SVC --> UI : ClockStatus (In/Out)

UI -> SVC : GetHistory(employeeId,\n  currentMonthRange)
SVC -> DB : GetClockingsByEmployee(\n  employeeId, monthRange)
DB -> PG : SELECT
PG --> DB : List<ClockingRecord>
DB --> SVC : List<ClockingRecord>
SVC --> UI : List<ClockingRecord>
UI --> EMP : Show clocking history\ntable for current month

note right of SVC
  C2: Method names aligned.
  GetCurrentStatus derives from
  most recent ClockingRecord.Type.
  DateRange.ForMonth used for
  current month filtering.
end note

@enduml
```

### SEQ-003: UC-003 — View All Employee Clockings

```plantuml
@startuml
title UC-003: View All Employee Clockings (Construction C2)

actor "HR Admin" as HR
participant "AllClockings UI\n(V003)" as UI
participant "ClockingService\n(CLS-001, COMP-002)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
database "PostgreSQL" as PG

HR -> UI : Navigate to "All Clockings"
HR -> UI : Select month filter
UI -> SVC : GetAllClockings(monthRange)
SVC -> DB : GetAllClockingsForMonth(\n  monthRange)
DB -> PG : SELECT FROM clockings\n  WHERE timestamp BETWEEN ? AND ?\n  ORDER BY employee_id, timestamp
PG --> DB : List<ClockingRecord>
DB --> SVC : List<ClockingRecord>
SVC --> UI : List<ClockingRecord>
UI --> HR : Show all employees'\nclockings for selected month

note right of SVC
  C2: GetAllClockings (was
  GetAllClockingsForMonth in INT-001).
  IPersistence method retains
  GetAllClockingsForMonth name.
  Employee names resolved from
  OIDC token subject — no LDAP
  call in current implementation.
end note

@enduml
```

### SEQ-004: UC-004 — Export Monthly Clocking Report

```plantuml
@startuml
title UC-004: Export Monthly Clocking Report (Construction C2)

actor "HR Admin" as HR
participant "AllClockings UI\n(V003)" as UI
participant "ClockingService\n(CLS-001, COMP-002)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
database "PostgreSQL" as PG

HR -> UI : Click "Export CSV"
UI -> SVC : ExportCsv(monthRange)
SVC -> DB : GetAllClockingsForMonth(\n  monthRange)
DB -> PG : SELECT FROM clockings\n  WHERE timestamp BETWEEN ? AND ?
PG --> DB : List<ClockingRecord>
DB --> SVC : List<ClockingRecord>

SVC -> SVC : Group by EmployeeId,\n  order by timestamp
SVC -> SVC : Write CSV:\n  "Employee,Date,TimeIn,TimeOut,Direction"
loop For each clocking record
  SVC -> SVC : Format row:\n  {employeeId},\n  {yyyy-MM-dd},\n  {HH:mm:ss},\n  {direction}
end
SVC --> UI : Stream (CSV content)
UI -> HR : Browser downloads CSV file

note right of SVC
  C2: ExportCsv returns Stream.
  CSV header: Employee,Date,
  TimeIn,TimeOut,Direction.
  CR-012 (deferred): TimeOut
  column currently empty —
  pairing logic not implemented.
  PERF-004: Streaming response.
end note

@enduml
```

### SEQ-005: UC-005 — Publish News

```plantuml
@startuml
title UC-005: Publish News (Construction C2 — Aligned with Implementation)

actor "HR Admin" as HR
participant "PublishNews UI\n(V004)" as UI
participant "NewsService\n(CLS-002, COMP-003)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
participant "AuditInterceptor\n(CLS-005, COMP-008)" as AUDIT
database "PostgreSQL" as PG

HR -> UI : Fill form (title, body,\ncategory, isFeatured checkbox)
UI -> UI : Validate fields client-side

alt Validation fails
  UI --> HR : Show validation errors
end

UI -> SVC : Publish(title, body, category,\nauthorId, isFeatured)

SVC -> SVC : Validate title/body\nnon-empty

alt Invalid input
  SVC --> UI : ArgumentException
  UI --> HR : Show error message
end

SVC -> SVC : Create NewsItem {\n  Title, Body, Category,\n  Status=Published, IsFeatured,\n  AuthorId, CreatedAt=now,\n  UpdatedAt=now\n}

SVC -> DB : SaveNewsItem(item)
DB -> PG : INSERT INTO news_items
PG --> DB : Saved (with generated Id)
DB --> SVC : NewsItem with Id

SVC -> AUDIT : LogAudit(\n  entityType="NEWS_ITEM",\n  entityId=item.Id.ToString(),\n  action=AuditAction.Publish,\n  author=authorId,\n  timestamp=now)
AUDIT -> DB : InsertAuditRecord(record)
DB -> PG : INSERT INTO audit_records
PG --> DB : Saved

SVC --> UI : NewsItem published
UI --> HR : Show confirmation +\nredirect to news management

note right of SVC
  C2 UPDATE: Method name Publish()
  (was PublishNews). isFeatured param
  included (CR-010, MAJOR-1 fix).
  Audit via LogAudit (M1 fix).
  TODO: Wrap SaveNewsItem + audit in
  ExecuteInTransactionAsync (M2 design
  correct, implementation pending).
end note

@enduml
```

### SEQ-006: UC-006 — Edit Published News

```plantuml
@startuml
title UC-006: Edit Published News (Construction C2 — Aligned with Implementation)

actor "HR Admin" as HR
participant "EditNews UI\n(V005)" as UI
participant "NewsService\n(CLS-002, COMP-003)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
participant "AuditInterceptor\n(CLS-005, COMP-008)" as AUDIT
database "PostgreSQL" as PG

HR -> UI : Select news item to edit
UI -> SVC : GetById(id)
SVC -> DB : GetNewsItem(id)
DB -> PG : SELECT FROM news_items\nWHERE id = ?
PG --> DB : NewsItem
DB --> SVC : NewsItem
SVC --> UI : Pre-populate form\n(title, body, category, isFeatured)

HR -> UI : Edit fields + submit
UI -> SVC : Edit(id, title, body,\ncategory, authorId, isFeatured)

SVC -> SVC : Validate title/body\nnon-empty

alt Invalid input
  SVC --> UI : ArgumentException
  UI --> HR : Show error
end

SVC -> DB : GetNewsItem(id)
DB -> PG : SELECT
PG --> DB : Existing item
DB --> SVC : Existing item

alt NewsItem not found
  SVC --> UI : InvalidOperationException
  UI --> HR : Show "not found" error
end

SVC -> DB : UpdateNewsItem(id, title,\nbody, category)
DB -> PG : UPDATE news_items SET ...\nWHERE id = ?
PG --> DB : Updated
DB --> SVC : Updated NewsItem

SVC -> AUDIT : LogAudit(\n  entityType="NEWS_ITEM",\n  entityId=id.ToString(),\n  action=AuditAction.Edit,\n  author=authorId,\n  timestamp=now)
AUDIT -> DB : InsertAuditRecord(record)
DB -> PG : INSERT INTO audit_records
PG --> DB : Saved

SVC --> UI : Updated NewsItem
UI --> HR : Show confirmation

note right of SVC
  C2 UPDATE: Method name Edit()
  (was EditNews). isFeatured param
  included (CR-010).
  Audit: AuditAction.Edit.
  TODO: Wrap update + audit in
  ExecuteInTransactionAsync.
end note

@enduml
```

### SEQ-007: UC-007 — Unpublish News

```plantuml
@startuml
title UC-007: Unpublish News (Construction C2 — Aligned with Implementation)

actor "HR Admin" as HR
participant "NewsMgmt UI\n(V006)" as UI
participant "NewsService\n(CLS-002, COMP-003)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
participant "AuditInterceptor\n(CLS-005, COMP-008)" as AUDIT
database "PostgreSQL" as PG

HR -> UI : Click "Unpublish" on\na news item
UI -> UI : Show confirmation dialog

alt HR confirms
  UI -> SVC : Unpublish(id, authorId)

  SVC -> DB : GetNewsItem(id)
  DB -> PG : SELECT FROM news_items
  PG --> DB : NewsItem
  DB --> SVC : NewsItem

  alt NewsItem not found
    SVC --> UI : InvalidOperationException
    UI --> HR : Show "not found" error
  end

  SVC -> DB : UpdateNewsStatus(id,\n  NewsStatus.Unpublished)
  DB -> PG : UPDATE news_items SET\n  status='Unpublished'\n  WHERE id = ?
  PG --> DB : Updated
  DB --> SVC : Updated NewsItem

  SVC -> AUDIT : LogAudit(\n    entityType="NEWS_ITEM",\n    entityId=id.ToString(),\n    action=AuditAction.Unpublish,\n    author=authorId,\n    timestamp=now)
  AUDIT -> DB : InsertAuditRecord(record)
  DB -> PG : INSERT INTO audit_records
  PG --> DB : Saved

  SVC --> UI : Unpublished NewsItem
  UI --> HR : Show confirmation\n(item hidden, record preserved)
else HR cancels
  UI --> HR : Return to news list
end

note right of SVC
  C2 UPDATE: Method name Unpublish()
  (was UnpublishNews).
  CON-013: Record preserved, NOT deleted.
  Status set to Unpublished only.
  Audit: AuditAction.Unpublish.
  TODO: Wrap status update + audit in
  ExecuteInTransactionAsync.
end note

@enduml
```

### SEQ-008: UC-008 — Read and Filter News

```plantuml
@startuml
title UC-008: Read and Filter News (Construction C2)

actor Employee as EMP
participant "MainPage UI\n(V001)" as UI
participant "NewsService\n(CLS-002, COMP-003)" as SVC
participant "PersistenceGateway\n(COMP-006)" as DB
database "PostgreSQL" as PG

== Load News Feed ==

EMP -> UI : Navigate to main page
UI -> SVC : GetFeaturedNews()
SVC -> DB : GetFeaturedNews()
DB -> PG : SELECT FROM news_items\n  WHERE status='Published'\n  AND is_featured = true\n  ORDER BY created_at DESC
PG --> DB : List<NewsItem>
DB --> SVC : List<NewsItem>
SVC --> UI : Featured news items

UI -> SVC : GetPublishedNews(null)
SVC -> DB : GetPublishedNews(null)
DB -> PG : SELECT FROM news_items\n  WHERE status='Published'\n  ORDER BY created_at DESC
PG --> DB : List<NewsItem>
DB --> SVC : List<NewsItem>
SVC --> UI : All published news
UI --> EMP : Show featured banners\nat top + news feed below

== Filter by Category ==

EMP -> UI : Select category filter\n(General, HR, IT, Events)
UI -> SVC : GetPublishedNews(category)
SVC -> DB : GetPublishedNews(category)
DB -> PG : SELECT FROM news_items\n  WHERE status='Published'\n  AND category = ?\n  ORDER BY created_at DESC
PG --> DB : List<NewsItem>
DB --> SVC : List<NewsItem>
SVC --> UI : Filtered news items
UI --> EMP : Show filtered news feed

note right of SVC
  C2: GetFeaturedNews queries
  is_featured=true AND
  status='Published' (FR-008).
  CR-010: isFeatured now settable
  in Publish/Edit — featured banner
  functional once Implementer
  applies CR-010 fix.
end note

@enduml
```

### SEQ-009: UC-009 — Search Employee Directory

```plantuml
@startuml
title UC-009: Search Employee Directory (Construction C2 — Aligned with Implementation)

actor Employee as EMP
participant "DirectorySearch UI\n(V007)" as UI
participant "DirectoryService\n(CLS-003, COMP-001)" as SVC
participant "LdapGateway\n(CLS-006, COMP-005)" as LDAP
database "Active Directory\n(LDAP)" as AD

EMP -> UI : Enter search query\n(name, department, or office)
UI -> SVC : Search(query)

alt Empty query
  SVC --> UI : Empty list
  UI --> EMP : Show "enter search term"
end

SVC -> SVC : EscapeLdapFilter(query)
SVC -> LDAP : SearchEntries(filter)
LDAP -> AD : LDAP search\n(|(cn=*query*)\n(department=*query*)\n(physicalDeliveryOfficeName=*query*))

alt AD returns results
  AD --> LDAP : Matching entries
  LDAP --> SVC : List<LdapSearchResult>
  SVC -> SVC : Map each result via\n  DirectoryEntry.FromLdapAttributes\n  (missing attrs → "N/A", R001)
  SVC --> UI : List<DirectoryEntry>
  UI --> EMP : Show results table:\n  name, title, dept, office,\n  email, extension
else No results
  AD --> LDAP : Empty
  LDAP --> SVC : Empty list
  SVC --> UI : Empty list
  UI --> EMP : "No colleagues found"
end

alt LDAP connection error
  LDAP --> SVC : Exception
  SVC --> UI : Error
  UI --> EMP : "Directory unavailable,\n  please try later"
end

note right of SVC
  C2 UPDATE: Method name Search()
  (unchanged). EscapeLdapFilter is
  private helper. R001 fallback:
  missing AD attributes → "N/A"
  via FromLdapAttributes factory.
  Corporate data only (CON-012).
end note

@enduml
```

### SEQ-010: UC-010 — Manage Worker Category

```plantuml
@startuml
title UC-010: Manage Worker Category (Construction C2 — Aligned with Implementation)

actor "HR Admin" as HR
participant "WorkerCategory UI\n(V008)" as UI
participant "WorkerCategoryService\n(CLS-004, COMP-004)" as SVC
participant "LdapGateway\n(CLS-006, COMP-005)" as LDAP
participant "PersistenceGateway\n(COMP-006)" as DB
participant "AuditInterceptor\n(CLS-005, COMP-008)" as AUDIT
database "PostgreSQL" as PG
database "Active Directory\n(LDAP)" as AD

== AD User Lookup ==

HR -> UI : Search for employee
UI -> SVC : LookupAdUser(query)
SVC -> SVC : EscapeLdapFilter(query)
SVC -> LDAP : SearchEntries(filter)
LDAP -> AD : LDAP search\n(|(cn=*query*)\n(sAMAccountName=*query*))
AD --> LDAP : Matching entries
LDAP --> SVC : List<LdapSearchResult>
SVC -> SVC : Map to DirectoryEntry\nvia FromLdapAttributes\n(missing → "N/A")
SVC --> UI : Employee list
UI --> HR : Show matching employees

== Category Assignment ==

HR -> UI : Select employee + category
UI -> SVC : AssignCategory(adUserId,\ncategory, authorId)

SVC -> SVC : Validate adUserId and\ncategory non-empty

alt Invalid input
  SVC --> UI : ArgumentException
  UI --> HR : Show error
end

SVC -> DB : UpsertWorkerCategory(\n  adUserId, category)
DB -> PG : INSERT ... ON CONFLICT\n  (ad_user_id) DO UPDATE\n  SET category = ?
PG --> DB : Saved
DB --> SVC : WorkerCategory

SVC -> AUDIT : LogAudit(\n  entityType="WORKER_CATEGORY",\n  entityId=adUserId,\n  action=AuditAction.CategoryChanged,\n  author=authorId,\n  timestamp=now)
AUDIT -> DB : InsertAuditRecord(record)
DB -> PG : INSERT INTO audit_records
PG --> DB : Saved

SVC --> UI : Category updated
UI --> HR : Show confirmation

note right of SVC
  C2 UPDATE: Method names aligned
  with implementation.
  Audit: AuditAction.CategoryChanged.
  entityId is string (adUserId), not Guid.
  TODO: Wrap upsert + audit in
  ExecuteInTransactionAsync.
end note

@enduml
```
## Design Packages and Classes
### Designer Class Diagrams — Application Services (Portal.Services)

> **Contributed by:** Designer (Analysis & Design Discipline)
> **Iteration:** Construction C2 — method signatures aligned with implementation

```plantuml
@startuml
title Portal Cuba Corp — Portal.Services Package (Construction C2 — Aligned with Implementation)

skinparam classAttributeIconSize 0

package "Portal.Services (Application Layer)" {

  interface "IClockingService\n(INT-001)" as INT001 {
    + RecordClocking(employeeId: string, timestamp: DateTime, type: ClockType, idempotencyKey: string) : ClockingResult
    + GetCurrentStatus(employeeId: string) : ClockStatus
    + GetHistory(employeeId: string, month: DateRange) : List<ClockingRecord>
    + GetAllClockings(month: DateRange) : List<ClockingRecord>
    + ExportCsv(month: DateRange) : Stream
  }

  class "ClockingService\n(CLS-001)" as CLS001 {
    - _persistence : IPersistence
    + RecordClocking(employeeId: string, timestamp: DateTime, type: ClockType, idempotencyKey: string) : ClockingResult
    + GetCurrentStatus(employeeId: string) : ClockStatus
    + GetHistory(employeeId: string, month: DateRange) : List<ClockingRecord>
    + GetAllClockings(month: DateRange) : List<ClockingRecord>
    + ExportCsv(month: DateRange) : Stream
  }

  interface "INewsService\n(INT-002)" as INT002 {
    + Publish(title: string, body: string, category: NewsCategory, authorId: string, isFeatured: bool) : NewsItem
    + Edit(id: Guid, title: string, body: string, category: NewsCategory, authorId: string, isFeatured: bool) : NewsItem
    + Unpublish(id: Guid, authorId: string) : NewsItem
    + GetById(id: Guid) : NewsItem?
    + GetPublishedNews(category: NewsCategory?) : List<NewsItem>
    + GetFeaturedNews() : List<NewsItem>
    + ListAll() : List<NewsItem>
  }

  class "NewsService\n(CLS-002)" as CLS002 {
    - _persistence : IPersistence
    - _auditLogger : IAuditLogger
    + Publish(title: string, body: string, category: NewsCategory, authorId: string, isFeatured: bool) : NewsItem
    + Edit(id: Guid, title: string, body: string, category: NewsCategory, authorId: string, isFeatured: bool) : NewsItem
    + Unpublish(id: Guid, authorId: string) : NewsItem
    + GetById(id: Guid) : NewsItem?
    + GetPublishedNews(category: NewsCategory?) : List<NewsItem>
    + GetFeaturedNews() : List<NewsItem>
    + ListAll() : List<NewsItem>
  }

  interface "IDirectoryService\n(INT-003)" as INT003 {
    + Search(query: string) : List<DirectoryEntry>
  }

  class "DirectoryService\n(CLS-003)" as CLS003 {
    - _ldapGateway : ILdapGateway
    + Search(query: string) : List<DirectoryEntry>
    - EscapeLdapFilter(value: string) : string
  }

  interface "IWorkerCategoryService\n(INT-004)" as INT004 {
    + AssignCategory(adUserId: string, category: string, authorId: string) : WorkerCategory
    + ListCategories() : List<WorkerCategory>
    + LookupAdUser(query: string) : List<DirectoryEntry>
  }

  class "WorkerCategoryService\n(CLS-004)" as CLS004 {
    - _persistence : IPersistence
    - _ldapGateway : ILdapGateway
    - _auditLogger : IAuditLogger
    + AssignCategory(adUserId: string, category: string, authorId: string) : WorkerCategory
    + ListCategories() : List<WorkerCategory>
    + LookupAdUser(query: string) : List<DirectoryEntry>
    - EscapeLdapFilter(value: string) : string
  }

  interface "IAuditLogger\n(INT-005)" as INT005 {
    + LogAudit(entityType: string, entityId: string, action: AuditAction, author: string, timestamp: DateTime) : void
  }

  class "AuditInterceptor\n(CLS-005)" as CLS005 {
    - _persistence : IPersistence
    + LogAudit(entityType: string, entityId: string, action: AuditAction, author: string, timestamp: DateTime) : void
  }
}

INT001 <|.. CLS001
INT002 <|.. CLS002
INT003 <|.. CLS003
INT004 <|.. CLS004
INT005 <|.. CLS005

CLS002 --> INT005 : _auditLogger
CLS002 --> INT007 : _persistence
CLS004 --> INT005 : _auditLogger
CLS004 --> INT006 : _ldapGateway
CLS004 --> INT007 : _persistence
CLS003 --> INT006 : _ldapGateway
CLS001 --> INT007 : _persistence

note right of INT002
  C2 UPDATE: Method names aligned
  with implementation (Publish, Edit,
  Unpublish, GetById, ListAll).
  isFeatured param added to Publish
  and Edit (CR-010, MAJOR-1 fix).
end note

note right of INT005
  C2 UPDATE: entityId type changed
  from Guid to string — implementation
  uses string for both Guid and
  adUserId entity IDs.
end note

@enduml
```

### Designer Class Diagrams — Infrastructure (Portal.Infrastructure)

> **Contributed by:** Designer (Analysis & Design Discipline)
> **Iteration:** Construction C2 — interface signatures aligned with implementation

```plantuml
@startuml
title Portal Cuba Corp — Portal.Infrastructure Package (Construction C2)

skinparam classAttributeIconSize 0

package "Portal.Infrastructure (Infrastructure Layer)" {

  interface "ILdapGateway\n(INT-006)" as INT006 {
    + SearchEntries(filter: string) : List<LdapSearchResult>
  }

  class "LdapGateway\n(CLS-006)" as CLS006 {
    - _settings : LdapSettings
    - _pool : LdapConnectionPool
    + SearchEntries(filter: string) : List<LdapSearchResult>
    - MapAttributes(entry: LdapEntry) : LdapSearchResult
    - AcquireConnection() : ILdapConnection
  }

  interface "IPersistence\n(INT-007)" as INT007 {
    + GetClockingsByEmployee(empId: string, range: DateRange) : List<ClockingRecord>
    + GetAllClockingsForMonth(range: DateRange) : List<ClockingRecord>
    + InsertClocking(record: ClockingRecord) : ClockingRecord
    + FindByIdempotencyKey(key: string) : ClockingRecord?
    + SaveNewsItem(item: NewsItem) : NewsItem
    + GetNewsItem(id: Guid) : NewsItem?
    + UpdateNewsItem(id: Guid, title: string, body: string, category: NewsCategory) : NewsItem
    + UpdateNewsStatus(id: Guid, status: NewsStatus) : NewsItem
    + GetPublishedNews(category: NewsCategory?) : List<NewsItem>
    + GetFeaturedNews() : List<NewsItem>
    + GetAllNewsItems() : List<NewsItem>
    + UpsertWorkerCategory(adUserId: string, category: string) : WorkerCategory
    + GetAllWorkerCategories() : List<WorkerCategory>
    + InsertAuditRecord(record: AuditRecord) : void
  }

  class "PersistenceGateway\n(CLS-007)" as CLS007 {
    - _dbContext : PortalDbContext
    + GetClockingsByEmployee(empId: string, range: DateRange) : List<ClockingRecord>
    + GetAllClockingsForMonth(range: DateRange) : List<ClockingRecord>
    + InsertClocking(record: ClockingRecord) : ClockingRecord
    + FindByIdempotencyKey(key: string) : ClockingRecord?
    + SaveNewsItem(item: NewsItem) : NewsItem
    + GetNewsItem(id: Guid) : NewsItem?
    + UpdateNewsItem(id: Guid, title: string, body: string, category: NewsCategory) : NewsItem
    + UpdateNewsStatus(id: Guid, status: NewsStatus) : NewsItem
    + GetPublishedNews(category: NewsCategory?) : List<NewsItem>
    + GetFeaturedNews() : List<NewsItem>
    + GetAllNewsItems() : List<NewsItem>
    + UpsertWorkerCategory(adUserId: string, category: string) : WorkerCategory
    + GetAllWorkerCategories() : List<WorkerCategory>
    + InsertAuditRecord(record: AuditRecord) : void
  }

  class "PortalDbContext\n(CLS-008)" as CLS008 {
    + DbSet<ClockingRecord> Clockings
    + DbSet<NewsItem> NewsItems
    + DbSet<WorkerCategory> WorkerCategories
    + DbSet<AuditRecord> AuditRecords
    + OnModelCreating(modelBuilder) : void
    + SaveChanges() : int
    + SaveChangesAsync() : Task<int>
  }

  class "LdapSettings\n(CLS-009)" as CLS009 {
    + Host : string
    + Port : int
    + BindDn : string
    + BindPassword : string
    + SearchBase : string
  }

  class "LdapConnectionPool\n(CLS-010)" as CLS010 {
    - _settings : LdapSettings
    + Acquire() : ILdapConnection
    + Release(conn: ILdapConnection) : void
  }
}

INT006 <|.. CLS006
INT007 <|.. CLS007
CLS007 --> CLS008 : _dbContext
CLS006 --> CLS009 : _settings
CLS006 --> CLS010 : _pool

note right of INT007
  C2 UPDATE: Method names aligned
  with implementation:
  SaveNewsItem, UpdateNewsItem,
  UpdateNewsStatus, GetNewsItem.
  ExecuteInTransactionAsync deferred
  to implementation (M2 design correct).
end note

@enduml
```

### Designer Class Diagrams — Domain (Portal.Domain)

> **Contributed by:** Designer (Analysis & Design Discipline)
> **Iteration:** Construction C2 — entity attributes and enum values aligned with implementation

```plantuml
@startuml
title Portal Cuba Corp — Portal.Domain Package (Construction C2 — Aligned with Implementation)

skinparam classAttributeIconSize 0

package "Portal.Domain (Domain Layer)" {

  enum "ClockType\n(CLS-011)" as CLS011 {
    In
    Out
  }

  enum "ClockStatus\n(CLS-012)" as CLS012 {
    ClockedIn
    ClockedOut
  }

  enum "NewsCategory\n(CLS-013)" as CLS013 {
    General
    HR
    IT
    Events
  }

  enum "NewsStatus\n(CLS-014)" as CLS014 {
    Published
    Unpublished
  }

  enum "AuditAction\n(CLS-015)" as CLS015 {
    Publish
    Edit
    Unpublish
    CategoryChanged
  }

  class "ClockingRecord\n(CLS-016)" as CLS016 {
    + Id : long
    + EmployeeId : string
    + Timestamp : DateTime
    + Type : ClockType
    + IdempotencyKey : string
  }

  class "NewsItem\n(CLS-017)" as CLS017 {
    + Id : Guid
    + Title : string
    + Body : string
    + Category : NewsCategory
    + Status : NewsStatus
    + IsFeatured : bool
    + CreatedAt : DateTime
    + UpdatedAt : DateTime
    + AuthorId : string
  }

  class "WorkerCategory\n(CLS-018)" as CLS018 {
    + AdUserId : string
    + Category : string
  }

  class "AuditRecord\n(CLS-019)" as CLS019 {
    + Id : long
    + EntityType : string
    + EntityId : string
    + Action : AuditAction
    + Author : string
    + Timestamp : DateTime
  }

  class "DirectoryEntry\n(CLS-020)" as CLS020 {
    + AdUserId : string
    + DisplayName : string
    + JobTitle : string
    + Department : string
    + Office : string
    + Email : string
    + Extension : string
    + {static} FromLdapAttributes(adUserId, displayName, jobTitle, department, office, email, extension) : DirectoryEntry
  }

  class "DateRange\n(CLS-021)" as CLS021 {
    + Start : DateTime
    + End : DateTime
    + {static} ForMonth(year: int, month: int) : DateRange
  }

  class "ClockingResult\n(CLS-022)" as CLS022 {
    + Record : ClockingRecord?
    + IsDuplicate : bool
    + IsSuccess : bool
    + ErrorMessage : string?
    + {static} Ok(record: ClockingRecord) : ClockingResult
    + {static} Duplicate(record: ClockingRecord) : ClockingResult
    + {static} Fail(message: string) : ClockingResult
  }

  class "LdapSearchResult\n(CLS-023)" as CLS023 {
    + AdUserId : string
    + DisplayName : string?
    + JobTitle : string?
    + Department : string?
    + Office : string?
    + Email : string?
    + Extension : string?
  }
}

CLS016 --> CLS011 : Type
CLS017 --> CLS013 : Category
CLS017 --> CLS014 : Status
CLS019 --> CLS015 : Action
CLS022 --> CLS016 : Record

note right of CLS014
  C2 UPDATE: Draft state removed.
  Implementation creates NewsItem
  directly as Published (UC-005).
  Only 2 states: Published, Unpublished.
end note

note right of CLS015
  C2 UPDATE: Enum values aligned
  with implementation: Publish, Edit,
  Unpublish, CategoryChanged.
end note

note right of CLS017
  C2 UPDATE: CreatedBy → AuthorId.
  No UpdatedBy field — audit trail
  captures editor identity via
  LogAudit author parameter.
  IsFeatured present (FR-008, CR-010).
end note

note right of CLS020
  R001: FromLdapAttributes defaults
  missing values to "N/A".
  Corporate data only (CON-012).
end note

@enduml
```

### Subsystem Interface Dependency Diagram

```plantuml
@startuml
title Portal Cuba Corp — Subsystem Interface Dependencies (Construction C2)

skinparam componentStyle rectangle
skinparam classAttributeIconSize 0

package "Portal.UI (Presentation)" {
  [Clocking UI] as UI_CLK
  [News UI] as UI_NEWS
  [Directory UI] as UI_DIR
  [Category UI] as UI_CAT
}

package "Portal.Services (Application)" {
  component "ClockingService\n(CLS-001)" as SVC_CLK
  component "NewsService\n(CLS-002)" as SVC_NEWS
  component "DirectoryService\n(CLS-003)" as SVC_DIR
  component "WorkerCategoryService\n(CLS-004)" as SVC_CAT
  component "AuditInterceptor\n(CLS-005)" as SVC_AUDIT
}

package "Portal.Infrastructure (Infrastructure)" {
  component "PersistenceGateway\n(CLS-007)" as INF_PERSIST
  component "LdapGateway\n(CLS-006)" as INF_LDAP
}

database "PostgreSQL" as PG
database "Active Directory" as AD

UI_CLK --> INT001 : IClockingService
UI_NEWS --> INT002 : INewsService
UI_DIR --> INT003 : IDirectoryService
UI_CAT --> INT004 : IWorkerCategoryService

SVC_CLK --> INT007 : IPersistence
SVC_NEWS --> INT007 : IPersistence
SVC_NEWS --> INT005 : IAuditLogger
SVC_DIR --> INT006 : ILdapGateway
SVC_CAT --> INT007 : IPersistence
SVC_CAT --> INT006 : ILdapGateway
SVC_CAT --> INT005 : IAuditLogger
SVC_AUDIT --> INT007 : IPersistence

INF_PERSIST --> PG : EF Core + Npgsql
INF_LDAP --> AD : Novell.Directory.Ldap

note right of SVC_NEWS
  All service dependencies are
  interface-based (DI-injected).
  No concrete class referenced
  across subsystem boundaries.
end note

@enduml
```
## Interface Contracts
All subsystem boundaries are defined by interfaces. No concrete class is referenced across a subsystem boundary — services depend on interfaces, not implementations.

### Construction C2 — Interface-Implementation Alignment

The interface contracts below are aligned with the actual implementation source code. Method names, parameter types, and return types match the implemented interfaces in `src/PortalCubaCorp.Application/` and `src/PortalCubaCorp.Infrastructure/`. Where the implementation diverges from the prior design (C1), the design is updated to match the valid implementation choice. Where the implementation is wrong (missing `isFeatured`, missing `ExecuteInTransactionAsync`), the design retains the correct contract and the implementation must be fixed.

| Finding | Root Cause | Resolution |
|---|---|---|
| M1 — IAuditLogger (INT-005) | Design Model specified `Log()`; implementation uses `LogAudit()` | `Log()` collides with `Microsoft.Extensions.Logging.ILogger.Log()` in .NET 10. `LogAudit()` is the correct idiom. Design Model updated to `LogAudit`. **Resolved C1.** |
| M2 — IPersistence (INT-007) | Design Model specified `BeginTransaction()` / `CommitTransaction()`; implementation does not expose them | EF Core `DbContext.Database.BeginTransaction()` already provides transaction management. Re-exposing via `IPersistence` is redundant. Replaced with `ExecuteInTransactionAsync(Func<Task> action)` — callback pattern. **Design correct; implementation pending.** |
| C2-1 — INT-002 method names | Design specified `PublishNews`, `EditNews`, etc.; implementation uses `Publish`, `Edit`, etc. | Implementation uses concise .NET-idiomatic names. Design updated to match. **Resolved C2.** |
| C2-2 — INT-005 entityId type | Design specified `Guid`; implementation uses `string` | `string` accommodates both `Guid.ToString()` (news) and `adUserId` (worker categories). Design updated. **Resolved C2.** |
| C2-3 — INT-001 method name | Design specified `GetAllClockingsForMonth`; implementation uses `GetAllClockings` | Shorter name in service interface. IPersistence retains `GetAllClockingsForMonth`. Design updated. **Resolved C2.** |

```plantuml
@startuml
title Portal Cuba Corp — Interface Contracts (Construction C2 — Aligned with Implementation)

skinparam classAttributeIconSize 0

interface "IAuditLogger\n(INT-005)" as INT005 {
  + LogAudit(entityType: string, entityId: string, action: AuditAction, author: string, timestamp: DateTime) : void
}

interface "IPersistence\n(INT-007)" as INT007 {
  + GetClockingsByEmployee(empId: string, range: DateRange) : List<ClockingRecord>
  + GetAllClockingsForMonth(range: DateRange) : List<ClockingRecord>
  + InsertClocking(record: ClockingRecord) : ClockingRecord
  + FindByIdempotencyKey(key: string) : ClockingRecord?
  + SaveNewsItem(item: NewsItem) : NewsItem
  + GetNewsItem(id: Guid) : NewsItem?
  + UpdateNewsItem(id: Guid, title: string, body: string, category: NewsCategory) : NewsItem
  + UpdateNewsStatus(id: Guid, status: NewsStatus) : NewsItem
  + GetPublishedNews(category: NewsCategory?) : List<NewsItem>
  + GetFeaturedNews() : List<NewsItem>
  + GetAllNewsItems() : List<NewsItem>
  + UpsertWorkerCategory(adUserId: string, category: string) : WorkerCategory
  + GetAllWorkerCategories() : List<WorkerCategory>
  + InsertAuditRecord(record: AuditRecord) : void
}

interface "ILdapGateway\n(INT-006)" as INT006 {
  + SearchEntries(filter: string) : List<LdapSearchResult>
}

note right of INT005
  C2 UPDATE: entityId type changed
  from Guid to string to support both
  Guid (news) and string (adUserId)
  entity identifiers.
  Method: LogAudit (M1 fix applied C1).
end note

note right of INT007
  C2 UPDATE: Method names aligned
  with implementation:
  - SaveNewsItem (was InsertNewsItem)
  - UpdateNewsItem (was UpdateNews)
  - UpdateNewsStatus (was SetNewsStatus)
  - GetNewsItem (was GetNewsById)
  ExecuteInTransactionAsync deferred
  to C2 implementation (M2 design
  correct, implementation pending).
end note

note right of INT006
  Single method: SearchEntries.
  Returns List<LdapSearchResult>.
  R001: missing attributes → null
  in LdapSearchResult, mapped to
  "N/A" by DirectoryEntry.FromLdapAttributes.
end note

@enduml
```

### Interface Operation Specifications

#### INT-001: IClockingService

| Operation | Signature | Precondition | Postcondition |
|---|---|---|---|
| RecordClocking | `ClockingResult RecordClocking(string employeeId, DateTime timestamp, ClockType type, string idempotencyKey)` | employeeId non-empty; idempotencyKey non-empty | Returns `Ok` with new record, `Duplicate` if key exists, or `Fail` with error message |
| GetCurrentStatus | `ClockStatus GetCurrentStatus(string employeeId)` | — | Returns `ClockedIn` if most recent record is `ClockType.In`, else `ClockedOut` |
| GetHistory | `List<ClockingRecord> GetHistory(string employeeId, DateRange month)` | — | Returns clockings for employee within date range, ordered by timestamp DESC |
| GetAllClockings | `List<ClockingRecord> GetAllClockings(DateRange month)` | HR role | Returns all clockings within date range |
| ExportCsv | `Stream ExportCsv(DateRange month)` | HR role | Returns CSV stream with header `Employee,Date,TimeIn,TimeOut,Direction` |

#### INT-002: INewsService

| Operation | Signature | Precondition | Postcondition |
|---|---|---|---|
| Publish | `NewsItem Publish(string title, string body, NewsCategory category, string authorId, bool isFeatured)` | title non-empty; body non-empty | Creates NewsItem with Status=Published, IsFeatured set; audit record inserted |
| Edit | `NewsItem Edit(Guid id, string title, string body, NewsCategory category, string authorId, bool isFeatured)` | title non-empty; body non-empty; item exists | Updates NewsItem fields; UpdatedAt set; audit record inserted |
| Unpublish | `NewsItem Unpublish(Guid id, string authorId)` | item exists | Sets Status=Unpublished; record preserved (CON-013); audit record inserted |
| GetById | `NewsItem? GetById(Guid id)` | — | Returns NewsItem or null |
| GetPublishedNews | `List<NewsItem> GetPublishedNews(NewsCategory? category)` | — | Returns Status=Published items, optionally filtered by category |
| GetFeaturedNews | `List<NewsItem> GetFeaturedNews()` | — | Returns Status=Published AND IsFeatured=true |
| ListAll | `List<NewsItem> ListAll()` | HR role | Returns all news items regardless of status |

#### INT-003: IDirectoryService

| Operation | Signature | Precondition | Postcondition |
|---|---|---|---|
| Search | `List<DirectoryEntry> Search(string query)` | query non-empty | Returns DirectoryEntry list from AD via LDAP; missing attributes → "N/A" (R001) |

#### INT-004: IWorkerCategoryService

| Operation | Signature | Precondition | Postcondition |
|---|---|---|---|
| AssignCategory | `WorkerCategory AssignCategory(string adUserId, string category, string authorId)` | adUserId non-empty; category non-empty | Upserts worker_categories row; audit record inserted |
| ListCategories | `List<WorkerCategory> ListCategories()` | — | Returns all worker category records |
| LookupAdUser | `List<DirectoryEntry> LookupAdUser(string query)` | query non-empty | Returns DirectoryEntry list from AD via LDAP |

#### INT-005: IAuditLogger

| Operation | Signature | Precondition | Postcondition |
|---|---|---|---|
| LogAudit | `void LogAudit(string entityType, string entityId, AuditAction action, string author, DateTime timestamp)` | — | Appends audit record (never updated or deleted) |

#### INT-006: ILdapGateway

| Operation | Signature | Precondition | Postcondition |
|---|---|---|---|
| SearchEntries | `List<LdapSearchResult> SearchEntries(string filter)` | filter is valid LDAP filter | Returns matching entries from AD; missing attributes are null |

#### INT-007: IPersistence

| Operation | Signature | Precondition | Postcondition |
|---|---|---|---|
| GetClockingsByEmployee | `List<ClockingRecord> GetClockingsByEmployee(string empId, DateRange range)` | — | Returns clockings for employee within range |
| GetAllClockingsForMonth | `List<ClockingRecord> GetAllClockingsForMonth(DateRange range)` | — | Returns all clockings within range |
| InsertClocking | `ClockingRecord InsertClocking(ClockingRecord record)` | idempotencyKey unique | Inserts and returns record with generated Id |
| FindByIdempotencyKey | `ClockingRecord? FindByIdempotencyKey(string key)` | — | Returns existing record or null |
| SaveNewsItem | `NewsItem SaveNewsItem(NewsItem item)` | — | Inserts and returns item with generated Id |
| GetNewsItem | `NewsItem? GetNewsItem(Guid id)` | — | Returns item or null |
| UpdateNewsItem | `NewsItem UpdateNewsItem(Guid id, string title, string body, NewsCategory category)` | item exists | Updates title, body, category; returns updated item |
| UpdateNewsStatus | `NewsItem UpdateNewsStatus(Guid id, NewsStatus status)` | item exists | Updates status; returns updated item |
| GetPublishedNews | `List<NewsItem> GetPublishedNews(NewsCategory? category)` | — | Returns Status=Published items, optionally filtered |
| GetFeaturedNews | `List<NewsItem> GetFeaturedNews()` | — | Returns Status=Published AND IsFeatured=true |
| GetAllNewsItems | `List<NewsItem> GetAllNewsItems()` | — | Returns all news items regardless of status |
| UpsertWorkerCategory | `WorkerCategory UpsertWorkerCategory(string adUserId, string category)` | adUserId non-empty | Inserts or updates worker_categories row (2 columns only per CON-009) |
| GetAllWorkerCategories | `List<WorkerCategory> GetAllWorkerCategories()` | — | Returns all worker category records |
| InsertAuditRecord | `void InsertAuditRecord(AuditRecord record)` | Called within active transaction | Appends audit record (never updated or deleted) |
## Persistent Data Classes
> **Contributed by:** Database Designer (Analysis & Design Discipline)
> **Persistence Engine:** PostgreSQL (CON-003 — declared by stakeholder)
> **ORM:** EF Core 10 + Npgsql (CON-001, CON-003)
> **Design Mechanism:** Repository + Unit of Work via PortalDbContext (SAD Logical View)
> **Iteration 2:** Aligned with M1/M2 resolution — `IAuditLogger.LogAudit()` and `IPersistence.ExecuteInTransactionAsync()` reflected in mechanism chain.

EF Core entity classes map to PostgreSQL tables. The PortalDbContext configures all mappings, constraints, and indexes in OnModelCreating. The schema is minimal — 4 tables — because employee data is read from AD at read time (CON-009) and authentication is handled by Keycloak (CON-004). No local user table exists.

### Schema Organization

```plantuml
@startuml
title Portal Cuba Corp — Schema Organization

skinparam packageStyle rectangle
skinparam linetype ortho

package "PostgreSQL Database\n(CON-003)" as DB {

  package "portal_schema" {
    rectangle "clockings\n(T1)" as T1
    rectangle "news_items\n(T2)" as T2
    rectangle "worker_categories\n(T3)" as T3
    rectangle "audit_records\n(T4)" as T4
  }
}

package "External Systems (Read-Only)" {
  rectangle "Active Directory\n(LDAP — CON-005)" as AD
  rectangle "Keycloak\n(OIDC — CON-004)" as KC
}

T4 ..> T2 : entity_id (logical FK)
T4 ..> T3 : entity_id (logical FK)
T1 ..> KC : employee_id from OIDC token
T2 ..> KC : created_by from OIDC token
T3 ..> KC : updated_by from OIDC token
T4 ..> KC : author from OIDC token
T3 ..> AD : ad_user_id references AD user

note bottom of DB
  4 tables total — minimal schema.
  No employee data stored locally (CON-009).
  All identity fields reference OIDC
  token subject claim, not a local user table.
end note

@enduml
```

### Entity-to-Table Mapping (<<table>> Class Diagram)

```plantuml
@startuml
title Portal Cuba Corp — Persistent Data Classes (PostgreSQL, Iteration 2)

skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Portal Schema" {

  class "clockings" as T1 <<table>> {
    + id : uuid <<PK>>
    --
    employee_id : varchar(255) NOT NULL
    clock_timestamp : timestamptz NOT NULL
    clock_type : varchar(10) NOT NULL <<CHECK: IN ('IN','OUT')>>
    idempotency_key : varchar(100) NULL <<UNIQUE>>
  }

  class "news_items" as T2 <<table>> {
    + id : uuid <<PK>>
    --
    title : varchar(200) NOT NULL
    body : text NOT NULL
    category : varchar(20) NOT NULL <<CHECK: IN ('General','HR','IT','Events')>>
    status : varchar(20) NOT NULL <<CHECK: IN ('Published','Unpublished')>>
    is_featured : boolean NOT NULL DEFAULT false
    created_by : varchar(255) NOT NULL
    created_at : timestamptz NOT NULL
    updated_at : timestamptz NOT NULL
  }

  class "worker_categories" as T3 <<table>> {
    + ad_user_id : varchar(255) <<PK>>
    --
    category : varchar(100) NOT NULL
    updated_by : varchar(255) NOT NULL
    updated_at : timestamptz NOT NULL
  }

  class "audit_records" as T4 <<table>> {
    + id : uuid <<PK>>
    --
    entity_type : varchar(50) NOT NULL
    entity_id : uuid NOT NULL
    action : varchar(20) NOT NULL <<CHECK: IN ('PUBLISH','EDIT','UNPUBLISH','CATEGORY_CHANGE')>>
    author : varchar(255) NOT NULL
    timestamp : timestamptz NOT NULL
  }
}

T4 ..> T2 : entity_id (logical FK)
T4 ..> T3 : entity_id (logical FK)

note right of T1
  Idempotency key is NULL for
  normal clockings; present only
  for offline-retry submissions (AC-005).
  UNIQUE index prevents duplicate
  inserts on retry.
end note

note right of T4
  Append-only table — no UPDATE
  or DELETE ever (NFR-004, CON-013).
  author = OIDC token subject claim.
  timestamp = server clock.
  Inserted within same transaction
  via ExecuteInTransactionAsync.
end note

note bottom of T3
  2 business columns only (CON-009):
  ad_user_id + category.
  updated_by/updated_at are audit
  columns required by NFR-004.
  ad_user_id references AD user
  but no FK constraint (external system).
end note

@enduml
```

### O/R Mapping Specification

| Design Class | Table | Identity Strategy | Loading | Notes |
|---|---|---|---|---|
| CLS-016 (ClockingRecord) | T1 (clockings) | UUID surrogate PK (`id`); `idempotency_key` nullable with UNIQUE index for offline retry (AC-005) | Eager — always loaded with all columns | `clock_type` mapped to CLS-011 (ClockType enum); `employee_id` from OIDC token subject, no FK (no local user table) |
| CLS-017 (NewsItem) | T2 (news_items) | UUID surrogate PK (`id`) | Eager for single-item views; paged query for list views | `category` mapped to CLS-013 (NewsCategory enum); `status` mapped to CLS-014 (NewsStatus enum); `is_featured` boolean for featured banner (FR-008); `created_by` from OIDC token subject |
| CLS-018 (WorkerCategory) | T3 (worker_categories) | Natural PK (`ad_user_id`) — no surrogate key per CON-009 ("two columns and nothing else") | Eager — small table (<200 rows) | `ad_user_id` is both PK and the link to AD; no FK constraint (external system); `updated_by` from OIDC token subject; audit columns (updated_by, updated_at) required by NFR-004 but are NOT the 2 business columns CON-009 limits — they are metadata columns |
| CLS-019 (AuditRecord) | T4 (audit_records) | UUID surrogate PK (`id`) | Eager — always loaded with all columns | Append-only: no UPDATE or DELETE ever (NFR-004, CON-013); `action` mapped to CLS-015 (AuditAction enum); `entity_id` is a logical FK to T2 or T3 (no physical FK — polymorphic reference); `author` from OIDC token subject; inserted via `IAuditLogger.LogAudit()` within `IPersistence.ExecuteInTransactionAsync()` callback — same transaction as the audited operation |
| CLS-020 (DirectoryEntry) | (not persisted) | N/A | N/A | Projected from AD at read time (CON-009); no table mapping |

### Index Strategy

| Index | Table | Columns | Type | Justified By |
|---|---|---|---|---|
| `ix_clockings_employee_timestamp` | T1 | (employee_id, clock_timestamp DESC) | B-tree composite | UC-002: employee views own history by month; NFR-002: <1s response — index covers the filter+sort |
| `ux_clockings_idempotency_key` | T1 | (idempotency_key) | B-tree UNIQUE | AC-005: offline retry idempotency — duplicate POST rejected by constraint, not application logic |
| `ix_clockings_timestamp` | T1 | (clock_timestamp) | B-tree | UC-003/UC-004: HR views all clockings for a month — range scan on timestamp |
| `ix_news_items_status_published_date` | T2 | (status, created_at DESC) | B-tree composite | UC-008: employees see published news sorted by date; NFR-001: <3s page load — covering index for the main page feed |
| `ix_news_items_status_featured_date` | T2 | (status, is_featured, created_at DESC) | B-tree composite | UC-008: featured news banner — filter Published + IsFeatured, sorted by date |
| `ix_news_items_category_status_date` | T2 | (category, status, created_at DESC) | B-tree composite | UC-008: filter by category — General/HR/IT/Events, Published only, sorted by date |
| `ix_audit_records_entity` | T4 | (entity_type, entity_id) | B-tree composite | NFR-004: audit trail lookup by entity — who published/edited/unpublished a specific news item or category |
| `ix_audit_records_author` | T4 | (author, timestamp DESC) | B-tree | NFR-004: audit trail by author — all actions by a specific HR user |

### Constraint Specification

| Table | Constraint | Type | Details |
|---|---|---|---|
| T1 (clockings) | `clock_type IN ('IN','OUT')` | CHECK | CLS-011 enum values only |
| T1 (clockings) | `idempotency_key` UNIQUE | UNIQUE | Nullable; NULL values not constrained (PostgreSQL NULL semantics) |
| T1 (clockings) | `employee_id` NOT NULL | NOT NULL | Always from OIDC token — no anonymous clockings |
| T1 (clockings) | `clock_timestamp` NOT NULL | NOT NULL | Server clock for normal; client timestamp accepted for offline retry (AC-005) |
| T2 (news_items) | `category IN ('General','HR','IT','Events')` | CHECK | CLS-013 enum values only |
| T2 (news_items) | `status IN ('Published','Unpublished')` | CHECK | CLS-014 enum values; no 'Deleted' — CON-013 forbids hard delete |
| T2 (news_items) | `title` NOT NULL, `body` NOT NULL | NOT NULL | News must have content |
| T2 (news_items) | `created_by` NOT NULL | NOT NULL | Always from OIDC token — audit (NFR-004) |
| T3 (worker_categories) | `ad_user_id` PRIMARY KEY | PK | Natural key — CON-009: "two columns and nothing else" (business columns) |
| T3 (worker_categories) | `category` NOT NULL | NOT NULL | Must have a category value |
| T3 (worker_categories) | `updated_by` NOT NULL | NOT NULL | Audit column (NFR-004) |
| T4 (audit_records) | `action IN ('PUBLISH','EDIT','UNPUBLISH','CATEGORY_CHANGE')` | CHECK | CLS-015 enum values only |
| T4 (audit_records) | All columns NOT NULL | NOT NULL | Audit record must be complete — no nullable fields |
| T4 (audit_records) | No UPDATE, no DELETE | Application-enforced | Append-only invariant (NFR-004, CON-013); enforced by EF Core interceptor + repository pattern — no Update/Delete methods exposed for T4 |

### Normalization Assessment

All 4 tables are in 3NF:
- **T1 (clockings):** Every non-key attribute depends only on the PK (`id`). No transitive dependencies. `employee_id` is not a FK to a local table (no local user table — CON-009), so no transitive dependency through a user entity.
- **T2 (news_items):** Every non-key attribute depends only on the PK (`id`). `created_by` is an OIDC subject string, not a FK to a local user table. No transitive dependencies.
- **T3 (worker_categories):** PK is `ad_user_id` (natural key). `category` depends only on the PK. No transitive dependencies. The 2 business columns (ad_user_id, category) satisfy CON-009 exactly; updated_by/updated_at are audit metadata required by NFR-004.
- **T4 (audit_records):** Every non-key attribute depends only on the PK (`id`). `entity_id` is a polymorphic logical reference (no physical FK), so no transitive dependency through a target table.

No denormalization is applied — the schema is small and normalized. At 200 employees and <100K rows per table, query performance is met by B-tree indexes alone without any denormalization trade-offs.

### Three-Level Mechanism Chain Resolution (Iteration 2 — M1/M2 Aligned)

| Analysis Mechanism | Design Mechanism | Implementation Mechanism |
|---|---|---|
| Persistence (objects need to be stored between sessions) | Repository + Unit of Work; 3NF normalized relational schema; append-only audit; idempotency via unique index; transaction boundary via `IPersistence.ExecuteInTransactionAsync(Func<Task> action)` callback pattern (M2 resolution) | EF Core 10 + Npgsql + PostgreSQL (CON-001, CON-003) |
| Audit Trail (who did what, when) | Interceptor pattern — same transaction as the audited operation via `ExecuteInTransactionAsync`; append-only table; `IAuditLogger.LogAudit(entityType, entityId, action, author, timestamp)` called within the transaction (M1 resolution — `LogAudit` avoids .NET `ILogger.Log()` collision); author from OIDC token subject claim; timestamp from server clock | EF Core SaveInterceptor + PostgreSQL audit_records table (CON-003) |
| Offline Retry (AC-005 — 5-min network drop tolerance) | Idempotency key on clockings table; UNIQUE index prevents duplicate inserts; server accepts client timestamp; `IPersistence.FindByIdempotencyKey(key)` checks before `InsertClocking` within `ExecuteInTransactionAsync` | clocking-retry.js (CON-002) + PostgreSQL UNIQUE constraint (CON-003) |

### Migration Strategy (Baseline)

The baseline migration creates all 4 tables with constraints and indexes in a single forward migration. This is the initial schema for the portal — no prior schema exists to migrate from.

**Migration: 0001_InitialSchema**

| Step | Action | Rollback |
|---|---|---|
| 1 | CREATE TABLE `clockings` with PK, CHECK, NOT NULL constraints | DROP TABLE `clockings` |
| 2 | CREATE UNIQUE INDEX `ux_clockings_idempotency_key` ON `clockings(idempotency_key)` | DROP INDEX `ux_clockings_idempotency_key` |
| 3 | CREATE INDEX `ix_clockings_employee_timestamp` ON `clockings(employee_id, clock_timestamp DESC)` | DROP INDEX `ix_clockings_employee_timestamp` |
| 4 | CREATE INDEX `ix_clockings_timestamp` ON `clockings(clock_timestamp)` | DROP INDEX `ix_clockings_timestamp` |
| 5 | CREATE TABLE `news_items` with PK, CHECK, NOT NULL constraints | DROP TABLE `news_items` |
| 6 | CREATE INDEX `ix_news_items_status_published_date` ON `news_items(status, created_at DESC)` | DROP INDEX |
| 7 | CREATE INDEX `ix_news_items_status_featured_date` ON `news_items(status, is_featured, created_at DESC)` | DROP INDEX |
| 8 | CREATE INDEX `ix_news_items_category_status_date` ON `news_items(category, status, created_at DESC)` | DROP INDEX |
| 9 | CREATE TABLE `worker_categories` with PK (natural: ad_user_id), NOT NULL constraints | DROP TABLE `worker_categories` |
| 10 | CREATE TABLE `audit_records` with PK, CHECK, NOT NULL constraints | DROP TABLE `audit_records` |
| 11 | CREATE INDEX `ix_audit_records_entity` ON `audit_records(entity_type, entity_id)` | DROP INDEX |
| 12 | CREATE INDEX `ix_audit_records_author` ON `audit_records(author, timestamp DESC)` | DROP INDEX |

**Idempotency:** The migration uses `CREATE TABLE IF NOT EXISTS` and `CREATE INDEX IF NOT EXISTS` for idempotent re-runs. EF Core migration framework tracks applied migrations in `__EFMigrationsHistory`.

**Schema stability:** This is the baseline schema for Elaboration. The 4 core tables (clockings, news_items, worker_categories, audit_records) with their PKs, logical FKs, and key indexes are STABLE — Construction iterations may add tables but should not restructure these. No schema evolution is anticipated within Elaboration.
## Boundary Classes and Navigation Map
> **Contributed by:** User-Interface Designer (Analysis & Design Discipline)
> **Purpose:** This section contains the interaction flows (activity diagrams with user/system swimlanes per UC), the Navigation Topology (state machine of all screens), Salt wireframes for primary screens, and UI Patterns. These are the user-interface realizations of all use cases — the direct translation of user goals into observable, navigable screen flows. CON-011: the custom design at `docs/inputs/employee-portal-design.html` is MANDATORY and authoritative for the UI visual layer.

### Navigation Topology

The following state machine defines ALL screens in the system, their relationships, and the conditions under which transitions fire. Every screen is a node; every user action causing a screen change is a directed edge with a guard condition. This model can be validated for: unreachable screens, dead-end screens, missing error states, and circular navigation traps.

**Validation results:** All 19 screens are reachable from the initial state. No dead-end screens without explicit intent (Error and Timeout both have exit transitions). No circular navigation traps (all back-navigation paths return to a hub screen). Session timeout and error states are explicit terminal handlers.

```plantuml
@startuml
title Portal Cuba Corp — Navigation Topology (State Machine)

state "Login Redirect\n(Keycloak OIDC)" as LOGIN
state "Main Page\n(Employee)" as MAIN_EMP
state "Main Page\n(HR Dashboard)" as MAIN_HR
state "My Clockings\nPage" as MY_CLOCK
state "Clock Confirmation\n(inline)" as CLOCK_CONF
state "Clock Error\n(inline)" as CLOCK_ERR
state "All Clockings\n(HR)" as ALL_CLOCK
state "Export CSV\n(download)" as EXPORT
state "Publish News\nForm (HR)" as PUB_FORM
state "Edit News\nForm (HR)" as EDIT_FORM
state "News Management\nList (HR)" as NEWS_MGMT
state "Unpublish\nConfirm Dialog" as UNPUB_DLG
state "Worker Categories\n(HR)" as CAT_MGMT
state "Employee Directory\nSearch" as DIR_SEARCH
state "Directory Results" as DIR_RESULTS
state "News Feed\n(main page)" as NEWS_FEED
state "News Detail\n(expanded)" as NEWS_DETAIL
state "Session Timeout\nDialog" as TIMEOUT
state "Error Page\n(generic)" as ERROR

[*] --> LOGIN : navigate to portal
LOGIN --> MAIN_EMP : OIDC token valid\n[role = Employee]
LOGIN --> MAIN_HR : OIDC token valid\n[role = HR]
LOGIN --> ERROR : OIDC auth failed

MAIN_EMP --> MY_CLOCK : click "My Clockings"
MY_CLOCK --> MAIN_EMP : click "Back"

MAIN_EMP --> CLOCK_CONF : press Clock In/Out\n[network OK]
MAIN_EMP --> CLOCK_ERR : press Clock In/Out\n[network down > 5 min]
CLOCK_CONF --> MAIN_EMP : confirmation displayed
CLOCK_ERR --> MAIN_EMP : error dismissed

MAIN_EMP --> NEWS_FEED : scroll to news section
NEWS_FEED --> NEWS_DETAIL : click news item
NEWS_DETAIL --> NEWS_FEED : click "Collapse"
NEWS_FEED --> MAIN_EMP : navigate back

MAIN_EMP --> DIR_SEARCH : click "Directory"
DIR_SEARCH --> DIR_RESULTS : click "Search" [results found]
DIR_SEARCH --> DIR_SEARCH : click "Search" [no results]
DIR_RESULTS --> DIR_SEARCH : click "New Search"
DIR_RESULTS --> MAIN_EMP : click "Back"
DIR_SEARCH --> MAIN_EMP : click "Back"

MAIN_HR --> ALL_CLOCK : click "All Clockings"
ALL_CLOCK --> EXPORT : click "Export CSV"
EXPORT --> ALL_CLOCK : download complete
ALL_CLOCK --> MAIN_HR : click "Back"

MAIN_HR --> NEWS_MGMT : click "Manage News"
NEWS_MGMT --> PUB_FORM : click "Publish New"
PUB_FORM --> NEWS_MGMT : publish success
NEWS_MGMT --> EDIT_FORM : click [Edit]
EDIT_FORM --> NEWS_MGMT : save success
NEWS_MGMT --> UNPUB_DLG : click [Unpublish]
UNPUB_DLG --> NEWS_MGMT : confirm unpublish
UNPUB_DLG --> NEWS_MGMT : cancel
NEWS_MGMT --> MAIN_HR : click "Back"

MAIN_HR --> CAT_MGMT : click "Worker Categories"
CAT_MGMT --> MAIN_HR : click "Back"

MAIN_EMP --> TIMEOUT : session expired
MAIN_HR --> TIMEOUT : session expired
TIMEOUT --> LOGIN : click "Re-login"

MAIN_EMP --> ERROR : unhandled exception
MAIN_HR --> ERROR : unhandled exception
ERROR --> LOGIN : click "Return to Portal"

@enduml
```

### Interaction Flows (Activity Diagrams with Swimlanes)

The following activity diagrams realize the user-interface interaction for each UC of UI significance. Each diagram uses user and system swimlanes to make the interaction sequence explicit and traceable to use-case flow steps.

#### UC-001: Clock In / Clock Out — Interaction Flow

**Traces to:** UC-001 Main Flow steps 1–9, Alternative Flows A1 (offline retry), A2 (timeout), A3 (idempotency)
**Usability criteria:** USA-005 (clock without HR help), USA-004 (no prior training), NFR-002 (<1s response)

```plantuml
@startuml
title UC-001: Clock In/Out — UI Interaction Flow

|Employee|
start
:Opens portal main page;
|System|
:Retrieves clocking status\nfrom database\n(employee id from OIDC token);
:Displays "Clock In" or "Clock Out"\nbutton based on current status;
|Employee|
:Presses Clock In/Out button;
|System (Client)|
:Records press timestamp +\ngenerates idempotency key\nin localStorage;
:Sends POST request with\ntimestamp + idempotency key;
|System (Server)|
:Validates request;
if (Network available?) then (yes)
  :Records clocking entry\nin PostgreSQL;
  :Returns confirmation\nwith recorded time;
  |System (Client)|
  :Displays confirmation on screen;
else (no — A1: offline retry)
  :Stores press in localStorage;
  :Retries POST for up to 5 minutes;
  if (Network restored within 5 min?) then (yes)
    |System (Server)|
    :Accepts original client timestamp;
    :Rejects duplicates\nby idempotency key;
    :Returns confirmation;
    |System (Client)|
    :Displays confirmation;
  else (no — A2)
    |System (Client)|
    :Stops retrying;
    :Displays "Clocking not recorded\n— report to HR";
  endif
endif
|Employee|
:Sees confirmation or error message;
stop
@enduml
```

#### UC-005: Publish News — Interaction Flow

**Traces to:** UC-005 Main Flow steps 1–7
**Usability criteria:** USA-006 (publish without technical assistance), AC-002

```plantuml
@startuml
title UC-005: Publish News — UI Interaction Flow

|HR Administrator|
start
:Navigates to "Publish News" form;
|System|
:Verifies HR role from OIDC token;
:Displays publish form\n(title, body, date, category);
|HR Administrator|
:Fills in title, body, date;
:Selects category\n(General, HR, IT, Events);
:Clicks "Publish";
|System|
:Validates form fields;
if (All fields valid?) then (yes)
  :Creates news item\nwith status = Published;
  :Creates audit record\n(author from OIDC, timestamp);
  :Persists news item + audit record;
  :Displays "News published successfully";
else (no)
  :Displays validation errors\nper invalid field;
  |HR Administrator|
  :Corrects errors and resubmits;
endif
|HR Administrator|
:Sees confirmation;
stop
@enduml
```

#### UC-007: Unpublish News — Interaction Flow

**Traces to:** UC-007 Main Flow steps 1–6
**Usability criteria:** CON-013 (never hard-delete), NFR-004 (audit trail)

```plantuml
@startuml
title UC-007: Unpublish News — UI Interaction Flow

|HR Administrator|
start
:Navigates to "News Management" list;
|System|
:Verifies HR role from OIDC token;
:Displays all news items with actions\n([Edit] [Unpublish]);
|HR Administrator|
:Clicks "Unpublish" on a news item;
|System|
:Displays confirmation dialog:\n"Unpublish this news item?\nIt will be hidden but not deleted.";
|HR Administrator|
:Clicks "Confirm";
|System|
:Sets news item status = Unpublished\n(does NOT delete record);
:Creates audit record\n(author, timestamp,\naction = Unpublished);
:Persists changes;
:Displays "News item unpublished";
|HR Administrator|
:Sees confirmation;
stop
@enduml
```

#### UC-008: Read and Filter News — Interaction Flow

**Traces to:** UC-008 Main Flow steps 1–7
**Usability criteria:** USA-001 (mandatory design), USA-004 (no prior training)

```plantuml
@startuml
title UC-008: Read and Filter News — UI Interaction Flow

|Employee|
start
:Navigates to main page;
|System|
:Retrieves published news items\nsorted by date (newest first);
:Identifies featured news items;
:Displays news feed with\nfeatured banners at top;
|Employee|
:Views news feed;
if (Wants to filter?) then (yes)
  :Selects category filter\n(General, HR, IT, Events);
  |System|
  :Filters news items\nby selected category;
  :Displays filtered results;
else (no)
  :Displays all news items;
endif
|Employee|
:Scrolls through news feed;
if (Clicks news item?) then (yes)
  |System|
  :Expands news detail (inline);
  |Employee|
  :Reads full news item;
  :Clicks "Collapse"\nto return to feed;
else (no)
  :Continues browsing;
endif
stop
@enduml
```

#### UC-009: Search Employee Directory — Interaction Flow

**Traces to:** UC-009 Main Flow steps 1–6, Alternative Flow A1 (no results)
**Usability criteria:** USA-003 (find colleague in <10s), AC-003, R001 (LDAP attribute risk)

```plantuml
@startuml
title UC-009: Search Employee Directory — UI Interaction Flow

|Employee|
start
:Navigates to "Employee Directory";
|System|
:Displays search form\n(name, department, office fields);
|Employee|
:Enters search criteria\n(name, department, or office);
:Clicks "Search";
|System|
:Queries Active Directory\nover LDAP with search criteria;
if (Results found?) then (yes)
  :Displays results list\n(name, job title, department,\noffice, email, extension);
  |Employee|
  :Views colleague information;
  if (Needs another search?) then (yes)
    :Enters new criteria;
    |System|
    :Queries AD again;
  else (no)
    :Done;
  endif
else (no results)
  :Displays "No employees found";
  |Employee|
  :Refines search criteria;
endif
stop
@enduml
```

#### UC-010: Manage Worker Category — Interaction Flow

**Traces to:** UC-010 Main Flow steps 1–8, Alternative Flows A1 (not found), A2 (invalid category)
**Usability criteria:** USA-006 (HR self-service), NFR-004 (audit trail)

```plantuml
@startuml
title UC-010: Manage Worker Category — UI Interaction Flow

|HR Administrator|
start
:Navigates to "Manage Worker Categories";
|System|
:Verifies HR role from OIDC token;
:Displays current category assignments\n(AD user id, category);
|HR Administrator|
:Searches for employee by AD user id;
|System|
:Looks up AD user id via LDAP;
if (Employee found in AD?) then (yes)
  :Displays employee name\n+ current category;
  |HR Administrator|
  :Assigns or updates category;
  :Clicks "Save";
  |System|
  :Validates category value;
  if (Category valid?) then (yes)
    :Persists worker category link\n(AD user id, category)\nin local table;
    :Creates audit record\n(author, timestamp,\naction = CategoryChanged);
    :Displays "Category updated successfully";
  else (no — A2)
    :Displays validation error;
  endif
else (no — A1)
  :Displays "Employee not found in AD";
endif
|HR Administrator|
:Sees confirmation or error;
stop
@enduml
```

#### Tabular Interaction Flows (Standard UCs)

**UC-002: View Own Clocking History** — Traces to UC-002 Main Flow steps 1–5. Usability: USA-005, USA-004.

| Step | Actor | Action | System Response |
|---|---|---|---|
| 1 | Employee | Clicks "My Clockings" from main page | Navigates to clocking history page |
| 2 | System | — | Retrieves current month's clocking records for authenticated employee |
| 3 | System | — | Displays table: date, time in, time out, direction |
| 4 | Employee | Reviews history | — |
| 5 | Employee | Clicks "Back" | Returns to main page |

**UC-003: View All Employee Clockings** — Traces to UC-003 Main Flow steps 1–6. Usability: USA-006.

| Step | Actor | Action | System Response |
|---|---|---|---|
| 1 | HR Admin | Navigates to "All Clockings" from HR dashboard | Verifies HR role from OIDC token |
| 2 | System | — | Displays filter controls (month selector, employee name search) |
| 3 | HR Admin | Selects month and/or enters employee name | — |
| 4 | System | — | Queries clockings from PostgreSQL; resolves employee name via LDAP |
| 5 | System | — | Displays results table: employee name, date, time in, time out, direction |
| 6 | HR Admin | Reviews clockings | — |

**UC-004: Export Monthly Clocking Report** — Traces to UC-004 Main Flow steps 1–5. Usability: USA-006, PERF-004.

| Step | Actor | Action | System Response |
|---|---|---|---|
| 1 | HR Admin | On "All Clockings" page, selects target month | — |
| 2 | HR Admin | Clicks "Export CSV" | — |
| 3 | System | — | Generates CSV file with all clockings for selected month |
| 4 | System | — | Triggers browser download dialog |
| 5 | HR Admin | Saves file | — |

**UC-006: Edit Published News** — Traces to UC-006 Main Flow steps 1–6. Usability: USA-006, NFR-004.

| Step | Actor | Action | System Response |
|---|---|---|---|
| 1 | HR Admin | Navigates to "News Management" list | Verifies HR role; displays all news items with [Edit] action |
| 2 | HR Admin | Clicks [Edit] on a news item | Opens edit form pre-populated with current title, body, date, category |
| 3 | HR Admin | Modifies fields and clicks "Save" | — |
| 4 | System | — | Validates form fields |
| 5 | System | — | Updates news item; creates audit record (author, timestamp, action = Edited) |
| 6 | System | — | Displays "News updated successfully"; returns to news management list |

### Wireframes (Salt)

The following Salt wireframes define the visual structure of all primary screens. CON-011: the custom design at `docs/inputs/employee-portal-design.html` is MANDATORY and authoritative for the UI visual layer — these wireframes capture the structural layout that the Implementer must follow.

#### Main Page (Employee)

```plantuml
@startsalt
title Primary Screen: Main Page (Employee)
{
  +Portal Cuba Corp - Employee Portal+
  +----------------------------------------------------------+
  |  [Logo]  Cuba Corp Portal          [Employee Name] [Logout]|
  +----------------------------------------------------------+
  |  Navigation: [Home] [My Clockings] [Directory]             |
  +----------------------------------------------------------+
  |  Clock In / Out                                            |
  |  +------------------------------------------------------+|
  |  |  Status: Not Clocked In                              ||
  |  |  [    Clock In    ]                                 ||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
  |  Featured News                                            |
  |  +------------------------------------------------------+|
  |  | [BANNER] Company Picnic — Saturday Aug 30            ||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
  |  News Feed                              [All] [General] [HR] [IT] [Events]|
  |  +------------------------------------------------------+|
  |  | HR Policy Update — HR — 08/27                        ||
  |  | Network Maintenance — IT — 08/25                     ||
  |  | Company Picnic — Events — 08/26                      ||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
}
@endsalt
```

#### Publish News (HR)

```plantuml
@startsalt
title Primary Screen: Publish News (HR)
{
  +Portal Cuba Corp - HR Dashboard+
  +----------------------------------------------------------+
  |  [Logo]  Cuba Corp Portal          [HR Admin] [Logout]    |
  +----------------------------------------------------------+
  |  Navigation: [Home] [All Clockings] [Manage News] [Categories]|
  +----------------------------------------------------------+
  |  Publish News                                              |
  |  +------------------------------------------------------+|
  |  |  Title: [____________________________________]       ||
  |  |  Category: [General v]                              ||
  |  |  Date: [08/28/2026]                                 ||
  |  |  Body:                                              ||
  |  |  +------------------------------------------------+||
  |  |  |                                                |||
  |  |  |                                                |||
  |  |  |                                                |||
  |  |  +------------------------------------------------+||
  |  |  [Publish]  [Cancel]                               ||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
}
@endsalt
```

#### Employee Directory Search

```plantuml
@startsalt
title Primary Screen: Employee Directory Search
{
  +Portal Cuba Corp - Employee Directory+
  +----------------------------------------------------------+
  |  [Logo]  Cuba Corp Portal          [Employee] [Logout]    |
  +----------------------------------------------------------+
  |  Navigation: [Home] [My Clockings] [Directory]             |
  +----------------------------------------------------------+
  |  Search Employee Directory                                 |
  |  +------------------------------------------------------+|
  |  |  Name: [____________]                                ||
  |  |  Department: [____________]                          ||
  |  |  Office: [All v]                                    ||
  |  |  [   Search   ]                                     ||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
  |  Results                                                   |
  |  +------------------------------------------------------+|
  |  |Name           | Job Title  | Dept    | Office | Email|Ext||
  |  |M. Rodriguez   | Developer  | IT      | Havana | m.r@ | 123||
  |  |C. Perez       | Accountant | Finance | Havana | c.p@ | 456||
  |  |A. Gomez       | HR Staff   | HR      | Santiago| a.g@ | 789||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
}
@endsalt
```

#### News Management (HR)

```plantuml
@startsalt
title Primary Screen: News Management (HR)
{
  +Portal Cuba Corp - HR Dashboard+
  +----------------------------------------------------------+
  |  [Logo]  Cuba Corp Portal          [HR Admin] [Logout]    |
  +----------------------------------------------------------+
  |  Navigation: [Home] [All Clockings] [Manage News] [Categories]|
  +----------------------------------------------------------+
  |  Manage News                              [Publish New]     |
  |  +------------------------------------------------------+|
  |  |Title              | Category | Date    | Status  | Actions   ||
  |  |Company Picnic     | Events   | 08/26   |Published|[Edit][Unpub]||
  |  |HR Policy Update   | HR       | 08/27   |Published|[Edit][Unpub]||
  |  |Network Maint.     | IT       | 08/25   |Unpublished|[Edit][Pub]||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
}
@endsalt
```

#### All Clockings (HR)

```plantuml
@startsalt
title Primary Screen: All Clockings (HR)
{
  +Portal Cuba Corp - HR Dashboard+
  +----------------------------------------------------------+
  |  [Logo]  Cuba Corp Portal          [HR Admin] [Logout]    |
  +----------------------------------------------------------+
  |  Navigation: [Home] [All Clockings] [Manage News] [Categories]|
  +----------------------------------------------------------+
  |  All Employee Clockings                                    |
  |  +------------------------------------------------------+|
  |  |  Month: [August 2026 v]    [Export CSV]              ||
  |  |  Employee: [____________]                           ||
  |  +------------------------------------------------------+|
  |  +------------------------------------------------------+|
  |  |Employee    | Date    | Time In | Time Out | Direction||
  |  |M. Rodriguez| 08/28   | 08:32   | ---      | In      ||
  |  |C. Perez    | 08/28   | 08:45   | ---      | In      ||
  |  |A. Gomez    | 08/27   | 08:30   | 17:15    | In/Out  ||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
}
@endsalt
```

#### Worker Categories (HR)

```plantuml
@startsalt
title Primary Screen: Worker Categories (HR)
{
  +Portal Cuba Corp - HR Dashboard+
  +----------------------------------------------------------+
  |  [Logo]  Cuba Corp Portal          [HR Admin] [Logout]    |
  +----------------------------------------------------------+
  |  Navigation: [Home] [All Clockings] [Manage News] [Categories]|
  +----------------------------------------------------------+
  |  Manage Worker Categories                                  |
  |  +------------------------------------------------------+|
  |  |  Search AD User: [____________]  [Search]             ||
  |  +------------------------------------------------------+|
  |  Current Assignments                                      |
  |  +------------------------------------------------------+|
  |  |AD User ID    | Employee Name  | Category  | Actions  ||
  |  |jrodriguez    | M. Rodriguez   | IT        | [Edit]   ||
  |  |cperez        | C. Perez       | Finance   | [Edit]   ||
  |  |agomez        | A. Gomez       | HR        | [Edit]   ||
  |  +------------------------------------------------------+|
  |  +------------------------------------------------------+|
  |  |  Assign Category                                     ||
  |  |  AD User ID: [____________]                          ||
  |  |  Category: [Select v]                                ||
  |  |  [   Save   ]                                        ||
  |  +------------------------------------------------------+|
  +----------------------------------------------------------+
}
@endsalt
```

### UI Patterns

> **Contributed by:** User-Interface Designer (Analysis & Design Discipline)
> **Purpose:** Interaction conventions, visual hierarchy, terminology, and accessibility rules that the Designer, Implementer, and Technical Writer must follow to ensure consistency across all screens. CON-011: the custom design at `docs/inputs/employee-portal-design.html` is MANDATORY and authoritative.

#### Interaction Conventions

| Pattern ID | Pattern | Rule | Rationale | Traces To |
|---|---|---|---|---|
| UIP-001 | Navigation bar | Persistent top navigation bar with role-based links. Employee: [Home] [My Clockings] [Directory]. HR: [Home] [All Clockings] [Manage News] [Categories]. | Consistency (Nielsen #4); recognition over recall (Nielsen #6) | CON-011, USA-004 |
| UIP-002 | Primary action button | Single prominent button per primary action (e.g., "Clock In" is the only large button on the main page). | Fitts's Law; error prevention (Nielsen #5) | USA-005, AC-001 |
| UIP-003 | Confirmation dialog | Destructive or irreversible actions (unpublish) require a confirmation dialog with clear wording: "Unpublish this news item? It will be hidden but not deleted." | User control and freedom (Nielsen #3); error prevention (Nielsen #5) | CON-013, UC-007 |
| UIP-004 | Inline feedback | Success/error messages appear inline on the same page (no redirect for confirmation). Clocking confirmation, validation errors, and audit confirmations display on-page. | Visibility of system status (Nielsen #1); minimize page reloads | NFR-002, USA-005 |
| UIP-005 | Form validation | Field-level validation errors display next to the invalid field. Summary of all errors at top of form. | Error recovery (Nielsen #9); error prevention (Nielsen #5) | USA-006, AC-002 |
| UIP-006 | Back navigation | Every non-main page has a "Back" link/button returning to the preceding hub screen. No dead-end screens. | User control and freedom (Nielsen #3) | Navigation Topology |
| UIP-007 | Category filter | News category filter uses pill/toggle buttons (All, General, HR, IT, Events) — not a dropdown. Selected state is visually distinct. | Recognition over recall (Nielsen #6); ease of use | USA-004, FR-008 |
| UIP-008 | Table actions | Action buttons in table rows use text labels ([Edit] [Unpublish]), not icons alone. | Recognition over recall (Nielsen #6); accessibility | USA-004 |

#### Visual Hierarchy

| Pattern ID | Element | Rule | Traces To |
|---|---|---|---|
| UIV-001 | Page header | Logo + portal name + user name + logout button. Consistent across all pages. | CON-011 |
| UIV-002 | Section headers | Each functional area on a page has a clear section header (e.g., "Clock In / Out", "Featured News", "News Feed"). | CON-011, USA-004 |
| UIV-003 | Featured news banner | Featured news items display with a visually distinct banner at the top of the news section. | FR-008, CON-011 |
| UIV-004 | Status indicators | Clocking status ("Not Clocked In" / "Clocked In at 08:32") is prominently displayed above the action button. | USA-005, AC-001 |
| UIV-005 | Table layout | Data tables (clockings, news management, directory results) use consistent column alignment: left-aligned text, right-aligned numbers. | CON-011 |

#### Terminology

| Term | Usage | Rationale |
|---|---|---|
| "Clock In" / "Clock Out" | Button labels — never "Check In", "Punch In", or "Register Entry" | Matches employee mental model; AC-001 |
| "My Clockings" | Navigation link to personal clocking history — never "Time Records" or "Attendance Log" | Self-descriptive; USA-004 |
| "Manage News" | HR navigation link — never "Content Management" or "Article Admin" | Simple, role-appropriate; AC-002 |
| "Unpublish" | Action to hide a news item — never "Delete" or "Remove" | CON-013: items are never hard-deleted |
| "Worker Categories" | HR navigation link — never "Employee Classification" or "Staff Tags" | Matches HR terminology |
| "Directory" | Navigation link — never "Phone Book" or "Contact List" | Replaces PDF phone directory; AC-003 |

#### Accessibility Rules

| Rule ID | Rule | Traces To |
|---|---|---|
| UIA-001 | All interactive elements (buttons, links, form fields) are keyboard-navigable. Tab order follows visual order. | WCAG 2.1 — Operable |
| UIA-002 | Color is never the sole indicator of status or category. Text labels accompany all color-coded elements. | WCAG 2.1 — Perceivable; R001 fallback |
| UIA-003 | Form fields have associated `<label>` elements. Error messages are programmatically associated with their fields. | WCAG 2.1 — Understandable |
| UIA-004 | Missing AD attributes display "N/A" text, not blank cells or red indicators. | R001 fallback; WCAG 2.1 — Robust |
| UIA-005 | Page uses semantic HTML structure (header, nav, main, section, footer). | WCAG 2.1 — Robust |

> **Note:** No specific accessibility standard (WCAG, EN 301 549, Section 508) was declared by the stakeholder in the Work Order. The rules above are baseline good practice derived from WCAG 2.1 principles. If the stakeholder declares a specific compliance level, these rules must be updated to reference it explicitly.

### UI Flow Coverage Summary

| UC | UI Significance | Flow Type | Diagram | UC Steps Covered |
|---|---|---|---|---|
| UC-001 | Critical (clocking + offline) | Activity (swimlanes) | ✅ | Main + A1, A2, A3 |
| UC-002 | Standard | Tabular | — | Main (5 steps) |
| UC-003 | Standard | Tabular | — | Main (6 steps) |
| UC-004 | Standard | Tabular | — | Main (5 steps) |
| UC-005 | Critical (publish + audit) | Activity (swimlanes) | ✅ | Main (7 steps) |
| UC-006 | Standard | Tabular | — | Main (6 steps) |
| UC-007 | Critical (unpublish + confirm) | Activity (swimlanes) | ✅ | Main (6 steps) |
| UC-008 | Critical (news feed + filter) | Activity (swimlanes) | ✅ | Main (7 steps) |
| UC-009 | Critical (directory + R001) | Activity (swimlanes) | ✅ | Main + A1 |
| UC-010 | Critical (category + audit) | Activity (swimlanes) | ✅ | Main + A1, A2 |
## Capsules, Protocols and Signals
### NewsItem Lifecycle State Machine (CLS-017)

The NewsItem entity has two lifecycle states governed by CON-013 (no hard delete) and NFR-004 (audit trail). Every transition is audited via `IAuditLogger.LogAudit()`. The implementation creates NewsItem directly as `Published` — there is no `Draft` state because no approval workflow exists in the declared scope.

> **Construction C2 Update:** The `Draft` state has been removed. The prior 3-state model (Draft → Published → Unpublished) assumed an intermediate creation step that does not exist in the implementation or the UC-005 flow. NewsItem is created as `Published` immediately upon `Publish()` call.

```plantuml
@startuml
title Portal Cuba Corp — NewsItem Lifecycle State Machine (CLS-017, Construction C2)

skinparam classAttributeIconSize 0

[*] --> Published : NewsService.Publish()\n(authorId, timestamp)
Published --> Published : Edit()\n(authorId, timestamp)\n[updates UpdatedAt]
Published --> Unpublished : Unpublish()\n(authorId, timestamp)
Unpublished --> Published : Publish()\n(re-publish allowed)

note right of Published
  Initial state when created.
  Visible to employees in news feed.
  Editable by HR (UC-006).
  Audit: AuditAction.Publish on creation,
  AuditAction.Edit on each edit.
  CON-013: never hard-deleted.
end note

note right of Unpublished
  Hidden from employee news feed.
  Record preserved for audit trail
  (CON-013, NFR-004).
  Can be re-published by HR.
  Audit: AuditAction.Unpublish.
end note

@enduml
```

### State Transition Audit Mapping

| From State | To State | Trigger | Audit Action (CLS-015) | UC |
|---|---|---|---|---|
| (new) | Published | Publish() | Publish | UC-005 |
| Published | Published | Edit() | Edit | UC-006 |
| Published | Unpublished | Unpublish() | Unpublish | UC-007 |
| Unpublished | Published | Publish() (re-publish) | Publish | UC-005 |

> **CON-013 enforcement:** No transition leads to a "Deleted" state. The Unpublished state is terminal unless HR explicitly re-publishes. The record remains in the `news_items` table indefinitely.

### Testability Entry Points

The design exposes dependency injection seams and observable state at every layer boundary, enabling unit tests without external dependencies (PostgreSQL, Active Directory, Keycloak).

| DI Seam | Interface | Test Replacement | Observable State |
|---|---|---|---|
| ClockingService → Persistence | INT-007 (IPersistence) | In-memory EF Core DbContext or mock IPersistence | ClockingRecord.IdempotencyKey uniqueness; ClockingResult.IsDuplicate flag |
| ClockingService → LDAP | INT-006 (ILdapGateway) | Mock returning preset LdapSearchResult | Employee name resolution in clocking list |
| NewsService → Persistence | INT-007 (IPersistence) | In-memory EF Core DbContext | NewsItem.Status transitions; NewsItem.UpdatedAt |
| NewsService → Audit | INT-005 (IAuditLogger) | Spy recording LogAudit calls | AuditAction enum; entityType; entityId; author; timestamp |
| DirectoryService → LDAP | INT-006 (ILdapGateway) | Mock returning LdapSearchResult with missing attributes | DirectoryEntry fields show "N/A" for missing attributes (R001) |
| WorkerCategoryService → Persistence | INT-007 (IPersistence) | In-memory EF Core DbContext | WorkerCategory upsert (INSERT ... ON CONFLICT UPDATE) |
| WorkerCategoryService → LDAP | INT-006 (ILdapGateway) | Mock returning preset DirectoryEntry | AD user lookup for category assignment |
| WorkerCategoryService → Audit | INT-005 (IAuditLogger) | Spy recording LogAudit calls | AuditAction.CategoryChanged; entityId = adUserId |
| PersistenceGateway → DbContext | CLS-008 (PortalDbContext) | EF Core in-memory provider | DbSet<Clockings/NewsItems/WorkerCategories/AuditRecords> queryable state |
| LdapGateway → Connection Pool | CLS-010 (LdapConnectionPool) | Mock ILdapConnection | Search filter construction; connection acquire/release |

> **Test harness pattern:** Register mock implementations in `IServiceCollection` during test setup. All services accept dependencies via constructor injection — no service locator, no static state. The `ExecuteInTransactionAsync` callback pattern allows tests to verify transactional behavior by asserting that audit records are only persisted when the business operation succeeds.
## Traceability
| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| **Analysis Classes** | | | |
| ACL-001 (ClockingUI) | UC-001, FR-001 | Derives | CLS-001, V001 |
| ACL-002 (ClockingController) | UC-001, UC-002, UC-003, UC-004, NFR-002 | Derives | CLS-001, INT-001 |
| ACL-003 (ClockingRecord) | UC-001 | Derives | CLS-016 |
| ACL-004 (DirectorySearchUI) | UC-009, FR-009 | Derives | CLS-003, V007 |
| ACL-005 (DirectoryController) | UC-009, R001, CON-005 | Derives | CLS-003, INT-003 |
| ACL-006 (DirectoryEntry) | UC-009, CON-012 | Derives | CLS-020 |
| ACL-007 (NewsUI) | UC-005, UC-006, UC-007 | Derives | CLS-002, V004, V005, V006 |
| ACL-008 (NewsController) | UC-005, UC-006, UC-007, NFR-004 | Derives | CLS-002, INT-002 |
| ACL-009 (NewsItem) | UC-005, UC-006, UC-007, UC-008 | Derives | CLS-017 |
| ACL-010 (AuditRecord) | UC-005, UC-006, UC-007, UC-010, NFR-004 | Derives | CLS-019 |
| ACL-011 (CategoryUI) | UC-010, FR-010 | Derives | CLS-004, V008 |
| ACL-012 (CategoryController) | UC-010, CON-009 | Derives | CLS-004, INT-004 |
| ACL-013 (WorkerCategory) | UC-010, CON-009 | Derives | CLS-018 |
| ACL-014 (NewsFeedUI) | UC-008, FR-008 | Derives | CLS-002, V001 |
| ACL-015 (NewsFeedController) | UC-008 | Derives | CLS-002, INT-002 |
| **Use-Case Realizations** | | | |
| SEQ-001 (UC-001 Clock In/Out) | UC-001, AC-005, NFR-002 | Derives | CLS-001, CLS-007, CLS-016 |
| SEQ-002 (UC-002 Clocking History) | UC-002, FR-002 | Derives | CLS-001, CLS-007 |
| SEQ-003 (UC-003 All Clockings) | UC-003, FR-003, CON-009 | Derives | CLS-001, CLS-006, CLS-007 |
| SEQ-004 (UC-004 CSV Export) | UC-004, FR-004, PERF-004 | Derives | CLS-001, CLS-006, CLS-007 |
| SEQ-005 (UC-005 Publish News) | UC-005, NFR-004, AC-002 | Derives | CLS-002, CLS-005, CLS-007, CLS-017 |
| SEQ-006 (UC-006 Edit News) | UC-006, NFR-004 | Derives | CLS-002, CLS-005, CLS-007 |
| SEQ-007 (UC-007 Unpublish News) | UC-007, CON-013, NFR-004 | Derives | CLS-002, CLS-005, CLS-007 |
| SEQ-008 (UC-008 Read/Filter News) | UC-008, FR-008 | Derives | CLS-002, CLS-007 |
| SEQ-009 (UC-009 Directory Search) | UC-009, R001, CON-005, CON-012 | Derives | CLS-003, CLS-006 |
| SEQ-010 (UC-010 Manage Category) | UC-010, CON-009, NFR-004 | Derives | CLS-004, CLS-005, CLS-006, CLS-007 |
| **Design Classes — Services** | | | |
| CLS-001 (ClockingService) | ACL-002, COMP-002, INT-001 | Realizes | INT-006, INT-007 |
| CLS-002 (NewsService) | ACL-008, COMP-003, INT-002 | Realizes | INT-005, INT-007 |
| CLS-003 (DirectoryService) | ACL-005, COMP-001, INT-003 | Realizes | INT-006 |
| CLS-004 (WorkerCategoryService) | ACL-012, COMP-004, INT-004 | Realizes | INT-005, INT-006, INT-007 |
| CLS-005 (AuditInterceptor) | ACL-010, COMP-008, INT-005 | Realizes | INT-007 |
| **Design Classes — Infrastructure** | | | |
| CLS-006 (LdapGateway) | COMP-005, INT-006 | Realizes | (AD external) |
| CLS-007 (PersistenceGateway) | COMP-006, INT-007 | Realizes | CLS-008 |
| CLS-008 (PortalDbContext) | COMP-006, CON-003 | Derives | CLS-016, CLS-017, CLS-018, CLS-019 |
| CLS-009 (LdapSettings) | CON-005, R001 | Derives | CLS-006 |
| CLS-010 (LdapConnectionPool) | CON-005 | Derives | CLS-006 |
| **Design Classes — Domain** | | | |
| CLS-011 (ClockType enum) | FR-001 | Derives | CLS-016 |
| CLS-012 (ClockStatus enum) | FR-001 | Derives | (UI) |
| CLS-013 (NewsCategory enum) | FR-005 | Derives | CLS-017 |
| CLS-014 (NewsStatus enum) | CON-013, FR-007 | Derives | CLS-017 |
| CLS-015 (AuditAction enum) | NFR-004 | Derives | CLS-019 |
| CLS-016 (ClockingRecord) | ACL-003, FR-001, AC-005 | Derives | T1 (clockings) |
| CLS-017 (NewsItem) | ACL-009, FR-005, CON-013 | Derives | T2 (news_items) |
| CLS-018 (WorkerCategory) | ACL-013, FR-010, CON-009 | Derives | T3 (worker_categories) |
| CLS-019 (AuditRecord) | ACL-010, NFR-004 | Derives | T4 (audit_records) |
| CLS-020 (DirectoryEntry) | ACL-006, CON-009, CON-012 | Derives | (not persisted — AD projection) |
| CLS-021 (DateRange) | — | Derives | (value object) |
| CLS-022 (ClockingResult) | AC-005 | Derives | CLS-016 |
| CLS-023 (LdapSearchResult) | CON-005 | Derives | CLS-006 |
| **Interfaces** | | | |
| INT-001 (IClockingService) | COMP-002, SAD | Derives | CLS-001 |
| INT-002 (INewsService) | COMP-003, SAD | Derives | CLS-002 |
| INT-003 (IDirectoryService) | COMP-001, SAD | Derives | CLS-003 |
| INT-004 (IWorkerCategoryService) | COMP-004, SAD | Derives | CLS-004 |
| INT-005 (IAuditLogger) | COMP-008, SAD | Derives | CLS-005 |
| INT-006 (ILdapGateway) | COMP-005, SAD | Derives | CLS-006 |
| INT-007 (IPersistence) | COMP-006, SAD | Derives | CLS-007 |
| **State Machines** | | | |
| NewsItem Lifecycle | CLS-017, CON-013, NFR-004 | Derives | CLS-002, CLS-005 |
| **Testability Entry Points** | | | |
| DI Seam: IPersistence | INT-007, CLS-007 | Derives | Test harness (in-memory DbContext) |
| DI Seam: ILdapGateway | INT-006, CLS-006 | Derives | Test harness (mock LDAP) |
| DI Seam: IAuditLogger | INT-005, CLS-005 | Derives | Test harness (audit spy) |
| **UI Elements (from UI Designer)** | | | |
| V001 (MainPageModel) | UC-001, UC-008, CON-011 | Derives | CLS-001, CLS-002 |
| V002 (ClockingPageModel) | UC-002 | Derives | CLS-001 |
| V003 (AllClockingsModel) | UC-003, UC-004 | Derives | CLS-001 |
| V004 (PublishNewsModel) | UC-005, AC-002 | Derives | CLS-002 |
| V005 (EditNewsModel) | UC-006 | Derives | CLS-002 |
| V006 (NewsManagementModel) | UC-007, CON-013 | Derives | CLS-002 |
| V007 (DirectorySearchModel) | UC-009, AC-003, R001 | Derives | CLS-003 |
| V008 (WorkerCategoryModel) | UC-010, CON-009 | Derives | CLS-004 |
| Navigation Topology | All UCs, CON-011 | Derives | V001–V008 |
| UI Patterns | CON-011, USA-001–USA-006 | Refines | V001–V008, Implementer |
| Wireframes | CON-011, All UCs | Derives | V001–V008 |
| **Database Tables** | | | |
| T1 (clockings) | CLS-016, AC-005 | Derives | PostgreSQL (CON-003) |
| T2 (news_items) | CLS-017, CON-013 | Derives | PostgreSQL (CON-003) |
| T3 (worker_categories) | CLS-018, CON-009 | Derives | PostgreSQL (CON-003) |
| T4 (audit_records) | CLS-019, NFR-004 | Derives | PostgreSQL (CON-003) |
