namespace FclEx.Atlassian;

public static class JiraStringBuilderExtensions
{
    public static JiraStringBuilder RenderCodeBlock(this JiraStringBuilder builder, Action<JiraStringBuilder> action)
        => builder.RenderBlock("{noformat}", action, "{noformat}");

    public static JiraStringBuilder RenderCodeBlock(this JiraStringBuilder builder, string text)
        => builder.RenderCodeBlock(m => m.Append(text));
}