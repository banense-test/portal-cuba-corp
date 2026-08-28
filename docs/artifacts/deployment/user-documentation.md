## Document Control
| Field | Value |
|---|---|
| Phase | Construction |
| Status | Draft |
| Milestone Target | End-of-Construction |
| Iteration | 2 (Cycle 1) |
| Date | 2026-08-28 |
| Prior Phase | Construction C1 (REQUEST_CHANGES — 1 Major, 4 Minor; IOC NOT achieved; stakeholder sanction REFUSED) |
| Author | Technical Writer (Deployment Discipline) |
| Audience | Employees (STK-004), HR Administrators (STK-001), Infrastructure team (STK-003) |
| Coverage | Install + Operate + Use + Maintain for Construction C2 build (UC-001 through UC-010) |
| C2 Evolution | CSV export format corrected per C2-MIN-4 (header: Employee,Date,Time,Direction). Featured news banner (CR-010) confirmed implemented and documented. Offline retry with idempotency (CR-011) confirmed implemented and documented. Directory office filter (CR-015) confirmed implemented and documented. |
| Styleguide | Terminological contract: "Clock In/Out" (not "punch" or "check-in"), "News item" (not "article" or "post"), "Worker category" (not "employee type" or "classification"), "Directory" (not "phonebook" or "address book"), "Unpublish" (not "hide" or "remove"). Active voice. Task-oriented headings. |
## Overview

Portal Cuba Corp is the employee portal for Cuba Corp — a single web application that centralizes clock in/out, HR news, and the employee directory into one place accessible from the corporate browser. It replaces shared Excel sheets, mass emails, and the outdated PDF phone directory.

### Who Uses This Guide

| Audience | Role | What You Need |
|---|---|---|
| Employee | Any of the 200 Cuba Corp employees across 3 offices | Clock in/out, view your clocking history, read news, search for colleagues |
| HR Administrator | HR staff with the HR role in Keycloak | View all clockings, export reports, publish/edit/unpublish news, manage worker categories |
| Infrastructure Administrator | Infrastructure team (STK-003) | Install, configure, and maintain the portal server |

### System Context

The following use case diagram shows what each user role can do in the portal:

```plantuml
@startuml
title Portal Cuba Corp — System Context (User Perspective)

left to right direction
skinparam packageStyle rectangle
skinparam actorStyle hollow

actor "Employee" as EMP
actor "HR Administrator" as HR

rectangle "Portal Cuba Corp — Employee Portal" {
  usecase "Clock In / Clock Out" as UC001
  usecase "View My Clocking History" as UC002
  usecase "View All Employee Clockings" as UC003
  usecase "Export Monthly Clocking Report (CSV)" as UC004
  usecase "Publish News" as UC005
  usecase "Edit Published News" as UC006
  usecase "Unpublish News" as UC007
  usecase "Read and Filter News" as UC008
  usecase "Search Employee Directory" as UC009
  usecase "Manage Worker Category" as UC010
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

note bottom of UC001
  Records exact time
  Shows confirmation
  Works offline (5 min retry)
end note

note bottom of UC005
  Title, body, date, category
  Featured banner option
  Audited: author + timestamp
end note

note bottom of UC009
  Search by name, department,
  or office
  Read-only from Active Directory
end note

@enduml
```

### Key Concepts

| Term | Definition |
|---|---|
| Clock In / Clock Out | Recording the start and end of your work session. The system captures the exact time automatically. |
| News item | An internal announcement published by HR. Each item has a title, body, date, and category. News items are never deleted — they can only be unpublished. |
| Category | A classification for news items: General, HR, IT, or Events. Employees can filter news by category. |
| Featured news | A news item marked as featured appears with a banner at the top of the news feed. |
| Worker category | A label assigned to an employee by HR, stored as a link between the employee's AD user ID and a category name. |
| Directory | The employee directory, searchable by name, department, or office. All data comes from Active Directory and is read-only in the portal. |
| Audit trail | A record of who performed an action and when. Every news publish, edit, unpublish, and worker category change is audited. |

