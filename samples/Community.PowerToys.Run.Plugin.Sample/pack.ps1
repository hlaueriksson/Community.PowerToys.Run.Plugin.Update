<#PSScriptInfo
.VERSION 0.0.0
.GUID 58d7b8e8-fa18-485d-baaf-4c413181280b
.AUTHOR Henrik Lau Eriksson
.COMPANYNAME
.COPYRIGHT
.TAGS PowerToys Run Plugins Pack
.LICENSEURI
.PROJECTURI https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Templates
.ICONURI
.EXTERNALMODULEDEPENDENCIES
.REQUIREDSCRIPTS
.EXTERNALSCRIPTDEPENDENCIES
.RELEASENOTES
#>

<#
    .Synopsis
    Packs the plugin into release archive.

    .Description
    Builds the project in Release configuration,
    copies the output files into plugin folder,
    packs the plugin folder into release archive.

    .Example
    .\pack.ps1

    .Link
    https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Templates
#>

# Clean
Get-ChildItem -Path "." -Directory -Include "bin", "obj" -Recurse | Remove-Item -Recurse -Force

# Name
$folder = Split-Path -Path $PWD -Leaf
$name = $folder.Split(".")[-1]
Write-Output "Pack: $name"

# Version
$json = Get-Content -Path "plugin.json" -Raw | ConvertFrom-Json
$version = $json.Version
Write-Output "Version: $version"

# Platforms
[xml]$csproj = Get-Content -Path "*.csproj"
$platforms = "$($csproj.Project.PropertyGroup.Platforms)".Trim() -split ";"

$dependencies = @("PowerToys.Common.UI.dll", "PowerToys.ManagedCommon.dll", "PowerToys.Settings.UI.Lib.dll", "Wox.Infrastructure.dll", "Wox.Plugin.dll")

foreach ($platform in $platforms)
{
    Write-Output "Platform: $platform"

    # Build
    dotnet build -c Release /p:TF_BUILD=true /p:Platform=$platform

    if (!$?) {
        # Build FAILED.
        Exit $LastExitCode
    }

    $output = ".\bin\$platform\Release\net8.0-windows\"
    $destination = ".\bin\$platform\$name"
    $zip = ".\bin\$platform\$name-$version-$($platform.ToLower()).zip"

    Copy-Item -Path $output -Destination $destination -Recurse -Exclude $dependencies
    Compress-Archive -Path $destination -DestinationPath $zip
}
