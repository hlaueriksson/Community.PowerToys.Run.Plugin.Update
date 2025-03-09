# Community.PowerToys.Run.Plugin.Update

[![build](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/actions/workflows/build.yml/badge.svg)](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/actions/workflows/build.yml)
[![Snyk Security Scan](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/actions/workflows/snyk.yml/badge.svg)](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/actions/workflows/snyk.yml)
[![Community.PowerToys.Run.Plugin.Update](https://img.shields.io/nuget/v/Community.PowerToys.Run.Plugin.Update.svg?label=Community.PowerToys.Run.Plugin.Update)](https://www.nuget.org/packages/Community.PowerToys.Run.Plugin.Update)
[![Mentioned in Awesome PowerToys Run Plugins](https://awesome.re/mentioned-badge.svg)](https://github.com/hlaueriksson/awesome-powertoys-run-plugins)

This NuGet package is intended for PowerToys Run community plugins authors.

It adds support for updating PowerToys Run Plugins.

It contains a `ARM64` and `x64` version of:

- `Community.PowerToys.Run.Plugin.Update.dll`

the images:

- `update.dark.png`
- `update.light.png`

and the script:

- `update.ps1`

Make sure these files are distributed together with your plugin.

## Installation

.NET CLI:

```cmd
dotnet add package Community.PowerToys.Run.Plugin.Update
```

Package Manager:

```cmd
PM> NuGet\Install-Package Community.PowerToys.Run.Plugin.Update
```

PackageReference:

```csproj
<PackageReference Include="Community.PowerToys.Run.Plugin.Update" Version="0.3.0" />
```

## Requirements

You must:

1. Package your plugin in a zip archive file
2. Distribute your plugin as an Asset via GitHub Releases
3. Tag the GitHub Release with the same version as the plugin

The zip archive must:

1. Follow the naming convention
2. Contain a folder with the same name as the plugin
3. Contain the the DLL, images and script from this NuGet package
4. Not contain any `PowerToys` or `Wox` DLLs

The `plugin.json` file must have:

1. A `Name` that matches the zip archive filename
4. A `Version` that matches the GitHub Release Tag
3. The `Website` set to the GitHub Repo URL where the plugin is distributed
4. `DynamicLoading` set to `true`

Zip archive naming convention:

- `<name>-<version>-<platform>.zip`

where:

- `<name>` is the name of the plugin and must match the `Name` in `plugin.json`
- `<version>` is the plugin version and should match the `Version` in `plugin.json`
- `<platform>` is `x64` or `arm64` depending on the operating system the plugin was built for

Zip archives must contain:

- `<name>`
    - `Images`
        - `update.light.png`
        - `update.dark.png`
    - `Community.PowerToys.Run.Plugin.Update.dll`
    - `update.ps1`

where:

- `<name>` is a folder with the same name as the plugin, i.e. must match the `Name` in `plugin.json`

Zip archives should not contain:

- `PowerToys.Common.UI.dll`
- `PowerToys.ManagedCommon.dll`
- `PowerToys.Settings.UI.Lib.dll`
- `Wox.Infrastructure.dll`
- `Wox.Plugin.dll`

Further reading:

- [Community plugin checklist](https://github.com/hlaueriksson/awesome-powertoys-run-plugins/blob/main/checklist.md)

## Caveats

Adding `Community.PowerToys.Run.Plugin.Update` to your plugin adds ~3 seconds overhead during `Init`.

Check the logs for benchmarks:

- `%LocalAppData%\Microsoft\PowerToys\PowerToys Run\Logs\<Version>\`

Example with [GEmojiSharp.PowerToysRun](https://github.com/hlaueriksson/GEmojiSharp/tree/master/src/GEmojiSharp.PowerToysRun):

```diff
-   Load cost for <GEmojiSharp> is <3ms>
+   Load cost for <GEmojiSharp> is <9ms>
-   Total initialize cost for <GEmojiSharp> is <3ms>
+   Total initialize cost for <GEmojiSharp> is <2969ms>
```

## Sample

The [Sample](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/tree/main/samples/Community.PowerToys.Run.Plugin.Sample) project showcases how to use the `Community.PowerToys.Run.Plugin.Update` NuGet package.

```csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0-windows10.0.22621.0</TargetFramework>
    <UseWPF>true</UseWPF>
    <Platforms>x64;ARM64</Platforms>
    <PlatformTarget>$(Platform)</PlatformTarget>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Community.PowerToys.Run.Plugin.Update" Version="0.3.0" />
  </ItemGroup>

  <ItemGroup>
    <None Include="plugin.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="Images/*.png">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

- Reference the latest version of `Community.PowerToys.Run.Plugin.Update`

```json
{
  "ID": "0F13EFB04E5749BD92B8FA3B4353F5A6",
  "ActionKeyword": "sample",
  "IsGlobal": false,
  "Name": "Sample",
  "Author": "hlaueriksson",
  "Version": "0.3.0",
  "Language": "csharp",
  "Website": "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update",
  "ExecuteFileName": "Community.PowerToys.Run.Plugin.Sample.dll",
  "IcoPathDark": "Images\\sample.dark.png",
  "IcoPathLight": "Images\\sample.light.png",
  "DynamicLoading": true
}
```

- The `Name` must match the zip archive filename
- The `Version` must match the GitHub Release Tag and should match the zip archive filename
- The `Website` must be the URL of the GitHub Repo
- Enable `DynamicLoading`

```cs
public class SampleSettings
{
    public PluginUpdateSettings Update { get; set; } = new PluginUpdateSettings { ResultScore = 100 };

    internal IEnumerable<PluginAdditionalOption> GetAdditionalOptions() => Update.GetAdditionalOptions();

    internal void SetAdditionalOptions(IEnumerable<PluginAdditionalOption> additionalOptions) => Update.SetAdditionalOptions(additionalOptions);
}
```

Create a settings class for the plugin that has:

- A `PluginUpdateSettings` property
    - You can use `ResultScore` to control the sort order of the "update result" in the PowerToys Run UI
- Methods that invokes `GetAdditionalOptions` and `SetAdditionalOptions` from the `PluginUpdateSettings` property
    - PowerToys will use the options from `GetAdditionalOptions` to populate the settings in the UI
    - PowerToys will use `SetAdditionalOptions` to update the plugin settings from the UI

```cs
public class Main : IPlugin, IContextMenu, ISettingProvider, ISavable, IDisposable
{
    public Main()
    {
        Storage = new PluginJsonStorage<SampleSettings>();
        Settings = Storage.Load();

        Updater = new PluginUpdateHandler(Settings.Update);
        Updater.UpdateInstalling += OnUpdateInstalling;
        Updater.UpdateInstalled += OnUpdateInstalled;
        Updater.UpdateSkipped += OnUpdateSkipped;
    }

    public static string PluginID => "0F13EFB04E5749BD92B8FA3B4353F5A6";

    public string Name => "Sample";

    public string Description => "Sample Description";

    public IEnumerable<PluginAdditionalOption> AdditionalOptions => Settings.GetAdditionalOptions();

    private PluginJsonStorage<SampleSettings> Storage { get; }

    private SampleSettings Settings { get; }

    private IPluginUpdateHandler Updater { get; }

    private PluginInitContext? Context { get; set; }

    private string? IconPath { get; set; }

    private bool Disposed { get; set; }

    public List<Result> Query(Query query)
    {
        var results = new List<Result>();

        if (Updater.IsUpdateAvailable())
        {
            results.AddRange(Updater.GetResults());
        }

        results.AddRange(
        [
            new Result
            {
                IcoPath = IconPath,
                Title = "1. Lower the version of this plugin by editing the plugin.json file",
                SubTitle = @"%LocalAppData%\Microsoft\PowerToys\PowerToys Run\Plugins\Sample\plugin.json",
            },
            new Result
            {
                IcoPath = IconPath,
                Title = "2. Restart PowerToys to reload the plugin",
                SubTitle = "Exit PowerToys from Windows System Tray, start PowerToys from the Windows Start Menu",
            },
            new Result
            {
                IcoPath = IconPath,
                Title = "3. You should now be able to update the plugin",
                SubTitle = "Select and press Enter on \"Sample v0.3.0 - Update available\"",
            },
        ]);

        return results;
    }

    public void Init(PluginInitContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(Context.API.GetCurrentTheme());

        Updater.Init(Context);
    }

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        var results = Updater.GetContextMenuResults(selectedResult);
        if (results.Count != 0)
        {
            return results;
        }

        return [];
    }

    public Control CreateSettingPanel() => throw new NotImplementedException();

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings.SetAdditionalOptions(settings.AdditionalOptions);
        Save();
    }

    public void Save() => Storage.Save();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed || !disposing)
        {
            return;
        }

        if (Context?.API != null)
        {
            Context.API.ThemeChanged -= OnThemeChanged;
        }

        Updater.Dispose();

        Disposed = true;
    }

    private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/sample.light.png" : "Images/sample.dark.png";

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

    private void OnUpdateInstalling(object? sender, PluginUpdateEventArgs e)
    {
        Log.Info("UpdateInstalling: " + e.Version, GetType());
    }

    private void OnUpdateInstalled(object? sender, PluginUpdateEventArgs e)
    {
        Log.Info("UpdateInstalled: " + e.Version, GetType());
        Context!.API.ShowNotification($"{Name} {e.Version}", "Update installed");
    }

    private void OnUpdateSkipped(object? sender, PluginUpdateEventArgs e)
    {
        Log.Info("UpdateSkipped: " + e.Version, GetType());
        Save();
        Context?.API.ChangeQuery(Context.CurrentPluginMetadata.ActionKeyword, true);
    }
}
```

Update the `Main` class with these changes:

- Implement the `IContextMenu`, `ISettingProvider`, `ISavable` interfaces
- Create storage and load settings in the constructor
- Create a `PluginUpdateHandler` and subscribe the update events in the constructor
- When implementing the `ISettingProvider` interface, use the methods defined in the settings class
    - Make sure to save the settings in the `UpdateSettings` method
- In the `Query` method, use the `IsUpdateAvailable` and `GetResults` methods from the `PluginUpdateHandler`
- In the `Init` method, invoke `Init` in the `PluginUpdateHandler` and pass the `PluginInitContext`
- When implementing the `IContextMenu` interface, use the `GetContextMenuResults` method from the `PluginUpdateHandler`
- By implementing the `ISavable` interface, you can make sure that the plugin settings are saved when they change
- In the `Dispose` method, invoke `Dispose` in the `PluginUpdateHandler`
- The update events can be used to communicate with the user
    - `UpdateInstalling` is raised when the user starts the installation
    - `UpdateInstalled` is raised when the installation is complete
        - This is after PowerToys has been restarted and the user has activated the plugin again
        - This is a good time to `ShowNotification`
    - `UpdateSkipped` is raised when the user decides to skip updates to the latest version
        - Make sure to save the settings
        - This is a good time to `ChangeQuery` to refresh the UI

## Usage

If the latest version (GitHub Release Tag) is greater than (`>`) the current version (`Version` in `plugin.json`), then an "update result" is displayed in the PowerToys Run UI.

GitHub Release Tag:

![GitHub Release Tag](https://raw.githubusercontent.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/main/github-release-tag.png)

`Version` in `plugin.json`:

![plugin.json Version](https://raw.githubusercontent.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/main/plugin-json-version.png)

Update available:

![PowerToys Run - Update available](https://raw.githubusercontent.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/main/ptrun.png)

The user can:

- View release notes
- Install update
- Skip update

The update is installed via a [PowerShell script](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/blob/main/src/Community.PowerToys.Run.Plugin.Update/update.ps1).

![User Account Control](https://raw.githubusercontent.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/main/uac.png)

- The installation requires the script to run as administrator

![Administrator: Windows PowerShell](https://raw.githubusercontent.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/main/ps1.png)

- The output of the script

## Settings

1. Open PowerToys Settings
2. Click PowerToys Run in the menu to the left
3. Scroll down to the Plugins section
4. Expand the given plugin

![PowerToys Settings](https://raw.githubusercontent.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/main/ptrun-settings.png)

- The user can disable updates
    - You can toggle this setting on and off to reset a previously skipped update

## Log

During installation, an `update.log` file is written to the plugin folder:

```txt
**********************
Windows PowerShell transcript start
Start time: 20240807183206
Username: DESKTOP-SHOAM2C\Henrik
RunAs User: DESKTOP-SHOAM2C\Henrik
Configuration Name: 
Machine: DESKTOP-SHOAM2C (Microsoft Windows NT 10.0.19045.0)
Host Application: C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe -ExecutionPolicy Bypass -File C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\Sample\update.ps1 https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/releases/download/v0.1.0/Sample-0.1.0-x64.zip
Process ID: 2124
PSVersion: 5.1.19041.4648
PSEdition: Desktop
PSCompatibleVersions: 1.0, 2.0, 3.0, 4.0, 5.0, 5.1.19041.4648
BuildVersion: 10.0.19041.4648
CLRVersion: 4.0.30319.42000
WSManStackVersion: 3.0
PSRemotingProtocolVersion: 2.3
SerializationVersion: 1.1.0.1
**********************
Transcript started, output file is C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\Sample\update.log
2024-08-07 18:32:06 Update plugin...
2024-08-07 18:32:06 AssetUrl: https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/releases/download/v0.1.0/Sample-0.1.0-x64.zip
2024-08-07 18:32:06 PluginDirectory: C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\Sample
2024-08-07 18:32:06 Log: C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\Sample\update.log
2024-08-07 18:32:06 AssetName: Sample-0.1.0-x64.zip
2024-08-07 18:32:06 Kill PowerToys
2024-08-07 18:32:07 Download release
2024-08-07 18:32:07 Hash: 5F7F0172D7EC6FD38CB52D4D8C1F1B224BC0F7C61F275A942F3EBF876DDC10A4
2024-08-07 18:32:07 Latest: https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/releases/latest
2024-08-07 18:32:08 Hash is verified
2024-08-07 18:32:08 Deletes plugin files
2024-08-07 18:32:08 Extract release
2024-08-07 18:32:09 Start PowerToys
2024-08-07 18:32:09 Update complete!
**********************
Windows PowerShell transcript end
End time: 20240807183209
**********************
```

## Community

Community plugins that use this package:

- [GEmojiSharp.PowerToysRun](https://github.com/hlaueriksson/GEmojiSharp/tree/master/src/GEmojiSharp.PowerToysRun)

## Disclaimer

This is not an official Microsoft PowerToys package.
