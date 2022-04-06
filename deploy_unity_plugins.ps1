$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$pluginDir=$args[0]

function Print-Help {
"
Usage: 
deploy_unity_plugins.ps1 <PLUGINS_DIR>

PLUGINS_DIR - Ros2ForUnity/Plugins.
"
}

if (([string]::IsNullOrEmpty($pluginDir)) -Or $args[0] -eq "--help" -Or $args[0] -eq "-h")
{
    Print-Help
    exit
}

if (Test-Path -Path $pluginDir) {
    Write-Host "Copying plugins to to: '$pluginDir' ..."
    (Copy-Item -Path $scriptPath\install\lib\dotnet\* -Destination ${pluginDir} 4>&1).Message
    Write-Host "Plugins copied to: '$pluginDir'" -ForegroundColor Green
    if(-not (Test-Path -Path $pluginDir\Windows\x86_64\)) {
        mkdir ${pluginDir}\Windows\x86_64\
    }
    Write-Host "Copying libraries to: '$pluginDir\Windows\x86_64\' ..."
    (Copy-Item -Path $scriptPath\install\bin\*.dll -Destination ${pluginDir}\Windows\x86_64\ 4>&1).Message
    if(-not (Test-Path -Path $scriptPath\install\standalone\)) {
        mkdir $scriptPath\install\standalone
    }
    (Copy-Item -Path $scriptPath\install\standalone\*.dll -Destination ${pluginDir}\Windows\x86_64\ 4>&1).Message
    if(-not (Test-Path -Path $scriptPath\install\resources\)) {
        mkdir $scriptPath\install\resources
    }
    (Copy-Item -Path $scriptPath\install\resources\*.dll -Destination ${pluginDir}\Windows\x86_64\ 4>&1).Message
    Write-Host "Libraries copied to '${pluginDir}\Windows\x86_64\'" -ForegroundColor Green
} else {
    Write-Host "Plugins directory: '$pluginDir' doesn't exist. Please create it first manually." -ForegroundColor Red
}
