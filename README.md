# Portal Cuba Corp

## Quick Start Guide (5 steps)

### Vision

Employee Portal is an internal web application for the company Cuba Corp (200 employees, 3 offices). It lets employees clock their entry and exit times, read news published by HR, and look up colleagues in the corporate directory. Today these processes are handled with shared Excel sheets, mass emails, and an outdated PDF with phone numbers. The goal is to centralize everything in a single web application accessible from the corporate browser.

## Objectives
- Reduce HR management time by 50%.
- Eliminate 100% of the Excel usage.
- 80% employee adoption within 3 months.

### Stakeholders

1. **Laura Gómez**
   - Role: HR Director (project sponsor)
   - Influence: High

2. **Miguel Torres**
   - Role: Software Engineer (He will not create the system, he will only be the one to clarify engineering-related doubts for the technical roles)
   - Influence: High

3. **Infrastructure team**
   - Role: operates Active Directory and Keycloak — both external systems the portal depends on
   - Influence: High (a change on their side breaks the portal)
   - Interest: Low in the portal's features; they care about not being asked to modify AD or take on new operational work
   - Concerns: the LDAP attributes we read must be populated consistently across the 3 offices; the OIDC client registration must exist before login can be tested

4. **Cuba Corp Employees**
   - Role: End users (200 people, 3 offices)
   - Influence: Medium

### Requirements

**UC01 — Clock In/Out**
The employee logs in with their corporate credentials (Active Directory). On the main screen they see a "Clock In" or "Clock Out" button depending on their current status. When pressed, the system records the exact time and shows a confirmation. The employee can view their clocking history for the current month. HR can view all employees' clockings and export a monthly report in CSV.

**UC02 — Read News**
HR publishes internal news and announcements (title, body, date, category). Employees see the news on the main page sorted by date. They can filter by category (General, HR, IT, Events). Featured news appears with a banner at the top. There are no comments or reactions — it is read-only for employees.

HR can EDIT a news item after publishing — a typo should not force a republish — and **every edit is audited exactly like the original publication** (who and when); otherwise the trail records only the final version of something that said something else when people read it. HR can UNPUBLISH an item, which **hides it and never deletes it**: deleting would destroy the audit trail. There is **no archive**: news are listed newest first and the category filter above is enough — with 200 employees there is no volume that justifies an archive screen, so do not build one.

**UC03 — Employee Directory**
The employee searches for colleagues by name, department, or office. Each entry shows: name, job title, department, office, email, and extension phone number. All of those fields come from Active Directory, read over LDAP, and are **READ-ONLY in the portal**. There is no edit form for them and no local copy: to change a job title or an extension you change it in AD. AD is operated by **Infrastructure**, not by the team building this portal.

The ONE thing the portal owns and HR manages is the **worker category**. It is stored as a link — **AD user id → category** — never as a duplicate of the employee. With the id the portal goes to AD and reads the rest. So the local table holds two columns and nothing else, and there is no synchronisation, no reconciliation and no conflict to resolve: the employee data has exactly one home.

The directory does not show private personal information (corporate data only).

## NFR / FURPS+:
- Performance: the page must load in under 3 seconds on the corporate network.
  Clock in/out operation must respond in under 1 second.
- Availability: extended working hours Monday–Friday 7:00–19:00,
  with fault tolerance within the corporate network. 24/7 is not required.
- Auditing: yes, mandatory traceability of who publishes each news item, who EDITS it and
  who unpublishes it (author + timestamp in every case), and of any change to a worker's
  category. Employee fields are read-only from AD, so there is nothing to audit there.
- Authentication: the portal is an OIDC client of the existing Keycloak, which federates Active Directory (already mentioned in constraints — Keycloak is not ours to deploy).
- Compatibility: corporate browsers, current Chrome and Edge.
- Access: only from the internal corporate network.

### Risks
1. **Active Directory integration**: the LDAP attributes the directory reads may not be filled consistently across the 3 offices (job title, extension). If not tested early the directory shows gaps (Medium / Medium).
2. **Digital clocking adoption**: Some employees may keep using Excel out of habit if the change is not communicated well (Medium / Low).

### Constraints
- Backend: .NET 10, REST API
- Frontend: Razor Pages (intranet, no SPA needed)
- Database: PostgreSQL
- Authentication: Keycloak is ALREADY RUNNING and is maintained separately — it is NOT part of this project. Do not deploy it, do not provision it, do not design its infrastructure, do not plan work for it. The portal is an OIDC **client**: register a client, redirect for login, validate the token, read roles from its claims. Nothing more.
- The employee directory is read DIRECTLY from Active Directory over LDAP. Keycloak is authentication and authorization only — it is not a directory to query. Corporate attributes (job title, department, office, email, extension) live in AD and AD is their system of record.
- Hosting: internal Windows Server (no cloud)
- No access from outside the corporate network
- Compatible with current Chrome and Edge
- Employee data is READ from AD on demand, never copied into the portal's database. There is no sync job and nothing to reconcile — the portal stores only `AD user id → worker category`. Everything else is projected from AD at read time.
- Active Directory is operated by the Infrastructure team. This project neither administers it nor writes to it.
- The custom design at docs/inputs/employee-portal-design.html is MANDATORY and authoritative for the UI visual layer, not only for its structure. The portal MUST implement it.

## Scope OUT:
- No native mobile app (responsive web only).
- No push notifications.
- No integration with the payroll system.
- No vacation or sick-leave management (separate system).
- No biometric clocking (AD username/password only).
- **No Keycloak work of any kind**: it is already deployed, already federated to AD, and maintained by someone else. No realm design, no client provisioning scripts, no Keycloak hosting, no Keycloak in the deployment diagram as something we install. It is an external system we consume.
- No writing back to Active Directory, and no editing of employee fields anywhere in the portal. AD is operated by Infrastructure; a wrong job title is fixed there, not here.
- No local copy of the employee. No sync job, no reconciliation screen, no conflict resolution — the portal stores `AD user id → worker category` and nothing else about a person.
- No news archive screen. Newest-first plus the category filter is the whole navigation.
- No hard delete of a news item. Unpublish hides it; the record stays for the audit trail.

## Acceptance criteria:
- An employee can clock in and out without help from HR or the development team.
- An HR Administrator can publish a news item without technical assistance.
- Any employee finds a colleague's phone/email in under 10 seconds.
- 80% of employees complete at least one clocking with no prior training.
- The system works temporarily offline: if the network drops for 5 minutes, the data syncs once it is back.
End of spec.
