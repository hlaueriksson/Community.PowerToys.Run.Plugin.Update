# Community.PowerToys.Run.Plugin.Update

[![build](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/actions/workflows/build.yml/badge.svg)](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/actions/workflows/build.yml)
[![Community.PowerToys.Run.Plugin.Update](https://img.shields.io/nuget/v/Community.PowerToys.Run.Plugin.Update.svg?label=Community.PowerToys.Run.Plugin.Update)](https://www.nuget.org/packages/Community.PowerToys.Run.Plugin.Update)

This NuGet package adds support for updating PowerToys Run Plugins.

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
<PackageReference Include="Community.PowerToys.Run.Plugin.Update" Version="0.1.0" />
```

## Example

Example of a `.csproj` file:

```csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Platforms>x64;ARM64</Platforms>
    <PlatformTarget>$(Platform)</PlatformTarget>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Community.PowerToys.Run.Plugin.Update" Version="0.1.0" />
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

Remember to enable `DynamicLoading` in the `plugin.json` file:

```json
"DynamicLoading": true
```

Example of a `Main.cs` file:

```cs
public class Main : IPlugin, IContextMenu, ISettingProvider, ISavable, IDisposable
{
    public Main()
    {
        Storage = new PluginJsonStorage<Plugin1Settings>();
        Settings = Storage.Load();
        Updater = new PluginUpdateHandler(Settings.Update);
        Updater.UpdateInstalling += OnUpdateInstalling;
        Updater.UpdateInstalled += OnUpdateInstalled;
        Updater.UpdateSkipped += OnUpdateSkipped;
    }

    public static string PluginID => "00000000000000000000000000000000";

    public string Name => "Plugin1";

    public string Description => "Plugin1 Description";

    public IEnumerable<PluginAdditionalOption> AdditionalOptions => Settings.GetAdditionalOptions();

    private PluginJsonStorage<Plugin1Settings> Storage { get; }

    private Plugin1Settings Settings { get; }

    private PluginUpdateHandler Updater { get; }

    private PluginInitContext? Context { get; set; }

    private string? IconPath { get; set; }

    private bool Disposed { get; set; }

    public List<Result> Query(Query query)
    {
        if (Updater.IsUpdateAvailable())
        {
            return Updater.GetResults();
        }

        // TODO: implement
        return [];
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
        if (results.Count != 0) return results;

        // TODO: implement
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

    private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/plugin1.light.png" : "Images/plugin1.dark.png";

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

Example of a `Settings` class:

```cs
public class Plugin1Settings
{
    public PluginUpdateSettings Update { get; set; } = new PluginUpdateSettings();

    internal IEnumerable<PluginAdditionalOption> GetAdditionalOptions() => Update.GetAdditionalOptions();

    internal void SetAdditionalOptions(IEnumerable<PluginAdditionalOption> additionalOptions) => Update.SetAdditionalOptions(additionalOptions);
}
```

## Disclaimer

This is not an official Microsoft PowerToys package.
