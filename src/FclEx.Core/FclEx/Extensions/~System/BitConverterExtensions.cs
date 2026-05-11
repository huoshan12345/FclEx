namespace FclEx.Extensions;

public static class BitConverterExtensions
{
    extension(BitConverter)
    {
#if NETSTANDARD
        [MethodImpl(AggressiveInlining)]
        public static int ToInt32(ReadOnlySpan<byte> value)
        {
            Check.NotLessThan(value.Length, sizeof(int));
            return Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(value));
        }
#endif
    }
}
