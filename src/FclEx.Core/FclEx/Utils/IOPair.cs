namespace FclEx.Utils;

public static class IOPair
{
    public static IOPair<TInput, TOutput> Create<TInput, TOutput>(TInput input, TOutput output)
        => new(input, output);
}

/// <summary>
/// The combination of input and output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="Input"></param>
/// <param name="Output"></param>
public readonly record struct IOPair<TInput, TOutput>(TInput Input, TOutput Output)
{
    public static implicit operator IOPair<TInput, TOutput>((TInput Input, TOutput Output) tuple)
    {
        return new(tuple.Input, tuple.Output);
    }
}