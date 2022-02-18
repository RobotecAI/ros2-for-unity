
<#
.SYNOPSIS
    Creates a 'unitypackage' from an input asset.
.DESCRIPTION
    This script screates a temporary Unity project in "%USERPROFILE%\AppData\Local\Temp" directory, copy input asset and makes an unity package out of it. Valid Unity license is required.
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
    [Parameter(Mandatory=$false)][string]$input_asset,
    [Parameter(Mandatory=$false)][string]$package_name="Ros2ForUnity",
    [Parameter(Mandatory=$false)][string]$output_dir
)

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$temp_dir = $Env:TEMP

if(-Not $PSBoundParameters.ContainsKey('input_asset')) {
    $input_asset= Join-Path -Path $scriptPath -ChildPath "\install\asset\Ros2ForUnity"
}

if(-Not $PSBoundParameters.ContainsKey('output_dir')) {
    $output_dir= Join-Path -Path $scriptPath -ChildPath "\install\unity_package"
}

if(-Not (Test-Path -Path "$input_asset")) {
    Write-Host "Input asset '$input_asset' doesn't exist! Use 'build.ps1' to build project first." -ForegroundColor Red
    exit 1
}

if(-Not (Test-Path -Path "$output_dir")) {
    mkdir ${output_dir} | Out-Null
}

& "$unity_path" -version | Tee-Object -Variable unity_version | Out-Null

if ($unity_version -match '^[0-9]{4}\.[0-9]*\.[0-9]*[f]?[0-9]*$') {
    Write-Host "Unity editor confirmed."
} else {
    while (1) {
        $confirmation = Read-Host "Can't confirm Unity editor. Do you want to force $unity_path as an Unity editor executable? [y]es or [n]o"
        if ($confirmation -eq 'y' -or $confirmation -eq 'Y') {
            break;
        } elseif ( $confirmation -eq 'n' -or $confirmation -eq 'N' ) {
            exit 1;
        } else {
            Write-Host "Please answer [y]es or [n]o.";
        }
    }
}
Write-Host "Using ${unity_path} editor."

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
& "$unity_path" -projectPath "$tmp_project_path" -exportPackage "Assets\$package_name" "$output_dir\$package_name.unitypackage" -batchmode -quit | Out-Null

# Cleaning up
Write-Host "Cleaning up temporary project..."
Remove-Item -Path "$tmp_project_path\Assets\*" -Force -Recurse -ErrorAction Ignore

Write-Host "Done!" -ForegroundColor Green

