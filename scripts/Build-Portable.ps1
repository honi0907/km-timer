# KM Timer ポータブルビルド

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Stop-Process -Name KmTimer -Force -ErrorAction SilentlyContinue

$csproj = Join-Path $root "KmTimer\KmTimer.csproj"
[xml]$xml = Get-Content $csproj
$version = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw "Version not found in KmTimer.csproj" }

$dist = Join-Path $root "dist"
$publish = Join-Path $dist "publish\$Runtime"
New-Item -ItemType Directory -Force -Path $publish | Out-Null

Get-ChildItem $dist -Filter "KmTimer-*-portable.zip" -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem $dist -Filter "KmTimer-*-x64-Setup.exe" -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem $dist -Filter "KmTimer-Setup-*" -ErrorAction SilentlyContinue | Remove-Item -Force

dotnet publish $csproj `
    -c $Configuration `
    -r $Runtime `
    -p:Platform=x64 `
    -p:PublishSingleFile=false `
    -p:WindowsAppSDKSelfContained=true `
    -o $publish

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

$zipName = "KmTimer-$version-x64-portable.zip"
$zipPath = Join-Path $dist $zipName
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Compress-Archive -Path (Join-Path $publish "*") -DestinationPath $zipPath -Force

Write-Host "OK: $zipPath"
Write-Host "Publish: $publish"
