using SlackNet.Blocks;
using SlackNet.WebApi;

namespace FclEx.Slack;

public static class MessageExtensions
{
    public static Message AddBlock(this Message message, Block block)
    {
        message.Blocks ??= new List<Block>();
        message.Blocks.Add(block);
        return message;
    }

    public static Message AddMarkdown(this Message message, string text)
    {
        return message.AddBlock(new SectionBlock
        {
            Text = new Markdown { Text = text }
        });
    }

    /// <summary>
    /// Channel, private group, or IM channel to send message to. Can be an encoded ID, or a name.
    /// </summary>
    public static Message Channel(this Message message, string channel)
    {
        message.Channel = channel;
        return message;
    }
}