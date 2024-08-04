Describe 'update' {
    BeforeAll {
        $subject = Join-Path $PSScriptRoot "..\..\src\Community.PowerToys.Run.Plugin.Update\update.ps1"
        $validAssetUrl = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
        $validPluginDirectory = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\"
    }

    It 'should throw when assetUrl does not start with https://github.com' {
        $invalid = "https://gitfail.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
        { & $subject -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
    }
    It 'should throw when assetUrl does not contain owner or repo' {
        $invalid = "https://github.com/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
        { & $subject -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
    }
    It 'should throw when assetUrl does not contain tag' {
        $invalid = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
        { & $subject -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
    }
    It 'should throw when assetUrl does not end with .zip' {
        $invalid = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.txt"
        { & $subject -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
    }
    It 'should throw when assetUrl is not found' {
        $invalid = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.4-x64.zip"
        { & $subject -assetUrl $invalid -pluginDirectory $validPluginDirectory } | Should -Throw "AssetUrl is invalid."
    }

    It 'should throw when pluginDirectory does not start with C:\Users' {
        $invalid = "C:\Losers\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\"
        { & $subject -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
    }
    It 'should throw when pluginDirectory does not contain user' {
        $invalid = "C:\Users\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\"
        { & $subject -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
    }
    It 'should throw when pluginDirectory does not contain the correct PowerToys path' {
        $invalid = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\"
        { & $subject -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
    }
    It 'should throw when pluginDirectory does not end with plugin folder' {
        $invalid = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\"
        { & $subject -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
    }
    It 'should throw when pluginDirectory is not found' {
        $invalid = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\404\"
        { & $subject -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "PluginDirectory is invalid."
    }

    It 'should work when params are valid' {
        { & $subject -assetUrl $validAssetUrl -pluginDirectory $validPluginDirectory } | Should -Not -Throw
    }
}
