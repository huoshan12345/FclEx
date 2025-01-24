namespace FclEx.Utils;

public static class Transput
{
    public static Transput<TInput, TOutput> Create<TInput, TOutput>(TInput input, TOutput output)
        => new(input, output);
}

/// <summary>
/// The combination of input and output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="Input"></param>
/// <param name="Output"></param>
public readonly record struct Transput<TInput, TOutput>(TInput Input, TOutput Output)
{
    public static implicit operator Transput<TInput, TOutput>((TInput Input, TOutput Output) tuple)
    {
        return new(tuple.Input, tuple.Output);
    }
}