## Getting Started

### Prerequisites

- A corporate computer with **Chrome** or **Edge** browser installed (CON-008)
- Access to the **corporate network** — the portal is not accessible from outside the office (CON-007)
- A **Keycloak account** with corporate credentials — your IT administrator sets this up
- For HR Administrators: the **HR role** must be assigned to your Keycloak account

### Logging In

1. Open your browser (Chrome or Edge)
2. Navigate to the portal URL provided by your IT administrator
3. You will be automatically redirected to the Keycloak login page
4. Enter your corporate username and password
5. After successful login, you are redirected back to the portal main page

> **Note:** The portal uses Keycloak for authentication. You do not create a separate account in the portal — your corporate credentials are all you need.

### The Main Page

After logging in, you see the main page with:

- **Clock In / Clock Out button** — at the top, showing the action available based on your current status
- **News feed** — sorted by date with featured news shown as a banner at the top
- **Navigation menu** — links to My Clockings, Employee Directory, and (for HR) HR management pages

## User Guide
### Clock In and Clock Out (UC-001)

The clock in/out feature records the exact time you start and end your work session. The button on the main page changes automatically based on your current status.

```plantuml
@startuml
title UC-001: Clock In / Clock Out — User Task Workflow

skinparam activityStyle rounded

start
:Open portal in browser\n(Chrome or Edge);
:Keycloak login redirect\n(automatic);
:Enter corporate credentials;

:Main page loads;
if (Currently clocked out?) then (yes)
  :See **Clock In** button;
else (no)
  :See **Clock Out** button;
endif

:Press Clock In/Out button;

if (Network available?) then (yes)
  :System records timestamp;
  :Show confirmation message\nwith date and time;
else (no — network dropped)
  :Browser stores clocking\nin local storage;
  :Retry automatically\nevery 10 seconds;
  if (Network restored within 5 min?) then (yes)
    :System records timestamp;
    :Show confirmation message;
    :Clear local storage;
  else (no — 5 min elapsed)
    :Show "Clocking failed —\ncontact HR" message;
    stop
  endif
endif

stop

@enduml
```

**To clock in or out:**

1. Open the portal in your browser
2. Look at the main page — you will see either a **Clock In** or **Clock Out** button depending on your current status
3. Click the button
4. A confirmation message appears showing the date and time of your clocking

> **Offline support:** If the network drops when you press the button, your clocking is saved in your browser and retried automatically every 10 seconds for up to 5 minutes. If the network comes back within that time, your clocking is recorded with the original timestamp. If the network is still down after 5 minutes, you will see a message to contact HR.

### View My Clocking History (UC-002)

1. Log in to the portal
2. Click **My Clockings** in the navigation menu
3. Your clocking history for the current month appears in a table showing date, clock in time, and clock out time

### View All Employee Clockings (UC-003 — HR Only)

1. Log in as an HR Administrator
2. Navigate to **All Clockings** in the HR management menu
3. A table appears showing all employees' clockings for the current month
4. Use the filter options to narrow results by employee or date range

### Export Monthly Clocking Report (UC-004 — HR Only)

The CSV export lets HR download a monthly clocking report for payroll or record-keeping.

```plantuml
@startuml
title UC-004: Export Monthly Clocking Report — HR Task Workflow (C2)

skinparam activityStyle rounded

start
:Log in as HR Administrator;
:Navigate to **All Clockings**;

:Select month and year;
:Optionally filter by employee;

:Click **Export CSV**;

:System queries clocking records\nfor selected period;
:System generates CSV with columns:\nEmployee, Date, Time, Direction;

:CSV file downloads to browser;
:Open in Excel or text editor;

note right
  CSV format (C2-MIN-4 fix):
  Header: Employee,Date,Time,Direction
  Each row = one clocking event
  Direction = "In" or "Out"
end note

stop

@enduml
```

