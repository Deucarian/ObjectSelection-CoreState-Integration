param(
    [Parameter(Mandatory = $true)]
    [string] $UnityEditorPath,

    [Parameter(Mandatory = $true)]
    [string] $ProjectPath
)

$ErrorActionPreference = "Stop"

& $UnityEditorPath `
    -batchmode `
    -nographics `
    -projectPath $ProjectPath `
    -runTests `
    -testPlatform editmode `
    -assemblyNames Deucarian.ObjectSelection.CoreStateBridge.Tests `
    -testResults (Join-Path $ProjectPath "TestResults.xml")

if ($LASTEXITCODE -ne 0) {
    throw "Unity EditMode tests failed with exit code $LASTEXITCODE"
}
