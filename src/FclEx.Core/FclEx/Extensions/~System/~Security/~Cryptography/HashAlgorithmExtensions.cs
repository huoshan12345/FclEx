namespace FclEx.Extensions;

public static class HashAlgorithmExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static byte[] Hash(this HashAlgorithm algorithm, byte[]? input)
    {
        return input.IsNullOrEmpty()
            ? []
            : algorithm.ComputeHash(input);
    }

    [MethodImpl(AggressiveInlining)]
    public static byte[] Hash(this HashAlgorithm algorithm, byte[]? input, int offset, int count)
    {
        return input.IsNullOrEmpty()
            ? []
            : algorithm.ComputeHash(input, offset, count);
    }

    [MethodImpl(AggressiveInlining)]
    public static byte[] Hash(this HashAlgorithm algorithm, ArraySegment<byte> input)
    {
        return algorithm.Hash(input.Array, input.Offset, input.Count);
    }
}