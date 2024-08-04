using Microsoft.PowerToys.Settings.UI.Library;

namespace Community.PowerToys.Run.Plugin.Update
{
    /// <summary>
    /// Settings for plugin updates.
    /// </summary>
    public class PluginUpdateSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the plugin should disable updates.
        /// </summary>
        public bool DisableUpdates { get; set; }

        /// <summary>
        /// Gets or sets whether the plugin should skip updates of a specific version.
        /// </summary>
        public string? SkipUpdate { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the last update was installed.
        /// </summary>
        public DateTime? UpdateTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the plugin update result score.
        /// </summary>
        public int ResultScore { get; set; }

        /// <summary>
        /// Gets additional options for the UI settings.
        /// </summary>
        /// <returns>An enumeration of additional options.</returns>
#pragma warning disable CA1024 // Use properties where appropriate
        public IEnumerable<PluginAdditionalOption> GetAdditionalOptions() =>
#pragma warning restore CA1024 // Use properties where appropriate
        [
            new()
            {
                Key = nameof(PluginUpdateSettings) + nameof(DisableUpdates),
                DisplayLabel = "Disable updates",
                DisplayDescription = "Disable updates of the plugin",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Value = false,
            },
        ];

        /// <summary>
        /// Sets additional options from the UI settings.
        /// </summary>
        /// <param name="additionalOptions">An enumeration of additional options.</param>
        public void SetAdditionalOptions(IEnumerable<PluginAdditionalOption> additionalOptions)
        {
            ArgumentNullException.ThrowIfNull(additionalOptions);

            var options = additionalOptions.ToList();

            DisableUpdates = options.Find(x => x.Key == nameof(PluginUpdateSettings) + nameof(DisableUpdates))?.Value ?? false;
            if (DisableUpdates)
            {
                SkipUpdate = null; // NOTE: toggle
            }
        }
    }
}
