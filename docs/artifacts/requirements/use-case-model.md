## Document Control
| Field | Value |
|---|---|
| Phase | Construction |
| Status | Draft |
| Milestone Target | End of Construction |
| Iteration | 2 (Cycle 1) |
| Date | 2026-08-28 |
| Prior Phase | Elaboration (LCA achieved — 0 Critical, 0 Major open; stakeholder sanction GRANTED) |
| Evolution | Construction Iter 1: Requirements baseline preserved — no approved CR affects scope or NFRs. All Elaboration implementation decisions (offline retry with idempotency key in UC-001, audit trail in UC-005/006/007/010, AD read-only in UC-009/010) already reflected in Elaboration baseline. Document Control updated to Construction phase. Construction Iter 2: CR-010 (IsFeatured flag) applied to UC-005 and UC-006 — IsFeatured is [DERIVED — from FR-008, awaiting stakeholder confirmation] enabling the featured news banner in UC-008. Use-Case Diagram title updated to Construction. |
## Use-Case Diagram
```plantuml
@startuml
title Portal Cuba Corp — Use-Case Model (Construction)

left to right direction
skinparam packageStyle rectangle
skinparam actorStyle hollow

actor "Employee" as EMP
actor "HR Administrator" as HR
actor "Active Directory\n(LDAP)" as AD <<external system>>
actor "Keycloak\n(OIDC)" as KC <<external system>>

rectangle "Portal Cuba Corp — System Boundary" {
  usecase "UC-001\nClock In / Clock Out" as UC001
  usecase "UC-002\nView Own Clocking\nHistory" as UC002
  usecase "UC-003\nView All Employee\nClockings" as UC003
  usecase "UC-004\nExport Monthly\nClocking Report" as UC004
  usecase "UC-005\nPublish News" as UC005
  usecase "UC-006\nEdit Published News" as UC006
  usecase "UC-007\nUnpublish News" as UC007
  usecase "UC-008\nRead and Filter News" as UC008
  usecase "UC-009\nSearch Employee\nDirectory" as UC009
  usecase "UC-010\nManage Worker\nCategory" as UC010
}

EMP --> UC001
EMP --> UC002
EMP --> UC008
EMP --> UC009

HR --> UC003
HR --> UC004
HR --> UC005
HR --> UC006
HR --> UC007
HR --> UC010

UC009 ..> AD : LDAP read\n(corporate attributes)
UC010 ..> AD : LDAP read\n(AD user id lookup)
UC003 ..> AD : LDAP read\n(employee name lookup)

EMP ..> KC : OIDC login\n(all UCs)
HR ..> KC : OIDC login + HR role\n(UC-003..UC-007, UC-010)

note right of UC001
  Architecturally significant
  NFR-002: <1s response
  AC-005: offline retry (5 min)
  CR-011: idempotency key
  Volatility: Low
end note

note right of UC009
  Architecturally significant
  R001: LDAP attribute risk
  Volatility: High
end note

note bottom of UC005
  NFR-004: Audit trail
  applies to UC-005, UC-006,
  UC-007, UC-010
  CR-010: IsFeatured flag
  [DERIVED — from FR-008]
end note

@enduml
```
## Actors

| ID | Actor | Type | Description | Source |
|---|---|---|---|---|
| ACT-001 | Employee | Human (primary) | Any authenticated Cuba Corp employee (200 across 3 offices). Uses the portal for clocking, news reading, and directory search. | STK-004 |
| ACT-002 | HR Administrator | Human (primary) | HR staff member with elevated permissions (determined by OIDC role claims from Keycloak). Manages news, views all clockings, exports reports, manages worker categories. | STK-001 |
| ACT-003 | Active Directory (LDAP) | External system | Corporate directory accessed via LDAP for read-only employee data (name, job title, department, office, email, extension). System of record for employee attributes. | CON-005, CON-009 |
| ACT-004 | Keycloak (OIDC) | External system | Identity provider for authentication and authorization. Portal is an OIDC client only — no provisioning or management. | CON-004 |

## Use-Case Survey

