namespace Confluent.Kafka;

public static class HeadersExtensions
{
    public static Headers Add(this Headers headers, string key, string? value)
    {
        if (value != null)
            headers.Add(key, Encoding.UTF8.GetBytes(value));
        return headers;
    }
}