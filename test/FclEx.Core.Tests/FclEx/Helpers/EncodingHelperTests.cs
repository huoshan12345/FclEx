namespace FclEx.Helpers;

public class EncodingHelperTests
{
    public static TheoryData<byte[], int> BomTestData => new()
    {
        { Encoding.UTF8.GetPreamble().Append((byte)'a').ToArray(), Encoding.UTF8.CodePage },
        { Encoding.Unicode.GetPreamble().Append((byte)'a').ToArray(), Encoding.Unicode.CodePage },
        { Encoding.BigEndianUnicode.GetPreamble().Append((byte)'a').ToArray(), Encoding.BigEndianUnicode.CodePage },
        { Encoding.UTF32.GetPreamble().Append((byte)'a').ToArray(), Encoding.UTF32.CodePage },
        { new UTF32Encoding(bigEndian: true, byteOrderMark: true).GetPreamble().Append((byte)'a').ToArray(), new UTF32Encoding(bigEndian: true, byteOrderMark: true).CodePage },
    };

    [Theory]
    [MemberData(nameof(BomTestData))]
    public void GetEncoding_Bom_ShouldReturnMatchingEncoding(byte[] bytes, int expectedCodePage)
    {
        using var stream = new MemoryStream(bytes);

        var encoding = EncodingHelper.GetEncoding(stream);

        Assert.Equal(expectedCodePage, encoding.CodePage);
    }

    [Fact]
    public void GetEncoding_ShouldRestoreOriginalPositionAndLeaveStreamOpen()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetPreamble().Append((byte)'a').ToArray());
        stream.Position = 1;

        _ = EncodingHelper.GetEncoding(stream);

        Assert.Equal(1, stream.Position);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void GetEncoding_NonSeekableStream_ShouldThrow()
    {
        using var stream = new NonSeekableStream([1]);

        var exception = Assert.Throws<ArgumentException>(() => EncodingHelper.GetEncoding(stream));

        Assert.Equal("stream", exception.ParamName);
    }

    private sealed class NonSeekableStream : MemoryStream
    {
        public NonSeekableStream(byte[] buffer) : base(buffer)
        {
        }

        public override bool CanSeek => false;
    }
}
