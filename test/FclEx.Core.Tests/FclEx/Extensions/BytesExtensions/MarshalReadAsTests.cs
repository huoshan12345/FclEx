namespace FclEx.Extensions.BytesExtensions;

public class MarshalReadAsTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NativeRecord
    {
        public short Kind;
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private unsafe struct FixedBufferRecord
    {
        public ushort Kind;
        public fixed byte Payload[5];
        public int Sequence;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ByValArrayElement
    {
        public short Code;
        public byte State;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ByValArrayRecord
    {
        public int Id;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I2)]
        public short[] Values;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.Struct)]
        public ByValArrayElement[] Elements;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PointerStringRecord
    {
        public int Id;
        public string Text;
    }

#if NET8_0_OR_GREATER
    [InlineArray(4)]
    private struct Int32InlineArray
    {
        private int _element0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct InlineArrayElement
    {
        public short Code;
        public byte State;
    }

    [InlineArray(3)]
    private struct StructInlineArray
    {
        private InlineArrayElement _element0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct InlineArrayRecord
    {
        public byte Kind;
        public Int32InlineArray Numbers;
        public StructInlineArray Elements;
    }
#endif

    [Fact]
    public void MarshalReadAs_ReadsSequentialLayoutAndAdvancesOffset()
    {
        var expected = new NativeRecord { Kind = 7, Value = 123456 };
        var values = new[] { expected };
        var bytes = MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
        var prefixed = new byte[bytes.Length + 2];
        bytes.CopyTo(prefixed, 2);
        var offset = 2;

        var actual = prefixed.MarshalReadAs<NativeRecord>(ref offset);

        Assert.Equal(expected.Kind, actual.Kind);
        Assert.Equal(expected.Value, actual.Value);
        Assert.Equal(prefixed.Length, offset);
    }

    [Fact]
    public void MarshalReadArrayAs_ReadsEveryValue()
    {
        var expected = new[]
        {
            new NativeRecord { Kind = 1, Value = 10 },
            new NativeRecord { Kind = 2, Value = 20 },
        };
        var bytes = MemoryMarshal.AsBytes(expected.AsSpan()).ToArray();

        var actual = bytes.MarshalReadArrayAs<NativeRecord>();

        Assert.Equal(expected.Select(x => (x.Kind, x.Value)), actual.Select(x => (x.Kind, x.Value)));
    }

    [Fact]
    public void MarshalReadArrayAs_RejectsTrailingPartialValue()
    {
        var bytes = new byte[Unsafe.SizeOf<NativeRecord>() + 1];

        Assert.Throws<ArgumentException>(() => bytes.MarshalReadArrayAs<NativeRecord>());
        Assert.Throws<ArgumentException>(() => bytes.AsReadOnlySpan().MarshalReadArrayAs<NativeRecord>());
    }

    [Fact]
    public void MarshalReadArrayAs_AllowsEmptyInput()
    {
        Assert.Empty(Array.Empty<byte>().MarshalReadArrayAs<NativeRecord>());
        Assert.Empty(ReadOnlySpan<byte>.Empty.MarshalReadArrayAs<NativeRecord>());
    }

    [Fact]
    public void MarshalReadAs_ReadsFixedBufferAndAdvancesByFullStructureSize()
    {
        var expected = CreateFixedBufferRecord(7, [1, 2, 3, 4, 5], 123456);
        var structureBytes = MemoryMarshal.AsBytes(new[] { expected }.AsSpan()).ToArray();
        var bytes = new byte[structureBytes.Length + 4];
        structureBytes.CopyTo(bytes, 3);
        var offset = 3;

        var actual = bytes.MarshalReadAs<FixedBufferRecord>(ref offset);

        AssertFixedBufferRecord(expected, actual);
        Assert.Equal(3 + Unsafe.SizeOf<FixedBufferRecord>(), offset);
        Assert.Equal(11, Unsafe.SizeOf<FixedBufferRecord>());
    }

    [Fact]
    public void MarshalReadAs_FromSpan_ReadsFixedBufferAndIgnoresTrailingBytes()
    {
        var expected = CreateFixedBufferRecord(42, [10, 20, 30, 40, 50], -99);
        var structureBytes = MemoryMarshal.AsBytes(new[] { expected }.AsSpan()).ToArray();
        var bytes = new byte[structureBytes.Length + 2];
        structureBytes.CopyTo(bytes, 0);
        bytes[^2] = 0xAA;
        bytes[^1] = 0xBB;

        var actual = bytes.AsReadOnlySpan().MarshalReadAs<FixedBufferRecord>();

        AssertFixedBufferRecord(expected, actual);
    }

    [Fact]
    public void MarshalReadArrayAs_ReadsEachFixedBufferIndependently()
    {
        var expected = new[]
        {
            CreateFixedBufferRecord(1, [1, 1, 2, 3, 5], 8),
            CreateFixedBufferRecord(2, [13, 21, 34, 55, 89], 144),
        };
        var bytes = MemoryMarshal.AsBytes(expected.AsSpan()).ToArray();

        var actual = bytes.MarshalReadArrayAs<FixedBufferRecord>();

        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
            AssertFixedBufferRecord(expected[i], actual[i]);
    }

    [Fact]
    public void MarshalReadAs_ReadsPrimitiveAndStructByValArrays()
    {
        var expected = CreateByValArrayRecord(
            42,
            [1, -2, 300, short.MaxValue],
            [(7, 8), (-9, 10)]);
        var structureBytes = new[] { expected }.MarshalArrayToBytes();
        var bytes = new byte[structureBytes.Length + 3];
        structureBytes.CopyTo(bytes, 2);
        var offset = 2;

        var actual = bytes.MarshalReadAs<ByValArrayRecord>(ref offset);

        AssertByValArrayRecord(expected, actual);
        Assert.Equal(2 + Marshal.SizeOf<ByValArrayRecord>(), offset);
        Assert.NotSame(expected.Values, actual.Values);
        Assert.NotSame(expected.Elements, actual.Elements);
    }

    [Fact]
    public void MarshalReadArrayAs_CreatesIndependentByValArraysForEveryStructure()
    {
        var expected = new[]
        {
            CreateByValArrayRecord(1, [1, 2, 3, 4], [(5, 6), (7, 8)]),
            CreateByValArrayRecord(2, [-1, -2, -3, -4], [(-5, 16), (-7, 18)]),
        };
        var bytes = expected.MarshalArrayToBytes();

        var actual = bytes.AsReadOnlySpan().MarshalReadArrayAs<ByValArrayRecord>();

        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
            AssertByValArrayRecord(expected[i], actual[i]);

        Assert.NotSame(actual[0].Values, actual[1].Values);
        Assert.NotSame(actual[0].Elements, actual[1].Elements);
    }

    [Fact]
    public void MarshalReadAs_RejectsManagedFieldsThatWouldDereferenceInputAsAPointer()
    {
        Assert.Throws<NotSupportedException>(() => new byte[64].MarshalReadAs<PointerStringRecord>());
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void MarshalReadAs_ReadsInlineArraysOfPrimitiveAndStructElements()
    {
        var expected = CreateInlineArrayRecord(
            7,
            [10, 20, 30, 40],
            [(1, 2), (300, 4), (-5, 6)]);
        var structureBytes = MemoryMarshal.AsBytes(new[] { expected }.AsSpan()).ToArray();
        var bytes = new byte[structureBytes.Length + 3];
        structureBytes.CopyTo(bytes, 2);
        var offset = 2;

        var actual = bytes.MarshalReadAs<InlineArrayRecord>(ref offset);

        AssertInlineArrayRecord(expected, actual);
        Assert.Equal(2 + Unsafe.SizeOf<InlineArrayRecord>(), offset);
        Assert.Equal(26, Unsafe.SizeOf<InlineArrayRecord>());
    }

    [Fact]
    public void MarshalReadArrayAs_ReadsInlineArraysWithoutSharingElements()
    {
        var expected = new[]
        {
            CreateInlineArrayRecord(
                1,
                [1, 2, 3, 4],
                [(10, 11), (12, 13), (14, 15)]),
            CreateInlineArrayRecord(
                2,
                [-1, -2, -3, -4],
                [(-10, 21), (-12, 23), (-14, 25)]),
        };
        var bytes = MemoryMarshal.AsBytes(expected.AsSpan()).ToArray();

        var actual = bytes.AsReadOnlySpan().MarshalReadArrayAs<InlineArrayRecord>();

        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
            AssertInlineArrayRecord(expected[i], actual[i]);
    }
#endif

    private static unsafe FixedBufferRecord CreateFixedBufferRecord(
        ushort kind,
        ReadOnlySpan<byte> payload,
        int sequence)
    {
        Assert.Equal(5, payload.Length);

        var result = new FixedBufferRecord
        {
            Kind = kind,
            Sequence = sequence,
        };
        var destination = result.Payload;
        payload.CopyTo(new Span<byte>(destination, 5));

        return result;
    }

    private static ByValArrayRecord CreateByValArrayRecord(
        int id,
        short[] values,
        (short Code, byte State)[] elements)
    {
        return new ByValArrayRecord
        {
            Id = id,
            Values = values,
            Elements = elements.Select(element => new ByValArrayElement
            {
                Code = element.Code,
                State = element.State,
            }).ToArray(),
        };
    }

#if NET8_0_OR_GREATER
    private static InlineArrayRecord CreateInlineArrayRecord(
        byte kind,
        ReadOnlySpan<int> numbers,
        ReadOnlySpan<(short Code, byte State)> elements)
    {
        Assert.Equal(4, numbers.Length);
        Assert.Equal(3, elements.Length);

        var result = new InlineArrayRecord { Kind = kind };
        for (var i = 0; i < numbers.Length; i++)
            result.Numbers[i] = numbers[i];

        for (var i = 0; i < elements.Length; i++)
        {
            result.Elements[i] = new InlineArrayElement
            {
                Code = elements[i].Code,
                State = elements[i].State,
            };
        }

        return result;
    }
#endif

    private static void AssertFixedBufferRecord(FixedBufferRecord expected, FixedBufferRecord actual)
    {
        Assert.Equal(expected.Kind, actual.Kind);
        Assert.Equal(expected.Sequence, actual.Sequence);
        Assert.Equal(GetPayload(expected), GetPayload(actual));
    }

    private static void AssertByValArrayRecord(ByValArrayRecord expected, ByValArrayRecord actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Values, actual.Values);
        Assert.Equal(
            expected.Elements.Select(element => (element.Code, element.State)),
            actual.Elements.Select(element => (element.Code, element.State)));
    }

#if NET8_0_OR_GREATER
    private static void AssertInlineArrayRecord(InlineArrayRecord expected, InlineArrayRecord actual)
    {
        Assert.Equal(expected.Kind, actual.Kind);
        for (var i = 0; i < 4; i++)
            Assert.Equal(expected.Numbers[i], actual.Numbers[i]);

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(expected.Elements[i].Code, actual.Elements[i].Code);
            Assert.Equal(expected.Elements[i].State, actual.Elements[i].State);
        }
    }
#endif

    private static unsafe byte[] GetPayload(FixedBufferRecord value)
    {
        var result = new byte[5];
        var source = value.Payload;
        new ReadOnlySpan<byte>(source, result.Length).CopyTo(result);

        return result;
    }
}
