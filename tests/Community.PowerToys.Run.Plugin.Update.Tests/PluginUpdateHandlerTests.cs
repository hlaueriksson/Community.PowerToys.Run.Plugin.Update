using System.Reflection;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Update.Tests
{
    public class PluginUpdateHandlerTests
    {
        [Test]
        public void Init_should_return_true_if_successfully_initialized()
        {
            var subject = GetSubject(new());
            var metadata = new PluginMetadata
            {
                Name = "Sample",
                Website = "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update",
            };
            var context = GetContext(metadata);

            var result = subject.Init(context);
            result.Should().BeTrue();
        }

        [Test]
        public void Init_should_return_false_if_settings_has_DisableUpdates()
        {
            var subject = GetSubject(new() { DisableUpdates = true });
            var context = GetContext(new());

            var result = subject.Init(context);
            result.Should().BeFalse();
        }

        [Test]
        public void Init_should_throw_if_context_is_null()
        {
            Action act = () => GetSubject(new()).Init(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Init_should_return_false_if_metadata_Website_is_invalid()
        {
            var subject = GetSubject(new());
            var metadata = new PluginMetadata
            {
                Name = "Sample",
                Website = "https://gitfail.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update",
            };
            var context = GetContext(metadata);

            var result = subject.Init(context);
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpdateAvailable_should_return_true_if_the_latest_version_is_greater_than_the_current_version()
        {
            var metadata = new PluginMetadata { Version = "0.0.9" };
            var release = new Release { tag_name = "v0.1.0" };
            var subject = GetSubject(new(), metadata, release, new());

            var result = subject.IsUpdateAvailable();
            result.Should().BeTrue();
        }

        [Test]
        public void IsUpdateAvailable_should_raise_event_if_UpdateWasInstalled()
        {
            var settings = new PluginUpdateSettings { UpdateTimestamp = DateTime.Now };
            var metadata = new PluginMetadata { Version = "0.1.0" };
            var release = new Release { tag_name = "v0.1.0" };
            var subject = GetSubject(settings, metadata, release, new());
            using var monitoredSubject = subject.Monitor();

            subject.IsUpdateAvailable();
            monitoredSubject.Should().Raise(nameof(subject.UpdateInstalled));
        }

        [Test]
        public void GetResults_should_return_result_if_update_is_available()
        {
            var metadata = new PluginMetadata { Version = "0.0.9" };
            var release = new Release { tag_name = "v0.1.0" };
            var subject = GetSubject(new(), metadata, release, new());

            var result = subject.GetResults();
            result.Should().NotBeEmpty();
        }

        [Test]
        public void GetResults_should_return_empty_list_if_no_update_is_available()
        {
            var subject = GetSubject(new());

            var result = subject.GetResults();
            result.Should().BeEmpty();
        }

        [Test]
        public void GetContextMenuResults_should_return_result_if_ContextData_is_Release()
        {
            var subject = GetSubject(new(), new());

            var result = subject.GetContextMenuResults(new Result { ContextData = new Release() });
            result.Should().NotBeEmpty();
        }

        [Test]
        public void GetContextMenuResults_should_return_empty_list_if_ContextData_is_not_Release()
        {
            var subject = GetSubject(new(), new());

            var result = subject.GetContextMenuResults(new Result { ContextData = new object() });
            result.Should().BeEmpty();
        }

        private static PluginUpdateHandler GetSubject(PluginUpdateSettings settings, PluginMetadata? metadata = null, Release? release = null, Asset? asset = null)
        {
            var result = new PluginUpdateHandler(settings);
            if (metadata != null) SetPropertyValue("Metadata", metadata);
            if (release != null) SetPropertyValue("LatestRelease", release);
            if (asset != null) SetPropertyValue("LatestAsset", asset);

            return result;

            void SetPropertyValue(string name, object value)
            {
                result.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(result, value);
            }
        }

        private static PluginInitContext GetContext(PluginMetadata metadata)
        {
            var result = new PluginInitContext { API = Substitute.For<IPublicAPI>() };
            result.GetType().GetProperty(nameof(result.CurrentPluginMetadata))!.SetValue(result, metadata);

            return result;
        }
    }
}