| UC ID | Name | Source | Primary Actor | MoSCoW | Volatility | Architecturally Significant | Detail Level |
|---|---|---|---|---|---|---|---|
| UC-001 | Clock In / Clock Out | FR-001 | Employee | Must | Low | Yes (NFR-002: <1s, AC-005: offline retry) | Detailed |
| UC-002 | View Own Clocking History | FR-002 | Employee | Must | Low | No | Detailed |
| UC-003 | View All Employee Clockings | FR-003 | HR Administrator | Must | Low | No | Detailed |
| UC-004 | Export Monthly Clocking Report | FR-004 | HR Administrator | Must | Low | No | Detailed |
| UC-005 | Publish News | FR-005 | HR Administrator | Must | Medium | No | Detailed |
| UC-006 | Edit Published News | FR-006 | HR Administrator | Must | Medium | No | Detailed |
| UC-007 | Unpublish News | FR-007 | HR Administrator | Must | Low | No | Detailed |
| UC-008 | Read and Filter News | FR-008 | Employee | Must | Medium | No | Detailed |
| UC-009 | Search Employee Directory | FR-009 | Employee | Must | High | Yes (R001: LDAP risk) | Detailed |
| UC-010 | Manage Worker Category | FR-010 | HR Administrator | Must | Medium | No | Detailed |

## Use-Case Specifications
### UC-001: Clock In / Clock Out — DETAILED

| Field | Value |
|---|---|
| Source | FR-001 |
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee opens the portal main page |
| Preconditions | Employee is authenticated via Keycloak OIDC |
| Postconditions | Clocking record persisted in PostgreSQL with employee id, client-side timestamp, clock direction (in/out), and idempotency key; confirmation displayed |
| MoSCoW | Must |
| Volatility | Low |

**Main Flow:**
1. Employee navigates to the portal main page.
2. System retrieves the employee's current clocking status from the database (authenticated employee id from OIDC token).
3. System displays a "Clock In" or "Clock Out" button depending on current status.
4. Employee presses the button.
5. Client records the press timestamp and generates an idempotency key in localStorage.
6. Client sends POST request to server with timestamp and idempotency key.
7. Server records the clocking entry (employee id, client timestamp, direction, idempotency key) in PostgreSQL.
8. Server returns confirmation with the recorded time.
9. Employee sees confirmation on screen.

**Alternative Flows:**
- **A1: Network error during POST (offline retry — AC-005 resolved):** Client stores the press in localStorage and retries the POST for up to 5 minutes. When the network is restored, the server accepts the original client-side timestamp (the moment the employee pressed) and rejects duplicates by idempotency key. This is a page-level script on an already-rendered Razor page — no SPA, no client-side router (CON-002 stands). This is not the excluded sync work: one action, one queue, one entity, nothing to reconcile.
- **A2: Network not restored within 5 minutes:** Client stops retrying and displays "Clocking not recorded — report to HR." The employee reports the clocking to HR manually.
- **A3: Duplicate POST received (idempotency):** Server detects the idempotency key already exists in the clocking table and returns the original confirmation without creating a duplicate record.

**Activity Diagram:**

```plantuml
@startuml
title UC-001: Clock In / Clock Out — Activity Diagram

|Employee|
start
:Opens portal main page;
|Portal API|
:Validate OIDC token;
:Query current clocking status\nfrom PostgreSQL;
|Employee|
if (Currently clocked in?) then (yes)
  :Display "Clock Out" button;
else (no)
  :Display "Clock In" button;
endif
:Presses button;
|Browser (Razor Page)|
:Record timestamp +\ngenerate idempotency key\nin localStorage;
:Send POST /api/clocking\n{timestamp, idempotencyKey, direction};
|Portal API|
if (Network available?) then (yes — Main Flow)
  :Insert clocking record in PostgreSQL;
  :Return confirmation with recorded time;
  |Browser (Razor Page)|
  :Display "Clocked {in/out} at HH:MM:SS";
  |Employee|
  :Sees confirmation;
else (no — A1: Offline Retry)
  |Browser (Razor Page)|
  :Store pending request in localStorage;
  :Display "Saving clocking...\nWill retry automatically";
  repeat
    :Retry POST /api/clocking;
  repeat while (Network still down?\nAND < 5 minutes?) is (yes)
  -> no;
  if (Network restored?) then (yes)
    |Portal API|
    :Insert clocking record in PostgreSQL;
    :Return confirmation;
    |Browser (Razor Page)|
    :Display "Clocked {in/out} at HH:MM:SS";
  else (no — A2: 5-min timeout)
    :Display "Clocking not recorded —\nreport to HR";
  endif
endif
stop
@enduml
```

