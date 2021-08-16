
<#
.SYNOPSIS
    Creates a 'unitypackage' from an input asset.
.DESCRIPTION
    This script screates a temporary Unity project in "%USERPROFILE%\AppData\Local\Temp" directory, copy input asset and makes an unity package out of it.
.PARAMETER unity_path
    Unity editor executable path
.PARAMETER input_asset
    input asset to pack into unity package
.PARAMETER package_name
    Unity package name
.PARAMETER output_dir
    output file directory
#>
Param (
    [Parameter(Mandatory=$true)][string]$unity_path,
    [Parameter(Mandatory=$true)][string]$input_asset,
    [Parameter(Mandatory=$true)][string]$package_name,
    [Parameter(Mandatory=$true)][string]$output_dir
)

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$temp_dir = $Env:TEMP

& "$unity_path" -version | Tee-Object -Variable unity_version | Out-Null

if ($unity_version -match '^[0-9]{4}\.[0-9]*\.[0-9]*[f]?[0-9]*$') {
    Write-Host "Unity editor confirmed..."
} else {
    Write-Host "Can't confirm Unity editor. Exiting."
    exit 1
}

$tmp_project_path = Join-Path -Path "$temp_dir" -ChildPath "\ros2cs_unity_project\$unity_version"

# Create temp project
if(Test-Path -Path "$tmp_project_path") {
    Write-Host "Found existing temporary project for Unity $unity_version."
    Remove-Item -Path "$tmp_project_path\Assets\*" -Force -Recurse -ErrorAction Ignore
} else {
    Write-Host "Creating Unity temporary project for Unity $unity_version..."
    & "$unity_path" -createProject "$tmp_project_path" -batchmode -quit | Out-Null
}

# Copy asset
Write-Host "Copying asset '$input_asset' to export..."
Copy-Item -Path "$input_asset" -Destination "$tmp_project_path\Assets\$package_name" -Recurse

# Creating asset
Write-Host "Saving unitypackage '$output_dir\$package_name.unitypackage'..."
& "$unity_path" -projectPath "$tmp_project_path" -exportPackage "$package_name" "$output_dir\$package_name.unitypackage" -batchmode -quit | Out-Null

# Cleaning up
Write-Host "Cleaning up temporary project..."
Remove-Item -Path "$tmp_project_path\Assets\*" -Force -Recurse -ErrorAction Ignore

Write-Host "Done!" -ForegroundColor Green

