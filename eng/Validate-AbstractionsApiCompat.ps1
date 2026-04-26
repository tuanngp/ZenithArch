param(
    [Parameter(Mandatory = $false)]
    [string]$ArtifactsDir = "artifacts",

    [Parameter(Mandatory = $false)]
    [string]$PackageId = "ZenithArch.Abstractions",

    [Parameter(Mandatory = $false)]
    [string]$ToolDir = ".tools"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$artifactsPath = Join-Path $repoRoot $ArtifactsDir

if (-not (Test-Path $artifactsPath)) {
    throw "Artifacts directory not found: $artifactsPath"
}

$escapedPackageId = [Regex]::Escape($PackageId)
$currentPackage = Get-ChildItem -Path $artifactsPath -Filter "$PackageId.*.nupkg" |
    Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $currentPackage) {
    throw "Unable to find current package in $artifactsPath for package id $PackageId"
}

if ($currentPackage.Name -notmatch "^$escapedPackageId\.(?<version>[^.]+\.[^.]+\.[^.]+(?:[-+][^.]*)?)\.nupkg$") {
    throw "Unable to parse package version from $($currentPackage.Name)"
}

$currentVersion = $Matches["version"]
$currentStableVersion = $currentVersion.Split("-")[0]

Write-Host "Current package: $($currentPackage.Name)" -ForegroundColor Cyan
Write-Host "Current version: $currentVersion" -ForegroundColor Cyan

$packageIdLower = $PackageId.ToLowerInvariant()
$indexUrl = "https://api.nuget.org/v3-flatcontainer/$packageIdLower/index.json"
try {
    $indexPayload = Invoke-RestMethod -Uri $indexUrl -Method Get
}
catch {
    Write-Host "No package index found on NuGet for $PackageId. Likely first publish. Skipping ApiCompat baseline validation." -ForegroundColor Yellow
    exit 0
}

$availableVersions = @($indexPayload.versions)
if ($availableVersions.Count -eq 0) {
    Write-Host "No package versions found on NuGet for $PackageId. Skipping ApiCompat baseline validation." -ForegroundColor Yellow
    exit 0
}

$candidates = foreach ($version in $availableVersions) {
    if ($version -match "-") {
        continue
    }

    try {
        $parsed = [Version]$version
    }
    catch {
        continue
    }

    [pscustomobject]@{
        Version = $version
        Parsed  = $parsed
    }
}

$currentParsedVersion = [Version]$currentStableVersion
$baselineCandidate = $candidates |
    Where-Object { $_.Parsed -lt $currentParsedVersion } |
    Sort-Object Parsed -Descending |
    Select-Object -First 1

if (-not $baselineCandidate) {
    Write-Host "No baseline version older than $currentVersion found for $PackageId. Skipping ApiCompat baseline validation." -ForegroundColor Yellow
    exit 0
}

$baselineVersion = $baselineCandidate.Version
$baselineDir = Join-Path $artifactsPath "baseline"
New-Item -ItemType Directory -Path $baselineDir -Force | Out-Null
$baselinePackagePath = Join-Path $baselineDir "$packageIdLower.$baselineVersion.nupkg"

if (-not (Test-Path $baselinePackagePath)) {
    $baselineUrl = "https://api.nuget.org/v3-flatcontainer/$packageIdLower/$baselineVersion/$packageIdLower.$baselineVersion.nupkg"
    Write-Host "Downloading baseline package: $baselineUrl" -ForegroundColor Cyan
    Invoke-WebRequest -Uri $baselineUrl -OutFile $baselinePackagePath
}

$toolPath = Join-Path $repoRoot $ToolDir
New-Item -ItemType Directory -Path $toolPath -Force | Out-Null
$isWindowsHost = $env:OS -eq "Windows_NT"
$apiCompatExe = Join-Path $toolPath "apicompat.exe"
if (-not $isWindowsHost) {
    $apiCompatExe = Join-Path $toolPath "apicompat"
}

if (-not (Test-Path $apiCompatExe)) {
    Write-Host "Installing ApiCompat tool into $toolPath" -ForegroundColor Cyan
    dotnet tool install microsoft.dotnet.apicompat.tool --tool-path $toolPath
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install microsoft.dotnet.apicompat.tool"
    }

    if (-not (Test-Path $apiCompatExe)) {
        throw "ApiCompat executable was not found in $toolPath after installation."
    }
}

Write-Host "Running ApiCompat baseline validation against version $baselineVersion" -ForegroundColor Cyan
& $apiCompatExe package `
    $currentPackage.FullName `
    --baseline-package $baselinePackagePath `
    --run-api-compat `
    --enable-strict-mode-for-baseline-validation `
    --noWarn CP0003 `
    --enable-rule-cannot-change-parameter-name

if ($LASTEXITCODE -ne 0) {
    throw "ApiCompat validation failed for $PackageId (baseline $baselineVersion -> current $currentVersion)."
}

Write-Host "ApiCompat validation passed: $PackageId baseline $baselineVersion -> $currentVersion" -ForegroundColor Green
