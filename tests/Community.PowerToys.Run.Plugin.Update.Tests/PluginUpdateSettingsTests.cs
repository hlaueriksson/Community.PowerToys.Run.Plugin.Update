using FluentAssertions;
using Microsoft.PowerToys.Settings.UI.Library;
using NUnit.Framework;

namespace Community.PowerToys.Run.Plugin.Update.Tests
{
    public class PluginUpdateSettingsTests
    {
        [Test]
        public void GetAdditionalOptions_should_reflect_settings()
        {
            var settings = new PluginUpdateSettings();

            var result = settings.GetAdditionalOptions();
            result.ElementAt(0).Should().Match<PluginAdditionalOption>(x => x.Key == "PluginUpdateSettingsDisableUpdates");
        }

        [Test]
        public void SetAdditionalOptions_should_update_settings()
        {
            var options = new[]
            {
                new PluginAdditionalOption
                {
                    Key = "PluginUpdateSettingsDisableUpdates",
                    Value = true,
                },
            };

            var settings = new PluginUpdateSettings
            {
                SkipUpdate = "v4.0.0",
            };
            settings.SetAdditionalOptions(options);
            settings.DisableUpdates.Should().BeTrue();
            settings.SkipUpdate.Should().BeNull();
        }
    }
}
