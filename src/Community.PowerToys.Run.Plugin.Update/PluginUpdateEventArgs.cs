namespace Community.PowerToys.Run.Plugin.Update
{
    /// <summary>
    /// Provides data for the <see cref="IPluginUpdateHandler"/> events.
    /// </summary>
    public class PluginUpdateEventArgs(string version, DateTime timestamp) : EventArgs
    {
        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        public string Version { get; } = version;

        /// <summary>
        /// Gets the update timestamp.
        /// </summary>
        public DateTime Timestamp { get; } = timestamp;
    }
}
