param(
    [string]$Target = (Join-Path $env:USERPROFILE "AppData\LocalLow\Freehold Games\CavesOfQud\Mods\MutationMeddley")
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Deploying Mutation Meddley"
Write-Host "Source: $repoRoot"
Write-Host "Target: $Target"

New-Item -ItemType Directory -Path $Target -Force | Out-Null

$runtimeFiles = @(
    "manifest.json",
    "Mutations.xml"
)

foreach ($file in $runtimeFiles) {
    $source = Join-Path $repoRoot $file
    if (-not (Test-Path $source)) {
        throw "Required runtime file not found: $source"
    }

    Copy-Item $source (Join-Path $Target $file) -Force
}

$runtimeDirectories = @(
    "Code",
    "Textures"
)

foreach ($directory in $runtimeDirectories) {
    $source = Join-Path $repoRoot $directory
    $destination = Join-Path $Target $directory

    if (Test-Path $destination) {
        Remove-Item $destination -Recurse -Force
    }

    if (Test-Path $source) {
        Copy-Item $source $destination -Recurse -Force
    }
}

$optionalFiles = @(
    "Preview.png",
    "preview.png"
)

foreach ($file in $optionalFiles) {
    $source = Join-Path $repoRoot $file
    if (Test-Path $source) {
        Copy-Item $source (Join-Path $Target $file) -Force
    }
}

Write-Host ""
Write-Host "Deployment complete."
Write-Host "Existing workshop.json was left untouched, if present."
Write-Host "Restart Caves of Qud before testing C# changes."
