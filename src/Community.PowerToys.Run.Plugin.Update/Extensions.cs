using System.Text.RegularExpressions;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Update
{
    internal static partial class Extensions
    {
        public static GitHubOptions GetGitHubOptions(this PluginMetadata metadata)
        {
            var result = new GitHubOptions();

            if (metadata.Website == null)
            {
                return result;
            }

            var match = GitHubRegex().Match(metadata.Website);
            if (match.Success)
            {
                result.Owner = match.Groups[1].Value;
                result.Repo = match.Groups[2].Value;
            }

            return result;
        }

        public static string FormatSize(this int bytes)
        {
            string[] suffixes = ["bytes", "KB", "MB", "GB"];

            if (bytes < 0)
            {
                return "-" + FormatSize(-bytes);
            }

            int i = 0;
            decimal d = bytes;
            while (Math.Round(d) >= 1000)
            {
                d /= 1024;
                i++;
            }

            return $"{d:n0} {suffixes[i]}";
        }

        [GeneratedRegex(@"https:\/\/github.com\/([^\/]+)\/([^\/]+)$")]
        private static partial Regex GitHubRegex();
    }
}
