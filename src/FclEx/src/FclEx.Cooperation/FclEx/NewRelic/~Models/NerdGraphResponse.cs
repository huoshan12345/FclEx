namespace FclEx.NewRelic;

public class NerdGraphResponse<T>
{
    [JsonProperty("actor")]
    public Actor<T> Actor { get; set; } = default!;
}

public class Actor<T>
{
    [JsonProperty("account")]
    public Account<T> Account { get; set; } = default!;
}

public class Account<T>
{
    [JsonProperty("nrql")]
    public NrqlResult<T> Nrql { get; set; } = default!;
}

public class NrqlResult<T>
{
    [JsonProperty("metadata")]
    public Metadata Metadata { get; set; } = default!;

    [JsonProperty("results")]
    public T[] Results { get; set; } = Array.Empty<T>();
}

public class Metadata
{
    [JsonProperty("facets")]
    public string[] Facets { get; set; } = Array.Empty<string>();

    [JsonProperty("timeWindow")]
    public TimeWindow TimeWindow { get; set; } = default!;
}

public class TimeWindow
{
    private long _begin;
    private long _end;

    [JsonProperty("begin")]
    public long Begin
    {
        get => _begin;
        set
        {
            _begin = value;
            BeginTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
        }
    }

    [JsonProperty("end")]
    public long End
    {
        get => _end;
        set
        {
            _end = value;
            EndTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
        }
    }

    [JsonIgnore] public DateTimeOffset BeginTime { get; private set; }
    [JsonIgnore] public DateTimeOffset EndTime { get; private set; }

    public static readonly TimeWindow Default = new()
    {
        Begin = DateTimeExtensions.UnixEpoch.ToUnixTimeMilliseconds(),
        End = new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds()
    };
}