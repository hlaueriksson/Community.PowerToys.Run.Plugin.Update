using System.Runtime.InteropServices;
using System.Windows.Input;
using ManagedCommon;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Common;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.Update
{
    /// <summary>
    /// Handles the update of a plugin.
    /// </summary>
    public interface IPluginUpdateHandler : IDisposable
    {
        /// <summary>
        /// Occurs when the plugin update is installing.
        /// </summary>
        event EventHandler<PluginUpdateEventArgs>? UpdateInstalling;

        /// <summary>
        /// Occurs when the plugin update has been installed.
        /// </summary>
        event EventHandler<PluginUpdateEventArgs>? UpdateInstalled;

        /// <summary>
        /// Occurs when the plugin update has been skipped.
        /// </summary>
        event EventHandler<PluginUpdateEventArgs>? UpdateSkipped;

        /// <summary>
        /// Initialize the handler with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for the plugin to update.</param>
        /// <returns><c>true</c> if successfully initialized; otherwise, <c>false</c>.</returns>
        bool Init(PluginInitContext context);

        /// <summary>
        /// Determines if an update is available for the plugin.
        /// </summary>
        /// <returns><c>true</c> if an update is available; otherwise, <c>false</c>.</returns>
        bool IsUpdateAvailable();

        /// <summary>
        /// Gets a update result for <see cref="IPlugin.Query"/>, if an update is available for the plugin.
        /// </summary>
        /// <returns>A list of one update result, or an empty list.</returns>
        List<Result> GetResults();

        /// <summary>
        /// Gets a context menu update result for <see cref="IContextMenu.LoadContextMenus"/>, if an update is available for the plugin.
        /// </summary>
        /// <param name="selectedResult">The selected <see cref="Result"/>.</param>
        /// <returns>A list of one context menu update result, or an empty list.</returns>
        List<ContextMenuResult> GetContextMenuResults(Result selectedResult);
    }

    /// <inheritdoc/>
    public sealed class PluginUpdateHandler(PluginUpdateSettings settings) : IPluginUpdateHandler
    {
        /// <inheritdoc/>
        public event EventHandler<PluginUpdateEventArgs>? UpdateInstalling;

        /// <inheritdoc/>
        public event EventHandler<PluginUpdateEventArgs>? UpdateInstalled;

        /// <inheritdoc/>
        public event EventHandler<PluginUpdateEventArgs>? UpdateSkipped;

        private PluginUpdateSettings Settings { get; } = settings;

        private PluginMetadata Metadata { get; set; } = null!;

        private IPublicAPI Api { get; set; } = null!;

        private string? IconPath { get; set; }

        private Release? LatestRelease { get; set; }

        private Asset? LatestAsset { get; set; }

        /// <inheritdoc/>
        public bool Init(PluginInitContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Metadata = context.CurrentPluginMetadata;
            Api = context.API;
            Api.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Api.GetCurrentTheme());

            LatestRelease = null;
            LatestAsset = null;

            if (Settings.DisableUpdates)
            {
                return false;
            }

            try
            {
                var client = new GitHubClient(Metadata.GetGitHubOptions());

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                LatestRelease = client.GetLatestReleaseAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Exception("GitHubClient failed.", ex, GetType());
            }

            if (LatestRelease == null)
            {
                return false;
            }

            LatestAsset = LatestRelease.assets.FirstOrDefault(x =>
                x.name.Contains(Metadata.Name, StringComparison.OrdinalIgnoreCase) &&
                x.name.Contains(RuntimeInformation.ProcessArchitecture.ToString(), StringComparison.OrdinalIgnoreCase) &&
                x.content_type.Contains("zip", StringComparison.OrdinalIgnoreCase));

            return LatestAsset != null;
        }

        /// <inheritdoc/>
        public bool IsUpdateAvailable()
        {
            if (Settings.DisableUpdates)
            {
                return false;
            }

            if (LatestRelease == null || LatestRelease.tag_name == Settings.SkipUpdate)
            {
                return false;
            }

            if (LatestAsset == null)
            {
                return false;
            }

            var latestVersion = new Version(LatestRelease.tag_name.Substring(1));
            var currentVersion = new Version(Metadata.Version);

            if (UpdateWasInstalled())
            {
                UpdateInstalled?.Invoke(this, new PluginUpdateEventArgs(LatestRelease.tag_name, Settings.UpdateTimestamp!.Value));
                Settings.UpdateTimestamp = null;
            }

            return latestVersion > currentVersion;

            bool UpdateWasInstalled()
            {
                return
                    latestVersion == currentVersion &&
                    Settings.UpdateTimestamp.HasValue &&
                    (DateTime.Now - Settings.UpdateTimestamp) <= TimeSpan.FromHours(1);
            }
        }

        /// <inheritdoc/>
        public List<Result> GetResults()
        {
            if (!IsUpdateAvailable())
            {
                return [];
            }

            return
            [
                new()
                {
                    IcoPath = IconPath,
                    Title = $"{Metadata.Name} {LatestRelease!.tag_name} - Update available",
                    SubTitle = "Download and install update",
                    ToolTipData = new ToolTipData($"{Metadata.Name} {LatestRelease.tag_name}", $"Tag: {LatestRelease.tag_name}\nPublished: {LatestRelease.published_at}\nFilename: {LatestAsset!.name}\nSize: {LatestAsset.size.FormatSize()}\n"),
                    Action = _ => InstallUpdate(),
                    ContextData = LatestRelease,
                    Score = Settings.ResultScore,
                }
            ];
        }

        /// <inheritdoc/>
        public List<ContextMenuResult> GetContextMenuResults(Result selectedResult)
        {
            if (selectedResult?.ContextData is not Release release)
            {
                return [];
            }

            return
            [
                new()
                {
                    PluginName = Metadata.Name,
                    Title = "View release notes",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xF6FA", // WebSearch
                    AcceleratorKey = Key.Home,
                    Action = _ => OpenInBrowser(release.html_url),
                },
                new()
                {
                    PluginName = Metadata.Name,
                    Title = $"Install update {release.tag_name}",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE777", // UpdateRestore
                    /*AcceleratorKey = Key.Enter,*/
                    Action = _ => InstallUpdate(),
                },
                new()
                {
                    PluginName = Metadata.Name,
                    Title = $"Skip update {release.tag_name}",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE711", // Cancel
                    AcceleratorKey = Key.End,
                    Action = _ => SkipUpdate(),
                },
            ];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Api != null)
            {
                Api.ThemeChanged -= OnThemeChanged;
            }
        }

        internal bool OpenInBrowser(string url)
        {
            if (!Helper.OpenCommandInShell(DefaultBrowserInfo.Path, DefaultBrowserInfo.ArgumentsPattern, url))
            {
                Log.Error("Open default browser failed.", GetType());
                Api.ShowMsg($"Plugin: {Metadata.Name}", "Open default browser failed.");
                return false;
            }

            return true;
        }

        internal bool InstallUpdate()
        {
            ArgumentNullException.ThrowIfNull(LatestRelease);
            ArgumentNullException.ThrowIfNull(LatestAsset);

            Settings.UpdateTimestamp = DateTime.Now;
            UpdateInstalling?.Invoke(this, new PluginUpdateEventArgs(LatestRelease.tag_name, Settings.UpdateTimestamp.Value));

            const string path = "powershell.exe";
            var arguments = $"-ExecutionPolicy Bypass -File \"{Metadata.PluginDirectory}\\update.ps1\" \"{LatestAsset.browser_download_url}\"";
            Log.Debug($"OpenInShell: {path} {arguments}", GetType());
            if (!Helper.OpenInShell(path, arguments, Metadata.PluginDirectory, Helper.ShellRunAsType.Administrator))
            {
                Log.Error("Run update.ps1 failed.", GetType());
                Api.ShowMsg($"Plugin: {Metadata.Name}", "Run update.ps1 failed.");
                return false;
            }

            return true;
        }

        internal bool SkipUpdate()
        {
            ArgumentNullException.ThrowIfNull(LatestRelease);
            ArgumentNullException.ThrowIfNull(LatestAsset);

            Settings.SkipUpdate = LatestRelease.tag_name;
            UpdateSkipped?.Invoke(this, new PluginUpdateEventArgs(LatestRelease.tag_name, DateTime.Now));

            return true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/update.light.png" : "Images/update.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
