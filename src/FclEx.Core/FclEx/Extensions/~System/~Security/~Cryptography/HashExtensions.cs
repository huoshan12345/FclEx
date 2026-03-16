namespace FclEx.Extensions;

public static class HashExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static byte[] Hash(this HashAlgorithm algorithm, byte[] input)
    {
        return input.IsNullOrEmpty()
            ? []
            : algorithm.ComputeHash(input);
    }

    [MethodImpl(AggressiveInlining)]
    public static byte[] Hash(this HashAlgorithm algorithm, ArraySegment<byte> input)
    {
        return input.Array.IsNullOrEmpty()
            ? []
            : algorithm.ComputeHash(input.Array, input.Offset, input.Count);
    }
}