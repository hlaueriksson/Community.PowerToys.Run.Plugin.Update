using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.Update;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure.Storage;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.Sample
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, ISettingProvider, ISavable, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main()
        {
            Storage = new PluginJsonStorage<SampleSettings>();
            Settings = Storage.Load();

            Updater = new PluginUpdateHandler(Settings.Update);
            Updater.UpdateInstalling += OnUpdateInstalling;
            Updater.UpdateInstalled += OnUpdateInstalled;
            Updater.UpdateSkipped += OnUpdateSkipped;
        }

        /// <summary>
        /// Gets the ID of the plugin.
        /// </summary>
        public static string PluginID => "0F13EFB04E5749BD92B8FA3B4353F5A6";

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public string Name => "Sample";

        /// <summary>
        /// Gets the description of the plugin.
        /// </summary>
        public string Description => "Sample Description";

        /// <summary>
        /// Gets additional options for the plugin.
        /// </summary>
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => Settings.GetAdditionalOptions();

        private PluginJsonStorage<SampleSettings> Storage { get; }

        private SampleSettings Settings { get; }

        private IPluginUpdateHandler Updater { get; }

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
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
                    SubTitle = "Select and press Enter on \"Sample v0.4.0 - Update available\"",
                },
            ]);

            return results;
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            Updater.Init(Context);
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var results = Updater.GetContextMenuResults(selectedResult);
            if (results.Count != 0)
            {
                return results;
            }

            return [];
        }

        /// <summary>
        /// Creates setting panel.
        /// </summary>
        /// <returns>The control.</returns>
        /// <exception cref="NotImplementedException">method is not implemented.</exception>
        public Control CreateSettingPanel() => throw new NotImplementedException();

        /// <summary>
        /// Updates settings.
        /// </summary>
        /// <param name="settings">The plugin settings.</param>
        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            Settings.SetAdditionalOptions(settings.AdditionalOptions);
            Save();
        }

        /// <summary>
        /// Saves settings.
        /// </summary>
        public void Save() => Storage.Save();

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
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
}
