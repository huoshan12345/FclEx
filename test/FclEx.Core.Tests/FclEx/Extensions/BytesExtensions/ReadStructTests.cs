namespace FclEx.Extensions.BytesExtensions;

public class ReadStructTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NativeRecord
    {
        public short Kind;
        public int Value;
    }

    [Fact]
    public void ReadStruct_ReadsSequentialLayoutAndAdvancesOffset()
    {
        var expected = new NativeRecord { Kind = 7, Value = 123456 };
        var values = new[] { expected };
        var bytes = MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
        var prefixed = new byte[bytes.Length + 2];
        bytes.CopyTo(prefixed, 2);
        var offset = 2;

        var actual = prefixed.ReadStruct<NativeRecord>(ref offset);

        Assert.Equal(expected.Kind, actual.Kind);
        Assert.Equal(expected.Value, actual.Value);
        Assert.Equal(prefixed.Length, offset);
    }

    [Fact]
    public void ReadStructArray_ReadsEveryValue()
    {
        var expected = new[]
        {
            new NativeRecord { Kind = 1, Value = 10 },
            new NativeRecord { Kind = 2, Value = 20 },
        };
        var bytes = MemoryMarshal.AsBytes(expected.AsSpan()).ToArray();

        var actual = bytes.ReadStructArray<NativeRecord>();

        Assert.Equal(expected.Select(x => (x.Kind, x.Value)), actual.Select(x => (x.Kind, x.Value)));
    }

    [Fact]
    public void ReadStructArray_RejectsTrailingPartialValue()
    {
        var bytes = new byte[Unsafe.SizeOf<NativeRecord>() + 1];

        Assert.Throws<ArgumentException>(() => bytes.ReadStructArray<NativeRecord>());
        Assert.Throws<ArgumentException>(() => bytes.AsReadOnlySpan().ReadStructArray<NativeRecord>());
    }

    [Fact]
    public void ReadStructArray_AllowsEmptyInput()
    {
        Assert.Empty(Array.Empty<byte>().ReadStructArray<NativeRecord>());
        Assert.Empty(ReadOnlySpan<byte>.Empty.ReadStructArray<NativeRecord>());
    }
}
