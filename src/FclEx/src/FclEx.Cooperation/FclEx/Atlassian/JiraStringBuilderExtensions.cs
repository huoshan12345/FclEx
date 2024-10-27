namespace FclEx.Atlassian;

public static class JiraStringBuilderExtensions
{
    public static JiraStringBuilder AppendCodeBlock(this JiraStringBuilder builder, Action<JiraStringBuilder> action)
        => builder.Append(m => m.AppendQuoted("{noformat}", m => action(builder), "{noformat}"));

    public static JiraStringBuilder AppendCodeBlock(this JiraStringBuilder builder, string text)
        => builder.AppendCodeBlock(m => m.Append(text));
}