**To export a monthly clocking report:**

1. Log in as an HR Administrator
2. Navigate to **All Clockings**
3. Select the month and year you want to export
4. Optionally filter by a specific employee
5. Click **Export CSV**
6. A CSV file downloads to your computer

> **CSV format:** The exported file contains four columns: **Employee** (employee name), **Date** (date of clocking), **Time** (time of clocking), and **Direction** (In or Out). Each row represents a single clocking event. You can open the file in Excel or any text editor.

### Publish News (UC-005 — HR Only)

HR publishes internal news and announcements with a title, body, date, and category. Publication is audited automatically.

```plantuml
@startuml
title UC-005: Publish News — HR Task Workflow

skinparam activityStyle rounded

start
:Log in as HR Administrator;
:Navigate to **News Management**;
:Click **New News Item**;

:Enter title;
:Enter body text;
:Select category\n(General, HR, IT, Events);
:Check **Featured** box\n(optional — shows banner);
:Set publication date;

:Click **Publish**;

:System saves news item;
:System records audit trail\n(author + timestamp);
:News item appears on main page;

if (Featured?) then (yes)
  :News appears with banner\nat top of news feed;
else (no)
  :News appears in date-sorted\nnews feed;
endif

stop

@enduml
```

**To publish a news item:**

1. Log in as an HR Administrator
2. Navigate to **News Management**
3. Click **New News Item**
4. Enter the title, body text, and select a category (General, HR, IT, or Events)
5. Check the **Featured** box if you want the news to appear with a banner at the top of the news feed
6. Set the publication date
7. Click **Publish**
8. The news item appears on the main page immediately

> **Audit trail:** Every publication is recorded with the HR administrator's identity and the exact timestamp. This audit record cannot be deleted.

> **Featured news:** Checking the Featured box when publishing causes the news item to appear with a prominent banner at the top of the employee news feed. This is useful for important announcements.

### Edit Published News (UC-006 — HR Only)

1. Log in as an HR Administrator
2. Navigate to **News Management**
3. Find the news item you want to edit and click **Edit**
4. Update the title, body, or category as needed
5. Click **Save**
6. The changes take effect immediately on the main page

> **Audit trail:** Every edit is recorded with the HR administrator's identity and the exact timestamp — exactly like the original publication. A typo fix does not require unpublishing and republishing.

### Unpublish News (UC-007 — HR Only)

Unpublishing hides a news item from employees while preserving the record for the audit trail. News items are never deleted.

```plantuml
@startuml
title UC-007: Unpublish News — HR Task Workflow

skinparam activityStyle rounded

start
:Log in as HR Administrator;
:Navigate to **News Management**;
:Find the news item;
:Click **Unpublish**;

:System hides news item\nfrom employee view;
:System records audit trail\n(author + timestamp);
:Show confirmation:\n"News item unpublished";

note right
  The news record stays
  in the database for
  traceability (CON-013).
  It can be re-published
  later if needed.
end note

stop

@enduml
```

**To unpublish a news item:**

1. Log in as an HR Administrator
2. Navigate to **News Management**
3. Find the news item and click **Unpublish**
4. A confirmation message appears: "News item unpublished"
5. The news item is hidden from employees but the record is preserved

> **Never deleted:** Unpublishing hides the news item — it does not delete it. The record stays for the audit trail (CON-013). You can re-publish the item later if needed.

#### News Item Lifecycle

The following state machine shows the lifecycle of a news item from the user's perspective:

```plantuml
@startuml
title News Item Lifecycle — State Machine (User Perspective)

skinparam stateStyle rounded

[*] --> Draft : HR creates news

Draft --> Published : HR publishes\n(audit: author + timestamp)

Published --> Published : HR edits\n(audit: author + timestamp)

Published --> Unpublished : HR unpublishes\n(audit: author + timestamp)

Unpublished --> Published : HR re-publishes\n(audit: author + timestamp)

note right of Unpublished
  News items are never deleted.
  Unpublishing hides the item
  while preserving the record
  for the audit trail (CON-013).
end note

@enduml
```

