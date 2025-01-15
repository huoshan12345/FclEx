using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using FclEx.Helpers;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
// ReSharper disable MemberCanBePrivate.Local

namespace FclEx.Build;

public record RepoInfo(string RootPath, string SolutionPath, string Name, string BasicText);

public class ReadmeGenerator
{
    public const string UserUrl = "https://github.com/huoshan12345";
    public const string FclEx = "FclEx";
    public const string Collaboration = "FclEx.Collaboration";
    public const string Ini = "Ini.Net";
    public const string Uci = "Uci.Net";

    public const string LicenseBadgeUrl = "https://img.shields.io/github/license/mashape/apistatus.svg";
    public const string BuildWorkflowPath = "actions/workflows/build.yml";
    public const string BuildWorkflowBadgePath = BuildWorkflowPath + "/badge.svg";

    private static readonly string RootPath = AppContext.BaseDirectory.TakeUntil("src", false);

    public static readonly IEnumerable<object[]> SolutionPaths = new RepoInfo[]
    {
        new(RootPath, Path.Combine("src", "FclEx.All.sln"), FclEx,
            "Some basic useful extensions and helpers for C# fundamental class libraries."),
        new(Path.Combine(RootPath, "..", "FclEx.Collaboration"), "FclEx.Collaboration.sln", Collaboration,
            "Some basic useful extensions and helpers for Atlassian, NewRelic and Slack."),
        new(Path.Combine(RootPath, "..", "Ini.Net"), "Ini.Net.sln", Ini,
            "A simple and efficient parser for INI format files implemented in C#."),
        new(Path.Combine(RootPath, "..", "Uci.Net"), "Uci.Net.sln", Uci,
            "A simple and efficient parser for UCI (Unified Configuration Interface) format files implemented in C#."),
    }.Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(SolutionPaths))]
    public async Task Generate(RepoInfo repo)
    {
        var path = Path.Combine(repo.RootPath, repo.SolutionPath);

        if (File.Exists(path) == false)
            return;

        var solution = SolutionFile.Parse(path);
        Assert.NotNull(solution);

        var list = new List<(string Name, string[] TargetFrameworks)>();
        var ordered = solution.ProjectsByGuid
            .OrderBy(m => m.Value.AbsolutePath, PathComparer.Instance);

        foreach (var (key, value) in ordered)
        {
            if (value.ProjectType != SolutionProjectType.KnownToBeMSBuildFormat)
                continue;

            var projectRootElement = ProjectRootElement.Open(value.AbsolutePath);
            var project = new Project(projectRootElement);

            if (project.GetPropertyValue("IsPackable").ToBool() == false)
                continue;

            var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            var frameworks = project.GetPropertyValue("TargetFrameworks").Split(';', options);

            list.Add((value.ProjectName, frameworks));
        }

        var readmePath = new FileInfo(Path.Combine(repo.RootPath, "README.md"));
        // Assert.True(readmePath.Exists);

        var repoUri = new Uri(new Uri(UserUrl + "/"), repo.Name + "/");
        var buildWorkflowBadgeUrl = new Uri(repoUri, BuildWorkflowBadgePath).ToString();
        var buildWorkflowUrl = new Uri(repoUri, BuildWorkflowPath).ToString();

        var str = StringBuilderHelper.Build(m =>
        {
            m.AppendHeading(repo.Name, 1);
            m.Append(' ');
            m.AppendBadge("LICENSE", LicenseBadgeUrl, "LICENSE.TXT");
            m.Append(' ');
            m.AppendBadge("Build", buildWorkflowBadgeUrl, buildWorkflowUrl);
            m.AppendLine();
            m.AppendLine();
            m.Append(repo.BasicText);
            m.AppendLine();
            m.AppendHeading("Latest Builds", 2);
            m.AppendLine();
            m.AppendLine();

            var rows = new List<List<Action<StringBuilder>>>();
            foreach (var (name, frameworks) in list)
            {
                var row = new List<Action<StringBuilder>>
                {
                    m => m.Append(name),
                    x => x.AppendFrameworkBadges(frameworks, "30a14e"),
                    x => x.AppendMyGetBadge(name),
                };
                rows.Add(row);
            }
            m.AppendMarkdownTable(["", "TargetFramework", "Package"], rows);
        });

        await File.WriteAllTextAsync(readmePath.FullName, str);
    }
}

