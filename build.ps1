
<#
.SYNOPSIS
    Builds Ros2ForUnity asset
.DESCRIPTION
    This script builds Ros2DorUnity asset
.PARAMETER with_tests
    Build tests
.PARAMETER standalone
    Add ros2 binaries
.PARAMETER clean_install
    Makes a clean installation. Removes install dir before deploying
#>
Param (
    [Parameter(Mandatory=$false)][switch]$with_tests=$false,
    [Parameter(Mandatory=$false)][switch]$standalone=$true,
    [Parameter(Mandatory=$false)][switch]$clean_install=$false
)

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

if(-Not (Test-Path -Path "$scriptPath\src\ros2cs")) {
    Write-Host "Pull repositories with 'pull_repositories.ps1' first." -ForegroundColor Red
    exit 1
}


Write-Host $msg -ForegroundColor Green
$options=""
if($with_tests) {
    $options="--with-tests"
}
if($standalone) {
    $options="--standalone"
}


if($clean_install) {
    Write-Host "Cleaning install directory..." -ForegroundColor White
    Remove-Item -Path "$scriptPath\install" -Force -Recurse -ErrorAction Ignore
}
& "$scriptPath\src\ros2cs\build.ps1" $options
if($?) {
    mkdir $scriptPath\install\asset | Out-Null
    (Copy-Item -verbose -Path $scriptPath\src\Ros2ForUnity -Destination $scriptPath\install\asset\Ros2ForUnity 4>&1).Message
    
    $plugin_path=Join-Path -Path $scriptPath -ChildPath "\install\asset\Ros2ForUnity\Plugins\"
    Write-Host "Deploying build to $plugin_path" -ForegroundColor Green
    & "$scriptPath\deploy_unity_plugins.ps1" $plugin_path
} else {
    Write-Host "Ros2cs build failed!" -ForegroundColor Red
    exit 1
}