### Read and Filter News (UC-008)

Employees see news on the main page sorted by date. Featured news appears with a banner at the top.

```plantuml
@startuml
title UC-008: Read and Filter News — User Task Workflow

skinparam activityStyle rounded

start
:Log in to portal;
:Main page loads with news feed;

if (Featured news exists?) then (yes)
  :Featured banner appears\nat top of news feed;
else (no)
  :News feed shows all items\nsorted by date;
endif

if (Want to filter by category?) then (yes)
  :Click category filter\n(General, HR, IT, Events);
  :News feed updates to show\nonly selected category;
else (no)
  :Browse all news items;
endif

:Click news item to read full text;

stop

@enduml
```

**To read and filter news:**

1. Log in to the portal — the main page shows the news feed
2. If there is featured news, it appears with a banner at the top
3. To filter by category, click one of the category filters: **General**, **HR**, **IT**, or **Events**
4. The news feed updates to show only items in the selected category
5. Click any news item to read its full text

> **Read-only:** Employees can read news but cannot comment, react, or publish. Only HR Administrators can publish, edit, and unpublish news.

### Search Employee Directory (UC-009)

The directory lets you find colleagues by name, department, or office. All data comes from Active Directory and is read-only.

```plantuml
@startuml
title UC-009: Search Employee Directory — User Task Workflow

skinparam activityStyle rounded

start
:Log in to portal;
:Click **Directory** in navigation;

:Enter search term\n(name, department, or office);
:Optionally filter by office;

:System queries Active Directory\nover LDAP;
:Results appear showing:\nname, job title, department,\noffice, email, extension;

if (Fields missing in AD?) then (yes)
  :Missing fields show "N/A";
  :Contact Infrastructure team\nto update AD;
else (no)
  :All fields populated;
endif

stop

@enduml
```

**To search for a colleague:**

1. Log in to the portal
2. Click **Directory** in the navigation menu
3. Enter a search term — a name, department, or office
4. Optionally filter by office to narrow results
5. Results appear showing: name, job title, department, office, email, and extension phone number
6. If any field shows "N/A", the information is missing in Active Directory — contact the Infrastructure team to update it

> **Read-only from Active Directory:** The directory displays corporate data only (name, job title, department, office, email, extension). No private personal information is shown (CON-012). All data comes from Active Directory and cannot be edited in the portal (CON-010).

> **Performance target:** You should be able to find a colleague's phone or email in under 10 seconds (AC-003).

### Manage Worker Category (UC-010 — HR Only)

HR manages worker categories by linking an employee's AD user ID to a category. The portal stores only this link — all other employee data is read from Active Directory at view time.

```plantuml
@startuml
title UC-010: Manage Worker Category — HR Task Workflow

skinparam activityStyle rounded

start
:Log in as HR Administrator;
:Navigate to **Worker Categories**;

if (Searching for employee?) then (yes)
  :Enter name or AD user ID;
  :System queries Active Directory\nfor matching employees;
  :Select employee from results;
else (browsing all)
  :Browse existing category list;
endif

:Select or change worker category;
:Click **Save**;

:System saves AD user ID to category link;
:System records audit trail\n(author + timestamp);
:Show confirmation:\n"Category updated";

note right
  Only AD user ID and category
  are stored locally.
  All other employee data is
  read from AD at view time.
end note

stop

@enduml
```

**To manage worker categories:**

1. Log in as an HR Administrator
2. Navigate to **Worker Categories**
3. Search for an employee by name or AD user ID, or browse the existing list
4. Select or change the worker category for the employee
5. Click **Save**
6. A confirmation message appears: "Category updated"