file static class Extensions
{
    public const string MyGetFeed = "huoshan12345";

    public enum TextAlignment
    {
        None,
        Left,
        Center,
        Right,
    }

    public static string ToSeparator(this TextAlignment alignment)
    {
        return alignment switch
        {
            TextAlignment.None => "----",
            TextAlignment.Left => ":----",
            TextAlignment.Center => "----:",
            TextAlignment.Right => ":----:",
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
        };
    }

    public record MarkdownColumn(string Text, TextAlignment Alignment = TextAlignment.None)
    {
        public static implicit operator MarkdownColumn(string text) => new(text);
    }

    private static readonly Regex _regFramework = new Regex(@"^([^\.]+)(\d+(?:\.\d+)?)(-\w+)?$");
    public static void AppendFrameworkBadges(this StringBuilder builder, IEnumerable<string> frameworks, string color)
    {
        foreach (var framework in frameworks)
        {
            var match = _regFramework.Match(framework);
            if (match.Success == false)
                throw new InvalidOperationException($"Cannot parse version from framework '{framework}'.");

            var target = match.Groups[1].Value;
            var version = match.Groups[2].Value;
            var platform = match.Groups[3].Value;
            builder.AppendBadge(framework, m => m.BuildStaticBadge(target + platform, version, color));
            builder.Append(' ');
        }
    }

    public static void AppendMarkdownTable(this StringBuilder builder, IEnumerable<MarkdownColumn> columns, IEnumerable<IEnumerable<Action<StringBuilder>>> rowActions)
    {
        var post = new List<Action<StringBuilder>>();
        foreach (var (_, (text, alignment), _, isLast) in columns.IndexExt())
        {
            builder.Append('|');
            builder.Append(text);
            post.Add(m => m.Append('|'));
            post.Add(m => m.Append(alignment.ToSeparator()));

            if (isLast)
            {
                builder.Append('|');
                builder.AppendLine();
                post.Add(m => m.Append('|'));
                post.Add(m => m.AppendLine());
            }
        }
        post.ForEach(m => m.Invoke(builder));

        foreach (var row in rowActions)
        {
            foreach (var (_, action, _, isLast) in row.IndexExt())
            {
                builder.Append('|');
                action(builder);

                if (isLast)
                {
                    builder.Append('|');
                    builder.AppendLine();
                }
            }
        }
    }

    public static StringBuilder BuildStaticBadge(this StringBuilder builder, string left, string right, string color)
    {
        // Example: https://img.shields.io/badge/.net-7.0-ff69b4.svg
        builder.Append("https://img.shields.io/badge/");
        builder.Append(Encode(left));
        builder.Append('-');
        builder.Append(Encode(right));
        builder.Append('-');
        builder.Append(color);
        builder.Append(".svg");
        return builder;

        static string Encode(string value)
        {
            return HttpUtility.UrlEncode(value)
                .Replace("-", "--"); // escape dash "-" as "--" according to shields.io.
        }
    }

    public static StringBuilder AppendMyGetBadge(this StringBuilder builder, string package)
    {
        return builder.AppendBadge("",
            m => m.BuildMyGetBadge(package),
            m => m.Append("https://www.myget.org/feed/huoshan12345/package/nuget/").Append(package));
    }

    public static StringBuilder BuildMyGetBadge(this StringBuilder builder, string package)
    {
        // Example: https://img.shields.io/myget/huoshan12345/v/fclex
        builder.Append("https://img.shields.io/myget/");
        builder.Append(MyGetFeed);
        builder.Append("/v/");
        builder.Append(package);
        builder.Append("?logo=myget&label=myget");
        return builder;
    }

    public static StringBuilder AppendLineBreak(this StringBuilder builder)
    {
        builder.Append(' ', 2);
        builder.AppendLine();
        return builder;
    }

    public static StringBuilder AppendHeading(this StringBuilder builder, string text, int level)
    {
        builder.Append('#', level);
        builder.Append(' ');
        builder.Append(text);
        return builder;
    }

    public static StringBuilder AppendBadge(this StringBuilder builder, string text, string badgeUrl, string? clickUrl = null)
    {
        return builder.AppendBadge(text, m => m.Append(badgeUrl), clickUrl is null ? null : m => m.Append(clickUrl));
    }

    public static StringBuilder AppendBadge(this StringBuilder builder, string text, Action<StringBuilder> badgeUrlAction, Action<StringBuilder>? clickUrlAction = null)
    {
        if (clickUrlAction is not null)
        {
            builder.Append('[');
        }

        builder.Append("![");
        builder.Append(text);
        builder.Append("](");
        badgeUrlAction(builder);
        builder.Append(')');

        if (clickUrlAction is not null)
        {
            builder.Append("](");
            clickUrlAction(builder);
            builder.Append(')');
        }
        return builder;
    }
}