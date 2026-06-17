$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$requiredFiles = @(
    "package.json",
    "README.md",
    "CHANGELOG.md",
    "LICENSE.md",
    "CONTRIBUTING.md",
    "Runtime/Deucarian.ObjectSelection.CoreStateIntegration.asmdef",
    "Runtime/ObjectSelectionCoreStateIntegration.cs",
    "Tests/EditMode/Deucarian.ObjectSelection.CoreStateIntegration.Tests.asmdef",
    "Samples~/CoreStateIntegrationSample/Deucarian.ObjectSelection.CoreStateIntegration.Samples.CoreStateIntegrationSample.asmdef",
    "Samples~/CoreStateIntegrationSample/CoreStateIntegrationSample.unity"
)

$requiredDirectories = @(
    "Runtime",
    "Tests/EditMode",
    "Samples~/CoreStateIntegrationSample",
    "Tools",
    ".github/workflows"
)

foreach ($directory in $requiredDirectories) {
    $path = Join-Path $root $directory
    if (-not (Test-Path -LiteralPath $path -PathType Container)) {
        throw "Missing required directory: $directory"
    }
}

foreach ($file in $requiredFiles) {
    $path = Join-Path $root $file
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $file"
    }
}

$package = Get-Content -LiteralPath (Join-Path $root "package.json") -Raw | ConvertFrom-Json
if ($package.name -ne "com.deucarian.object-selection.core-state-integration") {
    throw "Unexpected package name: $($package.name)"
}

if ($package.displayName -ne "Deucarian Object Selection Core State Integration") {
    throw "Unexpected package display name: $($package.displayName)"
}

if ($package.version -notmatch "^\d+\.\d+\.\d+$") {
    throw "Package version must be semver MAJOR.MINOR.PATCH: $($package.version)"
}

if ($package.dependencies."com.deucarian.object-selection" -ne "1.0.2") {
    throw "Package must depend on com.deucarian.object-selection 1.0.2"
}

if ($package.dependencies."com.deucarian.core-state" -ne "1.0.0") {
    throw "Package must depend on com.deucarian.core-state 1.0.0"
}

if ($package.dependencies."com.deucarian.logging" -ne "0.2.6") {
    throw "Package must depend on com.deucarian.logging 0.2.6"
}

$runtimeAsmdef = Get-Content -LiteralPath (Join-Path $root "Runtime/Deucarian.ObjectSelection.CoreStateIntegration.asmdef") -Raw | ConvertFrom-Json
if ($runtimeAsmdef.name -ne "Deucarian.ObjectSelection.CoreStateIntegration") {
    throw "Unexpected runtime asmdef name: $($runtimeAsmdef.name)"
}

if ($runtimeAsmdef.references -notcontains "Deucarian.ObjectSelection") {
    throw "Runtime asmdef must reference Deucarian.ObjectSelection"
}

if ($runtimeAsmdef.references -notcontains "Deucarian.CoreState") {
    throw "Runtime asmdef must reference Deucarian.CoreState"
}

if ($runtimeAsmdef.references -notcontains "Deucarian.Logging") {
    throw "Runtime asmdef must reference Deucarian.Logging"
}

$testAsmdef = Get-Content -LiteralPath (Join-Path $root "Tests/EditMode/Deucarian.ObjectSelection.CoreStateIntegration.Tests.asmdef") -Raw | ConvertFrom-Json
if ($testAsmdef.references -notcontains "Deucarian.ObjectSelection.CoreStateIntegration") {
    throw "Tests asmdef must reference Deucarian.ObjectSelection.CoreStateIntegration"
}

$sampleAsmdef = Get-Content -LiteralPath (Join-Path $root "Samples~/CoreStateIntegrationSample/Deucarian.ObjectSelection.CoreStateIntegration.Samples.CoreStateIntegrationSample.asmdef") -Raw | ConvertFrom-Json
if ($sampleAsmdef.references -notcontains "Deucarian.ObjectSelection.CoreStateIntegration") {
    throw "Sample asmdef must reference Deucarian.ObjectSelection.CoreStateIntegration"
}

if ($sampleAsmdef.references -notcontains "Deucarian.Logging") {
    throw "Sample asmdef must reference Deucarian.Logging"
}

$forbiddenReferences = @(
    "UIBinding",
    "API",
    "Session",
    "UnityEngine.UI",
    "UnityEngine.EventSystems",
    "UnityEngine.UIElements",
    "ServiceLocator"
)

$sourceFiles = Get-ChildItem -LiteralPath $root -Recurse -File -Filter "*.cs" |
    Where-Object { $_.FullName -notmatch "\\Tests\\" }
foreach ($sourceFile in $sourceFiles) {
    $content = Get-Content -LiteralPath $sourceFile.FullName -Raw
    foreach ($forbiddenReference in $forbiddenReferences) {
        if ($content -match [regex]::Escape($forbiddenReference)) {
            throw "Source file $($sourceFile.Name) contains forbidden reference: $forbiddenReference"
        }
    }
}

$forbiddenProjectScaffolding = @("Assets", "Packages", "ProjectSettings")
foreach ($directory in $forbiddenProjectScaffolding) {
    $path = Join-Path $root $directory
    if (Test-Path -LiteralPath $path -PathType Container) {
        throw "Package repository should not contain Unity project scaffolding directory: $directory"
    }
}

$generatedArtifacts = Get-ChildItem -LiteralPath $root -Recurse -Force -File |
    Where-Object { $_.Name -match "\.(unitypackage|zip|tar|tgz)$" }
if ($generatedArtifacts.Count -gt 0) {
    throw "Generated artifacts are present in the package repository."
}

Write-Host "Deucarian Object Selection Core State Integration package validation passed."
