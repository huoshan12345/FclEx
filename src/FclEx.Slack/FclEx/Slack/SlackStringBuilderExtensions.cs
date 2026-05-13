namespace FclEx.Slack;

public static class SlackStringBuilderExtensions
{
    public static SlackStringBuilder AppendLine(this SlackStringBuilder builder, char value)
    {
        return builder.Append(value).Append('\n');
    }

    public static SlackStringBuilder AppendLine(this SlackStringBuilder builder, string? value = null)
    {
        return builder.Append(value).Append('\n');
    }

    public static SlackStringBuilder Append(this SlackStringBuilder builder, DateTimeOffset dateTime)
    {
        // <!date^timestamp^token_string^optional_link|fallback_text>
        // e.g. <!date^1392734382^Posted {date_num} {time_secs}|Posted 2014-02-18 6:39:42 AM PST>
        builder.Append("<!date^");
        builder.Append(dateTime.ToUnixTimeSeconds().ToString());
        builder.Append("^{date_num} {time}|");
        builder.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss zzz"));
        builder.Append('>');
        return builder;
    }

    /// <summary>
    /// e.g. `code`
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static SlackStringBuilder AppendInlineCode(this SlackStringBuilder builder, Action<SlackStringBuilder> action)
        => builder.Append(m => m.AppendWrapped("`", x => action(builder)));

    public static SlackStringBuilder AppendInlineCode(this SlackStringBuilder builder, string text)
        => builder.AppendInlineCode(m => m.Append(text));

    /// <summary>
    /// *bold* will produce bold text
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static SlackStringBuilder AppendBold(this SlackStringBuilder builder, Action<SlackStringBuilder> action)
        => builder.Append(m => m.AppendWrapped("*", x => action(builder)));

    public static SlackStringBuilder AppendBold(this SlackStringBuilder builder, string text)
        => builder.AppendBold(m => m.Append(text));

    /// <summary>
    /// ~strike~ will produce strike-through text
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static SlackStringBuilder AppendStrike(this SlackStringBuilder builder, Action<SlackStringBuilder> action)
        => builder.Append(m => m.AppendWrapped("~", x => action(builder)));

    /// <summary>
    /// _italic_ will produce italicized text
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static SlackStringBuilder AppendItalic(this SlackStringBuilder builder, Action<SlackStringBuilder> action)
        => builder.Append(m => m.AppendWrapped("_", x => action(builder)));

    /// <summary>
    /// Multi-line code blocks by placing 3 back-ticks before and after the block:
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static SlackStringBuilder AppendCodeBlock(this SlackStringBuilder builder, Action<SlackStringBuilder> action)
            => builder.Append(m => m.AppendWrapped("```", x => action(builder)));

    public static SlackStringBuilder AppendCodeBlock(this SlackStringBuilder builder, string text)
        => builder.AppendCodeBlock(m => m.Append(text));

    /// <summary>
    /// Emoji can be included in their full-color, fully-illustrated form directly in text. <br/>
    /// Once published, Slack will then convert the emoji into their common 'colon' format. <br/>
    /// The list of emoji supported are taken from https://github.com/iamcal/emoji-data
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static SlackStringBuilder AppendEmoji(this SlackStringBuilder builder, string name)
        => builder.Append(m => m.AppendWrapped(":", x => m.Append(name)));

    public static SlackStringBuilder AppendLink(this SlackStringBuilder builder, string link, string? text = null)
    {
        builder.Append('<');
        builder.Append(link);
        if (text is { Length: > 0 })
        {
            builder.Append('|');
            builder.Append(text);
        }
        builder.Append('>');
        return builder;
    }

    public static SlackStringBuilder AppendLink(this SlackStringBuilder builder, Action<SlackStringBuilder> link, Action<SlackStringBuilder>? text = null)
    {
        builder.Append('<');
        link(builder);
        if (text is not null)
        {
            builder.Append('|');
            text(builder);
        }
        builder.Append('>');
        return builder;
    }

    public static SlackStringBuilder AppendUser(this SlackStringBuilder builder, string userId)
    {
        builder.Append("<@");
        builder.Append(userId);
        builder.Append('>');
        return builder;
    }

    public static SlackStringBuilder AppendUserGroup(this SlackStringBuilder builder, string groupId)
    {
        builder.Append("<!subteam^");
        builder.Append(groupId);
        builder.Append('>');
        return builder;
    }

    public static SlackStringBuilder AppendBlockQuoted(this SlackStringBuilder builder, string text)
    {
        foreach (var line in text.SplitToLines())
        {
            builder.Append('>');
            builder.Append(line);
        }
        return builder;
    }

    public static SlackStringBuilder AppendListItem(this SlackStringBuilder builder, Action<SlackStringBuilder> action)
    {
        builder.Append("• ");
        action(builder);
        builder.Append('\n');
        return builder;
    }
}