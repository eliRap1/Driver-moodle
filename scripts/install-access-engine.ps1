#Requires -RunAsAdministrator
<#
.SYNOPSIS
  Installs Microsoft Access Database Engine 2016 so the WCF service can talk to .accdb files.

.DESCRIPTION
  Detects OS bitness, downloads the matching installer from microsoft.com, runs it silently with /quiet.
  Required because the OleDb provider (Microsoft.ACE.OLEDB.12.0 / 16.0) is NOT bundled with Windows or .NET.

.NOTES
  Must be run from an elevated PowerShell prompt.
  After install: fully close and reopen Visual Studio before rebuilding.
#>

$ErrorActionPreference = 'Stop'

Write-Host '==================================================' -ForegroundColor Cyan
Write-Host '  Microsoft Access Database Engine 2016 installer  ' -ForegroundColor Cyan
Write-Host '==================================================' -ForegroundColor Cyan

# Confirm elevation
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host 'ERROR: this script must run as Administrator.' -ForegroundColor Red
    Write-Host '       Right-click PowerShell -> Run as administrator, then rerun.' -ForegroundColor Red
    exit 1
}

# Pick bitness. Default to x64 on 64-bit Windows; allow override via $env:ACE_BITNESS = 'x86'
$override = $env:ACE_BITNESS
if ($override -in 'x86', 'x64') {
    $arch = $override
    Write-Host "Using override ACE_BITNESS=$arch"
}
else {
    $arch = if ([Environment]::Is64BitOperatingSystem) { 'x64' } else { 'x86' }
    Write-Host "Detected OS architecture: $arch  (override with: \$env:ACE_BITNESS = 'x86')"
}

$urls = @{
    'x64' = 'https://download.microsoft.com/download/3/5/C/35C84C36-661A-44E6-9324-8786B8DBE231/accessdatabaseengine_X64.exe'
    'x86' = 'https://download.microsoft.com/download/3/5/C/35C84C36-661A-44E6-9324-8786B8DBE231/accessdatabaseengine.exe'
}

$url = $urls[$arch]
$tmpFile = Join-Path $env:TEMP "AccessDatabaseEngine_$arch.exe"

Write-Host "Downloading: $url" -ForegroundColor Yellow
Invoke-WebRequest -Uri $url -OutFile $tmpFile -UseBasicParsing

Write-Host 'Running installer silently (/quiet)...' -ForegroundColor Yellow
$proc = Start-Process -FilePath $tmpFile -ArgumentList '/quiet' -Wait -PassThru

if ($proc.ExitCode -eq 0) {
    Write-Host 'SUCCESS. Access Database Engine installed.' -ForegroundColor Green
}
elseif ($proc.ExitCode -eq 1638) {
    Write-Host 'Another version is already installed (exit 1638). Probably fine.' -ForegroundColor Yellow
}
else {
    Write-Host "Installer returned exit code $($proc.ExitCode)." -ForegroundColor Red
    Write-Host 'If you have Office installed in a different bitness, retry from elevated CMD with:' -ForegroundColor Red
    Write-Host "  $tmpFile /quiet" -ForegroundColor Red
    exit $proc.ExitCode
}

Remove-Item $tmpFile -Force -ErrorAction SilentlyContinue
Write-Host ''
Write-Host 'NEXT STEPS:' -ForegroundColor Cyan
Write-Host '  1. Fully close Visual Studio (all windows).'
Write-Host '  2. Reopen the solution.'
Write-Host '  3. Rebuild and F5.'
