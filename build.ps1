#Requires -Version 5.1
<#
.SYNOPSIS
    LionFire.AgUi.Blazor Build Script

.DESCRIPTION
    Build script for building, testing, and packaging the LionFire.AgUi.Blazor solution.

.PARAMETER Command
    The build command to execute. Valid values: restore, build, test, pack, clean, format-check, format-fix, all (default)

.PARAMETER Configuration
    Build configuration (default: Release)

.EXAMPLE
    .\build.ps1
    Runs the full build (restore, build, test, pack)

.EXAMPLE
    .\build.ps1 -Command test
    Runs only the tests

.EXAMPLE
    .\build.ps1 -Command build -Configuration Debug
    Builds in Debug configuration
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('restore', 'build', 'test', 'pack', 'clean', 'format-check', 'format-fix', 'all', 'help')]
    [string]$Command = 'all',

    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$ScriptDir = $PSScriptRoot
$ArtifactsDir = Join-Path $ScriptDir 'artifacts'
$TestResultsDir = Join-Path $ScriptDir 'TestResults'

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Invoke-Restore {
    Write-Info "Restoring dependencies..."
    & dotnet restore $ScriptDir
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
}

function Invoke-Build {
    Write-Info "Building solution ($Configuration)..."
    & dotnet build $ScriptDir --no-restore --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
}

function Invoke-Test {
    Write-Info "Running tests..."
    & dotnet test $ScriptDir --no-build --configuration $Configuration `
        --verbosity normal `
        --collect:"XPlat Code Coverage" `
        --logger trx `
        --results-directory $TestResultsDir
    if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
}

function Invoke-Pack {
    Write-Info "Creating NuGet packages..."
    if (-not (Test-Path $ArtifactsDir)) {
        New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null
    }
    & dotnet pack $ScriptDir --no-build --configuration $Configuration --output $ArtifactsDir
    if ($LASTEXITCODE -ne 0) { throw "Pack failed" }

    Write-Info "Packages created in $ArtifactsDir"
    $packages = Get-ChildItem -Path $ArtifactsDir -Filter "*.nupkg" -ErrorAction SilentlyContinue
    if ($packages) {
        $packages | ForEach-Object { Write-Host "  $_" }
    } else {
        Write-Warn "No packages found"
    }
}

function Invoke-Clean {
    Write-Info "Cleaning solution..."
    & dotnet clean $ScriptDir --configuration $Configuration
    if (Test-Path $ArtifactsDir) {
        Remove-Item -Path $ArtifactsDir -Recurse -Force
    }
    if (Test-Path $TestResultsDir) {
        Remove-Item -Path $TestResultsDir -Recurse -Force
    }
    Write-Info "Clean complete"
}

function Invoke-FormatCheck {
    Write-Info "Checking code formatting..."
    & dotnet format $ScriptDir --verify-no-changes --verbosity diagnostic
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Code formatting issues found. Run '.\build.ps1 format-fix' to fix."
        throw "Format check failed"
    }
}

function Invoke-FormatFix {
    Write-Info "Fixing code formatting..."
    & dotnet format $ScriptDir
    if ($LASTEXITCODE -ne 0) { throw "Format fix failed" }
    Write-Info "Formatting complete"
}

function Invoke-All {
    Invoke-Restore
    Invoke-Build
    Invoke-Test
    Invoke-Pack
}

function Show-Help {
    Write-Host @"
LionFire.AgUi.Blazor Build Script

Usage: .\build.ps1 [command] [-Configuration <config>]

Commands:
  restore      Restore NuGet dependencies
  build        Build the solution
  test         Run all tests
  pack         Create NuGet packages
  clean        Clean build outputs
  format-check Check code formatting
  format-fix   Fix code formatting
  all          Run restore, build, test, and pack (default)
  help         Show this help message

Parameters:
  -Configuration  Build configuration: Debug or Release (default: Release)

Examples:
  .\build.ps1                        # Full build
  .\build.ps1 test                   # Run tests only
  .\build.ps1 build -Configuration Debug
"@
}

# Main entry point
try {
    switch ($Command) {
        'restore' {
            Invoke-Restore
        }
        'build' {
            Invoke-Restore
            Invoke-Build
        }
        'test' {
            Invoke-Restore
            Invoke-Build
            Invoke-Test
        }
        'pack' {
            Invoke-Restore
            Invoke-Build
            Invoke-Test
            Invoke-Pack
        }
        'clean' {
            Invoke-Clean
        }
        'format-check' {
            Invoke-FormatCheck
        }
        'format-fix' {
            Invoke-FormatFix
        }
        'all' {
            Invoke-All
        }
        'help' {
            Show-Help
        }
    }
    Write-Info "Done!"
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
