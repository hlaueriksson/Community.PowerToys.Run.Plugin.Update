using System.Net.Http;
using System.Net.Http.Json;

namespace Community.PowerToys.Run.Plugin.Update
{
    /// <summary>
    /// GitHub API.
    /// </summary>
    public interface IGitHubClient
    {
        /// <summary>
        /// Get the latest release.
        /// </summary>
        /// <returns>A release.</returns>
        Task<Release?> GetLatestReleaseAsync();
    }

    /// <inheritdoc/>
    public class GitHubClient : IGitHubClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubClient"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public GitHubClient(GitHubOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentException.ThrowIfNullOrEmpty(options.Owner);
            ArgumentException.ThrowIfNullOrEmpty(options.Repo);

            Options = options;
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com"),
                Timeout = TimeSpan.FromSeconds(5),
            };
            HttpClient.DefaultRequestHeaders.Add("User-Agent", Options.Owner);
        }

        internal GitHubClient(HttpClient httpClient, GitHubOptions options)
        {
            HttpClient = httpClient;
            Options = options;
        }

        private GitHubOptions Options { get; }

        private HttpClient HttpClient { get; }

        /// <inheritdoc/>
        public async Task<Release?> GetLatestReleaseAsync()
        {
            // https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#get-the-latest-release
            return await HttpClient.GetFromJsonAsync<Release>($"/repos/{Options.Owner}/{Options.Repo}/releases/latest").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Options.
    /// </summary>
    public class GitHubOptions
    {
        /// <summary>
        /// Gets or sets the account owner of the repository.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the name of the repository.
        /// </summary>
        public string Repo { get; set; }
    }

    /*
    {
      "html_url": "https://github.com/octocat/Hello-World/releases/v1.0.0",
      "id": 1,
      "tag_name": "v1.0.0",
      "name": "v1.0.0",
      "body": "Description of the release",
      "draft": false,
      "prerelease": false,
      "created_at": "2013-02-27T19:35:32Z",
      "published_at": "2013-02-27T19:35:32Z",
      "assets": [
        {
          "browser_download_url": "https://github.com/octocat/Hello-World/releases/download/v1.0.0/example.zip",
          "id": 1,
          "name": "example.zip",
          "label": "short description",
          "state": "uploaded",
          "content_type": "application/zip",
          "size": 1024,
          "download_count": 42,
          "created_at": "2013-02-27T19:35:32Z",
          "updated_at": "2013-02-27T19:35:32Z"
        }
      ]
    }
     */

    /// <summary>
    /// Release of a GitHub repository.
    /// </summary>
    public class Release
    {
        public string html_url { get; set; }
        public int id { get; set; }
        public string tag_name { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
        public Asset[] assets { get; set; }
    }

    /// <summary>
    /// Release asset of a GitHub repository.
    /// </summary>
    public class Asset
    {
        public string browser_download_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string state { get; set; }
        public string content_type { get; set; }
        public int size { get; set; }
        public int download_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