> **Audit trail:** Every category change is recorded with the HR administrator's identity and timestamp. The portal stores only the AD user ID and the category — all other employee data is read from Active Directory at view time.
## Operations Guide

### Installation Topology

The following diagram shows what is installed where:

```plantuml
@startuml
title Portal Cuba Corp — Installation Topology

skinparam nodeStyle rounded

node "Client Browser\n(Chrome / Edge)" as CLIENT {
  artifact "Razor Pages\n(server-rendered HTML)" as RP
  artifact "clocking-retry.js\n(offline retry script)" as JS
}

node "Windows Server\n(Internal — Corporate Network)" as WINSERV {
  artifact ".NET 10 Application\n(PortalCubaCorp)" as APP
  artifact "PostgreSQL Database" as DB
}

node "Keycloak Server\n(External — already running)" as KCSERV {
  artifact "Keycloak\nOIDC Provider" as KC
}

node "Active Directory\n(External — already running)" as ADSERV {
  artifact "AD / LDAP\nDirectory" as AD
}

CLIENT --> WINSERV : HTTPS\n(corporate network only)
APP --> KCSERV : OIDC\n(redirect + token validation)
APP --> ADSERV : LDAP\n(read-only)
APP --> DB : local TCP\n(EF Core + Npgsql)

note bottom of WINSERV
  Prerequisites:
  - .NET 10 Runtime
  - PostgreSQL 16+
  - OIDC client registered in Keycloak
  - LDAP bind account in AD
  - Corporate network access
end note

@enduml
```

### Installation Prerequisites

The portal runs on a single internal Windows Server. The following must be in place before installation:

| Prerequisite | Details | Owner |
|---|---|---|
| Windows Server | Internal server accessible from the corporate network (CON-006) | Infrastructure team (STK-003) |
| .NET 10 Runtime | Required to run the ASP.NET 10 application (CON-001) | Infrastructure team |
| PostgreSQL 16+ | Database server running on the same Windows Server (CON-003) | Infrastructure team |
| Keycloak OIDC client | A client must be registered in the existing Keycloak instance with redirect URIs configured for the portal (CON-004) | Infrastructure team |
| LDAP bind account | An Active Directory service account with read-only LDAP access for querying employee attributes (CON-005, CON-010) | Infrastructure team |
| Corporate network access | The server must be reachable from all 3 offices via the corporate network (CON-007) | Infrastructure team |

### Installation Steps

1. **Install PostgreSQL** on the Windows Server if not already present
2. **Create a database** for the portal (e.g., `portal_cuba_corp`)
3. **Install .NET 10 Runtime** on the Windows Server
4. **Deploy the application** files to the server (e.g., `C:\inetpub\PortalCubaCorp` or a custom directory)
5. **Configure the application** by editing `appsettings.json` with the following parameters:

| Parameter | Description | Example |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Database=portal_cuba_corp;Username=portal_user;Password=****` |
| `Keycloak:Authority` | Keycloak realm URL | `https://keycloak.cubacorp.local/realms/cuba-corp` |
| `Keycloak:ClientId` | OIDC client ID registered in Keycloak | `portal-cuba-corp` |
| `Keycloak:ClientSecret` | OIDC client secret | (provided by Infrastructure team) |
| `Ldap:Host` | Active Directory server hostname | `ad.cubacorp.local` |
| `Ldap:Port` | LDAP port (389 for non-SSL, 636 for SSL) | `389` |
| `Ldap:BindDn` | LDAP bind account distinguished name | `CN=portal-readonly,OU=ServiceAccounts,DC=cubacorp,DC=local` |
| `Ldap:BindPassword` | LDAP bind account password | (provided by Infrastructure team) |
| `Ldap:SearchBase` | Base DN for employee searches | `DC=cubacorp,DC=local` |

