using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Extensions.ByteExtensions;

public class GetStringExtensions
{
    public static readonly IEnumerable<object[]> Strings = new[]
    {
        string.Empty,
        "123",
        "123asefajljsl;",
        "  \t\n  ",
        "   "
    }.Select(m => new object[] { m });

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
            .Select(m => m.First())
            .Cast<string>()
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