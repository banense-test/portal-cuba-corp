# Portal Cuba Corp — Deployment Guide

## Prerequisites

- Windows Server 2019+ (CON-006)
- .NET 10 SDK or Runtime
- PostgreSQL 15+ (CON-003)
- IIS with ASP.NET Core Module v2 (or Kestrel as standalone)
- Keycloak OIDC client registered (CON-004, STK-003 prerequisite)
- Active Directory LDAP connectivity (CON-005)
- Chrome or Edge browser on client machines (CON-008)

## Deployment Steps

### 1. Publish the Application

```powershell
# From the repository root on the build machine:
dotnet publish src/PortalCubaCorp/PortalCubaCorp.csproj -c Release -o C:\publish
```

### 2. Deploy to Windows Server

```powershell
# On the target Windows Server:
.\deploy\deploy.ps1 -Version 1.0.0
```

This script will:
1. Verify .NET 10 and PostgreSQL are available
2. Back up the current deployment (if exists)
3. Publish the application
4. Stop the IIS app pool
5. Copy new files to the deployment path
6. Preserve production appsettings.json
7. Start the IIS app pool
8. Verify the application is responding

### 3. Configure appsettings.Production.json

Ensure the production `appsettings.json` has the correct values for:
- PostgreSQL connection string
- Keycloak OIDC settings (Authority, ClientId, ClientSecret)
- LDAP settings (Host, Port, BindDn, BindPassword, SearchBase)

### 4. Verify Deployment

- Navigate to the portal URL in Chrome/Edge
- Verify Keycloak login redirect works
- Verify clock in/out functionality
- Verify directory search returns AD results
- Verify news publishing works for HR role

## Rollback Procedure

If the deployment fails or causes issues:

```powershell
# Rollback to the most recent backup:
.\deploy\rollback.ps1

# Or rollback to a specific backup:
.\deploy\rollback.ps1 -BackupDir C:\inetpub\portal-cuba-corp-backups\20260829_140000
```

### Rollback Criteria (per DEPLOYMENT_STRATEGY.md)

- Portal unavailable >30 min during business hours (NFR-003)
- Clocking data corruption or loss
- Keycloak OIDC integration failure
- AD LDAP connectivity failure

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/deploy.yml`) automates:
1. Build and test on every push to main
2. Publish the application on version tags (v*)
3. Create a deployment artifact
4. Manual deployment to the Windows Server using deploy.ps1

## Offline Behavior (AC-005)

- Clocking POST retried client-side via localStorage for up to 5 minutes
- Server accepts client timestamp, rejects duplicates by idempotency key
- Directory and news show "no connection" when offline
- No PWA, no service worker, no client cache of other data

## Security Notes

- No access from outside the corporate network (CON-007)
- OIDC authentication via Keycloak (CON-004)
- LDAP queries are read-only (CON-010)
- No employee data stored locally except AD user id → worker category (CON-009)
- News items are never hard-deleted (CON-013)