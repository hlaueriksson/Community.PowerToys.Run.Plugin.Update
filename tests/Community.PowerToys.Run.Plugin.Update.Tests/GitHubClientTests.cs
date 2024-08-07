using FluentAssertions;
using NUnit.Framework;

namespace Community.PowerToys.Run.Plugin.Update.Tests
{
    public class GitHubClientTests
    {
        [Test]
        public void Ctor_should_throw_if_options_is_null()
        {
            Action act = () => new GitHubClient(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_should_throw_if_options_Owner_is_null()
        {
            Action act = () => new GitHubClient(new GitHubOptions { Repo = "Repo" });
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Ctor_should_throw_if_options_Repo_is_null()
        {
            Action act = () => new GitHubClient(new GitHubOptions { Owner = "Owner" });
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public async Task GetLatestReleaseAsync_should_return_latest_release()
        {
            var options = new GitHubOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Update" };
            var subject = new GitHubClient(options);

            var result = await subject.GetLatestReleaseAsync();

            result.Should().NotBeNull();
        }

        [Test]
        public async Task GetLatestReleaseAsync_should_throw_if_Owner_is_invalid()
        {
            var options = new GitHubOptions { Owner = "userthatdoesnotexist", Repo = "Community.PowerToys.Run.Plugin.Update" };
            var subject = new GitHubClient(options);

            Func<Task> act = () => subject.GetLatestReleaseAsync();
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Test]
        public async Task GetLatestReleaseAsync_should_throw_if_Repo_is_invalid()
        {
            var options = new GitHubOptions { Owner = "hlaueriksson", Repo = "repothatdoesnotexist" };
            var subject = new GitHubClient(options);

            Func<Task> act = () => subject.GetLatestReleaseAsync();
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
