
<#
.SYNOPSIS
    Builds Ros2ForUnity asset
.DESCRIPTION
    This script builds Ros2DorUnity asset
.PARAMETER with_tests
    Build tests
.PARAMETER standalone
    Add ros2 binaries. Currently standalone flag is fixed to true, so there is no way to build without standalone libs. Parameter kept for future releases
.PARAMETER clean_install
    Makes a clean installation. Removes install dir before deploying
#>
Param (
    [Parameter(Mandatory = $false)][switch]$with_tests = $false,
    [Parameter(Mandatory = $false)][switch]$standalone = $false,
    [Parameter(Mandatory = $false)][switch]$clean_install = $false
)

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

if (-Not (Test-Path -Path "$scriptPath\src\ros2cs")) {
    Write-Host "Pull repositories with 'pull_repositories.ps1' first." -ForegroundColor Red
    exit 1
}

Write-Host $msg -ForegroundColor Green
$options = @{
    with_tests = $with_tests
    standalone = $standalone
}

if ($clean_install) {
    Write-Host "Cleaning install directory..." -ForegroundColor White
    Remove-Item -Path "$scriptPath\install" -Force -Recurse -ErrorAction Ignore
}

if ($standalone) {
    & "python" $SCRIPTPATH\src\scripts\metadata_generator.py --standalone
}
else {
    & "python" $SCRIPTPATH\src\scripts\metadata_generator.py
}
if ($LASTEXITCODE -ne 0) {
    Write-Host "Generating the Metadata failed!" -ForegroundColor Red
    exit 1
}

& "$scriptPath\src\ros2cs\build.ps1" @options
if ($?) {
    mkdir -Force $scriptPath\install\package | Out-Null
    Copy-Item -Path $scriptPath\src\Ros2ForUnity -Destination $scriptPath\install\package\ -Recurse -Force
    
    $plugin_path = Join-Path -Path $scriptPath -ChildPath "\install\package\Ros2ForUnity\Plugins\"
    Write-Host "Deploying build to $plugin_path" -ForegroundColor Green
    & "$scriptPath\deploy_unity_plugins.ps1" $plugin_path
}
else {
    Write-Host "Ros2cs build failed!" -ForegroundColor Red
    exit 1
}