**Sequence Diagram (Key Realization — Main + Offline Retry + Idempotency):**

```plantuml
@startuml
title UC-001: Clock In/Out — Sequence Diagram (Main + Offline Retry)

actor "Employee" as EMP
participant "Browser\n(Razor Page)" as UI
participant "Portal API\n(.NET 10)" as API
database "PostgreSQL" as DB

== Main Flow (Online) ==
EMP -> UI : Opens portal main page
UI -> API : GET /api/clocking/status (with OIDC token)
API -> API : Validate OIDC token
API -> DB : Query current clocking status
DB --> API : Status: not clocked in
API --> UI : Display "Clock In" button
EMP -> UI : Presses "Clock In"
UI -> UI : Record timestamp +\ngenerate idempotency key\n(localStorage)
UI -> API : POST /api/clocking {timestamp, idempotencyKey, direction:in}
API -> DB : INSERT clocking record\n(employeeId, timestamp, direction,\nidempotencyKey)
DB --> API : Insert confirmed
API --> UI : Confirmation with recorded time
UI --> EMP : "Clocked in at HH:MM:SS"

== Alternative Flow A1: Network Error (Offline Retry) ==
EMP -> UI : Presses "Clock Out"
UI -> UI : Record timestamp +\ngenerate idempotency key\n(localStorage)
UI -> API : POST /api/clocking {timestamp, idempotencyKey, direction:out}
API -x UI : Network error (timeout/5xx)
UI -> UI : Store pending request in localStorage\nStart retry timer (up to 5 min)
UI --> EMP : "Saving clocking...\nWill retry automatically"
loop Every 30s for up to 5 minutes
  UI -> API : Retry POST /api/clocking
  alt Network restored
    API -> DB : INSERT clocking record
    DB --> API : Insert confirmed
    API --> UI : Confirmation
    UI --> EMP : "Clocked out at HH:MM:SS"
  else Still offline
    API -x UI : Network error
  end
end

== Alternative Flow A2: 5-Minute Timeout ==
UI --> EMP : "Clocking not recorded —\nreport to HR"

== Alternative Flow A3: Duplicate POST (Idempotency) ==
UI -> API : POST /api/clocking {timestamp, idempotencyKey}
API -> DB : Check idempotencyKey exists
DB --> API : Key already exists
API --> UI : Return original confirmation\n(no duplicate created)
UI --> EMP : "Clocked out at HH:MM:SS"

@enduml
```

### UC-002: View Own Clocking History — DETAILED

| Field | Value |
|---|---|
| Source | FR-002 |
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee selects "My Clocking History" |
| Preconditions | Employee is authenticated via Keycloak OIDC |
| Postconditions | Employee's clocking history for the current month is displayed |
| MoSCoW | Must |
| Volatility | Low |

**Main Flow:**
1. Employee navigates to "My Clocking History" page.
2. System queries the database for the employee's clocking records for the current month.
3. System displays the clocking history in a table (date, time in, time out).
4. Employee reviews their history.

**Alternative Flows:**
- **A1: No clocking records for current month:** System displays "No clocking records for this month yet."

**Activity Diagram:**

```plantuml
@startuml
title UC-002: View Own Clocking History — Activity Diagram

|Employee|
start
:Navigate to "My Clocking History";
|Portal API|
:Validate OIDC token;
:Query clocking records for\nauthenticated employee id\n(current month);
if (Records found?) then (yes)
  :Return clocking history\n(date, time in, time out);
  |Employee|
  :View clocking history table;
else (no — A1)
  |Employee|
  :See "No clocking records\nfor this month yet";
endif
stop
@enduml
```

### UC-003: View All Employee Clockings — DETAILED

