<#PSScriptInfo
.VERSION 0.4.0
.GUID 812eb73f-54c9-48c5-bdf0-a04b415046c4
.AUTHOR Henrik Lau Eriksson
.COMPANYNAME
.COPYRIGHT
.TAGS PowerToys Run Plugins Update
.LICENSEURI
.PROJECTURI https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update
.ICONURI
.EXTERNALMODULEDEPENDENCIES
.REQUIREDSCRIPTS
.EXTERNALSCRIPTDEPENDENCIES
.RELEASENOTES
#>

<#
    .Synopsis
    Updates PowerToys Run community plugins.

    .Description
    Updates a plugin in these steps:
    1. Kills PowerToys
    2. Downloads the latest release from GitHub
    3. Verifies release hash
    4. Deletes the old plugin files
    5. Extracts the release zip file
    6. Starts PowerToys

    .Parameter AssetUrl
    The URL to the release zip file on GitHub

    .Parameter PluginDirectory
    The path to the plugin directory, i.e. a subdirectory under %LocalAppData%\Microsoft\PowerToys\PowerToys Run\Plugins\

    .Example
    .\update.ps1 "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"

    .Link
    https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update
#>
param (
    [Parameter(Position = 0, Mandatory = $true)]
    [ValidatePattern('^https://github\.com/[^/]+/[^/]+/releases/download/v\d+\.\d+\.\d+/[^/]+\.zip$')]
    [string]$assetUrl,

    [Parameter(Position = 1, Mandatory = $false)]
    [ValidatePattern('(?i)^C:\\Users\\[^\\]+\\AppData\\Local\\Microsoft\\PowerToys\\PowerToys Run\\Plugins\\[^\\]+\\?$')]
    [string]$pluginDirectory = "."
)

if ($pluginDirectory -eq ".") {
    $pluginDirectory = $PSScriptRoot
}

if (-Not (Test-Path -Path $pluginDirectory)) {
    throw "PluginDirectory is invalid."
}

try { Invoke-WebRequest -Uri $assetUrl -Method Head } catch {
    throw "AssetUrl is invalid."
}

$log = Join-Path $pluginDirectory "update.log"
Start-Transcript -Path $log

#Requires -RunAsAdministrator

$assetName = Split-Path $assetUrl -Leaf

function Write-Log {
    param (
        [string]$message
    )
    $result = "$(Get-Date -Format "yyyy-MM-dd HH:mm:ss") $message"
    Write-Output $result
}

Write-Log "Update plugin..."
Write-Log "AssetUrl: $assetUrl"
Write-Log "PluginDirectory: $pluginDirectory"
Write-Log "Log: $log"
Write-Log "AssetName: $assetName"

try {
    Write-Log "Kill PowerToys"
    $name = "PowerToys"
    $process = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($process) {
        Stop-Process -Name $name -Force
        while ($null -ne (Get-Process -Name $name -ErrorAction SilentlyContinue)) {
            Start-Sleep -Seconds 1
        }
    }

    Write-Log "Download release"
    $release = Join-Path $pluginDirectory $assetName
    Invoke-WebRequest -Uri $assetUrl -OutFile $release

    $hash = Get-FileHash $release -Algorithm SHA256 | Select-Object -ExpandProperty Hash
    Write-Log "Hash: $hash"
    if ($assetUrl -match 'github\.com/([^/]+)/([^/]+)/') {
        $owner = $matches[1]
        $repo = $matches[2]
        $latest = "https://github.com/$owner/$repo/releases/latest"
        Write-Log "Latest: $latest"
        $response = Invoke-WebRequest -Uri $latest
        if ($response.Content -match $hash) {
            Write-Log "Hash is verified"
        }
        else {
            Write-Warning "Hash could not be verified"
        }
    }

    Write-Log "Delete plugin files"
    Get-ChildItem -Path $pluginDirectory -Exclude @("update.*", "*.zip") | Remove-Item -Recurse -Force -Confirm:$false

    Write-Log "Extract release"
    $parent = Split-Path -Path $pluginDirectory -Parent
    Expand-Archive -Path $release -DestinationPath $parent -Force

    Write-Log "Start PowerToys"
    $machinePath = "C:\Program Files\PowerToys\PowerToys.exe"
    $userPath = "$env:LOCALAPPDATA\PowerToys\PowerToys.exe"
    if (Test-Path $machinePath) {
        Start-Process -FilePath $machinePath
    }
    elseif (Test-Path $userPath) {
        Start-Process -FilePath $userPath
    }
    else {
        Write-Error "Start PowerToys failed"
    }

    Write-Log "Update complete!"
}
catch {
    Write-Error $_
    Write-Log "Update failed!"
}
finally {
    Stop-Transcript
}
