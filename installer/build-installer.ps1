param(
    [string]$Version = "",
    [string]$DistDir = "",
    [string]$IsccPath = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$dist = if ([string]::IsNullOrWhiteSpace($DistDir)) { Join-Path $root "dist" } else { $DistDir }

function Get-ProjectVersion {
    param([string]$Root)
    $csproj = Join-Path $Root "KmTimer\KmTimer.csproj"
    [xml]$xml = Get-Content $csproj
    $version = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if (-not $version) { throw "Version not found in KmTimer.csproj" }
    return $version.Trim()
}

function Find-IsccPath {
    param([string]$Override)
    if (-not [string]::IsNullOrWhiteSpace($Override) -and (Test-Path $Override)) {
        return (Resolve-Path $Override).Path
    }

    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
    )
    foreach ($path in $candidates) {
        if (Test-Path $path) {
            return (Resolve-Path $path).Path
        }
    }

    return $null
}

function Ensure-InnoSetup {
    param([string]$IsccOverride)

    $iscc = Find-IsccPath -Override $IsccOverride
    if ($iscc) {
        return $iscc
    }

    Write-Host "Inno Setup 6 not found. Trying winget install..."
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if (-not $winget) {
        throw "Inno Setup 6 (ISCC.exe) not found. Install from https://jrsoftware.org/isdl.php or: winget install --id JRSoftware.InnoSetup -e"
    }

    winget install --id JRSoftware.InnoSetup -e --accept-package-agreements --accept-source-agreements
    $iscc = Find-IsccPath -Override $IsccOverride
    if (-not $iscc) {
        throw "ISCC.exe still not found after winget install. Reopen shell or pass -IsccPath."
    }

    return $iscc
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Get-ProjectVersion -Root $root
}

$publishDir = Join-Path $dist "publish\win-x64"
if (-not (Test-Path (Join-Path $publishDir "KmTimer.exe"))) {
    throw "Publish output not found: $publishDir (run scripts/Build-Portable.ps1 first)"
}

$iscc = Ensure-InnoSetup -IsccOverride $IsccPath
$issPath = Join-Path $PSScriptRoot "KmTimer.iss"

Get-ChildItem $dist -Filter "KmTimer-*-x64-Setup.exe" -ErrorAction SilentlyContinue | Remove-Item -Force

$defines = @(
    "/DAppVersion=$Version",
    "/DPublishDir=$publishDir",
    "/DOutputDir=$dist",
    "/DRepoRoot=$root"
)

Write-Host "ISCC KmTimer.iss ..."
& $iscc $defines $issPath
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup build failed"
}

$setupPath = Join-Path $dist "KmTimer-$Version-x64-Setup.exe"
if (-not (Test-Path $setupPath)) {
    throw "Setup was not generated: $setupPath"
}

Write-Host "Setup: $setupPath"
exit 0
