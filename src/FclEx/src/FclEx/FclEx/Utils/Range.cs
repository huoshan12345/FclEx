namespace FclEx.Utils;

public static class Range
{
    public static Range<T> Create<T>(T? min, T? max)
        where T : struct
    {
        return new Range<T>(min, max);
    }

    public static Range<T> Create<T>(T min, T max)
        where T : struct
    {
        return new Range<T>(min, max);
    }
}

public struct Range<T> where T : struct
{
    public static readonly Range<T> Empty = new Range<T>(null, null);

    public Range(Bound<T> min, Bound<T> max) : this()
    {
        Min = min;
        Max = max;
    }

    public Range(T? min, T? max) : this()
    {
        Min = min;
        Max = max;
    }

    public Range(T min, T max) : this()
    {
        Min = min;
        Max = max;
    }

    public Bound<T> Min { get; private set; }
    public Bound<T> Max { get; private set; }
}