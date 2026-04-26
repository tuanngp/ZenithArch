<#
.SYNOPSIS
Builds, packs, and optionally publishes the ZenithArch projects to NuGet.

.DESCRIPTION
This script automates the versioning, packaging, and deployment of ZenithArch NuGet packages.

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

.EXAMPLE
.\publish.ps1 -Increment None -SkipApiCompat
Builds and packs without running the Abstractions ApiCompat baseline validation.
#>
param (
    [Parameter(Mandatory=$false)]
    [ValidateSet("Major", "Minor", "Patch", "None")]
    [string]$Increment = "Patch",

    [Parameter(Mandatory=$false)]
    [string]$ApiKey,

    [Parameter(Mandatory=$false)]
    [switch]$Push,

    [Parameter(Mandatory=$false)]
    [switch]$SkipApiCompat
)

$ErrorActionPreference = "Stop"

# 1. Get current version from central props
$versionPropsFile = Join-Path $PSScriptRoot "Directory.Build.props"
[xml]$propsXml = Get-Content $versionPropsFile
$currentVersionStr = $propsXml.Project.PropertyGroup.VersionPrefix
if (-not $currentVersionStr) {
    Write-Host "Error: Cannot find <VersionPrefix> in $versionPropsFile" -ForegroundColor Red
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

    # 3. Update the shared version source
    $propsXml.Project.PropertyGroup.VersionPrefix = $newVersionStr
    $propsXml.Save($versionPropsFile)
    Write-Host "Updated Directory.Build.props" -ForegroundColor Gray
} else {
    $newVersionStr = $currentVersionStr
}

# 4. Build and test first
Write-Host "`nBuilding solution..." -ForegroundColor Cyan
dotnet build "ZenithArch.slnx" -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "`nRunning tests..." -ForegroundColor Cyan
dotnet test "ZenithArch.slnx" -c Release --no-build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# 5. Pack
Write-Host "`nPacking projects..." -ForegroundColor Cyan
$artifactsDir = (Join-Path $PSScriptRoot "artifacts")
if (Test-Path $artifactsDir) {
    Remove-Item -Path $artifactsDir -Recurse -Force
}

New-Item -ItemType Directory -Path $artifactsDir | Out-Null

$projectFiles = Get-ChildItem -Path (Join-Path $PSScriptRoot "src") -Filter "*.csproj" -Recurse | Sort-Object FullName
foreach ($projectFile in $projectFiles) {
    Write-Host "Packing $($projectFile.Name)..." -ForegroundColor Gray
    dotnet pack $projectFile.FullName -c Release -o $artifactsDir --no-build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

if (-not $SkipApiCompat) {
    Write-Host "`nRunning Abstractions API compatibility validation..." -ForegroundColor Cyan
    & (Join-Path $PSScriptRoot "eng/Validate-AbstractionsApiCompat.ps1") -ArtifactsDir "artifacts" -PackageId "ZenithArch.Abstractions"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

# 6. Push
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
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    Write-Host "`nSuccessfully published version $newVersionStr" -ForegroundColor Green
} else {
    Write-Host "`nPackages created in ./artifacts. Use -Push to publish them." -ForegroundColor Yellow
}
