<#
.SYNOPSIS
Builds, packs, and optionally publishes the RynorArch projects to NuGet.

.DESCRIPTION
This script automates the versioning, packaging, and deployment of RynorArch NuGet packages.

.EXAMPLE
.\publish.ps1 -Increment None
Builds the current version without bumping numbers and places .nupkg inside the /artifacts directory.

.EXAMPLE
.\publish.ps1 -Increment Patch
Bumps the patch version (e.g. 1.0.1 -> 1.0.2), builds, and creates packages in /artifacts.

.EXAMPLE
.\publish.ps1 -Increment Minor -Push
Bumps the minor version (e.g. 1.0.1 -> 1.1.0), automatically reads `$env:NUGET_API_KEY` for authentication, and publishes to NuGet.org.

.EXAMPLE
.\publish.ps1 -Increment Major -Push -ApiKey "YOUR_API_KEY"
Bumps the major version, builds, and pushes using the provided ApiKey.
#>
param (
    [Parameter(Mandatory=$false)]
    [ValidateSet("Major", "Minor", "Patch", "None")]
    [string]$Increment = "Patch",

    [Parameter(Mandatory=$false)]
    [string]$ApiKey,

    [Parameter(Mandatory=$false)]
    [switch]$Push
)

$ErrorActionPreference = "Stop"

# 1. Get current version
$generatorProject = "src\RynorArch.Generator\RynorArch.Generator.csproj"
[xml]$projXml = Get-Content $generatorProject
$currentVersionStr = $projXml.Project.PropertyGroup.Version
if (-not $currentVersionStr) {
    Write-Host "Error: Cannot find <Version> in $generatorProject" -ForegroundColor Red
    exit 1
}

Write-Host "Current Version: $currentVersionStr" -ForegroundColor Cyan

# 2. Calculate New Version
if ($Increment -ne "None") {
    $version = [Version]$currentVersionStr
    if ($Increment -eq "Major") {
        $newVersionStr = "$($version.Major + 1).0.0"
    } elseif ($Increment -eq "Minor") {
        $newVersionStr = "$($version.Major).$($version.Minor + 1).0"
    } else {
        $newVersionStr = "$($version.Major).$($version.Minor).$($version.Build + 1)"
    }

    Write-Host "Bumping version to: $newVersionStr" -ForegroundColor Green

    # 3. Update all csproj in src/
    $projectFiles = Get-ChildItem -Path "src" -Filter "*.csproj" -Recurse
    foreach ($file in $projectFiles) {
        $content = Get-Content $file.FullName
        $updatedContent = $content -replace "<Version>.*</Version>", "<Version>$newVersionStr</Version>"
        Set-Content -Path $file.FullName -Value $updatedContent -Encoding UTF8
        Write-Host "Updated $($file.Name)" -ForegroundColor Gray
    }
} else {
    $newVersionStr = $currentVersionStr
}

# 4. Pack
Write-Host "`nPacking projects..." -ForegroundColor Cyan
$artifactsDir = (Join-Path $PSScriptRoot "artifacts")
if (Test-Path $artifactsDir) {
    Remove-Item -Path $artifactsDir -Recurse -Force
}

New-Item -ItemType Directory -Path $artifactsDir | Out-Null

dotnet pack src\RynorArch.Abstractions\RynorArch.Abstractions.csproj -c Release -o $artifactsDir
dotnet pack src\RynorArch.Generator\RynorArch.Generator.csproj -c Release -o $artifactsDir
dotnet pack src\RynorArch.Cli\RynorArch.Cli.csproj -c Release -o $artifactsDir

# 5. Push
if ($Push) {
    if (-not $ApiKey) {
        $ApiKey = $env:NUGET_API_KEY
    }

    if (-not $ApiKey) {
        Write-Host "Error: -ApiKey parameter or NUGET_API_KEY environment variable is required when using -Push." -ForegroundColor Red
        exit 1
    }

    Write-Host "`nPushing packages to NuGet..." -ForegroundColor Cyan
    $packages = Get-ChildItem -Path $artifactsDir -Filter "*.nupkg" | Where-Object { $_.Name -notmatch "symbols" }

    foreach ($pkg in $packages) {
        Write-Host "Pushing $($pkg.Name)..." -ForegroundColor Yellow
        dotnet nuget push $pkg.FullName --api-key $ApiKey --source "https://api.nuget.org/v3/index.json" --skip-duplicate
    }

    Write-Host "`nSuccessfully published version $newVersionStr" -ForegroundColor Green
} else {
    Write-Host "`nPackages created in ./artifacts. Use -Push to publish them." -ForegroundColor Yellow
}
