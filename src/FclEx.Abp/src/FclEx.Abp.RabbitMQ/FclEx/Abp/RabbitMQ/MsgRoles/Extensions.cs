using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace FclEx.Abp.RabbitMQ.MsgRoles;

public static class Extensions
{
    public static void Publish<TOutput>(this Publisher<TOutput> publisher, TOutput msg, string routingKey, TimeSpan delay = default, string messageId = "")
    {
        publisher.Publish((msg, routingKey, delay, messageId));
    }

    public static void Publish<TOutput>(this Publisher<TOutput> publisher, IEnumerable<TOutput> msgs, string routingKey, TimeSpan delay = default)
    {
        publisher.Publish(msgs.Select(m => (OutputMessage<TOutput>)(m, routingKey, delay, "")));
    }

    public static void Publish<T, TOutput>(this Publisher<TOutput> publisher, IEnumerable<T> source, Func<T, (TOutput body, string routingKey, TimeSpan delay, string id)> selector)
    {
        publisher.Publish(source.Select(m => (OutputMessage<TOutput>)selector(m)));
    }

    public static void Publish<T, TOutput>(this Publisher<TOutput> publisher, IEnumerable<T> source, Func<T, (TOutput body, string routingKey)> selector)
    {
        publisher.Publish(source.Select(m => (OutputMessage<TOutput>)selector(m)));
    }
}