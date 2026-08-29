# Portal Cuba Corp — Rollback Script
# Reverts the deployment to a previous backup version.
#
# Usage: .\rollback.ps1 -BackupDir <path>
# Or:    .\rollback.ps1  (uses the most recent backup)

param(
    [string]$BackupDir,
    [string]$DeployPath = "C:\inetpub\portal-cuba-corp"
)

$ErrorActionPreference = "Stop"

function Write-Log {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
}

$BackupPath = "C:\inetpub\portal-cuba-corp-backups"

# Find the most recent backup if not specified
if (-not $BackupDir) {
    if (-not (Test-Path $BackupPath)) {
        Write-Log "ERROR: No backup directory found at $BackupPath"
        exit 1
    }
    $latestBackup = Get-ChildItem -Path $BackupPath -Directory | Sort-Object Name -Descending | Select-Object -First 1
    if ($null -eq $latestBackup) {
        Write-Log "ERROR: No backups found in $BackupPath"
        exit 1
    }
    $BackupDir = $latestBackup.FullName
    Write-Log "Using most recent backup: $BackupDir"
}

if (-not (Test-Path $BackupDir) {
    Write-Log "ERROR: Backup directory not found: $BackupDir"
    exit 1
}

Write-Log "Starting rollback from $BackupDir to $DeployPath"

# Stop IIS app pool if running
$iisAppPool = Get-IISAppPool -Name "portal-cuba-corp" -ErrorAction SilentlyContinue
if ($null -ne $iisAppPool) {
    Write-Log "Stopping IIS app pool: portal-cuba-corp"
    Stop-IISAppPool -Name "portal-cuba-corp"
}

# Preserve current appsettings
$prodAppSettings = $null
$appSettingsFile = Join-Path $DeployPath "appsettings.Production.json"
if (Test-Path $appSettingsFile) {
    $prodAppSettings = Get-Content $appSettingsFile -Raw
    Write-Log "Preserving current production appsettings.json"
}

# Clear current deployment
if (Test-Path $DeployPath) {
    Write-Log "Clearing current deployment"
    Remove-Item -Path "$DeployPath\*" -Recurse -Force
}

# Restore from backup
Write-Log "Restoring from backup"
Copy-Item -Path "$BackupDir\*" -Destination $DeployPath -Recurse -Force

# Restore production appsettings
if ($null -ne $prodAppSettings) {
    Set-Content -Path $appSettingsFile -Value $prodAppSettings
    Write-Log "Restored production appsettings.json"
}

# Start IIS app pool
if ($null -ne $iisAppPool) {
    Write-Log "Starting IIS app pool: portal-cuba-corp"
    Start-IISAppPool -Name "portal-cuba-corp"
}

# Verify
Start-Sleep -Seconds 5
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/" -UseBasicParsing -TimeoutSec 10
    Write-Log "Application is responding after rollback. Status: $($response.StatusCode)"
} catch {
    Write-Log "WARNING: Application not responding after rollback. Manual verification required."
}

Write-Log "Rollback completed."