| Field | Value |
|---|---|
| Source | FR-003 |
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR selects "All Employee Clockings" |
| Preconditions | HR Administrator is authenticated with HR role via Keycloak OIDC |
| Postconditions | All employees' clockings are displayed with employee names resolved from AD |
| MoSCoW | Must |
| Volatility | Low |

**Main Flow:**
1. HR Administrator navigates to "All Employee Clockings" page.
2. System verifies HR role from OIDC token.
3. System queries the database for all clocking records.
4. System resolves employee names from Active Directory via LDAP (using employee id from clocking records).
5. System displays clockings in a table (employee name, date, time in, time out).

**Alternative Flows:**
- **A1: AD unavailable for name resolution:** System displays employee id instead of name and shows a warning "AD unavailable — showing employee IDs."

**Activity Diagram:**

```plantuml
@startuml
title UC-003: View All Employee Clockings — Activity Diagram

|HR Administrator|
start
:Navigate to "All Employee Clockings";
|Portal API|
:Validate OIDC token;
:Verify HR role in token claims;
:Query all clocking records\nfrom PostgreSQL;
:Resolve employee names\nfrom AD via LDAP;
if (AD available?) then (yes — Main Flow)
  :Display clockings table\n(employee name, date, time in, time out);
  |HR Administrator|
  :Review all employee clockings;
else (no — A1)
  :Display employee IDs instead of names;
  :Show warning "AD unavailable —\nshowing employee IDs";
  |HR Administrator|
  :Review clockings with IDs;
endif
stop
@enduml
```

### UC-004: Export Monthly Clocking Report — DETAILED

| Field | Value |
|---|---|
| Source | FR-004 |
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR selects "Export Monthly Report" |
| Preconditions | HR Administrator is authenticated with HR role; clocking data exists for the selected month |
| Postconditions | CSV file is downloaded to the HR Administrator's machine |
| MoSCoW | Must |
| Volatility | Low |

**Main Flow:**
1. HR Administrator selects month and clicks "Export CSV."
2. System queries the database for all clocking records for the selected month.
3. System resolves employee names from AD via LDAP.
4. System generates a CSV file (RFC 4180 compliant) with columns: employee name, employee id, date, time in, time out.
5. System sends the CSV file as a download response.
6. HR Administrator receives the CSV file.

**Alternative Flows:**
- **A1: No clocking data for selected month:** System displays "No clocking data for the selected month."
- **A2: AD unavailable during export:** System exports with employee IDs instead of names and includes a note in the CSV header.

**Activity Diagram:**

```plantuml
@startuml
title UC-004: Export Monthly Clocking Report — Activity Diagram

|HR Administrator|
start
:Select month;
:Click "Export CSV";
|Portal API|
:Validate OIDC token;
:Verify HR role;
:Query all clocking records\nfor selected month;
if (Records found?) then (yes)
  :Resolve employee names\nfrom AD via LDAP;
  if (AD available?) then (yes)
    :Generate CSV (RFC 4180)\nwith employee names;
  else (no — A2)
    :Generate CSV with employee IDs\n+ note in header;
  endif
  :Send CSV file as download;
  |HR Administrator|
  :Receive CSV file;
else (no — A1)
  |HR Administrator|
  :See "No clocking data for\nthe selected month";
endif
stop
@enduml
```

### UC-005: Publish News — DETAILED

| Field | Value |
|---|---|
| Source | FR-005, NFR-004 |
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR selects "Publish News" |
| Preconditions | HR Administrator is authenticated with HR role via Keycloak OIDC |
| Postconditions | News item is published and visible to employees; audit record created (author + timestamp) |
| MoSCoW | Must |
| Volatility | Medium |

**Main Flow:**
1. HR Administrator navigates to "Publish News" page.
2. System displays a form with fields: title, body, date, category (General, HR, IT, Events), featured flag.
3. HR Administrator fills in the form and clicks "Publish."
4. System validates the form (title and body are required).
5. System persists the news item in PostgreSQL with status=published.
6. System creates an audit record: author identity (from OIDC token), timestamp, action=published.
7. System displays "News published successfully."

**Alternative Flows:**
- **A1: Validation error (missing title or body):** System displays validation error and does not persist.

**Activity Diagram:**

