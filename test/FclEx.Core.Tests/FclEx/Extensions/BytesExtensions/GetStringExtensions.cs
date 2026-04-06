namespace FclEx.Extensions.BytesExtensions;

public class GetStringExtensions
{
    public static readonly TheoryData<string> Strings = new()
    {
        string.Empty,
        "123",
        "123asefajljsl;",
        "  \t\n  ",
        "   "
    };

    [Theory]
    [MemberData(nameof(Strings))]
    public void GetString_ByteArray_Test(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        Assert.Equal(input, bytes.GetString());
    }

    [Theory]
    [MemberData(nameof(Strings))]
    public void GetString_ArraySegment_Test(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input).ToSegment();
        Assert.Equal(input, bytes.GetString());
    }

    [Fact]
    public void GetString_ArraySegment_Index_Test()
    {
        var bytesList = Strings
            .Select(m => m.Data)
            .Select(m => (Str: m, Bytes: Encoding.UTF8.GetBytes(m)))
            .ToList();

        var arr = bytesList.Select(m => m.Bytes).Concat().ToArray();

        var seed = (Offset: 0, List: new List<(string Str, ArraySegment<byte> Seg)>());
        var (_, list) = bytesList.Aggregate(seed, (x, y) =>
        {
            var seg = new ArraySegment<byte>(arr, x.Offset, y.Bytes!.Length);
            x.List!.Add((y.Str, seg));
            x.Offset += y.Bytes!.Length;
            return x;
        });

        foreach (var (str, seg) in list)
        {
            Assert.Equal(str, seg.GetString());
        }
    }
}