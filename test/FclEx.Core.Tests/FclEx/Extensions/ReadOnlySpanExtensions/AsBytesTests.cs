// ReSharper disable ConvertToConstant.Local
namespace FclEx.Extensions.ReadOnlySpanExtensions;

public class AsBytesTests
{
    [Fact]
    public void AsBytes_Decimal()
    {
        var value = 1.2345m;
        var bits = value.GetBits();
        var bytes = bits.AsReadOnlySpan().AsBytes();
        var expectedBytes = bits.AsReadOnlySpan().ToBytes();

#if NET5_0_OR_GREATER
        Assert.Equal(expectedBytes, bytes);
#else
        Assert.Equal(expectedBytes.ToArray(), bytes.ToArray());
#endif
        var actual = new decimal(bytes.ToIntArray());
        Assert.Equal(value, actual);
    }
}

file static class Extensions
{
    public static byte[] ToBytes(this ReadOnlySpan<int> span)
    {
        const int size = sizeof(int);
        var length = span.Length * size;
        var array = new byte[length];
        for (var i = 0; i < span.Length; i++)
        {
            var value = span[i];
            var valueBytes = BitConverter.GetBytes(value);
            valueBytes.CopyTo(array, i * 4);
        }
        return array;
    }

    public static int[] ToIntArray(this ReadOnlySpan<byte> span)
    {
        const int size = sizeof(int);
        var length = span.Length / size;
        var array = new int[length];
        for (var i = 0; i < length; i++)
        {
            var valueSpan = span[(i * size)..];
#if NET5_0_OR_GREATER
            array[i] = BitConverter.ToInt32(valueSpan);
#else
            array[i] = BitConverter.ToInt32(valueSpan.ToArray(), 0);
#endif
        }
        return array;
    }
}