6. **Run database migrations** to create the schema: `dotnet ef database update` (or the equivalent migration command for your deployment process)
7. **Configure IIS or Kestrel** as the web server to serve the application on the designated port
8. **Verify the OIDC redirect URI** in Keycloak points to the portal's HTTPS URL
9. **Test LDAP connectivity** from the server to Active Directory
10. **Start the application** and verify the login page loads

### Post-Installation Verification

| Check | Expected Result |
|---|---|
| Navigate to portal URL in Chrome | Keycloak login page appears |
| Log in with corporate credentials | Main page loads in under 3 seconds (NFR-001) |
| Press Clock In button | Confirmation appears in under 1 second (NFR-002) |
| Search employee directory | Results appear with colleague details |
| Publish a test news item (HR) | News appears on main page |
| Export clocking report (HR) | CSV file downloads successfully |

### Configuration Reference

#### Database Tables

The portal uses four PostgreSQL tables:

| Table | Purpose | Key Columns |
|---|---|---|
| `clockings` | Employee clock in/out records | id, employee_id, timestamp, type (in/out), idempotency_key |
| `news_items` | News articles | id, title, body, category, is_featured, status (draft/published/unpublished), author_id, published_at |
| `worker_categories` | AD user ID to category mapping | ad_user_id, category |
| `audit_records` | Audit trail for all audited actions | id, entity_type, entity_id, action, author_id, timestamp |

#### Operational Parameters

| Parameter | Value | Source |
|---|---|---|
| Operating hours | Monday–Friday 7:00–19:00 | NFR-003 |
| Page load target | Under 3 seconds on corporate network | NFR-001 |
| Clock in/out response target | Under 1 second | NFR-002 |
| Offline retry window | 5 minutes (clocking only) | AC-005 |
| Offline retry interval | Every 10 seconds | AC-005 |
| Concurrent users | ~200 (single server, no scaling needed) | CON-006 |
| Browser support | Chrome and Edge (current versions) | CON-008 |

### Monitoring and Maintenance

#### Routine Checks

| Task | Frequency | What to Check |
|---|---|---|
| Verify portal is accessible | Daily (before 7:00) | Login page loads, authentication works |
| Check PostgreSQL service | Daily | Service is running, disk space is adequate |
| Review audit logs | Weekly | Audit records are being written for news and category changes |
| Verify LDAP connectivity | Weekly | Directory search returns results from all 3 offices |
| Check Keycloak client status | Monthly | OIDC client is active, tokens are validating |
| Review disk space | Monthly | PostgreSQL logs and data have adequate space |

#### Troubleshooting

| Symptom | Likely Cause | Resolution |
|---|---|---|
| Login page does not appear | Application not running or IIS/Kestrel misconfigured | Check application process, check server logs |
| Login fails | Keycloak OIDC client misconfigured or Keycloak server down | Verify Keycloak is running, check client redirect URIs |
| Directory search returns "N/A" for some fields | AD attributes not filled for some employees (R001) | Contact Infrastructure team to update AD attributes |
| Directory search returns no results | LDAP bind account issue or network problem | Verify LDAP credentials, check network path to AD server |
| Clocking fails after 5 minutes | Network outage lasting more than 5 minutes | Employee should contact HR to record clocking manually |
| News not appearing on main page | News item may be unpublished or in draft status | HR should check News Management page |
| CSV export is empty | No clocking data for the selected period | Verify date range, verify employees have clocked in |
| Page load is slow | PostgreSQL needs maintenance or server resources are low | Run VACUUM ANALYZE on database, check server CPU/memory |

#### Backup

- **PostgreSQL database:** Back up daily using `pg_dump` or PostgreSQL's built-in backup tools. The database contains clockings, news items, worker categories, and audit records — all portal data.
- **Application configuration:** Back up `appsettings.json` after any configuration change.
- **Active Directory and Keycloak:** These are external systems maintained by the Infrastructure team (STK-003). The portal does not back them up.

## FAQ and Support

### Frequently Asked Questions

