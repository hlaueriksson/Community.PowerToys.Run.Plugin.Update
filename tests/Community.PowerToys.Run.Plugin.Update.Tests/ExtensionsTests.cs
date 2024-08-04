using FluentAssertions;
using NUnit.Framework;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Update.Tests
{
    public class ExtensionsTests
    {
        [Test]
        public void GetGitHubOptions_should_parse_Website()
        {
            new PluginMetadata { Website = "https://github.com/hlaueriksson/GEmojiSharp" }.GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions { Owner = "hlaueriksson", Repo = "GEmojiSharp" });

            new PluginMetadata { Website = "https://gitfail.com/hlaueriksson/GEmojiSharp" }.GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions());

            new PluginMetadata { Website = "" }.GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions());

            new PluginMetadata().GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions());
        }

        [TestCase(-1, "-1 bytes")]
        [TestCase(0, "0 bytes")]
        [TestCase(1, "1 bytes")]
        [TestCase(1024, "1 KB")]
        [TestCase(1024 * 1024, "1 MB")]
        [TestCase(1024 * 1024 * 1024, "1 GB")]
        [TestCase(int.MaxValue, "2 GB")]
        public void FormatSize_should_format_file_size(int bytes, string expected)
        {
            bytes.FormatSize().Should().Be(expected);
        }
    }
}
