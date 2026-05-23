namespace FclEx.Extensions;

public static class BitConverterExtensions
{
    extension(BitConverter)
    {
#if !NET5_0_OR_GREATER
        [MethodImpl(AggressiveInlining)]
        public static int ToInt32(ReadOnlySpan<byte> value)
        {
            Check.NotLessThan(value.Length, sizeof(int));
            return Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(value));
        }
#endif
    }
}
