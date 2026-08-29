# Portal Cuba Corp — Production Deployment Script
# Target: Internal Windows Server (CON-006)
# Prerequisites: .NET 10 Runtime, PostgreSQL, IIS or Kestrel
#
# Usage: .\deploy.ps1 -Version <version> [-Rollback]
#
# This script deploys the Portal Cuba Corp application to the internal
# Windows Server. It supports both initial deployment and rollback.

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [string]$DeployPath = "C:\inetpub\portal-cuba-corp",
    [string]$BackupPath = "C:\inetpub\portal-cuba-corp-backups",
    [string]$AppSettingsPath = "C:\inetpub\portal-cuba-corp\appsettings.Production.json",
    [switch]$Rollback
)

$ErrorActionPreference = "Stop"

function Write-Log {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
}

# --- Pre-flight checks ---
Write-Log "Starting deployment of Portal Cuba Corp v$Version"

# Check .NET 10 runtime
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Log "ERROR: .NET SDK not found. Install .NET 10 SDK."
    exit 1
}
Write-Log ".NET SDK version: $dotnetVersion"

# Check PostgreSQL connectivity
$pgIsReady = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
if ($null -eq $pgIsReady) {
    Write-Log "WARNING: PostgreSQL service not found. Ensure PostgreSQL is installed and running."
} else {
    Write-Log "PostgreSQL service status: $($pgIsReady.Status)"
}

# --- Backup current deployment ---
if (Test-Path $DeployPath) {
    $backupDir = Join-Path $BackupPath (Get-Date -Format 'yyyyMMdd_HHmmss')
    Write-Log "Backing up current deployment to $backupDir"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Copy-Item -Path "$DeployPath\*" -Destination $backupDir -Recurse -Force
    Write-Log "Backup completed."
} else {
    Write-Log "No existing deployment found at $DeployPath. Fresh install."
}

# --- Publish application ---
$publishOutput = Join-Path $env:TEMP "portal-cuba-corp-publish-$Version"
Write-Log "Publishing application to $publishOutput"

dotnet publish src/PortalCubaCorp/PortalCubaCorp.csproj `
    -c Release `
    -o $publishOutput `
    --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Log "ERROR: Publish failed."
    exit 1
}
Write-Log "Publish completed successfully."

# --- Stop application (if running under IIS) ---
$iisAppPool = Get-IISAppPool -Name "portal-cuba-corp" -ErrorAction SilentlyContinue
if ($null -ne $iisAppPool) {
    Write-Log "Stopping IIS app pool: portal-cuba-corp"
    Stop-IISAppPool -Name "portal-cuba-corp"
}

# --- Deploy new version ---
Write-Log "Deploying to $DeployPath"
New-Item -ItemType Directory -Path $DeployPath -Force | Out-Null

# Preserve production appsettings if it exists
$prodAppSettings = $null
if (Test-Path $AppSettingsPath) {
    $prodAppSettings = Get-Content $AppSettingsPath -Raw
    Write-Log "Preserving production appsettings.json"
}

# Copy published files
Copy-Item -Path "$publishOutput\*" -Destination $DeployPath -Recurse -Force

# Restore production appsettings
if ($null -ne $prodAppSettings) {
    Set-Content -Path $AppSettingsPath -Value $prodAppSettings
    Write-Log "Restored production appsettings.json"
}

# --- Start application ---
if ($null -ne $iisAppPool) {
    Write-Log "Starting IIS app pool: portal-cuba-corp"
    Start-IISAppPool -Name "portal-cuba-corp"
}

# --- Cleanup temp publish ---
Remove-Item -Path $publishOutput -Recurse -Force -ErrorAction SilentlyContinue

# --- Post-deployment verification ---
Write-Log "Post-deployment verification..."
Start-Sleep -Seconds 5

# Check if the application responds
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/" -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 302) {
        Write-Log "Application is responding. Status: $($response.StatusCode)"
    } else {
        Write-Log "WARNING: Application returned status $($response.StatusCode)"
    }
} catch {
    Write-Log "WARNING: Application not responding yet. May need manual verification."
}

Write-Log "Deployment of Portal Cuba Corp v$Version completed."
Write-Log "Backup location: $backupDir (if applicable)"
Write-Log "To rollback: .\rollback.ps1 -BackupDir $backupDir"