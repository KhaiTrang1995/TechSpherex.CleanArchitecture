#requires -Version 5.1
<#
.SYNOPSIS
    Starts the TechSpherex Clean Architecture template on Windows through the Aspire AppHost.

.DESCRIPTION
    Runs src/AppHost, which starts the API and opens the Aspire dashboard
    (resources, logs, traces, metrics).

    Default mode starts PostgreSQL and Redis as containers and needs Docker running.
    -InMemory keeps the database and cache in RAM instead: no Docker, data is lost on exit.

.PARAMETER InMemory
    Use the EF Core in-memory database and an in-process cache. No Docker required.

.PARAMETER Http
    Serve the Aspire dashboard over plain HTTP (http://localhost:17170). Use this when the
    ASP.NET Core HTTPS development certificate is not trusted on this machine.

.PARAMETER NoBuild
    Skip the build and run the binaries from the last build.

.EXAMPLE
    .\scripts\start.ps1
    Full stack: PostgreSQL + Redis containers, API, Aspire dashboard.

.EXAMPLE
    .\scripts\start.ps1 -InMemory -Http
    Quick demo without Docker or a trusted HTTPS certificate.
#>
[CmdletBinding()]
param(
    [switch]$InMemory,
    [switch]$Http,
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$appHost = Join-Path $repoRoot 'src\AppHost\TechSpherex.CleanArchitecture.AppHost.csproj'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error 'The .NET SDK was not found. Install .NET 11 SDK: https://dotnet.microsoft.com/download/dotnet/11.0'
}

# global.json pins the SDK; `dotnet --version` fails here when no matching SDK is installed.
Push-Location $repoRoot
try {
    $sdkVersion = & dotnet --version 2>$null
    $sdkFound = ($LASTEXITCODE -eq 0)
}
finally {
    Pop-Location
}
if (-not $sdkFound) {
    Write-Error 'No .NET SDK matching global.json was found. Install .NET 11 SDK: https://dotnet.microsoft.com/download/dotnet/11.0'
}

if (-not $InMemory) {
    $dockerReady = $false
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        & docker info *> $null
        $dockerReady = ($LASTEXITCODE -eq 0)
    }
    if (-not $dockerReady) {
        Write-Error 'Docker is not running. Start Docker Desktop, or run with -InMemory to skip PostgreSQL and Redis.'
    }
}

$profileName = 'https'
if ($Http) { $profileName = 'http' }

$dotnetArgs = @('run', '--project', $appHost, '--launch-profile', $profileName)
if ($NoBuild) { $dotnetArgs += '--no-build' }
if ($InMemory) { $dotnetArgs += @('--', '--UseInMemoryStores=true') }

$mode = 'PostgreSQL + Redis containers'
if ($InMemory) { $mode = 'in-memory database and cache (data is lost on exit)' }

Write-Host "SDK       : $sdkVersion"
Write-Host "Mode      : $mode"
Write-Host "Dashboard : launch profile '$profileName' - the login URL is printed below"
Write-Host 'Stop      : Ctrl+C'
Write-Host ''

& dotnet @dotnetArgs
exit $LASTEXITCODE