```plantuml
@startuml
title UC-005: Publish News — Activity Diagram

|HR Administrator|
start
:Navigate to "Publish News";
|Portal API|
:Validate OIDC token;
:Verify HR role;
:Display news form\n(title, body, date, category,\nfeatured flag);
|HR Administrator|
:Fill in form;
:Click "Publish";
|Portal API|
:Validate form\n(title + body required);
if (Valid?) then (yes — Main Flow)
  :Persist news item in PostgreSQL\n(status=published);
  :Create audit record\n(author from OIDC token,\ntimestamp, action=published);
  |HR Administrator|
  :See "News published successfully";
else (no — A1)
  |HR Administrator|
  :See validation error;
endif
stop
@enduml
```

### UC-006: Edit Published News — DETAILED

| Field | Value |
|---|---|
| Source | FR-006, NFR-004 |
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR selects a published news item to edit |
| Preconditions | HR Administrator is authenticated with HR role; news item exists and is published |
| Postconditions | News item is updated; audit record created (author + timestamp) |
| MoSCoW | Must |
| Volatility | Medium |

**Main Flow:**
1. HR Administrator navigates to the news management list.
2. System displays all news items (published and unpublished).
3. HR Administrator selects a published news item and clicks "Edit."
4. System displays the edit form pre-populated with current values.
5. HR Administrator modifies fields and clicks "Save."
6. System validates the form.
7. System updates the news item in PostgreSQL.
8. System creates an audit record: author identity, timestamp, action=edited.
9. System displays "News updated successfully."

**Alternative Flows:**
- **A1: Validation error:** System displays validation error and does not update.

**Activity Diagram:**

```plantuml
@startuml
title UC-006: Edit Published News — Activity Diagram

|HR Administrator|
start
:Navigate to news management list;
|Portal API|
:Validate OIDC token;
:Verify HR role;
:Display all news items\n(published + unpublished);
|HR Administrator|
:Select news item;
:Click "Edit";
|Portal API|
:Display edit form\n(pre-populated with current values);
|HR Administrator|
:Modify fields;
:Click "Save";
|Portal API|
:Validate form;
if (Valid?) then (yes — Main Flow)
  :Update news item in PostgreSQL;
  :Create audit record\n(author, timestamp, action=edited);
  |HR Administrator|
  :See "News updated successfully";
else (no — A1)
  |HR Administrator|
  :See validation error;
endif
stop
@enduml
```

### UC-007: Unpublish News — DETAILED

| Field | Value |
|---|---|
| Source | FR-007, CON-013, NFR-004 |
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR selects a published news item to unpublish |
| Preconditions | HR Administrator is authenticated with HR role; news item is published |
| Postconditions | News item status changed to unpublished (hidden from employees, record preserved); audit record created |
| MoSCoW | Must |
| Volatility | Low |

**Main Flow:**
1. HR Administrator navigates to the news management list.
2. System displays all news items.
3. HR Administrator selects a published news item and clicks "Unpublish."
4. System changes the news item status to unpublished in PostgreSQL (record is NOT deleted — CON-013).
5. System creates an audit record: author identity, timestamp, action=unpublished.
6. System displays "News unpublished successfully."

**Alternative Flows:**
- **A1: News item already unpublished:** System displays "This news item is already unpublished."

**Activity Diagram:**

```plantuml
@startuml
title UC-007: Unpublish News — Activity Diagram

|HR Administrator|
start
:Navigate to news management list;
|Portal API|
:Validate OIDC token;
:Verify HR role;
:Display all news items;
|HR Administrator|
:Select published news item;
:Click "Unpublish";
|Portal API|
if (Item currently published?) then (yes — Main Flow)
  :Change status to unpublished\n(record NOT deleted — CON-013);
  :Create audit record\n(author, timestamp, action=unpublished);
  |HR Administrator|
  :See "News unpublished successfully";
else (no — A1)
  |HR Administrator|
  :See "This news item is\nalready unpublished";
endif
stop
@enduml
```

### UC-008: Read and Filter News — DETAILED

| Field | Value |
|---|---|
| Source | FR-008 |
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee opens the portal main page |
| Preconditions | Employee is authenticated via Keycloak OIDC |
| Postconditions | News items are displayed, sorted by date, with optional category filter and featured banners |
| MoSCoW | Must |
| Volatility | Medium |

