using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using FclEx.Helpers;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
// ReSharper disable MemberCanBePrivate.Local

namespace FclEx.Build;

public class ReadmeGenerator
{
    public const string LicenseBadgeUrl = "https://img.shields.io/github/license/mashape/apistatus.svg";
    public const string BuildWorkflowUrl = "https://github.com/huoshan12345/FclEx/actions/workflows/build.yml";
    public const string BuildWorkflowBadgeUrl = BuildWorkflowUrl + "/badge.svg";

    [LocalOnlyFact]
    public async Task Generate()
    {
        var src = AppContext.BaseDirectory.TakeUntil("src");
        var dirs = new[]
        {
            Path.Combine(src, "FclEx", "src"),
            Path.Combine(src, "FclEx.Abp", "src"),
        };

        var list = new List<(string Name, string[] TargetFrameworks)>();

        foreach (var dir in dirs)
        {
            var dirInfo = new DirectoryInfo(dir);
            Assert.True(dirInfo.Exists, dirInfo.FullName);

            foreach (var sub in dirInfo.EnumerateDirectories().OrderBy(m => m.Name))
            {
                var projectFile = sub.EnumerateFiles("*.csproj").SingleOrDefault();
                if (projectFile is null)
                    continue;

                var projectRootElement = ProjectRootElement.Open(projectFile.FullName);
                var project = new Project(projectRootElement);

                if (project.GetPropertyValue("IsPackable").ToBool() == false)
                    continue;


                var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
                var frameworks = project.GetPropertyValue("TargetFrameworks").Split(';', options);

                var (name, _) = projectFile.GetFileNameAndExtension();
                list.Add((name, frameworks));
            }
        }

        var readmePath = new FileInfo(Path.Combine(src, "..", "README.md"));
        Assert.True(readmePath.Exists);

        var str = StringBuilderHelper.Build(m =>
        {
            m.AppendHeading("FclEx", 1);
            m.Append(' ');
            m.AppendBadge("LICENSE", LicenseBadgeUrl, "LICENSE.TXT");
            m.Append(' ');
            m.AppendBadge("Build", BuildWorkflowBadgeUrl, BuildWorkflowUrl);
            m.AppendLine();
            m.AppendLine();
            m.Append("Some basic useful extensions and helpers for C# fundamental class libraries.");
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