**Q: I forgot to clock in this morning. What should I do?**
A: Contact your HR administrator. They can view all clockings and help record a manual entry if needed.

**Q: The network went down when I tried to clock out. Did my clocking register?**
A: If the network comes back within 5 minutes, your clocking is automatically sent with the original timestamp. If the network is still down after 5 minutes, you will see a message to contact HR.

**Q: I searched for a colleague in the directory but some fields show "N/A". Why?**
A: The directory reads data from Active Directory. If a field (like job title or extension) is not filled in AD, it shows as "N/A" in the portal. Contact the Infrastructure team to update the missing information in Active Directory — the portal cannot edit AD data.

**Q: Can I delete a news item I published by mistake?**
A: No. News items are never deleted. You can **unpublish** the item, which hides it from employees but preserves the record for the audit trail. You can also edit the content to correct any mistakes.

**Q: How do I get HR administrator access?**
A: The HR role is assigned in Keycloak by the Infrastructure team. Contact them to request the HR role for your account.

**Q: Can I access the portal from home?**
A: No. The portal is only accessible from within the corporate network (CON-007). You need to be connected to the office network.

**Q: What browsers are supported?**
A: The portal supports current versions of Chrome and Edge (CON-008).

**Q: I see a "Clocking failed — contact HR" message. What happened?**
A: The network was down for more than 5 minutes when you tried to clock in or out. The system could not send your clocking within the retry window. Contact HR to record your clocking manually.

### Getting Help

| Issue Type | Contact |
|---|---|
| Cannot log in | Infrastructure team (STK-003) — Keycloak/account issues |
| Directory shows wrong information | Infrastructure team (STK-003) — AD data must be corrected in Active Directory |
| Clocking problems | HR department (STK-001) |
| News publishing issues | HR department (STK-001) |
| Portal is down or slow | Infrastructure team (STK-003) |
| Feature requests or bugs | HR Director (STK-001) or Software Engineer (STK-002) |

## Traceability

| Element | Traces From | Link Type | Traces To |
|---|---|---|---|
| User Documentation | All UCs, SAD Deployment View, Design Model | Derives | End users (STK-004), HR (STK-001), Infrastructure (STK-003) |
| Clock In/Out task (UC-001) | FR-001, AC-001, AC-005, NFR-002 | Refines | Activity diagram: UC-001 workflow |
| View My Clockings task (UC-002) | FR-002 | Refines | Procedural steps |
| View All Clockings task (UC-003) | FR-003 | Refines | Procedural steps |
| Export CSV task (UC-004) | FR-004 | Refines | Activity diagram: UC-004 workflow |
| Publish News task (UC-005) | FR-005, NFR-004, AC-002 | Refines | Activity diagram: UC-005 workflow |
| Edit News task (UC-006) | FR-006, NFR-004 | Refines | Procedural steps |
| Unpublish News task (UC-007) | FR-007, CON-013, NFR-004 | Refines | Activity diagram: UC-007 workflow, State machine: News lifecycle |
| Read and Filter News task (UC-008) | FR-008 | Refines | Activity diagram: UC-008 workflow |
| Search Directory task (UC-009) | FR-009, CON-005, CON-012, AC-003, R001 | Refines | Activity diagram: UC-009 workflow |
| Manage Worker Category task (UC-010) | FR-010, CON-009, NFR-004 | Refines | Activity diagram: UC-010 workflow |
| Installation Guide | SAD Deployment View, CON-001..CON-008 | Derives | Infrastructure team (STK-003) |
| Configuration Reference | SAD Logical View, SAD Implementation View | Derives | Operations procedures |
| Troubleshooting | R001, AC-005, NFR-001..NFR-003 | Derives | Support procedures |
| FAQ | AC-001..AC-005, CON-007, CON-008, CON-013 | Derives | End-user support |
| Terminology (Styleguide) | All FRs, all UCs | Refines | All documentation sections |