**Main Flow:**
1. Employee navigates to the portal main page.
2. System queries the database for published news items.
3. System displays featured news items with a banner at the top.
4. System displays remaining news items sorted by date (newest first).
5. Employee can select a category filter (General, HR, IT, Events).
6. System filters the displayed news items by the selected category.

**Alternative Flows:**
- **A1: No published news items:** System displays "No news items available."
- **A2: No news items in selected category:** System displays "No news items in this category."

**Activity Diagram:**

```plantuml
@startuml
title UC-008: Read and Filter News — Activity Diagram

|Employee|
start
:Navigate to portal main page;
|Portal API|
:Validate OIDC token;
:Query published news items\nfrom PostgreSQL;
if (News items found?) then (yes)
  :Display featured news\nwith banner at top;
  :Display remaining news\nsorted by date (newest first);
  |Employee|
  :Browse news;
  if (Selects category filter?) then (yes)
    |Portal API|
    :Filter by selected category\n(General, HR, IT, Events);
    if (Items in category?) then (yes)
      |Employee|
      :View filtered news;
    else (no — A2)
      |Employee|
      :See "No news items\nin this category";
    endif
  else (no)
    |Employee|
    :View all news;
  endif
else (no — A1)
  |Employee|
  :See "No news items available";
endif
stop
@enduml
```

### UC-009: Search Employee Directory — DETAILED

| Field | Value |
|---|---|
| Source | FR-009, CON-005, CON-012 |
| Primary Actor | Employee (ACT-001) |
| Trigger | Employee enters a search term in the directory search field |
| Preconditions | Employee is authenticated via Keycloak OIDC |
| Postconditions | Matching employee entries are displayed with corporate data only |
| MoSCoW | Must |
| Volatility | High (R001: LDAP attribute consistency risk) |

**Main Flow:**
1. Employee navigates to the "Directory" page.
2. System displays a search field.
3. Employee enters a search term (name, department, or office).
4. System queries Active Directory via LDAP with the search term.
5. AD returns matching entries with corporate attributes (name, job title, department, office, email, extension).
6. System displays the results in a list.
7. Employee reviews the results.

**Alternative Flows:**
- **A1: AD unavailable:** System displays "Directory unavailable — please try again later."
- **A2: Partial LDAP attributes (R001):** Some entries have missing attributes (e.g., extension phone not filled). System displays "Not available" for missing fields instead of hiding the entry.
- **A3: No results found:** System displays "No colleagues found matching your search."

**Activity Diagram:**

```plantuml
@startuml
title UC-009: Search Employee Directory — Activity Diagram

|Employee|
start
:Navigate to "Directory" page;
|Portal API|
:Validate OIDC token;
:Display search field;
|Employee|
:Enter search term\n(name, department, or office);
|Portal API|
:Query AD via LDAP\nwith search term;
if (AD available?) then (yes)
  if (Results found?) then (yes)
    :Map LDAP attributes to DTO\n(name, jobTitle, department,\noffice, email, extension);
    if (All attributes present?) then (yes — Main Flow)
      :Display full results;
      |Employee|
      :Review colleague information;
    else (no — A2: Partial attrs)
      :Display results with\n"Not available" for\nmissing fields;
      |Employee|
      :Review results with gaps;
    endif
  else (no — A3)
    |Employee|
    :See "No colleagues found\nmatching your search";
  endif
else (no — A1)
  |Employee|
  :See "Directory unavailable —\nplease try again later";
endif
stop
@enduml
```

**Sequence Diagram (Key Realization — with AD degradation):**

