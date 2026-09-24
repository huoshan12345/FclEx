#pragma warning disable RS1035

using System;

namespace FclEx;

public static class Constants
{
    public static readonly bool IsGithubAction = Environment.GetEnvironmentVariable("GITHUB_ACTION") is { Length: > 0 };
    public static readonly bool IsDependabot = Environment.GetEnvironmentVariable("GITHUB_DEPENDABOT_JOB_TOKEN") is { Length: > 0 };
}
