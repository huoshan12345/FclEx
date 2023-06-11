using System;


namespace FclEx.Abp.Configuration;

public struct IntConfigItem
{
    private int _value;

    public IntConfigItem(int @default, int? min = null, int? max = null)
    {
        if (min.HasValue && max.HasValue && min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "the min value cannot be greater than the max value.");
            
        if (min.HasValue) 
            Check.NotLessThan(@default, min.Value);

        if (max.HasValue) 
            Check.NotGreaterThan(@default, max.Value);

        Min = min;
        Max = max;
        Default = @default;
        _value = @default;
    }

    public int? Min { get; }
    public int? Max { get; }
    public int Default { get; }

    public int Value
    {
        get => _value;
        set => _value = SetValue(value);
    }

    private int SetValue(int attemptValue)
    {
        var v = attemptValue;
        if (Min.HasValue)
            v = Math.Max(v, Min.Value);
        if (Max.HasValue)
            v = Math.Min(v, Max.Value);
        return v;
    }

    public static implicit operator int(IntConfigItem item)
    {
        return item.Value;
    }
}