```plantuml
@startuml
title UC-009: Search Employee Directory — Sequence Diagram (with AD degradation)

actor "Employee" as EMP
participant "Browser\n(Razor Page)" as UI
participant "Portal API\n(.NET 10)" as API
participant "Active Directory\n(LDAP)" as AD

== Main Flow ==
EMP -> UI : Enters search term\n(name, department, or office)
UI -> API : GET /api/directory/search?q={term}\n(with OIDC token)
API -> API : Validate OIDC token
API -> AD : LDAP search(filter={term})\nattrs: cn, title, department,\nphysicalDeliveryOfficeName, mail, telephoneNumber
AD --> API : Matching entries\n(corporate attributes only)
API -> API : Map LDAP attributes to\nDTO {name, jobTitle, department,\noffice, email, extension}
API --> UI : Search results (list)
UI --> EMP : Display results\n(name, title, dept, office, email, ext)

== Alternative Flow A1: AD Unavailable (Graceful Degradation) ==
EMP -> UI : Enters search term
UI -> API : GET /api/directory/search?q={term}
API -> API : Validate OIDC token
API -> AD : LDAP search(filter={term})
AD -x API : Connection failed / timeout
API -> API : Catch LDAP exception\nLog error
API --> UI : HTTP 503 "Directory unavailable"
UI --> EMP : "Directory unavailable —\nplease try again later"

== Alternative Flow A2: Partial LDAP Attributes (R001) ==
EMP -> UI : Enters search term
UI -> API : GET /api/directory/search?q={term}
API -> AD : LDAP search(filter={term})
AD --> API : Entry found but\nsome attrs empty (e.g.,\ntelephoneNumber missing)
API -> API : Map available attrs\nMissing fields = "Not available"
API --> UI : Results with gaps
UI --> EMP : Display results\n(some fields show "Not available")

@enduml
```

### UC-010: Manage Worker Category — DETAILED

| Field | Value |
|---|---|
| Source | FR-010, CON-009, NFR-004 |
| Primary Actor | HR Administrator (ACT-002) |
| Trigger | HR selects "Manage Worker Categories" |
| Preconditions | HR Administrator is authenticated with HR role via Keycloak OIDC |
| Postconditions | Worker category link (AD user id → category) is created or updated; audit record created |
| MoSCoW | Must |
| Volatility | Medium |

**Main Flow:**
1. HR Administrator navigates to "Manage Worker Categories" page.
2. System verifies HR role from OIDC token.
3. System displays current worker category assignments (AD user id, category).
4. HR selects an employee (looks up AD user id via LDAP).
5. System confirms the employee exists in AD.
6. HR assigns or updates the category.
7. System validates the category value.
8. System persists the worker category link (AD user id, category) in the local table.
9. System creates an audit record: author identity from OIDC token, timestamp, action=CategoryChanged.
10. System displays "Category updated successfully."

**Alternative Flows:**
- **A1: Employee not found in AD:** System displays "Employee not found in AD."
- **A2: Invalid category value:** System displays a validation error.

**Activity Diagram:**

```plantuml
@startuml
title UC-010: Manage Worker Category — Activity Diagram

start
:HR Administrator selects
"Manage Worker Categories";
:System verifies HR role from OIDC token;
:System displays current worker category
assignments (AD user id, category);
:HR selects an employee
(looks up AD user id via LDAP);
if (Employee found in AD?) then (yes)
  :HR assigns or updates category;
  :System validates category value;
  if (Category valid?) then (yes)
    :System persists worker category link
    (AD user id, category) in local table;
    :System creates audit record:
    author identity from OIDC token,
    timestamp, action = CategoryChanged;
    :Display "Category updated successfully";
  else (no — A2)
    :Display validation error;
  endif
else (no — A1)
  :Display "Employee not found in AD";
endif
stop
@enduml
```
## Traceability
### Consolidated Requirements Traceability Flow

The following diagram shows the complete traceability chain from stakeholder needs (business goals) through declared features (FR-NNN) to use cases (UC-NNN) and acceptance criteria (AC-NNN). This consolidated view fulfills the Work Order's instruction to produce a consolidated requirements specification from all use cases and supplementary requirements.

```plantuml
@startuml
title Portal Cuba Corp — Requirements Traceability Flow

skinparam packageStyle rectangle
skinparam rectangleFontSize 10

package "Stakeholder Needs" {
  rectangle "BG-001: Reduce HR\nmanagement time 50%" as BG1
  rectangle "BG-002: Eliminate 100%\nExcel usage" as BG2
  rectangle "BG-003: 80% employee\nadoption in 3 months" as BG3
}

package "Declared Features (FR-NNN)" {
  rectangle "FR-001: Clock In/Out" as FR1
  rectangle "FR-002: View Own Clocking" as FR2
  rectangle "FR-003: View All Clockings" as FR3
  rectangle "FR-004: Export CSV Report" as FR4
  rectangle "FR-005: Publish News" as FR5
  rectangle "FR-006: Edit News" as FR6
  rectangle "FR-007: Unpublish News" as FR7
  rectangle "FR-008: Read/Filter News" as FR8
  rectangle "FR-009: Search Directory" as FR9
  rectangle "FR-010: Manage Category" as FR10
}

package "Use Cases (UC-NNN)" {
  rectangle "UC-001" as UC1
  rectangle "UC-002" as UC2
  rectangle "UC-003" as UC3
  rectangle "UC-004" as UC4
  rectangle "UC-005" as UC5
  rectangle "UC-006" as UC6
  rectangle "UC-007" as UC7
  rectangle "UC-008" as UC8
  rectangle "UC-009" as UC9
  rectangle "UC-010" as UC10
}

package "Acceptance Criteria" {
  rectangle "AC-001: Clock without help" as AC1
  rectangle "AC-002: Publish without help" as AC2
  rectangle "AC-003: Find colleague <10s" as AC3
  rectangle "AC-004: 80% clocking no training" as AC4
  rectangle "AC-005: Offline tolerance 5min" as AC5
}

BG1 --> FR1 : derives
BG1 --> FR2 : derives
BG1 --> FR3 : derives
BG1 --> FR4 : derives
BG2 --> FR1 : derives
BG2 --> FR9 : derives
BG2 --> FR10 : derives
BG3 --> FR5 : derives
BG3 --> FR6 : derives
BG3 --> FR7 : derives
BG3 --> FR8 : derives

FR1 --> UC1 : refines
FR2 --> UC2 : refines
FR3 --> UC3 : refines
FR4 --> UC4 : refines
FR5 --> UC5 : refines
FR6 --> UC6 : refines
FR7 --> UC7 : refines
FR8 --> UC8 : refines
FR9 --> UC9 : refines
FR10 --> UC10 : refines

UC1 --> AC1 : verifies
UC1 --> AC4 : verifies
UC1 --> AC5 : verifies
UC5 --> AC2 : verifies
UC9 --> AC3 : verifies

note bottom of BG1
  NFR-004 (audit trail) applies to
  UC-005, UC-006, UC-007, UC-010
end note

@enduml
```

### Traceability Table

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| UC-001 | FR-001, AC-005 | Refines | REQ-001, REL-003, REL-004, PERF-002, AC-001, AC-004, AC-005 |
| UC-002 | FR-002 | Refines | REQ-002 |
| UC-003 | FR-003 | Refines | REQ-003, PERF-005, REL-006 |
| UC-004 | FR-004 | Refines | REQ-004, PERF-004, STD-003 |
| UC-005 | FR-005, NFR-004 | Refines | REQ-005, AUD-001, AC-002 |
| UC-006 | FR-006, NFR-004 | Refines | REQ-006, AUD-001 |
| UC-007 | FR-007, CON-013, NFR-004 | Refines | REQ-007, AUD-001, AUD-003 |
| UC-008 | FR-008 | Refines | REQ-008, USA-001 |
| UC-009 | FR-009, CON-005, CON-012 | Refines | REQ-009, SEC-004, SEC-005, PERF-003, SUP-003, R001, AC-003 |
| UC-010 | FR-010, CON-009, NFR-004 | Refines | REQ-010, AUD-002, DC-006 |
| ACT-001 | STK-004 | Derives | UC-001, UC-002, UC-008, UC-009 |
| ACT-002 | STK-001 | Derives | UC-003..UC-007, UC-010 |
| ACT-003 | CON-005, CON-009 | Derives | UC-003, UC-009, UC-010 |
| ACT-004 | CON-004 | Derives | All UCs (auth) |
| UC-001..UC-004 | BG-001, BG-002 | Derives | (Business Goals) |
| UC-005..UC-008 | BG-003 | Derives | (Business Goals) |
| UC-009 | BG-002 | Derives | (Business Goals) |
| UC-009 | R001 | DependsOn | (LDAP attribute consistency) |
